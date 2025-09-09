using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 分享访问日志仓储接口 - 遵循开闭原则和接口隔离原则
/// </summary>
public interface IShareAccessLogRepository
{
    // 基础CRUD操作
    Task<ShareAccessLog?> GetByIdAsync(Guid id);
    Task<ShareAccessLog> CreateAsync(ShareAccessLog accessLog);
    Task<bool> DeleteAsync(Guid id);

    // 批量操作
    Task<int> BulkInsertAsync(IEnumerable<ShareAccessLog> accessLogs);
    Task<bool> DeleteExpiredLogsAsync(DateTime cutoffDate);
    Task<bool> DeleteByShareTokenIdAsync(Guid shareTokenId);

    // 查询操作
    Task<IEnumerable<ShareAccessLog>> GetByShareTokenIdAsync(Guid shareTokenId);
    Task<IEnumerable<ShareAccessLog>> GetByCodeSnippetIdAsync(Guid codeSnippetId);
    Task<IEnumerable<ShareAccessLog>> GetByIpAddressAsync(string ipAddress, DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShareAccessLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    // 分页查询
    Task<PaginatedResult<ShareAccessLog>> GetPagedAsync(AccessLogFilter filter);
    Task<PaginatedResult<ShareAccessLog>> GetByShareTokenIdPagedAsync(Guid shareTokenId, AccessLogFilter filter);

    // 统计查询
    Task<AccessStats> GetAccessStatsAsync(Guid shareTokenId);
    Task<AccessStats> GetAccessStatsByDateRangeAsync(Guid shareTokenId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<DailyAccessStat>> GetDailyAccessStatsAsync(Guid shareTokenId, int days);
    Task<IEnumerable<HourlyAccessStat>> GetHourlyAccessStatsAsync(Guid shareTokenId, DateTime date);
    Task<IEnumerable<AccessSourceStat>> GetAccessSourceStatsAsync(Guid shareTokenId);
    Task<IEnumerable<DeviceTypeStat>> GetDeviceTypeStatsAsync(Guid shareTokenId);
    Task<IEnumerable<CountryStat>> GetCountryStatsAsync(Guid shareTokenId);
    Task<IEnumerable<BrowserStat>> GetBrowserStatsAsync(Guid shareTokenId);

    // 性能优化查询
    Task<int> GetAccessCountAsync(Guid shareTokenId);
    Task<int> GetUniqueAccessCountAsync(Guid shareTokenId);
    Task<DateTime?> GetLastAccessTimeAsync(Guid shareTokenId);
    Task<bool> HasRecentAccessAsync(Guid shareTokenId, TimeSpan timeWindow);
    Task<IEnumerable<ShareAccessLog>> GetRecentAccessLogsAsync(int count);

    // 数据清理
    Task<int> CleanupOldLogsAsync(int retentionDays);
    Task<int> CleanupFailedAccessLogsAsync(int retentionDays);
}