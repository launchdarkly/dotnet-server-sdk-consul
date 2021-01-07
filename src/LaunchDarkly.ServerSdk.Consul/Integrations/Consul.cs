
namespace LaunchDarkly.Client.Integrations
{
    /// <summary>
    /// Integration between the LaunchDarkly SDK and Consul.
    /// </summary>
    public static class Consul
    {
        /// <summary>
        /// The default value for <see cref="ConsulDataStoreBuilder.Prefix"/>.
        /// </summary>
        public static readonly string DefaultPrefix = "launchdarkly";

        /// <summary>
        /// Returns a builder object for creating a Consul-backed data store.
        /// </summary>
        /// <remarks>
        /// This object can be modified with <see cref="ConsulDataStoreBuilder"/> methods for any desired
        /// custom Redis options. Then, pass it to <see cref="Components.PersistentDataStore(Interfaces.IPersistentDataStoreAsyncFactory)"/>
        /// and set any desired caching options. Finally, pass the result to <see cref="IConfigurationBuilder.DataStore(Interfaces.IFeatureStoreFactory)"/>.
        /// </remarks>
        /// <example>
        /// <code>
        ///     var config = Configuration.Builder("sdk-key")
        ///         .DataStore(
        ///             Components.PersistentDataStore(
        ///                 Consul.DataStore().Url("redis://my-redis-host")
        ///             ).CacheSeconds(15)
        ///         )
        ///         .Build();
        /// </code>
        /// </example>
        public static ConsulDataStoreBuilder DataStore() =>
            new ConsulDataStoreBuilder();
    }
}
