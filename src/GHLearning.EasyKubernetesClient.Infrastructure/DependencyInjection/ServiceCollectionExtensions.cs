using GHLearning.EasyKubernetesClient.Core;
using GHLearning.EasyKubernetesClient.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		Action<KubernetesOptions, IServiceProvider> kubernetesOptions)
		=> services
		.AddOptions<KubernetesOptions>()
		.Configure(kubernetesOptions)
		.Services
		.AddSingleton(TimeProvider.System)
		.AddTransient<IKubernetesClient, KubernetesClient>();
}
