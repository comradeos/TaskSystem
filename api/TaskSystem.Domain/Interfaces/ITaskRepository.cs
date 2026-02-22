using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Domain.Interfaces;

public interface ITaskRepository
{
    Task<int> CreateAsync(TaskSystem.Domain.Entities.Task task);

    Task<TaskSystem.Domain.Entities.Task?> GetByIdAsync(int id);

    Task<IEnumerable<TaskSystem.Domain.Entities.Task>> GetByProjectAsync(
        int projectId,
        int page,
        int size,
        int? status,
        int? assignedUserId,
        string? search
    );
    
    Task UpdateAsync(
        int id,
        int? status,
        int? priority,
        int? assignedUserId,
        string? assignedUserName
    );
}