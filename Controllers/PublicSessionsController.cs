using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
public class PublicSessionsController : ControllerBase
{
	private const string DefaultOgImagePath = "/share/default-session-og.png";
	private readonly IGameService _games;
	private readonly IConfiguration _configuration;

	public PublicSessionsController(IGameService games, IConfiguration configuration)
	{
		_games = games;
		_configuration = configuration;
	}

	[HttpGet("s/{slugOrId}")]
	public async Task<IActionResult> PublicSessionHtml(string slugOrId)
	{
		var game = await _games.GetPublicAsync(slugOrId);
		if (game == null)
			return NotFound("Session wurde nicht gefunden.");

		var url = $"{GetPublicOrigin()}{Request.Path}";
		var imageUrl = GetShareImageUrl();
		var title = WebUtility.HtmlEncode(game.Title);
		var description = WebUtility.HtmlEncode(BuildDescription(game));
		var encodedUrl = WebUtility.HtmlEncode(url);
		var encodedImageUrl = WebUtility.HtmlEncode(imageUrl);
		var detailUrl = BuildFrontendSessionUrl(game.Id);
		var encodedDetailUrl = WebUtility.HtmlEncode(detailUrl);
		var timeLabel = WebUtility.HtmlEncode(BuildTimeLabel(game));
		var locationLabel = WebUtility.HtmlEncode(BuildLocationLabel(game));
		var seatLabel = WebUtility.HtmlEncode(BuildSeatLabel(game));
		var systemsHtml = BuildSystemsHtml(game);
		var tablesHtml = BuildTablesHtml(game);
		var participantLabel = WebUtility.HtmlEncode(BuildParticipantLabel(game));
		var descriptionHtml = string.IsNullOrWhiteSpace(game.Description)
			? "<p class=\"muted\">Weitere Details findest du direkt in Tabletop Matchmaker.</p>"
			: $"<p>{WebUtility.HtmlEncode(game.Description)}</p>";

		var html = $$"""
<!doctype html>
<html lang="de">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{title}}</title>
  <link rel="canonical" href="{{encodedUrl}}">
  <meta property="og:title" content="{{title}}">
  <meta property="og:description" content="{{description}}">
  <meta property="og:url" content="{{encodedUrl}}">
  <meta property="og:type" content="website">
  <meta property="og:image" content="{{encodedImageUrl}}">
  <meta property="og:image:width" content="1200">
  <meta property="og:image:height" content="630">
  <meta name="twitter:card" content="summary_large_image">
  <meta name="twitter:title" content="{{title}}">
  <meta name="twitter:description" content="{{description}}">
  <meta name="twitter:image" content="{{encodedImageUrl}}">
  <style>
    :root {
      color: #172033;
      background: #eef3f6;
      font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      min-height: 100vh;
      background: radial-gradient(circle at 18% 8%, rgba(37, 99, 235, 0.16), transparent 28rem),
        radial-gradient(circle at 90% 12%, rgba(20, 184, 166, 0.18), transparent 26rem),
        linear-gradient(180deg, #f8fbfc 0%, #e7edf2 100%);
    }
    main {
      width: min(940px, calc(100% - 28px));
      margin: 0 auto;
      padding: 22px 0 30px;
    }
    .hero {
      overflow: hidden;
      border: 1px solid rgba(148, 163, 184, 0.28);
      border-radius: 26px;
      background: rgba(255, 255, 255, 0.88);
      box-shadow: 0 24px 70px rgba(15, 23, 42, 0.16);
    }
    .hero-top {
      padding: 22px;
      color: #ffffff;
      background: linear-gradient(135deg, #172033 0%, #1f766f 100%);
    }
    .brand {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 34px;
      font-weight: 900;
    }
    .brand-mark {
      display: grid;
      width: 42px;
      height: 42px;
      place-items: center;
      border-radius: 13px;
      background: #ffffff;
      color: #172033;
      font-weight: 950;
    }
    .kicker {
      margin: 0 0 8px;
      color: rgba(255, 255, 255, 0.76);
      font-size: 13px;
      font-weight: 850;
      text-transform: uppercase;
    }
    h1 {
      max-width: 760px;
      margin: 0;
      font-size: clamp(30px, 7vw, 58px);
      line-height: 0.98;
      letter-spacing: 0;
    }
    .hero-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 18px;
    }
    .pill {
      border: 1px solid rgba(255, 255, 255, 0.24);
      border-radius: 999px;
      padding: 8px 11px;
      background: rgba(255, 255, 255, 0.12);
      font-size: 14px;
      font-weight: 850;
    }
    .hero-body {
      display: grid;
      gap: 18px;
      padding: 20px;
    }
    .action-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 14px;
      flex-wrap: wrap;
    }
    .primary-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 48px;
      border-radius: 16px;
      padding: 0 18px;
      background: #172033;
      color: #ffffff;
      font-weight: 950;
      text-decoration: none;
    }
    .summary {
      color: #475569;
      font-size: 14px;
      font-weight: 750;
    }
    .grid {
      display: grid;
      grid-template-columns: 1.1fr 0.9fr;
      gap: 16px;
    }
    .panel {
      border: 1px solid #e2e8f0;
      border-radius: 18px;
      padding: 16px;
      background: #ffffff;
    }
    .panel h2 {
      margin: 0 0 10px;
      font-size: 16px;
    }
    .muted {
      color: #64748b;
    }
    .system-list {
      display: flex;
      flex-wrap: wrap;
      gap: 7px;
      margin: 0 0 12px;
      padding: 0;
      list-style: none;
    }
    .system-list li {
      border-radius: 999px;
      padding: 7px 10px;
      background: #eef2ff;
      color: #1e3a8a;
      font-size: 13px;
      font-weight: 900;
    }
    .table-list {
      display: grid;
      gap: 10px;
    }
    .table-item {
      display: grid;
      gap: 4px;
      border-top: 1px solid #e2e8f0;
      padding-top: 10px;
    }
    .table-item:first-child {
      border-top: 0;
      padding-top: 0;
    }
    .table-item span {
      color: #475569;
      font-size: 14px;
      font-weight: 750;
    }
    footer {
      padding: 18px 4px 0;
      color: #64748b;
      font-size: 13px;
      text-align: center;
    }
    @media (max-width: 720px) {
      main { width: min(100% - 18px, 940px); padding-top: 10px; }
      .hero { border-radius: 20px; }
      .hero-top { padding: 18px; }
      .brand { margin-bottom: 24px; }
      .hero-body { padding: 14px; }
      .grid { grid-template-columns: 1fr; }
      .action-row { align-items: stretch; }
      .primary-action { width: 100%; }
    }
  </style>
</head>
<body>
  <main>
    <article class="hero">
      <section class="hero-top">
        <div class="brand">
          <span class="brand-mark">TMM</span>
          <span>Tabletop Matchmaker</span>
        </div>
        <p class="kicker">Öffentlicher Spieltermin</p>
        <h1>{{title}}</h1>
        <div class="hero-meta">
          <span class="pill">{{timeLabel}}</span>
          <span class="pill">{{locationLabel}}</span>
          <span class="pill">{{seatLabel}}</span>
        </div>
      </section>
      <section class="hero-body">
        <div class="action-row">
          <div class="summary">{{description}}</div>
          <a class="primary-action" href="{{encodedDetailUrl}}">In Tabletop Matchmaker öffnen</a>
        </div>
        <div class="grid">
          <section class="panel">
            <h2>Spielinfos</h2>
            {{systemsHtml}}
            {{descriptionHtml}}
          </section>
          <section class="panel">
            <h2>Spielort</h2>
            <p><strong>{{locationLabel}}</strong></p>
            <p class="muted">{{timeLabel}}</p>
            <p class="muted">{{participantLabel}}</p>
          </section>
        </div>
        <section class="panel">
          <h2>Tische</h2>
          {{tablesHtml}}
        </section>
      </section>
    </article>
    <footer>Geteilt über Tabletop Matchmaker. Teilnehmerdetails werden öffentlich nicht angezeigt.</footer>
  </main>
</body>
</html>
""";

		return Content(html, "text/html", Encoding.UTF8);
	}

	private string BuildFrontendSessionUrl(string gameId)
	{
		var baseUrl =
			_configuration["Frontend:BaseUrl"] ??
			Environment.GetEnvironmentVariable("Frontend__BaseUrl");

		if (string.IsNullOrWhiteSpace(baseUrl))
			baseUrl = "http://localhost:5173";

		return $"{baseUrl.TrimEnd('/')}/sessions/{WebUtility.UrlEncode(gameId)}";
	}

	private string GetShareImageUrl()
	{
		var configuredUrl = Environment.GetEnvironmentVariable("TMM_DEFAULT_OG_IMAGE_URL");
		if (!string.IsNullOrWhiteSpace(configuredUrl))
			return configuredUrl.Trim();

		return $"{GetPublicOrigin()}{DefaultOgImagePath}";
	}

	private string GetPublicOrigin()
	{
		var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
		var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
		var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? Request.Scheme : forwardedProto;
		var host = string.IsNullOrWhiteSpace(forwardedHost) ? Request.Host.Value : forwardedHost;

		return $"{scheme}://{host}";
	}

	private static string BuildDescription(PublicGameResponse game)
	{
		var where = BuildLocationLabel(game);
		var when = BuildTimeLabel(game);
		var seats = BuildSeatLabel(game);

		return string.Join(" · ", new[] { where, when, seats }
			.Where(value => !string.IsNullOrWhiteSpace(value)));
	}

	private static string BuildLocationLabel(PublicGameResponse game)
	{
		return string.Join(", ", new[] { game.Location.Name, game.Location.City }
			.Where(value => !string.IsNullOrWhiteSpace(value)));
	}

	private static string BuildSeatLabel(PublicGameResponse game)
	{
		var maxPlayers = game.Tables.Sum(table => table.MaxPlayers);
		var assignedPlayers = game.Tables.Sum(table => table.AssignedPlayers.Count);

		return maxPlayers > 0
			? $"{assignedPlayers}/{maxPlayers} Plätze belegt"
			: "Plätze offen";
	}

	private static string BuildParticipantLabel(PublicGameResponse game)
	{
		var assignedPlayers = game.Tables.Sum(table => table.AssignedPlayers.Count);
		var maxPlayers = game.Tables.Sum(table => table.MaxPlayers);

		return maxPlayers > 0
			? $"{assignedPlayers} von {maxPlayers} Plätzen sind belegt."
			: "Teilnehmerzahl offen.";
	}

	private static string BuildSystemsHtml(PublicGameResponse game)
	{
		var systems = game.Tables
			.SelectMany(table => table.Systems)
			.Where(system => !string.IsNullOrWhiteSpace(system))
			.Select(system => system.Trim())
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (systems.Count == 0)
			return "<p class=\"muted\">System offen.</p>";

		var items = systems.Select(system => $"<li>{WebUtility.HtmlEncode(system)}</li>");
		return $"<ul class=\"system-list\">{string.Join("", items)}</ul>";
	}

	private static string BuildTablesHtml(PublicGameResponse game)
	{
		if (game.Tables.Count == 0)
			return "<p class=\"muted\">Noch keine Tischdetails hinterlegt.</p>";

		var tables = game.Tables.Select(table =>
		{
			var systems = table.Systems.Count == 0
				? "System offen"
				: string.Join(", ", table.Systems);
			var points = table.Points.HasValue ? $"{table.Points} Punkte" : null;
			var scenario = string.IsNullOrWhiteSpace(table.Scenario) ? null : table.Scenario;
			var notes = string.IsNullOrWhiteSpace(table.Notes) ? null : table.Notes;
			var details = string.Join(" · ", new[] { systems, points, scenario, notes }
				.Where(value => !string.IsNullOrWhiteSpace(value))
				.Select(WebUtility.HtmlEncode));
			var seats = $"{table.AssignedPlayers.Count}/{table.MaxPlayers} belegt";

			return $$"""
				<div class="table-item">
				  <strong>{{WebUtility.HtmlEncode(table.Name)}}</strong>
				  <span>{{details}}</span>
				  <span>{{WebUtility.HtmlEncode(seats)}}</span>
				</div>
				""";
		});

		return $"<div class=\"table-list\">{string.Join("", tables)}</div>";
	}

	private static string BuildTimeLabel(PublicGameResponse game)
	{
		if (game.TimingMode == SessionTimingMode.Open)
			return string.IsNullOrWhiteSpace(game.TimeLabel) ? "Termin offen" : game.TimeLabel;

		if (game.TimingMode == SessionTimingMode.Rough)
		{
			var date = game.StartTimeUtc.ToString("ddd., dd.MM.", CultureInfo.GetCultureInfo("de-DE"));
			return string.IsNullOrWhiteSpace(game.TimeLabel) ? date : $"{date} {game.TimeLabel}";
		}

		return game.StartTimeUtc.ToString("ddd., dd.MM. 'um' HH:mm", CultureInfo.GetCultureInfo("de-DE"));
	}
}
