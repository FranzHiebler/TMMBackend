using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services;

public static class LocationRules
{
	public static List<string> NormalizeSystems(IEnumerable<string>? systems)
	{
		return systems?
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList() ?? new List<string>();
	}

	public static LocationRole? GetCurrentUserRole(Location location, string userId)
	{
		return location.Members.FirstOrDefault(m => m.UserId == userId)?.Role;
	}

	public static bool CanViewMembers(Location location, string userId)
	{
		var isMember = location.Members.Any(m => m.UserId == userId);
		return isMember || location.AccessMode == LocationAccessMode.Open;
	}

	public static bool CanEditLocation(Location location, string userId)
	{
		var role = GetCurrentUserRole(location, userId);

		return role == LocationRole.Owner ||
			   role == LocationRole.Admin ||
			   role == LocationRole.Manager;
	}

	public static bool CanManageMembers(Location location, string userId)
	{
		var role = GetCurrentUserRole(location, userId);

		return role == LocationRole.Owner ||
			   role == LocationRole.Admin;
	}

	public static bool CanAssignRole(LocationRole? actorRole, LocationRole targetRole)
	{
		if (actorRole == LocationRole.Owner)
			return targetRole != LocationRole.Owner;

		if (actorRole == LocationRole.Admin)
		{
			return targetRole == LocationRole.Manager ||
				   targetRole == LocationRole.Member ||
				   targetRole == LocationRole.Applicant;
		}

		return false;
	}

	public static bool CanModifyTarget(LocationRole? actorRole, LocationRole targetCurrentRole)
	{
		if (targetCurrentRole == LocationRole.Owner)
			return false;

		if (actorRole == LocationRole.Owner)
			return true;

		if (actorRole == LocationRole.Admin)
			return targetCurrentRole != LocationRole.Admin;

		return false;
	}
}