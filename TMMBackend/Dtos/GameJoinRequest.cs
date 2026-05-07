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
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
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
