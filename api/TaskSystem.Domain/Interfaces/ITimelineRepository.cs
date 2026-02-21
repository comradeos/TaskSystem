using TaskSystem.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Domain.Interfaces;

public interface ITimelineRepository
{
    Task AddAsync(TimelineEvent timelineEvent);

    Task<IEnumerable<TimelineEvent>> GetByEntityAsync(
        string entityType,
        int entityId);
}