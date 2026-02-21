using Dapper;
using Npgsql;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Database;

namespace TaskSystem.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly PostgresConnection _connection;

    public TaskRepository(PostgresConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> CreateAsync(TaskSystem.Domain.Entities.Task task)
    {
        const int maxRetries = 3;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            await using var db = (NpgsqlConnection)_connection.Create();
            await db.OpenAsync();

            await using var tx = await db.BeginTransactionAsync();

            try
            {
                const string lockProjectSql = """
                    SELECT id
                    FROM projects
                    WHERE id = @ProjectId
                    FOR UPDATE
                """;

                var projectExists = await db.ExecuteScalarAsync<int?>(
                    lockProjectSql,
                    new { task.ProjectId },
                    tx);

                if (projectExists is null)
                    throw new Exception("Project not found");

                const string numberSql = """
                    SELECT COALESCE(MAX(number_in_project), 0) + 1
                    FROM tasks
                    WHERE project_id = @ProjectId
                """;

                var nextNumber = await db.ExecuteScalarAsync<int>(
                    numberSql,
                    new { task.ProjectId },
                    tx);

                const string insertSql = """
                    INSERT INTO tasks (
                        project_id,
                        number_in_project,
                        title,
                        description,
                        status,
                        priority,
                        author_user_id,
                        author_user_name,
                        assigned_user_id,
                        assigned_user_name
                    )
                    VALUES (
                        @ProjectId,
                        @NumberInProject,
                        @Title,
                        @Description,
                        @Status,
                        @Priority,
                        @AuthorUserId,
                        @AuthorUserName,
                        @AssignedUserId,
                        @AssignedUserName
                    )
                    RETURNING id
                """;

                var id = await db.ExecuteScalarAsync<int>(
                    insertSql,
                    new
                    {
                        task.ProjectId,
                        NumberInProject = nextNumber,
                        task.Title,
                        task.Description,
                        Status = (int)task.Status,
                        Priority = (int)task.Priority,
                        task.AuthorUserId,
                        task.AuthorUserName,
                        task.AssignedUserId,
                        task.AssignedUserName
                    },
                    tx);

                await tx.CommitAsync();

                return id;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                await tx.RollbackAsync();
            }
        }

        throw new Exception("Could not create task due to concurrency conflict.");
    }
        
    public async Task<TaskSystem.Domain.Entities.Task?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT id,
                   project_id AS ProjectId,
                   number_in_project AS NumberInProject,
                   title,
                   description,
                   status,
                   priority,
                   author_user_id AS AuthorUserId,
                   author_user_name AS AuthorUserName,
                   assigned_user_id AS AssignedUserId,
                   assigned_user_name AS AssignedUserName,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tasks
            WHERE id = @Id
        """;

        using var db = _connection.Create();
        return await db.QueryFirstOrDefaultAsync<TaskSystem.Domain.Entities.Task>(
            sql,
            new { Id = id });
    }

    public async Task<IEnumerable<TaskSystem.Domain.Entities.Task>> GetByProjectAsync(
        int projectId,
        int page,
        int size,
        int? status,
        int? assignedUserId,
        string? search)
    {
        var offset = (page - 1) * size;

        var sql = """
            SELECT id,
                   project_id AS ProjectId,
                   number_in_project AS NumberInProject,
                   title,
                   description,
                   status,
                   priority,
                   author_user_id AS AuthorUserId,
                   author_user_name AS AuthorUserName,
                   assigned_user_id AS AssignedUserId,
                   assigned_user_name AS AssignedUserName,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM tasks
            WHERE project_id = @ProjectId
        """;

        if (status.HasValue)
            sql += " AND status = @Status";

        if (assignedUserId.HasValue)
            sql += " AND assigned_user_id = @AssignedUserId";

        if (!string.IsNullOrWhiteSpace(search))
            sql += " AND title ILIKE @Search";

        sql += " ORDER BY number_in_project DESC LIMIT @Size OFFSET @Offset";

        using var db = _connection.Create();

        return await db.QueryAsync<TaskSystem.Domain.Entities.Task>(
            sql,
            new
            {
                ProjectId = projectId,
                Status = status,
                AssignedUserId = assignedUserId,
                Search = $"%{search}%",
                Size = size,
                Offset = offset
            });
    }
    
    public async Task UpdateAsync(
        int id,
        int? status,
        int? priority,
        int? assignedUserId,
        string? assignedUserName)
    {
        const string sql = """
                               UPDATE tasks
                               SET
                                   status = COALESCE(@Status, status),
                                   priority = COALESCE(@Priority, priority),
                                   assigned_user_id = @AssignedUserId,
                                   assigned_user_name = @AssignedUserName,
                                   updated_at = NOW()
                               WHERE id = @Id
                           """;

        using var db = _connection.Create();

        await db.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            Priority = priority,
            AssignedUserId = assignedUserId,
            AssignedUserName = assignedUserName
        });
    }
}