namespace GHLearning.EasyKubernetesClient.Core;

public record NodeMetricsCstm(
	string Node,
	decimal UsageCPUInM,
	decimal UsageMemoryInMi);
