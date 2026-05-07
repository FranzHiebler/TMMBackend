using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string UserId =>
		_httpContextAccessor.HttpContext?.Request.Headers["x-user-id"].FirstOrDefault()
		?? "64f1a2b3c4d5e6f7890abc12";

	public string DisplayName =>
		_httpContextAccessor.HttpContext?.Request.Headers["x-display-name"].FirstOrDefault()
		?? "Franz";
}