namespace Nulah.RSS.Data.Entities;

internal sealed class Feed : BaseEntity
{
	public string Title { get; set; } = null!;
	public string? ImageUrl { get; set; }
	public string? Description { get; set; }
}