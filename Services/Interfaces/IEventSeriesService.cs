using TabletopMatchMaker.Dtos;

namespace TabletopMatchMaker.Services.Interfaces;

public interface IEventSeriesService
{
	Task<List<EventSeriesResponse>> GetAllAsync();
	Task<EventSeriesResponse> CreateAsync(CreateEventSeriesRequest request);
	Task<GameResponse> CreateNextSessionAsync(string seriesId);
}
