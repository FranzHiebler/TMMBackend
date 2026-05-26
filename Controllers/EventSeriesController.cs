using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventSeriesController : ControllerBase
{
	private readonly IEventSeriesService _service;

	public EventSeriesController(IEventSeriesService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<List<EventSeriesResponse>>> GetAll()
	{
		return Ok(await _service.GetAllAsync());
	}

	[HttpPost]
	public async Task<ActionResult<EventSeriesResponse>> Create(CreateEventSeriesRequest request)
	{
		return Ok(await _service.CreateAsync(request));
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<EventSeriesResponse>> Update(string id, CreateEventSeriesRequest request)
	{
		return Ok(await _service.UpdateAsync(id, request));
	}

	[HttpPost("{id}/create-next-session")]
	public async Task<ActionResult<GameResponse>> CreateNextSession(string id)
	{
		return Ok(await _service.CreateNextSessionAsync(id));
	}
}
