using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IMessageService
{
	Task<List<ConversationResponse>> GetConversationsAsync();
	Task<ConversationDetailResponse> GetConversationAsync(string conversationId);
	Task<MessageResponse> SendDirectAsync(SendDirectMessageRequest request);
	Task MarkConversationReadAsync(string conversationId);
	Task<List<MessageResponse>> GetGameMessagesAsync(string gameId);
	Task<MessageResponse> SendGameMessageAsync(string gameId, SendGameSessionMessageRequest request);
	Task<List<MessageResponse>> GetTableMessagesAsync(string gameId, string tableId);
	Task<MessageResponse> SendTableMessageAsync(string gameId, string tableId, SendGameTableMessageRequest request);
}
