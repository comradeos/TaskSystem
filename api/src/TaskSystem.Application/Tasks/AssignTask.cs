using System.Text.Json;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Tasks;

public class AssignTask(
    ITaskRepository tasks,
    IUserRepository users,
    ITimelineRepository timeline
) : IAssignTaskUseCase
{
    public async Task ExecuteAsync(
        int taskId,
        int assigneeId,
        CancellationToken ct = default
    )
    {
        TaskItem? task = await tasks.GetByIdAsync(taskId);

        if (task is null)
        {
            throw new InvalidOperationException("Task not found.");
        }

        User? user = await users.GetByIdAsync(assigneeId);

        if (user is null)
        {
            throw new InvalidOperationException("User not found.");
        }

        int? oldAssignee = task.AssigneeId;
        TaskStatus oldStatus = task.Status;

        bool assigneeChanged = task.Assign(assigneeId);
        bool statusChanged = oldStatus != task.Status;

        if (!assigneeChanged && !statusChanged)
        {
            return;
        }

        await tasks.UpdateAsync(task, ct);

        DateTime occurredAt = DateTime.UtcNow;

        if (assigneeChanged)
        {
            string action;

            if (oldAssignee is null)
            {
                action = "Assigned";
            }
            else
            {
                action = "Reassigned";
            }

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