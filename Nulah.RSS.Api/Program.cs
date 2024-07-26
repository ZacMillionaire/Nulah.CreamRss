using Nulah.RSS.Core;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Api;

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

		builder.Services.AddDbContext<FeedContext>();
		builder.Services.AddTransient<IFeedReader, FeedReader>();
		builder.Services.AddTransient<IFeedManager, FeedManager>();
		builder.Services.AddTransient<IFeedStorage, FeedStorage>();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.UseCors(CorsPolicyName);

		app.MapControllers();

		app.Run();
	}
}