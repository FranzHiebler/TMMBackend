using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
	private readonly ILocationService _service;

	public LocationsController(ILocationService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<List<LocationOptionResponse>>> GetAll()
	{
		return Ok(await _service.GetAllAsync());
	}

	[HttpPost]
	public async Task<ActionResult<LocationResponse>> Create([FromBody] CreateLocationRequest request)
	{
		return Ok(await _service.CreateAsync(request));
	}

	[HttpGet("mine")]
	public async Task<ActionResult<List<LocationResponse>>> GetMine()
	{
		return Ok(await _service.GetMineAsync());
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(string id, [FromBody] CreateLocationRequest request)
	{
		await _service.UpdateAsync(id, request);
		return NoContent();
	}

	[HttpGet("{id}/members")]
	public async Task<ActionResult<List<LocationMemberResponse>>> GetMembers(string id)
	{
		return Ok(await _service.GetMembersAsync(id));
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<LocationResponse>>> Nearby(
		[FromQuery] SearchNearbyLocationsRequest request)
	{
		return Ok(await _service.SearchNearbyAsync(request));
	}

	[HttpGet("discovery")]
	public async Task<ActionResult<List<LocationDiscoveryResponse>>> Discovery(
		[FromQuery] LocationDiscoveryRequest request)
	{
		return Ok(await _service.DiscoveryAsync(request));
	}

	[HttpPost("{id}/join-requests")]
	public async Task<IActionResult> RequestMembership(
		string id,
		[FromBody] RequestLocationMembershipRequest request)
	{
		await _service.RequestMembershipAsync(id, request);
		return NoContent();
	}

	[HttpGet("{id}/join-requests")]
	public async Task<ActionResult<List<LocationJoinRequestResponse>>> GetJoinRequests(string id)
	{
		return Ok(await _service.GetJoinRequestsAsync(id));
	}

	[HttpPost("{id}/join-requests/{requestId}/accept")]
	public async Task<IActionResult> AcceptJoinRequest(string id, string requestId)
	{
		await _service.AcceptJoinRequestAsync(id, requestId);
		return NoContent();
	}

	[HttpPost("{id}/join-requests/{requestId}/reject")]
	public async Task<IActionResult> RejectJoinRequest(string id, string requestId)
	{
		await _service.RejectJoinRequestAsync(id, requestId);
		return NoContent();
	}

	[HttpPost("{id}/members")]
	public async Task<IActionResult> UpsertMember(
		string id,
		[FromBody] UpsertLocationMemberRequest request)
	{
		await _service.UpsertMemberAsync(id, request);
		return NoContent();
	}

	[HttpDelete("{id}/members/{userId}")]
	public async Task<IActionResult> RemoveMember(string id, string userId)
	{
		await _service.RemoveMemberAsync(id, userId);
		return NoContent();
	}
}
