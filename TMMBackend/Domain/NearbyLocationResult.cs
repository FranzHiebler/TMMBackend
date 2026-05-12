namespace TabletopMatchMaker.Domain;

public class NearbyLocationResult
{
	public string LocationId { get; set; } = default!;
	public double DistanceInMeters { get; set; }
	public string Name { get; set; } = default!;
	public string City { get; set; } = default!;
}