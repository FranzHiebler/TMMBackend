using TabletopMatchMaker.Domain;

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
	public SessionTimingMode TimingMode { get; set; }
	public string? TimeLabel { get; set; }
	public string? Description { get; set; }
	public List<GameTableDto> Tables { get; set; } = new();
	public List<GameChangeProposalDto> ChangeProposals { get; set; } = new();
	public List<SessionDateOptionDto> DateOptions { get; set; } = new();
	public List<SessionInvitationDto> Invitations { get; set; } = new();
	public List<WaitlistEntryDto> Waitlist { get; set; } = new();
	public GameResultDto? Result { get; set; }
	public string? PublicSlug { get; set; }
	public string? SeriesId { get; set; }

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

public class PublicGameResponse
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public GameSessionState Status { get; set; }
	public DateTime StartTimeUtc { get; set; }
	public SessionTimingMode TimingMode { get; set; }
	public string? TimeLabel { get; set; }
	public LocationSnapshotDto Location { get; set; } = new();
	public string? Description { get; set; }
	public List<GameTableDto> Tables { get; set; } = new();
	public int OpenSlots { get; set; }
}

public class CalendarItemResponse
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public string Kind { get; set; } = default!;
	public DateTime? StartTimeUtc { get; set; }
	public SessionTimingMode? TimingMode { get; set; }
	public string? TimeLabel { get; set; }
	public string? LocationName { get; set; }
	public string? Status { get; set; }
}
