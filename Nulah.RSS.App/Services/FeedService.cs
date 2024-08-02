using System.Net.Http.Json;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.App.Services;

public class FeedService
{
	public EventHandler? FeedsLoading;
	public EventHandler<List<FeedDetail>>? FeedsLoaded;


	private readonly HttpClient _feedClient;

	// lazy test data that will be cleared if the refresh feed button is clicked and the API is running
	private readonly List<FeedDetail> _cachedFeeds =
	[
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -1
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -2
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -3
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -4
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -5
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -6
		},
		new FeedDetail()
		{
			Title = "test",
			Description = "test",
			Location = "test",
			Id = -7
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
			FeedsLoaded?.Invoke(this, _cachedFeeds);
			return _cachedFeeds;
		}

		try
		{
			// Potential issue where mutliple components might call this at the same time, and invoke events multiple times.
			// Feels like an annoying problem an I could potentially prevent this with semaphors but I'm genuinely not sure
			// if I care too much currently.
			// There may be a bit of multiple flickers as a result of this, but I'll deal with that when it annoys me.
			FeedsLoading?.Invoke(this, EventArgs.Empty);

			// Otherwise if we loaded no feeds last time (regardless on if we have feeds previously saved or not), or 
			// we're forcing a refresh, clear whatever we might have had, then load new data in
			// We don't care if we have any results from this, as the endpoint is guaranteed to return a list with 0 or more
			// details, so just add them to the cache and return
			if (await _feedClient.GetFromJsonAsync<List<FeedDetail>>("rss/list") is { } loadedFeeds)
			{
				// Clear the cache here, just incase we have an exception and want to preserve the previous state.
				// Though if we have an exception, odds are the underlying data backend is cactus so attempting to view feeds
				// may also be busted, but eh, I'll figure out how I want to change this later if it becomes an issue
				_cachedFeeds.Clear();
				_cachedFeeds.AddRange(loadedFeeds);
			}
		}
		catch (Exception ex)
		{
			// Do something about the error here, maybe a shared modal or something but we'll need to evaluate how to handle
			// displaying errors from multiple callers (eg, opening the app makes a call to this)
		}
		finally
		{
			FeedsLoaded?.Invoke(this, _cachedFeeds);
		}

		return _cachedFeeds;
	}
}