using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class SystemDefinition
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("key")]
	public string Key { get; set; } = default!;

	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("shortCode")]
	public string? ShortCode { get; set; }

	[BsonElement("color")]
	public string? Color { get; set; }

	[BsonElement("markerColor")]
	public string? MarkerColor { get; set; }
}