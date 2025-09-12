using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.Controllers;

/// <summary>
/// 通知控制器 - 遵循单一职责原则，只负责通知相关的HTTP请求处理
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 通知CRUD操作

    /// <summary>
    /// 获取通知列表 - 支持搜索、筛选和分页
    /// </summary>
    /// <param name="type">通知类型筛选</param>
    /// <param name="priority">优先级筛选</param>
    /// <param name="status">状态筛选</param>
    /// <param name="isRead">是否已读筛选</param>
    /// <param name="channel">通知渠道筛选</param>
    /// <param name="search">搜索关键词</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="sortBy">排序方式</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页的通知列表</returns>
    [HttpGet]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<PaginatedResult<NotificationDto>>> GetNotifications(
        [FromQuery] NotificationType? type = null,
        [FromQuery] NotificationPriority? priority = null,
        [FromQuery] NotificationStatus? status = null,
        [FromQuery] bool? isRead = null,
        [FromQuery] NotificationChannel? channel = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] NotificationSort sortBy = NotificationSort.CreatedAtDesc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
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

            var filter = new NotificationFilterDto
            {
                UserId = currentUserId.Value,
                Type = type,
                Priority = priority,
                Status = status,
                IsRead = isRead,
                Channel = channel,
                Search = search,
                StartDate = startDate,
                EndDate = endDate,
                Sort = sortBy,
                Page = page,
                PageSize = pageSize
            };

            var result = await _notificationService.GetNotificationsAsync(filter, currentUserId.Value);
            
            _logger.LogInformation("用户 {UserId} 获取通知列表成功，页码: {Page}, 每页大小: {PageSize}, 总数: {TotalCount}", 
                currentUserId.Value, page, pageSize, result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知列表时发生错误");
            return StatusCode(500, new { message = "获取通知列表时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取单个通知详情
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>通知详情</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var notification = await _notificationService.GetNotificationByIdAsync(id, currentUserId.Value);
            if (notification == null)
            {
                return NotFound(new { message = "通知不存在或无权限访问" });
            }

            _logger.LogInformation("用户 {UserId} 获取通知详情成功，ID: {NotificationId}", 
                currentUserId.Value, id);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知详情时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "获取通知详情时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建新通知
    /// </summary>
    /// <param name="request">创建通知请求</param>
    /// <returns>创建的通知</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var sendRequest = new NotificationSendRequestDto
            {
                UserIds = new List<Guid> { request.UserId },
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                Message = request.Message,
                Priority = request.Priority,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                TriggeredByUserId = request.TriggeredByUserId,
                Action = request.Action,
                Channels = request.Channel != null ? new List<NotificationChannel> { request.Channel } : new List<NotificationChannel>(),
                ExpiresAt = request.ExpiresAt,
                ScheduledToSendAt = request.ScheduledToSendAt,
                DataJson = request.DataJson,
                Tag = request.Tag,
                Icon = request.Icon,
                Color = request.Color,
                RequiresConfirmation = request.RequiresConfirmation
            };

            var notification = await _notificationService.CreateAndSendNotificationAsync(sendRequest, currentUserId.Value);

            // 实时推送通知
            await _hubContext.Clients.User(request.UserId.ToString()).SendAsync("ReceiveNotification", notification);

            _logger.LogInformation("用户 {UserId} 创建通知成功，ID: {NotificationId}", 
                currentUserId.Value, notification.Id);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户无权限创建通知: {Message}", ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("创建通知参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知时发生错误");
            return StatusCode(500, new { message = "创建通知时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新通知内容
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的通知</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> UpdateNotification(Guid id, [FromBody] UpdateNotificationDto request)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
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

            var notification = await _notificationService.UpdateNotificationAsync(id, request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 更新通知成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(notification);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限更新通知 {NotificationId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("更新通知参数错误: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "更新通知时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="permanentDelete">是否永久删除</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteNotification(Guid id, [FromQuery] bool permanentDelete = false)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = permanentDelete 
                ? await _notificationService.PermanentDeleteNotificationAsync(id, currentUserId.Value)
                : await _notificationService.DeleteNotificationAsync(id, currentUserId.Value);

            if (!result)
            {
                return NotFound(new { message = "通知不存在" });
            }

            _logger.LogInformation("用户 {UserId} 删除通知成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("用户 {UserId} 无权限删除通知 {NotificationId}: {Message}", 
                GetCurrentUserId(), id, ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "删除通知时发生内部错误" });
        }
    }

    #endregion

    #region 通知状态管理

    /// <summary>
    /// 标记通知为已读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>更新后的通知</returns>
    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var updateRequest = new NotificationStatusUpdateDto
            {
                Status = NotificationStatus.Read,
                IsRead = true,
                ReadAt = DateTime.UtcNow
            };

            var notification = await _notificationService.UpdateNotificationStatusAsync(id, updateRequest, currentUserId.Value);

            // 通知前端更新
            await _hubContext.Clients.User(currentUserId.Value.ToString()).SendAsync("NotificationRead", id);

            _logger.LogInformation("用户 {UserId} 标记通知已读成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知已读时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "标记通知已读时发生内部错误" });
        }
    }

    /// <summary>
    /// 标记通知为未读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>更新后的通知</returns>
    [HttpPost("{id:guid}/unread")]
    public async Task<ActionResult<NotificationDto>> MarkAsUnread(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var updateRequest = new NotificationStatusUpdateDto
            {
                Status = NotificationStatus.Unread,
                IsRead = false,
                ReadAt = null
            };

            var notification = await _notificationService.UpdateNotificationStatusAsync(id, updateRequest, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 标记通知未读成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知未读时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "标记通知未读时发生内部错误" });
        }
    }

    /// <summary>
    /// 确认通知（需要确认的通知）
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>更新后的通知</returns>
    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<NotificationDto>> ConfirmNotification(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var updateRequest = new NotificationStatusUpdateDto
            {
                Status = NotificationStatus.Confirmed,
                IsRead = true,
                ReadAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow
            };

            var notification = await _notificationService.UpdateNotificationStatusAsync(id, updateRequest, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 确认通知成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "确认通知时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "确认通知时发生内部错误" });
        }
    }

    /// <summary>
    /// 归档通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>更新后的通知</returns>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<NotificationDto>> ArchiveNotification(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var updateRequest = new NotificationStatusUpdateDto
            {
                Status = NotificationStatus.Archived,
                IsArchived = true,
                ArchivedAt = DateTime.UtcNow
            };

            var notification = await _notificationService.UpdateNotificationStatusAsync(id, updateRequest, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 归档通知成功，ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "归档通知时发生错误，ID: {NotificationId}", id);
            return StatusCode(500, new { message = "归档通知时发生内部错误" });
        }
    }

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量标记已读
    /// </summary>
    /// <param name="request">批量标记已读请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("batch/mark-read")]
    public async Task<ActionResult> BatchMarkAsRead([FromBody] BatchMarkAsReadDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _notificationService.BatchMarkAsReadAsync(request, currentUserId.Value);

            // 通知前端更新
            await _hubContext.Clients.User(currentUserId.Value.ToString()).SendAsync("NotificationsBatchRead", request.NotificationIds);

            _logger.LogInformation("用户 {UserId} 批量标记已读成功，处理数量: {Count}", 
                currentUserId.Value, result.SuccessCount);

            return Ok(new { 
                successCount = result.SuccessCount,
                failureCount = result.FailureCount,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记已读时发生错误");
            return StatusCode(500, new { message = "批量标记已读时发生内部错误" });
        }
    }

    /// <summary>
    /// 批量删除通知
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("batch/delete")]
    public async Task<ActionResult> BatchDeleteNotifications([FromBody] BatchDeleteNotificationsDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _notificationService.BatchDeleteNotificationsAsync(request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 批量删除通知成功，处理数量: {Count}", 
                currentUserId.Value, result.SuccessCount);

            return Ok(new { 
                successCount = result.SuccessCount,
                failureCount = result.FailureCount,
                errors = result.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除通知时发生错误");
            return StatusCode(500, new { message = "批量删除通知时发生内部错误" });
        }
    }

    /// <summary>
    /// 标记所有通知为已读
    /// </summary>
    /// <param name="type">通知类型筛选</param>
    /// <param name="beforeDate">在此日期之前的通知</param>
    /// <returns>操作结果</returns>
    [HttpPost("mark-all-read")]
    public async Task<ActionResult> MarkAllAsRead(
        [FromQuery] NotificationType? type = null,
        [FromQuery] DateTime? beforeDate = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var request = new BatchMarkAsReadDto
            {
                MarkAllAsRead = true,
                TypeFilter = type,
                BeforeDate = beforeDate
            };

            var result = await _notificationService.BatchMarkAsReadAsync(request, currentUserId.Value);

            // 通知前端更新
            await _hubContext.Clients.User(currentUserId.Value.ToString()).SendAsync("AllNotificationsRead");

            _logger.LogInformation("用户 {UserId} 标记所有通知已读成功，处理数量: {Count}", 
                currentUserId.Value, result.SuccessCount);

            return Ok(new { 
                successCount = result.SuccessCount,
                message = "已成功标记所有通知为已读"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记所有通知已读时发生错误");
            return StatusCode(500, new { message = "标记所有通知已读时发生内部错误" });
        }
    }

    #endregion

    #region 通知统计和摘要

    /// <summary>
    /// 获取通知统计信息
    /// </summary>
    /// <returns>通知统计信息</returns>
    [HttpGet("stats")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var stats = await _notificationService.GetNotificationStatsAsync(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 获取通知统计信息成功", currentUserId.Value);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知统计信息时发生错误");
            return StatusCode(500, new { message = "获取通知统计信息时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取通知摘要
    /// </summary>
    /// <returns>通知摘要</returns>
    [HttpGet("summary")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
    public async Task<ActionResult<NotificationSummaryDto>> GetNotificationSummary()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var summary = await _notificationService.GetNotificationSummaryAsync(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 获取通知摘要成功", currentUserId.Value);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知摘要时发生错误");
            return StatusCode(500, new { message = "获取通知摘要时发生内部错误" });
        }
    }

    #endregion

    #region 通知设置管理

    /// <summary>
    /// 获取用户的通知设置
    /// </summary>
    /// <returns>通知设置列表</returns>
    [HttpGet("settings")]
    public async Task<ActionResult<IEnumerable<NotificationSettingDto>>> GetNotificationSettings()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var settings = await _notificationService.GetNotificationSettingsAsync(currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 获取通知设置成功", currentUserId.Value);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知设置时发生错误");
            return StatusCode(500, new { message = "获取通知设置时发生内部错误" });
        }
    }

    /// <summary>
    /// 创建通知设置
    /// </summary>
    /// <param name="request">创建设置请求</param>
    /// <returns>创建的通知设置</returns>
    [HttpPost("settings")]
    public async Task<ActionResult<NotificationSettingDto>> CreateNotificationSetting([FromBody] CreateNotificationSettingDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var setting = await _notificationService.CreateNotificationSettingAsync(request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 创建通知设置成功，ID: {SettingId}", 
                currentUserId.Value, setting.Id);

            return CreatedAtAction(nameof(GetNotificationSettings), new { }, setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知设置时发生错误");
            return StatusCode(500, new { message = "创建通知设置时发生内部错误" });
        }
    }

    /// <summary>
    /// 更新通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的通知设置</returns>
    [HttpPut("settings/{id:guid}")]
    public async Task<ActionResult<NotificationSettingDto>> UpdateNotificationSetting(Guid id, [FromBody] UpdateNotificationSettingDto request)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "设置ID不能为空" });
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

            var setting = await _notificationService.UpdateNotificationSettingAsync(id, request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 更新通知设置成功，ID: {SettingId}", 
                currentUserId.Value, id);

            return Ok(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知设置时发生错误，ID: {SettingId}", id);
            return StatusCode(500, new { message = "更新通知设置时发生内部错误" });
        }
    }

    /// <summary>
    /// 删除通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("settings/{id:guid}")]
    public async Task<ActionResult> DeleteNotificationSetting(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "设置ID不能为空" });
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _notificationService.DeleteNotificationSettingAsync(id, currentUserId.Value);

            if (!result)
            {
                return NotFound(new { message = "通知设置不存在" });
            }

            _logger.LogInformation("用户 {UserId} 删除通知设置成功，ID: {SettingId}", 
                currentUserId.Value, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知设置时发生错误，ID: {SettingId}", id);
            return StatusCode(500, new { message = "删除通知设置时发生内部错误" });
        }
    }

    #endregion

    #region 测试和工具功能

    /// <summary>
    /// 发送测试通知
    /// </summary>
    /// <param name="request">测试通知请求</param>
    /// <returns>测试结果</returns>
    [HttpPost("test")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<NotificationTestResultDto>> SendTestNotification([FromBody] TestNotificationDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "用户未登录" });
            }

            var result = await _notificationService.SendTestNotificationAsync(request, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 发送测试通知成功，目标用户: {TargetUserId}", 
                currentUserId.Value, request.UserId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送测试通知时发生错误");
            return StatusCode(500, new { message = "发送测试通知时发生内部错误" });
        }
    }

    /// <summary>
    /// 获取通知发送历史
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>发送历史列表</returns>
    [HttpGet("{id:guid}/delivery-history")]
    public async Task<ActionResult<PaginatedResult<NotificationDeliveryHistoryDto>>> GetDeliveryHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "通知ID不能为空" });
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

            var history = await _notificationService.GetNotificationDeliveryHistoryAsync(id, page, pageSize, currentUserId.Value);

            _logger.LogInformation("用户 {UserId} 获取通知发送历史成功，通知ID: {NotificationId}", 
                currentUserId.Value, id);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知发送历史时发生错误，通知ID: {NotificationId}", id);
            return StatusCode(500, new { message = "获取通知发送历史时发生内部错误" });
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取当前登录用户的ID
    /// </summary>
    /// <returns>用户ID，如果未登录则返回null</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}

/// <summary>
/// SignalR通知Hub - 用于实时推送通知
/// </summary>
public class NotificationHub : Hub
{
    /// <summary>
    /// 用户连接时调用
    /// </summary>
    /// <returns>任务</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 用户断开连接时调用
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>任务</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}