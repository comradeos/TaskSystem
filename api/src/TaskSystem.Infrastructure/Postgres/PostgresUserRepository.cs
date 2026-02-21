using System.Data;
using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresUserRepository(
    ICoreDbConnectionFactory factory
) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = """
                               SELECT
                                   id,
                                   login,
                                   password_hash AS PasswordHash,
                                   name,
                                   created_at AS CreatedAt
                               FROM users
                               WHERE id = @Id;
                           """;

        using IDbConnection connection = factory.CreateConnection();

        User? user =
            await connection.QuerySingleOrDefaultAsync<User>(
                sql,
                new
                {
                    Id = id
                }
            );

        return user;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        const string sql = """
                               SELECT
                                   id,
                                   login,
                                   password_hash AS PasswordHash,
                                   name,
                                   created_at AS CreatedAt
                               FROM users
                               WHERE login = @Login;
                           """;

        using IDbConnection connection = factory.CreateConnection();

        User? user =
            await connection.QuerySingleOrDefaultAsync<User>(
                sql,
                new
                {
                    Login = login
                }
            );

        return user;
    }
}