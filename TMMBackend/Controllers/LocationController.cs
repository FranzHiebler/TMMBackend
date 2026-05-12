using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services;
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
		try
		{
			await _service.UpdateAsync(id, request);
			return NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
	}

	[HttpGet("{id}/members")]
	public async Task<ActionResult<List<LocationMemberResponse>>> GetMembers(string id)
	{
		try
		{
			return Ok(await _service.GetMembersAsync(id));
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<LocationResponse>>> Nearby(
		[FromQuery] SearchNearbyLocationsRequest request)
	{
		return Ok(await _service.SearchNearbyAsync(request));
	}

	[HttpPost("{id}/join-requests")]
	public async Task<IActionResult> RequestMembership(
		string id,
		[FromBody] RequestLocationMembershipRequest request)
	{
		try
		{
			await _service.RequestMembershipAsync(id, request);
			return NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (GameActionException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("{id}/members")]
	public async Task<IActionResult> UpsertMember(
		string id,
		[FromBody] UpsertLocationMemberRequest request)
	{
		try
		{
			await _service.UpsertMemberAsync(id, request);
			return NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
		catch (GameActionException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpDelete("{id}/members/{userId}")]
	public async Task<IActionResult> RemoveMember(string id, string userId)
	{
		try
		{
			await _service.RemoveMemberAsync(id, userId);
			return NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (UnauthorizedAccessException)
		{
			return Forbid();
		}
		catch (GameActionException ex)
		{
			return BadRequest(ex.Message);
		}
	}
}