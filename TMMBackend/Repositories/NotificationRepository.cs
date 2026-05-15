using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class NotificationRepository : INotificationRepository
{
	private readonly IMongoCollection<Notification> _notifications;

	public NotificationRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_notifications = database.GetCollection<Notification>("notifications");
		_notifications.Indexes.CreateOne(new CreateIndexModel<Notification>(
			Builders<Notification>.IndexKeys
				.Ascending(x => x.UserId)
				.Ascending(x => x.IsRead)
				.Descending(x => x.CreatedAtUtc)));
	}

	public async Task CreateAsync(Notification notification)
	{
		await _notifications.InsertOneAsync(notification);
	}

	public async Task CreateManyAsync(List<Notification> notifications)
	{
		if (notifications.Count == 0) return;
		await _notifications.InsertManyAsync(notifications);
	}

	public async Task<List<Notification>> GetForUserAsync(string userId)
	{
		return await _notifications
			.Find(x => x.UserId == userId)
			.SortByDescending(x => x.CreatedAtUtc)
			.Limit(100)
			.ToListAsync();
	}

	public async Task<Notification?> GetByIdAsync(string id)
	{
		return await _notifications.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task UpdateAsync(Notification notification)
	{
		await _notifications.ReplaceOneAsync(x => x.Id == notification.Id, notification);
	}

	public async Task MarkAllReadAsync(string userId)
	{
		var update = Builders<Notification>.Update.Set(x => x.IsRead, true);
		await _notifications.UpdateManyAsync(x => x.UserId == userId && !x.IsRead, update);
	}
}
