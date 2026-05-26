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
