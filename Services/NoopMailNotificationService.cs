using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class NoopMailNotificationService : IMailNotificationService
{
	public Task QueueAsync(string userId, NotificationKind kind, string title, string body, string? linkUrl)
	{
		// SMTP/provider integration belongs here once real Auth and verified email addresses exist.
		return Task.CompletedTask;
	}
}
