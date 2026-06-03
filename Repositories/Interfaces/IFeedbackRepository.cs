using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IFeedbackRepository
{
	Task CreateAsync(FeedbackItem item);
	Task<FeedbackItem?> GetByIdAsync(string id);
	Task<List<FeedbackItem>> GetAdminListAsync(FeedbackStatus? status, FeedbackType? type);
	Task UpdateAsync(FeedbackItem item);
}
