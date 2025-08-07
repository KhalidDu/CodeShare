using System.Data;
using MySql.Data.MySqlClient;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Data;

/// <summary>
/// MySQL 连接工厂实现 - 遵循依赖倒置原则
/// </summary>
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}