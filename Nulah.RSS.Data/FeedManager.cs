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

	private async Task<Feed?> GetFeedById(int feedDetailId)
	{
		if (feedDetailId == 0)
		{
			return null;
		}

		return await _context.Feeds.FirstOrDefaultAsync(x => x.Id == feedDetailId);
	}
}