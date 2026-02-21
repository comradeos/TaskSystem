using Microsoft.AspNetCore.Mvc;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
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

        if (!user.IsAdmin)
        {
            return StatusCode(403, ApiResponse.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
                Detail = "Admin access required"
            }));
        }

        return Ok(ApiResponse.Success("Admin endpoint reached"));
    }
}