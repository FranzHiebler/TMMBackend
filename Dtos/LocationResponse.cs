namespace TabletopMatchMaker.Dtos;

public class LocationResponse
{
	public string? Id { get; set; }
	public string Name { get; set; } = default!;
	public string City { get; set; } = default!;
	public string? Address { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Role { get; set; }
	public bool IsOpen { get; set; }
	public List<string> SystemKeys { get; set; } = new();
	public bool HasPendingJoinRequest { get; set; }
}