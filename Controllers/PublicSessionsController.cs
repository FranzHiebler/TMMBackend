using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
public class PublicSessionsController : ControllerBase
{
	private readonly IGameService _games;

	public PublicSessionsController(IGameService games)
	{
		_games = games;
	}

	[HttpGet("s/{slugOrId}")]
	public async Task<IActionResult> PublicSessionHtml(string slugOrId)
	{
		var game = await _games.GetPublicAsync(slugOrId);
		if (game == null)
			return NotFound("Session wurde nicht gefunden.");

		var url = $"{Request.Scheme}://{Request.Host}{Request.Path}";
		var title = WebUtility.HtmlEncode(game.Title);
		var description = WebUtility.HtmlEncode($"{game.Location.Name}, {game.Location.City} · {game.TimeLabel ?? game.StartTimeUtc.ToString("g")}");
		var apiUrl = $"/api/Games/public/{WebUtility.UrlEncode(slugOrId)}";

		var html = $$"""
<!doctype html>
<html lang="de">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{title}}</title>
  <meta property="og:title" content="{{title}}">
  <meta property="og:description" content="{{description}}">
  <meta property="og:url" content="{{WebUtility.HtmlEncode(url)}}">
  <meta property="og:type" content="website">
</head>
<body>
  <main>
    <h1>{{title}}</h1>
    <p>{{description}}</p>
    <p>Öffentliche Session-Details: <a href="{{apiUrl}}">{{apiUrl}}</a></p>
  </main>
</body>
</html>
""";

		return Content(html, "text/html", Encoding.UTF8);
	}
}
