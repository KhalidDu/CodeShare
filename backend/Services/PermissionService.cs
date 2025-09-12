using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 权限管理服务实现 - 遵循里氏替换原则
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly ICodeSnippetRepository _snippetRepository;

    public PermissionService(
        IUserRepository userRepository,
        ICodeSnippetRepository snippetRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _snippetRepository = snippetRepository ?? throw new ArgumentNullException(nameof(snippetRepository));
    }

    /// <summary>
    /// 检查用户是否有权限访问代码片段
    /// </summary>
    public async Task<bool> CanAccessSnippetAsync(Guid userId, Guid snippetId, PermissionOperation operation)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            return false;
        }

        // 管理员拥有所有权限
        if (user.Role == UserRole.Admin)
        {
            return true;
        }

        // 代码片段创建者拥有所有权限
        if (snippet.CreatedBy == userId)
        {
            return true;
        }

        // 根据操作类型和用户角色检查权限
        return operation switch
        {
            PermissionOperation.Read => snippet.IsPublic || user.Role != UserRole.Viewer,
            PermissionOperation.Copy => snippet.IsPublic || user.Role != UserRole.Viewer,
            PermissionOperation.Edit => user.Role == UserRole.Editor && snippet.IsPublic,
            PermissionOperation.Delete => false, // 只有创建者和管理员可以删除
            _ => false
        };
    }

    /// <summary>
    /// 检查用户是否有管理员权限
    /// </summary>
    public async Task<bool> IsAdminAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive && user.Role == UserRole.Admin;
    }

    /// <summary>
    /// 检查用户是否有编辑权限
    /// </summary>
    public async Task<bool> CanEditAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive && 
               (user.Role == UserRole.Admin || user.Role == UserRole.Editor);
    }

    /// <summary>
    /// 检查用户是否可以创建代码片段
    /// </summary>
    public async Task<bool> CanCreateSnippetAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive && 
               (user.Role == UserRole.Admin || user.Role == UserRole.Editor);
    }

    /// <summary>
    /// 检查用户是否可以删除代码片段
    /// </summary>
    public async Task<bool> CanDeleteSnippetAsync(Guid userId, Guid snippetId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        // 管理员可以删除任何代码片段
        if (user.Role == UserRole.Admin)
        {
            return true;
        }

        // 检查是否为代码片段的创建者
        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        return snippet != null && snippet.CreatedBy == userId;
    }

    /// <summary>
    /// 检查用户是否有指定权限
    /// </summary>
    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        // 管理员拥有所有权限
        if (user.Role == UserRole.Admin)
        {
            return true;
        }

        // 根据权限名称检查具体权限
        return permission switch
        {
            "read" => true, // 所有活跃用户都有读权限
            "create" => user.Role == UserRole.User || user.Role == UserRole.Admin,
            "edit" => user.Role == UserRole.User || user.Role == UserRole.Admin,
            "delete" => user.Role == UserRole.Admin,
            _ => false
        };
    }

    /// <summary>
    /// 检查用户是否可以创建评论
    /// </summary>
    public async Task<bool> CanCreateCommentAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive;
    }

    /// <summary>
    /// 检查用户是否可以举报评论
    /// </summary>
    public async Task<bool> CanReportCommentAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null && user.IsActive;
    }
}