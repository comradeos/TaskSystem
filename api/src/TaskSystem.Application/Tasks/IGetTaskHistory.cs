using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public interface IGetTaskHistory
{
    Task<IReadOnlyList<TimelineEvent>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default);
}