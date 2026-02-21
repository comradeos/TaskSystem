using MongoDB.Bson;
using MongoDB.Driver;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Infrastructure.Mongo;

public sealed class MongoTimelineRepository : ITimelineRepository
{
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoTimelineRepository(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        _collection = database.GetCollection<BsonDocument>("timeline");

        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<BsonDocument>.IndexKeys
            .Ascending("EntityType")
            .Ascending("EntityId")
            .Ascending("OccurredAtUtc");

        var indexModel = new CreateIndexModel<BsonDocument>(indexKeys);

        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task AddAsync(TimelineEvent evt, CancellationToken ct = default)
    {
        var doc = new BsonDocument
        {
            { "EntityType", evt.EntityType },
            { "EntityId", evt.EntityId }, // всегда строка
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
            // best-effort: не падаем наружу
        }
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken ct = default)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("EntityType", entityType),
            Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Eq("EntityId", entityId),
                int.TryParse(entityId, out var intId)
                    ? Builders<BsonDocument>.Filter.Eq("EntityId", intId)
                    : Builders<BsonDocument>.Filter.Exists("___never___")
            )
        );

        var documents = await _collection
            .Find(filter)
            .Sort(Builders<BsonDocument>.Sort.Ascending("OccurredAtUtc"))
            .ToListAsync(ct);

        var result = new List<TimelineEvent>();

        foreach (var doc in documents)
        {
            var idValue = doc["EntityId"];
            string entityIdString = idValue.IsInt32
                ? idValue.AsInt32.ToString()
                : idValue.AsString;

            result.Add(new TimelineEvent(
                doc["EntityType"].AsString,
                entityIdString,
                doc["Action"].AsString,
                doc["Data"].AsString,
                doc["OccurredAtUtc"].ToUniversalTime()
            ));
        }

        return result;
    }

    public async Task PingAsync(CancellationToken ct = default)
    {
        await _collection.Database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: ct);
    }
}