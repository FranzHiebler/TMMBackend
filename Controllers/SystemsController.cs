using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Domain;
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
	public async Task<ActionResult<List<SystemResponse>>> GetAll()
	{
		var systems = await _repository.GetAllAsync();
		return Ok(systems.Select(ToResponse).ToList());
	}

	[HttpPost]
	public async Task<ActionResult<SystemResponse>> Create([FromBody] CreateSystemRequest request)
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		Validate(request);

		var system = await _repository.CreateAsync(request);
		return Ok(ToResponse(system));
	}

	[HttpPut("{key}")]
	public async Task<ActionResult<SystemResponse>> Update(string key, [FromBody] CreateSystemRequest request)
	{
		await _adminAuthorization.EnsureCurrentUserIsAdminAsync();
		request.Key = key;
		Validate(request);

		var system = await _repository.UpdateAsync(key, request);
		return Ok(ToResponse(system));
	}

	private static void Validate(CreateSystemRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Name))
			throw new DomainException("Key und Name sind erforderlich.");

		if (request.Key.Trim().Length > 80)
			throw new DomainException("Key darf maximal 80 Zeichen lang sein.");

		if (request.Name.Trim().Length > 120)
			throw new DomainException("Name darf maximal 120 Zeichen lang sein.");

		if (!string.IsNullOrWhiteSpace(request.ShortCode) && request.ShortCode.Trim().Length > 8)
			throw new DomainException("Kürzel darf maximal 8 Zeichen lang sein.");

		ValidateColor(request.Color, "Farbe");
		ValidateColor(request.MarkerColor, "Markerfarbe");
	}

	private static void ValidateColor(string? value, string label)
	{
		if (string.IsNullOrWhiteSpace(value))
			return;

		var color = value.Trim();

		if (!color.StartsWith("#") || color.Length != 7)
			throw new DomainException($"{label} muss im Format #RRGGBB angegeben werden.");
	}

	private static SystemResponse ToResponse(SystemDefinition system)
	{
		return new SystemResponse
		{
			Key = system.Key,
			Name = system.Name,
			ShortCode = system.ShortCode,
			Color = system.Color,
			MarkerColor = system.MarkerColor,
			Category = string.IsNullOrWhiteSpace(system.Category) ? "Tabletop" : system.Category
		};
	}
}
