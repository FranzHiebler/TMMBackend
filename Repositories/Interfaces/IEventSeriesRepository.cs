using TabletopMatchMaker.Domain;

namespace TabletopMatchMaker.Repositories.Interfaces;

public interface IEventSeriesRepository
{
	Task CreateAsync(EventSeries series);
	Task UpdateAsync(EventSeries series);
	Task<EventSeries?> GetByIdAsync(string id);
	Task<List<EventSeries>> GetAllAsync();
}
