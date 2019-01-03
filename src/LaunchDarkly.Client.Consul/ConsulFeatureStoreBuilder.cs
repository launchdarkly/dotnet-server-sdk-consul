﻿using System;
using System.Collections.Generic;
using Consul;
using LaunchDarkly.Client.Utils;

namespace LaunchDarkly.Client.Consul
{
    /// <summary>
    /// Builder for a Consul-based implementation of <see cref="IFeatureStore"/>.
    /// Create an instance of the builder by calling <see cref="ConsulComponents.ConsulFeatureStore"/>;
    /// configure it using the setter methods; then pass the builder to
    /// <see cref="ConfigurationExtensions.WithFeatureStore(Configuration, IFeatureStore)"/>.
    /// 
    /// The Consul client has many configuration options. This class has shortcut methods for
    /// some of them, but if you need more sophisticated control over the Consul client, use
    /// <see cref="WithConfig(Action{ConsulClientConfiguration})"/> or
    /// <see cref="WithExistingClient(ConsulClient)"/>.
    /// </summary>
    public sealed class ConsulFeatureStoreBuilder : IFeatureStoreFactory
    {
        public const string DefaultPrefix = "launchdarkly";

        private ConsulClient _existingClient;
        private List<Action<ConsulClientConfiguration>> _configActions = new List<Action<ConsulClientConfiguration>>();
        private Uri _address;
        private string _prefix = DefaultPrefix;
        private FeatureStoreCacheConfig _caching = FeatureStoreCacheConfig.Enabled;
        
        internal ConsulFeatureStoreBuilder() { }

        /// <summary>
        /// Creates a feature store instance based on the currently configured builder.
        /// </summary>
        /// <returns>the feature store</returns>
        public IFeatureStore CreateFeatureStore()
        {
            var core = new ConsulFeatureStoreCore(MakeClient(), _prefix);
            return CachingStoreWrapper.Builder(core).WithCaching(_caching).Build();
        }

        private ConsulClient MakeClient()
        {
            if (_existingClient != null)
            {
                return _existingClient;
            }
            return new ConsulClient(config => {
                if (_address != null)
                {
                    config.Address = _address;
                }
                foreach (var action in _configActions)
                {
                    action.Invoke(config);
                }
            });
        }

        /// <summary>
        /// Specifies the Consul agent's location.
        /// </summary>
        /// <param name="address">the URI of the Consul host</param>
        /// <returns>the builder</returns>
        public ConsulFeatureStoreBuilder WithAddress(Uri address)
        {
            _address = address;
            return this;
        }

        /// <summary>
        /// Specifies custom steps for configuring the Consul client. Your action may modify the
        /// <see cref="ConsulClientConfiguration"/> object in any way.
        /// </summary>
        /// <param name="configAction">an action for modifying the configuration</param>
        /// <returns>the builder</returns>
        public ConsulFeatureStoreBuilder WithConfig(Action<ConsulClientConfiguration> configAction)
        {
            if (configAction != null)
            {
                _configActions.Add(configAction);
            }
            return this;
        }

        /// <summary>
        /// Specifies an existing, already-configured Consul client instance that the feature store
        /// should use rather than creating one of its own. If you specify an existing client, then the
        /// other builder methods for configuring Consul are ignored.
        /// </summary>
        /// <param name="client">an existing Consul client instance</param>
        /// <returns>the builder</returns>
        public ConsulFeatureStoreBuilder WithExistingClient(ConsulClient client)
        {
            _existingClient = client;
            return this;
        }
        
        /// <summary>
        /// Specifies whether local caching should be enabled and if so, sets the cache properties. Local
        /// caching is enabled by default; see <see cref="FeatureStoreCacheConfig.Enabled"/>. To disable it, pass
        /// <see cref="FeatureStoreCacheConfig.Disabled"/> to this method.
        /// </summary>
        /// <param name="caching">a <see cref="FeatureStoreCacheConfig"/> object specifying caching parameters</param>
        /// <returns>the builder</returns>
        public ConsulFeatureStoreBuilder WithCaching(FeatureStoreCacheConfig caching)
        {
            _caching = caching;
            return this;
        }

        /// <summary>
        /// Sets an optional namespace prefix for all keys stored in Consul. Use this if you are sharing
        /// the same database table between multiple clients that are for different LaunchDarkly
        /// environments, to avoid key collisions.
        /// </summary>
        /// <param name="prefix">the namespace prefix</param>
        /// <returns>the builder</returns>
        public ConsulFeatureStoreBuilder WithPrefix(string prefix)
        {
            _prefix = String.IsNullOrEmpty(prefix) ? DefaultPrefix : prefix;
            return this;
        }
    }
}
