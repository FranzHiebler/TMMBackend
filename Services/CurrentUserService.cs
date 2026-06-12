using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IWebHostEnvironment _environment;

	public CurrentUserService(
		IHttpContextAccessor httpContextAccessor,
		IWebHostEnvironment environment)
	{
		_httpContextAccessor = httpContextAccessor;
		_environment = environment;
	}

	public string UserId => GetRequiredHeader("x-user-id");

	public string DisplayName => GetRequiredHeader("x-display-name");

	private string GetRequiredHeader(string name)
	{
		// Temporary auth boundary: test headers are allowed only for local Development.
		// Real cookie/session auth can replace this adapter without changing callers.
		if (!_environment.IsDevelopment())
			throw new UnauthorizedAccessException("Test-Header-Auth ist nur in Development erlaubt.");

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
