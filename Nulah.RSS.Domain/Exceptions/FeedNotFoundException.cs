namespace Nulah.RSS.Domain.Exceptions;

public class FeedNotFoundException : Exception
{
	public FeedNotFoundException(string message) : base(message)
	{
	}
}