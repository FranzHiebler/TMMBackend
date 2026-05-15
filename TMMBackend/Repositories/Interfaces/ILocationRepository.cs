using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface ILocationRepository
{
	Task<List<Location>> GetAllAsync();
	Task<List<Location>> GetByIdsAsync(List<string> ids);
	Task<Location?> GetByIdAsync(string id);
	Task CreateAsync(Location location);
	Task UpdateAsync(Location location);
	Task<List<Location>> GetForUserAsync(string userId);
	Task<List<NearbyLocationResult>> FindNearbyAsync(double lat, double lng, double radiusInMeters);
	Task<List<Location>> FindNearbyLocationsAsync(double lat, double lng, double radiusInMeters);
	Task<List<Location>> GetWithGeoAsync();
}
