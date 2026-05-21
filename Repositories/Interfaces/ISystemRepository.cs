using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface ISystemRepository
{
	Task<List<SystemDefinition>> GetAllAsync();
	Task<SystemDefinition> CreateAsync(CreateSystemRequest request);
}