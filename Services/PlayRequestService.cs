using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class PlayRequestService : IPlayRequestService
{
	private readonly IPlayRequestRepository _repository;
	private readonly ILocationLookupService _locations;
	private readonly IUserRepository _users;
	private readonly IGameService _games;
	private readonly ICurrentUserService _currentUser;
	private readonly INotificationService _notifications;

	public PlayRequestService(
		IPlayRequestRepository repository,
		ILocationLookupService locations,
		IUserRepository users,
		IGameService games,
		ICurrentUserService currentUser,
		INotificationService notifications)
	{
		_repository = repository;
		_locations = locations;
		_users = users;
		_games = games;
		_currentUser = currentUser;
		_notifications = notifications;
	}

	public async Task<List<PlayRequestResponse>> GetOpenAsync()
	{
		var requests = await _repository.GetOpenAsync();
		if (!_currentUser.CanSeeDevData)
		{
			var devUserIds = await GetDevUserIdsAsync();
			requests = requests
				.Where(request => !devUserIds.Contains(request.Owner.UserId))
				.ToList();
		}

		return (await Task.WhenAll(requests.Select(MapAsync))).ToList();
	}

	public async Task<List<PlayRequestResponse>> GetMineAsync()
	{
		var requests = await _repository.GetForUserAsync(_currentUser.UserId);
		return (await Task.WhenAll(requests.Select(MapAsync))).ToList();
	}

	public async Task<PlayRequestResponse> CreateAsync(CreatePlayRequestRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.SystemKey))
			throw new DomainException("System ist erforderlich.");

		var playRequest = new PlayRequest
		{
			Owner = new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName },
			SystemKey = request.SystemKey.Trim(),
			TimeNote = NormalizeOptional(request.TimeNote),
			ExactTimeUtc = request.ExactTimeUtc,
			RadiusKm = request.RadiusKm is > 0 and <= 500 ? request.RadiusKm : null,
			Note = NormalizeOptional(request.Note),
			Status = PlayRequestStatus.Open,
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		};

		if (!string.IsNullOrWhiteSpace(request.LocationId))
		{
			var location = await _locations.GetByIdAsync(request.LocationId);
			if (location == null)
				throw new DomainException("Spielort wurde nicht gefunden.");
			playRequest.LocationId = location.Id;
			playRequest.LocationName = location.Name;
			playRequest.City = location.City;
			playRequest.Latitude = location.Geo?.Coordinates.Latitude;
			playRequest.Longitude = location.Geo?.Coordinates.Longitude;
		}

		await _repository.CreateAsync(playRequest);
		await _notifications.NotifyAsync(
			_currentUser.UserId,
			NotificationKind.PlayRequestCreated,
			"Spielgesuch erstellt",
			"Dein Spielgesuch ist sichtbar.",
			$"/play-requests?requestId={playRequest.Id}");

		return await MapAsync(playRequest);
	}

	public async Task<GameResponse> ConvertToSessionAsync(string id, ConvertPlayRequestRequest request)
	{
		var playRequest = await GetMineOrThrow(id);
		if (playRequest.Status != PlayRequestStatus.Open)
			throw new DomainException("Dieses Spielgesuch ist nicht offen.");

		var game = await _games.CreateAsync(new CreateGameRequest
		{
			Title = $"{playRequest.SystemKey} Spiel",
			LocationId = request.LocationId,
			StartTimeUtc = request.StartTimeUtc,
			TimingMode = playRequest.ExactTimeUtc.HasValue ? SessionTimingMode.Fixed : SessionTimingMode.Rough,
			TimeLabel = playRequest.TimeNote,
			Description = playRequest.Note,
			Tables = new List<CreateGameTableRequest>
			{
				new()
				{
					Name = "Tisch 1",
					MaxPlayers = request.MaxPlayers,
					Systems = new List<string> { playRequest.SystemKey }
				}
			}
		});

		playRequest.Status = PlayRequestStatus.Converted;
		playRequest.ConvertedGameId = game.Id;
		playRequest.UpdatedAtUtc = DateTime.UtcNow;
		await _repository.UpdateAsync(playRequest);
		return game;
	}

	public async Task CloseAsync(string id)
	{
		var playRequest = await GetMineOrThrow(id);
		playRequest.Status = PlayRequestStatus.Closed;
		playRequest.UpdatedAtUtc = DateTime.UtcNow;
		await _repository.UpdateAsync(playRequest);
	}

	private async Task<PlayRequest> GetMineOrThrow(string id)
	{
		var playRequest = await _repository.GetByIdAsync(id)
			?? throw new DomainException("Spielgesuch nicht gefunden.");
		if (playRequest.Owner.UserId != _currentUser.UserId)
			throw new DomainException("Du darfst dieses Spielgesuch nicht bearbeiten.");
		return playRequest;
	}

	private async Task<PlayRequestResponse> MapAsync(PlayRequest request)
	{
		var mapPosition = await ResolveMapPositionAsync(request);

		return new PlayRequestResponse
		{
			Id = request.Id!,
			Owner = new ParticipantDto { UserId = request.Owner.UserId, DisplayName = request.Owner.DisplayName },
			SystemKey = request.SystemKey,
			LocationId = request.LocationId,
			LocationName = request.LocationName,
			City = mapPosition.City,
			Latitude = mapPosition.Latitude,
			Longitude = mapPosition.Longitude,
			LocationPrecision = mapPosition.Precision,
			TimeNote = request.TimeNote,
			ExactTimeUtc = request.ExactTimeUtc,
			RadiusKm = request.RadiusKm,
			Note = request.Note,
			Status = request.Status,
			ConvertedGameId = request.ConvertedGameId,
			CreatedAtUtc = request.CreatedAtUtc,
			UpdatedAtUtc = request.UpdatedAtUtc,
			IsMine = request.Owner.UserId == _currentUser.UserId
		};
	}

	private async Task<PlayRequestMapPosition> ResolveMapPositionAsync(PlayRequest request)
	{
		var isMine = request.Owner.UserId == _currentUser.UserId;

		if (!string.IsNullOrWhiteSpace(request.LocationId))
		{
			if (!request.Latitude.HasValue || !request.Longitude.HasValue)
				return PlayRequestMapPosition.Hidden(request.City);

			if (isMine)
				return new PlayRequestMapPosition(request.Latitude, request.Longitude, "exact", request.City);

			var location = await _locations.GetByIdAsync(request.LocationId);
			if (location?.AccessMode == LocationAccessMode.Open)
				return new PlayRequestMapPosition(request.Latitude, request.Longitude, "exact", request.City);

			if (!string.IsNullOrWhiteSpace(request.City))
				return new PlayRequestMapPosition(
					Math.Round(request.Latitude.Value, 2),
					Math.Round(request.Longitude.Value, 2),
					"approximate",
					request.City);

			return PlayRequestMapPosition.Hidden(request.City);
		}

		var owner = await _users.GetByIdAsync(request.Owner.UserId);
		if (owner == null || !owner.Latitude.HasValue || !owner.Longitude.HasValue)
			return PlayRequestMapPosition.Hidden(request.City);

		var canSeeOwnerCity = CanSee(owner.Visibility?.City, false, isMine);
		var visibleCity = canSeeOwnerCity ? owner.City : request.City;
		if (isMine)
			return new PlayRequestMapPosition(owner.Latitude, owner.Longitude, "exact", visibleCity);

		if (owner.HideOnMap)
			return PlayRequestMapPosition.Hidden(request.City);

		if (!isMine && !canSeeOwnerCity)
			return PlayRequestMapPosition.Hidden(request.City);

		return new PlayRequestMapPosition(
			Math.Round(owner.Latitude.Value, 2),
			Math.Round(owner.Longitude.Value, 2),
			"approximate",
			visibleCity);
	}

	private async Task<HashSet<string>> GetDevUserIdsAsync()
	{
		var devUsers = await _users.GetDevUsersAsync();
		return devUsers
			.Where(user => !string.IsNullOrWhiteSpace(user.Id))
			.Select(user => user.Id!)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
	}

	private static bool CanSee(ProfileFieldVisibility? visibility, bool isFriend, bool isOwnProfile)
	{
		if (isOwnProfile) return true;

		return visibility switch
		{
			ProfileFieldVisibility.Public => true,
			ProfileFieldVisibility.FriendsOnly => isFriend,
			_ => false
		};
	}

	private sealed record PlayRequestMapPosition(
		double? Latitude,
		double? Longitude,
		string Precision,
		string? City)
	{
		public static PlayRequestMapPosition Hidden(string? city) => new(null, null, "hidden", city);
	}

	private static string? NormalizeOptional(string? value) =>
		string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
