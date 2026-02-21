using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);

    Task<User?> GetByIdAsync(int id);

    Task<int> CreateAsync(User user);
    
    Task<IEnumerable<User>> GetAllAsync();
}