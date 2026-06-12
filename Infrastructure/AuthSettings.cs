namespace TabletopMatchMaker.Infrastructure;

public class AuthSettings
{
	public string GoogleClientId { get; set; } = "";
	public string CookieName { get; set; } = "tmm_session";
	public int SessionDays { get; set; } = 14;
	public string SystemAdminEmail { get; set; } = "joseffranzhiebler@googlemail.com";
}
