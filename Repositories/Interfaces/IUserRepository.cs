using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IUserRepository
{
	Task<List<UserProfile>> SearchAsync(string? query);
	Task<UserProfile?> GetByIdAsync(string userId);
	Task<UserProfile?> GetByEmailAsync(string email);
	Task UpsertAsync(UserProfile user);
}
