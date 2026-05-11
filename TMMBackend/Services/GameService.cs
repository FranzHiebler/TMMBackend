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

	public GameService(
		IGameRepository repository,
		LocationRepository locationRepository,
		ICurrentUserService currentUser)
	{
		_repository = repository;
		_locationRepository = locationRepository;
		_currentUser = currentUser;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		var location = await _locationRepository.GetByIdAsync(request.LocationId);

		if (location == null)
			throw new Exception("Location not found");

		var hasUsableRole = location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			m.Role != LocationRole.Applicant);

		var allowed =
			location.AccessMode == LocationAccessMode.Open ||
			hasUsableRole;

		if (!allowed)
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
		return Map(gameSession);
	}

	public async Task<GameResponse?> GetByIdAsync(string id)
	{
		var gameSession = await _repository.GetByIdAsync(id);
		return gameSession == null ? null : Map(gameSession);
	}


	public async Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");
		if (game.Status != GameSessionState.Open) throw new GameActionException("Diese Session ist nicht offen.");
		if (game.JoinMode != GameJoinMode.FirstComeFirstServe) throw new GameActionException("Direkter Beitritt ist für diese Session nicht aktiviert.");

		if (IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Du bist bereits in dieser Session angemeldet.");

		var table = game.Tables.FirstOrDefault(x => x.TableId == tableId);
		if (table == null) throw new GameActionException("Tisch nicht gefunden.");

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			throw new GameActionException("Der Tisch ist voll.");

		if (!SystemMatches(table.Systems, request.SystemKey))
			throw new GameActionException("Das gewählte System passt nicht zu diesem Tisch.");

		table.AssignedPlayers.Add(new ParticipantInfo
		{
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName
		});

		UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
	}

	public async Task ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");
		if (game.Status != GameSessionState.Open) throw new GameActionException("Diese Session ist nicht offen.");
		if (game.JoinMode != GameJoinMode.ApprovalRequired) throw new GameActionException("Bewerbungen sind für diese Session nicht aktiviert.")	;

		if (IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Du bist bereits in dieser Session angemeldet.");

		GameTable? table = null;

		if (!string.IsNullOrWhiteSpace(request.TableId))
		{
			table = game.Tables.FirstOrDefault(x => x.TableId == request.TableId);
			if (table == null) throw new GameActionException("Tisch nicht gefunden.");

			if (!SystemMatches(table.Systems, request.SystemKey))
				throw new GameActionException("Das gewählte System passt nicht zu diesem Tisch.");
		}
		else
		{
			table = game.Tables.First();
		}

		var alreadyApplied = game.Tables.Any(t =>
			t.Applications.Any(a =>
				a.Player.UserId == _currentUser.UserId &&
				a.Status == ApplicationStatus.Pending));

		if (alreadyApplied)
			return;

		table.Applications.Add(new TableApplication
		{
			ApplicationId = Guid.NewGuid().ToString("N"),
			TableId = request.TableId,
			Player = new ParticipantInfo
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			},
			SystemKey = request.SystemKey,
			Message = request.Message,
			Status = ApplicationStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
	}

	public async Task<bool> AssignPlayerToTableAsync(
		string gameId,
		string tableId,
		AssignPlayerToTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;

		if (!await CanManageSession(game))
			return false;

		var table = game.Tables.FirstOrDefault(x => x.TableId == tableId);
		if (table == null) return false;

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			return false;

		TableApplication? application = null;
		ParticipantInfo player;

		if (!string.IsNullOrWhiteSpace(request.ApplicationId))
		{
			application = game.Tables
				.SelectMany(t => t.Applications)
				.FirstOrDefault(a => a.ApplicationId == request.ApplicationId);

			if (application == null) return false;
			if (application.Status != ApplicationStatus.Pending) return false;
			if (!SystemMatches(table.Systems, application.SystemKey)) return false;

			player = application.Player;
		}
		else
		{
			if (string.IsNullOrWhiteSpace(request.UserId) ||
				string.IsNullOrWhiteSpace(request.DisplayName))
				return false;

			player = new ParticipantInfo
			{
				UserId = request.UserId,
				DisplayName = request.DisplayName
			};
		}

		if (IsUserAlreadyAssigned(game, player.UserId))
			return true;

		table.AssignedPlayers.Add(player);

		if (application != null)
		{
			application.Status = ApplicationStatus.Accepted;
			application.TableId = table.TableId;

			foreach (var otherApplication in game.Tables
				.SelectMany(t => t.Applications)
				.Where(a =>
					a.Player.UserId == player.UserId &&
					a.ApplicationId != application.ApplicationId &&
					a.Status == ApplicationStatus.Pending))
			{
				otherApplication.Status = ApplicationStatus.Withdrawn;
			}
		}

		UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return true;
	}
	public async Task<List<GameResponse>> SearchAsync(SearchGamesRequest request)
	{
		var games = await _repository.SearchAsync(request);
		return games.Select(Map).ToList();
	}

	public async Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request)
	{
		var nearbyLocations = await _locationRepository.FindNearbyAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters);

		var locationIds = nearbyLocations.Select(x => x.LocationId).ToList();

		var games = await _repository.SearchNearbyAsync(request, locationIds);
		return games.Select(Map).ToList();
	}

	private async Task<bool> CanManageSession(GameSession game)
	{
		if (game.Host.UserId == _currentUser.UserId)
			return true;

		var location = await _locationRepository.GetByIdAsync(game.LocationId);
		if (location == null) return false;

		return location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			(m.Role == LocationRole.Owner || m.Role == LocationRole.Manager));
	}

	private static bool IsUserAlreadyAssigned(GameSession game, string userId)
	{
		return game.Tables.Any(t => t.AssignedPlayers.Any(p => p.UserId == userId));
	}

	private static bool SystemMatches(List<string> systems, string? systemKey)
	{
		if (systems.Count == 0)
			return true;

		if (systems.Contains("egal", StringComparer.OrdinalIgnoreCase))
			return true;

		if (string.IsNullOrWhiteSpace(systemKey))
			return false;

		return systems.Contains(systemKey, StringComparer.OrdinalIgnoreCase);
	}

	private static void UpdateSessionState(GameSession game)
	{
		var hasOpenSlot = game.Tables.Any(t => t.AssignedPlayers.Count < t.MaxPlayers);
		game.Status = hasOpenSlot ? GameSessionState.Open : GameSessionState.Full;
	}
	public async Task RejectApplicationAsync(string gameId, string applicationId)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");

		if (!await CanManageSession(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");

		var application = game.Tables
			.SelectMany(t => t.Applications)
			.FirstOrDefault(a => a.ApplicationId == applicationId);

		if (application == null)
			throw new GameActionException("Bewerbung nicht gefunden.");

		if (application.Status != ApplicationStatus.Pending)
			throw new GameActionException("Diese Bewerbung wurde bereits bearbeitet.");

		application.Status = ApplicationStatus.Rejected;
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
	}

	public async Task<GameResponse> CreateChangeProposalAsync(string gameId, CreateChangeProposalRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");
		if (game.Status is GameSessionState.Cancelled or GameSessionState.Closed)
			throw new GameActionException("Diese Session kann nicht mehr geändert werden.");

		if (!IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new GameActionException("Nur angemeldete Spieler dürfen Änderungen vorschlagen.");

		GameTable? table = null;
		if (!string.IsNullOrWhiteSpace(request.TableId))
		{
			table = game.Tables.FirstOrDefault(t => t.TableId == request.TableId);
			if (table == null) throw new GameActionException("Tisch nicht gefunden.");
		}

		var proposedSystems = request.ProposedSystems?
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

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
			ProposedBy = new ParticipantInfo
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			},
			ProposedStartTimeUtc = request.ProposedStartTimeUtc,
			ProposedSystems = hasSystemChange ? proposedSystems : null,
			ProposedPoints = request.ProposedPoints,
			Message = request.Message,
			Status = ChangeProposalStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		game.UpdatedAt = DateTime.UtcNow;
		await _repository.UpdateAsync(game);
		return Map(game);
	}

	public async Task<GameResponse> AcceptChangeProposalAsync(string gameId, string proposalId)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");

		if (!await CanManageSession(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");

		var proposal = GetPendingChangeProposal(game, proposalId);
		GameTable? table = null;

		if (!string.IsNullOrWhiteSpace(proposal.TableId))
		{
			table = game.Tables.FirstOrDefault(t => t.TableId == proposal.TableId);
			if (table == null) throw new GameActionException("Tisch nicht gefunden.");
		}

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

		proposal.Status = ChangeProposalStatus.Accepted;
		proposal.ResolvedAt = DateTime.UtcNow;
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return Map(game);
	}

	public async Task<GameResponse> RejectChangeProposalAsync(string gameId, string proposalId)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");

		if (!await CanManageSession(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");

		var proposal = GetPendingChangeProposal(game, proposalId);
		proposal.Status = ChangeProposalStatus.Rejected;
		proposal.ResolvedAt = DateTime.UtcNow;
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return Map(game);
	}

	private static GameChangeProposal GetPendingChangeProposal(GameSession game, string proposalId)
	{
		var proposal = game.ChangeProposals.FirstOrDefault(p => p.ProposalId == proposalId);
		if (proposal == null) throw new GameActionException("Änderungsvorschlag nicht gefunden.");
		if (proposal.Status != ChangeProposalStatus.Pending)
			throw new GameActionException("Dieser Änderungsvorschlag wurde bereits bearbeitet.");
		return proposal;
	}

	private static GameResponse Map(GameSession game)
	{
		return new GameResponse
		{
			Id = game.Id!,
			Title = game.Title,
			Host = new ParticipantDto
			{
				UserId = game.Host.UserId,
				DisplayName = game.Host.DisplayName
			},
			Status = game.Status,
			JoinMode = game.JoinMode,
			LocationId = game.LocationId,
			Location = new LocationSnapshotDto
			{
				Name = game.LocationSnapshot.Name,
				City = game.LocationSnapshot.City
			},
			ClubId = game.ClubId,
			StartTimeUtc = game.StartTimeUtc,
			Description = game.Description,
			Tables = game.Tables.Select(t => new GameTableDto
			{
				Id = t.TableId,
				Name = t.Name,
				MaxPlayers = t.MaxPlayers,
				Systems = t.Systems,
				Scenario = t.Scenario,
				Points = t.Points,
				StartTimeUtc = t.StartTimeUtc,
				Notes = t.Notes,
				AssignedPlayers = t.AssignedPlayers.Select(p => new ParticipantDto
				{
					UserId = p.UserId,
					DisplayName = p.DisplayName
				}).ToList(),
				Applications = t.Applications.Select(a => new TableApplicationDto
				{
					Id = a.ApplicationId,
					TableId = a.TableId,
					Player = new ParticipantDto
					{
						UserId = a.Player.UserId,
						DisplayName = a.Player.DisplayName
					},
					SystemKey = a.SystemKey,
					Message = a.Message,
					Status = a.Status,
					CreatedAt = a.CreatedAt
				}).ToList()
			}).ToList(),
			ChangeProposals = game.ChangeProposals.Select(p => new GameChangeProposalDto
			{
				Id = p.ProposalId,
				TableId = p.TableId,
				ProposedBy = new ParticipantDto
				{
					UserId = p.ProposedBy.UserId,
					DisplayName = p.ProposedBy.DisplayName
				},
				ProposedStartTimeUtc = p.ProposedStartTimeUtc,
				ProposedSystems = p.ProposedSystems,
				ProposedPoints = p.ProposedPoints,
				Message = p.Message,
				Status = p.Status,
				CreatedAt = p.CreatedAt,
				ResolvedAt = p.ResolvedAt
			}).ToList()
		};
	}

	public async Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");

		if (!await CanManageSession(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");

		var table = game.Tables.FirstOrDefault(t => t.TableId == tableId);
		if (table == null) throw new GameActionException("Tisch nicht gefunden.");

		var player = table.AssignedPlayers.FirstOrDefault(p => p.UserId == userId);
		if (player == null) throw new GameActionException("Spieler ist nicht an diesem Tisch.");

		table.AssignedPlayers.Remove(player);

		var existingApplication = game.Tables
			.SelectMany(t => t.Applications)
			.FirstOrDefault(a => a.Player.UserId == userId);

		if (existingApplication != null)
		{
			existingApplication.Status = ApplicationStatus.Pending;
			existingApplication.TableId = table.TableId;
		}
		else
		{
			table.Applications.Add(new TableApplication
			{
				ApplicationId = Guid.NewGuid().ToString("N"),
				TableId = table.TableId,
				Player = player,
				SystemKey = table.Systems.Count == 1 ? table.Systems[0] : null,
				Message = "Vom Tisch entfernt",
				Status = ApplicationStatus.Pending,
				CreatedAt = DateTime.UtcNow
			});
		}

		UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
	}

	public async Task MovePlayerToTableAsync(string gameId, string userId, MovePlayerToTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) throw new GameActionException("Session nicht gefunden.");

		if (!await CanManageSession(game))
			throw new GameActionException("Du darfst diese Session nicht verwalten.");

		if (string.IsNullOrWhiteSpace(request.TargetTableId))
			throw new GameActionException("Zieltisch fehlt.");

		var sourceTable = game.Tables.FirstOrDefault(t =>
			t.AssignedPlayers.Any(p => p.UserId == userId));

		if (sourceTable == null)
			throw new GameActionException("Spieler ist keinem Tisch zugewiesen.");

		var targetTable = game.Tables.FirstOrDefault(t => t.TableId == request.TargetTableId);
		if (targetTable == null) throw new GameActionException("Zieltisch nicht gefunden.");

		if (sourceTable.TableId == targetTable.TableId)
			return;

		if (targetTable.AssignedPlayers.Count >= targetTable.MaxPlayers)
			throw new GameActionException("Der Zieltisch ist voll.");

		var player = sourceTable.AssignedPlayers.First(p => p.UserId == userId);

		sourceTable.AssignedPlayers.Remove(player);
		targetTable.AssignedPlayers.Add(player);

		UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
	}
}
