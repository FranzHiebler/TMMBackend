using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Repositories;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemsController : ControllerBase
{
	private readonly SystemRepository _repository;

	public SystemsController(SystemRepository repository)
	{
		_repository = repository;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var systems = await _repository.GetAllAsync();
		return Ok(systems.Select(x => new { key = x.Key, name = x.Name }));
	}
}