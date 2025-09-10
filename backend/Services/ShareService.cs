using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 分享服务实现 - 遵循单一职责原则和依赖倒置原则
/// 实现分享令牌生成、验证、撤销和统计功能
/// </summary>
public class ShareService : IShareService
{
    private readonly IShareTokenRepository _shareTokenRepository;
    private readonly IShareAccessLogRepository _shareAccessLogRepository;
    private readonly ICodeSnippetService _codeSnippetService;
    private readonly IPermissionService _permissionService;
    private readonly ICacheService _cacheService;
    private readonly IInputValidationService _validationService;
    private readonly ILogger<ShareService> _logger;

    // 缓存键前缀
    private const string SHARE_TOKEN_CACHE_PREFIX = "share_token_";
    private const string SHARE_STATS_CACHE_PREFIX = "share_stats_";
    private const string SHARE_URL_CACHE_PREFIX = "share_url_";

    public ShareService(
        IShareTokenRepository shareTokenRepository,
        IShareAccessLogRepository shareAccessLogRepository,
        ICodeSnippetService codeSnippetService,
        IPermissionService permissionService,
        ICacheService cacheService,
        IInputValidationService validationService,
        ILogger<ShareService> logger)
    {
        _shareTokenRepository = shareTokenRepository ?? throw new ArgumentNullException(nameof(shareTokenRepository));
        _shareAccessLogRepository = shareAccessLogRepository ?? throw new ArgumentNullException(nameof(shareAccessLogRepository));
        _codeSnippetService = codeSnippetService ?? throw new ArgumentNullException(nameof(codeSnippetService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 创建分享令牌
    /// </summary>
    public async Task<ShareTokenDto> CreateShareTokenAsync(CreateShareDto createShareDto, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 开始创建分享令牌，代码片段ID: {CodeSnippetId}", 
                currentUserId, createShareDto.CodeSnippetId);

            // 验证输入
            await ValidateCreateShareRequestAsync(createShareDto);

            // 检查代码片段是否存在且用户有权限
            var snippet = await _codeSnippetService.GetSnippetAsync(createShareDto.CodeSnippetId, currentUserId);
            if (snippet == null)
            {
                throw new ArgumentException("代码片段不存在或无权访问");
            }

            // 检查用户是否有分享权限
            var canShare = await _permissionService.CanAccessSnippetAsync(
                currentUserId, createShareDto.CodeSnippetId, PermissionOperation.Read);
            if (!canShare)
            {
                throw new UnauthorizedAccessException("用户没有分享此代码片段的权限");
            }

            // 生成分享令牌
            var token = await GenerateShareTokenAsync();
            var hashedPassword = !string.IsNullOrEmpty(createShareDto.Password) 
                ? HashPassword(createShareDto.Password) 
                : null;

            // 计算过期时间
            var expiresAt = createShareDto.ExpiresInHours > 0 
                ? DateTime.UtcNow.AddHours(createShareDto.ExpiresInHours) 
                : DateTime.MaxValue;

            // 创建分享令牌实体
            var shareToken = new ShareToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                CodeSnippetId = createShareDto.CodeSnippetId,
                CreatedBy = currentUserId,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                AccessCount = 0,
                MaxAccessCount = createShareDto.MaxAccessCount,
                Permission = createShareDto.Permission,
                Description = createShareDto.Description,
                Password = hashedPassword,
                AllowDownload = createShareDto.AllowDownload,
                AllowCopy = createShareDto.AllowCopy,
                LastAccessedAt = null,
                CreatorName = string.Empty, // 将由仓储层填充
                CodeSnippetTitle = snippet.Title,
                CodeSnippetLanguage = snippet.Language
            };

            // 保存到数据库
            var created = await _shareTokenRepository.CreateAsync(shareToken);

            // 清除相关缓存
            await ClearUserShareTokensCacheAsync(currentUserId);
            await ClearSnippetSharesCacheAsync(createShareDto.CodeSnippetId);

            _logger.LogInformation("用户 {UserId} 成功创建分享令牌，令牌ID: {ShareTokenId}", 
                currentUserId, created.Id);

            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "用户 {UserId} 创建分享令牌失败，代码片段ID: {CodeSnippetId}", 
                currentUserId, createShareDto.CodeSnippetId);
            throw;
        }
    }

    /// <summary>
    /// 根据令牌字符串获取分享信息
    /// </summary>
    public async Task<ShareTokenDto?> GetShareTokenByTokenAsync(string token, string? password = null)
    {
        try
        {
            _logger.LogDebug("根据令牌字符串获取分享信息: {Token}", token);

            // 从缓存获取
            var cacheKey = $"{SHARE_TOKEN_CACHE_PREFIX}{token}";
            var cached = await _cacheService.GetAsync<ShareTokenDto>(cacheKey);
            if (cached != null)
            {
                // 验证密码
                if (cached.HasPassword && !await VerifySharePasswordAsync(cached.Id, password ?? string.Empty))
                {
                    _logger.LogWarning("分享令牌 {Token} 密码验证失败", token);
                    return null;
                }
                return cached;
            }

            // 从数据库获取
            var shareToken = await _shareTokenRepository.GetByTokenAsync(token);
            if (shareToken == null)
            {
                _logger.LogWarning("分享令牌不存在: {Token}", token);
                return null;
            }

            // 验证令牌状态
            if (!await ValidateShareTokenStatusAsync(shareToken))
            {
                return null;
            }

            // 验证密码
            if (shareToken.Password != null && !await VerifySharePasswordAsync(shareToken.Id, password ?? string.Empty))
            {
                _logger.LogWarning("分享令牌 {Token} 密码验证失败", token);
                return null;
            }

            var dto = MapToDto(shareToken);

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据令牌字符串获取分享信息失败: {Token}", token);
            throw;
        }
    }

    /// <summary>
    /// 访问分享内容
    /// </summary>
    public async Task<AccessShareResponse> AccessShareAsync(string token, string? password, string ipAddress, string? userAgent = null)
    {
        try
        {
            _logger.LogDebug("访问分享内容: {Token}, IP: {IpAddress}", token, ipAddress);

            // 验证分享令牌
            var shareToken = await _shareTokenRepository.GetByTokenAsync(token);
            if (shareToken == null)
            {
                return new AccessShareResponse
                {
                    Success = false,
                    ErrorMessage = "分享链接不存在或已失效"
                };
            }

            // 验证令牌状态
            if (!await ValidateShareTokenStatusAsync(shareToken))
            {
                return new AccessShareResponse
                {
                    Success = false,
                    ErrorMessage = "分享链接已过期或已被禁用"
                };
            }

            // 验证密码
            if (shareToken.Password != null && !await VerifySharePasswordAsync(shareToken.Id, password ?? string.Empty))
            {
                // 记录失败的访问尝试
                await LogShareAccessAsync(shareToken.Id, ipAddress, userAgent, false, "密码错误");
                
                return new AccessShareResponse
                {
                    Success = false,
                    ErrorMessage = "访问密码错误"
                };
            }

            // 检查访问次数限制
            if (shareToken.MaxAccessCount > 0 && shareToken.AccessCount >= shareToken.MaxAccessCount)
            {
                return new AccessShareResponse
                {
                    Success = false,
                    ErrorMessage = "分享链接访问次数已达上限"
                };
            }

            // 获取关联的代码片段
            var snippet = await _codeSnippetService.GetSnippetAsync(shareToken.CodeSnippetId);
            if (snippet == null)
            {
                return new AccessShareResponse
                {
                    Success = false,
                    ErrorMessage = "关联的代码片段不存在"
                };
            }

            // 增加访问次数
            shareToken.AccessCount++;
            shareToken.LastAccessedAt = DateTime.UtcNow;
            await _shareTokenRepository.UpdateAsync(shareToken);

            // 记录访问日志
            var accessLogId = await LogShareAccessAsync(shareToken.Id, ipAddress, userAgent, true);

            // 清除缓存
            await ClearShareTokenCacheAsync(token);

            // 构建返回的分享令牌信息
            var shareTokenDto = MapToDto(shareToken);
            shareTokenDto.CodeSnippetCode = snippet.Code;
            shareTokenDto.CodeSnippetTitle = snippet.Title;
            shareTokenDto.CodeSnippetLanguage = snippet.Language;
            shareTokenDto.CurrentAccessCount = shareToken.AccessCount;

            _logger.LogInformation("分享内容访问成功，令牌: {Token}, 代码片段ID: {CodeSnippetId}", 
                token, shareToken.CodeSnippetId);

            return new AccessShareResponse
            {
                Success = true,
                ShareToken = shareTokenDto,
                AccessLogId = accessLogId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "访问分享内容失败: {Token}", token);
            return new AccessShareResponse
            {
                Success = false,
                ErrorMessage = "访问分享内容时发生内部错误"
            };
        }
    }

    /// <summary>
    /// 根据ID获取分享令牌
    /// </summary>
    public async Task<ShareTokenDto?> GetShareTokenByIdAsync(Guid id, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("根据ID获取分享令牌: {ShareTokenId}, 用户ID: {UserId}", id, currentUserId);

            var shareToken = await _shareTokenRepository.GetByIdAsync(id);
            if (shareToken == null)
            {
                return null;
            }

            // 检查权限
            if (currentUserId.HasValue && shareToken.CreatedBy != currentUserId.Value)
            {
                var canAccess = await _permissionService.CanAccessSnippetAsync(
                    currentUserId.Value, shareToken.CodeSnippetId, PermissionOperation.Read);
                if (!canAccess)
                {
                    return null;
                }
            }

            return MapToDto(shareToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据ID获取分享令牌失败: {ShareTokenId}", id);
            throw;
        }
    }

    /// <summary>
    /// 获取用户的所有分享令牌
    /// </summary>
    public async Task<IEnumerable<ShareTokenDto>> GetUserShareTokensAsync(Guid userId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取用户分享令牌: {UserId}, 当前用户: {CurrentUserId}", userId, currentUserId);

            // 检查权限
            if (currentUserId.HasValue && userId != currentUserId.Value)
            {
                var canView = await _permissionService.IsAdminAsync(currentUserId.Value);
                if (!canView)
                {
                    throw new UnauthorizedAccessException("无权查看其他用户的分享令牌");
                }
            }

            var shareTokens = await _shareTokenRepository.GetByUserIdAsync(userId);
            return shareTokens.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户分享令牌失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 分页获取用户的分享令牌
    /// </summary>
    public async Task<PaginatedResult<ShareTokenDto>> GetUserShareTokensPaginatedAsync(
        Guid userId, int page = 1, int pageSize = 10, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("分页获取用户分享令牌: {UserId}, 页码: {Page}, 大小: {PageSize}", 
                userId, page, pageSize);

            // 检查权限
            if (currentUserId.HasValue && userId != currentUserId.Value)
            {
                var canView = await _permissionService.IsAdminAsync(currentUserId.Value);
                if (!canView)
                {
                    throw new UnauthorizedAccessException("无权查看其他用户的分享令牌");
                }
            }

            var filter = new ShareTokenFilter
            {
                CreatedBy = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _shareTokenRepository.GetPagedAsync(filter);
            return new PaginatedResult<ShareTokenDto>
            {
                Items = result.Items.Select(MapToDto),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分页获取用户分享令牌失败: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// 更新分享令牌设置
    /// </summary>
    public async Task<ShareTokenDto> UpdateShareTokenAsync(Guid id, UpdateShareDto updateShareDto, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 更新分享令牌: {ShareTokenId}", currentUserId, id);

            var shareToken = await _shareTokenRepository.GetByIdAsync(id);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            // 检查权限
            if (shareToken.CreatedBy != currentUserId)
            {
                var canEdit = await _permissionService.IsAdminAsync(currentUserId);
                if (!canEdit)
                {
                    throw new UnauthorizedAccessException("无权修改此分享令牌");
                }
            }

            // 验证输入
            await ValidateUpdateShareRequestAsync(updateShareDto);

            // 更新字段
            bool hasChanges = false;
            if (updateShareDto.Description != null && shareToken.Description != updateShareDto.Description)
            {
                shareToken.Description = updateShareDto.Description;
                hasChanges = true;
            }

            if (updateShareDto.Permission.HasValue && shareToken.Permission != updateShareDto.Permission.Value)
            {
                shareToken.Permission = updateShareDto.Permission.Value;
                hasChanges = true;
            }

            if (updateShareDto.AllowDownload.HasValue && shareToken.AllowDownload != updateShareDto.AllowDownload.Value)
            {
                shareToken.AllowDownload = updateShareDto.AllowDownload.Value;
                hasChanges = true;
            }

            if (updateShareDto.AllowCopy.HasValue && shareToken.AllowCopy != updateShareDto.AllowCopy.Value)
            {
                shareToken.AllowCopy = updateShareDto.AllowCopy.Value;
                hasChanges = true;
            }

            if (updateShareDto.MaxAccessCount.HasValue && shareToken.MaxAccessCount != updateShareDto.MaxAccessCount.Value)
            {
                shareToken.MaxAccessCount = updateShareDto.MaxAccessCount.Value;
                hasChanges = true;
            }

            // 延长有效期
            if (updateShareDto.ExtendHours > 0)
            {
                shareToken.ExpiresAt = shareToken.ExpiresAt.AddHours(updateShareDto.ExtendHours);
                hasChanges = true;
            }

            if (hasChanges)
            {
                shareToken.UpdatedAt = DateTime.UtcNow;
                var updated = await _shareTokenRepository.UpdateAsync(shareToken);

                // 清除缓存
                await ClearShareTokenCacheAsync(shareToken.Token);
                await ClearUserShareTokensCacheAsync(currentUserId);
                await ClearShareStatsCacheAsync(id);

                _logger.LogInformation("用户 {UserId} 成功更新分享令牌: {ShareTokenId}", currentUserId, id);
                return MapToDto(updated);
            }

            return MapToDto(shareToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新分享令牌失败: {ShareTokenId}, 用户: {UserId}", id, currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 撤销分享令牌（禁用）
    /// </summary>
    public async Task<bool> RevokeShareTokenAsync(Guid id, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 撤销分享令牌: {ShareTokenId}", currentUserId, id);

            var shareToken = await _shareTokenRepository.GetByIdAsync(id);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            // 检查权限
            if (shareToken.CreatedBy != currentUserId)
            {
                var canRevoke = await _permissionService.IsAdminAsync(currentUserId);
                if (!canRevoke)
                {
                    throw new UnauthorizedAccessException("无权撤销此分享令牌");
                }
            }

            if (!shareToken.IsActive)
            {
                return true; // 已经被撤销
            }

            var success = await _shareTokenRepository.DeactivateTokenAsync(id);
            if (success)
            {
                // 清除缓存
                await ClearShareTokenCacheAsync(shareToken.Token);
                await ClearUserShareTokensCacheAsync(currentUserId);
                await ClearShareStatsCacheAsync(id);

                _logger.LogInformation("用户 {UserId} 成功撤销分享令牌: {ShareTokenId}", currentUserId, id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤销分享令牌失败: {ShareTokenId}, 用户: {UserId}", id, currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 删除分享令牌
    /// </summary>
    public async Task<bool> DeleteShareTokenAsync(Guid id, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 删除分享令牌: {ShareTokenId}", currentUserId, id);

            var shareToken = await _shareTokenRepository.GetByIdAsync(id);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            // 检查权限
            if (shareToken.CreatedBy != currentUserId)
            {
                var canDelete = await _permissionService.IsAdminAsync(currentUserId);
                if (!canDelete)
                {
                    throw new UnauthorizedAccessException("无权删除此分享令牌");
                }
            }

            // 删除访问日志
            await _shareAccessLogRepository.DeleteByShareTokenIdAsync(id);

            // 删除分享令牌
            var success = await _shareTokenRepository.DeleteAsync(id);
            if (success)
            {
                // 清除缓存
                await ClearShareTokenCacheAsync(shareToken.Token);
                await ClearUserShareTokensCacheAsync(currentUserId);
                await ClearShareStatsCacheAsync(id);
                await ClearSnippetSharesCacheAsync(shareToken.CodeSnippetId);

                _logger.LogInformation("用户 {UserId} 成功删除分享令牌: {ShareTokenId}", currentUserId, id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除分享令牌失败: {ShareTokenId}, 用户: {UserId}", id, currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 验证分享令牌是否有效
    /// </summary>
    public async Task<(bool IsValid, string? Message, ShareTokenDto? ShareToken)> ValidateShareTokenAsync(string token, string? password = null)
    {
        try
        {
            _logger.LogDebug("验证分享令牌: {Token}", token);

            var shareToken = await _shareTokenRepository.GetByTokenAsync(token);
            if (shareToken == null)
            {
                return (false, "分享令牌不存在", null);
            }

            // 验证令牌状态
            if (!await ValidateShareTokenStatusAsync(shareToken))
            {
                return (false, "分享令牌已失效", null);
            }

            // 验证密码
            if (shareToken.Password != null)
            {
                if (string.IsNullOrEmpty(password))
                {
                    return (false, "需要访问密码", null);
                }

                if (!await VerifySharePasswordAsync(shareToken.Id, password))
                {
                    return (false, "密码错误", null);
                }
            }

            return (true, null, MapToDto(shareToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证分享令牌失败: {Token}", token);
            return (false, "验证失败", null);
        }
    }

    /// <summary>
    /// 记录分享访问日志
    /// </summary>
    public async Task<Guid> LogShareAccessAsync(
        Guid shareTokenId,
        string ipAddress,
        string? userAgent = null,
        bool isSuccess = true,
        string? failureReason = null)
    {
        try
        {
            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            var accessLog = new ShareAccessLog
            {
                Id = Guid.NewGuid(),
                ShareTokenId = shareTokenId,
                CodeSnippetId = shareToken.CodeSnippetId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                AccessedAt = DateTime.UtcNow,
                IsSuccess = isSuccess,
                FailureReason = failureReason,
                Duration = 0,
                CodeSnippetTitle = shareToken.CodeSnippetTitle,
                CodeSnippetLanguage = shareToken.CodeSnippetLanguage,
                CreatorName = shareToken.CreatorName
            };

            await _shareAccessLogRepository.CreateAsync(accessLog);

            // 如果访问成功，更新访问计数
            if (isSuccess)
            {
                await _shareTokenRepository.IncrementAccessCountAsync(shareTokenId);
                await ClearShareStatsCacheAsync(shareTokenId);
            }

            return accessLog.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录分享访问日志失败: {ShareTokenId}", shareTokenId);
            throw;
        }
    }

    /// <summary>
    /// 批量记录分享访问日志
    /// </summary>
    public async Task<int> BulkLogShareAccessAsync(List<CreateAccessLogDto> accessLogs)
    {
        try
        {
            if (accessLogs == null || !accessLogs.Any())
            {
                return 0;
            }

            var logs = accessLogs.Select(dto => new ShareAccessLog
            {
                Id = Guid.NewGuid(),
                ShareTokenId = dto.ShareTokenId,
                CodeSnippetId = dto.CodeSnippetId,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
                Source = dto.Source,
                Country = dto.Country,
                City = dto.City,
                Browser = dto.Browser,
                OperatingSystem = dto.OperatingSystem,
                DeviceType = dto.DeviceType,
                AccessedAt = DateTime.UtcNow,
                IsSuccess = dto.IsSuccess,
                FailureReason = dto.FailureReason,
                Duration = dto.Duration,
                SessionId = dto.SessionId,
                Referer = dto.Referer,
                AcceptLanguage = dto.AcceptLanguage
            }).ToList();

            var successCount = await _shareAccessLogRepository.BulkInsertAsync(logs);

            // 批量更新访问计数
            var shareTokenIds = logs.Where(l => l.IsSuccess).Select(l => l.ShareTokenId).Distinct();
            foreach (var shareTokenId in shareTokenIds)
            {
                var accessCount = logs.Count(l => l.ShareTokenId == shareTokenId && l.IsSuccess);
                for (int i = 0; i < accessCount; i++)
                {
                    await _shareTokenRepository.IncrementAccessCountAsync(shareTokenId);
                }
                await ClearShareStatsCacheAsync(shareTokenId);
            }

            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量记录分享访问日志失败，数量: {Count}", accessLogs?.Count ?? 0);
            throw;
        }
    }

    /// <summary>
    /// 获取分享统计信息
    /// </summary>
    public async Task<ShareStatsDto> GetShareStatsAsync(Guid shareTokenId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取分享统计信息: {ShareTokenId}", shareTokenId);

            // 从缓存获取
            var cacheKey = $"{SHARE_STATS_CACHE_PREFIX}{shareTokenId}";
            var cached = await _cacheService.GetAsync<ShareStatsDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // 检查权限
            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            if (currentUserId.HasValue && shareToken.CreatedBy != currentUserId.Value)
            {
                var canAccess = await _permissionService.IsAdminAsync(currentUserId.Value);
                if (!canAccess)
                {
                    throw new UnauthorizedAccessException("无权查看此分享统计");
                }
            }

            // 获取统计信息
            var stats = await _shareTokenRepository.GetShareStatsAsync(shareTokenId);

            // 获取访问日志统计
            var accessStats = await _shareAccessLogRepository.GetAccessStatsAsync(shareTokenId);
            stats.TodayAccessCount = accessStats.TodayAccessCount;
            stats.ThisWeekAccessCount = accessStats.ThisWeekAccessCount;
            stats.ThisMonthAccessCount = accessStats.ThisMonthAccessCount;

            // 获取每日统计
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            var dailyStats = await _shareAccessLogRepository.GetDailyAccessStatsAsync(shareTokenId, 30);
            stats.DailyStats = dailyStats.Select(stat => new DailyAccessStatDto
            {
                Date = stat.Date,
                AccessCount = stat.AccessCount,
                UniqueVisitors = stat.UniqueVisitors
            }).ToList();

            // 获取来源统计
            var sourceStats = await _shareAccessLogRepository.GetAccessSourceStatsAsync(shareTokenId);
            stats.SourceStats = sourceStats.Select(stat => new AccessSourceStatDto
            {
                SourceType = stat.Source,
                AccessCount = stat.AccessCount,
                Percentage = stat.Percentage
            }).ToList();

            // 缓存结果
            await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(15));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享统计信息失败: {ShareTokenId}", shareTokenId);
            throw;
        }
    }

    /// <summary>
    /// 获取分享访问日志
    /// </summary>
    public async Task<PaginatedResult<ShareAccessLogDto>> GetShareAccessLogsAsync(AccessLogFilter filter, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取分享访问日志，分享令牌ID: {ShareTokenId}", filter.ShareTokenId);

            if (filter.ShareTokenId.HasValue)
            {
                // 检查权限
                var shareToken = await _shareTokenRepository.GetByIdAsync(filter.ShareTokenId.Value);
                if (shareToken == null)
                {
                    throw new ArgumentException("分享令牌不存在");
                }

                if (currentUserId.HasValue && shareToken.CreatedBy != currentUserId.Value)
                {
                    var canAccess = await _permissionService.IsAdminAsync(currentUserId.Value);
                    if (!canAccess)
                    {
                        throw new UnauthorizedAccessException("无权查看此分享访问日志");
                    }
                }
            }
            else if (currentUserId.HasValue)
            {
                // 如果没有指定分享令牌ID，只能查看自己的分享访问日志
                filter.CodeSnippetId = null; // 清除代码片段筛选
            }

            var result = await _shareAccessLogRepository.GetPagedAsync(filter);
            return new PaginatedResult<ShareAccessLogDto>
            {
                Items = result.Items.Select(MapToAccessLogDto),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享访问日志失败");
            throw;
        }
    }

    /// <summary>
    /// 获取代码片段的所有分享记录
    /// </summary>
    public async Task<IEnumerable<ShareTokenDto>> GetSnippetSharesAsync(Guid codeSnippetId, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogDebug("获取代码片段分享记录: {CodeSnippetId}", codeSnippetId);

            // 检查权限
            if (currentUserId.HasValue)
            {
                var canAccess = await _permissionService.CanAccessSnippetAsync(
                    currentUserId.Value, codeSnippetId, PermissionOperation.Read);
                if (!canAccess)
                {
                    throw new UnauthorizedAccessException("无权查看此代码片段的分享记录");
                }
            }

            var shareTokens = await _shareTokenRepository.GetByCodeSnippetIdAsync(codeSnippetId);
            return shareTokens.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码片段分享记录失败: {CodeSnippetId}", codeSnippetId);
            throw;
        }
    }

    /// <summary>
    /// 检查用户是否有权限访问分享
    /// </summary>
    public async Task<bool> HasShareAccessPermissionAsync(Guid shareTokenId, Guid? currentUserId = null)
    {
        try
        {
            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                return false;
            }

            // 管理员有所有权限
            if (currentUserId.HasValue && await _permissionService.IsAdminAsync(currentUserId.Value))
            {
                return true;
            }

            // 分享创建者有所有权限
            if (currentUserId.HasValue && shareToken.CreatedBy == currentUserId.Value)
            {
                return true;
            }

            // 检查分享是否有效
            return await ValidateShareTokenStatusAsync(shareToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查分享访问权限失败: {ShareTokenId}", shareTokenId);
            return false;
        }
    }

    /// <summary>
    /// 增加分享访问次数
    /// </summary>
    public async Task<bool> IncrementShareAccessCountAsync(Guid shareTokenId)
    {
        try
        {
            var success = await _shareTokenRepository.IncrementAccessCountAsync(shareTokenId);
            if (success)
            {
                await ClearShareStatsCacheAsync(shareTokenId);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "增加分享访问次数失败: {ShareTokenId}", shareTokenId);
            return false;
        }
    }

    /// <summary>
    /// 清理过期的分享令牌
    /// </summary>
    public async Task<int> CleanupExpiredShareTokensAsync()
    {
        try
        {
            _logger.LogInformation("开始清理过期的分享令牌");

            var expiredTokens = await _shareTokenRepository.GetExpiredTokensAsync();
            var cleanupCount = 0;

            foreach (var token in expiredTokens)
            {
                try
                {
                    // 删除访问日志
                    await _shareAccessLogRepository.DeleteByShareTokenIdAsync(token.Id);
                    
                    // 删除分享令牌
                    await _shareTokenRepository.DeleteAsync(token.Id);
                    
                    // 清除缓存
                    await ClearShareTokenCacheAsync(token.Token);
                    await ClearUserShareTokensCacheAsync(token.CreatedBy);
                    await ClearShareStatsCacheAsync(token.Id);
                    await ClearSnippetSharesCacheAsync(token.CodeSnippetId);

                    cleanupCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理过期分享令牌失败: {ShareTokenId}", token.Id);
                }
            }

            _logger.LogInformation("完成清理过期的分享令牌，数量: {Count}", cleanupCount);
            return cleanupCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期分享令牌失败");
            throw;
        }
    }

    /// <summary>
    /// 检查分享令牌是否过期
    /// </summary>
    public async Task<bool> IsShareTokenExpiredAsync(Guid shareTokenId)
    {
        try
        {
            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                return true;
            }

            return shareToken.ExpiresAt < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查分享令牌过期状态失败: {ShareTokenId}", shareTokenId);
            return true;
        }
    }

    /// <summary>
    /// 延长分享令牌有效期
    /// </summary>
    public async Task<ShareTokenDto> ExtendShareTokenExpiryAsync(Guid shareTokenId, int extendHours, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 延长分享令牌有效期: {ShareTokenId}, 延长小时数: {Hours}", 
                currentUserId, shareTokenId, extendHours);

            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            // 检查权限
            if (shareToken.CreatedBy != currentUserId)
            {
                var canExtend = await _permissionService.IsAdminAsync(currentUserId);
                if (!canExtend)
                {
                    throw new UnauthorizedAccessException("无权延长此分享令牌的有效期");
                }
            }

            if (extendHours <= 0)
            {
                throw new ArgumentException("延长时间必须大于0");
            }

            // 延长有效期
            var success = await _shareTokenRepository.ExtendTokenExpirationAsync(shareTokenId, TimeSpan.FromHours(extendHours));
            if (!success)
            {
                throw new InvalidOperationException("延长有效期失败");
            }

            // 重新获取更新后的分享令牌
            var updated = await _shareTokenRepository.GetByIdAsync(shareTokenId);

            // 清除缓存
            await ClearShareTokenCacheAsync(updated.Token);
            await ClearUserShareTokensCacheAsync(currentUserId);
            await ClearShareStatsCacheAsync(shareTokenId);

            _logger.LogInformation("用户 {UserId} 成功延长分享令牌有效期: {ShareTokenId}", currentUserId, shareTokenId);
            return MapToDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "延长分享令牌有效期失败: {ShareTokenId}, 用户: {UserId}", shareTokenId, currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 重置分享令牌访问统计
    /// </summary>
    public async Task<bool> ResetShareAccessStatsAsync(Guid shareTokenId, Guid currentUserId)
    {
        try
        {
            _logger.LogInformation("用户 {UserId} 重置分享访问统计: {ShareTokenId}", currentUserId, shareTokenId);

            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null)
            {
                throw new ArgumentException("分享令牌不存在");
            }

            // 检查权限
            if (shareToken.CreatedBy != currentUserId)
            {
                var canReset = await _permissionService.IsAdminAsync(currentUserId);
                if (!canReset)
                {
                    throw new UnauthorizedAccessException("无权重置此分享访问统计");
                }
            }

            // 重置访问计数
            shareToken.AccessCount = 0;
            shareToken.LastAccessedAt = null;
            shareToken.UpdatedAt = DateTime.UtcNow;

            await _shareTokenRepository.UpdateAsync(shareToken);

            // 删除访问日志
            await _shareAccessLogRepository.DeleteByShareTokenIdAsync(shareTokenId);

            // 清除缓存
            await ClearShareTokenCacheAsync(shareToken.Token);
            await ClearUserShareTokensCacheAsync(currentUserId);
            await ClearShareStatsCacheAsync(shareTokenId);

            _logger.LogInformation("用户 {UserId} 成功重置分享访问统计: {ShareTokenId}", currentUserId, shareTokenId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置分享访问统计失败: {ShareTokenId}, 用户: {UserId}", shareTokenId, currentUserId);
            throw;
        }
    }

    /// <summary>
    /// 获取分享链接的完整URL
    /// </summary>
    public async Task<string> GetShareUrlAsync(string token)
    {
        try
        {
            var cacheKey = $"{SHARE_URL_CACHE_PREFIX}{token}";
            var cached = await _cacheService.GetAsync<string>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // 这里应该从配置中获取基础URL
            var baseUrl = "https://codeshare.example.com/share/"; // 应该从配置中获取
            var shareUrl = $"{baseUrl}{token}";

            await _cacheService.SetAsync(cacheKey, shareUrl, TimeSpan.FromHours(1));
            return shareUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分享链接失败: {Token}", token);
            throw;
        }
    }

    /// <summary>
    /// 生成新的分享令牌字符串
    /// </summary>
    public async Task<string> GenerateShareTokenAsync()
    {
        try
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[16];
            rng.GetBytes(tokenBytes);
            
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, 12);

            // 确保令牌唯一
            var existing = await _shareTokenRepository.GetByTokenAsync(token);
            if (existing != null)
            {
                // 如果冲突，递归生成新的令牌
                return await GenerateShareTokenAsync();
            }

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成分享令牌失败");
            throw;
        }
    }

    /// <summary>
    /// 验证分享密码
    /// </summary>
    public async Task<bool> VerifySharePasswordAsync(Guid shareTokenId, string password)
    {
        try
        {
            var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
            if (shareToken == null || string.IsNullOrEmpty(shareToken.Password))
            {
                return false;
            }

            return VerifyPassword(password, shareToken.Password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证分享密码失败: {ShareTokenId}", shareTokenId);
            return false;
        }
    }

    #region 私有辅助方法

    /// <summary>
    /// 验证创建分享请求
    /// </summary>
    private async Task ValidateCreateShareRequestAsync(CreateShareDto createShareDto)
    {
        var expiryValidation = _validationService.ValidateShareExpiryHours(createShareDto.ExpiresInHours);
        if (!expiryValidation.IsValid)
        {
            throw new ArgumentException(expiryValidation.ErrorMessage);
        }

        var accessCountValidation = _validationService.ValidateShareMaxAccessCount(createShareDto.MaxAccessCount);
        if (!accessCountValidation.IsValid)
        {
            throw new ArgumentException(accessCountValidation.ErrorMessage);
        }

    }

    /// <summary>
    /// 验证更新分享请求
    /// </summary>
    private async Task ValidateUpdateShareRequestAsync(UpdateShareDto updateShareDto)
    {
        if (updateShareDto.Description != null)
        {
            var descriptionValidation = _validationService.ValidateShareDescription(updateShareDto.Description);
            if (!descriptionValidation.IsValid)
            {
                throw new ArgumentException(descriptionValidation.ErrorMessage);
            }
        }

        if (updateShareDto.MaxAccessCount.HasValue)
        {
            var accessCountValidation = _validationService.ValidateShareMaxAccessCount(updateShareDto.MaxAccessCount.Value);
            if (!accessCountValidation.IsValid)
            {
                throw new ArgumentException(accessCountValidation.ErrorMessage);
            }
        }

        if (updateShareDto.ExtendHours > 0)
        {
            var extendValidation = _validationService.ValidateShareExpiryHours(updateShareDto.ExtendHours);
            if (!extendValidation.IsValid)
            {
                throw new ArgumentException(extendValidation.ErrorMessage);
            }
        }
    }

    /// <summary>
    /// 验证分享令牌状态
    /// </summary>
    private async Task<bool> ValidateShareTokenStatusAsync(ShareToken shareToken)
    {
        // 检查是否激活
        if (!shareToken.IsActive)
        {
            _logger.LogWarning("分享令牌未激活: {Token}", shareToken.Token);
            return false;
        }

        // 检查是否过期
        if (shareToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("分享令牌已过期: {Token}", shareToken.Token);
            return false;
        }

        // 检查是否达到访问次数限制
        if (shareToken.MaxAccessCount > 0 && shareToken.AccessCount >= shareToken.MaxAccessCount)
        {
            _logger.LogWarning("分享令牌已达访问次数限制: {Token}", shareToken.Token);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 哈希密码
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    private bool VerifyPassword(string password, string hashedPassword)
    {
        var computedHash = HashPassword(password);
        return computedHash == hashedPassword;
    }

    /// <summary>
    /// 映射分享令牌为DTO
    /// </summary>
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
            CreatorName = shareToken.CreatorName,
            ExpiresAt = shareToken.ExpiresAt,
            CreatedAt = shareToken.CreatedAt,
            UpdatedAt = shareToken.UpdatedAt,
            IsActive = shareToken.IsActive,
            AccessCount = shareToken.AccessCount,
            MaxAccessCount = shareToken.MaxAccessCount,
            Permission = shareToken.Permission,
            Description = shareToken.Description,
            HasPassword = !string.IsNullOrEmpty(shareToken.Password),
            AllowDownload = shareToken.AllowDownload,
            AllowCopy = shareToken.AllowCopy,
            LastAccessedAt = shareToken.LastAccessedAt,
            CodeSnippetTitle = shareToken.CodeSnippetTitle,
            CodeSnippetLanguage = shareToken.CodeSnippetLanguage,
            CodeSnippetCode = string.Empty, // 将在AccessShareAsync中设置
            CurrentAccessCount = shareToken.AccessCount,
            IsExpired = isExpired,
            IsAccessLimitReached = isAccessLimitReached,
            RemainingAccessCount = remainingAccessCount
        };
    }

    /// <summary>
    /// 映射访问日志为DTO
    /// </summary>
    private ShareAccessLogDto MapToAccessLogDto(ShareAccessLog accessLog)
    {
        return new ShareAccessLogDto
        {
            Id = accessLog.Id,
            ShareTokenId = accessLog.ShareTokenId,
            CodeSnippetId = accessLog.CodeSnippetId,
            IpAddress = accessLog.IpAddress,
            UserAgent = accessLog.UserAgent,
            Source = accessLog.Source,
            Country = accessLog.Country,
            City = accessLog.City,
            Browser = accessLog.Browser,
            OperatingSystem = accessLog.OperatingSystem,
            DeviceType = accessLog.DeviceType,
            AccessedAt = accessLog.AccessedAt,
            IsSuccess = accessLog.IsSuccess,
            FailureReason = accessLog.FailureReason,
            Duration = accessLog.Duration,
            SessionId = accessLog.SessionId,
            Referer = accessLog.Referer,
            AcceptLanguage = accessLog.AcceptLanguage,
            CodeSnippetTitle = accessLog.CodeSnippetTitle,
            CodeSnippetLanguage = accessLog.CodeSnippetLanguage,
            CreatorName = accessLog.CreatorName,
            Location = !string.IsNullOrEmpty(accessLog.Country) && !string.IsNullOrEmpty(accessLog.City)
                ? $"{accessLog.City}, {accessLog.Country}"
                : accessLog.Country ?? accessLog.City,
            IsFirstAccess = false, // 需要通过查询确定
            AccessNumber = 0 // 需要通过查询确定
        };
    }

    /// <summary>
    /// 清除分享令牌缓存
    /// </summary>
    private async Task ClearShareTokenCacheAsync(string token)
    {
        var cacheKey = $"{SHARE_TOKEN_CACHE_PREFIX}{token}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// 清除用户分享令牌缓存
    /// </summary>
    private async Task ClearUserShareTokensCacheAsync(Guid userId)
    {
        await _cacheService.RemoveByPatternAsync($"user_shares_{userId}_*");
    }

    /// <summary>
    /// 清除分享统计缓存
    /// </summary>
    private async Task ClearShareStatsCacheAsync(Guid shareTokenId)
    {
        var cacheKey = $"{SHARE_STATS_CACHE_PREFIX}{shareTokenId}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// 清除代码片段分享缓存
    /// </summary>
    private async Task ClearSnippetSharesCacheAsync(Guid codeSnippetId)
    {
        await _cacheService.RemoveByPatternAsync($"snippet_shares_{codeSnippetId}_*");
    }

    #endregion

    #region 管理员功能

    /// <summary>
    /// 获取所有分享链接（管理员功能）
    /// </summary>
    /// <param name="filter">分享过滤器</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页的分享令牌结果</returns>
    public async Task<PaginatedResult<ShareTokenDto>> GetAllSharesAdminAsync(AdminShareFilter filter, Guid currentUserId)
    {
        // 验证管理员权限
        if (!await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("无管理员权限");
        }

        return await _shareTokenRepository.GetAllSharesAdminAsync(filter);
    }

    /// <summary>
    /// 获取系统分享统计信息（管理员功能）
    /// </summary>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>系统分享统计信息</returns>
    public async Task<SystemShareStatsDto> GetSystemShareStatsAsync(Guid currentUserId)
    {
        // 验证管理员权限
        if (!await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("无管理员权限");
        }

        // 尝试从缓存获取
        var cacheKey = "system_share_stats";
        var cachedStats = await _cacheService.GetAsync<SystemShareStatsDto>(cacheKey);
        if (cachedStats != null)
        {
            return cachedStats;
        }

        // 从数据库获取统计信息
        var stats = await _shareTokenRepository.GetSystemShareStatsAsync();

        // 缓存结果（缓存30分钟）
        await _cacheService.SetAsync(cacheKey, stats, TimeSpan.FromMinutes(30));

        return stats;
    }

    /// <summary>
    /// 批量操作分享链接（管理员功能）
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>批量操作结果</returns>
    public async Task<BulkOperationResultDto> BulkOperationSharesAsync(BulkShareOperationRequest request, Guid currentUserId)
    {
        // 验证管理员权限
        if (!await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("无管理员权限");
        }

        var result = new BulkOperationResultDto
        {
            TotalCount = request.ShareTokenIds.Count,
            SuccessCount = 0,
            FailureCount = 0,
            FailedShareTokenIds = new List<Guid>(),
            FailureReasons = new List<string>()
        };

        foreach (var shareTokenId in request.ShareTokenIds)
        {
            try
            {
                bool success = request.Operation switch
                {
                    "revoke" => await ForceRevokeShareTokenAsync(shareTokenId, currentUserId),
                    "delete" => await ForceDeleteShareTokenAsync(shareTokenId, currentUserId),
                    "extend" => request.OperationParam.HasValue && request.OperationParam.Value > 0 
                        ? await ExtendShareTokenExpiryAsync(shareTokenId, request.OperationParam.Value, currentUserId) != null
                        : false,
                    "activate" => await ActivateShareTokenAsync(shareTokenId, currentUserId),
                    _ => false
                };

                if (success)
                {
                    result.SuccessCount++;
                }
                else
                {
                    result.FailureCount++;
                    result.FailedShareTokenIds.Add(shareTokenId);
                    result.FailureReasons.Add($"操作失败：{request.Operation}");
                }
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.FailedShareTokenIds.Add(shareTokenId);
                result.FailureReasons.Add(ex.Message);
            }
        }

        // 清除相关缓存
        await ClearSystemShareStatsCacheAsync();

        return result;
    }

    /// <summary>
    /// 强制撤销分享令牌（管理员功能）
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功撤销</returns>
    public async Task<bool> ForceRevokeShareTokenAsync(Guid shareTokenId, Guid currentUserId)
    {
        // 验证管理员权限
        if (!await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("无管理员权限");
        }

        var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
        if (shareToken == null)
        {
            return false;
        }

        shareToken.IsActive = false;
        shareToken.UpdatedAt = DateTime.UtcNow;

        await _shareTokenRepository.UpdateAsync(shareToken);

        // 记录管理员操作日志
        _logger.LogInformation("管理员 {UserId} 强制撤销分享链接 {ShareTokenId}", currentUserId, shareTokenId);

        // 清除缓存
        await ClearShareStatsCacheAsync(shareTokenId);
        await ClearSystemShareStatsCacheAsync();

        return true;
    }

    /// <summary>
    /// 强制删除分享令牌（管理员功能）
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功删除</returns>
    public async Task<bool> ForceDeleteShareTokenAsync(Guid shareTokenId, Guid currentUserId)
    {
        // 验证管理员权限
        if (!await _permissionService.IsAdminAsync(currentUserId))
        {
            throw new UnauthorizedAccessException("无管理员权限");
        }

        var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
        if (shareToken == null)
        {
            return false;
        }

        // 删除相关的访问日志
        await _shareAccessLogRepository.DeleteByShareTokenIdAsync(shareTokenId);

        // 删除分享令牌
        await _shareTokenRepository.DeleteAsync(shareTokenId);

        // 记录管理员操作日志
        _logger.LogInformation("管理员 {UserId} 强制删除分享链接 {ShareTokenId}", currentUserId, shareTokenId);

        // 清除缓存
        await ClearShareStatsCacheAsync(shareTokenId);
        await ClearSnippetSharesCacheAsync(shareToken.CodeSnippetId);
        await ClearSystemShareStatsCacheAsync();

        return true;
    }

    /// <summary>
    /// 激活分享令牌（管理员功能）
    /// </summary>
    /// <param name="shareTokenId">分享令牌ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否成功激活</returns>
    private async Task<bool> ActivateShareTokenAsync(Guid shareTokenId, Guid currentUserId)
    {
        var shareToken = await _shareTokenRepository.GetByIdAsync(shareTokenId);
        if (shareToken == null)
        {
            return false;
        }

        // 检查是否已过期
        if (shareToken.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        shareToken.IsActive = true;
        shareToken.UpdatedAt = DateTime.UtcNow;

        await _shareTokenRepository.UpdateAsync(shareToken);

        // 记录管理员操作日志
        _logger.LogInformation("管理员 {UserId} 激活分享链接 {ShareTokenId}", currentUserId, shareTokenId);

        // 清除缓存
        await ClearShareStatsCacheAsync(shareTokenId);
        await ClearSystemShareStatsCacheAsync();

        return true;
    }

    /// <summary>
    /// 清除系统分享统计缓存
    /// </summary>
    private async Task ClearSystemShareStatsCacheAsync()
    {
        await _cacheService.RemoveAsync("system_share_stats");
    }

    #endregion
}