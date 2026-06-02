using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameAssignmentService : IGameAssignmentService
{
	private readonly IGameRepository _repository;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;
	private readonly INotificationService _notifications;

	public GameAssignmentService(
		IGameRepository repository,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization,
		INotificationService notifications)
	{
		_repository = repository;
		_currentUser = currentUser;
		_authorization = authorization;
		_notifications = notifications;
	}

	public async Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status != GameSessionState.Open)
			throw new DomainException("Diese Session ist nicht offen.");

		if (game.JoinMode != GameJoinMode.FirstComeFirstServe)
			throw new DomainException("Direkter Beitritt ist für diese Session nicht aktiviert.");

		if (GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new DomainException("Du bist bereits in dieser Session angemeldet.");

		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			throw new DomainException("Der Tisch ist voll.");

		if (!GameSessionRules.SystemMatches(table.Systems, request.SystemKey))
			throw new DomainException("Das gewählte System passt nicht zu diesem Tisch.");

		table.AssignedPlayers.Add(CurrentParticipant());

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);
	}

	public async Task ApplyAsync(string gameId, ApplyToGameRequest request)
	{
		var game = await GetGameOrThrow(gameId);

		if (game.Status != GameSessionState.Open)
			throw new DomainException("Diese Session ist nicht offen.");

		if (game.JoinMode != GameJoinMode.ApprovalRequired)
			throw new DomainException("Bewerbungen sind für diese Session nicht aktiviert.");

		if (GameSessionRules.IsUserAlreadyAssigned(game, _currentUser.UserId))
			throw new DomainException("Du bist bereits in dieser Session angemeldet.");

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

	public async Task AssignPlayerToTableAsync(
		string gameId,
		string tableId,
		AssignPlayerToTableRequest request)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);

		if (table.AssignedPlayers.Count >= table.MaxPlayers)
			throw new DomainException("Der Tisch ist voll.");

		var resolved = ResolvePlayerForAssignment(game, table, request);

		if (resolved == null)
			throw new DomainException("Spieler oder Bewerbung konnte nicht zugewiesen werden.");

		var (player, application) = resolved.Value;

		if (GameSessionRules.IsUserAlreadyAssigned(game, player.UserId))
			return;

		table.AssignedPlayers.Add(player);

		if (application != null)
			AcceptApplication(game, table, application, player.UserId);

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);

		if (application != null)
		{
			await _notifications.NotifyAsync(
				player.UserId,
				NotificationKind.ApplicationAccepted,
				"Bewerbung angenommen",
				$"Du bist bei \"{game.Title}\" dabei.",
				$"/sessions/{game.Id}");
		}
	}

	public async Task RejectApplicationAsync(string gameId, string applicationId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var application = game.Tables
			.SelectMany(t => t.Applications)
			.FirstOrDefault(a => a.ApplicationId == applicationId);

		if (application == null)
			throw new DomainException("Bewerbung nicht gefunden.");

		if (application.Status != ApplicationStatus.Pending)
			throw new DomainException("Diese Bewerbung wurde bereits bearbeitet.");

		application.Status = ApplicationStatus.Rejected;
		await SaveAsync(game);

		await _notifications.NotifyAsync(
			application.Player.UserId,
			NotificationKind.ApplicationRejected,
			"Bewerbung abgelehnt",
			$"Deine Bewerbung für \"{game.Title}\" wurde abgelehnt.",
			$"/sessions/{game.Id}");
	}

	public async Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId)
	{
		var game = await GetGameOrThrow(gameId);
		await EnsureCanManageAsync(game);

		var table = GameServiceHelpers.GetTableOrThrow(game, tableId);
		var player = table.AssignedPlayers.FirstOrDefault(p => p.UserId == userId);

		if (player == null)
			throw new DomainException("Spieler ist nicht an diesem Tisch.");

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
			throw new DomainException("Zieltisch fehlt.");

		var sourceTable = game.Tables.FirstOrDefault(t =>
			t.AssignedPlayers.Any(p => p.UserId == userId));

		if (sourceTable == null)
			throw new DomainException("Spieler ist keinem Tisch zugewiesen.");

		var targetTable = GameServiceHelpers.GetTableOrThrow(game, request.TargetTableId);

		if (sourceTable.TableId == targetTable.TableId)
			return;

		if (targetTable.AssignedPlayers.Count >= targetTable.MaxPlayers)
			throw new DomainException("Der Zieltisch ist voll.");

		var player = sourceTable.AssignedPlayers.First(p => p.UserId == userId);

		sourceTable.AssignedPlayers.Remove(player);
		targetTable.AssignedPlayers.Add(player);

		GameSessionRules.UpdateSessionState(game);
		await SaveAsync(game);
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

	private static GameTable GetApplicationTargetTable(GameSession game, ApplyToGameRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.TableId))
			return game.Tables.First();

		var table = GameServiceHelpers.GetTableOrThrow(game, request.TableId);

		if (!GameSessionRules.SystemMatches(table.Systems, request.SystemKey))
			throw new DomainException("Das gewählte System passt nicht zu diesem Tisch.");

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
