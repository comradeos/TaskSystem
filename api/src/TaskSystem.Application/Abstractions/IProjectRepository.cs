using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface IProjectRepository
{
    Task CreateAsync(Project project);
    Task<IReadOnlyList<Project>> GetAllAsync();
}