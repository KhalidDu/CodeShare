using System.Data;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 数据库连接工厂接口 - 遵循依赖倒置原则
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}