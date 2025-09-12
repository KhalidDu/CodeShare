using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论点赞仓储接口 - 遵循里氏替换原则
/// </summary>
public interface ICommentLikeRepository
{
    // 基础 CRUD 操作
    Task<CommentLike?> GetByIdAsync(Guid id);
    Task<CommentLike?> GetByUserAndCommentAsync(Guid userId, Guid commentId);
    Task<CommentLike> CreateAsync(CommentLike commentLike);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteByUserAndCommentAsync(Guid userId, Guid commentId);
    
    // 查询操作
    Task<IEnumerable<CommentLike>> GetByCommentIdAsync(Guid commentId);
    Task<IEnumerable<CommentLike>> GetByUserIdAsync(Guid userId);
    Task<PaginatedResult<CommentLike>> GetPagedByCommentIdAsync(Guid commentId, int page = 1, int pageSize = 20);
    Task<PaginatedResult<CommentLike>> GetPagedByUserIdAsync(Guid userId, int page = 1, int pageSize = 20);
    
    // 点赞状态查询
    Task<bool> IsLikedByUserAsync(Guid commentId, Guid userId);
    Task<int> GetLikeCountByCommentIdAsync(Guid commentId);
    Task<int> GetLikeCountByUserIdAsync(Guid userId);
    Task<DateTime?> GetFirstLikeTimeAsync(Guid commentId, Guid userId);
    
    // 批量查询
    Task<IEnumerable<CommentLike>> GetLikesByCommentIdsAsync(IEnumerable<Guid> commentIds);
    Task<IEnumerable<CommentLike>> GetLikesByUserIdsAsync(IEnumerable<Guid> userIds);
    Task<Dictionary<Guid, int>> GetLikeCountsByCommentIdsAsync(IEnumerable<Guid> commentIds);
    Task<Dictionary<Guid, bool>> GetLikeStatusByCommentIdsAsync(Guid userId, IEnumerable<Guid> commentIds);
    
    // 高级查询
    Task<IEnumerable<CommentLike>> GetLatestLikesByCommentIdAsync(Guid commentId, int count = 10);
    Task<IEnumerable<CommentLike>> GetLikesByDateRangeAsync(Guid commentId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<(Guid CommentId, int LikeCount)>> GetTopLikedCommentsAsync(int count = 10);
    
    // 统计信息
    Task<int> GetTotalLikesBySnippetIdAsync(Guid snippetId);
    Task<int> GetDailyLikesCountAsync(DateTime date);
    Task<int> GetMonthlyLikesCountAsync(int year, int month);
    Task<Dictionary<DateTime, int>> GetLikesCountByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // 缓存相关
    Task<IEnumerable<CommentLike>?> GetLikesFromCacheAsync(Guid commentId);
    Task<bool> SetLikesCacheAsync(Guid commentId, IEnumerable<CommentLike> likes, TimeSpan? expiration = null);
    Task<bool> RemoveLikesCacheAsync(Guid commentId);
    Task<bool> SetLikeStatusCacheAsync(Guid commentId, Guid userId, bool isLiked, TimeSpan? expiration = null);
    Task<bool?> GetLikeStatusFromCacheAsync(Guid commentId, Guid userId);
    
    // 事务相关
    Task<int> BulkInsertAsync(IEnumerable<CommentLike> likes);
    Task<int> BulkDeleteByCommentIdsAsync(IEnumerable<Guid> commentIds);
    Task<int> BulkDeleteByUserIdsAsync(IEnumerable<Guid> userIds);
    Task<bool> DeleteAllLikesByCommentIdAsync(Guid commentId);
    Task<bool> DeleteAllLikesByUserIdAsync(Guid userId);
    
    // 清理和维护
    Task<int> CleanOrphanedLikesAsync();
    Task<int> CleanOldLikesAsync(DateTime olderThan);
    Task<bool> ValidateLikeIntegrityAsync(Guid commentId);
    
    // 分析和报告
    Task<IEnumerable<(Guid UserId, int LikeCount)>> GetTopUsersByLikesAsync(int count = 10);
    Task<IEnumerable<(Guid CommentId, int LikeCount)>> GetMostLikedCommentsBySnippetIdAsync(Guid snippetId, int count = 10);
    Task<Dictionary<string, int>> GetLikesDistributionByHourAsync(Guid commentId);
    
    // 实时数据
    Task<int> GetRealtimeLikeCountAsync(Guid commentId);
    Task<bool> LikeExistsAsync(Guid commentId, Guid userId);
    Task<DateTime?> GetLastLikeTimeAsync(Guid commentId);
}