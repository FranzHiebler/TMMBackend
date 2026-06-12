using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
	private readonly IUserRepository _users;
	private readonly IAdminAuthorizationService _adminAuthorization;
	private readonly IAuthSessionService _sessions;

	public AdminController(
		IUserRepository users,
		IAdminAuthorizationService adminAuthorization,
		IAuthSessionService sessions)
	{
		_users = users;
		_adminAuthorization = adminAuthorization;
		_sessions = sessions;
	}

	[HttpGet("dev-users")]
	public async Task<ActionResult<List<DevUserResponse>>> GetDevUsers()
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		var users = await _users.GetDevUsersAsync();
		return Ok(users.Select(ToDevUserResponse).ToList());
	}

	[HttpPost("impersonation/start")]
	public async Task<ActionResult<AuthUserResponse>> StartImpersonation([FromBody] StartImpersonationRequest request)
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		var session = GetSessionOrThrow();
		var realUser = await GetRealAdminOrThrow(session);
		var targetUser = await _users.GetByIdAsync(request.TargetUserId);

		if (targetUser == null)
			throw new KeyNotFoundException("Dev-User wurde nicht gefunden.");

		if (!targetUser.IsDevUser)
			throw new UnauthorizedAccessException("Nur Dev-User dürfen impersonated werden.");

		session.EffectiveUserId = targetUser.Id!;
		session.DisplayName = targetUser.DisplayName;
		session.Email = targetUser.Email;
		session.IsSystemAdmin = realUser.IsSystemAdmin;
		session.IsDevUser = targetUser.IsDevUser;
		_sessions.SignIn(session);

		return Ok(AuthController.ToAuthResponse(targetUser, realUser, true));
	}

	[HttpPost("impersonation/stop")]
	public async Task<ActionResult<AuthUserResponse>> StopImpersonation()
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		var session = GetSessionOrThrow();
		var realUser = await GetRealAdminOrThrow(session);

		session.EffectiveUserId = realUser.Id!;
		session.DisplayName = realUser.DisplayName;
		session.Email = realUser.Email;
		session.IsSystemAdmin = realUser.IsSystemAdmin;
		session.IsDevUser = realUser.IsDevUser;
		_sessions.SignIn(session);

		return Ok(AuthController.ToAuthResponse(realUser, realUser));
	}

	private AuthSession GetSessionOrThrow()
	{
		return _sessions.GetCurrentSession()
			?? throw new AuthenticationRequiredException("Anmeldung erforderlich.");
	}

	private async Task<UserProfile> GetRealAdminOrThrow(AuthSession session)
	{
		var realUser = await _users.GetByIdAsync(session.RealUserId)
			?? throw new AuthenticationRequiredException("Session ist ungültig.");

		if (!realUser.IsSystemAdmin)
			throw new UnauthorizedAccessException("Nur Systemadmins dürfen Testansichten verwenden.");

		return realUser;
	}

	private static DevUserResponse ToDevUserResponse(UserProfile user)
	{
		return new DevUserResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			Email = user.Email,
			DefaultLocationId = user.DefaultLocationId,
			Description = user.City
		};
	}
}
