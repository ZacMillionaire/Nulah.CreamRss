using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Core.UnitTests.StaticFeedTests;

public class LoadFeedDetailsTests
{
	private readonly HttpClient _client;
	private readonly MockHttpMessageHandler _mockHandler = new();

	public LoadFeedDetailsTests()
	{
		// I can't simply substitute this mock with nsubstitute because if I do (using Substitute.For<MockHttpMessageHandler>())
		// then the internals of HttpClient won't actually call any methods.
		// Fucked if I know why but I wasted hours trying to figure that out after having no sleep so I'll probably
		// revisit this later because my alternative feels clunky.
		// Fuck HttpClient.
		_client = new HttpClient(_mockHandler);
	}

	[Fact]
	public void Load_BlankRssLocation_Should_ThrowException()
	{
		var rssReader = new FeedReader(_client);
		Assert.Throws<ArgumentNullException>(() => rssReader.ParseFeedDetails(""));
	}

	[Fact]
	public void Load_ArsTechnicaFeedDetails_Should_HaveTitleImageUriDescription()
	{
		var imageContent = TestHelpers.LoadImageResource("cropped-ars-logo-512_480-32x32.png");
		_mockHandler.SetResponseForUri(
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			imageContent
		);

		var rssReader = new FeedReader(_client);
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.ImageBlob);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("Ars Technica - All content", rssDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", rssDetail.ImageUrl);
		Assert.Equal(imageContent, rssDetail.ImageBlob);
		Assert.Equal("All Ars Technica stories", rssDetail.Description);
	}

	[Fact]
	public void Load_GitHubFeedDetails_Should_HaveTitle()
	{
		var rssReader = new FeedReader(_client);
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.Null(rssDetail.ImageBlob);
		Assert.Null(rssDetail.Description);

		Assert.Equal("GitHub Public Timeline Feed", rssDetail.Title);
	}

	[Fact]
	public void Load_DevToFeedDetails_Should_HaveTitleAndDescription()
	{
		var rssReader = new FeedReader(_client);
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/DevTo.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.Null(rssDetail.ImageUrl);
		Assert.Null(rssDetail.ImageBlob);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("DEV Community", rssDetail.Title);
		Assert.Equal("The most recent home feed on DEV Community.", rssDetail.Description);
	}

	[Fact]
	public void Load_WebToonsTowerOfGodFeedDetails_Should_HaveTitleImageUriDescription()
	{
		var imageContent = TestHelpers.LoadImageResource("7TowerOfGod_thumbnail_desktop.jpg");
		_mockHandler.SetResponseForUri(
			"https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg",
			imageContent
		);

		var rssReader = new FeedReader(_client);
		var rssDetail = rssReader.ParseFeedDetails("./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss");
		Assert.NotNull(rssDetail);

		Assert.NotNull(rssDetail.Title);
		Assert.NotNull(rssDetail.ImageUrl);
		Assert.NotNull(rssDetail.ImageBlob);
		Assert.NotNull(rssDetail.Description);

		Assert.Equal("Tower of God", rssDetail.Title);
		Assert.Equal("https://swebtoon-phinf.pstatic.net/20230324_9/167961213201856COO_JPEG/7TowerOfGod_thumbnail_desktop.jpg", rssDetail.ImageUrl);
		Assert.Equal(imageContent, rssDetail.ImageBlob);
		// The description has a space in the feed, but should be normalised to not
		Assert.Equal(
			"What do you desire? Money and wealth? Honor and pride? Authority and power? Revenge? Or something that transcends them all? Whatever you desire—it's here.",
			rssDetail.Description);
	}
}