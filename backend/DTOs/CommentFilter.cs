using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 评论筛选参数 - 提供灵活的评论查询能力
/// </summary>
public class CommentFilter
{
    /// <summary>
    /// 代码片段ID筛选
    /// </summary>
    public Guid? SnippetId { get; set; }
    
    /// <summary>
    /// 用户ID筛选
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// 评论状态筛选
    /// </summary>
    public CommentStatus? Status { get; set; }
    
    /// <summary>
    /// 父评论ID筛选（null表示查询根评论）
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 评论深度筛选
    /// </summary>
    public int? MinDepth { get; set; }
    
    /// <summary>
    /// 评论深度筛选
    /// </summary>
    public int? MaxDepth { get; set; }
    
    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 搜索关键词（在评论内容中搜索）
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 最小点赞数筛选
    /// </summary>
    public int? MinLikeCount { get; set; }
    
    /// <summary>
    /// 最大点赞数筛选
    /// </summary>
    public int? MaxLikeCount { get; set; }
    
    /// <summary>
    /// 最小回复数筛选
    /// </summary>
    public int? MinReplyCount { get; set; }
    
    /// <summary>
    /// 最大回复数筛选
    /// </summary>
    public int? MaxReplyCount { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public CommentSort SortBy { get; set; } = CommentSort.CreatedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含用户信息
    /// </summary>
    public bool IncludeUser { get; set; } = true;
    
    /// <summary>
    /// 是否包含回复
    /// </summary>
    public bool IncludeReplies { get; set; } = false;
    
    /// <summary>
    /// 是否包含点赞信息
    /// </summary>
    public bool IncludeLikes { get; set; } = false;
    
    /// <summary>
    /// 是否包含举报信息
    /// </summary>
    public bool IncludeReports { get; set; } = false;
    
    /// <summary>
    /// 当前用户ID（用于检查点赞状态和权限）
    /// </summary>
    public Guid? CurrentUserId { get; set; }
}

/// <summary>
/// 评论举报筛选参数
/// </summary>
public class CommentReportFilter
{
    /// <summary>
    /// 举报状态筛选
    /// </summary>
    public ReportStatus? Status { get; set; }
    
    /// <summary>
    /// 举报原因筛选
    /// </summary>
    public ReportReason? Reason { get; set; }
    
    /// <summary>
    /// 评论ID筛选
    /// </summary>
    public Guid? CommentId { get; set; }
    
    /// <summary>
    /// 用户ID筛选（举报者）
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// 处理人ID筛选
    /// </summary>
    public Guid? HandledBy { get; set; }
    
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
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public ReportSort SortBy { get; set; } = ReportSort.CreatedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含评论内容
    /// </summary>
    public bool IncludeComment { get; set; } = true;
    
    /// <summary>
    /// 是否包含举报者信息
    /// </summary>
    public bool IncludeReporter { get; set; } = true;
    
    /// <summary>
    /// 是否包含处理人信息
    /// </summary>
    public bool IncludeHandler { get; set; } = true;
    
    /// <summary>
    /// 优先级筛选（高优先级：举报次数多或待处理时间长）
    /// </summary>
    public bool? HighPriorityOnly { get; set; }
}

/// <summary>
/// 评论点赞筛选参数
/// </summary>
public class CommentLikeFilter
{
    /// <summary>
    /// 评论ID筛选
    /// </summary>
    public Guid? CommentId { get; set; }
    
    /// <summary>
    /// 用户ID筛选
    /// </summary>
    public Guid? UserId { get; set; }
    
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
    public LikeSort SortBy { get; set; } = LikeSort.CreatedAtDesc;
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 是否包含用户信息
    /// </summary>
    public bool IncludeUser { get; set; } = true;
    
    /// <summary>
    /// 是否包含评论信息
    /// </summary>
    public bool IncludeComment { get; set; } = true;
}

/// <summary>
/// 评论统计筛选参数
/// </summary>
public class CommentStatsFilter
{
    /// <summary>
    /// 代码片段ID列表
    /// </summary>
    public IEnumerable<Guid>? SnippetIds { get; set; }
    
    /// <summary>
    /// 用户ID列表
    /// </summary>
    public IEnumerable<Guid>? UserIds { get; set; }
    
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
    /// 是否按代码片段统计
    /// </summary>
    public bool GroupBySnippet { get; set; } = false;
}

/// <summary>
/// 评论批量操作参数
/// </summary>
public class CommentBatchOperation
{
    /// <summary>
    /// 评论ID列表
    /// </summary>
    public required IEnumerable<Guid> CommentIds { get; set; }
    
    /// <summary>
    /// 操作类型
    /// </summary>
    public CommentBatchOperationType Operation { get; set; }
    
    /// <summary>
    /// 操作原因（可选）
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 操作人ID
    /// </summary>
    public required Guid OperatorId { get; set; }
    
    /// <summary>
    /// 是否发送通知
    /// </summary>
    public bool SendNotification { get; set; } = true;
}

/// <summary>
/// 评论批量操作类型
/// </summary>
public enum CommentBatchOperationType
{
    /// <summary>
    /// 删除评论
    /// </summary>
    Delete = 0,
    
    /// <summary>
    /// 隐藏评论
    /// </summary>
    Hide = 1,
    
    /// <summary>
    /// 显示评论
    /// </summary>
    Show = 2,
    
    /// <summary>
    /// 批准评论
    /// </summary>
    Approve = 3,
    
    /// <summary>
    /// 拒绝评论
    /// </summary>
    Reject = 4,
    
    /// <summary>
    /// 移动到回收站
    /// </summary>
    MoveToTrash = 5,
    
    /// <summary>
    /// 恢复评论
    /// </summary>
    Restore = 6
}

/// <summary>
/// 评论点赞排序方式
/// </summary>
public enum LikeSort
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
    /// 按用户ID排序
    /// </summary>
    UserId = 2
}