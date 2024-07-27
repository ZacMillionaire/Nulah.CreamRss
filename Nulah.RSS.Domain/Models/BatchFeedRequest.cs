namespace Nulah.RSS.Domain.Models;

public class BatchFeedRequest
{
	public List<string> Locations { get; set; } = new();
}