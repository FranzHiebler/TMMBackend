using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class GameChangeProposalDto
{
	public string Id { get; set; } = default!;
	public string? TableId { get; set; }
	public ParticipantDto ProposedBy { get; set; } = new();
	public DateTime? ProposedStartTimeUtc { get; set; }
	public List<string>? ProposedSystems { get; set; }
	public int? ProposedPoints { get; set; }
	public string? Message { get; set; }
	public ChangeProposalStatus Status { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? ResolvedAt { get; set; }
}

public class SessionDateOptionDto
{
	public string Id { get; set; } = default!;
	public DateTime StartTimeUtc { get; set; }
	public string? Label { get; set; }
	public List<ParticipantDto> Votes { get; set; } = new();
	public DateTime CreatedAtUtc { get; set; }
}

public class SessionInvitationDto
{
	public string Id { get; set; } = default!;
	public ParticipantDto User { get; set; } = new();
	public SessionInvitationStatus Status { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? RespondedAtUtc { get; set; }
}

public class WaitlistEntryDto
{
	public string Id { get; set; } = default!;
	public string? TableId { get; set; }
	public ParticipantDto Player { get; set; } = new();
	public string? SystemKey { get; set; }
	public string? Message { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}

public class GameResultDto
{
	public GameResultKind Kind { get; set; }
	public string Value { get; set; } = default!;
	public string? Notes { get; set; }
	public ParticipantDto RecordedBy { get; set; } = new();
	public DateTime RecordedAtUtc { get; set; }
}

public class AddDateOptionRequest
{
	public DateTime StartTimeUtc { get; set; }
	public string? Label { get; set; }
}

public class InviteFriendToSessionRequest
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}

public class JoinWaitlistRequest
{
	public string? TableId { get; set; }
	public string? SystemKey { get; set; }
	public string? Message { get; set; }
}

public class CloseGameRequest
{
	public GameResultKind Kind { get; set; } = GameResultKind.FreeText;
	public string Value { get; set; } = default!;
	public string? Notes { get; set; }
}
