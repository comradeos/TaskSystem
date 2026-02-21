using FluentValidation;
using FluentValidation.AspNetCore;
using TaskSystem.Api.Middleware;
using TaskSystem.Application.Abstractions;
using TaskSystem.Application.Projects;
using TaskSystem.Application.Tasks;
using TaskSystem.Infrastructure.InMemory;
using TaskSystem.Infrastructure.Mongo;
using TaskSystem.Infrastructure.Postgres;

namespace TaskSystem.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssembly(
            typeof(Application.DTO.Projects.ProjectCreateRequest).Assembly
        );

        builder.Services.AddUseCases();
        builder.Services.AddSessionStore();

        await builder.Services.AddCoreDataAsync(builder.Configuration);
        builder.Services.AddTimelineData(builder.Configuration);

        WebApplication app = builder.Build();

        app.UseMiddleware<TraceIdMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<AuthMiddleware>();

        app.MapControllers();

        await app.RunAsync();
    }
}

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetTaskHistoryUseCase, GetTaskHistory>();
        services.AddScoped<IGetTaskCommentsUseCase, GetTaskComments>();

        services.AddScoped<ICreateProjectUseCase, CreateProject>();
        services.AddScoped<ICreateTaskUseCase, CreateTask>();

        services.AddScoped<IAssignTaskUseCase, AssignTask>();
        services.AddScoped<IChangeTaskStatusUseCase, ChangeTaskStatus>();

        services.AddScoped<IAddTaskCommentUseCase, AddTaskComment>();

        return services;
    }

    public static IServiceCollection AddSessionStore(this IServiceCollection services)
    {
        services.AddSingleton<ISessionStore, InMemorySessionStore>();

        return services;
    }

    public static async Task<IServiceCollection> AddCoreDataAsync(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string connectionString = configuration.Require("ConnectionStrings:PostgresCore");
        string coreUser = configuration.Require("CORE_DATA_USER");
        string corePassword = configuration.Require("CORE_DATA_PASSWORD");

        await PostgresSchemaInitializer.InitializeAsync(
            connectionString,
            coreUser,
            corePassword
        );

        services.AddSingleton<ICoreDbConnectionFactory>(
            new PostgresConnectionFactory(connectionString)
        );

        services.AddScoped<IProjectRepository, PostgresProjectRepository>();
        services.AddScoped<ITaskRepository, PostgresTaskRepository>();
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<ICommentRepository, PostgresCommentRepository>();

        return services;
    }

    public static IServiceCollection AddTimelineData(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string mongoConnection = configuration.Require("ConnectionStrings:MongoTimeline");
        string mongoDatabase = configuration.Require("MONGO_TIMELINE_DATABASE");

        services.AddSingleton<ITimelineRepository>(
            new MongoTimelineRepository(mongoConnection, mongoDatabase)
        );

        return services;
    }
}

internal static class ConfigurationExtensions
{
    public static string Require(this IConfiguration configuration, string key)
    {
        string? value = configuration[key];

        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException(
                $"Missing required configuration: {key}"
              )
            : value;
    }
}