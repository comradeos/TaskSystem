namespace TaskSystem.Application.Tasks;

public interface IAssignTaskUseCase
{
    Task ExecuteAsync(int taskId, int assigneeId, CancellationToken ct = default);
}