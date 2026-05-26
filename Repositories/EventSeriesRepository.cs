using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class EventSeriesRepository : IEventSeriesRepository
{
	private readonly IMongoCollection<EventSeries> _series;

	public EventSeriesRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_series = database.GetCollection<EventSeries>("eventSeries");
	}

	public Task CreateAsync(EventSeries series) => _series.InsertOneAsync(series);

	public Task UpdateAsync(EventSeries series) =>
		_series.ReplaceOneAsync(x => x.Id == series.Id, series);

	public async Task<EventSeries?> GetByIdAsync(string id) =>
		await _series.Find(x => x.Id == id).FirstOrDefaultAsync();

	public Task<List<EventSeries>> GetAllAsync() =>
		_series.Find(Builders<EventSeries>.Filter.Empty)
			.SortBy(x => x.Title)
			.ToListAsync();
}
