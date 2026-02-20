using System.Data;
using Npgsql;
using TaskSystem.Application.Abstractions;

namespace TaskSystem.Infrastructure.Postgres;

public class PostgresConnectionFactory(string connectionString) : ICoreDbConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}