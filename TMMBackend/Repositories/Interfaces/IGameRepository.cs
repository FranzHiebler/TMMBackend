using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IGameRepository
{
	Task CreateAsync(GameSession game);
	Task<GameSession?> GetByIdAsync(string id);
	Task<List<GameSession>> SearchAsync(SearchGamesRequest request);
	Task UpdateAsync(GameSession game);

	Task<List<GameSession>> SearchNearbyAsync(SearchNearbyGamesRequest request, List<string> locationIds);
}