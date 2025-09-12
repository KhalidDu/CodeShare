using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息会话DTO - 用于API响应
/// </summary>
public class MessageConversationDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 会话描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 会话类型
    /// </summary>
    public ConversationType ConversationType { get; set; }

    /// <summary>
    /// 参与者ID列表
    /// </summary>
    public List<Guid> ParticipantIds { get; set; } = new();

    /// <summary>
    /// 参与者信息
    /// </summary>
    public List<ConversationParticipantDto> Participants { get; set; } = new();

    /// <summary>
    /// 最后一条消息
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
    /// 是否已归档
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// 是否已置顶
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否已静音
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// 当前用户是否是参与者
    /// </summary>
    public bool IsParticipant { get; set; }

    /// <summary>
    /// 当前用户是否是创建者
    /// </summary>
    public bool IsCreator { get; set; }

    /// <summary>
    /// 当前用户是否可以编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 当前用户是否可以删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 当前用户是否可以添加参与者
    /// </summary>
    public bool CanAddParticipants { get; set; }

    /// <summary>
    /// 当前用户是否可以移除参与者
    /// </summary>
    public bool CanRemoveParticipants { get; set; }
}

/// <summary>
/// 会话参与者DTO
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
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户头像
    /// </summary>
    public string? UserAvatar { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public string Role { get; set; } = "Member";

    /// <summary>
    /// 最后阅读时间
    /// </summary>
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// 是否已静音
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// 是否已离开
    /// </summary>
    public bool HasLeft { get; set; }
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
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 总消息数量
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 已发送消息数量
    /// </summary>
    public int SentMessages { get; set; }

    /// <summary>
    /// 已接收消息数量
    /// </summary>
    public int ReceivedMessages { get; set; }

    /// <summary>
    /// 未读消息数量
    /// </summary>
    public int UnreadMessages { get; set; }

    /// <summary>
    /// 已读消息数量
    /// </summary>
    public int ReadMessages { get; set; }

    /// <summary>
    /// 已回复消息数量
    /// </summary>
    public int RepliedMessages { get; set; }

    /// <summary>
    /// 已转发消息数量
    /// </summary>
    public int ForwardedMessages { get; set; }

    /// <summary>
    /// 已删除消息数量
    /// </summary>
    public int DeletedMessages { get; set; }

    /// <summary>
    /// 总附件数量
    /// </summary>
    public int TotalAttachments { get; set; }

    /// <summary>
    /// 总附件大小（字节）
    /// </summary>
    public long TotalAttachmentSize { get; set; }

    /// <summary>
    /// 参与的会话数量
    /// </summary>
    public int ConversationCount { get; set; }

    /// <summary>
    /// 最后一条消息时间
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 按消息类型统计
    /// </summary>
    public Dictionary<MessageType, int> MessageTypeStats { get; set; } = new();

    /// <summary>
    /// 按优先级统计
    /// </summary>
    public Dictionary<MessagePriority, int> PriorityStats { get; set; } = new();

    /// <summary>
    /// 按消息状态统计
    /// </summary>
    public Dictionary<MessageStatus, int> StatusStats { get; set; } = new();

    /// <summary>
    /// 按标签统计
    /// </summary>
    public Dictionary<string, int> TagStats { get; set; } = new();
}

/// <summary>
/// 消息附件统计信息DTO
/// </summary>
public class MessageAttachmentStatsDto
{
    /// <summary>
    /// 总附件数量
    /// </summary>
    public int TotalAttachments { get; set; }

    /// <summary>
    /// 总文件大小（字节）
    /// </summary>
    public long TotalFileSize { get; set; }

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

    /// <summary>
    /// 总下载次数
    /// </summary>
    public int TotalDownloads { get; set; }

    /// <summary>
    /// 平均下载次数
    /// </summary>
    public double AverageDownloads { get; set; }

    /// <summary>
    /// 最大下载次数
    /// </summary>
    public int MaxDownloads { get; set; }

    /// <summary>
    /// 按附件类型统计
    /// </summary>
    public Dictionary<AttachmentType, int> AttachmentTypeStats { get; set; } = new();

    /// <summary>
    /// 按附件状态统计
    /// </summary>
    public Dictionary<AttachmentStatus, int> AttachmentStatusStats { get; set; } = new();

    /// <summary>
    /// 按文件扩展名统计
    /// </summary>
    public Dictionary<string, int> FileExtensionStats { get; set; } = new();

    /// <summary>
    /// 按Content-Type统计
    /// </summary>
    public Dictionary<string, int> ContentTypeStats { get; set; } = new();

    /// <summary>
    /// 按上传日期统计（按天）
    /// </summary>
    public Dictionary<DateTime, int> DailyUploadStats { get; set; } = new();

    /// <summary>
    /// 按上传日期统计（按月）
    /// </summary>
    public Dictionary<DateTime, int> MonthlyUploadStats { get; set; } = new();

    /// <summary>
    /// 按用户统计
    /// </summary>
    public Dictionary<Guid, UserAttachmentStatsDto> UserStats { get; set; } = new();
}

/// <summary>
/// 用户附件统计信息DTO
/// </summary>
public class UserAttachmentStatsDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 附件数量
    /// </summary>
    public int AttachmentCount { get; set; }

    /// <summary>
    /// 文件大小总和（字节）
    /// </summary>
    public long TotalFileSize { get; set; }

    /// <summary>
    /// 下载次数总和
    /// </summary>
    public int TotalDownloads { get; set; }

    /// <summary>
    /// 上传的附件列表
    /// </summary>
    public List<MessageAttachmentDto> Attachments { get; set; } = new();
}