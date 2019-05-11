using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Consul;
using LaunchDarkly.Client.Utils;

namespace LaunchDarkly.Client.Consul
{
    /// <summary>
    /// Internal implementation of the Consul feature store.
    /// 
    /// Implementation notes:
    /// 
    /// * Feature flags, segments, and any other kind of entity the LaunchDarkly client may wish
    /// to store, are stored as individual items with the key "{prefix}/features/{flag-key}",
    /// "{prefix}/segments/{segment-key}", etc.
    /// 
    /// * The special key "{prefix}/$inited" indicates that the store contains a complete data set.
    /// 
    /// * Since Consul has limited support for transactions(they can't contain more than 64
    /// operations), the Init method-- which replaces the entire data store-- is not guaranteed to
    /// be atomic, so there can be a race condition if another process is adding new data via
    /// Upsert. To minimize this, we don't delete all the data at the start; instead, we update
    /// the items we've received, and then delete all other items. That could potentially result in
    /// deleting new data from another process, but that would be the case anyway if the Init
    /// happened to execute later than the Upsert; we are relying on the fact that normally the
    /// process that did the Init will also receive the new data shortly and do its own Upsert.
    /// </summary>
    internal sealed class ConsulFeatureStoreCore : IFeatureStoreCoreAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConsulFeatureStoreCore));
        
        private readonly ConsulClient _client;
        private readonly string _prefix;
        
        internal ConsulFeatureStoreCore(ConsulClient client, string prefix)
        {
            Log.InfoFormat("Creating Consul feature store using host at {0}", client.Config.Address);

            _client = client;
            _prefix = String.IsNullOrEmpty(prefix) ? "" : (prefix + "/");
        }
        
        public async Task<bool> InitializedInternalAsync()
        {
            var result = await _client.KV.Get(InitedKey);
            return result.Response != null;
        }

        public async Task InitInternalAsync(IDictionary<IVersionedDataKind, IDictionary<string, IVersionedData>> allData)
        {
            // Start by reading the existing keys; we will later delete any of these that weren't in allData.
            var keysResult = await _client.KV.Keys(_prefix);
            var unusedOldKeys = keysResult.Response == null ? new HashSet<string>() :
                new HashSet<string>(keysResult.Response);

            var ops = new List<KVTxnOp>();
            var numItems = 0;

            // Insert or update every provided item
            foreach (var entry in allData)
            {
                var kind = entry.Key;
                foreach (var item in entry.Value.Values)
                {
                    var json = FeatureStoreHelpers.MarshalJson(item);
                    var key = ItemKey(kind, item.Key);
                    var op = new KVTxnOp(key, KVTxnVerb.Set)
                    {
                        Value = Encoding.UTF8.GetBytes(json)
                    };
                    ops.Add(op);
                    unusedOldKeys.Remove(key);
                    numItems++;
                }
            }

            // Now delete any previously existing items whose keys were not in the current data
            foreach (var oldKey in unusedOldKeys)
            {
                ops.Add(new KVTxnOp(oldKey, KVTxnVerb.Delete));
            }

            // Now set the special key that we check in InitializedInternalAsync()
            var initedOp = new KVTxnOp(InitedKey, KVTxnVerb.Set)
            {
                Value = new byte[0]
            };
            ops.Add(initedOp);

            await BatchOperationsAsync(ops);

            Log.InfoFormat("Initialized database with {0} items", numItems);
        }

        public async Task<IVersionedData> GetInternalAsync(IVersionedDataKind kind, String key)
        {
            var result = await _client.KV.Get(ItemKey(kind, key));
            return result.Response == null ? null :
                FeatureStoreHelpers.UnmarshalJson(kind, Encoding.UTF8.GetString(result.Response.Value));
        }
        
        public async Task<IDictionary<string, IVersionedData>> GetAllInternalAsync(IVersionedDataKind kind)
        {
            var ret = new Dictionary<string, IVersionedData>();
            var result = await _client.KV.List(KindKey(kind));
            foreach (var pair in result.Response)
            {
                var item = FeatureStoreHelpers.UnmarshalJson(kind, Encoding.UTF8.GetString(pair.Value));
                ret.Add(item.Key, item);
            }
            return ret;
        }

        public async Task<IVersionedData> UpsertInternalAsync(IVersionedDataKind kind, IVersionedData newItem)
        {
            var key = ItemKey(kind, newItem.Key);
            var json = FeatureStoreHelpers.MarshalJson(newItem);

            // We will potentially keep retrying indefinitely until someone's write succeeds
            while (true)
            {
                var oldValue = (await _client.KV.Get(key)).Response;
                var oldItem = oldValue == null ? null :
                    FeatureStoreHelpers.UnmarshalJson(kind, Encoding.UTF8.GetString(oldValue.Value));

                // Check whether the item is stale. If so, don't do the update (and return the existing item to
                // FeatureStoreWrapper so it can be cached)
                if (oldItem != null && oldItem.Version >= newItem.Version)
                {
                    return oldItem;
                }

                // Otherwise, try to write. We will do a compare-and-set operation, so the write will only succeed if
                // the key's ModifyIndex is still equal to the previous value returned by getEvenIfDeleted. If the
                // previous ModifyIndex was zero, it means the key did not previously exist and the write will only
                // succeed if it still doesn't exist.
                var modifyIndex = oldValue == null ? 0 : oldValue.ModifyIndex;
                var pair = new KVPair(key)
                {
                    Value = Encoding.UTF8.GetBytes(json),
                    ModifyIndex = modifyIndex
                };
                var result = await _client.KV.CAS(pair);
                if (result.Response)
                {
                    return newItem;
                }

                // If we failed, retry the whole shebang
                Log.Debug("Concurrent modification detected, retrying");
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }

        private string ItemKey(IVersionedDataKind kind, string key)
        {
            return KindKey(kind) + "/" + key;
        }

        private string KindKey(IVersionedDataKind kind)
        {
            return _prefix + kind.GetNamespace();
        }
        
        private string InitedKey
        {
            get
            {
                return _prefix + "$inited";
            }
        }

        private async Task BatchOperationsAsync(List<KVTxnOp> ops)
        {
            int batchSize = 64; // Consul can only do this many at a time
            for (int i = 0; i < ops.Count; i += batchSize)
            {
                var batch = ops.GetRange(i, Math.Min(batchSize, ops.Count - i));
                await _client.KV.Txn(batch);
            }
        }
    }
}
