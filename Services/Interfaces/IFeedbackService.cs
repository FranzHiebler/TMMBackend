using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IFeedbackService
{
	Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request);
	Task<List<FeedbackResponse>> GetAdminListAsync(FeedbackStatus? status, FeedbackType? type);
	Task<FeedbackResponse> UpdateAdminAsync(string id, UpdateFeedbackAdminRequest request);
}
