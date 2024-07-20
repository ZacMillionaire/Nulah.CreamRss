using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedManager
{
	Task<FeedDetail> SaveFeedDetail(FeedDetail feedDetail);
}