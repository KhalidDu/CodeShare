using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 分享令牌仓储接口 - 遵循开闭原则和接口隔离原则
/// </summary>
public interface IShareTokenRepository
{
    // 基础CRUD操作
    Task<ShareToken?> GetByIdAsync(Guid id);
    Task<ShareToken?> GetByTokenAsync(string token);
    Task<ShareToken> CreateAsync(ShareToken shareToken);
    Task<ShareToken> UpdateAsync(ShareToken shareToken);
    Task<bool> DeleteAsync(Guid id);
    
    // 查询操作
    Task<IEnumerable<ShareToken>> GetByCodeSnippetIdAsync(Guid codeSnippetId);
    Task<IEnumerable<ShareToken>> GetByUserIdAsync(Guid userId);
    Task<PaginatedResult<ShareToken>> GetPagedAsync(ShareTokenFilter filter);
    Task<IEnumerable<ShareToken>> GetActiveTokensAsync();
    Task<IEnumerable<ShareToken>> GetExpiredTokensAsync();
    
    // 统计功能
    Task<ShareStatsDto> GetShareStatsAsync(Guid shareTokenId);
    Task<IEnumerable<DailyAccessStatDto>> GetDailyAccessStatsAsync(Guid shareTokenId, DateTime startDate, DateTime endDate);
    Task<int> GetTotalAccessCountAsync(Guid shareTokenId);
    Task<int> GetTodayAccessCountAsync(Guid shareTokenId);
    Task<DateTime?> GetLastAccessTimeAsync(Guid shareTokenId);
    
    // 访问记录操作
    Task<bool> IncrementAccessCountAsync(Guid shareTokenId);
    Task<bool> UpdateLastAccessTimeAsync(Guid shareTokenId);
    Task<bool> DeactivateTokenAsync(Guid shareTokenId);
    Task<bool> ActivateTokenAsync(Guid shareTokenId);
    
    // 批量操作
    Task<int> DeleteExpiredTokensAsync();
    Task<int> DeactivateInactiveTokensAsync(TimeSpan inactiveThreshold);
    Task<bool> ExtendTokenExpirationAsync(Guid shareTokenId, TimeSpan extension);
    
    // 管理员功能
    Task<PaginatedResult<ShareTokenDto>> GetAllSharesAdminAsync(AdminShareFilter filter);
    Task<SystemShareStatsDto> GetSystemShareStatsAsync();
}

/// <summary>
/// 分享令牌过滤器 - 用于分页和条件查询
/// </summary>
public class ShareTokenFilter
{
    /// <summary>
    /// 搜索关键词（搜索令牌、描述、代码片段标题）
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// 代码片段ID筛选
    /// </summary>
    public Guid? CodeSnippetId { get; set; }
    
    /// <summary>
    /// 创建用户ID筛选
    /// </summary>
    public Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// 是否激活筛选
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// 是否过期筛选
    /// </summary>
    public bool? IsExpired { get; set; }
    
    /// <summary>
    /// 权限级别筛选
    /// </summary>
    public Models.SharePermission? Permission { get; set; }
    
    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 10;
}