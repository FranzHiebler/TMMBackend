using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TabletopMatchMaker.Domain;
using TabletopMatchMaker.Infrastructure;

namespace TabletopMatchMaker.Repositories;

public class LocationRepository
{
	private readonly IMongoCollection<Location> _locations;

	public LocationRepository(IOptions<MongoDbSettings> settings)
	{
		var client = new MongoClient(settings.Value.ConnectionString);
		var database = client.GetDatabase(settings.Value.DatabaseName);
		_locations = database.GetCollection<Location>("locations");
	}

	public async Task<List<Location>> GetAllAsync()
	{
		return await _locations.Find(FilterDefinition<Location>.Empty).ToListAsync();
	}

	public async Task<List<Location>> GetByIdsAsync(List<string> ids)
	{
		return await _locations
			.Find(x => ids.Contains(x.Id!))
			.ToListAsync();
	}
	public async Task CreateAsync(Location location)
	{
		await _locations.InsertOneAsync(location);
	}

	public async Task<List<NearbyLocationResult>> FindNearbyAsync(
		double lat,
		double lng,
		double radiusInMeters)
	{
		var pipeline = new[]
		{
			new BsonDocument("$geoNear", new BsonDocument
			{
				{ "near", new BsonDocument
					{
						{ "type", "Point" },
						{ "coordinates", new BsonArray { lng, lat } }
					}
				},
				{ "distanceField", "distanceInMeters" },
				{ "maxDistance", radiusInMeters },
				{ "spherical", true }
			})
		};

		var docs = await _locations.Aggregate<BsonDocument>(pipeline).ToListAsync();

		return docs.Select(d => new NearbyLocationResult
		{
			LocationId = d["_id"].AsObjectId.ToString(),
			Name = d["name"].AsString,
			City = d["city"].AsString,
			DistanceInMeters = d["distanceInMeters"].ToDouble()
		}).ToList();

	}

	public async Task<List<Location>> FindNearbyLocationsAsync(
		double lat,
		double lng,
		double radiusInMeters)
	{
		var nearby = await FindNearbyAsync(lat, lng, radiusInMeters);
		if (nearby.Count == 0)
			return new List<Location>();

		var ids = nearby.Select(x => x.LocationId).ToList();
		var locations = await GetByIdsAsync(ids);

		return locations
			.OrderBy(location => ids.IndexOf(location.Id!))
			.ToList();
	}

	public async Task<List<Location>> GetForUserAsync(string userId)
	{
		var memberFilter = Builders<Location>.Filter.ElemMatch(
			x => x.Members,
			m => m.UserId == userId
		);

		var openFilter = Builders<Location>.Filter.Eq(
			x => x.AccessMode,
			LocationAccessMode.Open
		);

		var filter = Builders<Location>.Filter.Or(memberFilter, openFilter);

		return await _locations.Find(filter).ToListAsync();
	}

	public async Task<Location?> GetByIdAsync(string id)
	{
		return await _locations
			.Find(x => x.Id == id)
			.FirstOrDefaultAsync();
	}

	public async Task UpdateAsync(Location location)
	{
		await _locations.ReplaceOneAsync(x => x.Id == location.Id, location);
	}
}
