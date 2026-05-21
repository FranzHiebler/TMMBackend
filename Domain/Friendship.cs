using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum FriendshipStatus
{
	Pending,
	Accepted,
	Rejected,
	Blocked
}

[BsonIgnoreExtraElements]
public class Friendship
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("requesterUserId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string RequesterUserId { get; set; } = default!;

	[BsonElement("requesterDisplayName")]
	public string RequesterDisplayName { get; set; } = default!;

	[BsonElement("receiverUserId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string ReceiverUserId { get; set; } = default!;

	[BsonElement("receiverDisplayName")]
	public string ReceiverDisplayName { get; set; } = default!;

	[BsonElement("status")]
	public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }

	[BsonElement("updatedAtUtc")]
	public DateTime UpdatedAtUtc { get; set; }
}
