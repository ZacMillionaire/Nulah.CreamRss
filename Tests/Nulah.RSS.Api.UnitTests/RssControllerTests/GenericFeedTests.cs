using System.Net;
using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests;

public class GenericFeedTests : WebApiFixture
{
	public GenericFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void NullRequestObject_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss(request: null));
		// For this test we don't care about the content as it could be any type of validation error returned from the framework.
		// We only care that the response is a 400 Bad Request
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
	}

	[Fact]
	public async void NullRssLocation_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss(rssLocation: null));
		// For this test we don't care about the content as it could be any type of validation error returned from the framework.
		// We only care that the response is a 400 Bad Request
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void EmptyRssLocation_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss(""));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void WhitespaceRssLocation_Should_ThrowHttpRequestExceptionException()
	{
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss("                            "));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Feed location is required", exception.Message);
	}

	[Fact]
	public async void InvalidRssLocation_ShouldReturn_Null()
	{
		var invalidRssLocation = "nonexistant/file/path.rss";
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss(invalidRssLocation));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		Assert.Equal($@"Unable to load feed from ""{invalidRssLocation}""", exception.Message);
	}

	[Fact]
	public async void NonExistentRssUrl_ShouldReturn_Null()
	{
		// Create a url that cannot possibly exist
		var invalidRssLocation = new Uri($"http://obviously-not-valid-uri-{Guid.NewGuid()}/fake.rss");
		var exception = await Assert.ThrowsAsync<HttpRequestException>(() => Api.PreviewRss(invalidRssLocation.ToString()));
		Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
		// We check the message content here to make sure we get a no host known message, however all we technically
		// care about is a bad request status code
		Assert.Equal($"No such host is known. ({invalidRssLocation.Host}:80)", exception.Message);
	}
}