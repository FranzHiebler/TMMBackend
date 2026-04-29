using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class GameSession
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("title")]
	public string Title { get; set; } = default!;

	[BsonElement("system")]
	public GameSystemInfo System { get; set; } = new();

	[BsonElement("host")]
	public ParticipantInfo Host { get; set; } = new();

	[BsonElement("participants")]
	public List<ParticipantInfo> Participants { get; set; } = new();

	[BsonElement("maxPlayers")]
	public int MaxPlayers { get; set; }

	[BsonElement("status")]
	public string Status { get; set; } = "Open";

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

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }

	[BsonElement("updatedAt")]
	public DateTime UpdatedAt { get; set; }
}

public class GameSystemInfo
{
	[BsonElement("key")]
	public string Key { get; set; } = default!;

	[BsonElement("name")]
	public string Name { get; set; } = default!;
}

public class ParticipantInfo
{
	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;
}

public class LocationSnapshot
{
	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("city")]
	public string City { get; set; } = default!;
}