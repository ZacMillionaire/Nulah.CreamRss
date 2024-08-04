using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests;

public class WebToonsFeedTests : WebApiFixture
{
	private readonly byte[] FeedImage;

	public WebToonsFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

		// Register the response for a uri
		FeedImage = TestHelpers.LoadImageResource("7TowerOfGod_thumbnail_desktop.jpg");
		WebApiFactory.TestHttpMessageHandler.SetResponseForUri(
			"https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg",
			FeedImage
		);

		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToPreview_FeedDetails_ShouldReturn_FeedDetailPreview()
	{
		var body = "./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss";
		var rssDetail = await Api.PreviewRss(body);

		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.ImageBlob);
		Assert.NotNull(rssDetail.Description);
		Assert.NotNull(rssDetail.Location);

		Assert.Equal(0, rssDetail.Id);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.CreatedUtc);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.UpdatedUtc);

		Assert.Equal("Tower of God", rssDetail.Title);
		Assert.Equal("https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg", rssDetail.ImageUrl);
		Assert.Equal(FeedImage, rssDetail.ImageBlob);
		// The description has a space in the feed, but should be normalised to not
		Assert.Equal(
			"What do you desire? Money and wealth? Honor and pride? Authority and power? Revenge? Or something that transcends them all? Whatever you desire—it's here.",
			rssDetail.Description);
		Assert.Equal(body, rssDetail.Location);
	}
}