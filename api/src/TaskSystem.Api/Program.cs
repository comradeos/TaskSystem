using TaskSystem.Application.Abstractions;
using TaskSystem.Infrastructure.Postgres;
using TaskSystem.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// STRICT ENV CONFIG (NO FALLBACK)
// ----------------------------------------------------

string Require(string key)
{
    var value = builder.Configuration[key];
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"Missing required configuration: {key}");
    return value;
}

var provider = Require("CORE_DB_PROVIDER");

// ----------------------------------------------------
// CORE DATABASE
// ----------------------------------------------------

if (provider.Equals("postgre", StringComparison.OrdinalIgnoreCase))
{
    var connectionString = Require("ConnectionStrings:PostgresCore");

    builder.Services.AddScoped<ICoreDbConnectionFactory>(
        _ => new PostgresConnectionFactory(connectionString));

    builder.Services.AddScoped<IProjectRepository, PostgresProjectRepository>();
}
else if (provider.Equals("mongo", StringComparison.OrdinalIgnoreCase))
{
    throw new NotImplementedException("Mongo core provider not implemented yet");
}
else
{
    throw new InvalidOperationException(
        $"Unknown CORE_DB_PROVIDER: {provider}");
}

// ----------------------------------------------------
// TIMELINE (ALWAYS MONGO)
// ----------------------------------------------------

var mongoConnection = Require("ConnectionStrings:MongoTimeline");
var mongoDatabase = Require("MONGO_TIMELINE_DATABASE");

builder.Services.AddSingleton<ITimelineRepository>(
    _ => new MongoTimelineRepository(mongoConnection, mongoDatabase));

// ----------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ----------------------------------------------------
// BASIC ENDPOINTS
// ----------------------------------------------------

app.MapGet("/", () => Results.Ok(new
{
    message = "TaskSystem API is running"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    provider
}));

// ----------------------------------------------------
// POSTGRES HEALTH CHECK
// ----------------------------------------------------

app.MapGet("/health/postgre", (ICoreDbConnectionFactory factory) =>
{
    try
    {
        using var connection = factory.CreateConnection();
        connection.Open();

        return Results.Ok(new { status = "PostgreSQL connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "PostgreSQL connection failed",
            detail: ex.Message,
            statusCode: 500);
    }
});

// ----------------------------------------------------
// MONGO HEALTH CHECK (REAL PING)
// ----------------------------------------------------

app.MapGet("/health/timeline", async (ITimelineRepository repo) =>
{
    try
    {
        await repo.PingAsync();
        return Results.Ok(new { status = "Mongo connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Mongo connection failed",
            detail: ex.Message,
            statusCode: 500);
    }
});

app.Run();