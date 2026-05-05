using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TMMBackend.Domain;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class GameSession
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("title")]
	public string Title { get; set; } = default!;

	[BsonElement("host")]
	public ParticipantInfo Host { get; set; } = new();

	[BsonElement("status")]
	public GameSessionState Status { get; set; } = GameSessionState.Open;

	[BsonElement("joinMode")]
	public GameJoinMode JoinMode { get; set; } = GameJoinMode.ApprovalRequired;

	[BsonElement("locationId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string LocationId { get; set; } = default!;

	[BsonElement("locationSnapshot")]
	public LocationSnapshot LocationSnapshot { get; set; } = new();

	[BsonElement("clubId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? ClubId { get; set; }

	[BsonElement("startTimeUtc")]
	public DateTime StartTimeUtc { get; set; }

	[BsonElement("description")]
	public string? Description { get; set; }

	[BsonElement("tables")]
	public List<GameTable> Tables { get; set; } = new();

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }

	[BsonElement("updatedAt")]
	public DateTime UpdatedAt { get; set; }
}

[BsonIgnoreExtraElements]
public class GameTable
{
	[BsonElement("tableId")]
	public string TableId { get; set; } = default!;

	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("maxPlayers")]
	public int MaxPlayers { get; set; }

	[BsonElement("systems")]
	public List<string> Systems { get; set; } = new();

	[BsonElement("scenario")]
	public string? Scenario { get; set; }

	[BsonElement("points")]
	public int? Points { get; set; }

	[BsonElement("notes")]
	public string? Notes { get; set; }

	[BsonElement("assignedPlayers")]
	public List<ParticipantInfo> AssignedPlayers { get; set; } = new();

	[BsonElement("applications")]
	public List<TableApplication> Applications { get; set; } = new();
}

[BsonIgnoreExtraElements]
public class TableApplication
{
	[BsonElement("applicationId")]
	public string ApplicationId { get; set; } = default!;

	[BsonElement("tableId")]
	public string? TableId { get; set; }

	[BsonElement("player")]
	public ParticipantInfo Player { get; set; } = new();

	[BsonElement("systemKey")]
	public string? SystemKey { get; set; }

	[BsonElement("message")]
	public string? Message { get; set; }

	[BsonElement("status")]
	public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }
}

public class ParticipantInfo
{
	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;
}

[BsonIgnoreExtraElements]
public class LocationSnapshot
{
	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("city")]
	public string City { get; set; } = default!;
}