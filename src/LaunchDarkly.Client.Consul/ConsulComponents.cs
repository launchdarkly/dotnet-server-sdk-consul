namespace LaunchDarkly.Client.Consul
{
    /// <summary>
    /// Entry point for using the Consul feature store with the LaunchDarkly SDK.
    /// 
    /// For more details about how and why you can use a persistent feature store, see:
    /// https://docs.launchdarkly.com/v2.0/docs/using-a-persistent-feature-store
    /// 
    /// To use the Consul feature store with the LaunchDarkly client, you will first obtain a
    /// builder by calling <see cref="ConsulComponents.ConsulFeatureStore()"/>,
    /// then optionally modify its properties, and then include it in your client configuration.
    /// For example:
    /// 
    /// <code>
    /// using LaunchDarkly.Client;
    /// using LaunchDarkly.Client.Consul;
    /// 
    /// var store = ConsulComponents.ConsulFeatureStore()
    ///     .WithCaching(FeatureStoreCaching.Enabled.WithTtlSeconds(30));
    /// var config = Configuration.Default("my-sdk-key")
    ///     .WithFeatureStoreFactory(store);
    /// </code>
    /// 
    /// The default Consul configuration uses an address of <code>localhost:8500</code>. To customize any
    /// properties of Consul, you can use the methods on <see cref="ConsulFeatureStoreBuilder"/>.
    /// 
    /// If you are using the same Consul host as a feature store for multiple LaunchDarkly
    /// environments, use the <see cref="ConsulFeatureStoreBuilder.WithPrefix(string)"/>
    /// option and choose a different prefix string for each, so they will not interfere with each
    /// other's data. 
    /// </summary>
    public abstract class ConsulComponents
    {
        /// <summary>
        /// Creates a builder for a Consul feature store. You can modify any of the store's properties with
        /// <see cref="ConsulFeatureStoreBuilder"/> methods before adding it to your client configuration
        /// with <see cref="ConfigurationExtensions.WithFeatureStoreFactory(Configuration, IFeatureStoreFactory)"/>.
        /// </summary>
        /// <returns>a builder</returns>
        public static ConsulFeatureStoreBuilder ConsulFeatureStore()
        {
            return new ConsulFeatureStoreBuilder();
        }
    }
}
