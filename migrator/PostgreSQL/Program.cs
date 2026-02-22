using Npgsql;

// мігратор на супер скору руку якій не предетендує на звання продакшн мігратора )

string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresCore");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("connection string not found");
    
    return;
}

Console.WriteLine("starting migration...");

await using var connection = new NpgsqlConnection(connectionString);

try
{
    await connection.OpenAsync();
}
catch (Exception ex)
{
    Console.WriteLine("cannot connect to postgresql");
    
    Console.WriteLine("ensure database container is running");
    
    Console.WriteLine(ex.Message);
    
    return;
}

const string createMigrationsTableSql = "CREATE TABLE IF NOT EXISTS schema_migrations ( id VARCHAR(200) PRIMARY KEY, applied_at TIMESTAMP NOT NULL DEFAULT NOW() );";

const string selectMigrationExistsSql = "SELECT COUNT(*) FROM schema_migrations WHERE id = @id";

const string insertMigrationSql = "INSERT INTO schema_migrations (id) VALUES (@id)";

const string selectUserExistsSql = "SELECT COUNT(*) FROM users WHERE login = @login";

const string insertAdminSql = " INSERT INTO users (name, login, password_hash, is_admin) VALUES (@name, @login, @password_hash, TRUE);";

await using (NpgsqlCommand cmd = new(createMigrationsTableSql, connection))
{
    await cmd.ExecuteNonQueryAsync();
}

string scriptsDir = Path.Combine(AppContext.BaseDirectory, "Scripts");

if (!Directory.Exists(scriptsDir))
{
    Console.WriteLine("directory \"Scripts\" not found");
    
    return;
}

List<string> files = Directory
    .GetFiles(scriptsDir, "*.sql")
    .OrderBy(f => f)
    .ToList();

await using NpgsqlCommand checkMigrationCmd = new(selectMigrationExistsSql, connection);

checkMigrationCmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Varchar);

await using NpgsqlCommand insertMigrationCmd = new(insertMigrationSql, connection);

insertMigrationCmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Varchar);

foreach (string file in files)
{
    string scriptName = Path.GetFileName(file);

    checkMigrationCmd.Parameters["id"].Value = scriptName;
    
    bool alreadyApplied = Convert.ToInt64(await checkMigrationCmd.ExecuteScalarAsync()) > 0;

    if (alreadyApplied)
    {
        Console.WriteLine($"{scriptName} already applied.");
        
        continue;
    }

    Console.WriteLine($"Applying {scriptName}...");

    string sql = await File.ReadAllTextAsync(file);

    await using (NpgsqlCommand applyCmd = new(sql, connection))
    {
        await applyCmd.ExecuteNonQueryAsync();
    }

    insertMigrationCmd.Parameters["id"].Value = scriptName;
    
    await insertMigrationCmd.ExecuteNonQueryAsync();

    Console.WriteLine($"{scriptName} applied.");
}

Console.WriteLine("schema migration completed.");

string? adminLogin = Environment.GetEnvironmentVariable("CORE_DATA_USER");

string? adminPassword = Environment.GetEnvironmentVariable("CORE_DATA_PASSWORD");

if (string.IsNullOrWhiteSpace(adminLogin) ||
    string.IsNullOrWhiteSpace(adminPassword))
{
    Console.WriteLine("admin credentials not provided.");
    
    return;
}

string passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

await using NpgsqlCommand checkUserCmd = new NpgsqlCommand(selectUserExistsSql, connection);

checkUserCmd.Parameters.Add("login", NpgsqlTypes.NpgsqlDbType.Varchar).Value = adminLogin;

bool exists = Convert.ToInt64(await checkUserCmd.ExecuteScalarAsync()) > 0;

if (!exists)
{
    await using NpgsqlCommand insertAdminCmd = new NpgsqlCommand(insertAdminSql, connection);
    
    insertAdminCmd.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Administrator";
    
    insertAdminCmd.Parameters.Add("login", NpgsqlTypes.NpgsqlDbType.Varchar).Value = adminLogin;
    
    insertAdminCmd.Parameters.Add("password_hash", NpgsqlTypes.NpgsqlDbType.Varchar).Value = passwordHash;

    await insertAdminCmd.ExecuteNonQueryAsync();

    Console.WriteLine("admin user created");
}
else
{
    Console.WriteLine("admin already exists");
}

Console.WriteLine("migration completed");