using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 消息实体 - 遵循单一职责原则，只负责消息数据
/// </summary>
public class Message
{
    /// <summary>
    /// 消息唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 发送者用户ID
    /// </summary>
    [Required(ErrorMessage = "发送者ID不能为空")]
    public Guid SenderId { get; set; }

    /// <summary>
    /// 接收者用户ID
    /// </summary>
    [Required(ErrorMessage = "接收者ID不能为空")]
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// 消息主题
    /// </summary>
    [StringLength(200, ErrorMessage = "主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required(ErrorMessage = "消息内容不能为空")]
    [StringLength(2000, ErrorMessage = "消息内容长度不能超过2000个字符")]
    [MinLength(1, ErrorMessage = "消息内容至少包含1个字符")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息类型
    /// </summary>
    [Required(ErrorMessage = "消息类型不能为空")]
    public MessageType MessageType { get; set; }

    /// <summary>
    /// 消息状态
    /// </summary>
    [Required(ErrorMessage = "消息状态不能为空")]
    public MessageStatus Status { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 父消息ID（用于消息回复）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID（用于消息分组）
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 消息标签（用于分类）
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 删除时间（软删除）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 发送者删除时间
    /// </summary>
    public DateTime? SenderDeletedAt { get; set; }

    /// <summary>
    /// 接收者删除时间
    /// </summary>
    public DateTime? ReceiverDeletedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 发送者IP地址
    /// </summary>
    [StringLength(45, ErrorMessage = "IP地址长度不能超过45个字符")]
    public string? SenderIp { get; set; }

    /// <summary>
    /// 发送者User-Agent
    /// </summary>
    [StringLength(500, ErrorMessage = "User-Agent长度不能超过500个字符")]
    public string? SenderUserAgent { get; set; }

    // 导航属性
    /// <summary>
    /// 发送者用户信息
    /// </summary>
    public User Sender { get; set; } = null!;

    /// <summary>
    /// 接收者用户信息
    /// </summary>
    public User Receiver { get; set; } = null!;

    /// <summary>
    /// 父消息（用于回复）
    /// </summary>
    public Message? Parent { get; set; }

    /// <summary>
    /// 回复消息列表
    /// </summary>
    public List<Message> Replies { get; set; } = new();

    /// <summary>
    /// 消息附件列表
    /// </summary>
    public List<MessageAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// 消息附件实体 - 记录消息附件信息
/// </summary>
public class MessageAttachment
{
    /// <summary>
    /// 附件唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的消息ID
    /// </summary>
    [Required(ErrorMessage = "消息ID不能为空")]
    public Guid MessageId { get; set; }

    /// <summary>
    /// 附件文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    [StringLength(255, ErrorMessage = "文件名长度不能超过255个字符")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 附件原始文件名
    /// </summary>
    [Required(ErrorMessage = "原始文件名不能为空")]
    [StringLength(255, ErrorMessage = "原始文件名长度不能超过255个字符")]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "文件大小必须大于等于0")]
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [StringLength(100, ErrorMessage = "文件类型长度不能超过100个字符")]
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [StringLength(10, ErrorMessage = "文件扩展名长度不能超过10个字符")]
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件存储路径
    /// </summary>
    [Required(ErrorMessage = "文件路径不能为空")]
    [StringLength(500, ErrorMessage = "文件路径长度不能超过500个字符")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件访问URL
    /// </summary>
    [StringLength(1000, ErrorMessage = "文件URL长度不能超过1000个字符")]
    public string? FileUrl { get; set; }

    /// <summary>
    /// 附件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 附件状态
    /// </summary>
    public AttachmentStatus AttachmentStatus { get; set; } = AttachmentStatus.Active;

    /// <summary>
    /// 文件哈希值（用于验证文件完整性）
    /// </summary>
    [StringLength(64, ErrorMessage = "文件哈希值长度不能超过64个字符")]
    public string? FileHash { get; set; }

    /// <summary>
    /// 缩略图路径
    /// </summary>
    [StringLength(500, ErrorMessage = "缩略图路径长度不能超过500个字符")]
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// 上传进度（0-100）
    /// </summary>
    [Range(0, 100, ErrorMessage = "上传进度必须在0-100之间")]
    public int UploadProgress { get; set; } = 0;

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; } = 0;

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后下载时间
    /// </summary>
    public DateTime? LastDownloadedAt { get; set; }

    /// <summary>
    /// 删除时间（软删除）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // 导航属性
    /// <summary>
    /// 关联的消息
    /// </summary>
    public Message Message { get; set; } = null!;
}

/// <summary>
/// 消息草稿实体 - 记录用户未发送的消息草稿
/// </summary>
public class MessageDraft
{
    /// <summary>
    /// 草稿唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 草稿作者用户ID
    /// </summary>
    [Required(ErrorMessage = "作者ID不能为空")]
    public Guid AuthorId { get; set; }

    /// <summary>
    /// 收件人用户ID（如果是私信）
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 消息主题
    /// </summary>
    [StringLength(200, ErrorMessage = "主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "消息内容长度不能超过2000个字符")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; } = MessageType.User;

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 父消息ID（用于回复草稿）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID（用于会话草稿）
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 草稿状态
    /// </summary>
    public DraftStatus Status { get; set; } = DraftStatus.Draft;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后自动保存时间
    /// </summary>
    public DateTime? LastAutoSavedAt { get; set; }

    /// <summary>
    /// 计划发送时间
    /// </summary>
    public DateTime? ScheduledToSendAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 是否定时发送
    /// </summary>
    public bool IsScheduled { get; set; } = false;

    /// <summary>
    /// 自动保存间隔（秒）
    /// </summary>
    [Range(30, 3600, ErrorMessage = "自动保存间隔必须在30-3600秒之间")]
    public int AutoSaveInterval { get; set; } = 120;

    /// <summary>
    /// 草稿备注
    /// </summary>
    [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
    public string? Notes { get; set; }

    // 导航属性
    /// <summary>
    /// 草稿作者用户信息
    /// </summary>
    public User Author { get; set; } = null!;

    /// <summary>
    /// 收件人用户信息
    /// </summary>
    public User? Receiver { get; set; }

    /// <summary>
    /// 父消息（用于回复草稿）
    /// </summary>
    public Message? Parent { get; set; }

    /// <summary>
    /// 草稿附件列表
    /// </summary>
    public List<MessageDraftAttachment> DraftAttachments { get; set; } = new();
}

/// <summary>
/// 消息草稿附件实体 - 记录草稿的附件信息
/// </summary>
public class MessageDraftAttachment
{
    /// <summary>
    /// 草稿附件唯一标识符
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的草稿ID
    /// </summary>
    [Required(ErrorMessage = "草稿ID不能为空")]
    public Guid DraftId { get; set; }

    /// <summary>
    /// 附件文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    [StringLength(255, ErrorMessage = "文件名长度不能超过255个字符")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 附件原始文件名
    /// </summary>
    [Required(ErrorMessage = "原始文件名不能为空")]
    [StringLength(255, ErrorMessage = "原始文件名长度不能超过255个字符")]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "文件大小必须大于等于0")]
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [StringLength(100, ErrorMessage = "文件类型长度不能超过100个字符")]
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [StringLength(10, ErrorMessage = "文件扩展名长度不能超过10个字符")]
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件存储路径
    /// </summary>
    [Required(ErrorMessage = "文件路径不能为空")]
    [StringLength(500, ErrorMessage = "文件路径长度不能超过500个字符")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件访问URL
    /// </summary>
    [StringLength(1000, ErrorMessage = "文件URL长度不能超过1000个字符")]
    public string? FileUrl { get; set; }

    /// <summary>
    /// 附件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 附件状态
    /// </summary>
    public AttachmentStatus AttachmentStatus { get; set; } = AttachmentStatus.Active;

    /// <summary>
    /// 上传进度（0-100）
    /// </summary>
    [Range(0, 100, ErrorMessage = "上传进度必须在0-100之间")]
    public int UploadProgress { get; set; } = 0;

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 删除时间（软删除）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // 导航属性
    /// <summary>
    /// 关联的草稿
    /// </summary>
    public MessageDraft Draft { get; set; } = null!;
}

/// <summary>
/// 消息类型枚举
/// </summary>
public enum MessageType
{
    /// <summary>
    /// 用户消息
    /// </summary>
    User = 0,

    /// <summary>
    /// 系统消息
    /// </summary>
    System = 1,

    /// <summary>
    /// 通知消息
    /// </summary>
    Notification = 2,

    /// <summary>
    /// 广播消息
    /// </summary>
    Broadcast = 3,

    /// <summary>
    /// 自动回复消息
    /// </summary>
    AutoReply = 4
}

/// <summary>
/// 消息状态枚举
/// </summary>
public enum MessageStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 1,

    /// <summary>
    /// 已送达
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// 已读
    /// </summary>
    Read = 3,

    /// <summary>
    /// 已回复
    /// </summary>
    Replied = 4,

    /// <summary>
    /// 已转发
    /// </summary>
    Forwarded = 5,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 6,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 7,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 8
}

/// <summary>
/// 消息优先级枚举
/// </summary>
public enum MessagePriority
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
/// 附件类型枚举
/// </summary>
public enum AttachmentType
{
    /// <summary>
    /// 图片文件
    /// </summary>
    Image = 0,

    /// <summary>
    /// 文档文件
    /// </summary>
    Document = 1,

    /// <summary>
    /// 视频文件
    /// </summary>
    Video = 2,

    /// <summary>
    /// 音频文件
    /// </summary>
    Audio = 3,

    /// <summary>
    /// 压缩文件
    /// </summary>
    Archive = 4,

    /// <summary>
    /// 代码文件
    /// </summary>
    Code = 5,

    /// <summary>
    /// 其他文件
    /// </summary>
    Other = 99
}

/// <summary>
/// 附件状态枚举
/// </summary>
public enum AttachmentStatus
{
    /// <summary>
    /// 活跃状态
    /// </summary>
    Active = 0,

    /// <summary>
    /// 上传中
    /// </summary>
    Uploading = 1,

    /// <summary>
    /// 上传失败
    /// </summary>
    UploadFailed = 2,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 3,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 4,

    /// <summary>
    /// 病毒检测中
    /// </summary>
    VirusScanning = 5,

    /// <summary>
    /// 检测到病毒
    /// </summary>
    VirusDetected = 6
}

/// <summary>
/// 草稿状态枚举
/// </summary>
public enum DraftStatus
{
    /// <summary>
    /// 草稿状态
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 1,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 3
}