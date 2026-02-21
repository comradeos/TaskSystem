namespace TaskSystem.Api.DTOs;

public class TimelineEventDto
{
    public string EventType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Data { get; set; }

    public DateTime CreatedAt { get; set; }
}