namespace TabletopMatchMaker.Services.Interfaces
{
	public interface ICurrentUserService
	{
		string UserId { get; }
		string DisplayName { get; }
		bool CanSeeDevData { get; }
	}
}
