using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Abstractions;

public interface ITaskRepository
{
    Task<int> CreateAsync(
        int projectId,
        string title,
        string description,
        TaskStatus status,
        TaskPriority priority,
        int? assigneeId);

    Task<TaskItem?> GetByIdAsync(int id);

    Task<IReadOnlyList<TaskItem>> GetByProjectAsync(int projectId);

    Task<IReadOnlyList<TaskItem>> GetPageByProjectAsync(
        int projectId,
        int page,
        int size,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default);

    Task<long> CountByProjectAsync(
        int projectId,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default);

    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
}