using System.Text.Json;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Tasks;

public class CreateTask(
    ITaskRepository tasks,
    IProjectRepository projects,
    ITimelineRepository timeline
) : ICreateTaskUseCase
{
    public async Task<int> ExecuteAsync(
        int projectId,
        string title,
        string description,
        TaskStatus status,
        TaskPriority priority,
        int? assigneeId,
        CancellationToken ct = default
    )
    {
        Project? project = await projects.GetByIdAsync(projectId);

        if (project is null)
        {
            throw new InvalidOperationException("Project not found.");
        }

        int id = await tasks.CreateAsync(
            projectId,
            title,
            description,
            status,
            priority,
            assigneeId
        );

        string data = JsonSerializer.Serialize(new
        {
            Title = title,
            Status = status,
            Priority = priority,
            AssigneeId = assigneeId
        });

        await timeline.AddAsync(
            new TimelineEvent(
                "Task",
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