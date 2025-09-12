using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论服务接口 - 遵循接口隔离原则和单一职责原则
/// 提供评论相关的业务逻辑操作
/// </summary>
public interface ICommentService
{
    // 基础评论管理操作
    #region 基础评论管理操作

    /// <summary>
    /// 创建新评论
    /// </summary>
    /// <param name="createCommentDto">创建评论请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的评论DTO</returns>
    Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, Guid userId);

    /// <summary>
    /// 更新评论内容
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="updateCommentDto">更新评论请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的评论DTO</returns>
    Task<CommentDto> UpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto, Guid userId);

    /// <summary>
    /// 删除评论（软删除）
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 获取评论详情
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>评论DTO</returns>
    Task<CommentDto?> GetCommentAsync(Guid commentId, Guid? currentUserId = null);

    /// <summary>
    /// 根据ID获取评论详情（别名方法）
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>评论DTO</returns>
    Task<CommentDto?> GetCommentByIdAsync(Guid commentId, Guid? currentUserId = null);

    /// <summary>
    /// 分页获取评论列表
    /// </summary>
    /// <param name="filter">评论筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetCommentsAsync(CommentFilterDto filter, Guid? currentUserId = null);

    /// <summary>
    /// 分页获取评论列表（别名方法）
    /// </summary>
    /// <param name="filter">评论筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetCommentsPaginatedAsync(CommentFilterDto filter, Guid? currentUserId = null);

    /// <summary>
    /// 获取代码片段的评论树结构
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>评论树结构</returns>
    Task<IEnumerable<CommentDto>> GetCommentTreeAsync(Guid snippetId, Guid? currentUserId = null);

    /// <summary>
    /// 获取评论的回复列表
    /// </summary>
    /// <param name="parentId">父评论ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>回复列表</returns>
    Task<IEnumerable<CommentDto>> GetRepliesAsync(Guid parentId, Guid? currentUserId = null);

    /// <summary>
    /// 获取用户的评论列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">评论筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>用户评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetUserCommentsAsync(Guid userId, CommentFilterDto filter, Guid? currentUserId = null);

    #endregion

    // 评论点赞管理
    #region 评论点赞管理

    /// <summary>
    /// 点赞评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否点赞成功</returns>
    Task<bool> LikeCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 取消点赞评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否取消点赞成功</returns>
    Task<bool> UnlikeCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 获取评论的点赞列表
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>点赞列表</returns>
    Task<PaginatedResult<CommentLikeDto>> GetCommentLikesAsync(Guid commentId, int page = 1, int pageSize = 20);

    /// <summary>
    /// 获取用户的点赞历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>用户点赞历史</returns>
    Task<PaginatedResult<CommentLikeDto>> GetUserLikedCommentsAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// 检查用户是否已点赞评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否已点赞</returns>
    Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId);

    #endregion

    // 评论举报管理
    #region 评论举报管理

    /// <summary>
    /// 举报评论
    /// </summary>
    /// <param name="createReportDto">创建举报请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>举报ID</returns>
    Task<Guid> ReportCommentAsync(CreateCommentReportDto createReportDto, Guid userId);

    /// <summary>
    /// 获取评论举报详情
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <returns>举报详情</returns>
    Task<CommentReportDto?> GetReportAsync(Guid reportId);

    /// <summary>
    /// 分页获取评论举报列表
    /// </summary>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>举报列表</returns>
    Task<PaginatedResult<CommentReportDto>> GetReportsAsync(CommentReportFilterDto filter);

    /// <summary>
    /// 处理评论举报
    /// </summary>
    /// <param name="reportId">举报ID</param>
    /// <param name="handleReportDto">处理举报请求</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>是否处理成功</returns>
    Task<bool> HandleReportAsync(Guid reportId, HandleCommentReportDto handleReportDto, Guid handlerId);

    /// <summary>
    /// 批量处理评论举报
    /// </summary>
    /// <param name="reportIds">举报ID列表</param>
    /// <param name="status">处理状态</param>
    /// <param name="resolution">处理说明</param>
    /// <param name="handlerId">处理人ID</param>
    /// <returns>是否批量处理成功</returns>
    Task<bool> BulkHandleReportsAsync(IEnumerable<Guid> reportIds, ReportStatus status, string? resolution, Guid handlerId);

    /// <summary>
    /// 获取用户的举报历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">举报筛选条件</param>
    /// <returns>用户举报历史</returns>
    Task<PaginatedResult<CommentReportDto>> GetUserReportsAsync(Guid userId, CommentReportFilterDto filter);

    /// <summary>
    /// 检查用户是否已举报评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否已举报</returns>
    Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId);

    #endregion

    // 评论审核管理
    #region 评论审核管理

    /// <summary>
    /// 审核评论（通过/拒绝）
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="approved">是否通过</param>
    /// <param name="reason">审核原因</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>是否审核成功</returns>
    Task<bool> ModerateCommentAsync(Guid commentId, bool approved, string? reason, Guid moderatorId);

    /// <summary>
    /// 批量审核评论
    /// </summary>
    /// <param name="batchOperationDto">批量操作请求</param>
    /// <param name="moderatorId">审核人ID</param>
    /// <returns>是否批量审核成功</returns>
    Task<bool> BulkModerateCommentsAsync(BatchCommentOperationDto batchOperationDto, Guid moderatorId);

    /// <summary>
    /// 获取待审核评论列表
    /// </summary>
    /// <param name="filter">评论筛选条件</param>
    /// <returns>待审核评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetPendingCommentsAsync(CommentFilterDto filter);

    /// <summary>
    /// 获取被隐藏评论列表
    /// </summary>
    /// <param name="filter">评论筛选条件</param>
    /// <returns>被隐藏评论列表</returns>
    Task<PaginatedResult<CommentDto>> GetHiddenCommentsAsync(CommentFilterDto filter);

    /// <summary>
    /// 显示/隐藏评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="hide">是否隐藏</param>
    /// <param name="moderatorId">操作人ID</param>
    /// <param name="reason">操作原因</param>
    /// <returns>是否操作成功</returns>
    Task<bool> SetCommentVisibilityAsync(Guid commentId, bool hide, Guid moderatorId, string? reason);

    #endregion

    // 评论统计分析
    #region 评论统计分析

    /// <summary>
    /// 获取评论统计信息
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>评论统计信息</returns>
    Task<CommentStatsDto> GetCommentStatsAsync(Guid snippetId);

    /// <summary>
    /// 获取用户评论统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户评论统计信息</returns>
    Task<UserCommentStatsDto> GetUserCommentStatsAsync(Guid userId);

    /// <summary>
    /// 获取全局评论统计信息
    /// </summary>
    /// <returns>全局评论统计信息</returns>
    Task<GlobalCommentStatsDto> GetGlobalCommentStatsAsync();

    /// <summary>
    /// 获取评论趋势统计
    /// </summary>
    /// <param name="days">统计天数</param>
    /// <returns>评论趋势统计</returns>
    Task<IEnumerable<CommentTrendDto>> GetCommentTrendAsync(int days = 30);

    /// <summary>
    /// 获取最热门评论
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="count">获取数量</param>
    /// <returns>最热门评论列表</returns>
    Task<IEnumerable<CommentDto>> GetTopCommentsAsync(Guid snippetId, int count = 5);

    /// <summary>
    /// 获取评论活动报告
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>评论活动报告</returns>
    Task<CommentActivityReportDto> GetCommentActivityReportAsync(DateTime startDate, DateTime endDate);

    #endregion

    // 搜索和筛选
    #region 搜索和筛选

    /// <summary>
    /// 搜索评论
    /// </summary>
    /// <param name="searchTerm">搜索词</param>
    /// <param name="filter">评论筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>搜索结果</returns>
    Task<PaginatedResult<CommentDto>> SearchCommentsAsync(string searchTerm, CommentFilterDto filter, Guid? currentUserId = null);

    /// <summary>
    /// 获取最新评论
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="count">获取数量</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>最新评论列表</returns>
    Task<IEnumerable<CommentDto>> GetLatestCommentsAsync(Guid snippetId, int count = 5, Guid? currentUserId = null);

    /// <summary>
    /// 获取最多点赞的评论
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="count">获取数量</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>最多点赞的评论列表</returns>
    Task<IEnumerable<CommentDto>> GetMostLikedCommentsAsync(Guid snippetId, int count = 5, Guid? currentUserId = null);

    #endregion

    // 缓存管理
    #region 缓存管理

    /// <summary>
    /// 清除评论缓存
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearCommentCacheAsync(Guid commentId);

    /// <summary>
    /// 清除代码片段评论缓存
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearSnippetCommentCacheAsync(Guid snippetId);

    /// <summary>
    /// 清除用户评论缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearUserCommentCacheAsync(Guid userId);

    /// <summary>
    /// 预热评论缓存
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>是否预热成功</returns>
    Task<bool> WarmupCommentCacheAsync(Guid snippetId);

    #endregion
}


/// <summary>
/// 评论活动报告DTO
/// </summary>
public class CommentActivityReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    public int TotalReports { get; set; }
    public int ActiveUsers { get; set; }
    public double AverageCommentsPerDay { get; set; }
    public double AverageLikesPerComment { get; set; }
    public int TopCommentersCount { get; set; }
    public int MostActiveDay { get; set; }
    public IEnumerable<UserCommentActivityDto> TopUsers { get; set; } = new List<UserCommentActivityDto>();
    public IEnumerable<CommentActivityByDateDto> ActivityByDate { get; set; } = new List<CommentActivityByDateDto>();
}

/// <summary>
/// 用户评论活动DTO
/// </summary>
public class UserCommentActivityDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public int ReportCount { get; set; }
    public double ActivityScore { get; set; }
}

/// <summary>
/// 按日期统计的评论活动DTO
/// </summary>
public class CommentActivityByDateDto
{
    public DateTime Date { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public int UserCount { get; set; }
    public int ReportCount { get; set; }
}