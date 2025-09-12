using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息DTO - 用于API响应
/// </summary>
public class MessageDto
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// 发送者用户名
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// 发送者头像
    /// </summary>
    public string? SenderAvatar { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// 接收者用户名
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// 接收者头像
    /// </summary>
    public string? ReceiverAvatar { get; set; }

    /// <summary>
    /// 消息主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
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
    /// 父消息ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

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
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 附件列表
    /// </summary>
    public List<MessageAttachmentDto> Attachments { get; set; } = new();

    /// <summary>
    /// 回复消息列表
    /// </summary>
    public List<MessageDto> Replies { get; set; } = new();

    /// <summary>
    /// 当前用户是否是发送者
    /// </summary>
    public bool IsSender { get; set; }

    /// <summary>
    /// 当前用户是否是接收者
    /// </summary>
    public bool IsReceiver { get; set; }

    /// <summary>
    /// 是否可以编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可以删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 是否可以回复
    /// </summary>
    public bool CanReply { get; set; }

    /// <summary>
    /// 是否可以转发
    /// </summary>
    public bool CanForward { get; set; }
}

/// <summary>
/// 创建消息请求DTO
/// </summary>
public class CreateMessageDto
{
    /// <summary>
    /// 接收者ID
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
    public MessageType MessageType { get; set; } = MessageType.User;

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 父消息ID（回复时使用）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 附件ID列表
    /// </summary>
    public List<Guid>? AttachmentIds { get; set; }
}

/// <summary>
/// 更新消息请求DTO
/// </summary>
public class UpdateMessageDto
{
    /// <summary>
    /// 消息主题
    /// </summary>
    [StringLength(200, ErrorMessage = "主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "消息内容长度不能超过2000个字符")]
    [MinLength(1, ErrorMessage = "消息内容至少包含1个字符")]
    public string? Content { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority? Priority { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// 消息附件DTO
/// </summary>
public class MessageAttachmentDto
{
    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 附件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 附件状态
    /// </summary>
    public AttachmentStatus AttachmentStatus { get; set; }

    /// <summary>
    /// 缩略图URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 上传进度
    /// </summary>
    public int UploadProgress { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// 消息草稿DTO
/// </summary>
public class MessageDraftDto
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 作者ID
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 接收者用户名
    /// </summary>
    public string? ReceiverName { get; set; }

    /// <summary>
    /// 消息主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority Priority { get; set; }

    /// <summary>
    /// 父消息ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 草稿状态
    /// </summary>
    public DraftStatus Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

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
    public bool IsScheduled { get; set; }

    /// <summary>
    /// 附件列表
    /// </summary>
    public List<MessageDraftAttachmentDto> DraftAttachments { get; set; } = new();
}

/// <summary>
/// 创建消息草稿请求DTO
/// </summary>
public class CreateMessageDraftDto
{
    /// <summary>
    /// 接收者ID
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
    /// 父消息ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

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
    /// 自动保存间隔
    /// </summary>
    [Range(30, 3600, ErrorMessage = "自动保存间隔必须在30-3600秒之间")]
    public int AutoSaveInterval { get; set; } = 120;
}

/// <summary>
/// 更新消息草稿请求DTO
/// </summary>
public class UpdateMessageDraftDto
{
    /// <summary>
    /// 接收者ID
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
    public string? Content { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority? Priority { get; set; }

    /// <summary>
    /// 父消息ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 消息标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

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
    public bool? IsScheduled { get; set; }

    /// <summary>
    /// 自动保存间隔
    /// </summary>
    [Range(30, 3600, ErrorMessage = "自动保存间隔必须在30-3600秒之间")]
    public int? AutoSaveInterval { get; set; }
}

/// <summary>
/// 消息草稿附件DTO
/// </summary>
public class MessageDraftAttachmentDto
{
    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 附件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 附件状态
    /// </summary>
    public AttachmentStatus AttachmentStatus { get; set; }

    /// <summary>
    /// 上传进度
    /// </summary>
    public int UploadProgress { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadedAt { get; set; }
}


/// <summary>
/// 简单消息统计信息DTO
/// </summary>
public class SimpleMessageStatsDto
{
    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadMessages { get; set; }

    /// <summary>
    /// 已发送消息数
    /// </summary>
    public int SentMessages { get; set; }

    /// <summary>
    /// 已接收消息数
    /// </summary>
    public int ReceivedMessages { get; set; }

    /// <summary>
    /// 已删除消息数
    /// </summary>
    public int DeletedMessages { get; set; }

    /// <summary>
    /// 总附件数
    /// </summary>
    public int TotalAttachments { get; set; }

    /// <summary>
    /// 总附件大小（字节）
    /// </summary>
    public long TotalAttachmentSize { get; set; }

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
}


/// <summary>
/// 批量操作消息请求DTO
/// </summary>
public class BatchMessageOperationDto
{
    /// <summary>
    /// 消息ID列表
    /// </summary>
    [Required(ErrorMessage = "消息ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一条消息")]
    public List<Guid> MessageIds { get; set; } = new();

    /// <summary>
    /// 操作类型
    /// </summary>
    [Required(ErrorMessage = "操作类型不能为空")]
    public MessageOperation Operation { get; set; }

    /// <summary>
    /// 操作原因
    /// </summary>
    [StringLength(500, ErrorMessage = "操作原因长度不能超过500个字符")]
    public string? Reason { get; set; }
}


/// <summary>
/// 消息搜索结果DTO
/// </summary>
public class MessageSearchResultDto
{
    /// <summary>
    /// 消息列表
    /// </summary>
    public List<MessageDto> Messages { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 高亮关键词
    /// </summary>
    public List<string> HighlightedTerms { get; set; } = new();

    /// <summary>
    /// 搜索时间（毫秒）
    /// </summary>
    public long SearchTime { get; set; }
}

/// <summary>
/// 消息导出选项DTO
/// </summary>
public class MessageExportOptionsDto
{
    /// <summary>
    /// 导出格式
    /// </summary>
    [Required(ErrorMessage = "导出格式不能为空")]
    public ExportFormat Format { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType? MessageType { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 是否包含附件
    /// </summary>
    public bool IncludeAttachments { get; set; } = false;

    /// <summary>
    /// 是否包含回复
    /// </summary>
    public bool IncludeReplies { get; set; } = true;
}


/// <summary>
/// 创建会话请求DTO
/// </summary>
public class CreateConversationDto
{
    /// <summary>
    /// 会话标题
    /// </summary>
    [Required(ErrorMessage = "会话标题不能为空")]
    [StringLength(200, ErrorMessage = "会话标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(500, ErrorMessage = "会话描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 会话类型
    /// </summary>
    [Required(ErrorMessage = "会话类型不能为空")]
    public ConversationType ConversationType { get; set; } = ConversationType.Private;

    /// <summary>
    /// 参与者ID列表
    /// </summary>
    [Required(ErrorMessage = "参与者列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个参与者")]
    public List<Guid> ParticipantIds { get; set; } = new();

    /// <summary>
    /// 初始消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "初始消息内容长度不能超过2000个字符")]
    public string? InitialMessage { get; set; }

    /// <summary>
    /// 会话标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool IsMuted { get; set; } = false;
}

/// <summary>
/// 消息搜索请求DTO
/// </summary>
public class MessageSearchRequestDto
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    [Required(ErrorMessage = "搜索关键词不能为空")]
    [StringLength(100, ErrorMessage = "搜索关键词长度不能超过100个字符")]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// 发送者ID筛选
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 接收者ID筛选
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public MessageType? MessageType { get; set; }

    /// <summary>
    /// 消息状态筛选
    /// </summary>
    public MessageStatus? Status { get; set; }

    /// <summary>
    /// 优先级筛选
    /// </summary>
    public MessagePriority? Priority { get; set; }

    /// <summary>
    /// 会话ID筛选
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 标签筛选
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 是否包含附件
    /// </summary>
    public bool? HasAttachments { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索范围
    /// </summary>
    public MessageSearchScope SearchScope { get; set; } = MessageSearchScope.All;

    /// <summary>
    /// 排序方式
    /// </summary>
    public MessageSearchSort SortBy { get; set; } = MessageSearchSort.Relevance;

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
    /// 是否包含发送者信息
    /// </summary>
    public bool IncludeSender { get; set; } = true;

    /// <summary>
    /// 是否包含接收者信息
    /// </summary>
    public bool IncludeReceiver { get; set; } = true;

    /// <summary>
    /// 是否包含附件信息
    /// </summary>
    public bool IncludeAttachments { get; set; } = false;

    /// <summary>
    /// 是否高亮匹配文本
    /// </summary>
    public bool HighlightMatches { get; set; } = true;
}

/// <summary>
/// 消息搜索范围枚举
/// </summary>
public enum MessageSearchScope
{
    /// <summary>
    /// 搜索所有内容
    /// </summary>
    All = 0,

    /// <summary>
    /// 仅搜索主题
    /// </summary>
    Subject = 1,

    /// <summary>
    /// 仅搜索内容
    /// </summary>
    Content = 2,

    /// <summary>
    /// 仅搜索标签
    /// </summary>
    Tag = 3,

    /// <summary>
    /// 搜索主题和内容
    /// </summary>
    SubjectAndContent = 4
}

/// <summary>
/// 消息搜索排序枚举
/// </summary>
public enum MessageSearchSort
{
    /// <summary>
    /// 按相关性排序
    /// </summary>
    Relevance = 0,

    /// <summary>
    /// 按创建时间降序
    /// </summary>
    CreatedAtDesc = 1,

    /// <summary>
    /// 按创建时间升序
    /// </summary>
    CreatedAtAsc = 2,

    /// <summary>
    /// 按优先级降序
    /// </summary>
    PriorityDesc = 3,

    /// <summary>
    /// 按优先级升序
    /// </summary>
    PriorityAsc = 4,

    /// <summary>
    /// 按未读状态优先排序
    /// </summary>
    UnreadFirst = 5
}

/// <summary>
/// 发送消息请求DTO
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// 接收者用户ID列表
    /// </summary>
    [Required(ErrorMessage = "接收者不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个接收者")]
    [MaxLength(100, ErrorMessage = "接收者数量不能超过100个")]
    public List<Guid> RecipientIds { get; set; } = new();

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required(ErrorMessage = "消息内容不能为空")]
    [StringLength(10000, ErrorMessage = "消息内容长度不能超过10000个字符")]
    [MinLength(1, ErrorMessage = "消息内容至少包含1个字符")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息主题
    /// </summary>
    [StringLength(200, ErrorMessage = "主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; } = MessageType.User;

    /// <summary>
    /// 消息优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 附件列表
    /// </summary>
    public List<Guid>? AttachmentIds { get; set; }

    /// <summary>
    /// 定时发送时间
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 回复消息请求DTO
/// </summary>
public class ReplyMessageRequest
{
    /// <summary>
    /// 原消息ID
    /// </summary>
    [Required(ErrorMessage = "原消息ID不能为空")]
    public Guid OriginalMessageId { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    [Required(ErrorMessage = "回复内容不能为空")]
    [StringLength(10000, ErrorMessage = "回复内容长度不能超过10000个字符")]
    [MinLength(1, ErrorMessage = "回复内容至少包含1个字符")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 回复主题
    /// </summary>
    [StringLength(200, ErrorMessage = "主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 消息优先级
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;

    /// <summary>
    /// 附件列表
    /// </summary>
    public List<Guid>? AttachmentIds { get; set; }

    /// <summary>
    /// 定时发送时间
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 转发消息请求DTO
/// </summary>
public class ForwardMessageRequest
{
    /// <summary>
    /// 原消息ID
    /// </summary>
    [Required(ErrorMessage = "原消息ID不能为空")]
    public Guid OriginalMessageId { get; set; }

    /// <summary>
    /// 目标会话ID列表
    /// </summary>
    [Required(ErrorMessage = "目标会话不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个目标会话")]
    [MaxLength(10, ErrorMessage = "转发目标不能超过10个")]
    public List<Guid> TargetConversationIds { get; set; } = new();

    /// <summary>
    /// 转发说明
    /// </summary>
    [StringLength(500, ErrorMessage = "转发说明长度不能超过500个字符")]
    public string? ForwardNote { get; set; }

    /// <summary>
    /// 是否包含原附件
    /// </summary>
    public bool IncludeAttachments { get; set; } = true;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量消息请求DTO
/// </summary>
public class BulkMessageRequest
{
    /// <summary>
    /// 消息ID列表
    /// </summary>
    [Required(ErrorMessage = "消息ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个消息")]
    [MaxLength(100, ErrorMessage = "一次最多操作100个消息")]
    public List<Guid> MessageIds { get; set; } = new();

    /// <summary>
    /// 操作类型
    /// </summary>
    [Required(ErrorMessage = "操作类型不能为空")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 操作原因
    /// </summary>
    [StringLength(500, ErrorMessage = "操作原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 标记已读请求DTO
/// </summary>
public class MarkAsReadRequest
{
    /// <summary>
    /// 消息ID
    /// </summary>
    [Required(ErrorMessage = "消息ID不能为空")]
    public Guid MessageId { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量标记已读请求DTO
/// </summary>
public class BulkMarkAsReadRequest
{
    /// <summary>
    /// 消息ID列表
    /// </summary>
    [Required(ErrorMessage = "消息ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个消息")]
    [MaxLength(1000, ErrorMessage = "一次最多标记1000个消息")]
    public List<Guid> MessageIds { get; set; } = new();

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 会话ID（可选，用于标记整个会话为已读）
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 消息列表响应DTO
/// </summary>
public class MessageListResponse
{
    /// <summary>
    /// 消息列表
    /// </summary>
    public List<MessageDto> Messages { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// 统计信息
    /// </summary>
    public MessageStatsResponse? Stats { get; set; }
}

/// <summary>
/// 消息统计响应DTO
/// </summary>
public class MessageStatsResponse
{
    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadMessages { get; set; }

    /// <summary>
    /// 已发送消息数
    /// </summary>
    public int SentMessages { get; set; }

    /// <summary>
    /// 已接收消息数
    /// </summary>
    public int ReceivedMessages { get; set; }

    /// <summary>
    /// 高优先级消息数
    /// </summary>
    public int HighPriorityMessages { get; set; }

    /// <summary>
    /// 包含附件的消息数
    /// </summary>
    public int MessagesWithAttachments { get; set; }

    /// <summary>
    /// 按消息类型统计
    /// </summary>
    public Dictionary<MessageType, int> MessagesByType { get; set; } = new();

    /// <summary>
    /// 按消息状态统计
    /// </summary>
    public Dictionary<MessageStatus, int> MessagesByStatus { get; set; } = new();

    /// <summary>
    /// 按消息优先级统计
    /// </summary>
    public Dictionary<MessagePriority, int> MessagesByPriority { get; set; } = new();

    /// <summary>
    /// 最新消息时间
    /// </summary>
    public DateTime? LatestMessageAt { get; set; }
}

/// <summary>
/// 消息摘要响应DTO
/// </summary>
public class MessageSummaryResponse
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 消息内容摘要
    /// </summary>
    public string ContentSummary { get; set; } = string.Empty;

    /// <summary>
    /// 消息长度
    /// </summary>
    public int ContentLength { get; set; }

    /// <summary>
    /// 消息主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// 发送者用户名
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// 发送者头像
    /// </summary>
    public string? SenderAvatar { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// 消息状态
    /// </summary>
    public MessageStatus Status { get; set; }

    /// <summary>
    /// 消息优先级
    /// </summary>
    public MessagePriority Priority { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 是否有附件
    /// </summary>
    public bool HasAttachments { get; set; }
}

/// <summary>
/// 未读消息响应DTO
/// </summary>
public class UnreadMessageResponse
{
    /// <summary>
    /// 未读消息列表
    /// </summary>
    public List<MessageSummaryResponse> UnreadMessages { get; set; } = new();

    /// <summary>
    /// 未读消息总数
    /// </summary>
    public int TotalUnreadCount { get; set; }

    /// <summary>
    /// 按会话分组的未读消息
    /// </summary>
    public Dictionary<Guid, ConversationUnreadInfo> ConversationUnreadInfo { get; set; } = new();

    /// <summary>
    /// 高优先级未读消息
    /// </summary>
    public List<MessageSummaryResponse> HighPriorityMessages { get; set; } = new();

    /// <summary>
    /// 最后获取时间
    /// </summary>
    public DateTime FetchedAt { get; set; }
}

/// <summary>
/// 会话未读信息DTO
/// </summary>
public class ConversationUnreadInfo
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 会话标题
    /// </summary>
    public string ConversationTitle { get; set; } = string.Empty;

    /// <summary>
    /// 未读消息数量
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// 高优先级未读消息数量
    /// </summary>
    public int HighPriorityCount { get; set; }

    /// <summary>
    /// 最新未读消息
    /// </summary>
    public MessageSummaryResponse? LatestUnreadMessage { get; set; }
}

/// <summary>
/// 消息搜索响应DTO
/// </summary>
public class MessageSearchResponse
{
    /// <summary>
    /// 搜索结果列表
    /// </summary>
    public List<MessageSearchResultDto> Results { get; set; } = new();

    /// <summary>
    /// 总结果数
    /// </summary>
    public int TotalResults { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string SearchKeyword { get; set; } = string.Empty;

    /// <summary>
    /// 搜索耗时（毫秒）
    /// </summary>
    public long SearchTimeMs { get; set; }
}

/// <summary>
/// 更新会话请求DTO
/// </summary>
public class UpdateConversationRequest
{
    /// <summary>
    /// 会话标题
    /// </summary>
    [StringLength(200, ErrorMessage = "会话标题长度不能超过200个字符")]
    public string? Title { get; set; }

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(500, ErrorMessage = "会话描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsPinned { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool? IsMuted { get; set; }

    /// <summary>
    /// 会话标签
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 会话响应DTO
/// </summary>
public class ConversationResponse
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
    /// 创建者ID
    /// </summary>
    public Guid CreatorId { get; set; }

    /// <summary>
    /// 创建者用户名
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// 参与者列表
    /// </summary>
    public List<ParticipantResponse> Participants { get; set; } = new();

    /// <summary>
    /// 消息总数
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadMessageCount { get; set; }

    /// <summary>
    /// 最新消息
    /// </summary>
    public MessageSummaryResponse? LatestMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最新消息时间
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// 会话标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 会话列表响应DTO
/// </summary>
public class ConversationListResponse
{
    /// <summary>
    /// 会话列表
    /// </summary>
    public List<ConversationResponse> Conversations { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }
}

/// <summary>
/// 会话摘要响应DTO
/// </summary>
public class ConversationSummaryResponse
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
    /// 会话类型
    /// </summary>
    public ConversationType ConversationType { get; set; }

    /// <summary>
    /// 参与者数量
    /// </summary>
    public int ParticipantCount { get; set; }

    /// <summary>
    /// 消息总数
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadMessageCount { get; set; }

    /// <summary>
    /// 最新消息
    /// </summary>
    public MessageSummaryResponse? LatestMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最新消息时间
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool IsMuted { get; set; }
}

/// <summary>
/// 添加参与者请求DTO
/// </summary>
public class AddParticipantRequest
{
    /// <summary>
    /// 用户ID列表
    /// </summary>
    [Required(ErrorMessage = "用户ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个用户")]
    [MaxLength(50, ErrorMessage = "一次最多添加50个用户")]
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// 用户角色
    /// </summary>
    public string Role { get; set; } = "member";

    /// <summary>
    /// 欢迎消息
    /// </summary>
    [StringLength(1000, ErrorMessage = "欢迎消息长度不能超过1000个字符")]
    public string? WelcomeMessage { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 移除参与者请求DTO
/// </summary>
public class RemoveParticipantRequest
{
    /// <summary>
    /// 用户ID列表
    /// </summary>
    [Required(ErrorMessage = "用户ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个用户")]
    [MaxLength(50, ErrorMessage = "一次最多移除50个用户")]
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// 移除原因
    /// </summary>
    [StringLength(500, ErrorMessage = "移除原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 参与者响应DTO
/// </summary>
public class ParticipantResponse
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
    /// 用户角色
    /// </summary>
    public string Role { get; set; } = "member";

    /// <summary>
    /// 加入时间
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// 最后活跃时间
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; }
}

/// <summary>
/// 上传附件请求DTO
/// </summary>
public class UploadAttachmentRequest
{
    /// <summary>
    /// 文件名
    /// </summary>
    [Required(ErrorMessage = "文件名不能为空")]
    [StringLength(255, ErrorMessage = "文件名长度不能超过255个字符")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型
    /// </summary>
    [Required(ErrorMessage = "文件类型不能为空")]
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [Required(ErrorMessage = "文件大小不能为空")]
    [Range(1, 100 * 1024 * 1024, ErrorMessage = "文件大小必须在1字节到100MB之间")]
    public long FileSize { get; set; }

    /// <summary>
    /// 文件内容（Base64编码）
    /// </summary>
    [Required(ErrorMessage = "文件内容不能为空")]
    public string FileContent { get; set; } = string.Empty;

    /// <summary>
    /// 文件MIME类型
    /// </summary>
    [StringLength(100, ErrorMessage = "MIME类型长度不能超过100个字符")]
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件描述
    /// </summary>
    [StringLength(500, ErrorMessage = "文件描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否为公开附件
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 附件响应DTO
/// </summary>
public class AttachmentResponse
{
    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件URL
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// 文件MIME类型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 上传者ID
    /// </summary>
    public Guid UploaderId { get; set; }

    /// <summary>
    /// 上传者用户名
    /// </summary>
    public string UploaderName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为公开附件
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 文件哈希值
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// 缩略图URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 是否可下载
    /// </summary>
    public bool CanDownload { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 附件列表响应DTO
/// </summary>
public class AttachmentListResponse
{
    /// <summary>
    /// 附件列表
    /// </summary>
    public List<AttachmentResponse> Attachments { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// 总文件大小
    /// </summary>
    public long TotalFileSize { get; set; }
}

/// <summary>
/// 附件信息响应DTO
/// </summary>
public class AttachmentInfoResponse
{
    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件大小（格式化显示）
    /// </summary>
    public string FormattedFileSize { get; set; } = string.Empty;

    /// <summary>
    /// 文件MIME类型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 上传者用户名
    /// </summary>
    public string UploaderName { get; set; } = string.Empty;

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 下载次数
    /// </summary>
    public int DownloadCount { get; set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// 是否为图片
    /// </summary>
    public bool IsImage { get; set; }

    /// <summary>
    /// 是否为视频
    /// </summary>
    public bool IsVideo { get; set; }

    /// <summary>
    /// 是否为音频
    /// </summary>
    public bool IsAudio { get; set; }

    /// <summary>
    /// 是否为文档
    /// </summary>
    public bool IsDocument { get; set; }
}

/// <summary>
/// 草稿列表响应DTO
/// </summary>
public class DraftListResponse
{
    /// <summary>
    /// 草稿列表
    /// </summary>
    public List<MessageDraftDto> Drafts { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }
}

/// <summary>
/// 定时发送请求DTO
/// </summary>
public class ScheduleSendRequest
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    [Required(ErrorMessage = "草稿ID不能为空")]
    public Guid DraftId { get; set; }

    /// <summary>
    /// 定时发送时间
    /// </summary>
    [Required(ErrorMessage = "定时发送时间不能为空")]
    public DateTime ScheduledAt { get; set; }

    /// <summary>
    /// 是否立即发送（如果时间已过）
    /// </summary>
    public bool SendImmediatelyIfPast { get; set; } = false;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 消息筛选条件DTO
/// </summary>
public class MessageFilter
{
    /// <summary>
    /// 会话ID筛选
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 发送者ID筛选
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 接收者ID筛选
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public MessageType? MessageType { get; set; }

    /// <summary>
    /// 消息状态筛选
    /// </summary>
    public MessageStatus? Status { get; set; }

    /// <summary>
    /// 消息优先级筛选
    /// </summary>
    public MessagePriority? Priority { get; set; }

    /// <summary>
    /// 父消息ID筛选
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [StringLength(200, ErrorMessage = "搜索关键词长度不能超过200个字符")]
    public string? Search { get; set; }

    /// <summary>
    /// 是否包含附件
    /// </summary>
    public bool? HasAttachments { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// 排序方式
    /// </summary>
    public MessageSearchSort SortBy { get; set; } = MessageSearchSort.CreatedAtDesc;

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
}

/// <summary>
/// 消息排序选项DTO
/// </summary>
public class MessageSortOptions
{
    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "Desc";

    /// <summary>
    /// 是否启用多字段排序
    /// </summary>
    public bool EnableMultiSort { get; set; } = false;

    /// <summary>
    /// 次要排序字段
    /// </summary>
    public string? SecondarySortBy { get; set; }

    /// <summary>
    /// 次要排序方向
    /// </summary>
    public string? SecondarySortDirection { get; set; }
}

/// <summary>
/// 消息分页选项DTO
/// </summary>
public class MessagePagination
{
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
    /// 是否包含总数
    /// </summary>
    public bool IncludeTotalCount { get; set; } = true;

    /// <summary>
    /// 最大返回数量
    /// </summary>
    [Range(1, 10000, ErrorMessage = "最大返回数量必须在1-10000之间")]
    public int? MaxResults { get; set; }

    /// <summary>
    /// 游标（用于游标分页）
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 游标方向
    /// </summary>
    public string CursorDirection { get; set; } = "next";
}

/// <summary>
/// 会话筛选条件DTO
/// </summary>
public class ConversationFilter
{
    /// <summary>
    /// 会话类型筛选
    /// </summary>
    public ConversationType? ConversationType { get; set; }

    /// <summary>
    /// 创建者ID筛选
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 参与者ID筛选
    /// </summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>
    /// 是否包含当前用户
    /// </summary>
    public bool? IncludeCurrentUser { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [StringLength(200, ErrorMessage = "搜索关键词长度不能超过200个字符")]
    public string? Search { get; set; }

    /// <summary>
    /// 是否有未读消息
    /// </summary>
    public bool? HasUnreadMessages { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsPinned { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool? IsMuted { get; set; }

    /// <summary>
    /// 排序方式
    /// </summary>
    public string SortBy { get; set; } = "LastMessageAtDesc";

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
}

/// <summary>
/// 附件筛选条件DTO
/// </summary>
public class AttachmentFilter
{
    /// <summary>
    /// 文件类型筛选
    /// </summary>
    public AttachmentType? AttachmentType { get; set; }

    /// <summary>
    /// 上传者ID筛选
    /// </summary>
    public Guid? UploaderId { get; set; }

    /// <summary>
    /// 消息ID筛选
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// 会话ID筛选
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// 最小文件大小（字节）
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "最小文件大小不能为负数")]
    public long? MinFileSize { get; set; }

    /// <summary>
    /// 最大文件大小（字节）
    /// </summary>
    [Range(0, long.MaxValue, ErrorMessage = "最大文件大小不能为负数")]
    public long? MaxFileSize { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [StringLength(200, ErrorMessage = "搜索关键词长度不能超过200个字符")]
    public string? Search { get; set; }

    /// <summary>
    /// MIME类型筛选
    /// </summary>
    [StringLength(100, ErrorMessage = "MIME类型长度不能超过100个字符")]
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件扩展名筛选
    /// </summary>
    [StringLength(10, ErrorMessage = "文件扩展名长度不能超过10个字符")]
    public string? FileExtension { get; set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool? IsExpired { get; set; }

    /// <summary>
    /// 排序方式
    /// </summary>
    public string SortBy { get; set; } = "CreatedAtDesc";

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
}