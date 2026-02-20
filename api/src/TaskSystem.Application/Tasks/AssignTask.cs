using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public sealed class AssignTask(
    ITaskRepository tasks,
    ITimelineRepository timeline)
    : IAssignTask
{
    public async Task ExecuteAsync(int taskId, int assigneeId, CancellationToken ct = default)
    {
        Console.WriteLine("ASSIGN USE CASE EXECUTED");
        
        var task = await tasks.GetByIdAsync(taskId);

        if (task is null)
            throw new InvalidOperationException("Task not found.");

        var oldAssignee = task.AssigneeId;

        task.Assign(assigneeId);

        await tasks.UpdateAsync(task, ct);

        if (oldAssignee != assigneeId)
        {
            await timeline.AddAsync(
                new TimelineEvent(
                    "Task",
                    task.Id.ToString(),
                    "Assigned",
                    new
                    {
                        OldAssignee = oldAssignee,
                        NewAssignee = assigneeId
                    },
                    DateTimeOffset.UtcNow
                ),
                ct
            );
        }
    }
}