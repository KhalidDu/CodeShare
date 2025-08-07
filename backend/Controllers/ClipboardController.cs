using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 剪贴板历史控制器 - 遵循单一职责原则，只负责剪贴板相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClipboardController : ControllerBase
{
    private readonly IClipboardService _clipboardService;
    private readonly ICodeSnippetService _codeSnippetService;
    private readonly ILogger<ClipboardController> _logger;

    public ClipboardController(
        IClipboardService clipboardService,
        ICodeSnippetService codeSnippetService,
        ILogger<ClipboardController> logger)
    {
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _codeSnippetService = codeSnippetService ?? throw new ArgumentNullException(nameof(codeSnippetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 记录代码片段复制操作 - 创建复制记录并更新统计
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>复制记录</returns>
    [HttpPost("copy/{snippetId:guid}")]
    public async Task<ActionResult<ClipboardHistoryDto>> RecordCopy(Guid snippetId)
    {
        try
        {
            // 输入验证
            if (snippetId == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();

            // 如果用户已登录，记录到剪贴板历史
            if (currentUserId.HasValue)
            {
                // 先增加复制计数（包含权限验证）
                var copyResult = await _codeSnippetService.IncrementCopyCountAsync(snippetId, currentUserId);
                if (!copyResult)
                {
                    return NotFound(new { message = "代码片段不存在或无权限访问" });
                }

                // 记录到剪贴板历史
                var history = await _clipboardService.RecordCopyAsync(currentUserId.Value, snippetId);

                _logger.LogInformation("用户 {UserId} 复制代码片段 {SnippetId} 成功", 
                    currentUserId.Value, snippetId);

                return Ok(history);
            }
            else
            {
                // 未登录用户只增加复制计数，不记录历史
                var copyResult = await _codeSnippetService.IncrementCopyCountAsync(snippetId, null);
                if (!copyResult)
                {
                    return NotFound(new { message = "代码片段不存在或无权限访问" });
                }

                _logger.LogInformation("匿名用户复制代码片段 {SnippetId} 成功", snippetId);

                return Ok(new { message = "复制成功", snippetId = snippetId });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("复制代码片段参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("无权限复制代码片段 {SnippetId}: {Message}", snippetId, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "复制代码片段时发生错误，ID: {SnippetId}", snippetId);
            return StatusCode(500, new { message = "复制代码片段时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取当前用户的剪贴板历史
    /// </summary>
    /// <param name="limit">返回记录数限制，默认50条，最大100条</param>
    /// <returns>剪贴板历史列表</returns>
    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ClipboardHistoryWithSnippetDto>>> GetClipboardHistory(
        [FromQuery] int limit = 50)
    {
        try
        {
            // 输入验证
            if (limit <= 0 || limit > 100)
            {
                limit = 50;
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var histories = await _clipboardService.GetUserClipboardHistoryAsync(currentUserId.Value, limit);
            
            // 获取相关的代码片段信息
            var enrichedHistories = new List<ClipboardHistoryWithSnippetDto>();
            foreach (var history in histories)
            {
                var snippet = await _codeSnippetService.GetSnippetAsync(history.SnippetId, currentUserId);
                enrichedHistories.Add(new ClipboardHistoryWithSnippetDto
                {
                    Id = history.Id,
                    UserId = history.UserId,
                    SnippetId = history.SnippetId,
                    CopiedAt = history.CopiedAt,
                    Snippet = snippet
                });
            }

            _logger.LogInformation("获取用户 {UserId} 的剪贴板历史成功，返回 {Count} 条记录", 
                currentUserId.Value, enrichedHistories.Count);

            return Ok(enrichedHistories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取剪贴板历史时发生错误");
            return StatusCode(500, new { message = "获取剪贴板历史时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取当前用户的剪贴板历史记录数量
    /// </summary>
    /// <returns>历史记录数量</returns>
    [HttpGet("history/count")]
    [Authorize]
    public async Task<ActionResult<int>> GetClipboardHistoryCount()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var count = await _clipboardService.GetUserHistoryCountAsync(currentUserId.Value);

            _logger.LogInformation("获取用户 {UserId} 的剪贴板历史记录数量成功: {Count}", 
                currentUserId.Value, count);

            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取剪贴板历史记录数量时发生错误");
            return StatusCode(500, new { message = "获取剪贴板历史记录数量时发生内部错误" });
        }
    }

    /// <summary>
    /// 清空当前用户的剪贴板历史
    /// </summary>
    /// <returns>清空结果</returns>
    [HttpDelete("history")]
    [Authorize]
    public async Task<ActionResult> ClearClipboardHistory()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _clipboardService.ClearUserClipboardHistoryAsync(currentUserId.Value);

            if (result)
            {
                _logger.LogInformation("用户 {UserId} 清空剪贴板历史成功", currentUserId.Value);
                return Ok(new { message = "剪贴板历史已清空" });
            }
            else
            {
                return NotFound(new { message = "没有找到剪贴板历史记录" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空剪贴板历史时发生错误");
            return StatusCode(500, new { message = "清空剪贴板历史时发生内部错误" });
        }
    }

    /// <summary>
    /// 重新复制历史记录中的代码片段
    /// </summary>
    /// <param name="historyId">历史记录ID</param>
    /// <returns>复制结果</returns>
    [HttpPost("history/{historyId:guid}/recopy")]
    [Authorize]
    public async Task<ActionResult<ClipboardHistoryDto>> RecopyFromHistory(Guid historyId)
    {
        try
        {
            // 输入验证
            if (historyId == Guid.Empty)
            {
                return BadRequest(new { message = "历史记录ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 获取历史记录
            var histories = await _clipboardService.GetUserClipboardHistoryAsync(currentUserId.Value, 1000);
            var targetHistory = histories.FirstOrDefault(h => h.Id == historyId);

            if (targetHistory == null)
            {
                return NotFound(new { message = "历史记录不存在" });
            }

            // 重新复制该代码片段
            var newHistory = await _clipboardService.RecordCopyAsync(currentUserId.Value, targetHistory.SnippetId);

            _logger.LogInformation("用户 {UserId} 从历史记录 {HistoryId} 重新复制代码片段 {SnippetId} 成功", 
                currentUserId.Value, historyId, targetHistory.SnippetId);

            return Ok(newHistory);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("重新复制历史记录参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重新复制历史记录时发生错误，历史记录ID: {HistoryId}", historyId);
            return StatusCode(500, new { message = "重新复制历史记录时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取代码片段的复制统计信息
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>复制统计信息</returns>
    [HttpGet("stats/{snippetId:guid}")]
    public async Task<ActionResult<CopyStatsDto>> GetCopyStats(Guid snippetId)
    {
        try
        {
            // 输入验证
            if (snippetId == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();

            // 检查代码片段是否存在以及用户是否有权限访问
            var snippet = await _codeSnippetService.GetSnippetAsync(snippetId, currentUserId);
            if (snippet == null)
            {
                return NotFound(new { message = "代码片段不存在或无权限访问" });
            }

            var copyStats = new CopyStatsDto
            {
                SnippetId = snippetId,
                TotalCopyCount = snippet.CopyCount,
                ViewCount = snippet.ViewCount
            };

            _logger.LogInformation("获取代码片段 {SnippetId} 的复制统计成功", snippetId);

            return Ok(copyStats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("无权限获取代码片段复制统计 {SnippetId}: {Message}", snippetId, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段复制统计时发生错误，ID: {SnippetId}", snippetId);
            return StatusCode(500, new { message = "获取复制统计时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量获取多个代码片段的复制统计
    /// </summary>
    /// <param name="request">批量查询请求</param>
    /// <returns>复制统计字典</returns>
    [HttpPost("stats/batch")]
    public async Task<ActionResult<Dictionary<Guid, int>>> GetBatchCopyStats([FromBody] BatchCopyStatsRequestDto request)
    {
        try
        {
            // 输入验证
            if (request?.SnippetIds == null || !request.SnippetIds.Any())
            {
                return BadRequest(new { message = "代码片段ID列表不能为空" });
            }

            if (request.SnippetIds.Count() > 100)
            {
                return BadRequest(new { message = "一次最多查询100个代码片段的统计" });
            }

            var stats = await _clipboardService.GetCopyCountsBatchAsync(request.SnippetIds);

            _logger.LogInformation("批量获取 {Count} 个代码片段的复制统计成功", request.SnippetIds.Count());

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取代码片段复制统计时发生错误");
            return StatusCode(500, new { message = "批量获取复制统计时发生内部错误" });
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