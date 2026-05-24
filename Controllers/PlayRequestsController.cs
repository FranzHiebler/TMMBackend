using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayRequestsController : ControllerBase
{
	private readonly IPlayRequestService _service;

	public PlayRequestsController(IPlayRequestService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<List<PlayRequestResponse>>> GetOpen()
	{
		return Ok(await _service.GetOpenAsync());
	}

	[HttpGet("mine")]
	public async Task<ActionResult<List<PlayRequestResponse>>> GetMine()
	{
		return Ok(await _service.GetMineAsync());
	}

	[HttpPost]
	public async Task<ActionResult<PlayRequestResponse>> Create(CreatePlayRequestRequest request)
	{
		return Ok(await _service.CreateAsync(request));
	}

	[HttpPost("{id}/convert")]
	public async Task<ActionResult<GameResponse>> Convert(string id, ConvertPlayRequestRequest request)
	{
		return Ok(await _service.ConvertToSessionAsync(id, request));
	}

	[HttpPost("{id}/close")]
	public async Task<IActionResult> Close(string id)
	{
		await _service.CloseAsync(id);
		return NoContent();
	}
}
