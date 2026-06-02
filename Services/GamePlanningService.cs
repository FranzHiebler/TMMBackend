using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GamePlanningService : IGamePlanningService
{
	private readonly IGameRepository _repository;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;
	private readonly INotificationService _notifications;
	private readonly IFriendRepository _friends;

	public GamePlanningService(
		IGameRepository repository,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		INotificationService notifications,
		IFriendRepository friends)
	{
		_repository = repository;
		_currentUser = currentUser;
		_authorization = authorization;
		_notifications = notifications;
		_friends = friends;
	}

	public async Task<GameResponse> AddDateOptionAsync(string gameId, AddDateOptionRequest request)
	{
		var game = await GetManagedGameAsync(gameId);
		if (request.StartTimeUtc == default)
			throw new DomainException("Terminvorschlag braucht eine Startzeit.");

		game.DateOptions.Add(new SessionDateOption
		{
			Id = Guid.NewGuid().ToString("N"),
			StartTimeUtc = request.StartTimeUtc,
			Label = NormalizeOptional(request.Label),
			CreatedAtUtc = DateTime.UtcNow
		});
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyManyAsync(
			GetSessionAudience(game).Where(id => id != _currentUser.UserId),
			NotificationKind.DateOptionAdded,
			"Neuer Terminvorschlag",
			$"Für {game.Title} wurde ein Termin vorgeschlagen.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> VoteDateOptionAsync(string gameId, string optionId)
	{
		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
		var option = game.DateOptions.FirstOrDefault(x => x.Id == optionId)
			?? throw new DomainException("Terminvorschlag nicht gefunden.");

		option.Votes.RemoveAll(x => x.UserId == _currentUser.UserId);
		option.Votes.Add(new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName });
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> SelectDateOptionAsync(string gameId, string optionId)
	{
		var game = await GetManagedGameAsync(gameId);
		var option = game.DateOptions.FirstOrDefault(x => x.Id == optionId)
			?? throw new DomainException("Terminvorschlag nicht gefunden.");

		game.StartTimeUtc = option.StartTimeUtc;
		game.TimingMode = SessionTimingMode.Fixed;
		game.TimeLabel = option.Label;
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyManyAsync(
			GetSessionAudience(game).Where(id => id != _currentUser.UserId),
			NotificationKind.DateOptionSelected,
			"Termin übernommen",
			$"Für {game.Title} wurde ein Termin übernommen.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> InviteFriendAsync(string gameId, InviteFriendToSessionRequest request)
	{
		var game = await GetManagedGameAsync(gameId);
		if (string.IsNullOrWhiteSpace(request.UserId))
			throw new DomainException("UserId ist erforderlich.");
		if (request.UserId == _currentUser.UserId)
			throw new DomainException("Du kannst dich nicht selbst einladen.");
		if (game.Invitations.Any(x => x.User.UserId == request.UserId && x.Status == SessionInvitationStatus.Pending))
			throw new DomainException("Diese Einladung ist bereits offen.");

		var friendship = await _friends.FindBetweenUsersAsync(_currentUser.UserId, request.UserId);
		if (friendship?.Status != FriendshipStatus.Accepted)
			throw new DomainException("Du kannst nur Freunde zu einer Session einladen.");

		var friendDisplayName = friendship.RequesterUserId == request.UserId
			? friendship.RequesterDisplayName
			: friendship.ReceiverDisplayName;

		game.Invitations.Add(new SessionInvitation
		{
			Id = Guid.NewGuid().ToString("N"),
			User = new ParticipantInfo
			{
				UserId = request.UserId,
				DisplayName = string.IsNullOrWhiteSpace(friendDisplayName) ? request.UserId : friendDisplayName.Trim()
			},
			Status = SessionInvitationStatus.Pending,
			CreatedAtUtc = DateTime.UtcNow
		});
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyAsync(
			request.UserId,
			NotificationKind.SessionInvitation,
			"Session-Einladung",
			$"{_currentUser.DisplayName} hat dich zu {game.Title} eingeladen.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> RespondInvitationAsync(string gameId, string invitationId, bool accept)
	{
		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
		var invitation = game.Invitations.FirstOrDefault(x => x.Id == invitationId)
			?? throw new DomainException("Einladung nicht gefunden.");
		if (invitation.User.UserId != _currentUser.UserId)
			throw new DomainException("Du darfst diese Einladung nicht beantworten.");

		invitation.Status = accept ? SessionInvitationStatus.Accepted : SessionInvitationStatus.Rejected;
		invitation.RespondedAtUtc = DateTime.UtcNow;
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyAsync(
			game.Host.UserId,
			accept ? NotificationKind.SessionInvitationAccepted : NotificationKind.SessionInvitationRejected,
			accept ? "Einladung angenommen" : "Einladung abgelehnt",
			$"{_currentUser.DisplayName} hat die Einladung zu {game.Title} {(accept ? "angenommen" : "abgelehnt")}.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> JoinWaitlistAsync(string gameId, JoinWaitlistRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
		if (game.Waitlist.Any(x => x.Player.UserId == _currentUser.UserId && x.TableId == request.TableId))
			throw new DomainException("Du stehst bereits auf der Warteliste.");

		game.Waitlist.Add(new WaitlistEntry
		{
			Id = Guid.NewGuid().ToString("N"),
			TableId = NormalizeOptional(request.TableId),
			Player = new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName },
			SystemKey = NormalizeOptional(request.SystemKey),
			Message = NormalizeOptional(request.Message),
			CreatedAtUtc = DateTime.UtcNow
		});
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyAsync(
			game.Host.UserId,
			NotificationKind.WaitlistJoined,
			"Neue Warteliste",
			$"{_currentUser.DisplayName} steht auf der Warteliste für {game.Title}.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> PromoteWaitlistEntryAsync(string gameId, string waitlistEntryId, string tableId)
	{
		var game = await GetManagedGameAsync(gameId);
		var entry = game.Waitlist.FirstOrDefault(x => x.Id == waitlistEntryId)
			?? throw new DomainException("Wartelisten-Eintrag nicht gefunden.");
		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);
		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			throw new DomainException("Der Tisch ist voll.");

		table.AssignedPlayers.Add(entry.Player);
		game.Waitlist.Remove(entry);
		GameSessionRules.UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyAsync(
			entry.Player.UserId,
			NotificationKind.WaitlistPromoted,
			"Von Warteliste nachgerückt",
			$"Du wurdest bei {game.Title} einem Tisch zugewiesen.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> CloseGameAsync(string gameId, CloseGameRequest request)
	{
		var game = await GetManagedGameAsync(gameId);
		if (string.IsNullOrWhiteSpace(request.Value))
			throw new DomainException("Ergebnis ist erforderlich.");

		game.Status = GameSessionState.Closed;
		game.Result = new GameResult
		{
			Kind = request.Kind,
			Value = request.Value.Trim(),
			Notes = NormalizeOptional(request.Notes),
			RecordedBy = new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName },
			RecordedAtUtc = DateTime.UtcNow
		};
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> CancelGameAsync(string gameId)
	{
		var game = await GetManagedGameAsync(gameId);
		if (game.Status == GameSessionState.Cancelled)
			return GameMapper.ToResponse(game);

		game.Status = GameSessionState.Cancelled;
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);

		await _notifications.NotifyManyAsync(
			GetSessionAudience(game).Where(id => id != _currentUser.UserId).Distinct(),
			NotificationKind.SessionClosed,
			"Spiel abgesagt",
			$"{game.Title} wurde abgesagt.",
			$"/sessions/{game.Id}");

		return GameMapper.ToResponse(game);
	}

	public async Task<List<CalendarItemResponse>> GetCalendarAsync()
	{
		var games = await _repository.GetRelevantForUserAsync(_currentUser.UserId);
		return games.Select(game => new CalendarItemResponse
		{
			Id = game.Id!,
			Title = game.Title,
			Kind = game.Host.UserId == _currentUser.UserId ? "Host" :
				game.Invitations.Any(x => x.User.UserId == _currentUser.UserId && x.Status == SessionInvitationStatus.Pending) ? "Einladung" :
				game.Waitlist.Any(x => x.Player.UserId == _currentUser.UserId) ? "Warteliste" :
				game.Tables.Any(t => t.Applications.Any(a => a.Player.UserId == _currentUser.UserId && a.Status == ApplicationStatus.Pending)) ? "Bewerbung" :
				game.Tables.Any(t => t.AssignedPlayers.Any(p => p.UserId == _currentUser.UserId)) ? "Teilnahme" :
				game.Invitations.Any(x => x.User.UserId == _currentUser.UserId && x.Status == SessionInvitationStatus.Accepted) ? "Einladung" :
				"Teilnahme",
			StartTimeUtc = game.TimingMode == SessionTimingMode.Open ? null : game.StartTimeUtc,
			TimingMode = game.TimingMode,
			TimeLabel = game.TimeLabel,
			LocationName = game.LocationSnapshot.Name,
			LocationCity = game.LocationSnapshot.City,
			Status = game.Status.ToString()
		}).OrderBy(x => x.StartTimeUtc ?? DateTime.MaxValue).ToList();
	}

	private async Task<GameSession> GetManagedGameAsync(string gameId)
	{
		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
		if (!await _authorization.CanManageSessionAsync(game))
			throw new DomainException("Du darfst diese Session nicht bearbeiten.");
		return game;
	}

	private static IEnumerable<string> GetSessionAudience(GameSession game)
	{
		yield return game.Host.UserId;
		foreach (var player in game.Tables.SelectMany(t => t.AssignedPlayers)) yield return player.UserId;
		foreach (var invitation in game.Invitations) yield return invitation.User.UserId;
		foreach (var entry in game.Waitlist) yield return entry.Player.UserId;
	}

	private static string? NormalizeOptional(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}
}
