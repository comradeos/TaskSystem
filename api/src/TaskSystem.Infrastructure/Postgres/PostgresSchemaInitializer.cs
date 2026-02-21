using System.Reflection;
using Dapper;
using Npgsql;

namespace TaskSystem.Infrastructure.Postgres;

public static class PostgresSchemaInitializer
{
    private const string MigrationFileName = "PostgresSchemaTables.sql";

    public static async Task InitializeAsync(string connectionString, string coreUser, string corePassword)
    {
        await using NpgsqlConnection connection = new(connectionString);
        
        await connection.OpenAsync();
        
        await ExecuteMigrationAsync(connection);
        
        await SeedCoreUserAsync(connection, coreUser, corePassword);
    }

    private static async Task ExecuteMigrationAsync(NpgsqlConnection connection)
    {
        string sql = await LoadEmbeddedSqlAsync(MigrationFileName);
        
        await connection.ExecuteAsync(sql);
    }

    private static async Task<string> LoadEmbeddedSqlAsync(string fileName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(x =>
                x.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Migration file '{fileName}' not found.");

        await using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Unable to load resource '{resourceName}'.");

        using StreamReader reader = new(stream);

        return await reader.ReadToEndAsync();
    }

    private static async Task SeedCoreUserAsync(NpgsqlConnection connection, string login, string password)
    {
        const string existsSql = "SELECT COUNT(*) FROM users WHERE login = @Login";

        int exists = await connection.ExecuteScalarAsync<int>(
            existsSql,
            new { Login = login });

        if (exists > 0)
            return;

        const string insertSql = "INSERT INTO users (login, password_hash, name, created_at) " +
                                 "VALUES (@Login, @PasswordHash, @Name, @CreatedAt)";

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        await connection.ExecuteAsync(insertSql, new
        {
            Login = login,
            PasswordHash = passwordHash,
            Name = login,
            CreatedAt = DateTime.UtcNow
        });
    }
}