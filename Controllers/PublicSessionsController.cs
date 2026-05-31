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
</head>
<body>
  <main>
    <h1>{{title}}</h1>
    <p>{{description}}</p>
    <p><a href="{{encodedDetailUrl}}">Session im Tabletop Matchmaker &ouml;ffnen</a></p>
  </main>
</body>
</html>
""";

		return Content(html, "text/html", Encoding.UTF8);
	}

	private string BuildFrontendSessionUrl(string gameId)
	{
		var baseUrl = _configuration["Frontend:BaseUrl"];
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
		var where = string.Join(", ", new[] { game.Location.Name, game.Location.City }
			.Where(value => !string.IsNullOrWhiteSpace(value)));
		var when = BuildTimeLabel(game);
		var maxPlayers = game.Tables.Sum(table => table.MaxPlayers);
		var assignedPlayers = game.Tables.Sum(table => table.AssignedPlayers.Count);
		var seats = maxPlayers > 0
			? $"{assignedPlayers}/{maxPlayers} Plätze belegt"
			: "Plätze offen";

		return string.Join(" · ", new[] { where, when, seats }
			.Where(value => !string.IsNullOrWhiteSpace(value)));
	}

	private static string BuildTimeLabel(PublicGameResponse game)
	{
		if (game.TimingMode == SessionTimingMode.Open)
			return string.IsNullOrWhiteSpace(game.TimeLabel) ? "Termin offen" : game.TimeLabel;

		if (game.TimingMode == SessionTimingMode.Rough)
		{
			var date = game.StartTimeUtc.ToString("ddd., dd.MM.", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
			return string.IsNullOrWhiteSpace(game.TimeLabel) ? date : $"{date} {game.TimeLabel}";
		}

		return game.StartTimeUtc.ToString("ddd., dd.MM. 'um' HH:mm", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
	}
}
