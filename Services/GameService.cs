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
	private readonly IGamePlanningService _planningService;

	public GameService(
		IGameRepository repository,
		ILocationLookupService locationService,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		IGameAssignmentService assignmentService,
		IGameProposalService proposalService,
		IGamePlanningService planningService)
	{
		_repository = repository;
		_locationService = locationService;
		_currentUser = currentUser;
		_authorization = authorization;
		_assignmentService = assignmentService;
		_proposalService = proposalService;
		_planningService = planningService;
	}

	public async Task<GameResponse> CreateAsync(CreateGameRequest request)
	{
		GameValidator.ValidateCreate(request);

		var location = await _locationService.GetByIdAsync(request.LocationId);
		if (location == null)
			throw new DomainException("Spielort wurde nicht gefunden.");

		if (!_authorization.CanCreateGameAtLocation(location))
			throw new DomainException("Du darfst an diesem Spielort keinen Spieltermin erstellen.");

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

	public Task<GameResponse> AddDateOptionAsync(string gameId, AddDateOptionRequest request)
	{
		return _planningService.AddDateOptionAsync(gameId, request);
	}

	public Task<GameResponse> VoteDateOptionAsync(string gameId, string optionId)
	{
		return _planningService.VoteDateOptionAsync(gameId, optionId);
	}

	public Task<GameResponse> SelectDateOptionAsync(string gameId, string optionId)
	{
		return _planningService.SelectDateOptionAsync(gameId, optionId);
	}

	public Task<GameResponse> InviteFriendAsync(string gameId, InviteFriendToSessionRequest request)
	{
		return _planningService.InviteFriendAsync(gameId, request);
	}

	public Task<GameResponse> RespondInvitationAsync(string gameId, string invitationId, bool accept)
	{
		return _planningService.RespondInvitationAsync(gameId, invitationId, accept);
	}

	public Task<GameResponse> JoinWaitlistAsync(string gameId, JoinWaitlistRequest request)
	{
		return _planningService.JoinWaitlistAsync(gameId, request);
	}

	public Task<GameResponse> PromoteWaitlistEntryAsync(string gameId, string waitlistEntryId, string tableId)
	{
		return _planningService.PromoteWaitlistEntryAsync(gameId, waitlistEntryId, tableId);
	}

	public Task<GameResponse> CloseGameAsync(string gameId, CloseGameRequest request)
	{
		return _planningService.CloseGameAsync(gameId, request);
	}

	public Task<GameResponse> CancelGameAsync(string gameId)
	{
		return _planningService.CancelGameAsync(gameId);
	}

	public async Task<PublicGameResponse?> GetPublicAsync(string slugOrId)
	{
		var game = await _repository.GetByPublicSlugOrIdAsync(slugOrId);
		return game == null ? null : GameMapper.ToPublicResponse(game);
	}

	public Task<List<CalendarItemResponse>> GetCalendarAsync()
	{
		return _planningService.GetCalendarAsync();
	}

	private static string? NormalizeOptional(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}
}
