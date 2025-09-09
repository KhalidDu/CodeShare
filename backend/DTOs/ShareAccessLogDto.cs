using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 分享访问日志数据传输对象 - 用于访问日志的数据传输和展示
/// </summary>
public class ShareAccessLogDto
{
    /// <summary>
    /// 访问日志唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的分享令牌ID
    /// </summary>
    public Guid ShareTokenId { get; set; }

    /// <summary>
    /// 关联的代码片段ID
    /// </summary>
    public Guid CodeSnippetId { get; set; }

    /// <summary>
    /// 访问者IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 访问者用户代理（User-Agent）
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 访问来源（direct、link、qr_code等）
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 访问者国家/地区
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 访问者城市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 访问者浏览器类型
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 访问者操作系统
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 访问者设备类型
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 访问时间
    /// </summary>
    public DateTime AccessedAt { get; set; }

    /// <summary>
    /// 访问是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 访问失败原因
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 访问持续时间（毫秒）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 访问者会话ID
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 访问者引用页（Referer）
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// 访问者语言偏好
    /// </summary>
    public string? AcceptLanguage { get; set; }

    /// <summary>
    /// 代码片段标题
    /// </summary>
    public string CodeSnippetTitle { get; set; } = string.Empty;

    /// <summary>
    /// 代码片段语言
    /// </summary>
    public string CodeSnippetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// 创建分享令牌的用户名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// IP地址地理位置信息
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 是否为首次访问
    /// </summary>
    public bool IsFirstAccess { get; set; }

    /// <summary>
    /// 访问序号（当日第几次访问）
    /// </summary>
    public int AccessNumber { get; set; }
}

/// <summary>
/// 访问日志过滤器
/// </summary>
public class AccessLogFilter
{
    /// <summary>
    /// 分享令牌ID
    /// </summary>
    public Guid? ShareTokenId { get; set; }

    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid? CodeSnippetId { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 访问来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 国家/地区
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 浏览器类型
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否只显示成功访问
    /// </summary>
    public bool? IsSuccess { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; } = "AccessedAt";

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// 访问统计数据
/// </summary>
public class AccessStats
{
    /// <summary>
    /// 分享令牌ID
    /// </summary>
    public Guid ShareTokenId { get; set; }

    /// <summary>
    /// 总访问次数
    /// </summary>
    public int TotalAccessCount { get; set; }

    /// <summary>
    /// 唯一访问次数（按IP地址）
    /// </summary>
    public int UniqueAccessCount { get; set; }

    /// <summary>
    /// 成功访问次数
    /// </summary>
    public int SuccessAccessCount { get; set; }

    /// <summary>
    /// 失败访问次数
    /// </summary>
    public int FailedAccessCount { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }

    /// <summary>
    /// 首次访问时间
    /// </summary>
    public DateTime? FirstAccessAt { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastAccessAt { get; set; }

    /// <summary>
    /// 今日访问次数
    /// </summary>
    public int TodayAccessCount { get; set; }

    /// <summary>
    /// 本周访问次数
    /// </summary>
    public int ThisWeekAccessCount { get; set; }

    /// <summary>
    /// 本月访问次数
    /// </summary>
    public int ThisMonthAccessCount { get; set; }

    /// <summary>
    /// 增长率（相比上期）
    /// </summary>
    public double GrowthRate { get; set; }
}

/// <summary>
/// 每日访问统计
/// </summary>
public class DailyAccessStat
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 成功访问次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 每小时访问统计
/// </summary>
public class HourlyAccessStat
{
    /// <summary>
    /// 小时（0-23）
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }
}

/// <summary>
/// 访问来源统计
/// </summary>
public class AccessSourceStat
{
    /// <summary>
    /// 来源类型
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 设备类型统计
/// </summary>
public class DeviceTypeStat
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 国家/地区统计
/// </summary>
public class CountryStat
{
    /// <summary>
    /// 国家/地区代码
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// 国家/地区名称
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 城市列表
    /// </summary>
    public List<CityStat> Cities { get; set; } = new();
}

/// <summary>
/// 城市统计
/// </summary>
public class CityStat
{
    /// <summary>
    /// 城市名称
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 浏览器统计
/// </summary>
public class BrowserStat
{
    /// <summary>
    /// 浏览器名称
    /// </summary>
    public string Browser { get; set; } = string.Empty;

    /// <summary>
    /// 浏览器版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 操作系统统计
/// </summary>
public class OperatingSystemStat
{
    /// <summary>
    /// 操作系统名称
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 唯一访问者数量
    /// </summary>
    public int UniqueVisitors { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 平均访问持续时间（毫秒）
    /// </summary>
    public double AverageDuration { get; set; }
}

/// <summary>
/// 创建访问日志请求DTO
/// </summary>
public class CreateAccessLogDto
{
    /// <summary>
    /// 分享令牌ID
    /// </summary>
    [Required(ErrorMessage = "分享令牌ID不能为空")]
    public Guid ShareTokenId { get; set; }

    /// <summary>
    /// 代码片段ID
    /// </summary>
    [Required(ErrorMessage = "代码片段ID不能为空")]
    public Guid CodeSnippetId { get; set; }

    /// <summary>
    /// 访问者IP地址
    /// </summary>
    [Required(ErrorMessage = "IP地址不能为空")]
    [StringLength(45, ErrorMessage = "IP地址长度不能超过45个字符")]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 访问者用户代理（User-Agent）
    /// </summary>
    [StringLength(500, ErrorMessage = "用户代理长度不能超过500个字符")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 访问来源
    /// </summary>
    [StringLength(50, ErrorMessage = "访问来源长度不能超过50个字符")]
    public string? Source { get; set; }

    /// <summary>
    /// 访问者国家/地区
    /// </summary>
    [StringLength(100, ErrorMessage = "国家/地区长度不能超过100个字符")]
    public string? Country { get; set; }

    /// <summary>
    /// 访问者城市
    /// </summary>
    [StringLength(100, ErrorMessage = "城市长度不能超过100个字符")]
    public string? City { get; set; }

    /// <summary>
    /// 访问者浏览器类型
    /// </summary>
    [StringLength(50, ErrorMessage = "浏览器类型长度不能超过50个字符")]
    public string? Browser { get; set; }

    /// <summary>
    /// 访问者操作系统
    /// </summary>
    [StringLength(50, ErrorMessage = "操作系统长度不能超过50个字符")]
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 访问者设备类型
    /// </summary>
    [StringLength(50, ErrorMessage = "设备类型长度不能超过50个字符")]
    public string? DeviceType { get; set; }

    /// <summary>
    /// 访问持续时间（毫秒）
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "访问持续时间不能为负数")]
    public int Duration { get; set; }

    /// <summary>
    /// 访问者会话ID
    /// </summary>
    [StringLength(100, ErrorMessage = "会话ID长度不能超过100个字符")]
    public string? SessionId { get; set; }

    /// <summary>
    /// 访问者引用页（Referer）
    /// </summary>
    [StringLength(500, ErrorMessage = "引用页长度不能超过500个字符")]
    public string? Referer { get; set; }

    /// <summary>
    /// 访问者语言偏好
    /// </summary>
    [StringLength(50, ErrorMessage = "语言偏好长度不能超过50个字符")]
    public string? AcceptLanguage { get; set; }

    /// <summary>
    /// 访问是否成功
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// 访问失败原因
    /// </summary>
    [StringLength(200, ErrorMessage = "失败原因长度不能超过200个字符")]
    public string? FailureReason { get; set; }
}

/// <summary>
/// 批量创建访问日志请求DTO
/// </summary>
public class BulkCreateAccessLogDto
{
    /// <summary>
    /// 访问日志列表
    /// </summary>
    [Required(ErrorMessage = "访问日志列表不能为空")]
    [MinLength(1, ErrorMessage = "访问日志列表至少包含一条记录")]
    [MaxLength(1000, ErrorMessage = "批量创建最多支持1000条记录")]
    public List<CreateAccessLogDto> AccessLogs { get; set; } = new();
}