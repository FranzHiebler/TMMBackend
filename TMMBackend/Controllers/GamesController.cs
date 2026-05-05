using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;
using TMMBackend.Dtos;
using TMMBackend.Services;

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

	[HttpPost]
	public async Task<ActionResult<GameResponse>> Create(CreateGameRequest request)
	{
		try
		{
			var result = await _service.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
		}
		catch (Exception ex) when (ex.Message == "Location not found")
		{
			return NotFound(ex.Message);
		}
		catch (Exception ex) when (
			ex.Message == "Not allowed to create game at this location")
		{
			return Forbid();
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<GameResponse>> GetById(string id)
	{
		var game = await _service.GetByIdAsync(id);

		if (game == null)
			return NotFound();

		return Ok(game);
	}

	[HttpPost("{id}/tables/{tableId}/join")]
	public async Task<IActionResult> JoinTable(
		string id,
		string tableId,
		[FromBody] JoinTableRequest request)
	{
		try
		{
			await _service.JoinTableAsync(id, tableId, request);
			return NoContent();
		}
		catch (GameActionException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("{id}/apply")]
	public async Task<IActionResult> Apply(
		string id,
		[FromBody] ApplyToGameRequest request)
	{
		try
		{
			await _service.ApplyAsync(id, request);
			return NoContent();
		}
		catch (GameActionException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("{id}/tables/{tableId}/assign")]
	public async Task<IActionResult> AssignPlayerToTable(
		string id,
		string tableId,
		[FromBody] AssignPlayerToTableRequest request)
	{
		var success = await _service.AssignPlayerToTableAsync(id, tableId, request);
		return success ? NoContent() : BadRequest("Assign failed");
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
}