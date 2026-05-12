using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
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
		var filter = string.IsNullOrWhiteSpace(query)
			? FilterDefinition<UserProfile>.Empty
			: Builders<UserProfile>.Filter.Regex(
				x => x.DisplayName,
				new BsonRegularExpression(query, "i"));

		return await _users.Find(filter).Limit(20).ToListAsync();
	}
}