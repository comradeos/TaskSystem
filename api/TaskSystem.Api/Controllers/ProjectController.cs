using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.Common;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController : BaseApiController
{
    private readonly IProjectRepository _projectRepository;
    private readonly TimelineService _timelineService;
    private readonly ITimelineRepository _timelineRepository;

    public ProjectController(
        IProjectRepository projectRepository,
        TimelineService timelineService,
        ITimelineRepository timelineRepository)
    {
        _projectRepository = projectRepository;
        _timelineService = timelineService;
        _timelineRepository = timelineRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Project> projects = await _projectRepository.GetAllAsync();

        IEnumerable<ProjectDto> result = projects.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Project? project = await _projectRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(project))
        {
            return NotFoundResponse("project not found");
        }

        ProjectDto dto = MapperHelper.ToDto(project);

        return Ok(ApiResponse.Success(dto));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        Project? project = await _projectRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(project))
        {
            return NotFoundResponse("project not found");
        }

        IEnumerable<TimelineEvent> events = await _timelineRepository.GetByEntityAsync("Project", id);

        IEnumerable<TimelineEventDto> result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequestDto request)
    {
        User user = CurrentUser;

        if (!RequestValidator.NotEmpty(request.Name))
        {
            return BadRequestResponse("project name is required");
        }

        Project project = new(0, request.Name, DateTime.UtcNow);

        int id = await _projectRepository.CreateAsync(project);

        await _timelineService.ProjectCreated(id, user.Id, user.Name, new { request.Name });

        CreateProjectResponseDto dto = new() { Id = id };
        
        return Ok(ApiResponse.Success(dto));
    }
}