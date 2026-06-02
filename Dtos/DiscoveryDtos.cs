using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class DiscoveryGamesRequest
{
	public DateTime? FromUtc { get; set; }
	public DateTime? ToUtc { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double RadiusKm { get; set; } = 50;
}

public class GameDiscoveryResponse
{
	public string GameId { get; set; } = default!;
	public string Title { get; set; } = default!;
	public DateTime StartTimeUtc { get; set; }
	public SessionTimingMode TimingMode { get; set; }
	public string? TimeLabel { get; set; }
	public string LocationId { get; set; } = default!;
	public string LocationName { get; set; } = default!;
	public string City { get; set; } = default!;
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string LocationPrecision { get; set; } = "hidden";
	public GameSessionState Status { get; set; }
	public bool IsHost { get; set; }
	public bool IsParticipant { get; set; }
	public bool IsOwnLocation { get; set; }
	public bool CanEdit { get; set; }
	public string TablesSummary { get; set; } = default!;
	public int AvailableSeats { get; set; }
	public GameJoinMode JoinMode { get; set; }
	public string? ApplicationStatus { get; set; }
}
