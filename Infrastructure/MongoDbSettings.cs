namespace TabletopMatchMaker.Infrastructure;

public class MongoDbSettings
{
	public string ConnectionString { get; set; } = default!;
	public string DatabaseName { get; set; } = default!;
	public string GamesCollectionName { get; set; } = default!;
}