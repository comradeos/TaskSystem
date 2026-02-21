using TaskSystem.Application.Abstractions;

namespace TaskSystem.Application.Tasks;

public sealed class GetTaskHistory(
    ITimelineRepository timeline)
    : IGetTaskHistory
{
    public async Task<IReadOnlyList<TimelineEvent>> ExecuteAsync(
        int taskId,
        CancellationToken ct = default)
    {
        return await timeline.GetByEntityAsync(
            "Task",
            taskId.ToString(),
            ct);
    }
}