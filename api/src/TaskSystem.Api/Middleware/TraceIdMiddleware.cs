using System.Diagnostics;

namespace TaskSystem.Api.Middleware;

public sealed class TraceIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string traceId =
            Activity.Current?.Id ??
            context.TraceIdentifier;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] =
                traceId;

            return Task.CompletedTask;
        });

        await next(context);
    }
}