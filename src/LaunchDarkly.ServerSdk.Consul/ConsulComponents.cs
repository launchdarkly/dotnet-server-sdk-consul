using System;

namespace LaunchDarkly.Client.Consul
{
    /// <summary>
    /// Obsolete entry point for the Consul integration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is retained in version 1.1 of the library for backward compatibility. For the new
    /// preferred way to configure the Consul integration, see <see cref="LaunchDarkly.Client.Integrations.Consul"/>.
    /// Updating to the latter now will make it easier to adopt version 6.0 of the LaunchDarkly .NET SDK, since
    /// an identical API is used there (except for the base namespace).
    /// </para>
    /// </remarks>
    [Obsolete("Use LaunchDarkly.Client.Integrations.Consul")]
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
