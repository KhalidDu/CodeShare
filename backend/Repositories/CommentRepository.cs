using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 评论仓储实现 - 使用 Dapper ORM 进行数据访问
/// 遵循单一职责原则和依赖注入模式
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CommentRepository> _logger;

    public CommentRepository(IDbConnectionFactory connectionFactory, ILogger<CommentRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础 CRUD 操作

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE Id = @Id AND DeletedAt IS NULL";

            return await connection.QuerySingleOrDefaultAsync<Comment>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论失败，评论ID: {CommentId}", id);
            throw;
        }
    }

    public async Task<Comment?> GetByIdWithUserAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT c.*, u.Id, u.Username, u.Email, u.Role, u.CreatedAt, u.UpdatedAt, u.IsActive
                FROM Comments c
                LEFT JOIN Users u ON c.UserId = u.Id
                WHERE c.Id = @Id AND c.DeletedAt IS NULL";

            var commentDict = new Dictionary<Guid, Comment>();
            
            var result = await connection.QueryAsync<Comment, User, Comment>(
                sql,
                (comment, user) =>
                {
                    if (!commentDict.TryGetValue(comment.Id, out var existingComment))
                    {
                        existingComment = comment;
                        existingComment.User = user;
                        commentDict[comment.Id] = existingComment;
                    }
                    return existingComment;
                },
                new { Id = id }
            );

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论和用户信息失败，评论ID: {CommentId}", id);
            throw;
        }
    }

    public async Task<Comment?> GetByIdWithRepliesAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                WITH RECURSIVE CommentTree AS (
                    SELECT c.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                    FROM Comments c
                    LEFT JOIN Users u ON c.UserId = u.Id
                    WHERE c.Id = @Id AND c.DeletedAt IS NULL
                    
                    UNION ALL
                    
                    SELECT c.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                    FROM Comments c
                    LEFT JOIN Users u ON c.UserId = u.Id
                    INNER JOIN CommentTree ct ON c.ParentId = ct.Id
                    WHERE c.DeletedAt IS NULL
                )
                SELECT * FROM CommentTree ORDER BY Depth, CreatedAt";

            var commentDict = new Dictionary<Guid, Comment>();
            
            var result = await connection.QueryAsync<Comment, User, Comment>(
                sql,
                (comment, user) =>
                {
                    if (!commentDict.TryGetValue(comment.Id, out var existingComment))
                    {
                        comment.User = user;
                        commentDict[comment.Id] = comment;
                        existingComment = comment;
                    }
                    return existingComment;
                },
                new { Id = id }
            );

            // 构建回复树结构
            var allComments = result.ToList();
            var rootComment = allComments.FirstOrDefault(c => c.Id == id);
            
            if (rootComment != null)
            {
                BuildCommentTree(rootComment, allComments);
            }

            return rootComment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论和回复失败，评论ID: {CommentId}", id);
            throw;
        }
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        try
        {
            comment.Id = Guid.NewGuid();
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;
            comment.LikeCount = 0;
            comment.ReplyCount = 0;

            // 如果是回复，需要设置 ParentPath 和 Depth
            if (comment.ParentId.HasValue)
            {
                var parentComment = await GetByIdAsync(comment.ParentId.Value);
                if (parentComment != null)
                {
                    comment.ParentPath = string.IsNullOrEmpty(parentComment.ParentPath) 
                        ? parentComment.Id.ToString() 
                        : $"{parentComment.ParentPath}/{parentComment.Id}";
                    comment.Depth = parentComment.Depth + 1;
                    
                    // 更新父评论的回复计数
                    await IncrementReplyCountAsync(parentComment.Id);
                }
            }
            else
            {
                comment.Depth = 0;
            }

            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Comments (
                    Id, Content, SnippetId, UserId, ParentId, ParentPath, Depth, 
                    LikeCount, ReplyCount, Status, CreatedAt, UpdatedAt, DeletedAt
                ) VALUES (
                    @Id, @Content, @SnippetId, @UserId, @ParentId, @ParentPath, @Depth,
                    @LikeCount, @ReplyCount, @Status, @CreatedAt, @UpdatedAt, @DeletedAt
                )";

            await connection.ExecuteAsync(sql, comment);
            _logger.LogInformation("创建评论成功，评论ID: {CommentId}", comment.Id);
            
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论失败");
            throw;
        }
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        try
        {
            comment.UpdatedAt = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    Content = @Content,
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND DeletedAt IS NULL";

            await connection.ExecuteAsync(sql, new
            {
                comment.Id,
                comment.Content,
                comment.Status,
                comment.UpdatedAt
            });

            _logger.LogInformation("更新评论成功，评论ID: {CommentId}", comment.Id);
            return comment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新评论失败，评论ID: {CommentId}", comment.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Comments WHERE Id = @Id";

            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            
            if (affected > 0)
            {
                _logger.LogInformation("删除评论成功，评论ID: {CommentId}", id);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论失败，评论ID: {CommentId}", id);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    Status = @Status,
                    DeletedAt = @DeletedAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = CommentStatus.Deleted,
                DeletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            if (affected > 0)
            {
                _logger.LogInformation("软删除评论成功，评论ID: {CommentId}", id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "软删除评论失败，评论ID: {CommentId}", id);
            throw;
        }
    }

    #endregion

    #region 分页查询

    public async Task<PaginatedResult<Comment>> GetPagedAsync(CommentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var (whereClause, parameters) = BuildWhereClause(filter);
            var orderByClause = BuildOrderByClause(filter.SortBy);
            
            var countSql = $"SELECT COUNT(*) FROM Comments c WHERE DeletedAt IS NULL {whereClause}";
            var total = await connection.QuerySingleAsync<int>(countSql, parameters);

            var offset = (filter.Page - 1) * filter.PageSize;
            var dataSql = $@"
                SELECT c.* FROM Comments c
                WHERE DeletedAt IS NULL {whereClause}
                ORDER BY {orderByClause}
                LIMIT @PageSize OFFSET @Offset";

            parameters.PageSize = filter.PageSize;
            parameters.Offset = offset;

            var comments = await connection.QueryAsync<Comment>(dataSql, parameters);

            return new PaginatedResult<Comment>
            {
                Items = comments,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取评论失败");
            throw;
        }
    }

    public async Task<PaginatedResult<Comment>> GetBySnippetIdAsync(Guid snippetId, CommentFilterDto filter)
    {
        try
        {
            filter.SnippetId = snippetId;
            return await GetPagedAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据代码片段ID获取评论失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<PaginatedResult<Comment>> GetByUserIdAsync(Guid userId, CommentFilterDto filter)
    {
        try
        {
            filter.UserId = userId;
            return await GetPagedAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据用户ID获取评论失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 树形结构查询

    public async Task<IEnumerable<Comment>> GetRootCommentsBySnippetIdAsync(Guid snippetId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE SnippetId = @SnippetId AND ParentId IS NULL AND DeletedAt IS NULL
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Comment>(sql, new { SnippetId = snippetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取根评论失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(Guid parentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE ParentId = @ParentId AND DeletedAt IS NULL
                ORDER BY CreatedAt ASC";

            return await connection.QueryAsync<Comment>(sql, new { ParentId = parentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取回复评论失败，父评论ID: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<Comment?> GetParentChainAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                WITH RECURSIVE ParentChain AS (
                    SELECT * FROM Comments WHERE Id = @CommentId
                    UNION ALL
                    SELECT c.* FROM Comments c
                    INNER JOIN ParentChain pc ON c.Id = pc.ParentId
                )
                SELECT * FROM ParentChain ORDER BY Depth ASC";

            var comments = await connection.QueryAsync<Comment>(sql, new { CommentId = commentId });
            return comments.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取父评论链失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region 搜索和筛选

    public async Task<IEnumerable<Comment>> SearchCommentsAsync(string searchTerm, CommentFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var (whereClause, parameters) = BuildWhereClause(filter);
            parameters.SearchTerm = $"%{searchTerm}%";
            
            var sql = $@"
                SELECT * FROM Comments 
                WHERE DeletedAt IS NULL AND (Content LIKE @SearchTerm) {whereClause}
                ORDER BY CreatedAt DESC
                LIMIT 100";

            return await connection.QueryAsync<Comment>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索评论失败，搜索词: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsByStatusAsync(CommentStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE Status = @Status AND DeletedAt IS NULL
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Comment>(sql, new { Status = (int)status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据状态获取评论失败，状态: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE CreatedAt BETWEEN @StartDate AND @EndDate 
                AND DeletedAt IS NULL
                ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Comment>(sql, new { StartDate = startDate, EndDate = endDate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据日期范围获取评论失败，开始时间: {StartDate}, 结束时间: {EndDate}", startDate, endDate);
            throw;
        }
    }

    #endregion

    #region 统计信息

    public async Task<int> GetCommentCountBySnippetIdAsync(Guid snippetId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Comments 
                WHERE SnippetId = @SnippetId AND DeletedAt IS NULL";

            return await connection.QuerySingleAsync<int>(sql, new { SnippetId = snippetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段评论数量失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<int> GetCommentCountByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Comments 
                WHERE UserId = @UserId AND DeletedAt IS NULL";

            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论数量失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<CommentStatsDto> GetCommentStatsBySnippetIdAsync(Guid snippetId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalComments,
                    SUM(CASE WHEN ParentId IS NULL THEN 1 ELSE 0 END) as RootComments,
                    SUM(CASE WHEN ParentId IS NOT NULL THEN 1 ELSE 0 END) as ReplyComments,
                    SUM(LikeCount) as TotalLikes,
                    COUNT(DISTINCT UserId) as ActiveUsers,
                    MAX(CreatedAt) as LatestCommentAt
                FROM Comments 
                WHERE SnippetId = @SnippetId AND DeletedAt IS NULL";

            return await connection.QuerySingleAsync<CommentStatsDto>(sql, new { SnippetId = snippetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论统计信息失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<IEnumerable<CommentStatsDto>> GetCommentStatsByUserIdsAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    u.Id as SnippetId,
                    COUNT(c.Id) as TotalComments,
                    SUM(CASE WHEN c.ParentId IS NULL THEN 1 ELSE 0 END) as RootComments,
                    SUM(CASE WHEN c.ParentId IS NOT NULL THEN 1 ELSE 0 END) as ReplyComments,
                    SUM(c.LikeCount) as TotalLikes,
                    COUNT(DISTINCT c.SnippetId) as ActiveUsers,
                    MAX(c.CreatedAt) as LatestCommentAt
                FROM Users u
                LEFT JOIN Comments c ON u.Id = c.UserId AND c.DeletedAt IS NULL
                WHERE u.Id IN @UserIds
                GROUP BY u.Id";

            return await connection.QueryAsync<CommentStatsDto>(sql, new { UserIds = userIds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论统计失败");
            throw;
        }
    }

    #endregion

    #region 批量操作

    public async Task<bool> UpdateCommentStatusAsync(IEnumerable<Guid> commentIds, CommentStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE Id IN @CommentIds AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                CommentIds = commentIds,
                Status = (int)status,
                UpdatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("批量更新评论状态成功，影响行数: {AffectedRows}", affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新评论状态失败");
            throw;
        }
    }

    public async Task<bool> IncrementLikeCountAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    LikeCount = LikeCount + 1,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @CommentId AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                CommentId = commentId,
                UpdatedAt = DateTime.UtcNow
            });

            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "增加评论点赞数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> DecrementLikeCountAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    LikeCount = CASE WHEN LikeCount > 0 THEN LikeCount - 1 ELSE 0 END,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @CommentId AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                CommentId = commentId,
                UpdatedAt = DateTime.UtcNow
            });

            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "减少评论点赞数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> IncrementReplyCountAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    ReplyCount = ReplyCount + 1,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @CommentId AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                CommentId = commentId,
                UpdatedAt = DateTime.UtcNow
            });

            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "增加评论回复数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> DecrementReplyCountAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    ReplyCount = CASE WHEN ReplyCount > 0 THEN ReplyCount - 1 ELSE 0 END,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @CommentId AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                CommentId = commentId,
                UpdatedAt = DateTime.UtcNow
            });

            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "减少评论回复数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region 权限和验证

    public async Task<bool> CanUserEditCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Comments 
                WHERE Id = @CommentId AND UserId = @UserId AND DeletedAt IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户编辑权限失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    public async Task<bool> CanUserDeleteCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Comments 
                WHERE Id = @CommentId AND UserId = @UserId AND DeletedAt IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户删除权限失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    public async Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentReports 
                WHERE CommentId = @CommentId AND UserId = @UserId";

            var count = await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户举报状态失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    #endregion

    #region 高级查询

    public async Task<IEnumerable<Comment>> GetLatestCommentsBySnippetIdAsync(Guid snippetId, int count = 5)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE SnippetId = @SnippetId AND DeletedAt IS NULL
                ORDER BY CreatedAt DESC
                LIMIT @Count";

            return await connection.QueryAsync<Comment>(sql, new { SnippetId = snippetId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新评论失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetMostLikedCommentsBySnippetIdAsync(Guid snippetId, int count = 5)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE SnippetId = @SnippetId AND DeletedAt IS NULL
                ORDER BY LikeCount DESC, CreatedAt DESC
                LIMIT @Count";

            return await connection.QueryAsync<Comment>(sql, new { SnippetId = snippetId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最受欢迎评论失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsWithPendingStatusAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Comments 
                WHERE Status = @Status AND DeletedAt IS NULL
                ORDER BY CreatedAt ASC";

            return await connection.QueryAsync<Comment>(sql, new { Status = (int)CommentStatus.Pending });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待审核评论失败");
            throw;
        }
    }

    #endregion

    #region 缓存相关

    public async Task<Comment?> GetFromCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务，目前直接从数据库获取
        return await GetByIdAsync(id);
    }

    public async Task<bool> SetCacheAsync(Comment comment, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool> RemoveCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务
        return true;
    }

    #endregion

    #region 事务相关

    public async Task<int> BulkInsertAsync(IEnumerable<Comment> comments)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Comments (
                    Id, Content, SnippetId, UserId, ParentId, ParentPath, Depth, 
                    LikeCount, ReplyCount, Status, CreatedAt, UpdatedAt, DeletedAt
                ) VALUES (
                    @Id, @Content, @SnippetId, @UserId, @ParentId, @ParentPath, @Depth,
                    @LikeCount, @ReplyCount, @Status, @CreatedAt, @UpdatedAt, @DeletedAt
                )";

            var commentList = comments.ToList();
            foreach (var comment in commentList)
            {
                comment.Id = Guid.NewGuid();
                comment.CreatedAt = DateTime.UtcNow;
                comment.UpdatedAt = DateTime.UtcNow;
            }

            var affected = await connection.ExecuteAsync(sql, commentList);
            _logger.LogInformation("批量插入评论成功，插入数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入评论失败");
            throw;
        }
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<Comment> comments)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Comments SET
                    Content = @Content,
                    Status = @Status,
                    LikeCount = @LikeCount,
                    ReplyCount = @ReplyCount,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND DeletedAt IS NULL";

            var commentList = comments.ToList();
            foreach (var comment in commentList)
            {
                comment.UpdatedAt = DateTime.UtcNow;
            }

            var affected = await connection.ExecuteAsync(sql, commentList);
            _logger.LogInformation("批量更新评论成功，更新数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新评论失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Comments WHERE Id IN @CommentIds";

            var affected = await connection.ExecuteAsync(sql, new { CommentIds = commentIds });
            _logger.LogInformation("批量删除评论成功，删除数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除评论失败");
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    private (string WhereClause, DynamicParameters Parameters) BuildWhereClause(CommentFilterDto filter)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (filter.SnippetId.HasValue)
        {
            conditions.Add("c.SnippetId = @SnippetId");
            parameters.Add("SnippetId", filter.SnippetId.Value);
        }

        if (filter.UserId.HasValue)
        {
            conditions.Add("c.UserId = @UserId");
            parameters.Add("UserId", filter.UserId.Value);
        }

        if (filter.Status.HasValue)
        {
            conditions.Add("c.Status = @Status");
            parameters.Add("Status", (int)filter.Status.Value);
        }

        if (filter.ParentId.HasValue)
        {
            conditions.Add("c.ParentId = @ParentId");
            parameters.Add("ParentId", filter.ParentId.Value);
        }
        else if (filter.ParentId == null)
        {
            conditions.Add("c.ParentId IS NULL");
        }

        if (filter.StartDate.HasValue)
        {
            conditions.Add("c.CreatedAt >= @StartDate");
            parameters.Add("StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            conditions.Add("c.CreatedAt <= @EndDate");
            parameters.Add("EndDate", filter.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            conditions.Add("c.Content LIKE @Search");
            parameters.Add("Search", $"%{filter.Search}%");
        }

        var whereClause = conditions.Count > 0 ? "AND " + string.Join(" AND ", conditions) : "";
        return (whereClause, parameters);
    }

    private string BuildOrderByClause(CommentSort sortBy)
    {
        return sortBy switch
        {
            CommentSort.CreatedAtAsc => "c.CreatedAt ASC",
            CommentSort.CreatedAtDesc => "c.CreatedAt DESC",
            CommentSort.LikeCountAsc => "c.LikeCount ASC, c.CreatedAt DESC",
            CommentSort.LikeCountDesc => "c.LikeCount DESC, c.CreatedAt DESC",
            CommentSort.ReplyCountAsc => "c.ReplyCount ASC, c.CreatedAt DESC",
            CommentSort.ReplyCountDesc => "c.ReplyCount DESC, c.CreatedAt DESC",
            _ => "c.CreatedAt DESC"
        };
    }

    private void BuildCommentTree(Comment root, IEnumerable<Comment> allComments)
    {
        var replies = allComments.Where(c => c.ParentId == root.Id).ToList();
        
        foreach (var reply in replies)
        {
            root.Replies.Add(reply);
            BuildCommentTree(reply, allComments);
        }
    }

    #endregion
}