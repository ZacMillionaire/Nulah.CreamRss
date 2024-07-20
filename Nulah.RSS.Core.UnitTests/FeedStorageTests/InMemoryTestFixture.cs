namespace Nulah.RSS.Core.UnitTests.FeedStorageTests;

public class InMemoryTestFixture
{
	internal readonly InMemoryDatabase Database;

	public InMemoryTestFixture()
	{
		Database = new InMemoryDatabase();
	}
}