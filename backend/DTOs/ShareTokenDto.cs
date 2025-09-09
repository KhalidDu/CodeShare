using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 分享令牌数据传输对象 - 用于分享令牌的数据传输和展示
/// </summary>
public class ShareTokenDto
{
    /// <summary>
    /// 分享令牌唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分享令牌字符串，用于访问分享的代码片段
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代码片段ID
    /// </summary>
    public Guid CodeSnippetId { get; set; }

    /// <summary>
    /// 创建分享令牌的用户ID
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 创建分享令牌的用户名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// 分享令牌过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 分享令牌创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 分享令牌最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 分享令牌是否已激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 访问次数统计
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 最大访问次数限制，0表示无限制
    /// </summary>
    public int MaxAccessCount { get; set; }

    /// <summary>
    /// 分享权限级别
    /// </summary>
    public Models.SharePermission Permission { get; set; }

    /// <summary>
    /// 分享描述信息
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否有密码保护
    /// </summary>
    public bool HasPassword { get; set; }

    /// <summary>
    /// 是否允许下载
    /// </summary>
    public bool AllowDownload { get; set; }

    /// <summary>
    /// 是否允许复制
    /// </summary>
    public bool AllowCopy { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// 关联的代码片段标题
    /// </summary>
    public string CodeSnippetTitle { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代码片段语言
    /// </summary>
    public string CodeSnippetLanguage { get; set; }

    /// <summary>
    /// 分享链接是否已过期
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// 分享链接是否已达到访问次数限制
    /// </summary>
    public bool IsAccessLimitReached { get; set; }

    /// <summary>
    /// 剩余访问次数，-1表示无限制
    /// </summary>
    public int RemainingAccessCount { get; set; }
}

/// <summary>
/// 创建分享请求DTO - 用于创建新的分享令牌
/// </summary>
public class CreateShareDto
{
    /// <summary>
    /// 要分享的代码片段ID
    /// </summary>
    [Required(ErrorMessage = "代码片段ID不能为空")]
    public Guid CodeSnippetId { get; set; }

    /// <summary>
    /// 分享权限级别
    /// </summary>
    [Required(ErrorMessage = "权限级别不能为空")]
    [Range(0, 2, ErrorMessage = "权限级别值无效")]
    public Models.SharePermission Permission { get; set; }

    /// <summary>
    /// 分享描述信息
    /// </summary>
    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 分享密码，用于保护分享链接
    /// </summary>
    [StringLength(64, ErrorMessage = "密码长度不能超过64个字符")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6个字符")]
    public string? Password { get; set; }

    /// <summary>
    /// 分享令牌有效期（小时），0表示永久有效
    /// </summary>
    [Range(0, 8760, ErrorMessage = "有效期必须在0-8760小时之间")]
    public int ExpiresInHours { get; set; } = 24;

    /// <summary>
    /// 最大访问次数限制，0表示无限制
    /// </summary>
    [Range(0, 10000, ErrorMessage = "最大访问次数必须在0-10000之间")]
    public int MaxAccessCount { get; set; } = 0;

    /// <summary>
    /// 是否允许下载
    /// </summary>
    public bool AllowDownload { get; set; } = true;

    /// <summary>
    /// 是否允许复制
    /// </summary>
    public bool AllowCopy { get; set; } = true;
}

/// <summary>
/// 更新分享请求DTO - 用于更新分享令牌设置
/// </summary>
public class UpdateShareDto
{
    /// <summary>
    /// 分享描述信息
    /// </summary>
    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 分享权限级别
    /// </summary>
    [Range(0, 2, ErrorMessage = "权限级别值无效")]
    public Models.SharePermission? Permission { get; set; }

    /// <summary>
    /// 是否允许下载
    /// </summary>
    public bool? AllowDownload { get; set; }

    /// <summary>
    /// 是否允许复制
    /// </summary>
    public bool? AllowCopy { get; set; }

    /// <summary>
    /// 最大访问次数限制，0表示无限制
    /// </summary>
    [Range(0, 10000, ErrorMessage = "最大访问次数必须在0-10000之间")]
    public int? MaxAccessCount { get; set; }

    /// <summary>
    /// 延长有效期（小时），0表示不延长
    /// </summary>
    [Range(0, 8760, ErrorMessage = "延长时间必须在0-8760小时之间")]
    public int ExtendHours { get; set; } = 0;
}

/// <summary>
/// 分享统计数据DTO - 用于展示分享的统计信息
/// </summary>
public class ShareStatsDto
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
    /// 总访问次数
    /// </summary>
    public int TotalAccessCount { get; set; }

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
    /// 剩余访问次数，-1表示无限制
    /// </summary>
    public int RemainingAccessCount { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 分享链接是否已过期
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// 分享链接是否已达到访问次数限制
    /// </summary>
    public bool IsAccessLimitReached { get; set; }

    /// <summary>
    /// 每日访问统计（最近30天）
    /// </summary>
    public List<DailyAccessStatDto> DailyStats { get; set; } = new();

    /// <summary>
    /// 访问来源统计
    /// </summary>
    public List<AccessSourceStatDto> SourceStats { get; set; } = new();
}

/// <summary>
/// 每日访问统计DTO
/// </summary>
public class DailyAccessStatDto
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
}

/// <summary>
/// 访问来源统计DTO
/// </summary>
public class AccessSourceStatDto
{
    /// <summary>
    /// 来源类型（direct、link、qr_code等）
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// 访问次数
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

