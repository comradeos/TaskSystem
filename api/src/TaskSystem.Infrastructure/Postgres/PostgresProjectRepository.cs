using System.Data;
using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresProjectRepository(
    ICoreDbConnectionFactory factory
) : IProjectRepository
{
    public async Task<int> CreateAsync(Project project)
    {
        const string sql = """
            INSERT INTO projects (
                name,
                created_at
            )
            VALUES (
                @Name,
                @CreatedAt
            )
            RETURNING id;
        """;

        using IDbConnection connection = factory.CreateConnection();

        int id =
            await connection.ExecuteScalarAsync<int>(
                sql,
                project
            );

        return id;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT
                id,
                name,
                created_at AS CreatedAt
            FROM projects
            WHERE id = @Id;
        """;

        using IDbConnection connection = factory.CreateConnection();

        Project? project =
            await connection.QuerySingleOrDefaultAsync<Project>(
                sql,
                new
                {
                    Id = id
                }
            );

        return project;
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id,
                name,
                created_at AS CreatedAt
            FROM projects
            ORDER BY created_at DESC;
        """;

        using IDbConnection connection = factory.CreateConnection();

        IEnumerable<Project> result =
            await connection.QueryAsync<Project>(sql);

        return result.AsList();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        const string sql = """
            SELECT 1
            FROM projects
            WHERE name = @Name
            LIMIT 1;
        """;

        using IDbConnection connection = factory.CreateConnection();

        int? result =
            await connection.QueryFirstOrDefaultAsync<int?>(
                sql,
                new
                {
                    Name = name
                }
            );

        if (result.HasValue)
        {
            return true;
        }

        return false;
    }

    public async Task<IReadOnlyList<Project>> GetPageAsync(
        int page,
        int size,
        CancellationToken ct = default
    )
    {
        int offset = (page - 1) * size;

        const string sql = """
            SELECT
                id,
                name,
                created_at AS CreatedAt
            FROM projects
            ORDER BY created_at DESC
            LIMIT @Size
            OFFSET @Offset;
        """;

        using IDbConnection connection = factory.CreateConnection();

        IEnumerable<Project> result =
            await connection.QueryAsync<Project>(
                sql,
                new
                {
                    Size = size,
                    Offset = offset
                }
            );

        return result.AsList();
    }

    public async Task<long> CountAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM projects;
        """;

        using IDbConnection connection = factory.CreateConnection();

        long count =
            await connection.ExecuteScalarAsync<long>(sql);

        return count;
    }
}