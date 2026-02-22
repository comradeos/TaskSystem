using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace TaskSystem.Infrastructure.Database;

public class MongoConnection
{
    private readonly IMongoDatabase _database;

    public MongoConnection(IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("MongoTimeline");

        string? databaseName = configuration["MONGO_TIMELINE_DATABASE"];

        MongoClient client = new MongoClient(connectionString);

        _database = client.GetDatabase(databaseName);
    }

    public IMongoDatabase GetDatabase()
    {
        return _database;
    }
}