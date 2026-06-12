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
	private readonly IAdminAuthorizationService _adminAuthorization;
	private readonly IWebHostEnvironment _environment;

	public UsersController(
		IUserRepository repository,
		ICurrentUserService currentUser,
		IFriendRepository friends,
		IAdminAuthorizationService adminAuthorization,
		IWebHostEnvironment environment)
	{
		_repository = repository;
		_currentUser = currentUser;
		_friends = friends;
		_adminAuthorization = adminAuthorization;
		_environment = environment;
	}

	[HttpGet("search")]
	public async Task<IActionResult> Search([FromQuery] string? query)
	{
		var users = await _repository.SearchAsync(query);
		var currentUserId = _currentUser.UserId;

		return Ok(users
			.Where(u => !u.HideProfile)
			.Select(u =>
			{
				var isOwnProfile = u.Id == currentUserId;
				var canSeeCity = CanSee(u.Visibility?.City, false, isOwnProfile);
				var canSeePostalCode = CanSee(u.Visibility?.PostalCode, false, isOwnProfile);
				var canSeeStreetAddress = CanSee(u.Visibility?.StreetAddress, false, isOwnProfile);
				var canShowOnMap =
					!u.HideOnMap &&
					(isOwnProfile || canSeeCity);
				var canUseExactPosition =
					!u.HideOnMap &&
					(isOwnProfile || (canSeeCity && canSeeStreetAddress));
				double? mapLatitude = null;
				double? mapLongitude = null;
				var locationPrecision = "hidden";

				if (canShowOnMap && u.Latitude.HasValue && u.Longitude.HasValue)
				{
					if (canUseExactPosition)
					{
						mapLatitude = u.Latitude;
						mapLongitude = u.Longitude;
						locationPrecision = "exact";
					}
					else
					{
						mapLatitude = Math.Round(u.Latitude.Value, 2);
						mapLongitude = Math.Round(u.Longitude.Value, 2);
						locationPrecision = "approximate";
					}
				}

				return new
				{
					userId = u.Id,
					displayName = u.DisplayName,
					postalCode = canSeePostalCode ? u.PostalCode : null,
					city = canSeeCity ? u.City : null,
					latitude = mapLatitude,
					longitude = mapLongitude,
					locationPrecision,
					favoriteSystemKeys = u.FavoriteSystemKeys ?? new List<string>(),
					lookingForGame = ToLookingForGameDto(u.LookingForGame ?? new LookingForGameStatus())
				};
			}));
	}

	[HttpGet("test-users")]
	public async Task<ActionResult<List<TestUserOptionResponse>>> GetTestUsers()
	{
		var users = await _repository.SearchAsync(null);

		return Ok(users
			.Where(u => !u.HideProfile)
			.Where(u => !string.IsNullOrWhiteSpace(u.Id))
			.Select(u => new TestUserOptionResponse
			{
				UserId = u.Id!,
				DisplayName = string.IsNullOrWhiteSpace(u.DisplayName) ? "Unbenannter Testnutzer" : u.DisplayName,
				IsDevUser = u.IsDevUser
			})
			.ToList());
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

	[HttpGet("me/permissions")]
	public async Task<ActionResult<UserPermissionsResponse>> GetMyPermissions()
	{
		var user = await _repository.GetByIdAsync(_currentUser.UserId);
		var isSystemAdmin = await _adminAuthorization.IsCurrentUserAdminAsync();

		return Ok(new UserPermissionsResponse
		{
			IsAdmin = isSystemAdmin,
			IsSystemAdmin = isSystemAdmin,
			IsDevUser = user?.IsDevUser == true
		});
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
		user.FirstName = NormalizeOptional(request.FirstName);
		user.LastName = NormalizeOptional(request.LastName);
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
		user.HideProfile = request.HideProfile;
		user.HideOnMap = request.HideOnMap;
		user.HideParticipation = request.HideParticipation;
		user.Visibility = ToDomainVisibility(request.Visibility);
		user.FavoriteSystemKeys = NormalizeStringList(request.FavoriteSystemKeys, 40, 80);
		user.Armies = (request.Armies ?? new List<UserArmyProfileDto>())
			.Where(a => !string.IsNullOrWhiteSpace(a.SystemKey) && !string.IsNullOrWhiteSpace(a.ArmyName))
			.Take(100)
			.Select(a => new UserArmyProfile
			{
				SystemKey = a.SystemKey.Trim(),
				ArmyName = a.ArmyName.Trim()
			})
			.ToList();
		user.LookingForGame = ToDomainLookingForGame(request.LookingForGame);
		if (request.DiscoverySettings != null)
			user.DiscoverySettings = ToDomainDiscoverySettings(request.DiscoverySettings);

		await _repository.UpsertAsync(user);

		return Ok(ToResponse(user));
	}

	[HttpPut("me/discovery-settings")]
	public async Task<ActionResult<UserDiscoverySettingsDto>> UpdateDiscoverySettings(
		[FromBody] UserDiscoverySettingsDto request)
	{
		var user = await _repository.GetByIdAsync(_currentUser.UserId)
			?? new UserProfile
			{
				Id = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName,
				CanBeContacted = true
			};

		user.DiscoverySettings = ToDomainDiscoverySettings(request);

		await _repository.UpsertAsync(user);

		return Ok(ToDiscoverySettingsDto(user.DiscoverySettings));
	}

	[HttpPost("me/profile-image")]
	public async Task<ActionResult<UserProfileResponse>> UploadProfileImage([FromForm] IFormFile file)
	{
		if (file == null || file.Length == 0)
			throw new DomainException("Bitte wähle ein Bild aus.");

		if (file.Length > 2 * 1024 * 1024)
			throw new DomainException("Profilbild darf maximal 2 MB groß sein.");

		var extension = GetProfileImageExtension(file.ContentType, file.FileName);
		var user = await _repository.GetByIdAsync(_currentUser.UserId)
			?? new UserProfile
			{
				Id = _currentUser.UserId,
				DisplayName = _currentUser.DisplayName,
				CanBeContacted = true
			};

		var uploadRoot = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads", "profile-images");
		Directory.CreateDirectory(uploadRoot);

		var safeUserId = SanitizeFileName(_currentUser.UserId);
		foreach (var existingFile in Directory.GetFiles(uploadRoot, $"{safeUserId}.*"))
		{
			System.IO.File.Delete(existingFile);
		}

		var fileName = $"{safeUserId}{extension}";
		var filePath = Path.Combine(uploadRoot, fileName);

		await using (var stream = System.IO.File.Create(filePath))
		{
			await file.CopyToAsync(stream);
		}

		user.ProfileImageUrl = $"{GetPublicOrigin()}/uploads/profile-images/{Uri.EscapeDataString(fileName)}";
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
		ValidateLength(request.FirstName, 120, "Vorname");
		ValidateLength(request.LastName, 120, "Nachname");
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
		ValidateLength(request.LookingForGame?.TimeNote, 500, "Suchstatus-Notiz");
		ValidateCoordinates(request.Latitude, request.Longitude);
	}

	private static string GetProfileImageExtension(string? contentType, string fileName)
	{
		var extension = Path.GetExtension(fileName).ToLowerInvariant();
		var normalizedContentType = contentType?.ToLowerInvariant();

		return (normalizedContentType, extension) switch
		{
			("image/jpeg", ".jpg" or ".jpeg") => ".jpg",
			("image/png", ".png") => ".png",
			("image/webp", ".webp") => ".webp",
			_ => throw new DomainException("Bitte lade ein JPG-, PNG- oder WEBP-Bild hoch.")
		};
	}

	private static string SanitizeFileName(string value)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
		return string.IsNullOrWhiteSpace(cleaned) ? Guid.NewGuid().ToString("N") : cleaned;
	}

	private string GetPublicOrigin()
	{
		var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
		var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
		var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? Request.Scheme : forwardedProto;
		var host = string.IsNullOrWhiteSpace(forwardedHost) ? Request.Host.Value : forwardedHost;

		return $"{scheme}://{host}";
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

	private static UserProfileVisibility ToDomainVisibility(UserProfileVisibilityDto? dto)
	{
		dto ??= new UserProfileVisibilityDto();

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

	private static List<string> NormalizeStringList(List<string>? values, int maxItems, int maxLength)
	{
		return (values ?? new List<string>())
			.Select(NormalizeOptional)
			.Where(value => !string.IsNullOrWhiteSpace(value))
			.Select(value => value!)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Take(maxItems)
			.Select(value => value.Length > maxLength ? value[..maxLength] : value)
			.ToList();
	}

	private static LookingForGameStatus ToDomainLookingForGame(LookingForGameStatusDto? dto)
	{
		if (dto == null)
			return new LookingForGameStatus();

		return new LookingForGameStatus
		{
			IsActive = dto.IsActive,
			SystemKey = NormalizeOptional(dto.SystemKey),
			RadiusKm = dto.RadiusKm is > 0 and <= 500 ? dto.RadiusKm : null,
			TimeNote = NormalizeOptional(dto.TimeNote),
			UpdatedAtUtc = dto.IsActive ? DateTime.UtcNow : dto.UpdatedAtUtc
		};
	}

	private static LookingForGameStatusDto ToLookingForGameDto(LookingForGameStatus status)
	{
		return new LookingForGameStatusDto
		{
			IsActive = status.IsActive,
			SystemKey = status.SystemKey,
			RadiusKm = status.RadiusKm,
			TimeNote = status.TimeNote,
			UpdatedAtUtc = status.UpdatedAtUtc
		};
	}

	private static UserDiscoverySettings ToDomainDiscoverySettings(UserDiscoverySettingsDto? dto)
	{
		dto ??= new UserDiscoverySettingsDto();

		return new UserDiscoverySettings
		{
			ShowLocations = dto.ShowLocations,
			ShowPlayers = dto.ShowPlayers,
			ShowMySessions = dto.ShowMySessions,
			ShowPublicSessions = dto.ShowPublicSessions,
			TimeWindowDays = Math.Clamp(dto.TimeWindowDays, 1, 56),
			RadiusKm = Math.Clamp(dto.RadiusKm, 10, 200),
			Latitude = dto.Latitude is >= -90 and <= 90 ? dto.Latitude : null,
			Longitude = dto.Longitude is >= -180 and <= 180 ? dto.Longitude : null,
			Zoom = Math.Clamp(dto.Zoom, 3, 18)
		};
	}

	private static UserDiscoverySettingsDto ToDiscoverySettingsDto(UserDiscoverySettings settings)
	{
		return new UserDiscoverySettingsDto
		{
			ShowLocations = settings.ShowLocations,
			ShowPlayers = settings.ShowPlayers,
			ShowMySessions = settings.ShowMySessions,
			ShowPublicSessions = settings.ShowPublicSessions,
			TimeWindowDays = settings.TimeWindowDays,
			RadiusKm = settings.RadiusKm,
			Latitude = settings.Latitude,
			Longitude = settings.Longitude,
			Zoom = settings.Zoom
		};
	}

	private static UserProfileResponse ToResponse(UserProfile user)
	{
		return new UserProfileResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			FirstName = user.FirstName,
			LastName = user.LastName,
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
			IsSystemAdmin = user.IsSystemAdmin,
			IsDevUser = user.IsDevUser,
			HideProfile = user.HideProfile,
			HideOnMap = user.HideOnMap,
			HideParticipation = user.HideParticipation,
			Visibility = ToVisibilityDto(user.Visibility ?? new UserProfileVisibility()),
			FavoriteSystemKeys = user.FavoriteSystemKeys ?? new List<string>(),
			Armies = (user.Armies ?? new List<UserArmyProfile>()).Select(a => new UserArmyProfileDto
			{
				SystemKey = a.SystemKey,
				ArmyName = a.ArmyName
			}).ToList(),
			LookingForGame = ToLookingForGameDto(user.LookingForGame ?? new LookingForGameStatus()),
			DiscoverySettings = ToDiscoverySettingsDto(user.DiscoverySettings ?? new UserDiscoverySettings())
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
		if (user.HideProfile && !isOwnProfile && !isFriend)
			throw new DomainException("Profil ist nicht öffentlich sichtbar.");
		var hiddenFields = BuildHiddenFields(user, isFriend, isOwnProfile);

		return Ok(new PublicUserProfileResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			ProfileImageUrl = user.ProfileImageUrl,
			CanBeContacted = user.CanBeContacted,
			HideProfile = user.HideProfile,
			IsFriend = isFriend,
			HiddenFields = hiddenFields,
			FavoriteSystemKeys = user.FavoriteSystemKeys ?? new List<string>(),
			Armies = (user.Armies ?? new List<UserArmyProfile>()).Select(a => new UserArmyProfileDto
			{
				SystemKey = a.SystemKey,
				ArmyName = a.ArmyName
			}).ToList(),
			LookingForGame = ToLookingForGameDto(user.LookingForGame ?? new LookingForGameStatus()),

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
