using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 评论点赞仓储实现 - 使用 Dapper ORM 进行数据访问
/// 遵循里氏替换原则和依赖注入模式
/// </summary>
public class CommentLikeRepository : ICommentLikeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CommentLikeRepository> _logger;

    public CommentLikeRepository(IDbConnectionFactory connectionFactory, ILogger<CommentLikeRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础 CRUD 操作

    public async Task<CommentLike?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE cl.Id = @Id";

            var result = await connection.QueryAsync<CommentLike, User, Comment, CommentLike>(
                sql,
                (like, user, comment) =>
                {
                    like.User = user;
                    like.Comment = comment;
                    return like;
                },
                new { Id = id }
            );

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞失败，点赞ID: {LikeId}", id);
            throw;
        }
    }

    public async Task<CommentLike?> GetByUserAndCommentAsync(Guid userId, Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE cl.UserId = @UserId AND cl.CommentId = @CommentId";

            var result = await connection.QueryAsync<CommentLike, User, Comment, CommentLike>(
                sql,
                (like, user, comment) =>
                {
                    like.User = user;
                    like.Comment = comment;
                    return like;
                },
                new { UserId = userId, CommentId = commentId }
            );

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论点赞失败，用户ID: {UserId}, 评论ID: {CommentId}", userId, commentId);
            throw;
        }
    }

    public async Task<CommentLike> CreateAsync(CommentLike commentLike)
    {
        try
        {
            commentLike.Id = Guid.NewGuid();
            commentLike.CreatedAt = DateTime.UtcNow;

            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO CommentLikes (Id, CommentId, UserId, CreatedAt)
                VALUES (@Id, @CommentId, @UserId, @CreatedAt)";

            await connection.ExecuteAsync(sql, commentLike);
            
            _logger.LogInformation("创建评论点赞成功，点赞ID: {LikeId}, 评论ID: {CommentId}, 用户ID: {UserId}", 
                commentLike.Id, commentLike.CommentId, commentLike.UserId);
            
            return commentLike;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论点赞失败");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE Id = @Id";

            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            
            if (affected > 0)
            {
                _logger.LogInformation("删除评论点赞成功，点赞ID: {LikeId}", id);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论点赞失败，点赞ID: {LikeId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteByUserAndCommentAsync(Guid userId, Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE UserId = @UserId AND CommentId = @CommentId";

            var affected = await connection.ExecuteAsync(sql, new { UserId = userId, CommentId = commentId });
            
            if (affected > 0)
            {
                _logger.LogInformation("删除用户评论点赞成功，用户ID: {UserId}, 评论ID: {CommentId}", userId, commentId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户评论点赞失败，用户ID: {UserId}, 评论ID: {CommentId}", userId, commentId);
            throw;
        }
    }

    #endregion

    #region 查询操作

    public async Task<IEnumerable<CommentLike>> GetByCommentIdAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                WHERE cl.CommentId = @CommentId
                ORDER BY cl.CreatedAt DESC";

            return await connection.QueryAsync<CommentLike, User, CommentLike>(
                sql,
                (like, user) =>
                {
                    like.User = user;
                    return like;
                },
                new { CommentId = commentId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞列表失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<IEnumerable<CommentLike>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentLikes cl
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE cl.UserId = @UserId
                ORDER BY cl.CreatedAt DESC";

            return await connection.QueryAsync<CommentLike, Comment, CommentLike>(
                sql,
                (like, comment) =>
                {
                    like.Comment = comment;
                    return like;
                },
                new { UserId = userId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户点赞列表失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<PaginatedResult<CommentLike>> GetPagedByCommentIdAsync(Guid commentId, int page = 1, int pageSize = 20)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var countSql = "SELECT COUNT(*) FROM CommentLikes WHERE CommentId = @CommentId";
            var total = await connection.QuerySingleAsync<int>(countSql, new { CommentId = commentId });

            var offset = (page - 1) * pageSize;
            var dataSql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                WHERE cl.CommentId = @CommentId
                ORDER BY cl.CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var likes = await connection.QueryAsync<CommentLike, User, CommentLike>(
                dataSql,
                (like, user) =>
                {
                    like.User = user;
                    return like;
                },
                new { CommentId = commentId, PageSize = pageSize, Offset = offset }
            );

            return new PaginatedResult<CommentLike>
            {
                Items = likes,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取评论点赞失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<PaginatedResult<CommentLike>> GetPagedByUserIdAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var countSql = "SELECT COUNT(*) FROM CommentLikes WHERE UserId = @UserId";
            var total = await connection.QuerySingleAsync<int>(countSql, new { UserId = userId });

            var offset = (page - 1) * pageSize;
            var dataSql = @"
                SELECT cl.*, c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentLikes cl
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE cl.UserId = @UserId
                ORDER BY cl.CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var likes = await connection.QueryAsync<CommentLike, Comment, CommentLike>(
                dataSql,
                (like, comment) =>
                {
                    like.Comment = comment;
                    return like;
                },
                new { UserId = userId, PageSize = pageSize, Offset = offset }
            );

            return new PaginatedResult<CommentLike>
            {
                Items = likes,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取用户点赞失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 点赞状态查询

    public async Task<bool> IsLikedByUserAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentLikes 
                WHERE CommentId = @CommentId AND UserId = @UserId";

            var count = await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查点赞状态失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    public async Task<int> GetLikeCountByCommentIdAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentLikes WHERE CommentId = @CommentId";

            return await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论点赞数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<int> GetLikeCountByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentLikes WHERE UserId = @UserId";

            return await connection.QuerySingleAsync<int>(sql, new { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户点赞数失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<DateTime?> GetFirstLikeTimeAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT MIN(CreatedAt) FROM CommentLikes 
                WHERE CommentId = @CommentId AND UserId = @UserId";

            return await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new { CommentId = commentId, UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取首次点赞时间失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    #endregion

    #region 批量查询

    public async Task<IEnumerable<CommentLike>> GetLikesByCommentIdsAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                WHERE cl.CommentId IN @CommentIds
                ORDER BY cl.CreatedAt DESC";

            return await connection.QueryAsync<CommentLike, User, CommentLike>(
                sql,
                (like, user) =>
                {
                    like.User = user;
                    return like;
                },
                new { CommentIds = commentIds }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取评论点赞失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentLike>> GetLikesByUserIdsAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentLikes cl
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE cl.UserId IN @UserIds
                ORDER BY cl.CreatedAt DESC";

            return await connection.QueryAsync<CommentLike, Comment, CommentLike>(
                sql,
                (like, comment) =>
                {
                    like.Comment = comment;
                    return like;
                },
                new { UserIds = userIds }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取用户点赞失败");
            throw;
        }
    }

    public async Task<Dictionary<Guid, int>> GetLikeCountsByCommentIdsAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT CommentId, COUNT(*) as LikeCount
                FROM CommentLikes
                WHERE CommentId IN @CommentIds
                GROUP BY CommentId";

            var results = await connection.QueryAsync<(Guid CommentId, int LikeCount)>(sql, new { CommentIds = commentIds });
            return results.ToDictionary(x => x.CommentId, x => x.LikeCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取点赞数失败");
            throw;
        }
    }

    public async Task<Dictionary<Guid, bool>> GetLikeStatusByCommentIdsAsync(Guid userId, IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT CommentId, 
                       CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END as IsLiked
                FROM CommentLikes
                WHERE UserId = @UserId AND CommentId IN @CommentIds
                GROUP BY CommentId";

            var results = await connection.QueryAsync<(Guid CommentId, bool IsLiked)>(sql, new { UserId = userId, CommentIds = commentIds });
            
            var dict = new Dictionary<Guid, bool>();
            foreach (var commentId in commentIds)
            {
                dict[commentId] = false;
            }
            
            foreach (var result in results)
            {
                dict[result.CommentId] = result.IsLiked;
            }
            
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取点赞状态失败");
            throw;
        }
    }

    #endregion

    #region 高级查询

    public async Task<IEnumerable<CommentLike>> GetLatestLikesByCommentIdAsync(Guid commentId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                WHERE cl.CommentId = @CommentId
                ORDER BY cl.CreatedAt DESC
                LIMIT @Count";

            return await connection.QueryAsync<CommentLike, User, CommentLike>(
                sql,
                (like, user) =>
                {
                    like.User = user;
                    return like;
                },
                new { CommentId = commentId, Count = count }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新点赞失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<IEnumerable<CommentLike>> GetLikesByDateRangeAsync(Guid commentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.*, u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive
                FROM CommentLikes cl
                LEFT JOIN Users u ON cl.UserId = u.Id
                WHERE cl.CommentId = @CommentId AND cl.CreatedAt BETWEEN @StartDate AND @EndDate
                ORDER BY cl.CreatedAt DESC";

            return await connection.QueryAsync<CommentLike, User, CommentLike>(
                sql,
                (like, user) =>
                {
                    like.User = user;
                    return like;
                },
                new { CommentId = commentId, StartDate = startDate, EndDate = endDate }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日期范围点赞失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<IEnumerable<(Guid CommentId, int LikeCount)>> GetTopLikedCommentsAsync(int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT CommentId, COUNT(*) as LikeCount
                FROM CommentLikes
                GROUP BY CommentId
                ORDER BY LikeCount DESC
                LIMIT @Count";

            return await connection.QueryAsync<(Guid CommentId, int LikeCount)>(sql, new { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取热门评论失败");
            throw;
        }
    }

    #endregion

    #region 统计信息

    public async Task<int> GetTotalLikesBySnippetIdAsync(Guid snippetId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentLikes cl
                INNER JOIN Comments c ON cl.CommentId = c.Id
                WHERE c.SnippetId = @SnippetId";

            return await connection.QuerySingleAsync<int>(sql, new { SnippetId = snippetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段总点赞数失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<int> GetDailyLikesCountAsync(DateTime date)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentLikes 
                WHERE DATE(CreatedAt) = DATE(@Date)";

            return await connection.QuerySingleAsync<int>(sql, new { Date = date });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取每日点赞数失败，日期: {Date}", date);
            throw;
        }
    }

    public async Task<int> GetMonthlyLikesCountAsync(int year, int month)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentLikes 
                WHERE YEAR(CreatedAt) = @Year AND MONTH(CreatedAt) = @Month";

            return await connection.QuerySingleAsync<int>(sql, new { Year = year, Month = month });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取每月点赞数失败，年份: {Year}, 月份: {Month}", year, month);
            throw;
        }
    }

    public async Task<Dictionary<DateTime, int>> GetLikesCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT DATE(CreatedAt) as Date, COUNT(*) as LikeCount
                FROM CommentLikes
                WHERE CreatedAt BETWEEN @StartDate AND @EndDate
                GROUP BY DATE(CreatedAt)
                ORDER BY Date";

            var results = await connection.QueryAsync<(DateTime Date, int LikeCount)>(sql, new { StartDate = startDate, EndDate = endDate });
            return results.ToDictionary(x => x.Date, x => x.LikeCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日期范围点赞统计失败");
            throw;
        }
    }

    #endregion

    #region 缓存相关

    public async Task<IEnumerable<CommentLike>?> GetLikesFromCacheAsync(Guid commentId)
    {
        // 这里可以集成缓存服务，目前直接从数据库获取
        return await GetByCommentIdAsync(commentId);
    }

    public async Task<bool> SetLikesCacheAsync(Guid commentId, IEnumerable<CommentLike> likes, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool> RemoveLikesCacheAsync(Guid commentId)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool> SetLikeStatusCacheAsync(Guid commentId, Guid userId, bool isLiked, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool?> GetLikeStatusFromCacheAsync(Guid commentId, Guid userId)
    {
        // 这里可以集成缓存服务，目前直接从数据库获取
        return await IsLikedByUserAsync(commentId, userId);
    }

    #endregion

    #region 事务相关

    public async Task<int> BulkInsertAsync(IEnumerable<CommentLike> likes)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO CommentLikes (Id, CommentId, UserId, CreatedAt)
                VALUES (@Id, @CommentId, @UserId, @CreatedAt)";

            var likeList = likes.ToList();
            foreach (var like in likeList)
            {
                like.Id = Guid.NewGuid();
                like.CreatedAt = DateTime.UtcNow;
            }

            var affected = await connection.ExecuteAsync(sql, likeList);
            _logger.LogInformation("批量插入点赞成功，插入数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入点赞失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteByCommentIdsAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE CommentId IN @CommentIds";

            var affected = await connection.ExecuteAsync(sql, new { CommentIds = commentIds });
            _logger.LogInformation("批量删除评论点赞成功，删除数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除评论点赞失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteByUserIdsAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE UserId IN @UserIds";

            var affected = await connection.ExecuteAsync(sql, new { UserIds = userIds });
            _logger.LogInformation("批量删除用户点赞成功，删除数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除用户点赞失败");
            throw;
        }
    }

    public async Task<bool> DeleteAllLikesByCommentIdAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE CommentId = @CommentId";

            var affected = await connection.ExecuteAsync(sql, new { CommentId = commentId });
            
            _logger.LogInformation("删除评论所有点赞成功，评论ID: {CommentId}, 删除数量: {Count}", commentId, affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论所有点赞失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> DeleteAllLikesByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE UserId = @UserId";

            var affected = await connection.ExecuteAsync(sql, new { UserId = userId });
            
            _logger.LogInformation("删除用户所有点赞成功，用户ID: {UserId}, 删除数量: {Count}", userId, affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除用户所有点赞失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 清理和维护

    public async Task<int> CleanOrphanedLikesAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                DELETE cl FROM CommentLikes cl
                LEFT JOIN Comments c ON cl.CommentId = c.Id
                WHERE c.Id IS NULL";

            var affected = await connection.ExecuteAsync(sql);
            _logger.LogInformation("清理孤立点赞记录成功，清理数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理孤立点赞记录失败");
            throw;
        }
    }

    public async Task<int> CleanOldLikesAsync(DateTime olderThan)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentLikes WHERE CreatedAt < @OlderThan";

            var affected = await connection.ExecuteAsync(sql, new { OlderThan = olderThan });
            _logger.LogInformation("清理旧点赞记录成功，清理数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理旧点赞记录失败");
            throw;
        }
    }

    public async Task<bool> ValidateLikeIntegrityAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 检查点赞数是否正确
            const string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM CommentLikes WHERE CommentId = @CommentId) as LikeCount,
                    (SELECT LikeCount FROM Comments WHERE Id = @CommentId) as CommentLikeCount";

            var result = await connection.QuerySingleAsync<(int LikeCount, int CommentLikeCount)>(sql, new { CommentId = commentId });
            
            return result.LikeCount == result.CommentLikeCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证点赞完整性失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region 分析和报告

    public async Task<IEnumerable<(Guid UserId, int LikeCount)>> GetTopUsersByLikesAsync(int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT UserId, COUNT(*) as LikeCount
                FROM CommentLikes
                GROUP BY UserId
                ORDER BY LikeCount DESC
                LIMIT @Count";

            return await connection.QueryAsync<(Guid UserId, int LikeCount)>(sql, new { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取热门点赞用户失败");
            throw;
        }
    }

    public async Task<IEnumerable<(Guid CommentId, int LikeCount)>> GetMostLikedCommentsBySnippetIdAsync(Guid snippetId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cl.CommentId, COUNT(*) as LikeCount
                FROM CommentLikes cl
                INNER JOIN Comments c ON cl.CommentId = c.Id
                WHERE c.SnippetId = @SnippetId
                GROUP BY cl.CommentId
                ORDER BY LikeCount DESC
                LIMIT @Count";

            return await connection.QueryAsync<(Guid CommentId, int LikeCount)>(sql, new { SnippetId = snippetId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段热门评论失败，代码片段ID: {SnippetId}", snippetId);
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetLikesDistributionByHourAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT HOUR(CreatedAt) as Hour, COUNT(*) as LikeCount
                FROM CommentLikes
                WHERE CommentId = @CommentId
                GROUP BY HOUR(CreatedAt)
                ORDER BY Hour";

            var results = await connection.QueryAsync<(int Hour, int LikeCount)>(sql, new { CommentId = commentId });
            
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < 24; i++)
            {
                dict[$"{i:00}:00"] = 0;
            }
            
            foreach (var result in results)
            {
                dict[$"{result.Hour:00}:00"] = result.LikeCount;
            }
            
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取点赞时间分布失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region 实时数据

    public async Task<int> GetRealtimeLikeCountAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentLikes WHERE CommentId = @CommentId";

            return await connection.QuerySingleAsync<int>(sql, new { CommentId = commentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取实时点赞数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> LikeExistsAsync(Guid commentId, Guid userId)
    {
        try
        {
            return await IsLikedByUserAsync(commentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查点赞是否存在失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    public async Task<DateTime?> GetLastLikeTimeAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT MAX(CreatedAt) FROM CommentLikes 
                WHERE CommentId = @CommentId";

            return await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new { CommentId = commentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最后点赞时间失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion
}