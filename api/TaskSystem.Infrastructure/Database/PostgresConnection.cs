using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TaskSystem.Infrastructure.Database;

public class PostgresConnection
{
    private readonly string _connectionString;

    public PostgresConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgresCore")
            ?? throw new InvalidOperationException("Postgres connection string not configured");
    }

    public IDbConnection Create()
    {
        return new NpgsqlConnection(_connectionString);
    }
}