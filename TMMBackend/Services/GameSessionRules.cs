using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Services;

public static class GameSessionRules
{
	public static bool IsUserAlreadyAssigned(GameSession game, string userId)
	{
		return game.Tables.Any(t => t.AssignedPlayers.Any(p => p.UserId == userId));
	}

	public static bool SystemMatches(List<string> systems, string? systemKey)
	{
		if (systems.Count == 0)
			return true;

		if (systems.Contains("egal", StringComparer.OrdinalIgnoreCase))
			return true;

		if (string.IsNullOrWhiteSpace(systemKey))
			return false;

		return systems.Contains(systemKey, StringComparer.OrdinalIgnoreCase);
	}

	public static void UpdateSessionState(GameSession game)
	{
		var hasOpenSlot = game.Tables.Any(t => t.AssignedPlayers.Count < t.MaxPlayers);
		game.Status = hasOpenSlot ? GameSessionState.Open : GameSessionState.Full;
	}

	public static GameChangeProposal GetPendingChangeProposal(GameSession game, string proposalId)
	{
		var proposal = game.ChangeProposals.FirstOrDefault(p => p.ProposalId == proposalId);

		if (proposal == null)
			throw new DomainException("Änderungsvorschlag nicht gefunden.");

		if (proposal.Status != ChangeProposalStatus.Pending)
			throw new DomainException("Dieser Änderungsvorschlag wurde bereits bearbeitet.");

		return proposal;
	}
}