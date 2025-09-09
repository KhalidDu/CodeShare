using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Data.Sqlite;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 分享访问日志仓储实现 - 使用 Dapper ORM
/// </summary>
public class ShareAccessLogRepository : IShareAccessLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly bool _isSqlite;

    public ShareAccessLogRepository(IDbConnectionFactory connectionFactory)
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

    public async Task<ShareAccessLog?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.Id = @Id";
            
            var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });
            
            if (result == null)
                return null;
                
            return new ShareAccessLog
            {
                Id = Guid.Parse(result.Id),
                ShareTokenId = Guid.Parse(result.ShareTokenId),
                CodeSnippetId = Guid.Parse(result.CodeSnippetId),
                IpAddress = result.IpAddress,
                UserAgent = result.UserAgent,
                Source = result.Source,
                Country = result.Country,
                City = result.City,
                Browser = result.Browser,
                OperatingSystem = result.OperatingSystem,
                DeviceType = result.DeviceType,
                AccessedAt = ParseDateTime(result.AccessedAt),
                IsSuccess = ParseBool(result.IsSuccess),
                FailureReason = result.FailureReason,
                Duration = ParseInt(result.Duration),
                SessionId = result.SessionId,
                Referer = result.Referer,
                AcceptLanguage = result.AcceptLanguage,
                CodeSnippetTitle = result.CodeSnippetTitle,
                CodeSnippetLanguage = result.CodeSnippetLanguage,
                CreatorName = result.CreatorName
            };
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.Id = @Id";
            
            return await connection.QuerySingleOrDefaultAsync<ShareAccessLog>(sql, new { Id = id });
        }
    }

    public async Task<ShareAccessLog> CreateAsync(ShareAccessLog accessLog)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ShareAccessLogs (Id, ShareTokenId, CodeSnippetId, IpAddress, UserAgent, Source, Country, City, 
                                      Browser, OperatingSystem, DeviceType, AccessedAt, IsSuccess, FailureReason, Duration, 
                                      SessionId, Referer, AcceptLanguage)
            VALUES (@Id, @ShareTokenId, @CodeSnippetId, @IpAddress, @UserAgent, @Source, @Country, @City, 
                    @Browser, @OperatingSystem, @DeviceType, @AccessedAt, @IsSuccess, @FailureReason, @Duration, 
                    @SessionId, @Referer, @AcceptLanguage)";
        
        var parameters = new
        {
            Id = _isSqlite ? (object)accessLog.Id.ToString() : accessLog.Id,
            ShareTokenId = _isSqlite ? (object)accessLog.ShareTokenId.ToString() : accessLog.ShareTokenId,
            CodeSnippetId = _isSqlite ? (object)accessLog.CodeSnippetId.ToString() : accessLog.CodeSnippetId,
            accessLog.IpAddress,
            accessLog.UserAgent,
            accessLog.Source,
            accessLog.Country,
            accessLog.City,
            accessLog.Browser,
            accessLog.OperatingSystem,
            accessLog.DeviceType,
            accessLog.AccessedAt,
            accessLog.IsSuccess,
            accessLog.FailureReason,
            accessLog.Duration,
            accessLog.SessionId,
            accessLog.Referer,
            accessLog.AcceptLanguage
        };
        
        await connection.ExecuteAsync(sql, parameters);
        return accessLog;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ShareAccessLogs WHERE Id = @Id";
        
        var idParam = _isSqlite ? (object)id.ToString() : id;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = idParam });
        return rowsAffected > 0;
    }

    public async Task<int> BulkInsertAsync(IEnumerable<ShareAccessLog> accessLogs)
    {
        if (!accessLogs.Any())
            return 0;

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO ShareAccessLogs (Id, ShareTokenId, CodeSnippetId, IpAddress, UserAgent, Source, Country, City, 
                                      Browser, OperatingSystem, DeviceType, AccessedAt, IsSuccess, FailureReason, Duration, 
                                      SessionId, Referer, AcceptLanguage)
            VALUES (@Id, @ShareTokenId, @CodeSnippetId, @IpAddress, @UserAgent, @Source, @Country, @City, 
                    @Browser, @OperatingSystem, @DeviceType, @AccessedAt, @IsSuccess, @FailureReason, @Duration, 
                    @SessionId, @Referer, @AcceptLanguage)";

        var parameters = accessLogs.Select(log => new
        {
            Id = _isSqlite ? (object)log.Id.ToString() : log.Id,
            ShareTokenId = _isSqlite ? (object)log.ShareTokenId.ToString() : log.ShareTokenId,
            CodeSnippetId = _isSqlite ? (object)log.CodeSnippetId.ToString() : log.CodeSnippetId,
            log.IpAddress,
            log.UserAgent,
            log.Source,
            log.Country,
            log.City,
            log.Browser,
            log.OperatingSystem,
            log.DeviceType,
            log.AccessedAt,
            log.IsSuccess,
            log.FailureReason,
            log.Duration,
            log.SessionId,
            log.Referer,
            log.AcceptLanguage
        });

        return await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<bool> DeleteExpiredLogsAsync(DateTime cutoffDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ShareAccessLogs WHERE AccessedAt < @CutoffDate";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteByShareTokenIdAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM ShareAccessLogs WHERE ShareTokenId = @ShareTokenId";
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        var rowsAffected = await connection.ExecuteAsync(sql, new { ShareTokenId = shareTokenIdParam });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<ShareAccessLog>> GetByShareTokenIdAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.ShareTokenId = @ShareTokenId
                ORDER BY sal.AccessedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenId.ToString() });
            
            return result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            });
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.ShareTokenId = @ShareTokenId
                ORDER BY sal.AccessedAt DESC";

            return await connection.QueryAsync<ShareAccessLog>(sql, new { ShareTokenId = shareTokenId });
        }
    }

    public async Task<IEnumerable<ShareAccessLog>> GetByCodeSnippetIdAsync(Guid codeSnippetId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.CodeSnippetId = @CodeSnippetId
                ORDER BY sal.AccessedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { CodeSnippetId = codeSnippetId.ToString() });
            
            return result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            });
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.CodeSnippetId = @CodeSnippetId
                ORDER BY sal.AccessedAt DESC";

            return await connection.QueryAsync<ShareAccessLog>(sql, new { CodeSnippetId = codeSnippetId });
        }
    }

    public async Task<IEnumerable<ShareAccessLog>> GetByIpAddressAsync(string ipAddress, DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.IpAddress = @IpAddress AND sal.AccessedAt BETWEEN @StartDate AND @EndDate
                ORDER BY sal.AccessedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { IpAddress = ipAddress, StartDate = startDate, EndDate = endDate });
            
            return result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            });
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.IpAddress = @IpAddress AND sal.AccessedAt BETWEEN @StartDate AND @EndDate
                ORDER BY sal.AccessedAt DESC";

            return await connection.QueryAsync<ShareAccessLog>(sql, new { IpAddress = ipAddress, StartDate = startDate, EndDate = endDate });
        }
    }

    public async Task<IEnumerable<ShareAccessLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.AccessedAt BETWEEN @StartDate AND @EndDate
                ORDER BY sal.AccessedAt DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { StartDate = startDate, EndDate = endDate });
            
            return result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            });
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                WHERE sal.AccessedAt BETWEEN @StartDate AND @EndDate
                ORDER BY sal.AccessedAt DESC";

            return await connection.QueryAsync<ShareAccessLog>(sql, new { StartDate = startDate, EndDate = endDate });
        }
    }

    public async Task<PaginatedResult<ShareAccessLog>> GetPagedAsync(AccessLogFilter filter)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = "WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (filter.ShareTokenId.HasValue)
        {
            whereClause += " AND sal.ShareTokenId = @ShareTokenId";
            parameters.Add("ShareTokenId", filter.ShareTokenId.Value);
        }
        
        if (filter.CodeSnippetId.HasValue)
        {
            whereClause += " AND sal.CodeSnippetId = @CodeSnippetId";
            parameters.Add("CodeSnippetId", filter.CodeSnippetId.Value);
        }
        
        if (!string.IsNullOrEmpty(filter.IpAddress))
        {
            whereClause += " AND sal.IpAddress = @IpAddress";
            parameters.Add("IpAddress", filter.IpAddress);
        }
        
        if (!string.IsNullOrEmpty(filter.Source))
        {
            whereClause += " AND sal.Source = @Source";
            parameters.Add("Source", filter.Source);
        }
        
        if (!string.IsNullOrEmpty(filter.Country))
        {
            whereClause += " AND sal.Country = @Country";
            parameters.Add("Country", filter.Country);
        }
        
        if (!string.IsNullOrEmpty(filter.DeviceType))
        {
            whereClause += " AND sal.DeviceType = @DeviceType";
            parameters.Add("DeviceType", filter.DeviceType);
        }
        
        if (!string.IsNullOrEmpty(filter.Browser))
        {
            whereClause += " AND sal.Browser = @Browser";
            parameters.Add("Browser", filter.Browser);
        }
        
        if (!string.IsNullOrEmpty(filter.OperatingSystem))
        {
            whereClause += " AND sal.OperatingSystem = @OperatingSystem";
            parameters.Add("OperatingSystem", filter.OperatingSystem);
        }
        
        if (filter.StartDate.HasValue)
        {
            whereClause += " AND sal.AccessedAt >= @StartDate";
            parameters.Add("StartDate", filter.StartDate.Value);
        }
        
        if (filter.EndDate.HasValue)
        {
            whereClause += " AND sal.AccessedAt <= @EndDate";
            parameters.Add("EndDate", filter.EndDate.Value);
        }
        
        if (filter.IsSuccess.HasValue)
        {
            whereClause += " AND sal.IsSuccess = @IsSuccess";
            parameters.Add("IsSuccess", filter.IsSuccess.Value);
        }

        // 获取总数
        var countSql = $"SELECT COUNT(DISTINCT sal.Id) FROM ShareAccessLogs sal {whereClause}";
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // 获取分页数据
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        // 排序
        var orderByClause = $"ORDER BY sal.{filter.SortBy} {filter.SortDirection.ToUpper()}";

        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            var dataSql = $@"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                {whereClause}
                {orderByClause}
                LIMIT @PageSize OFFSET @Offset";

            var result = await connection.QueryAsync<dynamic>(dataSql, parameters);
            
            var items = result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            }).ToList();

            return new PaginatedResult<ShareAccessLog>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            var dataSql = $@"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                {whereClause}
                {orderByClause}
                LIMIT @PageSize OFFSET @Offset";

            var items = await connection.QueryAsync<ShareAccessLog>(dataSql, parameters);

            return new PaginatedResult<ShareAccessLog>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }

    public async Task<PaginatedResult<ShareAccessLog>> GetByShareTokenIdPagedAsync(Guid shareTokenId, AccessLogFilter filter)
    {
        filter.ShareTokenId = shareTokenId;
        return await GetPagedAsync(filter);
    }

    public async Task<AccessStats> GetAccessStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COUNT(*) as TotalAccessCount,
                COUNT(DISTINCT IpAddress) as UniqueAccessCount,
                SUM(CASE WHEN IsSuccess = 1 THEN 1 ELSE 0 END) as SuccessAccessCount,
                SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) as FailedAccessCount,
                AVG(Duration) as AverageDuration,
                MIN(AccessedAt) as FirstAccessAt,
                MAX(AccessedAt) as LastAccessAt,
                SUM(CASE WHEN DATE(AccessedAt) = DATE('now') THEN 1 ELSE 0 END) as TodayAccessCount,
                SUM(CASE WHEN AccessedAt >= DATE('now', '-7 days') THEN 1 ELSE 0 END) as ThisWeekAccessCount,
                SUM(CASE WHEN AccessedAt >= DATE('now', '-30 days') THEN 1 ELSE 0 END) as ThisMonthAccessCount
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId";

        if (_isSqlite)
        {
            // SQLite日期函数
            var result = await connection.QuerySingleAsync<dynamic>(sql, new { ShareTokenId = shareTokenId.ToString() });
            
            return new AccessStats
            {
                ShareTokenId = shareTokenId,
                TotalAccessCount = ParseInt(result.TotalAccessCount),
                UniqueAccessCount = ParseInt(result.UniqueAccessCount),
                SuccessAccessCount = ParseInt(result.SuccessAccessCount),
                FailedAccessCount = ParseInt(result.FailedAccessCount),
                AverageDuration = Convert.ToDouble(result.AverageDuration ?? 0),
                FirstAccessAt = result.FirstAccessAt != null ? ParseDateTime(result.FirstAccessAt) : null,
                LastAccessAt = result.LastAccessAt != null ? ParseDateTime(result.LastAccessAt) : null,
                TodayAccessCount = ParseInt(result.TodayAccessCount),
                ThisWeekAccessCount = ParseInt(result.ThisWeekAccessCount),
                ThisMonthAccessCount = ParseInt(result.ThisMonthAccessCount),
                GrowthRate = 0.0 // 需要额外计算
            };
        }
        else
        {
            // MySQL日期函数
            var mysqlSql = @"
                SELECT 
                    COUNT(*) as TotalAccessCount,
                    COUNT(DISTINCT IpAddress) as UniqueAccessCount,
                    SUM(CASE WHEN IsSuccess = 1 THEN 1 ELSE 0 END) as SuccessAccessCount,
                    SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) as FailedAccessCount,
                    AVG(Duration) as AverageDuration,
                    MIN(AccessedAt) as FirstAccessAt,
                    MAX(AccessedAt) as LastAccessAt,
                    SUM(CASE WHEN DATE(AccessedAt) = CURDATE() THEN 1 ELSE 0 END) as TodayAccessCount,
                    SUM(CASE WHEN AccessedAt >= DATE_SUB(CURDATE(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) as ThisWeekAccessCount,
                    SUM(CASE WHEN AccessedAt >= DATE_SUB(CURDATE(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) as ThisMonthAccessCount
                FROM ShareAccessLogs 
                WHERE ShareTokenId = @ShareTokenId";

            var result = await connection.QuerySingleAsync<AccessStats>(mysqlSql, new { ShareTokenId = shareTokenId });
            result.ShareTokenId = shareTokenId;
            result.GrowthRate = 0.0; // 需要额外计算
            
            return result;
        }
    }

    public async Task<AccessStats> GetAccessStatsByDateRangeAsync(Guid shareTokenId, DateTime startDate, DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COUNT(*) as TotalAccessCount,
                COUNT(DISTINCT IpAddress) as UniqueAccessCount,
                SUM(CASE WHEN IsSuccess = 1 THEN 1 ELSE 0 END) as SuccessAccessCount,
                SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) as FailedAccessCount,
                AVG(Duration) as AverageDuration,
                MIN(AccessedAt) as FirstAccessAt,
                MAX(AccessedAt) as LastAccessAt
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId AND AccessedAt BETWEEN @StartDate AND @EndDate";

        var result = await connection.QuerySingleAsync<AccessStats>(sql, new { ShareTokenId = shareTokenIdParam, StartDate = startDate, EndDate = endDate });
        result.ShareTokenId = shareTokenId;
        result.GrowthRate = 0.0;
        
        return result;
    }

    public async Task<IEnumerable<DailyAccessStat>> GetDailyAccessStatsAsync(Guid shareTokenId, int days)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        if (_isSqlite)
        {
            // SQLite日期函数
            const string sql = @"
                SELECT 
                    DATE(AccessedAt) as Date,
                    COUNT(*) as AccessCount,
                    COUNT(DISTINCT IpAddress) as UniqueVisitors,
                    AVG(Duration) as AverageDuration
                FROM ShareAccessLogs 
                WHERE ShareTokenId = @ShareTokenId AND AccessedAt >= DATE('now', '-' || @Days || ' days')
                GROUP BY DATE(AccessedAt)
                ORDER BY Date DESC";

            var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenId.ToString(), Days = days });
            
            return result.Select(row => new DailyAccessStat
            {
                Date = DateTime.Parse(row.Date),
                AccessCount = ParseInt(row.AccessCount),
                UniqueVisitors = ParseInt(row.UniqueVisitors),
                AverageDuration = Convert.ToDouble(row.AverageDuration ?? 0),
                SuccessCount = ParseInt(row.AccessCount) // 简化处理，实际需要查询成功次数
            });
        }
        else
        {
            // MySQL日期函数
            const string sql = @"
                SELECT 
                    DATE(AccessedAt) as Date,
                    COUNT(*) as AccessCount,
                    COUNT(DISTINCT IpAddress) as UniqueVisitors,
                    AVG(Duration) as AverageDuration
                FROM ShareAccessLogs 
                WHERE ShareTokenId = @ShareTokenId AND AccessedAt >= DATE_SUB(CURDATE(), INTERVAL @Days DAY)
                GROUP BY DATE(AccessedAt)
                ORDER BY Date DESC";

            return await connection.QueryAsync<DailyAccessStat>(sql, new { ShareTokenId = shareTokenId, Days = days });
        }
    }

    public async Task<IEnumerable<HourlyAccessStat>> GetHourlyAccessStatsAsync(Guid shareTokenId, DateTime date)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        if (_isSqlite)
        {
            // SQLite日期函数
            const string sql = @"
                SELECT 
                    CAST(strftime('%H', AccessedAt) AS INTEGER) as Hour,
                    COUNT(*) as AccessCount,
                    COUNT(DISTINCT IpAddress) as UniqueVisitors
                FROM ShareAccessLogs 
                WHERE ShareTokenId = @ShareTokenId AND DATE(AccessedAt) = DATE(@Date)
                GROUP BY CAST(strftime('%H', AccessedAt) AS INTEGER)
                ORDER BY Hour";

            var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenId.ToString(), Date = date.ToString("yyyy-MM-dd") });
            
            return result.Select(row => new HourlyAccessStat
            {
                Hour = ParseInt(row.Hour),
                AccessCount = ParseInt(row.AccessCount),
                UniqueVisitors = ParseInt(row.UniqueVisitors)
            });
        }
        else
        {
            // MySQL日期函数
            const string sql = @"
                SELECT 
                    HOUR(AccessedAt) as Hour,
                    COUNT(*) as AccessCount,
                    COUNT(DISTINCT IpAddress) as UniqueVisitors
                FROM ShareAccessLogs 
                WHERE ShareTokenId = @ShareTokenId AND DATE(AccessedAt) = DATE(@Date)
                GROUP BY HOUR(AccessedAt)
                ORDER BY Hour";

            return await connection.QueryAsync<HourlyAccessStat>(sql, new { ShareTokenId = shareTokenId, Date = date });
        }
    }

    public async Task<IEnumerable<AccessSourceStat>> GetAccessSourceStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COALESCE(Source, 'unknown') as Source,
                COUNT(*) as AccessCount,
                COUNT(DISTINCT IpAddress) as UniqueVisitors,
                AVG(Duration) as AverageDuration
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId
            GROUP BY COALESCE(Source, 'unknown')
            ORDER BY AccessCount DESC";

        var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenIdParam });
        
        var totalAccessCount = result.Sum(x => Convert.ToInt32(x.AccessCount));
        
        return result.Select(row => new AccessSourceStat
        {
            Source = row.Source,
            AccessCount = ParseInt(row.AccessCount),
            UniqueVisitors = ParseInt(row.UniqueVisitors),
            Percentage = totalAccessCount > 0 ? (Convert.ToDouble(row.AccessCount) / totalAccessCount) * 100 : 0,
            AverageDuration = Convert.ToDouble(row.AverageDuration ?? 0)
        });
    }

    public async Task<IEnumerable<DeviceTypeStat>> GetDeviceTypeStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COALESCE(DeviceType, 'unknown') as DeviceType,
                COUNT(*) as AccessCount,
                COUNT(DISTINCT IpAddress) as UniqueVisitors,
                AVG(Duration) as AverageDuration
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId
            GROUP BY COALESCE(DeviceType, 'unknown')
            ORDER BY AccessCount DESC";

        var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenIdParam });
        
        var totalAccessCount = result.Sum(x => Convert.ToInt32(x.AccessCount));
        
        return result.Select(row => new DeviceTypeStat
        {
            DeviceType = row.DeviceType,
            AccessCount = ParseInt(row.AccessCount),
            UniqueVisitors = ParseInt(row.UniqueVisitors),
            Percentage = totalAccessCount > 0 ? (Convert.ToDouble(row.AccessCount) / totalAccessCount) * 100 : 0,
            AverageDuration = Convert.ToDouble(row.AverageDuration ?? 0)
        });
    }

    public async Task<IEnumerable<CountryStat>> GetCountryStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COALESCE(Country, 'unknown') as CountryCode,
                COALESCE(Country, 'Unknown') as CountryName,
                COUNT(*) as AccessCount,
                COUNT(DISTINCT IpAddress) as UniqueVisitors
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId
            GROUP BY COALESCE(Country, 'unknown')
            ORDER BY AccessCount DESC";

        var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenIdParam });
        
        var totalAccessCount = result.Sum(x => Convert.ToInt32(x.AccessCount));
        
        return result.Select(row => new CountryStat
        {
            CountryCode = row.CountryCode,
            CountryName = row.CountryName,
            AccessCount = ParseInt(row.AccessCount),
            UniqueVisitors = ParseInt(row.UniqueVisitors),
            Percentage = totalAccessCount > 0 ? (Convert.ToDouble(row.AccessCount) / totalAccessCount) * 100 : 0,
            Cities = new List<CityStat>() // 简化处理，可以后续添加城市统计
        });
    }

    public async Task<IEnumerable<BrowserStat>> GetBrowserStatsAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        
        const string sql = @"
            SELECT 
                COALESCE(Browser, 'unknown') as Browser,
                'unknown' as Version,
                COUNT(*) as AccessCount,
                COUNT(DISTINCT IpAddress) as UniqueVisitors,
                AVG(Duration) as AverageDuration
            FROM ShareAccessLogs 
            WHERE ShareTokenId = @ShareTokenId
            GROUP BY COALESCE(Browser, 'unknown')
            ORDER BY AccessCount DESC";

        var result = await connection.QueryAsync<dynamic>(sql, new { ShareTokenId = shareTokenIdParam });
        
        var totalAccessCount = result.Sum(x => Convert.ToInt32(x.AccessCount));
        
        return result.Select(row => new BrowserStat
        {
            Browser = row.Browser,
            Version = row.Version,
            AccessCount = ParseInt(row.AccessCount),
            UniqueVisitors = ParseInt(row.UniqueVisitors),
            Percentage = totalAccessCount > 0 ? (Convert.ToDouble(row.AccessCount) / totalAccessCount) * 100 : 0,
            AverageDuration = Convert.ToDouble(row.AverageDuration ?? 0)
        });
    }

    public async Task<int> GetAccessCountAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM ShareAccessLogs WHERE ShareTokenId = @ShareTokenId";
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        return await connection.QuerySingleAsync<int>(sql, new { ShareTokenId = shareTokenIdParam });
    }

    public async Task<int> GetUniqueAccessCountAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(DISTINCT IpAddress) FROM ShareAccessLogs WHERE ShareTokenId = @ShareTokenId";
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        return await connection.QuerySingleAsync<int>(sql, new { ShareTokenId = shareTokenIdParam });
    }

    public async Task<DateTime?> GetLastAccessTimeAsync(Guid shareTokenId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT MAX(AccessedAt) FROM ShareAccessLogs WHERE ShareTokenId = @ShareTokenId";
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        return await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new { ShareTokenId = shareTokenIdParam });
    }

    public async Task<bool> HasRecentAccessAsync(Guid shareTokenId, TimeSpan timeWindow)
    {
        using var connection = _connectionFactory.CreateConnection();
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        const string sql = "SELECT EXISTS(SELECT 1 FROM ShareAccessLogs WHERE ShareTokenId = @ShareTokenId AND AccessedAt >= @CutoffTime)";
        
        var shareTokenIdParam = _isSqlite ? (object)shareTokenId.ToString() : shareTokenId;
        return await connection.QuerySingleAsync<bool>(sql, new { ShareTokenId = shareTokenIdParam, CutoffTime = cutoffTime });
    }

    public async Task<IEnumerable<ShareAccessLog>> GetRecentAccessLogsAsync(int count)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        if (_isSqlite)
        {
            // SQLite专用查询 - 处理数据类型转换
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                ORDER BY sal.AccessedAt DESC
                LIMIT @Count";

            var result = await connection.QueryAsync<dynamic>(sql, new { Count = count });
            
            return result.Select(row => new ShareAccessLog
            {
                Id = Guid.Parse(row.Id),
                ShareTokenId = Guid.Parse(row.ShareTokenId),
                CodeSnippetId = Guid.Parse(row.CodeSnippetId),
                IpAddress = row.IpAddress,
                UserAgent = row.UserAgent,
                Source = row.Source,
                Country = row.Country,
                City = row.City,
                Browser = row.Browser,
                OperatingSystem = row.OperatingSystem,
                DeviceType = row.DeviceType,
                AccessedAt = ParseDateTime(row.AccessedAt),
                IsSuccess = ParseBool(row.IsSuccess),
                FailureReason = row.FailureReason,
                Duration = ParseInt(row.Duration),
                SessionId = row.SessionId,
                Referer = row.Referer,
                AcceptLanguage = row.AcceptLanguage,
                CodeSnippetTitle = row.CodeSnippetTitle,
                CodeSnippetLanguage = row.CodeSnippetLanguage,
                CreatorName = row.CreatorName
            });
        }
        else
        {
            // MySQL查询 - 使用Dapper自动映射
            const string sql = @"
                SELECT sal.*, st.Token, cs.Title as CodeSnippetTitle, cs.Language as CodeSnippetLanguage, u.Username as CreatorName
                FROM ShareAccessLogs sal
                LEFT JOIN ShareTokens st ON sal.ShareTokenId = st.Id
                LEFT JOIN CodeSnippets cs ON sal.CodeSnippetId = cs.Id
                LEFT JOIN Users u ON st.CreatedBy = u.Id
                ORDER BY sal.AccessedAt DESC
                LIMIT @Count";

            return await connection.QueryAsync<ShareAccessLog>(sql, new { Count = count });
        }
    }

    public async Task<int> CleanupOldLogsAsync(int retentionDays)
    {
        using var connection = _connectionFactory.CreateConnection();
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        
        const string sql = "DELETE FROM ShareAccessLogs WHERE AccessedAt < @CutoffDate";
        return await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
    }

    public async Task<int> CleanupFailedAccessLogsAsync(int retentionDays)
    {
        using var connection = _connectionFactory.CreateConnection();
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        
        const string sql = "DELETE FROM ShareAccessLogs WHERE IsSuccess = 0 AND AccessedAt < @CutoffDate";
        return await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
    }
}