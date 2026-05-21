using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class FriendRepository : IFriendRepository
{
	private readonly IMongoCollection<Friendship> _friendships;

	public FriendRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_friendships = database.GetCollection<Friendship>("friendships");
	}

	public async Task<List<Friendship>> GetAcceptedForUserAsync(string userId)
	{
		var filter = Builders<Friendship>.Filter.Eq(x => x.Status, FriendshipStatus.Accepted) &
			(Builders<Friendship>.Filter.Eq(x => x.RequesterUserId, userId) |
			 Builders<Friendship>.Filter.Eq(x => x.ReceiverUserId, userId));

		return await _friendships.Find(filter).SortByDescending(x => x.UpdatedAtUtc).ToListAsync();
	}

	public async Task<List<Friendship>> GetPendingRequestsForUserAsync(string userId)
	{
		return await _friendships
			.Find(x => x.ReceiverUserId == userId && x.Status == FriendshipStatus.Pending)
			.SortByDescending(x => x.CreatedAtUtc)
			.ToListAsync();
	}

	public async Task<Friendship?> GetByIdAsync(string id)
	{
		return await _friendships.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task<Friendship?> FindBetweenUsersAsync(string userAId, string userBId)
	{
		var filter =
			(Builders<Friendship>.Filter.Eq(x => x.RequesterUserId, userAId) &
			 Builders<Friendship>.Filter.Eq(x => x.ReceiverUserId, userBId)) |
			(Builders<Friendship>.Filter.Eq(x => x.RequesterUserId, userBId) &
			 Builders<Friendship>.Filter.Eq(x => x.ReceiverUserId, userAId));

		return await _friendships.Find(filter).FirstOrDefaultAsync();
	}

	public async Task CreateAsync(Friendship friendship)
	{
		await _friendships.InsertOneAsync(friendship);
	}

	public async Task UpdateAsync(Friendship friendship)
	{
		await _friendships.ReplaceOneAsync(x => x.Id == friendship.Id, friendship);
	}

	public async Task DeleteAsync(string id)
	{
		await _friendships.DeleteOneAsync(x => x.Id == id);
	}
}
