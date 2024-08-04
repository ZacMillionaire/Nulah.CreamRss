using System.ComponentModel.DataAnnotations;

namespace Nulah.RSS.Domain.Models;

public class FeedDetail
{
	public int Id { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "Title cannot be empty")]
	public string Title { get; set; } = null!;

	public string? ImageUrl { get; set; }
	public string? Description { get; set; }

	/// <summary>
	/// The location of the feed, either a file or remote uri. Distinct across all saved feeds
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "Location cannot be empty")]
	public string Location { get; set; } = null!;

	public DateTimeOffset CreatedUtc { get; set; }
	public DateTimeOffset UpdatedUtc { get; set; }
	public byte[]? ImageBlob { get; set; }
}