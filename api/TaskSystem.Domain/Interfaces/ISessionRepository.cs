using TaskSystem.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Domain.Interfaces;

public interface ISessionRepository
{
    Task CreateAsync(Session session);

    Task<Session?> GetByTokenAsync(string token);

    Task DeleteAsync(string token);
}