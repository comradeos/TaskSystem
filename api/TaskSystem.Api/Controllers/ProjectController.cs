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
        var projects = await _projectRepository.GetAllAsync();

        var result = projects.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(project))
            return NotFoundResponse("Project not found");

        var dto = MapperHelper.ToDto(project);

        return Ok(ApiResponse.Success(dto));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(project))
            return NotFoundResponse("Project not found");

        var events = await _timelineRepository
            .GetByEntityAsync("Project", id);

        var result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequestDto request)
    {
        var user = CurrentUser;

        if (!RequestValidator.NotEmpty(request.Name))
            return BadRequestResponse("Project name is required");

        var project = new Project(
            0,
            request.Name,
            DateTime.UtcNow);

        var id = await _projectRepository.CreateAsync(project);

        await _timelineService.ProjectCreated(
            id,
            user.Id,
            user.Name,
            new
            {
                request.Name
            });

        return Ok(ApiResponse.Success(new CreateProjectResponseDto
        {
            Id = id
        }));
    }
}