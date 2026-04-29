using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;
using TMMBackend.Domain;

namespace TabletopMatchMaker.Repositories;

public class GameRepository : IGameRepository
{
	private readonly IMongoCollection<GameSession> _games;

	public GameRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_games = database.GetCollection<GameSession>(settings.Value.GamesCollectionName);
	}

	public async Task CreateAsync(GameSession game)
	{
		await _games.InsertOneAsync(game);
	}

	public async Task<GameSession?> GetByIdAsync(string id)
	{
		return await _games.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task<List<GameSession>> SearchAsync(SearchGamesRequest r)
	{
		var f = Builders<GameSession>.Filter.Empty;

		if (!string.IsNullOrWhiteSpace(r.SystemKey))
			f &= Builders<GameSession>.Filter.Eq(x => x.System.Key, r.SystemKey);

		if (!string.IsNullOrWhiteSpace(r.City))
			f &= Builders<GameSession>.Filter.Eq(x => x.LocationSnapshot.City, r.City);

		if (r.OnlyOpen)
			f &= Builders<GameSession>.Filter.Eq(x => x.Status, GameSessionState.Open);

		if (r.FromUtc.HasValue)
			f &= Builders<GameSession>.Filter.Gte(x => x.StartTimeUtc, r.FromUtc.Value);

		if (r.OnlyJoinable)
		{
			f &= new MongoDB.Bson.BsonDocument("$expr",
				new MongoDB.Bson.BsonDocument("$lt", new MongoDB.Bson.BsonArray
				{
			new MongoDB.Bson.BsonDocument("$size", "$participants"),
			"$maxPlayers"
				}));
		}
		return await _games
			.Find(f)
			.SortBy(x => x.StartTimeUtc)
			.ToListAsync();
	}
	public async Task UpdateAsync(GameSession gameSessions)
	{
		await _games.ReplaceOneAsync(x => x.Id == gameSessions.Id, gameSessions);
	}

public async Task<List<GameSession>> SearchNearbyAsync(SearchNearbyGamesRequest r, List<string> locationIds)
{
	var f = Builders<GameSession>.Filter.Empty;

	if (locationIds.Count == 0)
		return new List<GameSession>();

	f &= Builders<GameSession>.Filter.In(x => x.LocationId, locationIds);

	if (!string.IsNullOrWhiteSpace(r.SystemKey))
		f &= Builders<GameSession>.Filter.Eq(x => x.System.Key, r.SystemKey);

	if (r.OnlyOpen)
		f &= Builders<GameSession>.Filter.Eq(x => x.Status, GameSessionState.Open);

	if (r.FromUtc.HasValue)
		f &= Builders<GameSession>.Filter.Gte(x => x.StartTimeUtc, r.FromUtc.Value);

	return await _games.Find(f)
		.SortBy(x => x.StartTimeUtc)
		.ToListAsync();
}
}