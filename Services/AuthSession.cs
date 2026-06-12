namespace TabletopMatchMaker.Services;

public class AuthSession
{
	public string RealUserId { get; set; } = default!;
	public string EffectiveUserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? Email { get; set; }
	public bool IsSystemAdmin { get; set; }
	public bool IsDevUser { get; set; }
	public bool IsImpersonating => RealUserId != EffectiveUserId;
	public DateTime ExpiresAtUtc { get; set; }
}
