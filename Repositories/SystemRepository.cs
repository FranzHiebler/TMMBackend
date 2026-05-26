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
			MarkerColor = NormalizeOptional(request.MarkerColor),
			Category = NormalizeCategory(request.Category)
		};

		await _systems.InsertOneAsync(system);
		return system;
	}

	public async Task<SystemDefinition> UpdateAsync(string key, CreateSystemRequest request)
	{
		var normalizedKey = NormalizeRequired(key, "Key").ToLowerInvariant();
		var system = await _systems.Find(x => x.Key == normalizedKey).FirstOrDefaultAsync()
			?? new SystemDefinition { Key = normalizedKey };

		system.Name = NormalizeRequired(request.Name, "Name");
		system.ShortCode = NormalizeOptional(request.ShortCode);
		system.Color = NormalizeOptional(request.Color);
		system.MarkerColor = NormalizeOptional(request.MarkerColor);
		system.Category = NormalizeCategory(request.Category);

		await _systems.ReplaceOneAsync(
			x => x.Key == normalizedKey,
			system,
			new ReplaceOptions { IsUpsert = true });

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

	private static string NormalizeCategory(string? value)
	{
		var category = NormalizeOptional(value) ?? "Tabletop";
		return category is "Tabletop" or "Brettspiel" or "Rollenspiel" or "TCG" or "Sonstiges"
			? category
			: "Tabletop";
	}
}
