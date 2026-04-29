using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameService : IGameService
{
	private readonly IGameRepository _repository;

	public GameService(IGameRepository repository)
	{
		_repository = repository;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		var gameSession = new GameSession
		{
			Title = request.Title,
			System = new GameSystemInfo
			{
				Key = request.SystemKey,
				Name = request.SystemName
			},
			Host = new ParticipantInfo
			{
				UserId = request.HostUserId,
				DisplayName = request.HostDisplayName
			},
			Participants = new List<ParticipantInfo>
		{
			new ParticipantInfo
			{
				UserId = request.HostUserId,
				DisplayName = request.HostDisplayName
			}
		},
			MaxPlayers = request.MaxPlayers,
			Status = "Open",
			LocationId = request.LocationId,
			LocationSnapshot = new LocationSnapshot
			{
				Name = request.LocationName,
				City = request.LocationCity
			},
			ClubId = request.ClubId,
			StartTimeUtc = request.StartTimeUtc,
			Description = request.Description,
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

	public async Task<bool> JoinAsync(string gameId, string userId, string displayName)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;

		if (game.Status != "Open") return false;
		if (game.Participants.Any(p => p.UserId == userId)) return true;
		if (game.Participants.Count >= game.MaxPlayers) return false;

		game.Participants.Add(new ParticipantInfo
		{
			UserId = userId,
			DisplayName = displayName
		});

		if (game.Participants.Count >= game.MaxPlayers)
			game.Status = "Full";

		game.UpdatedAt = DateTime.UtcNow;

		await _repository.UpdateAsync(game);
		return true;
	}

	public async Task<List<GameResponse>> SearchAsync(SearchGamesRequest r)
	{
		var games = await _repository.SearchAsync(r);
		return games.Select(Map).ToList();
	}

	private static GameResponse Map(GameSession game)
	{
		return new GameResponse
		{
			Id = game.Id!,
			Title = game.Title,

			System = new SystemDto
			{
				Key = game.System.Key,
				Name = game.System.Name
			},

			Host = new ParticipantDto
			{
				UserId = game.Host.UserId,
				DisplayName = game.Host.DisplayName
			},

			Participants = game.Participants.Select(p => new ParticipantDto
			{
				UserId = p.UserId,
				DisplayName = p.DisplayName
			}).ToList(),

			MaxPlayers = game.MaxPlayers,
			Status = game.Status,

			LocationId = game.LocationId,
			Location = new LocationSnapshotDto
			{
				Name = game.LocationSnapshot.Name,
				City = game.LocationSnapshot.City
			},

			ClubId = game.ClubId,
			StartTimeUtc = game.StartTimeUtc,
			Description = game.Description
		};
	}

	private readonly LocationRepository _locationRepository;

	public GameService(IGameRepository repository, LocationRepository locationRepository)
	{
		_repository = repository;
		_locationRepository = locationRepository;
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
}