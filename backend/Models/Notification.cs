using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 通知实体 - 遵循单一职责原则，只负责通知数据
/// </summary>
public class Notification
{
    /// <summary>
    /// 通知唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 接收通知的用户ID
    /// </summary>
    [Required(ErrorMessage = "接收用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    [Required(ErrorMessage = "通知标题不能为空")]
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    /// <summary>
    /// 通知消息
    /// </summary>
    [StringLength(500, ErrorMessage = "通知消息长度不能超过500个字符")]
    public string? Message { get; set; }

    /// <summary>
    /// 通知优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 通知状态
    /// </summary>
    [Required(ErrorMessage = "通知状态不能为空")]
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

    /// <summary>
    /// 相关实体类型
    /// </summary>
    public RelatedEntityType? RelatedEntityType { get; set; }

    /// <summary>
    /// 相关实体ID
    /// </summary>
    [StringLength(36, ErrorMessage = "相关实体ID长度不能超过36个字符")]
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// 触发通知的用户ID
    /// </summary>
    public Guid? TriggeredByUserId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public NotificationAction? Action { get; set; }

    /// <summary>
    /// 通知频道
    /// </summary>
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 计划发送时间
    /// </summary>
    public DateTime? ScheduledToSendAt { get; set; }

    /// <summary>
    /// 发送次数
    /// </summary>
    public int SendCount { get; set; } = 0;

    /// <summary>
    /// 最后发送时间
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [StringLength(1000, ErrorMessage = "错误信息长度不能超过1000个字符")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 通知数据（JSON格式存储额外信息）
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? DataJson { get; set; }

    /// <summary>
    /// 通知标签（用于分类）
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 通知图标
    /// </summary>
    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    /// <summary>
    /// 通知颜色
    /// </summary>
    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// 确认时间
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// 是否已归档
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// 归档时间
    /// </summary>
    public DateTime? ArchivedAt { get; set; }

    /// <summary>
    /// 是否已删除（软删除）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // 导航属性
    /// <summary>
    /// 接收通知的用户信息
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 触发通知的用户信息
    /// </summary>
    public User? TriggeredByUser { get; set; }

    /// <summary>
    /// 通知发送历史记录
    /// </summary>
    public List<NotificationDeliveryHistory> DeliveryHistory { get; set; } = new();

    /// <summary>
    /// 通知用户设置
    /// </summary>
    public NotificationSetting? UserSetting { get; set; }
}

/// <summary>
/// 通知设置实体 - 记录用户的通知偏好设置
/// </summary>
public class NotificationSetting
{
    /// <summary>
    /// 设置唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType? NotificationType { get; set; }

    /// <summary>
    /// 启用应用内通知
    /// </summary>
    public bool EnableInApp { get; set; } = true;

    /// <summary>
    /// 启用邮件通知
    /// </summary>
    public bool EnableEmail { get; set; } = true;

    /// <summary>
    /// 启用推送通知
    /// </summary>
    public bool EnablePush { get; set; } = true;

    /// <summary>
    /// 启用桌面通知
    /// </summary>
    public bool EnableDesktop { get; set; } = true;

    /// <summary>
    /// 启用声音提醒
    /// </summary>
    public bool EnableSound { get; set; } = true;

    /// <summary>
    /// 通知频率
    /// </summary>
    public NotificationFrequency Frequency { get; set; } = NotificationFrequency.Immediate;

    /// <summary>
    /// 免打扰开始时间
    /// </summary>
    public TimeSpan? QuietHoursStart { get; set; }

    /// <summary>
    /// 免打扰结束时间
    /// </summary>
    public TimeSpan? QuietHoursEnd { get; set; }

    /// <summary>
    /// 是否启用免打扰
    /// </summary>
    public bool EnableQuietHours { get; set; } = false;

    /// <summary>
    /// 邮件通知频率
    /// </summary>
    public EmailNotificationFrequency EmailFrequency { get; set; } = EmailNotificationFrequency.Immediate;

    /// <summary>
    /// 批量通知间隔（分钟）
    /// </summary>
    [Range(5, 1440, ErrorMessage = "批量通知间隔必须在5-1440分钟之间")]
    public int BatchIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// 是否启用批量通知
    /// </summary>
    public bool EnableBatching { get; set; } = false;

    /// <summary>
    /// 通知语言
    /// </summary>
    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string? Language { get; set; } = "zh-CN";

    /// <summary>
    /// 时区
    /// </summary>
    [StringLength(50, ErrorMessage = "时区长度不能超过50个字符")]
    public string? TimeZone { get; set; } = "Asia/Shanghai";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 是否为默认设置
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 设置名称
    /// </summary>
    [StringLength(100, ErrorMessage = "设置名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 设置描述
    /// </summary>
    [StringLength(500, ErrorMessage = "设置描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    // 导航属性
    /// <summary>
    /// 关联的用户
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// 用户的通知列表
    /// </summary>
    public List<Notification> Notifications { get; set; } = new();
}

/// <summary>
/// 通知发送历史记录实体 - 记录通知的发送状态和历史
/// </summary>
public class NotificationDeliveryHistory
{
    /// <summary>
    /// 历史记录唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的通知ID
    /// </summary>
    [Required(ErrorMessage = "通知ID不能为空")]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 发送渠道
    /// </summary>
    [Required(ErrorMessage = "发送渠道不能为空")]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 发送状态
    /// </summary>
    [Required(ErrorMessage = "发送状态不能为空")]
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [StringLength(1000, ErrorMessage = "错误信息长度不能超过1000个字符")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// 最后重试时间
    /// </summary>
    public DateTime? LastRetryAt { get; set; }

    /// <summary>
    /// 接收地址（邮箱地址、设备令牌等）
    /// </summary>
    [StringLength(500, ErrorMessage = "接收地址长度不能超过500个字符")]
    public string? RecipientAddress { get; set; }

    /// <summary>
    /// 发送服务提供商
    /// </summary>
    [StringLength(50, ErrorMessage = "服务提供商长度不能超过50个字符")]
    public string? Provider { get; set; }

    /// <summary>
    /// 发送成本
    /// </summary>
    [Column(TypeName = "DECIMAL(10,4)")]
    public decimal? Cost { get; set; }

    /// <summary>
    /// 发送元数据（JSON格式）
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? MetadataJson { get; set; }

    // 导航属性
    /// <summary>
    /// 关联的通知
    /// </summary>
    public Notification Notification { get; set; } = null!;
}

/// <summary>
/// 通知类型枚举
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 评论通知
    /// </summary>
    Comment = 0,

    /// <summary>
    /// 回复通知
    /// </summary>
    Reply = 1,

    /// <summary>
    /// 消息通知
    /// </summary>
    Message = 2,

    /// <summary>
    /// 系统通知
    /// </summary>
    System = 3,

    /// <summary>
    /// 点赞通知
    /// </summary>
    Like = 4,

    /// <summary>
    /// 分享通知
    /// </summary>
    Share = 5,

    /// <summary>
    /// 关注通知
    /// </summary>
    Follow = 6,

    /// <summary>
    /// 提及通知
    /// </summary>
    Mention = 7,

    /// <summary>
    /// 标签通知
    /// </summary>
    Tag = 8,

    /// <summary>
    /// 安全通知
    /// </summary>
    Security = 9,

    /// <summary>
    /// 账户通知
    /// </summary>
    Account = 10,

    /// <summary>
    /// 更新通知
    /// </summary>
    Update = 11,

    /// <summary>
    /// 维护通知
    /// </summary>
    Maintenance = 12,

    /// <summary>
    /// 公告通知
    /// </summary>
    Announcement = 13,

    /// <summary>
    /// 自定义通知
    /// </summary>
    Custom = 99
}

/// <summary>
/// 通知优先级枚举
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// 低优先级
    /// </summary>
    Low = 0,

    /// <summary>
    /// 普通优先级
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 高优先级
    /// </summary>
    High = 2,

    /// <summary>
    /// 紧急
    /// </summary>
    Urgent = 3,

    /// <summary>
    /// 系统紧急
    /// </summary>
    Critical = 4
}

/// <summary>
/// 通知状态枚举
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// 待发送
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 发送中
    /// </summary>
    Sending = 1,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 2,

    /// <summary>
    /// 已送达
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 4,

    /// <summary>
    /// 未读
    /// </summary>
    Unread = 5,

    /// <summary>
    /// 已确认
    /// </summary>
    Confirmed = 6,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 7,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 8,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 9,

    /// <summary>
    /// 已归档
    /// </summary>
    Archived = 10
}

/// <summary>
/// 相关实体类型枚举
/// </summary>
public enum RelatedEntityType
{
    /// <summary>
    /// 代码片段
    /// </summary>
    Snippet = 0,

    /// <summary>
    /// 评论
    /// </summary>
    Comment = 1,

    /// <summary>
    /// 消息
    /// </summary>
    Message = 2,

    /// <summary>
    /// 用户
    /// </summary>
    User = 3,

    /// <summary>
    /// 标签
    /// </summary>
    Tag = 4,

    /// <summary>
    /// 分享
    /// </summary>
    Share = 5,

    /// <summary>
    /// 系统
    /// </summary>
    System = 6,

    /// <summary>
    /// 其他
    /// </summary>
    Other = 99
}

/// <summary>
/// 通知操作类型枚举
/// </summary>
public enum NotificationAction
{
    /// <summary>
    /// 创建
    /// </summary>
    Created = 0,

    /// <summary>
    /// 更新
    /// </summary>
    Updated = 1,

    /// <summary>
    /// 删除
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// 点赞
    /// </summary>
    Liked = 3,

    /// <summary>
    /// 分享
    /// </summary>
    Shared = 4,

    /// <summary>
    /// 关注
    /// </summary>
    Followed = 5,

    /// <summary>
    /// 提及
    /// </summary>
    Mentioned = 6,

    /// <summary>
    /// 评论
    /// </summary>
    Commented = 7,

    /// <summary>
    /// 回复
    /// </summary>
    Replied = 8,

    /// <summary>
    /// 报告
    /// </summary>
    Reported = 9,

    /// <summary>
    /// 审核
    /// </summary>
    Moderated = 10,

    /// <summary>
    /// 其他
    /// </summary>
    Other = 99
}

/// <summary>
/// 通知渠道枚举
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// 应用内通知
    /// </summary>
    InApp = 0,

    /// <summary>
    /// 邮件通知
    /// </summary>
    Email = 1,

    /// <summary>
    /// 推送通知
    /// </summary>
    Push = 2,

    /// <summary>
    /// 桌面通知
    /// </summary>
    Desktop = 3,

    /// <summary>
    /// 短信通知
    /// </summary>
    SMS = 4,

    /// <summary>
    /// Webhook通知
    /// </summary>
    Webhook = 5,

    /// <summary>
    /// Slack通知
    /// </summary>
    Slack = 6,

    /// <summary>
    /// 微信通知
    /// </summary>
    WeChat = 7,

    /// <summary>
    /// 钉钉通知
    /// </summary>
    DingTalk = 8,

    /// <summary>
    /// 企业微信通知
    /// </summary>
    WeCom = 9
}

/// <summary>
/// 通知频率枚举
/// </summary>
public enum NotificationFrequency
{
    /// <summary>
    /// 立即通知
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// 每小时通知
    /// </summary>
    Hourly = 1,

    /// <summary>
    /// 每日通知
    /// </summary>
    Daily = 2,

    /// <summary>
    /// 每周通知
    /// </summary>
    Weekly = 3,

    /// <summary>
    /// 每月通知
    /// </summary>
    Monthly = 4,

    /// <summary>
    /// 从不通知
    /// </summary>
    Never = 5
}

/// <summary>
/// 邮件通知频率枚举
/// </summary>
public enum EmailNotificationFrequency
{
    /// <summary>
    /// 立即发送
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// 每小时汇总
    /// </summary>
    HourlyDigest = 1,

    /// <summary>
    /// 每日汇总
    /// </summary>
    DailyDigest = 2,

    /// <summary>
    /// 每周汇总
    /// </summary>
    WeeklyDigest = 3,

    /// <summary>
    /// 从不发送
    /// </summary>
    Never = 4
}

/// <summary>
/// 发送状态枚举
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// 待发送
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 发送中
    /// </summary>
    Sending = 1,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 2,

    /// <summary>
    /// 已送达
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 4,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 5,

    /// <summary>
    /// 已跳过
    /// </summary>
    Skipped = 6,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 7,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 8
}