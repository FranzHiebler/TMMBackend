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
			LocationId = d["_id"].ToString(),
			DistanceInMeters = d["distanceInMeters"].ToDouble()
		}).ToList();


	}
}