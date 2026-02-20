namespace TaskSystem.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public string Login { get; init; } = null!;
    public string PasswordHash { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}