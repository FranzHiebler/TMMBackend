using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class EventSeriesService : IEventSeriesService
{
	private readonly IEventSeriesRepository _repository;
	private readonly ILocationLookupService _locations;
	private readonly IGameService _games;
	private readonly ICurrentUserService _currentUser;
	private readonly IGameSessionAuthorizationService _authorization;

	public EventSeriesService(
		IEventSeriesRepository repository,
		ILocationLookupService locations,
		IGameService games,
		ICurrentUserService currentUser,
		IGameSessionAuthorizationService authorization)
	{
		_repository = repository;
		_locations = locations;
		_games = games;
		_currentUser = currentUser;
		_authorization = authorization;
	}

	public async Task<List<EventSeriesResponse>> GetAllAsync()
	{
		var all = await _repository.GetAllAsync();
		return all.Select(Map).ToList();
	}

	public async Task<EventSeriesResponse> CreateAsync(CreateEventSeriesRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Title))
			throw new DomainException("Titel ist erforderlich.");
		var location = await _locations.GetByIdAsync(request.LocationId)
			?? throw new DomainException("Spielort wurde nicht gefunden.");
		if (!_authorization.CanCreateGameAtLocation(location))
			throw new DomainException("Du darfst an diesem Spielort keine Serie erstellen.");

		var series = new EventSeries
		{
			Host = new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName },
			CreatedAtUtc = DateTime.UtcNow
		};

		Apply(series, request, location);
		await _repository.CreateAsync(series);
		return Map(series);
	}

	public async Task<EventSeriesResponse> UpdateAsync(string id, CreateEventSeriesRequest request)
	{
		var series = await _repository.GetByIdAsync(id)
			?? throw new DomainException("Event-Serie wurde nicht gefunden.");
		if (series.Host.UserId != _currentUser.UserId)
			throw new DomainException("Nur der Host kann diese Serie bearbeiten.");

		if (string.IsNullOrWhiteSpace(request.Title))
			throw new DomainException("Titel ist erforderlich.");
		var location = await _locations.GetByIdAsync(request.LocationId)
			?? throw new DomainException("Spielort wurde nicht gefunden.");
		if (!_authorization.CanCreateGameAtLocation(location))
			throw new DomainException("Du darfst an diesem Spielort keine Serie verwalten.");

		Apply(series, request, location);
		await _repository.UpdateAsync(series);
		return Map(series);
	}

	public async Task<GameResponse> CreateNextSessionAsync(string seriesId)
	{
		var series = await _repository.GetByIdAsync(seriesId)
			?? throw new DomainException("Event-Serie wurde nicht gefunden.");
		if (series.Host.UserId != _currentUser.UserId)
			throw new DomainException("Nur der Host kann daraus eine Session erzeugen.");

		var next = Upcoming(series).First();
		return await _games.CreateAsync(new CreateGameRequest
		{
			Title = series.Title,
			LocationId = series.LocationId,
			StartTimeUtc = next,
			TimingMode = SessionTimingMode.Rough,
			TimeLabel = series.TimeLabel,
			Description = series.Description,
			Tables = new List<CreateGameTableRequest>
			{
				new()
				{
					Name = "Tisch 1",
					MaxPlayers = series.DefaultMaxPlayers,
					Systems = series.SystemKeys.Count == 0 ? new List<string> { "egal" } : series.SystemKeys
				}
			}
		});
	}

	private EventSeriesResponse Map(EventSeries series)
	{
		return new EventSeriesResponse
		{
			Id = series.Id!,
			Title = series.Title,
			Host = new ParticipantDto { UserId = series.Host.UserId, DisplayName = series.Host.DisplayName },
			LocationId = series.LocationId,
			Location = new LocationSnapshotDto { Name = series.LocationSnapshot.Name, City = series.LocationSnapshot.City },
			SystemKeys = series.SystemKeys,
			RecurrenceKind = series.RecurrenceKind,
			DayOfWeek = series.DayOfWeek,
			TimeLabel = series.TimeLabel,
			StartHour = series.StartHour,
			StartDateUtc = series.StartDateUtc,
			EndDateUtc = series.EndDateUtc,
			DefaultMaxPlayers = series.DefaultMaxPlayers,
			Description = series.Description,
			UpcomingStartTimesUtc = Upcoming(series).Take(6).ToList()
		};
	}

	private static void Apply(EventSeries series, CreateEventSeriesRequest request, Location location)
	{
		if (request.EndDateUtc.HasValue && request.StartDateUtc.HasValue && request.EndDateUtc.Value < request.StartDateUtc.Value)
			throw new DomainException("Enddatum darf nicht vor dem Startdatum liegen.");

		series.Title = request.Title.Trim();
		series.LocationId = request.LocationId;
		series.LocationSnapshot = new LocationSnapshot { Name = location.Name, City = location.City };
		series.SystemKeys = request.SystemKeys.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList();
		series.RecurrenceKind = request.RecurrenceKind;
		series.DayOfWeek = request.DayOfWeek;
		series.TimeLabel = string.IsNullOrWhiteSpace(request.TimeLabel) ? null : request.TimeLabel.Trim();
		series.StartHour = Math.Clamp(request.StartHour, 0, 23);
		series.StartDateUtc = request.StartDateUtc;
		series.EndDateUtc = request.EndDateUtc;
		series.DefaultMaxPlayers = Math.Clamp(request.DefaultMaxPlayers, 1, 20);
		series.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
	}

	private static IEnumerable<DateTime> Upcoming(EventSeries series)
	{
		var floor = series.StartDateUtc?.Date ?? DateTime.UtcNow.Date;
		var cursor = floor.AddHours(series.StartHour);
		while (cursor.DayOfWeek != series.DayOfWeek || cursor <= DateTime.UtcNow)
			cursor = cursor.AddDays(1);

		for (var i = 0; i < 12; i++)
		{
			if (series.EndDateUtc.HasValue && cursor.Date > series.EndDateUtc.Value.Date)
				yield break;
			yield return cursor;
			cursor = series.RecurrenceKind switch
			{
				EventRecurrenceKind.BiWeekly => cursor.AddDays(14),
				EventRecurrenceKind.MonthlyFirstWeekday => FirstWeekdayOfNextMonth(cursor, series.DayOfWeek).AddHours(series.StartHour),
				_ => cursor.AddDays(7)
			};
		}
	}

	private static DateTime FirstWeekdayOfNextMonth(DateTime current, DayOfWeek day)
	{
		var next = new DateTime(current.Year, current.Month, 1).AddMonths(1);
		while (next.DayOfWeek != day)
			next = next.AddDays(1);
		return next.Date;
	}
}
