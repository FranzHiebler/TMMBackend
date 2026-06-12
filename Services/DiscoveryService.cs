using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class DiscoveryService : IDiscoveryService
{
	private readonly ILocationRepository _locations;
	private readonly IGameRepository _games;
	private readonly IUserRepository _users;
	private readonly ICurrentUserService _currentUser;

	public DiscoveryService(
		ILocationRepository locations,
		IGameRepository games,
		IUserRepository users,
		ICurrentUserService currentUser)
	{
		_locations = locations;
		_games = games;
		_users = users;
		_currentUser = currentUser;
	}

	public async Task<List<LocationDiscoveryResponse>> GetLocationsAsync(LocationDiscoveryRequest request)
	{
		var radiusInMeters = Math.Max(1, request.RadiusKm) * 1000;

		var locations = request.Latitude.HasValue && request.Longitude.HasValue
			? await _locations.FindNearbyLocationsAsync(
				request.Latitude.Value,
				request.Longitude.Value,
				radiusInMeters)
			: await _locations.GetWithGeoAsync();

		var devUserIds = await GetDevUserIdsAsync();
		var withGeo = locations
			.Where(location => location.Id != null && location.Geo != null)
			.Where(location => _currentUser.CanSeeDevData || !DevDataRules.IsDevLocation(location, devUserIds))
			.ToList();

		var upcomingGames = await _games.SearchUpcomingByLocationIdsAsync(
			DateTime.UtcNow,
			withGeo.Select(location => location.Id!).ToList());

		var gamesByLocation = upcomingGames
			.Where(game => _currentUser.CanSeeDevData || !DevDataRules.IsDevGame(game, devUserIds))
			.GroupBy(game => game.LocationId)
			.ToDictionary(
				group => group.Key,
				group => group.OrderBy(game => game.StartTimeUtc).ToList());

		return withGeo.Select(location =>
		{
			gamesByLocation.TryGetValue(location.Id!, out var games);
			var member = location.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);
			var canUseExactPosition = location.AccessMode == LocationAccessMode.Open || member != null;
			var latitude = location.Geo?.Coordinates.Latitude;
			var longitude = location.Geo?.Coordinates.Longitude;

			return new LocationDiscoveryResponse
			{
				LocationId = location.Id!,
				Name = location.Name,
				City = location.City,
				Address = canUseExactPosition ? location.Address : null,
				Latitude = latitude.HasValue ? (canUseExactPosition ? latitude : Math.Round(latitude.Value, 2)) : null,
				Longitude = longitude.HasValue ? (canUseExactPosition ? longitude : Math.Round(longitude.Value, 2)) : null,
				LocationPrecision = latitude.HasValue && longitude.HasValue
					? (canUseExactPosition ? "exact" : "approximate")
					: "hidden",
				IsOwnLocation = member != null,
				IsOpen = location.AccessMode == LocationAccessMode.Open,
				Role = member?.Role.ToString(),
				SystemKeys = location.SystemKeys,
				UpcomingGameCount = games?.Count ?? 0,
				NextGameStartTimeUtc = games?.FirstOrDefault()?.StartTimeUtc
			};
		}).ToList();
	}

	public async Task<List<GameDiscoveryResponse>> GetGamesAsync(DiscoveryGamesRequest request)
	{
		var fromUtc = request.FromUtc ?? DateTime.UtcNow;
		var toUtc = request.ToUtc ?? fromUtc.AddDays(30);
		List<string>? nearbyLocationIds = null;

		if (request.Latitude.HasValue && request.Longitude.HasValue)
		{
			var nearby = await _locations.FindNearbyAsync(
				request.Latitude.Value,
				request.Longitude.Value,
				request.RadiusKm * 1000);

			nearbyLocationIds = nearby.Select(x => x.LocationId).ToList();

			if (nearbyLocationIds.Count == 0)
				return new List<GameDiscoveryResponse>();
		}

		var devUserIds = await GetDevUserIdsAsync();
		var games = (await _games.SearchDiscoveryAsync(fromUtc, toUtc, nearbyLocationIds))
			.Where(game => _currentUser.CanSeeDevData || !DevDataRules.IsDevGame(game, devUserIds))
			.ToList();
		var locationIds = games
			.Select(game => game.LocationId)
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
		var locations = await _locations.GetByIdsAsync(locationIds);
		var locationsById = locations
			.Where(location => location.Id != null)
			.Where(location => _currentUser.CanSeeDevData || !DevDataRules.IsDevLocation(location, devUserIds))
			.ToDictionary(location => location.Id!, StringComparer.OrdinalIgnoreCase);
		var result = new List<GameDiscoveryResponse>();

		foreach (var game in games)
		{
			locationsById.TryGetValue(game.LocationId, out var location);
			if (location == null && !_currentUser.CanSeeDevData)
				continue;

			var isHost = game.Host.UserId == _currentUser.UserId;
			var isParticipant = game.Tables.Any(table =>
				table.AssignedPlayers.Any(player => player.UserId == _currentUser.UserId));

			var application = game.Tables
				.SelectMany(table => table.Applications)
				.FirstOrDefault(app => app.Player.UserId == _currentUser.UserId);

			var isOwnLocation = location?.Members.Any(member => member.UserId == _currentUser.UserId) ?? false;
			var canManageLocation = location?.Members.Any(member =>
				member.UserId == _currentUser.UserId &&
				(member.Role == LocationRole.Owner ||
				 member.Role == LocationRole.Admin ||
				 member.Role == LocationRole.Manager)) ?? false;
			var canUseExactPosition = location == null ||
				location.AccessMode == LocationAccessMode.Open ||
				isOwnLocation ||
				canManageLocation;
			var latitude = location?.Geo?.Coordinates.Latitude;
			var longitude = location?.Geo?.Coordinates.Longitude;

			result.Add(new GameDiscoveryResponse
			{
				GameId = game.Id!,
				Title = game.Title,
				StartTimeUtc = game.StartTimeUtc,
				TimingMode = game.TimingMode,
				TimeLabel = game.TimeLabel,
				LocationId = game.LocationId,
				LocationName = location?.Name ?? game.LocationSnapshot.Name,
				City = location?.City ?? game.LocationSnapshot.City,
				Latitude = latitude.HasValue ? (canUseExactPosition ? latitude : Math.Round(latitude.Value, 2)) : null,
				Longitude = longitude.HasValue ? (canUseExactPosition ? longitude : Math.Round(longitude.Value, 2)) : null,
				LocationPrecision = latitude.HasValue && longitude.HasValue
					? (canUseExactPosition ? "exact" : "approximate")
					: "hidden",
				Status = game.Status,
				IsHost = isHost,
				IsParticipant = isParticipant,
				IsOwnLocation = isOwnLocation,
				CanEdit = isHost || canManageLocation,
				TablesSummary = BuildTablesSummary(game),
				AvailableSeats = game.Tables.Sum(table => Math.Max(0, table.MaxPlayers - table.AssignedPlayers.Count)),
				JoinMode = game.JoinMode,
				ApplicationStatus = application?.Status.ToString()
			});
		}

		return result;
	}

	private async Task<HashSet<string>> GetDevUserIdsAsync()
	{
		if (_currentUser.CanSeeDevData)
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		var devUsers = await _users.GetDevUsersAsync();
		return devUsers
			.Where(user => !string.IsNullOrWhiteSpace(user.Id))
			.Select(user => user.Id!)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
	}

	private static string BuildTablesSummary(GameSession game)
	{
		return string.Join(" · ", game.Tables.Select(table =>
		{
			var systems = table.Systems.Count == 0
				? "Egal"
				: string.Join(", ", table.Systems);

			var points = table.Points.HasValue ? $" · {table.Points} Punkte" : "";

			return $"{table.Name}: {systems}{points}";
		}));
	}
}
