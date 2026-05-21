using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IGameAssignmentService
{
	Task JoinTableAsync(string gameId, string tableId, JoinTableRequest request);
	Task ApplyAsync(string gameId, ApplyToGameRequest request);
	Task AssignPlayerToTableAsync(string gameId, string tableId, AssignPlayerToTableRequest request);
	Task RejectApplicationAsync(string gameId, string applicationId);
	Task RemovePlayerFromTableAsync(string gameId, string tableId, string userId);
	Task MovePlayerToTableAsync(string gameId, string userId, MovePlayerToTableRequest request);
}