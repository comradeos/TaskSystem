namespace TaskSystem.Application.Abstractions;

public sealed class TimelineEvent
{
    public string EntityType { get; init; } = null!;
    public string EntityId { get; init; } = null!;
    public string Action { get; init; } = null!;
    public string Data { get; init; } = null!;
    public DateTime OccurredAtUtc { get; init; }

    public TimelineEvent(
        string entityType,
        string entityId,
        string action,
        string data,
        DateTime occurredAtUtc)
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        Data = data;
        OccurredAtUtc = occurredAtUtc;
    }
    
    public TimelineEvent() { }
}