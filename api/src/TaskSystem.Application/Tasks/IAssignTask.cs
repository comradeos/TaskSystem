namespace TaskSystem.Application.Tasks;

public interface IAssignTask
{
    Task ExecuteAsync(int taskId, int assigneeId, CancellationToken ct = default);
}