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
[Route("api/users")]
public class UserController : BaseApiController
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
        IEnumerable<User> users = await _userRepository.GetAllAsync();

        IEnumerable<UserDto> result = users.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        User? user = await _userRepository.GetByIdAsync(id);

        if (!RequestValidator.NotNull(user))
        {
            return NotFoundResponse("user not found");
        }

        IEnumerable<TimelineEvent> events = await _timelineRepository.GetByEntityAsync("User", id);

        IEnumerable<TimelineEventDto> result = events.Select(MapperHelper.ToDto);

        return Ok(ApiResponse.Success(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        User currentUser = CurrentUser;

        if (!currentUser.IsAdmin)
        {
            return ForbiddenResponse("admin only");
        }

        // можна було б зробити окрему валідацію для кожного поля
        if (!RequestValidator.NotEmpty(request.Name) ||
            !RequestValidator.NotEmpty(request.Login) ||
            !RequestValidator.NotEmpty(request.Password))
        {
            return BadRequestResponse("name login password are required");
        }

        string? passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        User user = new User(
            0,
            request.Name,
            request.Login,
            passwordHash,
            request.IsAdmin,
            DateTime.UtcNow
        );

        int id = await _userRepository.CreateAsync(user);

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
        
        CreateUserResponseDto dto = new() { Id = id };

        return Ok(ApiResponse.Success(dto));
    }
}