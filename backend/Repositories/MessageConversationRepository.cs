using CodeSnippetManager.Api.Data;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using Dapper;
using System.Data;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 消息会话仓储实现 - 使用Dapper ORM
/// </summary>
public class MessageConversationRepository : IMessageConversationRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<MessageConversationRepository> _logger;

    public MessageConversationRepository(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<MessageConversationRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<MessageConversation> CreateAsync(MessageConversation conversation)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO MessageConversations (
                    Id, Title, Description, CreatorId, CreatedAt, UpdatedAt,
                    LastMessageId, LastMessageTime, IsDeleted, ParticipantCount
                ) VALUES (
                    @Id, @Title, @Description, @CreatorId, @CreatedAt, @UpdatedAt,
                    @LastMessageId, @LastMessageTime, @IsDeleted, @ParticipantCount
                )";

            await connection.ExecuteAsync(sql, conversation);
            _logger.LogInformation($"创建会话成功: {conversation.Id}");
            return conversation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"创建会话失败: {conversation.Id}");
            throw;
        }
    }

    public async Task<MessageConversation?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT * FROM MessageConversations 
                WHERE Id = @Id AND IsDeleted = 0";

            var conversation = await connection.QueryFirstOrDefaultAsync<MessageConversation>(sql, new { Id = id });
            return conversation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取会话失败: {id}");
            throw;
        }
    }

    public async Task<IEnumerable<MessageConversation>> GetByParticipantsAsync(IEnumerable<Guid> participantIds)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT DISTINCT mc.* FROM MessageConversations mc
                INNER JOIN MessageConversationParticipants mcp ON mc.Id = mcp.ConversationId
                WHERE mcp.ParticipantId IN @ParticipantIds AND mc.IsDeleted = 0
                ORDER BY mc.LastMessageTime DESC";

            var conversations = await connection.QueryAsync<MessageConversation>(sql, new { ParticipantIds = participantIds });
            return conversations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据参与者获取会话失败");
            throw;
        }
    }

    public async Task<(IEnumerable<MessageConversation> Conversations, int TotalCount)> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string countSql = @"
                SELECT COUNT(DISTINCT mc.Id) FROM MessageConversations mc
                INNER JOIN MessageConversationParticipants mcp ON mc.Id = mcp.ConversationId
                WHERE mcp.ParticipantId = @UserId AND mc.IsDeleted = 0";

            const string dataSql = @"
                SELECT DISTINCT mc.* FROM MessageConversations mc
                INNER JOIN MessageConversationParticipants mcp ON mc.Id = mcp.ConversationId
                WHERE mcp.ParticipantId = @UserId AND mc.IsDeleted = 0
                ORDER BY mc.LastMessageTime DESC
                LIMIT @Offset, @PageSize";

            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId });
            var offset = (page - 1) * pageSize;
            var conversations = await connection.QueryAsync<MessageConversation>(dataSql, new { UserId = userId, Offset = offset, PageSize = pageSize });

            return (conversations, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取用户会话失败: {userId}");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(MessageConversation conversation)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageConversations SET
                    Title = @Title,
                    Description = @Description,
                    UpdatedAt = @UpdatedAt,
                    LastMessageId = @LastMessageId,
                    LastMessageTime = @LastMessageTime,
                    ParticipantCount = @ParticipantCount
                WHERE Id = @Id AND IsDeleted = 0";

            var affectedRows = await connection.ExecuteAsync(sql, conversation);
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新会话失败: {conversation.Id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageConversations 
                SET IsDeleted = 1, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"删除会话失败: {id}");
            throw;
        }
    }

    public async Task<bool> AddParticipantsAsync(Guid conversationId, IEnumerable<Guid> participantIds)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"
                    INSERT INTO MessageConversationParticipants (
                        Id, ConversationId, ParticipantId, JoinedAt, IsActive
                    ) VALUES ";

                var parameters = new DynamicParameters();
                var values = new List<string>();
                var index = 0;

                foreach (var participantId in participantIds)
                {
                    var paramId = $"participant_{index}";
                    values.Add($"(@{paramId}_id, @{paramId}_conversationId, @{paramId}_participantId, @{paramId}_joinedAt, 1)");
                    
                    parameters.Add($"{paramId}_id", Guid.NewGuid());
                    parameters.Add($"{paramId}_conversationId", conversationId);
                    parameters.Add($"{paramId}_participantId", participantId);
                    parameters.Add($"{paramId}_joinedAt", DateTime.UtcNow);
                    
                    index++;
                }

                var fullSql = sql + string.Join(", ", values);
                await connection.ExecuteAsync(fullSql, parameters, transaction);

                // 更新会话参与者数量
                const string updateCountSql = @"
                    UPDATE MessageConversations 
                    SET ParticipantCount = (
                        SELECT COUNT(*) FROM MessageConversationParticipants 
                        WHERE ConversationId = @ConversationId AND IsActive = 1
                    ),
                    UpdatedAt = @UpdatedAt
                    WHERE Id = @ConversationId";

                await connection.ExecuteAsync(updateCountSql, 
                    new { ConversationId = conversationId, UpdatedAt = DateTime.UtcNow }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"添加参与者失败: 会话 {conversationId}");
            throw;
        }
    }

    public async Task<bool> RemoveParticipantAsync(Guid conversationId, Guid participantId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string sql = @"
                    UPDATE MessageConversationParticipants 
                    SET IsActive = 0, LeftAt = @LeftAt
                    WHERE ConversationId = @ConversationId AND ParticipantId = @ParticipantId";

                await connection.ExecuteAsync(sql, 
                    new { ConversationId = conversationId, ParticipantId = participantId, LeftAt = DateTime.UtcNow }, transaction);

                // 更新会话参与者数量
                const string updateCountSql = @"
                    UPDATE MessageConversations 
                    SET ParticipantCount = (
                        SELECT COUNT(*) FROM MessageConversationParticipants 
                        WHERE ConversationId = @ConversationId AND IsActive = 1
                    ),
                    UpdatedAt = @UpdatedAt
                    WHERE Id = @ConversationId";

                await connection.ExecuteAsync(updateCountSql, 
                    new { ConversationId = conversationId, UpdatedAt = DateTime.UtcNow }, transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"移除参与者失败: 会话 {conversationId}, 用户 {participantId}");
            throw;
        }
    }

    public async Task<bool> UpdateLastMessageAsync(Guid conversationId, Guid lastMessageId, DateTime lastMessageTime)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                UPDATE MessageConversations 
                SET LastMessageId = @LastMessageId, LastMessageTime = @LastMessageTime, UpdatedAt = @UpdatedAt
                WHERE Id = @ConversationId";

            var affectedRows = await connection.ExecuteAsync(sql, new 
            { 
                ConversationId = conversationId, 
                LastMessageId = lastMessageId, 
                LastMessageTime = lastMessageTime,
                UpdatedAt = DateTime.UtcNow
            });
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新最后消息失败: 会话 {conversationId}");
            throw;
        }
    }

    public async Task<int> GetMessageCountAsync(Guid conversationId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Messages 
                WHERE ConversationId = @ConversationId AND IsDeleted = 0";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { ConversationId = conversationId });
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取消息数量失败: 会话 {conversationId}");
            throw;
        }
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid conversationId, Guid userId)
    {
        try
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM Messages m
                LEFT JOIN MessageReadStatus mrs ON m.Id = mrs.MessageId AND mrs.UserId = @UserId
                WHERE m.ConversationId = @ConversationId 
                AND m.IsDeleted = 0 
                AND m.SenderId != @UserId
                AND (mrs.Id IS NULL OR mrs.IsRead = 0)";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { ConversationId = conversationId, UserId = userId });
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取未读消息数量失败: 会话 {conversationId}, 用户 {userId}");
            throw;
        }
    }
}