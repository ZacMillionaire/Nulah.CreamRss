using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests;

public class DevToFeedTests : WebApiFixture
{
	public DevToFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToPreview_FeedDetails_ShouldReturn_FeedDetailPreview()
	{
		var body = "./TestFiles/SampleRssFeeds/DevTo.rss";
		var rssDetail = await Api.PreviewRss(body);

		Assert.NotNull(rssDetail);
		
		Assert.Equal(0, rssDetail.Id);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.CreatedUtc);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.UpdatedUtc);
		
		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.Description);
		Assert.NotNull(rssDetail.Location);

		Assert.Equal("DEV Community", rssDetail.Title);
		Assert.Equal("The most recent home feed on DEV Community.", rssDetail.Description);
		Assert.Equal(body,rssDetail.Location);
	}
}