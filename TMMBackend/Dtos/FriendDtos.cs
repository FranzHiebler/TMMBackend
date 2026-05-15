using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class SendFriendRequestRequest
{
	public string ReceiverUserId { get; set; } = default!;
	public string ReceiverDisplayName { get; set; } = default!;
}

public class FriendResponse
{
	public string Id { get; set; } = default!;
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public FriendshipStatus Status { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
}

public class FriendRequestResponse
{
	public string Id { get; set; } = default!;
	public string RequesterUserId { get; set; } = default!;
	public string RequesterDisplayName { get; set; } = default!;
	public DateTime CreatedAtUtc { get; set; }
}
