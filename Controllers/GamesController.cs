using Microsoft.AspNetCore.Mvc;
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
