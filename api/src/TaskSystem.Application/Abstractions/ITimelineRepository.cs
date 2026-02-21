namespace TaskSystem.Application.Abstractions;

public interface ITimelineRepository
{
    Task AddAsync(TimelineEvent evt, CancellationToken ct = default);

    Task TryAddAsync(TimelineEvent evt, CancellationToken ct = default);

    Task<IReadOnlyList<TimelineEvent>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken ct = default);

    Task PingAsync(CancellationToken ct = default);
}