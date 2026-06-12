using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class UserRepository : IUserRepository
{
	private readonly IMongoCollection<UserProfile> _users;

	public UserRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_users = database.GetCollection<UserProfile>("users");
	}

	public async Task<List<UserProfile>> SearchAsync(string? query)
	{
		var escapedQuery = string.IsNullOrWhiteSpace(query)
			? null
			: Regex.Escape(query.Trim());

		var filter = string.IsNullOrWhiteSpace(escapedQuery)
			? FilterDefinition<UserProfile>.Empty
			: Builders<UserProfile>.Filter.Regex(
				x => x.DisplayName,
				new BsonRegularExpression(escapedQuery, "i"));

		return await _users
			.Find(filter)
			.SortBy(x => x.DisplayName)
			.Limit(200)
			.ToListAsync();
	}

	public async Task<List<UserProfile>> GetDevUsersAsync()
	{
		return await _users
			.Find(x => x.IsDevUser)
			.SortBy(x => x.DisplayName)
			.ToListAsync();
	}

	public async Task<UserProfile?> GetByIdAsync(string userId)
	{
		return await _users.Find(x => x.Id == userId).FirstOrDefaultAsync();
	}

	public async Task<UserProfile?> GetByEmailAsync(string email)
	{
		var escapedEmail = Regex.Escape(email.Trim());
		return await _users
			.Find(Builders<UserProfile>.Filter.Regex(
				x => x.Email,
				new BsonRegularExpression($"^{escapedEmail}$", "i")))
			.FirstOrDefaultAsync();
	}

	public async Task UpsertAsync(UserProfile user)
	{
		await _users.ReplaceOneAsync(
			x => x.Id == user.Id,
			user,
			new ReplaceOptions { IsUpsert = true });
	}
}
