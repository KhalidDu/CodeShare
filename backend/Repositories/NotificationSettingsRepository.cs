using Dapper;
using System.Text.Json;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using System.Data;
using Microsoft.Extensions.Logging;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 通知设置仓储实现 - 遵循单一职责原则，负责通知设置的数据访问
/// </summary>
public class NotificationSettingsRepository : INotificationSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NotificationSettingsRepository> _logger;
    private readonly bool _isSqlite;

    public NotificationSettingsRepository(IDbConnectionFactory connectionFactory, ILogger<NotificationSettingsRepository> logger)
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

    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取通知设置
    /// </summary>
    public async Task<NotificationSetting?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE Id = @Id"
                : @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE Id = @Id";

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = HandleIdConversion(id) });
            
            if (result == null)
                return null;

            return MapToNotificationSetting(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知设置失败: {SettingId}", id);
            throw;
        }
    }

    /// <summary>
    /// 根据用户ID获取通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSetting>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId"
                : @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId";

            var results = await connection.QueryAsync<dynamic>(sql, new { UserId = HandleIdConversion(userId) });
            
            return results.Select(MapToNotificationSetting).Where(x => x != null).Cast<NotificationSetting>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 根据用户ID和通知类型获取通知设置
    /// </summary>
    public async Task<NotificationSetting?> GetByUserIdAndTypeAsync(Guid userId, NotificationType? notificationType)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND NotificationType = @NotificationType"
                : @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND NotificationType = @NotificationType";

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { 
                UserId = HandleIdConversion(userId), 
                NotificationType = (int)notificationType 
            });
            
            if (result == null)
                return null;

            return MapToNotificationSetting(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知设置失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 创建通知设置
    /// </summary>
    public async Task<NotificationSetting> CreateAsync(NotificationSetting setting)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"INSERT INTO NotificationSettings 
                   (Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                    EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                    IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                    IsDefault, CreatedAt, UpdatedAt)
                   VALUES 
                   (@Id, @UserId, @NotificationType, @IsEmailEnabled, @IsPushEnabled, @IsSmsEnabled, 
                    @EmailFrequency, @PushFrequency, @SmsFrequency, @QuietHoursStart, @QuietHoursEnd, 
                    @IsQuietHoursEnabled, @Language, @TimeZone, @IsSoundEnabled, @IsActive, 
                    @IsDefault, @CreatedAt, @UpdatedAt)"
                : @"INSERT INTO NotificationSettings 
                   (Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                    EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                    IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                    IsDefault, CreatedAt, UpdatedAt)
                   VALUES 
                   (@Id, @UserId, @NotificationType, @IsEmailEnabled, @IsPushEnabled, @IsSmsEnabled, 
                    @EmailFrequency, @PushFrequency, @SmsFrequency, @QuietHoursStart, @QuietHoursEnd, 
                    @IsQuietHoursEnabled, @Language, @TimeZone, @IsSoundEnabled, @IsActive, 
                    @IsDefault, @CreatedAt, @UpdatedAt)";

            var parameters = new
            {
                Id = HandleIdConversion(setting.Id),
                UserId = HandleIdConversion(setting.UserId),
                NotificationType = (int)setting.NotificationType,
                setting.IsEmailEnabled,
                setting.IsPushEnabled,
                setting.IsSmsEnabled,
                EmailFrequency = (int)setting.EmailFrequency,
                PushFrequency = (int)setting.PushFrequency,
                SmsFrequency = (int)setting.SmsFrequency,
                QuietHoursStart = setting.QuietHoursStart.ToString(),
                QuietHoursEnd = setting.QuietHoursEnd.ToString(),
                setting.IsQuietHoursEnabled,
                setting.Language,
                setting.TimeZone,
                setting.IsSoundEnabled,
                setting.IsActive,
                setting.IsDefault,
                setting.CreatedAt,
                setting.UpdatedAt
            };

            await connection.ExecuteAsync(sql, parameters);
            
            return setting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知设置失败: {UserId}, {NotificationType}", setting.UserId, setting.NotificationType);
            throw;
        }
    }

    /// <summary>
    /// 更新通知设置
    /// </summary>
    public async Task<NotificationSetting> UpdateAsync(NotificationSetting setting)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"UPDATE NotificationSettings 
                   SET NotificationType = @NotificationType,
                       IsEmailEnabled = @IsEmailEnabled,
                       IsPushEnabled = @IsPushEnabled,
                       IsSmsEnabled = @IsSmsEnabled,
                       EmailFrequency = @EmailFrequency,
                       PushFrequency = @PushFrequency,
                       SmsFrequency = @SmsFrequency,
                       QuietHoursStart = @QuietHoursStart,
                       QuietHoursEnd = @QuietHoursEnd,
                       IsQuietHoursEnabled = @IsQuietHoursEnabled,
                       Language = @Language,
                       TimeZone = @TimeZone,
                       IsSoundEnabled = @IsSoundEnabled,
                       IsActive = @IsActive,
                       IsDefault = @IsDefault,
                       UpdatedAt = @UpdatedAt
                   WHERE Id = @Id"
                : @"UPDATE NotificationSettings 
                   SET NotificationType = @NotificationType,
                       IsEmailEnabled = @IsEmailEnabled,
                       IsPushEnabled = @IsPushEnabled,
                       IsSmsEnabled = @IsSmsEnabled,
                       EmailFrequency = @EmailFrequency,
                       PushFrequency = @PushFrequency,
                       SmsFrequency = @SmsFrequency,
                       QuietHoursStart = @QuietHoursStart,
                       QuietHoursEnd = @QuietHoursEnd,
                       IsQuietHoursEnabled = @IsQuietHoursEnabled,
                       Language = @Language,
                       TimeZone = @TimeZone,
                       IsSoundEnabled = @IsSoundEnabled,
                       IsActive = @IsActive,
                       IsDefault = @IsDefault,
                       UpdatedAt = @UpdatedAt
                   WHERE Id = @Id";

            var parameters = new
            {
                Id = HandleIdConversion(setting.Id),
                NotificationType = (int)setting.NotificationType,
                setting.IsEmailEnabled,
                setting.IsPushEnabled,
                setting.IsSmsEnabled,
                EmailFrequency = (int)setting.EmailFrequency,
                PushFrequency = (int)setting.PushFrequency,
                SmsFrequency = (int)setting.SmsFrequency,
                QuietHoursStart = setting.QuietHoursStart.ToString(),
                QuietHoursEnd = setting.QuietHoursEnd.ToString(),
                setting.IsQuietHoursEnabled,
                setting.Language,
                setting.TimeZone,
                setting.IsSoundEnabled,
                setting.IsActive,
                setting.IsDefault,
                setting.UpdatedAt
            };

            await connection.ExecuteAsync(sql, parameters);
            
            return setting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知设置失败: {SettingId}", setting.Id);
            throw;
        }
    }

    /// <summary>
    /// 删除通知设置
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "DELETE FROM NotificationSettings WHERE Id = @Id"
                : "DELETE FROM NotificationSettings WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = HandleIdConversion(id) });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除通知设置失败: {SettingId}", id);
            throw;
        }
    }

    // 默认设置管理
    /// <summary>
    /// 获取用户的默认通知设置
    /// </summary>
    public async Task<NotificationSetting?> GetDefaultByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND IsDefault = 1"
                : @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND IsDefault = true";

            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { UserId = HandleIdConversion(userId) });
            
            if (result == null)
                return null;

            return MapToNotificationSetting(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户默认通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 设置默认通知设置
    /// </summary>
    public async Task<bool> SetDefaultAsync(Guid userId, Guid settingId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 先取消其他默认设置
                var clearDefaultSql = _isSqlite 
                    ? "UPDATE NotificationSettings SET IsDefault = 0 WHERE UserId = @UserId"
                    : "UPDATE NotificationSettings SET IsDefault = false WHERE UserId = @UserId";

                await connection.ExecuteAsync(clearDefaultSql, new { UserId = HandleIdConversion(userId) }, transaction);

                // 设置新的默认设置
                var setDefaultSql = _isSqlite 
                    ? "UPDATE NotificationSettings SET IsDefault = 1 WHERE Id = @Id AND UserId = @UserId"
                    : "UPDATE NotificationSettings SET IsDefault = true WHERE Id = @Id AND UserId = @UserId";

                var rowsAffected = await connection.ExecuteAsync(setDefaultSql, new { 
                    Id = HandleIdConversion(settingId), 
                    UserId = HandleIdConversion(userId) 
                }, transaction);

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置默认通知设置失败: {UserId}, {SettingId}", userId, settingId);
            throw;
        }
    }

    /// <summary>
    /// 取消默认设置
    /// </summary>
    public async Task<bool> UnsetDefaultAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "UPDATE NotificationSettings SET IsDefault = 0 WHERE UserId = @UserId AND IsDefault = 1"
                : "UPDATE NotificationSettings SET IsDefault = false WHERE UserId = @UserId AND IsDefault = true";

            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = HandleIdConversion(userId) });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消默认通知设置失败: {UserId}", userId);
            throw;
        }
    }

    // 批量操作
    /// <summary>
    /// 批量创建通知设置
    /// </summary>
    public async Task<int> BulkCreateAsync(IEnumerable<NotificationSetting> settings)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"INSERT INTO NotificationSettings 
                   (Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                    EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                    IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                    IsDefault, CreatedAt, UpdatedAt)
                   VALUES 
                   (@Id, @UserId, @NotificationType, @IsEmailEnabled, @IsPushEnabled, @IsSmsEnabled, 
                    @EmailFrequency, @PushFrequency, @SmsFrequency, @QuietHoursStart, @QuietHoursEnd, 
                    @IsQuietHoursEnabled, @Language, @TimeZone, @IsSoundEnabled, @IsActive, 
                    @IsDefault, @CreatedAt, @UpdatedAt)"
                : @"INSERT INTO NotificationSettings 
                   (Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                    EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                    IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                    IsDefault, CreatedAt, UpdatedAt)
                   VALUES 
                   (@Id, @UserId, @NotificationType, @IsEmailEnabled, @IsPushEnabled, @IsSmsEnabled, 
                    @EmailFrequency, @PushFrequency, @SmsFrequency, @QuietHoursStart, @QuietHoursEnd, 
                    @IsQuietHoursEnabled, @Language, @TimeZone, @IsSoundEnabled, @IsActive, 
                    @IsDefault, @CreatedAt, @UpdatedAt)";

            var parametersList = settings.Select(setting => new
            {
                Id = HandleIdConversion(setting.Id),
                UserId = HandleIdConversion(setting.UserId),
                NotificationType = (int)setting.NotificationType,
                setting.IsEmailEnabled,
                setting.IsPushEnabled,
                setting.IsSmsEnabled,
                EmailFrequency = (int)setting.EmailFrequency,
                PushFrequency = (int)setting.PushFrequency,
                SmsFrequency = (int)setting.SmsFrequency,
                QuietHoursStart = setting.QuietHoursStart.ToString(),
                QuietHoursEnd = setting.QuietHoursEnd.ToString(),
                setting.IsQuietHoursEnabled,
                setting.Language,
                setting.TimeZone,
                setting.IsSoundEnabled,
                setting.IsActive,
                setting.IsDefault,
                setting.CreatedAt,
                setting.UpdatedAt
            }).ToList();

            var rowsAffected = await connection.ExecuteAsync(sql, parametersList);
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量创建通知设置失败");
            throw;
        }
    }

    /// <summary>
    /// 批量更新通知设置
    /// </summary>
    public async Task<int> BulkUpdateAsync(IEnumerable<NotificationSetting> settings)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"UPDATE NotificationSettings 
                   SET NotificationType = @NotificationType,
                       IsEmailEnabled = @IsEmailEnabled,
                       IsPushEnabled = @IsPushEnabled,
                       IsSmsEnabled = @IsSmsEnabled,
                       EmailFrequency = @EmailFrequency,
                       PushFrequency = @PushFrequency,
                       SmsFrequency = @SmsFrequency,
                       QuietHoursStart = @QuietHoursStart,
                       QuietHoursEnd = @QuietHoursEnd,
                       IsQuietHoursEnabled = @IsQuietHoursEnabled,
                       Language = @Language,
                       TimeZone = @TimeZone,
                       IsSoundEnabled = @IsSoundEnabled,
                       IsActive = @IsActive,
                       IsDefault = @IsDefault,
                       UpdatedAt = @UpdatedAt
                   WHERE Id = @Id"
                : @"UPDATE NotificationSettings 
                   SET NotificationType = @NotificationType,
                       IsEmailEnabled = @IsEmailEnabled,
                       IsPushEnabled = @IsPushEnabled,
                       IsSmsEnabled = @IsSmsEnabled,
                       EmailFrequency = @EmailFrequency,
                       PushFrequency = @PushFrequency,
                       SmsFrequency = @SmsFrequency,
                       QuietHoursStart = @QuietHoursStart,
                       QuietHoursEnd = @QuietHoursEnd,
                       IsQuietHoursEnabled = @IsQuietHoursEnabled,
                       Language = @Language,
                       TimeZone = @TimeZone,
                       IsSoundEnabled = @IsSoundEnabled,
                       IsActive = @IsActive,
                       IsDefault = @IsDefault,
                       UpdatedAt = @UpdatedAt
                   WHERE Id = @Id";

            var parametersList = settings.Select(setting => new
            {
                Id = HandleIdConversion(setting.Id),
                NotificationType = (int)setting.NotificationType,
                setting.IsEmailEnabled,
                setting.IsPushEnabled,
                setting.IsSmsEnabled,
                EmailFrequency = (int)setting.EmailFrequency,
                PushFrequency = (int)setting.PushFrequency,
                SmsFrequency = (int)setting.SmsFrequency,
                QuietHoursStart = setting.QuietHoursStart.ToString(),
                QuietHoursEnd = setting.QuietHoursEnd.ToString(),
                setting.IsQuietHoursEnabled,
                setting.Language,
                setting.TimeZone,
                setting.IsSoundEnabled,
                setting.IsActive,
                setting.IsDefault,
                setting.UpdatedAt
            }).ToList();

            var rowsAffected = await connection.ExecuteAsync(sql, parametersList);
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新通知设置失败");
            throw;
        }
    }

    /// <summary>
    /// 批量删除通知设置
    /// </summary>
    public async Task<int> BulkDeleteAsync(IEnumerable<Guid> settingIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "DELETE FROM NotificationSettings WHERE Id IN @Ids"
                : "DELETE FROM NotificationSettings WHERE Id IN @Ids";

            var ids = settingIds.Select(HandleIdConversion).ToArray();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = ids });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除通知设置失败");
            throw;
        }
    }

    // 用户偏好管理
    /// <summary>
    /// 获取用户的所有通知偏好设置
    /// </summary>
    public async Task<Dictionary<NotificationType, NotificationSetting>> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            var settings = await GetByUserIdAsync(userId);
            return settings.ToDictionary(s => s.NotificationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知偏好设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户通知偏好设置
    /// </summary>
    public async Task<bool> UpdateUserPreferencesAsync(Guid userId, Dictionary<NotificationType, NotificationSetting> preferences)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var preference in preferences.Values)
                {
                    var existingSetting = await GetByUserIdAndTypeAsync(userId, preference.NotificationType);
                    
                    if (existingSetting != null)
                    {
                        // 更新现有设置
                        preference.Id = existingSetting.Id;
                        preference.UpdatedAt = DateTime.UtcNow;
                        await UpdateAsync(preference);
                    }
                    else
                    {
                        // 创建新设置
                        preference.Id = Guid.NewGuid();
                        preference.UserId = userId;
                        preference.CreatedAt = DateTime.UtcNow;
                        preference.UpdatedAt = DateTime.UtcNow;
                        await CreateAsync(preference);
                    }
                }

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
            _logger.LogError(ex, "更新用户通知偏好设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户对特定类型的通知偏好
    /// </summary>
    public async Task<NotificationSetting> GetUserPreferenceForTypeAsync(Guid userId, NotificationType notificationType)
    {
        try
        {
            var setting = await GetByUserIdAndTypeAsync(userId, notificationType);
            
            if (setting == null)
            {
                // 如果没有找到，返回默认设置
                setting = new NotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = notificationType,
                    IsEmailEnabled = true,
                    IsPushEnabled = true,
                    IsSmsEnabled = false,
                    EmailFrequency = EmailNotificationFrequency.Daily,
                    PushFrequency = NotificationFrequency.Immediate,
                    SmsFrequency = NotificationFrequency.Never,
                    QuietHoursStart = TimeSpan.FromHours(22),
                    QuietHoursEnd = TimeSpan.FromHours(8),
                    IsQuietHoursEnabled = false,
                    Language = "zh-CN",
                    TimeZone = "Asia/Shanghai",
                    IsSoundEnabled = true,
                    IsActive = true,
                    IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            return setting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知偏好失败: {UserId}, {NotificationType}", userId, notificationType);
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
            var setting = await GetByUserIdAndTypeAsync(userId, notificationType);
            
            if (setting == null)
                return true; // 默认启用

            return channel switch
            {
                NotificationChannel.Email => setting.IsEmailEnabled,
                NotificationChannel.Push => setting.IsPushEnabled,
                NotificationChannel.Sms => setting.IsSmsEnabled,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查通知启用状态失败: {UserId}, {NotificationType}, {Channel}", userId, notificationType, channel);
            throw;
        }
    }

    // 频率和时间管理
    /// <summary>
    /// 获取用户的通知频率设置
    /// </summary>
    public async Task<NotificationFrequency> GetNotificationFrequencyAsync(Guid userId, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                return setting?.PushFrequency ?? NotificationFrequency.Immediate;
            }

            // 如果没有指定类型，获取默认设置的频率
            var defaultSetting = await GetDefaultByUserIdAsync(userId);
            return defaultSetting?.PushFrequency ?? NotificationFrequency.Immediate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知频率设置失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的通知频率设置
    /// </summary>
    public async Task<bool> UpdateNotificationFrequencyAsync(Guid userId, NotificationFrequency frequency, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                if (setting == null)
                    return false;

                setting.PushFrequency = frequency;
                setting.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(setting);
            }
            else
            {
                // 更新所有设置的频率
                var settings = await GetByUserIdAsync(userId);
                foreach (var setting in settings)
                {
                    setting.PushFrequency = frequency;
                    setting.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(setting);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新通知频率设置失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的免打扰时间设置
    /// </summary>
    public async Task<(TimeSpan? Start, TimeSpan? End, bool Enabled)> GetQuietHoursAsync(Guid userId)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return (null, null, false);

            return (setting.QuietHoursStart, setting.QuietHoursEnd, setting.IsQuietHoursEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取免打扰时间设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的免打扰时间设置
    /// </summary>
    public async Task<bool> UpdateQuietHoursAsync(Guid userId, TimeSpan? start, TimeSpan? end, bool enabled)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return false;

            setting.QuietHoursStart = start ?? TimeSpan.FromHours(22);
            setting.QuietHoursEnd = end ?? TimeSpan.FromHours(8);
            setting.IsQuietHoursEnabled = enabled;
            setting.UpdatedAt = DateTime.UtcNow;
            
            await UpdateAsync(setting);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新免打扰时间设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 检查当前时间是否在用户的免打扰时间内
    /// </summary>
    public async Task<bool> IsInQuietHoursAsync(Guid userId, DateTime? currentTime = null)
    {
        try
        {
            var (start, end, enabled) = await GetQuietHoursAsync(userId);
            
            if (!enabled || start == null || end == null)
                return false;

            var now = currentTime ?? DateTime.UtcNow;
            var currentTimeSpan = now.TimeOfDay;
            
            // 处理跨天的情况
            if (start.Value > end.Value)
            {
                return currentTimeSpan >= start.Value || currentTimeSpan <= end.Value;
            }
            
            return currentTimeSpan >= start.Value && currentTimeSpan <= end.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查免打扰时间失败: {UserId}", userId);
            throw;
        }
    }

    // 批量和推送设置
    /// <summary>
    /// 获取用户的批量通知设置
    /// </summary>
    public async Task<(bool Enabled, int IntervalMinutes)> GetBatchSettingsAsync(Guid userId)
    {
        try
        {
            // 这里简化处理，实际应用中可能需要专门的批量设置表
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return (false, 60); // 默认禁用，60分钟间隔

            // 根据邮件频率判断是否启用批量通知
            var enabled = setting.EmailFrequency != EmailNotificationFrequency.Immediate;
            var intervalMinutes = setting.EmailFrequency switch
            {
                EmailNotificationFrequency.Hourly => 60,
                EmailNotificationFrequency.Daily => 1440,
                EmailNotificationFrequency.Weekly => 10080,
                _ => 60
            };

            return (enabled, intervalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取批量通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的批量通知设置
    /// </summary>
    public async Task<bool> UpdateBatchSettingsAsync(Guid userId, bool enabled, int intervalMinutes)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return false;

            // 根据间隔时间设置邮件频率
            setting.EmailFrequency = intervalMinutes switch
            {
                <= 60 => EmailNotificationFrequency.Hourly,
                <= 1440 => EmailNotificationFrequency.Daily,
                _ => EmailNotificationFrequency.Weekly
            };

            setting.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(setting);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新批量通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的邮件通知频率设置
    /// </summary>
    public async Task<EmailNotificationFrequency> GetEmailFrequencyAsync(Guid userId)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return EmailNotificationFrequency.Daily;

            return setting.EmailFrequency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取邮件通知频率设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的邮件通知频率设置
    /// </summary>
    public async Task<bool> UpdateEmailFrequencyAsync(Guid userId, EmailNotificationFrequency frequency)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return false;

            setting.EmailFrequency = frequency;
            setting.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(setting);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新邮件通知频率设置失败: {UserId}", userId);
            throw;
        }
    }

    // 渠道管理
    /// <summary>
    /// 启用用户的通知渠道
    /// </summary>
    public async Task<bool> EnableChannelAsync(Guid userId, NotificationChannel channel, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                if (setting == null)
                    return false;

                switch (channel)
                {
                    case NotificationChannel.Email:
                        setting.IsEmailEnabled = true;
                        break;
                    case NotificationChannel.Push:
                        setting.IsPushEnabled = true;
                        break;
                    case NotificationChannel.Sms:
                        setting.IsSmsEnabled = true;
                        break;
                }

                setting.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(setting);
            }
            else
            {
                // 更新所有设置的渠道
                var settings = await GetByUserIdAsync(userId);
                foreach (var setting in settings)
                {
                    switch (channel)
                    {
                        case NotificationChannel.Email:
                            setting.IsEmailEnabled = true;
                            break;
                        case NotificationChannel.Push:
                            setting.IsPushEnabled = true;
                            break;
                        case NotificationChannel.Sms:
                            setting.IsSmsEnabled = true;
                            break;
                    }

                    setting.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(setting);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启用通知渠道失败: {UserId}, {Channel}, {NotificationType}", userId, channel, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 禁用用户的通知渠道
    /// </summary>
    public async Task<bool> DisableChannelAsync(Guid userId, NotificationChannel channel, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                if (setting == null)
                    return false;

                switch (channel)
                {
                    case NotificationChannel.Email:
                        setting.IsEmailEnabled = false;
                        break;
                    case NotificationChannel.Push:
                        setting.IsPushEnabled = false;
                        break;
                    case NotificationChannel.Sms:
                        setting.IsSmsEnabled = false;
                        break;
                }

                setting.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(setting);
            }
            else
            {
                // 更新所有设置的渠道
                var settings = await GetByUserIdAsync(userId);
                foreach (var setting in settings)
                {
                    switch (channel)
                    {
                        case NotificationChannel.Email:
                            setting.IsEmailEnabled = false;
                            break;
                        case NotificationChannel.Push:
                            setting.IsPushEnabled = false;
                            break;
                        case NotificationChannel.Sms:
                            setting.IsSmsEnabled = false;
                            break;
                    }

                    setting.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(setting);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "禁用通知渠道失败: {UserId}, {Channel}, {NotificationType}", userId, channel, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 获取用户启用的通知渠道
    /// </summary>
    public async Task<List<NotificationChannel>> GetEnabledChannelsAsync(Guid userId, NotificationType? notificationType = null)
    {
        try
        {
            var enabledChannels = new List<NotificationChannel>();

            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                if (setting != null)
                {
                    if (setting.IsEmailEnabled) enabledChannels.Add(NotificationChannel.Email);
                    if (setting.IsPushEnabled) enabledChannels.Add(NotificationChannel.Push);
                    if (setting.IsSmsEnabled) enabledChannels.Add(NotificationChannel.Sms);
                }
            }
            else
            {
                var settings = await GetByUserIdAsync(userId);
                var hasEmailEnabled = settings.Any(s => s.IsEmailEnabled);
                var hasPushEnabled = settings.Any(s => s.IsPushEnabled);
                var hasSmsEnabled = settings.Any(s => s.IsSmsEnabled);

                if (hasEmailEnabled) enabledChannels.Add(NotificationChannel.Email);
                if (hasPushEnabled) enabledChannels.Add(NotificationChannel.Push);
                if (hasSmsEnabled) enabledChannels.Add(NotificationChannel.Sms);
            }

            return enabledChannels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取启用通知渠道失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    // 语言和时区设置
    /// <summary>
    /// 获取用户的通知语言设置
    /// </summary>
    public async Task<string> GetLanguageAsync(Guid userId)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return "zh-CN"; // 默认中文

            return setting.Language ?? "zh-CN";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取语言设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的通知语言设置
    /// </summary>
    public async Task<bool> UpdateLanguageAsync(Guid userId, string language)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return false;

            setting.Language = language;
            setting.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(setting);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新语言设置失败: {UserId}, {Language}", userId, language);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的时区设置
    /// </summary>
    public async Task<string> GetTimeZoneAsync(Guid userId)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return "Asia/Shanghai"; // 默认上海时区

            return setting.TimeZone ?? "Asia/Shanghai";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取时区设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的时区设置
    /// </summary>
    public async Task<bool> UpdateTimeZoneAsync(Guid userId, string timeZone)
    {
        try
        {
            var setting = await GetDefaultByUserIdAsync(userId);
            
            if (setting == null)
                return false;

            setting.TimeZone = timeZone;
            setting.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(setting);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新时区设置失败: {UserId}, {TimeZone}", userId, timeZone);
            throw;
        }
    }

    // 声音和提醒设置
    /// <summary>
    /// 检查用户是否启用声音提醒
    /// </summary>
    public async Task<bool> IsSoundEnabledAsync(Guid userId, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                return setting?.IsSoundEnabled ?? true;
            }

            var defaultSetting = await GetDefaultByUserIdAsync(userId);
            return defaultSetting?.IsSoundEnabled ?? true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查声音提醒设置失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 更新用户的声音提醒设置
    /// </summary>
    public async Task<bool> UpdateSoundSettingsAsync(Guid userId, bool enabled, NotificationType? notificationType = null)
    {
        try
        {
            if (notificationType.HasValue)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType.Value);
                if (setting == null)
                    return false;

                setting.IsSoundEnabled = enabled;
                setting.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(setting);
            }
            else
            {
                // 更新所有设置的声音设置
                var settings = await GetByUserIdAsync(userId);
                foreach (var setting in settings)
                {
                    setting.IsSoundEnabled = enabled;
                    setting.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(setting);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新声音提醒设置失败: {UserId}, {Enabled}, {NotificationType}", userId, enabled, notificationType);
            throw;
        }
    }

    // 状态管理
    /// <summary>
    /// 激活通知设置
    /// </summary>
    public async Task<bool> ActivateAsync(Guid settingId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "UPDATE NotificationSettings SET IsActive = 1 WHERE Id = @Id"
                : "UPDATE NotificationSettings SET IsActive = true WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = HandleIdConversion(settingId) });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "激活通知设置失败: {SettingId}", settingId);
            throw;
        }
    }

    /// <summary>
    /// 停用通知设置
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid settingId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "UPDATE NotificationSettings SET IsActive = 0 WHERE Id = @Id"
                : "UPDATE NotificationSettings SET IsActive = false WHERE Id = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = HandleIdConversion(settingId) });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停用通知设置失败: {SettingId}", settingId);
            throw;
        }
    }

    /// <summary>
    /// 获取激活的通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSetting>> GetActiveSettingsAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND IsActive = 1"
                : @"SELECT Id, UserId, NotificationType, IsEmailEnabled, IsPushEnabled, IsSmsEnabled, 
                          EmailFrequency, PushFrequency, SmsFrequency, QuietHoursStart, QuietHoursEnd, 
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled, IsActive, 
                          IsDefault, CreatedAt, UpdatedAt
                   FROM NotificationSettings 
                   WHERE UserId = @UserId AND IsActive = true";

            var results = await connection.QueryAsync<dynamic>(sql, new { UserId = HandleIdConversion(userId) });
            
            return results.Select(MapToNotificationSetting).Where(x => x != null).Cast<NotificationSetting>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取激活通知设置失败: {UserId}", userId);
            throw;
        }
    }

    // 缓存相关
    /// <summary>
    /// 从缓存获取通知设置
    /// </summary>
    public async Task<NotificationSetting?> GetFromCacheAsync(Guid userId, NotificationType? notificationType)
    {
        try
        {
            // 简化实现，直接从数据库获取
            if (notificationType.HasValue)
            {
                return await GetByUserIdAndTypeAsync(userId, notificationType.Value);
            }
            
            return await GetDefaultByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从缓存获取通知设置失败: {UserId}, {NotificationType}", userId, notificationType);
            throw;
        }
    }

    /// <summary>
    /// 设置通知设置缓存
    /// </summary>
    public async Task<bool> SetCacheAsync(NotificationSetting setting, TimeSpan? expiration = null)
    {
        try
        {
            // 简化实现，直接返回成功
            _logger.LogInformation("设置通知设置缓存: {UserId}, {NotificationType}", setting.UserId, setting.NotificationType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置通知设置缓存失败: {UserId}, {NotificationType}", setting.UserId, setting.NotificationType);
            return false;
        }
    }

    /// <summary>
    /// 移除通知设置缓存
    /// </summary>
    public async Task<bool> RemoveCacheAsync(Guid userId, NotificationType? notificationType)
    {
        try
        {
            // 简化实现，直接返回成功
            _logger.LogInformation("移除通知设置缓存: {UserId}, {NotificationType}", userId, notificationType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除通知设置缓存失败: {UserId}, {NotificationType}", userId, notificationType);
            return false;
        }
    }

    /// <summary>
    /// 清空用户的所有通知设置缓存
    /// </summary>
    public async Task<bool> ClearUserCacheAsync(Guid userId)
    {
        try
        {
            // 简化实现，直接返回成功
            _logger.LogInformation("清空用户通知设置缓存: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空用户通知设置缓存失败: {UserId}", userId);
            return false;
        }
    }

    // 默认设置初始化
    /// <summary>
    /// 为用户初始化默认通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSetting>> InitializeDefaultSettingsAsync(Guid userId)
    {
        try
        {
            var defaultSettings = new List<NotificationSetting>();
            var now = DateTime.UtcNow;

            // 为每种通知类型创建默认设置
            foreach (NotificationType notificationType in Enum.GetValues(typeof(NotificationType)))
            {
                var setting = new NotificationSetting
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = notificationType,
                    IsEmailEnabled = true,
                    IsPushEnabled = true,
                    IsSmsEnabled = false,
                    EmailFrequency = EmailNotificationFrequency.Daily,
                    PushFrequency = NotificationFrequency.Immediate,
                    SmsFrequency = NotificationFrequency.Never,
                    QuietHoursStart = TimeSpan.FromHours(22),
                    QuietHoursEnd = TimeSpan.FromHours(8),
                    IsQuietHoursEnabled = false,
                    Language = "zh-CN",
                    TimeZone = "Asia/Shanghai",
                    IsSoundEnabled = true,
                    IsActive = true,
                    IsDefault = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                defaultSettings.Add(setting);
            }

            // 批量创建设置
            await BulkCreateAsync(defaultSettings);

            return defaultSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化默认通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否已初始化通知设置
    /// </summary>
    public async Task<bool> HasInitializedSettingsAsync(Guid userId)
    {
        try
        {
            var settings = await GetByUserIdAsync(userId);
            return settings.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查通知设置初始化状态失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取系统默认通知设置
    /// </summary>
    public async Task<NotificationSetting> GetSystemDefaultSettingsAsync()
    {
        try
        {
            return new NotificationSetting
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Empty, // 系统默认设置
                NotificationType = NotificationType.System,
                IsEmailEnabled = true,
                IsPushEnabled = true,
                IsSmsEnabled = false,
                EmailFrequency = EmailNotificationFrequency.Daily,
                PushFrequency = NotificationFrequency.Immediate,
                SmsFrequency = NotificationFrequency.Never,
                QuietHoursStart = TimeSpan.FromHours(22),
                QuietHoursEnd = TimeSpan.FromHours(8),
                IsQuietHoursEnabled = false,
                Language = "zh-CN",
                TimeZone = "Asia/Shanghai",
                IsSoundEnabled = true,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统默认通知设置失败");
            throw;
        }
    }

    // 统计和分析
    /// <summary>
    /// 获取用户通知设置统计信息
    /// </summary>
    public async Task<NotificationSettingsStatsDto> GetSettingsStatsAsync(Guid userId)
    {
        try
        {
            var settings = await GetByUserIdAsync(userId);
            var stats = new NotificationSettingsStatsDto
            {
                TotalSettings = settings.Count(),
                ActiveSettings = settings.Count(s => s.IsActive),
                DefaultSettings = settings.Count(s => s.IsDefault),
                TypeStats = settings.GroupBy(s => s.NotificationType).ToDictionary(g => g.Key, g => g.Count()),
                ChannelStats = new Dictionary<NotificationChannel, int>
                {
                    { NotificationChannel.Email, settings.Count(s => s.IsEmailEnabled) },
                    { NotificationChannel.Push, settings.Count(s => s.IsPushEnabled) },
                    { NotificationChannel.Sms, settings.Count(s => s.IsSmsEnabled) }
                },
                FrequencyStats = settings.GroupBy(s => s.PushFrequency).ToDictionary(g => g.Key, g => g.Count()),
                QuietHoursEnabled = settings.Count(s => s.IsQuietHoursEnabled),
                BatchNotificationsEnabled = settings.Count(s => s.EmailFrequency != EmailNotificationFrequency.Immediate),
                LastUpdated = DateTime.UtcNow
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户通知设置统计信息失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 获取系统通知设置统计信息
    /// </summary>
    public async Task<SystemNotificationSettingsStatsDto> GetSystemSettingsStatsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT COUNT(DISTINCT UserId) as TotalUsers,
                          COUNT(CASE WHEN Id IS NOT NULL THEN 1 END) as UsersWithSettings,
                          Language, TimeZone, IsQuietHoursEnabled, IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                          EmailFrequency, PushFrequency
                   FROM NotificationSettings
                   GROUP BY Language, TimeZone, IsQuietHoursEnabled, IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                            EmailFrequency, PushFrequency"
                : @"SELECT COUNT(DISTINCT UserId) as TotalUsers,
                          COUNT(CASE WHEN Id IS NOT NULL THEN 1 END) as UsersWithSettings,
                          Language, TimeZone, IsQuietHoursEnabled, IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                          EmailFrequency, PushFrequency
                   FROM NotificationSettings
                   GROUP BY Language, TimeZone, IsQuietHoursEnabled, IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                            EmailFrequency, PushFrequency";

            var results = await connection.QueryAsync<dynamic>(sql);
            
            var totalUsers = results.Sum(r => (int)r.TotalUsers);
            var usersWithSettings = results.Sum(r => (int)r.UsersWithSettings);
            
            // 计算最受欢迎的渠道
            var emailCount = results.Sum(r => ParseBool(r.IsEmailEnabled) ? 1 : 0);
            var pushCount = results.Sum(r => ParseBool(r.IsPushEnabled) ? 1 : 0);
            var smsCount = results.Sum(r => ParseBool(r.IsSmsEnabled) ? 1 : 0);
            
            var mostPopularChannel = emailCount > pushCount ? 
                (emailCount > smsCount ? NotificationChannel.Email : NotificationChannel.Sms) :
                (pushCount > smsCount ? NotificationChannel.Push : NotificationChannel.Sms);

            // 计算最受欢迎的频率
            var frequencyGroups = results.GroupBy(r => (NotificationFrequency)r.PushFrequency);
            var mostPopularFrequency = frequencyGroups.OrderByDescending(g => g.Count()).First().Key;

            var stats = new SystemNotificationSettingsStatsDto
            {
                TotalUsers = totalUsers,
                UsersWithSettings = usersWithSettings,
                MostPopularChannel = mostPopularChannel,
                MostPopularFrequency = mostPopularFrequency,
                QuietHoursAdoptionRate = results.Count(r => ParseBool(r.IsQuietHoursEnabled)) / (double)totalUsers,
                BatchNotificationsAdoptionRate = results.Count(r => (EmailNotificationFrequency)r.EmailFrequency != EmailNotificationFrequency.Immediate) / (double)totalUsers,
                LanguageDistribution = results.GroupBy(r => r.Language).ToDictionary(g => g.Key, g => g.Count()),
                TimeZoneDistribution = results.GroupBy(r => r.TimeZone).ToDictionary(g => g.Key, g => g.Count())
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统通知设置统计信息失败");
            throw;
        }
    }

    /// <summary>
    /// 获取最常用的通知设置组合
    /// </summary>
    public async Task<IEnumerable<SettingsCombinationStatsDto>> GetPopularSettingsCombinationsAsync(int limit = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? @"SELECT 
                          IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                          EmailFrequency, PushFrequency, SmsFrequency,
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled,
                          COUNT(*) as UserCount
                   FROM NotificationSettings
                   WHERE IsActive = 1
                   GROUP BY IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                            EmailFrequency, PushFrequency, SmsFrequency,
                            IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled
                   ORDER BY UserCount DESC
                   LIMIT @Limit"
                : @"SELECT 
                          IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                          EmailFrequency, PushFrequency, SmsFrequency,
                          IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled,
                          COUNT(*) as UserCount
                   FROM NotificationSettings
                   WHERE IsActive = true
                   GROUP BY IsEmailEnabled, IsPushEnabled, IsSmsEnabled,
                            EmailFrequency, PushFrequency, SmsFrequency,
                            IsQuietHoursEnabled, Language, TimeZone, IsSoundEnabled
                   ORDER BY UserCount DESC
                   LIMIT @Limit";

            var results = await connection.QueryAsync<dynamic>(sql, new { Limit = limit });
            var totalUsers = await GetTotalUsersCountAsync();

            var combinations = results.Select(r => new SettingsCombinationStatsDto
            {
                Combination = $"Email:{ParseBool(r.IsEmailEnabled)},Push:{ParseBool(r.IsPushEnabled)},Sms:{ParseBool(r.IsSmsEnabled)}",
                UserCount = (int)r.UserCount,
                Percentage = (int)r.UserCount / (double)totalUsers * 100,
                Settings = new Dictionary<string, object>
                {
                    { "IsEmailEnabled", ParseBool(r.IsEmailEnabled) },
                    { "IsPushEnabled", ParseBool(r.IsPushEnabled) },
                    { "IsSmsEnabled", ParseBool(r.IsSmsEnabled) },
                    { "EmailFrequency", (EmailNotificationFrequency)r.EmailFrequency },
                    { "PushFrequency", (NotificationFrequency)r.PushFrequency },
                    { "SmsFrequency", (NotificationFrequency)r.SmsFrequency },
                    { "IsQuietHoursEnabled", ParseBool(r.IsQuietHoursEnabled) },
                    { "Language", r.Language },
                    { "TimeZone", r.TimeZone },
                    { "IsSoundEnabled", ParseBool(r.IsSoundEnabled) }
                }
            });

            return combinations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取常用通知设置组合失败");
            throw;
        }
    }

    // 导入导出
    /// <summary>
    /// 导出用户通知设置
    /// </summary>
    public async Task<IEnumerable<NotificationSetting>> ExportUserSettingsAsync(Guid userId)
    {
        try
        {
            return await GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出用户通知设置失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 导入用户通知设置
    /// </summary>
    public async Task<int> ImportUserSettingsAsync(Guid userId, IEnumerable<NotificationSetting> settings, bool overrideExisting = false)
    {
        try
        {
            var importedCount = 0;
            var existingSettings = await GetByUserIdAsync(userId);

            foreach (var setting in settings)
            {
                var existingSetting = existingSettings.FirstOrDefault(s => s.NotificationType == setting.NotificationType);
                
                if (existingSetting != null && overrideExisting)
                {
                    // 更新现有设置
                    setting.Id = existingSetting.Id;
                    setting.UserId = userId;
                    setting.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(setting);
                    importedCount++;
                }
                else if (existingSetting == null)
                {
                    // 创建新设置
                    setting.Id = Guid.NewGuid();
                    setting.UserId = userId;
                    setting.CreatedAt = DateTime.UtcNow;
                    setting.UpdatedAt = DateTime.UtcNow;
                    await CreateAsync(setting);
                    importedCount++;
                }
            }

            return importedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入用户通知设置失败: {UserId}", userId);
            throw;
        }
    }

    // 权限验证
    /// <summary>
    /// 检查用户是否可以访问通知设置
    /// </summary>
    public async Task<bool> CanUserAccessAsync(Guid settingId, Guid userId)
    {
        try
        {
            var setting = await GetByIdAsync(settingId);
            return setting != null && setting.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查设置访问权限失败: {SettingId}, {UserId}", settingId, userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否可以编辑通知设置
    /// </summary>
    public async Task<bool> CanUserEditAsync(Guid settingId, Guid userId)
    {
        try
        {
            var setting = await GetByIdAsync(settingId);
            return setting != null && setting.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查设置编辑权限失败: {SettingId}, {UserId}", settingId, userId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否可以删除通知设置
    /// </summary>
    public async Task<bool> CanUserDeleteAsync(Guid settingId, Guid userId)
    {
        try
        {
            var setting = await GetByIdAsync(settingId);
            return setting != null && setting.UserId == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查设置删除权限失败: {SettingId}, {UserId}", settingId, userId);
            throw;
        }
    }

    // 性能优化
    /// <summary>
    /// 预加载用户通知设置
    /// </summary>
    public async Task<Dictionary<Guid, IEnumerable<NotificationSetting>>> PreloadUsersSettingsAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            var result = new Dictionary<Guid, IEnumerable<NotificationSetting>>();
            
            foreach (var userId in userIds)
            {
                var settings = await GetByUserIdAsync(userId);
                result[userId] = settings;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预加载用户通知设置失败");
            throw;
        }
    }

    /// <summary>
    /// 批量获取用户通知偏好
    /// </summary>
    public async Task<Dictionary<Guid, NotificationSetting>> BulkGetUserPreferencesAsync(IEnumerable<Guid> userIds, NotificationType notificationType)
    {
        try
        {
            var result = new Dictionary<Guid, NotificationSetting>();
            
            foreach (var userId in userIds)
            {
                var setting = await GetByUserIdAndTypeAsync(userId, notificationType);
                if (setting != null)
                {
                    result[userId] = setting;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取用户通知偏好失败");
            throw;
        }
    }

    /// <summary>
    /// 批量更新通知设置状态
    /// </summary>
    public async Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> settingIds, bool isActive)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "UPDATE NotificationSettings SET IsActive = @IsActive WHERE Id IN @Ids"
                : "UPDATE NotificationSettings SET IsActive = @IsActive WHERE Id IN @Ids";

            var ids = settingIds.Select(HandleIdConversion).ToArray();
            var rowsAffected = await connection.ExecuteAsync(sql, new { IsActive = isActive ? 1 : 0, Ids = ids });
            
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新通知设置状态失败");
            throw;
        }
    }

    // 私有辅助方法
    /// <summary>
    /// 获取总用户数
    /// </summary>
    private async Task<int> GetTotalUsersCountAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = _isSqlite 
                ? "SELECT COUNT(DISTINCT UserId) FROM NotificationSettings"
                : "SELECT COUNT(DISTINCT UserId) FROM NotificationSettings";

            return await connection.QuerySingleAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取总用户数失败");
            throw;
        }
    }

    /// <summary>
    /// 将数据库结果映射到NotificationSetting实体
    /// </summary>
    private NotificationSetting MapToNotificationSetting(dynamic result)
    {
        try
        {
            return new NotificationSetting
            {
                Id = _isSqlite ? Guid.Parse(result.Id) : result.Id,
                UserId = _isSqlite ? Guid.Parse(result.UserId) : result.UserId,
                NotificationType = (NotificationType)ParseInt(result.NotificationType),
                IsEmailEnabled = ParseBool(result.IsEmailEnabled),
                IsPushEnabled = ParseBool(result.IsPushEnabled),
                IsSmsEnabled = ParseBool(result.IsSmsEnabled),
                EmailFrequency = (EmailNotificationFrequency)ParseInt(result.EmailFrequency),
                PushFrequency = (NotificationFrequency)ParseInt(result.PushFrequency),
                SmsFrequency = (NotificationFrequency)ParseInt(result.SmsFrequency),
                QuietHoursStart = ParseTimeSpan(result.QuietHoursStart) ?? TimeSpan.FromHours(22),
                QuietHoursEnd = ParseTimeSpan(result.QuietHoursEnd) ?? TimeSpan.FromHours(8),
                IsQuietHoursEnabled = ParseBool(result.IsQuietHoursEnabled),
                Language = result.Language,
                TimeZone = result.TimeZone,
                IsSoundEnabled = ParseBool(result.IsSoundEnabled),
                IsActive = ParseBool(result.IsActive),
                IsDefault = ParseBool(result.IsDefault),
                CreatedAt = ParseDateTime(result.CreatedAt),
                UpdatedAt = ParseDateTime(result.UpdatedAt)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "映射通知设置实体失败");
            throw;
        }
    }
}