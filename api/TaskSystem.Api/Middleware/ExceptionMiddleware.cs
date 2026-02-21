using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TaskSystem.Domain.Common;

namespace TaskSystem.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            if (ex is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;

                var conflictProblem = new ProblemDetails
                {
                    Title = "Conflict",
                    Status = 409,
                    Detail = "Entity already exists"
                };

                conflictProblem.Extensions["traceId"] = traceId;

                var conflictResponse = ApiResponse.Failure(conflictProblem);

                await context.Response.WriteAsJsonAsync(conflictResponse);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var defaultProblem = new ProblemDetails
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred"
            };

            defaultProblem.Extensions["traceId"] = traceId;

            var defaultResponse = ApiResponse.Failure(defaultProblem);

            await context.Response.WriteAsJsonAsync(defaultResponse);
        }
    }
}