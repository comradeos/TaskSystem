using System.Text.Json;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Projects;

public sealed class CreateProject(
    IProjectRepository projects,
    ITimelineRepository timeline)
    : ICreateProjectUseCase
{
    public async Task<int> ExecuteAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.");

        if (await projects.ExistsByNameAsync(name))
            throw new InvalidOperationException("Project with this name already exists.");

        var project = new Project
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        int id = await projects.CreateAsync(project);

        string data = JsonSerializer.Serialize(new
        {
            Name = name
        });

        await timeline.AddAsync(
            new TimelineEvent(
                "Project",
                id.ToString(),
                "Created",
                data,
                DateTime.UtcNow
            ),
            ct
        );

        return id;
    }
}