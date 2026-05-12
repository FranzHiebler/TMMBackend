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

	[BsonElement("changeProposals")]
	public List<GameChangeProposal> ChangeProposals { get; set; } = new();

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }

	[BsonElement("updatedAt")]
	public DateTime UpdatedAt { get; set; }
}