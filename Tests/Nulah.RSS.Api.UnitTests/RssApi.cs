using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Nulah.RSS.Api.Controllers;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Api.UnitTests;

/// <summary>
/// Used as a wrapper for any calls to the Rss Api.
/// </summary>
public class RssApi
{
	public const string BaseAddress = "http://rss-test-api";
	private readonly ApiWebApplicationFactory _rssApiFactory;

	private readonly JsonSerializerOptions _jsonSerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	/// <summary>
	/// Options to define the base address for the API to use for tests. Ideally in an effort to avoid duplicate
	/// bindings to the same port, but who knows how true that will hold
	/// </summary>
	private readonly WebApplicationFactoryClientOptions _webApplicationFactoryClientOptions = new()
	{
		BaseAddress = new Uri($"{BaseAddress}:0")
	};

	/// <summary>
	/// 
	/// </summary>
	/// <param name="apiClientFactory"></param>
	public RssApi(ApiWebApplicationFactory apiClientFactory)
	{
		_rssApiFactory = apiClientFactory;
	}

	/// <summary>
	/// Calls <see cref="RssController.Preview"/> and returns the result. This endpoint does not save any data.
	/// <para>
	/// This method will automatically wrap the given rssLocation into the appropriate request object
	/// </para>
	/// </summary>
	/// <param name="rssLocation"></param>
	/// <returns></returns>
	public async Task<FeedDetail?> PreviewRss(string rssLocation)
	{
		return await PreviewRss(new FeedRequest()
		{
			FeedLocation = rssLocation
		});
	}

	/// <summary>
	/// Calls <see cref="RssController.Preview"/> and returns the result. This endpoint does not save any data.
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	public async Task<FeedDetail?> PreviewRss(FeedRequest request)
	{
		// I don't believe we should need this in a using as from my understanding it should still be following the general
		// HttpClientFactory practices of cleaning up after itself.
		// Also this is a test context so the lifetime of these clients should be effectively ephemeral but happy to discover
		// thats not the case later.
		// Definition of happy is open to debate.
		// This may occassionally cause conflicts due to other things binding to port 5000 so I might have to add a random
		// port binding to this
		var client = _rssApiFactory.CreateClient(_webApplicationFactoryClientOptions);

		var rssDetail = await client.PostAsync(
			"/rss/preview",
			CreateFeedRequestContent(request)
		);

		// If we have a success code, deserialise the result and return it
		if (rssDetail.IsSuccessStatusCode)
		{
			return await DeserialiseResponse<FeedDetail?>(rssDetail);
		}

		// Otherwise throw a HttpRequestException with the detail and status code repeated
		throw new HttpRequestException(await rssDetail.Content.ReadAsStringAsync(), null, rssDetail.StatusCode);
	}

	/// <summary>
	/// Saves a given feed detail and returns the same feed with database properties.
	/// Calls <see cref="RssController.CreateFeedByDetail"/>
	/// </summary>
	/// <param name="rssDetail"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	public async Task<FeedDetail?> CreateRssFeedByDetail(FeedDetail rssDetail)
	{
		var client = _rssApiFactory.CreateClient(_webApplicationFactoryClientOptions);
		var createdFeedDetail = await client.PostAsync(
			"/rss/CreateFeedByDetail",
			CreateFeedRequestContent(rssDetail)
		);

		// If we have a success code, deserialise the result and return it
		if (createdFeedDetail.IsSuccessStatusCode)
		{
			return await DeserialiseResponse<FeedDetail?>(createdFeedDetail);
		}

		// Otherwise throw a HttpRequestException with the detail and status code repeated
		throw new HttpRequestException(await createdFeedDetail.Content.ReadAsStringAsync(), null, createdFeedDetail.StatusCode);
	}

	/// <summary>
	/// Saves a given feed detail and returns the same feed with database properties.
	/// Calls <see cref="RssController.CreateFeed"/>
	/// </summary>
	/// <param name="createFeedRequest"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	public async Task<FeedDetail?> CreateRssFeedByRequest(FeedRequest createFeedRequest)
	{
		var client = _rssApiFactory.CreateClient(_webApplicationFactoryClientOptions);
		var createdFeedDetail = await client.PostAsync(
			"/rss/CreateFeed",
			CreateFeedRequestContent(createFeedRequest)
		);

		// If we have a success code, deserialise the result and return it
		if (createdFeedDetail.IsSuccessStatusCode)
		{
			return await DeserialiseResponse<FeedDetail?>(createdFeedDetail);
		}

		// Otherwise throw a HttpRequestException with the detail and status code repeated
		throw new HttpRequestException(await createdFeedDetail.Content.ReadAsStringAsync(), null, createdFeedDetail.StatusCode);
	}

	/// <summary>
	/// Calls <see cref="RssController.UpdateFeedByDetail"/> and returns the result. This endpoint does save any data.
	/// </summary>
	/// <param name="savedDetail"></param>
	/// <returns></returns>
	/// <exception cref="HttpRequestException"></exception>
	public async Task<FeedDetail?> UpdateRssFeedByDetail(FeedDetail savedDetail)
	{
		var client = _rssApiFactory.CreateClient(_webApplicationFactoryClientOptions);
		var updatedFeedDetail = await client.PostAsync(
			"/rss/UpdateFeedByDetail",
			CreateFeedRequestContent(savedDetail)
		);

		// If we have a success code, deserialise the result and return it
		if (updatedFeedDetail.IsSuccessStatusCode)
		{
			return await DeserialiseResponse<FeedDetail?>(updatedFeedDetail);
		}

		// Otherwise throw a HttpRequestException with the detail and status code repeated
		throw new HttpRequestException(await updatedFeedDetail.Content.ReadAsStringAsync(), null, updatedFeedDetail.StatusCode);
	}

	public async Task<BatchFeedResult?> CreateBatchFeeds(BatchFeedRequest batchFeedRequest)
	{
		var client = _rssApiFactory.CreateClient(_webApplicationFactoryClientOptions);
		var batchFeedResult = await client.PostAsync(
			"batch/Rss/CreateFeed",
			CreateFeedRequestContent(batchFeedRequest)
		);

		// If we have a success code, deserialise the result and return it
		if (batchFeedResult.IsSuccessStatusCode)
		{
			return await DeserialiseResponse<BatchFeedResult?>(batchFeedResult);
		}

		// Otherwise throw a HttpRequestException with the detail and status code repeated
		throw new HttpRequestException(await batchFeedResult.Content.ReadAsStringAsync(), null, batchFeedResult.StatusCode);
	}

	/// <summary>
	/// Effectively serialises the given <see cref="T"/> into a <see cref="StringContent"/> with a media type of
	/// application/json appropriate for sending as the body of a POST
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	private static StringContent CreateFeedRequestContent<T>(T request)
	{
		var content = JsonSerializer.Serialize(request);
		return new StringContent(content, Encoding.UTF8, "application/json");
	}

	/// <summary>
	/// Deserialises a response to from the given response to the given type. If the response has a <see cref="HttpStatusCode.NoContent"/> status code,
	/// the default value of the type will be returned (which will often be null)
	/// </summary>
	/// <param name="responseMessage"></param>
	/// <typeparam name="T">Type to deserialise the content stream to</typeparam>
	/// <returns></returns>
	private async Task<T?> DeserialiseResponse<T>(HttpResponseMessage responseMessage)
	{
		if (responseMessage.StatusCode == HttpStatusCode.NoContent)
		{
			return default;
		}

		return await JsonSerializer.DeserializeAsync<T?>(await responseMessage.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
	}
}