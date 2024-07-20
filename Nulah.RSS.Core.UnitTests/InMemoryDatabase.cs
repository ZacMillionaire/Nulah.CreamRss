using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Nulah.RSS.Data;

namespace Nulah.RSS.Core.UnitTests;

public class InMemoryDatabase
{
	// 
	private readonly DateTimeOffset _baseDateTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

	public InMemoryDatabase()
	{
		var context = CreateContext();
		// Delete existing db before creating a new one
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
	}

	/// <summary>
	/// Creates an in-memory database context with the provided <paramref name="timeProvider"/>.
	/// <para>
	///	If no <paramref name="timeProvider"/> is given, it will be defaulted to an uncontrollable
	/// <see cref="FakeTimeProvider"/> with a base <see cref="DateTimeOffset"/> of 01/01/2000 00:00 UTC
	/// </para>
	/// </summary>
	/// <param name="timeProvider">
	/// Test controlled <see cref="TimeProvider"/>, or an uncontrolled default <see cref="FakeTimeProvider"/> with a
	/// base <see cref="DateTimeOffset"/> of 01/01/2000 00:00 UTC
	/// </param>
	/// <returns></returns>
	public FeedContext CreateContext(TimeProvider? timeProvider = null)
	{
		var builder = new DbContextOptionsBuilder<FeedContext>();
		builder.UseInMemoryDatabase(databaseName: "InMemoryFeed");

		var dbContextOptions = builder.Options;
		return new FeedContext(timeProvider ?? new FakeTimeProvider(_baseDateTime), dbContextOptions);
	}
}