using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;
using TabletopMatchMaker.Services.Validation;

namespace TabletopMatchMaker.Services;

public class GameService : IGameService
{
	private readonly IGameRepository _repository;
	private readonly ILocationLookupService _locationService;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;
	private readonly IGameAssignmentService _assignmentService;
	private readonly IGameProposalService _proposalService;
	private readonly INotificationService _notifications;

	public GameService(
		IGameRepository repository,
		ILocationLookupService locationService,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		IGameAssignmentService assignmentService,
		IGameProposalService proposalService,
		INotificationService notifications)
	{
		_repository = repository;
		_locationService = locationService;
		_currentUser = currentUser;
		_authorization = authorization;
		_assignmentService = assignmentService;
		_proposalService = proposalService;
		_notifications = notifications;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		GameValidator.ValidateCreate(request);

		var location = await _locationService.GetByIdAsync(request.LocationId);
		if (location == null)
			throw new DomainException("Location wurde nicht gefunden.");

		if (!_authorization.CanCreateGameAtLocation(location))
			throw new DomainException("Du darfst an dieser Location keine Game Session erstellen.");

		var gameSession = new GameSession
		{
			Title = request.Title,
			Host = new ParticipantInfo
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			},
			Status = GameSessionState.Open,
			JoinMode = request.JoinMode,
			LocationId = request.LocationId,
			LocationSnapshot = new LocationSnapshot
			{
				Name = location.Name,
				City = location.City
			},
			ClubId = request.ClubId,
			StartTimeUtc = request.StartTimeUtc == default ? DateTime.UtcNow.AddDays(30) : request.StartTimeUtc,
			TimingMode = request.TimingMode,
			TimeLabel = NormalizeOptional(request.TimeLabel),
			Description = request.Description,
			PublicSlug = Guid.NewGuid().ToString("N")[..10],
			Tables = request.Tables.Select(t => new GameTable
			{
				TableId = Guid.NewGuid().ToString("N"),
				Name = t.Name,
				MaxPlayers = t.MaxPlayers,
				Systems = t.Systems ?? new List<string>(),
				Scenario = t.Scenario,
				Points = t.Points,
				StartTimeUtc = t.StartTimeUtc,
				Notes = t.Notes
			}).ToList(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		await _repository.CreateAsync(gameSession);
		return GameMapper.ToResponse(gameSession);
	}

	public async Task<GameResponse?> GetByIdAsync(string id)
	{
		var gameSession = await _repository.GetByIdAsync(id);
		return gameSession == null ? null : GameMapper.ToResponse(gameSession);
	}

	public Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		return _assignmentService.JoinTableAsync(gameId, tableId, request);
	}

	public Task ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		return _assignmentService.ApplyAsync(gameId, request);
	}

	public Task AssignPlayerToTableAsync(
		string gameId,
		string tableId,
		AssignPlayerToTableRequest request)
	{
		return _assignmentService.AssignPlayerToTableAsync(gameId, tableId, request);
	}

	public async Task<List<GameResponse>> SearchAsync(SearchGamesRequest request)
	{
		GameValidator.ValidateSearch(request);

		var games = await _repository.SearchAsync(request);
		return games.Select(GameMapper.ToResponse).ToList();
	}

	public async Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request)
	{
		GameValidator.ValidateNearby(request);

		var nearbyLocations = await _locationService.FindNearbyAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters);

		if (nearbyLocations.Count == 0)
			return new List<GameResponse>();

		var locationIds = nearbyLocations
			.Select(x => x.LocationId)
			.ToList();

		var games = await _repository.SearchNearbyAsync(request, locationIds);

		var distanceByLocationId = nearbyLocations.ToDictionary(
			x => x.LocationId,
			x => x.DistanceInMeters);

		IEnumerable<GameSession> orderedGames =
			request.SortBy.Equals("date", StringComparison.OrdinalIgnoreCase)
				? games.OrderBy(g => g.StartTimeUtc)
				: games
					.OrderBy(g => distanceByLocationId.GetValueOrDefault(g.LocationId, double.MaxValue))
					.ThenBy(g => g.StartTimeUtc);

		if (request.SortDescending)
			orderedGames = orderedGames.Reverse();

		return orderedGames
			.Select(GameMapper.ToResponse)
			.ToList();
	}

	public Task RejectApplicationAsync(string gameId, string applicationId)
	{
		return _assignmentService.RejectApplicationAsync(gameId, applicationId);
	}

	public Task<GameResponse> CreateChangeProposalAsync(
		string gameId,
		CreateChangeProposalRequest request)
	{
		return _proposalService.CreateChangeProposalAsync(gameId, request);
	}

	public Task<GameResponse> AcceptChangeProposalAsync(string gameId, string proposalId)
	{
		return _proposalService.AcceptChangeProposalAsync(gameId, proposalId);
	}

	public Task<GameResponse> RejectChangeProposalAsync(string gameId, string proposalId)
	{
		return _proposalService.RejectChangeProposalAsync(gameId, proposalId);
	}

	public Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId)
	{
		return _assignmentService.RemovePlayerFromTableAsync(gameId, tableId, userId);
	}

	public Task MovePlayerToTableAsync(string gameId, string userId, MovePlayerToTableRequest request)
	{
		return _assignmentService.MovePlayerToTableAsync(gameId, userId, request);
	}

	public async Task<GameResponse> UpdateSessionAsync(string gameId, UpdateGameSessionRequest request)
	{
		GameValidator.ValidateUpdateSession(request);

		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");

		if (!await _authorization.CanManageSessionAsync(game))
			throw new DomainException("Du darfst diese Session nicht bearbeiten.");

		game.Title = request.Title.Trim();
		game.StartTimeUtc = request.StartTimeUtc;
		game.Description = string.IsNullOrWhiteSpace(request.Description)
			? null
			: request.Description.Trim();
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> UpdateTableAsync(
		string gameId,
		string tableId,
		UpdateGameTableRequest request)
	{
		GameValidator.ValidateUpdateTable(request);

		var game = await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");

		if (!await _authorization.CanManageSessionAsync(game))
			throw new DomainException("Du darfst diese Session nicht bearbeiten.");

		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);

		if (request.MaxPlayers < table.AssignedPlayers.Count)
			throw new DomainException("Maximale Spielerzahl darf nicht kleiner als die bereits zugewiesenen Spieler sein.");

		table.Name = request.Name.Trim();
		table.MaxPlayers = request.MaxPlayers;
		table.Systems = GameServiceHelpers.NormalizeSystems(request.Systems) ?? new List<string>();
		table.Scenario = string.IsNullOrWhiteSpace(request.Scenario) ? null : request.Scenario.Trim();
		table.Points = request.Points;
		table.StartTimeUtc = request.StartTimeUtc;
		table.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

		GameSessionRules.UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return GameMapper.ToResponse(game);
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

		game.Invitations.Add(new SessionInvitation
		{
			Id = Guid.NewGuid().ToString("N"),
			User = new ParticipantInfo
			{
				UserId = request.UserId,
				DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.UserId : request.DisplayName.Trim()
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

	public async Task<PublicGameResponse?> GetPublicAsync(string slugOrId)
	{
		var game = await _repository.GetByPublicSlugOrIdAsync(slugOrId);
		return game == null ? null : GameMapper.ToPublicResponse(game);
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
				game.Tables.Any(t => t.Applications.Any(a => a.Player.UserId == _currentUser.UserId && a.Status == ApplicationStatus.Pending)) ? "Bewerbung" :
				"Teilnahme",
			StartTimeUtc = game.TimingMode == SessionTimingMode.Open ? null : game.StartTimeUtc,
			TimingMode = game.TimingMode,
			TimeLabel = game.TimeLabel,
			LocationName = game.LocationSnapshot.Name,
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
