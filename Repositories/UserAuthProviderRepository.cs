using Microsoft.Extensions.Options;
using MongoDB.Bson;
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
		Normalize(provider);

		try
		{
			await _providers.ReplaceOneAsync(
				x => x.UserId == provider.UserId && x.Provider == provider.Provider,
				provider,
				new ReplaceOptions { IsUpsert = true });
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			var existing = await GetByProviderAsync(provider.Provider, provider.ProviderUserId)
				?? await GetByUserAndProviderAsync(provider.UserId, provider.Provider);

			if (existing == null)
				throw;

			existing.ProviderUserId = provider.ProviderUserId;
			existing.Email = provider.Email;
			existing.LastLoginAtUtc = provider.LastLoginAtUtc;
			if (existing.LinkedAtUtc == default)
				existing.LinkedAtUtc = provider.LinkedAtUtc;
			Normalize(existing);

			await _providers.ReplaceOneAsync(x => x.Id == existing.Id, existing);
		}
	}

	private static void Normalize(UserAuthProvider provider)
	{
		if (string.IsNullOrWhiteSpace(provider.Id))
			provider.Id = ObjectId.GenerateNewId().ToString();

		if (string.IsNullOrWhiteSpace(provider.UserId))
			throw new InvalidOperationException("UserAuthProvider.UserId fehlt.");

		if (string.IsNullOrWhiteSpace(provider.Provider))
			throw new InvalidOperationException("UserAuthProvider.Provider fehlt.");

		if (string.IsNullOrWhiteSpace(provider.ProviderUserId))
			throw new InvalidOperationException("UserAuthProvider.ProviderUserId fehlt.");

		if (string.IsNullOrWhiteSpace(provider.Email))
			throw new InvalidOperationException("UserAuthProvider.Email fehlt.");

		var now = DateTime.UtcNow;
		if (provider.LinkedAtUtc == default)
			provider.LinkedAtUtc = now;

		if (provider.LastLoginAtUtc == default)
			provider.LastLoginAtUtc = now;
	}
}
