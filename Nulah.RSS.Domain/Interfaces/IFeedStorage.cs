using System.ComponentModel.DataAnnotations;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedStorage
{
	/// <summary>
	/// Validates and creates the given <see cref="FeedDetail"/>, returning a <see cref="FeedDetail"/> with database
	/// generated values.
	/// <para>
	/// Validation errors will be contained within the <see cref="ValidationException.Data"/>
	/// property of the thrown exception.
	/// </para>
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="ValidationException"></exception>
	Task<FeedDetail> CreateFeedDetail(FeedDetail feedDetail);

	/// <summary>
	/// Updates the given feed detail, matching on Id and Location.
	/// <para>
	/// Validation errors will be contained within the <see cref="ValidationException.Data"/>
	/// property of the thrown exception.
	/// </para>
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Returns a feed if one is found by the given location
	/// </summary>
	/// <param name="feedRequestFeedLocation"></param>
	/// <returns></returns>
	Task<FeedDetail?> GetFeedByLocation(string? feedRequestFeedLocation);
}