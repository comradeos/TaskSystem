namespace TaskSystem.Api.DTOs;

public class ProjectDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}