using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class DiscoveryService : IDiscoveryService
{
	private readonly ILocationRepository _locations;
	private readonly IGameRepository _games;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;

	public DiscoveryService(
		ILocationRepository locations,
		IGameRepository games,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization)
	{
		_locations = locations;
		_games = games;
		_currentUser = currentUser;
		_authorization = authorization;
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

		var withGeo = locations
			.Where(location => location.Id != null && location.Geo != null)
			.ToList();

		var upcomingGames = await _games.SearchUpcomingByLocationIdsAsync(
			DateTime.UtcNow,
			withGeo.Select(location => location.Id!).ToList());

		var gamesByLocation = upcomingGames
			.GroupBy(game => game.LocationId)
			.ToDictionary(
				group => group.Key,
				group => group.OrderBy(game => game.StartTimeUtc).ToList());

		return withGeo.Select(location =>
		{
			gamesByLocation.TryGetValue(location.Id!, out var games);
			var member = location.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);

			return new LocationDiscoveryResponse
			{
				LocationId = location.Id!,
				Name = location.Name,
				City = location.City,
				Address = location.Address,
				Latitude = location.Geo?.Coordinates.Latitude,
				Longitude = location.Geo?.Coordinates.Longitude,
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

		var games = await _games.SearchDiscoveryAsync(fromUtc, toUtc, nearbyLocationIds);
		var result = new List<GameDiscoveryResponse>();

		foreach (var game in games)
		{
			var location = await _locations.GetByIdAsync(game.LocationId);

			var isHost = game.Host.UserId == _currentUser.UserId;
			var isParticipant = game.Tables.Any(table =>
				table.AssignedPlayers.Any(player => player.UserId == _currentUser.UserId));

			var application = game.Tables
				.SelectMany(table => table.Applications)
				.FirstOrDefault(app => app.Player.UserId == _currentUser.UserId);

			var isOwnLocation = location?.Members.Any(member => member.UserId == _currentUser.UserId) ?? false;

			result.Add(new GameDiscoveryResponse
			{
				GameId = game.Id!,
				Title = game.Title,
				StartTimeUtc = game.StartTimeUtc,
				LocationId = game.LocationId,
				LocationName = location?.Name ?? game.LocationSnapshot.Name,
				City = location?.City ?? game.LocationSnapshot.City,
				Latitude = location?.Geo?.Coordinates.Latitude,
				Longitude = location?.Geo?.Coordinates.Longitude,
				Status = game.Status,
				IsHost = isHost,
				IsParticipant = isParticipant,
				IsOwnLocation = isOwnLocation,
				CanEdit = isHost || await _authorization.CanManageSessionAsync(game),
				TablesSummary = BuildTablesSummary(game),
				AvailableSeats = game.Tables.Sum(table => Math.Max(0, table.MaxPlayers - table.AssignedPlayers.Count)),
				JoinMode = game.JoinMode,
				ApplicationStatus = application?.Status.ToString()
			});
		}

		return result;
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