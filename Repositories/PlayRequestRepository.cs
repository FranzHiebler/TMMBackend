using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class PlayRequestRepository : IPlayRequestRepository
{
	private readonly IMongoCollection<PlayRequest> _requests;

	public PlayRequestRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_requests = database.GetCollection<PlayRequest>("playRequests");
	}

	public Task CreateAsync(PlayRequest request) => _requests.InsertOneAsync(request);

	public async Task<PlayRequest?> GetByIdAsync(string id) =>
		await _requests.Find(x => x.Id == id).FirstOrDefaultAsync();

	public Task<List<PlayRequest>> GetOpenAsync() =>
		_requests.Find(x => x.Status == PlayRequestStatus.Open)
			.SortByDescending(x => x.UpdatedAtUtc)
			.ToListAsync();

	public Task<List<PlayRequest>> GetForUserAsync(string userId) =>
		_requests.Find(x => x.Owner.UserId == userId)
			.SortByDescending(x => x.UpdatedAtUtc)
			.ToListAsync();

	public Task UpdateAsync(PlayRequest request) =>
		_requests.ReplaceOneAsync(x => x.Id == request.Id, request);
}
