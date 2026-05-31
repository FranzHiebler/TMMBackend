using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class CreatePlayRequestRequest
{
	public string SystemKey { get; set; } = default!;
	public string? LocationId { get; set; }
	public string? TimeNote { get; set; }
	public DateTime? ExactTimeUtc { get; set; }
	public int? RadiusKm { get; set; }
	public string? Note { get; set; }
}

public class PlayRequestResponse
{
	public string Id { get; set; } = default!;
	public ParticipantDto Owner { get; set; } = new();
	public string SystemKey { get; set; } = default!;
	public string? LocationId { get; set; }
	public string? LocationName { get; set; }
	public string? City { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string LocationPrecision { get; set; } = "hidden";
	public string? TimeNote { get; set; }
	public DateTime? ExactTimeUtc { get; set; }
	public int? RadiusKm { get; set; }
	public string? Note { get; set; }
	public PlayRequestStatus Status { get; set; }
	public string? ConvertedGameId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
	public bool IsMine { get; set; }
}

public class ConvertPlayRequestRequest
{
	public string LocationId { get; set; } = default!;
	public int MaxPlayers { get; set; } = 2;
	public DateTime StartTimeUtc { get; set; }
}
