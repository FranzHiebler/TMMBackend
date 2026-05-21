namespace TabletopMatchMaker.Dtos;

public class JoinTableRequest
{
	public string? SystemKey { get; set; }
}

public class ApplyToGameRequest
{
	public string? TableId { get; set; }
	public string? SystemKey { get; set; }
	public string? Message { get; set; }
}

public class AssignPlayerToTableRequest
{
	public string? UserId { get; set; }
	public string? DisplayName { get; set; }
	public string? ApplicationId { get; set; }
}

public class CreateChangeProposalRequest
{
	public string? TableId { get; set; }
	public DateTime? ProposedStartTimeUtc { get; set; }
	public List<string>? ProposedSystems { get; set; }
	public int? ProposedPoints { get; set; }
	public string? Message { get; set; }
}

public class MovePlayerToTableRequest
{
	public string TargetTableId { get; set; } = default!;
}

public class UpdateGameSessionRequest
{
	public string Title { get; set; } = default!;
	public DateTime StartTimeUtc { get; set; }
	public string? Description { get; set; }
}

public class UpdateGameTableRequest
{
	public string Name { get; set; } = default!;
	public int MaxPlayers { get; set; }
	public List<string> Systems { get; set; } = new();
	public string? Scenario { get; set; }
	public int? Points { get; set; }
	public DateTime? StartTimeUtc { get; set; }
	public string? Notes { get; set; }
}