using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
	private readonly IMongoCollection<FeedbackItem> _feedback;

	public FeedbackRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_feedback = database.GetCollection<FeedbackItem>("feedbackItems");
	}

	public async Task CreateAsync(FeedbackItem item)
	{
		await _feedback.InsertOneAsync(item);
	}

	public async Task<FeedbackItem?> GetByIdAsync(string id)
	{
		return await _feedback.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task<List<FeedbackItem>> GetAllWithTicketNumbersAsync()
	{
		return await _feedback
			.Find(x => x.TicketNumber != null && x.TicketNumber != "")
			.Project<FeedbackItem>(
				Builders<FeedbackItem>.Projection
					.Include(x => x.Id)
					.Include(x => x.TicketNumber))
			.ToListAsync();
	}

	public async Task<List<FeedbackItem>> GetAdminListAsync(FeedbackStatus? status, FeedbackType? type)
	{
		var filter = Builders<FeedbackItem>.Filter.Empty;

		if (status.HasValue)
			filter &= Builders<FeedbackItem>.Filter.Eq(x => x.Status, status.Value);

		if (type.HasValue)
			filter &= Builders<FeedbackItem>.Filter.Eq(x => x.Type, type.Value);

		return await _feedback
			.Find(filter)
			.SortByDescending(x => x.CreatedAtUtc)
			.Limit(300)
			.ToListAsync();
	}

	public async Task UpdateAsync(FeedbackItem item)
	{
		await _feedback.ReplaceOneAsync(x => x.Id == item.Id, item);
	}
}
