using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface ITimelineRepository
{
    Task AddEventAsync(TimelineEvent evt);
    Task<IEnumerable<TimelineEvent>> GetByTaskIdAsync(Guid taskId);
    Task PingAsync();
}