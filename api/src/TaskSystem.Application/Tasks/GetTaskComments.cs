using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Tasks;

public class GetTaskComments(
    ITaskRepository tasks,
    ICommentRepository comments
) : IGetTaskCommentsUseCase
{
    public async Task<IReadOnlyList<TaskComment>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default
    )
    {
        TaskItem? task = await tasks.GetByIdAsync(taskId);

        if (task is null)
        {
            throw new InvalidOperationException("Task not found.");
        }

        return await comments.GetByTaskIdAsync(taskId, ct);
    }
}