using TaskSystem.Domain.Auth;

namespace TaskSystem.Application.Abstractions;

public interface ISessionStore
{
    UserSession Create(int userId, string login);
    UserSession? Get(string token);
    void Remove(string token);
}