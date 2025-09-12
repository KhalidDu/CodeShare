using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 评论验证服务接口 - 遵循单一职责原则
/// 提供评论相关的验证逻辑
/// </summary>
public interface ICommentValidationService
{
    // 基础验证
    #region 基础验证

    /// <summary>
    /// 验证创建评论请求
    /// </summary>
    /// <param name="createCommentDto">创建评论请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateCreateCommentAsync(CreateCommentDto createCommentDto, Guid userId);

    /// <summary>
    /// 验证更新评论请求
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="updateCommentDto">更新评论请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateUpdateCommentAsync(Guid commentId, UpdateCommentDto updateCommentDto, Guid userId);

    /// <summary>
    /// 验证删除评论请求
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateDeleteCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 验证评论内容
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateCommentContent(string content);

    /// <summary>
    /// 验证评论长度
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateCommentLength(string content);

    #endregion

    // 权限验证
    #region 权限验证

    /// <summary>
    /// 验证用户是否有权限编辑评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanEditCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 验证用户是否有权限删除评论
    /// </summary>
    /// <param name="commentId">评论ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanDeleteCommentAsync(Guid commentId, Guid userId);

    /// <summary>
    /// 验证用户是否有权限管理评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanManageCommentsAsync(Guid userId);

    /// <summary>
    /// 验证用户是否有权限审核评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanModerateCommentsAsync(Guid userId);

    /// <summary>
    /// 验证用户是否有权限处理举报
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanHandleReportsAsync(Guid userId);

    #endregion

    // 内容验证
    #region 内容验证

    /// <summary>
    /// 验证评论是否包含敏感内容
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>是否包含敏感内容</returns>
    Task<bool> ContainsSensitiveContentAsync(string content);

    /// <summary>
    /// 验证评论是否包含垃圾信息
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>是否包含垃圾信息</returns>
    Task<bool> IsSpamContentAsync(string content);

    /// <summary>
    /// 验证评论是否包含链接
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>是否包含链接</returns>
    bool ContainsLinks(string content);

    /// <summary>
    /// 验证评论是否包含邮箱地址
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>是否包含邮箱地址</returns>
    bool ContainsEmails(string content);

    /// <summary>
    /// 验证评论是否包含电话号码
    /// </summary>
    /// <param name="content">评论内容</param>
    /// <returns>是否包含电话号码</returns>
    bool ContainsPhoneNumbers(string content);

    #endregion

    // 重复性验证
    #region 重复性验证

    /// <summary>
    /// 验证是否为重复评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="content">评论内容</param>
    /// <param name="timeWindowMinutes">时间窗口（分钟）</param>
    /// <returns>是否为重复评论</returns>
    Task<bool> IsDuplicateCommentAsync(Guid userId, Guid snippetId, string content, int timeWindowMinutes = 5);

    /// <summary>
    /// 验证用户评论频率
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="timeWindowMinutes">时间窗口（分钟）</param>
    /// <param name="maxComments">最大评论数</param>
    /// <returns>是否超过频率限制</returns>
    Task<bool> ExceedsCommentFrequencyAsync(Guid userId, int timeWindowMinutes = 10, int maxComments = 5);

    /// <summary>
    /// 验证用户点赞频率
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="timeWindowMinutes">时间窗口（分钟）</param>
    /// <param name="maxLikes">最大点赞数</param>
    /// <returns>是否超过频率限制</returns>
    Task<bool> ExceedsLikeFrequencyAsync(Guid userId, int timeWindowMinutes = 10, int maxLikes = 10);

    /// <summary>
    /// 验证用户举报频率
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="timeWindowMinutes">时间窗口（分钟）</param>
    /// <param name="maxReports">最大举报数</param>
    /// <returns>是否超过频率限制</returns>
    Task<bool> ExceedsReportFrequencyAsync(Guid userId, int timeWindowMinutes = 60, int maxReports = 3);

    #endregion

    // 关联性验证
    #region 关联性验证

    /// <summary>
    /// 验证代码片段是否存在且允许评论
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>是否有效</returns>
    Task<bool> IsValidSnippetForCommentAsync(Guid snippetId);

    /// <summary>
    /// 验证父评论是否存在且允许回复
    /// </summary>
    /// <param name="parentId">父评论ID</param>
    /// <returns>是否有效</returns>
    Task<bool> IsValidParentCommentAsync(Guid parentId);

    /// <summary>
    /// 验证评论深度限制
    /// </summary>
    /// <param name="parentId">父评论ID</param>
    /// <param name="maxDepth">最大深度</param>
    /// <returns>是否超过深度限制</returns>
    Task<bool> ExceedsDepthLimitAsync(Guid parentId, int maxDepth = 5);

    /// <summary>
    /// 验证回复链的完整性
    /// </summary>
    /// <param name="parentPath">父评论路径</param>
    /// <returns>是否完整</returns>
    Task<bool> IsValidReplyChainAsync(string parentPath);

    #endregion

    // 批量验证
    #region 批量验证

    /// <summary>
    /// 批量验证评论
    /// </summary>
    /// <param name="commentIds">评论ID列表</param>
    /// <returns>验证结果</returns>
    Task<Dictionary<Guid, ValidationResult>> BulkValidateCommentsAsync(IEnumerable<Guid> commentIds);

    /// <summary>
    /// 批量验证用户权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="commentIds">评论ID列表</param>
    /// <returns>权限验证结果</returns>
    Task<Dictionary<Guid, bool>> BulkValidateUserPermissionsAsync(Guid userId, IEnumerable<Guid> commentIds);

    /// <summary>
    /// 批量验证内容
    /// </summary>
    /// <param name="contents">评论内容列表</param>
    /// <returns>内容验证结果</returns>
    Task<Dictionary<string, ValidationResult>> BulkValidateContentAsync(IEnumerable<string> contents);

    #endregion
}

/// <summary>
/// 评论验证结果DTO
/// </summary>
public class CommentValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? ValidationDetails { get; set; }
}

/// <summary>
/// 评论权限验证结果DTO
/// </summary>
public class CommentPermissionResultDto
{
    public bool HasPermission { get; set; }
    public string? DenyReason { get; set; }
    public string? PermissionType { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsModerator { get; set; }
    public bool IsOwner { get; set; }
}

/// <summary>
/// 评论内容检查结果DTO
/// </summary>
public class CommentContentCheckResultDto
{
    public bool IsClean { get; set; }
    public bool ContainsSensitiveContent { get; set; }
    public bool IsSpam { get; set; }
    public bool ContainsLinks { get; set; }
    public bool ContainsEmails { get; set; }
    public bool ContainsPhoneNumbers { get; set; }
    public double SpamScore { get; set; }
    public List<string> DetectedIssues { get; set; } = new();
    public List<string> SensitiveWords { get; set; } = new();
}

/// <summary>
/// 评论频率验证结果DTO
/// </summary>
public class CommentFrequencyResultDto
{
    public bool IsWithinLimit { get; set; }
    public int CurrentCount { get; set; }
    public int MaxAllowed { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public DateTime? ResetTime { get; set; }
    public string? LimitType { get; set; }
}