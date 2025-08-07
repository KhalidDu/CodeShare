using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using System.Security.Claims;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 版本管理控制器 - 遵循单一职责原则
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VersionsController : ControllerBase
{
    private readonly IVersionManagementService _versionManagementService;
    private readonly IPermissionService _permissionService;

    public VersionsController(
        IVersionManagementService versionManagementService,
        IPermissionService permissionService)
    {
        _versionManagementService = versionManagementService ?? throw new ArgumentNullException(nameof(versionManagementService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    }

    /// <summary>
    /// 获取代码片段的版本历史
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>版本历史列表</returns>
    [HttpGet("snippet/{snippetId:guid}")]
    public async Task<ActionResult<IEnumerable<SnippetVersionDto>>> GetVersionHistory(Guid snippetId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // 检查用户是否有权限查看此代码片段的版本历史
            var canAccess = await _permissionService.CanAccessSnippetAsync(
                currentUserId, snippetId, PermissionOperation.Read);
            
            if (!canAccess)
            {
                return Forbid("您没有权限查看此代码片段的版本历史");
            }

            var versions = await _versionManagementService.GetVersionHistoryAsync(snippetId);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            return BadRequest($"获取版本历史失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取特定版本的详情
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>版本详情</returns>
    [HttpGet("{versionId:guid}")]
    public async Task<ActionResult<SnippetVersionDto>> GetVersion(Guid versionId)
    {
        try
        {
            var version = await _versionManagementService.GetVersionAsync(versionId);
            if (version == null)
            {
                return NotFound("版本不存在");
            }

            var currentUserId = GetCurrentUserId();
            
            // 检查用户是否有权限查看此版本
            var canAccess = await _permissionService.CanAccessSnippetAsync(
                currentUserId, version.SnippetId, PermissionOperation.Read);
            
            if (!canAccess)
            {
                return Forbid("您没有权限查看此版本");
            }

            return Ok(version);
        }
        catch (Exception ex)
        {
            return BadRequest($"获取版本详情失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 恢复到指定版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="versionId">要恢复的版本ID</param>
    /// <returns>恢复结果</returns>
    [HttpPost("snippet/{snippetId:guid}/restore/{versionId:guid}")]
    public async Task<ActionResult> RestoreVersion(Guid snippetId, Guid versionId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // 检查用户是否有权限编辑此代码片段
            var canEdit = await _permissionService.CanAccessSnippetAsync(
                currentUserId, snippetId, PermissionOperation.Edit);
            
            if (!canEdit)
            {
                return Forbid("您没有权限恢复此代码片段的版本");
            }

            var success = await _versionManagementService.RestoreVersionAsync(snippetId, versionId);
            if (!success)
            {
                return BadRequest("版本恢复失败，请检查版本ID是否正确");
            }

            return Ok(new { message = "版本恢复成功" });
        }
        catch (Exception ex)
        {
            return BadRequest($"版本恢复失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 比较两个版本的差异
    /// </summary>
    /// <param name="fromVersionId">源版本ID</param>
    /// <param name="toVersionId">目标版本ID</param>
    /// <returns>版本比较结果</returns>
    [HttpGet("compare/{fromVersionId:guid}/{toVersionId:guid}")]
    public async Task<ActionResult<VersionComparisonDto>> CompareVersions(Guid fromVersionId, Guid toVersionId)
    {
        try
        {
            var comparison = await _versionManagementService.CompareVersionsAsync(fromVersionId, toVersionId);
            if (comparison == null)
            {
                return NotFound("版本不存在或版本不属于同一个代码片段");
            }

            var currentUserId = GetCurrentUserId();
            
            // 检查用户是否有权限查看此代码片段
            var canAccess = await _permissionService.CanAccessSnippetAsync(
                currentUserId, comparison.FromVersion.SnippetId, PermissionOperation.Read);
            
            if (!canAccess)
            {
                return Forbid("您没有权限比较此代码片段的版本");
            }

            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"版本比较失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 手动创建版本 (用于重要的变更点)
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="request">创建版本请求</param>
    /// <returns>创建的版本</returns>
    [HttpPost("snippet/{snippetId:guid}")]
    public async Task<ActionResult<SnippetVersionDto>> CreateVersion(Guid snippetId, [FromBody] CreateVersionRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // 检查用户是否有权限编辑此代码片段
            var canEdit = await _permissionService.CanAccessSnippetAsync(
                currentUserId, snippetId, PermissionOperation.Edit);
            
            if (!canEdit)
            {
                return Forbid("您没有权限为此代码片段创建版本");
            }

            var version = await _versionManagementService.CreateVersionAsync(snippetId, request.ChangeDescription);
            return CreatedAtAction(nameof(GetVersion), new { versionId = version.Id }, version);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest($"创建版本失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    /// <returns>用户ID</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("无效的用户身份");
        }
        return userId;
    }
}

/// <summary>
/// 创建版本请求
/// </summary>
public class CreateVersionRequest
{
    /// <summary>
    /// 变更描述
    /// </summary>
    public string ChangeDescription { get; set; } = string.Empty;
}