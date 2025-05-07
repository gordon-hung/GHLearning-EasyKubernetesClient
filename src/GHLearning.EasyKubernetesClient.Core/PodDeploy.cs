namespace GHLearning.EasyKubernetesClient.Core;

public record PodDeploy(
	string NamespaceName,
	string PodName,
	string NodeName,
	IDictionary<string, string> Labels,
	IReadOnlyCollection<PodContainer> Containers);
