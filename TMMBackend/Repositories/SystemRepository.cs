using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;

namespace TabletopMatchMaker.Repositories;

public class SystemRepository
{
	private readonly IMongoCollection<SystemDefinition> _systems;

	public SystemRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_systems = database.GetCollection<SystemDefinition>("systems");
	}

	public async Task<List<SystemDefinition>> GetAllAsync()
	{
		return await _systems.Find(FilterDefinition<SystemDefinition>.Empty).ToListAsync();
	}
}