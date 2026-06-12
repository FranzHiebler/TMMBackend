using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class UserAuthProviderRepository : IUserAuthProviderRepository
{
	private readonly IMongoCollection<UserAuthProvider> _providers;

	public UserAuthProviderRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_providers = database.GetCollection<UserAuthProvider>("userAuthProviders");
	}

	public async Task<UserAuthProvider?> GetByProviderAsync(string provider, string providerUserId)
	{
		return await _providers
			.Find(x => x.Provider == provider && x.ProviderUserId == providerUserId)
			.FirstOrDefaultAsync();
	}

	public async Task<UserAuthProvider?> GetByUserAndProviderAsync(string userId, string provider)
	{
		return await _providers
			.Find(x => x.UserId == userId && x.Provider == provider)
			.FirstOrDefaultAsync();
	}

	public async Task UpsertAsync(UserAuthProvider provider)
	{
		await _providers.ReplaceOneAsync(
			x => x.UserId == provider.UserId && x.Provider == provider.Provider,
			provider,
			new ReplaceOptions { IsUpsert = true });
	}
}
