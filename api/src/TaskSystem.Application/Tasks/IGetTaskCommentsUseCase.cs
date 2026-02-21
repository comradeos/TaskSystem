using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Tasks;

public interface IGetTaskCommentsUseCase
{
    Task<IReadOnlyList<TaskComment>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default);
}