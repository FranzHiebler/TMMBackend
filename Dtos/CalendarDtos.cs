using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class CalendarItemResponse
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public string Kind { get; set; } = default!;
	public DateTime? StartTimeUtc { get; set; }
	public SessionTimingMode? TimingMode { get; set; }
	public string? TimeLabel { get; set; }
	public string? LocationName { get; set; }
	public string? LocationCity { get; set; }
	public string? Status { get; set; }
}
