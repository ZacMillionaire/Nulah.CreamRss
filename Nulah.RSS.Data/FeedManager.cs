using Microsoft.EntityFrameworkCore;
using Nulah.RSS.Core.Models;
using Nulah.RSS.Data.Entities;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Data;

public class FeedManager : IFeedManager
{
	private readonly FeedContext _context;

	public FeedManager(FeedContext context)
	{
		_context = context;
	}

	public async Task<FeedDetail> SaveFeedDetail(FeedDetail feedDetail)
	{
		var feed = await GetFeedById(feedDetail.Id) ?? new Feed();

		feed = new Feed()
		{
			Description = feedDetail.Description,
			Title = feedDetail.Title,
			ImageUrl = feedDetail.ImageUrl
		};

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
			ImageUrl = feed.ImageUrl
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