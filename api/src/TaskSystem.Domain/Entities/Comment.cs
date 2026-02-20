namespace TaskSystem.Domain.Entities;

public class Comment
{
    public Guid Id { get; init; }
    public Guid TaskId { get; init; }
    public Guid AuthorId { get; init; }

    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}