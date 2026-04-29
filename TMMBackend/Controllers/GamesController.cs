using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
	private readonly IGameService _service;

	public GamesController(IGameService service)
	{
		_service = service;
	}

	// POST api/games
	[HttpPost]
	public async Task<ActionResult<GameResponse>> Create(CreateGameRequest request)
	{
		var result = await _service.CreateAsync(request);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	// GET api/games/{id}
	[HttpGet("{id}")]
	public async Task<ActionResult<GameResponse>> GetById(string id)
	{
		var game = await _service.GetByIdAsync(id);

		if (game == null)
			return NotFound();

		return Ok(game);
	}

	[HttpPost("{id}/join")]
	public async Task<IActionResult> Join(string id, JoinGameRequest request)
	{
		var success = await _service.JoinAsync(id, request.UserId, request.DisplayName);

		return success ? NoContent() : BadRequest("Join failed");
	}

	[HttpGet("search")]
	public async Task<ActionResult<List<GameResponse>>> Search([FromQuery] SearchGamesRequest r)
	{
		return Ok(await _service.SearchAsync(r));
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<GameResponse>>> Nearby([FromQuery] SearchNearbyGamesRequest request)
	{
		return Ok(await _service.SearchNearbyAsync(request));
	}
}