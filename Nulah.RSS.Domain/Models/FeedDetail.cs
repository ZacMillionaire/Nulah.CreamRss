using System.ComponentModel.DataAnnotations;

namespace Nulah.RSS.Domain.Models;

public class FeedDetail
{
	public int Id { get; set; }

	[Required(AllowEmptyStrings = false, ErrorMessage = "Title cannot be empty")]
	public string Title { get; set; } = null!;

	[Obsolete("Display images using ImageData. This property will not be available in future versions")]
	public string? ImageUrl { get; set; }

	/// <summary>
	/// Data image representation of the ImageUrl
	/// </summary>
	[Obsolete("Display images using ImageBlob. This property will not be available in future versions")]
	public string? ImageData { get; set; }

	public string? Description { get; set; }

	/// <summary>
	/// The location of the feed, either a file or remote uri. Distinct across all saved feeds
	/// </summary>
	[Required(AllowEmptyStrings = false, ErrorMessage = "Location cannot be empty")]
	public string Location { get; set; } = null!;

	public DateTimeOffset CreatedUtc { get; set; }
	public DateTimeOffset UpdatedUtc { get; set; }
	/// <summary>
	/// Header image for the feed (if any)
	/// </summary>
	public byte[]? ImageBlob { get; set; }
	/// <summary>
	/// The favicon (if any)
	/// </summary>
	public byte[]? Favicon { get; set; }
}