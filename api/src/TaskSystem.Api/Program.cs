using TaskSystem.Api.Middleware;
using TaskSystem.Application.Abstractions;
using TaskSystem.Infrastructure.InMemory;
using TaskSystem.Infrastructure.Mongo;
using TaskSystem.Infrastructure.Postgres;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaskSystem.Application.Projects;
using TaskSystem.Application.Tasks;

namespace TaskSystem.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        await ConfigureCoreDbAsync(builder);
        
        ConfigureTimelineDb(builder);

        builder.Services.AddScoped<IGetTaskHistory, GetTaskHistory>();
        builder.Services.AddScoped<ICreateProject, CreateProject>();
        builder.Services.AddScoped<ICreateTask, CreateTask>();
        builder.Services.AddScoped<IAssignTask, AssignTask>();
        builder.Services.AddScoped<IChangeTaskStatus, ChangeTaskStatus>();
        builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
        builder.Services.AddControllers();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssembly(typeof(Application.DTO.Projects.ProjectCreateRequest).Assembly);
        builder.Services.AddEndpointsApiExplorer();

        WebApplication app = builder.Build();

        app.UseMiddleware<TraceIdMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<AuthMiddleware>();

        app.MapControllers();

        await app.RunAsync();
    }

    private static async Task ConfigureCoreDbAsync(WebApplicationBuilder builder)
    {
        string connectionString = Require(builder, "ConnectionStrings:PostgresCore");

        string coreUser = Require(builder, "CORE_DATA_USER");
        string corePassword = Require(builder, "CORE_DATA_PASSWORD");

        await PostgresSchemaInitializer.InitializeAsync(connectionString, coreUser, corePassword);

        builder.Services.AddScoped<ICoreDbConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));
        builder.Services.AddScoped<IProjectRepository, PostgresProjectRepository>();
        builder.Services.AddScoped<ITaskRepository, PostgresTaskRepository>();
        builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
    }

    private static void ConfigureTimelineDb(WebApplicationBuilder builder)
    {
        string mongoConnection = Require(builder, "ConnectionStrings:MongoTimeline");
        string mongoDatabase = Require(builder, "MONGO_TIMELINE_DATABASE");

        builder.Services.AddSingleton<ITimelineRepository>(_ => new MongoTimelineRepository(mongoConnection, mongoDatabase));
    }

    private static string Require(WebApplicationBuilder builder, string key)
    {
        string? value = builder.Configuration[key];

        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Missing required configuration: {key}")
            : value;
    }
}