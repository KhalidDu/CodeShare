using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 消息服务实现 - 遵循单一职责原则和依赖倒置原则
/// 提供完整的消息发送、接收、管理、附件处理和会话管理业务逻辑
/// </summary>
public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMessageAttachmentRepository _messageAttachmentRepository;
    private readonly IMessageDraftRepository _messageDraftRepository;
    private readonly IMessageConversationRepository _messageConversationRepository;
    private readonly ICacheService _cacheService;
    private readonly IPermissionService _permissionService;
    private readonly IInputValidationService _inputValidationService;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MessageService> _logger;

    // 缓存键前缀
    private const string MESSAGE_CACHE_PREFIX = "message_";
    private const string USER_MESSAGES_CACHE_PREFIX = "user_messages_";
    private const string CONVERSATION_CACHE_PREFIX = "conversation_";
    private const string MESSAGE_STATS_CACHE_PREFIX = "message_stats_";

    public MessageService(
        IMessageRepository messageRepository,
        IMessageAttachmentRepository messageAttachmentRepository,
        IMessageDraftRepository messageDraftRepository,
        IMessageConversationRepository messageConversationRepository,
        ICacheService cacheService,
        IPermissionService permissionService,
        IInputValidationService inputValidationService,
        INotificationService notificationService,
        IFileStorageService fileStorageService,
        ILogger<MessageService> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _messageAttachmentRepository = messageAttachmentRepository ?? throw new ArgumentNullException(nameof(messageAttachmentRepository));
        _messageDraftRepository = messageDraftRepository ?? throw new ArgumentNullException(nameof(messageDraftRepository));
        _messageConversationRepository = messageConversationRepository ?? throw new ArgumentNullException(nameof(messageConversationRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 消息发送和接收操作

    /// <summary>
    /// 验证消息权限
    /// </summary>
    public async Task<bool> ValidateMessagePermissionAsync(Guid messageId, Guid userId, Interfaces.MessagePermission permission)
    {
        return await _messageRepository.ValidateMessagePermissionAsync(messageId, userId, permission);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public async Task<MessageDto> SendMessageAsync(CreateMessageDto request, Guid senderId)
    {
        try
        {
            _logger.LogInformation("用户 {SenderId} 尝试发送消息给 {ReceiverId}", senderId, request.ReceiverId);

            // 验证输入数据
            await ValidateMessageInputAsync(request);

            // 检查用户权限
            var canSendMessage = await _permissionService.CanSendMessageAsync(senderId, request.ReceiverId);
            if (!canSendMessage)
            {
                _logger.LogWarning("用户 {SenderId} 无权限发送消息给 {ReceiverId}", senderId, request.ReceiverId);
                throw new UnauthorizedAccessException("您没有权限发送消息给此用户");
            }

            // 如果是回复消息，检查父消息是否存在
            if (request.ParentId.HasValue)
            {
                var parentMessage = await _messageRepository.GetByIdAsync(request.ParentId.Value);
                if (parentMessage == null)
                {
                    _logger.LogWarning("父消息 {ParentId} 不存在", request.ParentId.Value);
                    throw new ArgumentException("父消息不存在");
                }
            }

            // 如果有会话ID，检查用户是否是会话参与者
            if (request.ConversationId.HasValue)
            {
                var isParticipant = await _messageConversationRepository.IsConversationParticipantAsync(request.ConversationId.Value, senderId);
                if (!isParticipant)
                {
                    _logger.LogWarning("用户 {SenderId} 不是会话 {ConversationId} 的参与者", senderId, request.ConversationId.Value);
                    throw new UnauthorizedAccessException("您不是此会话的参与者");
                }
            }

            // 创建消息实体
            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Subject = request.Subject,
                Content = request.Content,
                MessageType = request.MessageType,
                Priority = request.Priority,
                ParentId = request.ParentId,
                ConversationId = request.ConversationId,
                Tag = request.Tag,
                ExpiresAt = request.ExpiresAt,
                Status = MessageStatus.Sent,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 保存消息
            var createdMessage = await _messageRepository.CreateAsync(message);

            // 如果有附件ID列表，将附件关联到消息
            if (request.AttachmentIds != null && request.AttachmentIds.Any())
            {
                await AttachFilesToMessageAsync(createdMessage.Id, request.AttachmentIds, senderId);
            }

            // 发送通知给接收者
            await SendMessageNotificationAsync(createdMessage);

            // 清除相关缓存
            await ClearUserMessageCacheAsync(senderId);
            await ClearUserMessageCacheAsync(request.ReceiverId);
            if (request.ConversationId.HasValue)
            {
                await ClearConversationCacheAsync(request.ConversationId.Value);
            }

            _logger.LogInformation("用户 {SenderId} 成功发送消息 {MessageId} 给 {ReceiverId}", senderId, createdMessage.Id, request.ReceiverId);

            return await MapToMessageDtoAsync(createdMessage, senderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量发送消息
    /// </summary>
    public async Task<IEnumerable<MessageDto>> SendMultipleMessagesAsync(IEnumerable<CreateMessageDto> requests, Guid senderId)
    {
        try
        {
            _logger.LogInformation("用户 {SenderId} 尝试批量发送 {Count} 条消息", senderId, requests.Count());

            var messageDtos = new List<MessageDto>();
            var results = new List<MessageDto>();

            foreach (var request in requests)
            {
                try
                {
                    var messageDto = await SendMessageAsync(request, senderId);
                    messageDtos.Add(messageDto);
                    results.Add(messageDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批量发送消息时单条消息发送失败: {Message}", ex.Message);
                    // 继续发送其他消息，不中断整个批量操作
                }
            }

            _logger.LogInformation("用户 {SenderId} 成功批量发送 {SuccessCount}/{TotalCount} 条消息", 
                senderId, results.Count, requests.Count());

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量发送消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 回复消息
    /// </summary>
    public async Task<MessageDto> ReplyMessageAsync(Guid parentMessageId, CreateMessageDto request, Guid replyerId)
    {
        try
        {
            _logger.LogInformation("用户 {ReplyerId} 尝试回复消息 {ParentMessageId}", replyerId, parentMessageId);

            // 获取父消息
            var parentMessage = await _messageRepository.GetByIdAsync(parentMessageId);
            if (parentMessage == null)
            {
                _logger.LogWarning("父消息 {ParentMessageId} 不存在", parentMessageId);
                throw new ArgumentException("父消息不存在");
            }

            // 验证回复权限
            var canReply = await ValidateMessagePermissionAsync(parentMessageId, replyerId, MessagePermission.Reply);
            if (!canReply)
            {
                _logger.LogWarning("用户 {ReplyerId} 无权限回复消息 {ParentMessageId}", replyerId, parentMessageId);
                throw new UnauthorizedAccessException("您没有权限回复此消息");
            }

            // 设置回复参数
            request.ParentId = parentMessageId;
            request.ConversationId = parentMessage.ConversationId;
            request.ReceiverId = parentMessage.SenderId; // 回复给原发送者
            request.Subject = parentMessage.Subject != null ? $"Re: {parentMessage.Subject}" : null;

            // 发送回复消息
            var replyMessage = await SendMessageAsync(request, replyerId);

            // 更新父消息状态为已回复
            await _messageRepository.UpdateStatusAsync(parentMessageId, MessageStatus.Replied);

            _logger.LogInformation("用户 {ReplyerId} 成功回复消息 {ParentMessageId}，回复ID: {ReplyId}", 
                replyerId, parentMessageId, replyMessage.Id);

            return replyMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回复消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 转发消息
    /// </summary>
    public async Task<MessageDto> ForwardMessageAsync(Guid originalMessageId, CreateMessageDto request, Guid forwarderId)
    {
        try
        {
            _logger.LogInformation("用户 {ForwarderId} 尝试转发消息 {OriginalMessageId}", forwarderId, originalMessageId);

            // 获取原消息
            var originalMessage = await _messageRepository.GetByIdAsync(originalMessageId);
            if (originalMessage == null)
            {
                _logger.LogWarning("原消息 {OriginalMessageId} 不存在", originalMessageId);
                throw new ArgumentException("原消息不存在");
            }

            // 验证转发权限
            var canForward = await ValidateMessagePermissionAsync(originalMessageId, forwarderId, MessagePermission.Forward);
            if (!canForward)
            {
                _logger.LogWarning("用户 {ForwarderId} 无权限转发消息 {OriginalMessageId}", forwarderId, originalMessageId);
                throw new UnauthorizedAccessException("您没有权限转发此消息");
            }

            // 设置转发内容
            var forwardContent = $"""
                ---------- 转发消息 ----------
                原发送者: {originalMessage.SenderId}
                原发送时间: {originalMessage.CreatedAt}
                原消息内容:
                {originalMessage.Content}
                ---------- 我的回复 ----------
                {request.Content}
                """;

            request.Content = forwardContent;
            request.Subject = originalMessage.Subject != null ? $"Fwd: {originalMessage.Subject}" : null;

            // 发送转发消息
            var forwardedMessage = await SendMessageAsync(request, forwarderId);

            // 更新原消息状态为已转发
            await _messageRepository.UpdateStatusAsync(originalMessageId, MessageStatus.Forwarded);

            _logger.LogInformation("用户 {ForwarderId} 成功转发消息 {OriginalMessageId} 给 {ReceiverId}，转发ID: {ForwardedId}", 
                forwarderId, originalMessageId, request.ReceiverId, forwardedMessage.Id);

            return forwardedMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转发消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取消息详情
    /// </summary>
    public async Task<MessageDto?> GetMessageAsync(Guid messageId, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取消息详情: {MessageId}", messageId);

            // 验证查看权限
            var canView = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.View);
            if (!canView)
            {
                _logger.LogWarning("用户 {UserId} 无权限查看消息 {MessageId}", currentUserId, messageId);
                throw new UnauthorizedAccessException("您没有权限查看此消息");
            }

            // 尝试从缓存获取
            var cacheKey = $"{MESSAGE_CACHE_PREFIX}{messageId}";
            var cachedMessage = await _cacheService.GetAsync<MessageDto>(cacheKey);
            if (cachedMessage != null)
            {
                // 更新当前用户的权限状态
                cachedMessage.IsSender = cachedMessage.SenderId == currentUserId;
                cachedMessage.IsReceiver = cachedMessage.ReceiverId == currentUserId;
                cachedMessage.CanEdit = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Edit);
                cachedMessage.CanDelete = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Delete);
                cachedMessage.CanReply = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Reply);
                cachedMessage.CanForward = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Forward);
                return cachedMessage;
            }

            // 从数据库获取
            var message = await _messageRepository.GetByIdWithDetailsAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning("消息 {MessageId} 不存在", messageId);
                return null;
            }

            // 映射为DTO
            var messageDto = await MapToMessageDtoAsync(message, currentUserId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, messageDto, TimeSpan.FromMinutes(10));

            return messageDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息详情时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的消息列表
    /// </summary>
    public async Task<PaginatedResult<MessageDto>> GetUserMessagesAsync(Guid userId, MessageFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的消息列表", userId);

            // 验证权限：只能查看自己的消息
            if (userId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的消息");
            }

            // 获取消息列表
            var result = await _messageRepository.GetUserMessagesAsync(userId, filter);

            // 映射为DTO
            var messageDtos = new List<MessageDto>();
            foreach (var message in result.Items)
            {
                messageDtos.Add(await MapToMessageDtoAsync(message, currentUserId));
            }

            return new PaginatedResult<MessageDto>
            {
                Items = messageDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户消息列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户发送的消息列表
    /// </summary>
    public async Task<PaginatedResult<MessageDto>> GetSentMessagesAsync(Guid senderId, MessageFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {SenderId} 的已发送消息列表", senderId);

            // 验证权限：只能查看自己的已发送消息
            if (senderId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的已发送消息");
            }

            filter.SenderId = senderId;
            return await GetUserMessagesAsync(senderId, filter, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户已发送消息列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户接收的消息列表
    /// </summary>
    public async Task<PaginatedResult<MessageDto>> GetReceivedMessagesAsync(Guid receiverId, MessageFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {ReceiverId} 的已接收消息列表", receiverId);

            // 验证权限：只能查看自己的已接收消息
            if (receiverId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的已接收消息");
            }

            filter.ReceiverId = receiverId;
            return await GetUserMessagesAsync(receiverId, filter, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户已接收消息列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户未读消息列表
    /// </summary>
    public async Task<PaginatedResult<MessageDto>> GetUnreadMessagesAsync(Guid userId, MessageFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的未读消息列表", userId);

            // 验证权限：只能查看自己的未读消息
            if (userId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的未读消息");
            }

            filter.IsRead = false;
            return await GetUserMessagesAsync(userId, filter, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户未读消息列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 标记消息为已读
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试标记消息 {MessageId} 为已读", userId, messageId);

            // 验证权限
            var canView = await ValidateMessagePermissionAsync(messageId, userId, MessagePermission.View);
            if (!canView)
            {
                _logger.LogWarning("用户 {UserId} 无权限查看消息 {MessageId}", userId, messageId);
                return false;
            }

            // 检查是否是接收者
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null || message.ReceiverId != userId)
            {
                _logger.LogWarning("消息 {MessageId} 不存在或用户 {UserId} 不是接收者", messageId, userId);
                return false;
            }

            // 如果已经已读，直接返回成功
            if (message.IsRead)
            {
                return true;
            }

            // 标记为已读
            var result = await _messageRepository.MarkAsReadAsync(messageId, userId);

            if (result)
            {
                // 清除相关缓存
                await ClearMessageCacheAsync(messageId);
                await ClearUserMessageCacheAsync(userId);

                _logger.LogInformation("用户 {UserId} 成功标记消息 {MessageId} 为已读", userId, messageId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记消息为已读时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量标记消息为已读
    /// </summary>
    public async Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> messageIds, Guid userId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试批量标记 {Count} 条消息为已读", userId, messageIds.Count());

            if (!messageIds.Any())
            {
                return 0;
            }

            var successCount = 0;
            foreach (var messageId in messageIds)
            {
                try
                {
                    if (await MarkAsReadAsync(messageId, userId))
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批量标记已读时单条消息处理失败: {MessageId}", messageId);
                    // 继续处理其他消息
                }
            }

            _logger.LogInformation("用户 {UserId} 成功批量标记 {SuccessCount}/{TotalCount} 条消息为已读", 
                userId, successCount, messageIds.Count());

            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记消息为已读时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 更新消息
    /// </summary>
    public async Task<MessageDto> UpdateMessageAsync(Guid messageId, UpdateMessageDto request, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试更新消息 {MessageId}", currentUserId, messageId);

            // 验证编辑权限
            var canEdit = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Edit);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限编辑消息 {MessageId}", currentUserId, messageId);
                throw new UnauthorizedAccessException("您没有权限编辑此消息");
            }

            // 获取现有消息
            var existingMessage = await _messageRepository.GetByIdAsync(messageId);
            if (existingMessage == null)
            {
                _logger.LogWarning("消息 {MessageId} 不存在", messageId);
                throw new ArgumentException("消息不存在");
            }

            // 验证消息是否可以编辑（已读的消息通常不能编辑）
            if (existingMessage.IsRead)
            {
                throw new InvalidOperationException("已读的消息不能编辑");
            }

            // 验证消息是否已过期
            if (existingMessage.ExpiresAt.HasValue && existingMessage.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("已过期的消息不能编辑");
            }

            // 更新消息内容
            existingMessage.Subject = request.Subject ?? existingMessage.Subject;
            existingMessage.Content = request.Content ?? existingMessage.Content;
            existingMessage.Priority = request.Priority ?? existingMessage.Priority;
            existingMessage.Tag = request.Tag ?? existingMessage.Tag;
            existingMessage.ExpiresAt = request.ExpiresAt ?? existingMessage.ExpiresAt;
            existingMessage.UpdatedAt = DateTime.UtcNow;

            // 保存更新
            var updatedMessage = await _messageRepository.UpdateAsync(existingMessage);

            // 清除相关缓存
            await ClearMessageCacheAsync(messageId);
            await ClearUserMessageCacheAsync(currentUserId);

            _logger.LogInformation("用户 {UserId} 成功更新消息 {MessageId}", currentUserId, messageId);

            return await MapToMessageDtoAsync(updatedMessage, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试删除消息 {MessageId}", currentUserId, messageId);

            // 验证删除权限
            var canDelete = await ValidateMessagePermissionAsync(messageId, currentUserId, MessagePermission.Delete);
            if (!canDelete)
            {
                _logger.LogWarning("用户 {UserId} 无权限删除消息 {MessageId}", currentUserId, messageId);
                throw new UnauthorizedAccessException("您没有权限删除此消息");
            }

            // 执行软删除
            var result = await _messageRepository.SoftDeleteAsync(messageId, currentUserId);

            if (result)
            {
                // 清除相关缓存
                await ClearMessageCacheAsync(messageId);
                await ClearUserMessageCacheAsync(currentUserId);

                _logger.LogInformation("用户 {UserId} 成功删除消息 {MessageId}", currentUserId, messageId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量删除消息
    /// </summary>
    public async Task<BatchOperationResult> BatchDeleteMessagesAsync(BatchMessageOperationDto request, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试批量删除 {Count} 条消息", currentUserId, request.MessageIds.Count);

            if (!request.MessageIds.Any())
            {
                return new BatchOperationResult
                {
                    SuccessCount = 0,
                    FailedCount = 0,
                    Errors = new List<string> { "消息ID列表为空" }
                };
            }

            var result = new BatchOperationResult();
            var errors = new List<string>();

            foreach (var messageId in request.MessageIds)
            {
                try
                {
                    if (await DeleteMessageAsync(messageId, currentUserId))
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        errors.Add($"删除消息 {messageId} 失败");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    errors.Add($"删除消息 {messageId} 失败: {ex.Message}");
                }
            }

            result.Errors = errors;

            _logger.LogInformation("用户 {UserId} 批量删除消息完成: 成功 {SuccessCount}, 失败 {FailedCount}", 
                currentUserId, result.SuccessCount, result.FailedCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 搜索消息
    /// </summary>
    public async Task<MessageSearchResultDto> SearchMessagesAsync(string searchTerm, MessageFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("搜索消息: {SearchTerm}", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("搜索词不能为空");
            }

            if (searchTerm.Length > 100)
            {
                throw new ArgumentException("搜索词长度不能超过100个字符");
            }

            // 使用输入验证服务验证搜索词
            await _inputValidationService.ValidateSearchTermAsync(searchTerm);

            var startTime = DateTime.UtcNow;
            
            // 执行搜索
            filter.Search = searchTerm;
            var result = await _messageRepository.SearchMessagesAsync(searchTerm, filter, currentUserId);

            var searchTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // 映射搜索结果
            var messageDtos = new List<MessageDto>();
            foreach (var message in result.Items)
            {
                messageDtos.Add(await MapToMessageDtoAsync(message, currentUserId));
            }

            return new MessageSearchResultDto
            {
                Messages = messageDtos,
                TotalCount = result.TotalCount,
                HighlightedTerms = new List<string> { searchTerm },
                SearchTime = searchTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户消息统计信息
    /// </summary>
    public async Task<MessageStatsDto> GetUserMessageStatsAsync(Guid userId, MessageStatsFilterDto filter)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的消息统计信息", userId);

            // 尝试从缓存获取
            var cacheKey = $"{MESSAGE_STATS_CACHE_PREFIX}user_{userId}";
            var cachedStats = await _cacheService.GetAsync<MessageStatsDto>(cacheKey);
            if (cachedStats != null)
            {
                return cachedStats;
            }

            // 从数据库获取统计信息
            var stats = await _messageRepository.GetUserMessageStatsAsync(userId, filter);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(5));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户消息统计信息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 验证消息输入数据
    /// </summary>
    private async Task ValidateMessageInputAsync(CreateMessageDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.ReceiverId == Guid.Empty)
        {
            throw new ArgumentException("接收者ID不能为空");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("消息内容不能为空");
        }

        if (request.Content.Length > 2000)
        {
            throw new ArgumentException("消息内容长度不能超过2000个字符");
        }

        if (request.Subject != null && request.Subject.Length > 200)
        {
            throw new ArgumentException("消息主题长度不能超过200个字符");
        }

        if (request.Tag != null && request.Tag.Length > 100)
        {
            throw new ArgumentException("消息标签长度不能超过100个字符");
        }

        // 验证过期时间
        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
        {
            throw new ArgumentException("过期时间必须晚于当前时间");
        }

        // 使用输入验证服务验证内容
        await _inputValidationService.ValidateMessageContentAsync(request.Content);

        // 如果有主题，也验证主题
        if (!string.IsNullOrWhiteSpace(request.Subject))
        {
            await _inputValidationService.ValidateMessageSubjectAsync(request.Subject);
        }
    }

    /// <summary>
    /// 将文件附件关联到消息
    /// </summary>
    private async Task AttachFilesToMessageAsync(Guid messageId, IEnumerable<Guid> attachmentIds, Guid userId)
    {
        try
        {
            foreach (var attachmentId in attachmentIds)
            {
                await _messageAttachmentRepository.AttachToMessageAsync(attachmentId, messageId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "将文件附件关联到消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 发送消息通知
    /// </summary>
    private async Task SendMessageNotificationAsync(Message message)
    {
        try
        {
            var notification = new MessageNotificationDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Subject = message.Subject,
                Content = message.Content.Length > 100 ? message.Content.Substring(0, 100) + "..." : message.Content,
                MessageType = message.MessageType,
                Priority = message.Priority,
                CreatedAt = message.CreatedAt
            };

            await _notificationService.SendMessageNotificationAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息通知时发生错误: {Message}", ex.Message);
            // 通知发送失败不影响主要功能，只记录日志
        }
    }

    /// <summary>
    /// 映射消息实体到DTO
    /// </summary>
    private async Task<MessageDto> MapToMessageDtoAsync(Message message, Guid currentUserId)
    {
        var messageDto = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender?.UserName ?? "未知用户",
            SenderAvatar = message.Sender?.AvatarUrl,
            ReceiverId = message.ReceiverId,
            ReceiverName = message.Receiver?.UserName ?? "未知用户",
            ReceiverAvatar = message.Receiver?.AvatarUrl,
            Subject = message.Subject,
            Content = message.Content,
            MessageType = message.MessageType,
            Status = message.Status,
            Priority = message.Priority,
            ParentId = message.ParentId,
            ConversationId = message.ConversationId,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            Tag = message.Tag,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            ExpiresAt = message.ExpiresAt
        };

        // 设置当前用户相关状态
        messageDto.IsSender = message.SenderId == currentUserId;
        messageDto.IsReceiver = message.ReceiverId == currentUserId;
        messageDto.CanEdit = await ValidateMessagePermissionAsync(message.Id, currentUserId, MessagePermission.Edit);
        messageDto.CanDelete = await ValidateMessagePermissionAsync(message.Id, currentUserId, MessagePermission.Delete);
        messageDto.CanReply = await ValidateMessagePermissionAsync(message.Id, currentUserId, MessagePermission.Reply);
        messageDto.CanForward = await ValidateMessagePermissionAsync(message.Id, currentUserId, MessagePermission.Forward);

        return messageDto;
    }

  
    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除消息缓存
    /// </summary>
    public async Task<bool> ClearMessageCacheAsync(Guid messageId)
    {
        try
        {
            var cacheKey = $"{MESSAGE_CACHE_PREFIX}{messageId}";
            var result = await _cacheService.RemoveAsync(cacheKey);
            
            if (result)
            {
                _logger.LogDebug("成功清除消息缓存: {MessageId}", messageId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除消息缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 清除用户消息缓存
    /// </summary>
    public async Task<bool> ClearUserMessageCacheAsync(Guid userId)
    {
        try
        {
            var result = true;
            var cacheKeys = new[]
            {
                $"{USER_MESSAGES_CACHE_PREFIX}{userId}_sent",
                $"{USER_MESSAGES_CACHE_PREFIX}{userId}_received",
                $"{USER_MESSAGES_CACHE_PREFIX}{userId}_unread",
                $"{MESSAGE_STATS_CACHE_PREFIX}user_{userId}"
            };

            foreach (var cacheKey in cacheKeys)
            {
                var cleared = await _cacheService.RemoveAsync(cacheKey);
                if (!cleared)
                {
                    result = false;
                    _logger.LogWarning("清除缓存失败: {CacheKey}", cacheKey);
                }
            }

            if (result)
            {
                _logger.LogDebug("成功清除用户消息缓存: {UserId}", userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户消息缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 清除会话缓存
    /// </summary>
    public async Task<bool> ClearConversationCacheAsync(Guid conversationId)
    {
        try
        {
            var cacheKey = $"{CONVERSATION_CACHE_PREFIX}{conversationId}";
            var result = await _cacheService.RemoveAsync(cacheKey);
            
            if (result)
            {
                _logger.LogDebug("成功清除会话缓存: {ConversationId}", conversationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除会话缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    #endregion

    #region 会话管理操作（简化实现）

    /// <summary>
    /// 创建会话
    /// </summary>
    public async Task<ConversationDto> CreateConversationAsync(CreateConversationDto request, Guid creatorId)
    {
        try
        {
            _logger.LogInformation("用户 {CreatorId} 尝试创建会话", creatorId);

            // 验证输入数据
            await ValidateConversationInputAsync(request);

            // 检查参与者权限
            foreach (var participantId in request.ParticipantIds)
            {
                var canAddToConversation = await _permissionService.CanAddToConversationAsync(creatorId, participantId);
                if (!canAddToConversation)
                {
                    _logger.LogWarning("用户 {CreatorId} 无权限将用户 {ParticipantId} 添加到会话", creatorId, participantId);
                    throw new UnauthorizedAccessException($"您无权限将用户添加到会话");
                }
            }

            // 创建会话实体
            var conversation = new MessageConversation
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                ConversationType = request.ConversationType,
                CreatedBy = creatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPinned = request.IsPinned,
                IsMuted = request.IsMuted,
                Status = ConversationStatus.Active
            };

            // 保存会话
            var createdConversation = await _messageConversationRepository.CreateAsync(conversation);

            // 添加参与者（包括创建者）
            var allParticipants = new List<Guid> { creatorId };
            allParticipants.AddRange(request.ParticipantIds);
            
            await _messageConversationRepository.AddParticipantsAsync(createdConversation.Id, allParticipants, creatorId);

            // 如果有初始消息，发送初始消息
            if (!string.IsNullOrWhiteSpace(request.InitialMessage))
            {
                var initialMessage = new CreateMessageDto
                {
                    Content = request.InitialMessage,
                    MessageType = MessageType.User,
                    ConversationId = createdConversation.Id
                };

                // 给每个参与者发送消息
                foreach (var participantId in request.ParticipantIds)
                {
                    if (participantId != creatorId)
                    {
                        initialMessage.ReceiverId = participantId;
                        await SendMessageAsync(initialMessage, creatorId);
                    }
                }
            }

            _logger.LogInformation("用户 {CreatorId} 成功创建会话 {ConversationId}", creatorId, createdConversation.Id);

            return await MapToConversationDtoAsync(createdConversation, creatorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建会话时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取会话列表
    /// </summary>
    public async Task<PaginatedResult<ConversationDto>> GetConversationsAsync(Guid userId, MessageConversationFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的会话列表", userId);

            // 验证权限：只能查看自己的会话
            if (userId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的会话");
            }

            // 获取会话列表
            var result = await _messageConversationRepository.GetUserConversationsAsync(userId, filter);

            // 映射为DTO
            var conversationDtos = new List<ConversationDto>();
            foreach (var conversation in result.Items)
            {
                conversationDtos.Add(await MapToConversationDtoAsync(conversation, currentUserId));
            }

            return new PaginatedResult<ConversationDto>
            {
                Items = conversationDtos,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取会话详情
    /// </summary>
    public async Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取会话详情: {ConversationId}", conversationId);

            // 验证用户是否是会话参与者
            var isParticipant = await _messageConversationRepository.IsConversationParticipantAsync(conversationId, currentUserId);
            if (!isParticipant && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您不是此会话的参与者");
            }

            // 从数据库获取会话
            var conversation = await _messageConversationRepository.GetByIdWithDetailsAsync(conversationId);
            if (conversation == null)
            {
                _logger.LogWarning("会话 {ConversationId} 不存在", conversationId);
                return null;
            }

            return await MapToConversationDtoAsync(conversation, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话详情时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 发送会话消息
    /// </summary>
    public async Task<MessageDto> SendConversationMessageAsync(Guid conversationId, CreateMessageDto request, Guid senderId)
    {
        try
        {
            _logger.LogInformation("用户 {SenderId} 尝试向会话 {ConversationId} 发送消息", senderId, conversationId);

            // 验证用户是否是会话参与者
            var isParticipant = await _messageConversationRepository.IsConversationParticipantAsync(conversationId, senderId);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("您不是此会话的参与者");
            }

            // 获取会话所有参与者（除了发送者）
            var participants = await _messageConversationRepository.GetConversationParticipantsAsync(conversationId);
            var otherParticipants = participants.Where(p => p != senderId).ToList();

            if (!otherParticipants.Any())
            {
                throw new InvalidOperationException("会话中没有其他参与者");
            }

            // 设置会话ID
            request.ConversationId = conversationId;

            // 向每个参与者发送消息
            var messageDtos = new List<MessageDto>();
            foreach (var participantId in otherParticipants)
            {
                request.ReceiverId = participantId;
                var messageDto = await SendMessageAsync(request, senderId);
                messageDtos.Add(messageDto);
            }

            // 返回第一条消息作为结果
            return messageDtos.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送会话消息时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 映射会话实体到DTO
    /// </summary>
    private async Task<ConversationDto> MapToConversationDtoAsync(MessageConversation conversation, Guid currentUserId)
    {
        var participants = await _messageConversationRepository.GetConversationParticipantsAsync(conversation.Id);
        var lastMessage = await _messageRepository.GetConversationLastMessageAsync(conversation.Id);

        return new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Description = conversation.Description,
            ConversationType = conversation.ConversationType,
            CreatedBy = conversation.CreatedBy,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            IsPinned = conversation.IsPinned,
            IsMuted = conversation.IsMuted,
            IsArchived = conversation.IsArchived,
            Status = conversation.Status,
            ParticipantCount = participants.Count,
            Participants = participants.Select(p => new ConversationParticipantDto
            {
                UserId = p,
                JoinedAt = conversation.CreatedAt // 简化实现
            }).ToList(),
            LastMessage = lastMessage != null ? await MapToMessageDtoAsync(lastMessage, currentUserId) : null,
            UnreadCount = await _messageRepository.GetConversationUnreadCountAsync(conversation.Id, currentUserId)
        };
    }

    /// <summary>
    /// 验证会话输入数据
    /// </summary>
    private async Task ValidateConversationInputAsync(CreateConversationDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("会话标题不能为空");
        }

        if (request.Title.Length > 200)
        {
            throw new ArgumentException("会话标题长度不能超过200个字符");
        }

        if (request.Description != null && request.Description.Length > 500)
        {
            throw new ArgumentException("会话描述长度不能超过500个字符");
        }

        if (request.ParticipantIds == null || !request.ParticipantIds.Any())
        {
            throw new ArgumentException("参与者列表不能为空");
        }

        if (request.InitialMessage != null && request.InitialMessage.Length > 2000)
        {
            throw new ArgumentException("初始消息长度不能超过2000个字符");
        }

        // 使用输入验证服务验证内容
        await _inputValidationService.ValidateConversationTitleAsync(request.Title);

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            await _inputValidationService.ValidateConversationDescriptionAsync(request.Description);
        }

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            await _inputValidationService.ValidateMessageContentAsync(request.InitialMessage);
        }
    }

    #endregion

    #region 其他接口方法的简化实现

    public async Task<IEnumerable<MessageDto>> GetMessagesByIdsAsync(IEnumerable<Guid> messageIds, Guid currentUserId)
    {
        var messages = new List<MessageDto>();
        foreach (var messageId in messageIds)
        {
            var message = await GetMessageAsync(messageId, currentUserId);
            if (message != null)
            {
                messages.Add(message);
            }
        }
        return messages;
    }

    public async Task<MessageThreadDto> GetMessageThreadAsync(Guid rootMessageId, Guid currentUserId)
    {
        var rootMessage = await GetMessageAsync(rootMessageId, currentUserId);
        if (rootMessage == null)
        {
            throw new ArgumentException("根消息不存在");
        }

        var replies = await _messageRepository.GetMessageRepliesAsync(rootMessageId);
        var replyDtos = new List<MessageDto>();
        
        foreach (var reply in replies)
        {
            replyDtos.Add(await MapToMessageDtoAsync(reply, currentUserId));
        }

        return new MessageThreadDto
        {
            RootMessage = rootMessage,
            Replies = replyDtos,
            TotalReplies = replyDtos.Count,
            LastReplyAt = replyDtos.Any() ? replyDtos.Max(r => r.CreatedAt) : null
        };
    }

    public async Task<PaginatedResult<MessageDto>> GetConversationMessagesAsync(Guid conversationId, MessageFilterDto filter, Guid currentUserId)
    {
        // 验证用户是否是会话参与者
        var isParticipant = await _messageConversationRepository.IsConversationParticipantAsync(conversationId, currentUserId);
        if (!isParticipant && !await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("您不是此会话的参与者");
        }

        filter.ConversationId = conversationId;
        filter.ReceiverId = currentUserId; // 只获取当前用户接收的消息
        
        return await GetUserMessagesAsync(currentUserId, filter, currentUserId);
    }

    public async Task<ConversationDto> UpdateConversationAsync(Guid conversationId, UpdateConversationDto request, Guid currentUserId)
    {
        throw new NotImplementedException("更新会话功能尚未实现");
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid currentUserId)
    {
        throw new NotImplementedException("删除会话功能尚未实现");
    }

    public async Task<BulkOperationResultDto> AddConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds, Guid currentUserId)
    {
        throw new NotImplementedException("添加会话参与者功能尚未实现");
    }

    public async Task<bool> RemoveConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds, Guid currentUserId)
    {
        throw new NotImplementedException("移除会话参与者功能尚未实现");
    }

    public async Task<bool> SetConversationPinnedAsync(Guid conversationId, bool isPinned, Guid currentUserId)
    {
        throw new NotImplementedException("设置会话置顶功能尚未实现");
    }

    public async Task<bool> SetConversationMutedAsync(Guid conversationId, bool isMuted, Guid currentUserId)
    {
        throw new NotImplementedException("设置会话静音功能尚未实现");
    }

    public async Task<bool> SetConversationArchivedAsync(Guid conversationId, bool isArchived, Guid currentUserId)
    {
        throw new NotImplementedException("归档会话功能尚未实现");
    }

    public async Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId)
    {
        throw new NotImplementedException("标记会话已读功能尚未实现");
    }

    public async Task<MessageStatsDto> GetConversationStatsAsync(Guid conversationId)
    {
        throw new NotImplementedException("获取会话统计功能尚未实现");
    }

    public async Task<bool> LeaveConversationAsync(Guid conversationId, Guid userId)
    {
        throw new NotImplementedException("离开会话功能尚未实现");
    }

    public async Task<IEnumerable<MessageDto>> PreloadUserMessagesAsync(Guid userId, int count = 50)
    {
        var filter = new MessageFilterDto { Page = 1, PageSize = count };
        var result = await GetUserMessagesAsync(userId, filter, userId);
        return result.Items;
    }

    public async Task<IEnumerable<MessageDto>> PreloadConversationMessagesAsync(Guid conversationId, int count = 50)
    {
        var filter = new MessageFilterDto { Page = 1, PageSize = count };
        var result = await GetConversationMessagesAsync(conversationId, filter, Guid.Empty); // 简化实现
        return result.Items;
    }

    public async Task<bool> IsMessageExpiredAsync(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        return message?.ExpiresAt.HasValue == true && message.ExpiresAt.Value <= DateTime.UtcNow;
    }

    public async Task<bool> IsAttachmentExpiredAsync(Guid attachmentId)
    {
        throw new NotImplementedException("检查附件过期功能尚未实现");
    }

    public async Task<bool> IsDraftExpiredAsync(Guid draftId)
    {
        throw new NotImplementedException("检查草稿过期功能尚未实现");
    }

    public async Task<int> ProcessExpiredMessagesAsync()
    {
        throw new NotImplementedException("处理过期消息功能尚未实现");
    }

    public async Task<int> ProcessExpiredAttachmentsAsync()
    {
        throw new NotImplementedException("处理过期附件功能尚未实现");
    }

    public async Task<int> ProcessExpiredDraftsAsync()
    {
        throw new NotImplementedException("处理过期草稿功能尚未实现");
    }

    public async Task<Stream> ExportUserMessagesAsync(Guid userId, MessageExportOptionsDto options, Guid currentUserId)
    {
        throw new NotImplementedException("导出用户消息功能尚未实现");
    }

    public async Task<Stream> ExportConversationMessagesAsync(Guid conversationId, MessageExportOptionsDto options, Guid currentUserId)
    {
        throw new NotImplementedException("导出会话消息功能尚未实现");
    }

    public async Task<Stream> ExportMessageThreadAsync(Guid rootMessageId, MessageExportOptionsDto options, Guid currentUserId)
    {
        throw new NotImplementedException("导出消息线程功能尚未实现");
    }

    // 消息附件管理操作
    public async Task<MessageAttachmentDto> UploadAttachmentAsync(Guid messageId, Stream fileStream, string fileName, string contentType, Guid currentUserId)
    {
        throw new NotImplementedException("上传消息附件功能尚未实现");
    }

    public async Task<IEnumerable<MessageAttachmentDto>> UploadMultipleAttachmentsAsync(Guid messageId, IEnumerable<MessageFileUploadDto> files, Guid currentUserId)
    {
        throw new NotImplementedException("批量上传消息附件功能尚未实现");
    }

    public async Task<Stream> DownloadAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        throw new NotImplementedException("下载消息附件功能尚未实现");
    }

    public async Task<MessageAttachmentDto?> GetAttachmentInfoAsync(Guid attachmentId, Guid currentUserId)
    {
        throw new NotImplementedException("获取附件信息功能尚未实现");
    }

    public async Task<IEnumerable<MessageAttachmentDto>> GetMessageAttachmentsAsync(Guid messageId, Guid currentUserId)
    {
        throw new NotImplementedException("获取消息附件功能尚未实现");
    }

    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        throw new NotImplementedException("删除消息附件功能尚未实现");
    }

    public async Task<string> GetAttachmentDownloadUrlAsync(Guid attachmentId, Guid currentUserId, TimeSpan? expiration = null)
    {
        throw new NotImplementedException("获取附件下载URL功能尚未实现");
    }

    public async Task<bool> ValidateAttachmentIntegrityAsync(Guid attachmentId, string expectedHash)
    {
        throw new NotImplementedException("验证附件完整性功能尚未实现");
    }

    public async Task<MessageAttachmentStatsDto> GetUserAttachmentStatsAsync(Guid userId)
    {
        throw new NotImplementedException("获取用户附件统计功能尚未实现");
    }

    // 消息草稿管理操作
    public async Task<MessageDraftDto> CreateDraftAsync(CreateMessageDraftDto request, Guid authorId)
    {
        throw new NotImplementedException("创建消息草稿功能尚未实现");
    }

    public async Task<MessageDraftDto?> GetDraftAsync(Guid draftId, Guid currentUserId)
    {
        throw new NotImplementedException("获取消息草稿功能尚未实现");
    }

    public async Task<PaginatedResult<MessageDraftDto>> GetUserDraftsAsync(Guid userId, MessageDraftFilterDto filter, Guid currentUserId)
    {
        throw new NotImplementedException("获取用户草稿功能尚未实现");
    }

    public async Task<MessageDraftDto> UpdateDraftAsync(Guid draftId, UpdateMessageDraftDto request, Guid currentUserId)
    {
        throw new NotImplementedException("更新消息草稿功能尚未实现");
    }

    public async Task<bool> AutoSaveDraftAsync(Guid draftId, string content, Guid currentUserId)
    {
        throw new NotImplementedException("自动保存草稿功能尚未实现");
    }

    public async Task<MessageDto> SendDraftAsync(Guid draftId, Guid currentUserId)
    {
        throw new NotImplementedException("发送草稿功能尚未实现");
    }

    public async Task<bool> DeleteDraftAsync(Guid draftId, Guid currentUserId)
    {
        throw new NotImplementedException("删除消息草稿功能尚未实现");
    }

    public async Task<int> BulkDeleteDraftsAsync(IEnumerable<Guid> draftIds, Guid currentUserId)
    {
        throw new NotImplementedException("批量删除草稿功能尚未实现");
    }

    public async Task<bool> ScheduleDraftSendAsync(Guid draftId, DateTime scheduledTime, Guid currentUserId)
    {
        throw new NotImplementedException("设置定时发送功能尚未实现");
    }

    public async Task<bool> CancelScheduledSendAsync(Guid draftId, Guid currentUserId)
    {
        throw new NotImplementedException("取消定时发送功能尚未实现");
    }

    public async Task<IEnumerable<MessageDraftDto>> GetScheduledDraftsAsync(Guid userId, Guid currentUserId)
    {
        throw new NotImplementedException("获取定时发送草稿功能尚未实现");
    }

    public async Task<MessageDraftAttachmentDto> UploadDraftAttachmentAsync(Guid draftId, Stream fileStream, string fileName, string contentType, Guid currentUserId)
    {
        throw new NotImplementedException("上传草稿附件功能尚未实现");
    }

    public async Task<bool> DeleteDraftAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        throw new NotImplementedException("删除草稿附件功能尚未实现");
    }

    public async Task<int> CleanupExpiredDraftsAsync(Guid userId)
    {
        throw new NotImplementedException("清理过期草稿功能尚未实现");
    }

    public async Task<bool> ValidateConversationPermissionAsync(Guid conversationId, Guid userId, ConversationPermission permission)
    {
        throw new NotImplementedException("验证会话权限功能尚未实现");
    }

    // 批量操作方法实现
    public async Task<BulkOperationResultDto> BulkSendMessagesAsync(IEnumerable<CreateMessageDto> requests, Guid senderId)
    {
        throw new NotImplementedException("批量发送消息功能尚未实现");
    }

    public async Task<BulkOperationResultDto> BulkMarkMessagesAsReadAsync(IEnumerable<Guid> messageIds, Guid userId)
    {
        throw new NotImplementedException("批量标记消息为已读功能尚未实现");
    }

    public async Task<BulkOperationResultDto> BulkDeleteMessagesAsync(IEnumerable<Guid> messageIds, Guid userId)
    {
        throw new NotImplementedException("批量删除消息功能尚未实现");
    }

    // 会话管理方法实现
    public async Task<bool> RemoveConversationParticipantAsync(Guid conversationId, Guid participantId, Guid operatorId)
    {
        throw new NotImplementedException("移除会话参与者功能尚未实现");
    }

    // 消息类型和优先级管理方法实现
    public async Task<IEnumerable<MessageTypeDto>> GetMessageTypesAsync()
    {
        throw new NotImplementedException("获取消息类型功能尚未实现");
    }

    public async Task<IEnumerable<MessagePriorityDto>> GetMessagePrioritiesAsync()
    {
        throw new NotImplementedException("获取消息优先级功能尚未实现");
    }

    // 草稿管理方法实现
    public async Task<bool> ScheduleDraftAsync(Guid draftId, DateTime scheduledTime, Guid userId)
    {
        throw new NotImplementedException("安排草稿定时发送功能尚未实现");
    }

    #endregion
}