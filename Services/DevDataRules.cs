using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services;

public static class DevDataRules
{
	public static bool IsDevLocation(Location? location, ISet<string>? devUserIds = null)
	{
		if (location == null)
			return false;

		if (location.IsDevLocation)
			return true;

		if (location.Name.Contains("DevOrb", StringComparison.OrdinalIgnoreCase))
			return true;

		return devUserIds != null && location.Members.Any(member =>
			!string.IsNullOrWhiteSpace(member.UserId) && devUserIds.Contains(member.UserId));
	}

	public static bool IsDevGame(GameSession game, ISet<string> devUserIds)
	{
		if (devUserIds.Contains(game.Host.UserId))
			return true;

		return game.Tables.Any(table =>
			table.AssignedPlayers.Any(player => devUserIds.Contains(player.UserId)) ||
			table.Applications.Any(application => devUserIds.Contains(application.Player.UserId))) ||
			game.Waitlist.Any(entry => devUserIds.Contains(entry.Player.UserId)) ||
			game.Invitations.Any(invitation => devUserIds.Contains(invitation.User.UserId)) ||
			game.DateOptions.Any(option => option.Votes.Any(vote => devUserIds.Contains(vote.UserId)));
	}
}
