namespace TaskSystem.Application.DTO.Auth;

public class LoginRequest
{
    public string Login { get; init; } = null!;
    public string Password { get; init; } = null!;
}