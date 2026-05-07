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
			}).ToList()
		};
	}
}
