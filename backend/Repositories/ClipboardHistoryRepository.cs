using System.Data;
using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 剪贴板历史仓储实现 - 遵循迪米特法则
/// </summary>
public class ClipboardHistoryRepository : IClipboardHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClipboardHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// 根据ID获取剪贴板历史记录
    /// </summary>
    public async Task<ClipboardHistory?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT Id, UserId, SnippetId, CopiedAt
            FROM ClipboardHistory 
            WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<ClipboardHistory>(sql, new { Id = id });
    }

    /// <summary>
    /// 获取用户的剪贴板历史
    /// </summary>
    public async Task<IEnumerable<ClipboardHistory>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ch.Id, ch.UserId, ch.SnippetId, ch.CopiedAt
            FROM ClipboardHistory ch
            WHERE ch.UserId = @UserId
            ORDER BY ch.CopiedAt DESC
            LIMIT @Limit";

        return await connection.QueryAsync<ClipboardHistory>(sql, new { UserId = userId, Limit = limit });
    }

    /// <summary>
    /// 创建剪贴板历史记录
    /// </summary>
    public async Task<ClipboardHistory> CreateAsync(ClipboardHistory history)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ClipboardHistory (Id, UserId, SnippetId, CopiedAt)
            VALUES (@Id, @UserId, @SnippetId, @CopiedAt)";

        await connection.ExecuteAsync(sql, history);
        return history;
    }

    /// <summary>
    /// 删除用户的所有剪贴板历史
    /// </summary>
    public async Task<bool> DeleteByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ClipboardHistory WHERE UserId = @UserId";

        var affectedRows = await connection.ExecuteAsync(sql, new { UserId = userId });
        return affectedRows > 0;
    }

    /// <summary>
    /// 删除过期的剪贴板历史记录
    /// </summary>
    public async Task<int> DeleteExpiredAsync(DateTime cutoffDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ClipboardHistory WHERE CopiedAt < @CutoffDate";

        return await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
    }

    /// <summary>
    /// 获取代码片段的复制统计
    /// </summary>
    public async Task<int> GetCopyCountAsync(Guid snippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(*) 
            FROM ClipboardHistory 
            WHERE SnippetId = @SnippetId";

        return await connection.QuerySingleAsync<int>(sql, new { SnippetId = snippetId });
    }
}