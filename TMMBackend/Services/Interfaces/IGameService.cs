using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameService
{
	Task<GameResponse> CreateAsync(CreateGameRequest request);
	Task<GameResponse?> GetByIdAsync(string id);

	Task<bool> JoinAsync(string gameId, string userId, string displayName);

	Task<List<GameResponse>> SearchAsync(SearchGamesRequest request);

	Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request);
}