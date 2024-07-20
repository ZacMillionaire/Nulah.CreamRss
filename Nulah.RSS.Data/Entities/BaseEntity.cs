using System.ComponentModel.DataAnnotations;

namespace Nulah.RSS.Data.Entities;

internal class BaseEntity
{
	[Key]
	public int Id { get; set; }

	public DateTimeOffset CreatedUtc { get; internal set; }
	public DateTimeOffset UpdatedUtc { get; internal set; }
}