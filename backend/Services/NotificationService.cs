using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 通知服务实现 - 提供完整的通知管理功能，遵循单一职责和接口隔离原则
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationSettingsRepository _settingsRepository;
    private readonly ILogger<NotificationService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IPermissionService _permissionService;
    private readonly IUserRepository _userRepository;

    // 缓存键前缀
    private const string NOTIFICATION_CACHE_PREFIX = "notification_";
    private const string USER_NOTIFICATIONS_CACHE_PREFIX = "user_notifications_";
    private const string USER_SETTINGS_CACHE_PREFIX = "user_notification_settings_";
    private const string USER_STATS_CACHE_PREFIX = "user_notification_stats_";

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationSettingsRepository settingsRepository,
        ILogger<NotificationService> logger,
        IDistributedCache cache,
        IPermissionService permissionService,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    #region 通知创建和发送操作

    /// <summary>
    /// 创建并发送单个通知
    /// </summary>
    public async Task<NotificationDto> CreateAndSendNotificationAsync(NotificationSendRequestDto request, Guid? senderId = null)
    {
        try
        {
            _logger.LogInformation("创建通知: UserId={UserId}, Type={Type}, Title={Title}", 
                request.UserId, request.Type, request.Title);

            // 验证用户权限
            if (senderId.HasValue && !await _permissionService.HasPermissionAsync(senderId.Value, "SendNotification"))
            {
                throw new UnauthorizedAccessException("用户没有发送通知的权限");
            }

            // 检查用户通知设置
            var isEnabled = await IsNotificationEnabledAsync(request.UserId, request.Type, request.Channel);
            if (!isEnabled)
            {
                _logger.LogWarning("用户已禁用此类型通知: UserId={UserId}, Type={Type}, Channel={Channel}", 
                    request.UserId, request.Type, request.Channel);
                throw new InvalidOperationException("用户已禁用此类型通知");
            }

            // 检查免打扰时间
            var isInQuietHours = await _settingsRepository.IsInQuietHoursAsync(request.UserId);
            if (isInQuietHours && request.Priority < NotificationPriority.High)
            {
                _logger.LogInformation("用户当前处于免打扰时间，延迟发送通知: UserId={UserId}", request.UserId);
                // 对于低优先级通知，可以计划稍后发送
                request.ScheduledToSendAt = DateTime.UtcNow.AddHours(1);
            }

            // 创建通知实体
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                Message = request.Message,
                Priority = request.Priority,
                Status = NotificationStatus.Pending,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                TriggeredByUserId = senderId,
                Action = request.Action,
                Channel = request.Channel,
                DataJson = request.DataJson,
                Tag = request.Tag,
                Icon = request.Icon,
                Color = request.Color,
                RequiresConfirmation = request.RequiresConfirmation,
                ExpiresAt = request.ExpiresAt,
                ScheduledToSendAt = request.ScheduledToSendAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 保存到数据库
            var createdNotification = await _notificationRepository.CreateAsync(notification);

            // 如果是立即发送，尝试发送
            if (!request.ScheduledToSendAt.HasValue || request.ScheduledToSendAt.Value <= DateTime.UtcNow)
            {
                await SendNotificationAsync(createdNotification);
            }

            // 清除用户相关缓存
            await ClearUserNotificationCacheAsync(request.UserId);

            // 返回DTO
            return await MapToDtoAsync(createdNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知失败: UserId={UserId}, Type={Type}", request.UserId, request.Type);
            throw;
        }
    }

    /// <summary>
    /// 批量创建并发送通知
    /// </summary>
    public async Task<NotificationBatchSendResultDto> BatchCreateAndSendNotificationsAsync(IEnumerable<NotificationSendRequestDto> requests, Guid? senderId = null)
    {
        var result = new NotificationBatchSendResultDto();
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();
        var requestList = requests.ToList();

        _logger.LogInformation("开始批量创建通知: 总数量={Count}", requestList.Count);

        try
        {
            // 验证发送者权限
            if (senderId.HasValue && !await _permissionService.HasPermissionAsync(senderId.Value, "SendNotification"))
            {
                throw new UnauthorizedAccessException("用户没有发送通知的权限");
            }

            result.TotalRequests = requestList.Count;

            // 批量处理通知
            foreach (var request in requestList)
            {
                try
                {
                    var notification = await CreateAndSendNotificationAsync(request, senderId);
                    result.SuccessCount++;
                    result.CreatedNotificationIds.Add(notification.Id);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(Guid.NewGuid(), ex.Message);
                    _logger.LogWarning(ex, "批量创建单个通知失败: UserId={UserId}, Type={Type}", 
                        request.UserId, request.Type);
                }
            }

            stopWatch.Stop();
            result.ProcessingTimeMs = stopWatch.ElapsedMilliseconds;

            // 确定批量发送状态
            if (result.FailedCount == 0)
            {
                result.Status = NotificationBatchSendStatus.Success;
            }
            else if (result.SuccessCount > 0)
            {
                result.Status = NotificationBatchSendStatus.PartialSuccess;
            }
            else
            {
                result.Status = NotificationBatchSendStatus.Failed;
            }

            _logger.LogInformation("批量创建通知完成: 成功={SuccessCount}, 失败={FailedCount}, 耗时={ProcessingTimeMs}ms", 
                result.SuccessCount, result.FailedCount, result.ProcessingTimeMs);

            return result;
        }
        catch (Exception ex)
        {
            stopWatch.Stop();
            result.ProcessingTimeMs = stopWatch.ElapsedMilliseconds;
            result.Status = NotificationBatchSendStatus.Failed;
            
            _logger.LogError(ex, "批量创建通知失败");
            throw;
        }
    }

    /// <summary>
    /// 发送测试通知
    /// </summary>
    public async Task<NotificationTestResultDto> SendTestNotificationAsync(TestNotificationDto request, Guid? senderId = null)
    {
        var result = new NotificationTestResultDto();
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation("发送测试通知: UserId={UserId}, Channels={Channels}", 
            request.UserId, string.Join(", ", request.Channels));

        try
        {
            // 验证权限
            if (senderId.HasValue && !await _permissionService.HasPermissionAsync(senderId.Value, "SendNotification"))
            {
                throw new UnauthorizedAccessException("用户没有发送通知的权限");
            }

            // 验证用户存在
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            result.Success = true;
            result.Message = "测试通知发送完成";

            // 测试每个渠道
            foreach (var channel in request.Channels)
            {
                var channelResult = new NotificationChannelTestResult
                {
                    Channel = channel,
                    RecipientAddress = await GetRecipientAddressAsync(request.UserId, channel)
                };

                try
                {
                    // 创建测试通知
                    var testRequest = new NotificationSendRequestDto
                    {
                        UserId = request.UserId,
                        Type = NotificationType.System,
                        Title = "测试通知",
                        Content = $"这是一条通过 {channel} 渠道发送的测试通知",
                        Message = "测试通知消息",
                        Priority = NotificationPriority.Normal,
                        Channel = channel,
                        DataJson = JsonSerializer.Serialize(new { IsTest = true, TestId = Guid.NewGuid() })
                    };

                    var notification = await CreateAndSendNotificationAsync(testRequest, senderId);
                    
                    channelResult.Success = true;
                    channelResult.Message = "发送成功";
                    result.NotificationId = notification.Id;
                }
                catch (Exception ex)
                {
                    channelResult.Success = false;
                    channelResult.Error = ex.Message;
                    channelResult.Message = "发送失败";
                    result.Success = false;
                    result.Error = ex.Message;
                }

                channelResult.SendTimeMs = stopWatch.ElapsedMilliseconds;
                result.ChannelResults[channel] = channelResult;
            }

            stopWatch.Stop();
            result.SendTimeMs = stopWatch.ElapsedMilliseconds;

            return result;
        }
        catch (Exception ex)
        {
            stopWatch.Stop();
            result.SendTimeMs = stopWatch.ElapsedMilliseconds;
            result.Success = false;
            result.Error = ex.Message;
            
            _logger.LogError(ex, "发送测试通知失败");
            throw;
        }
    }

    /// <summary>
    /// 计划发送通知
    /// </summary>
    public async Task<NotificationDto> ScheduleNotificationAsync(NotificationSendRequestDto request, Guid? senderId = null)
    {
        try
        {
            _logger.LogInformation("计划发送通知: UserId={UserId}, Type={Type}, ScheduledTime={ScheduledTime}", 
                request.UserId, request.Type, request.ScheduledToSendAt);

            // 验证发送者权限
            if (senderId.HasValue && !await _permissionService.HasPermissionAsync(senderId.Value, "SendNotification"))
            {
                throw new UnauthorizedAccessException("用户没有发送通知的权限");
            }

            // 验证计划时间
            if (!request.ScheduledToSendAt.HasValue || request.ScheduledToSendAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("计划发送时间必须晚于当前时间");
            }

            // 创建通知
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                Message = request.Message,
                Priority = request.Priority,
                Status = NotificationStatus.Pending,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                TriggeredByUserId = senderId,
                Action = request.Action,
                Channel = request.Channel,
                DataJson = request.DataJson,
                Tag = request.Tag,
                Icon = request.Icon,
                Color = request.Color,
                RequiresConfirmation = request.RequiresConfirmation,
                ExpiresAt = request.ExpiresAt,
                ScheduledToSendAt = request.ScheduledToSendAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);

            return await MapToDtoAsync(createdNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "计划发送通知失败: UserId={UserId}, Type={Type}", request.UserId, request.Type);
            throw;
        }
    }

    /// <summary>
    /// 取消计划发送的通知
    /// </summary>
    public async Task<bool> CancelScheduledNotificationAsync(Guid notificationId, string cancelReason, Guid userId)
    {
        try
        {
            _logger.LogInformation("取消计划发送的通知: NotificationId={NotificationId}, UserId={UserId}", 
                notificationId, userId);

            // 验证权限
            if (!await _permissionService.HasPermissionAsync(userId, "CancelNotification"))
            {
                throw new UnauthorizedAccessException("用户没有取消通知的权限");
            }

            // 获取通知
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new ArgumentException("通知不存在");
            }

            // 验证通知状态
            if (notification.Status != NotificationStatus.Pending || !notification.ScheduledToSendAt.HasValue)
            {
                throw new InvalidOperationException("只能取消待发送的计划通知");
            }

            // 验证用户权限（只能取消自己发送的通知或系统管理员）
            if (notification.TriggeredByUserId != userId && 
                !await _permissionService.HasPermissionAsync(userId, "ManageNotifications"))
            {
                throw new UnauthorizedAccessException("用户没有取消此通知的权限");
            }

            // 更新通知状态
            notification.Status = NotificationStatus.Cancelled;
            notification.ErrorMessage = cancelReason;
            notification.UpdatedAt = DateTime.UtcNow;

            await _notificationRepository.UpdateAsync(notification);

            // 清除缓存
            await ClearNotificationCacheAsync(notificationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消计划发送的通知失败: NotificationId={NotificationId}", notificationId);
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 发送通知的内部方法
    /// </summary>
    private async Task SendNotificationAsync(Notification notification)
    {
        try
        {
            _logger.LogDebug("发送通知: Id={Id}, UserId={UserId}, Channel={Channel}", 
                notification.Id, notification.UserId, notification.Channel);

            // 更新状态为发送中
            notification.Status = NotificationStatus.Sending;
            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);

            // 根据渠道发送通知
            var sendSuccess = await SendByChannelAsync(notification);

            if (sendSuccess)
            {
                notification.Status = NotificationStatus.Sent;
                notification.DeliveredAt = DateTime.UtcNow;
                notification.SendCount++;
                notification.LastSentAt = DateTime.UtcNow;
                notification.ErrorMessage = null;
            }
            else
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = "发送失败";
            }

            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);

            // 记录发送历史
            await RecordDeliveryHistoryAsync(notification, sendSuccess);

            // 如果发送失败，尝试重试
            if (!sendSuccess && notification.SendCount < 3)
            {
                await RetryNotificationAsync(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送通知失败: Id={Id}", notification.Id);
            
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            notification.UpdatedAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    /// <summary>
    /// 根据渠道发送通知
    /// </summary>
    private async Task<bool> SendByChannelAsync(Notification notification)
    {
        try
        {
            switch (notification.Channel)
            {
                case NotificationChannel.InApp:
                    return await SendInAppNotificationAsync(notification);
                case NotificationChannel.Email:
                    return await SendEmailNotificationAsync(notification);
                case NotificationChannel.Push:
                    return await SendPushNotificationAsync(notification);
                case NotificationChannel.SMS:
                    return await SendSmsNotificationAsync(notification);
                case NotificationChannel.Desktop:
                    return await SendDesktopNotificationAsync(notification);
                default:
                    _logger.LogWarning("不支持的通知渠道: {Channel}", notification.Channel);
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通过渠道发送通知失败: Id={Id}, Channel={Channel}", 
                notification.Id, notification.Channel);
            return false;
        }
    }

    /// <summary>
    /// 发送应用内通知
    /// </summary>
    private async Task<bool> SendInAppNotificationAsync(Notification notification)
    {
        // 应用内通知主要是保存到数据库，已在SendNotificationAsync中处理
        // 这里可以添加实时推送逻辑
        return true;
    }

    /// <summary>
    /// 发送邮件通知
    /// </summary>
    private async Task<bool> SendEmailNotificationAsync(Notification notification)
    {
        // TODO: 实现邮件发送逻辑
        _logger.LogInformation("发送邮件通知: UserId={UserId}, Title={Title}", 
            notification.UserId, notification.Title);
        return true;
    }

    /// <summary>
    /// 发送推送通知
    /// </summary>
    private async Task<bool> SendPushNotificationAsync(Notification notification)
    {
        // TODO: 实现推送通知逻辑
        _logger.LogInformation("发送推送通知: UserId={UserId}, Title={Title}", 
            notification.UserId, notification.Title);
        return true;
    }

    /// <summary>
    /// 发送短信通知
    /// </summary>
    private async Task<bool> SendSmsNotificationAsync(Notification notification)
    {
        // TODO: 实现短信发送逻辑
        _logger.LogInformation("发送短信通知: UserId={UserId}, Title={Title}", 
            notification.UserId, notification.Title);
        return true;
    }

    /// <summary>
    /// 发送桌面通知
    /// </summary>
    private async Task<bool> SendDesktopNotificationAsync(Notification notification)
    {
        // TODO: 实现桌面通知逻辑
        _logger.LogInformation("发送桌面通知: UserId={UserId}, Title={Title}", 
            notification.UserId, notification.Title);
        return true;
    }

    /// <summary>
    /// 重试发送通知
    /// </summary>
    private async Task RetryNotificationAsync(Notification notification)
    {
        try
        {
            _logger.LogInformation("重试发送通知: Id={Id}, RetryCount={RetryCount}", 
                notification.Id, notification.SendCount);

            // 延迟重试
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, notification.SendCount)));

            await SendNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重试发送通知失败: Id={Id}", notification.Id);
        }
    }

    /// <summary>
    /// 记录发送历史
    /// </summary>
    private async Task RecordDeliveryHistoryAsync(Notification notification, bool success)
    {
        try
        {
            var history = new NotificationDeliveryHistory
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                Channel = notification.Channel,
                Status = success ? DeliveryStatus.Sent : DeliveryStatus.Failed,
                SentAt = DateTime.UtcNow,
                DeliveredAt = success ? DateTime.UtcNow : null,
                ErrorMessage = success ? null : notification.ErrorMessage,
                RetryCount = notification.SendCount - 1,
                RecipientAddress = await GetRecipientAddressAsync(notification.UserId, notification.Channel)
            };

            // TODO: 保存发送历史到数据库
            _logger.LogDebug("记录通知发送历史: Id={Id}, Success={Success}", history.Id, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录发送历史失败: NotificationId={NotificationId}", notification.Id);
        }
    }

    /// <summary>
    /// 获取接收地址
    /// </summary>
    private async Task<string?> GetRecipientAddressAsync(Guid userId, NotificationChannel channel)
    {
        try
        {
            // TODO: 根据渠道获取用户的接收地址
            switch (channel)
            {
                case NotificationChannel.Email:
                    var user = await _userRepository.GetByIdAsync(userId);
                    return user?.Email;
                case NotificationChannel.SMS:
                    // TODO: 获取用户手机号
                    return null;
                case NotificationChannel.Push:
                    // TODO: 获取用户设备令牌
                    return null;
                default:
                    return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取接收地址失败: UserId={UserId}, Channel={Channel}", userId, channel);
            return null;
        }
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    private async Task<NotificationDto> MapToDtoAsync(Notification notification)
    {
        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Content = notification.Content,
            Message = notification.Message,
            Priority = notification.Priority,
            Status = notification.Status,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            TriggeredByUserId = notification.TriggeredByUserId,
            Action = notification.Action,
            Channel = notification.Channel,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            DeliveredAt = notification.DeliveredAt,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt,
            ExpiresAt = notification.ExpiresAt,
            ScheduledToSendAt = notification.ScheduledToSendAt,
            SendCount = notification.SendCount,
            LastSentAt = notification.LastSentAt,
            ErrorMessage = notification.ErrorMessage,
            DataJson = notification.DataJson,
            Tag = notification.Tag,
            Icon = notification.Icon,
            Color = notification.Color,
            RequiresConfirmation = notification.RequiresConfirmation,
            ConfirmedAt = notification.ConfirmedAt,
            IsArchived = notification.IsArchived,
            ArchivedAt = notification.ArchivedAt,
            IsDeleted = notification.IsDeleted,
            DeletedAt = notification.DeletedAt,
            TimeSinceCreated = DateTime.UtcNow - notification.CreatedAt
        };

        // 获取用户信息
        if (notification.User != null)
        {
            dto.UserName = notification.User.Username;
            dto.UserAvatar = notification.User.Avatar;
        }

        // 获取触发用户信息
        if (notification.TriggeredByUser != null)
        {
            dto.TriggeredByUserName = notification.TriggeredByUser.Username;
            dto.TriggeredByUserAvatar = notification.TriggeredByUser.Avatar;
        }

        // 设置权限标识
        dto.CanMarkAsRead = !notification.IsRead;
        dto.CanDelete = notification.Status != NotificationStatus.Deleted;
        dto.CanArchive = !notification.IsArchived;
        dto.CanConfirm = notification.RequiresConfirmation && !notification.ConfirmedAt.HasValue;

        return dto;
    }

    /// <summary>
    /// 清除通知缓存
    /// </summary>
    private async Task ClearNotificationCacheAsync(Guid notificationId)
    {
        try
        {
            await _cache.RemoveAsync($"{NOTIFICATION_CACHE_PREFIX}{notificationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除通知缓存失败: NotificationId={NotificationId}", notificationId);
        }
    }

    /// <summary>
    /// 清除用户通知缓存
    /// </summary>
    private async Task ClearUserNotificationCacheAsync(Guid userId)
    {
        try
        {
            await _cache.RemoveAsync($"{USER_NOTIFICATIONS_CACHE_PREFIX}{userId}");
            await _cache.RemoveAsync($"{USER_STATS_CACHE_PREFIX}{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户通知缓存失败: UserId={UserId}", userId);
        }
    }

    #endregion

    #region 通知管理操作

    /// <summary>
    /// 获取通知详情
    /// </summary>
    public async Task<NotificationDto?> GetNotificationByIdAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("获取通知详情: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            // 尝试从缓存获取
            var cacheKey = $"{NOTIFICATION_CACHE_PREFIX}{id}";
            var cachedNotification = await _cache.GetStringAsync(cacheKey);
            
            Notification? notification;
            if (!string.IsNullOrEmpty(cachedNotification))
            {
                notification = JsonSerializer.Deserialize<Notification>(cachedNotification);
            }
            else
            {
                notification = await _notificationRepository.GetByIdWithUserAsync(id);
                if (notification != null)
                {
                    // 缓存通知
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(notification), cacheOptions);
                }
            }

            return notification != null ? await MapToDtoAsync(notification) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知详情失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 获取分页通知列表
    /// </summary>
    public async Task<PaginatedResult<NotificationDto>> GetNotificationsAsync(NotificationFilterDto filter, Guid userId)
    {
        try
        {
            _logger.LogDebug("获取通知列表: UserId={UserId}, Page={Page}, PageSize={PageSize}", 
                userId, filter.Page, filter.PageSize);

            // 确保只能访问自己的通知
            filter.UserId = userId;

            // 尝试从缓存获取
            var cacheKey = $"{USER_NOTIFICATIONS_CACHE_PREFIX}{userId}_{filter.Page}_{filter.PageSize}_{filter.Status}_{filter.Type}";
            var cachedResult = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedResult))
            {
                return JsonSerializer.Deserialize<PaginatedResult<NotificationDto>>(cachedResult)!;
            }

            // 从数据库获取
            var result = await _notificationRepository.GetPagedAsync(filter);
            
            // 映射为DTO
            var dtoResult = new PaginatedResult<NotificationDto>
            {
                Items = await Task.WhenAll(result.Items.Select(MapToDtoAsync)),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious
            };

            // 缓存结果
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dtoResult), cacheOptions);

            return dtoResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知列表失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 高级搜索通知
    /// </summary>
    public async Task<PaginatedResult<NotificationDto>> SearchNotificationsAsync(NotificationAdvancedFilterDto filter, Guid userId)
    {
        try
        {
            _logger.LogDebug("高级搜索通知: UserId={UserId}, SearchTerm={SearchTerm}", 
                userId, filter.SearchTerm);

            // 确保只能搜索自己的通知
            filter.UserId = userId;

            // 使用搜索功能
            var searchResults = await _notificationRepository.SearchAsync(filter.SearchTerm ?? "", filter);
            
            // 应用其他筛选条件
            var filteredResults = searchResults.Where(n => 
                (!filter.Status.HasValue || n.Status == filter.Status.Value) &&
                (!filter.Type.HasValue || n.Type == filter.Type.Value) &&
                (!filter.Priority.HasValue || n.Priority == filter.Priority.Value) &&
                (!filter.StartDate.HasValue || n.CreatedAt >= filter.StartDate.Value) &&
                (!filter.EndDate.HasValue || n.CreatedAt <= filter.EndDate.Value) &&
                (string.IsNullOrEmpty(filter.Tag) || n.Tag == filter.Tag) &&
                (string.IsNullOrEmpty(filter.RelatedEntityType) || 
                 n.RelatedEntityType?.ToString() == filter.RelatedEntityType)
            ).ToList();

            // 分页处理
            var totalCount = filteredResults.Count;
            var skipCount = (filter.Page - 1) * filter.PageSize;
            var pagedResults = filteredResults
                .Skip(skipCount)
                .Take(filter.PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            return new PaginatedResult<NotificationDto>
            {
                Items = await Task.WhenAll(pagedResults.Select(MapToDtoAsync)),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNext = filter.Page < totalPages,
                HasPrevious = filter.Page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "高级搜索通知失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新通知内容
    /// </summary>
    public async Task<NotificationDto> UpdateNotificationAsync(Guid id, UpdateNotificationDto request, Guid userId)
    {
        try
        {
            _logger.LogInformation("更新通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证编辑权限
            if (!await _notificationRepository.CanUserEditAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有编辑此通知的权限");
            }

            // 获取通知
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                throw new ArgumentException("通知不存在");
            }

            // 更新字段
            if (!string.IsNullOrEmpty(request.Title))
                notification.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Content))
                notification.Content = request.Content;
            
            if (!string.IsNullOrEmpty(request.Message))
                notification.Message = request.Message;
            
            if (request.Priority.HasValue)
                notification.Priority = request.Priority.Value;
            
            if (request.RelatedEntityType.HasValue)
                notification.RelatedEntityType = request.RelatedEntityType.Value;
            
            if (!string.IsNullOrEmpty(request.RelatedEntityId))
                notification.RelatedEntityId = request.RelatedEntityId;
            
            if (!string.IsNullOrEmpty(request.Tag))
                notification.Tag = request.Tag;
            
            if (!string.IsNullOrEmpty(request.Icon))
                notification.Icon = request.Icon;
            
            if (!string.IsNullOrEmpty(request.Color))
                notification.Color = request.Color;
            
            if (request.RequiresConfirmation.HasValue)
                notification.RequiresConfirmation = request.RequiresConfirmation.Value;
            
            if (request.ExpiresAt.HasValue)
                notification.ExpiresAt = request.ExpiresAt.Value;
            
            if (!string.IsNullOrEmpty(request.DataJson))
                notification.DataJson = request.DataJson;

            notification.UpdatedAt = DateTime.UtcNow;

            // 保存更新
            var updatedNotification = await _notificationRepository.UpdateAsync(notification);

            // 清除缓存
            await ClearNotificationCacheAsync(id);
            await ClearUserNotificationCacheAsync(userId);

            return await MapToDtoAsync(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 更新通知状态
    /// </summary>
    public async Task<NotificationDto> UpdateNotificationStatusAsync(Guid id, NotificationStatusUpdateDto request, Guid userId)
    {
        try
        {
            _logger.LogInformation("更新通知状态: Id={Id}, Status={Status}, UserId={UserId}", 
                id, request.Status, userId);

            // 验证编辑权限
            if (!await _notificationRepository.CanUserEditAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有编辑此通知的权限");
            }

            // 获取通知
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                throw new ArgumentException("通知不存在");
            }

            // 验证状态转换的合法性
            if (!IsValidStatusTransition(notification.Status, request.Status))
            {
                throw new InvalidOperationException($"无法从 {notification.Status} 状态转换为 {request.Status} 状态");
            }

            // 更新状态
            var oldStatus = notification.Status;
            notification.Status = request.Status;
            notification.UpdatedAt = DateTime.UtcNow;

            // 根据状态更新相关字段
            switch (request.Status)
            {
                case NotificationStatus.Read:
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    break;
                case NotificationStatus.Confirmed:
                    notification.ConfirmedAt = DateTime.UtcNow;
                    break;
                case NotificationStatus.Archived:
                    notification.IsArchived = true;
                    notification.ArchivedAt = DateTime.UtcNow;
                    break;
            }

            // 保存更新
            var updatedNotification = await _notificationRepository.UpdateAsync(notification);

            // 记录状态变更日志
            await LogStatusChangeAsync(id, oldStatus, request.Status, userId);

            // 清除缓存
            await ClearNotificationCacheAsync(id);
            await ClearUserNotificationCacheAsync(userId);

            return await MapToDtoAsync(updatedNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知状态失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 标记通知为已读
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("标记通知为已读: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            var result = await _notificationRepository.MarkAsReadAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知为已读失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 标记通知为未读
    /// </summary>
    public async Task<bool> MarkAsUnreadAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("标记通知为未读: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            var result = await _notificationRepository.MarkAsUnreadAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知为未读失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 确认通知
    /// </summary>
    public async Task<bool> ConfirmNotificationAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("确认通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            var result = await _notificationRepository.ConfirmAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "确认通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 归档通知
    /// </summary>
    public async Task<bool> ArchiveNotificationAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("归档通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            var result = await _notificationRepository.ArchiveAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "归档通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 取消归档通知
    /// </summary>
    public async Task<bool> UnarchiveNotificationAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("取消归档通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证访问权限
            if (!await _notificationRepository.CanUserAccessAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有访问此通知的权限");
            }

            var result = await _notificationRepository.UnarchiveAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消归档通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 删除通知（软删除）
    /// </summary>
    public async Task<bool> DeleteNotificationAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("删除通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证删除权限
            if (!await _notificationRepository.CanUserDeleteAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有删除此通知的权限");
            }

            var result = await _notificationRepository.SoftDeleteAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 彻底删除通知
    /// </summary>
    public async Task<bool> PermanentDeleteNotificationAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("彻底删除通知: Id={Id}, UserId={UserId}", id, userId);

            // 验证删除权限和管理员权限
            if (!await _permissionService.HasPermissionAsync(userId, "ManageNotifications"))
            {
                throw new UnauthorizedAccessException("用户没有彻底删除通知的权限");
            }

            var result = await _notificationRepository.DeleteAsync(id);
            
            if (result)
            {
                await ClearNotificationCacheAsync(id);
                await ClearUserNotificationCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "彻底删除通知失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 验证状态转换是否合法
    /// </summary>
    private bool IsValidStatusTransition(NotificationStatus currentStatus, NotificationStatus newStatus)
    {
        // 允许所有状态到已删除状态
        if (newStatus == NotificationStatus.Cancelled || newStatus == NotificationStatus.Failed)
            return true;

        // 根据当前状态验证允许的转换
        return currentStatus switch
        {
            NotificationStatus.Pending => newStatus is NotificationStatus.Sending or NotificationStatus.Sent or NotificationStatus.Cancelled,
            NotificationStatus.Sending => newStatus is NotificationStatus.Sent or NotificationStatus.Failed,
            NotificationStatus.Sent => newStatus is NotificationStatus.Delivered or NotificationStatus.Read or NotificationStatus.Failed,
            NotificationStatus.Delivered => newStatus is NotificationStatus.Read or NotificationStatus.Unread,
            NotificationStatus.Read => newStatus is NotificationStatus.Unread or NotificationStatus.Archived,
            NotificationStatus.Unread => newStatus is NotificationStatus.Read or NotificationStatus.Archived,
            NotificationStatus.Confirmed => newStatus is NotificationStatus.Archived,
            NotificationStatus.Archived => newStatus is NotificationStatus.Unarchived,
            NotificationStatus.Failed => newStatus is NotificationStatus.Pending, // 允许重试
            NotificationStatus.Expired => false, // 过期通知不能转换
            NotificationStatus.Cancelled => false, // 已取消通知不能转换
            _ => false
        };
    }

    /// <summary>
    /// 记录状态变更日志
    /// </summary>
    private async Task LogStatusChangeAsync(Guid notificationId, NotificationStatus oldStatus, NotificationStatus newStatus, Guid userId)
    {
        try
        {
            // TODO: 实现状态变更日志记录
            _logger.LogDebug("通知状态变更: Id={Id}, OldStatus={OldStatus}, NewStatus={NewStatus}, UserId={UserId}", 
                notificationId, oldStatus, newStatus, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录状态变更日志失败: NotificationId={NotificationId}", notificationId);
        }
    }

    #endregion

    #region 通知设置管理操作

    /// <summary>
    /// 获取用户通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSettingDto>> GetUserNotificationSettingsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取用户通知设置: UserId={UserId}", userId);

            // 尝试从缓存获取
            var cacheKey = $"{USER_SETTINGS_CACHE_PREFIX}{userId}";
            var cachedSettings = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedSettings))
            {
                return JsonSerializer.Deserialize<IEnumerable<NotificationSettingDto>>(cachedSettings)!;
            }

            // 从数据库获取
            var settings = await _settingsRepository.GetByUserIdAsync(userId);
            
            // 映射为DTO
            var dtos = settings.Select(MapSettingToDto).ToList();

            // 缓存结果
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dtos), cacheOptions);

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知设置失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户默认通知设置
    /// </summary>
    public async Task<NotificationSettingDto?> GetUserDefaultNotificationSettingsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取用户默认通知设置: UserId={UserId}", userId);

            var setting = await _settingsRepository.GetDefaultByUserIdAsync(userId);
            return setting != null ? MapSettingToDto(setting) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户默认通知设置失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户特定类型的通知设置
    /// </summary>
    public async Task<NotificationSettingDto?> GetUserNotificationSettingForTypeAsync(Guid userId, NotificationType notificationType)
    {
        try
        {
            _logger.LogDebug("获取用户特定类型通知设置: UserId={UserId}, Type={Type}", userId, notificationType);

            var setting = await _settingsRepository.GetByUserIdAndTypeAsync(userId, notificationType);
            return setting != null ? MapSettingToDto(setting) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户特定类型通知设置失败: UserId={UserId}, Type={Type}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 创建通知设置
    /// </summary>
    public async Task<NotificationSettingDto> CreateNotificationSettingAsync(CreateNotificationSettingDto request, Guid userId)
    {
        try
        {
            _logger.LogInformation("创建通知设置: UserId={UserId}, Type={Type}", userId, request.NotificationType);

            // 验证权限
            if (!await _permissionService.HasPermissionAsync(userId, "ManageNotificationSettings"))
            {
                throw new UnauthorizedAccessException("用户没有管理通知设置的权限");
            }

            // 检查是否已存在相同类型的设置
            var existingSetting = await _settingsRepository.GetByUserIdAndTypeAsync(userId, request.NotificationType);
            if (existingSetting != null)
            {
                throw new InvalidOperationException("已存在相同类型的通知设置");
            }

            // 创建设置实体
            var setting = new NotificationSetting
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = request.NotificationType,
                EnableInApp = request.EnableInApp,
                EnableEmail = request.EnableEmail,
                EnablePush = request.EnablePush,
                EnableDesktop = request.EnableDesktop,
                EnableSound = request.EnableSound,
                Frequency = request.Frequency,
                QuietHoursStart = request.QuietHoursStart,
                QuietHoursEnd = request.QuietHoursEnd,
                EnableQuietHours = request.EnableQuietHours,
                EmailFrequency = request.EmailFrequency,
                BatchIntervalMinutes = request.BatchIntervalMinutes,
                EnableBatching = request.EnableBatching,
                Language = request.Language ?? "zh-CN",
                TimeZone = request.TimeZone ?? "Asia/Shanghai",
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 如果是第一个设置，设置为默认
            var userSettings = await _settingsRepository.GetByUserIdAsync(userId);
            if (!userSettings.Any())
            {
                setting.IsDefault = true;
            }

            var createdSetting = await _settingsRepository.CreateAsync(setting);

            // 清除缓存
            await ClearUserSettingsCacheAsync(userId);

            return MapSettingToDto(createdSetting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知设置失败: UserId={UserId}, Type={Type}", userId, request.NotificationType);
            throw;
        }
    }

    /// <summary>
    /// 更新通知设置
    /// </summary>
    public async Task<NotificationSettingDto> UpdateNotificationSettingAsync(Guid id, UpdateNotificationSettingDto request, Guid userId)
    {
        try
        {
            _logger.LogInformation("更新通知设置: Id={Id}, UserId={UserId}", id, userId);

            // 验证权限
            if (!await _settingsRepository.CanUserEditAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有编辑此通知设置的权限");
            }

            // 获取设置
            var setting = await _settingsRepository.GetByIdAsync(id);
            if (setting == null)
            {
                throw new ArgumentException("通知设置不存在");
            }

            // 更新字段
            if (request.EnableInApp.HasValue)
                setting.EnableInApp = request.EnableInApp.Value;
            
            if (request.EnableEmail.HasValue)
                setting.EnableEmail = request.EnableEmail.Value;
            
            if (request.EnablePush.HasValue)
                setting.EnablePush = request.EnablePush.Value;
            
            if (request.EnableDesktop.HasValue)
                setting.EnableDesktop = request.EnableDesktop.Value;
            
            if (request.EnableSound.HasValue)
                setting.EnableSound = request.EnableSound.Value;
            
            if (request.Frequency.HasValue)
                setting.Frequency = request.Frequency.Value;
            
            if (request.QuietHoursStart.HasValue)
                setting.QuietHoursStart = request.QuietHoursStart.Value;
            
            if (request.QuietHoursEnd.HasValue)
                setting.QuietHoursEnd = request.QuietHoursEnd.Value;
            
            if (request.EnableQuietHours.HasValue)
                setting.EnableQuietHours = request.EnableQuietHours.Value;
            
            if (request.EmailFrequency.HasValue)
                setting.EmailFrequency = request.EmailFrequency.Value;
            
            if (request.BatchIntervalMinutes.HasValue)
                setting.BatchIntervalMinutes = request.BatchIntervalMinutes.Value;
            
            if (request.EnableBatching.HasValue)
                setting.EnableBatching = request.EnableBatching.Value;
            
            if (!string.IsNullOrEmpty(request.Language))
                setting.Language = request.Language;
            
            if (!string.IsNullOrEmpty(request.TimeZone))
                setting.TimeZone = request.TimeZone;
            
            if (!string.IsNullOrEmpty(request.Name))
                setting.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Description))
                setting.Description = request.Description;
            
            if (request.IsActive.HasValue)
                setting.IsActive = request.IsActive.Value;

            setting.UpdatedAt = DateTime.UtcNow;

            var updatedSetting = await _settingsRepository.UpdateAsync(setting);

            // 清除缓存
            await ClearUserSettingsCacheAsync(userId);

            return MapSettingToDto(updatedSetting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知设置失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 删除通知设置
    /// </summary>
    public async Task<bool> DeleteNotificationSettingAsync(Guid id, Guid userId)
    {
        try
        {
            _logger.LogDebug("删除通知设置: Id={Id}, UserId={UserId}", id, userId);

            // 验证权限
            if (!await _settingsRepository.CanUserDeleteAsync(id, userId))
            {
                throw new UnauthorizedAccessException("用户没有删除此通知设置的权限");
            }

            var result = await _settingsRepository.DeleteAsync(id);
            
            if (result)
            {
                await ClearUserSettingsCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知设置失败: Id={Id}, UserId={UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// 设置默认通知设置
    /// </summary>
    public async Task<bool> SetDefaultNotificationSettingAsync(Guid settingId, Guid userId)
    {
        try
        {
            _logger.LogInformation("设置默认通知设置: SettingId={SettingId}, UserId={UserId}", settingId, userId);

            // 验证权限
            if (!await _permissionService.HasPermissionAsync(userId, "ManageNotificationSettings"))
            {
                throw new UnauthorizedAccessException("用户没有管理通知设置的权限");
            }

            var result = await _settingsRepository.SetDefaultAsync(userId, settingId);
            
            if (result)
            {
                await ClearUserSettingsCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置默认通知设置失败: SettingId={SettingId}, UserId={UserId}", settingId, userId);
            throw;
        }
    }

    /// <summary>
    /// 初始化用户默认通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSettingDto>> InitializeDefaultNotificationSettingsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("初始化用户默认通知设置: UserId={UserId}", userId);

            // 检查是否已初始化
            var hasSettings = await _settingsRepository.HasInitializedSettingsAsync(userId);
            if (hasSettings)
            {
                throw new InvalidOperationException("用户通知设置已初始化");
            }

            // 获取系统默认设置
            var systemDefaults = await _settingsRepository.GetSystemDefaultSettingsAsync();

            // 为用户创建默认设置
            var userSettings = new List<NotificationSetting>();
            
            // 创建通用默认设置
            var generalSetting = new NotificationSetting
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = null,
                EnableInApp = true,
                EnableEmail = true,
                EnablePush = true,
                EnableDesktop = true,
                EnableSound = true,
                Frequency = NotificationFrequency.Immediate,
                EmailFrequency = EmailNotificationFrequency.Immediate,
                BatchIntervalMinutes = 30,
                EnableBatching = false,
                Language = "zh-CN",
                TimeZone = "Asia/Shanghai",
                Name = "默认设置",
                Description = "系统默认通知设置",
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            userSettings.Add(generalSetting);

            // 创建特定类型的默认设置
            var importantTypes = new[] { NotificationType.System, NotificationType.Security, NotificationType.Account };
            
            foreach (var type in importantTypes)
            {
                var typeSetting = new NotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = type,
                    EnableInApp = true,
                    EnableEmail = true,
                    EnablePush = true,
                    EnableDesktop = false,
                    EnableSound = true,
                    Frequency = NotificationFrequency.Immediate,
                    EmailFrequency = EmailNotificationFrequency.Immediate,
                    BatchIntervalMinutes = 15,
                    EnableBatching = false,
                    Language = "zh-CN",
                    TimeZone = "Asia/Shanghai",
                    Name = $"{type} 设置",
                    Description = $"{type} 类型的通知设置",
                    IsDefault = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                userSettings.Add(typeSetting);
            }

            // 批量创建
            await _settingsRepository.BulkCreateAsync(userSettings);

            // 清除缓存
            await ClearUserSettingsCacheAsync(userId);

            return userSettings.Select(MapSettingToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化用户默认通知设置失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否启用特定类型的通知
    /// </summary>
    public async Task<bool> IsNotificationEnabledAsync(Guid userId, NotificationType notificationType, NotificationChannel channel)
    {
        try
        {
            _logger.LogDebug("检查通知是否启用: UserId={UserId}, Type={Type}, Channel={Channel}", 
                userId, notificationType, channel);

            return await _settingsRepository.IsNotificationEnabledAsync(userId, notificationType, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查通知是否启用失败: UserId={UserId}, Type={Type}, Channel={Channel}", 
                userId, notificationType, channel);
            // 出错时默认返回true，确保重要通知不会被意外阻止
            return true;
        }
    }

    /// <summary>
    /// 更新用户免打扰设置
    /// </summary>
    public async Task<bool> UpdateQuietHoursAsync(Guid userId, bool enable, TimeSpan? startTime, TimeSpan? endTime)
    {
        try
        {
            _logger.LogInformation("更新免打扰设置: UserId={UserId}, Enable={Enable}, StartTime={StartTime}, EndTime={EndTime}", 
                userId, enable, startTime, endTime);

            var result = await _settingsRepository.UpdateQuietHoursAsync(userId, startTime, endTime, enable);
            
            if (result)
            {
                await ClearUserSettingsCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新免打扰设置失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户通知频率设置
    /// </summary>
    public async Task<bool> UpdateNotificationFrequencyAsync(Guid userId, NotificationFrequency frequency, NotificationType? notificationType = null)
    {
        try
        {
            _logger.LogInformation("更新通知频率设置: UserId={UserId}, Frequency={Frequency}, Type={Type}", 
                userId, frequency, notificationType);

            var result = await _settingsRepository.UpdateNotificationFrequencyAsync(userId, frequency, notificationType);
            
            if (result)
            {
                await ClearUserSettingsCacheAsync(userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知频率设置失败: UserId={UserId}, Frequency={Frequency}, Type={Type}", 
                userId, frequency, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 将设置实体映射为DTO
    /// </summary>
    private static NotificationSettingDto MapSettingToDto(NotificationSetting setting)
    {
        return new NotificationSettingDto
        {
            Id = setting.Id,
            UserId = setting.UserId,
            NotificationType = setting.NotificationType,
            EnableInApp = setting.EnableInApp,
            EnableEmail = setting.EnableEmail,
            EnablePush = setting.EnablePush,
            EnableDesktop = setting.EnableDesktop,
            EnableSound = setting.EnableSound,
            Frequency = setting.Frequency,
            QuietHoursStart = setting.QuietHoursStart,
            QuietHoursEnd = setting.QuietHoursEnd,
            EnableQuietHours = setting.EnableQuietHours,
            EmailFrequency = setting.EmailFrequency,
            BatchIntervalMinutes = setting.BatchIntervalMinutes,
            EnableBatching = setting.EnableBatching,
            Language = setting.Language,
            TimeZone = setting.TimeZone,
            Name = setting.Name,
            Description = setting.Description,
            IsDefault = setting.IsDefault,
            IsActive = setting.IsActive,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt,
            LastUsedAt = setting.LastUsedAt
        };
    }

    /// <summary>
    /// 清除用户设置缓存
    /// </summary>
    private async Task ClearUserSettingsCacheAsync(Guid userId)
    {
        try
        {
            await _cache.RemoveAsync($"{USER_SETTINGS_CACHE_PREFIX}{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户设置缓存失败: UserId={UserId}", userId);
        }
    }

    #endregion

    #region 批量操作功能

    /// <summary>
    /// 批量创建通知
    /// </summary>
    public async Task<IEnumerable<NotificationDto>> BulkCreateNotificationsAsync(IEnumerable<CreateNotificationDto> requests, Guid? senderId = null)
    {
        try
        {
            _logger.LogDebug("批量创建通知: Count={Count}", requests.Count());

            var requestList = requests.ToList();
            if (!requestList.Any())
                return Enumerable.Empty<NotificationDto>();

            // 验证权限
            if (senderId.HasValue)
            {
                foreach (var request in requestList)
                {
                    if (!await CanAccessUserAsync(senderId.Value, request.UserId))
                    {
                        throw new UnauthorizedAccessException($"无权向用户 {request.UserId} 发送通知");
                    }
                }
            }

            // 批量创建通知实体
            var notifications = requestList.Select(request => new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                Message = request.Message,
                Priority = request.Priority,
                Status = NotificationStatus.Pending,
                Channel = request.Channel,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                TriggeredByUserId = senderId,
                Action = request.Action,
                DataJson = request.DataJson,
                Tag = request.Tag,
                Icon = request.Icon,
                Color = request.Color,
                RequiresConfirmation = request.RequiresConfirmation,
                ExpiresAt = request.ExpiresAt,
                ScheduledToSendAt = request.ScheduledToSendAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            // 使用仓储批量创建
            var createdNotifications = await _notificationRepository.BulkCreateAsync(notifications);

            // 批量发送通知
            var sendTasks = createdNotifications.Select(notification => 
                SendNotificationAsync(notification, senderId));
            await Task.WhenAll(sendTasks);

            // 返回DTO结果
            return await Task.WhenAll(createdNotifications.Select(MapToDtoAsync));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量创建通知失败: Count={Count}", requests.Count());
            throw;
        }
    }

    /// <summary>
    /// 批量标记通知为已读
    /// </summary>
    public async Task<BatchOperationResultDto> BulkMarkAsReadAsync(BatchMarkAsReadDto request, Guid userId)
    {
        try
        {
            _logger.LogDebug("批量标记已读: UserId={UserId}, Count={Count}", 
                userId, request.NotificationIds?.Count ?? 0);

            if (request.MarkAllAsRead)
            {
                // 标记所有通知为已读
                var filter = new NotificationFilterDto
                {
                    UserId = userId,
                    IsRead = false,
                    TypeFilter = request.TypeFilter,
                    BeforeDate = request.BeforeDate
                };

                var unreadNotifications = await _notificationRepository.GetAsync(filter);
                var unreadIds = unreadNotifications.Select(n => n.Id).ToList();

                if (unreadIds.Any())
                {
                    var updatedCount = await _notificationRepository.BulkUpdateStatusAsync(
                        unreadIds, NotificationStatus.Read);
                    
                    // 清除相关缓存
                    await ClearUserNotificationCacheAsync(userId);

                    return new BatchOperationResultDto
                    {
                        SuccessCount = updatedCount,
                        FailedCount = 0,
                        TotalCount = unreadIds.Count,
                        Message = $"成功将 {updatedCount} 条通知标记为已读"
                    };
                }

                return new BatchOperationResultDto
                {
                    SuccessCount = 0,
                    FailedCount = 0,
                    TotalCount = 0,
                    Message = "没有找到未读通知"
                };
            }
            else
            {
                // 标记指定通知为已读
                if (request.NotificationIds == null || !request.NotificationIds.Any())
                {
                    throw new ArgumentException("通知ID列表不能为空");
                }

                var validIds = request.NotificationIds.Distinct().ToList();
                var updatedCount = await _notificationRepository.BulkUpdateStatusAsync(
                    validIds, NotificationStatus.Read);

                // 清除相关缓存
                await ClearUserNotificationCacheAsync(userId);

                return new BatchOperationResultDto
                {
                    SuccessCount = updatedCount,
                    FailedCount = validIds.Count - updatedCount,
                    TotalCount = validIds.Count,
                    Message = $"成功将 {updatedCount} 条通知标记为已读"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记已读失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量删除通知
    /// </summary>
    public async Task<BatchOperationResultDto> BulkDeleteNotificationsAsync(BatchDeleteNotificationsDto request, Guid userId)
    {
        try
        {
            _logger.LogDebug("批量删除通知: UserId={UserId}, Permanent={Permanent}", 
                userId, request.PermanentDelete);

            if (request.DeleteAll)
            {
                // 删除所有通知
                var filter = new NotificationFilterDto
                {
                    UserId = userId,
                    TypeFilter = request.TypeFilter,
                    BeforeDate = request.BeforeDate,
                    IncludeDeleted = request.PermanentDelete
                };

                var notificationsToDelete = await _notificationRepository.GetAsync(filter);
                var idsToDelete = notificationsToDelete.Select(n => n.Id).ToList();

                if (idsToDelete.Any())
                {
                    int deletedCount;
                    if (request.PermanentDelete)
                    {
                        deletedCount = await _notificationRepository.BulkPermanentDeleteAsync(idsToDelete);
                    }
                    else
                    {
                        deletedCount = await _notificationRepository.BulkSoftDeleteAsync(idsToDelete);
                    }

                    // 清除相关缓存
                    await ClearUserNotificationCacheAsync(userId);

                    return new BatchOperationResultDto
                    {
                        SuccessCount = deletedCount,
                        FailedCount = 0,
                        TotalCount = idsToDelete.Count,
                        Message = $"成功删除 {deletedCount} 条通知"
                    };
                }

                return new BatchOperationResultDto
                {
                    SuccessCount = 0,
                    FailedCount = 0,
                    TotalCount = 0,
                    Message = "没有找到可删除的通知"
                };
            }
            else
            {
                // 删除指定通知
                if (request.NotificationIds == null || !request.NotificationIds.Any())
                {
                    throw new ArgumentException("通知ID列表不能为空");
                }

                var validIds = request.NotificationIds.Distinct().ToList();
                int deletedCount;

                if (request.PermanentDelete)
                {
                    deletedCount = await _notificationRepository.BulkPermanentDeleteAsync(validIds);
                }
                else
                {
                    deletedCount = await _notificationRepository.BulkSoftDeleteAsync(validIds);
                }

                // 清除相关缓存
                await ClearUserNotificationCacheAsync(userId);

                return new BatchOperationResultDto
                {
                    SuccessCount = deletedCount,
                    FailedCount = validIds.Count - deletedCount,
                    TotalCount = validIds.Count,
                    Message = $"成功删除 {deletedCount} 条通知"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除通知失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量归档通知
    /// </summary>
    public async Task<BatchOperationResultDto> BulkArchiveNotificationsAsync(IEnumerable<Guid> notificationIds, Guid userId)
    {
        try
        {
            _logger.LogDebug("批量归档通知: UserId={UserId}, Count={Count}", 
                userId, notificationIds.Count());

            var validIds = notificationIds.Distinct().ToList();
            if (!validIds.Any())
            {
                throw new ArgumentException("通知ID列表不能为空");
            }

            // 验证通知所有权
            var notifications = await _notificationRepository.GetByIdsAsync(validIds);
            var unauthorizedIds = notifications.Where(n => n.UserId != userId).Select(n => n.Id).ToList();

            if (unauthorizedIds.Any())
            {
                throw new UnauthorizedAccessException($"无权操作通知: {string.Join(", ", unauthorizedIds)}");
            }

            var archivedCount = await _notificationRepository.BulkArchiveAsync(validIds);

            // 清除相关缓存
            await ClearUserNotificationCacheAsync(userId);

            return new BatchOperationResultDto
            {
                SuccessCount = archivedCount,
                FailedCount = validIds.Count - archivedCount,
                TotalCount = validIds.Count,
                Message = $"成功归档 {archivedCount} 条通知"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量归档通知失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量更新通知优先级
    /// </summary>
    public async Task<BatchOperationResultDto> BulkUpdatePriorityAsync(BatchUpdatePriorityDto request, Guid userId)
    {
        try
        {
            _logger.LogDebug("批量更新优先级: UserId={UserId}, Priority={Priority}", 
                userId, request.Priority);

            if (request.NotificationIds == null || !request.NotificationIds.Any())
            {
                throw new ArgumentException("通知ID列表不能为空");
            }

            var validIds = request.NotificationIds.Distinct().ToList();
            
            // 验证通知所有权
            var notifications = await _notificationRepository.GetByIdsAsync(validIds);
            var unauthorizedIds = notifications.Where(n => n.UserId != userId).Select(n => n.Id).ToList();

            if (unauthorizedIds.Any())
            {
                throw new UnauthorizedAccessException($"无权操作通知: {string.Join(", ", unauthorizedIds)}");
            }

            var updatedCount = await _notificationRepository.BulkUpdatePriorityAsync(validIds, request.Priority);

            // 清除相关缓存
            await ClearUserNotificationCacheAsync(userId);

            return new BatchOperationResultDto
            {
                SuccessCount = updatedCount,
                FailedCount = validIds.Count - updatedCount,
                TotalCount = validIds.Count,
                Message = $"成功更新 {updatedCount} 条通知的优先级"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新优先级失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量取消通知
    /// </summary>
    public async Task<BatchOperationResultDto> BulkCancelNotificationsAsync(IEnumerable<Guid> notificationIds, Guid userId, string reason = null)
    {
        try
        {
            _logger.LogDebug("批量取消通知: UserId={UserId}, Reason={Reason}", 
                userId, reason);

            var validIds = notificationIds.Distinct().ToList();
            if (!validIds.Any())
            {
                throw new ArgumentException("通知ID列表不能为空");
            }

            // 验证通知所有权
            var notifications = await _notificationRepository.GetByIdsAsync(validIds);
            var unauthorizedIds = notifications.Where(n => n.UserId != userId).Select(n => n.Id).ToList();

            if (unauthorizedIds.Any())
            {
                throw new UnauthorizedAccessException($"无权操作通知: {string.Join(", ", unauthorizedIds)}");
            }

            // 只能取消待发送或已计划的通知
            var cancellableIds = notifications
                .Where(n => n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Scheduled)
                .Select(n => n.Id)
                .ToList();

            if (!cancellableIds.Any())
            {
                return new BatchOperationResultDto
                {
                    SuccessCount = 0,
                    FailedCount = validIds.Count,
                    TotalCount = validIds.Count,
                    Message = "没有可取消的通知"
                };
            }

            var cancelledCount = await _notificationRepository.BulkCancelAsync(cancellableIds, reason);

            // 清除相关缓存
            await ClearUserNotificationCacheAsync(userId);

            return new BatchOperationResultDto
            {
                SuccessCount = cancelledCount,
                FailedCount = cancellableIds.Count - cancelledCount,
                TotalCount = validIds.Count,
                Message = $"成功取消 {cancelledCount} 条通知"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量取消通知失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量重试发送失败的通知
    /// </summary>
    public async Task<BatchOperationResultDto> BulkRetryFailedNotificationsAsync(IEnumerable<Guid> notificationIds, Guid userId)
    {
        try
        {
            _logger.LogDebug("批量重试失败通知: UserId={UserId}", userId);

            var validIds = notificationIds.Distinct().ToList();
            if (!validIds.Any())
            {
                throw new ArgumentException("通知ID列表不能为空");
            }

            // 验证通知所有权
            var notifications = await _notificationRepository.GetByIdsAsync(validIds);
            var unauthorizedIds = notifications.Where(n => n.UserId != userId).Select(n => n.Id).ToList();

            if (unauthorizedIds.Any())
            {
                throw new UnauthorizedAccessException($"无权操作通知: {string.Join(", ", unauthorizedIds)}");
            }

            // 只能重试发送失败的通知
            var failedNotifications = notifications
                .Where(n => n.Status == NotificationStatus.Failed)
                .ToList();

            if (!failedNotifications.Any())
            {
                return new BatchOperationResultDto
                {
                    SuccessCount = 0,
                    FailedCount = validIds.Count,
                    TotalCount = validIds.Count,
                    Message = "没有可重试的通知"
                };
            }

            // 重置状态并重新发送
            var resetCount = await _notificationRepository.BulkResetStatusAsync(
                failedNotifications.Select(n => n.Id).ToList(), NotificationStatus.Pending);

            var sendTasks = failedNotifications.Select(notification => 
                SendNotificationAsync(notification, null));
            await Task.WhenAll(sendTasks);

            // 清除相关缓存
            await ClearUserNotificationCacheAsync(userId);

            return new BatchOperationResultDto
            {
                SuccessCount = resetCount,
                FailedCount = failedNotifications.Count - resetCount,
                TotalCount = validIds.Count,
                Message = $"成功重试 {resetCount} 条通知"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量重试失败通知失败: UserId={UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 统计和摘要功能

    /// <summary>
    /// 获取用户通知统计信息
    /// </summary>
    public async Task<NotificationStatsDto> GetUserNotificationStatsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取用户通知统计: UserId={UserId}", userId);

            var cacheKey = $"{USER_STATS_CACHE_PREFIX}{userId}";
            
            // 尝试从缓存获取
            var cachedStats = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedStats))
            {
                return JsonSerializer.Deserialize<NotificationStatsDto>(cachedStats)!;
            }

            // 计算统计信息
            var stats = new NotificationStatsDto
            {
                TotalCount = await _notificationRepository.CountAsync(userId),
                UnreadCount = await _notificationRepository.CountAsync(userId, isRead: false),
                ReadCount = await _notificationRepository.CountAsync(userId, isRead: true),
                FailedCount = await _notificationRepository.CountAsync(userId, status: NotificationStatus.Failed),
                ArchivedCount = await _notificationRepository.CountAsync(userId, isArchived: true),
                DeletedCount = await _notificationRepository.CountAsync(userId, isDeleted: true),
                HighPriorityCount = await _notificationRepository.CountAsync(userId, priority: NotificationPriority.High),
                RecentCount = await _notificationRepository.CountAsync(userId, 
                    startDate: DateTime.UtcNow.AddHours(-24)),
                TodayCount = await _notificationRepository.CountAsync(userId, 
                    startDate: DateTime.UtcNow.Date),
                LastNotificationAt = await _notificationRepository.GetLastNotificationTimeAsync(userId),
                TypeStats = await GetNotificationTypeStatsAsync(userId),
                PriorityStats = await GetNotificationPriorityStatsAsync(userId),
                StatusStats = await GetNotificationStatusStatsAsync(userId),
                ChannelStats = await GetNotificationChannelStatsAsync(userId)
            };

            // 缓存结果
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), cacheOptions);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知统计失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取通知摘要信息
    /// </summary>
    public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("获取通知摘要: UserId={UserId}", userId);

            var cacheKey = $"{USER_SUMMARY_CACHE_PREFIX}{userId}";
            
            // 尝试从缓存获取
            var cachedSummary = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedSummary))
            {
                return JsonSerializer.Deserialize<NotificationSummaryDto>(cachedSummary)!;
            }

            // 获取基本信息
            var stats = await GetUserNotificationStatsAsync(userId);
            
            // 获取最近的通知
            var recentFilter = new NotificationFilterDto
            {
                UserId = userId,
                PageSize = 5,
                Sort = NotificationSort.CreatedAtDesc
            };
            var recentNotifications = await GetNotificationsAsync(recentFilter, userId);

            // 获取各类型通知数量
            var typeCounts = await GetNotificationTypeCountsAsync(userId);

            var summary = new NotificationSummaryDto
            {
                TotalNotifications = stats.TotalCount,
                UnreadNotifications = stats.UnreadCount,
                HighPriorityNotifications = stats.HighPriorityCount,
                RecentNotifications = recentNotifications.Items,
                TypeCounts = typeCounts,
                LastCheckedAt = DateTime.UtcNow
            };

            // 缓存结果
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(summary), cacheOptions);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知摘要失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取系统通知统计信息
    /// </summary>
    public async Task<SystemNotificationStatsDto> GetSystemNotificationStatsAsync()
    {
        try
        {
            _logger.LogDebug("获取系统通知统计");

            var cacheKey = SYSTEM_STATS_CACHE_KEY;
            
            // 尝试从缓存获取
            var cachedStats = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedStats))
            {
                return JsonSerializer.Deserialize<SystemNotificationStatsDto>(cachedStats)!;
            }

            // 计算系统统计信息
            var stats = new SystemNotificationStatsDto
            {
                TotalNotifications = await _notificationRepository.CountAsync(),
                ActiveNotifications = await _notificationRepository.CountAsync(isActive: true),
                FailedNotifications = await _notificationRepository.CountAsync(status: NotificationStatus.Failed),
                DeliveredNotifications = await _notificationRepository.CountAsync(status: NotificationStatus.Delivered),
                ReadNotifications = await _notificationRepository.CountAsync(isRead: true),
                HighPriorityNotifications = await _notificationRepository.CountAsync(priority: NotificationPriority.High),
                NotificationsToday = await _notificationRepository.CountAsync(
                    startDate: DateTime.UtcNow.Date),
                NotificationsThisWeek = await _notificationRepository.CountAsync(
                    startDate: DateTime.UtcNow.AddDays(-7)),
                NotificationsThisMonth = await _notificationRepository.CountAsync(
                    startDate: DateTime.UtcNow.AddDays(-30)),
                AvgDeliveryTime = await _notificationRepository.GetAverageDeliveryTimeAsync(),
                SuccessRate = await CalculateSuccessRateAsync(),
                TypeDistribution = await GetSystemTypeDistributionAsync(),
                ChannelStats = await GetSystemChannelStatsAsync(),
                HourlyStats = await GetHourlyStatsAsync(),
                DailyStats = await GetDailyStatsAsync(30),
                TopUsersByNotificationCount = await GetTopUsersByNotificationCountAsync(10),
                PerformanceMetrics = await GetPerformanceMetricsAsync()
            };

            // 缓存结果
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), cacheOptions);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统通知统计失败");
            throw;
        }
    }

    /// <summary>
    /// 获取通知趋势分析
    /// </summary>
    public async Task<NotificationTrendAnalysisDto> GetNotificationTrendAnalysisAsync(NotificationTrendRequestDto request)
    {
        try
        {
            _logger.LogDebug("获取通知趋势分析: UserId={UserId}, Period={Period}", 
                request.UserId, request.Period);

            var endDate = DateTime.UtcNow;
            var startDate = request.Period switch
            {
                TrendPeriod.Last7Days => endDate.AddDays(-7),
                TrendPeriod.Last30Days => endDate.AddDays(-30),
                TrendPeriod.Last90Days => endDate.AddDays(-90),
                TrendPeriod.LastYear => endDate.AddYears(-1),
                _ => endDate.AddDays(-30)
            };

            var analysis = new NotificationTrendAnalysisDto
            {
                Period = request.Period,
                StartDate = startDate,
                EndDate = endDate,
                TotalNotifications = await _notificationRepository.CountAsync(
                    userId: request.UserId, startDate: startDate, endDate: endDate),
                DailyTrends = await GetDailyTrendsAsync(request.UserId, startDate, endDate),
                TypeTrends = await GetTypeTrendsAsync(request.UserId, startDate, endDate),
                ChannelTrends = await GetChannelTrendsAsync(request.UserId, startDate, endDate),
                PriorityTrends = await GetPriorityTrendsAsync(request.UserId, startDate, endDate),
                SuccessRateTrend = await GetSuccessRateTrendAsync(request.UserId, startDate, endDate),
                PeakTimes = await GetPeakNotificationTimesAsync(request.UserId, startDate, endDate),
                UserEngagement = await CalculateUserEngagementAsync(request.UserId, startDate, endDate),
                Recommendations = await GenerateRecommendationsAsync(request.UserId, startDate, endDate)
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知趋势分析失败: UserId={UserId}", request.UserId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户活跃度统计
    /// </summary>
    public async Task<UserActivityStatsDto> GetUserActivityStatsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            _logger.LogDebug("获取用户活跃度统计: UserId={UserId}", userId);

            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var stats = new UserActivityStatsDto
            {
                UserId = userId,
                StartDate = start,
                EndDate = end,
                TotalActions = await _notificationRepository.GetUserActionCountAsync(userId, start, end),
                ReadActions = await _notificationRepository.GetUserActionCountAsync(userId, start, end, "read"),
                DeleteActions = await _notificationRepository.GetUserActionCountAsync(userId, start, end, "delete"),
                ArchiveActions = await _notificationRepository.GetUserActionCountAsync(userId, start, end, "archive"),
                AverageResponseTime = await _notificationRepository.GetAverageResponseTimeAsync(userId, start, end),
                MostActiveHour = await _notificationRepository.GetUserMostActiveHourAsync(userId, start, end),
                MostActiveDay = await _notificationRepository.GetUserMostActiveDayAsync(userId, start, end),
                ActivityByDayOfWeek = await GetUserActivityByDayOfWeekAsync(userId, start, end),
                ActivityByHour = await GetUserActivityByHourAsync(userId, start, end),
                EngagementScore = await CalculateUserEngagementScoreAsync(userId, start, end)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户活跃度统计失败: UserId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 生成通知报告
    /// </summary>
    public async Task<NotificationReportDto> GenerateNotificationReportAsync(NotificationReportRequestDto request)
    {
        try
        {
            _logger.LogDebug("生成通知报告: Type={Type}, Format={Format}", 
                request.ReportType, request.Format);

            var report = new NotificationReportDto
            {
                Id = Guid.NewGuid(),
                ReportType = request.ReportType,
                Format = request.Format,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = request.UserId,
                Period = new DateRangeDto
                {
                    Start = request.StartDate,
                    End = request.EndDate
                },
                Summary = await GenerateReportSummaryAsync(request),
                Data = await GenerateReportDataAsync(request),
                Charts = await GenerateReportChartsAsync(request),
                Insights = await GenerateReportInsightsAsync(request),
                Recommendations = await GenerateReportRecommendationsAsync(request)
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成通知报告失败: Type={Type}", request.ReportType);
            throw;
        }
    }

    // 辅助方法
    private async Task<Dictionary<NotificationType, int>> GetNotificationTypeStatsAsync(Guid userId)
    {
        var stats = new Dictionary<NotificationType, int>();
        var types = Enum.GetValues<NotificationType>();

        foreach (var type in types)
        {
            stats[type] = await _notificationRepository.CountAsync(userId, type: type);
        }

        return stats;
    }

    private async Task<Dictionary<NotificationPriority, int>> GetNotificationPriorityStatsAsync(Guid userId)
    {
        var stats = new Dictionary<NotificationPriority, int>();
        var priorities = Enum.GetValues<NotificationPriority>();

        foreach (var priority in priorities)
        {
            stats[priority] = await _notificationRepository.CountAsync(userId, priority: priority);
        }

        return stats;
    }

    private async Task<Dictionary<NotificationStatus, int>> GetNotificationStatusStatsAsync(Guid userId)
    {
        var stats = new Dictionary<NotificationStatus, int>();
        var statuses = Enum.GetValues<NotificationStatus>();

        foreach (var status in statuses)
        {
            stats[status] = await _notificationRepository.CountAsync(userId, status: status);
        }

        return stats;
    }

    private async Task<Dictionary<NotificationChannel, int>> GetNotificationChannelStatsAsync(Guid userId)
    {
        var stats = new Dictionary<NotificationChannel, int>();
        var channels = Enum.GetValues<NotificationChannel>();

        foreach (var channel in channels)
        {
            stats[channel] = await _notificationRepository.CountAsync(userId, channel: channel);
        }

        return stats;
    }

    private async Task<Dictionary<NotificationType, int>> GetNotificationTypeCountsAsync(Guid userId)
    {
        var counts = new Dictionary<NotificationType, int>();
        var types = Enum.GetValues<NotificationType>();

        foreach (var type in types)
        {
            counts[type] = await _notificationRepository.CountAsync(userId, type: type);
        }

        return counts;
    }

    private async Task<double> CalculateSuccessRateAsync()
    {
        var total = await _notificationRepository.CountAsync();
        var success = await _notificationRepository.CountAsync(status: NotificationStatus.Delivered) +
                      await _notificationRepository.CountAsync(status: NotificationStatus.Read);

        return total > 0 ? (double)success / total * 100 : 0;
    }

    private async Task<Dictionary<NotificationType, double>> GetSystemTypeDistributionAsync()
    {
        var distribution = new Dictionary<NotificationType, double>();
        var total = await _notificationRepository.CountAsync();

        if (total == 0) return distribution;

        var types = Enum.GetValues<NotificationType>();
        foreach (var type in types)
        {
            var count = await _notificationRepository.CountAsync(type: type);
            distribution[type] = (double)count / total * 100;
        }

        return distribution;
    }

    private async Task<Dictionary<NotificationChannel, int>> GetSystemChannelStatsAsync()
    {
        var stats = new Dictionary<NotificationChannel, int>();
        var channels = Enum.GetValues<NotificationChannel>();

        foreach (var channel in channels)
        {
            stats[channel] = await _notificationRepository.CountAsync(channel: channel);
        }

        return stats;
    }

    private async Task<Dictionary<int, int>> GetHourlyStatsAsync()
    {
        var stats = new Dictionary<int, int>();
        for (int hour = 0; hour < 24; hour++)
        {
            stats[hour] = await _notificationRepository.CountByHourAsync(hour);
        }
        return stats;
    }

    private async Task<Dictionary<DateTime, int>> GetDailyStatsAsync(int days)
    {
        var stats = new Dictionary<DateTime, int>();
        for (int i = 0; i < days; i++)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            stats[date] = await _notificationRepository.CountByDateAsync(date);
        }
        return stats;
    }

    private async Task<List<UserNotificationCountDto>> GetTopUsersByNotificationCountAsync(int limit)
    {
        return await _notificationRepository.GetTopUsersByNotificationCountAsync(limit);
    }

    private async Task<NotificationPerformanceMetricsDto> GetPerformanceMetricsAsync()
    {
        return new NotificationPerformanceMetricsDto
        {
            AverageDeliveryTime = await _notificationRepository.GetAverageDeliveryTimeAsync(),
            SuccessRate = await CalculateSuccessRateAsync(),
            AverageProcessingTime = await _notificationRepository.GetAverageProcessingTimeAsync(),
            PeakHour = await _notificationRepository.GetPeakHourAsync(),
            SystemLoad = await _notificationRepository.GetSystemLoadAsync()
        };
    }

    // 更多辅助方法实现...
    private async Task<List<DailyTrendDto>> GetDailyTrendsAsync(Guid userId, DateTime start, DateTime end)
    {
        var trends = new List<DailyTrendDto>();
        var current = start.Date;

        while (current <= end.Date)
        {
            var nextDay = current.AddDays(1);
            var count = await _notificationRepository.CountAsync(
                userId: userId, startDate: current, endDate: nextDay);

            trends.Add(new DailyTrendDto
            {
                Date = current,
                Count = count
            });

            current = nextDay;
        }

        return trends;
    }

    private async Task<double> CalculateUserEngagementScoreAsync(Guid userId, DateTime start, DateTime end)
    {
        var totalNotifications = await _notificationRepository.CountAsync(
            userId: userId, startDate: start, endDate: end);
        var readNotifications = await _notificationRepository.CountAsync(
            userId: userId, startDate: start, endDate: end, isRead: true);

        return totalNotifications > 0 ? (double)readNotifications / totalNotifications * 100 : 0;
    }

    private async Task<Dictionary<DayOfWeek, int>> GetUserActivityByDayOfWeekAsync(Guid userId, DateTime start, DateTime end)
    {
        var activity = new Dictionary<DayOfWeek, int>();
        var days = Enum.GetValues<DayOfWeek>();

        foreach (var day in days)
        {
            activity[day] = await _notificationRepository.CountByDayOfWeekAsync(userId, day, start, end);
        }

        return activity;
    }

    private async Task<Dictionary<int, int>> GetUserActivityByHourAsync(Guid userId, DateTime start, DateTime end)
    {
        var activity = new Dictionary<int, int>();
        for (int hour = 0; hour < 24; hour++)
        {
            activity[hour] = await _notificationRepository.CountByHourAsync(userId, hour, start, end);
        }
        return activity;
    }

    // 简化其他辅助方法的实现...
    private async Task<List<TypeTrendDto>> GetTypeTrendsAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<TypeTrendDto>(); // 简化实现
    }

    private async Task<List<ChannelTrendDto>> GetChannelTrendsAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<ChannelTrendDto>(); // 简化实现
    }

    private async Task<List<PriorityTrendDto>> GetPriorityTrendsAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<PriorityTrendDto>(); // 简化实现
    }

    private async Task<List<SuccessRateTrendDto>> GetSuccessRateTrendAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<SuccessRateTrendDto>(); // 简化实现
    }

    private async Task<List<PeakTimeDto>> GetPeakNotificationTimesAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<PeakTimeDto>(); // 简化实现
    }

    private async Task<UserEngagementDto> CalculateUserEngagementAsync(Guid userId, DateTime start, DateTime end)
    {
        return new UserEngagementDto(); // 简化实现
    }

    private async Task<List<string>> GenerateRecommendationsAsync(Guid userId, DateTime start, DateTime end)
    {
        return new List<string>(); // 简化实现
    }

    private async Task<ReportSummaryDto> GenerateReportSummaryAsync(NotificationReportRequestDto request)
    {
        return new ReportSummaryDto(); // 简化实现
    }

    private async Task<object> GenerateReportDataAsync(NotificationReportRequestDto request)
    {
        return new object(); // 简化实现
    }

    private async Task<List<ChartDto>> GenerateReportChartsAsync(NotificationReportRequestDto request)
    {
        return new List<ChartDto>(); // 简化实现
    }

    private async Task<List<string>> GenerateReportInsightsAsync(NotificationReportRequestDto request)
    {
        return new List<string>(); // 简化实现
    }

    private async Task<List<string>> GenerateReportRecommendationsAsync(NotificationReportRequestDto request)
    {
        return new List<string>(); // 简化实现
    }

    #endregion
}