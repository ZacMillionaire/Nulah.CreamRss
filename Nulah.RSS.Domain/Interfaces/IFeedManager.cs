using Nulah.RSS.Domain.Exceptions;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedManager
{
	/// <summary>
	/// Creates the given <paramref name="feedDetail"/> and returns a <see cref="FeedDetail"/> with database populated values.
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException">Thrown if a feed already exists by <see cref="FeedDetail.Location"/></exception>
	Task<FeedDetail> CreateFeedDetail(FeedDetail feedDetail);

	/// <summary>
	/// Updates an existing <see cref="FeedDetail"/> by Id.
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="FeedNotFoundException">No existing feed found by Id</exception>
	/// <exception cref="FeedLocationInUseException">Another feed was found with the incoming Location, and it's not the same as the feed being updated</exception>
	Task<FeedDetail> UpdateFeedDetail(FeedDetail feedDetail);

	/// <summary>
	/// Returns a <see cref="FeedDetail"/> by the specified <paramref name="feedId"/>. Returns null if no feed is found.
	/// </summary>
	/// <param name="feedId"></param>
	/// <returns></returns>
	Task<FeedDetail?> GetFeedDetail(int feedId);

	/// <summary>
	/// Returns all <see cref="FeedDetail"/> that exists.
	/// <para>
	///	Guaranteed to return a <see cref="List{T}"/> of <see cref="FeedDetail"/>, but not guaranteed to contain any elements
	/// </para>
	/// </summary>
	/// <returns></returns>
	Task<List<FeedDetail>> GetFeedDetails();
}