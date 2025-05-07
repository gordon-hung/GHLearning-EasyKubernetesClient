namespace GHLearning.EasyKubernetesClient.Core;

public record PodMetricsContainer(
	string ContainerName,
	decimal UsageCPUInM,
	decimal UsageMemoryInMi);
