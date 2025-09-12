using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论仓储接口 - 遵循开闭原则和接口隔离原则
/// </summary>
public interface ICommentRepository
{
    // 基础 CRUD 操作
    Task<Comment?> GetByIdAsync(Guid id);
    Task<Comment?> GetByIdWithUserAsync(Guid id);
    Task<Comment?> GetByIdWithRepliesAsync(Guid id);
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment> UpdateAsync(Comment comment);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SoftDeleteAsync(Guid id);
    
    // 分页查询
    Task<PaginatedResult<Comment>> GetPagedAsync(CommentFilterDto filter);
    Task<PaginatedResult<Comment>> GetBySnippetIdAsync(Guid snippetId, CommentFilterDto filter);
    Task<PaginatedResult<Comment>> GetByUserIdAsync(Guid userId, CommentFilterDto filter);
    
    // 树形结构查询
    Task<IEnumerable<Comment>> GetRootCommentsBySnippetIdAsync(Guid snippetId);
    Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(Guid parentId);
    Task<Comment?> GetParentChainAsync(Guid commentId);
    
    // 搜索和筛选
    Task<IEnumerable<Comment>> SearchCommentsAsync(string searchTerm, CommentFilterDto filter);
    Task<IEnumerable<Comment>> GetCommentsByStatusAsync(CommentStatus status);
    Task<IEnumerable<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // 统计信息
    Task<int> GetCommentCountBySnippetIdAsync(Guid snippetId);
    Task<int> GetCommentCountByUserIdAsync(Guid userId);
    Task<CommentStatsDto> GetCommentStatsBySnippetIdAsync(Guid snippetId);
    Task<IEnumerable<CommentStatsDto>> GetCommentStatsByUserIdsAsync(IEnumerable<Guid> userIds);
    
    // 批量操作
    Task<bool> UpdateCommentStatusAsync(IEnumerable<Guid> commentIds, CommentStatus status);
    Task<bool> IncrementLikeCountAsync(Guid commentId);
    Task<bool> DecrementLikeCountAsync(Guid commentId);
    Task<bool> IncrementReplyCountAsync(Guid commentId);
    Task<bool> DecrementReplyCountAsync(Guid commentId);
    
    // 权限和验证
    Task<bool> CanUserEditCommentAsync(Guid commentId, Guid userId);
    Task<bool> CanUserDeleteCommentAsync(Guid commentId, Guid userId);
    Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId);
    
    // 高级查询
    Task<IEnumerable<Comment>> GetLatestCommentsBySnippetIdAsync(Guid snippetId, int count = 5);
    Task<IEnumerable<Comment>> GetMostLikedCommentsBySnippetIdAsync(Guid snippetId, int count = 5);
    Task<IEnumerable<Comment>> GetCommentsWithPendingStatusAsync();
    
    // 缓存相关
    Task<Comment?> GetFromCacheAsync(Guid id);
    Task<bool> SetCacheAsync(Comment comment, TimeSpan? expiration = null);
    Task<bool> RemoveCacheAsync(Guid id);
    
    // 事务相关
    Task<int> BulkInsertAsync(IEnumerable<Comment> comments);
    Task<int> BulkUpdateAsync(IEnumerable<Comment> comments);
    Task<int> BulkDeleteAsync(IEnumerable<Guid> commentIds);
}