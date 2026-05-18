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
	private readonly IFriendRepository _friends;

	public UsersController(
		IUserRepository repository,
		ICurrentUserService currentUser,
		IFriendRepository friends)
	{
		_repository = repository;
		_currentUser = currentUser;
		_friends = friends;
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

	[HttpGet("me")]
	public async Task<ActionResult<UserProfileResponse>> GetMe()
	{
		var user = await _repository.GetByIdAsync(_currentUser.UserId);

		if (user == null)
		{
			return Ok(new UserProfileResponse
			{
				UserId = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName,
				CanBeContacted = true
			});
		}

		return Ok(ToResponse(user));
	}

	[HttpPut("me")]
	public async Task<ActionResult<UserProfileResponse>> UpdateMe(
		[FromBody] UpdateUserProfileRequest request)
	{
		ValidateProfile(request);

		var user = await _repository.GetByIdAsync(_currentUser.UserId)
			?? new UserProfile
			{
				Id = _currentUser.UserId
			};

		user.DisplayName = request.DisplayName.Trim();
		user.Email = NormalizeOptional(request.Email);
		user.PhoneNumber = NormalizeOptional(request.PhoneNumber);
		user.StreetAddress = NormalizeOptional(request.StreetAddress);
		user.PostalCode = NormalizeOptional(request.PostalCode);
		user.City = NormalizeOptional(request.City);
		user.Latitude = request.Latitude;
		user.Longitude = request.Longitude;
		user.TabletopTo = NormalizeOptional(request.TabletopTo);
		user.TabletopHerald = NormalizeOptional(request.TabletopHerald);
		user.T3 = NormalizeOptional(request.T3);
		user.NewRecruit = NormalizeOptional(request.NewRecruit);
		user.BestSportsPairings = NormalizeOptional(request.BestSportsPairings);
		user.ProfileImageUrl = NormalizeOptional(request.ProfileImageUrl);
		user.DefaultLocationId = NormalizeOptional(request.DefaultLocationId);
		user.CanBeContacted = request.CanBeContacted;
		user.Visibility = ToDomainVisibility(request.Visibility);

		await _repository.UpsertAsync(user);

		return Ok(ToResponse(user));
	}

	private static void ValidateProfile(UpdateUserProfileRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.DisplayName))
			throw new DomainException("Anzeigename ist erforderlich.");

		if (request.DisplayName.Trim().Length > 80)
			throw new DomainException("Anzeigename darf maximal 80 Zeichen lang sein.");

		ValidateLength(request.Email, 200, "E-Mail");
		ValidateLength(request.PhoneNumber, 50, "Telefonnummer");
		ValidateLength(request.StreetAddress, 200, "Straße / Adresse");
		ValidateLength(request.PostalCode, 20, "PLZ");
		ValidateLength(request.City, 120, "Ort");
		ValidateLength(request.TabletopTo, 200, "TabletopTO");
		ValidateLength(request.TabletopHerald, 200, "Tabletop Herald");
		ValidateLength(request.T3, 200, "T3");
		ValidateLength(request.NewRecruit, 200, "NewRecruit");
		ValidateLength(request.BestSportsPairings, 200, "Best Coast Pairings");
		ValidateLength(request.ProfileImageUrl, 500, "Profilbild");
		ValidateCoordinates(request.Latitude, request.Longitude);
	}

	private static void ValidateLength(string? value, int maxLength, string label)
	{
		if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
			throw new DomainException($"{label} darf maximal {maxLength} Zeichen lang sein.");
	}

	private static void ValidateCoordinates(double? latitude, double? longitude)
	{
		if (latitude == null && longitude == null)
			return;

		if (latitude == null || longitude == null)
			throw new DomainException("Latitude und Longitude müssen gemeinsam gesetzt werden.");

		if (latitude < -90 || latitude > 90)
			throw new DomainException("Latitude muss zwischen -90 und 90 liegen.");

		if (longitude < -180 || longitude > 180)
			throw new DomainException("Longitude muss zwischen -180 und 180 liegen.");
	}

	private static string? NormalizeOptional(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}

	private static UserProfileVisibility ToDomainVisibility(UserProfileVisibilityDto dto)
	{
		return new UserProfileVisibility
		{
			Email = dto.Email,
			PhoneNumber = dto.PhoneNumber,
			StreetAddress = dto.StreetAddress,
			PostalCode = dto.PostalCode,
			City = dto.City,
			TabletopTo = dto.TabletopTo,
			TabletopHerald = dto.TabletopHerald,
			T3 = dto.T3,
			NewRecruit = dto.NewRecruit,
			BestSportsPairings = dto.BestSportsPairings
		};
	}

	private static UserProfileVisibilityDto ToVisibilityDto(UserProfileVisibility visibility)
	{
		return new UserProfileVisibilityDto
		{
			Email = visibility.Email,
			PhoneNumber = visibility.PhoneNumber,
			StreetAddress = visibility.StreetAddress,
			PostalCode = visibility.PostalCode,
			City = visibility.City,
			TabletopTo = visibility.TabletopTo,
			TabletopHerald = visibility.TabletopHerald,
			T3 = visibility.T3,
			NewRecruit = visibility.NewRecruit,
			BestSportsPairings = visibility.BestSportsPairings
		};
	}

	private static UserProfileResponse ToResponse(UserProfile user)
	{
		return new UserProfileResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
			StreetAddress = user.StreetAddress,
			PostalCode = user.PostalCode,
			City = user.City,
			Latitude = user.Latitude,
			Longitude = user.Longitude,
			TabletopTo = user.TabletopTo,
			TabletopHerald = user.TabletopHerald,
			T3 = user.T3,
			NewRecruit = user.NewRecruit,
			BestSportsPairings = user.BestSportsPairings,
			ProfileImageUrl = user.ProfileImageUrl,
			DefaultLocationId = user.DefaultLocationId,
			CanBeContacted = user.CanBeContacted,
			Visibility = ToVisibilityDto(user.Visibility ?? new UserProfileVisibility())
		};
	}
	[HttpGet("{userId}/profile")]
	public async Task<ActionResult<PublicUserProfileResponse>> GetPublicProfile(string userId)
	{
		var user = await _repository.GetByIdAsync(userId);

		if (user == null)
			return NotFound(new { error = "Profil wurde nicht gefunden." });

		var friendship = await _friends.FindBetweenUsersAsync(_currentUser.UserId, userId);
		var isFriend = friendship?.Status == FriendshipStatus.Accepted;
		var isOwnProfile = _currentUser.UserId == userId;
		var hiddenFields = BuildHiddenFields(user, isFriend, isOwnProfile);

		return Ok(new PublicUserProfileResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			ProfileImageUrl = user.ProfileImageUrl,
			CanBeContacted = user.CanBeContacted,
			IsFriend = isFriend,
			HiddenFields = hiddenFields,

			Email = CanSee(user.Visibility?.Email, isFriend, isOwnProfile) ? user.Email : null,
			PhoneNumber = CanSee(user.Visibility?.PhoneNumber, isFriend, isOwnProfile) ? user.PhoneNumber : null,
			StreetAddress = CanSee(user.Visibility?.StreetAddress, isFriend, isOwnProfile) ? user.StreetAddress : null,
			PostalCode = CanSee(user.Visibility?.PostalCode, isFriend, isOwnProfile) ? user.PostalCode : null,
			City = CanSee(user.Visibility?.City, isFriend, isOwnProfile) ? user.City : null,
			TabletopTo = CanSee(user.Visibility?.TabletopTo, isFriend, isOwnProfile) ? user.TabletopTo : null,
			TabletopHerald = CanSee(user.Visibility?.TabletopHerald, isFriend, isOwnProfile) ? user.TabletopHerald : null,
			T3 = CanSee(user.Visibility?.T3, isFriend, isOwnProfile) ? user.T3 : null,
			NewRecruit = CanSee(user.Visibility?.NewRecruit, isFriend, isOwnProfile) ? user.NewRecruit : null,
			BestSportsPairings = CanSee(user.Visibility?.BestSportsPairings, isFriend, isOwnProfile) ? user.BestSportsPairings : null
		});
	}
	private static List<string> BuildHiddenFields(UserProfile user, bool isFriend, bool isOwnProfile)
	{
		var hidden = new List<string>();

		AddIfHidden(hidden, "email", user.Email, user.Visibility?.Email, isFriend, isOwnProfile);
		AddIfHidden(hidden, "phoneNumber", user.PhoneNumber, user.Visibility?.PhoneNumber, isFriend, isOwnProfile);
		AddIfHidden(hidden, "streetAddress", user.StreetAddress, user.Visibility?.StreetAddress, isFriend, isOwnProfile);
		AddIfHidden(hidden, "postalCode", user.PostalCode, user.Visibility?.PostalCode, isFriend, isOwnProfile);
		AddIfHidden(hidden, "city", user.City, user.Visibility?.City, isFriend, isOwnProfile);
		AddIfHidden(hidden, "tabletopTo", user.TabletopTo, user.Visibility?.TabletopTo, isFriend, isOwnProfile);
		AddIfHidden(hidden, "tabletopHerald", user.TabletopHerald, user.Visibility?.TabletopHerald, isFriend, isOwnProfile);
		AddIfHidden(hidden, "t3", user.T3, user.Visibility?.T3, isFriend, isOwnProfile);
		AddIfHidden(hidden, "newRecruit", user.NewRecruit, user.Visibility?.NewRecruit, isFriend, isOwnProfile);
		AddIfHidden(hidden, "bestSportsPairings", user.BestSportsPairings, user.Visibility?.BestSportsPairings, isFriend, isOwnProfile);

		return hidden;
	}

	private static void AddIfHidden(
		List<string> hidden,
		string key,
		string? value,
		ProfileFieldVisibility? visibility,
		bool isFriend,
		bool isOwnProfile)
	{
		if (string.IsNullOrWhiteSpace(value))
			return;

		if (!CanSee(visibility, isFriend, isOwnProfile))
			hidden.Add(key);
	}

	private static bool CanSee(ProfileFieldVisibility? visibility, bool isFriend, bool isOwnProfile)
	{
		if (isOwnProfile) return true;

		return visibility switch
		{
			ProfileFieldVisibility.Public => true,
			ProfileFieldVisibility.FriendsOnly => isFriend,
			_ => false
		};
	}
}