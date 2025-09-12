using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息统计信息DTO
/// </summary>
public class MessageStatsDto
{
    /// <summary>
    /// 总消息数量
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 已发送消息数量
    /// </summary>
    public int SentCount { get; set; }
    
    /// <summary>
    /// 已接收消息数量
    /// </summary>
    public int ReceivedCount { get; set; }
    
    /// <summary>
    /// 未读消息数量
    /// </summary>
    public int UnreadCount { get; set; }
    
    /// <summary>
    /// 已读消息数量
    /// </summary>
    public int ReadCount { get; set; }
    
    /// <summary>
    /// 已回复消息数量
    /// </summary>
    public int RepliedCount { get; set; }
    
    /// <summary>
    /// 已转发消息数量
    /// </summary>
    public int ForwardedCount { get; set; }
    
    /// <summary>
    /// 已删除消息数量
    /// </summary>
    public int DeletedCount { get; set; }
    
    /// <summary>
    /// 发送失败消息数量
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// 已过期消息数量
    /// </summary>
    public int ExpiredCount { get; set; }
    
    /// <summary>
    /// 按消息类型分组的统计
    /// </summary>
    public Dictionary<MessageType, int> MessageTypeStats { get; set; } = new();
    
    /// <summary>
    /// 按消息状态分组的统计
    /// </summary>
    public Dictionary<MessageStatus, int> MessageStatusStats { get; set; } = new();
    
    /// <summary>
    /// 按优先级分组的统计
    /// </summary>
    public Dictionary<MessagePriority, int> PriorityStats { get; set; } = new();
    
    /// <summary>
    /// 统计时间范围
    /// </summary>
    public DateRangeDto DateRange { get; set; } = new();
}

/// <summary>
/// 用户消息统计信息DTO
/// </summary>
public class UserMessageStatsDto
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
    /// 用户显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// 消息统计信息
    /// </summary>
    public MessageStatsDto Stats { get; set; } = new();
    
    /// <summary>
    /// 最后发送消息时间
    /// </summary>
    public DateTime? LastSentAt { get; set; }
    
    /// <summary>
    /// 最后接收消息时间
    /// </summary>
    public DateTime? LastReceivedAt { get; set; }
    
    /// <summary>
    /// 平均响应时间（秒）
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// 消息响应率（已回复/已接收）
    /// </summary>
    public double ResponseRate { get; set; }
}

/// <summary>
/// 消息会话信息DTO
/// </summary>
public class MessageConversationDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid ConversationId { get; set; }
    
    /// <summary>
    /// 会话类型
    /// </summary>
    public ConversationType ConversationType { get; set; }
    
    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 会话描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 参与者列表
    /// </summary>
    public List<ConversationParticipantDto> Participants { get; set; } = new();
    
    /// <summary>
    /// 最新消息
    /// </summary>
    public MessageDto? LastMessage { get; set; }
    
    /// <summary>
    /// 消息总数
    /// </summary>
    public int MessageCount { get; set; }
    
    /// <summary>
    /// 未读消息数量
    /// </summary>
    public int UnreadCount { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime? LastMessageAt { get; set; }
    
    /// <summary>
    /// 是否已静音
    /// </summary>
    public bool IsMuted { get; set; }
    
    /// <summary>
    /// 是否已置顶
    /// </summary>
    public bool IsPinned { get; set; }
    
    /// <summary>
    /// 会话标签
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// 会话参与者信息DTO
/// </summary>
public class ConversationParticipantDto
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
    /// 用户显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// 参与者角色
    /// </summary>
    public ParticipantRole Role { get; set; } = ParticipantRole.Member;
    
    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTime JoinedAt { get; set; }
    
    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public DateTime? LastActiveAt { get; set; }
    
    /// <summary>
    /// 未读消息数量
    /// </summary>
    public int UnreadCount { get; set; }
    
    /// <summary>
    /// 是否已静音
    /// </summary>
    public bool IsMuted { get; set; }
}

/// <summary>
/// 参与者角色枚举
/// </summary>
public enum ParticipantRole
{
    /// <summary>
    /// 普通成员
    /// </summary>
    Member = 0,
    
    /// <summary>
    /// 管理员
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// 所有者
    /// </summary>
    Owner = 2,
    
    /// <summary>
    /// 访客
    /// </summary>
    Guest = 3
}

/// <summary>
/// 消息DTO（用于传输和展示）
/// </summary>
public class MessageDto
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 发送者信息
    /// </summary>
    public UserDto Sender { get; set; } = new();
    
    /// <summary>
    /// 接收者信息
    /// </summary>
    public UserDto Receiver { get; set; } = new();
    
    /// <summary>
    /// 消息主题
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// 消息内容（摘要，用于列表展示）
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; }
    
    /// <summary>
    /// 消息状态
    /// </summary>
    public MessageStatus Status { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; }
    
    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }
    
    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }
    
    /// <summary>
    /// 消息标签
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 附件数量
    /// </summary>
    public int AttachmentCount { get; set; }
    
    /// <summary>
    /// 回复数量
    /// </summary>
    public int ReplyCount { get; set; }
    
    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired { get; set; }
    
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 用户信息DTO
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// 用户状态
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;
}

/// <summary>
/// 用户状态枚举
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// 活跃
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// 离线
    /// </summary>
    Offline = 1,
    
    /// <summary>
    /// 忙碌
    /// </summary>
    Busy = 2,
    
    /// <summary>
    /// 离开
    /// </summary>
    Away = 3,
    
    /// <summary>
    /// 请勿打扰
    /// </summary>
    DoNotDisturb = 4
}

/// <summary>
/// 日期范围DTO
/// </summary>
public class DateRangeDto
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 时间跨度（天数）
    /// </summary>
    public int? DurationDays { get; set; }
}

/// <summary>
/// 消息附件统计信息DTO
/// </summary>
public class MessageAttachmentStatsDto
{
    /// <summary>
    /// 总附件数量
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 总文件大小（字节）
    /// </summary>
    public long TotalFileSize { get; set; }
    
    /// <summary>
    /// 总下载次数
    /// </summary>
    public int TotalDownloadCount { get; set; }
    
    /// <summary>
    /// 按附件类型分组的统计
    /// </summary>
    public Dictionary<AttachmentType, int> AttachmentTypeStats { get; set; } = new();
    
    /// <summary>
    /// 按附件状态分组的统计
    /// </summary>
    public Dictionary<AttachmentStatus, int> AttachmentStatusStats { get; set; } = new();
    
    /// <summary>
    /// 按文件扩展名分组的统计
    /// </summary>
    public Dictionary<string, int> FileExtensionStats { get; set; } = new();
    
    /// <summary>
    /// 平均文件大小（字节）
    /// </summary>
    public double AverageFileSize { get; set; }
    
    /// <summary>
    /// 最大文件大小（字节）
    /// </summary>
    public long MaxFileSize { get; set; }
    
    /// <summary>
    /// 最小文件大小（字节）
    /// </summary>
    public long MinFileSize { get; set; }
}