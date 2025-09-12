using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 系统设置服务接口 - 遵循接口隔离原则
/// </summary>
public interface ISystemSettingsService
{
    // 基础设置管理
    Task<SystemSettingsDto?> GetSettingsAsync();
    Task<SystemSettingsDto> GetOrCreateSettingsAsync();
    Task<SystemSettingsDto> UpdateSettingsAsync(SystemSettingsDto settings);
    
    // 站点设置管理
    Task<SiteSettingsDto> GetSiteSettingsAsync();
    Task<SiteSettingsDto> UpdateSiteSettingsAsync(UpdateSiteSettingsRequest request, string updatedBy);
    Task<SettingsValidationResult> ValidateSiteSettingsAsync(SiteSettingsDto settings);
    
    // 安全设置管理
    Task<SecuritySettingsDto> GetSecuritySettingsAsync();
    Task<SecuritySettingsDto> UpdateSecuritySettingsAsync(UpdateSecuritySettingsRequest request, string updatedBy);
    Task<SettingsValidationResult> ValidateSecuritySettingsAsync(SecuritySettingsDto settings);
    
    // 功能设置管理
    Task<FeatureSettingsDto> GetFeatureSettingsAsync();
    Task<FeatureSettingsDto> UpdateFeatureSettingsAsync(UpdateFeatureSettingsRequest request, string updatedBy);
    Task<SettingsValidationResult> ValidateFeatureSettingsAsync(FeatureSettingsDto settings);
    
    // 邮件设置管理
    Task<EmailSettingsDto> GetEmailSettingsAsync();
    Task<EmailSettingsDto> UpdateEmailSettingsAsync(UpdateEmailSettingsRequest request, string updatedBy);
    Task<SettingsValidationResult> ValidateEmailSettingsAsync(EmailSettingsDto settings);
    Task<TestEmailResponse> SendTestEmailAsync(TestEmailRequest request);
    
    // 设置历史管理
    Task<SettingsHistoryResponse> GetSettingsHistoryAsync(SettingsHistoryRequest request);
    Task<SettingsHistoryStatistics> GetSettingsHistoryStatisticsAsync();
    Task<bool> DeleteSettingsHistoryAsync(Guid historyId);
    Task<int> BatchDeleteSettingsHistoryAsync(List<Guid> historyIds);
    Task<SettingsHistoryExportResponse> ExportSettingsHistoryAsync(SettingsHistoryExportRequest request);
    
    // 导入导出功能
    Task<string> ExportSettingsAsync(string format = "json");
    Task<SystemSettingsDto> ImportSettingsAsync(string jsonData, string updatedBy);
    Task<SettingsValidationResult> ValidateImportDataAsync(string jsonData);
    
    // 缓存管理
    Task ClearCacheAsync();
    Task RefreshCacheAsync();
    
    // 系统初始化
    Task<SystemSettingsDto> InitializeDefaultSettingsAsync();
    Task<bool> IsSettingsInitializedAsync();
}

/// <summary>
/// 设置变更历史服务接口 - 遵循接口隔离原则
/// </summary>
public interface ISettingsHistoryService
{
    Task<SettingsHistoryDto> CreateChangeHistoryAsync(CreateSettingsHistoryRequest request);
    Task<SettingsHistoryDto?> GetChangeHistoryByIdAsync(Guid id);
    Task<SettingsHistoryResponse> GetChangeHistoryAsync(SettingsHistoryRequest request);
    Task<SettingsHistoryStatistics> GetChangeHistoryStatisticsAsync();
    Task<List<SettingsHistoryDto>> GetRecentChangesAsync(int count = 10);
    Task<List<SettingsHistoryDto>> GetChangesBySettingTypeAsync(string settingType, int count = 50);
    Task<List<SettingsHistoryDto>> GetChangesByUserAsync(string username, int count = 50);
    Task<List<SettingsHistoryDto>> GetImportantChangesAsync(int count = 20);
    Task<List<SettingsHistoryDto>> GetFailedChangesAsync(int count = 20);
    Task<bool> DeleteChangeHistoryAsync(Guid id);
    Task<int> BatchDeleteChangeHistoryAsync(List<Guid> ids);
    Task<int> CleanExpiredHistoryAsync(DateTime cutoffDate);
}

/// <summary>
/// 设置验证服务接口 - 遵循接口隔离原则
/// </summary>
public interface ISettingsValidationService
{
    Task<SettingsValidationResult> ValidateSiteSettingsAsync(SiteSettingsDto settings);
    Task<SettingsValidationResult> ValidateSecuritySettingsAsync(SecuritySettingsDto settings);
    Task<SettingsValidationResult> ValidateFeatureSettingsAsync(FeatureSettingsDto settings);
    Task<SettingsValidationResult> ValidateEmailSettingsAsync(EmailSettingsDto settings);
    Task<SettingsValidationResult> ValidateAllSettingsAsync(SystemSettingsDto settings);
    
    // 单项验证
    Task<bool> ValidatePasswordPolicyAsync(string password);
    Task<bool> ValidateEmailConfigurationAsync(EmailSettingsDto settings);
    Task<bool> ValidateUrlFormatAsync(string url);
    Task<bool> ValidateFileSizeAsync(long fileSize, int maxSizeMB);
    Task<bool> ValidateFileTypeAsync(string fileName, string[] allowedTypes);
    
    // 批量验证
    Task<Dictionary<string, SettingsValidationResult>> ValidateMultipleSettingsAsync(Dictionary<string, object> settings);
}

