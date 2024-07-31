using Nulah.RSS.Core;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Test.Shared;

public class InMemoryTestFixture
{
	private readonly InMemoryTestDatabase _testDatabase;

	/// <summary>
	/// Creates a manager with a database context that has a time provider bound to the given time provider. This should
	/// not be directly used, and instead <see cref="CreateFeedStorage"/> should be used instead
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
	public IFeedManager CreateFeedManager(TimeProvider? timeProvider, string? databaseName = null)
		=> new FeedManager(_testDatabase.CreateContext(timeProvider, databaseName));

	/// <summary>
	/// Returns a <see cref="IFeedStorage"/> with a preset <see cref="IFeedManager"/>.
	/// <para>
	///	This is similar to <see cref="CreateFeedManager"/>, but instead
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
	public IFeedStorage CreateFeedStorage(TimeProvider? timeProvider, string? databaseName = null)
		=> new FeedStorage(CreateFeedManager(timeProvider, databaseName));

	/// <summary>
	/// Used to track if data has been seeded for this fixture.
	/// <para>
	///	This value is only modified from the test class that changes it, and changes to this value will not impact other
	/// test fixtures.
	/// </para>
	/// </summary>
	public bool DataSeeded;

	public InMemoryTestFixture()
	{
		_testDatabase = new InMemoryTestDatabase();
	}
}