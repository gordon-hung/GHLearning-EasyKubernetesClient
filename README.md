# GHLearning-EasyKubernetesClient

# Github

[![Github Actions Build](https://img.shields.io/badge/kubernetes%20client-beta-green.svg?style=flat&colorA=306CE8)](https://github.com/kubernetes-client/csharp)

# Usage

[![KubernetesClient](https://img.shields.io/nuget/v/KubernetesClient)](https://www.nuget.org/packages/KubernetesClient/)

```sh
dotnet add package KubernetesClient
```

## Authentication/Configuration
You should be able to use a standard KubeConfig file with this library,
see the `BuildConfigFromConfigFile` function below. Most authentication
methods are currently supported, but a few are not, see the
[known-issues](https://github.com/kubernetes-client/csharp#known-issues).

You should also be able to authenticate with the in-cluster service
account using the `InClusterConfig` function shown below.

## Monitoring
Metrics are built in to HttpClient using System.Diagnostics.DiagnosticsSource.
https://learn.microsoft.com/en-us/dotnet/core/diagnostics/built-in-metrics-system-net

There are many ways these metrics can be consumed/exposed but that decision is up to the application, not KubernetesClient itself.
https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-collection