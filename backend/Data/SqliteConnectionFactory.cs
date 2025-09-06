using System.Data;
using Microsoft.Data.Sqlite;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Data;

/// <summary>
/// SQLite 连接工厂实现 - 遵循依赖倒置原则
/// </summary>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}