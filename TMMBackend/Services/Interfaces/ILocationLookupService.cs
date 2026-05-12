using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services.Interfaces;

public interface ILocationLookupService
{
	Task<Location?> GetByIdAsync(string id);
	Task<List<NearbyLocationResult>> FindNearbyAsync(double lat, double lng, double radiusInMeters);
}