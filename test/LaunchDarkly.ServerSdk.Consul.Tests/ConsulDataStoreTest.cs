using System.Threading;
using System.Threading.Tasks;
using Consul;
using LaunchDarkly.Logging;
using LaunchDarkly.Sdk.Server.Interfaces;
using LaunchDarkly.Sdk.Server.SharedTests.DataStore;
using Xunit;
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

        [Fact]
        public void LogMessageAtStartup()
        {
            var logCapture = Logs.Capture();
            var logger = logCapture.Logger("BaseLoggerName"); // in real life, the SDK will provide its own base log name
            var context = new LdClientContext(new BasicConfiguration("", false, logger),
                LaunchDarkly.Sdk.Server.Configuration.Default(""));
            using (Consul.DataStore().Address("http://localhost:8500").Prefix("my-prefix")
                .CreatePersistentDataStore(context))
            {
                Assert.Collection(logCapture.GetMessages(),
                    m =>
                    {
                        Assert.Equal(LaunchDarkly.Logging.LogLevel.Info, m.Level);
                        Assert.Equal("BaseLoggerName.DataStore.Consul", m.LoggerName);
                        Assert.Equal("Using Consul data store at http://localhost:8500/ with prefix \"my-prefix\"",
                            m.Text);
                    });
            }
        }
    }
}
