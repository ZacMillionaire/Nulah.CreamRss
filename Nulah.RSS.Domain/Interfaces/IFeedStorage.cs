using Nulah.RSS.Domain.Models;

namespace Nulah.RSS.Domain.Interfaces;

public interface IFeedStorage
{
	Task<FeedDetail> SaveFeedDetails(FeedDetail feedDetail);
}