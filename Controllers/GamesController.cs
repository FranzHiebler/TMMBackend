using Microsoft.AspNetCore.Mvc;
using System.Text;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
	private readonly IGameService _service;
	private readonly IDiscoveryService _discovery;

	public GamesController(
		IGameService service,
		IDiscoveryService discovery)
	{
		_service = service;
		_discovery = discovery;
	}

	[HttpPost]
	public async Task<ActionResult<GameResponse>> Create(CreateGameRequest request)
	{
		var result = await _service.CreateAsync(request);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<GameResponse>> GetById(string id)
	{
		var game = await _service.GetByIdAsync(id);

		if (game == null)
			return NotFound(new { error = "Game Session wurde nicht gefunden." });

		return Ok(game);
	}

	[HttpGet("{id}/calendar.ics")]
	public async Task<IActionResult> CalendarIcs(string id)
	{
		var game = await _service.GetByIdAsync(id);

		if (game == null)
			return NotFound(new { error = "Game Session wurde nicht gefunden." });

		if (game.TimingMode == Domain.SessionTimingMode.Open)
			return BadRequest(new { error = "Dieser Spieltermin hat noch kein festes Datum." });

		var ics = BuildCalendarIcs(game, BuildFrontendSessionUrl(game.Id));
		var fileName = $"{SanitizeFileName(game.Title)}.ics";

		return File(Encoding.UTF8.GetBytes(ics), "text/calendar; charset=utf-8", fileName);
	}

	[HttpGet("public/{slugOrId}")]
	public async Task<ActionResult<PublicGameResponse>> GetPublic(string slugOrId)
	{
		var game = await _service.GetPublicAsync(slugOrId);
		return game == null ? NotFound(new { error = "Session wurde nicht gefunden." }) : Ok(game);
	}

	[HttpGet("calendar")]
	public async Task<ActionResult<List<CalendarItemResponse>>> Calendar()
	{
		return Ok(await _service.GetCalendarAsync());
	}

	private string BuildFrontendSessionUrl(string gameId)
	{
		var baseUrl =
			HttpContext.RequestServices.GetService<IConfiguration>()?["Frontend:BaseUrl"] ??
			Environment.GetEnvironmentVariable("Frontend__BaseUrl");

		if (string.IsNullOrWhiteSpace(baseUrl))
			baseUrl = "http://localhost:5173";

		return $"{baseUrl.TrimEnd('/')}/sessions/{Uri.EscapeDataString(gameId)}";
	}

	private static string BuildCalendarIcs(GameResponse game, string sessionUrl)
	{
		var startUtc = DateTime.SpecifyKind(game.StartTimeUtc, DateTimeKind.Utc);
		var endUtc = startUtc.AddHours(3);
		var location = string.Join(", ", new[] { game.Location.Name, game.Location.City }
			.Where(value => !string.IsNullOrWhiteSpace(value)));
		var tableSummary = string.Join(" | ", game.Tables.Select(table =>
		{
			var systems = table.Systems.Count == 0 ? "System offen" : string.Join(", ", table.Systems);
			var points = table.Points.HasValue ? $" · {table.Points} Punkte" : "";
			return $"{table.Name}: {systems}{points}";
		}));
		var description = string.Join("\\n", new[]
		{
			game.Description,
			tableSummary,
			sessionUrl
		}.Where(value => !string.IsNullOrWhiteSpace(value)));
		var now = DateTime.UtcNow;

		return string.Join("\r\n", new[]
		{
			"BEGIN:VCALENDAR",
			"VERSION:2.0",
			"PRODID:-//Tabletop Matchmaker//Spieltermin//DE",
			"CALSCALE:GREGORIAN",
			"METHOD:PUBLISH",
			"BEGIN:VEVENT",
			$"UID:{EscapeIcsText(game.Id)}@tabletop-matchmaker",
			$"DTSTAMP:{FormatIcsUtc(now)}",
			$"DTSTART:{FormatIcsUtc(startUtc)}",
			$"DTEND:{FormatIcsUtc(endUtc)}",
			$"SUMMARY:{EscapeIcsText(game.Title)}",
			$"DESCRIPTION:{EscapeIcsText(description)}",
			$"LOCATION:{EscapeIcsText(location)}",
			$"URL:{EscapeIcsText(sessionUrl)}",
			$"ORGANIZER;CN={EscapeIcsText(game.Host.DisplayName)}:MAILTO:no-reply@tabletop-matchmaker.local",
			"END:VEVENT",
			"END:VCALENDAR",
			""
		});
	}

	private static string FormatIcsUtc(DateTime value)
	{
		return value.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'");
	}

	private static string EscapeIcsText(string? value)
	{
		return (value ?? "")
			.Replace("\\", "\\\\")
			.Replace(";", "\\;")
			.Replace(",", "\\,")
			.Replace("\r\n", "\\n")
			.Replace("\n", "\\n")
			.Replace("\r", "\\n");
	}

	private static string SanitizeFileName(string value)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray()).Trim();
		return string.IsNullOrWhiteSpace(cleaned) ? "spieltermin" : cleaned;
	}

	[HttpGet("search")]
	public async Task<ActionResult<List<GameResponse>>> Search([FromQuery] SearchGamesRequest request)
	{
		return Ok(await _service.SearchAsync(request));
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<GameResponse>>> Nearby([FromQuery] SearchNearbyGamesRequest request)
	{
		return Ok(await _service.SearchNearbyAsync(request));
	}

	[HttpGet("discovery")]
	public async Task<ActionResult<List<GameDiscoveryResponse>>> Discovery([FromQuery] DiscoveryGamesRequest request)
	{
		return Ok(await _discovery.GetGamesAsync(request));
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<GameResponse>> UpdateSession(
		string id,
		[FromBody] UpdateGameSessionRequest request)
	{
		return Ok(await _service.UpdateSessionAsync(id, request));
	}

	[HttpPut("{id}/tables/{tableId}")]
	public async Task<ActionResult<GameResponse>> UpdateTable(
		string id,
		string tableId,
		[FromBody] UpdateGameTableRequest request)
	{
		return Ok(await _service.UpdateTableAsync(id, tableId, request));
	}
}
