using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services;

public static class LocationMapper
{
	public static LocationOptionResponse ToOptionResponse(Location location)
	{
		return new LocationOptionResponse
		{
			Id = location.Id,
			Name = location.Name,
			City = location.City
		};
	}

	public static LocationResponse ToResponse(Location location, string currentUserId)
	{
		var member = location.Members.FirstOrDefault(m => m.UserId == currentUserId);

		return new LocationResponse
		{
			Id = location.Id,
			Name = location.Name,
			City = location.City,
			Address = location.Address,
			Latitude = location.Geo?.Coordinates.Latitude,
			Longitude = location.Geo?.Coordinates.Longitude,
			Role = member?.Role.ToString(),
			IsOpen = location.AccessMode == LocationAccessMode.Open,
			SystemKeys = location.SystemKeys,
			HasPendingJoinRequest = location.JoinRequests.Any(r =>
				r.UserId == currentUserId &&
				r.Status == LocationJoinRequestStatus.Pending)
		};
	}

	public static LocationMemberResponse ToMemberResponse(LocationMember member)
	{
		return new LocationMemberResponse
		{
			UserId = member.UserId ?? "",
			DisplayName = member.DisplayName,
			Role = member.Role.ToString()
		};
	}

	public static LocationJoinRequestResponse ToJoinRequestResponse(LocationJoinRequest request)
	{
		return new LocationJoinRequestResponse
		{
			Id = request.RequestId,
			UserId = request.UserId,
			DisplayName = request.DisplayName,
			Message = request.Message,
			Status = request.Status.ToString(),
			CreatedAt = request.CreatedAt
		};
	}
}