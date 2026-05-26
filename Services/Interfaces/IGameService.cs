using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameService
{
	Task<GameResponse> CreateAsync(CreateGameRequest request);
	Task<GameResponse?> GetByIdAsync(string id);
	Task<GameResponse> UpdateSessionAsync(string gameId, UpdateGameSessionRequest request);
	Task<GameResponse> UpdateTableAsync(string gameId, string tableId, UpdateGameTableRequest request);
	Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request);
	Task ApplyAsync(string gameId, ApplyToGameRequest request);
	Task AssignPlayerToTableAsync(string gameId, string tableId, AssignPlayerToTableRequest request);
	Task<List<GameResponse>> SearchAsync(SearchGamesRequest request);
	Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request);
	Task RejectApplicationAsync(string gameId, string applicationId);
	Task<GameResponse> CreateChangeProposalAsync(string gameId, CreateChangeProposalRequest request);
	Task<GameResponse> AcceptChangeProposalAsync(string gameId, string proposalId);
	Task<GameResponse> RejectChangeProposalAsync(string gameId, string proposalId);
	Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId);
	Task MovePlayerToTableAsync(string gameId, string userId, MovePlayerToTableRequest request);
	Task<GameResponse> AddDateOptionAsync(string gameId, AddDateOptionRequest request);
	Task<GameResponse> VoteDateOptionAsync(string gameId, string optionId);
	Task<GameResponse> SelectDateOptionAsync(string gameId, string optionId);
	Task<GameResponse> InviteFriendAsync(string gameId, InviteFriendToSessionRequest request);
	Task<GameResponse> RespondInvitationAsync(string gameId, string invitationId, bool accept);
	Task<GameResponse> JoinWaitlistAsync(string gameId, JoinWaitlistRequest request);
	Task<GameResponse> PromoteWaitlistEntryAsync(string gameId, string waitlistEntryId, string tableId);
	Task<GameResponse> CloseGameAsync(string gameId, CloseGameRequest request);
	Task<GameResponse> CancelGameAsync(string gameId);
	Task<PublicGameResponse?> GetPublicAsync(string slugOrId);
	Task<List<CalendarItemResponse>> GetCalendarAsync();
}
