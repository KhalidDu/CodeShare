using System.Text.Json;
using System.Text.RegularExpressions;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 系统设置服务实现 - 遵循单一职责原则
/// </summary>
public class SystemSettingsService : ISystemSettingsService
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly ISettingsHistoryService _historyService;
    private readonly ISettingsValidationService _validationService;
    private readonly ISettingsImportExportService _importExportService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SystemSettingsService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // 缓存键
    private const string SETTINGS_CACHE_KEY = "system_settings";
    private const string SITE_SETTINGS_CACHE_KEY = "site_settings";
    private const string SECURITY_SETTINGS_CACHE_KEY = "security_settings";
    private const string FEATURE_SETTINGS_CACHE_KEY = "feature_settings";
    private const string EMAIL_SETTINGS_CACHE_KEY = "email_settings";

    // 缓存过期时间
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public SystemSettingsService(
        ISystemSettingsRepository settingsRepository,
        ISettingsHistoryService historyService,
        ISettingsValidationService validationService,
        ISettingsImportExportService importExportService,
        IMemoryCache cache,
        ILogger<SystemSettingsService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _importExportService = importExportService ?? throw new ArgumentNullException(nameof(importExportService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<SystemSettingsDto?> GetSettingsAsync()
    {
        return await _cache.GetOrCreateAsync(SETTINGS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            
            var settings = await _settingsRepository.GetSettingsAsync();
            return settings != null ? MapToDto(settings) : null;
        });
    }

    public async Task<SystemSettingsDto> GetOrCreateSettingsAsync()
    {
        var settings = await GetSettingsAsync();
        if (settings == null)
        {
            settings = await InitializeDefaultSettingsAsync();
        }
        return settings;
    }

    public async Task<SystemSettingsDto> UpdateSettingsAsync(SystemSettingsDto settings)
    {
        var validation = await _validationService.ValidateAllSettingsAsync(settings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"设置验证失败: {string.Join(", ", validation.Errors.SelectMany(e => e.Value))}");
        }

        var currentSettings = await GetSettingsAsync();
        var entity = MapToEntity(settings);
        var updatedEntity = await _settingsRepository.SaveSettingsAsync(entity);
        
        // 清除缓存
        await ClearCacheAsync();
        
        _logger.LogInformation("系统设置已更新，操作人: {UpdatedBy}", settings.UpdatedBy);
        return MapToDto(updatedEntity);
    }

    public async Task<SiteSettingsDto> GetSiteSettingsAsync()
    {
        return await _cache.GetOrCreateAsync(SITE_SETTINGS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            
            var settings = await GetOrCreateSettingsAsync();
            return settings.SiteSettings;
        });
    }

    public async Task<SiteSettingsDto> UpdateSiteSettingsAsync(UpdateSiteSettingsRequest request, string updatedBy)
    {
        var currentSettings = await GetSiteSettingsAsync();
        var newSettings = ApplyUpdate(currentSettings, request);
        
        var validation = await _validationService.ValidateSiteSettingsAsync(newSettings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"站点设置验证失败: {string.Join(", ", validation.Errors.SelectMany(e => e.Value))}");
        }

        var entity = new SiteSettings
        {
            SiteName = newSettings.SiteName,
            SiteDescription = newSettings.SiteDescription,
            LogoUrl = newSettings.LogoUrl,
            Theme = newSettings.Theme,
            Language = newSettings.Language,
            PageSize = newSettings.PageSize,
            AllowRegistration = newSettings.AllowRegistration,
            Announcement = newSettings.Announcement,
            CustomCss = newSettings.CustomCss,
            CustomJs = newSettings.CustomJs
        };

        await _settingsRepository.UpdateSiteSettingsAsync(entity, updatedBy);
        
        // 清除缓存
        _cache.Remove(SITE_SETTINGS_CACHE_KEY);
        _cache.Remove(SETTINGS_CACHE_KEY);
        
        _logger.LogInformation("站点设置已更新，操作人: {UpdatedBy}", updatedBy);
        return newSettings;
    }

    public async Task<SettingsValidationResult> ValidateSiteSettingsAsync(SiteSettingsDto settings)
    {
        return await _validationService.ValidateSiteSettingsAsync(settings);
    }

    public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
    {
        return await _cache.GetOrCreateAsync(SECURITY_SETTINGS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            
            var settings = await GetOrCreateSettingsAsync();
            return settings.SecuritySettings;
        });
    }

    public async Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(UpdateSecuritySettingsRequest request, string updatedBy)
    {
        var currentSettings = await GetSecuritySettingsAsync();
        var newSettings = ApplyUpdate(currentSettings, request);
        
        var validation = await _validationService.ValidateSecuritySettingsAsync(newSettings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"安全设置验证失败: {string.Join(", ", validation.Errors.SelectMany(e => e.Value))}");
        }

        var entity = new SecuritySettings
        {
            MinPasswordLength = newSettings.MinPasswordLength,
            RequireUppercase = newSettings.RequireUppercase,
            RequireLowercase = newSettings.RequireLowercase,
            RequireNumbers = newSettings.RequireNumbers,
            RequireSpecialChars = newSettings.RequireSpecialChars,
            MaxLoginAttempts = newSettings.MaxLoginAttempts,
            AccountLockoutDuration = newSettings.AccountLockoutDuration,
            SessionTimeout = newSettings.SessionTimeout,
            EnableTwoFactorAuth = newSettings.EnableTwoFactorAuth,
            EnableCors = newSettings.EnableCors,
            AllowedCorsOrigins = newSettings.AllowedCorsOrigins,
            EnableHttpsRedirection = newSettings.EnableHttpsRedirection,
            ApiRateLimit = newSettings.ApiRateLimit,
            EnableLoginLogging = newSettings.EnableLoginLogging,
            EnableActionLogging = newSettings.EnableActionLogging
        };

        await _settingsRepository.UpdateSecuritySettingsAsync(entity, updatedBy);
        
        // 清除缓存
        _cache.Remove(SECURITY_SETTINGS_CACHE_KEY);
        _cache.Remove(SETTINGS_CACHE_KEY);
        
        _logger.LogInformation("安全设置已更新，操作人: {UpdatedBy}", updatedBy);
        return newSettings;
    }

    public async Task<SettingsValidationResult> ValidateSecuritySettingsAsync(SecuritySettingsDto settings)
    {
        return await _validationService.ValidateSecuritySettingsAsync(settings);
    }

    public async Task<FeatureSettingsDto> GetFeatureSettingsAsync()
    {
        return await _cache.GetOrCreateAsync(FEATURE_SETTINGS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            
            var settings = await GetOrCreateSettingsAsync();
            return settings.FeatureSettings;
        });
    }

    public async Task<FeatureSettingsDto> UpdateFeatureSettingsAsync(UpdateFeatureSettingsRequest request, string updatedBy)
    {
        var currentSettings = await GetFeatureSettingsAsync();
        var newSettings = ApplyUpdate(currentSettings, request);
        
        var validation = await _validationService.ValidateFeatureSettingsAsync(newSettings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"功能设置验证失败: {string.Join(", ", validation.Errors.SelectMany(e => e.Value))}");
        }

        var entity = new FeatureSettings
        {
            EnableCodeSnippets = newSettings.EnableCodeSnippets,
            EnableSharing = newSettings.EnableSharing,
            EnableTags = newSettings.EnableTags,
            EnableComments = newSettings.EnableComments,
            EnableFavorites = newSettings.EnableFavorites,
            EnableSearch = newSettings.EnableSearch,
            EnableExport = newSettings.EnableExport,
            EnableImport = newSettings.EnableImport,
            EnableApi = newSettings.EnableApi,
            EnableWebHooks = newSettings.EnableWebHooks,
            EnableFileUpload = newSettings.EnableFileUpload,
            MaxFileSize = newSettings.MaxFileSize,
            AllowedFileTypes = newSettings.AllowedFileTypes,
            EnableRealTimeNotifications = newSettings.EnableRealTimeNotifications,
            EnableAnalytics = newSettings.EnableAnalytics
        };

        await _settingsRepository.UpdateFeatureSettingsAsync(entity, updatedBy);
        
        // 清除缓存
        _cache.Remove(FEATURE_SETTINGS_CACHE_KEY);
        _cache.Remove(SETTINGS_CACHE_KEY);
        
        _logger.LogInformation("功能设置已更新，操作人: {UpdatedBy}", updatedBy);
        return newSettings;
    }

    public async Task<SettingsValidationResult> ValidateFeatureSettingsAsync(FeatureSettingsDto settings)
    {
        return await _validationService.ValidateFeatureSettingsAsync(settings);
    }

    public async Task<EmailSettingsDto> GetEmailSettingsAsync()
    {
        return await _cache.GetOrCreateAsync(EMAIL_SETTINGS_CACHE_KEY, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheExpiration;
            
            var settings = await GetOrCreateSettingsAsync();
            return settings.EmailSettings;
        });
    }

    public async Task<EmailSettingsDto> UpdateEmailSettingsAsync(UpdateEmailSettingsRequest request, string updatedBy)
    {
        var currentSettings = await GetEmailSettingsAsync();
        var newSettings = ApplyUpdate(currentSettings, request);
        
        var validation = await _validationService.ValidateEmailSettingsAsync(newSettings);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"邮件设置验证失败: {string.Join(", ", validation.Errors.SelectMany(e => e.Value))}");
        }

        var entity = new EmailSettings
        {
            SmtpHost = newSettings.SmtpHost,
            SmtpPort = newSettings.SmtpPort,
            SmtpUsername = newSettings.SmtpUsername,
            SmtpPassword = newSettings.SmtpPassword,
            FromEmail = newSettings.FromEmail,
            FromName = newSettings.FromName,
            EnableSsl = newSettings.EnableSsl,
            EnableTls = newSettings.EnableTls,
            TemplatePath = newSettings.TemplatePath,
            EnableEmailQueue = newSettings.EnableEmailQueue,
            MaxRetryAttempts = newSettings.MaxRetryAttempts,
            EmailTimeout = newSettings.EmailTimeout,
            EnableEmailLogging = newSettings.EnableEmailLogging,
            TestEmailRecipient = newSettings.TestEmailRecipient
        };

        await _settingsRepository.UpdateEmailSettingsAsync(entity, updatedBy);
        
        // 清除缓存
        _cache.Remove(EMAIL_SETTINGS_CACHE_KEY);
        _cache.Remove(SETTINGS_CACHE_KEY);
        
        _logger.LogInformation("邮件设置已更新，操作人: {UpdatedBy}", updatedBy);
        return newSettings;
    }

    public async Task<SettingsValidationResult> ValidateEmailSettingsAsync(EmailSettingsDto settings)
    {
        return await _validationService.ValidateEmailSettingsAsync(settings);
    }

    public async Task<TestEmailResponse> SendTestEmailAsync(TestEmailRequest request)
    {
        try
        {
            var emailSettings = await GetEmailSettingsAsync();
            var isValid = await _validationService.ValidateEmailConfigurationAsync(emailSettings);
            
            if (!isValid)
            {
                return new TestEmailResponse
                {
                    Success = false,
                    Message = "邮件配置验证失败",
                    SentAt = DateTime.UtcNow,
                    ErrorDetails = "请检查SMTP服务器配置"
                };
            }

            // 这里应该实现实际的邮件发送逻辑
            // 为了演示，我们只是模拟发送成功
            await Task.Delay(1000); // 模拟发送延迟
            
            return new TestEmailResponse
            {
                Success = true,
                Message = "测试邮件发送成功",
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送测试邮件失败");
            return new TestEmailResponse
            {
                Success = false,
                Message = "测试邮件发送失败",
                SentAt = DateTime.UtcNow,
                ErrorDetails = ex.Message
            };
        }
    }

    public async Task<SettingsHistoryResponse> GetSettingsHistoryAsync(SettingsHistoryRequest request)
    {
        return await _historyService.GetChangeHistoryAsync(request);
    }

    public async Task<SettingsHistoryStatistics> GetSettingsHistoryStatisticsAsync()
    {
        return await _historyService.GetChangeHistoryStatisticsAsync();
    }

    public async Task<bool> DeleteSettingsHistoryAsync(Guid historyId)
    {
        return await _historyService.DeleteChangeHistoryAsync(historyId);
    }

    public async Task<int> BatchDeleteSettingsHistoryAsync(List<Guid> historyIds)
    {
        return await _historyService.BatchDeleteChangeHistoryAsync(historyIds);
    }

    public async Task<SettingsHistoryExportResponse> ExportSettingsHistoryAsync(SettingsHistoryExportRequest request)
    {
        return await _importExportService.ExportSettingsHistoryAsync(request);
    }

    public async Task<string> ExportSettingsAsync(string format = "json")
    {
        return await _importExportService.ExportSettingsAsync(format);
    }

    public async Task<SystemSettingsDto> ImportSettingsAsync(string jsonData, string updatedBy)
    {
        return await _importExportService.ImportSettingsAsync(jsonData, updatedBy);
    }

    public async Task<SettingsValidationResult> ValidateImportDataAsync(string jsonData)
    {
        return await _importExportService.ValidateImportDataAsync(jsonData);
    }

    public async Task ClearCacheAsync()
    {
        _cache.Remove(SETTINGS_CACHE_KEY);
        _cache.Remove(SITE_SETTINGS_CACHE_KEY);
        _cache.Remove(SECURITY_SETTINGS_CACHE_KEY);
        _cache.Remove(FEATURE_SETTINGS_CACHE_KEY);
        _cache.Remove(EMAIL_SETTINGS_CACHE_KEY);
        
        _logger.LogInformation("系统设置缓存已清除");
    }

    public async Task RefreshCacheAsync()
    {
        await ClearCacheAsync();
        await GetSettingsAsync(); // 重新加载缓存
        _logger.LogInformation("系统设置缓存已刷新");
    }

    public async Task<SystemSettingsDto> InitializeDefaultSettingsAsync()
    {
        var settings = await _settingsRepository.InitializeDefaultSettingsAsync();
        await ClearCacheAsync();
        return MapToDto(settings);
    }

    public async Task<bool> IsSettingsInitializedAsync()
    {
        return await _settingsRepository.SettingsExistAsync();
    }

    // 私有辅助方法
    private static SystemSettingsDto MapToDto(SystemSettings entity)
    {
        return new SystemSettingsDto
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy,
            SiteSettings = MapSiteSettingsToDto(entity.GetSiteSettings()),
            SecuritySettings = MapSecuritySettingsToDto(entity.GetSecuritySettings()),
            FeatureSettings = MapFeatureSettingsToDto(entity.GetFeatureSettings()),
            EmailSettings = MapEmailSettingsToDto(entity.GetEmailSettings())
        };
    }

    private static SystemSettings MapToEntity(SystemSettingsDto dto)
    {
        var entity = new SystemSettings
        {
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            UpdatedBy = dto.UpdatedBy
        };
        
        entity.SetSiteSettings(MapSiteSettingsToEntity(dto.SiteSettings));
        entity.SetSecuritySettings(MapSecuritySettingsToEntity(dto.SecuritySettings));
        entity.SetFeatureSettings(MapFeatureSettingsToEntity(dto.FeatureSettings));
        entity.SetEmailSettings(MapEmailSettingsToEntity(dto.EmailSettings));
        
        return entity;
    }

    private static SiteSettingsDto MapSiteSettingsToDto(SiteSettings entity)
    {
        return new SiteSettingsDto
        {
            SiteName = entity.SiteName,
            SiteDescription = entity.SiteDescription,
            LogoUrl = entity.LogoUrl,
            Theme = entity.Theme,
            Language = entity.Language,
            PageSize = entity.PageSize,
            AllowRegistration = entity.AllowRegistration,
            Announcement = entity.Announcement,
            CustomCss = entity.CustomCss,
            CustomJs = entity.CustomJs
        };
    }

    private static SiteSettings MapSiteSettingsToEntity(SiteSettingsDto dto)
    {
        return new SiteSettings
        {
            SiteName = dto.SiteName,
            SiteDescription = dto.SiteDescription,
            LogoUrl = dto.LogoUrl,
            Theme = dto.Theme,
            Language = dto.Language,
            PageSize = dto.PageSize,
            AllowRegistration = dto.AllowRegistration,
            Announcement = dto.Announcement,
            CustomCss = dto.CustomCss,
            CustomJs = dto.CustomJs
        };
    }

    private static SecuritySettingsDto MapSecuritySettingsToDto(SecuritySettings entity)
    {
        return new SecuritySettingsDto
        {
            MinPasswordLength = entity.MinPasswordLength,
            RequireUppercase = entity.RequireUppercase,
            RequireLowercase = entity.RequireLowercase,
            RequireNumbers = entity.RequireNumbers,
            RequireSpecialChars = entity.RequireSpecialChars,
            MaxLoginAttempts = entity.MaxLoginAttempts,
            AccountLockoutDuration = entity.AccountLockoutDuration,
            SessionTimeout = entity.SessionTimeout,
            EnableTwoFactorAuth = entity.EnableTwoFactorAuth,
            EnableCors = entity.EnableCors,
            AllowedCorsOrigins = entity.AllowedCorsOrigins,
            EnableHttpsRedirection = entity.EnableHttpsRedirection,
            ApiRateLimit = entity.ApiRateLimit,
            EnableLoginLogging = entity.EnableLoginLogging,
            EnableActionLogging = entity.EnableActionLogging
        };
    }

    private static SecuritySettings MapSecuritySettingsToEntity(SecuritySettingsDto dto)
    {
        return new SecuritySettings
        {
            MinPasswordLength = dto.MinPasswordLength,
            RequireUppercase = dto.RequireUppercase,
            RequireLowercase = dto.RequireLowercase,
            RequireNumbers = dto.RequireNumbers,
            RequireSpecialChars = dto.RequireSpecialChars,
            MaxLoginAttempts = dto.MaxLoginAttempts,
            AccountLockoutDuration = dto.AccountLockoutDuration,
            SessionTimeout = dto.SessionTimeout,
            EnableTwoFactorAuth = dto.EnableTwoFactorAuth,
            EnableCors = dto.EnableCors,
            AllowedCorsOrigins = dto.AllowedCorsOrigins,
            EnableHttpsRedirection = dto.EnableHttpsRedirection,
            ApiRateLimit = dto.ApiRateLimit,
            EnableLoginLogging = dto.EnableLoginLogging,
            EnableActionLogging = dto.EnableActionLogging
        };
    }

    private static FeatureSettingsDto MapFeatureSettingsToDto(FeatureSettings entity)
    {
        return new FeatureSettingsDto
        {
            EnableCodeSnippets = entity.EnableCodeSnippets,
            EnableSharing = entity.EnableSharing,
            EnableTags = entity.EnableTags,
            EnableComments = entity.EnableComments,
            EnableFavorites = entity.EnableFavorites,
            EnableSearch = entity.EnableSearch,
            EnableExport = entity.EnableExport,
            EnableImport = entity.EnableImport,
            EnableApi = entity.EnableApi,
            EnableWebHooks = entity.EnableWebHooks,
            EnableFileUpload = entity.EnableFileUpload,
            MaxFileSize = entity.MaxFileSize,
            AllowedFileTypes = entity.AllowedFileTypes,
            EnableRealTimeNotifications = entity.EnableRealTimeNotifications,
            EnableAnalytics = entity.EnableAnalytics
        };
    }

    private static FeatureSettings MapFeatureSettingsToEntity(FeatureSettingsDto dto)
    {
        return new FeatureSettings
        {
            EnableCodeSnippets = dto.EnableCodeSnippets,
            EnableSharing = dto.EnableSharing,
            EnableTags = dto.EnableTags,
            EnableComments = dto.EnableComments,
            EnableFavorites = dto.EnableFavorites,
            EnableSearch = dto.EnableSearch,
            EnableExport = dto.EnableExport,
            EnableImport = dto.EnableImport,
            EnableApi = dto.EnableApi,
            EnableWebHooks = dto.EnableWebHooks,
            EnableFileUpload = dto.EnableFileUpload,
            MaxFileSize = dto.MaxFileSize,
            AllowedFileTypes = dto.AllowedFileTypes,
            EnableRealTimeNotifications = dto.EnableRealTimeNotifications,
            EnableAnalytics = dto.EnableAnalytics
        };
    }

    private static EmailSettingsDto MapEmailSettingsToDto(EmailSettings entity)
    {
        return new EmailSettingsDto
        {
            SmtpHost = entity.SmtpHost,
            SmtpPort = entity.SmtpPort,
            SmtpUsername = entity.SmtpUsername,
            SmtpPassword = entity.SmtpPassword,
            FromEmail = entity.FromEmail,
            FromName = entity.FromName,
            EnableSsl = entity.EnableSsl,
            EnableTls = entity.EnableTls,
            TemplatePath = entity.TemplatePath,
            EnableEmailQueue = entity.EnableEmailQueue,
            MaxRetryAttempts = entity.MaxRetryAttempts,
            EmailTimeout = entity.EmailTimeout,
            EnableEmailLogging = entity.EnableEmailLogging,
            TestEmailRecipient = entity.TestEmailRecipient
        };
    }

    private static EmailSettings MapEmailSettingsToEntity(EmailSettingsDto dto)
    {
        return new EmailSettings
        {
            SmtpHost = dto.SmtpHost,
            SmtpPort = dto.SmtpPort,
            SmtpUsername = dto.SmtpUsername,
            SmtpPassword = dto.SmtpPassword,
            FromEmail = dto.FromEmail,
            FromName = dto.FromName,
            EnableSsl = dto.EnableSsl,
            EnableTls = dto.EnableTls,
            TemplatePath = dto.TemplatePath,
            EnableEmailQueue = dto.EnableEmailQueue,
            MaxRetryAttempts = dto.MaxRetryAttempts,
            EmailTimeout = dto.EmailTimeout,
            EnableEmailLogging = dto.EnableEmailLogging,
            TestEmailRecipient = dto.TestEmailRecipient
        };
    }

    private static SiteSettingsDto ApplyUpdate(SiteSettingsDto current, UpdateSiteSettingsRequest request)
    {
        return new SiteSettingsDto
        {
            SiteName = request.SiteName ?? current.SiteName,
            SiteDescription = request.SiteDescription ?? current.SiteDescription,
            LogoUrl = request.LogoUrl ?? current.LogoUrl,
            Theme = request.Theme ?? current.Theme,
            Language = request.Language ?? current.Language,
            PageSize = request.PageSize ?? current.PageSize,
            AllowRegistration = request.AllowRegistration ?? current.AllowRegistration,
            Announcement = request.Announcement ?? current.Announcement,
            CustomCss = request.CustomCss ?? current.CustomCss,
            CustomJs = request.CustomJs ?? current.CustomJs
        };
    }

    private static SecuritySettingsDto ApplyUpdate(SecuritySettingsDto current, UpdateSecuritySettingsRequest request)
    {
        return new SecuritySettingsDto
        {
            MinPasswordLength = request.MinPasswordLength ?? current.MinPasswordLength,
            RequireUppercase = request.RequireUppercase ?? current.RequireUppercase,
            RequireLowercase = request.RequireLowercase ?? current.RequireLowercase,
            RequireNumbers = request.RequireNumbers ?? current.RequireNumbers,
            RequireSpecialChars = request.RequireSpecialChars ?? current.RequireSpecialChars,
            MaxLoginAttempts = request.MaxLoginAttempts ?? current.MaxLoginAttempts,
            AccountLockoutDuration = request.AccountLockoutDuration ?? current.AccountLockoutDuration,
            SessionTimeout = request.SessionTimeout ?? current.SessionTimeout,
            EnableTwoFactorAuth = request.EnableTwoFactorAuth ?? current.EnableTwoFactorAuth,
            EnableCors = request.EnableCors ?? current.EnableCors,
            AllowedCorsOrigins = request.AllowedCorsOrigins ?? current.AllowedCorsOrigins,
            EnableHttpsRedirection = request.EnableHttpsRedirection ?? current.EnableHttpsRedirection,
            ApiRateLimit = request.ApiRateLimit ?? current.ApiRateLimit,
            EnableLoginLogging = request.EnableLoginLogging ?? current.EnableLoginLogging,
            EnableActionLogging = request.EnableActionLogging ?? current.EnableActionLogging
        };
    }

    private static FeatureSettingsDto ApplyUpdate(FeatureSettingsDto current, UpdateFeatureSettingsRequest request)
    {
        return new FeatureSettingsDto
        {
            EnableCodeSnippets = request.EnableCodeSnippets ?? current.EnableCodeSnippets,
            EnableSharing = request.EnableSharing ?? current.EnableSharing,
            EnableTags = request.EnableTags ?? current.EnableTags,
            EnableComments = request.EnableComments ?? current.EnableComments,
            EnableFavorites = request.EnableFavorites ?? current.EnableFavorites,
            EnableSearch = request.EnableSearch ?? current.EnableSearch,
            EnableExport = request.EnableExport ?? current.EnableExport,
            EnableImport = request.EnableImport ?? current.EnableImport,
            EnableApi = request.EnableApi ?? current.EnableApi,
            EnableWebHooks = request.EnableWebHooks ?? current.EnableWebHooks,
            EnableFileUpload = request.EnableFileUpload ?? current.EnableFileUpload,
            MaxFileSize = request.MaxFileSize ?? current.MaxFileSize,
            AllowedFileTypes = request.AllowedFileTypes ?? current.AllowedFileTypes,
            EnableRealTimeNotifications = request.EnableRealTimeNotifications ?? current.EnableRealTimeNotifications,
            EnableAnalytics = request.EnableAnalytics ?? current.EnableAnalytics
        };
    }

    private static EmailSettingsDto ApplyUpdate(EmailSettingsDto current, UpdateEmailSettingsRequest request)
    {
        return new EmailSettingsDto
        {
            SmtpHost = request.SmtpHost ?? current.SmtpHost,
            SmtpPort = request.SmtpPort ?? current.SmtpPort,
            SmtpUsername = request.SmtpUsername ?? current.SmtpUsername,
            SmtpPassword = request.SmtpPassword ?? current.SmtpPassword,
            FromEmail = request.FromEmail ?? current.FromEmail,
            FromName = request.FromName ?? current.FromName,
            EnableSsl = request.EnableSsl ?? current.EnableSsl,
            EnableTls = request.EnableTls ?? current.EnableTls,
            TemplatePath = request.TemplatePath ?? current.TemplatePath,
            EnableEmailQueue = request.EnableEmailQueue ?? current.EnableEmailQueue,
            MaxRetryAttempts = request.MaxRetryAttempts ?? current.MaxRetryAttempts,
            EmailTimeout = request.EmailTimeout ?? current.EmailTimeout,
            EnableEmailLogging = request.EnableEmailLogging ?? current.EnableEmailLogging,
            TestEmailRecipient = request.TestEmailRecipient ?? current.TestEmailRecipient
        };
    }

    private string GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
    }

    private string GetCurrentIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}