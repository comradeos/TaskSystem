using Serilog;
using TaskSystem.Api.Common;
using TaskSystem.Infrastructure.Database;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Repositories;
using TaskSystem.Api.Services;
using TaskSystem.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.WriteTo.Console();
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientCorsPolicy", policy =>
    {
        policy.WithOrigins(
                // залишив обидва порти для зручності шоб фронт не тестувати в докері
                "http://localhost:5173",
                "http://localhost:3001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PostgresConnection>();
builder.Services.AddSingleton<MongoConnection>();
builder.Services.AddSingleton<ICsvExportService, CsvExportService>();

builder.Services.AddScoped<ITimelineRepository, TimelineRepository>();
builder.Services.AddScoped<TimelineService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.UseCors("ClientCorsPolicy");

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SessionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();