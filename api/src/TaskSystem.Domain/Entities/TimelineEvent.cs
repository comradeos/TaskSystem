namespace TaskSystem.Domain.Entities;

public class TimelineEvent
{
    public Guid Id { get; init; }
    public Guid TaskId { get; init; }
    public string EventType { get; init; } = null!;
    public string Payload { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}