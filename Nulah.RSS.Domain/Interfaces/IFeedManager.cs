using System.ComponentModel.DataAnnotations;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedManager
{
	/// <summary>
	/// Creates or Updates the given <paramref name="feedDetail"/> if Id is given.
	/// <para>
	/// Validation errors will be contained within the <see cref="ValidationException.Data"/>
	/// property of the thrown exception.
	/// </para>
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="ValidationException"></exception>
	Task<FeedDetail> SaveFeedDetail(FeedDetail feedDetail);

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