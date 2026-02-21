using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly TimelineService _timelineService;

    public CommentController(
        ICommentRepository commentRepository,
        ITaskRepository taskRepository,
        TimelineService timelineService)
    {
        _commentRepository = commentRepository;
        _taskRepository = taskRepository;
        _timelineService = timelineService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequestDto request)
    {
        var user = HttpContext.Items["User"] as User;

        if (user is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session required"
            }));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Invalid request",
                Status = 400,
                Detail = "Content is required"
            }));
        }

        var task = await _taskRepository.GetByIdAsync(request.TaskId);

        if (task is null)
        {
            return BadRequest(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Invalid task",
                Status = 400,
                Detail = "Task does not exist"
            }));
        }

        var comment = new Comment(
            0,
            request.TaskId,
            user.Id,
            user.Name,
            request.Content,
            DateTime.UtcNow);

        var id = await _commentRepository.CreateAsync(comment);

        await _timelineService.CommentAdded(
            request.TaskId,
            user.Id,
            user.Name,
            new
            {
                request.Content
            });

        return Ok(ApiResponse.Success(new CreateCommentResponseDto
        {
            Id = id
        }));
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetByTask(int taskId)
    {
        var user = HttpContext.Items["User"] as User;

        if (user is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session required"
            }));
        }

        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task is null)
        {
            return BadRequest(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Invalid task",
                Status = 400,
                Detail = "Task does not exist"
            }));
        }

        var comments = await _commentRepository.GetByTaskAsync(taskId);

        var result = comments.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }
}