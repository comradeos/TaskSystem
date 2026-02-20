namespace TaskSystem.Domain.Entities;

public class Comment
{
    public int Id { get; init; }
    public int TaskId { get; init; }
    public int AuthorId { get; init; }
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}