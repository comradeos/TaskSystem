using System.Collections.Concurrent;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Auth;

namespace TaskSystem.Infrastructure.InMemory;

public sealed class InMemorySessionStore : ISessionStore
{
    private static readonly ConcurrentDictionary<string, UserSession> Sessions = new();

    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(8);

    public UserSession Create(int userId, string login)
    {
        string token = Convert.ToHexString(Guid.NewGuid().ToByteArray());

        UserSession session = new()
        {
            Token = token,
            UserId = userId,
            Login = login,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(Lifetime)
        };

        Sessions[token] = session;

        return session;
    }

    public UserSession? Get(string token)
    {
        if (!Sessions.TryGetValue(token, out var session))
        {
            return null;
        }

        if (session.ExpiresAt >= DateTime.UtcNow)
        {
            return session;
        }
        
        Sessions.TryRemove(token, out _);
        
        return null;
    }

    public void Remove(string token)
    {
        Sessions.TryRemove(token, out _);
    }
}