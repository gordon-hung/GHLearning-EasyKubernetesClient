using GHLearning.EasyKubernetesClient.Core;
using GHLearning.EasyKubernetesClient.WebApi.ViewModels;

using Microsoft.AspNetCore.Mvc;

namespace GHLearning.EasyKubernetesClient.WebApi.Controllers;

[Route("api/K8s")]
[ApiController]
public class KubernetesController(
	IKubernetesClient client) : ControllerBase
{
	[HttpGet]
	public IAsyncEnumerable<NamespaceCstm> ListNamespaceAsync()
		=> client.ListNamespaceAsync(HttpContext.RequestAborted);

	[HttpGet("Nodes/Metrics")]
	public IAsyncEnumerable<NodeMetricsCstm> GetKubernetesMetricsAsync()
		=> client.GetKubernetesMetricsAsync(cancellationToken: HttpContext.RequestAborted);

	[HttpGet("{namespaceName}")]
	public IAsyncEnumerable<PodDeploy> ListNamespacedPodAsync(
		string namespaceName)
		=> client.ListNamespacedPodAsync(namespaceName, HttpContext.RequestAborted);

	[HttpGet("Pods/Metrics")]
	public IAsyncEnumerable<PodMetricsCstm> GetKubernetesPodsMetricsAsync(
		[FromQuery] string? namespaceName)
		=> client.GetKubernetesPodsMetricsAsync(namespaceName, HttpContext.RequestAborted);

	[HttpPatch("{namespaceName}/{deploymentName}/Replicas")]
	public Task SetReplaceNamespacedDeploymentReplicasAsync(
		string namespaceName,
		string deploymentName,
		[FromBody] int replicas)
		=> client.SetReplaceNamespacedDeploymentReplicasAsync(namespaceName, deploymentName, replicas, HttpContext.RequestAborted);

	[HttpPost("{namespaceName}/{deploymentName}/Restart")]
	public Task RestartReplaceNamespacedDeploymentAsync(
		string namespaceName,
		string deploymentName)
		=> client.RestartReplaceNamespacedDeploymentAsync(namespaceName, deploymentName, HttpContext.RequestAborted);

	[HttpPatch("{namespaceName}/{deploymentName}/HorizontalPodAutoscaler")]
	public Task<object> SetReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(
		string namespaceName,
		string deploymentName,
		[FromBody] HorizontalPodAutoscalerViewModel model)
		=> client.SetReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(
			namespaceName: namespaceName,
			deploymentName: deploymentName,
			minReplicas: model.MinReplicas,
			maxReplicas: model.MaxReplicas,
			cpuAverageUtilization: model.CpuAverageUtilization,
			memoryAverageUtilization: model.MemoryAverageUtilization,
			cancellationToken: HttpContext.RequestAborted);

	[HttpDelete("{namespaceName}/{deploymentName}/HorizontalPodAutoscaler")]
	public Task DeleteReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(
		string namespaceName,
		string deploymentName)
		=> client.DeleteReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(
			namespaceName: namespaceName,
			deploymentName: deploymentName,
			cancellationToken: HttpContext.RequestAborted);
}
