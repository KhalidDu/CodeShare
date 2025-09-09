using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 代码片段控制器 - 遵循单一职责原则，只负责代码片段相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CodeSnippetsController : ControllerBase
{
    private readonly ICodeSnippetService _codeSnippetService;
    private readonly IVersionManagementService _versionManagementService;
    private readonly IShareService _shareService;
    private readonly ILogger<CodeSnippetsController> _logger;

    public CodeSnippetsController(
        ICodeSnippetService codeSnippetService,
        IVersionManagementService versionManagementService,
        IShareService shareService,
        ILogger<CodeSnippetsController> logger)
    {
        _codeSnippetService = codeSnippetService ?? throw new ArgumentNullException(nameof(codeSnippetService));
        _versionManagementService = versionManagementService ?? throw new ArgumentNullException(nameof(versionManagementService));
        _shareService = shareService ?? throw new ArgumentNullException(nameof(shareService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取代码片段列表 - 支持搜索、筛选和分页
    /// </summary>
    /// <param name="search">搜索关键词</param>
    /// <param name="language">编程语言筛选</param>
    /// <param name="tag">标签筛选</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页的代码片段列表</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetSnippets(
        [FromQuery] string? search = null,
        [FromQuery] string? language = null,
        [FromQuery] string? tag = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 输入验证
            if (page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var filter = new SnippetFilterDto
            {
                Search = search,
                Language = language,
                Tag = tag,
                Page = page,
                PageSize = pageSize
            };

            // 获取当前用户ID（如果已登录）
            var currentUserId = GetCurrentUserId();

            var result = await _codeSnippetService.GetSnippetsAsync(filter, currentUserId);
            
            _logger.LogInformation("获取代码片段列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                page, pageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段列表时发生错误");
            return StatusCode(500, new { message = "获取代码片段列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取单个代码片段详情
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>代码片段详情</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CodeSnippetDto>> GetSnippet(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var snippet = await _codeSnippetService.GetSnippetAsync(id, currentUserId);

            if (snippet == null)
            {
                return NotFound(new { message = "代码片段不存在或无权限访问" });
            }

            // 增加查看次数
            await _codeSnippetService.IncrementViewCountAsync(id, currentUserId);

            _logger.LogInformation("获取代码片段详情成功，ID: {SnippetId}", id);
            return Ok(snippet);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问代码片段 {SnippetId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段详情时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "获取代码片段详情时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建新的代码片段
    /// </summary>
    /// <param name="createDto">创建代码片段的数据</param>
    /// <returns>创建的代码片段</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CodeSnippetDto>> CreateSnippet([FromBody] CreateSnippetDto createDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(createDto.Title))
            {
                return BadRequest(new { message = "标题不能为空" });
            }

            if (string.IsNullOrWhiteSpace(createDto.Code))
            {
                return BadRequest(new { message = "代码内容不能为空" });
            }

            if (string.IsNullOrWhiteSpace(createDto.Language))
            {
                return BadRequest(new { message = "编程语言不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var snippet = await _codeSnippetService.CreateSnippetAsync(createDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 创建代码片段成功，ID: {SnippetId}", 
                currentUserId.Value, snippet.Id);

            return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippet);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限创建代码片段: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建代码片段参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建代码片段时发生错误");
            return StatusCode(500, new { message = "创建代码片段时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新代码片段
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <param name="updateDto">更新数据</param>
    /// <returns>更新后的代码片段</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CodeSnippetDto>> UpdateSnippet(Guid id, [FromBody] UpdateSnippetDto updateDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var snippet = await _codeSnippetService.UpdateSnippetAsync(id, updateDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 更新代码片段成功，ID: {SnippetId}", 
                currentUserId.Value, id);

            return Ok(snippet);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限更新代码片段 {SnippetId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新代码片段参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新代码片段时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "更新代码片段时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除代码片段
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteSnippet(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _codeSnippetService.DeleteSnippetAsync(id, currentUserId.Value);

            if (!result)
            {
                return NotFound(new { message = "代码片段不存在" });
            }

            _logger.LogInformation("用户 {UserId} 删除代码片段成功，ID: {SnippetId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限删除代码片段 {SnippetId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除代码片段时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "删除代码片段时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取用户的代码片段列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户的代码片段列表</returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<CodeSnippetDto>>> GetUserSnippets(Guid userId)
    {
        try
        {
            // 输入验证
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "用户ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var snippets = await _codeSnippetService.GetUserSnippetsAsync(userId, currentUserId);

            _logger.LogInformation("获取用户 {UserId} 的代码片段列表成功", userId);
            return Ok(snippets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户代码片段列表时发生错误，用户ID: {UserId}", userId);
            return StatusCode(500, new { message = "获取用户代码片段列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 复制代码片段 - 增加复制次数（已弃用，请使用 /api/clipboard/copy/{id}）
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>复制结果</returns>
    [HttpPost("{id:guid}/copy")]
    [Obsolete("此端点已弃用，请使用 /api/clipboard/copy/{id}")]
    public async Task<ActionResult> CopySnippet(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var result = await _codeSnippetService.IncrementCopyCountAsync(id, currentUserId);

            if (!result)
            {
                return NotFound(new { message = "代码片段不存在或无权限访问" });
            }

            _logger.LogInformation("代码片段 {SnippetId} 复制次数增加成功（使用已弃用的端点）", id);
            return Ok(new { 
                message = "复制成功", 
                warning = "此端点已弃用，请使用 /api/clipboard/copy/" + id 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("无权限复制代码片段 {SnippetId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制代码片段时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "复制代码片段时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取代码片段的版本历史 (便捷接口)
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>版本历史列表</returns>
    [HttpGet("{id:guid}/versions")]
    public async Task<ActionResult<IEnumerable<SnippetVersionDto>>> GetSnippetVersions(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 首先检查代码片段是否存在以及用户是否有权限访问
            var snippet = await _codeSnippetService.GetSnippetAsync(id, currentUserId);
            if (snippet == null)
            {
                return NotFound(new { message = "代码片段不存在或无权限访问" });
            }

            var versions = await _versionManagementService.GetVersionHistoryAsync(id);

            _logger.LogInformation("获取代码片段 {SnippetId} 的版本历史成功", id);
            return Ok(versions);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问代码片段版本历史 {SnippetId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段版本历史时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "获取版本历史时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建分享令牌 - 为代码片段生成分享链接
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <param name="createShareDto">创建分享请求</param>
    /// <returns>创建的分享令牌信息</returns>
    [HttpPost("{id:guid}/share")]
    [Authorize]
    public async Task<ActionResult<ShareTokenDto>> CreateShareToken(Guid id, [FromBody] CreateShareDto createShareDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 验证代码片段ID是否匹配
            if (createShareDto.CodeSnippetId != id)
            {
                return BadRequest(new { message = "代码片段ID不匹配" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 检查用户是否有权限分享该代码片段
            var snippet = await _codeSnippetService.GetSnippetAsync(id, currentUserId);
            if (snippet == null)
            {
                return NotFound(new { message = "代码片段不存在或无权限访问" });
            }

            var shareToken = await _shareService.CreateShareTokenAsync(createShareDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 为代码片段 {SnippetId} 创建分享令牌成功，分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, id, shareToken.Id);

            return CreatedAtAction(nameof(GetShareToken), new { id = shareToken.Id }, shareToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限分享代码片段 {SnippetId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建分享令牌参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建分享令牌时发生错误，代码片段ID: {SnippetId}", id);
            return StatusCode(500, new { message = "创建分享令牌时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享令牌详情
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>分享令牌详情</returns>
    [HttpGet("share/{id:guid}")]
    public async Task<ActionResult<ShareTokenDto>> GetShareToken(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var shareToken = await _shareService.GetShareTokenByIdAsync(id, currentUserId);

            if (shareToken == null)
            {
                return NotFound(new { message = "分享令牌不存在或无权限访问" });
            }

            _logger.LogInformation("获取分享令牌详情成功，ID: {ShareTokenId}", id);
            return Ok(shareToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问分享令牌 {ShareTokenId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享令牌详情时发生错误，ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享令牌详情时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取代码片段的所有分享记录
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>代码片段的分享令牌列表</returns>
    [HttpGet("{id:guid}/shares")]
    public async Task<ActionResult<IEnumerable<ShareTokenDto>>> GetSnippetShares(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var shares = await _shareService.GetSnippetSharesAsync(id, currentUserId);

            _logger.LogInformation("获取代码片段 {SnippetId} 的分享记录成功", id);
            return Ok(shares);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问代码片段 {SnippetId} 的分享记录: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段分享记录时发生错误，ID: {SnippetId}", id);
            return StatusCode(500, new { message = "获取分享记录时发生内部错误" });
        }
    }

    /// <summary>
    /// 撤销分享令牌（禁用）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>撤销结果</returns>
    [HttpDelete("share/{id:guid}")]
    [Authorize]
    public async Task<ActionResult> RevokeShareToken(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _shareService.RevokeShareTokenAsync(id, currentUserId.Value);

            if (!result)
            {
                return NotFound(new { message = "分享令牌不存在或无权限操作" });
            }

            _logger.LogInformation("用户 {UserId} 撤销分享令牌成功，ID: {ShareTokenId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限撤销分享令牌 {ShareTokenId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤销分享令牌时发生错误，ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "撤销分享令牌时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享统计信息
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>分享统计信息</returns>
    [HttpGet("share/{id:guid}/stats")]
    public async Task<ActionResult<ShareStatsDto>> GetShareStats(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            var stats = await _shareService.GetShareStatsAsync(id, currentUserId);

            _logger.LogInformation("获取分享统计信息成功，ID: {ShareTokenId}", id);
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问分享统计 {ShareTokenId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享统计信息时发生错误，ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享统计信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享访问日志
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="isSuccess">是否只显示成功访问</param>
    /// <returns>分页的访问日志结果</returns>
    [HttpGet("share/{id:guid}/logs")]
    public async Task<ActionResult<PaginatedResult<ShareAccessLogDto>>> GetShareAccessLogs(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? isSuccess = null)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            if (page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            var filter = new AccessLogFilter
            {
                ShareTokenId = id,
                Page = page,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate,
                IsSuccess = isSuccess
            };

            var logs = await _shareService.GetShareAccessLogsAsync(filter, currentUserId);

            _logger.LogInformation("获取分享访问日志成功，ID: {ShareTokenId}", id);
            return Ok(logs);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限访问分享访问日志 {ShareTokenId}: {Message}", id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享访问日志时发生错误，ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享访问日志时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取当前登录用户的ID
    /// </summary>
    /// <returns>用户ID，如果未登录则返回null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}