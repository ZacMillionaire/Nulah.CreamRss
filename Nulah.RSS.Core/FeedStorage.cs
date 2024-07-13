using Nulah.RSS.Core.Models;
using Nulah.RSS.Data;
using Nulah.RSS.Domain.Interfaces;

namespace Nulah.RSS.Core;

public class FeedStorage : IFeedStorage
{
	private readonly FeedManager _feedManager;

	public FeedStorage(FeedManager feedManager)
	{
		_feedManager = feedManager;
	}

	public async Task<FeedDetail> SaveFeedDetails(FeedDetail feedDetail)
	{
		return await _feedManager.SaveFeedDetail(feedDetail);
	}
}