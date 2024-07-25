using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class UpdateFeedDetailTests : IClassFixture<InMemoryTestFixture>
{
	private readonly InMemoryTestFixture _fixture;
	
	public UpdateFeedDetailTests(InMemoryTestFixture inMemoryTestFixture)
	{
		_fixture = inMemoryTestFixture;
	}

	[Fact]
	public async void Update_FeedDetail_With_FullDetails_ShouldReturn_FeedDetailWithSameId()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		// Create a new FeedDetail
		var newFeedDetail = new FeedDetail()
		{
			Description = $"test description {Guid.NewGuid}",
			Title = $"test title {Guid.NewGuid}",
			ImageUrl = $"test url {Guid.NewGuid}"
		};

		var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.NotEqual(0, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(testTimeOffset, createdFeedDetail.CreatedUtc);
		Assert.Equal(testTimeOffset, createdFeedDetail.UpdatedUtc);

		// Store previous values controlled by the database
		var previousFeedId = createdFeedDetail.Id;
		var previousCreatedUtc = createdFeedDetail.CreatedUtc;
		var previousUpdatedUtc = createdFeedDetail.UpdatedUtc;

		createdFeedDetail.Description = $"updated description {Guid.NewGuid()}";
		createdFeedDetail.Title = $"updated title {Guid.NewGuid()}";
		createdFeedDetail.ImageUrl = $"updated url {Guid.NewGuid()}";

		timeProvider.Advance(new TimeSpan(1));

		var updateFeedDetail = await feedStorage.SaveFeedDetails(createdFeedDetail);

		Assert.NotNull(updateFeedDetail);

		// Assert that the Id and createdUtc have not changed, but the UpdatedUtc matches our shifted time
		Assert.Equal(previousFeedId, updateFeedDetail.Id);
		// The updated feed should have a created that matches the original test time offset, as well as the created feed
		Assert.Equal(testTimeOffset, updateFeedDetail.CreatedUtc);
		Assert.Equal(previousCreatedUtc, updateFeedDetail.CreatedUtc);
		// UpdatedUtc should not be identical to what it was, but should be identical to the advanced timeProvider value
		Assert.NotEqual(previousUpdatedUtc, updateFeedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), updateFeedDetail.UpdatedUtc);
		// Mostly redundant but as an assurance, make sure updated and created times are not equal
		Assert.NotEqual(updateFeedDetail.UpdatedUtc, updateFeedDetail.CreatedUtc);

		// Changed values should be changed
		Assert.Equal(createdFeedDetail.Title, updateFeedDetail.Title);
		Assert.Equal(createdFeedDetail.Description, updateFeedDetail.Description);
		Assert.Equal(createdFeedDetail.ImageUrl, updateFeedDetail.ImageUrl);
	}
}