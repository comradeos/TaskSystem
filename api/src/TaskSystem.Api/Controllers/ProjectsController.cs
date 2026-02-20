using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Projects;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("projects")]
public sealed class ProjectsController(IProjectRepository projects) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ProjectResponse>> Create([FromBody] ProjectCreateRequest request)
    {
        bool existsByName = await projects.ExistsByNameAsync(request.Name);

        if (existsByName)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Project with the same name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        try
        {
            Project project = new Project
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            int id = await projects.CreateAsync(project);

            return Ok(new ProjectResponse
            {
                Id = id,
                Name = project.Name,
                CreatedAt = project.CreatedAt
            });
        }
        catch (Exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Project with the same name already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }
    
    [HttpGet("list")]
    public async Task<ActionResult<ProjectListResponse>> List()
    {
        IReadOnlyList<Project> projectsList = await projects.GetAllAsync();
        
        IReadOnlyList<ProjectResponse> projectResponses = projectsList
            .Select(p => new ProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                CreatedAt = p.CreatedAt
            })
            .ToList();
        
        ProjectListResponse response = new()
        {
            Items = projectResponses
        };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Get(int id)
    {
        Project? project = await projects.GetByIdAsync(id);

        if (project is null)
        {
            return Problem(title: "Project not found", statusCode: 404);
        }
        
        ProjectResponse response = new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            CreatedAt = project.CreatedAt
        };

        return Ok(response);
    }
}