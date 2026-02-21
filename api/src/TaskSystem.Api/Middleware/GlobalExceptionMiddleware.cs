using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace TaskSystem.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            string traceId =
                Activity.Current?.Id ??
                context.TraceIdentifier;

            logger.LogError(
                ex,
                "Unhandled exception. TraceId: {TraceId}",
                traceId
            );

            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;

            context.Response.ContentType =
                "application/problem+json";

            ProblemDetails problem = new()
            {
                Title = "Internal Server Error",
                Status = 500,
                Detail = "An unexpected error occurred.",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = traceId
                }
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}