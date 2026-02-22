using System.Data;
using Dapper;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;

namespace TaskSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PostgresConnection _connection;

    public UserRepository(PostgresConnection connection)
    {
        _connection = connection;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        const string sql = """
            SELECT id,
                   name,
                   login,
                   password_hash AS PasswordHash,
                   is_admin AS IsAdmin,
                   created_at AS CreatedAt
            FROM users
            WHERE login = @Login
        """;

        using IDbConnection db = _connection.Create();
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Login = login });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT id,
                   name,
                   login,
                   password_hash AS PasswordHash,
                   is_admin AS IsAdmin,
                   created_at AS CreatedAt
            FROM users
            WHERE id = @Id
        """;

        using IDbConnection db = _connection.Create();
        return await db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = """
            SELECT id,
                   name,
                   login,
                   password_hash AS PasswordHash,
                   is_admin AS IsAdmin,
                   created_at AS CreatedAt
            FROM users
            ORDER BY id ASC
        """;

        using IDbConnection db = _connection.Create();
        return await db.QueryAsync<User>(sql);
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = """
            INSERT INTO users (name, login, password_hash, is_admin)
            VALUES (@Name, @Login, @PasswordHash, @IsAdmin)
            RETURNING id
        """;

        using IDbConnection db = _connection.Create();

        return await db.ExecuteScalarAsync<int>(sql, new
        {
            user.Name,
            user.Login,
            user.PasswordHash,
            user.IsAdmin
        });
    }
}