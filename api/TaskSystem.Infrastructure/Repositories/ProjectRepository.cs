using Dapper;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;

namespace TaskSystem.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly PostgresConnection _connection;

    public ProjectRepository(PostgresConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> CreateAsync(Project project)
    {
        const string sql = """
                               INSERT INTO projects (name)
                               VALUES (@Name)
                               RETURNING id
                           """;

        using var db = _connection.Create();
        return await db.ExecuteScalarAsync<int>(sql, project);
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        const string sql = """
                               SELECT id,
                                      name,
                                      created_at AS CreatedAt
                               FROM projects
                               WHERE id = @Id
                           """;

        using var db = _connection.Create();
        return await db.QueryFirstOrDefaultAsync<Project>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        const string sql = """
                               SELECT id,
                                      name,
                                      created_at AS CreatedAt
                               FROM projects
                               ORDER BY created_at DESC
                           """;

        using var db = _connection.Create();
        return await db.QueryAsync<Project>(sql);
    }
}