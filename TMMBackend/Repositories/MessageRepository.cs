using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class MessageRepository : IMessageRepository
{
	private readonly IMongoCollection<MessageThread> _threads;
	private readonly IMongoCollection<Message> _messages;

	public MessageRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_threads = database.GetCollection<MessageThread>("messageThreads");
		_messages = database.GetCollection<Message>("messages");
	}

	public async Task<MessageThread?> GetThreadByIdAsync(string id)
	{
		return await _threads.Find(x => x.Id == id).FirstOrDefaultAsync();
	}

	public async Task<MessageThread?> FindThreadByParticipantIdsAsync(List<string> participantIds)
	{
		var normalized = participantIds.Distinct().OrderBy(x => x).ToList();
		var candidates = await _threads
			.Find(x => x.Participants.Count == normalized.Count)
			.ToListAsync();

		return candidates.FirstOrDefault(thread =>
			thread.Participants.Select(x => x.UserId).OrderBy(x => x).SequenceEqual(normalized));
	}

	public async Task<List<MessageThread>> GetThreadsForUserAsync(string userId)
	{
		return await _threads
			.Find(x => x.Participants.Any(p => p.UserId == userId))
			.SortByDescending(x => x.LastMessageAtUtc)
			.ToListAsync();
	}

	public async Task CreateThreadAsync(MessageThread thread)
	{
		await _threads.InsertOneAsync(thread);
	}

	public async Task UpdateThreadAsync(MessageThread thread)
	{
		await _threads.ReplaceOneAsync(x => x.Id == thread.Id, thread);
	}

	public async Task CreateMessageAsync(Message message)
	{
		await _messages.InsertOneAsync(message);
	}

	public async Task<List<Message>> GetConversationMessagesAsync(string conversationId)
	{
		return await _messages
			.Find(x => x.ConversationId == conversationId)
			.SortBy(x => x.CreatedAtUtc)
			.ToListAsync();
	}

	public async Task<List<Message>> GetGameMessagesAsync(string gameId)
	{
		return await _messages
			.Find(x => x.Kind == MessageKind.GameSession && x.GameId == gameId)
			.SortBy(x => x.CreatedAtUtc)
			.ToListAsync();
	}

	public async Task<List<Message>> GetTableMessagesAsync(string gameId, string tableId)
	{
		return await _messages
			.Find(x => x.Kind == MessageKind.GameTable && x.GameId == gameId && x.TableId == tableId)
			.SortBy(x => x.CreatedAtUtc)
			.ToListAsync();
	}

	public async Task<int> CountUnreadConversationMessagesAsync(
		string conversationId,
		string userId,
		DateTime? lastReadAtUtc)
	{
		var filter = Builders<Message>.Filter.Eq(x => x.ConversationId, conversationId) &
			Builders<Message>.Filter.Ne(x => x.Author.UserId, userId);

		if (lastReadAtUtc.HasValue)
			filter &= Builders<Message>.Filter.Gt(x => x.CreatedAtUtc, lastReadAtUtc.Value);

		return (int)await _messages.CountDocumentsAsync(filter);
	}
}
