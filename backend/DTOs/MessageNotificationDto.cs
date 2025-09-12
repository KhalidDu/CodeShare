using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息通知DTO
/// </summary>
public class MessageNotificationDto
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public MessageNotificationType Type { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 关联的消息ID
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 关联的会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 发送者信息
    /// </summary>
    public UserDto Sender { get; set; } = new();

    /// <summary>
    /// 接收者ID
    /// </summary>
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 通知状态
    /// </summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 通知图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 通知颜色
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 操作链接
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// 操作按钮
    /// </summary>
    public List<NotificationActionButton> ActionButtons { get; set; } = new();

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// 是否可以关闭
    /// </summary>
    public bool CanDismiss { get; set; } = true;

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;
}

/// <summary>
/// 通知操作按钮DTO
/// </summary>
public class NotificationActionButton
{
    /// <summary>
    /// 按钮文本
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 按钮类型
    /// </summary>
    public NotificationActionType Type { get; set; } = NotificationActionType.Default;

    /// <summary>
    /// 按钮样式
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// 点击链接
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 点击操作
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// 操作参数
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// 是否显示确认对话框
    /// </summary>
    public bool ShowConfirmation { get; set; } = false;

    /// <summary>
    /// 确认消息
    /// </summary>
    public string? ConfirmationMessage { get; set; }
}

/// <summary>
/// 实时消息DTO
/// </summary>
public class RealTimeMessageDto
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 发送者信息
    /// </summary>
    public UserDto Sender { get; set; } = new();

    /// <summary>
    /// 接收者信息
    /// </summary>
    public UserDto? Receiver { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// 附件信息
    /// </summary>
    public List<MessageAttachmentDto> Attachments { get; set; } = new();

    /// <summary>
    /// 实时消息类型
    /// </summary>
    public RealTimeMessageType RealTimeType { get; set; }

    /// <summary>
    /// 目标用户ID列表
    /// </summary>
    public List<Guid> TargetUserIds { get; set; } = new();

    /// <summary>
    /// 是否需要回执
    /// </summary>
    public bool RequiresReceipt { get; set; } = false;

    /// <summary>
    /// 消息状态
    /// </summary>
    public MessageStatus Status { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    public long SequenceNumber { get; set; }

    /// <summary>
    /// 服务器时间戳
    /// </summary>
    public DateTime ServerTimestamp { get; set; }
}

/// <summary>
/// 消息回执DTO
/// </summary>
public class MessageReceiptDto
{
    /// <summary>
    /// 回执ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// 回执类型
    /// </summary>
    public ReceiptType ReceiptType { get; set; }

    /// <summary>
    /// 回执状态
    /// </summary>
    public ReceiptStatus Status { get; set; } = ReceiptStatus.Pending;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// 设备信息
    /// </summary>
    public DeviceInfo? Device { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 设备信息DTO
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceType DeviceType { get; set; } = DeviceType.Web;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 屏幕分辨率
    /// </summary>
    public string? ScreenResolution { get; set; }

    /// <summary>
    /// 应用版本
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public DateTime? LastActiveAt { get; set; }
}

/// <summary>
/// 消息推送DTO
/// </summary>
public class MessagePushDto
{
    /// <summary>
    /// 推送ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 推送标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 推送内容
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 推送类型
    /// </summary>
    public PushType PushType { get; set; } = PushType.Message;

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// 目标设备Token
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>
    /// 推送平台
    /// </summary>
    public PushPlatform Platform { get; set; } = PushPlatform.Web;

    /// <summary>
    /// 推送状态
    /// </summary>
    public PushStatus Status { get; set; } = PushStatus.Pending;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 点击时间
    /// </summary>
    public DateTime? ClickedAt { get; set; }

    /// <summary>
    /// 关联的消息ID
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 关联的会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 图标URL
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// 图片URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 点击动作
    /// </summary>
    public string? ClickAction { get; set; }

    /// <summary>
    /// 动作数据
    /// </summary>
    public Dictionary<string, object>? ActionData { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public PushPriority Priority { get; set; } = PushPriority.Normal;

    /// <summary>
    /// 声音
    /// </summary>
    public string? Sound { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 用户在线状态DTO
/// </summary>
public class UserOnlineStatusDto
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
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 在线状态
    /// </summary>
    public UserOnlineStatus Status { get; set; } = UserOnlineStatus.Offline;

    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public DateTime LastActiveAt { get; set; }

    /// <summary>
    /// 当前活动
    /// </summary>
    public string? CurrentActivity { get; set; }

    /// <summary>
    /// 设备信息
    /// </summary>
    public DeviceInfo? Device { get; set; }

    /// <summary>
    /// 会话ID列表
    /// </summary>
    public List<Guid> ActiveConversationIds { get; set; } = new();

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// 自定义状态
    /// </summary>
    public Dictionary<string, object>? CustomStatus { get; set; }
}

/// <summary>
/// 打字状态DTO
/// </summary>
public class TypingStatusDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 是否正在打字
    /// </summary>
    public bool IsTyping { get; set; }

    /// <summary>
    /// 开始打字时间
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 目标用户ID列表
    /// </summary>
    public List<Guid> TargetUserIds { get; set; } = new();

    /// <summary>
    /// 打字状态类型
    /// </summary>
    public TypingStatusType TypingStatusType { get; set; } = TypingStatusType.Typing;
}

// 枚举定义
/// <summary>
/// 消息通知类型枚举
/// </summary>
public enum MessageNotificationType
{
    /// <summary>
    /// 新消息
    /// </summary>
    NewMessage = 0,

    /// <summary>
    /// 消息已读
    /// </summary>
    MessageRead = 1,

    /// <summary>
    /// 消息回复
    /// </summary>
    MessageReply = 2,

    /// <summary>
    /// 系统通知
    /// </summary>
    System = 3,

    /// <summary>
    /// 安全提醒
    /// </summary>
    Security = 4,

    /// <summary>
    /// 活动通知
    /// </summary>
    Activity = 5,

    /// <summary>
    /// 会话邀请
    /// </summary>
    ConversationInvitation = 6,

    /// <summary>
    /// 用户加入会话
    /// </summary>
    UserJoinedConversation = 7,

    /// <summary>
    /// 用户离开会话
    /// </summary>
    UserLeftConversation = 8
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
    Urgent = 3
}

/// <summary>
/// 通知状态枚举
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// 未读
    /// </summary>
    Unread = 0,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 1,

    /// <summary>
    /// 已处理
    /// </summary>
    Processed = 2,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 3,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 4
}

/// <summary>
/// 通知操作类型枚举
/// </summary>
public enum NotificationActionType
{
    /// <summary>
    /// 默认
    /// </summary>
    Default = 0,

    /// <summary>
    /// 主要
    /// </summary>
    Primary = 1,

    /// <summary>
    /// 次要
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// 成功
    /// </summary>
    Success = 3,

    /// <summary>
    /// 警告
    /// </summary>
    Warning = 4,

    /// <summary>
    /// 危险
    /// </summary>
    Danger = 5,

    /// <summary>
    /// 链接
    /// </summary>
    Link = 6
}

/// <summary>
/// 实时消息类型枚举
/// </summary>
public enum RealTimeMessageType
{
    /// <summary>
    /// 新消息
    /// </summary>
    NewMessage = 0,

    /// <summary>
    /// 消息更新
    /// </summary>
    MessageUpdate = 1,

    /// <summary>
    /// 消息删除
    /// </summary>
    MessageDelete = 2,

    /// <summary>
    /// 消息已读
    /// </summary>
    MessageRead = 3,

    /// <summary>
    /// 用户加入会话
    /// </summary>
    UserJoined = 4,

    /// <summary>
    /// 用户离开会话
    /// </summary>
    UserLeft = 5,

    /// <summary>
    /// 会话更新
    /// </summary>
    ConversationUpdate = 6,

    /// <summary>
    /// 打字状态
    /// </summary>
    TypingStatus = 7,

    /// <summary>
    /// 用户在线状态
    /// </summary>
    UserOnlineStatus = 8
}

/// <summary>
/// 回执类型枚举
/// </summary>
public enum ReceiptType
{
    /// <summary>
    /// 送达回执
    /// </summary>
    Delivered = 0,

    /// <summary>
    /// 已读回执
    /// </summary>
    Read = 1,

    /// <summary>
    /// 显示回执
    /// </summary>
    Displayed = 2,

    /// <summary>
    /// 点击回执
    /// </summary>
    Clicked = 3
}

/// <summary>
/// 回执状态枚举
/// </summary>
public enum ReceiptStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 1,

    /// <summary>
    /// 已送达
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// 已处理
    /// </summary>
    Processed = 3,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 4
}

/// <summary>
/// 设备类型枚举
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Web浏览器
    /// </summary>
    Web = 0,

    /// <summary>
    /// 移动设备
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// 桌面应用
    /// </summary>
    Desktop = 2,

    /// <summary>
    /// 平板设备
    /// </summary>
    Tablet = 3,

    /// <summary>
    /// 其他设备
    /// </summary>
    Other = 4
}

/// <summary>
/// 推送类型枚举
/// </summary>
public enum PushType
{
    /// <summary>
    /// 消息推送
    /// </summary>
    Message = 0,

    /// <summary>
    /// 通知推送
    /// </summary>
    Notification = 1,

    /// <summary>
    /// 系统推送
    /// </summary>
    System = 2,

    /// <summary>
    /// 营销推送
    /// </summary>
    Marketing = 3
}

/// <summary>
/// 推送平台枚举
/// </summary>
public enum PushPlatform
{
    /// <summary>
    /// Web推送
    /// </summary>
    Web = 0,

    /// <summary>
    /// iOS推送
    /// </summary>
    iOS = 1,

    /// <summary>
    /// Android推送
    /// </summary>
    Android = 2,

    /// <summary>
    /// Windows推送
    /// </summary>
    Windows = 3,

    /// <summary>
    /// 通用推送
    /// </summary>
    Universal = 4
}

/// <summary>
/// 推送状态枚举
/// </summary>
public enum PushStatus
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
    /// 已点击
    /// </summary>
    Clicked = 4,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 5,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 6
}

/// <summary>
/// 推送优先级枚举
/// </summary>
public enum PushPriority
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
    High = 2
}

/// <summary>
/// 用户在线状态枚举
/// </summary>
public enum UserOnlineStatus
{
    /// <summary>
    /// 在线
    /// </summary>
    Online = 0,

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
    DoNotDisturb = 4,

    /// <summary>
    /// 隐身
    /// </summary>
    Invisible = 5
}

/// <summary>
/// 打字状态类型枚举
/// </summary>
public enum TypingStatusType
{
    /// <summary>
    /// 打字中
    /// </summary>
    Typing = 0,

    /// <summary>
    /// 停止打字
    /// </summary>
    Stopped = 1,

    /// <summary>
    /// 正在录音
    /// </summary>
    Recording = 2,

    /// <summary>
    /// 正在上传
    /// </summary>
    Uploading = 3
}