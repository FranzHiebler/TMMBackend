using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IUserAuthProviderRepository
{
	Task<UserAuthProvider?> GetByProviderAsync(string provider, string providerUserId);
	Task<UserAuthProvider?> GetByUserAndProviderAsync(string userId, string provider);
	Task UpsertAsync(UserAuthProvider provider);
}
