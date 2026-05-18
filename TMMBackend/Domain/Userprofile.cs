using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum ProfileFieldVisibility
{
	Public,
	FriendsOnly,
	Private
}

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

	[BsonElement("phoneNumber")]
	public string? PhoneNumber { get; set; }

	[BsonElement("streetAddress")]
	public string? StreetAddress { get; set; }

	[BsonElement("postalCode")]
	public string? PostalCode { get; set; }

	[BsonElement("city")]
	public string? City { get; set; }

	[BsonElement("tabletopTo")]
	public string? TabletopTo { get; set; }

	[BsonElement("tabletopHerald")]
	public string? TabletopHerald { get; set; }

	[BsonElement("t3")]
	public string? T3 { get; set; }

	[BsonElement("newRecruit")]
	public string? NewRecruit { get; set; }

	[BsonElement("bestSportsPairings")]
	public string? BestSportsPairings { get; set; }

	[BsonElement("profileImageUrl")]
	public string? ProfileImageUrl { get; set; }

	[BsonElement("defaultLocationId")]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? DefaultLocationId { get; set; }

	[BsonElement("canBeContacted")]
	public bool CanBeContacted { get; set; } = true;

	[BsonElement("visibility")]
	public UserProfileVisibility Visibility { get; set; } = new();
}

[BsonIgnoreExtraElements]
public class UserProfileVisibility
{
	[BsonElement("email")]
	public ProfileFieldVisibility Email { get; set; } = ProfileFieldVisibility.Private;

	[BsonElement("phoneNumber")]
	public ProfileFieldVisibility PhoneNumber { get; set; } = ProfileFieldVisibility.Private;

	[BsonElement("streetAddress")]
	public ProfileFieldVisibility StreetAddress { get; set; } = ProfileFieldVisibility.Private;

	[BsonElement("postalCode")]
	public ProfileFieldVisibility PostalCode { get; set; } = ProfileFieldVisibility.Private;

	[BsonElement("city")]
	public ProfileFieldVisibility City { get; set; } = ProfileFieldVisibility.Private;

	[BsonElement("tabletopTo")]
	public ProfileFieldVisibility TabletopTo { get; set; } = ProfileFieldVisibility.Public;

	[BsonElement("tabletopHerald")]
	public ProfileFieldVisibility TabletopHerald { get; set; } = ProfileFieldVisibility.Public;

	[BsonElement("t3")]
	public ProfileFieldVisibility T3 { get; set; } = ProfileFieldVisibility.Public;

	[BsonElement("newRecruit")]
	public ProfileFieldVisibility NewRecruit { get; set; } = ProfileFieldVisibility.Public;

	[BsonElement("bestSportsPairings")]
	public ProfileFieldVisibility BestSportsPairings { get; set; } = ProfileFieldVisibility.Public;
}