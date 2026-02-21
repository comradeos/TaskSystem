using Microsoft.AspNetCore.Mvc;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Login and password are required",
                Status = StatusCodes.Status400BadRequest
            }));
        }

        var token = await _authService.LoginAsync(request.Login, request.Password);

        if (token is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid login or password",
                Status = StatusCodes.Status401Unauthorized
            }));
        }

        var response = new LoginResponseDto
        {
            SessionToken = token
        };

        return Ok(ApiResponse.Success(response));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Headers.TryGetValue("X-Session-Token", out var token))
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "Session token required"
            }));
        }

        await _authService.LogoutAsync(token!);

        return Ok(ApiResponse.Success("Logged out"));
    }
}