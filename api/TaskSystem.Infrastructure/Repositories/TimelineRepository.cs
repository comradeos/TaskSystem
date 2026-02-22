using MongoDB.Driver;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Infrastructure.Repositories;

public class TimelineRepository : ITimelineRepository
{
    private readonly IMongoCollection<TimelineEvent> _collection;

    public TimelineRepository(MongoConnection connection)
    {
        IMongoDatabase database = connection.GetDatabase();
        
        _collection = database.GetCollection<TimelineEvent>("timeline");
    }

    public async Task AddAsync(TimelineEvent timelineEvent)
    {
        await _collection.InsertOneAsync(timelineEvent);
    }

    public async Task<IEnumerable<TimelineEvent>> GetByEntityAsync(
        string entityType,
        int entityId)
    {
        FilterDefinition<TimelineEvent>? filter = Builders<TimelineEvent>.Filter.And(
            Builders<TimelineEvent>.Filter.Eq(x => x.EntityType, entityType),
            Builders<TimelineEvent>.Filter.Eq(x => x.EntityId, entityId)
        );

        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}