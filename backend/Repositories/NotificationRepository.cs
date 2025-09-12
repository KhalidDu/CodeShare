using Dapper;
using System.Text.Json;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using System.Data;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 通知仓储实现 - 遵循单一职责原则，负责通知的数据访问
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NotificationRepository> _logger;
    private readonly bool _isSqlite;

    public NotificationRepository(IDbConnectionFactory connectionFactory, ILogger<NotificationRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _isSqlite = connectionFactory.GetType().Name.Contains("Sqlite");
    }

    /// <summary>
    /// 处理数据库类型转换 - SQLite字符串转Guid
    /// </summary>
    private object HandleIdConversion(Guid id)
    {
        return _isSqlite ? id.ToString() : id;
    }

    /// <summary>
    /// 处理查询结果类型转换 - SQLite字符串转Guid
    /// </summary>
    private void HandleIdConversion(Notification notification)
    {
        // SQLite中ID已经是Guid类型，无需转换
        return;
    }

    /// <summary>
    /// 处理SQLite日期时间转换
    /// </summary>
    private DateTime ParseDateTime(object dateTimeValue)
    {
        if (_isSqlite)
        {
            // SQLite返回的是字符串格式的日期时间
            if (dateTimeValue is string dateString)
            {
                return DateTime.Parse(dateString);
            }
        }
        
        // MySQL直接返回DateTime类型
        return Convert.ToDateTime(dateTimeValue);
    }

    /// <summary>
    /// 处理SQLite布尔值转换
    /// </summary>
    private bool ParseBool(object boolValue)
    {
        if (_isSqlite)
        {
            // SQLite中布尔值可能以整数形式存储 (0 或 1)
            if (boolValue is long longValue)
            {
                return longValue != 0;
            }
            if (boolValue is int intValue)
            {
                return intValue != 0;
            }
        }
        
        // MySQL直接返回bool类型
        return Convert.ToBoolean(boolValue);
    }

    /// <summary>
    /// 处理SQLite整数转换
    /// </summary>
    private int ParseInt(object intValue)
    {
        if (_isSqlite)
        {
            // SQLite中整数可能以long形式返回
            if (intValue is long longValue)
            {
                return (int)longValue;
            }
        }
        
        // MySQL直接返回int类型
        return Convert.ToInt32(intValue);
    }

    /// <summary>
    /// 处理SQLite TimeSpan转换
    /// </summary>
    private TimeSpan? ParseTimeSpan(object timeSpanValue)
    {
        if (timeSpanValue == null || timeSpanValue == DBNull.Value)
            return null;

        if (_isSqlite)
        {
            // SQLite中TimeSpan可能以字符串形式存储
            if (timeSpanValue is string timeSpanString)
            {
                return TimeSpan.Parse(timeSpanString);
            }
        }
        
        // MySQL直接返回TimeSpan类型
        return (TimeSpan)timeSpanValue;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            if (_isSqlite)
            {
                // SQLite专用查询 - 处理字符串ID
                const string sql = @"
                    SELECT n.*, u.Username as UserName,
                           tu.Username as TriggeredByUserName
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.Id = @Id AND n.IsDeleted = 0";
                
                var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });
                
                if (result == null)
                    return null;
                    
                return MapToNotification(result);
            }
            else
            {
                // MySQL查询 - 使用Dapper自动映射
                const string sql = @"
                    SELECT n.*, u.Username as UserName,
                           tu.Username as TriggeredByUserName
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.Id = @Id AND n.IsDeleted = 0";
                
                var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
                
                if (result == null)
                    return null;
                    
                return MapToNotification(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<Notification?> GetByIdWithUserAsync(Guid id)
    {
        // GetByIdAsync 已经包含用户信息，直接调用
        return await GetByIdAsync(id);
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        // DeleteAsync 已经是软删除，直接调用
        return await DeleteAsync(id);
    }

    public async Task<PaginatedResult<Notification>> GetByUserIdAsync(Guid userId, NotificationFilterDto filter)
    {
        filter.UserId = userId;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Notification>> GetByTypeAsync(NotificationType type, NotificationFilterDto filter)
    {
        filter.Type = type;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Notification>> GetByRelatedEntityAsync(RelatedEntityType entityType, string entityId, NotificationFilterDto filter)
    {
        filter.RelatedEntityType = entityType;
        filter.RelatedEntityId = entityId;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Notification>> GetByStatusAsync(NotificationStatus status, NotificationFilterDto filter)
    {
        filter.Status = status;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Notification>> GetUnreadAsync(Guid userId, NotificationFilterDto filter)
    {
        filter.UserId = userId;
        filter.IsRead = false;
        return await GetPagedAsync(filter);
    }

    public async Task<PaginatedResult<Notification>> GetPagedAsync(NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            // 构建查询条件
            if (filter.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", filter.UserId.Value);
            }
            
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", filter.Status.Value);
            }
            
            if (filter.RelatedEntityType.HasValue)
            {
                conditions.Add("n.RelatedEntityType = @RelatedEntityType");
                parameters.Add("RelatedEntityType", filter.RelatedEntityType.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.RelatedEntityId))
            {
                conditions.Add("n.RelatedEntityId = @RelatedEntityId");
                parameters.Add("RelatedEntityId", filter.RelatedEntityId);
            }
            
            if (filter.TriggeredByUserId.HasValue)
            {
                conditions.Add("n.TriggeredByUserId = @TriggeredByUserId");
                parameters.Add("TriggeredByUserId", filter.TriggeredByUserId.Value);
            }
            
            if (filter.Action.HasValue)
            {
                conditions.Add("n.Action = @Action");
                parameters.Add("Action", filter.Action.Value);
            }
            
            if (filter.Channel.HasValue)
            {
                conditions.Add("n.Channel = @Channel");
                parameters.Add("Channel", filter.Channel.Value);
            }
            
            if (filter.IsRead.HasValue)
            {
                conditions.Add("n.IsRead = @IsRead");
                parameters.Add("IsRead", filter.IsRead.Value);
            }
            
            if (filter.IsArchived.HasValue)
            {
                conditions.Add("n.IsArchived = @IsArchived");
                parameters.Add("IsArchived", filter.IsArchived.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                conditions.Add("n.Tag = @Tag");
                parameters.Add("Tag", filter.Tag);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            if (filter.ExpiresBefore.HasValue)
            {
                conditions.Add("n.ExpiresAt <= @ExpiresBefore");
                parameters.Add("ExpiresBefore", filter.ExpiresBefore.Value);
            }
            
            if (filter.ExpiresAfter.HasValue)
            {
                conditions.Add("n.ExpiresAt >= @ExpiresAfter");
                parameters.Add("ExpiresAfter", filter.ExpiresAfter.Value);
            }
            
            if (filter.RequiresConfirmation.HasValue)
            {
                conditions.Add("n.RequiresConfirmation = @RequiresConfirmation");
                parameters.Add("RequiresConfirmation", filter.RequiresConfirmation.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Search))
            {
                conditions.Add("(n.Title LIKE @Search OR n.Content LIKE @Search OR n.Message LIKE @Search)");
                parameters.Add("Search", $"%{filter.Search}%");
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 获取总数
            var countSql = $"SELECT COUNT(DISTINCT n.Id) FROM Notifications n WHERE {whereClause}";
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            // 获取分页数据
            var offset = (filter.Page - 1) * filter.PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", filter.PageSize);

            // 构建排序
            var sortBy = filter.Sort switch
            {
                NotificationSort.CreatedAtAsc => "n.CreatedAt ASC",
                NotificationSort.CreatedAtDesc => "n.CreatedAt DESC",
                NotificationSort.UpdatedAtAsc => "n.UpdatedAt ASC",
                NotificationSort.UpdatedAtDesc => "n.UpdatedAt DESC",
                NotificationSort.PriorityAsc => "n.Priority ASC",
                NotificationSort.PriorityDesc => "n.Priority DESC",
                NotificationSort.ReadAtAsc => "n.ReadAt ASC",
                NotificationSort.ReadAtDesc => "n.ReadAt DESC",
                NotificationSort.TitleAsc => "n.Title ASC",
                NotificationSort.TitleDesc => "n.Title DESC",
                NotificationSort.TypeAsc => "n.Type ASC",
                NotificationSort.TypeDesc => "n.Type DESC",
                _ => "n.CreatedAt DESC"
            };

            var dataSql = $@"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE {whereClause}
                ORDER BY {sortBy}
                LIMIT @PageSize OFFSET @Offset";

            var result = await connection.QueryAsync<dynamic>(dataSql, parameters);
            
            var notifications = result.Select(MapToNotification).ToList();
            
            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知列表失败");
            throw;
        }
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO Notifications (Id, UserId, Type, Title, Content, Message, Priority, Status, 
                    RelatedEntityType, RelatedEntityId, TriggeredByUserId, Action, Channel, IsRead, ReadAt, 
                    DeliveredAt, CreatedAt, UpdatedAt, ExpiresAt, ScheduledToSendAt, SendCount, LastSentAt, 
                    ErrorMessage, DataJson, Tag, Icon, Color, RequiresConfirmation, ConfirmedAt, 
                    IsArchived, ArchivedAt, IsDeleted, DeletedAt)
                VALUES (@Id, @UserId, @Type, @Title, @Content, @Message, @Priority, @Status, 
                    @RelatedEntityType, @RelatedEntityId, @TriggeredByUserId, @Action, @Channel, @IsRead, @ReadAt, 
                    @DeliveredAt, @CreatedAt, @UpdatedAt, @ExpiresAt, @ScheduledToSendAt, @SendCount, @LastSentAt, 
                    @ErrorMessage, @DataJson, @Tag, @Icon, @Color, @RequiresConfirmation, @ConfirmedAt, 
                    @IsArchived, @ArchivedAt, @IsDeleted, @DeletedAt)";
            
            var parameters = new
            {
                Id = _isSqlite ? (object)notification.Id.ToString() : notification.Id,
                UserId = _isSqlite ? (object)notification.UserId.ToString() : notification.UserId,
                notification.Type,
                notification.Title,
                notification.Content,
                notification.Message,
                notification.Priority,
                notification.Status,
                RelatedEntityType = (int?)notification.RelatedEntityType,
                notification.RelatedEntityId,
                TriggeredByUserId = _isSqlite ? (object?)notification.TriggeredByUserId?.ToString() : notification.TriggeredByUserId,
                Action = (int?)notification.Action,
                notification.Channel,
                notification.IsRead,
                notification.ReadAt,
                notification.DeliveredAt,
                notification.CreatedAt,
                notification.UpdatedAt,
                notification.ExpiresAt,
                notification.ScheduledToSendAt,
                notification.SendCount,
                notification.LastSentAt,
                notification.ErrorMessage,
                notification.DataJson,
                notification.Tag,
                notification.Icon,
                notification.Color,
                notification.RequiresConfirmation,
                notification.ConfirmedAt,
                notification.IsArchived,
                notification.ArchivedAt,
                notification.IsDeleted,
                notification.DeletedAt
            };
            
            await connection.ExecuteAsync(sql, parameters);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知失败: {NotificationTitle}", notification.Title);
            throw;
        }
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET Title = @Title, Content = @Content, Message = @Message, Priority = @Priority, 
                    Status = @Status, RelatedEntityType = @RelatedEntityType, RelatedEntityId = @RelatedEntityId, 
                    Action = @Action, Channel = @Channel, IsRead = @IsRead, ReadAt = @ReadAt, 
                    DeliveredAt = @DeliveredAt, UpdatedAt = @UpdatedAt, ExpiresAt = @ExpiresAt, 
                    ScheduledToSendAt = @ScheduledToSendAt, SendCount = @SendCount, LastSentAt = @LastSentAt, 
                    ErrorMessage = @ErrorMessage, DataJson = @DataJson, Tag = @Tag, Icon = @Icon, 
                    Color = @Color, RequiresConfirmation = @RequiresConfirmation, ConfirmedAt = @ConfirmedAt, 
                    IsArchived = @IsArchived, ArchivedAt = @ArchivedAt, IsDeleted = @IsDeleted, 
                    DeletedAt = @DeletedAt
                WHERE Id = @Id";
            
            var parameters = new
            {
                Id = _isSqlite ? (object)notification.Id.ToString() : notification.Id,
                notification.Title,
                notification.Content,
                notification.Message,
                notification.Priority,
                notification.Status,
                RelatedEntityType = (int?)notification.RelatedEntityType,
                notification.RelatedEntityId,
                Action = (int?)notification.Action,
                notification.Channel,
                notification.IsRead,
                notification.ReadAt,
                notification.DeliveredAt,
                notification.UpdatedAt,
                notification.ExpiresAt,
                notification.ScheduledToSendAt,
                notification.SendCount,
                notification.LastSentAt,
                notification.ErrorMessage,
                notification.DataJson,
                notification.Tag,
                notification.Icon,
                notification.Color,
                notification.RequiresConfirmation,
                notification.ConfirmedAt,
                notification.IsArchived,
                notification.ArchivedAt,
                notification.IsDeleted,
                notification.DeletedAt
            };
            
            await connection.ExecuteAsync(sql, parameters);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知失败: {NotificationId}", notification.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 软删除
            const string sql = @"
                UPDATE Notifications 
                SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                DeletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM Notifications 
                WHERE UserId = @UserId AND IsRead = 0 AND IsDeleted = 0";
            
            var userIdParam = _isSqlite ? (object)userId.ToString() : userId;
            return await connection.QuerySingleAsync<int>(sql, new { UserId = userIdParam });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读通知数量失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            if (_isSqlite)
            {
                const string sql = @"
                    SELECT n.*, u.Username as UserName, u.Avatar as UserAvatar,
                           tu.Username as TriggeredByUserName, tu.Avatar as TriggeredByUserAvatar
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.UserId = @UserId AND n.IsDeleted = 0
                    ORDER BY n.CreatedAt DESC
                    LIMIT @Count";
                
                var result = await connection.QueryAsync<dynamic>(sql, new 
                { 
                    UserId = userId.ToString(), 
                    Count = count 
                });
                
                return result.Select(MapToNotification).ToList();
            }
            else
            {
                const string sql = @"
                    SELECT n.*, u.Username as UserName, u.Avatar as UserAvatar,
                           tu.Username as TriggeredByUserName, tu.Avatar as TriggeredByUserAvatar
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.UserId = @UserId AND n.IsDeleted = 0
                    ORDER BY n.CreatedAt DESC
                    LIMIT @Count";
                
                var result = await connection.QueryAsync<dynamic>(sql, new 
                { 
                    UserId = userId, 
                    Count = count 
                });
                
                return result.Select(MapToNotification).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByTypeAsync(NotificationType type, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Type = @Type AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Type = type, Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取类型通知失败: {Type}", type);
            throw;
        }
    }

    public async Task<List<Notification>> GetByPriorityAsync(NotificationPriority priority, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Priority = @Priority AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Priority = priority, Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取优先级通知失败: {Priority}", priority);
            throw;
        }
    }

    public async Task<List<Notification>> GetUnreadAsync(Guid userId, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            if (_isSqlite)
            {
                const string sql = @"
                    SELECT n.*, u.Username as UserName, u.Avatar as UserAvatar,
                           tu.Username as TriggeredByUserName, tu.Avatar as TriggeredByUserAvatar
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.UserId = @UserId AND n.IsRead = 0 AND n.IsDeleted = 0
                    ORDER BY n.CreatedAt DESC
                    LIMIT @Count";
                
                var result = await connection.QueryAsync<dynamic>(sql, new 
                { 
                    UserId = userId.ToString(), 
                    Count = count 
                });
                
                return result.Select(MapToNotification).ToList();
            }
            else
            {
                const string sql = @"
                    SELECT n.*, u.Username as UserName, u.Avatar as UserAvatar,
                           tu.Username as TriggeredByUserName, tu.Avatar as TriggeredByUserAvatar
                    FROM Notifications n 
                    LEFT JOIN Users u ON n.UserId = u.Id 
                    LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                    WHERE n.UserId = @UserId AND n.IsRead = 0 AND n.IsDeleted = 0
                    ORDER BY n.CreatedAt DESC
                    LIMIT @Count";
                
                var result = await connection.QueryAsync<dynamic>(sql, new 
                { 
                    UserId = userId, 
                    Count = count 
                });
                
                return result.Select(MapToNotification).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取未读通知失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Notification>> GetExpiredAsync(DateTime expiryDate, int count = 100)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.ExpiresAt <= @ExpiryDate AND n.IsDeleted = 0
                ORDER BY n.ExpiresAt ASC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { ExpiryDate = expiryDate, Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取过期通知失败: {ExpiryDate}", expiryDate);
            throw;
        }
    }

    public async Task<List<Notification>> GetScheduledAsync(DateTime scheduleDate, int count = 100)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.ScheduledToSendAt <= @ScheduleDate AND n.Status = @Status AND n.IsDeleted = 0
                ORDER BY n.ScheduledToSendAt ASC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new 
            { 
                ScheduleDate = scheduleDate, 
                Status = NotificationStatus.Pending,
                Count = count 
            });
            
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取计划发送通知失败: {ScheduleDate}", scheduleDate);
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsRead = 1, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                ReadAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知已读失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<bool> MarkAsUnreadAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsRead = 0, ReadAt = NULL, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知未读失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<int> BatchMarkAsReadAsync(List<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsRead = 1, ReadAt = @ReadAt, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids AND IsDeleted = 0";
            
            var ids = _isSqlite ? notificationIds.Select(id => (object)id.ToString()).ToList() : notificationIds.Cast<object>().ToList();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Ids = ids, 
                ReadAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记已读失败");
            throw;
        }
    }

    public async Task<int> BatchDeleteAsync(List<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids AND IsDeleted = 0";
            
            var ids = _isSqlite ? notificationIds.Select(id => (object)id.ToString()).ToList() : notificationIds.Cast<object>().ToList();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Ids = ids, 
                DeletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除通知失败");
            throw;
        }
    }

    public async Task<int> CleanExpiredAsync(DateTime cutoffDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt
                WHERE ExpiresAt < @CutoffDate AND IsDeleted = 0";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                CutoffDate = cutoffDate,
                DeletedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期通知失败: {CutoffDate}", cutoffDate);
            throw;
        }
    }

    public async Task<NotificationStatsDto> GetStatsAsync(Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT 
                    COUNT(*) as TotalCount,
                    SUM(CASE WHEN n.IsRead = 0 THEN 1 ELSE 0 END) as UnreadCount,
                    SUM(CASE WHEN n.IsRead = 1 THEN 1 ELSE 0 END) as ReadCount,
                    SUM(CASE WHEN n.Status = @FailedStatus THEN 1 ELSE 0 END) as FailedCount,
                    SUM(CASE WHEN n.IsArchived = 1 THEN 1 ELSE 0 END) as ArchivedCount,
                    SUM(CASE WHEN n.IsDeleted = 1 THEN 1 ELSE 0 END) as DeletedCount,
                    SUM(CASE WHEN n.Priority = @HighPriority THEN 1 ELSE 0 END) as HighPriorityCount,
                    SUM(CASE WHEN n.CreatedAt >= @RecentDate THEN 1 ELSE 0 END) as RecentCount,
                    SUM(CASE WHEN DATE(n.CreatedAt) = DATE('now') THEN 1 ELSE 0 END) as TodayCount,
                    MAX(n.CreatedAt) as LastNotificationAt
                FROM Notifications n
                WHERE {whereClause}";
            
            parameters.Add("FailedStatus", NotificationStatus.Failed);
            parameters.Add("HighPriority", NotificationPriority.High);
            parameters.Add("RecentDate", DateTime.UtcNow.AddHours(-24));
            
            var stats = await connection.QuerySingleAsync<NotificationStatsDto>(sql, parameters);
            
            // 获取按类型统计
            const string typeStatsSql = $@"
                SELECT n.Type, COUNT(*) as Count 
                FROM Notifications n
                WHERE {whereClause}
                GROUP BY n.Type";
            
            var typeStats = await connection.QueryAsync<dynamic>(typeStatsSql, parameters);
            stats.TypeStats = typeStats.ToDictionary(x => (NotificationType)x.Type, x => (int)x.Count);
            
            // 获取按优先级统计
            const string priorityStatsSql = $@"
                SELECT n.Priority, COUNT(*) as Count 
                FROM Notifications n
                WHERE {whereClause}
                GROUP BY n.Priority";
            
            var priorityStats = await connection.QueryAsync<dynamic>(priorityStatsSql, parameters);
            stats.PriorityStats = priorityStats.ToDictionary(x => (NotificationPriority)x.Priority, x => (int)x.Count);
            
            // 获取按状态统计
            const string statusStatsSql = $@"
                SELECT n.Status, COUNT(*) as Count 
                FROM Notifications n
                WHERE {whereClause}
                GROUP BY n.Status";
            
            var statusStats = await connection.QueryAsync<dynamic>(statusStatsSql, parameters);
            stats.StatusStats = statusStats.ToDictionary(x => (NotificationStatus)x.Status, x => (int)x.Count);
            
            // 获取按渠道统计
            const string channelStatsSql = $@"
                SELECT n.Channel, COUNT(*) as Count 
                FROM Notifications n
                WHERE {whereClause}
                GROUP BY n.Channel";
            
            var channelStats = await connection.QueryAsync<dynamic>(channelStatsSql, parameters);
            stats.ChannelStats = channelStats.ToDictionary(x => (NotificationChannel)x.Channel, x => (int)x.Count);
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知统计失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByAdvancedFilterAsync(NotificationAdvancedFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            // 构建高级筛选条件
            if (filter.UserIds?.Any() == true)
            {
                conditions.Add("n.UserId IN @UserIds");
                parameters.Add("UserIds", filter.UserIds);
            }
            
            if (filter.ExcludeUserIds?.Any() == true)
            {
                conditions.Add("n.UserId NOT IN @ExcludeUserIds");
                parameters.Add("ExcludeUserIds", filter.ExcludeUserIds);
            }
            
            if (filter.Types?.Any() == true)
            {
                conditions.Add("n.Type IN @Types");
                parameters.Add("Types", filter.Types);
            }
            
            if (filter.ExcludeTypes?.Any() == true)
            {
                conditions.Add("n.Type NOT IN @ExcludeTypes");
                parameters.Add("ExcludeTypes", filter.ExcludeTypes);
            }
            
            if (filter.Priorities?.Any() == true)
            {
                conditions.Add("n.Priority IN @Priorities");
                parameters.Add("Priorities", filter.Priorities);
            }
            
            if (filter.Statuses?.Any() == true)
            {
                conditions.Add("n.Status IN @Statuses");
                parameters.Add("Statuses", filter.Statuses);
            }
            
            if (filter.Channels?.Any() == true)
            {
                conditions.Add("n.Channel IN @Channels");
                parameters.Add("Channels", filter.Channels);
            }
            
            if (filter.RelatedEntityTypes?.Any() == true)
            {
                conditions.Add("n.RelatedEntityType IN @RelatedEntityTypes");
                parameters.Add("RelatedEntityTypes", filter.RelatedEntityTypes);
            }
            
            if (filter.RelatedEntityIds?.Any() == true)
            {
                conditions.Add("n.RelatedEntityId IN @RelatedEntityIds");
                parameters.Add("RelatedEntityIds", filter.RelatedEntityIds);
            }
            
            if (filter.TriggeredByUserIds?.Any() == true)
            {
                conditions.Add("n.TriggeredByUserId IN @TriggeredByUserIds");
                parameters.Add("TriggeredByUserIds", filter.TriggeredByUserIds);
            }
            
            if (filter.Actions?.Any() == true)
            {
                conditions.Add("n.Action IN @Actions");
                parameters.Add("Actions", filter.Actions);
            }
            
            if (filter.Tags?.Any() == true)
            {
                conditions.Add("n.Tag IN @Tags");
                parameters.Add("Tags", filter.Tags);
            }
            
            if (filter.ExcludeTags?.Any() == true)
            {
                conditions.Add("n.Tag NOT IN @ExcludeTags");
                parameters.Add("ExcludeTags", filter.ExcludeTags);
            }
            
            // 添加其他条件...
            if (filter.IsRead.HasValue)
            {
                conditions.Add("n.IsRead = @IsRead");
                parameters.Add("IsRead", filter.IsRead.Value);
            }
            
            if (filter.IsArchived.HasValue)
            {
                conditions.Add("n.IsArchived = @IsArchived");
                parameters.Add("IsArchived", filter.IsArchived.Value);
            }
            
            if (filter.RequiresConfirmation.HasValue)
            {
                conditions.Add("n.RequiresConfirmation = @RequiresConfirmation");
                parameters.Add("RequiresConfirmation", filter.RequiresConfirmation.Value);
            }
            
            if (filter.IsExpired.HasValue)
            {
                if (filter.IsExpired.Value)
                {
                    conditions.Add("n.ExpiresAt < @Now");
                }
                else
                {
                    conditions.Add("(n.ExpiresAt IS NULL OR n.ExpiresAt >= @Now)");
                }
                parameters.Add("Now", DateTime.UtcNow);
            }
            
            // 添加时间范围条件
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            if (filter.ExpiresAfter.HasValue)
            {
                conditions.Add("n.ExpiresAt >= @ExpiresAfter");
                parameters.Add("ExpiresAfter", filter.ExpiresAfter.Value);
            }
            
            if (filter.ExpiresBefore.HasValue)
            {
                conditions.Add("n.ExpiresAt <= @ExpiresBefore");
                parameters.Add("ExpiresBefore", filter.ExpiresBefore.Value);
            }
            
            // 添加搜索条件
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchConditions = new List<string>();
                
                if (filter.SearchScopes.Contains(NotificationSearchScope.Title) || filter.SearchScopes.Count == 0)
                {
                    searchConditions.Add("n.Title LIKE @Search");
                }
                
                if (filter.SearchScopes.Contains(NotificationSearchScope.Content) || filter.SearchScopes.Count == 0)
                {
                    searchConditions.Add("n.Content LIKE @Search");
                }
                
                if (filter.SearchScopes.Contains(NotificationSearchScope.Message) || filter.SearchScopes.Count == 0)
                {
                    searchConditions.Add("n.Message LIKE @Search");
                }
                
                if (filter.SearchScopes.Contains(NotificationSearchScope.Tag) || filter.SearchScopes.Count == 0)
                {
                    searchConditions.Add("n.Tag LIKE @Search");
                }
                
                if (searchConditions.Any())
                {
                    conditions.Add($"({string.Join(" OR ", searchConditions)})");
                    parameters.Add("Search", $"%{filter.Search}%");
                }
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 构建排序
            var sortBy = filter.SortField switch
            {
                NotificationSortField.CreatedAt => "n.CreatedAt",
                NotificationSortField.UpdatedAt => "n.UpdatedAt",
                NotificationSortField.Priority => "n.Priority",
                NotificationSortField.Title => "n.Title",
                NotificationSortField.Type => "n.Type",
                NotificationSortField.Status => "n.Status",
                NotificationSortField.ReadAt => "n.ReadAt",
                NotificationSortField.ExpiresAt => "n.ExpiresAt",
                NotificationSortField.SendCount => "n.SendCount",
                NotificationSortField.LastSentAt => "n.LastSentAt",
                _ => "n.CreatedAt"
            };
            
            var sortDirection = filter.SortDirection == SortDirection.Ascending ? "ASC" : "DESC";
            
            // 构建分页
            var offset = (filter.Page - 1) * filter.PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", Math.Min(filter.PageSize, filter.MaxPageSize));
            
            var sql = $@"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE {whereClause}
                ORDER BY {sortBy} {sortDirection}
                LIMIT @PageSize OFFSET @Offset";
            
            if (filter.CountOnly)
            {
                var countSql = $"SELECT COUNT(*) FROM Notifications n WHERE {whereClause}";
                var count = await connection.QuerySingleAsync<int>(countSql, parameters);
                return new List<Notification>(); // 返回空列表，只返回数量
            }
            
            var result = await connection.QueryAsync<dynamic>(sql, parameters);
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "高级筛选通知失败");
            throw;
        }
    }

    public async Task<List<Notification>> GetPendingSendAsync(int count = 100)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Status = @Status AND n.IsDeleted = 0
                ORDER BY n.CreatedAt ASC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new 
            { 
                Status = NotificationStatus.Pending,
                Count = count 
            });
            
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待发送通知失败");
            throw;
        }
    }

    public async Task<List<Notification>> GetFailedToSendAsync(int count = 100)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Status = @Status AND n.IsDeleted = 0
                ORDER BY n.CreatedAt ASC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new 
            { 
                Status = NotificationStatus.Failed,
                Count = count 
            });
            
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取发送失败通知失败");
            throw;
        }
    }

    public async Task<bool> ArchiveAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsArchived = 1, ArchivedAt = @ArchivedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                ArchivedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "归档通知失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<bool> UnarchiveAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsArchived = 0, ArchivedAt = NULL, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消归档通知失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<bool> ConfirmAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET ConfirmedAt = @ConfirmedAt, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND RequiresConfirmation = 1 AND IsDeleted = 0";
            
            var idParam = _isSqlite ? (object)id.ToString() : id;
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Id = idParam, 
                ConfirmedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "确认通知失败: {NotificationId}", id);
            throw;
        }
    }

    public async Task<int> BatchArchiveAsync(List<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsArchived = 1, ArchivedAt = @ArchivedAt, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids AND IsDeleted = 0";
            
            var ids = _isSqlite ? notificationIds.Select(id => (object)id.ToString()).ToList() : notificationIds.Cast<object>().ToList();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Ids = ids, 
                ArchivedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量归档通知失败");
            throw;
        }
    }

    public async Task<int> BatchUnarchiveAsync(List<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET IsArchived = 0, ArchivedAt = NULL, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids AND IsDeleted = 0";
            
            var ids = _isSqlite ? notificationIds.Select(id => (object)id.ToString()).ToList() : notificationIds.Cast<object>().ToList();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Ids = ids, 
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量取消归档通知失败");
            throw;
        }
    }

    public async Task<int> BatchConfirmAsync(List<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications 
                SET ConfirmedAt = @ConfirmedAt, UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids AND RequiresConfirmation = 1 AND IsDeleted = 0";
            
            var ids = _isSqlite ? notificationIds.Select(id => (object)id.ToString()).ToList() : notificationIds.Cast<object>().ToList();
            var rowsAffected = await connection.ExecuteAsync(sql, new 
            { 
                Ids = ids, 
                ConfirmedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量确认通知失败");
            throw;
        }
    }

    public async Task<List<Notification>> GetRecentAsync(int count = 20)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近通知失败");
            throw;
        }
    }

    public async Task<List<Notification>> GetByRelatedEntityAsync(RelatedEntityType entityType, string entityId, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.RelatedEntityType = @EntityType AND n.RelatedEntityId = @EntityId AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new 
            { 
                EntityType = entityType,
                EntityId = entityId,
                Count = count 
            });
            
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取相关实体通知失败: {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByTagAsync(string tag, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Tag = @Tag AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Tag = tag, Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取标签通知失败: {Tag}", tag);
            throw;
        }
    }

    public async Task<List<Notification>> GetByActionAsync(NotificationAction action, int count = 50)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.Action = @Action AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Action = action, Count = count });
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取操作通知失败: {Action}", action);
            throw;
        }
    }

    public async Task<Notification> GetByRelatedEntityLatestAsync(RelatedEntityType entityType, string entityId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE n.RelatedEntityType = @EntityType AND n.RelatedEntityId = @EntityId AND n.IsDeleted = 0
                ORDER BY n.CreatedAt DESC
                LIMIT 1";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new 
            { 
                EntityType = entityType,
                EntityId = entityId
            });
            
            return result != null ? MapToNotification(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取相关实体最新通知失败: {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<List<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.CreatedAt >= @StartDate AND n.CreatedAt <= @EndDate AND n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            parameters.Add("StartDate", startDate);
            parameters.Add("EndDate", endDate);
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = @"
                SELECT n.*, u.Username as UserName,
                       tu.Username as TriggeredByUserName
                FROM Notifications n 
                LEFT JOIN Users u ON n.UserId = u.Id 
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id 
                WHERE {whereClause}
                ORDER BY n.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql.Replace("{whereClause}", whereClause), parameters);
            return result.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日期范围通知失败: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<int> GetCountByTypeAsync(NotificationType type, Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.Type = @Type AND n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            parameters.Add("Type", type);
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $"SELECT COUNT(*) FROM Notifications n WHERE {whereClause}";
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取类型通知数量失败: {Type}", type);
            throw;
        }
    }

    public async Task<int> GetCountByStatusAsync(NotificationStatus status, Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.Status = @Status AND n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            parameters.Add("Status", status);
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $"SELECT COUNT(*) FROM Notifications n WHERE {whereClause}";
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取状态通知数量失败: {Status}", status);
            throw;
        }
    }

    public async Task<Dictionary<NotificationType, int>> GetCountByAllTypesAsync(Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT n.Type, COUNT(*) as Count 
                FROM Notifications n 
                WHERE {whereClause}
                GROUP BY n.Type";
            
            var result = await connection.QueryAsync<dynamic>(sql, parameters);
            return result.ToDictionary(x => (NotificationType)x.Type, x => (int)x.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有类型通知数量失败: {UserId}", userId);
            throw;
        }
    }

    public async Task<Dictionary<NotificationStatus, int>> GetCountByAllStatusesAsync(Guid? userId = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            if (userId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", userId.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT n.Status, COUNT(*) as Count 
                FROM Notifications n 
                WHERE {whereClause}
                GROUP BY n.Status";
            
            var result = await connection.QueryAsync<dynamic>(sql, parameters);
            return result.ToDictionary(x => (NotificationStatus)x.Status, x => (int)x.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有状态通知数量失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 映射数据库查询结果到Notification实体
    /// </summary>
    private Notification MapToNotification(dynamic result)
    {
        var notification = new Notification
        {
            Id = _isSqlite ? Guid.Parse(result.Id) : result.Id,
            UserId = _isSqlite ? Guid.Parse(result.UserId) : result.UserId,
            Type = (NotificationType)result.Type,
            Title = result.Title,
            Content = result.Content,
            Message = result.Message,
            Priority = (NotificationPriority)result.Priority,
            Status = (NotificationStatus)result.Status,
            RelatedEntityType = result.RelatedEntityType != null ? (RelatedEntityType?)result.RelatedEntityType : null,
            RelatedEntityId = result.RelatedEntityId,
            TriggeredByUserId = result.TriggeredByUserId != null ? (_isSqlite ? Guid.Parse(result.TriggeredByUserId) : result.TriggeredByUserId) : null,
            Action = result.Action != null ? (NotificationAction?)result.Action : null,
            Channel = (NotificationChannel)result.Channel,
            IsRead = ParseBool(result.IsRead),
            ReadAt = result.ReadAt as DateTime?,
            DeliveredAt = result.DeliveredAt as DateTime?,
            CreatedAt = ParseDateTime(result.CreatedAt),
            UpdatedAt = ParseDateTime(result.UpdatedAt),
            ExpiresAt = result.ExpiresAt as DateTime?,
            ScheduledToSendAt = result.ScheduledToSendAt as DateTime?,
            SendCount = ParseInt(result.SendCount),
            LastSentAt = result.LastSentAt as DateTime?,
            ErrorMessage = result.ErrorMessage,
            DataJson = result.DataJson,
            Tag = result.Tag,
            Icon = result.Icon,
            Color = result.Color,
            RequiresConfirmation = ParseBool(result.RequiresConfirmation),
            ConfirmedAt = result.ConfirmedAt as DateTime?,
            IsArchived = ParseBool(result.IsArchived),
            ArchivedAt = result.ArchivedAt as DateTime?,
            IsDeleted = ParseBool(result.IsDeleted),
            DeletedAt = result.DeletedAt as DateTime?
        };

        // 设置导航属性
        if (result.UserName != null)
        {
            notification.User = new User
            {
                Id = notification.UserId,
                Username = result.UserName
            };
        }

        if (result.TriggeredByUserName != null && notification.TriggeredByUserId.HasValue)
        {
            notification.TriggeredByUser = new User
            {
                Id = notification.TriggeredByUserId.Value,
                Username = result.TriggeredByUserName
            };
        }

        return notification;
    }

    /// <summary>
    /// 根据用户ID获取已读通知列表
    /// </summary>
    public async Task<PaginatedResult<Notification>> GetReadAsync(Guid userId, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // 构建查询条件
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            conditions.Add("n.UserId = @UserId");
            parameters.Add("UserId", HandleIdConversion(userId));
            
            conditions.Add("n.IsRead = 1");
            
            // 应用筛选条件
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                conditions.Add("n.Tag = @Tag");
                parameters.Add("Tag", filter.Tag);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Search))
            {
                conditions.Add("(n.Title LIKE @Search OR n.Content LIKE @Search OR n.Message LIKE @Search)");
                parameters.Add("Search", $"%{filter.Search}%");
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 获取总数
            const string countSql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            
            // 构建排序
            var orderBy = filter.Sort switch
            {
                NotificationSort.CreatedAtAsc => "n.CreatedAt ASC",
                NotificationSort.CreatedAtDesc => "n.CreatedAt DESC",
                NotificationSort.PriorityDesc => "n.Priority DESC",
                NotificationSort.PriorityAsc => "n.Priority ASC",
                NotificationSort.ReadAtDesc => "n.ReadAt DESC",
                NotificationSort.ReadAtAsc => "n.ReadAt ASC",
                _ => "n.CreatedAt DESC"
            };
            
            // 获取分页数据
            const string dataSql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY {orderBy}
                LIMIT @PageSize OFFSET @Offset";
            
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(dataSql, parameters);
            var notifications = results.Select(MapToNotification).ToList();
            
            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取已读通知失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取待发送通知列表
    /// </summary>
    public async Task<IEnumerable<Notification>> GetPendingToSendAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.Status = @Status
                ORDER BY n.CreatedAt ASC";
            
            var parameters = new DynamicParameters();
            parameters.Add("Status", (int)NotificationStatus.Pending);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待发送通知失败");
            throw;
        }
    }

    /// <summary>
    /// 获取计划发送通知列表
    /// </summary>
    public async Task<IEnumerable<Notification>> GetScheduledToSendAsync(DateTime beforeTime)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.Status = @Status
                AND n.ScheduledToSendAt <= @BeforeTime
                ORDER BY n.ScheduledToSendAt ASC";
            
            var parameters = new DynamicParameters();
            parameters.Add("Status", (int)NotificationStatus.Pending);
            parameters.Add("BeforeTime", beforeTime);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取计划发送通知失败");
            throw;
        }
    }

    /// <summary>
    /// 搜索通知
    /// </summary>
    public async Task<IEnumerable<Notification>> SearchAsync(string searchTerm, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            // 搜索条件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                conditions.Add("(n.Title LIKE @Search OR n.Content LIKE @Search OR n.Message LIKE @Search OR n.Tag LIKE @Search)");
                parameters.Add("Search", $"%{searchTerm}%");
            }
            
            // 应用筛选条件
            if (filter.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", HandleIdConversion(filter.UserId.Value));
            }
            
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY n.CreatedAt DESC
                LIMIT 100"; // 限制搜索结果数量
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "搜索通知失败: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// 根据标签获取通知列表
    /// </summary>
    public async Task<PaginatedResult<Notification>> GetByTagAsync(string tag, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            conditions.Add("n.Tag = @Tag");
            parameters.Add("Tag", tag);
            
            // 应用筛选条件
            if (filter.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", HandleIdConversion(filter.UserId.Value));
            }
            
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 获取总数
            const string countSql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            
            // 构建排序
            var orderBy = filter.Sort switch
            {
                NotificationSort.CreatedAtAsc => "n.CreatedAt ASC",
                NotificationSort.CreatedAtDesc => "n.CreatedAt DESC",
                NotificationSort.PriorityDesc => "n.Priority DESC",
                NotificationSort.PriorityAsc => "n.Priority ASC",
                _ => "n.CreatedAt DESC"
            };
            
            // 获取分页数据
            const string dataSql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY {orderBy}
                LIMIT @PageSize OFFSET @Offset";
            
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(dataSql, parameters);
            var notifications = results.Select(MapToNotification).ToList();
            
            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据标签获取通知失败: {Tag}", tag);
            throw;
        }
    }

    /// <summary>
    /// 根据优先级获取通知列表
    /// </summary>
    public async Task<PaginatedResult<Notification>> GetByPriorityAsync(NotificationPriority priority, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            conditions.Add("n.Priority = @Priority");
            parameters.Add("Priority", (int)priority);
            
            // 应用筛选条件
            if (filter.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", HandleIdConversion(filter.UserId.Value));
            }
            
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                conditions.Add("n.Tag = @Tag");
                parameters.Add("Tag", filter.Tag);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 获取总数
            const string countSql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            
            // 构建排序
            var orderBy = filter.Sort switch
            {
                NotificationSort.CreatedAtAsc => "n.CreatedAt ASC",
                NotificationSort.CreatedAtDesc => "n.CreatedAt DESC",
                NotificationSort.PriorityDesc => "n.Priority DESC",
                NotificationSort.PriorityAsc => "n.Priority ASC",
                _ => "n.CreatedAt DESC"
            };
            
            // 获取分页数据
            const string dataSql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY {orderBy}
                LIMIT @PageSize OFFSET @Offset";
            
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(dataSql, parameters);
            var notifications = results.Select(MapToNotification).ToList();
            
            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据优先级获取通知失败: {Priority}", priority);
            throw;
        }
    }

    /// <summary>
    /// 根据时间范围获取通知列表
    /// </summary>
    public async Task<PaginatedResult<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            conditions.Add("n.CreatedAt >= @StartDate");
            parameters.Add("StartDate", startDate);
            
            conditions.Add("n.CreatedAt <= @EndDate");
            parameters.Add("EndDate", endDate);
            
            // 应用筛选条件
            if (filter.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", HandleIdConversion(filter.UserId.Value));
            }
            
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                conditions.Add("n.Tag = @Tag");
                parameters.Add("Tag", filter.Tag);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            // 获取总数
            const string countSql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            
            // 构建排序
            var orderBy = filter.Sort switch
            {
                NotificationSort.CreatedAtAsc => "n.CreatedAt ASC",
                NotificationSort.CreatedAtDesc => "n.CreatedAt DESC",
                NotificationSort.PriorityDesc => "n.Priority DESC",
                NotificationSort.PriorityAsc => "n.Priority ASC",
                _ => "n.CreatedAt DESC"
            };
            
            // 获取分页数据
            const string dataSql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY {orderBy}
                LIMIT @PageSize OFFSET @Offset";
            
            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(dataSql, parameters);
            var notifications = results.Select(MapToNotification).ToList();
            
            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据时间范围获取通知失败: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// 获取系统通知统计信息
    /// </summary>
    public async Task<NotificationStatsDto> GetSystemStatsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalCount,
                    SUM(CASE WHEN n.IsRead = 0 THEN 1 ELSE 0 END) as UnreadCount,
                    SUM(CASE WHEN n.IsRead = 1 THEN 1 ELSE 0 END) as ReadCount,
                    SUM(CASE WHEN n.Status = @FailedStatus THEN 1 ELSE 0 END) as FailedCount,
                    SUM(CASE WHEN n.IsArchived = 1 THEN 1 ELSE 0 END) as ArchivedCount,
                    SUM(CASE WHEN n.IsDeleted = 1 THEN 1 ELSE 0 END) as DeletedCount,
                    SUM(CASE WHEN n.Priority = @HighPriority OR n.Priority = @UrgentPriority OR n.Priority = @CriticalPriority THEN 1 ELSE 0 END) as HighPriorityCount,
                    SUM(CASE WHEN n.CreatedAt >= @TodayStart THEN 1 ELSE 0 END) as TodayCount,
                    MAX(n.CreatedAt) as LastNotificationAt
                FROM Notifications n
                WHERE n.IsDeleted = 0";
            
            var parameters = new DynamicParameters();
            parameters.Add("FailedStatus", (int)NotificationStatus.Failed);
            parameters.Add("HighPriority", (int)NotificationPriority.High);
            parameters.Add("UrgentPriority", (int)NotificationPriority.Urgent);
            parameters.Add("CriticalPriority", (int)NotificationPriority.Critical);
            parameters.Add("TodayStart", DateTime.Today);
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
            
            return new NotificationStatsDto
            {
                TotalCount = result.TotalCount,
                UnreadCount = result.UnreadCount,
                ReadCount = result.ReadCount,
                FailedCount = result.FailedCount,
                ArchivedCount = result.ArchivedCount,
                DeletedCount = result.DeletedCount,
                HighPriorityCount = result.HighPriorityCount,
                RecentCount = await GetRecentCountAsync(connection),
                TodayCount = result.TodayCount,
                LastNotificationAt = result.LastNotificationAt as DateTime?
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统通知统计信息失败");
            throw;
        }
    }

    /// <summary>
    /// 获取通知数量（按筛选条件）
    /// </summary>
    public async Task<int> GetCountByFilterAsync(Guid userId, NotificationFilterDto filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            conditions.Add("n.UserId = @UserId");
            parameters.Add("UserId", HandleIdConversion(userId));
            
            // 应用筛选条件
            if (filter.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)filter.Type.Value);
            }
            
            if (filter.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)filter.Priority.Value);
            }
            
            if (filter.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)filter.Status.Value);
            }
            
            if (filter.IsRead.HasValue)
            {
                conditions.Add("n.IsRead = @IsRead");
                parameters.Add("IsRead", filter.IsRead.Value ? 1 : 0);
            }
            
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                conditions.Add("n.Tag = @Tag");
                parameters.Add("Tag", filter.Tag);
            }
            
            if (filter.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", filter.EndDate.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知数量失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取最近通知
    /// </summary>
    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(Guid userId, int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.UserId = @UserId
                ORDER BY n.CreatedAt DESC
                LIMIT @Count";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            parameters.Add("Count", count);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近通知失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量创建通知
    /// </summary>
    public async Task<int> BulkCreateAsync(IEnumerable<Notification> notifications)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var count = 0;
                const string sql = @"
                    INSERT INTO Notifications (
                        Id, UserId, Type, Title, Content, Message, Priority, Status,
                        RelatedEntityType, RelatedEntityId, TriggeredByUserId, Action, Channel,
                        IsRead, ReadAt, DeliveredAt, CreatedAt, UpdatedAt, ExpiresAt,
                        ScheduledToSendAt, SendCount, LastSentAt, ErrorMessage, DataJson,
                        Tag, Icon, Color, RequiresConfirmation, ConfirmedAt, IsArchived,
                        ArchivedAt, IsDeleted, DeletedAt
                    ) VALUES (
                        @Id, @UserId, @Type, @Title, @Content, @Message, @Priority, @Status,
                        @RelatedEntityType, @RelatedEntityId, @TriggeredByUserId, @Action, @Channel,
                        @IsRead, @ReadAt, @DeliveredAt, @CreatedAt, @UpdatedAt, @ExpiresAt,
                        @ScheduledToSendAt, @SendCount, @LastSentAt, @ErrorMessage, @DataJson,
                        @Tag, @Icon, @Color, @RequiresConfirmation, @ConfirmedAt, @IsArchived,
                        @ArchivedAt, @IsDeleted, @DeletedAt
                    )";
                
                foreach (var notification in notifications)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Id", HandleIdConversion(notification.Id));
                    parameters.Add("UserId", HandleIdConversion(notification.UserId));
                    parameters.Add("Type", (int)notification.Type);
                    parameters.Add("Title", notification.Title);
                    parameters.Add("Content", notification.Content);
                    parameters.Add("Message", notification.Message);
                    parameters.Add("Priority", (int)notification.Priority);
                    parameters.Add("Status", (int)notification.Status);
                    parameters.Add("RelatedEntityType", notification.RelatedEntityType.HasValue ? (int?)notification.RelatedEntityType.Value : null);
                    parameters.Add("RelatedEntityId", notification.RelatedEntityId);
                    parameters.Add("TriggeredByUserId", notification.TriggeredByUserId.HasValue ? HandleIdConversion(notification.TriggeredByUserId.Value) : null);
                    parameters.Add("Action", notification.Action.HasValue ? (int?)notification.Action.Value : null);
                    parameters.Add("Channel", (int)notification.Channel);
                    parameters.Add("IsRead", notification.IsRead ? 1 : 0);
                    parameters.Add("ReadAt", notification.ReadAt);
                    parameters.Add("DeliveredAt", notification.DeliveredAt);
                    parameters.Add("CreatedAt", notification.CreatedAt);
                    parameters.Add("UpdatedAt", notification.UpdatedAt);
                    parameters.Add("ExpiresAt", notification.ExpiresAt);
                    parameters.Add("ScheduledToSendAt", notification.ScheduledToSendAt);
                    parameters.Add("SendCount", notification.SendCount);
                    parameters.Add("LastSentAt", notification.LastSentAt);
                    parameters.Add("ErrorMessage", notification.ErrorMessage);
                    parameters.Add("DataJson", notification.DataJson);
                    parameters.Add("Tag", notification.Tag);
                    parameters.Add("Icon", notification.Icon);
                    parameters.Add("Color", notification.Color);
                    parameters.Add("RequiresConfirmation", notification.RequiresConfirmation ? 1 : 0);
                    parameters.Add("ConfirmedAt", notification.ConfirmedAt);
                    parameters.Add("IsArchived", notification.IsArchived ? 1 : 0);
                    parameters.Add("ArchivedAt", notification.ArchivedAt);
                    parameters.Add("IsDeleted", notification.IsDeleted ? 1 : 0);
                    parameters.Add("DeletedAt", notification.DeletedAt);
                    
                    await connection.ExecuteAsync(sql, parameters, transaction);
                    count++;
                }
                
                await transaction.CommitAsync();
                return count;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量创建通知失败");
            throw;
        }
    }

    /// <summary>
    /// 批量更新通知
    /// </summary>
    public async Task<int> BulkUpdateAsync(IEnumerable<Notification> notifications)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var count = 0;
                const string sql = @"
                    UPDATE Notifications SET
                        Type = @Type,
                        Title = @Title,
                        Content = @Content,
                        Message = @Message,
                        Priority = @Priority,
                        Status = @Status,
                        RelatedEntityType = @RelatedEntityType,
                        RelatedEntityId = @RelatedEntityId,
                        TriggeredByUserId = @TriggeredByUserId,
                        Action = @Action,
                        Channel = @Channel,
                        IsRead = @IsRead,
                        ReadAt = @ReadAt,
                        DeliveredAt = @DeliveredAt,
                        UpdatedAt = @UpdatedAt,
                        ExpiresAt = @ExpiresAt,
                        ScheduledToSendAt = @ScheduledToSendAt,
                        SendCount = @SendCount,
                        LastSentAt = @LastSentAt,
                        ErrorMessage = @ErrorMessage,
                        DataJson = @DataJson,
                        Tag = @Tag,
                        Icon = @Icon,
                        Color = @Color,
                        RequiresConfirmation = @RequiresConfirmation,
                        ConfirmedAt = @ConfirmedAt,
                        IsArchived = @IsArchived,
                        ArchivedAt = @ArchivedAt,
                        IsDeleted = @IsDeleted,
                        DeletedAt = @DeletedAt
                    WHERE Id = @Id";
                
                foreach (var notification in notifications)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Id", HandleIdConversion(notification.Id));
                    parameters.Add("Type", (int)notification.Type);
                    parameters.Add("Title", notification.Title);
                    parameters.Add("Content", notification.Content);
                    parameters.Add("Message", notification.Message);
                    parameters.Add("Priority", (int)notification.Priority);
                    parameters.Add("Status", (int)notification.Status);
                    parameters.Add("RelatedEntityType", notification.RelatedEntityType.HasValue ? (int?)notification.RelatedEntityType.Value : null);
                    parameters.Add("RelatedEntityId", notification.RelatedEntityId);
                    parameters.Add("TriggeredByUserId", notification.TriggeredByUserId.HasValue ? HandleIdConversion(notification.TriggeredByUserId.Value) : null);
                    parameters.Add("Action", notification.Action.HasValue ? (int?)notification.Action.Value : null);
                    parameters.Add("Channel", (int)notification.Channel);
                    parameters.Add("IsRead", notification.IsRead ? 1 : 0);
                    parameters.Add("ReadAt", notification.ReadAt);
                    parameters.Add("DeliveredAt", notification.DeliveredAt);
                    parameters.Add("UpdatedAt", notification.UpdatedAt);
                    parameters.Add("ExpiresAt", notification.ExpiresAt);
                    parameters.Add("ScheduledToSendAt", notification.ScheduledToSendAt);
                    parameters.Add("SendCount", notification.SendCount);
                    parameters.Add("LastSentAt", notification.LastSentAt);
                    parameters.Add("ErrorMessage", notification.ErrorMessage);
                    parameters.Add("DataJson", notification.DataJson);
                    parameters.Add("Tag", notification.Tag);
                    parameters.Add("Icon", notification.Icon);
                    parameters.Add("Color", notification.Color);
                    parameters.Add("RequiresConfirmation", notification.RequiresConfirmation ? 1 : 0);
                    parameters.Add("ConfirmedAt", notification.ConfirmedAt);
                    parameters.Add("IsArchived", notification.IsArchived ? 1 : 0);
                    parameters.Add("ArchivedAt", notification.ArchivedAt);
                    parameters.Add("IsDeleted", notification.IsDeleted ? 1 : 0);
                    parameters.Add("DeletedAt", notification.DeletedAt);
                    
                    var affected = await connection.ExecuteAsync(sql, parameters, transaction);
                    if (affected > 0)
                        count++;
                }
                
                await transaction.CommitAsync();
                return count;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新通知失败");
            throw;
        }
    }

    /// <summary>
    /// 批量删除通知
    /// </summary>
    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var count = 0;
                const string sql = "DELETE FROM Notifications WHERE Id = @Id";
                
                foreach (var notificationId in notificationIds)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("Id", HandleIdConversion(notificationId));
                    
                    var affected = await connection.ExecuteAsync(sql, parameters, transaction);
                    if (affected > 0)
                        count++;
                }
                
                await transaction.CommitAsync();
                return count;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除通知失败");
            throw;
        }
    }

    /// <summary>
    /// 批量标记未读
    /// </summary>
    public async Task<bool> BulkMarkAsUnreadAsync(IEnumerable<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                const string sql = @"
                    UPDATE Notifications SET
                        IsRead = 0,
                        ReadAt = NULL,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
                
                var parameters = new DynamicParameters();
                parameters.Add("UpdatedAt", DateTime.UtcNow);
                
                var ids = notificationIds.Select(id => HandleIdConversion(id)).ToArray();
                parameters.Add("Ids", ids);
                
                var count = await connection.ExecuteAsync(sql, parameters, transaction);
                
                await transaction.CommitAsync();
                return count > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记未读失败");
            throw;
        }
    }

    /// <summary>
    /// 批量软删除通知
    /// </summary>
    public async Task<bool> BulkSoftDeleteAsync(IEnumerable<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                const string sql = @"
                    UPDATE Notifications SET
                        IsDeleted = 1,
                        DeletedAt = @DeletedAt,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
                
                var parameters = new DynamicParameters();
                parameters.Add("DeletedAt", DateTime.UtcNow);
                parameters.Add("UpdatedAt", DateTime.UtcNow);
                
                var ids = notificationIds.Select(id => HandleIdConversion(id)).ToArray();
                parameters.Add("Ids", ids);
                
                var count = await connection.ExecuteAsync(sql, parameters, transaction);
                
                await transaction.CommitAsync();
                return count > 0;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量软删除通知失败");
            throw;
        }
    }

    /// <summary>
    /// 更新通知状态
    /// </summary>
    public async Task<bool> UpdateStatusAsync(Guid notificationId, NotificationStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications SET
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";
            
            var parameters = new DynamicParameters();
            parameters.Add("Id", HandleIdConversion(notificationId));
            parameters.Add("Status", (int)status);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            
            var affected = await connection.ExecuteAsync(sql, parameters);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知状态失败: {NotificationId}, {Status}", notificationId, status);
            throw;
        }
    }

    /// <summary>
    /// 从缓存获取通知
    /// </summary>
    public async Task<Notification?> GetFromCacheAsync(Guid id)
    {
        // 简化的缓存实现，实际项目中应该使用分布式缓存
        return await GetByIdAsync(id);
    }

    /// <summary>
    /// 设置通知缓存
    /// </summary>
    public async Task<bool> SetCacheAsync(Notification notification, TimeSpan? expiration = null)
    {
        // 简化的缓存实现，实际项目中应该使用分布式缓存
        return true;
    }

    /// <summary>
    /// 移除通知缓存
    /// </summary>
    public async Task<bool> RemoveCacheAsync(Guid id)
    {
        // 简化的缓存实现，实际项目中应该使用分布式缓存
        return true;
    }

    /// <summary>
    /// 清空用户通知缓存
    /// </summary>
    public async Task<bool> ClearUserCacheAsync(Guid userId)
    {
        // 简化的缓存实现，实际项目中应该使用分布式缓存
        return true;
    }

    /// <summary>
    /// 获取通知摘要
    /// </summary>
    public async Task<NotificationSummaryDto> GetSummaryAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalNotifications,
                    SUM(CASE WHEN n.IsRead = 0 THEN 1 ELSE 0 END) as UnreadNotifications,
                    SUM(CASE WHEN n.Priority IN (@HighPriority, @UrgentPriority, @CriticalPriority) THEN 1 ELSE 0 END) as HighPriorityNotifications,
                    MAX(n.CreatedAt) as LastCheckedAt
                FROM Notifications n
                WHERE n.IsDeleted = 0 AND n.UserId = @UserId";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            parameters.Add("HighPriority", (int)NotificationPriority.High);
            parameters.Add("UrgentPriority", (int)NotificationPriority.Urgent);
            parameters.Add("CriticalPriority", (int)NotificationPriority.Critical);
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
            
            // 获取最近通知
            var recentNotifications = await GetRecentNotificationsAsync(userId, 5);
            
            // 获取类型统计
            var typeStats = await GetCountByAllTypesAsync(userId);
            
            return new NotificationSummaryDto
            {
                TotalNotifications = result.TotalNotifications,
                UnreadNotifications = result.UnreadNotifications,
                HighPriorityNotifications = result.HighPriorityNotifications,
                RecentNotifications = recentNotifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Type = n.Type,
                    Title = n.Title,
                    Content = n.Content,
                    Priority = n.Priority,
                    Status = n.Status,
                    Channel = n.Channel,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList(),
                TypeCounts = typeStats,
                LastCheckedAt = result.LastCheckedAt as DateTime?
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知摘要失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取高优先级通知
    /// </summary>
    public async Task<IEnumerable<Notification>> GetHighPriorityNotificationsAsync(Guid userId, int count = 5)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.UserId = @UserId
                AND n.Priority IN (@HighPriority, @UrgentPriority, @CriticalPriority)
                AND n.IsRead = 0
                ORDER BY n.Priority DESC, n.CreatedAt DESC
                LIMIT @Count";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            parameters.Add("HighPriority", (int)NotificationPriority.High);
            parameters.Add("UrgentPriority", (int)NotificationPriority.Urgent);
            parameters.Add("CriticalPriority", (int)NotificationPriority.Critical);
            parameters.Add("Count", count);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取高优先级通知失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取紧急通知
    /// </summary>
    public async Task<IEnumerable<Notification>> GetUrgentNotificationsAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.UserId = @UserId
                AND n.Priority IN (@UrgentPriority, @CriticalPriority)
                AND n.IsRead = 0
                ORDER BY n.Priority DESC, n.CreatedAt DESC";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            parameters.Add("UrgentPriority", (int)NotificationPriority.Urgent);
            parameters.Add("CriticalPriority", (int)NotificationPriority.Critical);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取紧急通知失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取需要确认的通知
    /// </summary>
    public async Task<IEnumerable<Notification>> GetConfirmationRequiredAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.UserId = @UserId
                AND n.RequiresConfirmation = 1
                AND n.ConfirmedAt IS NULL
                ORDER BY n.CreatedAt DESC";
            
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取需要确认的通知失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取过期通知
    /// </summary>
    public async Task<IEnumerable<Notification>> GetExpiredNotificationsAsync(DateTime beforeTime)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE n.IsDeleted = 0
                AND n.ExpiresAt <= @BeforeTime
                ORDER BY n.ExpiresAt ASC";
            
            var parameters = new DynamicParameters();
            parameters.Add("BeforeTime", beforeTime);
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取过期通知失败");
            throw;
        }
    }

    /// <summary>
    /// 清理过期通知
    /// </summary>
    public async Task<int> CleanExpiredNotificationsAsync(DateTime beforeTime)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications SET
                    IsDeleted = 1,
                    DeletedAt = @DeletedAt,
                    UpdatedAt = @UpdatedAt
                WHERE n.IsDeleted = 0
                AND n.ExpiresAt <= @BeforeTime";
            
            var parameters = new DynamicParameters();
            parameters.Add("DeletedAt", DateTime.UtcNow);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            parameters.Add("BeforeTime", beforeTime);
            
            return await connection.ExecuteAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期通知失败");
            throw;
        }
    }

    /// <summary>
    /// 导出通知数据
    /// </summary>
    public async Task<IEnumerable<Notification>> ExportAsync(NotificationExportOptionsDto options)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0" };
            var parameters = new DynamicParameters();
            
            // 应用筛选条件
            if (options.UserId.HasValue)
            {
                conditions.Add("n.UserId = @UserId");
                parameters.Add("UserId", HandleIdConversion(options.UserId.Value));
            }
            
            if (options.Type.HasValue)
            {
                conditions.Add("n.Type = @Type");
                parameters.Add("Type", (int)options.Type.Value);
            }
            
            if (options.Priority.HasValue)
            {
                conditions.Add("n.Priority = @Priority");
                parameters.Add("Priority", (int)options.Priority.Value);
            }
            
            if (options.Status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)options.Status.Value);
            }
            
            if (options.StartDate.HasValue)
            {
                conditions.Add("n.CreatedAt >= @StartDate");
                parameters.Add("StartDate", options.StartDate.Value);
            }
            
            if (options.EndDate.HasValue)
            {
                conditions.Add("n.CreatedAt <= @EndDate");
                parameters.Add("EndDate", options.EndDate.Value);
            }
            
            if (options.IncludeDeleted)
            {
                conditions[0] = "1=1"; // 移除删除条件
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT n.*, u.Username, u.Email,
                       tu.Username as TriggeredByUserName, tu.Email as TriggeredByUserEmail
                FROM Notifications n
                LEFT JOIN Users u ON n.UserId = u.Id
                LEFT JOIN Users tu ON n.TriggeredByUserId = tu.Id
                WHERE {whereClause}
                ORDER BY n.CreatedAt DESC
                LIMIT 1000"; // 限制导出数量
            
            var results = await connection.QueryAsync<NotificationQueryResult>(sql, parameters);
            return results.Select(MapToNotification).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出通知数据失败");
            throw;
        }
    }

    /// <summary>
    /// 导入通知数据
    /// </summary>
    public async Task<int> ImportAsync(IEnumerable<Notification> notifications, bool overrideExisting = false)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var count = 0;
                
                foreach (var notification in notifications)
                {
                    var existingNotification = await GetByIdAsync(notification.Id);
                    
                    if (existingNotification == null || overrideExisting)
                    {
                        if (existingNotification != null)
                        {
                            // 更新现有通知
                            await UpdateAsync(notification);
                        }
                        else
                        {
                            // 创建新通知
                            await CreateAsync(notification);
                        }
                        count++;
                    }
                }
                
                await transaction.CommitAsync();
                return count;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入通知数据失败");
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否可以访问通知
    /// </summary>
    public async Task<bool> CanUserAccessAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
                return false;
            
            return notification.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户访问权限失败: {NotificationId}, {UserId}", notificationId, userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否可以编辑通知
    /// </summary>
    public async Task<bool> CanUserEditAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
                return false;
            
            return notification.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户编辑权限失败: {NotificationId}, {UserId}", notificationId, userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否可以删除通知
    /// </summary>
    public async Task<bool> CanUserDeleteAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
                return false;
            
            return notification.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户删除权限失败: {NotificationId}, {UserId}", notificationId, userId);
            throw;
        }
    }

    /// <summary>
    /// 开始事务
    /// </summary>
    public async Task<string> BeginTransactionAsync()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public async Task<bool> CommitTransactionAsync(string transactionId)
    {
        return true;
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public async Task<bool> RollbackTransactionAsync(string transactionId)
    {
        return true;
    }

    /// <summary>
    /// 预加载通知数据
    /// </summary>
    public async Task<IEnumerable<Notification>> PreloadAsync(Guid userId, int count = 50)
    {
        return await GetRecentNotificationsAsync(userId, count);
    }

    /// <summary>
    /// 获取通知数量（高性能版本）
    /// </summary>
    public async Task<int> GetCountFastAsync(Guid userId, NotificationStatus? status = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var conditions = new List<string> { "n.IsDeleted = 0", "n.UserId = @UserId" };
            var parameters = new DynamicParameters();
            parameters.Add("UserId", HandleIdConversion(userId));
            
            if (status.HasValue)
            {
                conditions.Add("n.Status = @Status");
                parameters.Add("Status", (int)status.Value);
            }
            
            var whereClause = string.Join(" AND ", conditions);
            
            const string sql = $@"
                SELECT COUNT(*) 
                FROM Notifications n 
                WHERE {whereClause}";
            
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知数量失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 批量更新通知状态（高性能版本）
    /// </summary>
    public async Task<int> BulkUpdateStatusFastAsync(IEnumerable<Guid> notificationIds, NotificationStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications SET
                    Status = @Status,
                    UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids";
            
            var parameters = new DynamicParameters();
            parameters.Add("Status", (int)status);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            parameters.Add("Ids", notificationIds.Select(id => HandleIdConversion(id)).ToArray());
            
            return await connection.ExecuteAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新通知状态失败");
            throw;
        }
    }

    /// <summary>
    /// 获取用户通知统计信息
    /// </summary>
    public async Task<NotificationStatsDto> GetStatsByUserIdAsync(Guid userId)
    {
        return await GetStatsAsync(userId);
    }

    /// <summary>
    /// 获取最近通知数量
    /// </summary>
    private async Task<int> GetRecentCountAsync(IDbConnection connection)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM Notifications n 
            WHERE n.IsDeleted = 0 
            AND n.CreatedAt >= @RecentTime";
        
        var parameters = new DynamicParameters();
        parameters.Add("RecentTime", DateTime.Now.AddHours(-24));
        
        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    /// <summary>
    /// 批量标记已读
    /// </summary>
    public async Task<bool> BulkMarkAsReadAsync(IEnumerable<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications SET
                    Status = @Status,
                    IsRead = 1,
                    ReadAt = @ReadAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids";
            
            var parameters = new DynamicParameters();
            parameters.Add("Status", (int)NotificationStatus.Read);
            parameters.Add("ReadAt", DateTime.UtcNow);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            parameters.Add("Ids", notificationIds.Select(id => HandleIdConversion(id)).ToArray());
            
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量标记已读失败");
            throw;
        }
    }

    /// <summary>
    /// 批量归档通知
    /// </summary>
    public async Task<bool> BulkArchiveAsync(IEnumerable<Guid> notificationIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE Notifications SET
                    Status = @Status,
                    IsArchived = 1,
                    ArchivedAt = @ArchivedAt,
                    UpdatedAt = @UpdatedAt
                WHERE Id IN @Ids";
            
            var parameters = new DynamicParameters();
            parameters.Add("Status", (int)NotificationStatus.Archived);
            parameters.Add("ArchivedAt", DateTime.UtcNow);
            parameters.Add("UpdatedAt", DateTime.UtcNow);
            parameters.Add("Ids", notificationIds.Select(id => HandleIdConversion(id)).ToArray());
            
            var rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量归档通知失败");
            throw;
        }
    }
}