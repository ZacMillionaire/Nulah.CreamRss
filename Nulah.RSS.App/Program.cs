using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nulah.RSS.App.Services;

namespace Nulah.RSS.App;

public class Program
{
	public static async Task Main(string[] args)
	{
		// Default blazor hookups
		var builder = WebAssemblyHostBuilder.CreateDefault(args);
		builder.RootComponents.Add<App>("#app");
		builder.RootComponents.Add<HeadOutlet>("head::after");

		// HttpClient to talk to the backend Api
		// TODO: maybe make this keyed/bound to a particular type for the FeedService
		builder.Services.AddScoped(sp =>
			// TODO: change this to configuration, for now its hardcoded to the api https address
			new HttpClient { BaseAddress = new Uri("https://localhost:7133") }
		);
		// Handles interacting with the Api
		builder.Services.AddScoped<FeedService>();

		await builder.Build().RunAsync();
	}
}