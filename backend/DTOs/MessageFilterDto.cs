using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息筛选参数 - 提供灵活的消息查询能力
/// </summary>
public class MessageFilterDto
{
    /// <summary>
    /// 发送者用户ID筛选
    /// </summary>
    public Guid? SenderId { get; set; }
    
    /// <summary>
    /// 接收者用户ID筛选
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
    /// 父消息ID筛选（null表示查询根消息）
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 会话ID筛选
    /// </summary>
    public Guid? ConversationId { get; set; }
    
    /// <summary>
    /// 标签筛选
    /// </summary>
    public string? Tag { get; set; }
    
    /// <summary>
    /// 是否已读筛选
    /// </summary>
    public bool? IsRead { get; set; }
    
    /// <summary>
    /// 是否已删除筛选（null表示查询所有，true表示查询已删除，false表示查询未删除）
    /// </summary>
    public bool? IsDeleted { get; set; }
    
    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 搜索关键词（在主题和内容中搜索）
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public MessageSort SortBy { get; set; } = MessageSort.CreatedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
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
    /// 是否包含回复消息
    /// </summary>
    public bool IncludeReplies { get; set; } = false;
    
    /// <summary>
    /// 是否包含父消息信息
    /// </summary>
    public bool IncludeParent { get; set; } = false;
    
    /// <summary>
    /// 当前用户ID（用于权限检查和消息过滤）
    /// </summary>
    public Guid? CurrentUserId { get; set; }
}

/// <summary>
/// 消息排序方式枚举
/// </summary>
public enum MessageSort
{
    /// <summary>
    /// 按创建时间降序
    /// </summary>
    CreatedAtDesc = 0,
    
    /// <summary>
    /// 按创建时间升序
    /// </summary>
    CreatedAtAsc = 1,
    
    /// <summary>
    /// 按优先级降序
    /// </summary>
    PriorityDesc = 2,
    
    /// <summary>
    /// 按优先级升序
    /// </summary>
    PriorityAsc = 3,
    
    /// <summary>
    /// 按状态排序
    /// </summary>
    Status = 4,
    
    /// <summary>
    /// 按主题排序
    /// </summary>
    Subject = 5,
    
    /// <summary>
    /// 按消息类型排序
    /// </summary>
    MessageType = 6,
    
    /// <summary>
    /// 按未读状态优先排序
    /// </summary>
    UnreadFirst = 7,
    
    /// <summary>
    /// 按高优先级和未读状态优先排序
    /// </summary>
    PriorityAndUnreadFirst = 8
}

/// <summary>
/// 消息会话筛选参数
/// </summary>
public class MessageConversationFilterDto
{
    /// <summary>
    /// 用户ID筛选（参与会话的用户）
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// 会话类型筛选
    /// </summary>
    public ConversationType? ConversationType { get; set; }
    
    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 是否包含未读消息
    /// </summary>
    public bool? HasUnread { get; set; }
    
    /// <summary>
    /// 最小消息数量筛选
    /// </summary>
    public int? MinMessageCount { get; set; }
    
    /// <summary>
    /// 搜索关键词（在会话消息内容中搜索）
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public ConversationSort SortBy { get; set; } = ConversationSort.LastMessageAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含参与者信息
    /// </summary>
    public bool IncludeParticipants { get; set; } = true;
    
    /// <summary>
    /// 是否包含最新消息
    /// </summary>
    public bool IncludeLastMessage { get; set; } = true;
    
    /// <summary>
    /// 是否包含未读消息数量
    /// </summary>
    public bool IncludeUnreadCount { get; set; } = true;
}

/// <summary>
/// 消息会话排序方式枚举
/// </summary>
public enum ConversationSort
{
    /// <summary>
    /// 按最新消息时间降序
    /// </summary>
    LastMessageAtDesc = 0,
    
    /// <summary>
    /// 按最新消息时间升序
    /// </summary>
    LastMessageAtAsc = 1,
    
    /// <summary>
    /// 按创建时间降序
    /// </summary>
    CreatedAtDesc = 2,
    
    /// <summary>
    /// 按创建时间升序
    /// </summary>
    CreatedAtAsc = 3,
    
    /// <summary>
    /// 按消息数量降序
    /// </summary>
    MessageCountDesc = 4,
    
    /// <summary>
    /// 按未读消息数量降序
    /// </summary>
    UnreadCountDesc = 5
}

/// <summary>
/// 消息操作类型枚举
/// </summary>
public enum MessageOperation
{
    /// <summary>
    /// 标记为已读
    /// </summary>
    MarkAsRead = 0,
    
    /// <summary>
    /// 标记为未读
    /// </summary>
    MarkAsUnread = 1,
    
    /// <summary>
    /// 删除
    /// </summary>
    Delete = 2,
    
    /// <summary>
    /// 归档
    /// </summary>
    Archive = 3,
    
    /// <summary>
    /// 恢复
    /// </summary>
    Restore = 4,
    
    /// <summary>
    /// 置顶
    /// </summary>
    Pin = 5,
    
    /// <summary>
    /// 取消置顶
    /// </summary>
    Unpin = 6,
    
    /// <summary>
    /// 静音
    /// </summary>
    Mute = 7,
    
    /// <summary>
    /// 取消静音
    /// </summary>
    Unmute = 8
}

/// <summary>
/// 消息会话类型枚举
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// 私聊会话
    /// </summary>
    Private = 0,
    
    /// <summary>
    /// 群聊会话
    /// </summary>
    Group = 1,
    
    /// <summary>
    /// 系统会话
    /// </summary>
    System = 2
}

/// <summary>
/// 消息附件筛选参数
/// </summary>
public class MessageAttachmentFilterDto
{
    /// <summary>
    /// 消息ID筛选
    /// </summary>
    public Guid? MessageId { get; set; }
    
    /// <summary>
    /// 附件类型筛选
    /// </summary>
    public AttachmentType? AttachmentType { get; set; }
    
    /// <summary>
    /// 附件状态筛选
    /// </summary>
    public AttachmentStatus? AttachmentStatus { get; set; }
    
    /// <summary>
    /// 文件类型筛选（根据Content-Type或文件扩展名）
    /// </summary>
    public string? ContentType { get; set; }
    
    /// <summary>
    /// 文件扩展名筛选
    /// </summary>
    public string? FileExtension { get; set; }
    
    /// <summary>
    /// 最小文件大小筛选（字节）
    /// </summary>
    public long? MinFileSize { get; set; }
    
    /// <summary>
    /// 最大文件大小筛选（字节）
    /// </summary>
    public long? MaxFileSize { get; set; }
    
    /// <summary>
    /// 文件名搜索关键词
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public AttachmentSort SortBy { get; set; } = AttachmentSort.UploadedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含消息信息
    /// </summary>
    public bool IncludeMessage { get; set; } = true;
    
    /// <summary>
    /// 是否包含下载统计
    /// </summary>
    public bool IncludeDownloadStats { get; set; } = true;
}

/// <summary>
/// 消息附件排序方式枚举
/// </summary>
public enum AttachmentSort
{
    /// <summary>
    /// 按上传时间降序
    /// </summary>
    UploadedAtDesc = 0,
    
    /// <summary>
    /// 按上传时间升序
    /// </summary>
    UploadedAtAsc = 1,
    
    /// <summary>
    /// 按文件大小降序
    /// </summary>
    FileSizeDesc = 2,
    
    /// <summary>
    /// 按文件大小升序
    /// </summary>
    FileSizeAsc = 3,
    
    /// <summary>
    /// 按文件名排序
    /// </summary>
    FileName = 4,
    
    /// <summary>
    /// 按下载次数降序
    /// </summary>
    DownloadCountDesc = 5,
    
    /// <summary>
    /// 按下载次数升序
    /// </summary>
    DownloadCountAsc = 6
}