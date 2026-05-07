using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class CreateGameRequest
{
	public string Title { get; set; } = default!;
	public string LocationId { get; set; } = default!;
	public string? ClubId { get; set; }
	public DateTime StartTimeUtc { get; set; }
	public string? Description { get; set; }
	public GameJoinMode JoinMode { get; set; } = GameJoinMode.ApprovalRequired;
	public List<CreateGameTableRequest> Tables { get; set; } = new();
}

public class CreateGameTableRequest
{
	public string Name { get; set; } = default!;
	public int MaxPlayers { get; set; }
	public List<string> Systems { get; set; } = new();
	public string? Scenario { get; set; }
	public int? Points { get; set; }
	public string? Notes { get; set; }
}