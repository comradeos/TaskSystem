using Dapper;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly PostgresConnection _connection;

    public SessionRepository(PostgresConnection connection)
    {
        _connection = connection;
    }

    public async Task CreateAsync(Session session)
    {
        const string sql = """
                               INSERT INTO sessions (user_id, session_token, created_at)
                               VALUES (@UserId, @SessionToken, @CreatedAt)
                           """;

        using var db = _connection.Create();
        await db.ExecuteAsync(sql, session);
    }

    public async Task<Session?> GetByTokenAsync(string token)
    {
        const string sql = """
                               SELECT id,
                                      user_id AS UserId,
                                      session_token AS SessionToken,
                                      created_at AS CreatedAt
                               FROM sessions
                               WHERE session_token = @Token
                           """;

        using var db = _connection.Create();
        return await db.QueryFirstOrDefaultAsync<Session>(sql, new { Token = token });
    }

    public async Task DeleteAsync(string token)
    {
        const string sql = """
                               DELETE FROM sessions
                               WHERE session_token = @Token
                           """;

        using var db = _connection.Create();
        await db.ExecuteAsync(sql, new { Token = token });
    }
}