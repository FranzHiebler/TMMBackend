using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace TabletopMatchMaker.Domain;

[BsonIgnoreExtraElements]
public class Location
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("name")]
	public string Name { get; set; } = default!;

	[BsonElement("shortName")]
	public string? ShortName { get; set; }

	[BsonElement("city")]
	public string City { get; set; } = default!;

	[BsonElement("geo")]
	public GeoJsonPoint<GeoJson2DGeographicCoordinates> Geo { get; set; } = default!;
}

public class GeoPoint
{
	[BsonElement("type")]
	public string Type { get; set; } = "Point";

	[BsonElement("coordinates")]
	public List<double> Coordinates { get; set; } = new();
}