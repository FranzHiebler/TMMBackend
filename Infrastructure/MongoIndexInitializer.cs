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
		await EnsurePlayRequestIndexesAsync(database);
		await EnsureEventSeriesIndexesAsync(database);
		await EnsureFeedbackIndexesAsync(database);
		await EnsureUserAuthProviderIndexesAsync(database);
	}

	private async Task EnsureGameIndexesAsync(IMongoDatabase database)
	{
		var games = database.GetCollection<GameSession>(_settings.GamesCollectionName);

		await games.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.StartTimeUtc)),
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.LocationId)),
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.Status)),
			new CreateIndexModel<GameSession>(Builders<GameSession>.IndexKeys.Ascending(x => x.PublicSlug))
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

		await users.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<UserProfile>(
				Builders<UserProfile>.IndexKeys.Ascending(x => x.DisplayName)),
			new CreateIndexModel<UserProfile>(
				Builders<UserProfile>.IndexKeys.Ascending(x => x.IsSystemAdmin)),
			new CreateIndexModel<UserProfile>(
				Builders<UserProfile>.IndexKeys.Ascending(x => x.IsDevUser))
		});
	}

	private async Task EnsurePlayRequestIndexesAsync(IMongoDatabase database)
	{
		var requests = database.GetCollection<PlayRequest>("playRequests");
		await requests.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<PlayRequest>(Builders<PlayRequest>.IndexKeys.Ascending(x => x.Status).Descending(x => x.UpdatedAtUtc)),
			new CreateIndexModel<PlayRequest>(Builders<PlayRequest>.IndexKeys.Ascending(x => x.Owner.UserId)),
			new CreateIndexModel<PlayRequest>(Builders<PlayRequest>.IndexKeys.Ascending(x => x.SystemKey))
		});
	}

	private async Task EnsureEventSeriesIndexesAsync(IMongoDatabase database)
	{
		var series = database.GetCollection<EventSeries>("eventSeries");
		await series.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<EventSeries>(Builders<EventSeries>.IndexKeys.Ascending(x => x.LocationId)),
			new CreateIndexModel<EventSeries>(Builders<EventSeries>.IndexKeys.Ascending(x => x.Host.UserId))
		});
	}

	private async Task EnsureFeedbackIndexesAsync(IMongoDatabase database)
	{
		var feedback = database.GetCollection<FeedbackItem>("feedbackItems");
		await feedback.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<FeedbackItem>(Builders<FeedbackItem>.IndexKeys.Descending(x => x.CreatedAtUtc)),
			new CreateIndexModel<FeedbackItem>(Builders<FeedbackItem>.IndexKeys.Ascending(x => x.TicketNumber)),
			new CreateIndexModel<FeedbackItem>(Builders<FeedbackItem>.IndexKeys.Ascending(x => x.Status)),
			new CreateIndexModel<FeedbackItem>(Builders<FeedbackItem>.IndexKeys.Ascending(x => x.Type)),
			new CreateIndexModel<FeedbackItem>(Builders<FeedbackItem>.IndexKeys.Ascending(x => x.UserId))
		});
	}

	private async Task EnsureUserAuthProviderIndexesAsync(IMongoDatabase database)
	{
		var providers = database.GetCollection<UserAuthProvider>("userAuthProviders");
		await providers.Indexes.CreateManyAsync(new[]
		{
			new CreateIndexModel<UserAuthProvider>(
				Builders<UserAuthProvider>.IndexKeys
					.Ascending(x => x.Provider)
					.Ascending(x => x.ProviderUserId),
				new CreateIndexOptions { Unique = true }),
			new CreateIndexModel<UserAuthProvider>(
				Builders<UserAuthProvider>.IndexKeys
					.Ascending(x => x.UserId)
					.Ascending(x => x.Provider),
				new CreateIndexOptions { Unique = true })
		});
	}
}
