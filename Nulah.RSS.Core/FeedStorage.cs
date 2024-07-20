using System.ComponentModel.DataAnnotations;
using Nulah.RSS.Domain.Interfaces;
using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Core;

public class FeedStorage : IFeedStorage
{
	private readonly IFeedManager _feedManager;

	public FeedStorage(IFeedManager feedManager)
	{
		_feedManager = feedManager;
	}

	/// <summary>
	/// Creates or Updates the given <paramref name="feedDetail"/> if Id is given.
	/// <para>
	/// Validation errors will be contained within the <see cref="ValidationException.Data"/>
	/// property of the thrown exception.
	/// </para>
	/// </summary>
	/// <param name="feedDetail"></param>
	/// <returns></returns>
	/// <exception cref="ValidationException"></exception>
	public async Task<FeedDetail> SaveFeedDetails(FeedDetail feedDetail)
	{
		var context = new ValidationContext(feedDetail, null, null);
		var validationErrors = new List<ValidationResult>();
		if (!Validator.TryValidateObject(feedDetail, context, validationErrors, true))
		{
			throw new ValidationException("FeedDetail is invalid")
			{
				Data = { { "ValidationErrors", validationErrors } }
			};
		}

		return await _feedManager.SaveFeedDetail(feedDetail);
	}
}