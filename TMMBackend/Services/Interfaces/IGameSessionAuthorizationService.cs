using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameSessionAuthorizationService
{
	Task<bool> CanManageSessionAsync(GameSession game);
	bool CanCreateGameAtLocation(Location location);
}