using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
	private readonly LocationRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public LocationsController(
		LocationRepository repository,
		ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	[HttpGet]
	public async Task<ActionResult<List<LocationOptionResponse>>> GetAll()
	{
		var locations = await _repository.GetAllAsync();
		return Ok(locations.Select(LocationMapper.ToOptionResponse));
	}

	[HttpPost]
	public async Task<ActionResult<LocationResponse>> Create([FromBody] CreateLocationRequest request)
	{
		var location = new Location
		{
			Name = request.Name,
			City = request.City,
			Address = request.Address,
			SystemKeys = LocationRules.NormalizeSystems(request.SystemKeys),
			Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
				new GeoJson2DGeographicCoordinates(request.Longitude, request.Latitude)
			),
			Members =
			[
				new LocationMember
				{
					UserId = _currentUser.UserId,
					DisplayName = _currentUser.DisplayName,
					Role = LocationRole.Owner
				}
			]
		};

		await _repository.CreateAsync(location);

		return Ok(LocationMapper.ToResponse(location, _currentUser.UserId));
	}

	[HttpGet("mine")]
	public async Task<ActionResult<List<LocationResponse>>> GetMine()
	{
		var locations = await _repository.GetForUserAsync(_currentUser.UserId);

		return Ok(locations.Select(location =>
			LocationMapper.ToResponse(location, _currentUser.UserId)));
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(string id, [FromBody] CreateLocationRequest request)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!LocationRules.CanEditLocation(location, _currentUser.UserId))
			return Forbid();

		location.Name = request.Name;
		location.City = request.City;
		location.Address = request.Address;
		location.SystemKeys = LocationRules.NormalizeSystems(request.SystemKeys);
		location.Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
			new GeoJson2DGeographicCoordinates(request.Longitude, request.Latitude)
		);

		await _repository.UpdateAsync(location);
		return NoContent();
	}

	[HttpGet("{id}/members")]
	public async Task<ActionResult<List<LocationMemberResponse>>> GetMembers(string id)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!LocationRules.CanViewMembers(location, _currentUser.UserId))
			return Forbid();

		return Ok(location.Members.Select(LocationMapper.ToMemberResponse));
	}

	[HttpGet("nearby")]
	public async Task<ActionResult<List<LocationResponse>>> Nearby(
		[FromQuery] SearchNearbyLocationsRequest request)
	{
		var locations = await _repository.FindNearbyLocationsAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters);

		var result = locations
			.Where(location => !location.Members.Any(m => m.UserId == _currentUser.UserId))
			.Where(location => !location.JoinRequests.Any(r =>
				r.UserId == _currentUser.UserId &&
				r.Status == LocationJoinRequestStatus.Pending))
			.Where(location =>
				string.IsNullOrWhiteSpace(request.SystemKey) ||
				location.SystemKeys.Contains(request.SystemKey, StringComparer.OrdinalIgnoreCase))
			.Select(location => LocationMapper.ToResponse(location, _currentUser.UserId));

		return Ok(result);
	}

	[HttpPost("{id}/join-requests")]
	public async Task<IActionResult> RequestMembership(
		string id,
		[FromBody] RequestLocationMembershipRequest request)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (location.Members.Any(m => m.UserId == _currentUser.UserId))
			return BadRequest("Du bist dort bereits Mitglied.");

		var existing = location.JoinRequests.FirstOrDefault(r =>
			r.UserId == _currentUser.UserId &&
			r.Status == LocationJoinRequestStatus.Pending);

		if (existing != null)
			return BadRequest("Du hast dort bereits angefragt.");

		location.JoinRequests.Add(new LocationJoinRequest
		{
			RequestId = Guid.NewGuid().ToString("N"),
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName,
			Message = request.Message,
			Status = LocationJoinRequestStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		await _repository.UpdateAsync(location);
		return NoContent();
	}

	[HttpPost("{id}/members")]
	public async Task<IActionResult> UpsertMember(
		string id,
		[FromBody] UpsertLocationMemberRequest request)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!LocationRules.CanManageMembers(location, _currentUser.UserId))
			return Forbid();

		var actorRole = LocationRules.GetCurrentUserRole(location, _currentUser.UserId);
		var member = location.Members.FirstOrDefault(m => m.UserId == request.UserId);

		if (!LocationRules.CanAssignRole(actorRole, request.Role))
			return BadRequest("Diese Rolle darfst du nicht vergeben.");

		if (member != null && !LocationRules.CanModifyTarget(actorRole, member.Role))
			return BadRequest("Dieses Mitglied darfst du nicht ändern.");

		if (member == null)
		{
			location.Members.Add(new LocationMember
			{
				UserId = request.UserId,
				DisplayName = request.DisplayName,
				Role = request.Role
			});
		}
		else
		{
			member.DisplayName = request.DisplayName;
			member.Role = request.Role;
		}

		await _repository.UpdateAsync(location);
		return NoContent();
	}

	[HttpDelete("{id}/members/{userId}")]
	public async Task<IActionResult> RemoveMember(string id, string userId)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!LocationRules.CanManageMembers(location, _currentUser.UserId))
			return Forbid();

		var actorRole = LocationRules.GetCurrentUserRole(location, _currentUser.UserId);
		var member = location.Members.FirstOrDefault(m => m.UserId == userId);

		if (member == null) return NotFound();

		if (member.Role == LocationRole.Owner)
			return BadRequest("Owner kann nicht entfernt werden.");

		if (!LocationRules.CanModifyTarget(actorRole, member.Role))
			return BadRequest("Dieses Mitglied darfst du nicht entfernen.");

		location.Members.Remove(member);

		await _repository.UpdateAsync(location);
		return NoContent();
	}
}