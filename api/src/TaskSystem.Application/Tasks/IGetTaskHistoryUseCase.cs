using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public interface IGetTaskHistoryUseCase
{
    Task<IReadOnlyList<TimelineEvent>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default);
}