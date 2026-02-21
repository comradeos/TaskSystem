namespace TaskSystem.Domain.Entities;

public class Project
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}