namespace TaskSystem.Api.DTOs;

public class LoginResponseDto
{
    public string SessionToken { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}