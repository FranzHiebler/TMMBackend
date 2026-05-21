using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
	private readonly IFriendService _service;

	public FriendsController(IFriendService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<List<FriendResponse>>> GetFriends()
	{
		return Ok(await _service.GetFriendsAsync());
	}

	[HttpGet("requests")]
	public async Task<ActionResult<List<FriendRequestResponse>>> GetRequests()
	{
		return Ok(await _service.GetRequestsAsync());
	}

	[HttpPost("request")]
	public async Task<ActionResult<FriendRequestResponse?>> SendRequest(SendFriendRequestRequest request)
	{
		return Ok(await _service.SendRequestAsync(request));
	}

	[HttpPost("{id}/accept")]
	public async Task<ActionResult<FriendResponse>> Accept(string id)
	{
		return Ok(await _service.AcceptAsync(id));
	}

	[HttpPost("{id}/reject")]
	public async Task<IActionResult> Reject(string id)
	{
		await _service.RejectAsync(id);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(string id)
	{
		await _service.DeleteAsync(id);
		return NoContent();
	}
}
