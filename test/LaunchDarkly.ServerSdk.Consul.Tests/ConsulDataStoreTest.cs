using System.Threading;
using System.Threading.Tasks;
using Consul;
using LaunchDarkly.Sdk.Server.Interfaces;
using LaunchDarkly.Sdk.Server.SharedTests.DataStore;
using Xunit.Abstractions;

namespace LaunchDarkly.Sdk.Server.Integrations
{
    public class ConsulDataStoreTest : PersistentDataStoreBaseTests
    {
        private static readonly TaskFactory _taskFactory = new TaskFactory(CancellationToken.None,
            TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        protected override PersistentDataStoreTestConfig Configuration =>
            new PersistentDataStoreTestConfig
            {
                StoreAsyncFactoryFunc = MakeStoreFactory,
                ClearDataAction = ClearAllData
            };

        public ConsulDataStoreTest(ITestOutputHelper testOutput) : base(testOutput) { }

        private IPersistentDataStoreAsyncFactory MakeStoreFactory(string prefix) =>
            Consul.DataStore().Prefix(prefix);

        private async Task ClearAllData(string prefix)
        {
            using (var client = new ConsulClient())
            {
                var keysResult = await client.KV.Keys(prefix ?? Consul.DefaultPrefix);
                if (keysResult.Response != null)
                {
                    foreach (var key in keysResult.Response)
                    {
                        await client.KV.Delete(key);
                    }
                }
            }
        }        
    }
}
