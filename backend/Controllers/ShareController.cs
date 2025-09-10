using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 分享控制器 - 提供代码片段分享功能的API接口
/// </summary>
/// <remarks>
/// 此控制器提供完整的代码片段分享功能，包括：
/// - 生成分享链接
/// - 通过分享链接访问代码片段
/// - 管理分享链接（撤销、删除、更新）
/// - 获取分享统计信息
/// - 管理员分享管理功能
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class ShareController : ControllerBase
{
    private readonly IShareService _shareService;
    private readonly ILogger<ShareController> _logger;

    public ShareController(
        IShareService shareService,
        ILogger<ShareController> logger)
    {
        _shareService = shareService ?? throw new ArgumentNullException(nameof(shareService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 创建分享链接
    /// </summary>
    /// <param name="createShareDto">创建分享的请求参数</param>
    /// <returns>创建的分享令牌信息</returns>
    /// <remarks>
    /// 为指定的代码片段生成分享链接，支持设置权限和有效期。
    /// 
    /// 示例请求：
    /// POST /api/share
    /// {
    ///   "codeSnippetId": "12345678-1234-1234-1234-123456789012",
    ///   "permission": 1,
    ///   "expiresAt": "2024-12-31T23:59:59Z",
    ///   "description": "分享给团队的代码片段"
    /// }
    /// </remarks>
    /// <response code="201">分享链接创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">代码片段不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ShareTokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShareTokenDto>> CreateShare([FromBody] CreateShareDto createShareDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (createShareDto.CodeSnippetId == Guid.Empty)
            {
                return BadRequest(new { message = "代码片段ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var shareToken = await _shareService.CreateShareTokenAsync(createShareDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功创建分享链接，代码片段ID: {CodeSnippetId}, 分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, createShareDto.CodeSnippetId, shareToken.Id);

            // 返回201 Created状态码和分享令牌信息
            return CreatedAtAction(nameof(GetShare), new { token = shareToken.Token }, shareToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限创建分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建分享链接参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建分享链接时发生错误");
            return StatusCode(500, new { message = "创建分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 通过分享令牌获取分享信息
    /// </summary>
    /// <param name="token">分享令牌</param>
    /// <param name="password">访问密码（如果有）</param>
    /// <returns>分享令牌信息</returns>
    /// <remarks>
    /// 通过分享令牌访问代码片段内容，无需认证。系统会自动记录访问日志。
    /// 
    /// 示例请求：
    /// GET /api/share/abc123def456ghi789
    /// </remarks>
    /// <response code="200">成功获取分享信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">分享链接不存在或已失效</response>
    /// <response code="410">分享链接已过期</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{token}")]
    [ProducesResponseType(typeof(ShareTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShareTokenDto>> GetShare(string token, [FromQuery] string? password = null)
    {
        try
        {
            // 输入验证
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "分享令牌不能为空" });
            }

            var shareToken = await _shareService.GetShareTokenByTokenAsync(token, password);
            if (shareToken == null)
            {
                return NotFound(new { message = "分享链接不存在或已失效" });
            }

            // 记录访问日志
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            await _shareService.LogShareAccessAsync(shareToken.Id, ipAddress, userAgent);

            _logger.LogInformation("通过分享令牌访问成功，令牌: {Token}, 代码片段ID: {CodeSnippetId}", 
                token, shareToken.CodeSnippetId);

            return Ok(shareToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通过分享令牌获取分享信息时发生错误，令牌: {Token}", token);
            return StatusCode(500, new { message = "获取分享信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 访问分享内容
    /// </summary>
    /// <param name="request">访问分享请求</param>
    /// <returns>访问结果</returns>
    /// <remarks>
    /// 通过分享令牌访问代码片段内容，支持密码保护和访问限制。
    /// 此接口会记录访问日志并更新访问统计。
    /// 
    /// 示例请求：
    /// POST /api/share/access
    /// {
    ///   "token": "abc123def456",
    ///   "password": "password123",
    ///   "userAgent": "Mozilla/5.0..."
    /// }
    /// </remarks>
    /// <response code="200">访问成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">密码错误</response>
    /// <response code="404">分享链接不存在或已失效</response>
    /// <response code="410">分享链接已过期</response>
    /// <response code="429">访问次数已达上限</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("access")]
    [ProducesResponseType(typeof(AccessShareResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessShareResponse>> AccessShare([FromBody] AccessShareRequest request)
    {
        try
        {
            // 输入验证
            if (request == null)
            {
                return BadRequest(new { message = "请求参数不能为空" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { message = "分享令牌不能为空" });
            }

            // 访问分享内容
            var ipAddress = GetClientIpAddress();
            var userAgent = request.UserAgent ?? Request.Headers["User-Agent"].ToString();
            
            var accessResult = await _shareService.AccessShareAsync(request.Token, request.Password, ipAddress, userAgent);

            if (!accessResult.Success)
            {
                // 根据错误类型返回不同的状态码
                if (accessResult.ErrorMessage?.Contains("密码") == true)
                {
                    return Unauthorized(new AccessShareResponse
                    {
                        Success = false,
                        ErrorMessage = accessResult.ErrorMessage
                    });
                }
                else if (accessResult.ErrorMessage?.Contains("过期") == true)
                {
                    return StatusCode(StatusCodes.Status410Gone, new AccessShareResponse
                    {
                        Success = false,
                        ErrorMessage = accessResult.ErrorMessage
                    });
                }
                else if (accessResult.ErrorMessage?.Contains("次数") == true)
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests, new AccessShareResponse
                    {
                        Success = false,
                        ErrorMessage = accessResult.ErrorMessage
                    });
                }
                else
                {
                    return NotFound(new AccessShareResponse
                    {
                        Success = false,
                        ErrorMessage = accessResult.ErrorMessage
                    });
                }
            }

            _logger.LogInformation("分享内容访问成功，令牌: {Token}, 代码片段ID: {CodeSnippetId}", 
                request.Token, accessResult.ShareToken?.CodeSnippetId);

            return Ok(accessResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "访问分享内容时发生错误，令牌: {Token}", request?.Token);
            return StatusCode(500, new AccessShareResponse
            {
                Success = false,
                ErrorMessage = "访问分享内容时发生内部错误"
            });
        }
    }

    /// <summary>
    /// 验证分享令牌是否有效
    /// </summary>
    /// <param name="token">分享令牌</param>
    /// <param name="password">访问密码（如果有）</param>
    /// <returns>验证结果</returns>
    [HttpPost("{token}/validate")]
    public async Task<ActionResult<object>> ValidateShare(string token, [FromBody] ValidateShareRequest? request = null)
    {
        try
        {
            // 输入验证
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "分享令牌不能为空" });
            }

            var password = request?.Password;
            var validationResult = await _shareService.ValidateShareTokenAsync(token, password);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("分享令牌验证失败，令牌: {Token}, 原因: {Message}", token, validationResult.Message);
                return BadRequest(new { isValid = false, message = validationResult.Message });
            }

            _logger.LogDebug("分享令牌验证成功，令牌: {Token}", token);
            return Ok(new { isValid = true, message = "分享令牌有效" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证分享令牌时发生错误，令牌: {Token}", token);
            return StatusCode(500, new { message = "验证分享令牌时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取用户的分享链接列表
    /// </summary>
    /// <param name="page">页码，默认为1</param>
    /// <param name="pageSize">每页大小，默认为10，最大为100</param>
    /// <returns>用户的分享链接列表</returns>
    /// <remarks>
    /// 获取当前用户创建的所有分享链接，支持分页。
    /// 
    /// 示例请求：
    /// GET /api/share/my-shares?page=1&pageSize=10
    /// </remarks>
    /// <response code="200">成功获取分享链接列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("my-shares")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<ShareTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<ShareTokenDto>>> GetMyShares(
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

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _shareService.GetUserShareTokensPaginatedAsync(
                currentUserId.Value, page, pageSize, currentUserId.Value);

            _logger.LogInformation("获取用户 {UserId} 的分享链接列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, page, pageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取分享链接列表: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户分享链接列表时发生错误");
            return StatusCode(500, new { message = "获取分享链接列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享链接的统计信息
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>分享统计信息</returns>
    /// <remarks>
    /// 获取分享链接的详细统计信息，包括访问次数、复制次数、唯一访客等。
    /// 只有分享链接的创建者或管理员可以访问此接口。
    /// 
    /// 示例请求：
    /// GET /api/share/87654321-4321-4321-4321-210987654321/stats
    /// </remarks>
    /// <response code="200">成功获取分享统计信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">分享链接不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}/stats")]
    [Authorize]
    [ProducesResponseType(typeof(ShareStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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

            _logger.LogInformation("获取分享统计信息成功，分享令牌ID: {ShareTokenId}", id);
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取分享统计信息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取分享统计信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享统计信息时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享统计信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新分享链接设置
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="updateShareDto">更新分享的请求参数</param>
    /// <returns>更新后的分享令牌信息</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ShareTokenDto>> UpdateShare(Guid id, [FromBody] UpdateShareDto updateShareDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
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

            var shareToken = await _shareService.UpdateShareTokenAsync(id, updateShareDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功更新分享链接，分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, id);

            return Ok(shareToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限更新分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新分享链接参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新分享链接时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "更新分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 撤销分享链接（禁用）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>撤销结果</returns>
    /// <remarks>
    /// 撤销指定的分享链接，使其失效。此操作不会删除分享记录，只是将其标记为非活跃状态。
    /// 只有分享链接的创建者或管理员可以执行此操作。
    /// 
    /// 示例请求：
    /// DELETE /api/share/87654321-4321-4321-4321-210987654321/revoke
    /// </remarks>
    /// <response code="204">分享链接撤销成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">分享链接不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("{id:guid}/revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RevokeShare(Guid id)
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

            var success = await _shareService.RevokeShareTokenAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "分享链接不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功撤销分享链接，分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限撤销分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("撤销分享链接参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤销分享链接时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "撤销分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除分享链接
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteShare(Guid id)
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

            var success = await _shareService.DeleteShareTokenAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "分享链接不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功删除分享链接，分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除分享链接参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除分享链接时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "删除分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享链接的访问日志
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>访问日志列表</returns>
    [HttpGet("{id:guid}/access-logs")]
    [Authorize]
    public async Task<ActionResult<PaginatedResult<ShareAccessLogDto>>> GetShareAccessLogs(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
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
                PageSize = pageSize
            };

            var result = await _shareService.GetShareAccessLogsAsync(filter, currentUserId);

            _logger.LogInformation("获取分享访问日志成功，分享令牌ID: {ShareTokenId}, 页码: {Page}, 每页大小: {PageSize}", 
                id, page, pageSize);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取分享访问日志: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取分享访问日志参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享访问日志时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享访问日志时发生内部错误" });
        }
    }

    /// <summary>
    /// 延长分享链接有效期
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="request">延长有效期的请求参数</param>
    /// <returns>更新后的分享令牌信息</returns>
    [HttpPost("{id:guid}/extend")]
    [Authorize]
    public async Task<ActionResult<ShareTokenDto>> ExtendShareExpiry(Guid id, [FromBody] ExtendShareExpiryRequest request)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            if (request == null || request.ExtendHours <= 0)
            {
                return BadRequest(new { message = "延长时间必须大于0" });
            }

            if (request.ExtendHours > 8760) // 1年
            {
                return BadRequest(new { message = "延长时间不能超过8760小时（1年）" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var shareToken = await _shareService.ExtendShareTokenExpiryAsync(id, request.ExtendHours, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功延长分享链接有效期，分享令牌ID: {ShareTokenId}, 延长小时数: {Hours}", 
                currentUserId.Value, id, request.ExtendHours);

            return Ok(shareToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限延长分享链接有效期: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("延长分享链接有效期参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "延长分享链接有效期时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "延长分享链接有效期时发生内部错误" });
        }
    }

    /// <summary>
    /// 重置分享链接访问统计
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>重置结果</returns>
    [HttpPost("{id:guid}/reset-stats")]
    [Authorize]
    public async Task<ActionResult> ResetShareStats(Guid id)
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

            var success = await _shareService.ResetShareAccessStatsAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "分享链接不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功重置分享链接访问统计，分享令牌ID: {ShareTokenId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限重置分享链接访问统计: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("重置分享链接访问统计参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置分享链接访问统计时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "重置分享链接访问统计时发生内部错误" });
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

    /// <summary>
    /// 获取客户端IP地址
    /// </summary>
    /// <returns>客户端IP地址</returns>
    private string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // 检查是否通过代理
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ipAddress = forwardedFor.Split(',')[0].Trim();
        }

        return ipAddress;
    }

    /// <summary>
    /// 获取所有分享链接（仅管理员）
    /// </summary>
    /// <param name="filter">分享过滤器</param>
    /// <returns>分享链接列表</returns>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaginatedResult<ShareTokenDto>>> GetAllShares([FromQuery] AdminShareFilter filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "过滤器参数不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _shareService.GetAllSharesAdminAsync(filter, currentUserId.Value);

            _logger.LogInformation("管理员获取所有分享链接成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限获取所有分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员获取所有分享链接时发生错误");
            return StatusCode(500, new { message = "获取分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取系统分享统计信息（仅管理员）
    /// </summary>
    /// <returns>系统分享统计信息</returns>
    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemShareStatsDto>> GetSystemShareStats()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var stats = await _shareService.GetSystemShareStatsAsync(currentUserId.Value);

            _logger.LogInformation("管理员获取系统分享统计信息成功");
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限获取系统分享统计: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员获取系统分享统计时发生错误");
            return StatusCode(500, new { message = "获取系统分享统计时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量操作分享链接（仅管理员）
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("admin/bulk-operation")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> BulkOperationShares([FromBody] BulkShareOperationRequest request)
    {
        try
        {
            // 输入验证
            if (request == null)
            {
                return BadRequest(new { message = "请求参数不能为空" });
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

            var result = await _shareService.BulkOperationSharesAsync(request, currentUserId.Value);

            _logger.LogInformation("管理员批量操作分享链接成功，操作类型: {Operation}, 处理数量: {SuccessCount}/{TotalCount}", 
                request.Operation, result.SuccessCount, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限批量操作分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("批量操作分享链接参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员批量操作分享链接时发生错误");
            return StatusCode(500, new { message = "批量操作分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取分享链接的详细访问日志（仅管理员）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="filter">访问日志过滤器</param>
    /// <returns>访问日志列表</returns>
    [HttpGet("admin/{id:guid}/access-logs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaginatedResult<ShareAccessLogDto>>> GetShareAccessLogsAdmin(
        Guid id,
        [FromQuery] AccessLogFilter filter)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "分享令牌ID不能为空" });
            }

            if (filter == null)
            {
                return BadRequest(new { message = "过滤器参数不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 确保过滤器包含分享令牌ID
            filter.ShareTokenId = id;

            var result = await _shareService.GetShareAccessLogsAsync(filter, currentUserId);

            _logger.LogInformation("管理员获取分享访问日志成功，分享令牌ID: {ShareTokenId}, 页码: {Page}, 每页大小: {PageSize}", 
                id, filter.Page, filter.PageSize);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限获取分享访问日志: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员获取分享访问日志时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "获取分享访问日志时发生内部错误" });
        }
    }

    /// <summary>
    /// 强制撤销分享链接（仅管理员）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>撤销结果</returns>
    [HttpDelete("admin/{id:guid}/revoke")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ForceRevokeShare(Guid id)
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

            var success = await _shareService.ForceRevokeShareTokenAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "分享链接不存在" });
            }

            _logger.LogInformation("管理员强制撤销分享链接成功，分享令牌ID: {ShareTokenId}", id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限强制撤销分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员强制撤销分享链接时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "强制撤销分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 强制删除分享链接（仅管理员）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> ForceDeleteShare(Guid id)
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

            var success = await _shareService.ForceDeleteShareTokenAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "分享链接不存在" });
            }

            _logger.LogInformation("管理员强制删除分享链接成功，分享令牌ID: {ShareTokenId}", id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限强制删除分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员强制删除分享链接时发生错误，分享令牌ID: {ShareTokenId}", id);
            return StatusCode(500, new { message = "强制删除分享链接时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取用户的分享链接列表（仅管理员）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>用户的分享链接列表</returns>
    [HttpGet("admin/users/{userId:guid}/shares")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaginatedResult<ShareTokenDto>>> GetUserSharesAdmin(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // 输入验证
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "用户ID不能为空" });
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
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _shareService.GetUserShareTokensPaginatedAsync(userId, page, pageSize, currentUserId.Value);

            _logger.LogInformation("管理员获取用户 {UserId} 的分享链接列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                userId, page, pageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("管理员无权限获取用户分享链接: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员获取用户分享链接时发生错误，用户ID: {UserId}", userId);
            return StatusCode(500, new { message = "获取用户分享链接时发生内部错误" });
        }
    }
}

/// <summary>
/// 验证分享请求
/// </summary>
public class ValidateShareRequest
{
    /// <summary>
    /// 访问密码（如果分享链接设置了密码保护）
    /// </summary>
    /// <example>password123</example>
    public string? Password { get; set; }
}

/// <summary>
/// 延长分享有效期请求
/// </summary>
public class ExtendShareExpiryRequest
{
    /// <summary>
    /// 延长的小时数（1-8760小时，即1天到1年）
    /// </summary>
    /// <example>24</example>
    [Required(ErrorMessage = "延长时间不能为空")]
    [Range(1, 8760, ErrorMessage = "延长时间必须在1-8760小时之间")]
    public int ExtendHours { get; set; }
}

