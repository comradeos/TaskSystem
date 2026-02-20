using System.Data;
using Microsoft.AspNetCore.Mvc;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(ICoreDbConnectionFactory coreDbFactory, ITimelineRepository timelineRepository) : ControllerBase
{
    [HttpGet]
    public IActionResult General()
    {
        return Ok(new
        {
            status = "ok",
            traceId = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("postgres")]
    public IActionResult Postgres()
    {
        try
        {
            using IDbConnection connection = coreDbFactory.CreateConnection();
            
            connection.Open();

            return Ok(new
            {
                status = "PostgreSQL connected",
                traceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception)
        {
            return Problem(
                title: "PostgreSQL connection failed",
                statusCode: 500);
        }
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> Timeline()
    {
        try
        {
            await timelineRepository.PingAsync();

            return Ok(new
            {
                status = "Timeline MongoDB connected",
                traceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception)
        {
            return Problem(
                title: "Timeline MongoDB connection failed",
                statusCode: 500);
        }
    }
}