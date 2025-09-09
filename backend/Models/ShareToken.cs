using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 分享令牌实体 - 遵循单一职责原则，只负责分享令牌数据
/// </summary>
public class ShareToken
{
    /// <summary>
    /// 分享令牌唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分享令牌字符串，用于访问分享的代码片段
    /// </summary>
    [Required(ErrorMessage = "分享令牌不能为空")]
    [StringLength(64, ErrorMessage = "分享令牌长度不能超过64个字符")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代码片段ID
    /// </summary>
    [Required(ErrorMessage = "代码片段ID不能为空")]
    public Guid CodeSnippetId { get; set; }

    /// <summary>
    /// 创建分享令牌的用户ID
    /// </summary>
    [Required(ErrorMessage = "创建用户ID不能为空")]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 分享令牌过期时间
    /// </summary>
    [Required(ErrorMessage = "过期时间不能为空")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 分享令牌创建时间
    /// </summary>
    [Required(ErrorMessage = "创建时间不能为空")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 分享令牌最后更新时间
    /// </summary>
    [Required(ErrorMessage = "更新时间不能为空")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 分享令牌是否已激活
    /// </summary>
    [Required(ErrorMessage = "激活状态不能为空")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 访问次数统计
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "访问次数不能为负数")]
    public int AccessCount { get; set; }

    /// <summary>
    /// 最大访问次数限制，0表示无限制
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "最大访问次数不能为负数")]
    public int MaxAccessCount { get; set; }

    /// <summary>
    /// 分享权限级别
    /// </summary>
    [Required(ErrorMessage = "权限级别不能为空")]
    public SharePermission Permission { get; set; }

    /// <summary>
    /// 分享描述信息
    /// </summary>
    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 分享密码，用于保护分享链接
    /// </summary>
    [StringLength(64, ErrorMessage = "密码长度不能超过64个字符")]
    public string? Password { get; set; }

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

    // 导航属性 - 用于 Multi-mapping 查询结果
    /// <summary>
    /// 创建分享令牌的用户名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代码片段标题
    /// </summary>
    public string CodeSnippetTitle { get; set; } = string.Empty;

    /// <summary>
    /// 关联的代码片段语言
    /// </summary>
    public string CodeSnippetLanguage { get; set; } = string.Empty;
}

/// <summary>
/// 分享权限枚举
/// </summary>
public enum SharePermission
{
    /// <summary>
    /// 只读权限
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// 可编辑权限
    /// </summary>
    Edit = 1,

    /// <summary>
    /// 完全权限
    /// </summary>
    Full = 2
}