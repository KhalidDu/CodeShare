using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 分享访问日志实体 - 遵循单一职责原则，只负责分享访问记录数据
/// </summary>
public class ShareAccessLog
{
    /// <summary>
    /// 访问日志唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的分享令牌ID
    /// </summary>
    [Required(ErrorMessage = "分享令牌ID不能为空")]
    public Guid ShareTokenId { get; set; }

    /// <summary>
    /// 关联的代码片段ID
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
    /// 访问来源（direct、link、qr_code等）
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
    /// 访问时间
    /// </summary>
    [Required(ErrorMessage = "访问时间不能为空")]
    public DateTime AccessedAt { get; set; }

    /// <summary>
    /// 访问是否成功
    /// </summary>
    [Required(ErrorMessage = "访问状态不能为空")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 访问失败原因
    /// </summary>
    [StringLength(200, ErrorMessage = "失败原因长度不能超过200个字符")]
    public string? FailureReason { get; set; }

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

    // 导航属性 - 用于 Multi-mapping 查询结果
    /// <summary>
    /// 分享令牌信息
    /// </summary>
    public ShareToken? ShareToken { get; set; }

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
}

/// <summary>
/// 访问来源枚举
/// </summary>
public enum AccessSource
{
    /// <summary>
    /// 直接访问
    /// </summary>
    Direct = 0,

    /// <summary>
    /// 链接访问
    /// </summary>
    Link = 1,

    /// <summary>
    /// 二维码访问
    /// </summary>
    QrCode = 2,

    /// <summary>
    /// 邮件访问
    /// </summary>
    Email = 3,

    /// <summary>
    /// 社交媒体访问
    /// </summary>
    Social = 4,

    /// <summary>
    /// 搜索引擎访问
    /// </summary>
    Search = 5,

    /// <summary>
    /// 其他来源
    /// </summary>
    Other = 99
}

/// <summary>
/// 设备类型枚举
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// 桌面设备
    /// </summary>
    Desktop = 0,

    /// <summary>
    /// 移动设备
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// 平板设备
    /// </summary>
    Tablet = 2,

    /// <summary>
    /// 智能电视
    /// </summary>
    SmartTV = 3,

    /// <summary>
    /// 可穿戴设备
    /// </summary>
    Wearable = 4,

    /// <summary>
    /// 其他设备
    /// </summary>
    Other = 99
}