namespace GHLearning.EasyKubernetesClient.Core;

public record NamespaceCstm(
	string Name,
	string Status,
	IDictionary<string, string> Labels);
