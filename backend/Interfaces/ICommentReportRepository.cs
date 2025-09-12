using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论举报仓储接口 - 遵循单一职责原则
/// </summary>
public interface ICommentReportRepository
{
    // 基础 CRUD 操作
    Task<CommentReport?> GetByIdAsync(Guid id);
    Task<CommentReport> CreateAsync(CommentReport report);
    Task<CommentReport> UpdateAsync(CommentReport report);
    Task<bool> DeleteAsync(Guid id);
    
    // 查询操作
    Task<IEnumerable<CommentReport>> GetByCommentIdAsync(Guid commentId);
    Task<IEnumerable<CommentReport>> GetByUserIdAsync(Guid userId);
    Task<PaginatedResult<CommentReport>> GetPagedAsync(CommentReportFilter filter);
    Task<CommentReport?> GetByUserAndCommentAsync(Guid userId, Guid commentId);
    
    // 按状态查询
    Task<IEnumerable<CommentReport>> GetByStatusAsync(ReportStatus status);
    Task<IEnumerable<CommentReport>> GetPendingReportsAsync();
    Task<IEnumerable<CommentReport>> GetResolvedReportsAsync();
    Task<IEnumerable<CommentReport>> GetReportsUnderInvestigationAsync();
    
    // 按原因查询
    Task<IEnumerable<CommentReport>> GetByReasonAsync(ReportReason reason);
    Task<Dictionary<ReportReason, int>> GetReportCountByReasonAsync();
    Task<IEnumerable<(ReportReason Reason, int Count)>> GetTopReportReasonsAsync(int count = 5);
    
    // 按处理人查询
    Task<IEnumerable<CommentReport>> GetByHandledByAsync(Guid handledBy);
    Task<IEnumerable<CommentReport>> GetReportsByHandlerAsync(Guid handlerId, ReportStatus? status = null);
    
    // 重复举报检查
    Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId);
    Task<int> GetReportCountByCommentIdAsync(Guid commentId);
    Task<bool> IsCommentReportedMultipleTimesAsync(Guid commentId, int threshold = 3);
    
    // 批量查询
    Task<IEnumerable<CommentReport>> GetReportsByCommentIdsAsync(IEnumerable<Guid> commentIds);
    Task<IEnumerable<CommentReport>> GetReportsByUserIdsAsync(IEnumerable<Guid> userIds);
    Task<Dictionary<Guid, int>> GetReportCountsByCommentIdsAsync(IEnumerable<Guid> commentIds);
    
    // 统计信息
    Task<int> GetTotalReportsCountAsync();
    Task<int> GetReportsCountByStatusAsync(ReportStatus status);
    Task<int> GetReportsCountByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<CommentReportStatsDto> GetReportStatsAsync();
    Task<IEnumerable<CommentReportSummaryDto>> GetDailyReportStatsAsync(int days = 30);
    
    // 时间相关查询
    Task<IEnumerable<CommentReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<CommentReport>> GetRecentReportsAsync(int hours = 24);
    Task<DateTime?> GetOldestPendingReportTimeAsync();
    
    // 高级查询
    Task<IEnumerable<CommentReport>> GetHighPriorityReportsAsync();
    Task<IEnumerable<CommentReport>> GetReportsNeedingAttentionAsync();
    Task<IEnumerable<(Guid CommentId, int ReportCount)>> GetMostReportedCommentsAsync(int count = 10);
    
    // 处理相关
    Task<CommentReport?> MarkAsResolvedAsync(Guid reportId, Guid handledBy, string? resolution = null);
    Task<CommentReport?> MarkAsRejectedAsync(Guid reportId, Guid handledBy, string? resolution = null);
    Task<CommentReport?> AssignToHandlerAsync(Guid reportId, Guid handlerId);
    Task<CommentReport?> UpdateResolutionAsync(Guid reportId, string resolution);
    
    // 批量操作
    Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> reportIds, ReportStatus status);
    Task<int> BulkAssignToHandlerAsync(IEnumerable<Guid> reportIds, Guid handlerId);
    Task<int> BulkDeleteOldReportsAsync(DateTime olderThan);
    
    // 缓存相关
    Task<IEnumerable<CommentReport>?> GetReportsFromCacheAsync(Guid commentId);
    Task<bool> SetReportsCacheAsync(Guid commentId, IEnumerable<CommentReport> reports, TimeSpan? expiration = null);
    Task<bool> RemoveReportsCacheAsync(Guid commentId);
    Task<bool> SetReportCountCacheAsync(Guid commentId, int count, TimeSpan? expiration = null);
    Task<int?> GetReportCountFromCacheAsync(Guid commentId);
    
    // 清理和维护
    Task<int> CleanOldResolvedReportsAsync(DateTime olderThan);
    Task<int> CleanDuplicateReportsAsync();
    Task<bool> ValidateReportIntegrityAsync(Guid reportId);
    Task<int> AutoResolveOldReportsAsync(TimeSpan resolveAfter);
    
    // 分析和报告
    Task<IEnumerable<(DateTime Date, int Count)>> GetReportTrendAsync(int days = 30);
    Task<Dictionary<Guid, int>> GetUserReportStatisticsAsync();
    Task<IEnumerable<(Guid UserId, int ReportCount)>> GetTopReportingUsersAsync(int count = 10);
    Task<double> GetAverageResolutionTimeAsync();
    
    // 实时数据
    Task<int> GetRealtimePendingReportsCountAsync();
    Task<bool> HasUrgentReportsAsync();
    Task<IEnumerable<CommentReport>> GetUrgentReportsAsync();
    
    // 导出相关
    Task<IEnumerable<CommentReportExportDto>> GetReportsForExportAsync(CommentReportFilter filter);
    Task<byte[]> GenerateReportCsvAsync(CommentReportFilter filter);
    Task<byte[]> GenerateReportJsonAsync(CommentReportFilter filter);
}

/// <summary>
/// 评论举报统计信息DTO
/// </summary>
public class CommentReportStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int RejectedReports { get; set; }
    public int UnderInvestigationReports { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public int ReportsThisWeek { get; set; }
    public int ReportsThisMonth { get; set; }
    public DateTime LastReportTime { get; set; }
}

/// <summary>
/// 评论举报摘要DTO
/// </summary>
public class CommentReportSummaryDto
{
    public DateTime Date { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int RejectedReports { get; set; }
}

/// <summary>
/// 评论举报导出DTO
/// </summary>
public class CommentReportExportDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public string CommentContent { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReportReason Reason { get; set; }
    public string ReasonText => GetReasonText(Reason);
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public string StatusText => GetStatusText(Status);
    public DateTime CreatedAt { get; set; }
    public DateTime? HandledAt { get; set; }
    public string? HandlerName { get; set; }
    public string? Resolution { get; set; }
    public double? ResolutionTimeHours { get; set; }

    private static string GetReasonText(ReportReason reason) => reason switch
    {
        ReportReason.Spam => "垃圾信息",
        ReportReason.Inappropriate => "不当内容",
        ReportReason.Harassment => "侮辱性言论",
        ReportReason.HateSpeech => "仇恨言论",
        ReportReason.Misinformation => "虚假信息",
        ReportReason.CopyrightViolation => "侵权内容",
        ReportReason.Other => "其他原因",
        _ => "未知原因"
    };

    private static string GetStatusText(ReportStatus status) => status switch
    {
        ReportStatus.Pending => "待处理",
        ReportStatus.Resolved => "已处理",
        ReportStatus.Rejected => "已驳回",
        ReportStatus.UnderInvestigation => "正在调查",
        _ => "未知状态"
    };
}