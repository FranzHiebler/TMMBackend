using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IGameRepository
{
	Task CreateAsync(GameSession game);
	Task<GameSession?> GetByIdAsync(string id);
	Task<List<GameSession>> SearchAsync(SearchGamesRequest request);
	Task<List<GameSession>> SearchNearbyAsync(SearchNearbyGamesRequest request, List<string> locationIds);
	Task<List<GameSession>> SearchDiscoveryAsync(DateTime fromUtc, DateTime toUtc, List<string>? locationIds);
	Task<List<GameSession>> SearchUpcomingByLocationIdsAsync(DateTime fromUtc, List<string> locationIds);
	Task UpdateAsync(GameSession game);
}
