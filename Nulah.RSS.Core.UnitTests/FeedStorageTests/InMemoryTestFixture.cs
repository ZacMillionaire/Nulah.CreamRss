using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class InMemoryTestFixture
{
	private readonly InMemoryDatabase _database;

	/// <summary>
	/// Creates a manager with a database context that has a time provider bound to the given time provider.
	/// <para>
	///	Managers created with this will have distinct contexts unless a previously used database name is given.
	/// </para>
	/// </summary>
	/// <param name="timeProvider">
	///	A controllable time provider, should be supplied by the caller, but if explicitly null one will be created at
	///	a fixed point in time
	/// </param>
	/// <param name="databaseName">
	///	Use the same name as a previous test (or reuse the name across a range of tests), to share context. Otherwise if
	///	left null each call will use a fresh context
	///	</param>
	/// <returns></returns>
	internal IFeedManager CreateManager(TimeProvider timeProvider, string? databaseName = null)
		=> new FeedManager(_database.CreateContext(timeProvider, databaseName));

	public InMemoryTestFixture()
	{
		_database = new InMemoryDatabase();
	}
}