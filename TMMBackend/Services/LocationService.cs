using MongoDB.Driver.GeoJsonObjectModel;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Repositories;
using TabletopMatchMaker.Services.Interfaces;

namespace TabletopMatchMaker.Services;

public class LocationService : ILocationService
{
	private readonly LocationRepository _repository;
	private readonly ICurrentUserService _currentUser;

	public LocationService(LocationRepository repository, ICurrentUserService currentUser)
	{
		_repository = repository;
		_currentUser = currentUser;
	}

	public Task<Location?> GetByIdAsync(string id)
	{
		return _repository.GetByIdAsync(id);
	}

	public async Task<List<LocationOptionResponse>> GetAllAsync()
	{
		var locations = await _repository.GetAllAsync();
		return locations.Select(LocationMapper.ToOptionResponse).ToList();
	}

	public async Task<LocationResponse> CreateAsync(CreateLocationRequest request)
	{
		var location = new Location
		{
			Name = request.Name,
			City = request.City,
			Address = request.Address,
			SystemKeys = LocationRules.NormalizeSystems(request.SystemKeys),
			Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
				new GeoJson2DGeographicCoordinates(request.Longitude, request.Latitude)
			),
			Members =
			[
				new LocationMember
				{
					UserId = _currentUser.UserId,
					DisplayName = _currentUser.DisplayName,
					Role = LocationRole.Owner
				}
			]
		};

		await _repository.CreateAsync(location);
		return LocationMapper.ToResponse(location, _currentUser.UserId);
	}

	public async Task<List<LocationResponse>> GetMineAsync()
	{
		var locations = await _repository.GetForUserAsync(_currentUser.UserId);

		return locations
			.Select(location => LocationMapper.ToResponse(location, _currentUser.UserId))
			.ToList();
	}

	public async Task UpdateAsync(string id, CreateLocationRequest request)
	{
		var location = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Location wurde nicht gefunden.");

		if (!LocationRules.CanEditLocation(location, _currentUser.UserId))
			throw new UnauthorizedAccessException("Du darfst diese Location nicht bearbeiten.");

		location.Name = request.Name;
		location.City = request.City;
		location.Address = request.Address;
		location.SystemKeys = LocationRules.NormalizeSystems(request.SystemKeys);
		location.Geo = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
			new GeoJson2DGeographicCoordinates(request.Longitude, request.Latitude)
		);

		await _repository.UpdateAsync(location);
	}

	public async Task<List<LocationMemberResponse>> GetMembersAsync(string id)
	{
		var location = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Location wurde nicht gefunden.");

		if (!LocationRules.CanViewMembers(location, _currentUser.UserId))
			throw new UnauthorizedAccessException("Du darfst die Mitglieder nicht sehen.");

		return location.Members.Select(LocationMapper.ToMemberResponse).ToList();
	}

	public async Task<List<LocationResponse>> SearchNearbyAsync(SearchNearbyLocationsRequest request)
	{
		var locations = await _repository.FindNearbyLocationsAsync(
			request.Latitude,
			request.Longitude,
			request.RadiusInMeters);

		return locations
			.Where(location => !location.Members.Any(m => m.UserId == _currentUser.UserId))
			.Where(location => !location.JoinRequests.Any(r =>
				r.UserId == _currentUser.UserId &&
				r.Status == LocationJoinRequestStatus.Pending))
			.Where(location =>
				string.IsNullOrWhiteSpace(request.SystemKey) ||
				location.SystemKeys.Contains(request.SystemKey, StringComparer.OrdinalIgnoreCase))
			.Select(location => LocationMapper.ToResponse(location, _currentUser.UserId))
			.ToList();
	}

	public async Task RequestMembershipAsync(string id, RequestLocationMembershipRequest request)
	{
		var location = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Location wurde nicht gefunden.");

		if (location.Members.Any(m => m.UserId == _currentUser.UserId))
			throw new GameActionException("Du bist dort bereits Mitglied.");

		var existing = location.JoinRequests.FirstOrDefault(r =>
			r.UserId == _currentUser.UserId &&
			r.Status == LocationJoinRequestStatus.Pending);

		if (existing != null)
			throw new GameActionException("Du hast dort bereits angefragt.");

		location.JoinRequests.Add(new LocationJoinRequest
		{
			RequestId = Guid.NewGuid().ToString("N"),
			UserId = _currentUser.UserId,
			DisplayName = _currentUser.DisplayName,
			Message = request.Message,
			Status = LocationJoinRequestStatus.Pending,
			CreatedAt = DateTime.UtcNow
		});

		await _repository.UpdateAsync(location);
	}

	public async Task UpsertMemberAsync(string id, UpsertLocationMemberRequest request)
	{
		var location = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Location wurde nicht gefunden.");

		if (!LocationRules.CanManageMembers(location, _currentUser.UserId))
			throw new UnauthorizedAccessException("Du darfst Mitglieder nicht verwalten.");

		var actorRole = LocationRules.GetCurrentUserRole(location, _currentUser.UserId);
		var member = location.Members.FirstOrDefault(m => m.UserId == request.UserId);

		if (!LocationRules.CanAssignRole(actorRole, request.Role))
			throw new GameActionException("Diese Rolle darfst du nicht vergeben.");

		if (member != null && !LocationRules.CanModifyTarget(actorRole, member.Role))
			throw new GameActionException("Dieses Mitglied darfst du nicht ändern.");

		if (member == null)
		{
			location.Members.Add(new LocationMember
			{
				UserId = request.UserId,
				DisplayName = request.DisplayName,
				Role = request.Role
			});
		}
		else
		{
			member.DisplayName = request.DisplayName;
			member.Role = request.Role;
		}

		await _repository.UpdateAsync(location);
	}

	public async Task RemoveMemberAsync(string id, string userId)
	{
		var location = await _repository.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Location wurde nicht gefunden.");

		if (!LocationRules.CanManageMembers(location, _currentUser.UserId))
			throw new UnauthorizedAccessException("Du darfst Mitglieder nicht verwalten.");

		var actorRole = LocationRules.GetCurrentUserRole(location, _currentUser.UserId);
		var member = location.Members.FirstOrDefault(m => m.UserId == userId)
			?? throw new KeyNotFoundException("Mitglied wurde nicht gefunden.");

		if (member.Role == LocationRole.Owner)
			throw new GameActionException("Owner kann nicht entfernt werden.");

		if (!LocationRules.CanModifyTarget(actorRole, member.Role))
			throw new GameActionException("Dieses Mitglied darfst du nicht entfernen.");

		location.Members.Remove(member);
		await _repository.UpdateAsync(location);
	}
}