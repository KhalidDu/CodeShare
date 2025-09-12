using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using SystemSettings = CodeSnippetManager.Api.Models.SystemSettings;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 系统设置仓储接口 - 遵循单一职责原则，负责系统设置的数据访问
/// </summary>
public interface ISystemSettingsRepository
{
    /// <summary>
    /// 获取系统设置
    /// </summary>
    Task<SystemSettings?> GetSettingsAsync();
    
    /// <summary>
    /// 创建或更新系统设置
    /// </summary>
    Task<SystemSettings> SaveSettingsAsync(SystemSettings settings);
    
    /// <summary>
    /// 更新站点设置
    /// </summary>
    Task<SystemSettings> UpdateSiteSettingsAsync(SiteSettings siteSettings, string updatedBy);
    
    /// <summary>
    /// 更新安全设置
    /// </summary>
    Task<SystemSettings> UpdateSecuritySettingsAsync(SecuritySettings securitySettings, string updatedBy);
    
    /// <summary>
    /// 更新功能设置
    /// </summary>
    Task<SystemSettings> UpdateFeatureSettingsAsync(FeatureSettings featureSettings, string updatedBy);
    
    /// <summary>
    /// 更新邮件设置
    /// </summary>
    Task<SystemSettings> UpdateEmailSettingsAsync(EmailSettings emailSettings, string updatedBy);
    
    /// <summary>
    /// 记录设置变更历史
    /// </summary>
    Task<SettingsHistory> RecordChangeHistoryAsync(CreateSettingsHistoryRequest request);
    
    /// <summary>
    /// 获取设置变更历史
    /// </summary>
    Task<List<SettingsHistory>> GetChangeHistoryAsync(SettingsHistoryRequest request);
    
    /// <summary>
    /// 根据ID获取设置变更历史
    /// </summary>
    Task<SettingsHistory?> GetHistoryByIdAsync(Guid id);
    
    /// <summary>
    /// 获取设置变更历史统计
    /// </summary>
    Task<SettingsHistoryStatistics> GetChangeHistoryStatisticsAsync();
    
    /// <summary>
    /// 初始化默认设置
    /// </summary>
    Task<SystemSettings> InitializeDefaultSettingsAsync();
    
    /// <summary>
    /// 检查设置是否存在
    /// </summary>
    Task<bool> SettingsExistAsync();
    
    /// <summary>
    /// 删除设置变更历史记录
    /// </summary>
    Task<bool> DeleteChangeHistoryAsync(Guid historyId);
    
    /// <summary>
    /// 批量删除设置变更历史记录
    /// </summary>
    Task<int> BatchDeleteChangeHistoryAsync(List<Guid> historyIds);
    
    /// <summary>
    /// 清理过期的设置变更历史记录
    /// </summary>
    Task<int> CleanExpiredChangeHistoryAsync(DateTime cutoffDate);
}

/// <summary>
/// 设置变更历史仓储接口 - 遵循单一职责原则，负责设置变更历史的数据访问
/// </summary>
public interface ISettingsHistoryRepository
{
    /// <summary>
    /// 创建设置变更历史记录
    /// </summary>
    Task<SettingsHistory> CreateAsync(SettingsHistory history);
    
    /// <summary>
    /// 根据ID获取设置变更历史记录
    /// </summary>
    Task<SettingsHistory?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 获取设置变更历史列表
    /// </summary>
    Task<List<SettingsHistory>> GetByFilterAsync(SettingsHistoryRequest request);
    
    /// <summary>
    /// 获取设置变更历史统计
    /// </summary>
    Task<SettingsHistoryStatistics> GetStatisticsAsync();
    
    /// <summary>
    /// 删除设置变更历史记录
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 批量删除设置变更历史记录
    /// </summary>
    Task<int> BatchDeleteAsync(List<Guid> ids);
    
    /// <summary>
    /// 清理过期的设置变更历史记录
    /// </summary>
    Task<int> CleanExpiredAsync(DateTime cutoffDate);
    
    /// <summary>
    /// 获取最近的设置变更记录
    /// </summary>
    Task<List<SettingsHistory>> GetRecentChangesAsync(int count = 10);
    
    /// <summary>
    /// 根据设置类型获取变更历史
    /// </summary>
    Task<List<SettingsHistory>> GetBySettingTypeAsync(string settingType, int count = 50);
    
    /// <summary>
    /// 根据操作人获取变更历史
    /// </summary>
    Task<List<SettingsHistory>> GetByChangedByAsync(string changedBy, int count = 50);
    
    /// <summary>
    /// 获取重要变更记录
    /// </summary>
    Task<List<SettingsHistory>> GetImportantChangesAsync(int count = 20);
    
    /// <summary>
    /// 获取失败的变更记录
    /// </summary>
    Task<List<SettingsHistory>> GetFailedChangesAsync(int count = 20);
    
    /// <summary>
    /// 获取时间范围内的变更记录
    /// </summary>
    Task<List<SettingsHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}