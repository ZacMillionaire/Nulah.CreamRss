using System.ComponentModel.DataAnnotations;

namespace Nulah.RSS.Domain.Models;

public class FeedDetail
{
	public int Id { get; set; }
	[Required(AllowEmptyStrings = false, ErrorMessage = "Title cannot be empty")]
	public string Title { get; set; } = null!;
	public string? ImageUrl { get; set; }
	public string? Description { get; set; }

	public DateTimeOffset CreatedUtc { get; set; }
	public DateTimeOffset UpdatedUtc { get; set; }
}