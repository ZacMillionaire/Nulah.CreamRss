using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.ArsTechnica;

public class ArsTechnicaUpdateFeedTests : WebApiFixture
{
	private readonly byte[] FeedImage;

	public ArsTechnicaUpdateFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		// These tests control the Api in their test methods to provide shared, but still isolated shared contexts
		// Register the response for a uri
		FeedImage = TestHelpers.LoadImageResource("cropped-ars-logo-512_480-32x32.png");
	}


	[Fact]
	public async void AttemptToUpdateFeedDetails_ShouldReturn_FeedDetailWithUpdatedValues()
	{
		var timeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// This test will be against the same in-memory database context, with a guid to keep it distinct between runs
		var databaseName = $"Ars-Technica-UpdateFeed-Api-Tests-{Guid.NewGuid()}";

		var apifactory = new ApiWebApplicationFactory()
		{
			TimeProvider = timeProvider,
			DatabaseName = databaseName
		};

		apifactory.TestHttpMessageHandler.SetResponseForUri(
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			FeedImage
		);

		var updateApi = new RssApi(apifactory);

		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";
		var rssDetail = await updateApi.PreviewRss(body);
		Assert.NotNull(rssDetail);

		// Advance time to avoid testing against the initial value
		timeProvider.Advance(new TimeSpan(100));

		// Create an initial feed
		var savedDetail = await updateApi.CreateRssFeedByDetail(rssDetail);

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal(FeedImage, savedDetail.ImageBlob);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.UpdatedUtc);

		timeProvider.Advance(new TimeSpan(100));

		savedDetail.Description = "Updated description";

		var updatedDetail = await updateApi.UpdateRssFeedByDetail(savedDetail);

		Assert.NotNull(updatedDetail);

		// Make sure that the Id remained the same, and that the description has changed
		Assert.Equal(savedDetail.Id, updatedDetail.Id);
		Assert.Equal(savedDetail.Description, updatedDetail.Description);
		Assert.Equal(savedDetail.CreatedUtc, updatedDetail.CreatedUtc);
		Assert.Equal(FeedImage, updatedDetail.ImageBlob);
		// make sure the updated time has changed
		Assert.NotEqual(savedDetail.UpdatedUtc, updatedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), updatedDetail.UpdatedUtc);
	}

	[Fact]
	public async void AttemptToCreateFeedDetailsFromLocation_AfterUpdate_ShouldReturn_FeedDetailWithParsedDetails()
	{
		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";

		// This test requires a shared context between calls, so we make an api to share
		// We also makesure the date time for this provider differs from the one set for the rest of tests to ensure we're asserting
		// correct values
		var timeProvider = new FakeTimeProvider(new(2022, 2, 2, 1, 2, 3, TimeSpan.Zero));
		var apiWebApplicationFactory = new ApiWebApplicationFactory()
		{
			TimeProvider = timeProvider,
			// Ensure that the context name is distinct from other tests in this class
			DatabaseName = $"Ars-Technica-Api-Tests-{Guid.NewGuid()}"
		};
		apiWebApplicationFactory.TestHttpMessageHandler.SetResponseForUri(
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			FeedImage
		);
		var createUpdateApi = new RssApi(apiWebApplicationFactory);

		// Advance time to avoid testing against the initial value
		timeProvider.Advance(new TimeSpan(100));

		var savedDetail = await createUpdateApi.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = body
		});

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal(FeedImage, savedDetail.ImageBlob);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.UpdatedUtc);

		// Update the feed with specific changes
		timeProvider.Advance(new TimeSpan(100));

		savedDetail.Description = "Updated description";
		savedDetail.ImageBlob = [];

		var updatedDetail = await createUpdateApi.UpdateRssFeedByDetail(savedDetail);

		Assert.NotNull(updatedDetail);

		// Make sure that the Id remained the same, and that the description has changed
		Assert.Equal(savedDetail.Id, updatedDetail.Id);
		Assert.Equal(savedDetail.Description, updatedDetail.Description);
		Assert.Equal(savedDetail.CreatedUtc, updatedDetail.CreatedUtc);
		Assert.Equal([], updatedDetail.ImageBlob);
		// make sure the updated time has changed
		Assert.NotEqual(savedDetail.UpdatedUtc, updatedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), updatedDetail.UpdatedUtc);

		// Advance time to reflect an updatedUtc
		timeProvider.Advance(new TimeSpan(1, 0, 0));

		// Attempting to create an RSS feed by request another time by matching location will update it, which in this
		// case should revert the details back to the parsed feed
		var updateDetailAttempt = await createUpdateApi.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = body
		});

		Assert.NotNull(updateDetailAttempt);
		Assert.Equal(1, updateDetailAttempt.Id);

		Assert.Equal("Ars Technica - All content", updateDetailAttempt.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", updateDetailAttempt.ImageUrl);
		Assert.Equal(FeedImage, updateDetailAttempt.ImageBlob);
		Assert.Equal("All Ars Technica stories", updateDetailAttempt.Description);
		Assert.Equal(body, updateDetailAttempt.Location);

		// Created should match the initial create
		Assert.Equal(savedDetail.CreatedUtc, updateDetailAttempt.CreatedUtc);
		// Updated should reflect the advanced time
		Assert.Equal(timeProvider.GetUtcNow(), updateDetailAttempt.UpdatedUtc);
	}


	[Fact]
	public async void CreatingFeedByRequest_Twice_ShouldReturn_FeedDetailWithParsedDetails_WithNoUpdateChanged()
	{
		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";

		// This test requires a shared context between calls, so we make an api to share
		// We also makesure the date time for this provider differs from the one set for the rest of tests to ensure we're asserting
		// correct values
		var timeProvider = new FakeTimeProvider(new(2022, 2, 2, 1, 2, 3, TimeSpan.Zero));
		var apiWebApplicationFactory = new ApiWebApplicationFactory()
		{
			TimeProvider = timeProvider,
			// Ensure that the context name is distinct from other tests in this class
			DatabaseName = $"Ars-Technica-Api-Tests-Double-Create-{Guid.NewGuid()}"
		};
		apiWebApplicationFactory.TestHttpMessageHandler.SetResponseForUri(
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			FeedImage
		);
		var createUpdateApi = new RssApi(apiWebApplicationFactory);

		// Advance time to avoid testing against the initial value
		timeProvider.Advance(new TimeSpan(100));

		var savedDetail = await createUpdateApi.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = body
		});

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(FeedImage, savedDetail.ImageBlob);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.UpdatedUtc);

		// Advance time to reflect an updatedUtc
		timeProvider.Advance(new TimeSpan(1, 0, 0));

		// Calling this endpoint twice without any other changes to the feed otherwise should change nothing, including
		// the updated time
		var updateDetailAttempt = await createUpdateApi.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = body
		});

		Assert.NotNull(updateDetailAttempt);
		// Nothing about this should have changed and effectively we've just returned the existing feed detail
		// by location
		Assert.Equal(savedDetail.Id, updateDetailAttempt.Id);
		Assert.Equal(savedDetail.Title, updateDetailAttempt.Title);
		Assert.Equal(savedDetail.ImageUrl, updateDetailAttempt.ImageUrl);
		Assert.Equal(savedDetail.ImageBlob, updateDetailAttempt.ImageBlob);
		Assert.Equal(savedDetail.Description, updateDetailAttempt.Description);
		Assert.Equal(savedDetail.Location, updateDetailAttempt.Location);
		Assert.Equal(savedDetail.CreatedUtc, updateDetailAttempt.CreatedUtc);
		Assert.Equal(savedDetail.UpdatedUtc, updateDetailAttempt.UpdatedUtc);
	}
}