using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameService : IGameService
{
	private readonly IGameRepository _repository;
	private readonly LocationRepository _locationRepository;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;

	public GameService(
		IGameRepository repository,
		LocationRepository locationRepository,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization)
	{
		_repository = repository;
		_locationRepository = locationRepository;
		_currentUser = currentUser;
		_authorization = authorization;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		var location = await _locationRepository.GetByIdAsync(request.LocationId);

		if (location == null)
			throw new Exception("Location not found");

		if (!_authorization.CanCreateGameAtLocation(location))
			throw new Exception("Not allowed to create game at this location");

		if (request.Tables.Count == 0)
			throw new Exception("At least one table is required");

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
			StartTimeUtc = request.StartTimeUtc,
			Description = request.Description,
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

	public async Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status != GameSessionState.Open)
			throw new GameActionException("Diese Session ist nicht offen.");

		if (game.JoinMode != GameJoinMode.FirstComeFirstServe)
			throw new GameActionException("Direkter Beitritt ist für diese Session nicht aktiviert.");

		if (GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Du bist bereits in dieser Session angemeldet.");

		var table = GetTableOrThrow(game, tableId);

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			throw new GameActionException("Der Tisch ist voll.");

		if (!GameSessionRules.SystemMatches(table.Systems, request.SystemKey))
			throw new GameActionException("Das gewählte System passt nicht zu diesem Tisch.");

		table.AssignedPlayers.Add(CurrentParticipant());

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);
	}

	public async Task ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status != GameSessionState.Open)
			throw new GameActionException("Diese Session ist nicht offen.");

		if (game.JoinMode != GameJoinMode.ApprovalRequired)
			throw new GameActionException("Bewerbungen sind für diese Session nicht aktiviert.");

		if (GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Du bist bereits in dieser Session angemeldet.");

		var table = GetApplicationTargetTable(game, request);

		var alreadyApplied = game.Tables.Any(t =>
			t.Applications.Any(a =>
				a.Player.UserId == _currentUser.UserId &&
				a.Status == ApplicationStatus.Pending));

		if (alreadyApplied)
			return;

		table.Applications.Add(new TableApplication
		{
			ApplicationId = Guid.NewGuid().ToString("N"),
			TableId = table.TableId,
			Player = CurrentParticipant(),
			SystemKey = request.SystemKey,
			Message = request.Message,
			Status = ApplicationStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		await SaveAsync(game);
	}

	public async Task<bool> AssignPlayerToTableAsync(
		string gameId,
		string tableId,
		AssignPlayerToTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;

		if (!await _authorization.CanManageSessionAsync(game))
			return false;

		var table = game.Tables.FirstOrDefault(x => x.TableId == tableId);
		if (table == null) return false;

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			return false;

		var resolved = ResolvePlayerForAssignment(game, table, request);
		if (resolved == null) return false;

		var (player, application) = resolved.Value;

		if (GameSessionRules.IsUserAlreadyAssigned(game, player.UserId))
			return true;

		table.AssignedPlayers.Add(player);

		if (application != null)
			AcceptApplication(game, table, application, player.UserId);

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);

		return true;
	}

	public async Task<List<GameResponse>> SearchAsync(SearchGamesRequest request)
	{
		var games = await _repository.SearchAsync(request);
		return games.Select(GameMapper.ToResponse).ToList();
	}

	public async Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request)
	{
		var nearbyLocations = await _locationRepository.FindNearbyAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters);

		var locationIds = nearbyLocations.Select(x => x.LocationId).ToList();

		var games = await _repository.SearchNearbyAsync(request, locationIds);
		return games.Select(GameMapper.ToResponse).ToList();
	}

	public async Task RejectApplicationAsync(string gameId, string applicationId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var application = game.Tables
			.SelectMany(t => t.Applications)
			.FirstOrDefault(a => a.ApplicationId == applicationId);

		if (application == null)
			throw new GameActionException("Bewerbung nicht gefunden.");

		if (application.Status != ApplicationStatus.Pending)
			throw new GameActionException("Diese Bewerbung wurde bereits bearbeitet.");

		application.Status = ApplicationStatus.Rejected;
		await SaveAsync(game);
	}

	public async Task<GameResponse> CreateChangeProposalAsync(string gameId, CreateChangeProposalRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status is GameSessionState.Cancelled or GameSessionState.Closed)
			throw new GameActionException("Diese Session kann nicht mehr geändert werden.");

		if (!GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Nur angemeldete Spieler dürfen Änderungen vorschlagen.");

		var table = ResolveProposalTable(game, request);
		var proposedSystems = NormalizeSystems(request.ProposedSystems);

		var hasTimeChange = request.ProposedStartTimeUtc.HasValue;
		var hasSystemChange = proposedSystems is { Count: > 0 };
		var hasPointsChange = request.ProposedPoints.HasValue;

		if (!hasTimeChange && !hasSystemChange && !hasPointsChange)
			throw new GameActionException("Bitte mindestens Uhrzeit, System oder Punkte vorschlagen.");

		if ((hasSystemChange || hasPointsChange) && table == null)
			throw new GameActionException("System- oder Punkteänderungen brauchen einen Tisch.");

		if (request.ProposedPoints is < 0)
			throw new GameActionException("Punkte dürfen nicht negativ sein.");

		game.ChangeProposals.Add(new GameChangeProposal
		{
			ProposalId = Guid.NewGuid().ToString("N"),
			TableId = table?.TableId,
			ProposedBy = CurrentParticipant(),
			ProposedStartTimeUtc = request.ProposedStartTimeUtc,
			ProposedSystems = hasSystemChange ? proposedSystems : null,
			ProposedPoints = request.ProposedPoints,
			Message = request.Message,
			Status = ChangeProposalStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		await SaveAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> AcceptChangeProposalAsync(string gameId, string proposalId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var proposal = GameSessionRules.GetPendingChangeProposal(game, proposalId);
		var table = ResolveExistingProposalTable(game, proposal);

		ApplyProposal(game, table, proposal);

		proposal.Status = ChangeProposalStatus.Accepted;
		proposal.ResolvedAt = DateTime.UtcNow;

		await SaveAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task<GameResponse> RejectChangeProposalAsync(string gameId, string proposalId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var proposal = GameSessionRules.GetPendingChangeProposal(game, proposalId);
		proposal.Status = ChangeProposalStatus.Rejected;
		proposal.ResolvedAt = DateTime.UtcNow;

		await SaveAsync(game);
		return GameMapper.ToResponse(game);
	}

	public async Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var table = GetTableOrThrow(game, tableId);
		var player = table.AssignedPlayers.FirstOrDefault(p => p.UserId == userId);

		if (player == null)
			throw new GameActionException("Spieler ist nicht an diesem Tisch.");

		table.AssignedPlayers.Remove(player);
		RestoreApplicationAfterRemoval(game, table, player);

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);
	}

	public async Task MovePlayerToTableAsync(string gameId, string userId, MovePlayerToTableRequest request)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		if (string.IsNullOrWhiteSpace(request.TargetTableId))
			throw new GameActionException("Zieltisch fehlt.");

		var sourceTable = game.Tables.FirstOrDefault(t =>
			t.AssignedPlayers.Any(p => p.UserId == userId));

		if (sourceTable == null)
			throw new GameActionException("Spieler ist keinem Tisch zugewiesen.");

		var targetTable = GetTableOrThrow(game, request.TargetTableId);

		if (sourceTable.TableId == targetTable.TableId)
			return;

		if (targetTable.AssignedPlayers.Count >= targetTable.MaxPlayers)
			throw new GameActionException("Der Zieltisch ist voll.");

		var player = sourceTable.AssignedPlayers.First(p => p.UserId == userId);

		sourceTable.AssignedPlayers.Remove(player);
		targetTable.AssignedPlayers.Add(player);

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);
	}

	private async Task<GameSession> GetGameOrThrow(string gameId)
	{
		return await _repository.GetByIdAsync(gameId)
			?? throw new GameActionException("Session nicht gefunden.");
	}

	private static GameTable GetTableOrThrow(GameSession game, string tableId)
	{
		return game.Tables.FirstOrDefault(x => x.TableId == tableId)
			?? throw new GameActionException("Tisch nicht gefunden.");
	}

	private async Task EnsureCanManageAsync(GameSession game)
	{
		if (!await _authorization.CanManageSessionAsync(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");
	}

	private ParticipantInfo CurrentParticipant()
	{
		return new ParticipantInfo
		{
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName
		};
	}

	private async Task SaveAsync(GameSession game)
	{
		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);
	}

	private static GameTable GetApplicationTargetTable(GameSession game, ApplyToGameRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.TableId))
			return game.Tables.First();

		var table = GetTableOrThrow(game, request.TableId);

		if (!GameSessionRules.SystemMatches(table.Systems, request.SystemKey))
			throw new GameActionException("Das gewählte System passt nicht zu diesem Tisch.");

		return table;
	}

	private static (ParticipantInfo player, TableApplication? application)? ResolvePlayerForAssignment(
		GameSession game,
		GameTable table,
		AssignPlayerToTableRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.ApplicationId))
		{
			var application = game.Tables
				.SelectMany(t => t.Applications)
				.FirstOrDefault(a => a.ApplicationId == request.ApplicationId);

			if (application == null) return null;
			if (application.Status != ApplicationStatus.Pending) return null;
			if (!GameSessionRules.SystemMatches(table.Systems, application.SystemKey)) return null;

			return (application.Player, application);
		}

		if (string.IsNullOrWhiteSpace(request.UserId) ||
			string.IsNullOrWhiteSpace(request.DisplayName))
			return null;

		return (new ParticipantInfo
		{
			UserId = request.UserId,
			DisplayName = request.DisplayName
		}, null);
	}

	private static void AcceptApplication(
		GameSession game,
		GameTable table,
		TableApplication application,
		string userId)
	{
		application.Status = ApplicationStatus.Accepted;
		application.TableId = table.TableId;

		foreach (var otherApplication in game.Tables
			.SelectMany(t => t.Applications)
			.Where(a =>
				a.Player.UserId == userId &&
				a.ApplicationId != application.ApplicationId &&
				a.Status == ApplicationStatus.Pending))
		{
			otherApplication.Status = ApplicationStatus.Withdrawn;
		}
	}

	private static GameTable? ResolveProposalTable(GameSession game, CreateChangeProposalRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.TableId))
			return null;

		return game.Tables.FirstOrDefault(t => t.TableId == request.TableId)
			?? throw new GameActionException("Tisch nicht gefunden.");
	}

	private static GameTable? ResolveExistingProposalTable(GameSession game, GameChangeProposal proposal)
	{
		if (string.IsNullOrWhiteSpace(proposal.TableId))
			return null;

		return game.Tables.FirstOrDefault(t => t.TableId == proposal.TableId)
			?? throw new GameActionException("Tisch nicht gefunden.");
	}

	private static void ApplyProposal(GameSession game, GameTable? table, GameChangeProposal proposal)
	{
		if (proposal.ProposedStartTimeUtc.HasValue)
		{
			if (table != null)
				table.StartTimeUtc = proposal.ProposedStartTimeUtc.Value;
			else
				game.StartTimeUtc = proposal.ProposedStartTimeUtc.Value;
		}

		if (proposal.ProposedSystems is { Count: > 0 })
		{
			if (table == null) throw new GameActionException("Tisch nicht gefunden.");
			table.Systems = proposal.ProposedSystems;
		}

		if (proposal.ProposedPoints.HasValue)
		{
			if (table == null) throw new GameActionException("Tisch nicht gefunden.");
			table.Points = proposal.ProposedPoints;
		}
	}

	private static List<string>? NormalizeSystems(IEnumerable<string>? systems)
	{
		return systems?
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	private static void RestoreApplicationAfterRemoval(
		GameSession game,
		GameTable table,
		ParticipantInfo player)
	{
		var existingApplicationTable = game.Tables.FirstOrDefault(t =>
			t.Applications.Any(a => a.Player.UserId == player.UserId));

		var existingApplication = existingApplicationTable?
			.Applications
			.FirstOrDefault(a => a.Player.UserId == player.UserId);

		if (existingApplication != null)
		{
			existingApplication.Status = ApplicationStatus.Pending;
			existingApplication.TableId = table.TableId;
			existingApplication.Message = null;
			existingApplication.SystemKey = table.Systems.Count == 1 ? table.Systems[0] : null;
			existingApplication.CreatedAt = DateTime.UtcNow;

			if (existingApplicationTable != null && existingApplicationTable.TableId != table.TableId)
			{
				existingApplicationTable.Applications.Remove(existingApplication);
				table.Applications.Add(existingApplication);
			}

			return;
		}

		table.Applications.Add(new TableApplication
		{
			ApplicationId = Guid.NewGuid().ToString("N"),
			TableId = table.TableId,
			Player = player,
			SystemKey = table.Systems.Count == 1 ? table.Systems[0] : null,
			Message = null,
			Status = ApplicationStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});
	}
}