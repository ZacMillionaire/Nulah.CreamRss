using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Exceptions;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

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
			Description = $"test description {Guid.NewGuid()}",
			Title = $"test title {Guid.NewGuid()}",
			ImageUrl = $"test url {Guid.NewGuid()}",
			Location = $"test location {Guid.NewGuid()}"
		};

		var createdFeedDetail = await feedStorage.CreateFeedDetail(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.NotEqual(0, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(newFeedDetail.Location, createdFeedDetail.Location);
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

		var updateFeedDetail = await feedStorage.UpdateFeedDetail(createdFeedDetail);

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
		// Location should not have changed
		Assert.Equal(newFeedDetail.Location, updateFeedDetail.Location);
	}

	[Fact]
	public async void Update_FeedDetail_With_FullDetails_And_UpdatedLocation_ShouldReturn_FeedDetailWithSameId()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		// Create a new FeedDetail
		var newFeedDetail = new FeedDetail()
		{
			Description = $"test description {Guid.NewGuid()}",
			Title = $"test title {Guid.NewGuid()}",
			ImageUrl = $"test url {Guid.NewGuid()}",
			Location = $"test location {Guid.NewGuid()}"
		};

		var createdFeedDetail = await feedStorage.CreateFeedDetail(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.NotEqual(0, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(newFeedDetail.Location, createdFeedDetail.Location);
		Assert.Equal(testTimeOffset, createdFeedDetail.CreatedUtc);
		Assert.Equal(testTimeOffset, createdFeedDetail.UpdatedUtc);

		// Store previous values controlled by the database
		var previousFeedId = createdFeedDetail.Id;
		var previousCreatedUtc = createdFeedDetail.CreatedUtc;
		var previousUpdatedUtc = createdFeedDetail.UpdatedUtc;

		createdFeedDetail.Description = $"updated description {Guid.NewGuid()}";
		createdFeedDetail.Title = $"updated title {Guid.NewGuid()}";
		createdFeedDetail.ImageUrl = $"updated url {Guid.NewGuid()}";
		createdFeedDetail.Location = $"updated location {Guid.NewGuid()}";

		timeProvider.Advance(new TimeSpan(1));

		var updateFeedDetail = await feedStorage.UpdateFeedDetail(createdFeedDetail);

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
		// Location should not have changed
		Assert.Equal(createdFeedDetail.Location, updateFeedDetail.Location);
	}

	[Fact]
	public async void UpdateFeedDetail_WithId_ThatDoesNotExist_Should_ThrowFeedNotFoundException()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);
		var randomId = new Random().Next(1, 1000);
		var ex = await Assert.ThrowsAsync<FeedNotFoundException>(() => feedStorage.UpdateFeedDetail(new FeedDetail()
			{
				Location = "doesn't matter",
				Title = "doesn't matter",
				Id = randomId
			})
		);

		Assert.Equal($"No feed found with the specified Id: {randomId}", ex.Message);
	}

	[Fact]
	public async void UpdateFeedDetail_WithExistingLocation_ThatExistsInAnotherFeed_Should_ThrowFeedLocationInUseException()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		// Create 2 new FeedDetails
		var createdFeedDetail = await feedStorage.CreateFeedDetail(new FeedDetail()
		{
			Description = $"test description {Guid.NewGuid()}",
			Title = $"test title {Guid.NewGuid()}",
			ImageUrl = $"test url {Guid.NewGuid()}",
			Location = $"test location {Guid.NewGuid()}"
		});
		Assert.NotNull(createdFeedDetail);

		var createdFeedDetail2 = await feedStorage.CreateFeedDetail(new FeedDetail()
		{
			Description = $"test description {Guid.NewGuid()}",
			Title = $"test title {Guid.NewGuid()}",
			ImageUrl = $"test url {Guid.NewGuid()}",
			Location = $"test location {Guid.NewGuid()}"
		});
		Assert.NotNull(createdFeedDetail2);

		// Update the 2nd detail with the location of the first
		createdFeedDetail2.Location = createdFeedDetail.Location;
		var ex = await Assert.ThrowsAsync<FeedLocationInUseException>(() => feedStorage.UpdateFeedDetail(createdFeedDetail2)
		);

		Assert.Equal($"""Feed location "{createdFeedDetail2.Location}" already exists by another feed: {createdFeedDetail.Id}""", ex.Message);
	}
	
	
	[Fact]
	public async void UpdateFeedDetail_WithMissingRequiredProperties_Should_ThrowValidationException()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);
		
		// create a feed to update
		var createdFeedDetail = await feedStorage.CreateFeedDetail(new FeedDetail()
		{
			Description = $"test description {Guid.NewGuid()}",
			Title = $"test title {Guid.NewGuid()}",
			ImageUrl = $"test url {Guid.NewGuid()}",
			Location = $"test location {Guid.NewGuid()}"
		});
		Assert.NotNull(createdFeedDetail);
		
		var exception = await Assert.ThrowsAsync<ValidationException>(() => feedStorage.UpdateFeedDetail(new FeedDetail()
			{
				Location = null,
				Title = null,
				Id = 0
			})
		);
		
		Assert.Equal("FeedDetail is invalid", exception.GetBaseException().Message);
		Assert.NotNull(exception.Data);
		Assert.Single(exception.Data);
		var validationResult = exception.Data["ValidationErrors"] as List<ValidationResult>;
		Assert.NotNull(validationResult);
		Assert.Equal(3, validationResult.Count);

		// These assertations are definitely going to be flimsy as more are added
		// There's definitely going to be a better way of doing this that isn't returning a more valuable result
		// ...which I'll probably do in the future anyway
		// I'm assuming that the order of properties defined is the order these come out as, but however its reflected
		// out might also be in alphabetical order instead.
		Assert.Equal("Title cannot be empty", validationResult[0].ErrorMessage);
		Assert.Equal("Location cannot be empty", validationResult[1].ErrorMessage);
		// This one is the last element as we add it in the implementation _after_ validation has already created the initial list
		// This means if we add other validations after this, this will need to be updated
		Assert.Equal("Id must be greater than 0", validationResult.Last().ErrorMessage);
	}
}