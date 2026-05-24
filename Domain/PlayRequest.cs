using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum PlayRequestStatus
{
	Open,
	Converted,
	Closed
}

[BsonIgnoreExtraElements]
public class PlayRequest
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("owner")]
	public ParticipantInfo Owner { get; set; } = new();

	[BsonElement("systemKey")]
	public string SystemKey { get; set; } = default!;

	[BsonElement("locationId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? LocationId { get; set; }

	[BsonElement("locationName")]
	public string? LocationName { get; set; }

	[BsonElement("city")]
	public string? City { get; set; }

	[BsonElement("latitude")]
	public double? Latitude { get; set; }

	[BsonElement("longitude")]
	public double? Longitude { get; set; }

	[BsonElement("timeNote")]
	public string? TimeNote { get; set; }

	[BsonElement("exactTimeUtc")]
	public DateTime? ExactTimeUtc { get; set; }

	[BsonElement("radiusKm")]
	public int? RadiusKm { get; set; }

	[BsonElement("note")]
	public string? Note { get; set; }

	[BsonElement("status")]
	public PlayRequestStatus Status { get; set; } = PlayRequestStatus.Open;

	[BsonElement("convertedGameId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? ConvertedGameId { get; set; }

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }

	[BsonElement("updatedAtUtc")]
	public DateTime UpdatedAtUtc { get; set; }
}
