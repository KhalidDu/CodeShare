using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 通知设置仓储接口 - 遵循单一职责原则，负责通知设置的数据访问和操作
/// </summary>
public interface INotificationSettingsRepository
{
    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <returns>通知设置实体或null</returns>
    Task<NotificationSetting?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据用户ID获取通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置列表</returns>
    Task<IEnumerable<NotificationSetting>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// 根据用户ID和通知类型获取通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>通知设置实体或null</returns>
    Task<NotificationSetting?> GetByUserIdAndTypeAsync(Guid userId, NotificationType? notificationType);

    /// <summary>
    /// 创建通知设置
    /// </summary>
    /// <param name="setting">通知设置实体</param>
    /// <returns>创建的通知设置实体</returns>
    Task<NotificationSetting> CreateAsync(NotificationSetting setting);

    /// <summary>
    /// 更新通知设置
    /// </summary>
    /// <param name="setting">通知设置实体</param>
    /// <returns>更新后的通知设置实体</returns>
    Task<NotificationSetting> UpdateAsync(NotificationSetting setting);

    /// <summary>
    /// 删除通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid id);

    // 默认设置管理
    /// <summary>
    /// 获取用户的默认通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>默认通知设置实体或null</returns>
    Task<NotificationSetting?> GetDefaultByUserIdAsync(Guid userId);

    /// <summary>
    /// 设置默认通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="settingId">设置ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> SetDefaultAsync(Guid userId, Guid settingId);

    /// <summary>
    /// 取消默认设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UnsetDefaultAsync(Guid userId);

    // 批量操作
    /// <summary>
    /// 批量创建通知设置
    /// </summary>
    /// <param name="settings">通知设置列表</param>
    /// <returns>创建的设置数量</returns>
    Task<int> BulkCreateAsync(IEnumerable<NotificationSetting> settings);

    /// <summary>
    /// 批量更新通知设置
    /// </summary>
    /// <param name="settings">通知设置列表</param>
    /// <returns>更新的设置数量</returns>
    Task<int> BulkUpdateAsync(IEnumerable<NotificationSetting> settings);

    /// <summary>
    /// 批量删除通知设置
    /// </summary>
    /// <param name="settingIds">设置ID列表</param>
    /// <returns>删除的设置数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> settingIds);

    // 用户偏好管理
    /// <summary>
    /// 获取用户的所有通知偏好设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户通知偏好设置字典</returns>
    Task<Dictionary<NotificationType, NotificationSetting>> GetUserPreferencesAsync(Guid userId);

    /// <summary>
    /// 更新用户通知偏好设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="preferences">偏好设置字典</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateUserPreferencesAsync(Guid userId, Dictionary<NotificationType, NotificationSetting> preferences);

    /// <summary>
    /// 获取用户对特定类型的通知偏好
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>通知设置实体</returns>
    Task<NotificationSetting> GetUserPreferenceForTypeAsync(Guid userId, NotificationType notificationType);

    /// <summary>
    /// 检查用户是否启用特定类型的通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <param name="channel">通知渠道</param>
    /// <returns>是否启用</returns>
    Task<bool> IsNotificationEnabledAsync(Guid userId, NotificationType notificationType, NotificationChannel channel);

    // 频率和时间管理
    /// <summary>
    /// 获取用户的通知频率设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>通知频率</returns>
    Task<NotificationFrequency> GetNotificationFrequencyAsync(Guid userId, NotificationType? notificationType = null);

    /// <summary>
    /// 更新用户的通知频率设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="frequency">通知频率</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateNotificationFrequencyAsync(Guid userId, NotificationFrequency frequency, NotificationType? notificationType = null);

    /// <summary>
    /// 获取用户的免打扰时间设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>免打扰时间设置</returns>
    Task<(TimeSpan? Start, TimeSpan? End, bool Enabled)> GetQuietHoursAsync(Guid userId);

    /// <summary>
    /// 更新用户的免打扰时间设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="enabled">是否启用</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateQuietHoursAsync(Guid userId, TimeSpan? start, TimeSpan? end, bool enabled);

    /// <summary>
    /// 检查当前时间是否在用户的免打扰时间内
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="currentTime">当前时间</param>
    /// <returns>是否在免打扰时间内</returns>
    Task<bool> IsInQuietHoursAsync(Guid userId, DateTime? currentTime = null);

    // 批量和推送设置
    /// <summary>
    /// 获取用户的批量通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>批量通知设置</returns>
    Task<(bool Enabled, int IntervalMinutes)> GetBatchSettingsAsync(Guid userId);

    /// <summary>
    /// 更新用户的批量通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="enabled">是否启用</param>
    /// <param name="intervalMinutes">间隔分钟数</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateBatchSettingsAsync(Guid userId, bool enabled, int intervalMinutes);

    /// <summary>
    /// 获取用户的邮件通知频率设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>邮件通知频率</returns>
    Task<EmailNotificationFrequency> GetEmailFrequencyAsync(Guid userId);

    /// <summary>
    /// 更新用户的邮件通知频率设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="frequency">邮件通知频率</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateEmailFrequencyAsync(Guid userId, EmailNotificationFrequency frequency);

    // 渠道管理
    /// <summary>
    /// 启用用户的通知渠道
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="channel">通知渠道</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>操作是否成功</returns>
    Task<bool> EnableChannelAsync(Guid userId, NotificationChannel channel, NotificationType? notificationType = null);

    /// <summary>
    /// 禁用用户的通知渠道
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="channel">通知渠道</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>操作是否成功</returns>
    Task<bool> DisableChannelAsync(Guid userId, NotificationChannel channel, NotificationType? notificationType = null);

    /// <summary>
    /// 获取用户启用的通知渠道
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>启用的通知渠道列表</returns>
    Task<List<NotificationChannel>> GetEnabledChannelsAsync(Guid userId, NotificationType? notificationType = null);

    // 语言和时区设置
    /// <summary>
    /// 获取用户的通知语言设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>语言代码</returns>
    Task<string> GetLanguageAsync(Guid userId);

    /// <summary>
    /// 更新用户的通知语言设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="language">语言代码</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateLanguageAsync(Guid userId, string language);

    /// <summary>
    /// 获取用户的时区设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>时区</returns>
    Task<string> GetTimeZoneAsync(Guid userId);

    /// <summary>
    /// 更新用户的时区设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="timeZone">时区</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateTimeZoneAsync(Guid userId, string timeZone);

    // 声音和提醒设置
    /// <summary>
    /// 检查用户是否启用声音提醒
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>是否启用声音提醒</returns>
    Task<bool> IsSoundEnabledAsync(Guid userId, NotificationType? notificationType = null);

    /// <summary>
    /// 更新用户的声音提醒设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="enabled">是否启用</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateSoundSettingsAsync(Guid userId, bool enabled, NotificationType? notificationType = null);

    // 状态管理
    /// <summary>
    /// 激活通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ActivateAsync(Guid settingId);

    /// <summary>
    /// 停用通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> DeactivateAsync(Guid settingId);

    /// <summary>
    /// 获取激活的通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>激活的通知设置列表</returns>
    Task<IEnumerable<NotificationSetting>> GetActiveSettingsAsync(Guid userId);

    // 缓存相关
    /// <summary>
    /// 从缓存获取通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>通知设置实体或null</returns>
    Task<NotificationSetting?> GetFromCacheAsync(Guid userId, NotificationType? notificationType);

    /// <summary>
    /// 设置通知设置缓存
    /// </summary>
    /// <param name="setting">通知设置实体</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>操作是否成功</returns>
    Task<bool> SetCacheAsync(NotificationSetting setting, TimeSpan? expiration = null);

    /// <summary>
    /// 移除通知设置缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>操作是否成功</returns>
    Task<bool> RemoveCacheAsync(Guid userId, NotificationType? notificationType);

    /// <summary>
    /// 清空用户的所有通知设置缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ClearUserCacheAsync(Guid userId);

    // 默认设置初始化
    /// <summary>
    /// 为用户初始化默认通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的默认设置列表</returns>
    Task<IEnumerable<NotificationSetting>> InitializeDefaultSettingsAsync(Guid userId);

    /// <summary>
    /// 检查用户是否已初始化通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否已初始化</returns>
    Task<bool> HasInitializedSettingsAsync(Guid userId);

    /// <summary>
    /// 获取系统默认通知设置
    /// </summary>
    /// <returns>系统默认通知设置</returns>
    Task<NotificationSetting> GetSystemDefaultSettingsAsync();

    // 统计和分析
    /// <summary>
    /// 获取用户通知设置统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>设置统计信息</returns>
    Task<NotificationSettingsStatsDto> GetSettingsStatsAsync(Guid userId);

    /// <summary>
    /// 获取系统通知设置统计信息
    /// </summary>
    /// <returns>系统设置统计信息</returns>
    Task<SystemNotificationSettingsStatsDto> GetSystemSettingsStatsAsync();

    /// <summary>
    /// 获取最常用的通知设置组合
    /// </summary>
    /// <param name="limit">限制数量</param>
    /// <returns>设置组合统计</returns>
    Task<IEnumerable<SettingsCombinationStatsDto>> GetPopularSettingsCombinationsAsync(int limit = 10);

    // 导入导出
    /// <summary>
    /// 导出用户通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置列表</returns>
    Task<IEnumerable<NotificationSetting>> ExportUserSettingsAsync(Guid userId);

    /// <summary>
    /// 导入用户通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="settings">通知设置列表</param>
    /// <param name="overrideExisting">是否覆盖现有设置</param>
    /// <returns>导入的设置数量</returns>
    Task<int> ImportUserSettingsAsync(Guid userId, IEnumerable<NotificationSetting> settings, bool overrideExisting = false);

    // 权限验证
    /// <summary>
    /// 检查用户是否可以访问通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以访问</returns>
    Task<bool> CanUserAccessAsync(Guid settingId, Guid userId);

    /// <summary>
    /// 检查用户是否可以编辑通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以编辑</returns>
    Task<bool> CanUserEditAsync(Guid settingId, Guid userId);

    /// <summary>
    /// 检查用户是否可以删除通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以删除</returns>
    Task<bool> CanUserDeleteAsync(Guid settingId, Guid userId);

    // 性能优化
    /// <summary>
    /// 预加载用户通知设置
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    /// <returns>用户设置字典</returns>
    Task<Dictionary<Guid, IEnumerable<NotificationSetting>>> PreloadUsersSettingsAsync(IEnumerable<Guid> userIds);

    /// <summary>
    /// 批量获取用户通知偏好
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>用户偏好字典</returns>
    Task<Dictionary<Guid, NotificationSetting>> BulkGetUserPreferencesAsync(IEnumerable<Guid> userIds, NotificationType notificationType);

    /// <summary>
    /// 批量更新通知设置状态
    /// </summary>
    /// <param name="settingIds">设置ID列表</param>
    /// <param name="isActive">是否激活</param>
    /// <returns>更新的设置数量</returns>
    Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> settingIds, bool isActive);
}

/// <summary>
/// 通知设置统计信息DTO
/// </summary>
public class NotificationSettingsStatsDto
{
    /// <summary>
    /// 总设置数量
    /// </summary>
    public int TotalSettings { get; set; }

    /// <summary>
    /// 激活设置数量
    /// </summary>
    public int ActiveSettings { get; set; }

    /// <summary>
    /// 默认设置数量
    /// </summary>
    public int DefaultSettings { get; set; }

    /// <summary>
    /// 按通知类型统计的设置数量
    /// </summary>
    public Dictionary<NotificationType, int> TypeStats { get; set; } = new();

    /// <summary>
    /// 按渠道统计的启用数量
    /// </summary>
    public Dictionary<NotificationChannel, int> ChannelStats { get; set; } = new();

    /// <summary>
    /// 按频率统计的设置数量
    /// </summary>
    public Dictionary<NotificationFrequency, int> FrequencyStats { get; set; } = new();

    /// <summary>
    /// 启用免打扰功能的用户数量
    /// </summary>
    public int QuietHoursEnabled { get; set; }

    /// <summary>
    /// 启用批量通知的用户数量
    /// </summary>
    public int BatchNotificationsEnabled { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 系统通知设置统计信息DTO
/// </summary>
public class SystemNotificationSettingsStatsDto
{
    /// <summary>
    /// 总用户数量
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// 已初始化设置的用户数量
    /// </summary>
    public int UsersWithSettings { get; set; }

    /// <summary>
    /// 最受欢迎的通知渠道
    /// </summary>
    public NotificationChannel MostPopularChannel { get; set; }

    /// <summary>
    /// 最受欢迎的通知频率
    /// </summary>
    public NotificationFrequency MostPopularFrequency { get; set; }

    /// <summary>
    /// 启用免打扰功能的用户比例
    /// </summary>
    public double QuietHoursAdoptionRate { get; set; }

    /// <summary>
    /// 启用批量通知的用户比例
    /// </summary>
    public double BatchNotificationsAdoptionRate { get; set; }

    /// <summary>
    /// 按语言分布的设置数量
    /// </summary>
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();

    /// <summary>
    /// 按时区分布的设置数量
    /// </summary>
    public Dictionary<string, int> TimeZoneDistribution { get; set; } = new();
}

/// <summary>
/// 设置组合统计信息DTO
/// </summary>
public class SettingsCombinationStatsDto
{
    /// <summary>
    /// 设置组合描述
    /// </summary>
    public string Combination { get; set; } = string.Empty;

    /// <summary>
    /// 使用此组合的用户数量
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// 占总用户的比例
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 设置详情
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}