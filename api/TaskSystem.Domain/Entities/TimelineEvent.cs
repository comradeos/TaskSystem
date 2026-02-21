namespace TaskSystem.Domain.Entities;

public class TimelineEvent
{
    public string Id { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Data { get; set; }

    public DateTime CreatedAt { get; set; }
}