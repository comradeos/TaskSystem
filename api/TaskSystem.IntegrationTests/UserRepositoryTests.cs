using Dapper;
using Microsoft.Extensions.Configuration;
using TaskSystem.Infrastructure.Database;
using TaskSystem.Infrastructure.Repositories;
using User = TaskSystem.Domain.Entities.User;

namespace TaskSystem.IntegrationTests;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task Create_ThenGetById_ReturnsUser()
    {
        IConfiguration config = BuildConfiguration();

        PostgresConnection connection = new(config);
        UserRepository repo = new(connection);

        int id = 0;

        string login = $"it_user_{Guid.NewGuid():N}";
        
        User user = new User(
            0,
            "Integration Test User",
            login,
            "hash",
            false,
            DateTime.UtcNow);

        try
        {
            id = await repo.CreateAsync(user);

            User? loaded = await repo.GetByIdAsync(id);

            Assert.NotNull(loaded);
            Assert.Equal(id, loaded!.Id);
            Assert.Equal(user.Name, loaded.Name);
            Assert.Equal(user.Login, loaded.Login);
            Assert.Equal(user.PasswordHash, loaded.PasswordHash);
            Assert.Equal(user.IsAdmin, loaded.IsAdmin);
        }
        finally
        {
            if (id > 0)
            {
                using var db = connection.Create();
                await db.ExecuteAsync(
                    "DELETE FROM users WHERE id = @Id",
                    new { Id = id });
            }
        }
    }

    [Fact]
    public async Task Create_ThenGetByLogin_ReturnsUser()
    {
        IConfiguration config = BuildConfiguration();

        var connection = new PostgresConnection(config);
        var repo = new UserRepository(connection);

        int id = 0;

        var login = $"it_user_{Guid.NewGuid():N}";
        var user = new User(
            0,
            "Integration Test User",
            login,
            "hash",
            true,
            DateTime.UtcNow);

        try
        {
            id = await repo.CreateAsync(user);

            User? loaded = await repo.GetByLoginAsync(login);

            Assert.NotNull(loaded);
            Assert.Equal(id, loaded!.Id);
            Assert.Equal(user.Name, loaded.Name);
            Assert.Equal(user.Login, loaded.Login);
            Assert.Equal(user.PasswordHash, loaded.PasswordHash);
            Assert.Equal(user.IsAdmin, loaded.IsAdmin);
        }
        finally
        {
            if (id > 0)
            {
                using var db = connection.Create();
                await db.ExecuteAsync(
                    "DELETE FROM users WHERE id = @Id",
                    new { Id = id });
            }
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        // знаю шо це хард код але для псевдо тестів нічого крашого не вигадав ) сорі 
        Dictionary<string, string?> dict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:PostgresCore"] =
                "Host=core-data;Port=5432;Database=core_data;Username=root;Password=root"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }
}