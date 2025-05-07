using System.Net;
using System.Net.Sockets;
using System.Reflection;
using GHLearning.EasyKubernetesClient.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GHLearning.EasyKubernetesClient.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InfoController(
	IOptions<KubernetesOptions> kubernetesOptions) : ControllerBase
{
	/// <summary>
	/// Gets the asynchronous.
	/// </summary>
	/// <param name="hostingEnvironment">The hosting environment.</param>
	/// <param name="configuration">The configuration.</param>
	/// <returns></returns>
	[HttpGet]
	public async Task<object> GetAsync(
		[FromServices] IWebHostEnvironment hostingEnvironment,
		[FromServices] IConfiguration configuration)
	{
		var hostName = Dns.GetHostName();
		var hostEntry = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
		var hostIp = Array.Find(hostEntry.AddressList,
			x => x.AddressFamily == AddressFamily.InterNetwork);

		return new
		{
			Environment.MachineName,
			HostName = hostName,
			HostIp = hostIp?.ToString() ?? string.Empty,
			Environment = hostingEnvironment.EnvironmentName,
			OsVersion = $"{Environment.OSVersion}",
			Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
			ProcessCount = Environment.ProcessorCount,
			AspnetcoreHttpsPorts = configuration["ASPNETCORE_HTTPS_PORTS"],
			AspnetcoreHttpPorts = configuration["ASPNETCORE_HTTP_PORTS"],
			AspnetcoreEnvironment = configuration["ASPNETCORE_ENVIRONMENT"],
			Kubernetes = kubernetesOptions.Value,
		};
	}
}
