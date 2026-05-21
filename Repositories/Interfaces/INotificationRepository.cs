using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface INotificationRepository
{
	Task CreateAsync(Notification notification);
	Task CreateManyAsync(List<Notification> notifications);
	Task<List<Notification>> GetForUserAsync(string userId);
	Task<Notification?> GetByIdAsync(string id);
	Task UpdateAsync(Notification notification);
	Task MarkAllReadAsync(string userId);
}
