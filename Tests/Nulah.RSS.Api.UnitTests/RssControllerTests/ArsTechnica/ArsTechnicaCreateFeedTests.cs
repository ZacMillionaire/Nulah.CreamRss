using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.ArsTechnica;

public class ArsTechnicaCreateFeedTests : WebApiFixture
{
	public ArsTechnicaCreateFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// All tests in this class will be against a fresh in-memory database context and will be isolated from each other.
		// Uncomment the following line if you want to make your life harder and have all tests share context.
		// WebApiFactory.DatabaseName = "Ars-Technica-Api-Tests";
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToPreview_FeedDetails_ShouldReturn_FeedDetailPreview()
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
		Assert.NotNull(rssDetail.Location);

		Assert.Equal("Ars Technica - All content", rssDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", rssDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", rssDetail.Description);
		Assert.Equal(body, rssDetail.Location);
	}

	[Fact]
	public async void AttemptToCreateFeedDetails_ShouldReturn_FeedDetailWithDatabaseGeneratedValues()
	{
		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";
		var rssDetail = await Api.PreviewRss(body);
		Assert.NotNull(rssDetail);

		// Advance time to avoid testing against the initial value
		WebApiFactory.TimeProvider.Advance(new TimeSpan(100));

		var savedDetail = await Api.CreateRssFeedByDetail(rssDetail);

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), savedDetail.UpdatedUtc);
	}
}