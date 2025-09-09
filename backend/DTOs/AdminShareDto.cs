using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 管理员分享过滤器
/// </summary>
public class AdminShareFilter
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    [StringLength(100, ErrorMessage = "搜索关键词长度不能超过100个字符")]
    public string? Search { get; set; }

    /// <summary>
    /// 创建者用户ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid? CodeSnippetId { get; set; }

    /// <summary>
    /// 是否只显示激活状态的分享
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 是否只显示已过期的分享
    /// </summary>
    public bool? IsExpired { get; set; }

    /// <summary>
    /// 是否有密码保护
    /// </summary>
    public bool? HasPassword { get; set; }

    /// <summary>
    /// 分享权限级别
    /// </summary>
    public SharePermission? Permission { get; set; }

    /// <summary>
    /// 开始创建时间
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束创建时间
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序字段
    /// </summary>
    [StringLength(50, ErrorMessage = "排序字段长度不能超过50个字符")]
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "排序方向必须是asc或desc")]
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// 批量操作分享请求
/// </summary>
public class BulkShareOperationRequest
{
    /// <summary>
    /// 分享令牌ID列表
    /// </summary>
    [Required(ErrorMessage = "分享令牌ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少选择一个分享链接")]
    [MaxLength(100, ErrorMessage = "最多支持100个分享链接")]
    public List<Guid> ShareTokenIds { get; set; } = new();

    /// <summary>
    /// 操作类型：revoke（撤销）、delete（删除）、extend（延长）、activate（激活）
    /// </summary>
    [Required(ErrorMessage = "操作类型不能为空")]
    [RegularExpression("^(revoke|delete|extend|activate)$", ErrorMessage = "操作类型无效")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 操作参数（如延长的小时数）
    /// </summary>
    public int? OperationParam { get; set; }
}

/// <summary>
/// 系统分享统计信息
/// </summary>
public class SystemShareStatsDto
{
    /// <summary>
    /// 总分享数量
    /// </summary>
    public int TotalShares { get; set; }

    /// <summary>
    /// 活跃分享数量
    /// </summary>
    public int ActiveShares { get; set; }

    /// <summary>
    /// 过期分享数量
    /// </summary>
    public int ExpiredShares { get; set; }

    /// <summary>
    /// 总访问次数
    /// </summary>
    public long TotalAccessCount { get; set; }

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
    /// 参与分享的用户数量
    /// </summary>
    public int ActiveUserCount { get; set; }

    /// <summary>
    /// 热门分享排行
    /// </summary>
    public List<PopularShareDto> PopularShares { get; set; } = new();

    /// <summary>
    /// 活跃用户排行
    /// </summary>
    public List<ActiveUserDto> ActiveUsers { get; set; } = new();

    /// <summary>
    /// 每日分享统计（最近30天）
    /// </summary>
    public List<DailyShareStatDto> DailyStats { get; set; } = new();

    /// <summary>
    /// 权限分布统计
    /// </summary>
    public List<PermissionStatDto> PermissionStats { get; set; } = new();

    /// <summary>
    /// 语言分布统计
    /// </summary>
    public List<LanguageStatDto> LanguageStats { get; set; } = new();
}

/// <summary>
/// 热门分享统计
/// </summary>
public class PopularShareDto
{
    /// <summary>
    /// 分享令牌ID
    /// </summary>
    public Guid ShareTokenId { get; set; }

    /// <summary>
    /// 分享令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 代码片段标题
    /// </summary>
    public string CodeSnippetTitle { get; set; } = string.Empty;

    /// <summary>
    /// 代码片段语言
    /// </summary>
    public string CodeSnippetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// 创建者用户名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 活跃用户统计
/// </summary>
public class ActiveUserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 分享数量
    /// </summary>
    public int ShareCount { get; set; }

    /// <summary>
    /// 总访问次数
    /// </summary>
    public int TotalAccessCount { get; set; }

    /// <summary>
    /// 最后分享时间
    /// </summary>
    public DateTime? LastShareAt { get; set; }
}

/// <summary>
/// 每日分享统计
/// </summary>
public class DailyShareStatDto
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 新增分享数量
    /// </summary>
    public int NewShares { get; set; }

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 活跃用户数
    /// </summary>
    public int ActiveUsers { get; set; }
}

/// <summary>
/// 权限统计
/// </summary>
public class PermissionStatDto
{
    /// <summary>
    /// 权限级别
    /// </summary>
    public SharePermission Permission { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// 分享数量
    /// </summary>
    public int ShareCount { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 语言统计
/// </summary>
public class LanguageStatDto
{
    /// <summary>
    /// 编程语言
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// 分享数量
    /// </summary>
    public int ShareCount { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 批量操作结果DTO
/// </summary>
public class BulkOperationResultDto
{
    /// <summary>
    /// 总处理数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功处理数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败处理数量
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 失败的分享令牌ID列表
    /// </summary>
    public List<Guid> FailedShareTokenIds { get; set; } = new();

    /// <summary>
    /// 失败原因列表
    /// </summary>
    public List<string> FailureReasons { get; set; } = new();
}