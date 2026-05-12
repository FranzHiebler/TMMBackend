using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemsController : ControllerBase
{
	private readonly ISystemRepository _repository;
	private readonly IAdminAuthorizationService _adminAuthorization;

	public SystemsController(
		ISystemRepository repository,
		IAdminAuthorizationService adminAuthorization)
	{
		_repository = repository;
		_adminAuthorization = adminAuthorization;
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
		_adminAuthorization.EnsureCurrentUserIsAdmin();

		if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
			throw new DomainException("Key und Name sind erforderlich.");

		var system = await _repository.CreateAsync(request.Key, request.Name);
		return Ok(new { key = system.Key, name = system.Name });
	}
}