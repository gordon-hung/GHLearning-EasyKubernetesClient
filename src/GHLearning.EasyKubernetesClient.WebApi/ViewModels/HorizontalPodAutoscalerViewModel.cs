namespace GHLearning.EasyKubernetesClient.WebApi.ViewModels;

public record HorizontalPodAutoscalerViewModel(
	int MinReplicas = 1,
	int MaxReplicas = 1,
	int CpuAverageUtilization = 50,
	int MemoryAverageUtilization = 50);
