using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

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