using Microsoft.Extensions.Options;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class AdminAuthorizationService : IAdminAuthorizationService
{
	private readonly ICurrentUserService _currentUser;
	private readonly IUserRepository _users;
	private readonly IWebHostEnvironment _environment;
	private readonly IAuthSessionService _authSession;
	private readonly AdminSettings _settings;

	public AdminAuthorizationService(
		ICurrentUserService currentUser,
		IUserRepository users,
		IWebHostEnvironment environment,
		IAuthSessionService authSession,
		IOptions<AdminSettings> settings)
	{
		_currentUser = currentUser;
		_users = users;
		_environment = environment;
		_authSession = authSession;
		_settings = settings.Value;
	}

	public async Task<bool> IsCurrentUserAdminAsync()
	{
		var session = _authSession.GetCurrentSession();
		var adminUserId = session?.RealUserId ?? _currentUser.UserId;
		var profile = await _users.GetByIdAsync(adminUserId);
		if (profile?.IsSystemAdmin == true)
			return true;

		return _environment.IsDevelopment()
			&& _settings.UserIds.Contains(adminUserId, StringComparer.OrdinalIgnoreCase);
	}

	public async Task EnsureCurrentUserIsAdminAsync()
	{
		if (!await IsCurrentUserAdminAsync())
			throw new UnauthorizedAccessException("Du darfst diese Aktion nicht ausführen.");
	}
}
