using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 消息草稿筛选参数 - 提供灵活的草稿查询能力
/// </summary>
public class MessageDraftFilterDto
{
    /// <summary>
    /// 作者用户ID筛选
    /// </summary>
    public Guid? AuthorId { get; set; }
    
    /// <summary>
    /// 收件人用户ID筛选
    /// </summary>
    public Guid? ReceiverId { get; set; }
    
    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public MessageType? MessageType { get; set; }
    
    /// <summary>
    /// 消息优先级筛选
    /// </summary>
    public MessagePriority? Priority { get; set; }
    
    /// <summary>
    /// 草稿状态筛选
    /// </summary>
    public DraftStatus? Status { get; set; }
    
    /// <summary>
    /// 父消息ID筛选（用于回复草稿）
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
    /// 是否定时发送筛选
    /// </summary>
    public bool? IsScheduled { get; set; }
    
    /// <summary>
    /// 计划发送时间筛选（开始）
    /// </summary>
    public DateTime? ScheduledStartDate { get; set; }
    
    /// <summary>
    /// 计划发送时间筛选（结束）
    /// </summary>
    public DateTime? ScheduledEndDate { get; set; }
    
    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 过期时间筛选（开始）
    /// </summary>
    public DateTime? ExpiresStartDate { get; set; }
    
    /// <summary>
    /// 过期时间筛选（结束）
    /// </summary>
    public DateTime? ExpiresEndDate { get; set; }
    
    /// <summary>
    /// 搜索关键词（在主题和内容中搜索）
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public DraftSort SortBy { get; set; } = DraftSort.UpdatedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含作者信息
    /// </summary>
    public bool IncludeAuthor { get; set; } = true;
    
    /// <summary>
    /// 是否包含收件人信息
    /// </summary>
    public bool IncludeReceiver { get; set; } = true;
    
    /// <summary>
    /// 是否包含草稿附件
    /// </summary>
    public bool IncludeAttachments { get; set; } = false;
    
    /// <summary>
    /// 是否包含父消息信息
    /// </summary>
    public bool IncludeParent { get; set; } = false;
    
    /// <summary>
    /// 当前用户ID（用于权限检查）
    /// </summary>
    public Guid? CurrentUserId { get; set; }
}

/// <summary>
/// 消息草稿排序方式枚举
/// </summary>
public enum DraftSort
{
    /// <summary>
    /// 按更新时间降序
    /// </summary>
    UpdatedAtDesc = 0,
    
    /// <summary>
    /// 按更新时间升序
    /// </summary>
    UpdatedAtAsc = 1,
    
    /// <summary>
    /// 按创建时间降序
    /// </summary>
    CreatedAtDesc = 2,
    
    /// <summary>
    /// 按创建时间升序
    /// </summary>
    CreatedAtAsc = 3,
    
    /// <summary>
    /// 按优先级降序
    /// </summary>
    PriorityDesc = 4,
    
    /// <summary>
    /// 按优先级升序
    /// </summary>
    PriorityAsc = 5,
    
    /// <summary>
    /// 按计划发送时间升序（最早的优先）
    /// </summary>
    ScheduledToSendAtAsc = 6,
    
    /// <summary>
    /// 按计划发送时间降序
    /// </summary>
    ScheduledToSendAtDesc = 7,
    
    /// <summary>
    /// 按自动保存时间降序
    /// </summary>
    LastAutoSavedAtDesc = 8,
    
    /// <summary>
    /// 按草稿状态排序
    /// </summary>
    Status = 9
}

/// <summary>
/// 消息统计查询参数
/// </summary>
public class MessageStatsFilterDto
{
    /// <summary>
    /// 用户ID列表
    /// </summary>
    public IEnumerable<Guid>? UserIds { get; set; }
    
    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public MessageType? MessageType { get; set; }
    
    /// <summary>
    /// 消息状态筛选
    /// </summary>
    public MessageStatus? Status { get; set; }
    
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 是否按天统计
    /// </summary>
    public bool GroupByDay { get; set; } = false;
    
    /// <summary>
    /// 是否按用户统计
    /// </summary>
    public bool GroupByUser { get; set; } = false;
    
    /// <summary>
    /// 是否按消息类型统计
    /// </summary>
    public bool GroupByMessageType { get; set; } = false;
    
    /// <summary>
    /// 是否按消息状态统计
    /// </summary>
    public bool GroupByStatus { get; set; } = false;
    
    /// <summary>
    /// 是否包含附件统计
    /// </summary>
    public bool IncludeAttachmentStats { get; set; } = false;
    
    /// <summary>
    /// 是否包含会话统计
    /// </summary>
    public bool IncludeConversationStats { get; set; } = false;
}