using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoExport
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var conn = "mongodb+srv://TMM:kcSV7XkxEaDHKs5s@cluster0.zhe0k7j.mongodb.net/";
			var dbName = "tmm";

			var output = Path.Combine(AppContext.BaseDirectory, "mongo-structure.txt");

			var client = new MongoClient(conn);
			var db = client.GetDatabase(dbName);

			using var writer = new StreamWriter(output, false);

			writer.WriteLine("MONGO STRUCTURE EXPORT");
			writer.WriteLine($"Generated: {DateTime.Now}");
			writer.WriteLine($"DB: {dbName}");
			writer.WriteLine("========================================");

			var collections = await db.ListCollectionNames().ToListAsync();

			foreach (var name in collections.OrderBy(x => x))
			{
				writer.WriteLine($"\n=== COLLECTION: {name} ===");

				var col = db.GetCollection<BsonDocument>(name);

				var count = await col.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
				writer.WriteLine($"COUNT: {count}");

				writer.WriteLine("INDEXES:");
				var indexes = await col.Indexes.List().ToListAsync();
				foreach (var idx in indexes)
					writer.WriteLine(idx.ToJson());

				writer.WriteLine("DOCUMENTS:");

				var docs = await col.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();

				if (docs.Count == 0)
				{
					writer.WriteLine("No documents");
				}
				else
				{
					foreach (var d in docs)
					{
						writer.WriteLine(Mask(d).ToJson());
						writer.WriteLine("----------------------------------------");
					}
				}
			}

			Console.WriteLine($"Fertig: {output}");
		}

		static BsonDocument Mask(BsonDocument doc)
		{
			var result = new BsonDocument();

			foreach (var el in doc)
			{
				var key = el.Name.ToLowerInvariant();

				if (key.Contains("password") ||
					key.Contains("token") ||
					key.Contains("secret") ||
					key.Contains("apikey") ||
					key.Contains("connectionstring"))
				{
					result[el.Name] = "***MASKED***";
				}
				else if (el.Value.IsBsonDocument)
				{
					result[el.Name] = Mask(el.Value.AsBsonDocument);
				}
				else if (el.Value.IsBsonArray)
				{
					var arr = new BsonArray();

					foreach (var item in el.Value.AsBsonArray)
					{
						if (item.IsBsonDocument)
							arr.Add(Mask(item.AsBsonDocument));
						else
							arr.Add(item);
					}

					result[el.Name] = arr;
				}
				else
				{
					result[el.Name] = el.Value;
				}
			}

			return result;
		}
	}
}