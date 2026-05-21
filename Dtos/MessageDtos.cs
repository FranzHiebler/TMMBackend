using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class MessageRecipientRequest
{
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
}

public class SendDirectMessageRequest
{
	public string? ConversationId { get; set; }
	public List<MessageRecipientRequest> Recipients { get; set; } = new();
	public string Body { get; set; } = default!;
}

public class SendGameSessionMessageRequest
{
	public string Body { get; set; } = default!;
}

public class SendGameTableMessageRequest
{
	public string Body { get; set; } = default!;
}

public class MessageResponse
{
	public string Id { get; set; } = default!;
	public MessageKind Kind { get; set; }
	public string? ConversationId { get; set; }
	public string? GameId { get; set; }
	public string? TableId { get; set; }
	public ParticipantInfo Author { get; set; } = new();
	public string Body { get; set; } = default!;
	public DateTime CreatedAtUtc { get; set; }
	public bool IsMine { get; set; }
}

public class ConversationResponse
{
	public string Id { get; set; } = default!;
	public List<ParticipantInfo> Participants { get; set; } = new();
	public string? LastMessagePreview { get; set; }
	public DateTime? LastMessageAtUtc { get; set; }
	public int UnreadCount { get; set; }
}

public class ConversationDetailResponse : ConversationResponse
{
	public List<MessageResponse> Messages { get; set; } = new();
}

public class NotificationResponse
{
	public string Id { get; set; } = default!;
	public NotificationKind Kind { get; set; }
	public string Title { get; set; } = default!;
	public string Body { get; set; } = default!;
	public string? LinkUrl { get; set; }
	public bool IsRead { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}
