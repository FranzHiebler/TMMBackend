using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class AuthSessionService : IAuthSessionService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IDataProtector _protector;
	private readonly AuthSettings _settings;

	public AuthSessionService(
		IHttpContextAccessor httpContextAccessor,
		IDataProtectionProvider dataProtectionProvider,
		IOptions<AuthSettings> settings)
	{
		_httpContextAccessor = httpContextAccessor;
		_protector = dataProtectionProvider.CreateProtector("TabletopMatchmaker.AuthSession.v1");
		_settings = settings.Value;
	}

	public AuthSession? GetCurrentSession()
	{
		var context = _httpContextAccessor.HttpContext;
		var cookie = context?.Request.Cookies[_settings.CookieName];
		if (string.IsNullOrWhiteSpace(cookie))
			return null;

		try
		{
			var json = _protector.Unprotect(cookie);
			var session = JsonSerializer.Deserialize<AuthSession>(json);

			return session?.ExpiresAtUtc > DateTime.UtcNow ? session : null;
		}
		catch
		{
			return null;
		}
	}

	public void SignIn(AuthSession session)
	{
		var context = _httpContextAccessor.HttpContext
			?? throw new InvalidOperationException("Kein HTTP-Kontext verfügbar.");

		var json = JsonSerializer.Serialize(session);
		var cookie = _protector.Protect(json);

		context.Response.Cookies.Append(_settings.CookieName, cookie, new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			SameSite = SameSiteMode.None,
			Expires = session.ExpiresAtUtc,
			Path = "/"
		});
	}

	public void SignOut()
	{
		var context = _httpContextAccessor.HttpContext
			?? throw new InvalidOperationException("Kein HTTP-Kontext verfügbar.");

		context.Response.Cookies.Delete(_settings.CookieName, new CookieOptions
		{
			Secure = true,
			SameSite = SameSiteMode.None,
			Path = "/"
		});
	}
}
