using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class Location
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; } = default!;

	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("shortName")]
	public string? ShortName { get; set; }

	[BsonElement("city")]
	public string City { get; set; } = default!;

	[BsonElement("address")]
	public string? Address { get; set; }

	[BsonElement("geo")]
	public GeoJsonPoint<GeoJson2DGeographicCoordinates> Geo { get; set; } = default!;

	[BsonElement("members")]
	public List<LocationMember> Members { get; set; } = new();

	[BsonElement("accessMode")]
	public LocationAccessMode AccessMode { get; set; } = LocationAccessMode.MembersOnly;
}
