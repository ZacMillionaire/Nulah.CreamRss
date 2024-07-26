namespace Nulah.RSS.Domain.Exceptions;

public class FeedLocationInUseException : Exception
{
	public FeedLocationInUseException(string message) : base(message)
	{
	}
}