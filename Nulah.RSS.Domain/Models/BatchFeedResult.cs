namespace Nulah.RSS.Domain.Models;

public class BatchFeedResult
{
	public List<FeedDetail> CreatedFeeds { get; set; } = new();
	public List<string> Errors { get; set; } = new();
}