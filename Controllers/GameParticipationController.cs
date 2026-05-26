using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/Games")]
public class GameParticipationController : ControllerBase
{
	private readonly IGameService _service;

	public GameParticipationController(IGameService service)
	{
		_service = service;
	}

	[HttpPost("{id}/tables/{tableId}/join")]
	public async Task<IActionResult> JoinTable(
		string id,
		string tableId,
		[FromBody] JoinTableRequest request)
	{
		await _service.JoinTableAsync(id, tableId, request);
		return NoContent();
	}

	[HttpPost("{id}/apply")]
	public async Task<IActionResult> Apply(
		string id,
		[FromBody] ApplyToGameRequest request)
	{
		await _service.ApplyAsync(id, request);
		return NoContent();
	}

	[HttpPost("{id}/tables/{tableId}/assign")]
	public async Task<IActionResult> AssignPlayerToTable(
		string id,
		string tableId,
		[FromBody] AssignPlayerToTableRequest request)
	{
		await _service.AssignPlayerToTableAsync(id, tableId, request);
		return NoContent();
	}

	[HttpPost("{id}/applications/{applicationId}/reject")]
	public async Task<IActionResult> RejectApplication(string id, string applicationId)
	{
		await _service.RejectApplicationAsync(id, applicationId);
		return NoContent();
	}

	[HttpPost("{id}/tables/{tableId}/players/{userId}/remove")]
	public async Task<IActionResult> RemovePlayerFromTable(
		string id,
		string tableId,
		string userId)
	{
		await _service.RemovePlayerFromTableAsync(id, tableId, userId);
		return NoContent();
	}

	[HttpPost("{id}/players/{userId}/move")]
	public async Task<IActionResult> MovePlayerToTable(
		string id,
		string userId,
		[FromBody] MovePlayerToTableRequest request)
	{
		await _service.MovePlayerToTableAsync(id, userId, request);
		return NoContent();
	}

	[HttpPost("{id}/waitlist")]
	public async Task<ActionResult<GameResponse>> JoinWaitlist(string id, JoinWaitlistRequest request)
	{
		return Ok(await _service.JoinWaitlistAsync(id, request));
	}

	[HttpPost("{id}/waitlist/{entryId}/promote")]
	public async Task<ActionResult<GameResponse>> PromoteWaitlist(string id, string entryId, [FromQuery] string tableId)
	{
		return Ok(await _service.PromoteWaitlistEntryAsync(id, entryId, tableId));
	}
}
