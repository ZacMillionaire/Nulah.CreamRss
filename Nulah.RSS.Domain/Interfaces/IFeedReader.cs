using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedReader
{
	/// <summary>
	/// Attempts to parse the feed located at the given location. The location can be a remote URL or locally accessible file.
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled
	/// </para>
	/// </summary>
	/// <param name="feedLocation">Remote URL or file path</param>
	/// <returns></returns>
	Task<FeedDetail> ParseFeedDetails(string? feedLocation);

	/// <summary>
	/// Returns all <see cref="FeedItem"/> for a feed by given location. Empty or null values will throw an exception.
	/// <para>
	/// Any failure to load the feed from the location (such as network issues or files that can't be found)
	/// will throw an exception that should be handled.
	/// </para>
	/// </summary>
	/// <param name="rssLocation"></param>
	/// <returns></returns>
	List<FeedItem> ParseFeedItems(string rssLocation);
}