using MongoDB.Bson;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class FriendService : IFriendService
{
	private readonly IFriendRepository _repository;
	private readonly ICurrentUserService _currentUser;
	private readonly INotificationService _notifications;

	public FriendService(
		IFriendRepository repository,
		ICurrentUserService currentUser,
		INotificationService notifications)
	{
		_repository = repository;
		_currentUser = currentUser;
		_notifications = notifications;
	}

	public async Task<List<FriendResponse>> GetFriendsAsync()
	{
		var friendships = await _repository.GetAcceptedForUserAsync(_currentUser.UserId);
		return friendships.Select(MapFriend).ToList();
	}

	public async Task<List<FriendRequestResponse>> GetRequestsAsync()
	{
		var requests = await _repository.GetPendingRequestsForUserAsync(_currentUser.UserId);
		return requests.Select(MapRequest).ToList();
	}

	public async Task<FriendRequestResponse?> SendRequestAsync(SendFriendRequestRequest request)
	{
		var receiverUserId = request.ReceiverUserId.Trim();
		var receiverDisplayName = string.IsNullOrWhiteSpace(request.ReceiverDisplayName)
			? receiverUserId
			: request.ReceiverDisplayName.Trim();

		if (receiverUserId == _currentUser.UserId)
			throw new DomainException("Du kannst dich nicht selbst als Freund hinzufügen.");

		var existing = await _repository.FindBetweenUsersAsync(_currentUser.UserId, receiverUserId);
		if (existing != null)
		{
			if (existing.Status == FriendshipStatus.Accepted)
				return null;

			if (existing.Status == FriendshipStatus.Pending)
				return existing.ReceiverUserId == _currentUser.UserId ? MapRequest(existing) : null;

			existing.RequesterUserId = _currentUser.UserId;
			existing.RequesterDisplayName = _currentUser.DisplayName;
			existing.ReceiverUserId = receiverUserId;
			existing.ReceiverDisplayName = receiverDisplayName;
			existing.Status = FriendshipStatus.Pending;
			existing.UpdatedAtUtc = DateTime.UtcNow;
			await _repository.UpdateAsync(existing);
			await NotifyRequestAsync(existing);
			return null;
		}

		var now = DateTime.UtcNow;
		var friendship = new Friendship
		{
			Id = ObjectId.GenerateNewId().ToString(),
			RequesterUserId = _currentUser.UserId,
			RequesterDisplayName = _currentUser.DisplayName,
			ReceiverUserId = receiverUserId,
			ReceiverDisplayName = receiverDisplayName,
			Status = FriendshipStatus.Pending,
			CreatedAtUtc = now,
			UpdatedAtUtc = now
		};

		await _repository.CreateAsync(friendship);
		await NotifyRequestAsync(friendship);
		return null;
	}

	public async Task<FriendResponse> AcceptAsync(string id)
	{
		var friendship = await GetOrThrow(id);

		if (friendship.ReceiverUserId != _currentUser.UserId)
			throw new UnauthorizedAccessException("Nur der Empfänger darf diese Anfrage annehmen.");

		friendship.Status = FriendshipStatus.Accepted;
		friendship.UpdatedAtUtc = DateTime.UtcNow;
		await _repository.UpdateAsync(friendship);

		await _notifications.NotifyAsync(
			friendship.RequesterUserId,
			NotificationKind.FriendAccepted,
			"Freundschaft angenommen",
			$"{_currentUser.DisplayName} hat deine Anfrage angenommen.",
			"/friends");

		return MapFriend(friendship);
	}

	public async Task RejectAsync(string id)
	{
		var friendship = await GetOrThrow(id);

		if (friendship.ReceiverUserId != _currentUser.UserId)
			throw new UnauthorizedAccessException("Nur der Empfänger darf diese Anfrage ablehnen.");

		friendship.Status = FriendshipStatus.Rejected;
		friendship.UpdatedAtUtc = DateTime.UtcNow;
		await _repository.UpdateAsync(friendship);
	}

	public async Task DeleteAsync(string id)
	{
		var friendship = await GetOrThrow(id);

		if (friendship.RequesterUserId != _currentUser.UserId &&
			friendship.ReceiverUserId != _currentUser.UserId)
			throw new UnauthorizedAccessException("Du darfst diese Freundschaft nicht entfernen.");

		await _repository.DeleteAsync(id);
	}

	private async Task<Friendship> GetOrThrow(string id)
	{
		return await _repository.GetByIdAsync(id)
			?? throw new DomainException("Freundschaft nicht gefunden.");
	}

	private async Task NotifyRequestAsync(Friendship friendship)
	{
		await _notifications.NotifyAsync(
			friendship.ReceiverUserId,
			NotificationKind.FriendRequest,
			"Neue Freundschaftsanfrage",
			$"{friendship.RequesterDisplayName} möchte dich als Freund hinzufügen.",
			"/friends");
	}

	private FriendResponse MapFriend(Friendship friendship)
	{
		var isRequester = friendship.RequesterUserId == _currentUser.UserId;

		return new FriendResponse
		{
			Id = friendship.Id!,
			UserId = isRequester ? friendship.ReceiverUserId : friendship.RequesterUserId,
			DisplayName = isRequester ? friendship.ReceiverDisplayName : friendship.RequesterDisplayName,
			Status = friendship.Status,
			UpdatedAtUtc = friendship.UpdatedAtUtc
		};
	}

	private static FriendRequestResponse MapRequest(Friendship friendship)
	{
		return new FriendRequestResponse
		{
			Id = friendship.Id!,
			RequesterUserId = friendship.RequesterUserId,
			RequesterDisplayName = friendship.RequesterDisplayName,
			CreatedAtUtc = friendship.CreatedAtUtc
		};
	}
}
