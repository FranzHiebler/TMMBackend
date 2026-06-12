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
	public bool IsDevUser { get; set; }
	public bool IsImpersonating { get; set; }
}
