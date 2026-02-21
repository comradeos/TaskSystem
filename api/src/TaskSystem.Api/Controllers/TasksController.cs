using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Common;
using TaskSystem.Application.DTO.Tasks;
using TaskSystem.Application.Tasks;
using TaskSystem.Domain.Entities;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TasksController(
    ICreateTaskUseCase createTask,
    IAssignTaskUseCase assigner,
    IChangeTaskStatusUseCase changeTaskStatus,
    IGetTaskHistoryUseCase getTaskHistory,
    IAddTaskCommentUseCase addComment,
    IGetTaskCommentsUseCase getTaskComments,
    ITaskRepository tasks)
    : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<object>> Create(
        [FromBody] TaskCreateRequest request,
        CancellationToken ct)
    {
        try
        {
            int id = await createTask.ExecuteAsync(
                request.ProjectId,
                request.Title,
                request.Description,
                request.Status,
                request.Priority,
                request.AssigneeId,
                ct);

            return Ok(new { id });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> Get(int id)
    {
        TaskItem? task = await tasks.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(task);
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<ActionResult<PageResponse<TaskItem>>> ListByProject(
        int projectId,
        [FromQuery] PageRequest page,
        [FromQuery] int? status,
        [FromQuery] int? assigneeId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        TaskStatus? parsedStatus = null;

        if (status.HasValue)
        {
            if (!Enum.IsDefined(typeof(TaskStatus), status.Value))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid status.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            parsedStatus = (TaskStatus)status.Value;
        }

        IReadOnlyList<TaskItem> items = await tasks.GetPageByProjectAsync(
            projectId,
            page.Page,
            page.Size,
            parsedStatus,
            assigneeId,
            search,
            ct);

        long total = await tasks.CountByProjectAsync(
            projectId,
            parsedStatus,
            assigneeId,
            search,
            ct);

        return Ok(new PageResponse<TaskItem>
        {
            Items = items,
            Page = page.Page,
            Size = page.Size,
            Total = total
        });
    }

    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult> Assign(
        int id,
        [FromBody] AssignRequest request,
        CancellationToken ct)
    {
        try
        {
            await assigner.ExecuteAsync(id, request.AssigneeId, ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{id:int}/status")]
    public async Task<ActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken ct)
    {
        try
        {
            await changeTaskStatus.ExecuteAsync(id, request.Status, ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found or invalid status change.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<object>> AddComment(
        int id,
        [FromBody] CommentCreateRequest request,
        CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj)
            || userIdObj is not int userId)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        try
        {
            int commentId = await addComment.ExecuteAsync(
                id,
                userId,
                request.Text,
                ct);

            return Ok(new { id = commentId });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<PageResponse<TaskComment>>> GetComments(
        int id,
        [FromQuery] PageRequest page,
        CancellationToken ct)
    {
        try
        {
            IReadOnlyList<TaskComment> all =
                await getTaskComments.ExecuteAsync(id, ct);

            int total = all.Count;

            int skip = (page.Page - 1) * page.Size;
            if (skip < 0) skip = 0;

            var items = all
                .Skip(skip)
                .Take(page.Size)
                .ToList();

            return Ok(new PageResponse<TaskComment>
            {
                Items = items,
                Page = page.Page,
                Size = page.Size,
                Total = total
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found.",
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IReadOnlyList<TimelineEvent>>> History(
        int id,
        CancellationToken ct)
    {
        IReadOnlyList<TimelineEvent> history =
            await getTaskHistory.ExecuteAsync(id, ct);

        return Ok(history);
    }
}