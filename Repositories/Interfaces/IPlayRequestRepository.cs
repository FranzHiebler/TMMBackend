using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IPlayRequestRepository
{
	Task CreateAsync(PlayRequest request);
	Task<PlayRequest?> GetByIdAsync(string id);
	Task<List<PlayRequest>> GetOpenAsync();
	Task<List<PlayRequest>> GetForUserAsync(string userId);
	Task UpdateAsync(PlayRequest request);
}
