using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
			// adapting from:
			// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#customize-webapplicationfactory
			// remove the existing context configuration by removing the context itself, and the context options
			if (services.SingleOrDefault(d => d.ServiceType == typeof(FeedContext))
			    is { } context)
			{
				services.Remove(context);
			}

			// If we don't also do this, the configuration set in program.cs for the api will still be used which can
			// lead to unintended behaviours
			if (services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FeedContext>))
			    is { } feedContextOptions)
			{
				services.Remove(feedContextOptions);
			}

			// The below might not make full sense, but it ensures that default api behaviours (such as migrations),
			// will still pass but also ensure we don't create a db file for no reason.
			// This has no impact on tests as they'll be using IFeedStorage which gets a db context we _actually_
			// control.
			// Create open SqliteConnection so EF won't automatically close it.
			services.AddSingleton<DbConnection>(container =>
			{
				var connection = new SqliteConnection("DataSource=:memory:");
				connection.Open();

				return connection;
			});

			services.AddDbContext<FeedContext>((container, options) =>
			{
				var connection = container.GetRequiredService<DbConnection>();
				options.UseSqlite(connection);
			});

			// What the api will actually be testing against for anything hitting a FeedContext.
			// The above is simply to allow program.cs in the Api project to run to completion.
			// Can't say I'm super happy about this but it only affects tests and lets be real this is a godless arena
			// and we can do anything here so long as tests pass (not actually but I'll regret this later instead).
			services.AddTransient<IFeedStorage>(_ =>
				new InMemoryTestFixture().CreateFeedStorage(TimeProvider, DatabaseName)
			);
		});
	}
}