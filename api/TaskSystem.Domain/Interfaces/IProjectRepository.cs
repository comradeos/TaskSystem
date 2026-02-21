using TaskSystem.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Domain.Interfaces;

public interface IProjectRepository
{
    Task<int> CreateAsync(Project project);

    Task<Project?> GetByIdAsync(int id);

    Task<IEnumerable<Project>> GetAllAsync();
}