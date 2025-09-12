using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知日志DTO - 用于返回通知日志详情
/// </summary>
public class NotificationLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public NotificationLogLevel Level { get; set; }

    /// <summary>
    /// 日志类型
    /// </summary>
    public NotificationLogType LogType { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 详细信息
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 操作结果
    /// </summary>
    public NotificationOperationResult OperationResult { get; set; }

    /// <summary>
    /// 操作耗时（毫秒）
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// 操作状态码
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// 操作响应
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// 操作请求
    /// </summary>
    public string? Request { get; set; }

    /// <summary>
    /// 操作元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// 操作来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 操作用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 操作IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 操作用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 相关通知信息
    /// </summary>
    public NotificationDto? Notification { get; set; }

    /// <summary>
    /// 关联的发送历史
    /// </summary>
    public NotificationDeliveryHistoryDto? DeliveryHistory { get; set; }
}

/// <summary>
/// 通知日志筛选条件DTO
/// </summary>
public class NotificationLogFilterDto
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid? NotificationId { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public NotificationLogLevel? Level { get; set; }

    /// <summary>
    /// 日志类型
    /// </summary>
    public NotificationLogType? LogType { get; set; }

    /// <summary>
    /// 操作结果
    /// </summary>
    public NotificationOperationResult? OperationResult { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 搜索范围
    /// </summary>
    public List<NotificationLogSearchScope> SearchScopes { get; set; } = new();

    /// <summary>
    /// 排序字段
    /// </summary>
    public NotificationLogSort Sort { get; set; } = NotificationLogSort.CreatedAtDesc;

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 最大页大小
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// 是否包含异常信息
    /// </summary>
    public bool IncludeExceptions { get; set; } = false;

    /// <summary>
    /// 是否包含详细信息
    /// </summary>
    public bool IncludeDetails { get; set; } = false;

    /// <summary>
    /// 是否包含通知信息
    /// </summary>
    public bool IncludeNotification { get; set; } = false;

    /// <summary>
    /// 是否只统计数量
    /// </summary>
    public bool CountOnly { get; set; } = false;
}

/// <summary>
/// 通知日志统计DTO
/// </summary>
public class NotificationLogStatsDto
{
    /// <summary>
    /// 总日志数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 今日日志数量
    /// </summary>
    public int TodayCount { get; set; }

    /// <summary>
    /// 本周日志数量
    /// </summary>
    public int WeekCount { get; set; }

    /// <summary>
    /// 本月日志数量
    /// </summary>
    public int MonthCount { get; set; }

    /// <summary>
    /// 错误日志数量
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 警告日志数量
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// 信息日志数量
    /// </summary>
    public int InfoCount { get; set; }

    /// <summary>
    /// 调试日志数量
    /// </summary>
    public int DebugCount { get; set; }

    /// <summary>
    /// 按级别统计
    /// </summary>
    public Dictionary<NotificationLogLevel, int> LevelStats { get; set; } = new();

    /// <summary>
    /// 按类型统计
    /// </summary>
    public Dictionary<NotificationLogType, int> TypeStats { get; set; } = new();

    /// <summary>
    /// 按结果统计
    /// </summary>
    public Dictionary<NotificationOperationResult, int> ResultStats { get; set; } = new();

    /// <summary>
    /// 按来源统计
    /// </summary>
    public Dictionary<string, int> SourceStats { get; set; } = new();

    /// <summary>
    /// 按小时统计
    /// </summary>
    public List<NotificationLogHourlyStatsDto> HourlyStats { get; set; } = new();

    /// <summary>
    /// 按日期统计
    /// </summary>
    public List<NotificationLogDailyStatsDto> DailyStats { get; set; } = new();

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 通知日志小时统计DTO
/// </summary>
public class NotificationLogHourlyStatsDto
{
    /// <summary>
    /// 时间
    /// </summary>
    public DateTime Hour { get; set; }

    /// <summary>
    /// 日志数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 错误数量
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 平均响应时间
    /// </summary>
    public double AverageDuration { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }
}

/// <summary>
/// 通知日志日期统计DTO
/// </summary>
public class NotificationLogDailyStatsDto
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 日志数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 错误数量
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 平均响应时间
    /// </summary>
    public double AverageDuration { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 峰值时间
    /// </summary>
    public string? PeakHour { get; set; }

    /// <summary>
    /// 峰值数量
    /// </summary>
    public int PeakCount { get; set; }
}

/// <summary>
/// 通知日志列表DTO
/// </summary>
public class NotificationLogListDto
{
    /// <summary>
    /// 日志列表
    /// </summary>
    public List<NotificationLogDto> Logs { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// 筛选条件
    /// </summary>
    public NotificationLogFilterDto Filter { get; set; } = new();
}

/// <summary>
/// 通知日志导出选项DTO
/// </summary>
public class NotificationLogExportOptionsDto
{
    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; } = ExportFormat.Json;

    /// <summary>
    /// 筛选条件
    /// </summary>
    public NotificationLogFilterDto Filter { get; set; } = new();

    /// <summary>
    /// 是否包含异常信息
    /// </summary>
    public bool IncludeExceptions { get; set; } = false;

    /// <summary>
    /// 是否包含详细信息
    /// </summary>
    public bool IncludeDetails { get; set; } = false;

    /// <summary>
    /// 是否包含通知信息
    /// </summary>
    public bool IncludeNotification { get; set; } = false;

    /// <summary>
    /// 字段选择
    /// </summary>
    public List<string> Fields { get; set; } = new();

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 是否压缩
    /// </summary>
    public bool Compress { get; set; } = false;

    /// <summary>
    /// 最大记录数
    /// </summary>
    public int? MaxRecords { get; set; }
}

/// <summary>
/// 通知日志清理选项DTO
/// </summary>
public class NotificationLogCleanupOptionsDto
{
    /// <summary>
    /// 清理类型
    /// </summary>
    public NotificationLogCleanupType CleanupType { get; set; } = NotificationLogCleanupType.ByDate;

    /// <summary>
    /// 保留天数
    /// </summary>
    public int? RetentionDays { get; set; }

    /// <summary>
    /// 保留记录数
    /// </summary>
    public int? RetentionCount { get; set; }

    /// <summary>
    /// 清理前备份
    /// </summary>
    public bool BackupBeforeCleanup { get; set; } = true;

    /// <summary>
    /// 备份路径
    /// </summary>
    public string? BackupPath { get; set; }

    /// <summary>
    /// 筛选条件
    /// </summary>
    public NotificationLogFilterDto? Filter { get; set; }

    /// <summary>
    /// 是否只模拟运行
    /// </summary>
    public bool DryRun { get; set; } = false;
}

/// <summary>
/// 通知日志清理结果DTO
/// </summary>
public class NotificationLogCleanupResultDto
{
    /// <summary>
    /// 清理是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 清理消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 删除的记录数
    /// </summary>
    public int DeletedCount { get; set; }

    /// <summary>
    /// 保留的记录数
    /// </summary>
    public int RetainedCount { get; set; }

    /// <summary>
    /// 备份文件路径
    /// </summary>
    public string? BackupFilePath { get; set; }

    /// <summary>
    /// 清理耗时
    /// </summary>
    public TimeSpan CleanupDuration { get; set; }

    /// <summary>
    /// 清理时间
    /// </summary>
    public DateTime CleanedAt { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 通知日志级别枚举
/// </summary>
public enum NotificationLogLevel
{
    /// <summary>
    /// 调试级别
    /// </summary>
    Debug = 0,

    /// <summary>
    /// 信息级别
    /// </summary>
    Info = 1,

    /// <summary>
    /// 警告级别
    /// </summary>
    Warning = 2,

    /// <summary>
    /// 错误级别
    /// </summary>
    Error = 3,

    /// <summary>
    /// 严重错误级别
    /// </summary>
    Critical = 4
}

/// <summary>
/// 通知日志类型枚举
/// </summary>
public enum NotificationLogType
{
    /// <summary>
    /// 创建通知
    /// </summary>
    Create = 0,

    /// <summary>
    /// 更新通知
    /// </summary>
    Update = 1,

    /// <summary>
    /// 删除通知
    /// </summary>
    Delete = 2,

    /// <summary>
    /// 发送通知
    /// </summary>
    Send = 3,

    /// <summary>
    /// 重试发送
    /// </summary>
    Retry = 4,

    /// <summary>
    /// 送达通知
    /// </summary>
    Deliver = 5,

    /// <summary>
    /// 阅读通知
    /// </summary>
    Read = 6,

    /// <summary>
    /// 确认通知
    /// </summary>
    Confirm = 7,

    /// <summary>
    /// 归档通知
    /// </summary>
    Archive = 8,

    /// <summary>
    /// 恢复通知
    /// </summary>
    Restore = 9,

    /// <summary>
    /// 批量操作
    /// </summary>
    Batch = 10,

    /// <summary>
    /// 系统操作
    /// </summary>
    System = 11,

    /// <summary>
    /// 用户操作
    /// </summary>
    User = 12,

    /// <summary>
    /// API调用
    /// </summary>
    Api = 13,

    /// <summary>
    /// Webhook调用
    /// </summary>
    Webhook = 14,

    /// <summary>
    /// 模板渲染
    /// </summary>
    Template = 15,

    /// <summary>
    /// 渠道配置
    /// </summary>
    Channel = 16,

    /// <summary>
    /// 订阅管理
    /// </summary>
    Subscription = 17,

    /// <summary>
    /// 设置管理
    /// </summary>
    Settings = 18,

    /// <summary>
    /// 其他操作
    /// </summary>
    Other = 99
}

/// <summary>
/// 通知操作结果枚举
/// </summary>
public enum NotificationOperationResult
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 1,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// 取消
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// 跳过
    /// </summary>
    Skipped = 4,

    /// <summary>
    /// 重试
    /// </summary>
    Retry = 5,

    /// <summary>
    /// 验证失败
    /// </summary>
    ValidationFailed = 6,

    /// <summary>
    /// 权限不足
    /// </summary>
    Unauthorized = 7,

    /// <summary>
    /// 限流
    /// </summary>
    RateLimited = 8,

    /// <summary>
    /// 系统错误
    /// </summary>
    SystemError = 9,

    /// <summary>
    /// 网络错误
    /// </summary>
    NetworkError = 10,

    /// <summary>
    /// 未知错误
    /// </summary>
    Unknown = 99
}

/// <summary>
/// 通知日志搜索范围枚举
/// </summary>
public enum NotificationLogSearchScope
{
    /// <summary>
    /// 搜索消息
    /// </summary>
    Message = 0,

    /// <summary>
    /// 搜索详细信息
    /// </summary>
    Details = 1,

    /// <summary>
    /// 搜索异常信息
    /// </summary>
    Exception = 2,

    /// <summary>
    /// 搜索来源
    /// </summary>
    Source = 3,

    /// <summary>
    /// 搜索状态码
    /// </summary>
    StatusCode = 4,

    /// <summary>
    /// 搜索用户名
    /// </summary>
    UserName = 5,

    /// <summary>
    /// 搜索所有文本字段
    /// </summary>
    AllText = 6
}

/// <summary>
/// 通知日志排序枚举
/// </summary>
public enum NotificationLogSort
{
    /// <summary>
    /// 创建时间降序
    /// </summary>
    CreatedAtDesc = 0,

    /// <summary>
    /// 创建时间升序
    /// </summary>
    CreatedAtAsc = 1,

    /// <summary>
    /// 级别降序
    /// </summary>
    LevelDesc = 2,

    /// <summary>
    /// 级别升序
    /// </summary>
    LevelAsc = 3,

    /// <summary>
    /// 耗时降序
    /// </summary>
    DurationDesc = 4,

    /// <summary>
    /// 耗时升序
    /// </summary>
    DurationAsc = 5,

    /// <summary>
    /// 用户名升序
    /// </summary>
    UserNameAsc = 6,

    /// <summary>
    /// 用户名降序
    /// </summary>
    UserNameDesc = 7,

    /// <summary>
    /// 来源升序
    /// </summary>
    SourceAsc = 8,

    /// <summary>
    /// 来源降序
    /// </summary>
    SourceDesc = 9
}

/// <summary>
/// 通知日志清理类型枚举
/// </summary>
public enum NotificationLogCleanupType
{
    /// <summary>
    /// 按日期清理
    /// </summary>
    ByDate = 0,

    /// <summary>
    /// 按数量清理
    /// </summary>
    ByCount = 1,

    /// <summary>
    /// 按级别清理
    /// </summary>
    ByLevel = 2,

    /// <summary>
    /// 按类型清理
    /// </summary>
    ByType = 3,

    /// <summary>
    /// 全部清理
    /// </summary>
    All = 4
}