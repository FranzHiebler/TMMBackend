using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameService : IGameService
{
	private readonly IGameRepository _repository;
	private readonly ILocationLookupService _locationService;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;
	private readonly IGameAssignmentService _assignmentService;
	private readonly IGameProposalService _proposalService;

	public GameService(
		IGameRepository repository,
		ILocationLookupService locationService,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		IGameAssignmentService assignmentService,
		IGameProposalService proposalService)
	{
		_repository = repository;
		_locationService = locationService;
		_currentUser = currentUser;
		_authorization = authorization;
		_assignmentService = assignmentService;
		_proposalService = proposalService;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		var location = await _locationService.GetByIdAsync(request.LocationId);
		if (location == null)
			throw new GameActionException("Location wurde nicht gefunden.");

		if (!_authorization.CanCreateGameAtLocation(location))
			throw new GameActionException("Du darfst an dieser Location keine Game Session erstellen.");

		if (request.Tables.Count == 0)
			throw new GameActionException("Es muss mindestens ein Tisch angelegt werden.");

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

	public Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		return _assignmentService.JoinTableAsync(gameId, tableId, request);
	}

	public Task ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		return _assignmentService.ApplyAsync(gameId, request);
	}

	public Task<bool> AssignPlayerToTableAsync(
		string gameId,
		string tableId,
		AssignPlayerToTableRequest request)
	{
		return _assignmentService.AssignPlayerToTableAsync(gameId, tableId, request);
	}

	public async Task<List<GameResponse>> SearchAsync(SearchGamesRequest request)
	{
		var games = await _repository.SearchAsync(request);
		return games.Select(GameMapper.ToResponse).ToList();
	}

	public async Task<List<GameResponse>> SearchNearbyAsync(SearchNearbyGamesRequest request)
	{
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
}