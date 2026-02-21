using System.Data;
using Dapper;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresTaskRepository(
    ICoreDbConnectionFactory factory
) : ITaskRepository
{
    public async Task<int> CreateAsync(
        int projectId,
        string title,
        string description,
        TaskStatus status,
        TaskPriority priority,
        int? assigneeId
    )
    {
        using IDbConnection connection = factory.CreateConnection();

        connection.Open();

        using IDbTransaction transaction =
            connection.BeginTransaction();

        try
        {
            const string lockProjectSql = """
                SELECT id
                FROM projects
                WHERE id = @ProjectId
                FOR UPDATE;
            """;

            int? projectExists =
                await connection.QueryFirstOrDefaultAsync<int?>(
                    lockProjectSql,
                    new
                    {
                        ProjectId = projectId
                    },
                    transaction
                );

            if (projectExists is null)
            {
                throw new InvalidOperationException("Project not found.");
            }

            const string nextNumberSql = """
                SELECT COALESCE(MAX(number), 0)
                FROM tasks
                WHERE project_id = @ProjectId;
            """;

            int currentMax =
                await connection.ExecuteScalarAsync<int>(
                    nextNumberSql,
                    new
                    {
                        ProjectId = projectId
                    },
                    transaction
                );

            int nextNumber = currentMax + 1;

            const string insertSql = """
                INSERT INTO tasks (
                    project_id,
                    number,
                    title,
                    description,
                    status,
                    priority,
                    assignee_id,
                    created_at
                )
                VALUES (
                    @ProjectId,
                    @Number,
                    @Title,
                    @Description,
                    @Status,
                    @Priority,
                    @AssigneeId,
                    @CreatedAt
                )
                RETURNING id;
            """;

            int newId =
                await connection.ExecuteScalarAsync<int>(
                    insertSql,
                    new
                    {
                        ProjectId = projectId,
                        Number = nextNumber,
                        Title = title,
                        Description = description,
                        Status = (int)status,
                        Priority = (int)priority,
                        AssigneeId = assigneeId,
                        CreatedAt = DateTime.UtcNow
                    },
                    transaction
                );

            transaction.Commit();

            return newId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT
                id,
                project_id AS ProjectId,
                number,
                title,
                description,
                status,
                priority,
                assignee_id AS AssigneeId,
                created_at AS CreatedAt
            FROM tasks
            WHERE id = @Id;
        """;

        using IDbConnection connection = factory.CreateConnection();

        TaskItem? task =
            await connection.QuerySingleOrDefaultAsync<TaskItem>(
                sql,
                new
                {
                    Id = id
                }
            );

        return task;
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectAsync(int projectId)
    {
        const string sql = """
            SELECT
                id,
                project_id AS ProjectId,
                number,
                title,
                description,
                status,
                priority,
                assignee_id AS AssigneeId,
                created_at AS CreatedAt
            FROM tasks
            WHERE project_id = @ProjectId
            ORDER BY number;
        """;

        using IDbConnection connection = factory.CreateConnection();

        IEnumerable<TaskItem> result =
            await connection.QueryAsync<TaskItem>(
                sql,
                new
                {
                    ProjectId = projectId
                }
            );

        return result.AsList();
    }

    public async Task<IReadOnlyList<TaskItem>> GetPageByProjectAsync(
        int projectId,
        int page,
        int size,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default
    )
    {
        int offset = (page - 1) * size;

        DynamicParameters parameters =
            new DynamicParameters();

        parameters.Add("ProjectId", projectId);
        parameters.Add("Limit", size);
        parameters.Add("Offset", offset);

        string where =
            BuildWhere(parameters, projectId, status, assigneeId, search);

        string sql = $"""
            SELECT
                id,
                project_id AS ProjectId,
                number,
                title,
                description,
                status,
                priority,
                assignee_id AS AssigneeId,
                created_at AS CreatedAt
            FROM tasks
            {where}
            ORDER BY number ASC
            LIMIT @Limit
            OFFSET @Offset;
        """;

        using IDbConnection connection = factory.CreateConnection();

        IEnumerable<TaskItem> result =
            await connection.QueryAsync<TaskItem>(
                sql,
                parameters
            );

        return result.AsList();
    }

    public async Task<long> CountByProjectAsync(
        int projectId,
        TaskStatus? status = null,
        int? assigneeId = null,
        string? search = null,
        CancellationToken ct = default
    )
    {
        DynamicParameters parameters =
            new DynamicParameters();

        parameters.Add("ProjectId", projectId);

        string where =
            BuildWhere(parameters, projectId, status, assigneeId, search);

        string sql = $"""
            SELECT COUNT(*)
            FROM tasks
            {where};
        """;

        using IDbConnection connection = factory.CreateConnection();

        long count =
            await connection.ExecuteScalarAsync<long>(
                sql,
                parameters
            );

        return count;
    }

    private static string BuildWhere(
        DynamicParameters p,
        int projectId,
        TaskStatus? status,
        int? assigneeId,
        string? search
    )
    {
        List<string> clauses =
            new List<string>
            {
                "project_id = @ProjectId"
            };

        if (status.HasValue)
        {
            clauses.Add("status = @Status");
            p.Add("Status", (int)status.Value);
        }

        if (assigneeId.HasValue)
        {
            clauses.Add("assignee_id = @AssigneeId");
            p.Add("AssigneeId", assigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            clauses.Add("title ILIKE @Search");
            p.Add("Search", $"%{search.Trim()}%");
        }

        string where =
            "WHERE " + string.Join(" AND ", clauses);

        return where;
    }

    public async Task UpdateAsync(
        TaskItem task,
        CancellationToken ct = default
    )
    {
        const string sql = """
            UPDATE tasks
            SET
                title = @Title,
                description = @Description,
                status = @Status,
                priority = @Priority,
                assignee_id = @AssigneeId,
                updated_at = NOW(),
                version = version + 1
            WHERE id = @Id;
        """;

        using IDbConnection connection = factory.CreateConnection();

        int rows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    Status = (int)task.Status,
                    Priority = (int)task.Priority,
                    task.AssigneeId
                }
            );

        if (rows == 0)
        {
            throw new InvalidOperationException(
                "Task not found during update."
            );
        }
    }
}