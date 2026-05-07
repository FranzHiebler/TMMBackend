using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameService
{
	Task<GameResponse> CreateAsync(CreateGameRequest request);
	Task<GameResponse?> GetByIdAsync(string id);
	Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request);
	Task ApplyAsync(string gameId, ApplyToGameRequest request);
	Task<bool> AssignPlayerToTableAsync(string gameId, string tableId, AssignPlayerToTableRequest request);
	Task<List<GameResponse>> SearchAsync(SearchGamesRequest request);
	Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request);
	Task RejectApplicationAsync(string gameId, string applicationId);

}