using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;

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
			ImageUrl = "test url"
		};

		var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(1, createdFeedDetail.Id);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
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
			Id = 200,
			CreatedUtc = testTimeOffset.AddDays(2),
			UpdatedUtc = testTimeOffset.AddDays(3)
		};

		var createdFeedDetail = await feedStorage.SaveFeedDetails(newFeedDetail);

		Assert.NotNull(createdFeedDetail);
		Assert.Equal(newFeedDetail.Title, createdFeedDetail.Title);
		Assert.Equal(newFeedDetail.Description, createdFeedDetail.Description);
		Assert.Equal(newFeedDetail.ImageUrl, createdFeedDetail.ImageUrl);
		Assert.Equal(1, createdFeedDetail.Id);
		Assert.NotEqual(newFeedDetail.Id, createdFeedDetail.Id);
		Assert.NotEqual(newFeedDetail.CreatedUtc, createdFeedDetail.CreatedUtc);
		Assert.NotEqual(newFeedDetail.UpdatedUtc, createdFeedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), createdFeedDetail.UpdatedUtc);
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
}