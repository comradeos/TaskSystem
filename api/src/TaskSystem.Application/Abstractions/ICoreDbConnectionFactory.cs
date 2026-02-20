using System.Data;

namespace TaskSystem.Application.Abstractions;

public interface ICoreDbConnectionFactory
{
    IDbConnection CreateConnection();
}