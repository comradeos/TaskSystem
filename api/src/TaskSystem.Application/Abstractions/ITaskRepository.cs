using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface ITaskRepository
{
    Task CreateAsync(TaskItem task);
    Task<TaskItem?> GetByIdAsync(Guid id);
}