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
    private readonly ILogger<CodeSnippetsController> _logger;

    public CodeSnippetsController(
        ICodeSnippetService codeSnippetService,
        ILogger<CodeSnippetsController> logger)
    {
        _codeSnippetService = codeSnippetService ?? throw new ArgumentNullException(nameof(codeSnippetService));
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
    /// 复制代码片段 - 增加复制次数
    /// </summary>
    /// <param name="id">代码片段ID</param>
    /// <returns>复制结果</returns>
    [HttpPost("{id:guid}/copy")]
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

            _logger.LogInformation("代码片段 {SnippetId} 复制次数增加成功", id);
            return Ok(new { message = "复制成功" });
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
    /// 获取当前登录用户的ID
    /// </summary>
    /// <returns>用户ID，如果未登录则返回null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}