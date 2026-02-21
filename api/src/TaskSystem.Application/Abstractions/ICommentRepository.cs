using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface ICommentRepository
{
    Task<int> AddAsync(
        int taskId,
        int authorId,
        string text,
        DateTime createdAtUtc,
        CancellationToken ct = default);

    Task<IReadOnlyList<TaskComment>> GetByTaskIdAsync(
        int taskId,
        CancellationToken ct = default);
}