using Dapper;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CodeSnippetManager.Api.Repositories;

/// <summary>
/// 评论举报仓储实现 - 使用 Dapper ORM 进行数据访问
/// 遵循单一职责原则和依赖注入模式
/// </summary>
public class CommentReportRepository : ICommentReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CommentReportRepository> _logger;

    public CommentReportRepository(IDbConnectionFactory connectionFactory, ILogger<CommentReportRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region 基础 CRUD 操作

    public async Task<CommentReport?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.Id = @Id";

            var result = await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                sql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { Id = id }
            );

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报失败，举报ID: {ReportId}", id);
            throw;
        }
    }

    public async Task<CommentReport> CreateAsync(CommentReport report)
    {
        try
        {
            report.Id = Guid.NewGuid();
            report.CreatedAt = DateTime.UtcNow;
            report.Status = ReportStatus.Pending;

            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO CommentReports (
                    Id, CommentId, UserId, Reason, Description, Status, 
                    CreatedAt, HandledAt, HandledBy, Resolution
                ) VALUES (
                    @Id, @CommentId, @UserId, @Reason, @Description, @Status,
                    @CreatedAt, @HandledAt, @HandledBy, @Resolution
                )";

            await connection.ExecuteAsync(sql, report);
            
            _logger.LogInformation("创建评论举报成功，举报ID: {ReportId}, 评论ID: {CommentId}, 用户ID: {UserId}", 
                report.Id, report.CommentId, report.UserId);
            
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建评论举报失败");
            throw;
        }
    }

    public async Task<CommentReport> UpdateAsync(CommentReport report)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE CommentReports SET
                    Status = @Status,
                    HandledAt = @HandledAt,
                    HandledBy = @HandledBy,
                    Resolution = @Resolution
                WHERE Id = @Id";

            await connection.ExecuteAsync(sql, report);
            
            _logger.LogInformation("更新评论举报成功，举报ID: {ReportId}", report.Id);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新评论举报失败，举报ID: {ReportId}", report.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM CommentReports WHERE Id = @Id";

            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            
            if (affected > 0)
            {
                _logger.LogInformation("删除评论举报成功，举报ID: {ReportId}", id);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除评论举报失败，举报ID: {ReportId}", id);
            throw;
        }
    }

    #endregion

    #region 查询操作

    public async Task<IEnumerable<CommentReport>> GetByCommentIdAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.CommentId = @CommentId
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, User, User, CommentReport>(
                sql,
                (report, user, handler) =>
                {
                    report.User = user;
                    report.Handler = handler;
                    return report;
                },
                new { CommentId = commentId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报列表失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.UserId = @UserId
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, Comment, User, CommentReport>(
                sql,
                (report, comment, handler) =>
                {
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { UserId = userId }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户举报列表失败，用户ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<PaginatedResult<CommentReport>> GetPagedAsync(CommentReportFilter filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var (whereClause, parameters) = BuildWhereClause(filter);
            var orderByClause = BuildOrderByClause(filter.SortBy);
            
            var countSql = $"SELECT COUNT(*) FROM CommentReports cr {whereClause}";
            var total = await connection.QuerySingleAsync<int>(countSql, parameters);

            var offset = (filter.Page - 1) * filter.PageSize;
            var dataSql = $@"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                {whereClause}
                ORDER BY {orderByClause}
                LIMIT @PageSize OFFSET @Offset";

            parameters.PageSize = filter.PageSize;
            parameters.Offset = offset;

            var reports = await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                dataSql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                parameters
            );

            return new PaginatedResult<CommentReport>
            {
                Items = reports,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取评论举报失败");
            throw;
        }
    }

    public async Task<CommentReport?> GetByUserAndCommentAsync(Guid userId, Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.UserId = @UserId AND cr.CommentId = @CommentId";

            var result = await connection.QueryAsync<CommentReport, User, CommentReport>(
                sql,
                (report, handler) =>
                {
                    report.Handler = handler;
                    return report;
                },
                new { UserId = userId, CommentId = commentId }
            );

            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户评论举报失败，用户ID: {UserId}, 评论ID: {CommentId}", userId, commentId);
            throw;
        }
    }

    #endregion

    #region 按状态查询

    public async Task<IEnumerable<CommentReport>> GetByStatusAsync(ReportStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.Status = @Status
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                sql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { Status = (int)status }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据状态获取评论举报失败，状态: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetPendingReportsAsync()
    {
        try
        {
            return await GetByStatusAsync(ReportStatus.Pending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取待处理举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetResolvedReportsAsync()
    {
        try
        {
            return await GetByStatusAsync(ReportStatus.Resolved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取已处理举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetReportsUnderInvestigationAsync()
    {
        try
        {
            return await GetByStatusAsync(ReportStatus.UnderInvestigation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取调查中举报失败");
            throw;
        }
    }

    #endregion

    #region 按原因查询

    public async Task<IEnumerable<CommentReport>> GetByReasonAsync(ReportReason reason)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.Reason = @Reason
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                sql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { Reason = (int)reason }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据原因获取评论举报失败，原因: {Reason}", reason);
            throw;
        }
    }

    public async Task<Dictionary<ReportReason, int>> GetReportCountByReasonAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Reason, COUNT(*) as Count
                FROM CommentReports
                GROUP BY Reason";

            var results = await connection.QueryAsync<(int Reason, int Count)>(sql);
            
            var dict = new Dictionary<ReportReason, int>();
            foreach (var result in results)
            {
                dict[(ReportReason)result.Reason] = result.Count;
            }
            
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取举报原因统计失败");
            throw;
        }
    }

    public async Task<IEnumerable<(ReportReason Reason, int Count)>> GetTopReportReasonsAsync(int count = 5)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT Reason, COUNT(*) as Count
                FROM CommentReports
                GROUP BY Reason
                ORDER BY Count DESC
                LIMIT @Count";

            var results = await connection.QueryAsync<(int Reason, int Count)>(sql, new { Count = count });
            return results.Select(r => ((ReportReason)r.Reason, r.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取热门举报原因失败");
            throw;
        }
    }

    #endregion

    #region 按处理人查询

    public async Task<IEnumerable<CommentReport>> GetByHandledByAsync(Guid handledBy)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                WHERE cr.HandledBy = @HandledBy
                ORDER BY cr.HandledAt DESC";

            return await connection.QueryAsync<CommentReport, User, Comment, CommentReport>(
                sql,
                (report, user, comment) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    return report;
                },
                new { HandledBy = handledBy }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据处理人获取举报失败，处理人ID: {HandledBy}", handledBy);
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetReportsByHandlerAsync(Guid handlerId, ReportStatus? status = null)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = new StringBuilder(@"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                WHERE cr.HandledBy = @HandlerId");

            var parameters = new DynamicParameters();
            parameters.Add("HandlerId", handlerId);

            if (status.HasValue)
            {
                sql.Append(" AND cr.Status = @Status");
                parameters.Add("Status", (int)status.Value);
            }

            sql.Append(" ORDER BY cr.HandledAt DESC");

            return await connection.QueryAsync<CommentReport, User, Comment, CommentReport>(
                sql.ToString(),
                (report, user, comment) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    return report;
                },
                parameters
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据处理人获取举报失败，处理人ID: {HandlerId}", handlerId);
            throw;
        }
    }

    #endregion

    #region 重复举报检查

    public async Task<bool> HasUserReportedCommentAsync(Guid commentId, Guid userId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentReports 
                WHERE CommentId = @CommentId AND UserId = @UserId AND Status != @RejectedStatus";

            var count = await connection.QuerySingleAsync<int>(sql, new 
            { 
                CommentId = commentId, 
                UserId = userId,
                RejectedStatus = (int)ReportStatus.Rejected
            });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查用户举报状态失败，评论ID: {CommentId}, 用户ID: {UserId}", commentId, userId);
            throw;
        }
    }

    public async Task<int> GetReportCountByCommentIdAsync(Guid commentId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentReports 
                WHERE CommentId = @CommentId AND Status != @RejectedStatus";

            return await connection.QuerySingleAsync<int>(sql, new 
            { 
                CommentId = commentId,
                RejectedStatus = (int)ReportStatus.Rejected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取评论举报数失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> IsCommentReportedMultipleTimesAsync(Guid commentId, int threshold = 3)
    {
        try
        {
            var count = await GetReportCountByCommentIdAsync(commentId);
            return count >= threshold;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查评论多次举报失败，评论ID: {CommentId}", commentId);
            throw;
        }
    }

    #endregion

    #region 批量查询

    public async Task<IEnumerable<CommentReport>> GetReportsByCommentIdsAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.CommentId IN @CommentIds
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, User, User, CommentReport>(
                sql,
                (report, user, handler) =>
                {
                    report.User = user;
                    report.Handler = handler;
                    return report;
                },
                new { CommentIds = commentIds }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取评论举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetReportsByUserIdsAsync(IEnumerable<Guid> userIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.UserId IN @UserIds
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, Comment, User, CommentReport>(
                sql,
                (report, comment, handler) =>
                {
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { UserIds = userIds }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取用户举报失败");
            throw;
        }
    }

    public async Task<Dictionary<Guid, int>> GetReportCountsByCommentIdsAsync(IEnumerable<Guid> commentIds)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT CommentId, COUNT(*) as ReportCount
                FROM CommentReports
                WHERE CommentId IN @CommentIds AND Status != @RejectedStatus
                GROUP BY CommentId";

            var results = await connection.QueryAsync<(Guid CommentId, int ReportCount)>(sql, new 
            { 
                CommentIds = commentIds,
                RejectedStatus = (int)ReportStatus.Rejected
            });
            
            var dict = new Dictionary<Guid, int>();
            foreach (var commentId in commentIds)
            {
                dict[commentId] = 0;
            }
            
            foreach (var result in results)
            {
                dict[result.CommentId] = result.ReportCount;
            }
            
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取举报数失败");
            throw;
        }
    }

    #endregion

    #region 统计信息

    public async Task<int> GetTotalReportsCountAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentReports";

            return await connection.QuerySingleAsync<int>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取总举报数失败");
            throw;
        }
    }

    public async Task<int> GetReportsCountByStatusAsync(ReportStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentReports WHERE Status = @Status";

            return await connection.QuerySingleAsync<int>(sql, new { Status = (int)status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据状态获取举报数失败，状态: {Status}", status);
            throw;
        }
    }

    public async Task<int> GetReportsCountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT COUNT(*) FROM CommentReports 
                WHERE CreatedAt BETWEEN @StartDate AND @EndDate";

            return await connection.QuerySingleAsync<int>(sql, new { StartDate = startDate, EndDate = endDate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据日期范围获取举报数失败");
            throw;
        }
    }

    public async Task<CommentReportStatsDto> GetReportStatsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    COUNT(*) as TotalReports,
                    SUM(CASE WHEN Status = @PendingStatus THEN 1 ELSE 0 END) as PendingReports,
                    SUM(CASE WHEN Status = @ResolvedStatus THEN 1 ELSE 0 END) as ResolvedReports,
                    SUM(CASE WHEN Status = @RejectedStatus THEN 1 ELSE 0 END) as RejectedReports,
                    SUM(CASE WHEN Status = @UnderInvestigationStatus THEN 1 ELSE 0 END) as UnderInvestigationReports,
                    AVG(CASE WHEN HandledAt IS NOT NULL AND Status != @PendingStatus THEN 
                        TIMESTAMPDIFF(HOUR, CreatedAt, HandledAt) ELSE NULL END) as AverageResolutionTimeHours,
                    SUM(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 ELSE 0 END) as ReportsThisWeek,
                    SUM(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) as ReportsThisMonth,
                    MAX(CreatedAt) as LastReportTime
                FROM CommentReports";

            return await connection.QuerySingleAsync<CommentReportStatsDto>(sql, new
            {
                PendingStatus = (int)ReportStatus.Pending,
                ResolvedStatus = (int)ReportStatus.Resolved,
                RejectedStatus = (int)ReportStatus.Rejected,
                UnderInvestigationStatus = (int)ReportStatus.UnderInvestigation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取举报统计信息失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReportSummaryDto>> GetDailyReportStatsAsync(int days = 30)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    DATE(CreatedAt) as Date,
                    COUNT(*) as TotalReports,
                    SUM(CASE WHEN Status = @PendingStatus THEN 1 ELSE 0 END) as PendingReports,
                    SUM(CASE WHEN Status = @ResolvedStatus THEN 1 ELSE 0 END) as ResolvedReports,
                    SUM(CASE WHEN Status = @RejectedStatus THEN 1 ELSE 0 END) as RejectedReports
                FROM CommentReports
                WHERE CreatedAt >= DATE_SUB(NOW(), INTERVAL @Days DAY)
                GROUP BY DATE(CreatedAt)
                ORDER BY Date DESC";

            return await connection.QueryAsync<CommentReportSummaryDto>(sql, new
            {
                Days = days,
                PendingStatus = (int)ReportStatus.Pending,
                ResolvedStatus = (int)ReportStatus.Resolved,
                RejectedStatus = (int)ReportStatus.Rejected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取每日举报统计失败");
            throw;
        }
    }

    #endregion

    #region 时间相关查询

    public async Task<IEnumerable<CommentReport>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.CreatedAt BETWEEN @StartDate AND @EndDate
                ORDER BY cr.CreatedAt DESC";

            return await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                sql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new { StartDate = startDate, EndDate = endDate }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据日期范围获取举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetRecentReportsAsync(int hours = 24)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddHours(-hours);
            
            return await GetReportsByDateRangeAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近举报失败");
            throw;
        }
    }

    public async Task<DateTime?> GetOldestPendingReportTimeAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT MIN(CreatedAt) FROM CommentReports 
                WHERE Status = @PendingStatus";

            return await connection.QuerySingleOrDefaultAsync<DateTime?>(sql, new 
            { 
                PendingStatus = (int)ReportStatus.Pending 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最久待处理举报时间失败");
            throw;
        }
    }

    #endregion

    #region 高级查询

    public async Task<IEnumerable<CommentReport>> GetHighPriorityReportsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                WITH ReportPriority AS (
                    SELECT 
                        cr.*,
                        COUNT(r.id) as ReportCount,
                        TIMESTAMPDIFF(HOUR, cr.CreatedAt, NOW()) as HoursPending
                    FROM CommentReports cr
                    LEFT JOIN CommentReports r ON cr.CommentId = r.CommentId AND r.Status != @RejectedStatus
                    WHERE cr.Status = @PendingStatus
                    GROUP BY cr.Id
                    HAVING ReportCount >= 3 OR HoursPending >= 24
                )
                SELECT cr.*, 
                       u.Id as UserId, u.Username, u.Email, u.Role, u.CreatedAt as UserCreatedAt, u.UpdatedAt as UserUpdatedAt, u.IsActive,
                       c.Id as CommentId, c.Content as CommentContent, c.CreatedAt as CommentCreatedAt,
                       h.Id as HandlerId, h.Username as HandlerName, h.Email as HandlerEmail
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                WHERE cr.Id IN (SELECT Id FROM ReportPriority)
                ORDER BY cr.CreatedAt ASC";

            return await connection.QueryAsync<CommentReport, User, Comment, User, CommentReport>(
                sql,
                (report, user, comment, handler) =>
                {
                    report.User = user;
                    report.Comment = comment;
                    report.Handler = handler;
                    return report;
                },
                new
                {
                    PendingStatus = (int)ReportStatus.Pending,
                    RejectedStatus = (int)ReportStatus.Rejected
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取高优先级举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetReportsNeedingAttentionAsync()
    {
        try
        {
            return await GetHighPriorityReportsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取需要关注的举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<(Guid CommentId, int ReportCount)>> GetMostReportedCommentsAsync(int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT CommentId, COUNT(*) as ReportCount
                FROM CommentReports
                WHERE Status != @RejectedStatus
                GROUP BY CommentId
                ORDER BY ReportCount DESC
                LIMIT @Count";

            return await connection.QueryAsync<(Guid CommentId, int ReportCount)>(sql, new 
            { 
                Count = count,
                RejectedStatus = (int)ReportStatus.Rejected
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取被举报最多的评论失败");
            throw;
        }
    }

    #endregion

    #region 处理相关

    public async Task<CommentReport?> MarkAsResolvedAsync(Guid reportId, Guid handledBy, string? resolution = null)
    {
        try
        {
            var report = await GetByIdAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.Status = ReportStatus.Resolved;
            report.HandledAt = DateTime.UtcNow;
            report.HandledBy = handledBy;
            report.Resolution = resolution;

            await UpdateAsync(report);
            
            _logger.LogInformation("标记举报为已处理成功，举报ID: {ReportId}, 处理人ID: {HandledBy}", reportId, handledBy);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记举报为已处理失败，举报ID: {ReportId}", reportId);
            throw;
        }
    }

    public async Task<CommentReport?> MarkAsRejectedAsync(Guid reportId, Guid handledBy, string? resolution = null)
    {
        try
        {
            var report = await GetByIdAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.Status = ReportStatus.Rejected;
            report.HandledAt = DateTime.UtcNow;
            report.HandledBy = handledBy;
            report.Resolution = resolution;

            await UpdateAsync(report);
            
            _logger.LogInformation("标记举报为已驳回成功，举报ID: {ReportId}, 处理人ID: {HandledBy}", reportId, handledBy);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记举报为已驳回失败，举报ID: {ReportId}", reportId);
            throw;
        }
    }

    public async Task<CommentReport?> AssignToHandlerAsync(Guid reportId, Guid handlerId)
    {
        try
        {
            var report = await GetByIdAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.HandledBy = handlerId;

            await UpdateAsync(report);
            
            _logger.LogInformation("分配举报处理人成功，举报ID: {ReportId}, 处理人ID: {HandlerId}", reportId, handlerId);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分配举报处理人失败，举报ID: {ReportId}", reportId);
            throw;
        }
    }

    public async Task<CommentReport?> UpdateResolutionAsync(Guid reportId, string resolution)
    {
        try
        {
            var report = await GetByIdAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.Resolution = resolution;

            await UpdateAsync(report);
            
            _logger.LogInformation("更新举报处理结果成功，举报ID: {ReportId}", reportId);
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新举报处理结果失败，举报ID: {ReportId}", reportId);
            throw;
        }
    }

    #endregion

    #region 批量操作

    public async Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> reportIds, ReportStatus status)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE CommentReports SET
                    Status = @Status,
                    HandledAt = CASE WHEN @Status != @PendingStatus THEN NOW() ELSE HandledAt END
                WHERE Id IN @ReportIds";

            var affected = await connection.ExecuteAsync(sql, new
            {
                ReportIds = reportIds,
                Status = (int)status,
                PendingStatus = (int)ReportStatus.Pending
            });

            _logger.LogInformation("批量更新举报状态成功，影响行数: {AffectedRows}", affected);
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新举报状态失败");
            throw;
        }
    }

    public async Task<int> BulkAssignToHandlerAsync(IEnumerable<Guid> reportIds, Guid handlerId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE CommentReports SET
                    HandledBy = @HandlerId
                WHERE Id IN @ReportIds";

            var affected = await connection.ExecuteAsync(sql, new
            {
                ReportIds = reportIds,
                HandlerId = handlerId
            });

            _logger.LogInformation("批量分配举报处理人成功，影响行数: {AffectedRows}", affected);
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量分配举报处理人失败");
            throw;
        }
    }

    public async Task<int> BulkDeleteOldReportsAsync(DateTime olderThan)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                DELETE FROM CommentReports 
                WHERE CreatedAt < @OlderThan AND Status = @ResolvedStatus";

            var affected = await connection.ExecuteAsync(sql, new
            {
                OlderThan = olderThan,
                ResolvedStatus = (int)ReportStatus.Resolved
            });

            _logger.LogInformation("批量删除旧举报成功，删除数量: {Count}", affected);
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除旧举报失败");
            throw;
        }
    }

    #endregion

    #region 缓存相关

    public async Task<IEnumerable<CommentReport>?> GetReportsFromCacheAsync(Guid commentId)
    {
        // 这里可以集成缓存服务，目前直接从数据库获取
        return await GetByCommentIdAsync(commentId);
    }

    public async Task<bool> SetReportsCacheAsync(Guid commentId, IEnumerable<CommentReport> reports, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool> RemoveReportsCacheAsync(Guid commentId)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<bool> SetReportCountCacheAsync(Guid commentId, int count, TimeSpan? expiration = null)
    {
        // 这里可以集成缓存服务
        return true;
    }

    public async Task<int?> GetReportCountFromCacheAsync(Guid commentId)
    {
        // 这里可以集成缓存服务，目前直接从数据库获取
        return await GetReportCountByCommentIdAsync(commentId);
    }

    #endregion

    #region 清理和维护

    public async Task<int> CleanOldResolvedReportsAsync(DateTime olderThan)
    {
        try
        {
            return await BulkDeleteOldReportsAsync(olderThan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理旧已处理举报失败");
            throw;
        }
    }

    public async Task<int> CleanDuplicateReportsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                DELETE cr1 FROM CommentReports cr1
                INNER JOIN CommentReports cr2 ON 
                    cr1.CommentId = cr2.CommentId AND 
                    cr1.UserId = cr2.UserId AND 
                    cr1.Id < cr2.Id AND
                    cr1.Status = @PendingStatus AND
                    cr2.Status = @PendingStatus
                WHERE cr1.CreatedAt < DATE_SUB(NOW(), INTERVAL 1 DAY)";

            var affected = await connection.ExecuteAsync(sql, new { PendingStatus = (int)ReportStatus.Pending });
            _logger.LogInformation("清理重复举报成功，清理数量: {Count}", affected);
            
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理重复举报失败");
            throw;
        }
    }

    public async Task<bool> ValidateReportIntegrityAsync(Guid reportId)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM CommentReports WHERE Id = @ReportId) as ReportExists,
                    (SELECT COUNT(*) FROM Comments WHERE Id = (SELECT CommentId FROM CommentReports WHERE Id = @ReportId)) as CommentExists,
                    (SELECT COUNT(*) FROM Users WHERE Id = (SELECT UserId FROM CommentReports WHERE Id = @ReportId)) as UserExists";

            var result = await connection.QuerySingleAsync<(int ReportExists, int CommentExists, int UserExists)>(sql, new { ReportId = reportId });
            
            return result.ReportExists > 0 && result.CommentExists > 0 && result.UserExists > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证举报完整性失败，举报ID: {ReportId}", reportId);
            throw;
        }
    }

    public async Task<int> AutoResolveOldReportsAsync(TimeSpan resolveAfter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE CommentReports SET
                    Status = @ResolvedStatus,
                    HandledAt = NOW(),
                    Resolution = 'Auto-resolved due to inactivity'
                WHERE Status = @PendingStatus AND CreatedAt < DATE_SUB(NOW(), INTERVAL @ResolveAfterSecond SECOND)";

            var affected = await connection.ExecuteAsync(sql, new
            {
                PendingStatus = (int)ReportStatus.Pending,
                ResolvedStatus = (int)ReportStatus.Resolved,
                ResolveAfterSecond = (int)resolveAfter.TotalSeconds
            });

            _logger.LogInformation("自动处理旧举报成功，处理数量: {Count}", affected);
            return affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "自动处理旧举报失败");
            throw;
        }
    }

    #endregion

    #region 分析和报告

    public async Task<IEnumerable<(DateTime Date, int Count)>> GetReportTrendAsync(int days = 30)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT DATE(CreatedAt) as Date, COUNT(*) as Count
                FROM CommentReports
                WHERE CreatedAt >= DATE_SUB(NOW(), INTERVAL @Days DAY)
                GROUP BY DATE(CreatedAt)
                ORDER BY Date ASC";

            return await connection.QueryAsync<(DateTime Date, int Count)>(sql, new { Days = days });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取举报趋势失败");
            throw;
        }
    }

    public async Task<Dictionary<Guid, int>> GetUserReportStatisticsAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT UserId, COUNT(*) as ReportCount
                FROM CommentReports
                GROUP BY UserId
                ORDER BY ReportCount DESC";

            var results = await connection.QueryAsync<(Guid UserId, int ReportCount)>(sql);
            return results.ToDictionary(x => x.UserId, x => x.ReportCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户举报统计失败");
            throw;
        }
    }

    public async Task<IEnumerable<(Guid UserId, int ReportCount)>> GetTopReportingUsersAsync(int count = 10)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT UserId, COUNT(*) as ReportCount
                FROM CommentReports
                GROUP BY UserId
                ORDER BY ReportCount DESC
                LIMIT @Count";

            return await connection.QueryAsync<(Guid UserId, int ReportCount)>(sql, new { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取举报用户排行榜失败");
            throw;
        }
    }

    public async Task<double> GetAverageResolutionTimeAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT AVG(TIMESTAMPDIFF(HOUR, CreatedAt, HandledAt)) as AverageHours
                FROM CommentReports
                WHERE Status = @ResolvedStatus AND HandledAt IS NOT NULL";

            return await connection.QuerySingleOrDefaultAsync<double>(sql, new 
            { 
                ResolvedStatus = (int)ReportStatus.Resolved 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取平均处理时间失败");
            throw;
        }
    }

    #endregion

    #region 实时数据

    public async Task<int> GetRealtimePendingReportsCountAsync()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM CommentReports WHERE Status = @PendingStatus";

            return await connection.QuerySingleAsync<int>(sql, new { PendingStatus = (int)ReportStatus.Pending });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取实时待处理举报数失败");
            throw;
        }
    }

    public async Task<bool> HasUrgentReportsAsync()
    {
        try
        {
            var highPriorityReports = await GetHighPriorityReportsAsync();
            return highPriorityReports.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查紧急举报失败");
            throw;
        }
    }

    public async Task<IEnumerable<CommentReport>> GetUrgentReportsAsync()
    {
        try
        {
            return await GetHighPriorityReportsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取紧急举报失败");
            throw;
        }
    }

    #endregion

    #region 导出相关

    public async Task<IEnumerable<CommentReportExportDto>> GetReportsForExportAsync(CommentReportFilter filter)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var (whereClause, parameters) = BuildWhereClause(filter);
            var orderByClause = BuildOrderByClause(filter.SortBy);
            
            var sql = $@"
                SELECT 
                    cr.Id,
                    cr.CommentId,
                    c.Content as CommentContent,
                    cr.UserId,
                    u.Username as UserName,
                    cr.Reason,
                    cr.Description,
                    cr.Status,
                    cr.CreatedAt,
                    cr.HandledAt,
                    h.Username as HandlerName,
                    cr.Resolution,
                    TIMESTAMPDIFF(HOUR, cr.CreatedAt, COALESCE(cr.HandledAt, NOW())) as ResolutionTimeHours
                FROM CommentReports cr
                LEFT JOIN Users u ON cr.UserId = u.Id
                LEFT JOIN Comments c ON cr.CommentId = c.Id
                LEFT JOIN Users h ON cr.HandledBy = h.Id
                {whereClause}
                ORDER BY {orderByClause}";

            return await connection.QueryAsync<CommentReportExportDto>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取导出举报数据失败");
            throw;
        }
    }

    public async Task<byte[]> GenerateReportCsvAsync(CommentReportFilter filter)
    {
        try
        {
            var reports = await GetReportsForExportAsync(filter);
            
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            
            // 写入CSV头
            await writer.WriteLineAsync("ID,评论ID,评论内容,举报用户ID,举报用户名,举报原因,举报描述,状态,创建时间,处理时间,处理人,处理结果,处理时长(小时)");
            
            // 写入数据行
            foreach (var report in reports)
            {
                await writer.WriteLineAsync($"{report.Id},{report.CommentId},\"{report.CommentContent}\",{report.UserId},{report.UserName},{report.Reason},\"{report.Description}\",{report.Status},{report.CreatedAt:yyyy-MM-dd HH:mm:ss},{report.HandledAt:yyyy-MM-dd HH:mm:ss},{report.HandlerName},\"{report.Resolution}\",{report.ResolutionTimeHours}");
            }
            
            await writer.FlushAsync();
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成举报CSV失败");
            throw;
        }
    }

    public async Task<byte[]> GenerateReportJsonAsync(CommentReportFilter filter)
    {
        try
        {
            var reports = await GetReportsForExportAsync(filter);
            var json = JsonSerializer.Serialize(reports, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成举报JSON失败");
            throw;
        }
    }

    #endregion

    #region 私有辅助方法

    private (string WhereClause, DynamicParameters Parameters) BuildWhereClause(CommentReportFilter filter)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (filter.Status.HasValue)
        {
            conditions.Add("cr.Status = @Status");
            parameters.Add("Status", (int)filter.Status.Value);
        }

        if (filter.Reason.HasValue)
        {
            conditions.Add("cr.Reason = @Reason");
            parameters.Add("Reason", (int)filter.Reason.Value);
        }

        if (filter.CommentId.HasValue)
        {
            conditions.Add("cr.CommentId = @CommentId");
            parameters.Add("CommentId", filter.CommentId.Value);
        }

        if (filter.UserId.HasValue)
        {
            conditions.Add("cr.UserId = @UserId");
            parameters.Add("UserId", filter.UserId.Value);
        }

        if (filter.HandledBy.HasValue)
        {
            conditions.Add("cr.HandledBy = @HandledBy");
            parameters.Add("HandledBy", filter.HandledBy.Value);
        }

        if (filter.StartDate.HasValue)
        {
            conditions.Add("cr.CreatedAt >= @StartDate");
            parameters.Add("StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            conditions.Add("cr.CreatedAt <= @EndDate");
            parameters.Add("EndDate", filter.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            conditions.Add("(c.Content LIKE @Search OR u.Username LIKE @Search OR cr.Description LIKE @Search)");
            parameters.Add("Search", $"%{filter.Search}%");
        }

        if (filter.HighPriorityOnly.HasValue && filter.HighPriorityOnly.Value)
        {
            conditions.Add(@"
                (cr.Id IN (
                    SELECT cr2.Id FROM CommentReports cr2
                    LEFT JOIN CommentReports r ON cr2.CommentId = r.CommentId AND r.Status != @RejectedStatus
                    WHERE cr2.Status = @PendingStatus
                    GROUP BY cr2.Id
                    HAVING COUNT(r.id) >= 3 OR TIMESTAMPDIFF(HOUR, cr2.CreatedAt, NOW()) >= 24
                ))");
            parameters.Add("PendingStatus", (int)ReportStatus.Pending);
            parameters.Add("RejectedStatus", (int)ReportStatus.Rejected);
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        return (whereClause, parameters);
    }

    private string BuildOrderByClause(ReportSort sortBy)
    {
        return sortBy switch
        {
            ReportSort.CreatedAtAsc => "cr.CreatedAt ASC",
            ReportSort.CreatedAtDesc => "cr.CreatedAt DESC",
            ReportSort.Reason => "cr.Reason ASC, cr.CreatedAt DESC",
            ReportSort.Status => "cr.Status ASC, cr.CreatedAt DESC",
            _ => "cr.CreatedAt DESC"
        };
    }

    #endregion
}