using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Nulah.RSS.Data.Entities;

[ExcludeFromCodeCoverage]
internal class BaseEntity
{
	[Key]
	public int Id { get; set; }

	public DateTimeOffset CreatedUtc { get; internal set; }
	public DateTimeOffset UpdatedUtc { get; internal set; }
}