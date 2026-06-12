using TabletopMatchMaker.Services;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IAuthSessionService
{
	AuthSession? GetCurrentSession();
	void SignIn(AuthSession session);
	void SignOut();
}
