using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemsController : ControllerBase
{
	private readonly SystemRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public SystemsController(SystemRepository repository, ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var systems = await _repository.GetAllAsync();
		return Ok(systems.Select(x => new { key = x.Key, name = x.Name }));
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateSystemRequest request)
	{
		if (_currentUser.UserId != "64f1a2b3c4d5e6f7890abc12")
			return Forbid();

		if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
			return BadRequest("Key und Name sind erforderlich.");

		var system = await _repository.CreateAsync(request.Key, request.Name);
		return Ok(new { key = system.Key, name = system.Name });
	}
}
