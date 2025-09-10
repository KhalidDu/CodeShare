using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 分享服务接口 - 定义分享功能的业务契约
/// </summary>
public interface IShareService
{
    /// <summary>
    /// 创建分享令牌
    /// </summary>
    /// <param name="createShareDto">创建分享请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>创建的分享令牌信息</returns>
    Task<ShareTokenDto> CreateShareTokenAsync(CreateShareDto createShareDto, Guid currentUserId);

    /// <summary>
    /// 根据令牌字符串获取分享信息
    /// </summary>
    /// <param name="token">分享令牌字符串</param>
    /// <param name="password">访问密码（如果有）</param>
    /// <returns>分享令牌信息</returns>
    Task<ShareTokenDto?> GetShareTokenByTokenAsync(string token, string? password = null);

    /// <summary>
    /// 访问分享内容
    /// </summary>
    /// <param name="token">分享令牌字符串</param>
    /// <param name="password">访问密码（如果有）</param>
    /// <param name="ipAddress">访问者IP地址</param>
    /// <param name="userAgent">用户代理</param>
    /// <returns>访问结果</returns>
    Task<AccessShareResponse> AccessShareAsync(string token, string? password, string ipAddress, string? userAgent = null);

    /// <summary>
    /// 根据ID获取分享令牌
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>分享令牌信息</returns>
    Task<ShareTokenDto?> GetShareTokenByIdAsync(Guid id, Guid? currentUserId = null);

    /// <summary>
    /// 获取用户的所有分享令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>用户的分享令牌列表</returns>
    Task<IEnumerable<ShareTokenDto>> GetUserShareTokensAsync(Guid userId, Guid? currentUserId = null);

    /// <summary>
    /// 分页获取用户的分享令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>分页的分享令牌结果</returns>
    Task<PaginatedResult<ShareTokenDto>> GetUserShareTokensPaginatedAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 10, 
        Guid? currentUserId = null);

    /// <summary>
    /// 更新分享令牌设置
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="updateShareDto">更新分享请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>更新后的分享令牌信息</returns>
    Task<ShareTokenDto> UpdateShareTokenAsync(Guid id, UpdateShareDto updateShareDto, Guid currentUserId);

    /// <summary>
    /// 撤销分享令牌（禁用）
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功撤销</returns>
    Task<bool> RevokeShareTokenAsync(Guid id, Guid currentUserId);

    /// <summary>
    /// 删除分享令牌
    /// </summary>
    /// <param name="id">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功删除</returns>
    Task<bool> DeleteShareTokenAsync(Guid id, Guid currentUserId);

    /// <summary>
    /// 验证分享令牌是否有效
    /// </summary>
    /// <param name="token">分享令牌字符串</param>
    /// <param name="password">访问密码（如果有）</param>
    /// <returns>验证结果和相关信息</returns>
    Task<(bool IsValid, string? Message, ShareTokenDto? ShareToken)> ValidateShareTokenAsync(string token, string? password = null);

    /// <summary>
    /// 记录分享访问日志
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="ipAddress">访问者IP地址</param>
    /// <param name="userAgent">用户代理</param>
    /// <param name="isSuccess">访问是否成功</param>
    /// <param name="failureReason">失败原因</param>
    /// <returns>创建的访问日志ID</returns>
    Task<Guid> LogShareAccessAsync(
        Guid shareTokenId,
        string ipAddress,
        string? userAgent = null,
        bool isSuccess = true,
        string? failureReason = null);

    /// <summary>
    /// 批量记录分享访问日志
    /// </summary>
    /// <param name="accessLogs">访问日志列表</param>
    /// <returns>成功记录的数量</returns>
    Task<int> BulkLogShareAccessAsync(List<CreateAccessLogDto> accessLogs);

    /// <summary>
    /// 获取分享统计信息
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>分享统计信息</returns>
    Task<ShareStatsDto> GetShareStatsAsync(Guid shareTokenId, Guid? currentUserId = null);

    /// <summary>
    /// 获取分享访问日志
    /// </summary>
    /// <param name="filter">访问日志过滤器</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>分页的访问日志结果</returns>
    Task<PaginatedResult<ShareAccessLogDto>> GetShareAccessLogsAsync(AccessLogFilter filter, Guid? currentUserId = null);

    /// <summary>
    /// 获取代码片段的所有分享记录
    /// </summary>
    /// <param name="codeSnippetId">代码片段ID</param>
    /// <param name="currentUserId">当前用户ID（用于权限验证）</param>
    /// <returns>代码片段的分享令牌列表</returns>
    Task<IEnumerable<ShareTokenDto>> GetSnippetSharesAsync(Guid codeSnippetId, Guid? currentUserId = null);

    /// <summary>
    /// 检查用户是否有权限访问分享
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否有权限访问</returns>
    Task<bool> HasShareAccessPermissionAsync(Guid shareTokenId, Guid? currentUserId = null);

    /// <summary>
    /// 增加分享访问次数
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <returns>是否成功</returns>
    Task<bool> IncrementShareAccessCountAsync(Guid shareTokenId);

    /// <summary>
    /// 清理过期的分享令牌
    /// </summary>
    /// <returns>清理的数量</returns>
    Task<int> CleanupExpiredShareTokensAsync();

    /// <summary>
    /// 检查分享令牌是否过期
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <returns>是否已过期</returns>
    Task<bool> IsShareTokenExpiredAsync(Guid shareTokenId);

    /// <summary>
    /// 延长分享令牌有效期
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="extendHours">延长的小时数</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>更新后的分享令牌信息</returns>
    Task<ShareTokenDto> ExtendShareTokenExpiryAsync(Guid shareTokenId, int extendHours, Guid currentUserId);

    /// <summary>
    /// 重置分享令牌访问统计
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ResetShareAccessStatsAsync(Guid shareTokenId, Guid currentUserId);

    /// <summary>
    /// 获取分享链接的完整URL
    /// </summary>
    /// <param name="token">分享令牌字符串</param>
    /// <returns>完整的分享链接URL</returns>
    Task<string> GetShareUrlAsync(string token);

    /// <summary>
    /// 生成新的分享令牌字符串
    /// </summary>
    /// <returns>生成的令牌字符串</returns>
    Task<string> GenerateShareTokenAsync();

    /// <summary>
    /// 验证分享密码
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="password">要验证的密码</param>
    /// <returns>密码是否正确</returns>
    Task<bool> VerifySharePasswordAsync(Guid shareTokenId, string password);

    /// <summary>
    /// 获取所有分享链接（管理员功能）
    /// </summary>
    /// <param name="filter">分享过滤器</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页的分享令牌结果</returns>
    Task<PaginatedResult<ShareTokenDto>> GetAllSharesAdminAsync(AdminShareFilter filter, Guid currentUserId);

    /// <summary>
    /// 获取系统分享统计信息（管理员功能）
    /// </summary>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>系统分享统计信息</returns>
    Task<SystemShareStatsDto> GetSystemShareStatsAsync(Guid currentUserId);

    /// <summary>
    /// 批量操作分享链接（管理员功能）
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>批量操作结果</returns>
    Task<BulkOperationResultDto> BulkOperationSharesAsync(BulkShareOperationRequest request, Guid currentUserId);

    /// <summary>
    /// 强制撤销分享令牌（管理员功能）
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功撤销</returns>
    Task<bool> ForceRevokeShareTokenAsync(Guid shareTokenId, Guid currentUserId);

    /// <summary>
    /// 强制删除分享令牌（管理员功能）
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功删除</returns>
    Task<bool> ForceDeleteShareTokenAsync(Guid shareTokenId, Guid currentUserId);
}