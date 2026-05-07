using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TMMBackend.Domain;

[BsonIgnoreExtraElements]
public class LocationMember
{
	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? UserId { get; set; }

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;

	[BsonElement("role")]
	public LocationRole Role { get; set; }
}