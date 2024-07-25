using Microsoft.EntityFrameworkCore;
using Nulah.RSS.Data.Entities;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Data;

public class FeedManager : IFeedManager
{
	private readonly FeedContext _context;

	public FeedManager(FeedContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Creates or updates a <see cref="Feed"/> with the given <see cref="FeedDetail"/>.
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	public async Task<FeedDetail> SaveFeedDetail(FeedDetail feedDetail)
	{
		var feed = await GetFeedById(feedDetail.Id) ?? new Feed();

		feed.Description = feedDetail.Description;
		feed.Title = feedDetail.Title;
		feed.ImageUrl = feedDetail.ImageUrl;

		// If this is a new item add it to the context
		if (feed.Id == 0)
		{
			_context.Feeds.Add(feed);
		}

		await _context.SaveChangesAsync();

		return FeedToFeedDetail(feed);
	}

	/// <summary>
	/// Returns a <see cref="FeedDetail"/> by the specified <paramref name="feedId"/>. Returns null if no feed is found.
	/// </summary>
	/// <param name="feedId"></param>
	/// <returns></returns>
	public async Task<FeedDetail?> GetFeedDetail(int feedId)
	{
		var feed = await GetFeedById(feedId);

		return feed == null
			? null
			: FeedToFeedDetail(feed);
	}

	/// <summary>
	/// Converts a given <see cref="Feed"/> to a <see cref="FeedDetail"/>
	/// </summary>
	/// <param name="feed"></param>
	/// <returns></returns>
	private static FeedDetail FeedToFeedDetail(Feed feed)
	{
		return new FeedDetail()
		{
			Description = feed.Description,
			Id = feed.Id,
			Title = feed.Title,
			ImageUrl = feed.ImageUrl,
			CreatedUtc = feed.CreatedUtc,
			UpdatedUtc = feed.UpdatedUtc,
		};
	}

	/// <summary>
	/// Returns the specified <see cref="Feed"/> by Id, or null if not found or <paramref name="feedDetailId"/> is less than 1
	/// </summary>
	/// <param name="feedDetailId"></param>
	/// <returns></returns>
	private async Task<Feed?> GetFeedById(int feedDetailId)
	{
		if (feedDetailId < 0)
		{
			return null;
		}

		return await _context.Feeds.FirstOrDefaultAsync(x => x.Id == feedDetailId);
	}
}