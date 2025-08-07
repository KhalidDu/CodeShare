using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 标签仓储实现 - 使用 Dapper ORM，遵循里氏替换原则
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TagRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// 根据ID获取标签
    /// </summary>
    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<Tag>(sql, new { Id = id });
    }

    /// <summary>
    /// 根据名称获取标签
    /// </summary>
    public async Task<Tag?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags WHERE Name = @Name";
        
        return await connection.QuerySingleOrDefaultAsync<Tag>(sql, new { Name = name });
    }

    /// <summary>
    /// 获取所有标签
    /// </summary>
    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tags ORDER BY Name";
        
        return await connection.QueryAsync<Tag>(sql);
    }

    /// <summary>
    /// 创建新标签
    /// </summary>
    public async Task<Tag> CreateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Tags (Id, Name, Color, CreatedBy, CreatedAt)
            VALUES (@Id, @Name, @Color, @CreatedBy, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, tag);
        return tag;
    }

    /// <summary>
    /// 更新标签信息
    /// </summary>
    public async Task<Tag> UpdateAsync(Tag tag)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Tags 
            SET Name = @Name, Color = @Color
            WHERE Id = @Id";
        
        await connection.ExecuteAsync(sql, tag);
        return tag;
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Tags WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    /// <summary>
    /// 获取代码片段关联的标签
    /// </summary>
    public async Task<IEnumerable<Tag>> GetTagsBySnippetIdAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT t.* 
            FROM Tags t
            INNER JOIN SnippetTags st ON t.Id = st.TagId
            WHERE st.SnippetId = @SnippetId
            ORDER BY t.Name";
        
        return await connection.QueryAsync<Tag>(sql, new { SnippetId = snippetId });
    }

    /// <summary>
    /// 根据名称前缀搜索标签，用于自动补全功能
    /// 使用 Dapper 优化查询性能
    /// </summary>
    public async Task<IEnumerable<Tag>> SearchTagsByPrefixAsync(string prefix, int limit = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Tags 
            WHERE Name LIKE @Prefix 
            ORDER BY Name 
            LIMIT @Limit";
        
        return await connection.QueryAsync<Tag>(sql, new { 
            Prefix = $"{prefix}%", 
            Limit = limit 
        });
    }

    /// <summary>
    /// 获取标签使用统计信息
    /// 使用 Dapper Multi-mapping 优化查询
    /// </summary>
    public async Task<IEnumerable<TagUsageStatistic>> GetTagUsageStatisticsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                t.Id as TagId,
                t.Name as TagName,
                t.Color as TagColor,
                t.CreatedAt,
                COUNT(st.SnippetId) as UsageCount
            FROM Tags t
            LEFT JOIN SnippetTags st ON t.Id = st.TagId
            GROUP BY t.Id, t.Name, t.Color, t.CreatedAt
            ORDER BY UsageCount DESC, t.Name";
        
        return await connection.QueryAsync<TagUsageStatistic>(sql);
    }

    /// <summary>
    /// 获取最常用的标签
    /// 使用 Dapper 批量操作优化性能
    /// </summary>
    public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int limit = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT t.*
            FROM Tags t
            LEFT JOIN SnippetTags st ON t.Id = st.TagId
            GROUP BY t.Id, t.Name, t.Color, t.CreatedBy, t.CreatedAt
            ORDER BY COUNT(st.SnippetId) DESC, t.Name
            LIMIT @Limit";
        
        return await connection.QueryAsync<Tag>(sql, new { Limit = limit });
    }

    /// <summary>
    /// 检查标签是否被使用
    /// 减少类间耦合，遵循迪米特法则
    /// </summary>
    public async Task<bool> IsTagInUseAsync(Guid tagId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*) 
            FROM SnippetTags 
            WHERE TagId = @TagId";
        
        var count = await connection.QuerySingleAsync<int>(sql, new { TagId = tagId });
        return count > 0;
    }
}