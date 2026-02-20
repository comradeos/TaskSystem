namespace TaskSystem.Domain.Entities;

public class Project
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}