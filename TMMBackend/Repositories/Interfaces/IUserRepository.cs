using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IUserRepository
{
	Task<List<UserProfile>> SearchAsync(string? query);
}