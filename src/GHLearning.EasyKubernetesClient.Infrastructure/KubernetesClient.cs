using System.Runtime.CompilerServices;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Options;
using GHLearning.EasyKubernetesClient.Core;

namespace GHLearning.EasyKubernetesClient.Infrastructure;

internal sealed class KubernetesClient : IKubernetesClient
{
	private readonly Kubernetes _kubernetes;
	private readonly TimeProvider _timeProvider;

	public KubernetesClient(
		IOptions<KubernetesOptions> options,
		TimeProvider timeProvider)
	{
		var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(options.Value.KubeconfigPath, options.Value.CurrentContext);
		_kubernetes = new Kubernetes(config);
		_timeProvider = timeProvider;
	}

	public async Task SetReplaceNamespacedDeploymentReplicasAsync(string namespaceName, string deploymentName, int replicas = 1, CancellationToken cancellationToken = default)
	{
		var deployment = await _kubernetes.ReadNamespacedDeploymentAsync(
			name: deploymentName,
			namespaceParameter: namespaceName,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		deployment.Spec.Replicas = replicas;

		await _kubernetes.ReplaceNamespacedDeploymentAsync(
			body: deployment,
			name: deploymentName,
			namespaceParameter: namespaceName,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	public async IAsyncEnumerable<NodeMetricsCstm> GetKubernetesMetricsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var nodeMetricsList = await _kubernetes.GetKubernetesNodesMetricsAsync().ConfigureAwait(false);

		foreach (var item in nodeMetricsList.Items)
		{
			var usageCPUInM = ParseUsage(item.Usage, "cpu", 1000000.0m);
			var usageMemoryInMi = ParseUsage(item.Usage, "memory", 1024.0m);

			yield return new NodeMetricsCstm(
				item.Metadata.Name,
				usageCPUInM,
				usageMemoryInMi);
		}
	}

	public async IAsyncEnumerable<PodMetricsCstm> GetKubernetesPodsMetricsAsync(string? namespaceName = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var podMetricsList = string.IsNullOrEmpty(namespaceName)
			? await _kubernetes.GetKubernetesPodsMetricsAsync().ConfigureAwait(false)
			: await _kubernetes.GetKubernetesPodsMetricsByNamespaceAsync(namespaceName).ConfigureAwait(false);

		foreach (var item in podMetricsList.Items)
		{
			var containers = new List<PodMetricsContainer>();
			foreach (var container in item.Containers)
			{
				var usageCPUInM = ParseUsage(container.Usage, "cpu", 1000000.0m);
				var usageMemoryInMi = ParseUsage(container.Usage, "memory", 1024.0m);

				containers.Add(new PodMetricsContainer(
					container.Name,
					usageCPUInM,
					usageMemoryInMi));
			}

			yield return new PodMetricsCstm(
				NamespaceName: item.Metadata.Namespace(),
				PodName: item.Metadata.Name,
				UsageCPUInM: containers.Sum(container => container.UsageCPUInM),
				UsageMemoryInMi: containers.Sum(container => container.UsageMemoryInMi),
				Containers: containers);
		}
	}

	public async IAsyncEnumerable<NamespaceCstm> ListNamespaceAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var list = await _kubernetes.ListNamespaceAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		foreach (var item in list.Items)
		{
			yield return new NamespaceCstm(
				Name: item.Name(),
				Status: item.Status.Phase,
				Labels: item.Labels());
		}
	}

	public async IAsyncEnumerable<PodDeploy> ListNamespacedPodAsync(string namespaceName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var podList = await _kubernetes.ListNamespacedPodAsync(
			namespaceParameter: namespaceName,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		foreach (var item in podList.Items)
		{
			var containers = new List<PodContainer>();
			foreach (var container in item.Spec.Containers)
			{
				var requestsCPU = "0";
				var requestsMemory = "0";
				var limitsCPU = "0";
				var limitsMemory = "0";
				if (container.Resources != null)
				{
					if (container.Resources.Requests != null)
					{
						requestsCPU = container.Resources.Requests["cpu"]?.ToString();
						requestsMemory = container.Resources.Requests["memory"]?.ToString();
					}

					if (container.Resources.Limits != null)
					{
						limitsCPU = container.Resources.Limits["cpu"]?.ToString();
						limitsMemory = container.Resources.Limits["memory"]?.ToString();
					}
				}

				containers.Add(new PodContainer(
				ContainerName: container.Name,
				ImageName: container.Image,
				RequestsCPU: requestsCPU,
				RequestsMemory: requestsMemory,
				LimitsCPU: limitsCPU,
				LimitsMemory: limitsMemory));
			}

			yield return new PodDeploy(
				NamespaceName: item.Metadata.Namespace(),
				PodName: item.Metadata.Name,
				NodeName: item.Spec.NodeName,
				Labels: item.Metadata.Labels,
				Containers: containers);
		}
	}

	public async Task RestartReplaceNamespacedDeploymentAsync(string namespaceName, string deploymentName, CancellationToken cancellationToken = default)
	{
		var deployment = await _kubernetes.ReadNamespacedDeploymentAsync(
			name: deploymentName,
			namespaceParameter: namespaceName,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		deployment.Spec.Template.Metadata.Annotations["kubectl.kubernetes.io/restartedAt"] = _timeProvider.GetUtcNow().ToString("O");

		await _kubernetes.ReplaceNamespacedDeploymentAsync(
			body: deployment,
			name: deploymentName,
			namespaceParameter: namespaceName,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<object> SetReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(string namespaceName, string deploymentName, int minReplicas = 1, int maxReplicas = 1, int cpuAverageUtilization = 50, int memoryAverageUtilization = 50, CancellationToken cancellationToken = default)
	{
		var group = "autoscaling";
		var version = "v2";
		var plural = "horizontalpodautoscalers";

		var hpa = new V2HorizontalPodAutoscaler
		{
			ApiVersion = "autoscaling/v2",
			Kind = "HorizontalPodAutoscaler",
			Metadata = new V1ObjectMeta
			{
				Name = deploymentName
			},
			Spec = new V2HorizontalPodAutoscalerSpec
			{
				ScaleTargetRef = new V2CrossVersionObjectReference
				{
					ApiVersion = "apps/v1",
					Kind = "Deployment",
					Name = deploymentName
				},
				MinReplicas = minReplicas,
				MaxReplicas = maxReplicas,
				Metrics =
				[
					new V2MetricSpec
					{
						Type = "Resource",
						Resource = new V2ResourceMetricSource
						{
							Name = "cpu",
							Target = new V2MetricTarget
							{
								Type = "Utilization",
								AverageUtilization = cpuAverageUtilization
							}
						}
					},
					new V2MetricSpec
					{
						Type = "Resource",
						Resource = new V2ResourceMetricSource
						{
							Name = "memory",
							Target = new V2MetricTarget
							{
								Type = "Utilization",
								AverageUtilization = memoryAverageUtilization
							}
						}
					}
				]
			}
		};

		try
		{
			return await _kubernetes.CreateNamespacedCustomObjectAsync(
			   body: hpa,
			   group: group,
			   version: version,
			   namespaceParameter: namespaceName,
			   plural: plural,
			   cancellationToken: cancellationToken)
			   .ConfigureAwait(false);
		}
		catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			await _kubernetes.DeleteNamespacedCustomObjectAsync(
				group: group,
				version: version,
				namespaceParameter: namespaceName,
				plural: plural,
				name: deploymentName,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return await _kubernetes.CreateNamespacedCustomObjectAsync(
			   body: hpa,
			   group: group,
			   version: version,
			   namespaceParameter: namespaceName,
			   plural: plural,
			   cancellationToken: cancellationToken)
			   .ConfigureAwait(false);
		}
	}

	public async Task DeleteReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(string namespaceName, string deploymentName, CancellationToken cancellationToken = default)
	{
		var group = "autoscaling";
		var version = "v2";
		var plural = "horizontalpodautoscalers";

		try
		{
			var result = await _kubernetes.GetNamespacedCustomObjectAsync(
			   group: group,
			   version: version,
			   namespaceParameter: namespaceName,
			   plural: plural,
			   name: deploymentName,
			   cancellationToken: cancellationToken)
			   .ConfigureAwait(false);

			await _kubernetes.DeleteNamespacedCustomObjectAsync(
				group: group,
				version: version,
				namespaceParameter: namespaceName,
				plural: plural,
				name: deploymentName,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
		}
	}

	private static decimal ParseUsage(IDictionary<string, ResourceQuantity> usage, string key, decimal divisor)
	{
		if (usage.TryGetValue(key, out var value))
		{
			if (long.TryParse(value.Value.Replace("n", "").Replace("Ki", ""), out var parsedValue))
			{
				return parsedValue / divisor;
			}
		}

		return 0.0m;
	}
}
