using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameProposalService : IGameProposalService
{
	private readonly IGameRepository _repository;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;

	public GameProposalService(
		IGameRepository repository,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization)
	{
		_repository = repository;
		_currentUser = currentUser;
		_authorization = authorization;
	}

	public async Task<GameResponse> CreateChangeProposalAsync(
		string gameId,
		CreateChangeProposalRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status is GameSessionState.Cancelled or GameSessionState.Closed)
			throw new DomainException("Diese Session kann nicht mehr geändert werden.");

		if (!GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new DomainException("Nur angemeldete Spieler dürfen Änderungen vorschlagen.");

		var table = GameServiceHelpers.ResolveProposalTable(game, request);
		var proposedSystems = GameServiceHelpers.NormalizeSystems(request.ProposedSystems);

		var hasTimeChange = request.ProposedStartTimeUtc.HasValue;
		var hasSystemChange = proposedSystems is { Count: > 0 };
		var hasPointsChange = request.ProposedPoints.HasValue;

		if (!hasTimeChange && !hasSystemChange && !hasPointsChange)
			throw new DomainException("Bitte mindestens Uhrzeit, System oder Punkte vorschlagen.");

		if ((hasSystemChange || hasPointsChange) && table == null)
			throw new DomainException("System- oder Punkteänderungen brauchen einen Tisch.");

		if (request.ProposedPoints is < 0)
			throw new DomainException("Punkte dürfen nicht negativ sein.");

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
		var table = GameServiceHelpers.ResolveExistingProposalTable(game, proposal);

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

	private async Task<GameSession> GetGameOrThrow(string gameId)
	{
		return await _repository.GetByIdAsync(gameId)
			?? throw new DomainException("Session nicht gefunden.");
	}

	private async Task EnsureCanManageAsync(GameSession game)
	{
		if (!await _authorization.CanManageSessionAsync(game))
			throw new DomainException("Du darfst diese Session nicht verwalten.");
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

	private static void ApplyProposal(
		GameSession game,
		GameTable? table,
		GameChangeProposal proposal)
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
			if (table == null) throw new DomainException("Tisch nicht gefunden.");
			table.Systems = proposal.ProposedSystems;
		}

		if (proposal.ProposedPoints.HasValue)
		{
			if (table == null) throw new DomainException("Tisch nicht gefunden.");
			table.Points = proposal.ProposedPoints;
		}
	}
}