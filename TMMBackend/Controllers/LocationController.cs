using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Services.Interfaces;
using TMMBackend.Domain;
using TMMBackend.Dtos;
using TMMBackend.Services.Interfaces;

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
				new GeoJson2DGeographicCoordinates(
					request.Longitude,
					request.Latitude
				)
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

		return Ok(new
		{
			id = location.Id,
			name = location.Name,
			city = location.City,
			address = location.Address,
			latitude = location.Geo.Coordinates.Latitude,
			longitude = location.Geo.Coordinates.Longitude,
			role = LocationRole.Owner.ToString(),
			isOpen = location.AccessMode == LocationAccessMode.Open
		});
	}

	[HttpGet("mine")]
	public async Task<IActionResult> GetMine()
	{
		var locations = await _repository.GetForUserAsync(_currentUser.UserId);


		return Ok(locations.Select(x => new
		{
			id = x.Id,
			name = x.Name,
			city = x.City,
			address = x.Address,
			latitude = x.Geo?.Coordinates.Latitude,
			longitude = x.Geo?.Coordinates.Longitude,
			role = x.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId)?.Role.ToString(),
			isOpen = x.AccessMode == LocationAccessMode.Open
		}));
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(
	string id,
	[FromBody] CreateLocationRequest request)
	{
		var location = await _repository.GetByIdAsync(id);

		if (location == null)
			return NotFound();

		var isOwnerOrManager = location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			(m.Role == LocationRole.Owner || m.Role == LocationRole.Manager));

		if (!isOwnerOrManager)
			return Forbid();

		location.Name = request.Name;
		location.City = request.City;
		location.Address = request.Address;
		location.Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
			new GeoJson2DGeographicCoordinates(
				request.Longitude,
				request.Latitude
			)
		);

		await _repository.UpdateAsync(location);

		return NoContent();
	}

	[HttpGet("{id}/members")]
	public async Task<IActionResult> GetMembers(string id)
	{
		var location = await _repository.GetByIdAsync(id);

		if (location == null)
			return NotFound();

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
	public async Task<IActionResult> UpsertMember(
		string id,
		[FromBody] UpsertLocationMemberRequest request)
	{
		var location = await _repository.GetByIdAsync(id);

		if (location == null)
			return NotFound();

		if (!IsOwner(location))
			return Forbid();

		if (request.Role == LocationRole.Owner &&
			location.Members.Any(m => m.Role == LocationRole.Owner && m.UserId != request.UserId))
		{
			return BadRequest("Nur ein Owner pro Location ist aktuell erlaubt.");
		}

		var member = location.Members.FirstOrDefault(m => m.UserId == request.UserId);

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

		if (location == null)
			return NotFound();

		if (!IsOwner(location))
			return Forbid();

		var member = location.Members.FirstOrDefault(m => m.UserId == userId);

		if (member == null)
			return NotFound();

		if (member.Role == LocationRole.Owner)
			return BadRequest("Owner kann nicht entfernt werden.");

		location.Members.Remove(member);

		await _repository.UpdateAsync(location);

		return NoContent();
	}

	private bool IsOwner(Location location)
	{
		return location.Members.Any(m =>
			m.UserId == _currentUser.UserId &&
			m.Role == LocationRole.Owner);
	}

}