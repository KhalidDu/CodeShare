namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 评论实体 - 遵循单一职责原则，只负责评论数据
/// </summary>
public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SnippetId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentPath { get; set; }
    public int Depth { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public CommentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // 导航属性
    public User User { get; set; } = null!;
    public CodeSnippet Snippet { get; set; } = null!;
    public Comment? Parent { get; set; }
    public List<Comment> Replies { get; set; } = new();
    public List<CommentLike> Likes { get; set; } = new();
    public List<CommentReport> Reports { get; set; } = new();
}

/// <summary>
/// 评论点赞实体 - 记录用户对评论的点赞行为
/// </summary>
public class CommentLike
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 导航属性
    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}

/// <summary>
/// 评论举报实体 - 记录用户对评论的举报行为
/// </summary>
public class CommentReport
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? HandledAt { get; set; }
    public Guid? HandledBy { get; set; }
    public string? Resolution { get; set; }
    
    // 导航属性
    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? Handler { get; set; }
}

/// <summary>
/// 评论状态枚举
/// </summary>
public enum CommentStatus
{
    /// <summary>
    /// 正常状态
    /// </summary>
    Normal = 0,
    
    /// <summary>
    /// 已被软删除
    /// </summary>
    Deleted = 1,
    
    /// <summary>
    /// 已被隐藏（管理员操作）
    /// </summary>
    Hidden = 2,
    
    /// <summary>
    /// 待审核
    /// </summary>
    Pending = 3
}

/// <summary>
/// 举报原因枚举
/// </summary>
public enum ReportReason
{
    /// <summary>
    /// 垃圾信息
    /// </summary>
    Spam = 0,
    
    /// <summary>
    /// 不当内容
    /// </summary>
    Inappropriate = 1,
    
    /// <summary>
    /// 侮辱性言论
    /// </summary>
    Harassment = 2,
    
    /// <summary>
    /// 仇恨言论
    /// </summary>
    HateSpeech = 3,
    
    /// <summary>
    /// 虚假信息
    /// </summary>
    Misinformation = 4,
    
    /// <summary>
    /// 侵权内容
    /// </summary>
    CopyrightViolation = 5,
    
    /// <summary>
    /// 其他原因
    /// </summary>
    Other = 99
}

/// <summary>
/// 举报状态枚举
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// 已处理
    /// </summary>
    Resolved = 1,
    
    /// <summary>
    /// 已驳回
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// 正在调查
    /// </summary>
    UnderInvestigation = 3
}