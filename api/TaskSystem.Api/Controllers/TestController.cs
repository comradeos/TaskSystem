using Microsoft.AspNetCore.Mvc;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

// тестовй контролер для перевірки 
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        User? user = HttpContext.Items["User"] as User;

        if (user is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
                Detail = "session required"
            }));
        }

        return Ok(ApiResponse.Success(new
        {
            user.Id,
            user.Name,
            user.IsAdmin
        }));
    }

    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        User? user = HttpContext.Items["User"] as User;

        if (user is null)
        {
            return Unauthorized(ApiResponse.Failure(new ProblemDetails
            {
                Title = "unauthorized",
                Status = 401,
                Detail = "session required"
            }));
        }

        if (!user.IsAdmin)
        {
            return StatusCode(403, ApiResponse.Failure(new ProblemDetails
            {
                Title = "forbidden",
                Status = 403,
                Detail = "admin access required"
            }));
        }

        return Ok(ApiResponse.Success("admin endpoint reached"));
    }
}