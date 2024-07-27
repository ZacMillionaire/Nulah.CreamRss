using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Api.UnitTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public IConfiguration Configuration { get; private set; }

	/// <summary>
	/// Used to control the time provider for the created <see cref="FeedContext"/>. Defaults to a base value if not changed
	/// </summary>
	public FakeTimeProvider TimeProvider = new(new(2010, 1, 1, 0, 0, 0, TimeSpan.Zero));

	/// <summary>
	/// Used to control the database name for the created <see cref="FeedContext"/>. If null (and by default), each client created from this
	/// will use a unique in memory context. Specify a name if you wish to share context across test classes
	/// </summary>
	public string? DatabaseName = null;

	/// <summary>
	/// Provides a default set of options for use when deserialising Api responses
	/// </summary>
	public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(config =>
		{
			// Configuration = new ConfigurationBuilder()
			// 	.AddJsonFile("integrationsettings.json")
			// 	.Build();

			//config.AddConfiguration(Configuration);
		});

		builder.ConfigureTestServices(services =>
		{
			services.AddDbContext<FeedContext>(_ =>
				new InMemoryTestDatabase().CreateContext(TimeProvider, DatabaseName)
			);
			services.AddTransient<IFeedStorage>(_ =>
				new InMemoryTestFixture().CreateFeedStorage(TimeProvider, DatabaseName)
			);
		});
	}
}