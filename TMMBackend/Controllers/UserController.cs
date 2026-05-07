using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Repositories;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly UserRepository _repository;

	public UsersController(UserRepository repository)
	{
		_repository = repository;
	}

	[HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string? query)
	{
		var users = await _repository.SearchAsync(query);

		return Ok(users.Select(u => new
		{
			userId = u.Id,
			displayName = u.DisplayName
		}));
	}
}