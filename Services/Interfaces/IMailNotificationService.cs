using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IMailNotificationService
{
	Task QueueAsync(string userId, NotificationKind kind, string title, string body, string? linkUrl);
}
