using TaskSystem.Api.Middleware;
using TaskSystem.Application.Abstractions;
using TaskSystem.Infrastructure.InMemory;
using TaskSystem.Infrastructure.Mongo;
using TaskSystem.Infrastructure.Postgres;
using FluentValidation;
using FluentValidation.AspNetCore;
using TaskSystem.Application.Tasks;

namespace TaskSystem.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        string provider = Require(builder, "CORE_DB_PROVIDER");

        await ConfigureCoreDbAsync(builder, provider);
        
        ConfigureTimelineDb(builder);

        builder.Services.AddScoped<IAssignTask, AssignTask>();
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

    private static async Task ConfigureCoreDbAsync(WebApplicationBuilder builder, string provider)
    {
        string rootPassword = Require(builder, "DB_USER_ROOT_DEFAULT_PASSWORD"); // єтот пароль для всех баз данных, которые нужно инициализировать
        
        if (provider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
        {
            string connectionString = Require(builder, "ConnectionStrings:PostgresCore");

            await PostgresSchemaInitializer.InitializeAsync(connectionString, rootPassword);

            builder.Services.AddScoped<ICoreDbConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));
            builder.Services.AddScoped<IProjectRepository, PostgresProjectRepository>();
            builder.Services.AddScoped<ITaskRepository, PostgresTaskRepository>();
            builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();

            return;
        }

        if (provider.Equals("mongo", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotImplementedException("Mongo core provider not implemented yet.");
        }

        throw new InvalidOperationException($"Unknown CORE_DB_PROVIDER: {provider}");
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