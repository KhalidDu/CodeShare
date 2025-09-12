using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 消息草稿仓库实现 - 遵循单一职责原则和依赖倒置原则
/// 提供完整的消息草稿CRUD操作、查询功能、自动保存管理和定时发送功能
/// </summary>
public class MessageDraftRepository : IMessageDraftRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<MessageDraftRepository> _logger;
    private readonly IMemoryCache _cache;
    private readonly IInputValidationService _inputValidationService;

    // 缓存键常量
    private const string DRAFT_CACHE_PREFIX = "message_draft_";
    private const string USER_DRAFTS_CACHE_PREFIX = "user_drafts_";
    private const string CONVERSATION_DRAFTS_CACHE_PREFIX = "conversation_drafts_";
    private const string DRAFT_STATS_CACHE_PREFIX = "draft_stats_";

    public MessageDraftRepository(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<MessageDraftRepository> logger,
        IMemoryCache cache,
        IInputValidationService inputValidationService)
    {
        _dbConnectionFactory = dbConnectionFactory ?? throw new ArgumentNullException(nameof(dbConnectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _inputValidationService = inputValidationService ?? throw new ArgumentNullException(nameof(inputValidationService));
    }

    #region 基础CRUD操作

    /// <summary>
    /// 根据ID获取消息草稿
    /// </summary>
    public async Task<MessageDraft?> GetByIdAsync(Guid id, Guid currentUserId)
    {
        try
        {
            // 检查缓存
            var cacheKey = $"{DRAFT_CACHE_PREFIX}{id}";
            if (_cache.TryGetValue(cacheKey, out MessageDraft? cachedDraft))
            {
                // 验证用户权限
                if (cachedDraft != null && await CanUserAccessDraftAsync(id, currentUserId))
                {
                    return cachedDraft;
                }
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT md.*, 
                       u.Id, u.UserName, u.Email, u.ProfilePicture, u.FirstName, u.LastName, u.Bio, u.Location,
                       u2.Id, u2.UserName, u2.Email, u2.ProfilePicture, u2.FirstName, u2.LastName, u2.Bio, u2.Location,
                       m.Id, m.SenderId, m.ReceiverId, m.Subject, m.Content, m.MessageType, m.Status, m.Priority,
                       m.ParentId, m.ConversationId, m.IsRead, m.ReadAt, m.Tag, m.CreatedAt, m.UpdatedAt, m.DeletedAt,
                       m.SenderDeletedAt, m.ReceiverDeletedAt, m.ExpiresAt, m.SenderIp, m.SenderUserAgent
                FROM MessageDrafts md
                LEFT JOIN Users u ON md.AuthorId = u.Id
                LEFT JOIN Users u2 ON md.ReceiverId = u2.Id
                LEFT JOIN Messages m ON md.ParentId = m.Id
                WHERE md.Id = @Id AND md.DeletedAt IS NULL
                ORDER BY md.CreatedAt DESC";

            var result = await connection.QueryAsync<MessageDraft, User, User, Message, MessageDraft>(
                sql,
                (draft, author, receiver, parent) =>
                {
                    draft.Author = author;
                    draft.Receiver = receiver;
                    draft.Parent = parent;
                    return draft;
                },
                new { Id = id },
                splitOn: "Id,Id,Id");

            var draft = result.FirstOrDefault();

            if (draft != null)
            {
                // 验证用户权限
                if (!await CanUserAccessDraftAsync(id, currentUserId))
                {
                    _logger.LogWarning("用户 {UserId} 尝试访问无权限的草稿 {DraftId}", currentUserId, id);
                    return null;
                }

                // 获取草稿附件
                draft.DraftAttachments = (await GetDraftAttachmentsAsync(id)).ToList();

                // 设置缓存
                _cache.Set(cacheKey, draft, TimeSpan.FromMinutes(30));
            }

            return draft;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息草稿失败: {DraftId}", id);
            throw;
        }
    }

    /// <summary>
    /// 创建消息草稿
    /// </summary>
    public async Task<Guid> CreateAsync(MessageDraft draft)
    {
        try
        {
            // 验证输入
            await ValidateDraftAsync(draft);

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO MessageDrafts (
                    Id, AuthorId, ReceiverId, Subject, Content, MessageType, Priority, ParentId, ConversationId,
                    Tag, Status, CreatedAt, UpdatedAt, LastAutoSavedAt, ScheduledToSendAt, ExpiresAt, IsScheduled,
                    AutoSaveInterval, Notes
                ) VALUES (
                    @Id, @AuthorId, @ReceiverId, @Subject, @Content, @MessageType, @Priority, @ParentId, @ConversationId,
                    @Tag, @Status, @CreatedAt, @UpdatedAt, @LastAutoSavedAt, @ScheduledToSendAt, @ExpiresAt, @IsScheduled,
                    @AutoSaveInterval, @Notes
                )";

            await connection.ExecuteAsync(sql, draft);

            // 清除相关缓存
            await ClearUserDraftsCacheAsync(draft.AuthorId);
            if (draft.ConversationId.HasValue)
            {
                await ClearConversationDraftsCacheAsync(draft.ConversationId.Value);
            }

            _logger.LogInformation("创建消息草稿成功: {DraftId}, 作者: {AuthorId}", draft.Id, draft.AuthorId);
            return draft.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建消息草稿失败: {AuthorId}", draft.AuthorId);
            throw;
        }
    }

    /// <summary>
    /// 更新消息草稿
    /// </summary>
    public async Task<bool> UpdateAsync(MessageDraft draft)
    {
        try
        {
            // 验证输入和权限
            await ValidateDraftAsync(draft);
            if (!await CanUserAccessDraftAsync(draft.Id, draft.AuthorId))
            {
                _logger.LogWarning("用户 {UserId} 尝试更新无权限的草稿 {DraftId}", draft.AuthorId, draft.Id);
                return false;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                UPDATE MessageDrafts SET
                    ReceiverId = @ReceiverId,
                    Subject = @Subject,
                    Content = @Content,
                    MessageType = @MessageType,
                    Priority = @Priority,
                    ParentId = @ParentId,
                    ConversationId = @ConversationId,
                    Tag = @Tag,
                    Status = @Status,
                    UpdatedAt = @UpdatedAt,
                    LastAutoSavedAt = @LastAutoSavedAt,
                    ScheduledToSendAt = @ScheduledToSendAt,
                    ExpiresAt = @ExpiresAt,
                    IsScheduled = @IsScheduled,
                    AutoSaveInterval = @AutoSaveInterval,
                    Notes = @Notes
                WHERE Id = @Id AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, draft);

            if (affected > 0)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draft.Id);
                await ClearUserDraftsCacheAsync(draft.AuthorId);
                if (draft.ConversationId.HasValue)
                {
                    await ClearConversationDraftsCacheAsync(draft.ConversationId.Value);
                }

                _logger.LogInformation("更新消息草稿成功: {DraftId}", draft.Id);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息草稿失败: {DraftId}", draft.Id);
            throw;
        }
    }

    /// <summary>
    /// 删除消息草稿
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, Guid currentUserId)
    {
        try
        {
            // 验证权限
            if (!await CanUserAccessDraftAsync(id, currentUserId))
            {
                _logger.LogWarning("用户 {UserId} 尝试删除无权限的草稿 {DraftId}", currentUserId, id);
                return false;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = "UPDATE MessageDrafts SET DeletedAt = @DeletedAt WHERE Id = @Id AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });

            if (affected > 0)
            {
                // 清除相关缓存
                await ClearDraftCacheAsync(id);
                await ClearUserDraftsCacheAsync(currentUserId);

                _logger.LogInformation("删除消息草稿成功: {DraftId}, 操作者: {UserId}", id, currentUserId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息草稿失败: {DraftId}", id);
            throw;
        }
    }

    #endregion

    #region 分页和搜索功能

    /// <summary>
    /// 分页获取用户的草稿列表
    /// </summary>
    public async Task<PagedResultDto<MessageDraft>> GetPagedAsync(MessageDraftQueryDto query, Guid currentUserId)
    {
        try
        {
            // 构建查询条件
            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            conditions.Add("md.DeletedAt IS NULL");
            conditions.Add("md.AuthorId = @AuthorId");
            parameters.Add("AuthorId", currentUserId);

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                var validatedKeyword = _inputValidationService.SanitizeInput(query.Keyword);
                conditions.Add("(md.Subject LIKE @Keyword OR md.Content LIKE @Keyword OR md.Tag LIKE @Keyword)");
                parameters.Add("Keyword", $"%{validatedKeyword}%");
            }

            if (query.Status.HasValue)
            {
                conditions.Add("md.Status = @Status");
                parameters.Add("Status", query.Status.Value);
            }

            if (query.MessageType.HasValue)
            {
                conditions.Add("md.MessageType = @MessageType");
                parameters.Add("MessageType", query.MessageType.Value);
            }

            if (query.Priority.HasValue)
            {
                conditions.Add("md.Priority = @Priority");
                parameters.Add("Priority", query.Priority.Value);
            }

            if (query.ReceiverId.HasValue)
            {
                conditions.Add("md.ReceiverId = @ReceiverId");
                parameters.Add("ReceiverId", query.ReceiverId.Value);
            }

            if (query.ConversationId.HasValue)
            {
                conditions.Add("md.ConversationId = @ConversationId");
                parameters.Add("ConversationId", query.ConversationId.Value);
            }

            if (query.ParentId.HasValue)
            {
                conditions.Add("md.ParentId = @ParentId");
                parameters.Add("ParentId", query.ParentId.Value);
            }

            if (!string.IsNullOrEmpty(query.Tag))
            {
                var validatedTag = _inputValidationService.SanitizeInput(query.Tag);
                conditions.Add("md.Tag = @Tag");
                parameters.Add("Tag", validatedTag);
            }

            if (query.IsScheduled.HasValue)
            {
                conditions.Add("md.IsScheduled = @IsScheduled");
                parameters.Add("IsScheduled", query.IsScheduled.Value);
            }

            if (query.StartDate.HasValue)
            {
                conditions.Add("md.CreatedAt >= @StartDate");
                parameters.Add("StartDate", query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                conditions.Add("md.CreatedAt <= @EndDate");
                parameters.Add("EndDate", query.EndDate.Value);
            }

            var whereClause = string.Join(" AND ", conditions);

            using var connection = _dbConnectionFactory.CreateConnection();

            // 获取总数
            var countSql = $"SELECT COUNT(*) FROM MessageDrafts md WHERE {whereClause}";
            var total = await connection.QuerySingleAsync<int>(countSql, parameters);

            // 获取分页数据
            var dataSql = $@"
                SELECT md.*, 
                       u.Id, u.UserName, u.Email, u.ProfilePicture, u.FirstName, u.LastName, u.Bio, u.Location,
                       u2.Id, u2.UserName, u2.Email, u2.ProfilePicture, u2.FirstName, u2.LastName, u2.Bio, u2.Location,
                       m.Id, m.SenderId, m.ReceiverId, m.Subject, m.Content, m.MessageType, m.Status, m.Priority,
                       m.ParentId, m.ConversationId, m.IsRead, m.ReadAt, m.Tag, m.CreatedAt, m.UpdatedAt, m.DeletedAt,
                       m.SenderDeletedAt, m.ReceiverDeletedAt, m.ExpiresAt, m.SenderIp, m.SenderUserAgent
                FROM MessageDrafts md
                LEFT JOIN Users u ON md.AuthorId = u.Id
                LEFT JOIN Users u2 ON md.ReceiverId = u2.Id
                LEFT JOIN Messages m ON md.ParentId = m.Id
                WHERE {whereClause}
                ORDER BY {GetSortExpression(query.SortBy, query.SortDirection)}
                LIMIT @Limit OFFSET @Offset";

            parameters.Add("Limit", query.PageSize);
            parameters.Add("Offset", (query.Page - 1) * query.PageSize);

            var result = await connection.QueryAsync<MessageDraft, User, User, Message, MessageDraft>(
                dataSql,
                (draft, author, receiver, parent) =>
                {
                    draft.Author = author;
                    draft.Receiver = receiver;
                    draft.Parent = parent;
                    return draft;
                },
                parameters,
                splitOn: "Id,Id,Id");

            var drafts = result.ToList();

            // 为每个草稿加载附件
            foreach (var draft in drafts)
            {
                draft.DraftAttachments = (await GetDraftAttachmentsAsync(draft.Id)).ToList();
            }

            return new PagedResultDto<MessageDraft>
            {
                Items = drafts,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取用户草稿列表失败: {UserId}", currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 获取会话的草稿列表
    /// </summary>
    public async Task<List<MessageDraft>> GetConversationDraftsAsync(Guid conversationId, Guid currentUserId)
    {
        try
        {
            // 检查缓存
            var cacheKey = $"{CONVERSATION_DRAFTS_CACHE_PREFIX}{conversationId}";
            if (_cache.TryGetValue(cacheKey, out List<MessageDraft>? cachedDrafts))
            {
                return cachedDrafts ?? new List<MessageDraft>();
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT md.*, 
                       u.Id, u.UserName, u.Email, u.ProfilePicture, u.FirstName, u.LastName, u.Bio, u.Location,
                       u2.Id, u2.UserName, u2.Email, u2.ProfilePicture, u2.FirstName, u2.LastName, u2.Bio, u2.Location,
                       m.Id, m.SenderId, m.ReceiverId, m.Subject, m.Content, m.MessageType, m.Status, m.Priority,
                       m.ParentId, m.ConversationId, m.IsRead, m.ReadAt, m.Tag, m.CreatedAt, m.UpdatedAt, m.DeletedAt,
                       m.SenderDeletedAt, m.ReceiverDeletedAt, m.ExpiresAt, m.SenderIp, m.SenderUserAgent
                FROM MessageDrafts md
                LEFT JOIN Users u ON md.AuthorId = u.Id
                LEFT JOIN Users u2 ON md.ReceiverId = u2.Id
                LEFT JOIN Messages m ON md.ParentId = m.Id
                WHERE md.ConversationId = @ConversationId 
                  AND md.DeletedAt IS NULL
                  AND md.AuthorId = @AuthorId
                ORDER BY md.UpdatedAt DESC";

            var result = await connection.QueryAsync<MessageDraft, User, User, Message, MessageDraft>(
                sql,
                (draft, author, receiver, parent) =>
                {
                    draft.Author = author;
                    draft.Receiver = receiver;
                    draft.Parent = parent;
                    return draft;
                },
                new { ConversationId = conversationId, AuthorId = currentUserId },
                splitOn: "Id,Id,Id");

            var drafts = result.ToList();

            // 为每个草稿加载附件
            foreach (var draft in drafts)
            {
                draft.DraftAttachments = (await GetDraftAttachmentsAsync(draft.Id)).ToList();
            }

            // 设置缓存
            _cache.Set(cacheKey, drafts, TimeSpan.FromMinutes(10));

            return drafts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话草稿列表失败: {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的草稿统计信息
    /// </summary>
    public async Task<UserDraftStatsDto> GetUserDraftStatsAsync(Guid userId)
    {
        try
        {
            // 检查缓存
            var cacheKey = $"{DRAFT_STATS_CACHE_PREFIX}{userId}";
            if (_cache.TryGetValue(cacheKey, out UserDraftStatsDto? cachedStats))
            {
                return cachedStats!;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    COUNT(*) as TotalDrafts,
                    COUNT(CASE WHEN md.Status = 0 THEN 1 END) as DraftCount,
                    COUNT(CASE WHEN md.Status = 1 THEN 1 END) as SentCount,
                    COUNT(CASE WHEN md.Status = 2 THEN 1 END) as CancelledCount,
                    COUNT(CASE WHEN md.Status = 3 THEN 1 END) as ExpiredCount,
                    COUNT(CASE WHEN md.IsScheduled = 1 THEN 1 END) as ScheduledCount,
                    COUNT(CASE WHEN md.ExpiresAt <= @Now THEN 1 END) as ExpiredDraftsCount,
                    COUNT(CASE WHEN md.ScheduledToSendAt <= @Now AND md.IsScheduled = 1 AND md.Status = 0 THEN 1 END) as ReadyToSendCount,
                    COUNT(CASE WHEN md.ReceiverId IS NOT NULL THEN 1 END) as DirectMessageCount,
                    COUNT(CASE WHEN md.ConversationId IS NOT NULL THEN 1 END) as ConversationDraftCount,
                    COUNT(CASE WHEN md.ParentId IS NOT NULL THEN 1 END) as ReplyDraftCount,
                    COALESCE(SUM(CASE WHEN md.UpdatedAt >= DATE_SUB(@Now, INTERVAL 24 HOUR) THEN 1 ELSE 0 END), 0) as RecentActivityCount
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL";

            var stats = await connection.QuerySingleAsync<UserDraftStatsDto>(sql, new { UserId = userId, Now = DateTime.UtcNow });

            // 获取按类型统计
            var typeStatsSql = @"
                SELECT md.MessageType, COUNT(*) as Count
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL
                GROUP BY md.MessageType";

            var typeStats = await connection.QueryAsync<KeyValuePair<MessageType, int>>(typeStatsSql, new { UserId = userId });
            stats.TypeStats = typeStats.ToDictionary(kv => kv.Key, kv => kv.Value);

            // 获取按优先级统计
            var priorityStatsSql = @"
                SELECT md.Priority, COUNT(*) as Count
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL
                GROUP BY md.Priority";

            var priorityStats = await connection.QueryAsync<KeyValuePair<MessagePriority, int>>(priorityStatsSql, new { UserId = userId });
            stats.PriorityStats = priorityStats.ToDictionary(kv => kv.Key, kv => kv.Value);

            // 获取按状态统计
            var statusStatsSql = @"
                SELECT md.Status, COUNT(*) as Count
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL
                GROUP BY md.Status";

            var statusStats = await connection.QueryAsync<KeyValuePair<DraftStatus, int>>(statusStatsSql, new { UserId = userId });
            stats.StatusStats = statusStats.ToDictionary(kv => kv.Key, kv => kv.Value);

            // 获取按标签统计
            var tagStatsSql = @"
                SELECT md.Tag, COUNT(*) as Count
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL AND md.Tag IS NOT NULL
                GROUP BY md.Tag
                ORDER BY Count DESC
                LIMIT 10";

            var tagStats = await connection.QueryAsync<KeyValuePair<string, int>>(tagStatsSql, new { UserId = userId });
            stats.TagStats = tagStats.ToDictionary(kv => kv.Key, kv => kv.Value);

            // 获取最近的草稿
            var recentDraftsSql = @"
                SELECT md.Id, md.Subject, md.UpdatedAt, md.Status
                FROM MessageDrafts md
                WHERE md.AuthorId = @UserId AND md.DeletedAt IS NULL
                ORDER BY md.UpdatedAt DESC
                LIMIT 5";

            stats.RecentDrafts = (await connection.QueryAsync<RecentDraftDto>(recentDraftsSql, new { UserId = userId })).ToList();

            // 设置缓存
            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(15));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户草稿统计失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 草稿管理功能

    /// <summary>
    /// 自动保存草稿
    /// </summary>
    public async Task<bool> AutoSaveDraftAsync(MessageDraft draft)
    {
        try
        {
            draft.UpdatedAt = DateTime.UtcNow;
            draft.LastAutoSavedAt = DateTime.UtcNow;

            // 如果草稿不存在，创建新草稿
            if (draft.Id == Guid.Empty)
            {
                draft.Id = Guid.NewGuid();
                draft.CreatedAt = DateTime.UtcNow;
                draft.Status = DraftStatus.Draft;
                return await CreateAsync(draft) != Guid.Empty;
            }

            // 更新现有草稿
            return await UpdateAsync(draft);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动保存草稿失败: {DraftId}", draft.Id);
            return false;
        }
    }

    /// <summary>
    /// 发送草稿
    /// </summary>
    public async Task<bool> SendDraftAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            // 验证权限和草稿状态
            if (!await CanUserAccessDraftAsync(draftId, currentUserId))
            {
                _logger.LogWarning("用户 {UserId} 尝试发送无权限的草稿 {DraftId}", currentUserId, draftId);
                return false;
            }

            var draft = await GetByIdAsync(draftId, currentUserId);
            if (draft == null || draft.Status != DraftStatus.Draft)
            {
                _logger.LogWarning("草稿 {DraftId} 不存在或状态不正确", draftId);
                return false;
            }

            // 验证草稿内容
            if (string.IsNullOrWhiteSpace(draft.Content))
            {
                _logger.LogWarning("草稿 {DraftId} 内容为空，无法发送", draftId);
                return false;
            }

            // 更新草稿状态为已发送
            draft.Status = DraftStatus.Sent;
            draft.UpdatedAt = DateTime.UtcNow;

            var success = await UpdateAsync(draft);
            if (success)
            {
                _logger.LogInformation("草稿发送成功: {DraftId}, 发送者: {UserId}", draftId, currentUserId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送草稿失败: {DraftId}", draftId);
            return false;
        }
    }

    /// <summary>
    /// 取消定时发送的草稿
    /// </summary>
    public async Task<bool> CancelScheduledDraftAsync(Guid draftId, Guid currentUserId)
    {
        try
        {
            // 验证权限
            if (!await CanUserAccessDraftAsync(draftId, currentUserId))
            {
                _logger.LogWarning("用户 {UserId} 尝试取消无权限的定时草稿 {DraftId}", currentUserId, draftId);
                return false;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                UPDATE MessageDrafts SET
                    IsScheduled = 0,
                    ScheduledToSendAt = NULL,
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id 
                  AND AuthorId = @AuthorId 
                  AND DeletedAt IS NULL
                  AND IsScheduled = 1";

            var affected = await connection.ExecuteAsync(sql, new
            {
                Id = draftId,
                AuthorId = currentUserId,
                Status = DraftStatus.Cancelled,
                UpdatedAt = DateTime.UtcNow
            });

            if (affected > 0)
            {
                // 清除缓存
                await ClearDraftCacheAsync(draftId);
                await ClearUserDraftsCacheAsync(currentUserId);

                _logger.LogInformation("取消定时草稿成功: {DraftId}, 操作者: {UserId}", draftId, currentUserId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消定时草稿失败: {DraftId}", draftId);
            return false;
        }
    }

    /// <summary>
    /// 获取即将过期的草稿
    /// </summary>
    public async Task<List<MessageDraft>> GetExpiringDraftsAsync(TimeSpan withinTimeframe)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT md.*, 
                       u.Id, u.UserName, u.Email, u.ProfilePicture, u.FirstName, u.LastName, u.Bio, u.Location,
                       u2.Id, u2.UserName, u2.Email, u2.ProfilePicture, u2.FirstName, u2.LastName, u2.Bio, u2.Location,
                       m.Id, m.SenderId, m.ReceiverId, m.Subject, m.Content, m.MessageType, m.Status, m.Priority,
                       m.ParentId, m.ConversationId, m.IsRead, m.ReadAt, m.Tag, m.CreatedAt, m.UpdatedAt, m.DeletedAt,
                       m.SenderDeletedAt, m.ReceiverDeletedAt, m.ExpiresAt, m.SenderIp, m.SenderUserAgent
                FROM MessageDrafts md
                LEFT JOIN Users u ON md.AuthorId = u.Id
                LEFT JOIN Users u2 ON md.ReceiverId = u2.Id
                LEFT JOIN Messages m ON md.ParentId = m.Id
                WHERE md.ExpiresAt <= @ExpiryDate 
                  AND md.DeletedAt IS NULL
                  AND md.Status = 0
                ORDER BY md.ExpiresAt ASC";

            var result = await connection.QueryAsync<MessageDraft, User, User, Message, MessageDraft>(
                sql,
                (draft, author, receiver, parent) =>
                {
                    draft.Author = author;
                    draft.Receiver = receiver;
                    draft.Parent = parent;
                    return draft;
                },
                new { ExpiryDate = DateTime.UtcNow.Add(withinTimeframe) },
                splitOn: "Id,Id,Id");

            var drafts = result.ToList();

            // 为每个草稿加载附件
            foreach (var draft in drafts)
            {
                draft.DraftAttachments = (await GetDraftAttachmentsAsync(draft.Id)).ToList();
            }

            return drafts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取即将过期草稿失败");
            throw;
        }
    }

    /// <summary>
    /// 获取待发送的定时草稿
    /// </summary>
    public async Task<List<MessageDraft>> GetScheduledDraftsToSendAsync()
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT md.*, 
                       u.Id, u.UserName, u.Email, u.ProfilePicture, u.FirstName, u.LastName, u.Bio, u.Location,
                       u2.Id, u2.UserName, u2.Email, u2.ProfilePicture, u2.FirstName, u2.LastName, u2.Bio, u2.Location,
                       m.Id, m.SenderId, m.ReceiverId, m.Subject, m.Content, m.MessageType, m.Status, m.Priority,
                       m.ParentId, m.ConversationId, m.IsRead, m.ReadAt, m.Tag, m.CreatedAt, m.UpdatedAt, m.DeletedAt,
                       m.SenderDeletedAt, m.ReceiverDeletedAt, m.ExpiresAt, m.SenderIp, m.SenderUserAgent
                FROM MessageDrafts md
                LEFT JOIN Users u ON md.AuthorId = u.Id
                LEFT JOIN Users u2 ON md.ReceiverId = u2.Id
                LEFT JOIN Messages m ON md.ParentId = m.Id
                WHERE md.ScheduledToSendAt <= @Now 
                  AND md.IsScheduled = 1
                  AND md.Status = 0
                  AND md.DeletedAt IS NULL
                ORDER BY md.ScheduledToSendAt ASC";

            var result = await connection.QueryAsync<MessageDraft, User, User, Message, MessageDraft>(
                sql,
                (draft, author, receiver, parent) =>
                {
                    draft.Author = author;
                    draft.Receiver = receiver;
                    draft.Parent = parent;
                    return draft;
                },
                new { Now = DateTime.UtcNow },
                splitOn: "Id,Id,Id");

            var drafts = result.ToList();

            // 为每个草稿加载附件
            foreach (var draft in drafts)
            {
                draft.DraftAttachments = (await GetDraftAttachmentsAsync(draft.Id)).ToList();
            }

            return drafts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待发送定时草稿失败");
            throw;
        }
    }

    /// <summary>
    /// 清理过期的草稿
    /// </summary>
    public async Task<int> CleanupExpiredDraftsAsync()
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            
            // 获取即将过期的草稿ID列表（用于清除缓存）
            var expiredIdsSql = @"
                SELECT Id FROM MessageDrafts 
                WHERE ExpiresAt <= @Now 
                  AND DeletedAt IS NULL
                  AND Status = 0";

            var expiredIds = await connection.QueryAsync<Guid>(expiredIdsSql, new { Now = DateTime.UtcNow });

            // 删除过期草稿
            var sql = "UPDATE MessageDrafts SET DeletedAt = @DeletedAt WHERE ExpiresAt <= @Now AND DeletedAt IS NULL AND Status = 0";
            var affected = await connection.ExecuteAsync(sql, new { DeletedAt = DateTime.UtcNow, Now = DateTime.UtcNow });

            // 清除相关缓存
            foreach (var draftId in expiredIds)
            {
                await ClearDraftCacheAsync(draftId);
            }

            if (affected > 0)
            {
                _logger.LogInformation("清理过期草稿完成，删除了 {Count} 个草稿", affected);
            }

            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期草稿失败");
            throw;
        }
    }

    #endregion

    #region 权限验证和安全检查

    /// <summary>
    /// 检查用户是否有权限访问草稿
    /// </summary>
    public async Task<bool> CanUserAccessDraftAsync(Guid draftId, Guid userId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT COUNT(*) FROM MessageDrafts 
                WHERE Id = @Id AND AuthorId = @UserId AND DeletedAt IS NULL";

            var count = await connection.QuerySingleAsync<int>(sql, new { Id = draftId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户草稿访问权限失败: {DraftId}, {UserId}", draftId, userId);
            return false;
        }
    }

    /// <summary>
    /// 检查用户是否可以编辑草稿
    /// </summary>
    public async Task<bool> CanUserEditDraftAsync(Guid draftId, Guid userId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT COUNT(*) FROM MessageDrafts 
                WHERE Id = @Id 
                  AND AuthorId = @UserId 
                  AND DeletedAt IS NULL
                  AND Status = 0";

            var count = await connection.QuerySingleAsync<int>(sql, new { Id = draftId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户草稿编辑权限失败: {DraftId}, {UserId}", draftId, userId);
            return false;
        }
    }

    /// <summary>
    /// 验证草稿数据
    /// </summary>
    private async Task ValidateDraftAsync(MessageDraft draft)
    {
        if (draft == null)
            throw new ArgumentNullException(nameof(draft));

        if (draft.AuthorId == Guid.Empty)
            throw new ArgumentException("草稿作者ID不能为空");

        if (!string.IsNullOrWhiteSpace(draft.Subject) && draft.Subject.Length > 200)
            throw new ArgumentException("草稿主题长度不能超过200个字符");

        if (string.IsNullOrWhiteSpace(draft.Content))
            throw new ArgumentException("草稿内容不能为空");

        if (draft.Content.Length > 2000)
            throw new ArgumentException("草稿内容长度不能超过2000个字符");

        if (!string.IsNullOrWhiteSpace(draft.Tag) && draft.Tag.Length > 100)
            throw new ArgumentException("草稿标签长度不能超过100个字符");

        if (draft.AutoSaveInterval < 30 || draft.AutoSaveInterval > 3600)
            throw new ArgumentException("自动保存间隔必须在30-3600秒之间");

        if (draft.ScheduledToSendAt.HasValue && draft.ScheduledToSendAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("定时发送时间必须在未来");

        if (draft.ExpiresAt.HasValue && draft.ExpiresAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("过期时间必须在未来");

        // 验证收件人是否存在（如果指定了收件人）
        if (draft.ReceiverId.HasValue)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var receiverExists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM Users WHERE Id = @Id", new { Id = draft.ReceiverId.Value });
            
            if (receiverExists == 0)
                throw new ArgumentException("收件人不存在");
        }

        // 验证父消息是否存在（如果指定了父消息）
        if (draft.ParentId.HasValue)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var parentExists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM Messages WHERE Id = @Id AND DeletedAt IS NULL", new { Id = draft.ParentId.Value });
            
            if (parentExists == 0)
                throw new ArgumentException("父消息不存在");
        }
    }

    #endregion

    #region 草稿附件管理

    /// <summary>
    /// 获取草稿附件列表
    /// </summary>
    private async Task<IEnumerable<MessageDraftAttachment>> GetDraftAttachmentsAsync(Guid draftId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                SELECT * FROM MessageDraftAttachments 
                WHERE DraftId = @DraftId AND DeletedAt IS NULL
                ORDER BY UploadedAt ASC";

            return await connection.QueryAsync<MessageDraftAttachment>(sql, new { DraftId = draftId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取草稿附件失败: {DraftId}", draftId);
            return Enumerable.Empty<MessageDraftAttachment>();
        }
    }

    /// <summary>
    /// 添加草稿附件
    /// </summary>
    public async Task<bool> AddDraftAttachmentAsync(MessageDraftAttachment attachment)
    {
        try
        {
            // 验证附件数据
            await ValidateDraftAttachmentAsync(attachment);

            // 验证用户权限
            if (!await CanUserAccessDraftAsync(attachment.DraftId, attachment.Draft?.AuthorId ?? Guid.Empty))
            {
                _logger.LogWarning("用户尝试为无权限的草稿添加附件: {DraftId}", attachment.DraftId);
                return false;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO MessageDraftAttachments (
                    Id, DraftId, FileName, OriginalFileName, FileSize, ContentType, FileExtension,
                    FilePath, FileUrl, AttachmentType, AttachmentStatus, UploadProgress, UploadedAt, DeletedAt
                ) VALUES (
                    @Id, @DraftId, @FileName, @OriginalFileName, @FileSize, @ContentType, @FileExtension,
                    @FilePath, @FileUrl, @AttachmentType, @AttachmentStatus, @UploadProgress, @UploadedAt, @DeletedAt
                )";

            var affected = await connection.ExecuteAsync(sql, attachment);

            if (affected > 0)
            {
                // 清除草稿缓存
                await ClearDraftCacheAsync(attachment.DraftId);

                _logger.LogInformation("添加草稿附件成功: {AttachmentId}, 草稿: {DraftId}", attachment.Id, attachment.DraftId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加草稿附件失败: {DraftId}", attachment.DraftId);
            return false;
        }
    }

    /// <summary>
    /// 更新草稿附件上传进度
    /// </summary>
    public async Task<bool> UpdateDraftAttachmentProgressAsync(Guid attachmentId, int progress)
    {
        try
        {
            if (progress < 0 || progress > 100)
                throw new ArgumentException("上传进度必须在0-100之间");

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = @"
                UPDATE MessageDraftAttachments SET
                    UploadProgress = @Progress,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new
            {
                Id = attachmentId,
                Progress = progress,
                UpdatedAt = DateTime.UtcNow
            });

            if (affected > 0)
            {
                // 获取草稿ID并清除缓存
                var draftId = await connection.QuerySingleAsync<Guid?>(
                    "SELECT DraftId FROM MessageDraftAttachments WHERE Id = @Id", new { Id = attachmentId });
                
                if (draftId.HasValue)
                {
                    await ClearDraftCacheAsync(draftId.Value);
                }

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新草稿附件上传进度失败: {AttachmentId}", attachmentId);
            return false;
        }
    }

    /// <summary>
    /// 删除草稿附件
    /// </summary>
    public async Task<bool> DeleteDraftAttachmentAsync(Guid attachmentId, Guid currentUserId)
    {
        try
        {
            // 验证用户权限
            var draftId = await GetDraftIdByAttachmentIdAsync(attachmentId);
            if (!draftId.HasValue || !await CanUserAccessDraftAsync(draftId.Value, currentUserId))
            {
                _logger.LogWarning("用户尝试删除无权限的草稿附件: {AttachmentId}", attachmentId);
                return false;
            }

            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = "UPDATE MessageDraftAttachments SET DeletedAt = @DeletedAt WHERE Id = @Id AND DeletedAt IS NULL";

            var affected = await connection.ExecuteAsync(sql, new { Id = attachmentId, DeletedAt = DateTime.UtcNow });

            if (affected > 0)
            {
                // 清除草稿缓存
                if (draftId.HasValue)
                {
                    await ClearDraftCacheAsync(draftId.Value);
                }

                _logger.LogInformation("删除草稿附件成功: {AttachmentId}, 操作者: {UserId}", attachmentId, currentUserId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除草稿附件失败: {AttachmentId}", attachmentId);
            return false;
        }
    }

    /// <summary>
    /// 验证草稿附件数据
    /// </summary>
    private async Task ValidateDraftAttachmentAsync(MessageDraftAttachment attachment)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));

        if (attachment.DraftId == Guid.Empty)
            throw new ArgumentException("草稿ID不能为空");

        if (string.IsNullOrWhiteSpace(attachment.FileName))
            throw new ArgumentException("附件文件名不能为空");

        if (attachment.FileName.Length > 255)
            throw new ArgumentException("附件文件名长度不能超过255个字符");

        if (string.IsNullOrWhiteSpace(attachment.OriginalFileName))
            throw new ArgumentException("附件原始文件名不能为空");

        if (attachment.OriginalFileName.Length > 255)
            throw new ArgumentException("附件原始文件名长度不能超过255个字符");

        if (attachment.FileSize < 0)
            throw new ArgumentException("附件文件大小不能为负数");

        if (string.IsNullOrWhiteSpace(attachment.FilePath))
            throw new ArgumentException("附件文件路径不能为空");

        if (attachment.FilePath.Length > 500)
            throw new ArgumentException("附件文件路径长度不能超过500个字符");

        // 验证文件大小限制（例如：50MB）
        const long maxFileSize = 50 * 1024 * 1024; // 50MB
        if (attachment.FileSize > maxFileSize)
            throw new ArgumentException($"附件文件大小不能超过{maxFileSize / 1024 / 1024}MB");

        // 验证文件类型
        if (!string.IsNullOrWhiteSpace(attachment.ContentType))
        {
            var allowedTypes = new[]
            {
                "image/jpeg", "image/png", "image/gif", "image/webp",
                "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/plain", "text/csv",
                "application/zip", "application/x-rar-compressed"
            };

            if (!allowedTypes.Contains(attachment.ContentType.ToLower()))
            {
                throw new ArgumentException("不支持的文件类型");
            }
        }
    }

    /// <summary>
    /// 根据附件ID获取草稿ID
    /// </summary>
    private async Task<Guid?> GetDraftIdByAttachmentIdAsync(Guid attachmentId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var sql = "SELECT DraftId FROM MessageDraftAttachments WHERE Id = @Id AND DeletedAt IS NULL";
            return await connection.QuerySingleOrDefaultAsync<Guid?>(sql, new { Id = attachmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据附件ID获取草稿ID失败: {AttachmentId}", attachmentId);
            return null;
        }
    }

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除草稿缓存
    /// </summary>
    private async Task ClearDraftCacheAsync(Guid draftId)
    {
        var cacheKey = $"{DRAFT_CACHE_PREFIX}{draftId}";
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// 清除用户草稿缓存
    /// </summary>
    private async Task ClearUserDraftsCacheAsync(Guid userId)
    {
        var cacheKey = $"{USER_DRAFTS_CACHE_PREFIX}{userId}";
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// 清除会话草稿缓存
    /// </summary>
    private async Task ClearConversationDraftsCacheAsync(Guid conversationId)
    {
        var cacheKey = $"{CONVERSATION_DRAFTS_CACHE_PREFIX}{conversationId}";
        _cache.Remove(cacheKey);
    }

    /// <summary>
    /// 清除草稿统计缓存
    /// </summary>
    private async Task ClearDraftStatsCacheAsync(Guid userId)
    {
        var cacheKey = $"{DRAFT_STATS_CACHE_PREFIX}{userId}";
        _cache.Remove(cacheKey);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取排序表达式
    /// </summary>
    private string GetSortExpression(string? sortBy, string? sortDirection)
    {
        var direction = sortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
        
        return sortBy?.ToLower() switch
        {
            "subject" => $"md.Subject {direction}",
            "createdat" => $"md.CreatedAt {direction}",
            "updatedat" => $"md.UpdatedAt {direction}",
            "priority" => $"md.Priority {direction}",
            "status" => $"md.Status {direction}",
            "scheduledtosendat" => $"md.ScheduledToSendAt {direction}",
            "expiresat" => $"md.ExpiresAt {direction}",
            _ => $"md.UpdatedAt {direction}" // 默认按更新时间排序
        };
    }

    #endregion
}

/// <summary>
/// 用户草稿统计信息DTO
/// </summary>
public class UserDraftStatsDto
{
    /// <summary>
    /// 总草稿数量
    /// </summary>
    public int TotalDrafts { get; set; }

    /// <summary>
    /// 草稿数量
    /// </summary>
    public int DraftCount { get; set; }

    /// <summary>
    /// 已发送数量
    /// </summary>
    public int SentCount { get; set; }

    /// <summary>
    /// 已取消数量
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// 已过期数量
    /// </summary>
    public int ExpiredCount { get; set; }

    /// <summary>
    /// 定时发送数量
    /// </summary>
    public int ScheduledCount { get; set; }

    /// <summary>
    /// 已过期草稿数量
    /// </summary>
    public int ExpiredDraftsCount { get; set; }

    /// <summary>
    /// 待发送草稿数量
    /// </summary>
    public int ReadyToSendCount { get; set; }

    /// <summary>
    /// 私信草稿数量
    /// </summary>
    public int DirectMessageCount { get; set; }

    /// <summary>
    /// 会话草稿数量
    /// </summary>
    public int ConversationDraftCount { get; set; }

    /// <summary>
    /// 回复草稿数量
    /// </summary>
    public int ReplyDraftCount { get; set; }

    /// <summary>
    /// 最近活动数量（24小时内）
    /// </summary>
    public int RecentActivityCount { get; set; }

    /// <summary>
    /// 按消息类型统计
    /// </summary>
    public Dictionary<MessageType, int> TypeStats { get; set; } = new();

    /// <summary>
    /// 按优先级统计
    /// </summary>
    public Dictionary<MessagePriority, int> PriorityStats { get; set; } = new();

    /// <summary>
    /// 按状态统计
    /// </summary>
    public Dictionary<DraftStatus, int> StatusStats { get; set; } = new();

    /// <summary>
    /// 按标签统计
    /// </summary>
    public Dictionary<string, int> TagStats { get; set; } = new();

    /// <summary>
    /// 最近的草稿列表
    /// </summary>
    public List<RecentDraftDto> RecentDrafts { get; set; } = new();
}

/// <summary>
/// 最近草稿DTO
/// </summary>
public class RecentDraftDto
{
    /// <summary>
    /// 草稿ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 草稿主题
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 草稿状态
    /// </summary>
    public DraftStatus Status { get; set; }
}

/// <summary>
/// 消息草稿查询DTO
/// </summary>
public class MessageDraftQueryDto
{
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 草稿状态
    /// </summary>
    public DraftStatus? Status { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType? MessageType { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public MessagePriority? Priority { get; set; }

    /// <summary>
    /// 收件人ID
    /// </summary>
    public Guid? ReceiverId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// 父消息ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 是否定时发送
    /// </summary>
    public bool? IsScheduled { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 排序方向
    /// </summary>
    public string? SortDirection { get; set; }
}

/// <summary>
/// 分页结果DTO
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
}