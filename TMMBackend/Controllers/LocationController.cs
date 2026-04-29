using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Repositories;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
	private readonly LocationRepository _repository;
	public LocationsController(LocationRepository repository) { _repository = repository; }

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var locations = await _repository.GetAllAsync();
		return Ok(locations.Select(x => new { id = x.Id, name = x.Name, city = x.City }));
	}
}