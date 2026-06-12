using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class UserAuthProvider
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("provider")]
	public string Provider { get; set; } = default!;

	[BsonElement("providerUserId")]
	public string ProviderUserId { get; set; } = default!;

	[BsonElement("email")]
	public string Email { get; set; } = default!;

	[BsonElement("linkedAtUtc")]
	public DateTime LinkedAtUtc { get; set; }

	[BsonElement("lastLoginAtUtc")]
	public DateTime LastLoginAtUtc { get; set; }
}
