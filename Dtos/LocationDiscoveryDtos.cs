namespace TabletopMatchMaker.Dtos;

public class LocationDiscoveryRequest
{
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double RadiusKm { get; set; } = 80;
}

public class LocationDiscoveryResponse
{
	public string LocationId { get; set; } = default!;
	public string Name { get; set; } = default!;
	public string City { get; set; } = default!;
	public string? Address { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string LocationPrecision { get; set; } = "hidden";
	public bool IsOwnLocation { get; set; }
	public bool IsOpen { get; set; }
	public string? Role { get; set; }
	public List<string> SystemKeys { get; set; } = new();
	public int UpcomingGameCount { get; set; }
	public DateTime? NextGameStartTimeUtc { get; set; }
}
