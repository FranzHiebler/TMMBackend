using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IWebHostEnvironment _environment;
	private readonly IAuthSessionService _authSession;

	public CurrentUserService(
		IHttpContextAccessor httpContextAccessor,
		IWebHostEnvironment environment,
		IAuthSessionService authSession)
	{
		_httpContextAccessor = httpContextAccessor;
		_environment = environment;
		_authSession = authSession;
	}

	public string UserId => _authSession.GetCurrentSession()?.EffectiveUserId ?? GetRequiredDevelopmentHeader("x-user-id");

	public string DisplayName => _authSession.GetCurrentSession()?.DisplayName ?? GetRequiredDevelopmentHeader("x-display-name");

	private string GetRequiredDevelopmentHeader(string name)
	{
		// Temporary auth boundary: test headers are allowed only for local Development.
		// Real cookie/session auth can replace this adapter without changing callers.
		if (!_environment.IsDevelopment())
			throw new AuthenticationRequiredException("Anmeldung erforderlich.");

		var value = _httpContextAccessor
			.HttpContext?
			.Request
			.Headers[name]
			.FirstOrDefault();

		if (string.IsNullOrWhiteSpace(value))
			throw new UnauthorizedAccessException($"Header fehlt: {name}");

		return value;
	}
}
