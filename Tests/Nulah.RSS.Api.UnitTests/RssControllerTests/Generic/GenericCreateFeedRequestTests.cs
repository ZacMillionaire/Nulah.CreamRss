using System.Net;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.Generic;

/// <summary>
/// These tests validate that attempts to create rss feeds with null, empty or invalid feed locations fail as expected
/// </summary>
public class GenericCreateFeedRequestTests : WebApiFixture
{
	public GenericCreateFeedRequestTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void Create_With_NullFeedRequest_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(null));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		// The framework should throw a 400 with no body being sent. We don't validate what it returns because we don't really care
		// and we don't control the message it returns yet (or ever probably)
	}

	[Fact]
	public async void Create_With_EmptyFeedRequest_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(new FeedRequest()));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void Create_With_EmptyFeedRequestLocation_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = ""
		}));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void Create_With_WhitespaceRssLocation_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = "                            "
		}));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void InvalidRssLocation_ShouldReturn_Null()
	{
		var invalidRssLocation = "nonexistant/file/path.rss";
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = invalidRssLocation
		}));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Unable to load feed from ""{invalidRssLocation}""", exception.Message);
	}

	[Fact]
	public async void NonExistentRssUrl_ShouldReturn_Null()
	{
		// Create a url that cannot possibly exist
		var invalidRssLocation = new Uri($"http://obviously-not-valid-uri-{Guid.NewGuid()}/fake.rss");
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = invalidRssLocation.ToString()
		}));

		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		// We check the message content here to make sure we get a no host known message, however all we technically
		// care about is a bad request status code
		Assert.Equal($"No such host is known. ({invalidRssLocation.Host}:80)", exception.Message);
	}
}