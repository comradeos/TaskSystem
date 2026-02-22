using Microsoft.AspNetCore.Mvc;
using TaskSystem.Domain.Common;
using TaskSystem.Domain.Entities;

namespace TaskSystem.Api.Common;

public abstract class BaseApiController : ControllerBase
{
    protected User CurrentUser
    {
        get
        {
            var user = HttpContext.Items["User"] as User;

            if (user is null)
                throw new UnauthorizedAccessException("Session required");

            return user;
        }
    }

    protected IActionResult UnauthorizedResponse(string message = "Session required")
    {
        return Unauthorized(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Unauthorized",
            Status = 401,
            Detail = message
        }));
    }

    protected IActionResult ForbiddenResponse(string message = "Forbidden")
    {
        return StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse.Failure(new ProblemDetails
            {
                Title = "Forbidden",
                Status = 403,
                Detail = message
            }));
    }

    protected IActionResult BadRequestResponse(string message)
    {
        return BadRequest(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Invalid request",
            Status = 400,
            Detail = message
        }));
    }

    protected IActionResult NotFoundResponse(string message)
    {
        return NotFound(ApiResponse.Failure(new ProblemDetails
        {
            Title = "Not Found",
            Status = 404,
            Detail = message
        }));
    }
}