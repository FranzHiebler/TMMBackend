namespace TabletopMatchMaker.Services;

public class GameActionException : Exception
{
	public GameActionException(string message) : base(message) { }
}