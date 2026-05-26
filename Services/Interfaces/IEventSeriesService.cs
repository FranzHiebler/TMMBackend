using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IEventSeriesService
{
	Task<List<EventSeriesResponse>> GetAllAsync();
	Task<EventSeriesResponse> CreateAsync(CreateEventSeriesRequest request);
	Task<EventSeriesResponse> UpdateAsync(string id, CreateEventSeriesRequest request);
	Task<GameResponse> CreateNextSessionAsync(string seriesId);
}
