using Npgsql;
using BCrypt.Net;

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__PostgresCore");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string not found.");
    return;
}

Console.WriteLine("Starting migration...");

await using var connection = new NpgsqlConnection(connectionString);

try
{
    await connection.OpenAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Cannot connect to PostgreSQL.");
    Console.WriteLine("Ensure database container is running.");
    Console.WriteLine(ex.Message);
    return;
}

await using (var versionTableCmd = new NpgsqlCommand(
    @"CREATE TABLE IF NOT EXISTS schema_migrations (
        id VARCHAR(200) PRIMARY KEY,
        applied_at TIMESTAMP NOT NULL DEFAULT NOW()
      )",
    connection))
{
    await versionTableCmd.ExecuteNonQueryAsync();
}

var scriptsDir = Path.Combine(AppContext.BaseDirectory, "Scripts");

if (!Directory.Exists(scriptsDir))
{
    Console.WriteLine("Scripts directory not found.");
    return;
}

var files = Directory.GetFiles(scriptsDir, "*.sql")
    .OrderBy(f => f)
    .ToList();

foreach (var file in files)
{
    var scriptName = Path.GetFileName(file);

    await using var checkVersionCmd = new NpgsqlCommand(
        "SELECT COUNT(*) FROM schema_migrations WHERE id = @id",
        connection);

    checkVersionCmd.Parameters.AddWithValue("id", scriptName);

    var alreadyApplied =
        Convert.ToInt64(await checkVersionCmd.ExecuteScalarAsync()) > 0;

    if (alreadyApplied)
    {
        Console.WriteLine($"{scriptName} already applied.");
        continue;
    }

    Console.WriteLine($"Applying {scriptName}...");

    var sql = await File.ReadAllTextAsync(file);

    await using (var cmd = new NpgsqlCommand(sql, connection))
    {
        await cmd.ExecuteNonQueryAsync();
    }

    await using var insertVersionCmd = new NpgsqlCommand(
        "INSERT INTO schema_migrations (id) VALUES (@id)",
        connection);

    insertVersionCmd.Parameters.AddWithValue("id", scriptName);
    await insertVersionCmd.ExecuteNonQueryAsync();

    Console.WriteLine($"{scriptName} applied.");
}

Console.WriteLine("Schema migration completed.");

var adminLogin = Environment.GetEnvironmentVariable("CORE_DATA_USER");
var adminPassword = Environment.GetEnvironmentVariable("CORE_DATA_PASSWORD");

if (string.IsNullOrWhiteSpace(adminLogin) ||
    string.IsNullOrWhiteSpace(adminPassword))
{
    Console.WriteLine("Admin credentials not provided.");
    return;
}

var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

await using var checkCmd = new NpgsqlCommand(
    "SELECT COUNT(*) FROM users WHERE login = @login",
    connection);

checkCmd.Parameters.AddWithValue("login", adminLogin);

var exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;

if (!exists)
{
    await using var insertCmd = new NpgsqlCommand(
        @"INSERT INTO users (name, login, password_hash, is_admin)
          VALUES (@name, @login, @password_hash, TRUE)",
        connection);

    insertCmd.Parameters.AddWithValue("name", "Administrator");
    insertCmd.Parameters.AddWithValue("login", adminLogin);
    insertCmd.Parameters.AddWithValue("password_hash", passwordHash);

    await insertCmd.ExecuteNonQueryAsync();

    Console.WriteLine("Admin user created.");
}
else
{
    Console.WriteLine("Admin already exists.");
}

Console.WriteLine("Migration completed.");