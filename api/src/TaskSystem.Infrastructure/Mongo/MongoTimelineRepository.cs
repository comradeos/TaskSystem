using MongoDB.Bson;
using MongoDB.Driver;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Infrastructure.Mongo;

public sealed class MongoTimelineRepository : ITimelineRepository
{
    private readonly IMongoCollection<TimelineEvent> _collection;

    public MongoTimelineRepository(string connectionString, string databaseName)
    {
        MongoClient client = new MongoClient(connectionString);
        IMongoDatabase? database = client.GetDatabase(databaseName);

        _collection = database.GetCollection<TimelineEvent>("timeline");

        CreateIndexes();
    }

    private void CreateIndexes()
    {
        IndexKeysDefinition<TimelineEvent>? indexKeys = Builders<TimelineEvent>.IndexKeys
            .Ascending(x => x.EntityType)
            .Ascending(x => x.EntityId)
            .Ascending(x => x.OccurredAtUtc);

        CreateIndexModel<TimelineEvent> indexModel = new CreateIndexModel<TimelineEvent>(indexKeys);

        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task AddAsync(TimelineEvent evt, CancellationToken ct = default)
    {
        Console.WriteLine("TIMELINE WRITE");
        
        await _collection.InsertOneAsync(evt, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetByEntityAsync(string entityType, string entityId, CancellationToken ct = default)
    {
        FilterDefinition<TimelineEvent>? filter = Builders<TimelineEvent>.Filter.And(
            Builders<TimelineEvent>.Filter.Eq(x => x.EntityType, entityType),
            Builders<TimelineEvent>.Filter.Eq(x => x.EntityId, entityId)
        );

        return await _collection
            .Find(filter)
            .SortBy(x => x.OccurredAtUtc)
            .ToListAsync(ct);
    }

    public async Task PingAsync(CancellationToken ct = default)
    {
        await _collection.Database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: ct);
    }
}