using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Infrastructure;

public class MongoIndexInitializer
{
	private readonly MongoDbSettings _settings;

	public MongoIndexInitializer(IOptions<MongoDbSettings> settings)
	{
		_settings = settings.Value;
	}

	public async Task EnsureIndexesAsync()
	{
		var client = new MongoClient(_settings.ConnectionString);
		var database = client.GetDatabase(_settings.DatabaseName);

		await EnsureGameIndexesAsync(database);
		await EnsureLocationIndexesAsync(database);
		await EnsureFriendIndexesAsync(database);
		await EnsureMessageIndexesAsync(database);
		await EnsureNotificationIndexesAsync(database);
		await EnsureSystemIndexesAsync(database);
		await EnsureUserIndexesAsync(database);
	}

	private async Task EnsureGameIndexesAsync(IMongoDatabase database)
	{
		var games = database.GetCollection<GameSession>(_settings.GamesCollectionName);

		await games.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.StartTimeUtc)),
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.LocationId)),
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.Status))
		});
	}

	private async Task EnsureLocationIndexesAsync(IMongoDatabase database)
	{
		var locations = database.GetCollection<Location>("locations");

		await locations.Indexes.CreateOneAsync(new CreateIndexModel<Location>(
			Builders<Location>.IndexKeys.Geo2DSphere(x => x.Geo)));
	}

	private async Task EnsureFriendIndexesAsync(IMongoDatabase database)
	{
		var friendships = database.GetCollection<Friendship>("friendships");

		await friendships.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<Friendship>(Builders<Friendship>.IndexKeys.Ascending(x => x.RequesterUserId)),
			new CreateIndexModel<Friendship>(Builders<Friendship>.IndexKeys.Ascending(x => x.ReceiverUserId)),
			new CreateIndexModel<Friendship>(Builders<Friendship>.IndexKeys.Ascending(x => x.Status))
		});
	}

	private async Task EnsureMessageIndexesAsync(IMongoDatabase database)
	{
		var threads = database.GetCollection<MessageThread>("messageThreads");
		var messages = database.GetCollection<Message>("messages");

		await threads.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<MessageThread>(Builders<MessageThread>.IndexKeys.Ascending("participants.userId")),
			new CreateIndexModel<MessageThread>(Builders<MessageThread>.IndexKeys.Descending(x => x.LastMessageAtUtc))
		});

		await messages.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(x => x.ConversationId)),
			new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(x => x.GameId)),
			new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(x => x.GameId).Ascending(x => x.TableId)),
			new CreateIndexModel<Message>(Builders<Message>.IndexKeys.Ascending(x => x.CreatedAtUtc))
		});
	}

	private async Task EnsureNotificationIndexesAsync(IMongoDatabase database)
	{
		var notifications = database.GetCollection<Notification>("notifications");

		await notifications.Indexes.CreateOneAsync(new CreateIndexModel<Notification>(
			Builders<Notification>.IndexKeys
				.Ascending(x => x.UserId)
				.Ascending(x => x.IsRead)
				.Descending(x => x.CreatedAtUtc)));
	}

	private async Task EnsureSystemIndexesAsync(IMongoDatabase database)
	{
		var systems = database.GetCollection<SystemDefinition>("systems");

		await systems.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<SystemDefinition>(
				Builders<SystemDefinition>.IndexKeys.Ascending(x => x.Key),
				new CreateIndexOptions { Unique = true }),

			new CreateIndexModel<SystemDefinition>(
				Builders<SystemDefinition>.IndexKeys.Ascending(x => x.Name))
		});
	}

	private async Task EnsureUserIndexesAsync(IMongoDatabase database)
	{
		var users = database.GetCollection<UserProfile>("users");

		await users.Indexes.CreateOneAsync(
			new CreateIndexModel<UserProfile>(
				Builders<UserProfile>.IndexKeys.Ascending(x => x.DisplayName)));
	}
}