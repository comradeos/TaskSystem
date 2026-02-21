using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Tasks;

public interface ICreateTask
{
    Task<int> ExecuteAsync(
        int projectId,
        string title,
        string description,
        TaskStatus status,
        TaskPriority priority,
        int? assigneeId,
        CancellationToken ct = default);
}