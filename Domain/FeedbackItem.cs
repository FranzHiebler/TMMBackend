using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TabletopMatchMaker.Domain;

public enum FeedbackType
{
	Info,
	Suggestion,
	Bug
}

public enum FeedbackStatus
{
	Open,
	InProgress,
	Done,
	Ignored
}

[BsonIgnoreExtraElements]
public class FeedbackItem
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string? Id { get; set; }

	[BsonElement("type")]
	public FeedbackType Type { get; set; }

	[BsonElement("message")]
	public string Message { get; set; } = default!;

	[BsonElement("userId")]
	public string UserId { get; set; } = default!;

	[BsonElement("displayName")]
	public string DisplayName { get; set; } = default!;

	[BsonElement("pageUrl")]
	public string? PageUrl { get; set; }

	[BsonElement("pathname")]
	public string? Pathname { get; set; }

	[BsonElement("search")]
	public string? Search { get; set; }

	[BsonElement("hash")]
	public string? Hash { get; set; }

	[BsonElement("pageTitle")]
	public string? PageTitle { get; set; }

	[BsonElement("userAgent")]
	public string? UserAgent { get; set; }

	[BsonElement("viewportWidth")]
	public int? ViewportWidth { get; set; }

	[BsonElement("viewportHeight")]
	public int? ViewportHeight { get; set; }

	[BsonElement("referrer")]
	public string? Referrer { get; set; }

	[BsonElement("createdAtUtc")]
	public DateTime CreatedAtUtc { get; set; }

	[BsonElement("status")]
	public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;

	[BsonElement("adminNote")]
	public string? AdminNote { get; set; }

	[BsonElement("resolvedAtUtc")]
	public DateTime? ResolvedAtUtc { get; set; }
}
