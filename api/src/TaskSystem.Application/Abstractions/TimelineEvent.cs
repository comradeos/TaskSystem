namespace TaskSystem.Application.Abstractions;

public sealed class TimelineEvent
{
    public string EntityType { get; init; } = default!;

    public string EntityId { get; init; } = default!;

    public string Action { get; init; } = default!;

    public string Data { get; init; } = default!;

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

    // Пустой конструктор нужен Mongo
    public TimelineEvent() { }
}