using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Validation;

public static class LocationValidator
{
	public static void ValidateCreateOrUpdate(CreateLocationRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
			throw new DomainException("Name der Location ist erforderlich.");

		if (request.Name.Trim().Length > 120)
			throw new DomainException("Name der Location darf maximal 120 Zeichen lang sein.");

		if (string.IsNullOrWhiteSpace(request.City))
			throw new DomainException("Stadt ist erforderlich.");

		ValidateCoordinates(request.Latitude, request.Longitude);

		if (request.SystemKeys.Any(string.IsNullOrWhiteSpace))
			throw new DomainException("Systeme dürfen nicht leer sein.");
	}

	public static void ValidateNearby(SearchNearbyLocationsRequest request)
	{
		ValidateCoordinates(request.Latitude, request.Longitude);

		if (request.RadiusInMeters <= 0)
			throw new DomainException("Radius muss größer als 0 sein.");

		if (request.RadiusInMeters > 200000)
			throw new DomainException("Radius darf maximal 200 km betragen.");
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