using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论举报管理服务接口 - 遵循单一职责原则
/// 提供评论举报相关的业务逻辑操作
/// </summary>
public interface ICommentReportManagementService
{
    // 举报创建和管理
    #region 举报创建和管理

    /// <summary>
    /// 创建评论举报
    /// </summary>
    /// <param name="createReportDto">创建举报请求</param>
    /// <param name="userId">举报人ID</param>
    /// <returns>举报ID</returns>
    Task<Guid> CreateReportAsync(CreateCommentReportDto createReportDto, Guid userId);

    /// <summary>
    /// 批量创建评论举报
    /// </summary>
    /// <param name="createReports">创建举报请求列表</param>
    /// <param name="userId">举报人ID</param>
    /// <returns>批量创建结果</returns>
    Task<BulkCreateReportResultDto> BulkCreateReportsAsync(IEnumerable<CreateCommentReportDto> createReports, Guid userId);

    /// <summary>
    /// 撤销评论举报
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="userId">举报人ID</param>
    /// <param name="reason">撤销原因</param>
    /// <returns>是否撤销成功</returns>
    Task<bool> WithdrawReportAsync(Guid reportId, Guid userId, string? reason);

    /// <summary>
    /// 更新举报信息
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="updateReportDto">更新举报请求</param>
    /// <param name="userId">操作人ID</param>
    /// <returns>更新后的举报信息</returns>
    Task<CommentReportDto> UpdateReportAsync(Guid reportId, UpdateCommentReportDto updateReportDto, Guid userId);

    /// <summary>
    /// 删除举报
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="userId">操作人ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteReportAsync(Guid reportId, Guid userId);

    #endregion

    // 举报查询和检索
    #region 举报查询和检索

    /// <summary>
    /// 获取举报详情
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <returns>举报详情</returns>
    Task<CommentReportDto?> GetReportAsync(Guid reportId);

    /// <summary>
    /// 分页获取举报列表
    /// </summary>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>举报列表</returns>
    Task<PaginatedResult<CommentReportDto>> GetReportsAsync(CommentReportFilterDto filter);

    /// <summary>
    /// 获取评论的举报列表
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>评论举报列表</returns>
    Task<PaginatedResult<CommentReportDto>> GetCommentReportsAsync(Guid commentId, CommentReportFilterDto filter);

    /// <summary>
    /// 获取用户的举报列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>用户举报列表</returns>
    Task<PaginatedResult<CommentReportDto>> GetUserReportsAsync(Guid userId, CommentReportFilterDto filter);

    /// <summary>
    /// 获取待处理举报列表
    /// </summary>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>待处理举报列表</returns>
    Task<PaginatedResult<CommentReportDto>> GetPendingReportsAsync(CommentReportFilterDto filter);

    /// <summary>
    /// 获取高优先级举报列表
    /// </summary>
    /// <param name="count">获取数量</param>
    /// <returns>高优先级举报列表</returns>
    Task<IEnumerable<CommentReportDto>> GetHighPriorityReportsAsync(int count = 10);

    /// <summary>
    /// 搜索举报
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>搜索结果</returns>
    Task<PaginatedResult<CommentReportDto>> SearchReportsAsync(string searchTerm, CommentReportFilterDto filter);

    #endregion

    // 举报处理和管理
    #region 举报处理和管理

    /// <summary>
    /// 处理举报
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="handleReportDto">处理举报请求</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>处理结果</returns>
    Task<ReportHandlingResultDto> HandleReportAsync(Guid reportId, HandleCommentReportDto handleReportDto, Guid handlerId);

    /// <summary>
    /// 批量处理举报
    /// </summary>
    /// <param name="reportIds">举报ID列表</param>
    /// <param name="status">处理状态</param>
    /// <param name="resolution">处理说明</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>批量处理结果</returns>
    Task<BulkReportHandlingResultDto> BulkHandleReportsAsync(IEnumerable<Guid> reportIds, ReportStatus status, string? resolution, Guid handlerId);

    /// <summary>
    /// 分配举报处理人
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>是否分配成功</returns>
    Task<bool> AssignReportHandlerAsync(Guid reportId, Guid handlerId);

    /// <summary>
    /// 批量分配举报处理人
    /// </summary>
    /// <param name="reportIds">举报ID列表</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>是否批量分配成功</returns>
    Task<bool> BulkAssignReportHandlersAsync(IEnumerable<Guid> reportIds, Guid handlerId);

    /// <summary>
    /// 升级举报优先级
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="reason">升级原因</param>
    /// <param name="upgradedById">升级人ID</param>
    /// <returns>是否升级成功</returns>
    Task<bool> EscalateReportAsync(Guid reportId, string reason, Guid upgradedById);

    /// <summary>
    /// 标记举报为紧急
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="reason">紧急原因</param>
    /// <param name="markedById">标记人ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkReportAsUrgentAsync(Guid reportId, string reason, Guid markedById);

    /// <summary>
    /// 暂停举报处理
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="reason">暂停原因</param>
    /// <param name="pausedById">暂停人ID</param>
    /// <returns>是否暂停成功</returns>
    Task<bool> PauseReportHandlingAsync(Guid reportId, string reason, Guid pausedById);

    /// <summary>
    /// 恢复举报处理
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="reason">恢复原因</param>
    /// <param name="resumedById">恢复人ID</param>
    /// <returns>是否恢复成功</returns>
    Task<bool> ResumeReportHandlingAsync(Guid reportId, string reason, Guid resumedById);

    #endregion

    // 举报规则和策略
    #region 举报规则和策略

    /// <summary>
    /// 创建举报规则
    /// </summary>
    /// <param name="rule">举报规则</param>
    /// <returns>创建的规则</returns>
    Task<ReportRuleDto> CreateReportRuleAsync(CreateReportRuleDto rule);

    /// <summary>
    /// 更新举报规则
    /// </summary>
    /// <param name="ruleId">规则ID</param>
    /// <param name="rule">更新规则</param>
    /// <returns>更新后的规则</returns>
    Task<ReportRuleDto> UpdateReportRuleAsync(Guid ruleId, UpdateReportRuleDto rule);

    /// <summary>
    /// 删除举报规则
    /// </summary>
    /// <param name="ruleId">规则ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteReportRuleAsync(Guid ruleId);

    /// <summary>
    /// 获取举报规则列表
    /// </summary>
    /// <param name="filter">规则筛选条件</param>
    /// <returns>举报规则列表</returns>
    Task<PaginatedResult<ReportRuleDto>> GetReportRulesAsync(ReportRuleFilterDto filter);

    /// <summary>
    /// 应用举报规则
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>规则应用结果</returns>
    Task<ReportRuleApplicationResultDto> ApplyReportRulesAsync(Guid commentId);

    /// <summary>
    /// 自动举报违规评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="ruleIds">规则ID列表</param>
    /// <returns>自动举报结果</returns>
    Task<AutoReportResultDto> AutoReportCommentAsync(Guid commentId, IEnumerable<Guid> ruleIds);

    #endregion

    // 举报统计和分析
    #region 举报统计和分析

    /// <summary>
    /// 获取举报统计信息
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>举报统计信息</returns>
    Task<ReportStatsDto> GetReportStatsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取举报趋势统计
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns>举报趋势统计</returns>
    Task<IEnumerable<ReportTrendDto>> GetReportTrendAsync(int days = 30);

    /// <summary>
    /// 获取举报原因统计
    /// </summary>
    /// <returns>举报原因统计</returns>
    Task<ReportReasonStatsDto> GetReportReasonStatsAsync();

    /// <summary>
    /// 获取举报处理人员统计
    /// </summary>
    /// <returns>处理人员统计</returns>
    Task<IEnumerable<ReportHandlerStatsDto>> GetReportHandlerStatsAsync();

    /// <summary>
    /// 获取举报效率报告
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>举报效率报告</returns>
    Task<ReportEfficiencyReportDto> GetReportEfficiencyReportAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取重复举报统计
    /// </summary>
    /// <returns>重复举报统计</returns>
    Task<ReportDuplicationStatsDto> GetReportDuplicationStatsAsync();

    #endregion

    // 举报历史和跟踪
    #region 举报历史和跟踪

    /// <summary>
    /// 获取举报处理历史
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <returns>举报处理历史</returns>
    Task<IEnumerable<ReportHandlingHistoryDto>> GetReportHandlingHistoryAsync(Guid reportId);

    /// <summary>
    /// 获取用户举报历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">历史筛选条件</param>
    /// <returns>用户举报历史</returns>
    Task<PaginatedResult<ReportHandlingHistoryDto>> GetUserReportHistoryAsync(Guid userId, ReportHistoryFilterDto filter);

    /// <summary>
    /// 获取处理人处理历史
    /// </summary>
    /// <param name="handlerId">处理人ID</param>
    /// <param name="filter">历史筛选条件</param>
    /// <returns>处理人处理历史</returns>
    Task<PaginatedResult<ReportHandlingHistoryDto>> GetHandlerHistoryAsync(Guid handlerId, ReportHistoryFilterDto filter);

    /// <summary>
    /// 添加举报处理备注
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="note">备注内容</param>
    /// <param name="addedById">添加人ID</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddReportHandlingNoteAsync(Guid reportId, string note, Guid addedById);

    #endregion

    // 举报导出和报告
    #region 举报导出和报告

    /// <summary>
    /// 导出举报数据
    /// </summary>
    /// <param name="filter">导出筛选条件</param>
    /// <param name="format">导出格式</param>
    /// <returns>导出文件内容</returns>
    Task<byte[]> ExportReportsAsync(CommentReportFilterDto filter, ExportFormat format);

    /// <summary>
    /// 生成举报报告
    /// </summary>
    /// <param name="reportRequest">报告请求</param>
    /// <returns>举报报告</returns>
    Task<ReportGenerationResultDto> GenerateReportAsync(ReportGenerationRequestDto reportRequest);

    /// <summary>
    /// 获取举报摘要
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>举报摘要</returns>
    Task<ReportSummaryDto> GetReportSummaryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取举报热力图数据
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns>举报热力图数据</returns>
    Task<ReportHeatmapDataDto> GetReportHeatmapDataAsync(int days = 30);

    #endregion
}

/// <summary>
/// 批量创建举报结果DTO
/// </summary>
public class BulkCreateReportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<Guid> SuccessfulReportIds { get; set; } = new();
    public List<ReportCreationFailureDto> Failures { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// 举报创建失败详情DTO
/// </summary>
public class ReportCreationFailureDto
{
    public CreateCommentReportDto? Request { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 更新举报请求DTO
/// </summary>
public class UpdateCommentReportDto
{
    [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    public ReportReason? Reason { get; set; }
}

/// <summary>
/// 举报处理结果DTO
/// </summary>
public class ReportHandlingResultDto
{
    public Guid ReportId { get; set; }
    public bool Success { get; set; }
    public ReportStatus NewStatus { get; set; }
    public string? Resolution { get; set; }
    public Guid HandlerId { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public DateTime HandledAt { get; set; }
    public double HandlingTimeMinutes { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 批量处理举报结果DTO
/// </summary>
public class BulkReportHandlingResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<Guid> SuccessfulReportIds { get; set; } = new();
    public List<ReportHandlingFailureDto> Failures { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// 举报处理失败详情DTO
/// </summary>
public class ReportHandlingFailureDto
{
    public Guid ReportId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 创建举报规则DTO
/// </summary>
public class CreateReportRuleDto
{
    [Required(ErrorMessage = "规则名称不能为空")]
    [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "规则条件不能为空")]
    public string Condition { get; set; } = string.Empty;

    [Required(ErrorMessage = "规则动作不能为空")]
    public ReportRuleAction Action { get; set; }

    [StringLength(500, ErrorMessage = "规则描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int Priority { get; set; } = 0;
}

/// <summary>
/// 更新举报规则DTO
/// </summary>
public class UpdateReportRuleDto
{
    [StringLength(100, ErrorMessage = "规则名称长度不能超过100个字符")]
    public string? Name { get; set; }

    public string? Condition { get; set; }

    public ReportRuleAction? Action { get; set; }

    [StringLength(500, ErrorMessage = "规则描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? Priority { get; set; }
}

/// <summary>
/// 举报规则DTO
/// </summary>
public class ReportRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public ReportRuleAction Action { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UsageCount { get; set; }
}

/// <summary>
/// 举报规则动作枚举
/// </summary>
public enum ReportRuleAction
{
    Flag = 0,
    Escalate = 1,
    AutoResolve = 2,
    NotifyAdmin = 3,
    SuspendUser = 4,
    DeleteComment = 5
}

/// <summary>
/// 举报规则筛选条件DTO
/// </summary>
public class ReportRuleFilterDto
{
    public bool? IsActive { get; set; }
    public ReportRuleAction? Action { get; set; }
    public string? Search { get; set; }
    public ReportRuleSort SortBy { get; set; } = ReportRuleSort.PriorityDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 举报规则排序枚举
/// </summary>
public enum ReportRuleSort
{
    PriorityDesc = 0,
    PriorityAsc = 1,
    CreatedDesc = 2,
    CreatedAsc = 3,
    UsageDesc = 4,
    UsageAsc = 5
}

/// <summary>
/// 举报规则应用结果DTO
/// </summary>
public class ReportRuleApplicationResultDto
{
    public Guid CommentId { get; set; }
    public List<AppliedReportRuleDto> AppliedRules { get; set; } = new();
    public ReportRuleAction RecommendedAction { get; set; }
    public bool ShouldAutoReport { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 应用举报规则DTO
/// </summary>
public class AppliedReportRuleDto
{
    public Guid RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public bool Triggered { get; set; }
    public int Priority { get; set; }
    public string? MatchedCondition { get; set; }
}

/// <summary>
/// 自动举报结果DTO
/// </summary>
public class AutoReportResultDto
{
    public Guid CommentId { get; set; }
    public bool Success { get; set; }
    public Guid? ReportId { get; set; }
    public List<Guid> AppliedRuleIds { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 举报统计信息DTO
/// </summary>
public class ReportStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int RejectedReports { get; set; }
    public int UnderInvestigationReports { get; set; }
    public int UrgentReports { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double ResolutionRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// 举报趋势统计DTO
/// </summary>
public class ReportTrendDto
{
    public DateTime Date { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public int RejectedReports { get; set; }
    public double ResolutionRate { get; set; }
}

/// <summary>
/// 举报原因统计DTO
/// </summary>
public class ReportReasonStatsDto
{
    public Dictionary<ReportReason, int> ReasonCounts { get; set; } = new();
    public Dictionary<ReportReason, double> ReasonRates { get; set; } = new();
    public int TotalReports { get; set; }
    public ReportReason MostCommonReason { get; set; }
    public double MostCommonReasonRate { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 举报处理人员统计DTO
/// </summary>
public class ReportHandlerStatsDto
{
    public Guid HandlerId { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public int TotalHandled { get; set; }
    public int ResolvedCount { get; set; }
    public int RejectedCount { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public DateTime LastActivityAt { get; set; }
    public int DaysActive { get; set; }
}

/// <summary>
/// 举报效率报告DTO
/// </summary>
public class ReportEfficiencyReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalReports { get; set; }
    public int ResolvedReports { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double ThroughputPerHour { get; set; }
    public int HandlersCount { get; set; }
    public double EfficiencyScore { get; set; }
    public List<ReportHandlerPerformanceDto> HandlerPerformance { get; set; } = new();
    public List<HourlyReportStatsDto> HourlyStats { get; set; } = new();
}

/// <summary>
/// 举报处理人员表现DTO
/// </summary>
public class ReportHandlerPerformanceDto
{
    public Guid HandlerId { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public int HandledCount { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double PerformanceScore { get; set; }
    public int WorkingHours { get; set; }
}

/// <summary>
/// 每小时举报统计DTO
/// </summary>
public class HourlyReportStatsDto
{
    public int Hour { get; set; }
    public int ReportCount { get; set; }
    public int ResolvedCount { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

/// <summary>
/// 举报重复统计DTO
/// </summary>
public class ReportDuplicationStatsDto
{
    public int TotalReports { get; set; }
    public int UniqueCommentsReported { get; set; }
    public int DuplicatedReports { get; set; }
    public double DuplicationRate { get; set; }
    public Dictionary<Guid, int> MostReportedComments { get; set; } = new();
    public Dictionary<Guid, int> MostActiveReporters { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 举报处理历史DTO
/// </summary>
public class ReportHandlingHistoryDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid HandlerId { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public ReportStatus OldStatus { get; set; }
    public ReportStatus NewStatus { get; set; }
    public string? Action { get; set; }
    public string? Notes { get; set; }
    public DateTime ActionAt { get; set; }
    public double ProcessingTimeMinutes { get; set; }
}

/// <summary>
/// 举报历史筛选条件DTO
/// </summary>
public class ReportHistoryFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportStatus? Status { get; set; }
    public string? Search { get; set; }
    public ReportHistorySort SortBy { get; set; } = ReportHistorySort.ActionDesc;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 举报历史排序枚举
/// </summary>
public enum ReportHistorySort
{
    ActionDesc = 0,
    ActionAsc = 1,
    ReportDesc = 2,
    ReportAsc = 3,
    HandlerAsc = 4,
    HandlerDesc = 5
}

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    Csv = 0,
    Json = 1,
    Excel = 2,
    Pdf = 3
}

/// <summary>
/// 举报生成请求DTO
/// </summary>
public class ReportGenerationRequestDto
{
    [Required(ErrorMessage = "报告类型不能为空")]
    public ReportType Type { get; set; }

    [Required(ErrorMessage = "开始日期不能为空")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "结束日期不能为空")]
    public DateTime EndDate { get; set; }

    public ReportFormat Format { get; set; } = ReportFormat.Pdf;

    public Dictionary<string, string>? Filters { get; set; }

    public List<string>? IncludeSections { get; set; }
}

/// <summary>
/// 报告类型枚举
/// </summary>
public enum ReportType
{
    Summary = 0,
    Detailed = 1,
    Trend = 2,
    Performance = 3,
    Custom = 99
}

/// <summary>
/// 报告格式枚举
/// </summary>
public enum ReportFormat
{
    Pdf = 0,
    Excel = 1,
    Html = 2,
    Json = 3
}

/// <summary>
/// 报告生成结果DTO
/// </summary>
public class ReportGenerationResultDto
{
    public bool Success { get; set; }
    public string? ReportId { get; set; }
    public byte[]? ReportData { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// 举报摘要DTO
/// </summary>
public class ReportSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalReports { get; set; }
    public int ResolvedReports { get; set; }
    public int PendingReports { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public ReportReason MostCommonReason { get; set; }
    public int UniqueCommentsAffected { get; set; }
    public int UniqueUsersInvolved { get; set; }
}

/// <summary>
/// 举报热力图数据DTO
/// </summary>
public class ReportHeatmapDataDto
{
    public List<ReportHeatmapPointDto> DataPoints { get; set; } = new();
    public int MaxReports { get; set; }
    public int MinReports { get; set; }
    public double AverageReports { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// 举报热力图点DTO
/// </summary>
public class ReportHeatmapPointDto
{
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int ReportCount { get; set; }
    public double Intensity { get; set; }
}