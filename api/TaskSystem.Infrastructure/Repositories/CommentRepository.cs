using System.Data;
using Dapper;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;

namespace TaskSystem.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly PostgresConnection _connection;

    public CommentRepository(PostgresConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> CreateAsync(Comment comment)
    {
        const string sql = """
                               INSERT INTO comments (
                                   task_id,
                                   user_id,
                                   user_name,
                                   content
                               )
                               VALUES (
                                   @TaskId,
                                   @UserId,
                                   @UserName,
                                   @Content
                               )
                               RETURNING id
                           """;

        using IDbConnection db = _connection.Create();
        
        return await db.ExecuteScalarAsync<int>(sql, new
        {
            comment.TaskId,
            comment.UserId,
            comment.UserName,
            comment.Content
        });
    }

    public async Task<IEnumerable<Comment>> GetByTaskAsync(int taskId)
    {
        const string sql = """
                               SELECT id,
                                      task_id AS TaskId,
                                      user_id AS UserId,
                                      user_name AS UserName,
                                      content,
                                      created_at AS CreatedAt
                               FROM comments
                               WHERE task_id = @TaskId
                               ORDER BY created_at ASC
                           """;

        using IDbConnection db = _connection.Create();

        return await db.QueryAsync<Comment>(sql, new { TaskId = taskId });
    }
}