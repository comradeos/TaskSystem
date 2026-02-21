using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Common;
using TaskSystem.Application.DTO.Projects;
using TaskSystem.Application.Projects;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController(
    ICreateProjectUseCase createProject,
    IProjectRepository projects
) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] ProjectCreateRequest request,
        CancellationToken ct
    )
    {
        try
        {
            int id = await createProject.ExecuteAsync(request.Name, ct);

            Project? project = await projects.GetByIdAsync(id);

            if (project is null)
            {
                return Problem(
                    title: "Project creation failed.",
                    statusCode: 500
                );
            }

            return Ok(new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                CreatedAt = project.CreatedAt
            });
        }
        catch (InvalidOperationException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Project with the same name already exists.",
                Status = StatusCodes.Status409Conflict
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

    [HttpGet("list")]
    public async Task<ActionResult<PageResponse<ProjectResponse>>> List(
        [FromQuery] PageRequest request,
        CancellationToken ct
    )
    {
        IReadOnlyList<Project> page =
            await projects.GetPageAsync(request.Page, request.Size, ct);

        long total = await projects.CountAsync(ct);

        IReadOnlyList<ProjectResponse> items = page
            .Select(p => new ProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                CreatedAt = p.CreatedAt
            })
            .ToList();

        return Ok(new PageResponse<ProjectResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            Total = total
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Get(int id)
    {
        Project? project = await projects.GetByIdAsync(id);

        if (project is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            CreatedAt = project.CreatedAt
        });
    }
}