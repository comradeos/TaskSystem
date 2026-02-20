namespace TaskSystem.Application.DTO.Projects;

public class ProjectResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}