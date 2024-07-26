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

	/// <inheritdoc />
	public async Task<FeedDetail> CreateFeedDetail(FeedDetail feedDetail)
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

		return await _feedManager.CreateFeedDetail(feedDetail);
	}

	/// <inheritdoc />
	public async Task<FeedDetail> UpdateFeedDetail(FeedDetail feedDetail)
	{
		var context = new ValidationContext(feedDetail, null, null);
		var validationErrors = new List<ValidationResult>();
		if (!Validator.TryValidateObject(feedDetail, context, validationErrors, true))
		{
			// Add a validation here where the Id needs to be greater than 0
			if (feedDetail.Id <= 0)
			{
				validationErrors.Add(new ValidationResult("Id must be greater than 0", new[] { nameof(feedDetail.Id) }));
			}

			throw new ValidationException("FeedDetail is invalid")
			{
				Data = { { "ValidationErrors", validationErrors } }
			};
		}

		return await _feedManager.UpdateFeedDetail(feedDetail);
	}

	/// <inheritdoc />
	public async Task<FeedDetail?> GetFeedDetail(int feedId)
	{
		return await _feedManager.GetFeedDetail(feedId);
	}

	/// <inheritdoc />
	public async Task<List<FeedDetail>> GetFeedDetails()
	{
		return await _feedManager.GetFeedDetails();
	}
}