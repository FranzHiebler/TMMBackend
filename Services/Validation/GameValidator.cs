using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Validation;

public static class GameValidator
{
	public static void ValidateCreate(CreateGameRequest request)
	{
		ValidateSessionBase(request.Title, request.StartTimeUtc, request.TimingMode);

		if (string.IsNullOrWhiteSpace(request.LocationId))
			throw new DomainException("Location ist erforderlich.");

		if (request.Tables == null || request.Tables.Count == 0)
			throw new DomainException("Es muss mindestens ein Tisch angelegt werden.");

		foreach (var table in request.Tables)
			ValidateTable(table.Name, table.MaxPlayers, table.Systems, table.Points);
	}

	public static void ValidateUpdateSession(UpdateGameSessionRequest request)
	{
		ValidateSessionBase(request.Title, request.StartTimeUtc, SessionTimingMode.Fixed);
	}

	public static void ValidateUpdateTable(UpdateGameTableRequest table)
	{
		ValidateTable(table.Name, table.MaxPlayers, table.Systems, table.Points);
	}

	public static void ValidateSearch(SearchGamesRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.City) && request.City.Trim().Length > 120)
			throw new DomainException("Stadt darf maximal 120 Zeichen lang sein.");

		if (!string.IsNullOrWhiteSpace(request.SystemKey) && request.SystemKey.Trim().Length > 80)
			throw new DomainException("System darf maximal 80 Zeichen lang sein.");
	}

	public static void ValidateNearby(SearchNearbyGamesRequest request)
	{
		ValidateCoordinates(request.Latitude, request.Longitude);

		if (request.RadiusInMeters <= 0)
			throw new DomainException("Radius muss größer als 0 sein.");

		if (request.RadiusInMeters > 200000)
			throw new DomainException("Radius darf maximal 200 km betragen.");

		if (!string.IsNullOrWhiteSpace(request.SortBy) &&
			!request.SortBy.Equals("distance", StringComparison.OrdinalIgnoreCase) &&
			!request.SortBy.Equals("date", StringComparison.OrdinalIgnoreCase))
			throw new DomainException("Sortierung muss 'distance' oder 'date' sein.");

		if (!string.IsNullOrWhiteSpace(request.SystemKey) && request.SystemKey.Trim().Length > 80)
			throw new DomainException("System darf maximal 80 Zeichen lang sein.");
	}

	private static void ValidateSessionBase(string title, DateTime startTimeUtc, SessionTimingMode timingMode)
	{
		if (string.IsNullOrWhiteSpace(title))
			throw new DomainException("Titel ist erforderlich.");

		if (title.Trim().Length > 120)
			throw new DomainException("Titel darf maximal 120 Zeichen lang sein.");

		if (timingMode == SessionTimingMode.Fixed && startTimeUtc == default)
			throw new DomainException("Startzeit ist erforderlich.");
	}

	private static void ValidateTable(string name, int maxPlayers, List<string> systems, int? points)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new DomainException("Jeder Tisch braucht einen Namen.");

		if (maxPlayers < 1)
			throw new DomainException("Ein Tisch braucht mindestens einen Spielerplatz.");

		if (maxPlayers > 20)
			throw new DomainException("Ein Tisch darf maximal 20 Spielerplätze haben.");

		if (points is < 0)
			throw new DomainException("Punkte dürfen nicht negativ sein.");

		if (systems.Any(string.IsNullOrWhiteSpace))
			throw new DomainException("Systeme dürfen nicht leer sein.");
	}

	private static void ValidateCoordinates(double latitude, double longitude)
	{
		if (latitude < -90 || latitude > 90)
			throw new DomainException("Latitude muss zwischen -90 und 90 liegen.");

		if (longitude < -180 || longitude > 180)
			throw new DomainException("Longitude muss zwischen -180 und 180 liegen.");

		if (latitude == 0 && longitude == 0)
			throw new DomainException("Bitte gültige Koordinaten setzen.");
	}
}


