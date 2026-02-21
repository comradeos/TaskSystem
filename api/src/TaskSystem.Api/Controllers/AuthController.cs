using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.DTO.Auth;
using TaskSystem.Domain.Auth;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    IUserRepository users,
    ISessionStore sessions
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        bool isInvalidRequest =
            string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password);

        if (isInvalidRequest)
        {
            return Problem(
                title: "Invalid credentials",
                statusCode: 400
            );
        }

        User? user = await users.GetByLoginAsync(request.Login);

        if (user is null)
        {
            return Problem(
                title: "Invalid login or password",
                statusCode: 401
            );
        }

        bool passwordValid =
            BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            return Problem(
                title: "Invalid login or password",
                statusCode: 401
            );
        }

        UserSession session = sessions.Create(user.Id, user.Login);

        return Ok(new
        {
            token = session.Token,
            expiresAt = session.ExpiresAt
        });
    }
}