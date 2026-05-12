using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class LocationSnapshot
{
	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("city")]
	public string City { get; set; } = default!;
}