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

}