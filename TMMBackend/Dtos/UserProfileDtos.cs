namespace TabletopMatchMaker.Dtos;

public class UserProfileResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Email { get; set; }
	public string? DefaultLocationId { get; set; }
}

public class UpdateUserProfileRequest
{
	public string DisplayName { get; set; } = default!;
	public string? DefaultLocationId { get; set; }
}