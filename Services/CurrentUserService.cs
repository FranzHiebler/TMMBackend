using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string UserId => GetRequiredHeader("x-user-id");

	public string DisplayName => GetRequiredHeader("x-display-name");

	private string GetRequiredHeader(string name)
	{
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