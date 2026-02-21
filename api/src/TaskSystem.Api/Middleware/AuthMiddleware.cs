using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;
using TaskSystem.Domain.Auth;

namespace TaskSystem.Api.Middleware;

public sealed class AuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ISessionStore sessions
    )
    {
        string? path = context.Request.Path.Value;

        if (path != null && path.StartsWith("/auth/login"))
        {
            await next(context);
            return;
        }

        string? header =
            context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header) ||
            !header.StartsWith("Bearer "))
        {
            await WriteUnauthorized(context, "Unauthorized");
            return;
        }

        string token =
            header["Bearer ".Length..]
                .Trim();

        UserSession? session = sessions.Get(token);

        if (session is null)
        {
            await WriteUnauthorized(
                context,
                "Invalid or expired token"
            );
            return;
        }

        context.Items["UserId"] = session.UserId;
        context.Items["Login"] = session.Login;

        await next(context);
    }

    private static async Task WriteUnauthorized(
        HttpContext context,
        string title
    )
    {
        ProblemDetails problem = new()
        {
            Title = title,
            Status = StatusCodes.Status401Unauthorized,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.ContentType =
            "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}