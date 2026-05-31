using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
public class PublicSessionsController : ControllerBase
{
	private const string DefaultOgImagePath = "/share/default-session-og.png";
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
		var imageUrl = GetShareImageUrl();
		var title = WebUtility.HtmlEncode(game.Title);
		var description = WebUtility.HtmlEncode($"{game.Location.Name}, {game.Location.City} - {game.TimeLabel ?? game.StartTimeUtc.ToString("g")}");
		var encodedUrl = WebUtility.HtmlEncode(url);
		var encodedImageUrl = WebUtility.HtmlEncode(imageUrl);
		var apiUrl = $"/api/Games/public/{WebUtility.UrlEncode(slugOrId)}";

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
    <p>&Ouml;ffentliche Session-Details: <a href="{{apiUrl}}">{{apiUrl}}</a></p>
  </main>
</body>
</html>
""";

		return Content(html, "text/html", Encoding.UTF8);
	}

	private string GetShareImageUrl()
	{
		var configuredUrl = Environment.GetEnvironmentVariable("TMM_DEFAULT_OG_IMAGE_URL");
		if (!string.IsNullOrWhiteSpace(configuredUrl))
			return configuredUrl.Trim();

		return $"{Request.Scheme}://{Request.Host}{DefaultOgImagePath}";
	}
}
