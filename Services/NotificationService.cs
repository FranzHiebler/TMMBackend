using MongoDB.Bson;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class NotificationService : INotificationService
{
	private readonly INotificationRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public NotificationService(
		INotificationRepository repository,
		ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	public async Task<List<NotificationResponse>> GetMineAsync()
	{
		var notifications = await _repository.GetForUserAsync(_currentUser.UserId);
		return notifications.Select(Map).ToList();
	}

	public async Task MarkReadAsync(string id)
	{
		var notification = await _repository.GetByIdAsync(id)
			?? throw new DomainException("Benachrichtigung nicht gefunden.");

		if (notification.UserId != _currentUser.UserId)
			throw new UnauthorizedAccessException("Du darfst diese Benachrichtigung nicht lesen.");

		notification.IsRead = true;
		await _repository.UpdateAsync(notification);
	}

	public async Task MarkAllReadAsync()
	{
		await _repository.MarkAllReadAsync(_currentUser.UserId);
	}

	public async Task NotifyAsync(
		string userId,
		NotificationKind kind,
		string title,
		string body,
		string? linkUrl)
	{
		await _repository.CreateAsync(Create(userId, kind, title, body, linkUrl));
	}

	public async Task NotifyManyAsync(
		IEnumerable<string> userIds,
		NotificationKind kind,
		string title,
		string body,
		string? linkUrl)
	{
		var notifications = userIds
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Distinct()
			.Select(userId => Create(userId, kind, title, body, linkUrl))
			.ToList();

		await _repository.CreateManyAsync(notifications);
	}

	private static Notification Create(
		string userId,
		NotificationKind kind,
		string title,
		string body,
		string? linkUrl)
	{
		return new Notification
		{
			Id = ObjectId.GenerateNewId().ToString(),
			UserId = userId,
			Kind = kind,
			Title = title,
			Body = body,
			LinkUrl = linkUrl,
			IsRead = false,
			CreatedAtUtc = DateTime.UtcNow
		};
	}

	private static NotificationResponse Map(Notification notification)
	{
		return new NotificationResponse
		{
			Id = notification.Id!,
			Kind = notification.Kind,
			Title = notification.Title,
			Body = notification.Body,
			LinkUrl = notification.LinkUrl,
			IsRead = notification.IsRead,
			CreatedAtUtc = notification.CreatedAtUtc
		};
	}
}
