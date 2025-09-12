using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 权限管理服务接口 - 遵循里氏替换原则
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 检查用户是否有权限访问代码片段
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="operation">操作类型</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanAccessSnippetAsync(Guid userId, Guid snippetId, PermissionOperation operation);

    /// <summary>
    /// 检查用户是否有管理员权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否为管理员</returns>
    Task<bool> IsAdminAsync(Guid userId);

    /// <summary>
    /// 检查用户是否有编辑权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有编辑权限</returns>
    Task<bool> CanEditAsync(Guid userId);

    /// <summary>
    /// 检查用户是否可以创建代码片段
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以创建</returns>
    Task<bool> CanCreateSnippetAsync(Guid userId);

    /// <summary>
    /// 检查用户是否可以删除代码片段
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>是否可以删除</returns>
    Task<bool> CanDeleteSnippetAsync(Guid userId, Guid snippetId);

    /// <summary>
    /// 检查用户是否有指定权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="permission">权限名称</param>
    /// <returns>是否有权限</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permission);

    /// <summary>
    /// 检查用户是否可以创建评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以创建评论</returns>
    Task<bool> CanCreateCommentAsync(Guid userId);

    /// <summary>
    /// 检查用户是否可以举报评论
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以举报评论</returns>
    Task<bool> CanReportCommentAsync(Guid userId);
}

/// <summary>
/// 权限操作枚举
/// </summary>
public enum PermissionOperation
{
    Read,
    Edit,
    Delete,
    Copy
}