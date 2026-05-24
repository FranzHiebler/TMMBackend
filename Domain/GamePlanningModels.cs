using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum SessionTimingMode
{
	Fixed,
	Rough,
	Open
}

public enum SessionInvitationStatus
{
	Pending,
	Accepted,
	Rejected
}

public enum GameResultKind
{
	Matrix20,
	Matrix6,
	Score,
	FreeText
}

[BsonIgnoreExtraElements]
public class SessionDateOption
{
	[BsonElement("id")]
	public string Id { get; set; } = default!;

	[BsonElement("startTimeUtc")]
	public DateTime StartTimeUtc { get; set; }

	[BsonElement("label")]
	public string? Label { get; set; }

	[BsonElement("votes")]
	public List<ParticipantInfo> Votes { get; set; } = new();

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class SessionInvitation
{
	[BsonElement("id")]
	public string Id { get; set; } = default!;

	[BsonElement("user")]
	public ParticipantInfo User { get; set; } = new();

	[BsonElement("status")]
	public SessionInvitationStatus Status { get; set; } = SessionInvitationStatus.Pending;

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }

	[BsonElement("respondedAtUtc")]
	public DateTime? RespondedAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class WaitlistEntry
{
	[BsonElement("id")]
	public string Id { get; set; } = default!;

	[BsonElement("tableId")]
	public string? TableId { get; set; }

	[BsonElement("player")]
	public ParticipantInfo Player { get; set; } = new();

	[BsonElement("systemKey")]
	public string? SystemKey { get; set; }

	[BsonElement("message")]
	public string? Message { get; set; }

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class GameResult
{
	[BsonElement("kind")]
	public GameResultKind Kind { get; set; } = GameResultKind.FreeText;

	[BsonElement("value")]
	public string Value { get; set; } = default!;

	[BsonElement("notes")]
	public string? Notes { get; set; }

	[BsonElement("recordedBy")]
	public ParticipantInfo RecordedBy { get; set; } = new();

	[BsonElement("recordedAtUtc")]
	public DateTime RecordedAtUtc { get; set; }
}
