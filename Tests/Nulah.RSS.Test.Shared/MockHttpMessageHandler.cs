using System.Net;

namespace Nulah.RSS.Test.Shared;

/// <summary>
/// Used for mocking httpclient responses - should be identical behaviour to <see cref="MockAspHttpDelegationHandler"/>
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
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

	public HttpResponseMessage MockSend(HttpRequestMessage request, CancellationToken cancellationToken)
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