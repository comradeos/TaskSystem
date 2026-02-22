using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IProjectRepository
{
    Task<int> CreateAsync(Project project);

    Task<Project?> GetByIdAsync(int id);

    Task<IEnumerable<Project>> GetAllAsync();
}