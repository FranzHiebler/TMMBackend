using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum MessageKind
{
	Direct,
	GameSession,
	GameTable
}

public class MessageParticipant
{
	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;

	[BsonElement("lastReadAtUtc")]
	public DateTime? LastReadAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class MessageThread
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("participants")]
	public List<MessageParticipant> Participants { get; set; } = new();

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }

	[BsonElement("updatedAtUtc")]
	public DateTime UpdatedAtUtc { get; set; }

	[BsonElement("lastMessagePreview")]
	public string? LastMessagePreview { get; set; }

	[BsonElement("lastMessageAtUtc")]
	public DateTime? LastMessageAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class Message
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("kind")]
	public MessageKind Kind { get; set; }

	[BsonElement("conversationId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? ConversationId { get; set; }

	[BsonElement("gameId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? GameId { get; set; }

	[BsonElement("tableId")]
	public string? TableId { get; set; }

	[BsonElement("author")]
	public ParticipantInfo Author { get; set; } = new();

	[BsonElement("body")]
	public string Body { get; set; } = default!;

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }
}
