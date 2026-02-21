using System.Text;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Enums;
using TaskSystem.Domain.Interfaces;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimelineService _timelineService;
    private readonly ITimelineRepository _timelineRepository;

    public TaskController(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        TimelineService timelineService,
        ITimelineRepository timelineRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _timelineService = timelineService;
        _timelineRepository = timelineRepository;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequestDto request)
    {
        var user = HttpContext.Items["User"] as User;

        if (user is null)
            return UnauthorizedResponse();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequestResponse("Title is required");

        if (!Enum.IsDefined(typeof(TaskStatus), request.Status))
            return BadRequestResponse("Invalid task status");

        if (!Enum.IsDefined(typeof(TaskPriority), request.Priority))
            return BadRequestResponse("Invalid task priority");

        var project = await _projectRepository.GetByIdAsync(request.ProjectId);

        if (project is null)
            return BadRequestResponse("Project does not exist");

        string? assignedUserName = null;

        if (request.AssignedUserId.HasValue)
        {
            var assignedUser =
                await _userRepository.GetByIdAsync(request.AssignedUserId.Value);

            if (assignedUser is null)
                return BadRequestResponse("Assigned user does not exist");

            assignedUserName = assignedUser.Name;
        }

        var task = new TaskSystem.Domain.Entities.Task(
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
        var user = HttpContext.Items["User"] as User;

        if (user is null)
            return UnauthorizedResponse();

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
            var csv = GenerateCsv(tasks);
            var bytes = Encoding.UTF8.GetBytes(csv);

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
        var user = HttpContext.Items["User"] as User;

        if (user is null)
            return UnauthorizedResponse();

        var task = await _taskRepository.GetByIdAsync(id);

        if (task is null)
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

            if (assignedUser is null)
                return BadRequestResponse("Assigned user does not exist");

            assignedUserName = assignedUser.Name;
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
        var user = HttpContext.Items["User"] as User;

        if (user is null)
            return UnauthorizedResponse();

        var task = await _taskRepository.GetByIdAsync(id);

        if (task is null)
            return NotFoundResponse("Task not found");

        var events = await _timelineRepository
            .GetByEntityAsync("Task", id);

        var result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    private IActionResult UnauthorizedResponse() =>
        Unauthorized(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Unauthorized",
            Status = 401,
            Detail = "Session required"
        }));

    private IActionResult BadRequestResponse(string message) =>
        BadRequest(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Invalid request",
            Status = 400,
            Detail = message
        }));

    private IActionResult NotFoundResponse(string message) =>
        NotFound(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Not Found",
            Status = 404,
            Detail = message
        }));

    private string GenerateCsv(IEnumerable<TaskSystem.Domain.Entities.Task> tasks)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Id,Number,Title,Status,Priority,Author,AssignedTo,CreatedAt");

        foreach (var t in tasks)
        {
            sb.AppendLine(string.Join(",",
                t.Id,
                t.NumberInProject,
                Escape(t.Title),
                t.Status,
                t.Priority,
                Escape(t.AuthorUserName),
                Escape(t.AssignedUserName),
                t.CreatedAt));
        }

        return sb.ToString();
    }

    private string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        if (value.Contains(",") || value.Contains("\""))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}