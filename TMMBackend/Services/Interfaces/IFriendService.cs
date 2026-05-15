using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IFriendService
{
	Task<List<FriendResponse>> GetFriendsAsync();
	Task<List<FriendRequestResponse>> GetRequestsAsync();
	Task<FriendRequestResponse?> SendRequestAsync(SendFriendRequestRequest request);
	Task<FriendResponse> AcceptAsync(string id);
	Task RejectAsync(string id);
	Task DeleteAsync(string id);
}
