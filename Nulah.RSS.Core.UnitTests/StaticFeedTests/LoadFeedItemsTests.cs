using Nulah.RSS.Core.Models;

namespace Nulah.RSS.Core.UnitTests.StaticFeedTests;

public class LoadFeedItemsTests
{
	[Fact]
	public void Load_BlankRssLocation_Should_ThrowException()
	{
		var rssReader = new RssReader();
		Assert.Throws<ArgumentNullException>(() => rssReader.LoaddRssItems(""));
	}

	[Fact]
	public void Load_ArsTechnicaFeedItems_Should_HaveAllProperties()
	{
		var rssReader = new RssReader();
		var rssItems = rssReader.LoaddRssItems("./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss");
		Assert.NotNull(rssItems);
		Assert.Equal(20, rssItems.Count);
		Assert.All(rssItems, AssertAllItemPropertiesNotNull);
	}

	[Fact]
	public void Load_GitHubFeedItems_Should_HaveAllProperties_Except_Summary()
	{
		var rssReader = new RssReader();
		var rssItems = rssReader.LoaddRssItems("./TestFiles/SampleRssFeeds/GitHubZacMillionaire.atom");
		Assert.NotNull(rssItems);
		Assert.Equal(6, rssItems.Count);

		// Annoyingly github does not have a summary...I may change summary to just be a truncated content, depending
		Assert.All(rssItems, x =>
		{
			Assert.NotNull(x.Title);
			Assert.NotNull(x.Url);
			Assert.NotNull(x.Content);
			Assert.Null(x.Summary);
			Assert.NotNull(x.Author);
			Assert.NotEqual(DateTimeOffset.MinValue, x.Published);
		});
	}

	[Fact]
	public void Load_DevToFeedItems_Should_HaveAllProperties()
	{
		var rssReader = new RssReader();
		var rssItems = rssReader.LoaddRssItems("./TestFiles/SampleRssFeeds/DevTo.rss");
		Assert.NotNull(rssItems);
		Assert.Equal(12, rssItems.Count);
		Assert.All(rssItems, AssertAllItemPropertiesNotNull);
	}

	[Fact]
	public void Load_WebToonsTowerOfGodFeedItems_Should_HaveAllProperties()
	{
		var rssReader = new RssReader();
		var rssItems = rssReader.LoaddRssItems("./TestFiles/SampleRssFeeds/WebToonsTowerOfGod.rss");
		Assert.NotNull(rssItems);
		Assert.Equal(20, rssItems.Count);
		Assert.All(rssItems, AssertAllItemPropertiesNotNull);
	}


	private void AssertAllItemPropertiesNotNull(RssItem rssItem)
	{
		Assert.NotNull(rssItem.Title);
		Assert.NotNull(rssItem.Url);
		Assert.NotNull(rssItem.Content);
		Assert.NotNull(rssItem.Summary);
		Assert.NotNull(rssItem.Author);
		Assert.NotEqual(DateTimeOffset.MinValue, rssItem.Published);
	}
}