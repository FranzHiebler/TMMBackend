using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameSessionAuthorizationService : IGameSessionAuthorizationService
{
	private readonly ILocationLookupService _locationService;
	private readonly ICurrentUserService _currentUser;

	public GameSessionAuthorizationService(
		ILocationLookupService locationService,
		ICurrentUserService currentUser)
	{
		_locationService = locationService;
		_currentUser = currentUser;
	}

	public bool CanCreateGameAtLocation(Location location)
	{
		var hasUsableRole = location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			m.Role != LocationRole.Applicant);

		return location.AccessMode == LocationAccessMode.Open || hasUsableRole;
	}

	public async Task<bool> CanManageSessionAsync(GameSession game)
	{
		if (game.Host.UserId == _currentUser.UserId)
			return true;

		var location = await _locationService.GetByIdAsync(game.LocationId);
		if (location == null) return false;

		return location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			(m.Role == LocationRole.Owner ||
			 m.Role == LocationRole.Admin ||
			 m.Role == LocationRole.Manager));
	}
}