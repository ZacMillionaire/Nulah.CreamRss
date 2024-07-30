using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nulah.RSS.Core;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Api;

[ExcludeFromCodeCoverage]
public class Program
{
	private static readonly string CorsPolicyName = "CorsPolicy";

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.

		builder.Services.AddControllers();
		builder.Services.AddCors(policy =>
		{
			policy.AddPolicy(CorsPolicyName, opt => opt
				.WithOrigins([
					// Default origins to 5001/5002 for backend/frontend localhost respectively if not configured in
					// app settings
					builder.Configuration["BackendUrl"] ?? "https://localhost:5001",
					builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"
				])
				.AllowAnyHeader()
				.AllowAnyMethod());
		});

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddDbContext<FeedContext>(opts =>
			// Set the database location to be the same as the assembly executing directory as this is intended to
			// only be a data-on-disk application, no remote database connections.
			opts.UseSqlite($"Data Source={AppContext.BaseDirectory}/nulah.rss.db")
		);
		builder.Services.AddTransient<IFeedReader, FeedReader>();
		builder.Services.AddTransient<IFeedManager, FeedManager>();
		builder.Services.AddTransient<IFeedStorage, FeedStorage>();

		var app = builder.Build();

		// Use if DEBUG here to ensure there is no way to launch the swagger api in release builds
		// by not even letting it be compiled!
		// Don't worry about EnsureExists as it has a conditional attribute that creates a
		// release-safe empty method.
#if DEBUG
		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			// Only run this during development to make sure the database is using latest migrations each run
			using (var serviceScope = app.Services.CreateScope())
			{
				var ctx = serviceScope.ServiceProvider.GetRequiredService<FeedContext>();
				ctx.EnsureExists();
			}

			app.UseSwagger();
			app.UseSwaggerUI();
		}
#endif

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.UseCors(CorsPolicyName);

		app.MapControllers();

		app.Run();
	}
}