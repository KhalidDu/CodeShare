using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 评论DTO - 用于API响应
/// </summary>
public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SnippetId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentPath { get; set; }
    public int Depth { get; set; }
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public CommentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReport { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

/// <summary>
/// 创建评论请求DTO
/// </summary>
public class CreateCommentDto
{
    [Required(ErrorMessage = "评论内容不能为空")]
    [StringLength(2000, ErrorMessage = "评论内容长度不能超过2000个字符")]
    [MinLength(1, ErrorMessage = "评论内容至少包含1个字符")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "代码片段ID不能为空")]
    public Guid SnippetId { get; set; }

    public Guid? ParentId { get; set; }
}

/// <summary>
/// 更新评论请求DTO
/// </summary>
public class UpdateCommentDto
{
    [Required(ErrorMessage = "评论内容不能为空")]
    [StringLength(2000, ErrorMessage = "评论内容长度不能超过2000个字符")]
    [MinLength(1, ErrorMessage = "评论内容至少包含1个字符")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 评论点赞请求DTO
/// </summary>
public class CommentLikeDto
{
    /// <summary>
    /// 评论ID
    /// </summary>
    public Guid CommentId { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 点赞时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 评论审核请求DTO
/// </summary>
public class ModerateCommentDto
{
    /// <summary>
    /// 审核状态
    /// </summary>
    [Required(ErrorMessage = "审核状态不能为空")]
    public CommentStatus Status { get; set; }

    /// <summary>
    /// 审核原因
    /// </summary>
    [StringLength(500, ErrorMessage = "审核原因长度不能超过500个字符")]
    public string? Reason { get; set; }
}

/// <summary>
/// 批量操作结果DTO
/// </summary>
public class BatchOperationResultDto
{
    /// <summary>
    /// 总数
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();
    
    /// <summary>
    /// 成功的ID列表
    /// </summary>
    public List<Guid> SuccessfulIds { get; set; } = new();
    
    /// <summary>
    /// 失败的ID列表
    /// </summary>
    public List<Guid> FailedIds { get; set; } = new();
}

/// <summary>
/// 创建评论举报请求DTO
/// </summary>
public class CreateCommentReportDto
{
    [Required(ErrorMessage = "评论ID不能为空")]
    public Guid CommentId { get; set; }

    [Required(ErrorMessage = "举报原因不能为空")]
    public ReportReason Reason { get; set; }

    [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
    public string? Description { get; set; }
}

/// <summary>
/// 评论举报响应DTO
/// </summary>
public class CommentReportDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? HandledAt { get; set; }
    public string? HandlerName { get; set; }
    public string? Resolution { get; set; }
}

/// <summary>
/// 评论筛选条件DTO
/// </summary>
public class CommentFilterDto
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
    [StringLength(100, ErrorMessage = "搜索关键词长度不能超过100个字符")]
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public CommentSort SortBy { get; set; } = CommentSort.CreatedAtDesc;
    
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
/// 评论举报筛选条件DTO
/// </summary>
public class CommentReportFilterDto
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
    [StringLength(100, ErrorMessage = "搜索关键词长度不能超过100个字符")]
    public string? Search { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public ReportSort SortBy { get; set; } = ReportSort.CreatedAtDesc;
    
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
/// 评论统计信息DTO
/// </summary>
public class CommentStatsDto
{
    public Guid SnippetId { get; set; }
    public int TotalComments { get; set; }
    public int RootComments { get; set; }
    public int ReplyComments { get; set; }
    public int TotalLikes { get; set; }
    public int ActiveUsers { get; set; }
    public DateTime? LatestCommentAt { get; set; }
}

/// <summary>
/// 处理评论举报请求DTO
/// </summary>
public class HandleCommentReportDto
{
    [Required(ErrorMessage = "处理结果不能为空")]
    public ReportStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "处理说明长度不能超过500个字符")]
    public string? Resolution { get; set; }
}

/// <summary>
/// 批量操作评论请求DTO
/// </summary>
public class BatchCommentOperationDto
{
    /// <summary>
    /// 评论ID列表
    /// </summary>
    [Required(ErrorMessage = "评论ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一个评论")]
    [MaxLength(100, ErrorMessage = "一次最多操作100个评论")]
    public List<Guid> CommentIds { get; set; } = new();

    /// <summary>
    /// 操作类型
    /// </summary>
    [Required(ErrorMessage = "操作类型不能为空")]
    public CommentOperation Operation { get; set; }

    /// <summary>
    /// 操作原因（可选）
    /// </summary>
    [StringLength(500, ErrorMessage = "操作原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 操作人ID
    /// </summary>
    public Guid OperatorId { get; set; }
}

/// <summary>
/// 评论操作类型枚举
/// </summary>
public enum CommentOperation
{
    Delete = 0,
    Hide = 1,
    Show = 2,
    Approve = 3
}

/// <summary>
/// 评论排序枚举
/// </summary>
public enum CommentSort
{
    CreatedAtDesc = 0,
    CreatedAtAsc = 1,
    LikeCountDesc = 2,
    LikeCountAsc = 3,
    ReplyCountDesc = 4,
    ReplyCountAsc = 5
}

/// <summary>
/// 评论举报排序枚举
/// </summary>
public enum ReportSort
{
    CreatedAtDesc = 0,
    CreatedAtAsc = 1,
    Reason = 2,
    Status = 3
}

/// <summary>
/// 评论点赞响应DTO - 包含用户信息
/// </summary>
public class CommentLikeResponseDto
{
    /// <summary>
    /// 点赞ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 评论ID
    /// </summary>
    public Guid CommentId { get; set; }
    
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
    /// 点赞时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 用户是否为当前登录用户
    /// </summary>
    public bool IsCurrentUser { get; set; }
}

/// <summary>
/// 评论统计详细信息DTO
/// </summary>
public class CommentStatsDetailDto
{
    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }
    
    /// <summary>
    /// 评论总数
    /// </summary>
    public int TotalComments { get; set; }
    
    /// <summary>
    /// 根评论数
    /// </summary>
    public int RootComments { get; set; }
    
    /// <summary>
    /// 回复评论数
    /// </summary>
    public int ReplyComments { get; set; }
    
    /// <summary>
    /// 点赞总数
    /// </summary>
    public int TotalLikes { get; set; }
    
    /// <summary>
    /// 活跃用户数
    /// </summary>
    public int ActiveUsers { get; set; }
    
    /// <summary>
    /// 最新评论时间
    /// </summary>
    public DateTime? LatestCommentAt { get; set; }
    
    /// <summary>
    /// 平均评论长度
    /// </summary>
    public double AverageCommentLength { get; set; }
    
    /// <summary>
    /// 每日评论统计
    /// </summary>
    public List<DailyCommentStatsDto> DailyStats { get; set; } = new();
    
    /// <summary>
    /// 用户评论统计
    /// </summary>
    public List<UserCommentStatsDto> UserStats { get; set; } = new();
    
    /// <summary>
    /// 状态分布统计
    /// </summary>
    public Dictionary<CommentStatus, int> StatusDistribution { get; set; } = new();
}



/// <summary>
/// 评论简略信息DTO - 用于列表展示
/// </summary>
public class CommentSummaryDto
{
    /// <summary>
    /// 评论ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 评论内容摘要
    /// </summary>
    public string ContentSummary { get; set; } = string.Empty;
    
    /// <summary>
    /// 评论长度
    /// </summary>
    public int ContentLength { get; set; }
    
    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }
    
    /// <summary>
    /// 代码片段标题
    /// </summary>
    public string SnippetTitle { get; set; } = string.Empty;
    
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
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 回复数
    /// </summary>
    public int ReplyCount { get; set; }
    
    /// <summary>
    /// 评论状态
    /// </summary>
    public CommentStatus Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 当前用户是否已点赞
    /// </summary>
    public bool IsLikedByCurrentUser { get; set; }
}

/// <summary>
/// 评论导出DTO - 用于导出功能
/// </summary>
public class CommentExportDto
{
    /// <summary>
    /// 评论ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 评论内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }
    
    /// <summary>
    /// 代码片段标题
    /// </summary>
    public string SnippetTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// 代码片段语言
    /// </summary>
    public string SnippetLanguage { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// 父评论ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 评论深度
    /// </summary>
    public int Depth { get; set; }
    
    /// <summary>
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 回复数
    /// </summary>
    public int ReplyCount { get; set; }
    
    /// <summary>
    /// 评论状态
    /// </summary>
    public CommentStatus Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// 评论搜索结果DTO
/// </summary>
public class CommentSearchResultDto
{
    /// <summary>
    /// 评论ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 评论内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 高亮内容
    /// </summary>
    public string HighlightedContent { get; set; } = string.Empty;
    
    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }
    
    /// <summary>
    /// 代码片段标题
    /// </summary>
    public string SnippetTitle { get; set; } = string.Empty;
    
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
    /// 点赞数
    /// </summary>
    public int LikeCount { get; set; }
    
    /// <summary>
    /// 回复数
    /// </summary>
    public int ReplyCount { get; set; }
    
    /// <summary>
    /// 评论状态
    /// </summary>
    public CommentStatus Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 搜索分数
    /// </summary>
    public double SearchScore { get; set; }
    
    /// <summary>
    /// 匹配的关键词
    /// </summary>
    public List<string> MatchedKeywords { get; set; } = new();
}

/// <summary>
/// 评论搜索筛选DTO
/// </summary>
public class CommentSearchFilterDto
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    [Required(ErrorMessage = "搜索关键词不能为空")]
    [StringLength(200, ErrorMessage = "搜索关键词长度不能超过200个字符")]
    public string Keyword { get; set; } = string.Empty;
    
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
    public CommentSearchScope SearchScope { get; set; } = CommentSearchScope.All;
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public CommentSearchSort SortBy { get; set; } = CommentSearchSort.Relevance;
    
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
/// 评论搜索范围枚举
/// </summary>
public enum CommentSearchScope
{
    /// <summary>
    /// 搜索所有内容
    /// </summary>
    All = 0,
    
    /// <summary>
    /// 仅搜索评论内容
    /// </summary>
    Content = 1,
    
    /// <summary>
    /// 仅搜索用户名
    /// </summary>
    UserName = 2,
    
    /// <summary>
    /// 仅搜索代码片段标题
    /// </summary>
    SnippetTitle = 3
}

/// <summary>
/// 评论搜索排序枚举
/// </summary>
public enum CommentSearchSort
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
    /// 按点赞数降序
    /// </summary>
    LikeCountDesc = 3,
    
    /// <summary>
    /// 按回复数降序
    /// </summary>
    ReplyCountDesc = 4
}