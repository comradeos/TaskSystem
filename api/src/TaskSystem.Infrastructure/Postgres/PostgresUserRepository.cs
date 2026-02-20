using System.Data;
using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Postgres;

public sealed class PostgresUserRepository(ICoreDbConnectionFactory factory) : IUserRepository
{
    public async Task<User?> GetByLoginAsync(string login)
    {
        const string sql = "SELECT id, login, password_hash AS PasswordHash, name, created_at AS CreatedAt FROM users WHERE login = @Login";

        using IDbConnection connection = factory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Login = login });
    }   
}