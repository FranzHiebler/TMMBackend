using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;
using TabletopMatchMaker.Services;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private const string GoogleProvider = "google";

	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IUserRepository _users;
	private readonly IUserAuthProviderRepository _providers;
	private readonly IAuthSessionService _sessions;
	private readonly AuthSettings _settings;

	public AuthController(
		IHttpClientFactory httpClientFactory,
		IUserRepository users,
		IUserAuthProviderRepository providers,
		IAuthSessionService sessions,
		IOptions<AuthSettings> settings)
	{
		_httpClientFactory = httpClientFactory;
		_users = users;
		_providers = providers;
		_sessions = sessions;
		_settings = settings.Value;
	}

	[HttpPost("google")]
	public async Task<ActionResult<AuthUserResponse>> Google([FromBody] GoogleLoginRequest request)
	{
		if (string.IsNullOrWhiteSpace(_settings.GoogleClientId))
			throw new DomainException("Google Login ist nicht konfiguriert.");

		if (string.IsNullOrWhiteSpace(request.IdToken))
			throw new DomainException("Google ID Token fehlt.");

		var googleUser = await ValidateGoogleTokenAsync(request.IdToken);
		var user = await FindOrCreateUserAsync(googleUser);
		var now = DateTime.UtcNow;
		var provider = await _providers.GetByUserAndProviderAsync(user.Id!, GoogleProvider)
			?? new UserAuthProvider
			{
				Id = ObjectId.GenerateNewId().ToString(),
				UserId = user.Id!,
				Provider = GoogleProvider,
				ProviderUserId = googleUser.Sub,
				LinkedAtUtc = now
			};

		provider.ProviderUserId = googleUser.Sub;
		provider.Email = googleUser.Email;
		provider.LastLoginAtUtc = now;
		await _providers.UpsertAsync(provider);

		SignIn(user);
		return Ok(ToAuthResponse(user));
	}

	[HttpGet("me")]
	public async Task<ActionResult<AuthUserResponse>> Me()
	{
		var session = _sessions.GetCurrentSession();
		if (session == null)
			return Unauthorized(new { error = "Nicht angemeldet." });

		var user = await _users.GetByIdAsync(session.EffectiveUserId);
		if (user == null)
			return Unauthorized(new { error = "Session ist ungültig." });

		return Ok(ToAuthResponse(user, session.IsImpersonating));
	}

	[HttpPost("logout")]
	public IActionResult Logout()
	{
		_sessions.SignOut();
		return NoContent();
	}

	private async Task<GoogleTokenInfo> ValidateGoogleTokenAsync(string idToken)
	{
		var client = _httpClientFactory.CreateClient();
		var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}";
		GoogleTokenInfo? token;
		try
		{
			token = await client.GetFromJsonAsync<GoogleTokenInfo>(url);
		}
		catch (HttpRequestException)
		{
			throw new AuthenticationRequiredException("Google Token konnte nicht geprüft werden.");
		}

		if (token == null)
			throw new AuthenticationRequiredException("Google Token konnte nicht geprüft werden.");

		if (!string.Equals(token.Aud, _settings.GoogleClientId, StringComparison.Ordinal))
			throw new AuthenticationRequiredException("Google Token gehört nicht zu dieser App.");

		if (!string.Equals(token.EmailVerified, "true", StringComparison.OrdinalIgnoreCase))
			throw new AuthenticationRequiredException("Google E-Mail ist nicht bestätigt.");

		if (string.IsNullOrWhiteSpace(token.Sub) || string.IsNullOrWhiteSpace(token.Email))
			throw new AuthenticationRequiredException("Google Token enthält keine gültigen Nutzerdaten.");

		return token;
	}

	private async Task<UserProfile> FindOrCreateUserAsync(GoogleTokenInfo googleUser)
	{
		var now = DateTime.UtcNow;
		var provider = await _providers.GetByProviderAsync(GoogleProvider, googleUser.Sub);
		var user = provider == null ? null : await _users.GetByIdAsync(provider.UserId);
		user ??= await _users.GetByEmailAsync(googleUser.Email);
		user ??= new UserProfile
		{
			Id = ObjectId.GenerateNewId().ToString(),
			DisplayName = NormalizeDisplayName(googleUser),
			CanBeContacted = true,
			Visibility = new UserProfileVisibility()
		};

		user.Email = googleUser.Email;
		user.FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? googleUser.GivenName : user.FirstName;
		user.LastName = string.IsNullOrWhiteSpace(user.LastName) ? googleUser.FamilyName : user.LastName;
		user.DisplayName = IsFranz(googleUser.Email) ? "Franz H." : NormalizeDisplayName(googleUser, user.DisplayName);
		user.ProfileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl) ? googleUser.Picture : user.ProfileImageUrl;

		if (IsFranz(googleUser.Email))
		{
			user.FirstName = "Franz";
			user.LastName = "Hiebler";
			user.IsSystemAdmin = true;
			user.IsDevUser = false;
			user.HideProfile = false;
			user.HideOnMap = false;
			user.HideParticipation = false;
			user.Visibility = new UserProfileVisibility
			{
				Email = ProfileFieldVisibility.Public,
				PhoneNumber = ProfileFieldVisibility.Private,
				StreetAddress = ProfileFieldVisibility.Public,
				PostalCode = ProfileFieldVisibility.Public,
				City = ProfileFieldVisibility.Public,
				TabletopTo = ProfileFieldVisibility.Public,
				TabletopHerald = ProfileFieldVisibility.Public,
				T3 = ProfileFieldVisibility.Public,
				NewRecruit = ProfileFieldVisibility.Public,
				BestSportsPairings = ProfileFieldVisibility.Public
			};
		}

		await _users.UpsertAsync(user);
		return user;
	}

	private void SignIn(UserProfile user)
	{
		_sessions.SignIn(new AuthSession
		{
			RealUserId = user.Id!,
			EffectiveUserId = user.Id!,
			DisplayName = user.DisplayName,
			Email = user.Email,
			IsSystemAdmin = user.IsSystemAdmin,
			IsDevUser = user.IsDevUser,
			ExpiresAtUtc = DateTime.UtcNow.AddDays(Math.Clamp(_settings.SessionDays, 1, 60))
		});
	}

	private bool IsFranz(string email)
	{
		return string.Equals(email, _settings.SystemAdminEmail, StringComparison.OrdinalIgnoreCase);
	}

	private static string NormalizeDisplayName(GoogleTokenInfo googleUser, string? fallback = null)
	{
		if (!string.IsNullOrWhiteSpace(googleUser.Name))
			return googleUser.Name.Trim();

		if (!string.IsNullOrWhiteSpace(fallback))
			return fallback.Trim();

		return googleUser.Email.Split('@')[0];
	}

	private static AuthUserResponse ToAuthResponse(UserProfile user, bool isImpersonating = false)
	{
		return new AuthUserResponse
		{
			UserId = user.Id!,
			DisplayName = user.DisplayName,
			Email = user.Email,
			IsSystemAdmin = user.IsSystemAdmin,
			IsDevUser = user.IsDevUser,
			IsImpersonating = isImpersonating
		};
	}

	private sealed class GoogleTokenInfo
	{
		public string Sub { get; set; } = "";
		public string Aud { get; set; } = "";
		public string Email { get; set; } = "";
		[JsonPropertyName("email_verified")]
		public string EmailVerified { get; set; } = "";
		public string? Name { get; set; }
		[JsonPropertyName("given_name")]
		public string? GivenName { get; set; }
		[JsonPropertyName("family_name")]
		public string? FamilyName { get; set; }
		public string? Picture { get; set; }
	}
}
