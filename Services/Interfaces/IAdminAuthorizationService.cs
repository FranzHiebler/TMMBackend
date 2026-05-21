namespace TabletopMatchMaker.Services.Interfaces;

public interface IAdminAuthorizationService
{
	bool IsCurrentUserAdmin();
	void EnsureCurrentUserIsAdmin();
}