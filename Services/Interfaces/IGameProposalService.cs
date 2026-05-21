using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameProposalService
{
	Task<GameResponse> CreateChangeProposalAsync(string gameId, CreateChangeProposalRequest request);
	Task<GameResponse> AcceptChangeProposalAsync(string gameId, string proposalId);
	Task<GameResponse> RejectChangeProposalAsync(string gameId, string proposalId);
}