using System.Net.Http.Json;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.App.Services;

public class FeedService
{
	private readonly HttpClient _feedClient;

	// lazy test data that will be cleared if the refresh feed button is clicked and the API is running
	private readonly List<FeedDetail> _cachedFeeds =
	[
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test"
		},
	];

	public FeedService(HttpClient httpClient)
	{
		_feedClient = httpClient;
	}

	public async Task<List<FeedDetail>> FetchFeeds(bool forceRefresh = false)
	{
		// If we've loaded feeds before and have any in the system and we're not forcing a refresh, return whatever
		// we returned previously
		if (!forceRefresh && _cachedFeeds.Count > 0)
		{
			return _cachedFeeds;
		}

		// Otherwise if we loaded no feeds last time (regardless on if we have feeds previously saved or not), or 
		// we're forcing a refresh, clear whatever we might have had, then load new data in
		_cachedFeeds.Clear();

		// We don't care if we have any results from this, as the endpoint is guaranteed to return a list with 0 or more
		// details, so just add them to the cache and return
		if (await _feedClient.GetFromJsonAsync<List<FeedDetail>>("rss/list") is { } loadedFeeds)
		{
			_cachedFeeds.AddRange(loadedFeeds);
		}

		return _cachedFeeds;
	}
}