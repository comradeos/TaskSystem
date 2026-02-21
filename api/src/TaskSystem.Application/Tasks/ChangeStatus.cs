using System.Text.Json;
using TaskSystem.Application.Abstractions;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Tasks;

public sealed class ChangeTaskStatus(
    ITaskRepository tasks,
    ITimelineRepository timeline)
    : IChangeTaskStatusUseCase
{
    public async Task ExecuteAsync(
        int taskId,
        TaskStatus newStatus,
        CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(taskId);

        if (task is null)
            throw new InvalidOperationException("Task not found.");

        var oldStatus = task.Status;

        bool changed = task.ChangeStatus(newStatus);

        if (!changed)
            return;

        await tasks.UpdateAsync(task, ct);

        string data = JsonSerializer.Serialize(new
        {
            OldStatus = oldStatus,
            NewStatus = newStatus
        });

        await timeline.TryAddAsync(
            new TimelineEvent(
                "Task",
                task.Id.ToString(),
                "StatusChanged",
                data,
                DateTime.UtcNow
            ),
            ct
        );
    }
}