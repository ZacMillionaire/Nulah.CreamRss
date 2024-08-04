using System.Net;

namespace Nulah.RSS.Core.UnitTests.StaticFeedTests;

public class MockHttpMessageHandler : HttpMessageHandler
{
	private readonly Dictionary<string, byte[]> _responseDictionary = new();

	/// <summary>
	/// Sets the response for a given uri.
	///
	/// This exists because a substituted type using nSubstitute will not be called within HttpClient for...reasons?
	/// </summary>
	/// <param name="uri"></param>
	/// <param name="returnResponse"></param>
	public void SetResponseForUri(string uri, byte[] returnResponse)
	{
		// Don't care if this succeeds, TryAdd is cleaner than checking if a key exists, then setting the value if it doesn't.
		// It's also more perfomant as it has internal optimisations that I don't care to get into the details of.
		// Here's a quick reasoning https://www.jetbrains.com/help/rider/CanSimplifyDictionaryLookupWithTryAdd.html
		// and the relevant CA https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1864
		_responseDictionary.TryAdd(uri, returnResponse);
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return Task.FromResult(MockSend(request, cancellationToken));
	}

	protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		return MockSend(request, cancellationToken);
	}

	public virtual HttpResponseMessage MockSend(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Don't care if this returns a value, if any exceptions are thrown as a result of this it's a failure of either
		// registering the Url we're simulating the response of via SetResponseForUri(string,byte[]), or we don't have the image
		// to load from under TestFiles/SampleRssFeeds/Images
		_responseDictionary.TryGetValue(request.RequestUri!.ToString(), out var imageContent);

		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			// imageContent is assumed and treated to be never null
			Content = new ByteArrayContent(imageContent!)
		};
	}
}

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
		var imageContent = LoadImageResource("cropped-ars-logo-512_480-32x32.png");
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
		Assert.Equal(imageContent,rssDetail.ImageBlob);
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
		var imageContent = LoadImageResource("7TowerOfGod_thumbnail_desktop.jpg");
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
		Assert.Equal(imageContent,rssDetail.ImageBlob);
		// The description has a space in the feed, but should be normalised to not
		Assert.Equal(
			"What do you desire? Money and wealth? Honor and pride? Authority and power? Revenge? Or something that transcends them all? Whatever you desire—it's here.",
			rssDetail.Description);
	}

	/// <summary>
	/// Returns the contents of the image by name, located under TestFiles/SampleRssFeeds/Images.
	/// </summary>
	/// <param name="imageName"></param>
	/// <returns></returns>
	private static byte[] LoadImageResource(string imageName)
	{
		return File.ReadAllBytes($"./TestFiles/SampleRssFeeds/Images/{imageName}");
	}
}