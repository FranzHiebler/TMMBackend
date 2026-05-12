using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class GameSessionAuthorizationService : IGameSessionAuthorizationService
{
	private readonly LocationRepository _locationRepository;
	private readonly ICurrentUserService _currentUser;

	public GameSessionAuthorizationService(
		LocationRepository locationRepository,
		ICurrentUserService currentUser)
	{
		_locationRepository = locationRepository;
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

		var location = await _locationRepository.GetByIdAsync(game.LocationId);
		if (location == null) return false;

		return location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			(m.Role == LocationRole.Owner || m.Role == LocationRole.Manager));
	}
}