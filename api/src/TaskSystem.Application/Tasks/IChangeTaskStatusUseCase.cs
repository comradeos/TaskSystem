using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.Tasks;

public interface IChangeTaskStatusUseCase
{
    Task ExecuteAsync(int taskId, TaskStatus newStatus, CancellationToken ct = default);
}