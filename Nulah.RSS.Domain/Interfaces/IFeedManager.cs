using Nulah.RSS.Core.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedManager
{
	Task<FeedDetail> SaveFeedDetail(FeedDetail feedDetail);
}