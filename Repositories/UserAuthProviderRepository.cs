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

	public async Task InsertAsync(UserAuthProvider provider)
	{
		Normalize(provider);
		try
		{
			await _providers.InsertOneAsync(provider);
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			var existing = await GetByProviderAsync(provider.Provider, provider.ProviderUserId)
				?? await GetByUserAndProviderAsync(provider.UserId, provider.Provider);

			if (existing == null)
				throw;

			await UpdateLoginAsync(existing, provider.ProviderUserId, provider.Email, provider.LastLoginAtUtc);
		}
	}

	public async Task UpdateLoginAsync(UserAuthProvider provider, string providerUserId, string email, DateTime lastLoginAtUtc)
	{
		if (string.IsNullOrWhiteSpace(provider.Id))
			throw new InvalidOperationException("UserAuthProvider.Id fehlt.");

		if (string.IsNullOrWhiteSpace(providerUserId))
			throw new InvalidOperationException("UserAuthProvider.ProviderUserId fehlt.");

		if (string.IsNullOrWhiteSpace(email))
			throw new InvalidOperationException("UserAuthProvider.Email fehlt.");

		var linkedAtUtc = provider.LinkedAtUtc == default ? lastLoginAtUtc : provider.LinkedAtUtc;
		var update = Builders<UserAuthProvider>.Update
			.Set(x => x.ProviderUserId, providerUserId)
			.Set(x => x.Email, email)
			.Set(x => x.LinkedAtUtc, linkedAtUtc)
			.Set(x => x.LastLoginAtUtc, lastLoginAtUtc);

		await _providers.UpdateOneAsync(x => x.Id == provider.Id, update);
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
