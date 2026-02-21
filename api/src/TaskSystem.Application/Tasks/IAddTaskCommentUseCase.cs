namespace TaskSystem.Application.Tasks;

public interface IAddTaskCommentUseCase
{
    Task<int> ExecuteAsync(
        int taskId,
        int authorId,
        string text,
        CancellationToken ct = default);
}