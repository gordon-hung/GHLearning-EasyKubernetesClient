namespace GHLearning.EasyKubernetesClient.Core;

public record PodMetricsCstm(
	string NamespaceName,
	string PodName,
	decimal UsageCPUInM,
	decimal UsageMemoryInMi,
	IReadOnlyCollection<PodMetricsContainer> Containers);
