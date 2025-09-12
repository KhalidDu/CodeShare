using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 消息草稿服务实现 - 遵循单一职责原则
/// 提供完整的消息草稿创建、编辑、自动保存、定时发送和管理功能
/// </summary>
public class MessageDraftService
{
    private readonly IMessageDraftRepository _messageDraftRepository;
    private readonly IMessageDraftAttachmentRepository _messageDraftAttachmentRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICacheService _cacheService;
    private readonly IPermissionService _permissionService;
    private readonly IInputValidationService _inputValidationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MessageDraftService> _logger;

    // 配置常量
    private const int MAX_DRAFTS_PER_USER = 50;
    private const int MAX_DRAFT_AGE_DAYS = 30;
    private const int AUTO_SAVE_INTERVAL_MIN = 30;
    private const int AUTO_SAVE_INTERVAL_MAX = 3600;
    private const string DRAFT_CACHE_PREFIX = "draft_";
    private const long MAX_DRAFT_ATTACHMENT_SIZE = 10 * 1024 * 1024; // 10MB
    private const int MAX_DRAFT_ATTACHMENTS = 5;

    public MessageDraftService(
        IMessageDraftRepository messageDraftRepository,
        IMessageDraftAttachmentRepository messageDraftAttachmentRepository,
        IMessageRepository messageRepository,
        ICacheService cacheService,
        IPermissionService permissionService,
        IInputValidationService inputValidationService,
        IFileStorageService fileStorageService,
        ILogger<MessageDraftService> logger)
    {
        _messageDraftRepository = messageDraftRepository ?? throw new ArgumentNullException(nameof(messageDraftRepository));
        _messageDraftAttachmentRepository = messageDraftAttachmentRepository ?? throw new ArgumentNullException(nameof(messageDraftAttachmentRepository));
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 草稿管理操作

    /// <summary>
    /// 创建消息草稿
    /// </summary>
    public async Task<MessageDraftDto> CreateDraftAsync(CreateMessageDraftDto request, Guid authorId)
    {
        try
        {
            _logger.LogInformation("用户 {AuthorId} 尝试创建消息草稿", authorId);

            // 验证输入数据
            await ValidateDraftInputAsync(request);

            // 检查用户草稿数量限制
            var userDraftCount = await _messageDraftRepository.GetUserDraftCountAsync(authorId);
            if (userDraftCount >= MAX_DRAFTS_PER_USER)
            {
                throw new InvalidOperationException($"您已达到草稿数量上限（{MAX_DRAFTS_PER_USER}个）");
            }

            // 清理过期草稿
            await CleanupExpiredDraftsAsync(authorId);

            // 创建草稿实体
            var draft = new MessageDraft
            {
                Id = Guid.NewGuid(),
                AuthorId = authorId,
                ReceiverId = request.ReceiverId,
                Subject = request.Subject,
                Content = request.Content,
                MessageType = request.MessageType,
                Priority = request.Priority,
                ParentId = request.ParentId,
                ConversationId = request.ConversationId,
                Tag = request.Tag,
                Status = DraftStatus.Draft,
                ScheduledToSendAt = request.ScheduledToSendAt,
                ExpiresAt = request.ExpiresAt,
                IsScheduled = request.IsScheduled,
                AutoSaveInterval = request.AutoSaveInterval,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 保存草稿
            var createdDraft = await _messageDraftRepository.CreateAsync(draft);

            _logger.LogInformation("用户 {AuthorId} 成功创建消息草稿 {DraftId}", authorId, createdDraft.Id);

            return await MapToDraftDtoAsync(createdDraft, authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建消息草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取消息草稿
    /// </summary>
    public async Task<MessageDraftDto?> GetDraftAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取消息草稿: {DraftId}", draftId);

            // 验证草稿查看权限
            var canView = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canView)
            {
                _logger.LogWarning("用户 {UserId} 无权限查看草稿 {DraftId}", currentUserId, draftId);
                throw new UnauthorizedAccessException("您没有权限查看此草稿");
            }

            // 尝试从缓存获取
            var cacheKey = $"{DRAFT_CACHE_PREFIX}{draftId}";
            var cachedDraft = await _cacheService.GetAsync<MessageDraftDto>(cacheKey);
            if (cachedDraft != null)
            {
                return cachedDraft;
            }

            // 从数据库获取
            var draft = await _messageDraftRepository.GetByIdWithDetailsAsync(draftId);
            if (draft == null)
            {
                _logger.LogWarning("草稿 {DraftId} 不存在", draftId);
                return null;
            }

            // 检查草稿是否过期
            if (await IsDraftExpiredAsync(draft))
            {
                await _messageDraftRepository.SoftDeleteAsync(draftId);
                return null;
            }

            // 映射为DTO
            var draftDto = await MapToDraftDtoAsync(draft, currentUserId);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, draftDto, TimeSpan.FromMinutes(5));

            return draftDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的草稿列表
    /// </summary>
    public async Task<PaginatedResult<MessageDraftDto>> GetUserDraftsAsync(Guid userId, MessageDraftFilterDto filter, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的草稿列表", userId);

            // 验证权限：只能查看自己的草稿
            if (userId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的草稿");
            }

            // 获取草稿列表
            var result = await _messageDraftRepository.GetUserDraftsAsync(userId, filter);

            // 过滤过期草稿
            var validDrafts = new List<MessageDraft>();
            foreach (var draft in result.Items)
            {
                if (!await IsDraftExpiredAsync(draft))
                {
                    validDrafts.Add(draft);
                }
            }

            // 映射为DTO
            var draftDtos = new List<MessageDraftDto>();
            foreach (var draft in validDrafts)
            {
                draftDtos.Add(await MapToDraftDtoAsync(draft, currentUserId));
            }

            return new PaginatedResult<MessageDraftDto>
            {
                Items = draftDtos,
                TotalCount = draftDtos.Count,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户草稿列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 更新消息草稿
    /// </summary>
    public async Task<MessageDraftDto> UpdateDraftAsync(Guid draftId, UpdateMessageDraftDto request, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试更新草稿 {DraftId}", currentUserId, draftId);

            // 验证编辑权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限编辑草稿 {DraftId}", currentUserId, draftId);
                throw new UnauthorizedAccessException("您没有权限编辑此草稿");
            }

            // 获取现有草稿
            var existingDraft = await _messageDraftRepository.GetByIdAsync(draftId);
            if (existingDraft == null)
            {
                _logger.LogWarning("草稿 {DraftId} 不存在", draftId);
                throw new ArgumentException("草稿不存在");
            }

            // 检查草稿是否过期
            if (await IsDraftExpiredAsync(existingDraft))
            {
                throw new InvalidOperationException("草稿已过期");
            }

            // 检查草稿状态
            if (existingDraft.Status != DraftStatus.Draft)
            {
                throw new InvalidOperationException("此草稿不能编辑");
            }

            // 验证输入数据
            await ValidateDraftUpdateInputAsync(request);

            // 更新草稿内容
            existingDraft.ReceiverId = request.ReceiverId ?? existingDraft.ReceiverId;
            existingDraft.Subject = request.Subject ?? existingDraft.Subject;
            existingDraft.Content = request.Content ?? existingDraft.Content;
            existingDraft.Priority = request.Priority ?? existingDraft.Priority;
            existingDraft.ParentId = request.ParentId ?? existingDraft.ParentId;
            existingDraft.ConversationId = request.ConversationId ?? existingDraft.ConversationId;
            existingDraft.Tag = request.Tag ?? existingDraft.Tag;
            existingDraft.ScheduledToSendAt = request.ScheduledToSendAt ?? existingDraft.ScheduledToSendAt;
            existingDraft.ExpiresAt = request.ExpiresAt ?? existingDraft.ExpiresAt;
            existingDraft.IsScheduled = request.IsScheduled ?? existingDraft.IsScheduled;
            existingDraft.AutoSaveInterval = request.AutoSaveInterval ?? existingDraft.AutoSaveInterval;
            existingDraft.UpdatedAt = DateTime.UtcNow;

            // 保存更新
            var updatedDraft = await _messageDraftRepository.UpdateAsync(existingDraft);

            // 清除缓存
            await ClearDraftCacheAsync(draftId);

            _logger.LogInformation("用户 {UserId} 成功更新草稿 {DraftId}", currentUserId, draftId);

            return await MapToDraftDtoAsync(updatedDraft, currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 自动保存草稿
    /// </summary>
    public async Task<bool> AutoSaveDraftAsync(Guid draftId, string content, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("用户 {UserId} 自动保存草稿 {DraftId}", currentUserId, draftId);

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                return false;
            }

            // 获取草稿
            var draft = await _messageDraftRepository.GetByIdAsync(draftId);
            if (draft == null || await IsDraftExpiredAsync(draft))
            {
                return false;
            }

            // 检查是否需要自动保存（内容有变化且达到时间间隔）
            var timeSinceLastAutoSave = DateTime.UtcNow - (draft.LastAutoSavedAt ?? draft.CreatedAt);
            if (timeSinceLastAutoSave.TotalSeconds < draft.AutoSaveInterval || 
                draft.Content == content)
            {
                return true; // 无需保存
            }

            // 验证内容
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            if (content.Length > 2000)
            {
                return false;
            }

            // 更新草稿
            draft.Content = content;
            draft.LastAutoSavedAt = DateTime.UtcNow;
            draft.UpdatedAt = DateTime.UtcNow;

            var result = await _messageDraftRepository.UpdateAsync(draft);

            if (result)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draftId);
                _logger.LogDebug("用户 {UserId} 成功自动保存草稿 {DraftId}", currentUserId, draftId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动保存草稿时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 发送草稿
    /// </summary>
    public async Task<MessageDto> SendDraftAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试发送草稿 {DraftId}", currentUserId, draftId);

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限发送草稿 {DraftId}", currentUserId, draftId);
                throw new UnauthorizedAccessException("您没有权限发送此草稿");
            }

            // 获取草稿详情
            var draft = await _messageDraftRepository.GetByIdWithDetailsAsync(draftId);
            if (draft == null)
            {
                _logger.LogWarning("草稿 {DraftId} 不存在", draftId);
                throw new ArgumentException("草稿不存在");
            }

            // 检查草稿状态
            if (draft.Status != DraftStatus.Draft)
            {
                throw new InvalidOperationException("此草稿不能发送");
            }

            // 检查是否过期
            if (await IsDraftExpiredAsync(draft))
            {
                throw new InvalidOperationException("草稿已过期");
            }

            // 验证接收者
            if (draft.ReceiverId == null || draft.ReceiverId == Guid.Empty)
            {
                throw new InvalidOperationException("请指定接收者");
            }

            // 验证内容
            if (string.IsNullOrWhiteSpace(draft.Content))
            {
                throw new InvalidOperationException("消息内容不能为空");
            }

            // 创建消息请求
            var messageRequest = new CreateMessageDto
            {
                ReceiverId = draft.ReceiverId.Value,
                Subject = draft.Subject,
                Content = draft.Content,
                MessageType = draft.MessageType,
                Priority = draft.Priority,
                ParentId = draft.ParentId,
                ConversationId = draft.ConversationId,
                Tag = draft.Tag
            };

            // 发送消息（这里需要注入MessageService，但为了避免循环依赖，简化实现）
            // 在实际项目中，可以使用中介者模式或工厂模式解决依赖问题
            var message = await SendDraftMessageAsync(messageRequest, currentUserId);

            // 如果有草稿附件，转移到消息附件
            var draftAttachments = await _messageDraftAttachmentRepository.GetDraftAttachmentsAsync(draftId);
            foreach (var draftAttachment in draftAttachments)
            {
                await TransferDraftAttachmentToMessageAsync(draftAttachment.Id, message.Id);
            }

            // 更新草稿状态为已发送
            draft.Status = DraftStatus.Sent;
            draft.UpdatedAt = DateTime.UtcNow;
            await _messageDraftRepository.UpdateAsync(draft);

            // 清除缓存
            await ClearDraftCacheAsync(draftId);

            _logger.LogInformation("用户 {UserId} 成功发送草稿 {DraftId}，消息ID: {MessageId}", currentUserId, draftId, message.Id);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 删除消息草稿
    /// </summary>
    public async Task<bool> DeleteDraftAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试删除草稿 {DraftId}", currentUserId, draftId);

            // 验证删除权限
            var canDelete = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canDelete)
            {
                _logger.LogWarning("用户 {UserId} 无权限删除草稿 {DraftId}", currentUserId, draftId);
                throw new UnauthorizedAccessException("您没有权限删除此草稿");
            }

            // 获取草稿附件
            var draftAttachments = await _messageDraftAttachmentRepository.GetDraftAttachmentsAsync(draftId);

            // 删除附件文件
            foreach (var attachment in draftAttachments)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(attachment.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除草稿附件文件失败: {FilePath}", attachment.FilePath);
                }
            }

            // 软删除草稿
            var result = await _messageDraftRepository.SoftDeleteAsync(draftId);

            if (result)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draftId);
                _logger.LogInformation("用户 {UserId} 成功删除草稿 {DraftId}", currentUserId, draftId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 批量删除草稿
    /// </summary>
    public async Task<int> BulkDeleteDraftsAsync(IEnumerable<Guid> draftIds, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试批量删除 {Count} 个草稿", currentUserId, draftIds.Count());

            if (!draftIds.Any())
            {
                return 0;
            }

            var successCount = 0;
            foreach (var draftId in draftIds)
            {
                try
                {
                    if (await DeleteDraftAsync(draftId, currentUserId))
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "批量删除草稿时单个草稿删除失败: {DraftId}", draftId);
                    // 继续处理其他草稿
                }
            }

            _logger.LogInformation("用户 {UserId} 成功批量删除 {SuccessCount}/{TotalCount} 个草稿", 
                currentUserId, successCount, draftIds.Count());

            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除草稿时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 设置定时发送
    /// </summary>
    public async Task<bool> ScheduleDraftSendAsync(Guid draftId, DateTime scheduledTime, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试设置草稿 {DraftId} 定时发送", currentUserId, draftId);

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                return false;
            }

            // 获取草稿
            var draft = await _messageDraftRepository.GetByIdAsync(draftId);
            if (draft == null || await IsDraftExpiredAsync(draft))
            {
                return false;
            }

            // 验证定时时间
            if (scheduledTime <= DateTime.UtcNow)
            {
                throw new ArgumentException("定时发送时间必须晚于当前时间");
            }

            if (scheduledTime > DateTime.UtcNow.AddDays(30))
            {
                throw new ArgumentException("定时发送时间不能超过30天");
            }

            // 更新草稿
            draft.ScheduledToSendAt = scheduledTime;
            draft.IsScheduled = true;
            draft.UpdatedAt = DateTime.UtcNow;

            var result = await _messageDraftRepository.UpdateAsync(draft);

            if (result)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draftId);
                _logger.LogInformation("用户 {UserId} 成功设置草稿 {DraftId} 定时发送时间为 {ScheduledTime}", 
                    currentUserId, draftId, scheduledTime);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置定时发送时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 取消定时发送
    /// </summary>
    public async Task<bool> CancelScheduledSendAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试取消草稿 {DraftId} 的定时发送", currentUserId, draftId);

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                return false;
            }

            // 获取草稿
            var draft = await _messageDraftRepository.GetByIdAsync(draftId);
            if (draft == null)
            {
                return false;
            }

            // 更新草稿
            draft.ScheduledToSendAt = null;
            draft.IsScheduled = false;
            draft.UpdatedAt = DateTime.UtcNow;

            var result = await _messageDraftRepository.UpdateAsync(draft);

            if (result)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draftId);
                _logger.LogInformation("用户 {UserId} 成功取消草稿 {DraftId} 的定时发送", currentUserId, draftId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消定时发送时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 获取定时发送的草稿列表
    /// </summary>
    public async Task<IEnumerable<MessageDraftDto>> GetScheduledDraftsAsync(Guid userId, Guid currentUserId)
    {
        try
        {
            _logger.LogDebug("获取用户 {UserId} 的定时发送草稿列表", userId);

            // 验证权限
            if (userId != currentUserId && !await _permissionService.IsAdminAsync(currentUserId))
            {
                throw new UnauthorizedAccessException("您只能查看自己的定时发送草稿");
            }

            // 获取定时发送的草稿
            var scheduledDrafts = await _messageDraftRepository.GetScheduledDraftsAsync(userId);

            // 过滤有效草稿并映射为DTO
            var draftDtos = new List<MessageDraftDto>();
            foreach (var draft in scheduledDrafts)
            {
                if (!await IsDraftExpiredAsync(draft))
                {
                    draftDtos.Add(await MapToDraftDtoAsync(draft, currentUserId));
                }
            }

            return draftDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取定时发送草稿列表时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 上传草稿附件
    /// </summary>
    public async Task<MessageDraftAttachmentDto> UploadDraftAttachmentAsync(Guid draftId, Stream fileStream, string fileName, string contentType, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试为草稿 {DraftId} 上传附件: {FileName}", currentUserId, draftId, fileName);

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(draftId, currentUserId);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限为草稿 {DraftId} 上传附件", currentUserId, draftId);
                throw new UnauthorizedAccessException("您没有权限为此草稿上传附件");
            }

            // 获取草稿
            var draft = await _messageDraftRepository.GetByIdAsync(draftId);
            if (draft == null || await IsDraftExpiredAsync(draft))
            {
                throw new ArgumentException("草稿不存在或已过期");
            }

            // 验证文件
            await ValidateDraftAttachmentFileAsync(fileStream, fileName, contentType);

            // 检查附件数量限制
            var existingAttachments = await _messageDraftAttachmentRepository.GetDraftAttachmentsAsync(draftId);
            if (existingAttachments.Count() >= MAX_DRAFT_ATTACHMENTS)
            {
                throw new InvalidOperationException($"每条草稿最多只能上传{MAX_DRAFT_ATTACHMENTS}个附件");
            }

            // 生成唯一文件名
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            // 上传文件到存储
            var storagePath = await _fileStorageService.UploadFileAsync(fileStream, uniqueFileName, "draft-attachments");

            // 确定附件类型
            var attachmentType = DetermineDraftAttachmentType(fileExtension);

            // 创建附件记录
            var attachment = new MessageDraftAttachment
            {
                Id = Guid.NewGuid(),
                DraftId = draftId,
                FileName = uniqueFileName,
                OriginalFileName = fileName,
                FileSize = fileStream.Length,
                ContentType = contentType,
                FileExtension = fileExtension,
                FilePath = storagePath,
                FileUrl = await _fileStorageService.GetFileUrlAsync(storagePath),
                AttachmentType = attachmentType,
                AttachmentStatus = AttachmentStatus.Active,
                UploadedAt = DateTime.UtcNow,
                UploadProgress = 100
            };

            // 保存附件记录
            var createdAttachment = await _messageDraftAttachmentRepository.CreateAsync(attachment);

            _logger.LogInformation("用户 {UserId} 成功为草稿 {DraftId} 上传附件: {FileName}", currentUserId, draftId, fileName);

            return await MapToDraftAttachmentDtoAsync(createdAttachment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传草稿附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 删除草稿附件
    /// </summary>
    public async Task<bool> DeleteDraftAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 尝试删除草稿附件 {AttachmentId}", currentUserId, attachmentId);

            // 获取附件信息
            var attachment = await _messageDraftAttachmentRepository.GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                _logger.LogWarning("草稿附件 {AttachmentId} 不存在", attachmentId);
                return false;
            }

            // 验证权限
            var canEdit = await ValidateDraftPermissionAsync(attachment.DraftId, currentUserId);
            if (!canEdit)
            {
                _logger.LogWarning("用户 {UserId} 无权限删除草稿附件 {AttachmentId}", currentUserId, attachmentId);
                throw new UnauthorizedAccessException("您没有权限删除此附件");
            }

            // 删除存储文件
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // 删除附件记录
            var result = await _messageDraftAttachmentRepository.DeleteAsync(attachmentId);

            if (result)
            {
                _logger.LogInformation("用户 {UserId} 成功删除草稿附件 {AttachmentId}: {FileName}", 
                    currentUserId, attachmentId, attachment.OriginalFileName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除草稿附件时发生错误: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 清理过期草稿
    /// </summary>
    public async Task<int> CleanupExpiredDraftsAsync(Guid userId)
    {
        try
        {
            _logger.LogDebug("清理用户 {UserId} 的过期草稿", userId);

            var expiredDrafts = await _messageDraftRepository.GetExpiredDraftsAsync(userId, MAX_DRAFT_AGE_DAYS);
            var cleanupCount = 0;

            foreach (var draft in expiredDrafts)
            {
                try
                {
                    await _messageDraftRepository.SoftDeleteAsync(draft.Id);
                    cleanupCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理过期草稿失败: {DraftId}", draft.Id);
                }
            }

            if (cleanupCount > 0)
            {
                _logger.LogInformation("清理用户 {UserId} 的过期草稿完成: 清理了 {Count} 个草稿", userId, cleanupCount);
            }

            return cleanupCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期草稿时发生错误: {Message}", ex.Message);
            return 0;
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 验证草稿输入数据
    /// </summary>
    private async Task ValidateDraftInputAsync(CreateMessageDraftDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Content != null && request.Content.Length > 2000)
        {
            throw new ArgumentException("草稿内容长度不能超过2000个字符");
        }

        if (request.Subject != null && request.Subject.Length > 200)
        {
            throw new ArgumentException("草稿主题长度不能超过200个字符");
        }

        if (request.Tag != null && request.Tag.Length > 100)
        {
            throw new ArgumentException("草稿标签长度不能超过100个字符");
        }

        if (request.AutoSaveInterval < AUTO_SAVE_INTERVAL_MIN || request.AutoSaveInterval > AUTO_SAVE_INTERVAL_MAX)
        {
            throw new ArgumentException($"自动保存间隔必须在{AUTO_SAVE_INTERVAL_MIN}-{AUTO_SAVE_INTERVAL_MAX}秒之间");
        }

        // 验证定时发送时间
        if (request.ScheduledToSendAt.HasValue)
        {
            if (request.ScheduledToSendAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("定时发送时间必须晚于当前时间");
            }

            if (request.ScheduledToSendAt.Value > DateTime.UtcNow.AddDays(30))
            {
                throw new ArgumentException("定时发送时间不能超过30天");
            }
        }

        // 验证过期时间
        if (request.ExpiresAt.HasValue)
        {
            if (request.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("过期时间必须晚于当前时间");
            }
        }

        // 使用输入验证服务验证内容
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            await _inputValidationService.ValidateMessageContentAsync(request.Content);
        }

        if (!string.IsNullOrWhiteSpace(request.Subject))
        {
            await _inputValidationService.ValidateMessageSubjectAsync(request.Subject);
        }
    }

    /// <summary>
    /// 验证草稿更新输入数据
    /// </summary>
    private async Task ValidateDraftUpdateInputAsync(UpdateMessageDraftDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Content != null && request.Content.Length > 2000)
        {
            throw new ArgumentException("草稿内容长度不能超过2000个字符");
        }

        if (request.Subject != null && request.Subject.Length > 200)
        {
            throw new ArgumentException("草稿主题长度不能超过200个字符");
        }

        if (request.Tag != null && request.Tag.Length > 100)
        {
            throw new ArgumentException("草稿标签长度不能超过100个字符");
        }

        if (request.AutoSaveInterval.HasValue && (request.AutoSaveInterval.Value < AUTO_SAVE_INTERVAL_MIN || request.AutoSaveInterval.Value > AUTO_SAVE_INTERVAL_MAX))
        {
            throw new ArgumentException($"自动保存间隔必须在{AUTO_SAVE_INTERVAL_MIN}-{AUTO_SAVE_INTERVAL_MAX}秒之间");
        }

        // 验证定时发送时间
        if (request.ScheduledToSendAt.HasValue)
        {
            if (request.ScheduledToSendAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("定时发送时间必须晚于当前时间");
            }

            if (request.ScheduledToSendAt.Value > DateTime.UtcNow.AddDays(30))
            {
                throw new ArgumentException("定时发送时间不能超过30天");
            }
        }

        // 验证过期时间
        if (request.ExpiresAt.HasValue)
        {
            if (request.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("过期时间必须晚于当前时间");
            }
        }

        // 使用输入验证服务验证内容
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            await _inputValidationService.ValidateMessageContentAsync(request.Content);
        }

        if (!string.IsNullOrWhiteSpace(request.Subject))
        {
            await _inputValidationService.ValidateMessageSubjectAsync(request.Subject);
        }
    }

    /// <summary>
    /// 验证草稿附件文件
    /// </summary>
    private async Task ValidateDraftAttachmentFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("文件不能为空");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("文件名不能为空");
        }

        if (fileName.Length > 255)
        {
            throw new ArgumentException("文件名长度不能超过255个字符");
        }

        // 验证文件大小
        if (fileStream.Length > MAX_DRAFT_ATTACHMENT_SIZE)
        {
            throw new ArgumentException($"草稿附件大小不能超过{MAX_DRAFT_ATTACHMENT_SIZE / (1024 * 1024)}MB");
        }

        // 使用输入验证服务验证文件名
        await _inputValidationService.ValidateFileNameAsync(fileName);
    }

    /// <summary>
    /// 验证草稿权限
    /// </summary>
    private async Task<bool> ValidateDraftPermissionAsync(Guid draftId, Guid userId)
    {
        var draft = await _messageDraftRepository.GetByIdAsync(draftId);
        return draft != null && draft.AuthorId == userId;
    }

    /// <summary>
    /// 检查草稿是否过期
    /// </summary>
    private async Task<bool> IsDraftExpiredAsync(MessageDraft draft)
    {
        // 检查显式过期时间
        if (draft.ExpiresAt.HasValue && draft.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return true;
        }

        // 检查最大年龄
        if (draft.CreatedAt.AddDays(MAX_DRAFT_AGE_DAYS) <= DateTime.UtcNow)
        {
            return true;
        }

        // 检查状态
        if (draft.Status == DraftStatus.Expired || draft.Status == DraftStatus.Cancelled)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查草稿是否过期（通过ID）
    /// </summary>
    private async Task<bool> IsDraftExpiredAsync(Guid draftId)
    {
        var draft = await _messageDraftRepository.GetByIdAsync(draftId);
        return draft == null || await IsDraftExpiredAsync(draft);
    }

    /// <summary>
    /// 发送草稿消息（简化实现）
    /// </summary>
    private async Task<MessageDto> SendDraftMessageAsync(CreateMessageDto request, Guid senderId)
    {
        // 这里应该调用MessageService.SendMessageAsync
        // 为了避免循环依赖，创建一个简化的消息实体
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
            Status = MessageStatus.Sent,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 保存消息
        var createdMessage = await _messageRepository.CreateAsync(message);

        // 映射为DTO（简化实现）
        return new MessageDto
        {
            Id = createdMessage.Id,
            SenderId = createdMessage.SenderId,
            ReceiverId = createdMessage.ReceiverId,
            Subject = createdMessage.Subject,
            Content = createdMessage.Content,
            MessageType = createdMessage.MessageType,
            Status = createdMessage.Status,
            Priority = createdMessage.Priority,
            ParentId = createdMessage.ParentId,
            ConversationId = createdMessage.ConversationId,
            IsRead = createdMessage.IsRead,
            CreatedAt = createdMessage.CreatedAt,
            UpdatedAt = createdMessage.UpdatedAt,
            IsSender = true,
            IsReceiver = false,
            CanEdit = true,
            CanDelete = true,
            CanReply = true,
            CanForward = true
        };
    }

    /// <summary>
    /// 转移草稿附件到消息附件
    /// </summary>
    private async Task TransferDraftAttachmentToMessageAsync(Guid draftAttachmentId, Guid messageId)
    {
        var draftAttachment = await _messageDraftAttachmentRepository.GetByIdAsync(draftAttachmentId);
        if (draftAttachment == null)
        {
            return;
        }

        // 创建消息附件记录
        var messageAttachment = new MessageAttachment
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            FileName = draftAttachment.FileName,
            OriginalFileName = draftAttachment.OriginalFileName,
            FileSize = draftAttachment.FileSize,
            ContentType = draftAttachment.ContentType,
            FileExtension = draftAttachment.FileExtension,
            FilePath = draftAttachment.FilePath,
            FileUrl = draftAttachment.FileUrl,
            AttachmentType = draftAttachment.AttachmentType,
            AttachmentStatus = AttachmentStatus.Active,
            UploadedAt = draftAttachment.UploadedAt
        };

        // 保存消息附件
        await _messageAttachmentRepository.CreateAsync(messageAttachment);

        // 删除草稿附件记录
        await _messageDraftAttachmentRepository.DeleteAsync(draftAttachmentId);
    }

    /// <summary>
    /// 生成唯一文件名
    /// </summary>
    private string GenerateUniqueFileName(string originalFileName)
    {
        var fileExtension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{random}{fileExtension}";
    }

    /// <summary>
    /// 确定草稿附件类型
    /// </summary>
    private AttachmentType DetermineDraftAttachmentType(string fileExtension)
    {
        return fileExtension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".svg" => AttachmentType.Image,
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" => AttachmentType.Document,
            ".mp3" or ".wav" or ".flac" or ".m4a" => AttachmentType.Audio,
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" => AttachmentType.Video,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => AttachmentType.Archive,
            ".cs" or ".js" or ".ts" or ".html" or ".css" or ".py" or ".java" or ".cpp" or ".php" => AttachmentType.Code,
            _ => AttachmentType.Other
        };
    }

    /// <summary>
    /// 映射草稿实体到DTO
    /// </summary>
    private async Task<MessageDraftDto> MapToDraftDtoAsync(MessageDraft draft, Guid currentUserId)
    {
        // 获取草稿附件
        var draftAttachments = await _messageDraftAttachmentRepository.GetDraftAttachmentsAsync(draft.Id);
        var attachmentDtos = new List<MessageDraftAttachmentDto>();
        
        foreach (var attachment in draftAttachments)
        {
            attachmentDtos.Add(await MapToDraftAttachmentDtoAsync(attachment));
        }

        return new MessageDraftDto
        {
            Id = draft.Id,
            AuthorId = draft.AuthorId,
            ReceiverId = draft.ReceiverId,
            Subject = draft.Subject,
            Content = draft.Content,
            MessageType = draft.MessageType,
            Priority = draft.Priority,
            ParentId = draft.ParentId,
            ConversationId = draft.ConversationId,
            Tag = draft.Tag,
            Status = draft.Status,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt,
            ScheduledToSendAt = draft.ScheduledToSendAt,
            ExpiresAt = draft.ExpiresAt,
            IsScheduled = draft.IsScheduled,
            DraftAttachments = attachmentDtos
        };
    }

    /// <summary>
    /// 映射草稿附件实体到DTO
    /// </summary>
    private async Task<MessageDraftAttachmentDto> MapToDraftAttachmentDtoAsync(MessageDraftAttachment attachment)
    {
        return new MessageDraftAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            FileExtension = attachment.FileExtension,
            FileUrl = attachment.FileUrl,
            AttachmentType = attachment.AttachmentType,
            AttachmentStatus = attachment.AttachmentStatus,
            UploadProgress = attachment.UploadProgress,
            UploadedAt = attachment.UploadedAt
        };
    }

    /// <summary>
    /// 清除草稿缓存
    /// </summary>
    private async Task<bool> ClearDraftCacheAsync(Guid draftId)
    {
        try
        {
            var cacheKey = $"{DRAFT_CACHE_PREFIX}{draftId}";
            var result = await _cacheService.RemoveAsync(cacheKey);
            
            if (result)
            {
                _logger.LogDebug("成功清除草稿缓存: {DraftId}", draftId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除草稿缓存时发生错误: {Message}", ex.Message);
            return false;
        }
    }

    #endregion
}