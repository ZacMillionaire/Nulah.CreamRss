using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class CreateFeedDetailTests : IClassFixture<InMemoryTestFixture>
{
	private readonly InMemoryDatabase _inMemoryDatabase;

	/// <summary>
	/// Creates a manager with a database context that has a time provider bound to the given time provider
	/// </summary>
	/// <returns></returns>
	private IFeedManager CreateManager(TimeProvider timeProvider) => new FeedManager(_inMemoryDatabase.CreateContext(timeProvider));

	public CreateFeedDetailTests(InMemoryTestFixture inMemoryTestFixture)
	{
		_inMemoryDatabase = inMemoryTestFixture.Database;
	}

	[Fact]
	public async void Create_FeedDetail_With_FullDetails_ShouldReturn_FeedDetailWithId()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = new FeedStorage(CreateManager(timeProvider));

		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			Title = "test title",
			ImageUrl = "test url"
		};

		var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.NotEqual(0, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(testTimeOffset, createdFeedDetail.CreatedUtc);
		Assert.Equal(testTimeOffset, createdFeedDetail.UpdatedUtc);
	}

	[Fact]
	public async void Create_FeedDetail_With_DatabasePropertiesAndFullDetails_ShouldReturn_FeedDetailWithIdAndDatabaseSetProperties()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = new FeedStorage(CreateManager(timeProvider));

		// This test should result in the Id, CreatedUtc and UpdatedUtc properties being ignored and set by the
		// appropriate manager class
		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			Title = "test title",
			ImageUrl = "test url",
			Id = 200,
			CreatedUtc = testTimeOffset.AddDays(2),
			UpdatedUtc = testTimeOffset.AddDays(3)
		};

		var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.NotEqual(newFeedDetail.Id, createdFeedDetail.Id);
		Assert.NotEqual(newFeedDetail.CreatedUtc, createdFeedDetail.CreatedUtc);
		Assert.NotEqual(newFeedDetail.UpdatedUtc, createdFeedDetail.UpdatedUtc);
	}

	[Fact]
	public async void Create_FeedDetail_Without_Title_ShouldThrowValidationException()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = new FeedStorage(CreateManager(timeProvider));

		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			ImageUrl = "test url",
		};

		var exception = await Assert.ThrowsAsync<ValidationException>(() => feedStorage.SaveFeedDetails(newFeedDetail));
		Assert.Equal("FeedDetail is invalid", exception.GetBaseException().Message);
		Assert.NotNull(exception.Data);
		Assert.Equal(1, exception.Data.Count);
		var validationResult = exception.Data["ValidationErrors"] as List<ValidationResult>;
		Assert.NotNull(validationResult);
		Assert.Single(validationResult);
		Assert.Equal([nameof(FeedDetail.Title)], validationResult.First().MemberNames);
		Assert.Equal("Title cannot be empty", validationResult.First().ErrorMessage);
	}

	[Fact]
	public async void Update_FeedDetail_With_FullDetails_ShouldReturn_FeedDetailWithSameId()
	{
		var testTimeOffset = new DateTimeOffset(1992, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = new FeedStorage(CreateManager(timeProvider));

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