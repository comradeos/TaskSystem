using System.Reflection;
using Dapper;
using Npgsql;

namespace TaskSystem.Infrastructure.Postgres;

public static class PostgresSchemaInitializer
{
    private const string MigrationFileName = "PostgresSchemaTables.sql";
    private const string DefaultRootLogin = "root";

    public static async Task InitializeAsync(string connectionString, string rootPassword)
    {
        await using NpgsqlConnection connection = new(connectionString);
        
        await connection.OpenAsync();
        
        await ExecuteMigrationAsync(connection);
        
        await SeedRootUserAsync(connection, rootPassword);
    }

    private static async Task ExecuteMigrationAsync(NpgsqlConnection connection)
    {
        string sql = await LoadEmbeddedSqlAsync(MigrationFileName);
        
        await connection.ExecuteAsync(sql);
    }

    private static async Task<string> LoadEmbeddedSqlAsync(string fileName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Migration file '{fileName}' not found.");

        await using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Unable to load resource '{resourceName}'.");

        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync();
    }

    private static async Task SeedRootUserAsync(NpgsqlConnection connection, string rootPassword)
    {
        const string existsSql = "SELECT COUNT(*) FROM users WHERE login = @Login";

        int exists = await connection.ExecuteScalarAsync<int>(existsSql, new { Login = DefaultRootLogin });

        if (exists > 0)
        {
            return;
        }

        const string insertSql = "INSERT INTO users (login, password_hash, name, created_at) VALUES (@Login, @PasswordHash, @Name, @CreatedAt)";

        string? passwordHash = BCrypt.Net.BCrypt.HashPassword(rootPassword);

        await connection.ExecuteAsync(insertSql, new
        {
            Login = DefaultRootLogin,
            PasswordHash = passwordHash,
            Name = DefaultRootLogin,
            CreatedAt = DateTime.UtcNow
        });
    }
}