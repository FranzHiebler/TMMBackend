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
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			},
			Participants = new List<ParticipantInfo>
		{
			new ParticipantInfo
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			}
		},
			MaxPlayers = request.MaxPlayers,
			Status = GameSessionState.Open,
			LocationId = request.LocationId,
			LocationSnapshot = new LocationSnapshot
			{
				Name = location.Name,
				City = location.City
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

	public async Task<bool> JoinAsync(string gameId)
	{
		var game = await _repository.GetByIdAsync(gameId);
		if (game == null) return false;

		if (game.Status != GameSessionState.Open) return false;
		if (game.Participants.Any(p => p.UserId == _currentUser.UserId)) return true;
		if (game.Participants.Count >= game.MaxPlayers) return false;

		game.Participants.Add(new ParticipantInfo
		{
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName,
		});

		if (game.Participants.Count >= game.MaxPlayers)
			game.Status = GameSessionState.Full;

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