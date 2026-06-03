using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
	private readonly IFeedbackService _service;

	public FeedbackController(IFeedbackService service)
	{
		_service = service;
	}

	[HttpPost]
	public async Task<ActionResult<FeedbackResponse>> Create([FromBody] CreateFeedbackRequest request)
	{
		return Ok(await _service.CreateAsync(request));
	}

	[HttpGet("admin")]
	public async Task<ActionResult<List<FeedbackResponse>>> GetAdmin(
		[FromQuery] FeedbackStatus? status,
		[FromQuery] FeedbackType? type)
	{
		return Ok(await _service.GetAdminListAsync(status, type));
	}

	[HttpPut("admin/{id}")]
	public async Task<ActionResult<FeedbackResponse>> UpdateAdmin(
		string id,
		[FromBody] UpdateFeedbackAdminRequest request)
	{
		return Ok(await _service.UpdateAdminAsync(id, request));
	}
}
