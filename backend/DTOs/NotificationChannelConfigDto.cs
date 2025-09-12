using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知渠道配置DTO - 用于返回渠道配置详情
/// </summary>
public class NotificationChannelConfigDto
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 渠道描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 渠道配置
    /// </summary>
    public Dictionary<string, string> Config { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否为默认配置
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否支持批量发送
    /// </summary>
    public bool SupportsBatchSend { get; set; } = false;

    /// <summary>
    /// 批量大小限制
    /// </summary>
    public int? BatchSizeLimit { get; set; }

    /// <summary>
    /// 每日发送限制
    /// </summary>
    public int? DailySendLimit { get; set; }

    /// <summary>
    /// 每小时发送限制
    /// </summary>
    public int? HourlySendLimit { get; set; }

    /// <summary>
    /// 发送统计
    /// </summary>
    public NotificationChannelStatsDto Stats { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>
    /// 创建者信息
    /// </summary>
    public UserInfoDto? Creator { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 是否可测试
    /// </summary>
    public bool CanTest { get; set; }
}

/// <summary>
/// 通知渠道统计DTO
/// </summary>
public class NotificationChannelStatsDto
{
    /// <summary>
    /// 总发送次数
    /// </summary>
    public int TotalSent { get; set; }

    /// <summary>
    /// 成功发送次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送次数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均响应时间
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// 今日发送次数
    /// </summary>
    public int TodaySent { get; set; }

    /// <summary>
    /// 本月发送次数
    /// </summary>
    public int MonthSent { get; set; }

    /// <summary>
    /// 总成本
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// 最后发送时间
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    /// <summary>
    /// 最后错误时间
    /// </summary>
    public DateTime? LastErrorAt { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastErrorMessage { get; set; }
}

/// <summary>
/// 通知渠道配置创建请求DTO
/// </summary>
public class NotificationChannelConfigCreateDto
{
    /// <summary>
    /// 渠道名称
    /// </summary>
    [Required(ErrorMessage = "渠道名称不能为空")]
    [StringLength(100, ErrorMessage = "渠道名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 渠道描述
    /// </summary>
    [StringLength(500, ErrorMessage = "渠道描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    [Required(ErrorMessage = "通知渠道不能为空")]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 渠道配置
    /// </summary>
    [Required(ErrorMessage = "渠道配置不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个配置项")]
    public Dictionary<string, string> Config { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否为默认配置
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 优先级
    /// </summary>
    [Range(0, 100, ErrorMessage = "优先级必须在0-100之间")]
    public int Priority { get; set; } = 0;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    [Range(0, 10, ErrorMessage = "最大重试次数必须在0-10之间")]
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    [Range(1, 3600, ErrorMessage = "重试间隔必须在1-3600秒之间")]
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否支持批量发送
    /// </summary>
    public bool SupportsBatchSend { get; set; } = false;

    /// <summary>
    /// 批量大小限制
    /// </summary>
    [Range(1, 1000, ErrorMessage = "批量大小限制必须在1-1000之间")]
    public int? BatchSizeLimit { get; set; }

    /// <summary>
    /// 每日发送限制
    /// </summary>
    [Range(1, 100000, ErrorMessage = "每日发送限制必须在1-100000之间")]
    public int? DailySendLimit { get; set; }

    /// <summary>
    /// 每小时发送限制
    /// </summary>
    [Range(1, 10000, ErrorMessage = "每小时发送限制必须在1-10000之间")]
    public int? HourlySendLimit { get; set; }
}

/// <summary>
/// 通知渠道配置更新请求DTO
/// </summary>
public class NotificationChannelConfigUpdateDto
{
    /// <summary>
    /// 渠道名称
    /// </summary>
    [StringLength(100, ErrorMessage = "渠道名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 渠道描述
    /// </summary>
    [StringLength(500, ErrorMessage = "渠道描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 渠道配置
    /// </summary>
    public Dictionary<string, string>? Config { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 是否为默认配置
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    [Range(0, 100, ErrorMessage = "优先级必须在0-100之间")]
    public int? Priority { get; set; }

    /// <summary>
    /// 最大重试次数
    /// </summary>
    [Range(0, 10, ErrorMessage = "最大重试次数必须在0-10之间")]
    public int? MaxRetryCount { get; set; }

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    [Range(1, 3600, ErrorMessage = "重试间隔必须在1-3600秒之间")]
    public int? RetryIntervalSeconds { get; set; }

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// 是否支持批量发送
    /// </summary>
    public bool? SupportsBatchSend { get; set; }

    /// <summary>
    /// 批量大小限制
    /// </summary>
    [Range(1, 1000, ErrorMessage = "批量大小限制必须在1-1000之间")]
    public int? BatchSizeLimit { get; set; }

    /// <summary>
    /// 每日发送限制
    /// </summary>
    [Range(1, 100000, ErrorMessage = "每日发送限制必须在1-100000之间")]
    public int? DailySendLimit { get; set; }

    /// <summary>
    /// 每小时发送限制
    /// </summary>
    [Range(1, 10000, ErrorMessage = "每小时发送限制必须在1-10000之间")]
    public int? HourlySendLimit { get; set; }
}

/// <summary>
/// 通知渠道配置测试请求DTO
/// </summary>
public class NotificationChannelConfigTestDto
{
    /// <summary>
    /// 配置ID
    /// </summary>
    [Required(ErrorMessage = "配置ID不能为空")]
    public Guid ConfigId { get; set; }

    /// <summary>
    /// 测试类型
    /// </summary>
    public NotificationChannelTestType TestType { get; set; } = NotificationChannelTestType.Connectivity;

    /// <summary>
    /// 测试数据
    /// </summary>
    public Dictionary<string, object>? TestData { get; set; }

    /// <summary>
    /// 测试接收地址
    /// </summary>
    public string? TestRecipient { get; set; }

    /// <summary>
    /// 是否使用默认测试数据
    /// </summary>
    public bool UseDefaultTestData { get; set; } = true;
}

/// <summary>
/// 通知渠道配置测试结果DTO
/// </summary>
public class NotificationChannelConfigTestResultDto
{
    /// <summary>
    /// 测试是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 测试消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 测试类型
    /// </summary>
    public NotificationChannelTestType TestType { get; set; }

    /// <summary>
    /// 测试时间
    /// </summary>
    public DateTime TestAt { get; set; }

    /// <summary>
    /// 测试耗时
    /// </summary>
    public TimeSpan TestDuration { get; set; }

    /// <summary>
    /// 响应时间
    /// </summary>
    public TimeSpan? ResponseTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public Dictionary<string, object>? ErrorDetails { get; set; }

    /// <summary>
    /// 测试结果数据
    /// </summary>
    public Dictionary<string, object>? ResultData { get; set; }

    /// <summary>
    /// 建议
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// 通知渠道配置列表DTO
/// </summary>
public class NotificationChannelConfigListDto
{
    /// <summary>
    /// 渠道配置列表
    /// </summary>
    public List<NotificationChannelConfigDto> Configs { get; set; } = new();

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
}

/// <summary>
/// 通知渠道测试类型枚举
/// </summary>
public enum NotificationChannelTestType
{
    /// <summary>
    /// 连通性测试
    /// </summary>
    Connectivity = 0,

    /// <summary>
    /// 认证测试
    /// </summary>
    Authentication = 1,

    /// <summary>
    /// 发送测试
    /// </summary>
    SendTest = 2,

    /// <summary>
    /// 批量发送测试
    /// </summary>
    BatchSendTest = 3,

    /// <summary>
    /// 性能测试
    /// </summary>
    Performance = 4,

    /// <summary>
    /// 完整测试
    /// </summary>
    FullTest = 5
}

/// <summary>
/// 通知渠道配置筛选条件DTO
/// </summary>
public class NotificationChannelConfigFilterDto
{
    /// <summary>
    /// 渠道类型
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 是否为默认配置
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public NotificationChannelConfigSort Sort { get; set; } = NotificationChannelConfigSort.CreatedAtDesc;

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 通知渠道配置排序枚举
/// </summary>
public enum NotificationChannelConfigSort
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
    /// 名称升序
    /// </summary>
    NameAsc = 2,

    /// <summary>
    /// 名称降序
    /// </summary>
    NameDesc = 3,

    /// <summary>
    /// 优先级降序
    /// </summary>
    PriorityDesc = 4,

    /// <summary>
    /// 优先级升序
    /// </summary>
    PriorityAsc = 5,

    /// <summary>
    /// 最后使用时间降序
    /// </summary>
    LastUsedAtDesc = 6,

    /// <summary>
    /// 最后使用时间升序
    /// </summary>
    LastUsedAtAsc = 7,

    /// <summary>
    /// 成功率降序
    /// </summary>
    SuccessRateDesc = 8,

    /// <summary>
    /// 成功率升序
    /// </summary>
    SuccessRateAsc = 9
}