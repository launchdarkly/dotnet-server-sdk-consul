# LaunchDarkly Server-Side SDK for .NET - Consul integration

[![CircleCI](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-consul.svg?style=svg)](https://circleci.com/gh/launchdarkly/dotnet-server-sdk-consul)

This library provides a Consul-backed persistence mechanism (data store) for the [LaunchDarkly server-side .NET SDK](https://github.com/launchdarkly/dotnet-server-sdk), replacing the default in-memory data store. It uses [this open-source Consul client library](https://github.com/PlayFab/consuldotnet).

For more information, see also: [Using a persistent data store](https://docs.launchdarkly.com/v2.0/docs/using-a-persistent-feature-store).

Version 2.0.0 and above of this library works with version 6.0.0 and above of the LaunchDarkly .NET SDK. For earlier versions of the SDK, use the latest 1.x release of this library.

## Supported .NET versions

This version of the library is built for the following targets:

* .NET Framework 4.5.2: runs on .NET Framework 4.5.x and above.
* .NET Standard 2.0: runs on .NET Core 2.x and 3.x, or .NET 5, in an application; or within a library that is targeted to .NET Standard 2.x or .NET 5.

The .NET build tools should automatically load the most appropriate build of the library for whatever platform your application or library is targeted to.

## Quick setup

1. Use [NuGet](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) to add this package to your project:

        Install-Package LaunchDarkly.ServerSdk.Consul

2. Import the package (note that the namespace is different from the package name):

        using LaunchDarkly.Sdk.Server.Integrations;

3. When configuring your `LdClient`, add the Consul data store as a `PersistentDataStore`. You may specify any custom Consul options using the methods of `ConsulDataStoreBuilder`. For instance, to customize the Redis URI:

        var ldConfig = Configuration.Default("YOUR_SDK_KEY")
            .DataStore(
                Components.PersistentDataStore(
                    Consul.DataStore().Address("http://my-consul-host:8500")
                )
            )
            .Build();
        var ldClient = new LdClient(ldConfig);

By default, the store will try to connect to a local Consul instance on port 8500.

## Caching behavior

The LaunchDarkly SDK has a standard caching mechanism for any persistent data store, to reduce database traffic. This is configured through the SDK's `PersistentDataStoreBuilder` class as described in the SDK documentation. For instance, to specify a cache TTL of 5 minutes:

        var config = Configuration.Default("YOUR_SDK_KEY")
            .DataStore(
                Components.PersistentDataStore(
                    Consul.DataStore().Address("http://my-consul-host:8500")
                ).CacheTime(TimeSpan.FromMinutes(5))
            )
            .Build();

## How the SDK uses Consul

The Consul integrations for all LaunchDarkly server-side SDKs use the same conventions, so that SDK instances and Relay Proxy instances sharing a single Consul store can interoperate correctly. The storage schema is as follows:

* There is always a "prefix" string that provides a namespace for the overall data set. If you do not specify a prefix in your configuration, it is `launchdarkly`.

* For each data item that the SDK can store, such as a feature flag, there is a Consul key-value pair where the key is `PREFIX/TYPE/KEY`. `PREFIX` is the configured prefix string. `TYPE` denotes the type of data; currently, the types are `features` and `segments`, but this is subject to change in the future. `KEY` is the unique key of the item (such as the flag key for a feature flag). The value is a serialized representation of that item, in a format that is determined by the SDK.

* An additional key, `PREFIX/$inited`, is created with an arbitrary value when the SDK stores a full set of feature flag data. This allows a new SDK instance to check whether there is already a valid data set that was stored earlier.

* The SDK will never add, modify, or remove any keys in Consul other than the ones described above, so it is safe to share a Consul instance that is also being used for other purposes.

## Signing

The published version of this assembly is strong-named. Building the code locally in the default Debug configuration does not use strong-naming and does not require a key file.

## Contributing

See [Contributing](./CONTRIBUTING.md).

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
