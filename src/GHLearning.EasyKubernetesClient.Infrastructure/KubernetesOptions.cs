namespace GHLearning.EasyKubernetesClient.Infrastructure;

public record KubernetesOptions
{
	public string KubeconfigPath { get; set; } = default!;
	public string CurrentContext { get; set; } = default!;
}
