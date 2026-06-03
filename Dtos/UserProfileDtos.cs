using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class UserProfileResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? StreetAddress { get; set; }
	public string? PostalCode { get; set; }
	public string? City { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? TabletopTo { get; set; }
	public string? TabletopHerald { get; set; }
	public string? T3 { get; set; }
	public string? NewRecruit { get; set; }
	public string? BestSportsPairings { get; set; }
	public string? ProfileImageUrl { get; set; }
	public string? DefaultLocationId { get; set; }
	public bool CanBeContacted { get; set; }
	public bool HideProfile { get; set; }
	public bool HideOnMap { get; set; }
	public bool HideParticipation { get; set; }
	public UserProfileVisibilityDto Visibility { get; set; } = new();
	public List<string> FavoriteSystemKeys { get; set; } = new();
	public List<UserArmyProfileDto> Armies { get; set; } = new();
	public LookingForGameStatusDto LookingForGame { get; set; } = new();
	public UserDiscoverySettingsDto DiscoverySettings { get; set; } = new();
}

public class UpdateUserProfileRequest
{
	public string DisplayName { get; set; } = default!;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? StreetAddress { get; set; }
	public string? PostalCode { get; set; }
	public string? City { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? TabletopTo { get; set; }
	public string? TabletopHerald { get; set; }
	public string? T3 { get; set; }
	public string? NewRecruit { get; set; }
	public string? BestSportsPairings { get; set; }
	public string? ProfileImageUrl { get; set; }
	public string? DefaultLocationId { get; set; }
	public bool CanBeContacted { get; set; } = true;
	public bool HideProfile { get; set; }
	public bool HideOnMap { get; set; }
	public bool HideParticipation { get; set; }
	public UserProfileVisibilityDto Visibility { get; set; } = new();
	public List<string>? FavoriteSystemKeys { get; set; } = new();
	public List<UserArmyProfileDto>? Armies { get; set; } = new();
	public LookingForGameStatusDto? LookingForGame { get; set; } = new();
	public UserDiscoverySettingsDto? DiscoverySettings { get; set; }
}

public class UserDiscoverySettingsDto
{
	public bool ShowLocations { get; set; } = true;
	public bool ShowPlayers { get; set; } = true;
	public bool ShowMySessions { get; set; } = true;
	public bool ShowPublicSessions { get; set; } = true;
	public int TimeWindowDays { get; set; } = 7;
	public int RadiusKm { get; set; } = 80;
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int Zoom { get; set; } = 10;
}

public class UserPermissionsResponse
{
	public bool IsAdmin { get; set; }
}

public class TestUserOptionResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}

public class UserArmyProfileDto
{
	public string SystemKey { get; set; } = default!;
	public string ArmyName { get; set; } = default!;
}

public class LookingForGameStatusDto
{
	public bool IsActive { get; set; }
	public string? SystemKey { get; set; }
	public int? RadiusKm { get; set; }
	public string? TimeNote { get; set; }
	public DateTime? UpdatedAtUtc { get; set; }
}
public class UserProfileVisibilityDto
{
	public ProfileFieldVisibility Email { get; set; } = ProfileFieldVisibility.Private;
	public ProfileFieldVisibility PhoneNumber { get; set; } = ProfileFieldVisibility.Private;
	public ProfileFieldVisibility StreetAddress { get; set; } = ProfileFieldVisibility.Private;
	public ProfileFieldVisibility PostalCode { get; set; } = ProfileFieldVisibility.Private;
	public ProfileFieldVisibility City { get; set; } = ProfileFieldVisibility.Private;
	public ProfileFieldVisibility TabletopTo { get; set; } = ProfileFieldVisibility.Public;
	public ProfileFieldVisibility TabletopHerald { get; set; } = ProfileFieldVisibility.Public;
	public ProfileFieldVisibility T3 { get; set; } = ProfileFieldVisibility.Public;
	public ProfileFieldVisibility NewRecruit { get; set; } = ProfileFieldVisibility.Public;
	public ProfileFieldVisibility BestSportsPairings { get; set; } = ProfileFieldVisibility.Public;
}

public class PublicUserProfileResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? StreetAddress { get; set; }
	public string? PostalCode { get; set; }
	public string? City { get; set; }
	public string? TabletopTo { get; set; }
	public string? TabletopHerald { get; set; }
	public string? T3 { get; set; }
	public string? NewRecruit { get; set; }
	public string? BestSportsPairings { get; set; }
	public string? ProfileImageUrl { get; set; }
	public bool CanBeContacted { get; set; }
	public bool HideProfile { get; set; }
	public bool IsFriend { get; set; }
	public List<string> FavoriteSystemKeys { get; set; } = new();
	public List<UserArmyProfileDto> Armies { get; set; } = new();
	public LookingForGameStatusDto LookingForGame { get; set; } = new();

	public List<string> HiddenFields { get; set; } = new();
}
