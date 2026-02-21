using TaskSystem.Domain.Entities;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;
using TaskPriority = TaskSystem.Domain.Enums.TaskPriority;

namespace TaskSystem.Application.Abstractions;

public interface ITaskRepository
{
    Task<int> CreateAsync(
        int projectId,
        string title,
        string description,
        TaskStatus status,
        TaskPriority priority,
        int? assigneeId,
        CancellationToken ct = default
    );

    Task<TaskItem?> GetByIdAsync(
        int id,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<TaskItem>> GetByProjectAsync(
        int projectId,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<TaskItem>> GetPageByProjectAsync(
        int projectId,
        int page,
        int size,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default
    );

    Task<long> CountByProjectAsync(
        int projectId,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<TaskItem>> GetAllByProjectAsync(
        int projectId,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default
    );

    Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default
    );
}