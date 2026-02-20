using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresProjectRepository : IProjectRepository
{
    private readonly ICoreDbConnectionFactory _factory;

    public PostgresProjectRepository(ICoreDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task CreateAsync(Project project)
    {
        const string sql = """
            INSERT INTO projects (id, name, created_at)
            VALUES (@Id, @Name, @CreatedAt)
        """;

        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(sql, project);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();

        var result = await connection.QueryAsync<Project>(
            "SELECT id, name, created_at AS CreatedAt FROM projects");

        return result.ToList();
    }
}