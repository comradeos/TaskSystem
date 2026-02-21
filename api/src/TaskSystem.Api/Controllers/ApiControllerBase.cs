using Microsoft.AspNetCore.Mvc;

namespace TaskSystem.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult NotFoundProblem(string title)
    {
        return NotFound(new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status404NotFound
        });
    }

    protected ActionResult BadRequestProblem(string title)
    {
        return BadRequest(new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest
        });
    }

    protected ActionResult ConflictProblem(string title)
    {
        return Conflict(new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status409Conflict
        });
    }

    protected ActionResult UnauthorizedProblem(string title)
    {
        return Unauthorized(new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status401Unauthorized
        });
    }
}