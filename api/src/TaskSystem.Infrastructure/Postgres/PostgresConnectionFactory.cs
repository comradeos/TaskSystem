using System.Data;
using Npgsql;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresConnectionFactory : ICoreDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
}