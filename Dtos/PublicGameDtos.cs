using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

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
