using Microsoft.EntityFrameworkCore;
using Nulah.RSS.Data.Entities;
using Nulah.RSS.Domain.Exceptions;
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

	/// <inheritdoc/>
	public async Task<FeedDetail> CreateFeedDetail(FeedDetail feedDetail)
	{
		// If we found a feed by location, abort as feeds are distinct by their source location
		if (await GetFeedByLocation(feedDetail.Location) != null)
		{
			throw new ArgumentException($"""Feed location "{feedDetail.Location}" already exists""");
		}

		var feed = new Feed
		{
			Description = feedDetail.Description,
			Title = feedDetail.Title,
			ImageUrl = feedDetail.ImageUrl,
			Location = feedDetail.Location
		};

		_context.Feeds.Add(feed);

		await _context.SaveChangesAsync();

		return FeedToFeedDetail(feed);
	}

	public async Task<FeedDetail> UpdateFeedDetail(FeedDetail feedDetail)
	{
		var feed = await GetFeedById(feedDetail.Id);

		if (feed == null)
		{
			throw new FeedNotFoundException($"No feed found with the specified Id: {feedDetail.Id}");
		}

		// Check to see if we have any other feeds with the incoming location, and if we do, throw an exception
		// if the Id of the returned existing feed does not match the incoming Id
		if (await GetFeedByLocation(feedDetail.Location) is { } existingByLocation
		    && existingByLocation.Id != feed.Id)
		{
			throw new FeedLocationInUseException($"""Feed location "{feedDetail.Location}" already exists by another feed: {existingByLocation.Id}""");
		}

		feed.Description = feedDetail.Description;
		feed.Title = feedDetail.Title;
		feed.ImageUrl = feedDetail.ImageUrl;
		feed.Location = feedDetail.Location;

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
	/// Returns a <see cref="FeedDetail"/> by the specified <paramref name="feedLocation"/>. Returns null if no feed is found.
	/// </summary>
	/// <param name="feedLocation"></param>
	/// <returns></returns>
	public async Task<FeedDetail?> GetFeedDetail(string? feedLocation)
	{
		var feed = await GetFeedByLocation(feedLocation);

		return feed == null
			? null
			: FeedToFeedDetail(feed);
	}

	/// <inheritdoc />
	public async Task<List<FeedDetail>> GetFeedDetails()
	{
		var feeds = await GetFeedsByCriteria();

		if (feeds.Count == 0)
		{
			return new List<FeedDetail>();
		}

		return feeds.Select(FeedToFeedDetail)
			.ToList();
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
			Location = feed.Location,
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

	/// <summary>
	/// Returns the specified <see cref="Feed"/> by Location, or null if not found
	/// </summary>
	/// <param name="location"></param>
	/// <returns></returns>
	private async Task<Feed?> GetFeedByLocation(string? location)
	{
		// Avoid a database call if location is not meaningful
		if (string.IsNullOrWhiteSpace(location))
		{
			return null;
		}

		return await _context.Feeds.FirstOrDefaultAsync(x => x.Location == location);
	}

	/// <summary>
	/// Returns all <see cref="Feed"/> that exist, by the given TODO: Criteria
	/// <para>
	///	Guaranteed to return a <see cref="List{T}"/>, but is not guaranteed to contain any elements.
	/// </para>
	/// </summary>
	/// <returns></returns>
	private async Task<List<Feed>> GetFeedsByCriteria( /*TODO: Implement criteria later*/)
	{
		return await _context.Feeds
			// TODO: criteria stuff here
			.ToListAsync();
	}
}