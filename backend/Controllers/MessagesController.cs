using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 消息控制器 - 提供消息功能的RESTful API接口
/// </summary>
/// <remarks>
/// 此控制器提供完整的消息功能，包括：
/// - 消息的创建、读取、更新、删除（CRUD）
/// - 消息附件管理
/// - 消息草稿管理
/// - 会话管理
/// - 消息搜索和筛选
/// - 批量操作支持
/// - 消息统计和分析
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, NoStore = false)]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    // 简单缓存，用于缓存热门消息
    private static readonly ConcurrentDictionary<string, CacheEntry> _messageCache = new();

    public MessagesController(
        IMessageService messageService,
        ILogger<MessagesController> logger)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 消息CRUD操作

    /// <summary>
    /// 获取消息列表
    /// </summary>
    /// <param name="filter">消息筛选条件</param>
    /// <returns>消息列表</returns>
    /// <remarks>
    /// 根据筛选条件获取消息列表，支持分页、排序和多种筛选条件。
    /// 
    /// 示例请求：
    /// GET /api/messages?userId=12345678-1234-1234-1234-123456789012&amp;page=1&amp;pageSize=20&amp;sortBy=CreatedAtDesc
    /// </remarks>
    /// <response code="200">成功获取消息列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessages([FromQuery] MessageFilterDto filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 设置当前用户ID
            filter.CurrentUserId = currentUserId.Value;

            // 生成缓存键
            var cacheKey = GenerateCacheKey(filter);

            // 尝试从缓存获取
            if (ShouldUseCache(filter))
            {
                if (_messageCache.TryGetValue(cacheKey, out var cachedEntry))
                {
                    if (cachedEntry.ExpiresAt > DateTime.UtcNow)
                    {
                        _logger.LogDebug("从缓存获取消息列表成功，缓存键: {CacheKey}", cacheKey);
                        return Ok(cachedEntry.Data);
                    }
                    else
                    {
                        _messageCache.TryRemove(cacheKey, out _);
                    }
                }
            }

            // 从服务获取数据
            var result = await _messageService.GetUserMessagesAsync(currentUserId.Value, filter, currentUserId.Value);

            // 缓存结果
            if (ShouldUseCache(filter))
            {
                var cacheEntry = new CacheEntry
                {
                    Data = result,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };
                _messageCache.TryAdd(cacheKey, cacheEntry);
            }

            _logger.LogInformation("获取消息列表成功，筛选条件: {Filter}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                filter, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取消息列表: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取消息列表参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息列表时发生错误");
            return StatusCode(500, new { message = "获取消息列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取指定消息的详细信息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息详细信息</returns>
    /// <remarks>
    /// 获取指定消息的详细信息，包括附件和回复信息。
    /// 
    /// 示例请求：
    /// GET /api/messages/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取消息信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<MessageDto>> GetMessage(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var message = await _messageService.GetMessageAsync(id, currentUserId.Value);

            if (message == null)
            {
                return NotFound(new { message = "消息不存在" });
            }

            _logger.LogInformation("获取消息信息成功，消息ID: {MessageId}", id);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取消息信息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取消息信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息信息时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "获取消息信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 发送新消息
    /// </summary>
    /// <param name="createMessageDto">发送消息的请求参数</param>
    /// <returns>发送的消息信息</returns>
    /// <remarks>
    /// 发送新消息给指定用户，支持附件和定时发送。
    /// 
    /// 示例请求：
    /// POST /api/messages
    /// {
    ///   "receiverId": "12345678-1234-1234-1234-123456789012",
    ///   "subject": "关于代码片段的讨论",
    ///   "content": "我想和你讨论一下你分享的代码片段",
    ///   "messageType": 0,
    ///   "priority": 1
    /// }
    /// </remarks>
    /// <response code="201">消息发送成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="429">发送频率限制</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageDto createMessageDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var message = await _messageService.SendMessageAsync(createMessageDto, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功发送消息，消息ID: {MessageId}, 接收者ID: {ReceiverId}", 
                currentUserId.Value, message.Id, createMessageDto.ReceiverId);

            // 返回201 Created状态码和消息信息
            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限发送消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("发送消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息时发生错误");
            return StatusCode(500, new { message = "发送消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新消息内容
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <param name="updateMessageDto">更新消息的请求参数</param>
    /// <returns>更新后的消息信息</returns>
    /// <remarks>
    /// 更新指定消息的内容，只有消息作者可以更新自己的消息。
    /// 
    /// 示例请求：
    /// PUT /api/messages/12345678-1234-1234-1234-123456789012
    /// {
    ///   "subject": "更新的主题",
    ///   "content": "更新后的消息内容"
    /// }
    /// </remarks>
    /// <response code="200">消息更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDto>> UpdateMessage(Guid id, [FromBody] UpdateMessageDto updateMessageDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
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

            var message = await _messageService.UpdateMessageAsync(id, updateMessageDto, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功更新消息，消息ID: {MessageId}", 
                currentUserId.Value, id);

            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限更新消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "更新消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 删除指定消息，只有消息作者或管理员可以删除消息。
    /// 此操作会将消息标记为已删除状态，而不是物理删除。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="204">消息删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteMessage(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.DeleteMessageAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "消息不存在" });
            }

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功删除消息，消息ID: {MessageId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "删除消息时发生内部错误" });
        }
    }

    #endregion

    #region 未读消息管理

    /// <summary>
    /// 获取未读消息列表
    /// </summary>
    /// <param name="filter">消息筛选条件</param>
    /// <returns>未读消息列表</returns>
    /// <remarks>
    /// 获取当前用户的未读消息列表。
    /// 
    /// 示例请求：
    /// GET /api/messages/unread?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取未读消息列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("unread")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetUnreadMessages([FromQuery] MessageFilterDto filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 设置筛选条件为只查询未读消息
            filter.CurrentUserId = currentUserId.Value;
            filter.IsRead = false;

            var result = await _messageService.GetUserMessagesAsync(currentUserId.Value, filter, currentUserId.Value);

            _logger.LogInformation("获取未读消息列表成功，用户ID: {UserId}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读消息列表时发生错误");
            return StatusCode(500, new { message = "获取未读消息列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 标记消息为已读
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>标记结果</returns>
    /// <remarks>
    /// 将指定消息标记为已读状态。
    /// 
    /// 示例请求：
    /// PUT /api/messages/12345678-1234-1234-1234-123456789012/read
    /// </remarks>
    /// <response code="200">消息标记已读成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("{id:guid}/read")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MarkMessageAsRead(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.MarkMessageAsReadAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "消息不存在" });
            }

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功标记消息为已读，消息ID: {MessageId}", 
                currentUserId.Value, id);

            return Ok(new { message = "消息标记已读成功" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限标记消息已读: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("标记消息已读参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记消息已读时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "标记消息已读时发生内部错误" });
        }
    }

    #endregion

    #region 消息附件管理

    /// <summary>
    /// 上传消息附件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="file">附件文件</param>
    /// <returns>上传的附件信息</returns>
    /// <remarks>
    /// 为指定消息上传附件文件。
    /// 
    /// 示例请求：
    /// POST /api/messages/12345678-1234-1234-1234-123456789012/attachments
    /// </remarks>
    /// <response code="201">附件上传成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="413">文件过大</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{id:guid}/attachments")]
    [Authorize]
    [ProducesResponseType(typeof(MessageAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB 限制
    public async Task<ActionResult<MessageAttachmentDto>> UploadAttachment(Guid id, IFormFile file)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "文件不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            using var stream = file.OpenReadStream();
            var attachment = await _messageService.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功上传消息附件，消息ID: {MessageId}, 附件ID: {AttachmentId}, 文件名: {FileName}", 
                currentUserId.Value, id, attachment.Id, file.FileName);

            return CreatedAtAction(nameof(GetAttachment), new { messageId = id, attachmentId = attachment.Id }, attachment);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限上传消息附件: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("上传消息附件参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传消息附件时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "上传消息附件时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取消息附件列表
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>附件列表</returns>
    /// <remarks>
    /// 获取指定消息的所有附件。
    /// 
    /// 示例请求：
    /// GET /api/messages/12345678-1234-1234-1234-123456789012/attachments
    /// </remarks>
    /// <response code="200">成功获取附件列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">消息不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}/attachments")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<MessageAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<IEnumerable<MessageAttachmentDto>>> GetMessageAttachments(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var attachments = await _messageService.GetMessageAttachmentsAsync(id, currentUserId.Value);

            _logger.LogInformation("获取消息附件列表成功，消息ID: {MessageId}, 附件数量: {Count}", 
                id, attachments.Count());

            return Ok(attachments);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取消息附件: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取消息附件参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息附件时发生错误，消息ID: {MessageId}", id);
            return StatusCode(500, new { message = "获取消息附件时发生内部错误" });
        }
    }

    /// <summary>
    /// 下载消息附件
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>附件文件流</returns>
    /// <remarks>
    /// 下载指定附件文件。
    /// 
    /// 示例请求：
    /// GET /api/messages/attachments/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功下载附件</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">附件不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("attachments/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DownloadAttachment(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "附件ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var fileStream = await _messageService.DownloadAttachmentAsync(id, currentUserId.Value);
            var attachmentInfo = await _messageService.GetAttachmentInfoAsync(id, currentUserId.Value);

            if (attachmentInfo == null)
            {
                return NotFound(new { message = "附件不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功下载消息附件，附件ID: {AttachmentId}", 
                currentUserId.Value, id);

            return File(fileStream, attachmentInfo.ContentType ?? "application/octet-stream", attachmentInfo.OriginalFileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限下载消息附件: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("下载消息附件参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载消息附件时发生错误，消息ID: {MessageId}, 附件ID: {AttachmentId}", 
                messageId, attachmentId);
            return StatusCode(500, new { message = "下载消息附件时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除消息附件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="attachmentId">附件ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 删除指定消息的附件。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/12345678-1234-1234-1234-123456789012/attachments/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="204">附件删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">附件不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("{messageId:guid}/attachments/{attachmentId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAttachment(Guid messageId, Guid attachmentId)
    {
        try
        {
            // 输入验证
            if (messageId == Guid.Empty || attachmentId == Guid.Empty)
            {
                return BadRequest(new { message = "消息ID和附件ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.DeleteAttachmentAsync(attachmentId, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "附件不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功删除消息附件，消息ID: {MessageId}, 附件ID: {AttachmentId}", 
                currentUserId.Value, messageId, attachmentId);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除消息附件: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除消息附件参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息附件时发生错误，消息ID: {MessageId}, 附件ID: {AttachmentId}", 
                messageId, attachmentId);
            return StatusCode(500, new { message = "删除消息附件时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取附件信息
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>附件详细信息</returns>
    /// <remarks>
    /// 获取指定附件的详细信息，包括文件信息、上传时间等。
    /// 
    /// 示例请求：
    /// GET /api/messages/attachments/12345678-1234-1234-1234-123456789012/info
    /// </remarks>
    /// <response code="200">成功获取附件信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">附件不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("attachments/{id:guid}/info")]
    [Authorize]
    [ProducesResponseType(typeof(MessageAttachmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<MessageAttachmentDto>> GetAttachmentInfo(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "附件ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var attachment = await _messageService.GetAttachmentInfoAsync(id, currentUserId.Value);

            if (attachment == null)
            {
                return NotFound(new { message = "附件不存在" });
            }

            _logger.LogInformation("获取附件信息成功，附件ID: {AttachmentId}", id);
            return Ok(attachment);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取附件信息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取附件信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取附件信息时发生错误，附件ID: {AttachmentId}", id);
            return StatusCode(500, new { message = "获取附件信息时发生内部错误" });
        }
    }

    #endregion

    #region 消息草稿管理

    /// <summary>
    /// 创建消息草稿
    /// </summary>
    /// <param name="createDraftDto">创建草稿的请求参数</param>
    /// <returns>创建的草稿信息</returns>
    /// <remarks>
    /// 创建新的消息草稿，支持定时发送和附件。
    /// 
    /// 示例请求：
    /// POST /api/messages/drafts
    /// {
    ///   "receiverId": "12345678-1234-1234-1234-123456789012",
    ///   "subject": "草稿主题",
    ///   "content": "草稿内容",
    ///   "isScheduled": false
    /// }
    /// </remarks>
    /// <response code="201">草稿创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("drafts")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDraftDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDraftDto>> CreateDraft([FromBody] CreateMessageDraftDto createDraftDto)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var draft = await _messageService.CreateDraftAsync(createDraftDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功创建消息草稿，草稿ID: {DraftId}", 
                currentUserId.Value, draft.Id);

            return CreatedAtAction(nameof(GetDraft), new { id = draft.Id }, draft);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建消息草稿参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建消息草稿时发生错误");
            return StatusCode(500, new { message = "创建消息草稿时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取消息草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿详细信息</returns>
    /// <remarks>
    /// 获取指定草稿的详细信息。
    /// 
    /// 示例请求：
    /// GET /api/messages/drafts/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取草稿信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">草稿不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("drafts/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDraftDto>> GetDraft(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "草稿ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var draft = await _messageService.GetDraftAsync(id, currentUserId.Value);

            if (draft == null)
            {
                return NotFound(new { message = "草稿不存在" });
            }

            _logger.LogInformation("获取消息草稿成功，草稿ID: {DraftId}", id);
            return Ok(draft);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取消息草稿: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取消息草稿参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息草稿时发生错误，草稿ID: {DraftId}", id);
            return StatusCode(500, new { message = "获取消息草稿时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取用户草稿列表
    /// </summary>
    /// <param name="filter">草稿筛选条件</param>
    /// <returns>草稿列表</returns>
    /// <remarks>
    /// 获取当前用户的草稿列表。
    /// 
    /// 示例请求：
    /// GET /api/messages/drafts?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取草稿列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("drafts")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<MessageDraftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<MessageDraftDto>>> GetUserDrafts([FromQuery] MessageDraftFilterDto filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _messageService.GetUserDraftsAsync(currentUserId.Value, filter, currentUserId.Value);

            _logger.LogInformation("获取用户草稿列表成功，用户ID: {UserId}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户草稿列表时发生错误");
            return StatusCode(500, new { message = "获取用户草稿列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新消息草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <param name="updateDraftDto">更新草稿的请求参数</param>
    /// <returns>更新后的草稿信息</returns>
    /// <remarks>
    /// 更新指定草稿的内容。
    /// 
    /// 示例请求：
    /// PUT /api/messages/drafts/12345678-1234-1234-1234-123456789012
    /// {
    ///   "subject": "更新的草稿主题",
    ///   "content": "更新的草稿内容"
    /// }
    /// </remarks>
    /// <response code="200">草稿更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">草稿不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("drafts/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDraftDto>> UpdateDraft(Guid id, [FromBody] UpdateMessageDraftDto updateDraftDto)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "草稿ID不能为空" });
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

            var draft = await _messageService.UpdateDraftAsync(id, updateDraftDto, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功更新消息草稿，草稿ID: {DraftId}", 
                currentUserId.Value, id);

            return Ok(draft);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限更新消息草稿: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新消息草稿参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息草稿时发生错误，草稿ID: {DraftId}", id);
            return StatusCode(500, new { message = "更新消息草稿时发生内部错误" });
        }
    }

    /// <summary>
    /// 发送草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>发送后的消息信息</returns>
    /// <remarks>
    /// 将草稿作为正式消息发送。
    /// 
    /// 示例请求：
    /// POST /api/messages/drafts/12345678-1234-1234-1234-123456789012/send
    /// </remarks>
    /// <response code="200">草稿发送成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">草稿不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("drafts/{id:guid}/send")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDto>> SendDraft(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "草稿ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var message = await _messageService.SendDraftAsync(id, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功发送消息草稿，草稿ID: {DraftId}, 消息ID: {MessageId}", 
                currentUserId.Value, id, message.Id);

            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限发送消息草稿: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("发送消息草稿参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息草稿时发生错误，草稿ID: {DraftId}", id);
            return StatusCode(500, new { message = "发送消息草稿时发生内部错误" });
        }
    }

    /// <summary>
    /// 定时发送草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <param name="request">定时发送请求</param>
    /// <returns>定时发送结果</returns>
    /// <remarks>
    /// 设置草稿在指定时间自动发送。
    /// 
    /// 示例请求：
    /// POST /api/messages/drafts/12345678-1234-1234-1234-123456789012/schedule
    /// {
    ///   "scheduledTime": "2024-12-25T10:00:00Z",
    ///   "timezone": "UTC"
    /// }
    /// </remarks>
    /// <response code="200">定时发送设置成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">草稿不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("drafts/{id:guid}/schedule")]
    [Authorize]
    [ProducesResponseType(typeof(ScheduleDraftResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ScheduleDraftResultDto>> ScheduleDraft(Guid id, [FromBody] ScheduleDraftDto request)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "草稿ID不能为空" });
            }

            if (request == null)
            {
                return BadRequest(new { message = "定时发送请求不能为空" });
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

            var result = await _messageService.ScheduleDraftAsync(id, request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功设置草稿定时发送，草稿ID: {DraftId}, 计划发送时间: {ScheduledTime}", 
                currentUserId.Value, id, result.ScheduledTime);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限设置草稿定时发送: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("设置草稿定时发送参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置草稿定时发送时发生错误，草稿ID: {DraftId}", id);
            return StatusCode(500, new { message = "设置草稿定时发送时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除消息草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 删除指定的草稿。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/drafts/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="204">草稿删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">草稿不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("drafts/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteDraft(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "草稿ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.DeleteDraftAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "草稿不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功删除消息草稿，草稿ID: {DraftId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除消息草稿: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除消息草稿参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息草稿时发生错误，草稿ID: {DraftId}", id);
            return StatusCode(500, new { message = "删除消息草稿时发生内部错误" });
        }
    }

    #endregion

    #region 会话管理

    /// <summary>
    /// 创建会话
    /// </summary>
    /// <param name="request">创建会话的请求参数</param>
    /// <returns>创建的会话信息</returns>
    /// <remarks>
    /// 创建新的消息会话，可以添加多个参与者。
    /// 
    /// 示例请求：
    /// POST /api/messages/conversations
    /// {
    ///   "subject": "项目讨论",
    ///   "description": "关于新项目的讨论",
    ///   "participantIds": ["12345678-1234-1234-1234-123456789012"],
    ///   "initialMessage": "欢迎加入项目讨论！"
    /// }
    /// </remarks>
    /// <response code="201">会话创建成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("conversations")]
    [Authorize]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConversationDto>> CreateConversation([FromBody] CreateConversationDto request)
    {
        try
        {
            // 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var conversation = await _messageService.CreateConversationAsync(request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功创建会话，会话ID: {ConversationId}, 参与者数量: {ParticipantCount}", 
                currentUserId.Value, conversation.Id, conversation.ParticipantIds.Count);

            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建会话参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建会话时发生错误");
            return StatusCode(500, new { message = "创建会话时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取会话列表
    /// </summary>
    /// <param name="filter">会话筛选条件</param>
    /// <returns>会话列表</returns>
    /// <remarks>
    /// 获取当前用户的会话列表。
    /// 
    /// 示例请求：
    /// GET /api/messages/conversations?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取会话列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("conversations")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<ConversationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<ConversationDto>>> GetConversations([FromQuery] MessageConversationFilterDto filter)
    {
        try
        {
            // 输入验证
            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _messageService.GetConversationsAsync(currentUserId.Value, filter, currentUserId.Value);

            _logger.LogInformation("获取会话列表成功，用户ID: {UserId}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话列表时发生错误");
            return StatusCode(500, new { message = "获取会话列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取会话详情
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <returns>会话详细信息</returns>
    /// <remarks>
    /// 获取指定会话的详细信息。
    /// 
    /// 示例请求：
    /// GET /api/messages/conversations/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取会话信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("conversations/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var conversation = await _messageService.GetConversationAsync(id, currentUserId.Value);

            if (conversation == null)
            {
                return NotFound(new { message = "会话不存在" });
            }

            _logger.LogInformation("获取会话信息成功，会话ID: {ConversationId}", id);
            return Ok(conversation);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取会话信息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取会话信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话信息时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "获取会话信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新会话
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <param name="request">更新会话的请求参数</param>
    /// <returns>更新后的会话信息</returns>
    /// <remarks>
    /// 更新指定会话的信息，只有会话创建者或管理员可以更新会话。
    /// 
    /// 示例请求：
    /// PUT /api/messages/conversations/12345678-1234-1234-1234-123456789012
    /// {
    ///   "subject": "更新的会话主题",
    ///   "description": "更新的会话描述"
    /// }
    /// </remarks>
    /// <response code="200">会话更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("conversations/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ConversationDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationDto request)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
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

            var conversation = await _messageService.UpdateConversationAsync(id, request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功更新会话，会话ID: {ConversationId}", 
                currentUserId.Value, id);

            return Ok(conversation);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限更新会话: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新会话参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新会话时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "更新会话时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 删除指定会话，只有会话创建者或管理员可以删除会话。
    /// 此操作会将会话标记为已删除状态，而不是物理删除。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/conversations/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="204">会话删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("conversations/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteConversation(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.DeleteConversationAsync(id, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "会话不存在" });
            }

            _logger.LogInformation("用户 {UserId} 成功删除会话，会话ID: {ConversationId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限删除会话: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("删除会话参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除会话时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "删除会话时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取会话消息列表
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <param name="filter">消息筛选条件</param>
    /// <returns>会话消息列表</returns>
    /// <remarks>
    /// 获取指定会话的消息列表。
    /// 
    /// 示例请求：
    /// GET /api/messages/conversations/12345678-1234-1234-1234-123456789012/messages?page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">成功获取会话消息列表</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("conversations/{id:guid}/messages")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetConversationMessages(Guid id, [FromQuery] MessageFilterDto filter)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
            }

            if (filter == null)
            {
                return BadRequest(new { message = "筛选参数不能为空" });
            }

            if (filter.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            filter.ConversationId = id;
            filter.CurrentUserId = currentUserId.Value;

            var result = await _messageService.GetConversationMessagesAsync(id, filter, currentUserId.Value);

            _logger.LogInformation("获取会话消息列表成功，会话ID: {ConversationId}, 页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                id, filter.Page, filter.PageSize, result.TotalCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取会话消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取会话消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话消息时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "获取会话消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 发送会话消息
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <param name="request">发送消息的请求参数</param>
    /// <returns>发送的消息信息</returns>
    /// <remarks>
    /// 向指定会话发送消息。
    /// 
    /// 示例请求：
    /// POST /api/messages/conversations/12345678-1234-1234-1234-123456789012/messages
    /// {
    ///   "content": "大家好，我想讨论一下这个话题",
    ///   "priority": 1
    /// }
    /// </remarks>
    /// <response code="201">会话消息发送成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("conversations/{id:guid}/messages")]
    [Authorize]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageDto>> SendConversationMessage(Guid id, [FromBody] CreateMessageDto request)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
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

            request.ConversationId = id;
            var message = await _messageService.SendConversationMessageAsync(id, request, currentUserId.Value);

            // 清除相关缓存
            ClearConversationCache(id);

            _logger.LogInformation("用户 {UserId} 成功发送会话消息，会话ID: {ConversationId}, 消息ID: {MessageId}", 
                currentUserId.Value, id, message.Id);

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限发送会话消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("发送会话消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送会话消息时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "发送会话消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 标记会话为已读
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <returns>标记结果</returns>
    /// <remarks>
    /// 将指定会话的所有消息标记为已读。
    /// 
    /// 示例请求：
    /// POST /api/messages/conversations/12345678-1234-1234-1234-123456789012/read
    /// </remarks>
    /// <response code="200">会话标记已读成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("conversations/{id:guid}/read")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MarkConversationAsRead(Guid id)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var count = await _messageService.MarkConversationAsReadAsync(id, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功标记会话为已读，会话ID: {ConversationId}, 标记消息数量: {Count}", 
                currentUserId.Value, id, count);

            return Ok(new { 
                message = "会话标记已读成功",
                markedCount = count
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限标记会话已读: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("标记会话已读参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记会话已读时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "标记会话已读时发生内部错误" });
        }
    }

    /// <summary>
    /// 添加会话参与者
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <param name="request">添加参与者请求</param>
    /// <returns>添加结果</returns>
    /// <remarks>
    /// 向指定会话添加新的参与者。
    /// 
    /// 示例请求：
    /// POST /api/messages/conversations/12345678-1234-1234-1234-123456789012/participants
    /// {
    ///   "participantIds": ["12345678-1234-1234-1234-123456789012"],
    ///   "message": "欢迎加入我们的讨论！"
    /// }
    /// </remarks>
    /// <response code="200">参与者添加成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("conversations/{id:guid}/participants")]
    [Authorize]
    [ProducesResponseType(typeof(AddParticipantResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddParticipantResultDto>> AddConversationParticipants(Guid id, [FromBody] AddConversationParticipantsDto request)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID不能为空" });
            }

            if (request == null || request.ParticipantIds == null || !request.ParticipantIds.Any())
            {
                return BadRequest(new { message = "参与者ID列表不能为空" });
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

            var result = await _messageService.AddConversationParticipantsAsync(id, request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功添加会话参与者，会话ID: {ConversationId}, 添加数量: {SuccessCount}/{TotalCount}", 
                currentUserId.Value, id, result.SuccessCount, request.ParticipantIds.Count);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限添加会话参与者: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("添加会话参与者参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加会话参与者时发生错误，会话ID: {ConversationId}", id);
            return StatusCode(500, new { message = "添加会话参与者时发生内部错误" });
        }
    }

    /// <summary>
    /// 移除会话参与者
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <param name="userId">要移除的用户ID</param>
    /// <returns>移除结果</returns>
    /// <remarks>
    /// 从指定会话中移除参与者。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/conversations/12345678-1234-1234-1234-123456789012/participants/12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">参与者移除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="404">会话不存在或用户不是参与者</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("conversations/{id:guid}/participants/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveConversationParticipant(Guid id, Guid userId)
    {
        try
        {
            // 输入验证
            if (id == Guid.Empty || userId == Guid.Empty)
            {
                return BadRequest(new { message = "会话ID和用户ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var success = await _messageService.RemoveConversationParticipantAsync(id, userId, currentUserId.Value);

            if (!success)
            {
                return NotFound(new { message = "会话不存在或用户不是参与者" });
            }

            _logger.LogInformation("用户 {UserId} 成功移除会话参与者，会话ID: {ConversationId}, 被移除用户ID: {RemovedUserId}", 
                currentUserId.Value, id, userId);

            return Ok(new { message = "参与者移除成功" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限移除会话参与者: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("移除会话参与者参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除会话参与者时发生错误，会话ID: {ConversationId}, 用户ID: {UserId}", 
                id, userId);
            return StatusCode(500, new { message = "移除会话参与者时发生内部错误" });
        }
    }

    #endregion

    #region 消息元数据管理

    /// <summary>
    /// 获取消息类型列表
    /// </summary>
    /// <returns>消息类型列表</returns>
    /// <remarks>
    /// 获取系统支持的所有消息类型。
    /// 
    /// 示例请求：
    /// GET /api/messages/types
    /// </remarks>
    /// <response code="200">成功获取消息类型列表</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("types")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<MessageTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<IEnumerable<MessageTypeDto>>> GetMessageTypes()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var types = await _messageService.GetMessageTypesAsync();

            _logger.LogInformation("获取消息类型列表成功，类型数量: {Count}", types.Count());

            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息类型列表时发生错误");
            return StatusCode(500, new { message = "获取消息类型列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取消息优先级列表
    /// </summary>
    /// <returns>消息优先级列表</returns>
    /// <remarks>
    /// 获取系统支持的所有消息优先级。
    /// 
    /// 示例请求：
    /// GET /api/messages/priorities
    /// </remarks>
    /// <response code="200">成功获取消息优先级列表</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("priorities")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<MessagePriorityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<IEnumerable<MessagePriorityDto>>> GetMessagePriorities()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var priorities = await _messageService.GetMessagePrioritiesAsync();

            _logger.LogInformation("获取消息优先级列表成功，优先级数量: {Count}", priorities.Count());

            return Ok(priorities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息优先级列表时发生错误");
            return StatusCode(500, new { message = "获取消息优先级列表时发生内部错误" });
        }
    }

    #endregion

    #region 批量操作和工具方法

    /// <summary>
    /// 批量发送消息
    /// </summary>
    /// <param name="request">批量发送请求</param>
    /// <returns>发送结果</returns>
    /// <remarks>
    /// 批量发送消息给多个接收者。
    /// 
    /// 示例请求：
    /// POST /api/messages/bulk
    /// {
    ///   "receiverIds": ["12345678-1234-1234-1234-123456789012"],
    ///   "subject": "批量消息主题",
    ///   "content": "批量消息内容",
    ///   "messageType": 0,
    ///   "priority": 1
    /// }
    /// </remarks>
    /// <response code="200">批量发送成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="429">发送频率限制</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(BulkSendMessageResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkSendMessageResultDto>> BulkSendMessages([FromBody] BulkSendMessageDto request)
    {
        try
        {
            // 输入验证
            if (request == null || request.ReceiverIds == null || !request.ReceiverIds.Any())
            {
                return BadRequest(new { message = "接收者ID列表不能为空" });
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

            var result = await _messageService.BulkSendMessagesAsync(request, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功批量发送消息，接收者数量: {ReceiverCount}, 成功数量: {SuccessCount}", 
                currentUserId.Value, request.ReceiverIds.Count, result.SuccessCount);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限批量发送消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("批量发送消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量发送消息时发生错误");
            return StatusCode(500, new { message = "批量发送消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量标记消息为已读
    /// </summary>
    /// <param name="request">批量标记请求</param>
    /// <returns>标记结果</returns>
    /// <remarks>
    /// 批量将指定消息标记为已读状态。
    /// 
    /// 示例请求：
    /// PUT /api/messages/bulk/read
    /// {
    ///   "messageIds": ["12345678-1234-1234-1234-123456789012"]
    /// }
    /// </remarks>
    /// <response code="200">批量标记成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("bulk/read")]
    [Authorize]
    [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkOperationResultDto>> BulkMarkMessagesAsRead([FromBody] BulkMessageIdsDto request)
    {
        try
        {
            // 输入验证
            if (request == null || request.MessageIds == null || !request.MessageIds.Any())
            {
                return BadRequest(new { message = "消息ID列表不能为空" });
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

            var result = await _messageService.BulkMarkMessagesAsReadAsync(request, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功批量标记消息为已读，处理数量: {SuccessCount}/{TotalCount}", 
                currentUserId.Value, result.SuccessCount, request.MessageIds.Count);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限批量标记消息已读: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("批量标记消息已读参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记消息已读时发生错误");
            return StatusCode(500, new { message = "批量标记消息已读时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量删除消息
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <returns>删除结果</returns>
    /// <remarks>
    /// 批量删除指定消息。
    /// 
    /// 示例请求：
    /// DELETE /api/messages/bulk
    /// {
    ///   "messageIds": ["12345678-1234-1234-1234-123456789012"]
    /// }
    /// </remarks>
    /// <response code="200">批量删除成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(BulkOperationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkOperationResultDto>> BulkDeleteMessages([FromBody] BulkMessageIdsDto request)
    {
        try
        {
            // 输入验证
            if (request == null || request.MessageIds == null || !request.MessageIds.Any())
            {
                return BadRequest(new { message = "消息ID列表不能为空" });
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

            var result = await _messageService.BulkDeleteMessagesAsync(request, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功批量删除消息，处理数量: {SuccessCount}/{TotalCount}", 
                currentUserId.Value, result.SuccessCount, request.MessageIds.Count);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限批量删除消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("批量删除消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除消息时发生错误");
            return StatusCode(500, new { message = "批量删除消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量操作消息
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <returns>操作结果</returns>
    /// <remarks>
    /// 批量操作消息，支持标记已读、删除等操作。
    /// 
    /// 示例请求：
    /// POST /api/messages/batch
    /// {
    ///   "messageIds": ["12345678-1234-1234-1234-123456789012"],
    ///   "operation": 0,
    ///   "reason": "批量标记已读"
    /// }
    /// </remarks>
    /// <response code="200">批量操作成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("batch")]
    [Authorize]
    [ProducesResponseType(typeof(BatchOperationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BatchOperationResultDto>> BatchOperationMessages([FromBody] BatchMessageOperationDto request)
    {
        try
        {
            // 输入验证
            if (request == null || request.MessageIds == null || !request.MessageIds.Any())
            {
                return BadRequest(new { message = "消息ID列表不能为空" });
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

            var result = await _messageService.BatchDeleteMessagesAsync(request, currentUserId.Value);

            // 清除相关缓存
            ClearMessageCache(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 成功批量操作消息，操作类型: {Operation}, 处理数量: {SuccessCount}/{TotalCount}", 
                currentUserId.Value, request.Operation, result.SuccessCount, request.MessageIds.Count);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限批量操作消息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("批量操作消息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量操作消息时发生错误");
            return StatusCode(500, new { message = "批量操作消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 搜索消息
    /// </summary>
    /// <param name="request">搜索请求</param>
    /// <returns>搜索结果</returns>
    /// <remarks>
    /// 在消息内容中搜索关键词。
    /// 
    /// 示例请求：
    /// GET /api/messages/search?q=代码片段&amp;page=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">搜索成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(MessageSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageSearchResultDto>> SearchMessages([FromQuery] MessageSearchRequestDto request)
    {
        try
        {
            // 输入验证
            if (string.IsNullOrEmpty(request.Query))
            {
                return BadRequest(new { message = "搜索关键词不能为空" });
            }

            if (request.Page < 1)
            {
                return BadRequest(new { message = "页码必须大于0" });
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                return BadRequest(new { message = "每页大小必须在1-100之间" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var filter = new MessageFilterDto
            {
                Search = request.Query,
                Page = request.Page,
                PageSize = request.PageSize,
                CurrentUserId = currentUserId.Value
            };

            var result = await _messageService.SearchMessagesAsync(request.Query, filter, currentUserId.Value);

            _logger.LogInformation("搜索消息成功，关键词: {Query}, 结果数量: {Count}, 搜索时间: {SearchTime}ms", 
                request.Query, result.Messages.Count, result.SearchTime);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索消息时发生错误，关键词: {Query}", request?.Query);
            return StatusCode(500, new { message = "搜索消息时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取消息统计信息
    /// </summary>
    /// <param name="userId">用户ID（可选，默认为当前用户）</param>
    /// <returns>统计信息</returns>
    /// <remarks>
    /// 获取用户的消息统计信息。如果不指定用户ID，则返回当前用户的统计信息。
    /// 
    /// 示例请求：
    /// GET /api/messages/stats
    /// GET /api/messages/stats?userId=12345678-1234-1234-1234-123456789012
    /// </remarks>
    /// <response code="200">成功获取统计信息</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(MessageStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<ActionResult<MessageStatsDto>> GetMessageStats([FromQuery] Guid? userId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            // 如果没有指定用户ID，使用当前用户ID
            var targetUserId = userId ?? currentUserId.Value;

            var filter = new MessageStatsFilterDto();
            var stats = await _messageService.GetUserMessageStatsAsync(targetUserId, filter);

            _logger.LogInformation("获取消息统计信息成功，用户ID: {UserId}", targetUserId);
            return Ok(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限获取消息统计信息: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("获取消息统计信息参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户消息统计信息时发生错误，用户ID: {UserId}", userId);
            return StatusCode(500, new { message = "获取消息统计信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 导出用户消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="options">导出选项</param>
    /// <returns>导出文件</returns>
    /// <remarks>
    /// 导出指定用户的消息数据。
    /// 
    /// 示例请求：
    /// GET /api/messages/12345678-1234-1234-1234-123456789012/export?format=0
    /// </remarks>
    /// <response code="200">导出成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="401">用户未认证</response>
    /// <response code="403">用户权限不足</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{userId:guid}/export")]
    [Authorize]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportUserMessages(Guid userId, [FromQuery] MessageExportOptionsDto options)
    {
        try
        {
            // 输入验证
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "用户ID不能为空" });
            }

            if (options == null)
            {
                return BadRequest(new { message = "导出选项不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var fileStream = await _messageService.ExportUserMessagesAsync(userId, options, currentUserId.Value);
            var contentType = options.Format switch
            {
                ExportFormat.Json => "application/json",
                ExportFormat.Csv => "text/csv",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.Pdf => "application/pdf",
                _ => "application/octet-stream"
            };

            var fileName = $"messages_export_{userId}_{DateTime.UtcNow:yyyyMMdd}.{options.Format.ToString().ToLower()}";

            _logger.LogInformation("用户 {UserId} 成功导出消息数据，用户ID: {TargetUserId}, 格式: {Format}", 
                currentUserId.Value, userId, options.Format);

            return File(fileStream, contentType, fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限导出消息数据: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("导出消息数据参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出用户消息数据时发生错误，用户ID: {UserId}", userId);
            return StatusCode(500, new { message = "导出消息数据时发生内部错误" });
        }
    }

    #endregion

    #region 私有方法

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
    /// 生成缓存键
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>缓存键</returns>
    private string GenerateCacheKey(MessageFilterDto filter)
    {
        var key = $"messages_{filter.CurrentUserId}_{filter.Page}_{filter.PageSize}_{filter.SortBy}";
        if (filter.SenderId.HasValue)
            key += $"_{filter.SenderId}";
        if (filter.ReceiverId.HasValue)
            key += $"_{filter.ReceiverId}";
        if (filter.MessageType.HasValue)
            key += $"_{filter.MessageType}";
        if (filter.Status.HasValue)
            key += $"_{filter.Status}";
        if (filter.Priority.HasValue)
            key += $"_{filter.Priority}";
        if (filter.ConversationId.HasValue)
            key += $"_{filter.ConversationId}";
        if (!string.IsNullOrEmpty(filter.Tag))
            key += $"_{filter.Tag}";
        if (filter.IsRead.HasValue)
            key += $"_{filter.IsRead}";
        if (!string.IsNullOrEmpty(filter.Search))
            key += $"_{filter.Search}";
        
        return key;
    }

    /// <summary>
    /// 判断是否应该使用缓存
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>是否使用缓存</returns>
    private bool ShouldUseCache(MessageFilterDto filter)
    {
        // 只对简单的查询使用缓存
        return filter != null && 
               filter.CurrentUserId.HasValue && 
               !filter.SenderId.HasValue &&
               !filter.ReceiverId.HasValue &&
               filter.MessageType == null &&
               filter.Status == null &&
               filter.Priority == null &&
               filter.ConversationId == null &&
               string.IsNullOrEmpty(filter.Tag) &&
               filter.IsRead == null &&
               string.IsNullOrEmpty(filter.Search) &&
               filter.Page <= 5 && // 只缓存前5页
               filter.PageSize <= 20;
    }

    /// <summary>
    /// 清除消息相关缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    private void ClearMessageCache(Guid userId)
    {
        var keysToRemove = _messageCache.Keys
            .Where(key => key.StartsWith($"messages_{userId}"))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _messageCache.TryRemove(key, out _);
        }

        _logger.LogDebug("清除消息缓存成功，用户ID: {UserId}", userId);
    }

    /// <summary>
    /// 清除会话相关缓存
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    private void ClearConversationCache(Guid conversationId)
    {
        // 这里可以添加会话缓存清除逻辑
        _logger.LogDebug("清除会话缓存成功，会话ID: {ConversationId}", conversationId);
    }

    #endregion
}

/// <summary>
/// 缓存条目
/// </summary>
internal class CacheEntry
{
    public PaginatedResult<MessageDto> Data { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// 消息搜索请求DTO
/// </summary>
public class MessageSearchRequestDto
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    [Required(ErrorMessage = "搜索关键词不能为空")]
    [StringLength(100, ErrorMessage = "搜索关键词长度不能超过100个字符")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// 页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 消息统计筛选条件DTO
/// </summary>
public class MessageStatsFilterDto
{
    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }
}