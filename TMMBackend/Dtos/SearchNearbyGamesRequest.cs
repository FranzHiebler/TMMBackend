namespace TabletopMatchMaker.Dtos;

public class SearchNearbyGamesRequest
{
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public double RadiusInMeters { get; set; } = 20000;

	public string? SystemKey { get; set; }
	public bool OnlyOpen { get; set; } = true;
	public DateTime? FromUtc { get; set; }

	public string SortBy { get; set; } = "distance"; // distance | date
	public bool SortDescending { get; set; } = false;
}