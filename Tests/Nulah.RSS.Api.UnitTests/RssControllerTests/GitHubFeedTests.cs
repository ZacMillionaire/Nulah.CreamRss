using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests;

public class GitHubFeedTests : WebApiFixture
{
	public GitHubFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToPreview_Feed_ShouldReturn_FeedDetailPreview()
	{
		var body = "./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom";
		var rssDetail = await Api.PreviewRss(body);

		Assert.NotNull(rssDetail);

		Assert.Equal(0, rssDetail.Id);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.CreatedUtc);
		Assert.Equal(DateTimeOffset.MinValue, rssDetail.UpdatedUtc);

		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.Null(rssDetail.Description);

		Assert.Equal("GitHub Public Timeline Feed", rssDetail.Title);
	}
}