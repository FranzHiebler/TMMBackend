using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IFriendRepository
{
	Task<List<Friendship>> GetAcceptedForUserAsync(string userId);
	Task<List<Friendship>> GetPendingRequestsForUserAsync(string userId);
	Task<Friendship?> GetByIdAsync(string id);
	Task<Friendship?> FindBetweenUsersAsync(string userAId, string userBId);
	Task CreateAsync(Friendship friendship);
	Task UpdateAsync(Friendship friendship);
	Task DeleteAsync(string id);
}
