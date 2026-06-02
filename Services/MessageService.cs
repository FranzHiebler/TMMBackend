using MongoDB.Bson;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class MessageService : IMessageService
{
	private const int MaxBodyLength = 2000;

	private readonly IMessageRepository _messages;
	private readonly IGameRepository _games;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;
	private readonly INotificationService _notifications;

	public MessageService(
		IMessageRepository messages,
		IGameRepository games,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		INotificationService notifications)
	{
		_messages = messages;
		_games = games;
		_currentUser = currentUser;
		_authorization = authorization;
		_notifications = notifications;
	}

	public async Task<List<ConversationResponse>> GetConversationsAsync()
	{
		var threads = await _messages.GetThreadsForUserAsync(_currentUser.UserId);
		var result = new List<ConversationResponse>();

		foreach (var thread in threads)
		{
			result.Add(await MapConversationAsync(thread));
		}

		return result;
	}

	public async Task<ConversationDetailResponse> GetConversationAsync(string conversationId)
	{
		var thread = await GetThreadForCurrentUserOrThrow(conversationId);
		var messages = await _messages.GetConversationMessagesAsync(conversationId);
		var response = await MapConversationAsync(thread);

		return new ConversationDetailResponse
		{
			Id = response.Id,
			Participants = response.Participants,
			LastMessagePreview = response.LastMessagePreview,
			LastMessageAtUtc = response.LastMessageAtUtc,
			UnreadCount = response.UnreadCount,
			Messages = messages.Select(MapMessage).ToList()
		};
	}

	public async Task<MessageResponse> SendDirectAsync(SendDirectMessageRequest request)
	{
		var body = NormalizeBody(request.Body);
		var thread = string.IsNullOrWhiteSpace(request.ConversationId)
			? await GetOrCreateThreadAsync(request.Recipients)
			: await GetThreadForCurrentUserOrThrow(request.ConversationId);

		var message = CreateMessage(MessageKind.Direct, body);
		message.ConversationId = thread.Id;

		await _messages.CreateMessageAsync(message);
		UpdateThreadAfterMessage(thread, body);
		await _messages.UpdateThreadAsync(thread);

		var recipients = thread.Participants
			.Select(x => x.UserId)
			.Where(x => x != _currentUser.UserId);

		await _notifications.NotifyManyAsync(
			recipients,
			NotificationKind.DirectMessage,
			"Neue Direktnachricht",
			$"{_currentUser.DisplayName}: {TrimPreview(body)}",
			$"/messages?conversationId={thread.Id}");

		return MapMessage(message);
	}

	public async Task MarkConversationReadAsync(string conversationId)
	{
		var thread = await GetThreadForCurrentUserOrThrow(conversationId);
		var participant = thread.Participants.First(x => x.UserId == _currentUser.UserId);
		participant.LastReadAtUtc = DateTime.UtcNow;
		await _messages.UpdateThreadAsync(thread);
	}

	public async Task<List<MessageResponse>> GetGameMessagesAsync(string gameId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanReadGameMessagesAsync(game);
		var messages = await _messages.GetGameMessagesAsync(gameId);
		return messages.Select(MapMessage).ToList();
	}

	public async Task<MessageResponse> SendGameMessageAsync(
		string gameId,
		SendGameSessionMessageRequest request)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanWriteGameMessagesAsync(game);

		var body = NormalizeBody(request.Body);
		var message = CreateMessage(MessageKind.GameSession, body);
		message.GameId = gameId;

		await _messages.CreateMessageAsync(message);

		await _notifications.NotifyManyAsync(
			GetGameAudience(game).Where(x => x != _currentUser.UserId),
			NotificationKind.GameSessionMessage,
			$"Neue Nachricht: {game.Title}",
			$"{_currentUser.DisplayName}: {TrimPreview(body)}",
			$"/sessions/{game.Id}");

		return MapMessage(message);
	}

	public async Task<List<MessageResponse>> GetTableMessagesAsync(string gameId, string tableId)
	{
		var game = await GetGameOrThrow(gameId);
		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);
		await EnsureCanAccessTableMessagesAsync(game, table);
		var messages = await _messages.GetTableMessagesAsync(gameId, tableId);
		return messages.Select(MapMessage).ToList();
	}

	public async Task<MessageResponse> SendTableMessageAsync(
		string gameId,
		string tableId,
		SendGameTableMessageRequest request)
	{
		var game = await GetGameOrThrow(gameId);
		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);
		await EnsureCanAccessTableMessagesAsync(game, table);

		var body = NormalizeBody(request.Body);
		var message = CreateMessage(MessageKind.GameTable, body);
		message.GameId = gameId;
		message.TableId = tableId;

		await _messages.CreateMessageAsync(message);

		await _notifications.NotifyManyAsync(
			GetTableAudience(game, table).Where(x => x != _currentUser.UserId),
			NotificationKind.GameTableMessage,
			$"Neue Tischnachricht: {table.Name}",
			$"{_currentUser.DisplayName}: {TrimPreview(body)}",
			$"/sessions/{game.Id}");

		return MapMessage(message);
	}

	private async Task<MessageThread> GetOrCreateThreadAsync(List<MessageRecipientRequest> recipients)
	{
		var participants = recipients
			.Where(x => !string.IsNullOrWhiteSpace(x.UserId))
			.Select(x => new MessageParticipant
			{
				UserId = x.UserId,
				DisplayName = string.IsNullOrWhiteSpace(x.DisplayName) ? x.UserId : x.DisplayName
			})
			.ToList();

		if (participants.All(x => x.UserId != _currentUser.UserId))
		{
			participants.Add(new MessageParticipant
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName,
				LastReadAtUtc = DateTime.UtcNow
			});
		}

		participants = participants
			.GroupBy(x => x.UserId)
			.Select(x => x.First())
			.ToList();

		if (participants.Count < 2)
			throw new DomainException("Eine Direktnachricht braucht mindestens eine weitere Person.");

		var ids = participants.Select(x => x.UserId).ToList();
		var existing = await _messages.FindThreadByParticipantIdsAsync(ids);
		if (existing != null) return existing;

		var now = DateTime.UtcNow;
		var thread = new MessageThread
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Participants = participants,
			CreatedAtUtc = now,
			UpdatedAtUtc = now
		};

		await _messages.CreateThreadAsync(thread);
		return thread;
	}

	private async Task<MessageThread> GetThreadForCurrentUserOrThrow(string conversationId)
	{
		var thread = await _messages.GetThreadByIdAsync(conversationId)
			?? throw new DomainException("Conversation nicht gefunden.");

		if (thread.Participants.All(x => x.UserId != _currentUser.UserId))
			throw new UnauthorizedAccessException("Du bist nicht Teil dieser Conversation.");

		return thread;
	}

	private async Task<GameSession> GetGameOrThrow(string gameId)
	{
		return await _games.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
	}

	private async Task EnsureCanReadGameMessagesAsync(GameSession game)
	{
		if (!string.IsNullOrWhiteSpace(game.PublicSlug))
			return;

		await EnsureCanWriteGameMessagesAsync(game);
	}

	private async Task EnsureCanWriteGameMessagesAsync(GameSession game)
	{
		if (game.Host.UserId == _currentUser.UserId)
			return;

		if (GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			return;

		if (game.Invitations.Any(i => i.User.UserId == _currentUser.UserId))
			return;

		if (game.Tables.Any(t => t.Applications.Any(a =>
			a.Player.UserId == _currentUser.UserId &&
			(a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted))))
			return;

		if (await _authorization.CanManageSessionAsync(game))
			return;

		throw new UnauthorizedAccessException("Du darfst diese Session-Nachrichten nicht sehen.");
	}

	private async Task EnsureCanAccessTableMessagesAsync(GameSession game, GameTable table)
	{
		if (game.Host.UserId == _currentUser.UserId)
			return;

		if (table.AssignedPlayers.Any(p => p.UserId == _currentUser.UserId))
			return;

		if (table.Applications.Any(a =>
			a.Player.UserId == _currentUser.UserId &&
			(a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted)))
			return;

		if (await _authorization.CanManageSessionAsync(game))
			return;

		throw new UnauthorizedAccessException("Du darfst diese Tisch-Nachrichten nicht sehen.");
	}

	private async Task<ConversationResponse> MapConversationAsync(MessageThread thread)
	{
		var currentParticipant = thread.Participants.FirstOrDefault(x => x.UserId == _currentUser.UserId);
		var unread = await _messages.CountUnreadConversationMessagesAsync(
			thread.Id!,
			_currentUser.UserId,
			currentParticipant?.LastReadAtUtc);

		return new ConversationResponse
		{
			Id = thread.Id!,
			Participants = thread.Participants
				.Select(x => new ParticipantInfo { UserId = x.UserId, DisplayName = x.DisplayName })
				.ToList(),
			LastMessagePreview = thread.LastMessagePreview,
			LastMessageAtUtc = thread.LastMessageAtUtc,
			UnreadCount = unread
		};
	}

	private Message CreateMessage(MessageKind kind, string body)
	{
		return new Message
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Kind = kind,
			Author = new ParticipantInfo
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			},
			Body = body,
			CreatedAtUtc = DateTime.UtcNow
		};
	}

	private MessageResponse MapMessage(Message message)
	{
		return new MessageResponse
		{
			Id = message.Id!,
			Kind = message.Kind,
			ConversationId = message.ConversationId,
			GameId = message.GameId,
			TableId = message.TableId,
			Author = message.Author,
			Body = message.Body,
			CreatedAtUtc = message.CreatedAtUtc,
			IsMine = message.Author.UserId == _currentUser.UserId
		};
	}

	private static void UpdateThreadAfterMessage(MessageThread thread, string body)
	{
		var now = DateTime.UtcNow;
		thread.LastMessagePreview = TrimPreview(body);
		thread.LastMessageAtUtc = now;
		thread.UpdatedAtUtc = now;
	}

	private static string NormalizeBody(string? body)
	{
		var value = body?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(value))
			throw new DomainException("Nachricht darf nicht leer sein.");

		if (value.Length > MaxBodyLength)
			throw new DomainException($"Nachricht darf maximal {MaxBodyLength} Zeichen haben.");

		return value;
	}

	private static string TrimPreview(string body)
	{
		return body.Length <= 120 ? body : $"{body[..117]}...";
	}

	private static IEnumerable<string> GetGameAudience(GameSession game)
	{
		var users = new HashSet<string> { game.Host.UserId };

		foreach (var table in game.Tables)
		{
			foreach (var player in table.AssignedPlayers)
				users.Add(player.UserId);

			foreach (var application in table.Applications.Where(a =>
				a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted))
				users.Add(application.Player.UserId);
		}

		foreach (var invitation in game.Invitations)
			users.Add(invitation.User.UserId);

		return users;
	}

	private static IEnumerable<string> GetTableAudience(GameSession game, GameTable table)
	{
		var users = new HashSet<string> { game.Host.UserId };

		foreach (var player in table.AssignedPlayers)
			users.Add(player.UserId);

		foreach (var application in table.Applications.Where(a =>
			a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Accepted))
			users.Add(application.Player.UserId);

		return users;
	}
}
