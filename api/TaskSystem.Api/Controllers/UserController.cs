using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Helpers;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly TimelineService _timelineService;
    private readonly ITimelineRepository _timelineRepository;

    public UserController(
        IUserRepository userRepository,
        TimelineService timelineService,
        ITimelineRepository timelineRepository)
    {
        _userRepository = userRepository;
        _timelineService = timelineService;
        _timelineRepository = timelineRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUser = HttpContext.Items["User"] as User;

        if (currentUser is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session required"
            }));
        }

        var users = await _userRepository.GetAllAsync();

        var result = users.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var currentUser = HttpContext.Items["User"] as User;

        if (currentUser is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session required"
            }));
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Not Found",
                Status = 404,
                Detail = "User not found"
            }));
        }

        var events = await _timelineRepository
            .GetByEntityAsync("User", id);

        var result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        var currentUser = HttpContext.Items["User"] as User;

        if (currentUser is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session required"
            }));
        }

        if (!currentUser.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse.Failure(new ProblemDetails
                {
                    Title = "Forbidden",
                    Status = 403,
                    Detail = "Admin only"
                }));
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Invalid request",
                Status = 400,
                Detail = "Name, login and password are required"
            }));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            0,
            request.Name,
            request.Login,
            passwordHash,
            request.IsAdmin,
            DateTime.UtcNow);

        var id = await _userRepository.CreateAsync(user);

        await _timelineService.UserCreated(
            id,
            currentUser.Id,
            currentUser.Name,
            new
            {
                request.Name,
                request.Login,
                request.IsAdmin
            });

        return Ok(ApiResponse.Success(new CreateUserResponseDto
        {
            Id = id
        }));
    }
}