using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.ArsTechnica;

public class ArsTechnicaUpdateFeedTests : WebApiFixture
{
	public ArsTechnicaUpdateFeedTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		// These tests control the Api in their test methods to provide shared, but still isolated shared contexts
	}


	[Fact]
	public async void AttemptToUpdateFeedDetails_ShouldReturn_FeedDetailWithUpdatedValues()
	{
		var timeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		// This test will be against the same in-memory database context, with a guid to keep it distinct between runs
		var databaseName = $"Ars-Technica-UpdateFeed-Api-Tests-{Guid.NewGuid()}";
		var updateApi = new RssApi(new ApiWebApplicationFactory()
		{
			TimeProvider = timeProvider,
			DatabaseName = databaseName
		});

		var body = "./TestFiles/SampleRssFeeds/ArsTechnicaAllContent.rss";
		var rssDetail = await updateApi.PreviewRss(body);
		Assert.NotNull(rssDetail);

		// Advance time to avoid testing against the initial value
		timeProvider.Advance(new TimeSpan(100));

		// Create an initial feed
		var savedDetail = await updateApi.CreateRssFeedByDetail(rssDetail);

		Assert.NotNull(savedDetail);
		Assert.Equal(1, savedDetail.Id);

		Assert.Equal("Ars Technica - All content", savedDetail.Title);
		Assert.Equal("https://cdn.arstechnica.net/wp-content/uploads/2016/10/cropped-ars-logo-512_480-32x32.png", savedDetail.ImageUrl);
		Assert.Equal("All Ars Technica stories", savedDetail.Description);
		Assert.Equal(body, savedDetail.Location);

		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.CreatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), savedDetail.UpdatedUtc);

		timeProvider.Advance(new TimeSpan(100));

		savedDetail.Description = "Updated description";

		var updatedDetail = await updateApi.UpdateRssFeedByDetail(savedDetail);

		Assert.NotNull(updatedDetail);

		// Make sure that the Id remained the same, and that the description has changed
		Assert.Equal(savedDetail.Id, updatedDetail.Id);
		Assert.Equal(savedDetail.Description, updatedDetail.Description);
		Assert.Equal(savedDetail.CreatedUtc, updatedDetail.CreatedUtc);
		// make sure the updated time has changed
		Assert.NotEqual(savedDetail.UpdatedUtc, updatedDetail.UpdatedUtc);
		Assert.Equal(timeProvider.GetUtcNow(), updatedDetail.UpdatedUtc);
	}
}