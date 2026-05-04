namespace TMMBackend.Domain;

public enum GameSessionState
{
	Open,
	Full,
	Closed,
	Cancelled
}

public enum GameJoinMode
{
	ApprovalRequired,
	FirstComeFirstServe
}

public enum ApplicationStatus
{
	Pending,
	Accepted,
	Rejected,
	Withdrawn
}