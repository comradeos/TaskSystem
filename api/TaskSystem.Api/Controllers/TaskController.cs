using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.Common;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Enums;
using TaskSystem.Domain.Interfaces;
using Task = TaskSystem.Domain.Entities.Task;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : BaseApiController
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimelineService _timelineService;
    private readonly ITimelineRepository _timelineRepository;
    private readonly ICsvExportService _csvExportService;

    public TaskController(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        TimelineService timelineService,
        ITimelineRepository timelineRepository,
        ICsvExportService csvExportService)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _timelineService = timelineService;
        _timelineRepository = timelineRepository;
        _csvExportService = csvExportService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequestDto request)
    {
        var user = CurrentUser;

        if (!RequestValidator.NotEmpty(request.Title))
            return BadRequestResponse("Title is required");

        if (!Enum.IsDefined(typeof(TaskStatus), request.Status))
            return BadRequestResponse("Invalid task status");

        if (!Enum.IsDefined(typeof(TaskPriority), request.Priority))
            return BadRequestResponse("Invalid task priority");

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);

        if (!RequestValidator.NotNull(project))
            return BadRequestResponse("Project does not exist");

        string? assignedUserName = null;

        if (request.AssignedUserId.HasValue)
        {
            var assignedUser =
                await _userRepository.GetByIdAsync(request.AssignedUserId.Value);

            if (!RequestValidator.NotNull(assignedUser))
                return BadRequestResponse("Assigned user does not exist");

            assignedUserName = assignedUser?.Name;
        }

        var task = new Task(
            0,
            request.ProjectId,
            0,
            request.Title,
            request.Description,
            (TaskStatus)request.Status,
            (TaskPriority)request.Priority,
            user.Id,
            user.Name,
            request.AssignedUserId,
            assignedUserName,
            DateTime.UtcNow,
            DateTime.UtcNow);

        var id = await _taskRepository.CreateAsync(task);

        await _timelineService.TaskCreated(
            id,
            user.Id,
            user.Name,
            new
            {
                request.Title,
                request.Status,
                request.Priority,
                request.AssignedUserId
            });

        return Ok(ApiResponse.Success(new CreateTaskResponseDto
        {
            Id = id
        }));
    }

    [HttpGet("{projectId:int}")]
    public async Task<IActionResult> GetByProject(
        int projectId,
        int page = 1,
        int size = 10,
        int? status = null,
        int? assignedUserId = null,
        string? search = null,
        bool export = false)
    {
        if (!export && (page < 1 || size < 1 || size > 100))
            return BadRequestResponse("Invalid page or size");

        if (status.HasValue &&
            !Enum.IsDefined(typeof(TaskStatus), status.Value))
            return BadRequestResponse("Invalid task status");

        var tasks = await _taskRepository.GetByProjectAsync(
            projectId,
            export ? 1 : page,
            export ? 100000 : size,
            status,
            assignedUserId,
            search);

        if (export)
        {
            var bytes = _csvExportService.ExportTasks(tasks);

            return File(
                bytes,
                "text/csv",
                $"tasks_project_{projectId}.csv");
        }

        var result = tasks.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTaskRequestDto request)
    {
        var user = CurrentUser;

        var task = await _taskRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(task))
            return NotFoundResponse("Task not found");

        if (request.Status.HasValue &&
            !Enum.IsDefined(typeof(TaskStatus), request.Status.Value))
            return BadRequestResponse("Invalid task status");

        if (request.Priority.HasValue &&
            !Enum.IsDefined(typeof(TaskPriority), request.Priority.Value))
            return BadRequestResponse("Invalid task priority");

        string? assignedUserName = null;

        if (request.AssignedUserId.HasValue)
        {
            var assignedUser =
                await _userRepository.GetByIdAsync(request.AssignedUserId.Value);

            if (!RequestValidator.NotNull(assignedUser))
                return BadRequestResponse("Assigned user does not exist");

            assignedUserName = assignedUser?.Name;
        }

        await _taskRepository.UpdateAsync(
            id,
            request.Status,
            request.Priority,
            request.AssignedUserId,
            assignedUserName);

        await _timelineService.TaskUpdated(
            id,
            user.Id,
            user.Name,
            new
            {
                request.Status,
                request.Priority,
                request.AssignedUserId
            });

        var updated = await _taskRepository.GetByIdAsync(id);

        return Ok(ApiResponse.Success(
            MapperHelper.ToDto(updated!)));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(task))
            return NotFoundResponse("Task not found");

        var events = await _timelineRepository
            .GetByEntityAsync("Task", id);

        var result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }
    
    [HttpGet("by-id/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(task))
            return NotFoundResponse("Task not found");

        var result = MapperHelper.ToDto(task!);

        return Ok(ApiResponse.Success(result));
    }
}