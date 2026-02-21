namespace TaskSystem.Api.DTOs;

public class CreateUserRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}