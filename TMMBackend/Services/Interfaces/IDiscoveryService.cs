using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IDiscoveryService
{
	Task<List<LocationDiscoveryResponse>> GetLocationsAsync(LocationDiscoveryRequest request);
	Task<List<GameDiscoveryResponse>> GetGamesAsync(DiscoveryGamesRequest request);
}