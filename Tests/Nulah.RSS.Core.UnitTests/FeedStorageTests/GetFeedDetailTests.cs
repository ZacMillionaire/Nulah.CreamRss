using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class GetFeedDetailTests : IClassFixture<InMemoryTestFixture>, IAsyncLifetime
{
	private readonly InMemoryTestFixture _fixture;
	private readonly string _databaseName = "GetFeedDetailsTests";
	private readonly DateTimeOffset _testTimeOffset = new(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
	private readonly int _testFeedsToCreate = 10;

	/// <summary>
	/// Returns a new <see cref="FakeTimeProvider"/> set to the same value of _testTimeOffset.
	/// <para>This guarantees the same starting time provider for all tests</para>
	/// </summary>
	private FakeTimeProvider TimeProvider => new(_testTimeOffset);

	public GetFeedDetailTests(InMemoryTestFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async void Get_FeedByZeroId_Should_ReturnNull()
	{
		var feedStorage = _fixture.CreateFeedStorage(null, _databaseName);
		var fetchedFeed = await feedStorage.GetFeedDetail(0);

		Assert.Null(fetchedFeed);
	}

	[Fact]
	public async void Get_FeedByNegativeId_Should_ReturnNull()
	{
		var feedStorage = _fixture.CreateFeedStorage(null, _databaseName);
		var fetchedFeed = await feedStorage.GetFeedDetail(-1);

		Assert.Null(fetchedFeed);
	}

	[Fact]
	public async void Get_FeedsById_Should_FetchFeedDetails_With_MatchingDetails()
	{
		var timeProvider = TimeProvider;

		var feedStorage = _fixture.CreateFeedStorage(timeProvider, _databaseName);

		for (var i = 1; i <= _testFeedsToCreate; i++)
		{
			var fetchedFeed = await feedStorage.GetFeedDetail(i);

			Assert.NotNull(fetchedFeed);
			Assert.Equal(i, fetchedFeed.Id);
			Assert.Equal($"test title {i}", fetchedFeed.Title);
			Assert.Equal($"test description {i}", fetchedFeed.Description);
			Assert.Equal($"test url {i}", fetchedFeed.ImageUrl);
			Assert.Equal(timeProvider.GetUtcNow(), fetchedFeed.CreatedUtc);
			Assert.Equal(timeProvider.GetUtcNow(), fetchedFeed.UpdatedUtc);

			// Advance time to match seeded data
			timeProvider.Advance(new TimeSpan(i));
		}
	}

	[Fact]
	public async void Get_FeedDetailsList_Should_ReturnAllFeeds()
	{
		var feedStorage = _fixture.CreateFeedStorage(null, _databaseName);
		var feeds = await feedStorage.GetFeedDetails();

		Assert.Equal(_testFeedsToCreate, feeds.Count);
	}

	[Fact]
	public async void Get_FeedDetailsList_WithNoFeedDetailsSaved_Should_ReturnEmptyList()
	{
		// Omit the database name to test against an empty database
		var feedStorage = _fixture.CreateFeedStorage(null);
		var feeds = await feedStorage.GetFeedDetails();

		Assert.Equal(0, feeds.Count);
	}

	/// <summary>
	/// Creates seed data for get feed detail tests
	/// </summary>
	private async Task CreateFeedDetails()
	{
		var testTimeProvider = TimeProvider;
		var feedStorage = _fixture.CreateFeedStorage(testTimeProvider, _databaseName);

		// Generate 10 feed details from 1 .. 10
		for (var i = 1; i <= _testFeedsToCreate; i++)
		{
			// Create it with the current value of i to pretend to have unique values
			var newFeedDetail = new FeedDetail()
			{
				Description = $"test description {i}",
				Title = $"test title {i}",
				ImageUrl = $"test url {i}"
			};

			var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

			Assert.NotNull(createdFeedDetail);
			Assert.Equal(i, createdFeedDetail.Id);
			Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
			Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
			Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
			// Assert times against the current timeprovider
			Assert.Equal(testTimeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
			Assert.Equal(testTimeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);
			// Advance time by the value of i
			testTimeProvider.Advance(new TimeSpan(i));
		}
	}

	public async Task InitializeAsync()
	{
		// Check if we've seeded the data before by checking the fixture, and seeding data once if we havent
		if (!_fixture.DataSeeded)
		{
			await CreateFeedDetails();
			// Prevent subsequent tests in this class from seeding again, as this will run each test method executed
			_fixture.DataSeeded = true;
		}
	}

	public async Task DisposeAsync()
	{
		await Task.Yield();
	}
}