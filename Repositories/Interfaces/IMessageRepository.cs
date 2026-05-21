using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IMessageRepository
{
	Task<MessageThread?> GetThreadByIdAsync(string id);
	Task<MessageThread?> FindThreadByParticipantIdsAsync(List<string> participantIds);
	Task<List<MessageThread>> GetThreadsForUserAsync(string userId);
	Task CreateThreadAsync(MessageThread thread);
	Task UpdateThreadAsync(MessageThread thread);
	Task CreateMessageAsync(Message message);
	Task<List<Message>> GetConversationMessagesAsync(string conversationId);
	Task<List<Message>> GetGameMessagesAsync(string gameId);
	Task<List<Message>> GetTableMessagesAsync(string gameId, string tableId);
	Task<int> CountUnreadConversationMessagesAsync(string conversationId, string userId, DateTime? lastReadAtUtc);
}
