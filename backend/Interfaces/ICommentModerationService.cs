using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论审核服务接口 - 遵循单一职责原则
/// 提供评论审核相关的业务逻辑操作
/// </summary>
public interface ICommentModerationService
{
    // 评论审核操作
    #region 评论审核操作

    /// <summary>
    /// 审核评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="approved">是否通过审核</param>
    /// <param name="reason">审核原因</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>审核结果</returns>
    Task<ModerationResultDto> ModerateCommentAsync(Guid commentId, bool approved, string? reason, Guid moderatorId);

    /// <summary>
    /// 批量审核评论
    /// </summary>
    /// <param name="commentIds">评论ID列表</param>
    /// <param name="approved">是否通过审核</param>
    /// <param name="reason">审核原因</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>批量审核结果</returns>
    Task<BulkModerationResultDto> BulkModerateCommentsAsync(IEnumerable<Guid> commentIds, bool approved, string? reason, Guid moderatorId);

    /// <summary>
    /// 自动审核评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>自动审核结果</returns>
    Task<AutoModerationResultDto> AutoModerateCommentAsync(Guid commentId);

    /// <summary>
    /// 撤销审核结果
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="reason">撤销原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否撤销成功</returns>
    Task<bool> RevokeModerationAsync(Guid commentId, string reason, Guid moderatorId);

    #endregion

    // 评论状态管理
    #region 评论状态管理

    /// <summary>
    /// 设置评论状态
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="status">评论状态</param>
    /// <param name="reason">状态变更原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetCommentStatusAsync(Guid commentId, CommentStatus status, string? reason, Guid moderatorId);

    /// <summary>
    /// 批量设置评论状态
    /// </summary>
    /// <param name="commentIds">评论ID列表</param>
    /// <param name="status">评论状态</param>
    /// <param name="reason">状态变更原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否批量设置成功</returns>
    Task<bool> BulkSetCommentStatusAsync(IEnumerable<Guid> commentIds, CommentStatus status, string? reason, Guid moderatorId);

    /// <summary>
    /// 隐藏评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="reason">隐藏原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否隐藏成功</returns>
    Task<bool> HideCommentAsync(Guid commentId, string? reason, Guid moderatorId);

    /// <summary>
    /// 显示评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="reason">显示原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否显示成功</returns>
    Task<bool> ShowCommentAsync(Guid commentId, string? reason, Guid moderatorId);

    /// <summary>
    /// 标记评论为待审核
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="reason">标记原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkCommentAsPendingAsync(Guid commentId, string? reason, Guid moderatorId);

    #endregion

    // 审核队列管理
    #region 审核队列管理

    /// <summary>
    /// 获取待审核评论队列
    /// </summary>
    /// <param name="filter">队列筛选条件</param>
    /// <returns>待审核评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetModerationQueueAsync(ModerationQueueFilterDto filter);

    /// <summary>
    /// 获取高优先级审核队列
    /// </summary>
    /// <param name="count">获取数量</param>
    /// <returns>高优先级评论列表</returns>
    Task<IEnumerable<CommentDto>> GetHighPriorityModerationQueueAsync(int count = 10);

    /// <summary>
    /// 获取用户待审核评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>用户待审核评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetUserPendingCommentsAsync(Guid userId, ModerationQueueFilterDto filter);

    /// <summary>
    /// 分配审核任务
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>是否分配成功</returns>
    Task<bool> AssignModerationTaskAsync(Guid commentId, Guid moderatorId);

    /// <summary>
    /// 批量分配审核任务
    /// </summary>
    /// <param name="commentIds">评论ID列表</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>是否批量分配成功</returns>
    Task<bool> BulkAssignModerationTasksAsync(IEnumerable<Guid> commentIds, Guid moderatorId);

    #endregion

    // 审核规则管理
    #region 审核规则管理

    /// <summary>
    /// 创建审核规则
    /// </summary>
    /// <param name="rule">审核规则</param>
    /// <returns>创建的规则</returns>
    Task<ModerationRuleDto> CreateModerationRuleAsync(CreateModerationRuleDto rule);

    /// <summary>
    /// 更新审核规则
    /// </summary>
    /// <param name="ruleId">规则ID</param>
    /// <param name="rule">更新规则</param>
    /// <returns>更新后的规则</returns>
    Task<ModerationRuleDto> UpdateModerationRuleAsync(Guid ruleId, UpdateModerationRuleDto rule);

    /// <summary>
    /// 删除审核规则
    /// </summary>
    /// <param name="ruleId">规则ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteModerationRuleAsync(Guid ruleId);

    /// <summary>
    /// 获取审核规则列表
    /// </summary>
    /// <param name="filter">规则筛选条件</param>
    /// <returns>审核规则列表</returns>
    Task<PaginatedResult<ModerationRuleDto>> GetModerationRulesAsync(ModerationRuleFilterDto filter);

    /// <summary>
    /// 应用审核规则
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>规则应用结果</returns>
    Task<RuleApplicationResultDto> ApplyModerationRulesAsync(Guid commentId);

    #endregion

    // 审核统计和报告
    #region 审核统计和报告

    /// <summary>
    /// 获取审核统计信息
    /// </summary>
    /// <param name="moderatorId">审核人ID（可选）</param>
    /// <returns>审核统计信息</returns>
    Task<ModerationStatsDto> GetModerationStatsAsync(Guid? moderatorId = null);

    /// <summary>
    /// 获取审核人员统计
    /// </summary>
    /// <returns>审核人员统计信息</returns>
    Task<IEnumerable<ModeratorStatsDto>> GetModeratorStatsAsync();

    /// <summary>
    /// 获取审核趋势统计
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns>审核趋势统计</returns>
    Task<IEnumerable<ModerationTrendDto>> GetModerationTrendAsync(int days = 30);

    /// <summary>
    /// 获取审核效率报告
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>审核效率报告</returns>
    Task<ModerationEfficiencyReportDto> GetModerationEfficiencyReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取违规类型统计
    /// </summary>
    /// <returns>违规类型统计</returns>
    Task<ViolationTypeStatsDto> GetViolationTypeStatsAsync();

    #endregion

    // 自动化审核
    #region 自动化审核

    /// <summary>
    /// 配置自动审核设置
    /// </summary>
    /// <param name="settings">自动审核设置</param>
    /// <returns>配置结果</returns>
    Task<bool> ConfigureAutoModerationAsync(AutoModerationSettingsDto settings);

    /// <summary>
    /// 获取自动审核设置
    /// </summary>
    /// <returns>自动审核设置</returns>
    Task<AutoModerationSettingsDto> GetAutoModerationSettingsAsync();

    /// <summary>
    /// 运行自动审核扫描
    /// </summary>
    /// <param name="filter">扫描筛选条件</param>
    /// <returns>扫描结果</returns>
    Task<AutoModerationScanResultDto> RunAutoModerationScanAsync(AutoModerationScanFilterDto filter);

    /// <summary>
    /// 批量自动审核
    /// </summary>
    /// <param name="commentIds">评论ID列表</param>
    /// <returns>批量自动审核结果</returns>
    Task<BulkAutoModerationResultDto> BulkAutoModerateAsync(IEnumerable<Guid> commentIds);

    #endregion

    // 审核历史
    #region 审核历史

    /// <summary>
    /// 获取评论审核历史
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>审核历史记录</returns>
    Task<IEnumerable<CommentModerationHistoryDto>> GetCommentModerationHistoryAsync(Guid commentId);

    /// <summary>
    /// 获取审核人员审核历史
    /// </summary>
    /// <param name="moderatorId">审核人ID</param>
    /// <param name="filter">历史筛选条件</param>
    /// <returns>审核历史记录</returns>
    Task<PaginatedResult<CommentModerationHistoryDto>> GetModeratorHistoryAsync(Guid moderatorId, ModerationHistoryFilterDto filter);

    /// <summary>
    /// 撤销审核操作
    /// </summary>
    /// <param name="historyId">历史记录ID</param>
    /// <param name="reason">撤销原因</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <returns>是否撤销成功</returns>
    Task<bool> RevertModerationActionAsync(Guid historyId, string reason, Guid moderatorId);

    #endregion
}

/// <summary>
/// 审核结果DTO
/// </summary>
public class ModerationResultDto
{
    public Guid CommentId { get; set; }
    public bool Success { get; set; }
    public bool Approved { get; set; }
    public string? Reason { get; set; }
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public DateTime ModeratedAt { get; set; }
    public CommentStatus NewStatus { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 批量审核结果DTO
/// </summary>
public class BulkModerationResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<Guid> SuccessfulCommentIds { get; set; } = new();
    public List<ModerationFailureDto> Failures { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// 审核失败详情DTO
/// </summary>
public class ModerationFailureDto
{
    public Guid CommentId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 自动审核结果DTO
/// </summary>
public class AutoModerationResultDto
{
    public Guid CommentId { get; set; }
    public bool IsProcessed { get; set; }
    public bool AutoApproved { get; set; }
    public bool FlaggedForReview { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
    public List<AppliedRuleDto> AppliedRules { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// 应用规则DTO
/// </summary>
public class AppliedRuleDto
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public bool Triggered { get; set; }
    public double Confidence { get; set; }
    public string? MatchedPattern { get; set; }
}

/// <summary>
/// 审核队列筛选条件DTO
/// </summary>
public class ModerationQueueFilterDto
{
    public Guid? AssignedModeratorId { get; set; }
    public CommentStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }
    public ModerationQueueSort SortBy { get; set; } = ModerationQueueSort.PriorityDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 审核队列排序枚举
/// </summary>
public enum ModerationQueueSort
{
    PriorityDesc = 0,
    PriorityAsc = 1,
    CreatedAtDesc = 2,
    CreatedAtAsc = 3,
    ReportCountDesc = 4,
    ReportCountAsc = 5
}

/// <summary>
/// 创建审核规则DTO
/// </summary>
public class CreateModerationRuleDto
{
    [Required(ErrorMessage = "规则名称不能为空")]
    [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "规则类型不能为空")]
    public ModerationRuleType Type { get; set; }

    [Required(ErrorMessage = "规则条件不能为空")]
    public string Condition { get; set; } = string.Empty;

    [Required(ErrorMessage = "规则动作不能为空")]
    public ModerationAction Action { get; set; }

    [Range(0, 1, ErrorMessage = "置信度必须在0到1之间")]
    public double ConfidenceThreshold { get; set; } = 0.8;

    [StringLength(500, ErrorMessage = "规则描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 更新审核规则DTO
/// </summary>
public class UpdateModerationRuleDto
{
    [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
    public string? Name { get; set; }

    public ModerationAction? Action { get; set; }

    [Range(0, 1, ErrorMessage = "置信度必须在0到1之间")]
    public double? ConfidenceThreshold { get; set; }

    [StringLength(500, ErrorMessage = "规则描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// 审核规则DTO
/// </summary>
public class ModerationRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ModerationRuleType Type { get; set; }
    public string Condition { get; set; } = string.Empty;
    public ModerationAction Action { get; set; }
    public double ConfidenceThreshold { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UsageCount { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// 审核规则类型枚举
/// </summary>
public enum ModerationRuleType
{
    Keyword = 0,
    Pattern = 1,
    Spam = 2,
    Sensitive = 3,
    Length = 4,
    Frequency = 5,
    Reputation = 6,
    Custom = 99
}

/// <summary>
/// 审核动作枚举
/// </summary>
public enum ModerationAction
{
    Approve = 0,
    Reject = 1,
    Flag = 2,
    Hide = 3,
    Delete = 4,
    RequireReview = 5
}

/// <summary>
/// 规则筛选条件DTO
/// </summary>
public class ModerationRuleFilterDto
{
    public ModerationRuleType? Type { get; set; }
    public ModerationAction? Action { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public ModerationRuleSort SortBy { get; set; } = ModerationRuleSort.UpdatedDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 审核规则排序枚举
/// </summary>
public enum ModerationRuleSort
{
    CreatedDesc = 0,
    CreatedAsc = 1,
    UpdatedDesc = 2,
    UpdatedAsc = 3,
    NameAsc = 4,
    NameDesc = 5,
    UsageDesc = 6,
    UsageAsc = 7
}

/// <summary>
/// 规则应用结果DTO
/// </summary>
public class RuleApplicationResultDto
{
    public Guid CommentId { get; set; }
    public List<AppliedRuleDto> AppliedRules { get; set; } = new();
    public ModerationAction RecommendedAction { get; set; }
    public double OverallConfidence { get; set; }
    public bool RequiresHumanReview { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 审核统计信息DTO
/// </summary>
public class ModerationStatsDto
{
    public int TotalProcessed { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }
    public int FlaggedCount { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
    public DateTime LastProcessedAt { get; set; }
    public int ModeratorsActive { get; set; }
    public int RulesActive { get; set; }
}

/// <summary>
/// 审核人员统计DTO
/// </summary>
public class ModeratorStatsDto
{
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int TotalProcessed { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int FlaggedCount { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
    public DateTime LastActivityAt { get; set; }
    public int DaysActive { get; set; }
    public double PerformanceScore { get; set; }
}

/// <summary>
/// 审核趋势统计DTO
/// </summary>
public class ModerationTrendDto
{
    public DateTime Date { get; set; }
    public int TotalProcessed { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }
    public int FlaggedCount { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
}

/// <summary>
/// 审核效率报告DTO
/// </summary>
public class ModerationEfficiencyReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalProcessed { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
    public double ThroughputPerHour { get; set; }
    public int ModeratorsCount { get; set; }
    public double EfficiencyScore { get; set; }
    public List<ModeratorPerformanceDto> ModeratorPerformance { get; set; } = new();
    public List<HourlyProcessingStatsDto> HourlyStats { get; set; } = new();
}

/// <summary>
/// 审核人员表现DTO
/// </summary>
public class ModeratorPerformanceDto
{
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
    public double ApprovalRate { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
    public double PerformanceScore { get; set; }
    public int WorkingHours { get; set; }
}

/// <summary>
/// 每小时处理统计DTO
/// </summary>
public class HourlyProcessingStatsDto
{
    public int Hour { get; set; }
    public int ProcessedCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public double AverageProcessingTimeMinutes { get; set; }
}

/// <summary>
/// 违规类型统计DTO
/// </summary>
public class ViolationTypeStatsDto
{
    public Dictionary<string, int> ViolationCounts { get; set; } = new();
    public Dictionary<string, double> ViolationRates { get; set; } = new();
    public int TotalViolations { get; set; }
    public string MostCommonViolation { get; set; } = string.Empty;
    public double MostCommonViolationRate { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 自动审核设置DTO
/// </summary>
public class AutoModerationSettingsDto
{
    public bool IsEnabled { get; set; }
    public double ApprovalThreshold { get; set; }
    public double RejectionThreshold { get; set; }
    public int ScanIntervalMinutes { get; set; }
    public bool AutoApproveTrustedUsers { get; set; }
    public bool AutoFlagSuspiciousContent { get; set; }
    public List<string> EnabledRuleTypes { get; set; } = new();
    public int MaxBatchSize { get; set; }
    public DateTime LastConfiguredAt { get; set; }
}

/// <summary>
/// 自动审核扫描筛选条件DTO
/// </summary>
public class AutoModerationScanFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? SnippetId { get; set; }
    public Guid? UserId { get; set; }
    public CommentStatus? Status { get; set; }
    public int MaxCommentsToScan { get; set; } = 1000;
    public bool IncludeReplies { get; set; } = true;
}

/// <summary>
/// 自动审核扫描结果DTO
/// </summary>
public class AutoModerationScanResultDto
{
    public DateTime ScanStartedAt { get; set; }
    public DateTime ScanCompletedAt { get; set; }
    public int TotalScanned { get; set; }
    public int AutoApproved { get; set; }
    public int AutoRejected { get; set; }
    public int FlaggedForReview { get; set; }
    public int NoAction { get; set; }
    public double AverageConfidenceScore { get; set; }
    public List<AutoModerationIssueDto> IssuesFound { get; set; } = new();
    public List<AppliedRuleDto> MostTriggeredRules { get; set; } = new();
}

/// <summary>
/// 自动审核问题DTO
/// </summary>
public class AutoModerationIssueDto
{
    public Guid CommentId { get; set; }
    public string IssueType { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> Details { get; set; } = new();
    public ModerationAction RecommendedAction { get; set; }
}

/// <summary>
/// 批量自动审核结果DTO
/// </summary>
public class BulkAutoModerationResultDto
{
    public int TotalProcessed { get; set; }
    public int AutoApproved { get; set; }
    public int AutoRejected { get; set; }
    public int FlaggedForReview { get; set; }
    public int NoAction { get; set; }
    public int Errors { get; set; }
    public List<BulkAutoModerationItemDto> Results { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// 批量自动审核项目DTO
/// </summary>
public class BulkAutoModerationItemDto
{
    public Guid CommentId { get; set; }
    public bool Success { get; set; }
    public ModerationAction ActionTaken { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> AppliedRules { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 评论审核历史DTO
/// </summary>
public class CommentModerationHistoryDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public CommentStatus OldStatus { get; set; }
    public CommentStatus NewStatus { get; set; }
    public ModerationAction Action { get; set; }
    public string? Reason { get; set; }
    public bool IsAutoModerated { get; set; }
    public DateTime ActionAt { get; set; }
    public List<AppliedRuleDto> AppliedRules { get; set; } = new();
}

/// <summary>
/// 审核历史筛选条件DTO
/// </summary>
public class ModerationHistoryFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CommentStatus? Status { get; set; }
    public ModerationAction? Action { get; set; }
    public bool? IsAutoModerated { get; set; }
    public string? Search { get; set; }
    public ModerationHistorySort SortBy { get; set; } = ModerationHistorySort.ActionDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 审核历史排序枚举
/// </summary>
public enum ModerationHistorySort
{
    ActionDesc = 0,
    ActionAsc = 1,
    CommentDesc = 2,
    CommentAsc = 3,
    ModeratorAsc = 4,
    ModeratorDesc = 5
}