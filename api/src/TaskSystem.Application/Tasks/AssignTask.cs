using System.Text.Json;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public sealed class AssignTask(
    ITaskRepository tasks,
    IUserRepository users,
    ITimelineRepository timeline)
    : IAssignTaskUseCase
{
    public async Task ExecuteAsync(
        int taskId,
        int assigneeId,
        CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(taskId);
        if (task is null)
            throw new InvalidOperationException("Task not found.");

        var user = await users.GetByIdAsync(assigneeId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        var oldAssignee = task.AssigneeId;
        var oldStatus = task.Status;

        bool assigneeChanged = task.Assign(assigneeId);
        bool statusChanged = oldStatus != task.Status;

        if (!assigneeChanged && !statusChanged)
            return;

        await tasks.UpdateAsync(task, ct);

        var occurredAt = DateTime.UtcNow;

        if (assigneeChanged)
        {
            string action = oldAssignee is null
                ? "Assigned"
                : "Reassigned";

            string data = JsonSerializer.Serialize(new
            {
                OldAssignee = oldAssignee,
                NewAssignee = assigneeId
            });

            await timeline.TryAddAsync(
                new TimelineEvent(
                    "Task",
                    task.Id.ToString(),
                    action,
                    data,
                    occurredAt
                ),
                ct
            );
        }

        if (statusChanged)
        {
            string statusData = JsonSerializer.Serialize(new
            {
                OldStatus = oldStatus,
                NewStatus = task.Status
            });

            await timeline.TryAddAsync(
                new TimelineEvent(
                    "Task",
                    task.Id.ToString(),
                    "StatusChanged",
                    statusData,
                    occurredAt
                ),
                ct
            );
        }
    }
}