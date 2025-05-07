namespace GHLearning.EasyKubernetesClient.Core;

public interface IKubernetesClient
{
	Task SetReplaceNamespacedDeploymentReplicasAsync(string namespaceName, string deploymentName, int replicas = 1, CancellationToken cancellationToken = default);

	Task RestartReplaceNamespacedDeploymentAsync(string namespaceName, string deploymentName, CancellationToken cancellationToken = default);

	IAsyncEnumerable<NamespaceCstm> ListNamespaceAsync(CancellationToken cancellationToken = default);

	IAsyncEnumerable<PodDeploy> ListNamespacedPodAsync(string namespaceName, CancellationToken cancellationToken = default);

	IAsyncEnumerable<PodMetricsCstm> GetKubernetesPodsMetricsAsync(string? namespaceName = null, CancellationToken cancellationToken = default);

	IAsyncEnumerable<NodeMetricsCstm> GetKubernetesMetricsAsync(CancellationToken cancellationToken = default);

	Task<object> SetReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(string namespaceName, string deploymentName, int minReplicas = 1, int maxReplicas = 1, int cpuAverageUtilization = 50, int memoryAverageUtilization = 50, CancellationToken cancellationToken = default);

	Task DeleteReplaceNamespacedDeploymentHorizontalPodAutoscalerAsync(string namespaceName, string deploymentName, CancellationToken cancellationToken = default);
}
