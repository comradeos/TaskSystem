using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public class GetTaskHistory(
    ITimelineRepository timeline
) : IGetTaskHistoryUseCase
{
    public async Task<IReadOnlyList<TimelineEvent>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default
    )
    {
        return await timeline.GetByEntityAsync(
            "Task",
            taskId.ToString(),
            ct
        );
    }
}