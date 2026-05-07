using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GeoJsonObjectModel;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
	private readonly LocationRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public LocationsController(LocationRepository repository, ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var locations = await _repository.GetAllAsync();
		return Ok(locations.Select(x => new { id = x.Id, name = x.Name, city = x.City }));
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateLocationRequest request)
	{
		var location = new Location
		{
			Name = request.Name,
			City = request.City,
			Address = request.Address,
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
		return Ok(ToLocationResponse(location));
	}

	[HttpGet("mine")]
	public async Task<IActionResult> GetMine()
	{
		var locations = await _repository.GetForUserAsync(_currentUser.UserId);
		return Ok(locations.Select(ToLocationResponse));
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(string id, [FromBody] CreateLocationRequest request)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!CanEditLocation(location))
			return Forbid();

		location.Name = request.Name;
		location.City = request.City;
		location.Address = request.Address;
		location.Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
			new GeoJson2DGeographicCoordinates(request.Longitude, request.Latitude)
		);

		await _repository.UpdateAsync(location);
		return NoContent();
	}

	[HttpGet("{id}/members")]
	public async Task<IActionResult> GetMembers(string id)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		var isMember = location.Members.Any(m => m.UserId == _currentUser.UserId);
		if (!isMember && location.AccessMode != LocationAccessMode.Open)
			return Forbid();

		return Ok(location.Members.Select(m => new
		{
			userId = m.UserId,
			displayName = m.DisplayName,
			role = m.Role.ToString()
		}));
	}

	[HttpPost("{id}/members")]
	public async Task<IActionResult> UpsertMember(string id, [FromBody] UpsertLocationMemberRequest request)
	{
		var location = await _repository.GetByIdAsync(id);
		if (location == null) return NotFound();

		if (!CanManageMembers(location))
			return Forbid();

		var actorRole = GetCurrentUserRole(location);
		var member = location.Members.FirstOrDefault(m => m.UserId == request.UserId);

		if (!CanAssignRole(actorRole, request.Role))
			return BadRequest("Diese Rolle darfst du nicht vergeben.");

		if (member != null && !CanModifyTarget(actorRole, member.Role))
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

		if (!CanManageMembers(location))
			return Forbid();

		var actorRole = GetCurrentUserRole(location);
		var member = location.Members.FirstOrDefault(m => m.UserId == userId);

		if (member == null) return NotFound();

		if (member.Role == LocationRole.Owner)
			return BadRequest("Owner kann nicht entfernt werden.");

		if (!CanModifyTarget(actorRole, member.Role))
			return BadRequest("Dieses Mitglied darfst du nicht entfernen.");

		location.Members.Remove(member);

		await _repository.UpdateAsync(location);
		return NoContent();
	}

	private object ToLocationResponse(Location location)
	{
		var member = location.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);

		return new
		{
			id = location.Id,
			name = location.Name,
			city = location.City,
			address = location.Address,
			latitude = location.Geo?.Coordinates.Latitude,
			longitude = location.Geo?.Coordinates.Longitude,
			role = member?.Role.ToString(),
			isOpen = location.AccessMode == LocationAccessMode.Open
		};
	}

	private LocationRole? GetCurrentUserRole(Location location)
	{
		return location.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId)?.Role;
	}

	private bool CanEditLocation(Location location)
	{
		var role = GetCurrentUserRole(location);
		return role == LocationRole.Owner ||
			   role == LocationRole.Admin ||
			   role == LocationRole.Manager;
	}

	private bool CanManageMembers(Location location)
	{
		var role = GetCurrentUserRole(location);
		return role == LocationRole.Owner ||
			   role == LocationRole.Admin;
	}

	private static bool CanAssignRole(LocationRole? actorRole, LocationRole targetRole)
	{
		if (actorRole == LocationRole.Owner)
			return targetRole != LocationRole.Owner;

		if (actorRole == LocationRole.Admin)
			return targetRole == LocationRole.Manager ||
				   targetRole == LocationRole.Member ||
				   targetRole == LocationRole.Applicant;

		return false;
	}

	private static bool CanModifyTarget(LocationRole? actorRole, LocationRole targetCurrentRole)
	{
		if (targetCurrentRole == LocationRole.Owner)
			return false;

		if (actorRole == LocationRole.Owner)
			return true;

		if (actorRole == LocationRole.Admin)
			return targetCurrentRole != LocationRole.Admin;

		return false;
	}
}