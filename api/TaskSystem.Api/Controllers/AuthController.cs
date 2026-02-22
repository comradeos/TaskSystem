using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using TaskSystem.Api.Common;
using TaskSystem.Api.DTOs;
using TaskSystem.Api.Services;
using TaskSystem.Domain.Common;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!RequestValidator.NotEmpty(request.Login) ||
            !RequestValidator.NotEmpty(request.Password))
        {
            return BadRequestResponse("Login and password are required");
        }

        LoginResult? result = await _authService.LoginAsync(request.Login, request.Password);

        if (result is null)
        {
            return UnauthorizedResponse("Invalid login or password");
        }

        LoginResponseDto response = new()
        {
            SessionToken = result.SessionToken,
            Id = result.Id,
            Name = result.Name,
            IsAdmin = result.IsAdmin
        };

        return Ok(ApiResponse.Success(response));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Headers.TryGetValue("X-Session-Token", out StringValues token))
        {
            return UnauthorizedResponse("Session token required");
        }

        await _authService.LogoutAsync(token!);

        return Ok(ApiResponse.Success("Logged out"));
    }
}