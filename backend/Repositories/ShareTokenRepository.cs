using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Data.Sqlite;
using System.Data;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 分享令牌仓储实现 - 使用 Dapper ORM
/// </summary>
public class ShareTokenRepository : IShareTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly bool _isSqlite;

    public ShareTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
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
    /// 处理SharePermission枚举转换
    /// </summary>
    private Models.SharePermission ParsePermission(object permissionValue)
    {
        var intValue = ParseInt(permissionValue);
        return (Models.SharePermission)intValue;
    }

    public async Task<ShareToken?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });
            
            if (result == null)
                return null;
                
            return MapToShareToken(result);
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
            return result != null ? MapToShareToken(result) : null;
        }
    }

    public async Task<ShareToken?> GetByTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Token = @Token";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Token = token });
            return result != null ? MapToShareToken(result) : null;
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Token = @Token";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Token = token });
            return result != null ? MapToShareToken(result) : null;
        }
    }

    public async Task<ShareToken> CreateAsync(ShareToken shareToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ShareTokens (Id, Token, CodeSnippetId, CreatedBy, ExpiresAt, CreatedAt, UpdatedAt, IsActive, 
                                  AccessCount, MaxAccessCount, Permission, Description, Password, AllowDownload, AllowCopy, LastAccessedAt)
            VALUES (@Id, @Token, @CodeSnippetId, @CreatedBy, @ExpiresAt, @CreatedAt, @UpdatedAt, @IsActive, 
                    @AccessCount, @MaxAccessCount, @Permission, @Description, @Password, @AllowDownload, @AllowCopy, @LastAccessedAt)";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareToken.Id.ToString() : shareToken.Id,
            shareToken.Token,
            CodeSnippetId = _isSqlite ? (object)shareToken.CodeSnippetId.ToString() : shareToken.CodeSnippetId,
            CreatedBy = _isSqlite ? (object)shareToken.CreatedBy.ToString() : shareToken.CreatedBy,
            shareToken.ExpiresAt,
            shareToken.CreatedAt,
            shareToken.UpdatedAt,
            shareToken.IsActive,
            shareToken.AccessCount,
            shareToken.MaxAccessCount,
            Permission = (int)shareToken.Permission,
            shareToken.Description,
            shareToken.Password,
            shareToken.AllowDownload,
            shareToken.AllowCopy,
            LastAccessedAt = shareToken.LastAccessedAt.HasValue ? (object)shareToken.LastAccessedAt.Value : DBNull.Value
        };
        
        await connection.ExecuteAsync(sql, parameters);
        return shareToken;
    }

    public async Task<ShareToken> UpdateAsync(ShareToken shareToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ShareTokens 
            SET Token = @Token, ExpiresAt = @ExpiresAt, UpdatedAt = @UpdatedAt, IsActive = @IsActive,
                AccessCount = @AccessCount, MaxAccessCount = @MaxAccessCount, Permission = @Permission,
                Description = @Description, Password = @Password, AllowDownload = @AllowDownload, 
                AllowCopy = @AllowCopy, LastAccessedAt = @LastAccessedAt
            WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareToken.Id.ToString() : shareToken.Id,
            shareToken.Token,
            shareToken.ExpiresAt,
            shareToken.UpdatedAt,
            shareToken.IsActive,
            shareToken.AccessCount,
            shareToken.MaxAccessCount,
            Permission = (int)shareToken.Permission,
            shareToken.Description,
            shareToken.Password,
            shareToken.AllowDownload,
            shareToken.AllowCopy,
            LastAccessedAt = shareToken.LastAccessedAt.HasValue ? (object)shareToken.LastAccessedAt.Value : DBNull.Value
        };
        
        await connection.ExecuteAsync(sql, parameters);
        return shareToken;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ShareTokens WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)id.ToString() : id;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = idParam });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<ShareToken>> GetByCodeSnippetIdAsync(Guid codeSnippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.CodeSnippetId = @CodeSnippetId
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { CodeSnippetId = codeSnippetId.ToString() });
            return result.Select(MapToShareToken);
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.CodeSnippetId = @CodeSnippetId
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { CodeSnippetId = codeSnippetId });
            return result.Select(MapToShareToken);
        }
    }

    public async Task<IEnumerable<ShareToken>> GetByUserIdAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.CreatedBy = @UserId
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { UserId = userId.ToString() });
            return result.Select(MapToShareToken);
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.CreatedBy = @UserId
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });
            return result.Select(MapToShareToken);
        }
    }

    public async Task<PaginatedResult<ShareToken>> GetPagedAsync(ShareTokenFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (!string.IsNullOrEmpty(filter.Search))
        {
            whereClause += " AND (st.Token LIKE @Search OR st.Description LIKE @Search OR cs.Title LIKE @Search)";
            parameters.Add("Search", $"%{filter.Search}%");
        }
        
        if (filter.CodeSnippetId.HasValue)
        {
            whereClause += " AND st.CodeSnippetId = @CodeSnippetId";
            parameters.Add("CodeSnippetId", _isSqlite ? filter.CodeSnippetId.Value.ToString() : filter.CodeSnippetId.Value);
        }
        
        if (filter.CreatedBy.HasValue)
        {
            whereClause += " AND st.CreatedBy = @CreatedBy";
            parameters.Add("CreatedBy", _isSqlite ? filter.CreatedBy.Value.ToString() : filter.CreatedBy.Value);
        }
        
        if (filter.IsActive.HasValue)
        {
            whereClause += " AND st.IsActive = @IsActive";
            parameters.Add("IsActive", filter.IsActive.Value);
        }
        
        if (filter.IsExpired.HasValue)
        {
            if (filter.IsExpired.Value)
            {
                whereClause += " AND st.ExpiresAt < @Now";
            }
            else
            {
                whereClause += " AND (st.ExpiresAt >= @Now OR st.ExpiresAt IS NULL)";
            }
            parameters.Add("Now", DateTime.UtcNow);
        }
        
        if (filter.Permission.HasValue)
        {
            whereClause += " AND st.Permission = @Permission";
            parameters.Add("Permission", (int)filter.Permission.Value);
        }

        // 获取总数
        var countSql = $"SELECT COUNT(DISTINCT st.Id) FROM ShareTokens st LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // 获取分页数据
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var dataSql = $@"
            SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
            FROM ShareTokens st 
            LEFT JOIN Users u ON st.CreatedBy = u.Id 
            LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
            {whereClause}
            ORDER BY st.CreatedAt DESC
            LIMIT @PageSize OFFSET @Offset";

        var result = await connection.QueryAsync<dynamic>(dataSql, parameters);
        var items = result.Select(MapToShareToken);

        return new PaginatedResult<ShareToken>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<ShareToken>> GetActiveTokensAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.IsActive = 1 AND (st.ExpiresAt IS NULL OR st.ExpiresAt > @Now)
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });
            return result.Select(MapToShareToken);
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.IsActive = 1 AND (st.ExpiresAt IS NULL OR st.ExpiresAt > @Now)
                ORDER BY st.CreatedAt DESC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Now = DateTime.UtcNow });
            return result.Select(MapToShareToken);
        }
    }

    public async Task<IEnumerable<ShareToken>> GetExpiredTokensAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.ExpiresAt < @Now
                ORDER BY st.ExpiresAt ASC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });
            return result.Select(MapToShareToken);
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.ExpiresAt < @Now
                ORDER BY st.ExpiresAt ASC";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Now = DateTime.UtcNow });
            return result.Select(MapToShareToken);
        }
    }

    public async Task<ShareStatsDto> GetShareStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = shareTokenId.ToString() });
            if (result == null)
                throw new KeyNotFoundException($"ShareToken with ID {shareTokenId} not found");

            var shareToken = MapToShareToken(result);
            return await BuildShareStatsAsync(connection, shareToken);
        }
        else
        {
            const string sql = @"
                SELECT st.*, u.Username as CreatorName, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage
                FROM ShareTokens st 
                LEFT JOIN Users u ON st.CreatedBy = u.Id 
                LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id 
                WHERE st.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = shareTokenId });
            if (result == null)
                throw new KeyNotFoundException($"ShareToken with ID {shareTokenId} not found");

            var shareToken = MapToShareToken(result);
            return await BuildShareStatsAsync(connection, shareToken);
        }
    }

    public async Task<IEnumerable<DailyAccessStatDto>> GetDailyAccessStatsAsync(Guid shareTokenId, DateTime startDate, DateTime endDate)
    {
        // 注意：这里假设有一个ShareAccessLogs表来记录访问日志
        // 如果没有这个表，这个方法需要返回空列表或基于现有数据估算
        using var connection = _connectionFactory.CreateConnection();
        
        // 由于没有访问日志表，返回基于创建时间的模拟数据
        var shareToken = await GetByIdAsync(shareTokenId);
        if (shareToken == null)
            return new List<DailyAccessStatDto>();

        var stats = new List<DailyAccessStatDto>();
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            stats.Add(new DailyAccessStatDto
            {
                Date = currentDate,
                AccessCount = 0, // 需要实际的访问日志表来统计
                UniqueVisitors = 0
            });
            currentDate = currentDate.AddDays(1);
        }
        
        return stats;
    }

    public async Task<int> GetTotalAccessCountAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT AccessCount FROM ShareTokens WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        var result = await connection.QuerySingleOrDefaultAsync<int?>(sql, new { Id = idParam });
        return result ?? 0;
    }

    public async Task<int> GetTodayAccessCountAsync(Guid shareTokenId)
    {
        // 由于没有访问日志表，返回0
        // 实际实现需要访问日志表来统计今日访问次数
        return 0;
    }

    public async Task<DateTime?> GetLastAccessTimeAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT LastAccessedAt FROM ShareTokens WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        var result = await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new { Id = idParam });
        return result;
    }

    public async Task<bool> IncrementAccessCountAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE ShareTokens 
            SET AccessCount = AccessCount + 1, LastAccessedAt = @Now
            WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastAccessTimeAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE ShareTokens SET LastAccessedAt = @Now WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateTokenAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE ShareTokens SET IsActive = 0, UpdatedAt = @Now WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> ActivateTokenAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE ShareTokens SET IsActive = 1, UpdatedAt = @Now WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<int> DeleteExpiredTokensAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ShareTokens WHERE ExpiresAt < @Now";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow });
        return rowsAffected;
    }

    public async Task<int> DeactivateInactiveTokensAsync(TimeSpan inactiveThreshold)
    {
        using var connection = _connectionFactory.CreateConnection();
        var thresholdDate = DateTime.UtcNow.Subtract(inactiveThreshold);
        
        const string sql = @"
            UPDATE ShareTokens 
            SET IsActive = 0, UpdatedAt = @Now 
            WHERE LastAccessedAt < @ThresholdDate AND IsActive = 1";
        
        var parameters = new
        {
            ThresholdDate = thresholdDate,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected;
    }

    public async Task<bool> ExtendTokenExpirationAsync(Guid shareTokenId, TimeSpan extension)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE ShareTokens SET ExpiresAt = DATE_ADD(ExpiresAt, INTERVAL @ExtensionSeconds SECOND), UpdatedAt = @Now WHERE Id = @Id";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId,
            ExtensionSeconds = (int)extension.TotalSeconds,
            Now = DateTime.UtcNow
        };
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    /// <summary>
    /// 将动态查询结果映射为ShareToken对象
    /// </summary>
    private ShareToken MapToShareToken(dynamic result)
    {
        return new ShareToken
        {
            Id = Guid.Parse(result.Id),
            Token = result.Token,
            CodeSnippetId = Guid.Parse(result.CodeSnippetId),
            CreatedBy = Guid.Parse(result.CreatedBy),
            ExpiresAt = ParseDateTime(result.ExpiresAt),
            CreatedAt = ParseDateTime(result.CreatedAt),
            UpdatedAt = ParseDateTime(result.UpdatedAt),
            IsActive = ParseBool(result.IsActive),
            AccessCount = ParseInt(result.AccessCount),
            MaxAccessCount = ParseInt(result.MaxAccessCount),
            Permission = ParsePermission(result.Permission),
            Description = result.Description ?? string.Empty,
            Password = result.Password,
            AllowDownload = ParseBool(result.AllowDownload),
            AllowCopy = ParseBool(result.AllowCopy),
            LastAccessedAt = result.LastAccessedAt != null ? ParseDateTime(result.LastAccessedAt) : null,
            CreatorName = result.CreatorName ?? string.Empty,
            CodeSnippetTitle = result.CodeSnippetTitle ?? string.Empty,
            CodeSnippetLanguage = result.CodeSnippetLanguage ?? string.Empty
        };
    }

    /// <summary>
    /// 构建分享统计数据
    /// </summary>
    private async Task<ShareStatsDto> BuildShareStatsAsync(IDbConnection connection, ShareToken shareToken)
    {
        var now = DateTime.UtcNow;
        var isExpired = shareToken.ExpiresAt < now;
        var isAccessLimitReached = shareToken.MaxAccessCount > 0 && shareToken.AccessCount >= shareToken.MaxAccessCount;
        var remainingAccessCount = shareToken.MaxAccessCount > 0 ? Math.Max(0, shareToken.MaxAccessCount - shareToken.AccessCount) : -1;

        return new ShareStatsDto
        {
            ShareTokenId = shareToken.Id,
            Token = shareToken.Token,
            TotalAccessCount = shareToken.AccessCount,
            TodayAccessCount = await GetTodayAccessCountAsync(shareToken.Id),
            ThisWeekAccessCount = 0, // 需要访问日志表
            ThisMonthAccessCount = 0, // 需要访问日志表
            RemainingAccessCount = remainingAccessCount,
            LastAccessedAt = shareToken.LastAccessedAt,
            CreatedAt = shareToken.CreatedAt,
            ExpiresAt = shareToken.ExpiresAt,
            IsExpired = isExpired,
            IsAccessLimitReached = isAccessLimitReached,
            DailyStats = new List<DailyAccessStatDto>(),
            SourceStats = new List<AccessSourceStatDto>()
        };
    }

    #region 管理员功能

    /// <summary>
    /// 获取所有分享链接（管理员功能）
    /// </summary>
    /// <param name="filter">分享过滤器</param>
    /// <returns>分页的分享令牌结果</returns>
    public async Task<PaginatedResult<ShareTokenDto>> GetAllSharesAdminAsync(AdminShareFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // 构建基础查询
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // 搜索条件
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            whereConditions.Add(@"(st.Token LIKE @Search OR 
                                 st.Description LIKE @Search OR 
                                 cs.Title LIKE @Search OR 
                                 u.Username LIKE @Search)");
            parameters.Add("@Search", $"%{filter.Search}%");
        }

        if (filter.CreatedBy.HasValue)
        {
            whereConditions.Add("st.CreatedBy = @CreatedBy");
            parameters.Add("@CreatedBy", filter.CreatedBy.Value);
        }

        if (filter.CodeSnippetId.HasValue)
        {
            whereConditions.Add("st.CodeSnippetId = @CodeSnippetId");
            parameters.Add("@CodeSnippetId", filter.CodeSnippetId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            whereConditions.Add("st.IsActive = @IsActive");
            parameters.Add("@IsActive", filter.IsActive.Value);
        }

        if (filter.IsExpired.HasValue)
        {
            if (filter.IsExpired.Value)
            {
                whereConditions.Add("st.ExpiresAt < @Now");
            }
            else
            {
                whereConditions.Add("st.ExpiresAt >= @Now");
            }
            parameters.Add("@Now", DateTime.UtcNow);
        }

        if (filter.HasPassword.HasValue)
        {
            if (filter.HasPassword.Value)
            {
                whereConditions.Add("st.Password IS NOT NULL AND st.Password != ''");
            }
            else
            {
                whereConditions.Add("(st.Password IS NULL OR st.Password = '')");
            }
        }

        if (filter.Permission.HasValue)
        {
            whereConditions.Add("st.Permission = @Permission");
            parameters.Add("@Permission", (int)filter.Permission.Value);
        }

        if (filter.StartDate.HasValue)
        {
            whereConditions.Add("st.CreatedAt >= @StartDate");
            parameters.Add("@StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            whereConditions.Add("st.CreatedAt <= @EndDate");
            parameters.Add("@EndDate", filter.EndDate.Value);
        }

        // 构建WHERE子句
        var whereClause = whereConditions.Count > 0 
            ? $"WHERE {string.Join(" AND ", whereConditions)}" 
            : "";

        // 构建排序
        var orderBy = filter.SortBy?.ToLower() switch
        {
            "token" => "st.Token",
            "accescount" => "st.AccessCount",
            "createdat" => "st.CreatedAt",
            "expiresat" => "st.ExpiresAt",
            "creator" => "u.Username",
            _ => "st.CreatedAt"
        };

        var direction = filter.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";
        var orderByClause = $"ORDER BY {orderBy} {direction}";

        // 计算分页
        var offset = (filter.Page - 1) * filter.PageSize;

        // 获取总数
        var countSql = $@"
            SELECT COUNT(*)
            FROM ShareTokens st
            LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id
            LEFT JOIN Users u ON st.CreatedBy = u.Id
            {whereClause}";

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // 获取分页数据
        var dataSql = $@"
            SELECT st.*, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
            FROM ShareTokens st
            LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id
            LEFT JOIN Users u ON st.CreatedBy = u.Id
            {whereClause}
            {orderByClause}
            LIMIT @Limit OFFSET @Offset";

        parameters.Add("@Limit", filter.PageSize);
        parameters.Add("@Offset", offset);

        var shareTokens = await connection.QueryAsync<ShareToken>(dataSql, parameters);

        // 转换为DTO
        var items = shareTokens.Select(MapToDto).ToList();

        return new PaginatedResult<ShareTokenDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// 获取系统分享统计信息（管理员功能）
    /// </summary>
    /// <returns>系统分享统计信息</returns>
    public async Task<SystemShareStatsDto> GetSystemShareStatsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // 基础统计
        var basicStatsSql = @"
            SELECT 
                COUNT(*) as TotalShares,
                SUM(CASE WHEN IsActive = 1 AND ExpiresAt > @Now THEN 1 ELSE 0 END) as ActiveShares,
                SUM(CASE WHEN ExpiresAt <= @Now THEN 1 ELSE 0 END) as ExpiredShares,
                SUM(AccessCount) as TotalAccessCount,
                COUNT(DISTINCT CreatedBy) as ActiveUsers
            FROM ShareTokens";

        var basicStats = await connection.QuerySingleAsync<dynamic>(basicStatsSql, new { Now = DateTime.UtcNow });

        // 今日访问统计
        var todayAccessSql = @"
            SELECT COUNT(*) as Count
            FROM ShareAccessLogs
            WHERE DATE(AccessedAt) = DATE(@Now)";

        var todayAccess = await connection.QuerySingleAsync<int>(todayAccessSql, new { Now = DateTime.UtcNow });

        // 本周访问统计
        var weekStart = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var weekAccessSql = @"
            SELECT COUNT(*) as Count
            FROM ShareAccessLogs
            WHERE AccessedAt >= @WeekStart";

        var weekAccess = await connection.QuerySingleAsync<int>(weekAccessSql, new { WeekStart = weekStart });

        // 本月访问统计
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthAccessSql = @"
            SELECT COUNT(*) as Count
            FROM ShareAccessLogs
            WHERE AccessedAt >= @MonthStart";

        var monthAccess = await connection.QuerySingleAsync<int>(monthAccessSql, new { MonthStart = monthStart });

        // 热门分享排行
        var popularSharesSql = @"
            SELECT TOP 10 st.Id, st.Token, cs.Title, cs.Language, u.Username, st.AccessCount, st.CreatedAt
            FROM ShareTokens st
            LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id
            LEFT JOIN Users u ON st.CreatedBy = u.Id
            WHERE st.IsActive = 1
            ORDER BY st.AccessCount DESC, st.CreatedAt DESC";

        var popularShares = await connection.QueryAsync<dynamic>(popularSharesSql);

        // 活跃用户排行
        var activeUsersSql = @"
            SELECT TOP 10 u.Id, u.Username, COUNT(st.Id) as ShareCount, SUM(st.AccessCount) as TotalAccessCount, MAX(st.CreatedAt) as LastShareAt
            FROM Users u
            LEFT JOIN ShareTokens st ON u.Id = st.CreatedBy
            GROUP BY u.Id, u.Username
            HAVING COUNT(st.Id) > 0
            ORDER BY ShareCount DESC, TotalAccessCount DESC";

        var activeUsers = await connection.QueryAsync<dynamic>(activeUsersSql);

        // 权限分布统计
        var permissionStatsSql = @"
            SELECT 
                Permission,
                COUNT(*) as ShareCount
            FROM ShareTokens
            GROUP BY Permission
            ORDER BY Permission";

        var permissionStats = await connection.QueryAsync<dynamic>(permissionStatsSql);

        // 语言分布统计
        var languageStatsSql = @"
            SELECT TOP 10 
                cs.Language,
                COUNT(*) as ShareCount
            FROM ShareTokens st
            LEFT JOIN CodeSnippets cs ON st.CodeSnippetId = cs.Id
            WHERE cs.Language IS NOT NULL AND cs.Language != ''
            GROUP BY cs.Language
            ORDER BY ShareCount DESC";

        var languageStats = await connection.QueryAsync<dynamic>(languageStatsSql);

        // 构建结果
        var result = new SystemShareStatsDto
        {
            TotalShares = basicStats.TotalShares,
            ActiveShares = basicStats.ActiveShares,
            ExpiredShares = basicStats.ExpiredShares,
            TotalAccessCount = basicStats.TotalAccessCount,
            TodayAccessCount = todayAccess,
            ThisWeekAccessCount = weekAccess,
            ThisMonthAccessCount = monthAccess,
            ActiveUserCount = basicStats.ActiveUsers,
            PopularShares = popularShares.Select(p => new PopularShareDto
            {
                ShareTokenId = p.Id,
                Token = p.Token,
                CodeSnippetTitle = p.Title,
                CodeSnippetLanguage = p.Language,
                CreatorName = p.Username,
                AccessCount = p.AccessCount,
                CreatedAt = p.CreatedAt
            }).ToList(),
            ActiveUsers = activeUsers.Select(u => new ActiveUserDto
            {
                UserId = u.Id,
                Username = u.Username,
                ShareCount = u.ShareCount,
                TotalAccessCount = u.TotalAccessCount,
                LastShareAt = u.LastShareAt
            }).ToList(),
            PermissionStats = permissionStats.Select(p => new PermissionStatDto
            {
                Permission = (Models.SharePermission)p.Permission,
                PermissionName = GetPermissionName((Models.SharePermission)p.Permission),
                ShareCount = p.ShareCount,
                Percentage = basicStats.TotalShares > 0 ? (double)p.ShareCount / basicStats.TotalShares * 100 : 0
            }).ToList(),
            LanguageStats = languageStats.Select(l => new LanguageStatDto
            {
                Language = l.Language,
                ShareCount = l.ShareCount,
                Percentage = basicStats.TotalShares > 0 ? (double)l.ShareCount / basicStats.TotalShares * 100 : 0
            }).ToList()
        };

        return result;
    }

    /// <summary>
    /// 获取权限名称
    /// </summary>
    /// <param name="permission">权限枚举</param>
    /// <returns>权限名称</returns>
    private string GetPermissionName(Models.SharePermission permission)
    {
        return permission switch
        {
            Models.SharePermission.ReadOnly => "只读",
            Models.SharePermission.Edit => "可编辑",
            Models.SharePermission.Full => "完全权限",
            _ => "未知"
        };
    }

    /// <summary>
    /// 将ShareToken实体映射为ShareTokenDto
    /// </summary>
    /// <param name="shareToken">分享令牌实体</param>
    /// <returns>分享令牌DTO</returns>
    private ShareTokenDto MapToDto(ShareToken shareToken)
    {
        var now = DateTime.UtcNow;
        var isExpired = shareToken.ExpiresAt < now;
        var isAccessLimitReached = shareToken.MaxAccessCount > 0 && shareToken.AccessCount >= shareToken.MaxAccessCount;
        var remainingAccessCount = shareToken.MaxAccessCount > 0 ? Math.Max(0, shareToken.MaxAccessCount - shareToken.AccessCount) : -1;

        return new ShareTokenDto
        {
            Id = shareToken.Id,
            Token = shareToken.Token,
            CodeSnippetId = shareToken.CodeSnippetId,
            CreatedBy = shareToken.CreatedBy,
            CreatorName = "", // 需要从数据库查询
            ExpiresAt = shareToken.ExpiresAt,
            CreatedAt = shareToken.CreatedAt,
            UpdatedAt = shareToken.UpdatedAt,
            IsActive = shareToken.IsActive,
            AccessCount = shareToken.AccessCount,
            MaxAccessCount = shareToken.MaxAccessCount,
            Permission = (Models.SharePermission)shareToken.Permission,
            Description = shareToken.Description,
            HasPassword = !string.IsNullOrEmpty(shareToken.Password),
            AllowDownload = shareToken.AllowDownload,
            AllowCopy = shareToken.AllowCopy,
            LastAccessedAt = shareToken.LastAccessedAt,
            CodeSnippetTitle = "", // 需要从数据库查询
            CodeSnippetLanguage = "", // 需要从数据库查询
            IsExpired = isExpired,
            IsAccessLimitReached = isAccessLimitReached,
            RemainingAccessCount = remainingAccessCount
        };
    }

    #endregion
}