using Microsoft.Extensions.Options;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class AdminAuthorizationService : IAdminAuthorizationService
{
	private readonly ICurrentUserService _currentUser;
	private readonly AdminSettings _settings;

	public AdminAuthorizationService(
		ICurrentUserService currentUser,
		IOptions<AdminSettings> settings)
	{
		_currentUser = currentUser;
		_settings = settings.Value;
	}

	public bool IsCurrentUserAdmin()
	{
		return _settings.UserIds.Contains(_currentUser.UserId, StringComparer.OrdinalIgnoreCase);
	}

	public void EnsureCurrentUserIsAdmin()
	{
		if (!IsCurrentUserAdmin())
			throw new UnauthorizedAccessException("Du darfst diese Aktion nicht ausführen.");
	}
}