using Microsoft.AspNetCore.Mvc;
using System.Text;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Common;
using TaskSystem.Application.DTO.Tasks;
using TaskSystem.Application.Tasks;
using TaskSystem.Domain.Entities;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Api.Controllers;

[Route("tasks")]
public class TasksController(
    ICreateTaskUseCase createTask,
    IAssignTaskUseCase assigner,
    IChangeTaskStatusUseCase changeTaskStatus,
    IGetTaskHistoryUseCase getTaskHistory,
    IAddTaskCommentUseCase addComment,
    IGetTaskCommentsUseCase getTaskComments,
    ITaskRepository tasks
) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] TaskCreateRequest request,
        CancellationToken ct
    )
    {
        try
        {
            int id =
                await createTask.ExecuteAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.Status,
                    request.Priority,
                    request.AssigneeId,
                    ct
                );

            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (InvalidOperationException)
        {
            return NotFoundProblem("Project not found.");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        TaskItem? task =
            await tasks.GetByIdAsync(id);

        if (task is null)
            return NotFoundProblem("Task not found.");

        return Ok(task);
    }

    // ✅ Убрали {page} из route
    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> ListByProject(
        int projectId,
        [FromQuery] PageRequest pageRequest,
        [FromQuery] TaskStatus? status,
        [FromQuery] int? assigneeId,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        int page =
            pageRequest.Page;

        int size =
            pageRequest.Size;

        IReadOnlyList<TaskItem> items =
            await tasks.GetPageByProjectAsync(
                projectId,
                page,
                size,
                status,
                assigneeId,
                search,
                ct
            );

        long total =
            await tasks.CountByProjectAsync(
                projectId,
                status,
                assigneeId,
                search,
                ct
            );

        PageResponse<TaskItem> response =
            new PageResponse<TaskItem>
            {
                Items = items,
                Page = page,
                Size = size,
                Total = total
            };

        return Ok(response);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(
        int id,
        [FromBody] AssignRequest request,
        CancellationToken ct
    )
    {
        try
        {
            await assigner.ExecuteAsync(
                id,
                request.AssigneeId,
                ct
            );

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFoundProblem("Task not found.");
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken ct
    )
    {
        try
        {
            await changeTaskStatus.ExecuteAsync(
                id,
                request.Status,
                ct
            );

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFoundProblem(
                "Task not found or invalid status change."
            );
        }
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(
        int id,
        [FromBody] CommentCreateRequest request,
        CancellationToken ct
    )
    {
        bool hasUser =
            HttpContext.Items.TryGetValue(
                "UserId",
                out object? userObject
            );

        if (!hasUser)
            return UnauthorizedProblem("Unauthorized");

        if (userObject is not int userId)
            return UnauthorizedProblem("Unauthorized");

        try
        {
            int commentId =
                await addComment.ExecuteAsync(
                    id,
                    userId,
                    request.Text,
                    ct
                );

            return Created("", new { id = commentId });
        }
        catch (InvalidOperationException)
        {
            return NotFoundProblem("Task not found.");
        }
    }

    [HttpGet("{id:int}/comments")]
    public async Task<IActionResult> GetComments(
        int id,
        [FromQuery] PageRequest pageRequest,
        CancellationToken ct
    )
    {
        try
        {
            IReadOnlyList<TaskComment> all =
                await getTaskComments.ExecuteAsync(
                    id,
                    ct
                );

            int page =
                pageRequest.Page;

            int size =
                pageRequest.Size;

            int total =
                all.Count;

            int skip =
                (page - 1) * size;

            if (skip < 0)
                skip = 0;

            IReadOnlyList<TaskComment> items =
                all.Skip(skip)
                   .Take(size)
                   .ToList();

            PageResponse<TaskComment> response =
                new PageResponse<TaskComment>
                {
                    Items = items,
                    Page = page,
                    Size = size,
                    Total = total
                };

            return Ok(response);
        }
        catch (InvalidOperationException)
        {
            return NotFoundProblem("Task not found.");
        }
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> History(
        int id,
        CancellationToken ct
    )
    {
        IReadOnlyList<TimelineEvent> history =
            await getTaskHistory.ExecuteAsync(
                id,
                ct
            );

        return Ok(history);
    }

    [HttpGet("project/{projectId:int}/export")]
    public async Task<IActionResult> ExportByProject(
        int projectId,
        [FromQuery] TaskStatus? status,
        [FromQuery] int? assigneeId,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        IReadOnlyList<TaskItem> items =
            await tasks.GetAllByProjectAsync(
                projectId,
                status,
                assigneeId,
                search,
                ct
            );

        string csv =
            BuildCsv(items);

        byte[] bytes =
            Encoding.UTF8.GetBytes(csv);

        string fileName =
            $"tasks_project_{projectId}.csv";

        return File(
            bytes,
            "text/csv",
            fileName
        );
    }

    private static string BuildCsv(
        IReadOnlyList<TaskItem> items
    )
    {
        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine(
            "Id,ProjectId,Number,Title,Description,Status,Priority,AssigneeId,CreatedAt"
        );

        foreach (TaskItem task in items)
        {
            string line =
                task.Id + "," +
                task.ProjectId + "," +
                task.Number + "," +
                "\"" + Escape(task.Title) + "\"," +
                "\"" + Escape(task.Description) + "\"," +
                task.Status + "," +
                task.Priority + "," +
                task.AssigneeId + "," +
                task.CreatedAt.ToString("O");

            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\"", "\"\"");
    }
}