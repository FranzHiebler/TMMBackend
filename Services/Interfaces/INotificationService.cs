using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface INotificationService
{
	Task<List<NotificationResponse>> GetMineAsync();
	Task MarkReadAsync(string id);
	Task MarkAllReadAsync();
	Task NotifyAsync(string userId, NotificationKind kind, string title, string body, string? linkUrl);
	Task NotifyManyAsync(IEnumerable<string> userIds, NotificationKind kind, string title, string body, string? linkUrl);
}
