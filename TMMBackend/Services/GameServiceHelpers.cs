using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services;

public static class GameServiceHelpers
{
	public static GameTable GetTableOrThrow(GameSession game, string tableId)
	{
		return game.Tables.FirstOrDefault(x => x.TableId == tableId)
			?? throw new DomainException("Tisch nicht gefunden.");
	}

	public static List<string>? NormalizeSystems(IEnumerable<string>? systems)
	{
		return systems?
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	public static GameTable? ResolveProposalTable(GameSession game, CreateChangeProposalRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.TableId))
			return null;

		return game.Tables.FirstOrDefault(t => t.TableId == request.TableId)
			?? throw new DomainException("Tisch nicht gefunden.");
	}

	public static GameTable? ResolveExistingProposalTable(GameSession game, GameChangeProposal proposal)
	{
		if (string.IsNullOrWhiteSpace(proposal.TableId))
			return null;

		return game.Tables.FirstOrDefault(t => t.TableId == proposal.TableId)
			?? throw new DomainException("Tisch nicht gefunden.");
	}
}