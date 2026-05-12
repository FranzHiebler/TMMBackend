using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

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

	[BsonElement("startTimeUtc")]
	public DateTime? StartTimeUtc { get; set; }

	[BsonElement("notes")]
	public string? Notes { get; set; }

	[BsonElement("assignedPlayers")]
	public List<ParticipantInfo> AssignedPlayers { get; set; } = new();

	[BsonElement("applications")]
	public List<TableApplication> Applications { get; set; } = new();
}