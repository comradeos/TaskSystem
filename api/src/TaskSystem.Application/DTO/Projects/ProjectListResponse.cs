namespace TaskSystem.Application.DTO.Projects;

public class ProjectListResponse
{
    public IReadOnlyList<ProjectResponse> Items { get; init; } = [];
}