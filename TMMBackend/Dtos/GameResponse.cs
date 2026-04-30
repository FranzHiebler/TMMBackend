using TMMBackend.Domain;

namespace TabletopMatchMaker.Dtos;

public class GameResponse
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public ParticipantDto Host { get; set; } = new();
	public GameSessionState Status { get; set; }
	public GameJoinMode JoinMode { get; set; }
	public string LocationId { get; set; } = default!;
	public LocationSnapshotDto Location { get; set; } = new();
	public string? ClubId { get; set; }
	public DateTime StartTimeUtc { get; set; }
	public string? Description { get; set; }
	public List<GameTableDto> Tables { get; set; } = new();

	public int MaxPlayers => Tables.Sum(x => x.MaxPlayers);
	public int AssignedPlayers => Tables.Sum(x => x.AssignedPlayers.Count);
	public int OpenSlots => MaxPlayers - AssignedPlayers;
}

public class GameTableDto
{
	public string Id { get; set; } = default!;
	public string Name { get; set; } = default!;
	public int MaxPlayers { get; set; }
	public List<string> Systems { get; set; } = new();
	public string? Scenario { get; set; }
	public int? Points { get; set; }
	public string? Notes { get; set; }
	public List<ParticipantDto> AssignedPlayers { get; set; } = new();
	public List<TableApplicationDto> Applications { get; set; } = new();
	public int OpenSlots => MaxPlayers - AssignedPlayers.Count;
}

public class TableApplicationDto
{
	public string Id { get; set; } = default!;
	public string? TableId { get; set; }
	public ParticipantDto Player { get; set; } = new();
	public string? SystemKey { get; set; }
	public string? Message { get; set; }
	public ApplicationStatus Status { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class ParticipantDto
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}

public class LocationSnapshotDto
{
	public string Name { get; set; } = default!;
	public string City { get; set; } = default!;
}