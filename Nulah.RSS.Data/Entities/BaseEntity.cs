using System.ComponentModel.DataAnnotations;

namespace Nulah.RSS.Data.Entities;

internal class BaseEntity
{
	[Key]
	public int Id { get; set; }

	public DateTime CreatedUtc { get; internal set; }
	public DateTime UpdatedUtc { get; internal set; }
}