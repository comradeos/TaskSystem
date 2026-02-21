using System.Text.Json;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public sealed class AssignTask(
    ITaskRepository tasks,
    IUserRepository users,
    ITimelineRepository timeline)
    : IAssignTask
{
    public async Task ExecuteAsync(int taskId, int assigneeId, CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(taskId);
        if (task is null)
            throw new InvalidOperationException("Task not found.");

        var user = await users.GetByIdAsync(assigneeId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        var oldAssignee = task.AssigneeId;
        var oldStatus = task.Status;

        bool changed = task.Assign(assigneeId);

        await tasks.UpdateAsync(task, ct);

        if (changed)
        {
            string action = oldAssignee is null
                ? "Assigned"
                : "Reassigned";

            string data = JsonSerializer.Serialize(new
            {
                OldAssignee = oldAssignee,
                NewAssignee = assigneeId
            });

            await timeline.AddAsync(
                new TimelineEvent(
                    "Task",
                    task.Id.ToString(),
                    action,
                    data,
                    DateTime.UtcNow
                ),
                ct
            );
        }

        if (oldStatus != task.Status)
        {
            string statusData = JsonSerializer.Serialize(new
            {
                OldStatus = oldStatus,
                NewStatus = task.Status
            });

            await timeline.AddAsync(
                new TimelineEvent(
                    "Task",
                    task.Id.ToString(),
                    "StatusChanged",
                    statusData,
                    DateTime.UtcNow
                ),
                ct
            );
        }
    }
}