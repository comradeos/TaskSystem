using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Tasks;

public class AddTaskComment(
    ITaskRepository tasks,
    ICommentRepository comments,
    ITimelineRepository timeline
) : IAddTaskCommentUseCase
{
    public async Task<int> ExecuteAsync(
        int taskId,
        int authorId,
        string text,
        CancellationToken ct = default
    )
    {
        TaskItem? task = await tasks.GetByIdAsync(taskId);

        if (task is null)
        {
            throw new InvalidOperationException("Task not found.");
        }

        DateTime createdAt = DateTime.UtcNow;

        int commentId = await comments.AddAsync(
            taskId,
            authorId,
            text,
            createdAt,
            ct
        );

        await timeline.TryAddAsync(
            new TimelineEvent(
                "Task",
                taskId.ToString(),
                "CommentAdded",
                $"{{\"CommentId\":{commentId},\"AuthorId\":{authorId}}}",
                createdAt
            ),
            ct
        );

        return commentId;
    }
}