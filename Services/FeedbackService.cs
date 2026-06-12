using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class FeedbackService : IFeedbackService
{
	private const int MaxMessageLength = 1000;
	private const int MaxAdminNoteLength = 2000;
	private const int MaxReporterNameLength = 120;
	private const int MaxContextLength = 500;
	private const int MaxUserAgentLength = 600;

	private readonly IFeedbackRepository _repository;
	private readonly ICurrentUserService _currentUser;
	private readonly IAdminAuthorizationService _adminAuthorization;

	public FeedbackService(
		IFeedbackRepository repository,
		ICurrentUserService currentUser,
		IAdminAuthorizationService adminAuthorization)
	{
		_repository = repository;
		_currentUser = currentUser;
		_adminAuthorization = adminAuthorization;
	}

	public async Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request)
	{
		var message = NormalizeRequired(request.Message, MaxMessageLength, "Feedback");
		var ticketNumber = await CreateNextTicketNumberAsync();

		var item = new FeedbackItem
		{
			Type = request.Type,
			TicketNumber = ticketNumber,
			Message = message,
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName,
			ReporterName = NormalizeOptional(request.ReporterName, MaxReporterNameLength),
			PageUrl = NormalizeOptional(request.Context?.PageUrl, MaxContextLength),
			Pathname = NormalizeOptional(request.Context?.Pathname, MaxContextLength),
			Search = NormalizeOptional(request.Context?.Search, MaxContextLength),
			Hash = NormalizeOptional(request.Context?.Hash, MaxContextLength),
			PageTitle = NormalizeOptional(request.Context?.PageTitle, MaxContextLength),
			UserAgent = NormalizeOptional(request.Context?.UserAgent, MaxUserAgentLength),
			ViewportWidth = NormalizeViewport(request.Context?.ViewportWidth),
			ViewportHeight = NormalizeViewport(request.Context?.ViewportHeight),
			Referrer = NormalizeOptional(request.Context?.Referrer, MaxContextLength),
			CreatedAtUtc = DateTime.UtcNow,
			Status = FeedbackStatus.Open
		};

		await _repository.CreateAsync(item);
		return ToResponse(item);
	}

	public async Task<List<FeedbackResponse>> GetAdminListAsync(FeedbackStatus? status, FeedbackType? type)
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		var items = await _repository.GetAdminListAsync(status, type);
		return items.Select(ToResponse).ToList();
	}

	public async Task<FeedbackResponse> UpdateAdminAsync(string id, UpdateFeedbackAdminRequest request)
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();

		var item = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Feedback wurde nicht gefunden.");

		item.Status = request.Status;
		item.AdminNote = NormalizeOptional(request.AdminNote, MaxAdminNoteLength);
		item.ResolvedAtUtc = request.Status is FeedbackStatus.Done or FeedbackStatus.Ignored
			? DateTime.UtcNow
			: null;

		await _repository.UpdateAsync(item);
		return ToResponse(item);
	}

	private static string NormalizeRequired(string? value, int maxLength, string label)
	{
		var normalized = NormalizeOptional(value, maxLength);
		if (string.IsNullOrWhiteSpace(normalized))
			throw new DomainException($"{label} darf nicht leer sein.");

		return normalized;
	}

	private static string? NormalizeOptional(string? value, int maxLength)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		var trimmed = value.Trim();
		return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
	}

	private static int? NormalizeViewport(int? value)
	{
		if (!value.HasValue || value.Value <= 0 || value.Value > 10000)
			return null;

		return value.Value;
	}

	private async Task<string> CreateNextTicketNumberAsync()
	{
		var items = await _repository.GetAllWithTicketNumbersAsync();
		var maxNumber = items
			.Select(item => ParseTicketNumber(item.TicketNumber))
			.DefaultIfEmpty(0)
			.Max();

		return $"FB-{maxNumber + 1:0000}";
	}

	private static int ParseTicketNumber(string? ticketNumber)
	{
		if (string.IsNullOrWhiteSpace(ticketNumber))
			return 0;

		var normalized = ticketNumber.Trim();
		if (!normalized.StartsWith("FB-", StringComparison.OrdinalIgnoreCase))
			return 0;

		return int.TryParse(normalized[3..], out var number) ? number : 0;
	}

	private static FeedbackResponse ToResponse(FeedbackItem item)
	{
		return new FeedbackResponse
		{
			Id = item.Id ?? "",
			TicketNumber = item.TicketNumber,
			Type = item.Type,
			Message = item.Message,
			UserId = item.UserId,
			DisplayName = item.DisplayName,
			ReporterName = item.ReporterName,
			PageUrl = item.PageUrl,
			Pathname = item.Pathname,
			Search = item.Search,
			Hash = item.Hash,
			PageTitle = item.PageTitle,
			UserAgent = item.UserAgent,
			ViewportWidth = item.ViewportWidth,
			ViewportHeight = item.ViewportHeight,
			Referrer = item.Referrer,
			CreatedAtUtc = item.CreatedAtUtc,
			Status = item.Status,
			AdminNote = item.AdminNote,
			ResolvedAtUtc = item.ResolvedAtUtc
		};
	}
}
