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
			Title = request.Title.Trim(),
			Host = new ParticipantInfo { UserId = _currentUser.UserId, DisplayName = _currentUser.DisplayName },
			LocationId = request.LocationId,
			LocationSnapshot = new LocationSnapshot { Name = location.Name, City = location.City },
			SystemKeys = request.SystemKeys.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToList(),
			RecurrenceKind = request.RecurrenceKind,
			DayOfWeek = request.DayOfWeek,
			TimeLabel = string.IsNullOrWhiteSpace(request.TimeLabel) ? null : request.TimeLabel.Trim(),
			StartHour = Math.Clamp(request.StartHour, 0, 23),
			DefaultMaxPlayers = Math.Clamp(request.DefaultMaxPlayers, 1, 20),
			Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
			CreatedAtUtc = DateTime.UtcNow
		};

		await _repository.CreateAsync(series);
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
			DefaultMaxPlayers = series.DefaultMaxPlayers,
			Description = series.Description,
			UpcomingStartTimesUtc = Upcoming(series).Take(6).ToList()
		};
	}

	private static IEnumerable<DateTime> Upcoming(EventSeries series)
	{
		var cursor = DateTime.UtcNow.Date.AddHours(series.StartHour);
		while (cursor.DayOfWeek != series.DayOfWeek || cursor <= DateTime.UtcNow)
			cursor = cursor.AddDays(1);

		for (var i = 0; i < 12; i++)
		{
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
