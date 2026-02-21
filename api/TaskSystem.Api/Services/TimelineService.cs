using System.Text.Json;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Enums;
using TaskSystem.Domain.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Api.Services;

public class TimelineService
{
    private readonly ITimelineRepository _repository;

    public TimelineService(ITimelineRepository repository)
    {
        _repository = repository;
    }

    private async Task AddAsync(
        TimelineEventType eventType,
        string entityType,
        int entityId,
        int? userId,
        string? userName,
        object? data = null)
    {
        var timelineEvent = new TimelineEvent
        {
            Id = Guid.NewGuid().ToString(),
            EventType = eventType.ToString(),
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserName = userName,
            Data = data is null ? null : JsonSerializer.Serialize(data),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(timelineEvent);
    }

    public Task UserCreated(int id, int actorId, string actorName, object? data)
        => AddAsync(TimelineEventType.UserCreated, "User", id, actorId, actorName, data);

    public Task ProjectCreated(int id, int actorId, string actorName, object? data)
        => AddAsync(TimelineEventType.ProjectCreated, "Project", id, actorId, actorName, data);

    public Task TaskCreated(int id, int actorId, string actorName, object? data)
        => AddAsync(TimelineEventType.TaskCreated, "Task", id, actorId, actorName, data);

    public Task TaskUpdated(int id, int actorId, string actorName, object? data)
        => AddAsync(TimelineEventType.TaskUpdated, "Task", id, actorId, actorName, data);

    public Task CommentAdded(int taskId, int actorId, string actorName, object? data)
        => AddAsync(TimelineEventType.CommentAdded, "Task", taskId, actorId, actorName, data);
}