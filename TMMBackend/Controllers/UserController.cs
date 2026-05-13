using Microsoft.AspNetCore.Mvc;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly IUserRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public UsersController(
		IUserRepository repository,
		ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	[HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string? query)
	{
		var users = await _repository.SearchAsync(query);

		return Ok(users.Select(u => new
		{
			userId = u.Id,
			displayName = u.DisplayName,
			email = u.Email
		}));
	}

	[HttpGet("me")]
	public async Task<ActionResult<UserProfileResponse>> GetMe()
	{
		var user = await _repository.GetByIdAsync(_currentUser.UserId);

		if (user == null)
		{
			return Ok(new UserProfileResponse
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName
			});
		}

		return Ok(ToResponse(user));
	}

	[HttpPut("me")]
	public async Task<ActionResult<UserProfileResponse>> UpdateMe(
		[FromBody] UpdateUserProfileRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.DisplayName))
			throw new DomainException("Anzeigename ist erforderlich.");

		if (request.DisplayName.Trim().Length > 80)
			throw new DomainException("Anzeigename darf maximal 80 Zeichen lang sein.");

		var user = await _repository.GetByIdAsync(_currentUser.UserId)
			?? new UserProfile
			{
				Id = _currentUser.UserId,
				Email = null
			};

		user.DisplayName = request.DisplayName.Trim();
		user.DefaultLocationId = string.IsNullOrWhiteSpace(request.DefaultLocationId)
			? null
			: request.DefaultLocationId;

		await _repository.UpsertAsync(user);

		return Ok(ToResponse(user));
	}

	private static UserProfileResponse ToResponse(UserProfile user)
	{
		return new UserProfileResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			Email = user.Email,
			DefaultLocationId = user.DefaultLocationId
		};
	}
}