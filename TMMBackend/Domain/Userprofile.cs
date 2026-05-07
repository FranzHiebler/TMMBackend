using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class UserProfile
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;

	[BsonElement("email")]
	public string? Email { get; set; }
}