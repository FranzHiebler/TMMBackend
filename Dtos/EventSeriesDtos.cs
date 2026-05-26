using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class CreateEventSeriesRequest
{
	public string Title { get; set; } = default!;
	public string LocationId { get; set; } = default!;
	public List<string> SystemKeys { get; set; } = new();
	public EventRecurrenceKind RecurrenceKind { get; set; } = EventRecurrenceKind.Weekly;
	public DayOfWeek DayOfWeek { get; set; }
	public string? TimeLabel { get; set; }
	public int StartHour { get; set; } = 18;
	public DateTime? StartDateUtc { get; set; }
	public DateTime? EndDateUtc { get; set; }
	public int DefaultMaxPlayers { get; set; } = 2;
	public string? Description { get; set; }
}

public class EventSeriesResponse
{
	public string Id { get; set; } = default!;
	public string Title { get; set; } = default!;
	public ParticipantDto Host { get; set; } = new();
	public string LocationId { get; set; } = default!;
	public LocationSnapshotDto Location { get; set; } = new();
	public List<string> SystemKeys { get; set; } = new();
	public EventRecurrenceKind RecurrenceKind { get; set; }
	public DayOfWeek DayOfWeek { get; set; }
	public string? TimeLabel { get; set; }
	public int StartHour { get; set; }
	public DateTime? StartDateUtc { get; set; }
	public DateTime? EndDateUtc { get; set; }
	public int DefaultMaxPlayers { get; set; }
	public string? Description { get; set; }
	public List<DateTime> UpcomingStartTimesUtc { get; set; } = new();
}
