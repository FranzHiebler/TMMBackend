using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
	private readonly INotificationService _service;

	public NotificationsController(INotificationService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<List<NotificationResponse>>> GetMine()
	{
		return Ok(await _service.GetMineAsync());
	}

	[HttpPost("{id}/read")]
	public async Task<IActionResult> MarkRead(string id)
	{
		await _service.MarkReadAsync(id);
		return NoContent();
	}

	[HttpPost("read-all")]
	public async Task<IActionResult> MarkAllRead()
	{
		await _service.MarkAllReadAsync();
		return NoContent();
	}
}
