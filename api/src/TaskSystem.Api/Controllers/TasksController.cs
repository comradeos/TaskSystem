using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Tasks;
using TaskSystem.Application.Tasks;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TasksController(
    ICreateTask createTask,
    IAssignTask assignTask,
    IChangeTaskStatus changeTaskStatus,
    IGetTaskHistory getTaskHistory,
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
    public async Task<ActionResult<IReadOnlyList<TaskItem>>> ListByProject(int projectId)
    {
        IReadOnlyList<TaskItem> result = await tasks.GetByProjectAsync(projectId);
        return Ok(result);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult> Assign(
        int id,
        [FromBody] AssignTaskRequest request,
        CancellationToken ct)
    {
        try
        {
            await assignTask.ExecuteAsync(id, request.AssigneeId, ct);
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
        [FromBody] ChangeTaskStatusRequest request,
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