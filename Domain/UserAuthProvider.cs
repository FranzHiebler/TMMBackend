using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class UserAuthProvider
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = string.Empty;

	[BsonElement("provider")]
	public string Provider { get; set; } = string.Empty;

	[BsonElement("providerUserId")]
	public string ProviderUserId { get; set; } = string.Empty;

	[BsonElement("email")]
	public string Email { get; set; } = string.Empty;

	[BsonElement("linkedAtUtc")]
	public DateTime LinkedAtUtc { get; set; }

	[BsonElement("lastLoginAtUtc")]
	public DateTime LastLoginAtUtc { get; set; }
}
