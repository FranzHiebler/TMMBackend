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
	private readonly AdminSettings _settings;

	public AdminAuthorizationService(
		ICurrentUserService currentUser,
		IUserRepository users,
		IWebHostEnvironment environment,
		IOptions<AdminSettings> settings)
	{
		_currentUser = currentUser;
		_users = users;
		_environment = environment;
		_settings = settings.Value;
	}

	public async Task<bool> IsCurrentUserAdminAsync()
	{
		var profile = await _users.GetByIdAsync(_currentUser.UserId);
		if (profile?.IsSystemAdmin == true)
			return true;

		return _environment.IsDevelopment()
			&& _settings.UserIds.Contains(_currentUser.UserId, StringComparer.OrdinalIgnoreCase);
	}

	public async Task EnsureCurrentUserIsAdminAsync()
	{
		if (!await IsCurrentUserAdminAsync())
			throw new UnauthorizedAccessException("Du darfst diese Aktion nicht ausführen.");
	}
}
