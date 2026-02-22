using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.Common;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using Task = TaskSystem.Domain.Entities.Task;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : BaseApiController
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
        User user = CurrentUser;

        if (!RequestValidator.NotEmpty(request.Content))
        {
            return BadRequestResponse("Content is required");
        }

        Task? task = await _taskRepository.GetByIdAsync(request.TaskId);

        if (!RequestValidator.NotNull(task))
        {
            return BadRequestResponse("Task does not exist");
        }

        Comment comment = new(
            0,
            request.TaskId,
            user.Id,
            user.Name,
            request.Content,
            DateTime.UtcNow
        );

        int id = await _commentRepository.CreateAsync(comment);

        await _timelineService.CommentAdded(
            request.TaskId,
            user.Id,
            user.Name,
            new { request.Content }
        );
        
        CreateCommentResponseDto dto = new() { Id = id };

        return Ok(ApiResponse.Success(dto));
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetByTask(int taskId)
    {
        Task? task = await _taskRepository.GetByIdAsync(taskId);

        if (!RequestValidator.NotNull(task))
        {
            return BadRequestResponse("Task does not exist");
        }

        IEnumerable<Comment> comments = await _commentRepository.GetByTaskAsync(taskId);

        IEnumerable<CommentDto> result = comments.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }
}