using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Dtos;
using TabletopMatchMaker.Infrastructure;
using TabletopMatchMaker.Repositories.Interfaces;

namespace TabletopMatchMaker.Repositories;

public class SystemRepository : ISystemRepository
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
		return await _systems
			.Find(FilterDefinition<SystemDefinition>.Empty)
			.SortBy(x => x.Name)
			.ToListAsync();
	}

	public async Task<SystemDefinition> CreateAsync(CreateSystemRequest request)
	{
		var normalizedKey = NormalizeRequired(request.Key, "Key").ToLowerInvariant();
		var existing = await _systems.Find(x => x.Key == normalizedKey).FirstOrDefaultAsync();

		if (existing != null)
			return existing;

		var system = new SystemDefinition
		{
			Key = normalizedKey,
			Name = NormalizeRequired(request.Name, "Name"),
			ShortCode = NormalizeOptional(request.ShortCode),
			Color = NormalizeOptional(request.Color),
			MarkerColor = NormalizeOptional(request.MarkerColor)
		};

		await _systems.InsertOneAsync(system);
		return system;
	}

	private static string NormalizeRequired(string? value, string label)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException($"{label} ist erforderlich.");

		return value.Trim();
	}

	private static string? NormalizeOptional(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}
}