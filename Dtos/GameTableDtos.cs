using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class GameTableDto
{
	public string Id { get; set; } = default!;
	public string Name { get; set; } = default!;
	public int MaxPlayers { get; set; }
	public List<string> Systems { get; set; } = new();
	public string? Scenario { get; set; }
	public int? Points { get; set; }
	public DateTime? StartTimeUtc { get; set; }
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
