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

	[BsonElement("firstName")]
	public string? FirstName { get; set; }

	[BsonElement("lastName")]
	public string? LastName { get; set; }

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

	[BsonElement("latitude")]
	public double? Latitude { get; set; }

	[BsonElement("longitude")]
	public double? Longitude { get; set; }

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

	[BsonElement("isSystemAdmin")]
	public bool IsSystemAdmin { get; set; }

	[BsonElement("isDevUser")]
	public bool IsDevUser { get; set; }

	[BsonElement("hideProfile")]
	public bool HideProfile { get; set; }

	[BsonElement("hideOnMap")]
	public bool HideOnMap { get; set; }

	[BsonElement("hideParticipation")]
	public bool HideParticipation { get; set; }

	[BsonElement("visibility")]
	public UserProfileVisibility Visibility { get; set; } = new();

	[BsonElement("favoriteSystemKeys")]
	public List<string> FavoriteSystemKeys { get; set; } = new();

	[BsonElement("armies")]
	public List<UserArmyProfile> Armies { get; set; } = new();

	[BsonElement("lookingForGame")]
	public LookingForGameStatus LookingForGame { get; set; } = new();

	[BsonElement("discoverySettings")]
	public UserDiscoverySettings DiscoverySettings { get; set; } = new();
}

[BsonIgnoreExtraElements]
public class UserArmyProfile
{
	[BsonElement("systemKey")]
	public string SystemKey { get; set; } = default!;

	[BsonElement("armyName")]
	public string ArmyName { get; set; } = default!;
}

[BsonIgnoreExtraElements]
public class LookingForGameStatus
{
	[BsonElement("isActive")]
	public bool IsActive { get; set; }

	[BsonElement("systemKey")]
	public string? SystemKey { get; set; }

	[BsonElement("radiusKm")]
	public int? RadiusKm { get; set; }

	[BsonElement("timeNote")]
	public string? TimeNote { get; set; }

	[BsonElement("updatedAtUtc")]
	public DateTime? UpdatedAtUtc { get; set; }
}

[BsonIgnoreExtraElements]
public class UserDiscoverySettings
{
	[BsonElement("showLocations")]
	public bool ShowLocations { get; set; } = true;

	[BsonElement("showPlayers")]
	public bool ShowPlayers { get; set; } = true;

	[BsonElement("showMySessions")]
	public bool ShowMySessions { get; set; } = true;

	[BsonElement("showPublicSessions")]
	public bool ShowPublicSessions { get; set; } = true;

	[BsonElement("timeWindowDays")]
	public int TimeWindowDays { get; set; } = 7;

	[BsonElement("radiusKm")]
	public int RadiusKm { get; set; } = 80;

	[BsonElement("latitude")]
	public double? Latitude { get; set; }

	[BsonElement("longitude")]
	public double? Longitude { get; set; }

	[BsonElement("zoom")]
	public int Zoom { get; set; } = 10;
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
