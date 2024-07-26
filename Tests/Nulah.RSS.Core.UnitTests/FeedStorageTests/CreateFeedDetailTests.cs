using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class CreateFeedDetailTests : IClassFixture<InMemoryTestFixture>
{
	private readonly InMemoryTestFixture _fixture;

	public CreateFeedDetailTests(InMemoryTestFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async void Create_FeedDetail_With_FullDetails_ShouldReturn_FeedDetailWithId()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			Title = "test title",
			ImageUrl = "test url",
			Location = "testlocation"
		};

		var createdFeedDetail = await feedStorage.CreateFeedDetail(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(1, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(newFeedDetail.Location, createdFeedDetail.Location);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);
	}

	[Fact]
	public async void Create_FeedDetail_With_DatabasePropertiesAndFullDetails_ShouldReturn_FeedDetailWithIdAndDatabaseSetProperties()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		// This test should result in the Id, CreatedUtc and UpdatedUtc properties being ignored and set by the
		// appropriate manager class
		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			Title = "test title",
			ImageUrl = "test url",
			Location = "testlocation",
			Id = 200,
			CreatedUtc = testTimeOffset.AddDays(2),
			UpdatedUtc = testTimeOffset.AddDays(3)
		};

		var createdFeedDetail = await feedStorage.CreateFeedDetail(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(newFeedDetail.Location, createdFeedDetail.Location);
		Assert.Equal(1, createdFeedDetail.Id);
		Assert.NotEqual(newFeedDetail.Id, createdFeedDetail.Id);
		Assert.NotEqual(newFeedDetail.CreatedUtc, createdFeedDetail.CreatedUtc);
		Assert.NotEqual(newFeedDetail.UpdatedUtc, createdFeedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);
	}

	[Fact]
	public async void Create_FeedDetail_With_DuplicateLocation_Should_ThrowException()
	{
		var testTimeOffset = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(testTimeOffset);

		var feedStorage = _fixture.CreateFeedStorage(timeProvider);

		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			Title = "test title",
			ImageUrl = "test url",
			Location = "testlocation"
		};

		var createdFeedDetail = await feedStorage.CreateFeedDetail(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(1, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(newFeedDetail.Location, createdFeedDetail.Location);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);

		timeProvider.Advance(new TimeSpan(1));

		var duplicateLocationFeedDetail = new FeedDetail()
		{
			Description = "test description 1",
			Title = "test title 1",
			ImageUrl = "test url 1",
			Location = newFeedDetail.Location
		};

		var ex = await Assert.ThrowsAsync<ArgumentException>(() => feedStorage.CreateFeedDetail(duplicateLocationFeedDetail));
		Assert.Equal($"""Feed location "{duplicateLocationFeedDetail.Location}" already exists""", ex.Message);
	}

	[Fact]
	public async void Create_FeedDetail_Without_Title_ShouldThrowValidationException()
	{
		var feedStorage = _fixture.CreateFeedStorage(null);

		var newFeedDetail = new FeedDetail()
		{
			Description = "test description",
			ImageUrl = "test url",
		};

		var exception = await Assert.ThrowsAsync<ValidationException>(() => feedStorage.CreateFeedDetail(newFeedDetail));
		Assert.Equal("FeedDetail is invalid", exception.GetBaseException().Message);
		Assert.NotNull(exception.Data);
		Assert.Single(exception.Data);
		var validationResult = exception.Data["ValidationErrors"] as List<ValidationResult>;
		Assert.NotNull(validationResult);
		Assert.Equal(2, validationResult.Count);

		// These assertations are definitely going to be flimsy as more are added
		// There's definitely going to be a better way of doing this that isn't returning a more valuable result
		// ...which I'll probably do in the future anyway
		// I'm assuming that the order of properties defined is the order these come out as, but however its reflected
		// out might also be in alphabetical order instead.
		Assert.Equal("Title cannot be empty", validationResult[0].ErrorMessage);
		Assert.Equal("Location cannot be empty", validationResult[1].ErrorMessage);
	}
}