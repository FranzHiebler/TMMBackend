using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;
using TMMBackend.Domain;
using TMMBackend.Dtos;
using TMMBackend.Services.Interfaces;

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

		var isMember = location.Members.Any(m => m.UserId == _currentUser.UserId);

		var allowed =
			location.AccessMode == LocationAccessMode.Open ||
			isMember;

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
				Id = Guid.NewGuid().ToString("N"),
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

	public async Task<bool> JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;
		if (game.Status != GameSessionState.Open) return false;
		if (game.JoinMode != GameJoinMode.FirstComeFirstServe) return false;

		if (IsUserAlreadyAssigned(game, _currentUser.UserId))
			return true;

		var table = game.Tables.FirstOrDefault(x => x.Id == tableId);
		if (table == null) return false;

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			return false;

		if (!SystemMatches(table.Systems, request.SystemKey))
			return false;

		table.AssignedPlayers.Add(new ParticipantInfo
		{
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName
		});

		UpdateSessionState(game);
		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return true;
	}

	public async Task<bool> ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;
		if (game.Status != GameSessionState.Open) return false;
		if (game.JoinMode != GameJoinMode.ApprovalRequired) return false;

		if (IsUserAlreadyAssigned(game, _currentUser.UserId))
			return false;

		GameTable? table = null;

		if (!string.IsNullOrWhiteSpace(request.TableId))
		{
			table = game.Tables.FirstOrDefault(x => x.Id == request.TableId);
			if (table == null) return false;

			if (!SystemMatches(table.Systems, request.SystemKey))
				return false;
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
			return true;

		table.Applications.Add(new TableApplication
		{
			Id = Guid.NewGuid().ToString("N"),
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
		return true;
	}

	public async Task<bool> AssignPlayerToTableAsync(string gameId, string tableId, AssignPlayerToTableRequest request)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;

		if (!await CanManageSession(game))
			return false;

		var table = game.Tables.FirstOrDefault(x => x.Id == tableId);
		if (table == null) return false;

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			return false;

		if (IsUserAlreadyAssigned(game, request.UserId))
			return true;

		table.AssignedPlayers.Add(new ParticipantInfo
		{
			UserId = request.UserId,
			DisplayName = request.DisplayName
		});

		if (!string.IsNullOrWhiteSpace(request.ApplicationId))
		{
			var app = game.Tables
				.SelectMany(t => t.Applications)
				.FirstOrDefault(a => a.Id == request.ApplicationId);

			if (app != null)
				app.Status = ApplicationStatus.Accepted;
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
				Id = t.Id,
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
					Id = a.Id,
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