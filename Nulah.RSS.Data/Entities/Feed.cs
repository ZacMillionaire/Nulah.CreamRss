using System.ComponentModel.DataAnnotations.Schema;

namespace Nulah.RSS.Data.Entities;

internal sealed class Feed : BaseEntity
{
	public string Title { get; set; } = null!;
	public string? ImageUrl { get; set; }
	public byte[]? ImageBlob { get; set; }
	public byte[]? FaviconBlob { get; set; }
	public string? Description { get; set; }

	[Index(IsUnique = true)]
	public string Location { get; set; } = null!;
}