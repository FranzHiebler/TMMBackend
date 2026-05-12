using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services.Interfaces;

public interface ILocationLookupService
{
	Task<Location?> GetByIdAsync(string id);
}