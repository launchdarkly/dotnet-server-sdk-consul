using System;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using LaunchDarkly.Client.SharedTests.FeatureStore;

namespace LaunchDarkly.Client.Consul.Tests
{
    public class ConsulFeatureStoreTest : FeatureStoreBaseTests
    {
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
        
        protected override IFeatureStore CreateStoreImpl(FeatureStoreCacheConfig caching)
        {
            return Components.PersistentDataStore(
                    Integrations.Consul.DataStore()
                ).CacheTime(caching.Ttl)
                .CreateFeatureStore();
        }
        
        protected override IFeatureStore CreateStoreImplWithPrefix(string prefix)
        {
            return Components.PersistentDataStore(
                    Integrations.Consul.DataStore().Prefix(prefix)
                ).NoCaching()
                .CreateFeatureStore();
        }
        
        override protected void ClearAllData()
        {
            using (var client = CreateTestClient())
            {
                WaitSafely(async () =>
                {
                    var keysResult = await client.KV.Keys("");
                    if (keysResult.Response != null)
                    {
                        foreach (var key in keysResult.Response)
                        {
                            await client.KV.Delete(key);
                        }
                    }
                });
            }
        }

        private ConsulClient CreateTestClient()
        {
            return new ConsulClient();
        }
        
        private void WaitSafely(Func<Task> taskFn)
        {
            _taskFactory.StartNew(taskFn)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}