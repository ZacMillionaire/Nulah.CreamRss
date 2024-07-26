using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests;

public class ArsTechnicaFeedTests : WebApiFixture
{
	public ArsTechnicaFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToPreview_Feed_ShouldReturn_FeedDetailPreview()
	{
		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";
		var rssDetail = await Api.PreviewRss(body);

		Assert.NotNull(rssDetail);

		Assert.Equal(0, rssDetail.Id);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.CreatedUtc);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.UpdatedUtc);
		
		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("Ars Technica - All content", rssDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", rssDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", rssDetail.Description);
	}
}