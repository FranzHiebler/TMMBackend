using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Domain;

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

		if (!string.IsNullOrWhiteSpace(r.City))
			f &= Builders<GameSession>.Filter.Eq(x => x.LocationSnapshot.City, r.City);

		if (r.OnlyOpen)
			f &= Builders<GameSession>.Filter.Eq(x => x.Status, GameSessionState.Open);

		if (r.FromUtc.HasValue)
			f &= Builders<GameSession>.Filter.Gte(x => x.StartTimeUtc, r.FromUtc.Value);

		var games = await _games.Find(f).SortBy(x => x.StartTimeUtc).ToListAsync();

		if (!string.IsNullOrWhiteSpace(r.SystemKey))
		{
			games = games
				.Where(g => g.Tables.Any(t => SystemMatches(t.Systems, r.SystemKey)))
				.ToList();
		}

		if (r.OnlyJoinable)
		{
			games = games
				.Where(g => g.Tables.Any(t => t.AssignedPlayers.Count < t.MaxPlayers))
				.ToList();
		}

		return games;
	}

	public async Task<List<GameSession>> SearchNearbyAsync(SearchNearbyGamesRequest r, List<string> locationIds)
	{
		if (locationIds.Count == 0)
			return new List<GameSession>();

		var f = Builders<GameSession>.Filter.In(x => x.LocationId, locationIds);

		if (r.OnlyOpen)
			f &= Builders<GameSession>.Filter.Eq(x => x.Status, GameSessionState.Open);

		if (r.FromUtc.HasValue)
			f &= Builders<GameSession>.Filter.Gte(x => x.StartTimeUtc, r.FromUtc.Value);

		var games = await _games.Find(f).SortBy(x => x.StartTimeUtc).ToListAsync();

		if (!string.IsNullOrWhiteSpace(r.SystemKey))
		{
			games = games
				.Where(g => g.Tables.Any(t => SystemMatches(t.Systems, r.SystemKey)))
				.ToList();
		}

		return games;
	}

	public async Task UpdateAsync(GameSession game)
	{
		await _games.ReplaceOneAsync(x => x.Id == game.Id, game);
	}

	private static bool SystemMatches(List<string> systems, string systemKey)
	{
		return systems.Count == 0
			|| systems.Contains("egal", StringComparer.OrdinalIgnoreCase)
			|| systems.Contains(systemKey, StringComparer.OrdinalIgnoreCase);
	}
}