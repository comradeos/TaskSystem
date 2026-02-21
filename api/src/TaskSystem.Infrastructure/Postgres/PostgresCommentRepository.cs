using System.Data;
using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresCommentRepository(
    ICoreDbConnectionFactory factory
) : ICommentRepository
{
    public async Task<int> AddAsync(
        int taskId,
        int authorId,
        string text,
        DateTime createdAtUtc,
        CancellationToken ct = default
    )
    {
        const string sql = """
                           INSERT INTO comments (task_id, author_id, content, created_at)
                           VALUES (@TaskId, @AuthorId, @Text, @CreatedAt)
                           RETURNING id;
                           """;

        using IDbConnection connection = factory.CreateConnection();

        CommandDefinition cmd = new CommandDefinition(
            sql,
            new
            {
                TaskId = taskId,
                AuthorId = authorId,
                Text = text,
                CreatedAt = createdAtUtc
            },
            cancellationToken: ct
        );

        int id = await connection.ExecuteScalarAsync<int>(cmd);

        return id;
    }

    public async Task<IReadOnlyList<TaskComment>> GetByTaskIdAsync(
        int taskId,
        CancellationToken ct = default
    )
    {
        const string sql = """
                           SELECT
                               id,
                               task_id AS TaskId,
                               author_id AS AuthorId,
                               content AS Text,
                               created_at AS CreatedAtUtc
                           FROM comments
                           WHERE task_id = @TaskId
                           ORDER BY created_at ASC;
                           """;

        using IDbConnection connection = factory.CreateConnection();

        CommandDefinition cmd = new CommandDefinition(
            sql,
            new
            {
                TaskId = taskId
            },
            cancellationToken: ct
        );

        IEnumerable<TaskComment> result = await connection.QueryAsync<TaskComment>(cmd);

        return result.AsList();
    }
}