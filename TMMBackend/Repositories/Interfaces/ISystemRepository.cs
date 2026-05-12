using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface ISystemRepository
{
	Task<List<SystemDefinition>> GetAllAsync();
	Task<SystemDefinition> CreateAsync(string key, string name);
}