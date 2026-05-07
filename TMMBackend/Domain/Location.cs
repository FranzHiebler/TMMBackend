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

	[BsonElement("systemKeys")]
	public List<string> SystemKeys { get; set; } = new();

	[BsonElement("joinRequests")]
	public List<LocationJoinRequest> JoinRequests { get; set; } = new();

	[BsonElement("accessMode")]
	public LocationAccessMode AccessMode { get; set; } = LocationAccessMode.MembersOnly;
}

[BsonIgnoreExtraElements]
public class LocationJoinRequest
{
	[BsonElement("requestId")]
	public string RequestId { get; set; } = default!;

	[BsonElement("userId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;

	[BsonElement("message")]
	public string? Message { get; set; }

	[BsonElement("status")]
	public LocationJoinRequestStatus Status { get; set; } = LocationJoinRequestStatus.Pending;

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }
}
