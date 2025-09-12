using Dapper;
using System.Text.Json;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using System.Data;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 系统设置仓储实现 - 遵循单一职责原则，负责系统设置的数据访问
/// </summary>
public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SystemSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<SystemSettings?> GetSettingsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SystemSettings LIMIT 1";
        
        return await connection.QuerySingleOrDefaultAsync<SystemSettings>(sql);
    }

    public async Task<SystemSettings> SaveSettingsAsync(SystemSettings settings)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // 检查是否已存在设置记录
        const string checkSql = "SELECT COUNT(*) FROM SystemSettings";
        var count = await connection.ExecuteScalarAsync<int>(checkSql);
        
        if (count == 0)
        {
            // 插入新记录
            const string insertSql = @"
                INSERT INTO SystemSettings (Id, CreatedAt, UpdatedAt, UpdatedBy, 
                    SiteSettingsJson, SecuritySettingsJson, FeatureSettingsJson, EmailSettingsJson)
                VALUES (@Id, @CreatedAt, @UpdatedAt, @UpdatedBy, 
                    @SiteSettingsJson, @SecuritySettingsJson, @FeatureSettingsJson, @EmailSettingsJson)";
            
            await connection.ExecuteAsync(insertSql, settings);
        }
        else
        {
            // 更新现有记录
            const string updateSql = @"
                UPDATE SystemSettings 
                SET UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy,
                    SiteSettingsJson = @SiteSettingsJson, 
                    SecuritySettingsJson = @SecuritySettingsJson,
                    FeatureSettingsJson = @FeatureSettingsJson, 
                    EmailSettingsJson = @EmailSettingsJson";
            
            await connection.ExecuteAsync(updateSql, settings);
        }
        
        return settings;
    }

    public async Task<SystemSettings> UpdateSiteSettingsAsync(SiteSettings siteSettings, string updatedBy)
    {
        var settings = await GetSettingsAsync() ?? await InitializeDefaultSettingsAsync();
        
        // 记录变更历史
        var oldSettings = settings.GetSiteSettings();
        await RecordSettingsChangeAsync("Site", "SiteSettings", 
            JsonSerializer.Serialize(oldSettings), 
            JsonSerializer.Serialize(siteSettings), updatedBy);
        
        settings.SetSiteSettings(siteSettings);
        settings.UpdatedBy = updatedBy;
        
        return await SaveSettingsAsync(settings);
    }

    public async Task<SystemSettings> UpdateSecuritySettingsAsync(SecuritySettings securitySettings, string updatedBy)
    {
        var settings = await GetSettingsAsync() ?? await InitializeDefaultSettingsAsync();
        
        // 记录变更历史
        var oldSettings = settings.GetSecuritySettings();
        await RecordSettingsChangeAsync("Security", "SecuritySettings", 
            JsonSerializer.Serialize(oldSettings), 
            JsonSerializer.Serialize(securitySettings), updatedBy);
        
        settings.SetSecuritySettings(securitySettings);
        settings.UpdatedBy = updatedBy;
        
        return await SaveSettingsAsync(settings);
    }

    public async Task<SystemSettings> UpdateFeatureSettingsAsync(FeatureSettings featureSettings, string updatedBy)
    {
        var settings = await GetSettingsAsync() ?? await InitializeDefaultSettingsAsync();
        
        // 记录变更历史
        var oldSettings = settings.GetFeatureSettings();
        await RecordSettingsChangeAsync("Feature", "FeatureSettings", 
            JsonSerializer.Serialize(oldSettings), 
            JsonSerializer.Serialize(featureSettings), updatedBy);
        
        settings.SetFeatureSettings(featureSettings);
        settings.UpdatedBy = updatedBy;
        
        return await SaveSettingsAsync(settings);
    }

    public async Task<SystemSettings> UpdateEmailSettingsAsync(EmailSettings emailSettings, string updatedBy)
    {
        var settings = await GetSettingsAsync() ?? await InitializeDefaultSettingsAsync();
        
        // 记录变更历史
        var oldSettings = settings.GetEmailSettings();
        await RecordSettingsChangeAsync("Email", "EmailSettings", 
            JsonSerializer.Serialize(oldSettings), 
            JsonSerializer.Serialize(emailSettings), updatedBy);
        
        settings.SetEmailSettings(emailSettings);
        settings.UpdatedBy = updatedBy;
        
        return await SaveSettingsAsync(settings);
    }

    public async Task<SettingsHistory> RecordChangeHistoryAsync(CreateSettingsHistoryRequest request)
    {
        var history = new SettingsHistory
        {
            SettingType = request.SettingType,
            SettingKey = request.SettingKey,
            OldValue = request.OldValue,
            NewValue = request.NewValue,
            ChangedBy = request.ChangedBy,
            ChangedById = request.ChangedById,
            ChangeReason = request.ChangeReason,
            ChangeCategory = request.ChangeCategory,
            ClientIp = request.ClientIp,
            UserAgent = request.UserAgent,
            IsImportant = request.IsImportant,
            Status = request.Status,
            ErrorMessage = request.ErrorMessage,
            Metadata = request.Metadata
        };

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO SettingsHistory (Id, CreatedAt, SettingType, SettingKey, OldValue, NewValue, 
                ChangedBy, ChangedById, ChangeReason, ChangeCategory, ClientIp, UserAgent, 
                IsImportant, Status, ErrorMessage, Metadata)
            VALUES (@Id, @CreatedAt, @SettingType, @SettingKey, @OldValue, @NewValue, 
                @ChangedBy, @ChangedById, @ChangeReason, @ChangeCategory, @ClientIp, @UserAgent, 
                @IsImportant, @Status, @ErrorMessage, @Metadata)";
        
        await connection.ExecuteAsync(sql, history);
        return history;
    }

    public async Task<List<SettingsHistory>> GetChangeHistoryAsync(SettingsHistoryRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var conditions = new List<string>();
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(request.SettingType))
        {
            conditions.Add("SettingType = @SettingType");
            parameters.Add("SettingType", request.SettingType);
        }
        
        if (!string.IsNullOrEmpty(request.ChangeCategory))
        {
            conditions.Add("ChangeCategory = @ChangeCategory");
            parameters.Add("ChangeCategory", request.ChangeCategory);
        }
        
        if (!string.IsNullOrEmpty(request.ChangedBy))
        {
            conditions.Add("ChangedBy LIKE @ChangedBy");
            parameters.Add("ChangedBy", $"%{request.ChangedBy}%");
        }
        
        if (request.StartDate.HasValue)
        {
            conditions.Add("CreatedAt >= @StartDate");
            parameters.Add("StartDate", request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            conditions.Add("CreatedAt <= @EndDate");
            parameters.Add("EndDate", request.EndDate.Value);
        }
        
        if (request.IsImportant.HasValue)
        {
            conditions.Add("IsImportant = @IsImportant");
            parameters.Add("IsImportant", request.IsImportant.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            conditions.Add("Status = @Status");
            parameters.Add("Status", request.Status);
        }
        
        if (!string.IsNullOrEmpty(request.SettingKey))
        {
            conditions.Add("SettingKey LIKE @SettingKey");
            parameters.Add("SettingKey", $"%{request.SettingKey}%");
        }
        
        var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        
        var sortBy = request.SortBy.ToLower() switch
        {
            "settingtype" => "SettingType",
            "changedby" => "ChangedBy",
            "category" => "ChangeCategory",
            _ => "CreatedAt"
        };
        
        var sortDirection = request.SortDirection.ToLower() == "asc" ? "ASC" : "DESC";
        
        var offset = (request.PageNumber - 1) * request.PageSize;
        
        var sql = $@"
            SELECT * FROM SettingsHistory 
            {whereClause}
            ORDER BY {sortBy} {sortDirection}
            LIMIT @Limit OFFSET @Offset";
        
        parameters.Add("Limit", request.PageSize);
        parameters.Add("Offset", offset);
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, parameters);
        return result.ToList();
    }

    public async Task<SettingsHistory?> GetHistoryByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT Id, CreatedAt, SettingType, SettingKey, OldValue, NewValue, 
                   ChangedBy, ChangedById, ChangeReason, ChangeCategory, ClientIp, 
                   UserAgent, IsImportant, Status, ErrorMessage, Metadata
            FROM SettingsHistory 
            WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<SettingsHistory>(sql, new { Id = id });
    }

    public async Task<SettingsHistoryStatistics> GetChangeHistoryStatisticsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                COUNT(*) as TotalChanges,
                SUM(CASE WHEN DATE(CreatedAt) = DATE('now') THEN 1 ELSE 0 END) as TodayChanges,
                SUM(CASE WHEN CreatedAt >= datetime('now', '-7 days') THEN 1 ELSE 0 END) as ThisWeekChanges,
                SUM(CASE WHEN CreatedAt >= datetime('now', '-30 days') THEN 1 ELSE 0 END) as ThisMonthChanges,
                SUM(CASE WHEN IsImportant = 1 THEN 1 ELSE 0 END) as ImportantChanges,
                SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as FailedChanges,
                MAX(CreatedAt) as LastChangeTime
            FROM SettingsHistory";
        
        var stats = await connection.QuerySingleAsync<SettingsHistoryStatistics>(sql);
        
        // 获取按设置类型统计
        const string typeStatsSql = @"
            SELECT SettingType, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY SettingType";
        
        var typeStats = await connection.QueryAsync<dynamic>(typeStatsSql);
        stats.ChangesBySettingType = typeStats.ToDictionary(x => (string)x.SettingType, x => (int)x.Count);
        
        // 获取按变更分类统计
        const string categoryStatsSql = @"
            SELECT ChangeCategory, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY ChangeCategory";
        
        var categoryStats = await connection.QueryAsync<dynamic>(categoryStatsSql);
        stats.ChangesByCategory = categoryStats.ToDictionary(x => (string)x.ChangeCategory, x => (int)x.Count);
        
        // 获取按操作人统计
        const string userStatsSql = @"
            SELECT ChangedBy, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY ChangedBy 
            ORDER BY Count DESC 
            LIMIT 10";
        
        var userStats = await connection.QueryAsync<dynamic>(userStatsSql);
        stats.ChangesByUser = userStats.ToDictionary(x => (string)x.ChangedBy, x => (int)x.Count);
        
        // 获取最活跃的操作人
        if (userStats.Any())
        {
            stats.MostActiveUser = userStats.First().ChangedBy;
        }
        
        // 获取最常变更的设置类型
        if (typeStats.Any())
        {
            stats.MostChangedSettingType = typeStats.First().SettingType;
        }
        
        return stats;
    }

    public async Task<SystemSettings> InitializeDefaultSettingsAsync()
    {
        var defaultSettings = new SystemSettings
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "System"
        };
        
        // 设置默认值
        defaultSettings.SetSiteSettings(new SiteSettings());
        defaultSettings.SetSecuritySettings(new SecuritySettings());
        defaultSettings.SetFeatureSettings(new FeatureSettings());
        defaultSettings.SetEmailSettings(new EmailSettings());
        
        return await SaveSettingsAsync(defaultSettings);
    }

    public async Task<bool> SettingsExistAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM SystemSettings";
        
        var count = await connection.ExecuteScalarAsync<int>(sql);
        return count > 0;
    }

    public async Task<bool> DeleteChangeHistoryAsync(Guid historyId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = historyId });
        return rowsAffected > 0;
    }

    public async Task<int> BatchDeleteChangeHistoryAsync(List<Guid> historyIds)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE Id IN @Ids";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = historyIds });
        return rowsAffected;
    }

    public async Task<int> CleanExpiredChangeHistoryAsync(DateTime cutoffDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE CreatedAt < @CutoffDate";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
        return rowsAffected;
    }

    /// <summary>
    /// 记录设置变更的私有方法
    /// </summary>
    private async Task RecordSettingsChangeAsync(string settingType, string settingKey, string oldValue, string newValue, string changedBy)
    {
        var request = new CreateSettingsHistoryRequest
        {
            SettingType = settingType,
            SettingKey = settingKey,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangeReason = "系统设置更新",
            ChangeCategory = "System",
            IsImportant = false,
            Status = "Success"
        };
        
        await RecordChangeHistoryAsync(request);
    }
}

/// <summary>
/// 设置变更历史仓储实现 - 遵循单一职责原则，负责设置变更历史的数据访问
/// </summary>
public class SettingsHistoryRepository : ISettingsHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SettingsHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<SettingsHistory> CreateAsync(SettingsHistory history)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO SettingsHistory (Id, CreatedAt, SettingType, SettingKey, OldValue, NewValue, 
                ChangedBy, ChangedById, ChangeReason, ChangeCategory, ClientIp, UserAgent, 
                IsImportant, Status, ErrorMessage, Metadata)
            VALUES (@Id, @CreatedAt, @SettingType, @SettingKey, @OldValue, @NewValue, 
                @ChangedBy, @ChangedById, @ChangeReason, @ChangeCategory, @ClientIp, @UserAgent, 
                @IsImportant, @Status, @ErrorMessage, @Metadata)";
        
        await connection.ExecuteAsync(sql, history);
        return history;
    }

    public async Task<SettingsHistory?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SettingsHistory WHERE Id = @Id";
        
        return await connection.QuerySingleOrDefaultAsync<SettingsHistory>(sql, new { Id = id });
    }

    public async Task<List<SettingsHistory>> GetByFilterAsync(SettingsHistoryRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var conditions = new List<string>();
        var parameters = new DynamicParameters();
        
        // 构建查询条件
        if (!string.IsNullOrEmpty(request.SettingType))
        {
            conditions.Add("SettingType = @SettingType");
            parameters.Add("SettingType", request.SettingType);
        }
        
        if (!string.IsNullOrEmpty(request.ChangeCategory))
        {
            conditions.Add("ChangeCategory = @ChangeCategory");
            parameters.Add("ChangeCategory", request.ChangeCategory);
        }
        
        if (!string.IsNullOrEmpty(request.ChangedBy))
        {
            conditions.Add("ChangedBy LIKE @ChangedBy");
            parameters.Add("ChangedBy", $"%{request.ChangedBy}%");
        }
        
        if (request.StartDate.HasValue)
        {
            conditions.Add("CreatedAt >= @StartDate");
            parameters.Add("StartDate", request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            conditions.Add("CreatedAt <= @EndDate");
            parameters.Add("EndDate", request.EndDate.Value);
        }
        
        if (request.IsImportant.HasValue)
        {
            conditions.Add("IsImportant = @IsImportant");
            parameters.Add("IsImportant", request.IsImportant.Value);
        }
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            conditions.Add("Status = @Status");
            parameters.Add("Status", request.Status);
        }
        
        if (!string.IsNullOrEmpty(request.SettingKey))
        {
            conditions.Add("SettingKey LIKE @SettingKey");
            parameters.Add("SettingKey", $"%{request.SettingKey}%");
        }
        
        var whereClause = conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        
        var sortBy = request.SortBy.ToLower() switch
        {
            "settingtype" => "SettingType",
            "changedby" => "ChangedBy",
            "category" => "ChangeCategory",
            _ => "CreatedAt"
        };
        
        var sortDirection = request.SortDirection.ToLower() == "asc" ? "ASC" : "DESC";
        
        var offset = (request.PageNumber - 1) * request.PageSize;
        
        var sql = $@"
            SELECT * FROM SettingsHistory 
            {whereClause}
            ORDER BY {sortBy} {sortDirection}
            LIMIT @Limit OFFSET @Offset";
        
        parameters.Add("Limit", request.PageSize);
        parameters.Add("Offset", offset);
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, parameters);
        return result.ToList();
    }

    public async Task<SettingsHistoryStatistics> GetStatisticsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                COUNT(*) as TotalChanges,
                SUM(CASE WHEN DATE(CreatedAt) = DATE('now') THEN 1 ELSE 0 END) as TodayChanges,
                SUM(CASE WHEN CreatedAt >= datetime('now', '-7 days') THEN 1 ELSE 0 END) as ThisWeekChanges,
                SUM(CASE WHEN CreatedAt >= datetime('now', '-30 days') THEN 1 ELSE 0 END) as ThisMonthChanges,
                SUM(CASE WHEN IsImportant = 1 THEN 1 ELSE 0 END) as ImportantChanges,
                SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) as FailedChanges,
                MAX(CreatedAt) as LastChangeTime
            FROM SettingsHistory";
        
        var stats = await connection.QuerySingleAsync<SettingsHistoryStatistics>(sql);
        
        // 获取按设置类型统计
        const string typeStatsSql = @"
            SELECT SettingType, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY SettingType";
        
        var typeStats = await connection.QueryAsync<dynamic>(typeStatsSql);
        stats.ChangesBySettingType = typeStats.ToDictionary(x => (string)x.SettingType, x => (int)x.Count);
        
        // 获取按变更分类统计
        const string categoryStatsSql = @"
            SELECT ChangeCategory, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY ChangeCategory";
        
        var categoryStats = await connection.QueryAsync<dynamic>(categoryStatsSql);
        stats.ChangesByCategory = categoryStats.ToDictionary(x => (string)x.ChangeCategory, x => (int)x.Count);
        
        // 获取按操作人统计
        const string userStatsSql = @"
            SELECT ChangedBy, COUNT(*) as Count 
            FROM SettingsHistory 
            GROUP BY ChangedBy 
            ORDER BY Count DESC 
            LIMIT 10";
        
        var userStats = await connection.QueryAsync<dynamic>(userStatsSql);
        stats.ChangesByUser = userStats.ToDictionary(x => (string)x.ChangedBy, x => (int)x.Count);
        
        // 获取最活跃的操作人
        if (userStats.Any())
        {
            stats.MostActiveUser = userStats.First().ChangedBy;
        }
        
        // 获取最常变更的设置类型
        if (typeStats.Any())
        {
            stats.MostChangedSettingType = typeStats.First().SettingType;
        }
        
        return stats;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> BatchDeleteAsync(List<Guid> ids)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE Id IN @Ids";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Ids = ids });
        return rowsAffected;
    }

    public async Task<int> CleanExpiredAsync(DateTime cutoffDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM SettingsHistory WHERE CreatedAt < @CutoffDate";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
        return rowsAffected;
    }

    public async Task<List<SettingsHistory>> GetRecentChangesAsync(int count = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            ORDER BY CreatedAt DESC 
            LIMIT @Count";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<SettingsHistory>> GetBySettingTypeAsync(string settingType, int count = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            WHERE SettingType = @SettingType 
            ORDER BY CreatedAt DESC 
            LIMIT @Count";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { SettingType = settingType, Count = count });
        return result.ToList();
    }

    public async Task<List<SettingsHistory>> GetByChangedByAsync(string changedBy, int count = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            WHERE ChangedBy = @ChangedBy 
            ORDER BY CreatedAt DESC 
            LIMIT @Count";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { ChangedBy = changedBy, Count = count });
        return result.ToList();
    }

    public async Task<List<SettingsHistory>> GetImportantChangesAsync(int count = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            WHERE IsImportant = 1 
            ORDER BY CreatedAt DESC 
            LIMIT @Count";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<SettingsHistory>> GetFailedChangesAsync(int count = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            WHERE Status = 'Failed' 
            ORDER BY CreatedAt DESC 
            LIMIT @Count";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { Count = count });
        return result.ToList();
    }

    public async Task<List<SettingsHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM SettingsHistory 
            WHERE CreatedAt >= @StartDate AND CreatedAt <= @EndDate 
            ORDER BY CreatedAt DESC";
        
        var result = await connection.QueryAsync<SettingsHistory>(sql, new { StartDate = startDate, EndDate = endDate });
        return result.ToList();
    }
}