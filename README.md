# LaunchDarkly Server-Side SDK for .NET - Consul integration

[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-consul.svg?style=svg)](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-consul)

This library provides a Consul-backed persistence mechanism (feature store) for the [LaunchDarkly server-side .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk), replacing the default in-memory feature store. It uses [this open-source Consul client library](https://github.com/PlayFab/consuldotnet).

The minimum version of the LaunchDarkly server-side .NET SDK for use with this library is 5.6.4.

For more information, see also: [Using a persistent feature store](https://docs.launchdarkly.com/v2.0/docs/using-a-persistent-feature-store).

## .NET platform compatibility

This version of the library is compatible with .NET Framework version 4.5 and above, .NET Standard 1.6, and .NET Standard 2.0.

## Quick setup

1. Use [NuGet](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) to add this package to your project:

        Install-Package LaunchDarkly.ServerSdk.Consul

2. Import the package (note that the namespace is different from the package name):

        using LaunchDarkly.Client.Consul;

3. When configuring your `LDClient`, add the Consul feature store:

        Configuration ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .WithFeatureStoreFactory(ConsulComponents.ConsulFeatureStore());
        LdClient ldClient = new LdClient(ldConfig);

4. Optionally, you can change the Consul configuration by calling methods on the builder returned by `ConsulFeatureStore()`:

        Configuration ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .WithFeatureStoreFactory(
                ConsulComponents.ConsulFeatureStore()
                    .WithAddress(new Uri("http://my-consul-host:8500"))
            );
        LdClient ldClient = new LdClient(ldConfig);

5. If you are running a [LaunchDarkly Relay Proxy](https://github.com/launchdarkly/ld-relay) instance, you can use it in [daemon mode](https://github.com/launchdarkly/ld-relay#daemon-mode), so that the SDK retrieves flag data only from Redis and does not communicate directly with LaunchDarkly. This is controlled by the SDK's `UseLdd` option:

        Configuration ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .WithFeatureStoreFactory(ConsulComponents.ConsulFeatureStore())
            .WithUseLdd(true);
        LdClient ldClient = new LdClient(ldConfig);

## Caching behavior

To reduce traffic to Consul, there is an optional in-memory cache that retains the last known data for a configurable amount of time. This is on by default; to turn it off (and guarantee that the latest feature flag data will always be retrieved from Consul for every flag evaluation), configure the builder as follows:

                ConsulComponents.ConsulFeatureStore()
                    .WithCaching(FeatureStoreCacheConfig.Disabled)

Or, to cache for longer than the default of 30 seconds:

                ConsulComponents.ConsulFeatureStore()
                    .WithCaching(FeatureStoreCacheConfig.Enabled.WithTtlSeconds(60))

## Signing

The published version of this assembly is strong-named. Building the code locally in the default Debug configuration does not use strong-naming and does not require a key file.

## Development notes

This project imports the `dotnet-base` and `dotnet-server-sdk-shared-tests` repositories as subtrees. See the `README.md` file in each of those directories for more information.

To run unit tests, you must have a local Consul server. More information [here](https://learn.hashicorp.com/consul/getting-started/install).

Releases are done using the release script in `dotnet-base`. Since the published package includes a .NET Framework 4.5 build, the release must be done from Windows.

## About LaunchDarkly
 
* LaunchDarkly is a continuous delivery platform that provides feature flags as a service and allows developers to iterate quickly and safely. We allow you to easily flag your features and manage them from the LaunchDarkly dashboard.  With LaunchDarkly, you can:
    * Roll out a new feature to a subset of your users (like a group of users who opt-in to a beta tester group), gathering feedback and bug reports from real-world use cases.
    * Gradually roll out a feature to an increasing percentage of users, and track the effect that the feature has on key metrics (for instance, how likely is a user to complete a purchase if they have feature A versus feature B?).
    * Turn off a feature that you realize is causing performance problems in production, without needing to re-deploy, or even restart the application with a changed configuration file.
    * Grant access to certain features based on user attributes, like payment plan (eg: users on the ‘gold’ plan get access to more features than users in the ‘silver’ plan). Disable parts of your application to facilitate maintenance, without taking everything offline.
* LaunchDarkly provides feature flag SDKs for a wide variety of languages and technologies. Check out [our documentation](https://docs.launchdarkly.com/docs) for a complete list.
* Explore LaunchDarkly
    * [launchdarkly.com](https://www.launchdarkly.com/ "LaunchDarkly Main Website") for more information
    * [docs.launchdarkly.com](https://docs.launchdarkly.com/  "LaunchDarkly Documentation") for our documentation and SDK reference guides
    * [apidocs.launchdarkly.com](https://apidocs.launchdarkly.com/  "LaunchDarkly API Documentation") for our API documentation
    * [blog.launchdarkly.com](https://blog.launchdarkly.com/  "LaunchDarkly Blog Documentation") for the latest product updates
    * [Feature Flagging Guide](https://github.com/launchdarkly/featureflags/  "Feature Flagging Guide") for best practices and strategies
