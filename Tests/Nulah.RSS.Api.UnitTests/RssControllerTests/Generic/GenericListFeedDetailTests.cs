using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.Generic;

public class GenericListFeedDetailTests : WebApiFixture, IAsyncLifetime
{
	private readonly int _testFeedsToCreate = 10;
	private readonly string? _databaseName = $"{Guid.NewGuid()}";

	public GenericListFeedDetailTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// Randomly generate a database name, but create 1 instance of the API shared across all tests
		WebApiFactory.DatabaseName = _databaseName;
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void GetList_ShouldReturn_AllFeeds()
	{
		var details = await Api.GetFeeds();
		Assert.All(details, detail =>
		{
			// Honestly just checking we get data back and created/updated are the same.
			// TODO: check more specific details later when I implement getting feeds by Id
			Assert.NotNull(detail);
			Assert.NotEqual(0, detail.Id);
			Assert.StartsWith($"test title ", detail.Title);
			Assert.StartsWith($"test description", detail.Description);
			Assert.StartsWith($"test url", detail.ImageUrl);
			Assert.StartsWith($"test location", detail.Location);
			Assert.Equal(detail.CreatedUtc, detail.UpdatedUtc);
		});
	}

	/// <summary>
	/// Creates seed data for get feed detail tests
	/// </summary>
	private async Task CreateFeedDetails()
	{
		// This may/may not match the datetime in the provider, it probably should but its cold so I'm just copy and pasting
		// while my fingers have feelings
		var timeProvider = new FakeTimeProvider(new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// Create an api just for seeding with a matching database name to ensure we share the right context
		var api = new RssApi(new ApiWebApplicationFactory()
		{
			TimeProvider = timeProvider,
			DatabaseName = _databaseName
		});

		// Advance time to avoid testing against the initial value
		timeProvider.Advance(new TimeSpan(100));


		// Generate 10 feed details from 1 .. 10
		for (var i = 1; i <= _testFeedsToCreate; i++)
		{
			// Create it with the current value of i to pretend to have unique values
			var newFeedDetail = new FeedDetail()
			{
				Description = $"test description {i}",
				Title = $"test title {i}",
				ImageUrl = $"test url {i}",
				Location = $"test location {i}"
			};

			var createdFeedDetail = await api.CreateRssFeedByDetail(newFeedDetail);

			Assert.NotNull(createdFeedDetail);
			Assert.Equal(i, createdFeedDetail.Id);
			Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
			Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
			Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
			// Assert times against the current timeprovider
			Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
			Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);
			// Advance time by the value of i
			timeProvider.Advance(new TimeSpan(i));
		}
	}

	public async Task InitializeAsync()
	{
		// Check if we've seeded the data before by checking the fixture, and seeding data once if we havent
		if (!DataSeeded)
		{
			await CreateFeedDetails();
			// Prevent subsequent tests in this class from seeding again, as this will run each test method executed
			DataSeeded = true;
		}
	}

	public async Task DisposeAsync()
	{
		await Task.Yield();
	}
}