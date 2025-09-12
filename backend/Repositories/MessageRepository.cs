using Dapper;
using System.Text;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 消息仓储实现 - 使用 Dapper ORM
/// 提供完整的消息CRUD操作、查询功能和会话管理
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<MessageRepository> _logger;

    public MessageRepository(IDbConnectionFactory connectionFactory, ILogger<MessageRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础 CRUD 操作

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT * FROM Messages WHERE Id = @Id AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<Message>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message?> GetByIdWithSenderAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, s.Id, s.Username, s.Email, s.Role, s.CreatedAt, s.UpdatedAt, s.IsActive
                FROM Messages m
                LEFT JOIN Users s ON m.SenderId = s.Id
                WHERE m.Id = @Id AND m.DeletedAt IS NULL";
            
            var messageDict = new Dictionary<Guid, Message>();
            
            var messages = await connection.QueryAsync<Message, User, Message>(
                sql,
                (message, sender) =>
                {
                    if (!messageDict.TryGetValue(message.Id, out var existingMessage))
                    {
                        existingMessage = message;
                        existingMessage.Sender = sender;
                        messageDict.Add(message.Id, existingMessage);
                    }
                    return existingMessage;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            return messages.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息（包含发送者信息）失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message?> GetByIdWithUsersAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT m.*, 
                       s.Id, s.Username, s.Email, s.Role, s.CreatedAt, s.UpdatedAt, s.IsActive,
                       r.Id, r.Username, r.Email, r.Role, r.CreatedAt, r.UpdatedAt, r.IsActive
                FROM Messages m
                LEFT JOIN Users s ON m.SenderId = s.Id
                LEFT JOIN Users r ON m.ReceiverId = r.Id
                WHERE m.Id = @Id AND m.DeletedAt IS NULL";
            
            var messageDict = new Dictionary<Guid, Message>();
            
            var messages = await connection.QueryAsync<Message, User, User, Message>(
                sql,
                (message, sender, receiver) =>
                {
                    if (!messageDict.TryGetValue(message.Id, out var existingMessage))
                    {
                        existingMessage = message;
                        existingMessage.Sender = sender;
                        existingMessage.Receiver = receiver;
                        messageDict.Add(message.Id, existingMessage);
                    }
                    return existingMessage;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            return messages.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息（包含发送者和接收者信息）失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message?> GetByIdWithAttachmentsAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string messageSql = "SELECT * FROM Messages WHERE Id = @Id AND DeletedAt IS NULL";
            const string attachmentSql = "SELECT * FROM MessageAttachments WHERE MessageId = @Id AND DeletedAt IS NULL";
            
            var message = await connection.QuerySingleOrDefaultAsync<Message>(messageSql, new { Id = id });
            if (message != null)
            {
                var attachments = await connection.QueryAsync<MessageAttachment>(attachmentSql, new { Id = id });
                message.Attachments = attachments.ToList();
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息（包含附件信息）失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message?> GetByIdWithRepliesAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string messageSql = "SELECT * FROM Messages WHERE Id = @Id AND DeletedAt IS NULL";
            const string repliesSql = "SELECT * FROM Messages WHERE ParentId = @Id AND DeletedAt IS NULL ORDER BY CreatedAt ASC";
            
            var message = await connection.QuerySingleOrDefaultAsync<Message>(messageSql, new { Id = id });
            if (message != null)
            {
                var replies = await connection.QueryAsync<Message>(repliesSql, new { Id = id });
                message.Replies = replies.ToList();
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息（包含回复消息）失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message?> GetByIdWithFullDetailsAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 获取基本信息
            const string messageSql = @"
                SELECT m.*, 
                       s.Id, s.Username, s.Email, s.Role, s.CreatedAt, s.UpdatedAt, s.IsActive,
                       r.Id, r.Username, r.Email, r.Role, r.CreatedAt, r.UpdatedAt, r.IsActive
                FROM Messages m
                LEFT JOIN Users s ON m.SenderId = s.Id
                LEFT JOIN Users r ON m.ReceiverId = r.Id
                WHERE m.Id = @Id AND m.DeletedAt IS NULL";
            
            var messageDict = new Dictionary<Guid, Message>();
            
            var messages = await connection.QueryAsync<Message, User, User, Message>(
                messageSql,
                (message, sender, receiver) =>
                {
                    if (!messageDict.TryGetValue(message.Id, out var existingMessage))
                    {
                        existingMessage = message;
                        existingMessage.Sender = sender;
                        existingMessage.Receiver = receiver;
                        messageDict.Add(message.Id, existingMessage);
                    }
                    return existingMessage;
                },
                new { Id = id },
                splitOn: "Id"
            );
            
            var message = messages.FirstOrDefault();
            if (message != null)
            {
                // 获取附件
                const string attachmentSql = "SELECT * FROM MessageAttachments WHERE MessageId = @Id AND DeletedAt IS NULL";
                var attachments = await connection.QueryAsync<MessageAttachment>(attachmentSql, new { Id = id });
                message.Attachments = attachments.ToList();
                
                // 获取回复
                const string repliesSql = "SELECT * FROM Messages WHERE ParentId = @Id AND DeletedAt IS NULL ORDER BY CreatedAt ASC";
                var replies = await connection.QueryAsync<Message>(repliesSql, new { Id = id });
                message.Replies = replies.ToList();
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息（完整信息）失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<Message> CreateAsync(Message message)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO Messages (
                    Id, SenderId, ReceiverId, Subject, Content, MessageType, Status, Priority,
                    ParentId, ConversationId, IsRead, ReadAt, Tag, CreatedAt, UpdatedAt,
                    DeletedAt, SenderDeletedAt, ReceiverDeletedAt, ExpiresAt, SenderIp, SenderUserAgent
                ) VALUES (
                    @Id, @SenderId, @ReceiverId, @Subject, @Content, @MessageType, @Status, @Priority,
                    @ParentId, @ConversationId, @IsRead, @ReadAt, @Tag, @CreatedAt, @UpdatedAt,
                    @DeletedAt, @SenderDeletedAt, @ReceiverDeletedAt, @ExpiresAt, @SenderIp, @SenderUserAgent
                )";
            
            await connection.ExecuteAsync(sql, message);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建消息失败: {SenderId} -> {ReceiverId}", message.SenderId, message.ReceiverId);
            throw;
        }
    }

    public async Task<Message> UpdateAsync(Message message)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Messages SET
                    Subject = @Subject,
                    Content = @Content,
                    MessageType = @MessageType,
                    Status = @Status,
                    Priority = @Priority,
                    IsRead = @IsRead,
                    ReadAt = @ReadAt,
                    Tag = @Tag,
                    UpdatedAt = @UpdatedAt,
                    ExpiresAt = @ExpiresAt
                WHERE Id = @Id AND DeletedAt IS NULL";
            
            await connection.ExecuteAsync(sql, message);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息失败: {MessageId}", message.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE Messages SET DeletedAt = @Now WHERE Id = @Id AND DeletedAt IS NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除消息失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Messages WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "硬删除消息失败: {MessageId}", id);
            throw;
        }
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE Messages SET DeletedAt = NULL WHERE Id = @Id AND DeletedAt IS NOT NULL";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复消息失败: {MessageId}", id);
            throw;
        }
    }

    #endregion

    #region 分页查询

    public async Task<PaginatedResult<Message>> GetPagedAsync(MessageFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildWhereClause(filter);
            var orderByClause = BuildOrderByClause(filter.SortBy);
            
            var countSql = $"SELECT COUNT(*) FROM Messages m {whereClause}";
            var dataSql = $@"
                SELECT m.* FROM Messages m 
                {whereClause}
                {orderByClause}
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = BuildParameters(filter);
            parameters["Limit"] = filter.PageSize;
            parameters["Offset"] = (filter.Page - 1) * filter.PageSize;
            
            var totalCount = await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Message>(dataSql, parameters);
            
            // 加载关联数据
            if (filter.IncludeSender || filter.IncludeReceiver || filter.IncludeAttachments || filter.IncludeReplies)
            {
                items = await LoadRelatedDataAsync(items.ToList(), filter, connection);
            }
            
            return new PaginatedResult<Message>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取消息列表失败");
            throw;
        }
    }

    public async Task<PaginatedResult<Message>> GetSentMessagesAsync(Guid userId, MessageFilterDto filter)
    {
        filter.SenderId = userId;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Message>> GetReceivedMessagesAsync(Guid userId, MessageFilterDto filter)
    {
        filter.ReceiverId = userId;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Message>> GetUnreadMessagesAsync(Guid userId, MessageFilterDto filter)
    {
        filter.ReceiverId = userId;
        filter.IsRead = false;
        return await GetPagedAsync(filter);
    }

    #endregion

    #region 会话管理

    public async Task<PaginatedResult<MessageConversationDto>> GetConversationsAsync(Guid userId, MessageConversationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildConversationWhereClause(filter);
            var orderByClause = BuildConversationOrderByClause(filter.SortBy);
            
            var countSql = $@"
                SELECT COUNT(DISTINCT c.Id) 
                FROM Conversations c
                JOIN ConversationParticipants cp ON c.Id = cp.ConversationId
                {whereClause}";
            
            var dataSql = $@"
                SELECT DISTINCT c.*, 
                       (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id AND DeletedAt IS NULL) as MessageCount,
                       (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id AND ReceiverId = @UserId AND IsRead = 0 AND DeletedAt IS NULL) as UnreadCount
                FROM Conversations c
                JOIN ConversationParticipants cp ON c.Id = cp.ConversationId
                {whereClause}
                {orderByClause}
                LIMIT @Limit OFFSET @Offset";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("Limit", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var totalCount = await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
            var conversations = await connection.QueryAsync<dynamic>(dataSql, parameters);
            
            var result = new List<MessageConversationDto>();
            foreach (var conv in conversations)
            {
                var conversationDto = new MessageConversationDto
                {
                    Id = conv.Id,
                    Title = conv.Title,
                    Description = conv.Description,
                    ConversationType = conv.ConversationType,
                    MessageCount = conv.MessageCount,
                    UnreadCount = conv.UnreadCount,
                    CreatedAt = conv.CreatedAt,
                    UpdatedAt = conv.UpdatedAt,
                    IsArchived = conv.IsArchived,
                    IsPinned = conv.IsPinned,
                    IsMuted = conv.IsMuted
                };
                
                // 加载参与者信息
                if (filter.IncludeParticipants)
                {
                    conversationDto.Participants = await GetConversationParticipantsAsync(conv.Id, connection);
                }
                
                // 加载最新消息
                if (filter.IncludeLastMessage)
                {
                    conversationDto.LastMessage = await GetLastMessageAsync(conv.Id, connection);
                }
                
                result.Add(conversationDto);
            }
            
            return new PaginatedResult<MessageConversationDto>
            {
                Items = result,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话列表失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<PaginatedResult<Message>> GetConversationMessagesAsync(Guid conversationId, MessageFilterDto filter)
    {
        filter.ConversationId = conversationId;
        return await GetPagedAsync(filter);
    }

    public async Task<Guid> CreateConversationAsync(IEnumerable<Guid> participants, ConversationType conversationType, string title, string? description = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conversationId = Guid.NewGuid();
            
            // 创建会话
            const string conversationSql = @"
                INSERT INTO Conversations (Id, Title, Description, ConversationType, CreatedAt, UpdatedAt, IsArchived, IsPinned, IsMuted)
                VALUES (@Id, @Title, @Description, @ConversationType, @CreatedAt, @UpdatedAt, @IsArchived, @IsPinned, @IsMuted)";
            
            await connection.ExecuteAsync(conversationSql, new
            {
                Id = conversationId,
                Title = title,
                Description = description,
                ConversationType = conversationType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsArchived = false,
                IsPinned = false,
                IsMuted = false
            });
            
            // 添加参与者
            const string participantSql = @"
                INSERT INTO ConversationParticipants (Id, ConversationId, UserId, Role, JoinedAt, LastReadAt, IsMuted, HasLeft)
                VALUES (@Id, @ConversationId, @UserId, @Role, @JoinedAt, @LastReadAt, @IsMuted, @HasLeft)";
            
            var participantTasks = participants.Select(async userId =>
            {
                await connection.ExecuteAsync(participantSql, new
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    UserId = userId,
                    Role = participants.First() == userId ? "Admin" : "Member",
                    JoinedAt = DateTime.UtcNow,
                    LastReadAt = DateTime.UtcNow,
                    IsMuted = false,
                    HasLeft = false
                });
            });
            
            await Task.WhenAll(participantTasks);
            
            return conversationId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建会话失败: {Title}", title);
            throw;
        }
    }

    public async Task<bool> UpdateConversationAsync(Guid conversationId, string title, string? description = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Conversations SET
                    Title = @Title,
                    Description = @Description,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                Id = conversationId,
                Title = title,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新会话失败: {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 检查用户是否是会话创建者
            const string checkSql = "SELECT COUNT(*) FROM ConversationParticipants WHERE ConversationId = @ConversationId AND UserId = @UserId AND Role = 'Admin'";
            var isAdmin = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { ConversationId = conversationId, UserId = userId });
            
            if (isAdmin == 0)
            {
                return false;
            }
            
            // 删除会话
            const string deleteSql = "DELETE FROM Conversations WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(deleteSql, new { Id = conversationId });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除会话失败: {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<bool> AddConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO ConversationParticipants (Id, ConversationId, UserId, Role, JoinedAt, LastReadAt, IsMuted, HasLeft)
                VALUES (@Id, @ConversationId, @UserId, @Role, @JoinedAt, @LastReadAt, @IsMuted, @HasLeft)";
            
            var tasks = userIds.Select(userId => connection.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                LastReadAt = DateTime.UtcNow,
                IsMuted = false,
                HasLeft = false
            }));
            
            var results = await Task.WhenAll(tasks);
            return results.All(r => r > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加会话参与者失败: {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<bool> RemoveConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM ConversationParticipants WHERE ConversationId = @ConversationId AND UserId IN @UserIds";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { ConversationId = conversationId, UserIds = userIds });
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除会话参与者失败: {ConversationId}", conversationId);
            throw;
        }
    }

    #endregion

    #region 回复管理

    public async Task<PaginatedResult<Message>> GetRepliesAsync(Guid messageId, MessageFilterDto filter)
    {
        filter.ParentId = messageId;
        return await GetPagedAsync(filter);
    }

    public async Task<IEnumerable<Message>> GetParentChainAsync(Guid messageId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                WITH RECURSIVE MessageChain AS (
                    SELECT * FROM Messages WHERE Id = @MessageId
                    UNION ALL
                    SELECT m.* FROM Messages m
                    JOIN MessageChain mc ON m.Id = mc.ParentId
                )
                SELECT * FROM MessageChain WHERE Id != @MessageId ORDER BY CreatedAt ASC";
            
            return await connection.QueryAsync<Message>(sql, new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取父消息链失败: {MessageId}", messageId);
            throw;
        }
    }

    public async Task<Message?> GetRootMessageAsync(Guid messageId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                WITH RECURSIVE MessageChain AS (
                    SELECT * FROM Messages WHERE Id = @MessageId
                    UNION ALL
                    SELECT m.* FROM Messages m
                    JOIN MessageChain mc ON m.Id = mc.ParentId
                )
                SELECT * FROM MessageChain WHERE ParentId IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<Message>(sql, new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取根消息失败: {MessageId}", messageId);
            throw;
        }
    }

    #endregion

    #region 搜索和筛选

    public async Task<IEnumerable<Message>> SearchMessagesAsync(string searchTerm, MessageFilterDto filter)
    {
        filter.Search = searchTerm;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<Message>> GetMessagesByStatusAsync(MessageStatus status, MessageFilterDto filter)
    {
        filter.Status = status;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<Message>> GetMessagesByTypeAsync(MessageType messageType, MessageFilterDto filter)
    {
        filter.MessageType = messageType;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<Message>> GetMessagesByPriorityAsync(MessagePriority priority, MessageFilterDto filter)
    {
        filter.Priority = priority;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<Message>> GetMessagesByTagAsync(string tag, MessageFilterDto filter)
    {
        filter.Tag = tag;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime startDate, DateTime endDate, MessageFilterDto filter)
    {
        filter.StartDate = startDate;
        filter.EndDate = endDate;
        var result = await GetPagedAsync(filter);
        return result.Items;
    }

    #endregion

    #region 状态管理

    public async Task<bool> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    IsRead = 1,
                    ReadAt = @Now,
                    UpdatedAt = @Now
                WHERE Id = @MessageId AND ReceiverId = @UserId AND IsRead = 0";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                MessageId = messageId,
                UserId = userId,
                Now = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记消息为已读失败: {MessageId}, {UserId}", messageId, userId);
            throw;
        }
    }

    public async Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> messageIds, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    IsRead = 1,
                    ReadAt = @Now,
                    UpdatedAt = @Now
                WHERE Id IN @MessageIds AND ReceiverId = @UserId AND IsRead = 0";
            
            return await connection.ExecuteAsync(sql, new
            {
                MessageIds = messageIds,
                UserId = userId,
                Now = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记消息为已读失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    IsRead = 1,
                    ReadAt = @Now,
                    UpdatedAt = @Now
                WHERE ConversationId = @ConversationId AND ReceiverId = @UserId AND IsRead = 0";
            
            return await connection.ExecuteAsync(sql, new
            {
                ConversationId = conversationId,
                UserId = userId,
                Now = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记会话消息为已读失败: {ConversationId}, {UserId}", conversationId, userId);
            throw;
        }
    }

    public async Task<bool> UpdateMessageStatusAsync(Guid messageId, MessageStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    Status = @Status,
                    UpdatedAt = @Now
                WHERE Id = @MessageId";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                MessageId = messageId,
                Status = status,
                Now = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新消息状态失败: {MessageId}, {Status}", messageId, status);
            throw;
        }
    }

    public async Task<int> UpdateMultipleMessageStatusAsync(IEnumerable<Guid> messageIds, MessageStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    Status = @Status,
                    UpdatedAt = @Now
                WHERE Id IN @MessageIds";
            
            return await connection.ExecuteAsync(sql, new
            {
                MessageIds = messageIds,
                Status = status,
                Now = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新消息状态失败: {Status}", status);
            throw;
        }
    }

    #endregion

    #region 统计信息

    public async Task<MessageStatsDto> GetUserMessageStatsAsync(Guid userId, MessageStatsFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildStatsWhereClause(filter);
            
            const string sql = $@"
                SELECT 
                    COUNT(*) as TotalMessages,
                    SUM(CASE WHEN SenderId = @UserId THEN 1 ELSE 0 END) as SentMessages,
                    SUM(CASE WHEN ReceiverId = @UserId THEN 1 ELSE 0 END) as ReceivedMessages,
                    SUM(CASE WHEN ReceiverId = @UserId AND IsRead = 0 THEN 1 ELSE 0 END) as UnreadMessages,
                    SUM(CASE WHEN ReceiverId = @UserId AND IsRead = 1 THEN 1 ELSE 0 END) as ReadMessages,
                    SUM(CASE WHEN ParentId IS NOT NULL THEN 1 ELSE 0 END) as RepliedMessages,
                    COUNT(DISTINCT CASE WHEN a.Id IS NOT NULL THEN 1 ELSE 0 END) as TotalAttachments,
                    COALESCE(SUM(a.FileSize), 0) as TotalAttachmentSize,
                    MAX(CASE WHEN ReceiverId = @UserId THEN CreatedAt ELSE NULL END) as LastMessageAt
                FROM Messages m
                LEFT JOIN MessageAttachments a ON m.Id = a.MessageId AND a.DeletedAt IS NULL
                {whereClause}";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            
            var stats = await connection.QuerySingleAsync<dynamic>(sql, parameters);
            
            return new MessageStatsDto
            {
                TotalMessages = stats.TotalMessages,
                SentMessages = stats.SentMessages,
                ReceivedMessages = stats.ReceivedMessages,
                UnreadMessages = stats.UnreadMessages,
                ReadMessages = stats.ReadMessages,
                RepliedMessages = stats.RepliedMessages,
                TotalAttachments = stats.TotalAttachments,
                TotalAttachmentSize = stats.TotalAttachmentSize,
                LastMessageAt = stats.LastMessageAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户消息统计失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<UserMessageStatsDto>> GetUsersMessageStatsAsync(IEnumerable<Guid> userIds, MessageStatsFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildStatsWhereClause(filter);
            
            const string sql = $@"
                SELECT 
                    u.Id as UserId,
                    u.Username as UserName,
                    COUNT(m.Id) as TotalMessages,
                    SUM(CASE WHEN m.SenderId = u.Id THEN 1 ELSE 0 END) as SentMessages,
                    SUM(CASE WHEN m.ReceiverId = u.Id THEN 1 ELSE 0 END) as ReceivedMessages,
                    SUM(CASE WHEN m.ReceiverId = u.Id AND m.IsRead = 0 THEN 1 ELSE 0 END) as UnreadMessages,
                    MAX(CASE WHEN m.ReceiverId = u.Id THEN m.CreatedAt ELSE NULL END) as LastMessageAt
                FROM Users u
                LEFT JOIN Messages m ON (m.SenderId = u.Id OR m.ReceiverId = u.Id) AND m.DeletedAt IS NULL
                WHERE u.Id IN @UserIds
                GROUP BY u.Id, u.Username
                ORDER BY TotalMessages DESC";
            
            return await connection.QueryAsync<UserMessageStatsDto>(sql, new { UserIds = userIds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取多用户消息统计失败");
            throw;
        }
    }

    public async Task<MessageStatsDto> GetConversationStatsAsync(Guid conversationId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalMessages,
                    COUNT(DISTINCT CASE WHEN a.Id IS NOT NULL THEN 1 ELSE 0 END) as TotalAttachments,
                    COALESCE(SUM(a.FileSize), 0) as TotalAttachmentSize,
                    MAX(CreatedAt) as LastMessageAt
                FROM Messages m
                LEFT JOIN MessageAttachments a ON m.Id = a.MessageId AND a.DeletedAt IS NULL
                WHERE m.ConversationId = @ConversationId AND m.DeletedAt IS NULL";
            
            var stats = await connection.QuerySingleAsync<dynamic>(sql, new { ConversationId = conversationId });
            
            return new MessageStatsDto
            {
                TotalMessages = stats.TotalMessages,
                TotalAttachments = stats.TotalAttachments,
                TotalAttachmentSize = stats.TotalAttachmentSize,
                LastMessageAt = stats.LastMessageAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话统计失败: {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM Messages WHERE ReceiverId = @UserId AND IsRead = 0 AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<int>(sql, new { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读消息数量失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetConversationUnreadCountAsync(Guid conversationId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM Messages WHERE ConversationId = @ConversationId AND ReceiverId = @UserId AND IsRead = 0 AND DeletedAt IS NULL";
            
            return await connection.QuerySingleOrDefaultAsync<int>(sql, new { ConversationId = conversationId, UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取会话未读消息数量失败: {ConversationId}, {UserId}", conversationId, userId);
            throw;
        }
    }

    public async Task<int> GetMessageCountAsync(MessageFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var whereClause = BuildWhereClause(filter);
            var countSql = $"SELECT COUNT(*) FROM Messages m {whereClause}";
            
            var parameters = BuildParameters(filter);
            return await connection.QuerySingleOrDefaultAsync<int>(countSql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息数量失败");
            throw;
        }
    }

    #endregion

    #region 权限和验证

    public async Task<bool> CanUserViewMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Messages 
                WHERE Id = @MessageId 
                AND (SenderId = @UserId OR ReceiverId = @UserId) 
                AND DeletedAt IS NULL";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { MessageId = messageId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户查看消息权限失败: {MessageId}, {UserId}", messageId, userId);
            throw;
        }
    }

    public async Task<bool> CanUserEditMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Messages 
                WHERE Id = @MessageId 
                AND SenderId = @UserId 
                AND DeletedAt IS NULL
                AND CreatedAt > DATE_SUB(NOW(), INTERVAL 24 HOUR)"; // 24小时内可以编辑
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { MessageId = messageId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户编辑消息权限失败: {MessageId}, {UserId}", messageId, userId);
            throw;
        }
    }

    public async Task<bool> CanUserDeleteMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Messages 
                WHERE Id = @MessageId 
                AND (SenderId = @UserId OR ReceiverId = @UserId) 
                AND DeletedAt IS NULL";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { MessageId = messageId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户删除消息权限失败: {MessageId}, {UserId}", messageId, userId);
            throw;
        }
    }

    public async Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM ConversationParticipants 
                WHERE ConversationId = @ConversationId 
                AND UserId = @UserId 
                AND HasLeft = 0";
            
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { ConversationId = conversationId, UserId = userId });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户是否在会话中失败: {ConversationId}, {UserId}", conversationId, userId);
            throw;
        }
    }

    #endregion

    #region 高级查询

    public async Task<IEnumerable<Message>> GetLatestMessagesAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Messages 
                WHERE (SenderId = @UserId OR ReceiverId = @UserId) 
                AND DeletedAt IS NULL
                ORDER BY CreatedAt DESC 
                LIMIT @Count";
            
            return await connection.QueryAsync<Message>(sql, new { UserId = userId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最新消息失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetHighPriorityUnreadMessagesAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Messages 
                WHERE ReceiverId = @UserId 
                AND IsRead = 0 
                AND Priority IN (2, 3) 
                AND DeletedAt IS NULL
                ORDER BY Priority DESC, CreatedAt ASC 
                LIMIT @Count";
            
            return await connection.QueryAsync<Message>(sql, new { UserId = userId, Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取高优先级未读消息失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetExpiringMessagesAsync(Guid userId, int hours = 24)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Messages 
                WHERE (SenderId = @UserId OR ReceiverId = @UserId) 
                AND ExpiresAt IS NOT NULL 
                AND ExpiresAt <= DATE_ADD(NOW(), INTERVAL @Hours HOUR)
                AND DeletedAt IS NULL
                ORDER BY ExpiresAt ASC";
            
            return await connection.QueryAsync<Message>(sql, new { UserId = userId, Hours = hours });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取即将过期消息失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetExpiredMessagesAsync(MessageFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM Messages 
                WHERE ExpiresAt IS NOT NULL 
                AND ExpiresAt < NOW()
                AND DeletedAt IS NULL";
            
            return await connection.QueryAsync<Message>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取已过期消息失败");
            throw;
        }
    }

    #endregion

    #region 缓存相关

    public async Task<Message?> GetFromCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接从数据库获取
        return await GetByIdAsync(id);
    }

    public async Task<bool> SetCacheAsync(Message message, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功
        return true;
    }

    public async Task<bool> RemoveCacheAsync(Guid id)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功
        return true;
    }

    public async Task<int> RemoveMultipleCacheAsync(IEnumerable<Guid> messageIds)
    {
        // 这里可以集成缓存服务，如Redis
        // 目前直接返回成功数量
        return messageIds.Count();
    }

    #endregion

    #region 批量操作

    public async Task<int> BulkInsertAsync(IEnumerable<Message> messages)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO Messages (
                    Id, SenderId, ReceiverId, Subject, Content, MessageType, Status, Priority,
                    ParentId, ConversationId, IsRead, ReadAt, Tag, CreatedAt, UpdatedAt,
                    DeletedAt, SenderDeletedAt, ReceiverDeletedAt, ExpiresAt, SenderIp, SenderUserAgent
                ) VALUES (
                    @Id, @SenderId, @ReceiverId, @Subject, @Content, @MessageType, @Status, @Priority,
                    @ParentId, @ConversationId, @IsRead, @ReadAt, @Tag, @CreatedAt, @UpdatedAt,
                    @DeletedAt, @SenderDeletedAt, @ReceiverDeletedAt, @ExpiresAt, @SenderIp, @SenderUserAgent
                )";
            
            return await connection.ExecuteAsync(sql, messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量插入消息失败");
            throw;
        }
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<Message> messages)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Messages SET
                    Subject = @Subject,
                    Content = @Content,
                    MessageType = @MessageType,
                    Status = @Status,
                    Priority = @Priority,
                    IsRead = @IsRead,
                    ReadAt = @ReadAt,
                    Tag = @Tag,
                    UpdatedAt = @UpdatedAt,
                    ExpiresAt = @ExpiresAt
                WHERE Id = @Id";
            
            return await connection.ExecuteAsync(sql, messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新消息失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> messageIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Messages WHERE Id IN @MessageIds";
            
            return await connection.ExecuteAsync(sql, new { MessageIds = messageIds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除消息失败");
            throw;
        }
    }

    public async Task<int> BulkSoftDeleteAsync(IEnumerable<Guid> messageIds, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE Messages SET
                    DeletedAt = @Now,
                    CASE WHEN SenderId = @UserId THEN SenderDeletedAt = @Now ELSE SenderDeletedAt END,
                    CASE WHEN ReceiverId = @UserId THEN ReceiverDeletedAt = @Now ELSE ReceiverDeletedAt END
                WHERE Id IN @MessageIds AND DeletedAt IS NULL";
            
            return await connection.ExecuteAsync(sql, new
            {
                MessageIds = messageIds,
                UserId = userId,
                Now = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量软删除消息失败: {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    private string BuildWhereClause(MessageFilterDto filter)
    {
        var whereClause = new StringBuilder("WHERE m.DeletedAt IS NULL");
        
        if (filter.SenderId.HasValue)
            whereClause.Append(" AND m.SenderId = @SenderId");
        
        if (filter.ReceiverId.HasValue)
            whereClause.Append(" AND m.ReceiverId = @ReceiverId");
        
        if (filter.MessageType.HasValue)
            whereClause.Append(" AND m.MessageType = @MessageType");
        
        if (filter.Status.HasValue)
            whereClause.Append(" AND m.Status = @Status");
        
        if (filter.Priority.HasValue)
            whereClause.Append(" AND m.Priority = @Priority");
        
        if (filter.ParentId.HasValue)
            whereClause.Append(filter.ParentId == null ? " AND m.ParentId IS NULL" : " AND m.ParentId = @ParentId");
        
        if (filter.ConversationId.HasValue)
            whereClause.Append(" AND m.ConversationId = @ConversationId");
        
        if (!string.IsNullOrEmpty(filter.Tag))
            whereClause.Append(" AND m.Tag = @Tag");
        
        if (filter.IsRead.HasValue)
            whereClause.Append($" AND m.IsRead = {(filter.IsRead.Value ? 1 : 0)}");
        
        if (filter.IsDeleted.HasValue)
        {
            if (filter.IsDeleted.Value)
                whereClause.Append(" AND m.DeletedAt IS NOT NULL");
            else
                whereClause.Append(" AND m.DeletedAt IS NULL");
        }
        
        if (filter.StartDate.HasValue)
            whereClause.Append(" AND m.CreatedAt >= @StartDate");
        
        if (filter.EndDate.HasValue)
            whereClause.Append(" AND m.CreatedAt <= @EndDate");
        
        if (!string.IsNullOrEmpty(filter.Search))
            whereClause.Append(" AND (m.Subject LIKE @Search OR m.Content LIKE @Search)");
        
        return whereClause.ToString();
    }

    private string BuildOrderByClause(MessageSort sortBy)
    {
        return sortBy switch
        {
            MessageSort.CreatedAtAsc => "ORDER BY m.CreatedAt ASC",
            MessageSort.PriorityDesc => "ORDER BY m.Priority DESC, m.CreatedAt DESC",
            MessageSort.PriorityAsc => "ORDER BY m.Priority ASC, m.CreatedAt ASC",
            MessageSort.Status => "ORDER BY m.Status ASC, m.CreatedAt DESC",
            MessageSort.Subject => "ORDER BY m.Subject ASC, m.CreatedAt DESC",
            MessageSort.MessageType => "ORDER BY m.MessageType ASC, m.CreatedAt DESC",
            MessageSort.UnreadFirst => "ORDER BY m.IsRead ASC, m.CreatedAt DESC",
            MessageSort.PriorityAndUnreadFirst => "ORDER BY m.Priority DESC, m.IsRead ASC, m.CreatedAt DESC",
            _ => "ORDER BY m.CreatedAt DESC"
        };
    }

    private string BuildConversationWhereClause(MessageConversationFilterDto filter)
    {
        var whereClause = new StringBuilder("WHERE cp.UserId = @UserId");
        
        if (filter.ConversationType.HasValue)
            whereClause.Append(" AND c.ConversationType = @ConversationType");
        
        if (filter.StartDate.HasValue)
            whereClause.Append(" AND c.CreatedAt >= @StartDate");
        
        if (filter.EndDate.HasValue)
            whereClause.Append(" AND c.CreatedAt <= @EndDate");
        
        if (filter.HasUnread.HasValue)
        {
            if (filter.HasUnread.Value)
                whereClause.Append(" AND (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id AND ReceiverId = @UserId AND IsRead = 0) > 0");
            else
                whereClause.Append(" AND (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id AND ReceiverId = @UserId AND IsRead = 0) = 0");
        }
        
        if (filter.MinMessageCount.HasValue)
            whereClause.Append(" AND (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id) >= @MinMessageCount");
        
        if (!string.IsNullOrEmpty(filter.Search))
            whereClause.Append(" AND (c.Title LIKE @Search OR c.Description LIKE @Search)");
        
        return whereClause.ToString();
    }

    private string BuildConversationOrderByClause(ConversationSort sortBy)
    {
        return sortBy switch
        {
            ConversationSort.LastMessageAtAsc => "ORDER BY (SELECT MAX(CreatedAt) FROM Messages WHERE ConversationId = c.Id) ASC",
            ConversationSort.CreatedAtDesc => "ORDER BY c.CreatedAt DESC",
            ConversationSort.CreatedAtAsc => "ORDER BY c.CreatedAt ASC",
            ConversationSort.MessageCountDesc => "ORDER BY (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id) DESC",
            ConversationSort.UnreadCountDesc => "ORDER BY (SELECT COUNT(*) FROM Messages WHERE ConversationId = c.Id AND ReceiverId = @UserId AND IsRead = 0) DESC",
            _ => "ORDER BY (SELECT MAX(CreatedAt) FROM Messages WHERE ConversationId = c.Id) DESC"
        };
    }

    private string BuildStatsWhereClause(MessageStatsFilterDto filter)
    {
        var whereClause = new StringBuilder("WHERE m.DeletedAt IS NULL");
        
        if (filter.UserIds != null && filter.UserIds.Any())
            whereClause.Append(" AND (m.SenderId IN @UserIds OR m.ReceiverId IN @UserIds)");
        
        if (filter.MessageType.HasValue)
            whereClause.Append(" AND m.MessageType = @MessageType");
        
        if (filter.Status.HasValue)
            whereClause.Append(" AND m.Status = @Status");
        
        if (filter.StartDate.HasValue)
            whereClause.Append(" AND m.CreatedAt >= @StartDate");
        
        if (filter.EndDate.HasValue)
            whereClause.Append(" AND m.CreatedAt <= @EndDate");
        
        return whereClause.ToString();
    }

    private DynamicParameters BuildParameters(MessageFilterDto filter)
    {
        var parameters = new DynamicParameters();
        
        if (filter.SenderId.HasValue)
            parameters.Add("SenderId", filter.SenderId.Value);
        
        if (filter.ReceiverId.HasValue)
            parameters.Add("ReceiverId", filter.ReceiverId.Value);
        
        if (filter.MessageType.HasValue)
            parameters.Add("MessageType", filter.MessageType.Value);
        
        if (filter.Status.HasValue)
            parameters.Add("Status", filter.Status.Value);
        
        if (filter.Priority.HasValue)
            parameters.Add("Priority", filter.Priority.Value);
        
        if (filter.ParentId.HasValue)
            parameters.Add("ParentId", filter.ParentId.Value);
        
        if (filter.ConversationId.HasValue)
            parameters.Add("ConversationId", filter.ConversationId.Value);
        
        if (!string.IsNullOrEmpty(filter.Tag))
            parameters.Add("Tag", filter.Tag);
        
        if (filter.StartDate.HasValue)
            parameters.Add("StartDate", filter.StartDate.Value);
        
        if (filter.EndDate.HasValue)
            parameters.Add("EndDate", filter.EndDate.Value);
        
        if (!string.IsNullOrEmpty(filter.Search))
            parameters.Add("Search", $"%{filter.Search}%");
        
        return parameters;
    }

    private async Task<List<Message>> LoadRelatedDataAsync(List<Message> messages, MessageFilterDto filter, IDbConnection connection)
    {
        if (!messages.Any())
            return messages;

        var messageIds = messages.Select(m => m.Id).ToList();

        // 加载发送者信息
        if (filter.IncludeSender)
        {
            const string senderSql = "SELECT * FROM Users WHERE Id IN @UserIds";
            var senders = await connection.QueryAsync<User>(senderSql, new { UserIds = messages.Select(m => m.SenderId).Distinct() });
            
            foreach (var message in messages)
            {
                message.Sender = senders.FirstOrDefault(s => s.Id == message.SenderId);
            }
        }

        // 加载接收者信息
        if (filter.IncludeReceiver)
        {
            const string receiverSql = "SELECT * FROM Users WHERE Id IN @UserIds";
            var receivers = await connection.QueryAsync<User>(receiverSql, new { UserIds = messages.Select(m => m.ReceiverId).Distinct() });
            
            foreach (var message in messages)
            {
                message.Receiver = receivers.FirstOrDefault(r => r.Id == message.ReceiverId);
            }
        }

        // 加载附件信息
        if (filter.IncludeAttachments)
        {
            const string attachmentSql = "SELECT * FROM MessageAttachments WHERE MessageId IN @MessageIds AND DeletedAt IS NULL";
            var attachments = await connection.QueryAsync<MessageAttachment>(attachmentSql, new { MessageIds = messageIds });
            
            foreach (var message in messages)
            {
                message.Attachments = attachments.Where(a => a.MessageId == message.Id).ToList();
            }
        }

        // 加载回复信息
        if (filter.IncludeReplies)
        {
            const string repliesSql = "SELECT * FROM Messages WHERE ParentId IN @MessageIds AND DeletedAt IS NULL ORDER BY CreatedAt ASC";
            var replies = await connection.QueryAsync<Message>(repliesSql, new { MessageIds = messageIds });
            
            foreach (var message in messages)
            {
                message.Replies = replies.Where(r => r.ParentId == message.Id).ToList();
            }
        }

        return messages;
    }

    private async Task<List<ConversationParticipantDto>> GetConversationParticipantsAsync(Guid conversationId, IDbConnection connection)
    {
        const string sql = @"
            SELECT cp.*, u.Username, u.Email, u.IsActive
            FROM ConversationParticipants cp
            LEFT JOIN Users u ON cp.UserId = u.Id
            WHERE cp.ConversationId = @ConversationId AND cp.HasLeft = 0
            ORDER BY cp.JoinedAt ASC";
        
        var participants = await connection.QueryAsync<dynamic>(sql, new { ConversationId = conversationId });
        
        return participants.Select(p => new ConversationParticipantDto
        {
            UserId = p.UserId,
            UserName = p.Username,
            Role = p.Role,
            JoinedAt = p.JoinedAt,
            LastReadAt = p.LastReadAt,
            IsMuted = p.IsMuted,
            HasLeft = p.HasLeft
        }).ToList();
    }

    private async Task<MessageDto?> GetLastMessageAsync(Guid conversationId, IDbConnection connection)
    {
        const string sql = @"
            SELECT m.*, 
                   s.Username as SenderName, s.Email as SenderEmail,
                   r.Username as ReceiverName, r.Email as ReceiverEmail
            FROM Messages m
            LEFT JOIN Users s ON m.SenderId = s.Id
            LEFT JOIN Users r ON m.ReceiverId = r.Id
            WHERE m.ConversationId = @ConversationId AND m.DeletedAt IS NULL
            ORDER BY m.CreatedAt DESC
            LIMIT 1";
        
        return await connection.QuerySingleOrDefaultAsync<MessageDto>(sql, new { ConversationId = conversationId });
    }

    #endregion
}