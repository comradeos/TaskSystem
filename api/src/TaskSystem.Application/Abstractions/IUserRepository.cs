using TaskSystem.Domain.Entities;

namespace TaskSystem.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
}