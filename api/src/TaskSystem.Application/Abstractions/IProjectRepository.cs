using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface IProjectRepository
{
    Task<int> CreateAsync(Project project);
    Task<Project?> GetByIdAsync(int id);
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<bool> ExistsByNameAsync(string name);
}