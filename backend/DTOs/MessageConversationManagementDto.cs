using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 创建消息会话请求DTO
/// </summary>
public class CreateMessageConversationRequestDto
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
    [StringLength(1000, ErrorMessage = "会话描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 会话类型
    /// </summary>
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
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsPinned { get; set; } = false;

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool IsMuted { get; set; } = false;

    /// <summary>
    /// 会话图标
    /// </summary>
    [StringLength(500, ErrorMessage = "会话图标URL长度不能超过500个字符")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 会话颜色
    /// </summary>
    [StringLength(7, ErrorMessage = "会话颜色长度不能超过7个字符")]
    public string? Color { get; set; }

    /// <summary>
    /// 自定义设置
    /// </summary>
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// 更新消息会话请求DTO
/// </summary>
public class UpdateMessageConversationRequestDto
{
    /// <summary>
    /// 会话标题
    /// </summary>
    [StringLength(200, ErrorMessage = "会话标题长度不能超过200个字符")]
    public string? Title { get; set; }

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "会话描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 会话标签
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool? IsPinned { get; set; }

    /// <summary>
    /// 是否静音
    /// </summary>
    public bool? IsMuted { get; set; }

    /// <summary>
    /// 会话图标
    /// </summary>
    [StringLength(500, ErrorMessage = "会话图标URL长度不能超过500个字符")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 会话颜色
    /// </summary>
    [StringLength(7, ErrorMessage = "会话颜色长度不能超过7个字符")]
    public string? Color { get; set; }

    /// <summary>
    /// 自定义设置
    /// </summary>
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// 添加会话参与者请求DTO
/// </summary>
public class AddConversationParticipantsRequestDto
{
    /// <summary>
    /// 要添加的用户ID列表
    /// </summary>
    [Required(ErrorMessage = "用户ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要添加一个用户")]
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// 参与者角色
    /// </summary>
    public ParticipantRole Role { get; set; } = ParticipantRole.Member;

    /// <summary>
    /// 添加原因
    /// </summary>
    [StringLength(500, ErrorMessage = "添加原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否发送欢迎消息
    /// </summary>
    public bool SendWelcomeMessage { get; set; } = true;

    /// <summary>
    /// 欢迎消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "欢迎消息长度不能超过2000个字符")]
    public string? WelcomeMessage { get; set; }
}

/// <summary>
/// 移除会话参与者请求DTO
/// </summary>
public class RemoveConversationParticipantsRequestDto
{
    /// <summary>
    /// 要移除的用户ID列表
    /// </summary>
    [Required(ErrorMessage = "用户ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要移除一个用户")]
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// 移除原因
    /// </summary>
    [StringLength(500, ErrorMessage = "移除原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否发送离开消息
    /// </summary>
    public bool SendLeaveMessage { get; set; } = true;

    /// <summary>
    /// 离开消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "离开消息长度不能超过2000个字符")]
    public string? LeaveMessage { get; set; }
}

/// <summary>
/// 更新会话参与者角色请求DTO
/// </summary>
public class UpdateConversationParticipantRoleRequestDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 新角色
    /// </summary>
    [Required(ErrorMessage = "角色不能为空")]
    public ParticipantRole NewRole { get; set; }

    /// <summary>
    /// 更新原因
    /// </summary>
    [StringLength(500, ErrorMessage = "更新原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否发送通知消息
    /// </summary>
    public bool SendNotificationMessage { get; set; } = true;

    /// <summary>
    /// 通知消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "通知消息长度不能超过2000个字符")]
    public string? NotificationMessage { get; set; }
}

/// <summary>
/// 会话消息列表请求DTO
/// </summary>
public class ConversationMessagesRequestDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    [Required(ErrorMessage = "会话ID不能为空")]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页数量必须在1-100之间")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序方式
    /// </summary>
    public MessageSort SortBy { get; set; } = MessageSort.CreatedAtDesc;

    /// <summary>
    /// 基准消息ID（用于分页）
    /// </summary>
    public Guid? AnchorMessageId { get; set; }

    /// <summary>
    /// 是否包含已删除的消息
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// 是否包含附件信息
    /// </summary>
    public bool IncludeAttachments { get; set; } = false;

    /// <summary>
    /// 是否包含回复消息
    /// </summary>
    public bool IncludeReplies { get; set; } = false;

    /// <summary>
    /// 消息类型筛选
    /// </summary>
    public MessageType? MessageType { get; set; }

    /// <summary>
    /// 发送者ID筛选
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }
}

/// <summary>
/// 会话消息列表响应DTO
/// </summary>
public class ConversationMessagesResponseDto
{
    /// <summary>
    /// 会话信息
    /// </summary>
    public MessageConversationDto Conversation { get; set; } = new();

    /// <summary>
    /// 消息列表
    /// </summary>
    public List<MessageDto> Messages { get; set; } = new();

    /// <summary>
    /// 分页信息
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();

    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// 最后阅读的消息ID
    /// </summary>
    public Guid? LastReadMessageId { get; set; }

    /// <summary>
    /// 最后阅读时间
    /// </summary>
    public DateTime? LastReadAt { get; set; }
}

/// <summary>
/// 分页信息DTO
/// </summary>
public class PaginationInfo
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 上一页页码
    /// </summary>
    public int? PreviousPage { get; set; }

    /// <summary>
    /// 下一页页码
    /// </summary>
    public int? NextPage { get; set; }
}

/// <summary>
/// 标记会话消息为已读请求DTO
/// </summary>
public class MarkConversationMessagesAsReadRequestDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    [Required(ErrorMessage = "会话ID不能为空")]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 消息ID列表（空表示标记所有未读消息）
    /// </summary>
    public List<Guid>? MessageIds { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否发送已读回执
    /// </summary>
    public bool SendReadReceipt { get; set; } = true;
}

/// <summary>
/// 会话统计信息DTO
/// </summary>
public class ConversationStatsDto
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 总消息数
    /// </summary>
    public int TotalMessages { get; set; }

    /// <summary>
    /// 今日消息数
    /// </summary>
    public int TodayMessages { get; set; }

    /// <summary>
    /// 本周消息数
    /// </summary>
    public int ThisWeekMessages { get; set; }

    /// <summary>
    /// 本月消息数
    /// </summary>
    public int ThisMonthMessages { get; set; }

    /// <summary>
    /// 参与者数量
    /// </summary>
    public int ParticipantCount { get; set; }

    /// <summary>
    /// 活跃参与者数量
    /// </summary>
    public int ActiveParticipants { get; set; }

    /// <summary>
    /// 总附件数量
    /// </summary>
    public int TotalAttachments { get; set; }

    /// <summary>
    /// 总附件大小（字节）
    /// </summary>
    public long TotalAttachmentSize { get; set; }

    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 按日期统计的消息数量
    /// </summary>
    public Dictionary<DateTime, int> DailyMessageStats { get; set; } = new();

    /// <summary>
    /// 按用户统计的消息数量
    /// </summary>
    public Dictionary<Guid, UserMessageStatsDto> UserMessageStats { get; set; } = new();

    /// <summary>
    /// 按消息类型统计
    /// </summary>
    public Dictionary<MessageType, int> MessageTypeStats { get; set; } = new();

    /// <summary>
    /// 按优先级统计
    /// </summary>
    public Dictionary<MessagePriority, int> PriorityStats { get; set; } = new();
}