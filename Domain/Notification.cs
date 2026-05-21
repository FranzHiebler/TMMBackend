using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum NotificationKind
{
	DirectMessage,
	GameSessionMessage,
	GameTableMessage,
	ApplicationAccepted,
	ApplicationRejected,
	FriendRequest,
	FriendAccepted
}

[BsonIgnoreExtraElements]
public class Notification
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("kind")]
	public NotificationKind Kind { get; set; }

	[BsonElement("title")]
	public string Title { get; set; } = default!;

	[BsonElement("body")]
	public string Body { get; set; } = default!;

	[BsonElement("linkUrl")]
	public string? LinkUrl { get; set; }

	[BsonElement("isRead")]
	public bool IsRead { get; set; }

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }
}
