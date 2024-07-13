namespace Nulah.RSS.Core.Models;

public class FeedItem
{
	public string Title { get; set; } = null!;
	public string Url { get; set; } = null!;
	public string? Summary { get; set; }
	public string? Content { get; set; }
	public string? Author { get; set; }
	public DateTimeOffset Published { get; set; }
}