using MongoDB.Bson;
using MongoDB.Driver;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Infrastructure.Mongo;

public class MongoTimelineRepository : ITimelineRepository
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoTimelineRepository(string connectionString, string databaseName)
    {
        MongoClient client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase(databaseName);

        _collection = database.GetCollection<BsonDocument>("timeline");

        CreateIndexes();
    }

    private void CreateIndexes()
    {
        IndexKeysDefinition<BsonDocument> indexKeys =
            Builders<BsonDocument>.IndexKeys
                .Ascending("EntityType")
                .Ascending("EntityId")
                .Ascending("OccurredAtUtc");

        CreateIndexModel<BsonDocument> indexModel =
            new CreateIndexModel<BsonDocument>(indexKeys);

        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task AddAsync(TimelineEvent evt, CancellationToken ct = default)
    {
        BsonDocument doc = new BsonDocument
        {
            { "EntityType", evt.EntityType },
            { "EntityId", evt.EntityId },
            { "Action", evt.Action },
            { "Data", evt.Data },
            { "OccurredAtUtc", evt.OccurredAtUtc }
        };

        await _collection.InsertOneAsync(doc, cancellationToken: ct);
    }

    public async Task TryAddAsync(TimelineEvent evt, CancellationToken ct = default)
    {
        try
        {
            await AddAsync(evt, ct);
        }
        catch
        {
            // логгирование ошибки нужно
        }
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken ct = default
    )
    {
        FilterDefinition<BsonDocument> entityTypeFilter =
            Builders<BsonDocument>.Filter.Eq("EntityType", entityType);

        FilterDefinition<BsonDocument> entityIdFilter;

        if (int.TryParse(entityId, out var intId))
        {
            entityIdFilter =
                Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Eq("EntityId", entityId),
                    Builders<BsonDocument>.Filter.Eq("EntityId", intId)
                );
        }
        else
        {
            entityIdFilter =
                Builders<BsonDocument>.Filter.Eq("EntityId", entityId);
        }

        FilterDefinition<BsonDocument> filter =
            Builders<BsonDocument>.Filter.And(
                entityTypeFilter,
                entityIdFilter
            );

        List<BsonDocument> documents =
            await _collection
                .Find(filter)
                .Sort(
                    Builders<BsonDocument>.Sort.Ascending("OccurredAtUtc")
                )
                .ToListAsync(ct);

        List<TimelineEvent> result = new List<TimelineEvent>();

        foreach (BsonDocument doc in documents)
        {
            BsonValue idValue = doc["EntityId"];

            string? entityIdString = idValue.IsInt32 ? idValue.AsInt32.ToString() : idValue.AsString;

            TimelineEvent timelineEvent = new TimelineEvent(
                doc["EntityType"].AsString,
                entityIdString,
                doc["Action"].AsString,
                doc["Data"].AsString,
                doc["OccurredAtUtc"].ToUniversalTime()
            );

            result.Add(timelineEvent);
        }

        return result;
    }

    public async Task PingAsync(CancellationToken ct = default)
    {
        await _collection.Database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: ct
        );
    }
}