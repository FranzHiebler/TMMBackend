using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public class ParticipantInfo
{
	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;
}