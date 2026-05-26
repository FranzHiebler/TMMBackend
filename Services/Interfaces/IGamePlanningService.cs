using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGamePlanningService
{
	Task<GameResponse> AddDateOptionAsync(string gameId, AddDateOptionRequest request);
	Task<GameResponse> VoteDateOptionAsync(string gameId, string optionId);
	Task<GameResponse> SelectDateOptionAsync(string gameId, string optionId);
	Task<GameResponse> InviteFriendAsync(string gameId, InviteFriendToSessionRequest request);
	Task<GameResponse> RespondInvitationAsync(string gameId, string invitationId, bool accept);
	Task<GameResponse> JoinWaitlistAsync(string gameId, JoinWaitlistRequest request);
	Task<GameResponse> PromoteWaitlistEntryAsync(string gameId, string waitlistEntryId, string tableId);
	Task<GameResponse> CloseGameAsync(string gameId, CloseGameRequest request);
	Task<GameResponse> CancelGameAsync(string gameId);
	Task<List<CalendarItemResponse>> GetCalendarAsync();
}
