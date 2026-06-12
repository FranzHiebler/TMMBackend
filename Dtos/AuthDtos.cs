namespace TabletopMatchMaker.Dtos;

public class GoogleLoginRequest
{
	public string IdToken { get; set; } = default!;
}

public class AuthUserResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Email { get; set; }
	public bool IsSystemAdmin { get; set; }
	public bool RealUserIsSystemAdmin { get; set; }
	public bool IsDevUser { get; set; }
	public bool IsImpersonating { get; set; }
	public string? RealUserId { get; set; }
	public string? RealDisplayName { get; set; }
	public string? EffectiveUserId { get; set; }
	public string? EffectiveDisplayName { get; set; }
}

public class DevUserResponse
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Email { get; set; }
	public string? DefaultLocationId { get; set; }
	public string? Description { get; set; }
}

public class StartImpersonationRequest
{
	public string TargetUserId { get; set; } = default!;
}
