namespace TabletopMatchMaker.Services.Interfaces;

public interface IAdminAuthorizationService
{
	Task<bool> IsCurrentUserAdminAsync();
	Task EnsureCurrentUserIsAdminAsync();
}
