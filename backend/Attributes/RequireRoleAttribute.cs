using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Attributes;

/// <summary>
/// 基于角色的授权属性 - 遵循开闭原则，支持扩展新的角色验证
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _requiredRoles;

    public RequireRoleAttribute(params UserRole[] requiredRoles)
    {
        _requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 检查用户是否已认证
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 获取用户角色
        var userRoleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userRoleClaim) || !Enum.TryParse<UserRole>(userRoleClaim, out var userRole))
        {
            context.Result = new ForbidResult();
            return;
        }

        // 检查用户角色是否满足要求
        if (!_requiredRoles.Contains(userRole))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// 管理员权限属性
/// </summary>
public class RequireAdminAttribute : RequireRoleAttribute
{
    public RequireAdminAttribute() : base(UserRole.Admin)
    {
    }
}

/// <summary>
/// 编辑者权限属性（包括管理员）
/// </summary>
public class RequireEditorAttribute : RequireRoleAttribute
{
    public RequireEditorAttribute() : base(UserRole.Admin, UserRole.Editor)
    {
    }
}