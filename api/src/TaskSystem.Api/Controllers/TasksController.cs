using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Tasks;
using TaskSystem.Application.Tasks;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TasksController(
    IAssignTask assignTask,
    ITaskRepository tasks,
    IProjectRepository projects)
    : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<object>> Create(
        [FromBody] TaskCreateRequest request)
    {
        Project? project = await projects.GetByIdAsync(request.ProjectId);

        if (project is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        int id = await tasks.CreateAsync(
            request.ProjectId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.AssigneeId);

        return Ok(new { id });
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
}