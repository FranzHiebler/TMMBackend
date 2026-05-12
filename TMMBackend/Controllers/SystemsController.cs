using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemsController : ControllerBase
{
	private const string AdminUserId = "64f1a2b3c4d5e6f7890abc12";

	private readonly ISystemRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public SystemsController(ISystemRepository repository, ICurrentUserService currentUser)
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
		try
		{
			if (_currentUser.UserId != AdminUserId)
				return Forbid();

			if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
				return BadRequest("Key und Name sind erforderlich.");

			var system = await _repository.CreateAsync(request.Key, request.Name);
			return Ok(new { key = system.Key, name = system.Name });
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized("Benutzer wurde nicht übergeben.");
		}
	}
}