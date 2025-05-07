namespace GHLearning.EasyKubernetesClient.Core;

public record PodContainer(
	string ContainerName,
	string ImageName,
	string? RequestsCPU,
	string? RequestsMemory,
	string? LimitsCPU,
	string? LimitsMemory);
