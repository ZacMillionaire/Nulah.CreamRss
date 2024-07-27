using System.Net;
using Microsoft.Extensions.Time.Testing;

namespace Nulah.RSS.Api.UnitTests.RssControllerTests.Generic;

public class CreateBatchTests : WebApiFixture
{
	public CreateBatchTests(ApiWebApplicationFactory fixture) : base(fixture)
	{
		WebApiFactory.TimeProvider = new FakeTimeProvider(new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
		Api = new RssApi(WebApiFactory);
	}

	[Fact]
	public async void BatchRequest_WithNullRequest_ShouldThrowHttpRequestException()
	{
		var ex = await Assert.ThrowsAsync<HttpRequestException>(() => Api.CreateBatchFeeds(null));
		Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
		// We don't check anything else here as this is a framework level check, we're just validation that null
		// requests don't succeed
	}
}