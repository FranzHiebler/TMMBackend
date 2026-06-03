using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Dtos;

public class FeedbackContextRequest
{
	public string? PageUrl { get; set; }
	public string? Pathname { get; set; }
	public string? Search { get; set; }
	public string? Hash { get; set; }
	public string? PageTitle { get; set; }
	public string? UserAgent { get; set; }
	public int? ViewportWidth { get; set; }
	public int? ViewportHeight { get; set; }
	public string? Referrer { get; set; }
}

public class CreateFeedbackRequest
{
	public FeedbackType Type { get; set; }
	public string Message { get; set; } = default!;
	public FeedbackContextRequest? Context { get; set; }
}

public class UpdateFeedbackAdminRequest
{
	public FeedbackStatus Status { get; set; }
	public string? AdminNote { get; set; }
}

public class FeedbackResponse
{
	public string Id { get; set; } = default!;
	public FeedbackType Type { get; set; }
	public string Message { get; set; } = default!;
	public string UserId { get; set; } = default!;
	public string DisplayName { get; set; } = default!;
	public string? PageUrl { get; set; }
	public string? Pathname { get; set; }
	public string? Search { get; set; }
	public string? Hash { get; set; }
	public string? PageTitle { get; set; }
	public string? UserAgent { get; set; }
	public int? ViewportWidth { get; set; }
	public int? ViewportHeight { get; set; }
	public string? Referrer { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public FeedbackStatus Status { get; set; }
	public string? AdminNote { get; set; }
	public DateTime? ResolvedAtUtc { get; set; }
}
