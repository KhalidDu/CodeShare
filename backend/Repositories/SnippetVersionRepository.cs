using System.Data;
using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 代码片段版本仓储实现 - 遵循开闭原则
/// </summary>
public class SnippetVersionRepository : ISnippetVersionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SnippetVersionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// 根据ID获取版本
    /// </summary>
    public async Task<SnippetVersion?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, SnippetId, VersionNumber, Title, Description, Code, Language, 
                   CreatedBy, CreatedAt, ChangeDescription
            FROM SnippetVersions 
            WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<SnippetVersion>(sql, new { Id = id });
    }

    /// <summary>
    /// 获取代码片段的所有版本
    /// </summary>
    public async Task<IEnumerable<SnippetVersion>> GetBySnippetIdAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, SnippetId, VersionNumber, Title, Description, Code, Language, 
                   CreatedBy, CreatedAt, ChangeDescription
            FROM SnippetVersions 
            WHERE SnippetId = @SnippetId
            ORDER BY VersionNumber DESC";

        return await connection.QueryAsync<SnippetVersion>(sql, new { SnippetId = snippetId });
    }

    /// <summary>
    /// 获取代码片段的最新版本
    /// </summary>
    public async Task<SnippetVersion?> GetLatestVersionAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, SnippetId, VersionNumber, Title, Description, Code, Language, 
                   CreatedBy, CreatedAt, ChangeDescription
            FROM SnippetVersions 
            WHERE SnippetId = @SnippetId
            ORDER BY VersionNumber DESC
            LIMIT 1";

        return await connection.QuerySingleOrDefaultAsync<SnippetVersion>(sql, new { SnippetId = snippetId });
    }

    /// <summary>
    /// 创建新版本
    /// </summary>
    public async Task<SnippetVersion> CreateAsync(SnippetVersion version)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO SnippetVersions (Id, SnippetId, VersionNumber, Title, Description, Code, Language, 
                                       CreatedBy, CreatedAt, ChangeDescription)
            VALUES (@Id, @SnippetId, @VersionNumber, @Title, @Description, @Code, @Language, 
                    @CreatedBy, @CreatedAt, @ChangeDescription)";

        await connection.ExecuteAsync(sql, version);
        return version;
    }

    /// <summary>
    /// 获取下一个版本号
    /// </summary>
    public async Task<int> GetNextVersionNumberAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COALESCE(MAX(VersionNumber), 0) + 1
            FROM SnippetVersions 
            WHERE SnippetId = @SnippetId";

        return await connection.QuerySingleAsync<int>(sql, new { SnippetId = snippetId });
    }

    /// <summary>
    /// 删除代码片段的所有版本
    /// </summary>
    public async Task<bool> DeleteBySnippetIdAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SnippetVersions WHERE SnippetId = @SnippetId";

        var affectedRows = await connection.ExecuteAsync(sql, new { SnippetId = snippetId });
        return affectedRows > 0;
    }
}