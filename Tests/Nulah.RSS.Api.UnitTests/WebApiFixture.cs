namespace Nulah.RSS.Api.UnitTests;

/// <summary>
/// Fixture to be used to provide access to the Api wrapper <see cref="RssApi"/>.
/// <para>
///	The inheriting class for this fixture MUST create a new instance of a <see cref="RssApi"/> for <see cref="Api"/>,
/// which can have an optional <see cref="TimeProvider"/> given to be able to control time if needed for updates.
/// </para>
/// </summary>
public abstract class WebApiFixture : IClassFixture<ApiWebApplicationFactory>
{
	protected readonly ApiWebApplicationFactory WebApiFactory;

	protected RssApi Api = null!;

	/// <summary>
	/// Used to track if data has been seeded for this fixture.
	/// <para>
	///	This value is only modified from the test class that changes it, and changes to this value will not impact other
	/// test fixtures.
	/// </para>
	/// </summary>
	protected bool DataSeeded;

	protected WebApiFixture(ApiWebApplicationFactory fixture)
	{
		WebApiFactory = fixture;
	}
}