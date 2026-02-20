using MongoDB.Bson;
using MongoDB.Driver;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Mongo;

public class MongoTimelineRepository : ITimelineRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<TimelineEvent> _collection;

    public MongoTimelineRepository(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _collection = _database.GetCollection<TimelineEvent>("timeline");
    }

    public async Task AddEventAsync(TimelineEvent timelineEvent)
    {
        await _collection.InsertOneAsync(timelineEvent);
    }

    public async Task<IEnumerable<TimelineEvent>> GetByTaskIdAsync(Guid taskId)
    {
        var filter = Builders<TimelineEvent>
            .Filter.Eq(x => x.TaskId, taskId);

        var events = await _collection
            .Find(filter)
            .SortBy(x => x.CreatedAt)
            .ToListAsync();

        return events;
    }

    public async Task PingAsync()
    {
        await _database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1));
    }
}