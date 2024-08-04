using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.Generic;

public class CreateBatchTests : WebApiFixture
{
	private readonly Dictionary<string, byte[]> FeedImages = new Dictionary<string, byte[]>()
	{
		{
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			TestHelpers.LoadImageResource("cropped-ars-logo-512_480-32x32.png")
		},
		{
			"https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg",
			TestHelpers.LoadImageResource("7TowerOfGod_thumbnail_desktop.jpg")
		}
	};

	public CreateBatchTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

		// Register the response for a uri
		foreach (var feedImage in FeedImages)
		{
			WebApiFactory.TestHttpMessageHandler.SetResponseForUri(
				feedImage.Key,
				feedImage.Value
			);
		}


		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void BatchRequest_WithNullRequest_ShouldThrowHttpRequestException()
	{
		var ex = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateBatchFeeds(null));
		Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
		// We don't check anything else here as this is a framework level check, we're just validation that null
		// requests don't succeed
	}

	[Fact]
	public async void BatchRequest_WithEmptyRequest_Should_ReturnErrorResponse_WithError()
	{
		var ex = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateBatchFeeds(new()));
		Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
		Assert.NotNull(ex.Message);
		var errorContent = JsonSerializer.Deserialize<BatchFeedResult>(ex.Message, ApiWebApplicationFactory.DefaultJsonSerializerOptions);
		Assert.NotNull(errorContent);
		Assert.Empty(errorContent.CreatedFeeds);
		Assert.Single(errorContent.Errors);
		Assert.Equal("No rss locations given to create", errorContent.Errors.First());
	}

	[Fact]
	public async void BatchRequest_WithEmptyLocations_ShouldThrowHttpRequestException()
	{
		var batchResult = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateBatchFeeds(new BatchFeedRequest()
			{
				Locations = ["", null, "      "]
			})
		);
		Assert.Equal(HttpStatusCode.BadRequest, batchResult.StatusCode);
		Assert.NotNull(batchResult.Message);
		var errorContent = JsonSerializer.Deserialize<BatchFeedResult>(batchResult.Message, ApiWebApplicationFactory.DefaultJsonSerializerOptions);
		Assert.NotNull(errorContent);
		Assert.Empty(errorContent.CreatedFeeds);
		Assert.Equal(3, errorContent.Errors.Count);
		Assert.All(errorContent.Errors, error =>
		{
			Assert.Equal("Feed location is required", error);
		});
	}

	[Fact]
	public async void BatchRequest_WithInvalidLocation_ShouldThrowHttpRequestException()
	{
		var invalidRssLocation = new Uri($"http://obviously-not-valid-uri-{Guid.NewGuid()}/fake.rss");
		var batchResult = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateBatchFeeds(new BatchFeedRequest()
			{
				Locations = [invalidRssLocation.ToString()]
			})
		);
		Assert.Equal(HttpStatusCode.BadRequest, batchResult.StatusCode);
		Assert.NotNull(batchResult.Message);
		var errorContent = JsonSerializer.Deserialize<BatchFeedResult>(batchResult.Message, ApiWebApplicationFactory.DefaultJsonSerializerOptions);
		Assert.NotNull(errorContent);
		Assert.Empty(errorContent.CreatedFeeds);
		Assert.Single(errorContent.Errors);
		Assert.All(errorContent.Errors, error =>
		{
			Assert.Equal($"No such host is known. ({invalidRssLocation.Host}:80)", error);
		});
	}

	[Fact]
	public async void BatchRequest_WithValidLocations_Should_ReturnOkResult_WithCreatedFeeds()
	{
		var batchResult = await Api.CreateBatchFeeds(new BatchFeedRequest()
		{
			// Test some but not necessarily all of our static test files (because odds are I'll be too lazy to add more here over time)
			Locations =
			[
				"./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss",
				"./TestFiles/SampleRssFeeds/DevTo.rss",
				"./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss",
				"./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom"
			]
		});

		Assert.NotNull(batchResult);
		Assert.Empty(batchResult.Errors);
		Assert.Equal(4, batchResult.CreatedFeeds.Count);
		Assert.All(batchResult.CreatedFeeds, feedDetail =>
		{
			Assert.NotEqual(0, feedDetail.Id);
			Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), feedDetail.CreatedUtc);
			Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), feedDetail.UpdatedUtc);
		});
	}

	[Fact]
	public async void BatchRequest_WithValidAndInvalidLocations_Should_ReturnOkResult_WithCreatedFeedsAndErrors()
	{
		var invalidRssLocation = "nonexistant/file/path.rss";
		// A request with valid and invalid feed locations will still return a 200 Ok, but will contain errors
		var batchResult = await Api.CreateBatchFeeds(new BatchFeedRequest()
		{
			// Test some but not necessarily all of our static test files (because odds are I'll be too lazy to add more here over time)
			Locations =
			[
				"./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss",
				"./TestFiles/SampleRssFeeds/DevTo.rss",
				"./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss",
				"./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom",
				"    ",
				invalidRssLocation
			]
		});

		Assert.NotNull(batchResult);

		Assert.Equal(2, batchResult.Errors.Count);
		Assert.Contains("Feed location is required", batchResult.Errors);
		Assert.Contains($@"Unable to load feed from ""{invalidRssLocation}""", batchResult.Errors);

		Assert.Equal(4, batchResult.CreatedFeeds.Count);
		Assert.All(batchResult.CreatedFeeds, feedDetail =>
		{
			Assert.NotEqual(0, feedDetail.Id);
			Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), feedDetail.CreatedUtc);
			Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), feedDetail.UpdatedUtc);
		});
	}

	// TODO: test batch update when I discover a way to simulate an rss endpoint I can control/refactor it to be mockable
	// TODO: test batch update when feeds have been updated otherwise - I'd do this now but I'm currently too lazy
}