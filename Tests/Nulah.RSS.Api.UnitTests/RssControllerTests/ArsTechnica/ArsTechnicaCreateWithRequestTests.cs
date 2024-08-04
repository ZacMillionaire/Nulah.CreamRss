using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Domain.Models;
using Nulah.RSS.Test.Shared;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.ArsTechnica;

public class ArsTechnicaCreateWithRequestTests : WebApiFixture
{
	private readonly byte[] FeedImage;
	public ArsTechnicaCreateWithRequestTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// Register the response for a uri
		FeedImage = TestHelpers.LoadImageResource("cropped-ars-logo-512_480-32x32.png");
		WebApiFactory.TestHttpMessageHandler.SetResponseForUri(
			"https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png",
			FeedImage
		);
		// All tests in this class will be against a fresh in-memory database context and will be isolated from each other.
		// Uncomment the following line if you want to make your life harder and have all tests share context.
		// WebApiFactory.DatabaseName = "Ars-Technica-Api-Tests";
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void AttemptToCreateFeedDetailsFromLocation_ShouldReturn_FeedDetailWithDatabaseGeneratedValues()
	{
		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";

		// Advance time to avoid testing against the initial value
		WebApiFactory.TimeProvider.Advance(new TimeSpan(100));

		var savedDetail = await Api.CreateRssFeedByRequest(new FeedRequest()
		{
			FeedLocation = body
		});

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal(FeedImage, savedDetail.ImageBlob);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(WebApiFactory.TimeProvider.GetUtcNow(), savedDetail.UpdatedUtc);
	}
}