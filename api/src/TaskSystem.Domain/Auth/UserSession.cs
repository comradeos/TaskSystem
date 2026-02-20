namespace TaskSystem.Domain.Auth;

public sealed class UserSession
{
    public string Token { get; init; } = null!;
    public int UserId { get; init; }
    public string Login { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}