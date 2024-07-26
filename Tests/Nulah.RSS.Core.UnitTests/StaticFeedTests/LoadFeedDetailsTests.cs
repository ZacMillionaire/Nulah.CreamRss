namespace Nulah.RSS.Core.UnitTests.StaticFeedTests;

public class LoadFeedDetailsTests
{
	[Fact]
	public void Load_BlankRssLocation_Should_ThrowException()
	{
		var rssReader = new FeedReader();
		Assert.Throws<ArgumentNullException>(() => rssReader.ParseFeedDetails(""));
	}

	[Fact]
	public void Load_ArsTechnicaFeedDetails_Should_HaveTitleImageUriDescription()
	{
		var rssReader = new FeedReader();
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("Ars Technica - All content", rssDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", rssDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", rssDetail.Description);
	}

	[Fact]
	public void Load_GitHubFeedDetails_Should_HaveTitle()
	{
		var rssReader = new FeedReader();
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.Null(rssDetail.Description);

		Assert.Equal("GitHub Public Timeline Feed", rssDetail.Title);
	}

	[Fact]
	public void Load_DevToFeedDetails_Should_HaveTitleAndDescription()
	{
		var rssReader = new FeedReader();
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/DevTo.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("DEV Community", rssDetail.Title);
		Assert.Equal("The most recent home feed on DEV Community.", rssDetail.Description);
	}

	[Fact]
	public void Load_WebToonsTowerOfGodFeedDetails_Should_HaveTitleImageUriDescription()
	{
		var rssReader = new FeedReader();
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("Tower of God", rssDetail.Title);
		Assert.Equal("https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg", rssDetail.ImageUrl);
		// The description has a space in the feed, but should be normalised to not
		Assert.Equal(
			"What do you desire? Money and wealth? Honor and pride? Authority and power? Revenge? Or something that transcends them all? Whatever you desire—it's here.",
			rssDetail.Description);
	}
}