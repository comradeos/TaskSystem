using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Api.Middleware;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISessionRepository sessionRepository,
        IUserRepository userRepository)
    {
        if (!context.Request.Headers.TryGetValue("X-Session-Token", out var token))
        {
            await _next(context);
            return;
        }

        var session = await sessionRepository.GetByTokenAsync(token!);

        if (session is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                result = false,
                data = new
                {
                    title = "Unauthorized",
                    status = 401,
                    detail = "Invalid session token"
                }
            });

            return;
        }

        var user = await userRepository.GetByIdAsync(session.UserId);

        if (user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.Items["User"] = user;

        await _next(context);
    }
}