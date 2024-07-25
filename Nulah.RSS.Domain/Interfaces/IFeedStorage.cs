using System.ComponentModel.DataAnnotations;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedStorage
{
	/// <summary>
	/// Validates and saves the given <see cref="FeedDetail"/>, returning a <see cref="FeedDetail"/> with database
	/// generated values
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="ValidationException"></exception>
	Task<FeedDetail> SaveFeedDetails(FeedDetail feedDetail);

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