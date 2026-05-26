using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum EventRecurrenceKind
{
	Weekly,
	BiWeekly,
	MonthlyFirstWeekday
}

[BsonIgnoreExtraElements]
public class EventSeries
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("title")]
	public string Title { get; set; } = default!;

	[BsonElement("host")]
	public ParticipantInfo Host { get; set; } = new();

	[BsonElement("locationId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string LocationId { get; set; } = default!;

	[BsonElement("locationSnapshot")]
	public LocationSnapshot LocationSnapshot { get; set; } = new();

	[BsonElement("systemKeys")]
	public List<string> SystemKeys { get; set; } = new();

	[BsonElement("recurrenceKind")]
	public EventRecurrenceKind RecurrenceKind { get; set; } = EventRecurrenceKind.Weekly;

	[BsonElement("dayOfWeek")]
	public DayOfWeek DayOfWeek { get; set; }

	[BsonElement("timeLabel")]
	public string? TimeLabel { get; set; }

	[BsonElement("startHour")]
	public int StartHour { get; set; } = 18;

	[BsonElement("startDateUtc")]
	public DateTime? StartDateUtc { get; set; }

	[BsonElement("endDateUtc")]
	public DateTime? EndDateUtc { get; set; }

	[BsonElement("defaultMaxPlayers")]
	public int DefaultMaxPlayers { get; set; } = 2;

	[BsonElement("description")]
	public string? Description { get; set; }

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }
}
