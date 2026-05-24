using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IPlayRequestService
{
	Task<List<PlayRequestResponse>> GetOpenAsync();
	Task<List<PlayRequestResponse>> GetMineAsync();
	Task<PlayRequestResponse> CreateAsync(CreatePlayRequestRequest request);
	Task<GameResponse> ConvertToSessionAsync(string id, ConvertPlayRequestRequest request);
	Task CloseAsync(string id);
}
