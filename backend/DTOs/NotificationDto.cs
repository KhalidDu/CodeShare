using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知DTO - 用于API响应
/// </summary>
public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Message { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    public RelatedEntityType? RelatedEntityType { get; set; }
    public string? RelatedEntityId { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public string? TriggeredByUserName { get; set; }
    public string? TriggeredByUserAvatar { get; set; }
    public NotificationAction? Action { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ScheduledToSendAt { get; set; }
    public int SendCount { get; set; }
    public DateTime? LastSentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DataJson { get; set; }
    public string? Tag { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool RequiresConfirmation { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public TimeSpan? TimeSinceCreated { get; set; }
    public bool CanMarkAsRead { get; set; }
    public bool CanDelete { get; set; }
    public bool CanArchive { get; set; }
    public bool CanConfirm { get; set; }
    public List<NotificationDeliveryHistoryDto> DeliveryHistory { get; set; } = new();
}

/// <summary>
/// 创建通知请求DTO
/// </summary>
public class CreateNotificationDto
{
    [Required(ErrorMessage = "接收用户ID不能为空")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "通知标题不能为空")]
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    [StringLength(500, ErrorMessage = "通知消息长度不能超过500个字符")]
    public string? Message { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public RelatedEntityType? RelatedEntityType { get; set; }

    [StringLength(36, ErrorMessage = "相关实体ID长度不能超过36个字符")]
    public string? RelatedEntityId { get; set; }

    public Guid? TriggeredByUserId { get; set; }

    public NotificationAction? Action { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ScheduledToSendAt { get; set; }

    [StringLength(1000, ErrorMessage = "错误信息长度不能超过1000个字符")]
    public string? DataJson { get; set; }

    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    public bool RequiresConfirmation { get; set; } = false;
}

/// <summary>
/// 更新通知请求DTO
/// </summary>
public class UpdateNotificationDto
{
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    [StringLength(500, ErrorMessage = "通知消息长度不能超过500个字符")]
    public string? Message { get; set; }

    public NotificationPriority? Priority { get; set; }

    public NotificationAction? Action { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ScheduledToSendAt { get; set; }

    [StringLength(1000, ErrorMessage = "错误信息长度不能超过1000个字符")]
    public string? ErrorMessage { get; set; }

    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    public bool? RequiresConfirmation { get; set; }
}

/// <summary>
/// 通知状态更新请求DTO
/// </summary>
public class NotificationStatusUpdateDto
{
    [Required(ErrorMessage = "通知状态不能为空")]
    public NotificationStatus Status { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public bool? IsArchived { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// 批量标记已读请求DTO
/// </summary>
public class BatchMarkAsReadDto
{
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    public bool MarkAllAsRead { get; set; } = false;

    public NotificationType? TypeFilter { get; set; }

    public DateTime? BeforeDate { get; set; }
}

/// <summary>
/// 批量删除通知请求DTO
/// </summary>
public class BatchDeleteNotificationsDto
{
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    public bool DeleteAll { get; set; } = false;

    public NotificationType? TypeFilter { get; set; }

    public DateTime? BeforeDate { get; set; }

    public bool PermanentDelete { get; set; } = false;
}

/// <summary>
/// 通知筛选条件DTO
/// </summary>
public class NotificationFilterDto
{
    public Guid? UserId { get; set; }

    public NotificationType? Type { get; set; }

    public NotificationPriority? Priority { get; set; }

    public NotificationStatus? Status { get; set; }

    public RelatedEntityType? RelatedEntityType { get; set; }

    public string? RelatedEntityId { get; set; }

    public Guid? TriggeredByUserId { get; set; }

    public NotificationAction? Action { get; set; }

    public NotificationChannel? Channel { get; set; }

    public bool? IsRead { get; set; }

    public bool? IsArchived { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Tag { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? ExpiresBefore { get; set; }

    public DateTime? ExpiresAfter { get; set; }

    public bool? RequiresConfirmation { get; set; }

    public string? Search { get; set; }

    public NotificationSort Sort { get; set; } = NotificationSort.CreatedAtDesc;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 通知设置DTO - 用于API响应
/// </summary>
public class NotificationSettingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType? NotificationType { get; set; }
    public bool EnableInApp { get; set; }
    public bool EnableEmail { get; set; }
    public bool EnablePush { get; set; }
    public bool EnableDesktop { get; set; }
    public bool EnableSound { get; set; }
    public NotificationFrequency Frequency { get; set; }
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public bool EnableQuietHours { get; set; }
    public EmailNotificationFrequency EmailFrequency { get; set; }
    public int BatchIntervalMinutes { get; set; }
    public bool EnableBatching { get; set; }
    public string Language { get; set; } = "zh-CN";
    public string TimeZone { get; set; } = "Asia/Shanghai";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsDefault { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>
/// 创建通知设置请求DTO
/// </summary>
public class CreateNotificationSettingDto
{
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    public NotificationType? NotificationType { get; set; }

    public bool EnableInApp { get; set; } = true;

    public bool EnableEmail { get; set; } = true;

    public bool EnablePush { get; set; } = true;

    public bool EnableDesktop { get; set; } = true;

    public bool EnableSound { get; set; } = true;

    public NotificationFrequency Frequency { get; set; } = NotificationFrequency.Immediate;

    public TimeSpan? QuietHoursStart { get; set; }

    public TimeSpan? QuietHoursEnd { get; set; }

    public bool EnableQuietHours { get; set; } = false;

    public EmailNotificationFrequency EmailFrequency { get; set; } = EmailNotificationFrequency.Immediate;

    [Range(5, 1440, ErrorMessage = "批量通知间隔必须在5-1440分钟之间")]
    public int BatchIntervalMinutes { get; set; } = 30;

    public bool EnableBatching { get; set; } = false;

    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string Language { get; set; } = "zh-CN";

    [StringLength(50, ErrorMessage = "时区长度不能超过50个字符")]
    public string TimeZone { get; set; } = "Asia/Shanghai";

    public bool IsDefault { get; set; } = false;

    [StringLength(100, ErrorMessage = "设置名称长度不能超过100个字符")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "设置描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 更新通知设置请求DTO
/// </summary>
public class UpdateNotificationSettingDto
{
    public NotificationType? NotificationType { get; set; }

    public bool? EnableInApp { get; set; }

    public bool? EnableEmail { get; set; }

    public bool? EnablePush { get; set; }

    public bool? EnableDesktop { get; set; }

    public bool? EnableSound { get; set; }

    public NotificationFrequency? Frequency { get; set; }

    public TimeSpan? QuietHoursStart { get; set; }

    public TimeSpan? QuietHoursEnd { get; set; }

    public bool? EnableQuietHours { get; set; }

    public EmailNotificationFrequency? EmailFrequency { get; set; }

    [Range(5, 1440, ErrorMessage = "批量通知间隔必须在5-1440分钟之间")]
    public int? BatchIntervalMinutes { get; set; }

    public bool? EnableBatching { get; set; }

    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string? Language { get; set; }

    [StringLength(50, ErrorMessage = "时区长度不能超过50个字符")]
    public string? TimeZone { get; set; }

    public bool? IsDefault { get; set; }

    [StringLength(100, ErrorMessage = "设置名称长度不能超过100个字符")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "设置描述长度不能超过500个字符")]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// 通知发送历史DTO - 用于API响应
/// </summary>
public class NotificationDeliveryHistoryDto
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? RecipientAddress { get; set; }
    public string? Provider { get; set; }
    public decimal? Cost { get; set; }
    public string? MetadataJson { get; set; }
    public TimeSpan? TimeSinceSent { get; set; }
    public TimeSpan? DeliveryTime { get; set; }
}

/// <summary>
/// 通知统计信息DTO
/// </summary>
public class NotificationStatsDto
{
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int ReadCount { get; set; }
    public int FailedCount { get; set; }
    public int ArchivedCount { get; set; }
    public int DeletedCount { get; set; }
    public int HighPriorityCount { get; set; }
    public int RecentCount { get; set; }
    public int TodayCount { get; set; }
    public DateTime? LastNotificationAt { get; set; }
    public Dictionary<NotificationType, int> TypeStats { get; set; } = new();
    public Dictionary<NotificationPriority, int> PriorityStats { get; set; } = new();
    public Dictionary<NotificationStatus, int> StatusStats { get; set; } = new();
    public Dictionary<NotificationChannel, int> ChannelStats { get; set; } = new();
}

/// <summary>
/// 通知摘要DTO
/// </summary>
public class NotificationSummaryDto
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int HighPriorityNotifications { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; } = new();
    public Dictionary<NotificationType, int> TypeCounts { get; set; } = new();
    public DateTime? LastCheckedAt { get; set; }
}

/// <summary>
/// 通知测试请求DTO
/// </summary>
public class TestNotificationDto
{
    [Required(ErrorMessage = "接收用户ID不能为空")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "通知标题不能为空")]
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public List<NotificationChannel> Channels { get; set; } = new();

    public bool SendImmediately { get; set; } = true;

    public string? DataJson { get; set; }
}

/// <summary>
/// 通知导出选项DTO
/// </summary>
public class NotificationExportOptionsDto
{
    public ExportFormat Format { get; set; } = ExportFormat.Json;

    public Guid? UserId { get; set; }

    public NotificationType? Type { get; set; }

    public NotificationPriority? Priority { get; set; }

    public NotificationStatus? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IncludeDeleted { get; set; } = false;

    public bool IncludeArchived { get; set; } = false;

    public bool IncludeDeliveryHistory { get; set; } = false;

    public bool IncludeUserInfo { get; set; } = false;
}

/// <summary>
/// 通知导入数据DTO
/// </summary>
public class NotificationImportDataDto
{
    [Required(ErrorMessage = "通知数据不能为空")]
    [MinLength(1, ErrorMessage = "至少需要包含一个通知")]
    public List<ImportNotificationDto> Notifications { get; set; } = new();

    public bool OverrideExisting { get; set; } = false;

    public bool ValidateOnly { get; set; } = false;

    public string? ValidationReport { get; set; }
}

/// <summary>
/// 导入通知DTO
/// </summary>
public class ImportNotificationDto
{
    [Required(ErrorMessage = "通知标题不能为空")]
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    [StringLength(500, ErrorMessage = "通知消息长度不能超过500个字符")]
    public string? Message { get; set; }

    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

    public RelatedEntityType? RelatedEntityType { get; set; }

    public string? RelatedEntityId { get; set; }

    public NotificationAction? Action { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public bool IsRead { get; set; } = false;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? Tag { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public bool RequiresConfirmation { get; set; } = false;

    public string? DataJson { get; set; }
}

/// <summary>
/// 通知排序枚举
/// </summary>
public enum NotificationSort
{
    CreatedAtDesc = 0,
    CreatedAtAsc = 1,
    UpdatedAtDesc = 2,
    UpdatedAtAsc = 3,
    PriorityDesc = 4,
    PriorityAsc = 5,
    ReadAtDesc = 6,
    ReadAtAsc = 7,
    TitleAsc = 8,
    TitleDesc = 9,
    TypeAsc = 10,
    TypeDesc = 11
}


/// <summary>
/// 通知权限DTO
/// </summary>
public class NotificationPermissionsDto
{
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanMarkAsRead { get; set; }
    public bool CanArchive { get; set; }
    public bool CanConfirm { get; set; }
    public bool CanManageSettings { get; set; }
    public bool CanSendTest { get; set; }
    public bool CanExport { get; set; }
    public bool CanImport { get; set; }
    public bool CanViewStats { get; set; }
    public bool CanManageOthers { get; set; }
}

/// <summary>
/// 通知发送请求DTO
/// </summary>
public class NotificationSendRequestDto
{
    [Required(ErrorMessage = "接收用户ID不能为空")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType Type { get; set; }

    [Required(ErrorMessage = "通知标题不能为空")]
    [StringLength(200, ErrorMessage = "通知标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "通知内容长度不能超过2000个字符")]
    public string? Content { get; set; }

    [StringLength(500, ErrorMessage = "通知消息长度不能超过500个字符")]
    public string? Message { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public RelatedEntityType? RelatedEntityType { get; set; }

    [StringLength(36, ErrorMessage = "相关实体ID长度不能超过36个字符")]
    public string? RelatedEntityId { get; set; }

    public Guid? TriggeredByUserId { get; set; }

    public NotificationAction? Action { get; set; }

    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? ScheduledToSendAt { get; set; }

    [StringLength(1000, ErrorMessage = "数据JSON长度不能超过1000个字符")]
    public string? DataJson { get; set; }

    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    public bool RequiresConfirmation { get; set; } = false;
}

/// <summary>
/// 通知批量发送结果DTO
/// </summary>
public class NotificationBatchSendResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<NotificationDto> SuccessfulNotifications { get; set; } = new();
    public List<NotificationSendErrorDto> FailedNotifications { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}

/// <summary>
/// 通知发送错误DTO
/// </summary>
public class NotificationSendErrorDto
{
    public NotificationSendRequestDto Request { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime ErrorAt { get; set; }
}

/// <summary>
/// 通知测试结果DTO
/// </summary>
public class NotificationTestResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<NotificationChannelTestResultDto> ChannelResults { get; set; } = new();
    public DateTime TestAt { get; set; }
    public TimeSpan TestDuration { get; set; }
    public NotificationDto? TestNotification { get; set; }
}

/// <summary>
/// 通知渠道测试结果DTO
/// </summary>
public class NotificationChannelTestResultDto
{
    public NotificationChannel Channel { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 通知批量操作结果DTO
/// </summary>
public class NotificationBatchOperationResultDto
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingDuration { get; set; }
}

/// <summary>
/// 通知批量操作DTO
/// </summary>
public class NotificationBatchOperationDto
{
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要选择一个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    [Required(ErrorMessage = "操作类型不能为空")]
    public NotificationBatchOperationType OperationType { get; set; }

    public DateTime? OperationTime { get; set; }
    public string? OperationReason { get; set; }
}

/// <summary>
/// 通知批量操作类型枚举
/// </summary>
public enum NotificationBatchOperationType
{
    MarkAsRead,
    MarkAsUnread,
    Archive,
    Unarchive,
    Delete,
    PermanentDelete,
    Confirm
}

/// <summary>
/// 系统通知统计DTO
/// </summary>
public class SystemNotificationStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalNotifications { get; set; }
    public int TodayNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public double AverageDeliveryTime { get; set; }
    public Dictionary<NotificationType, int> TypeDistribution { get; set; } = new();
    public Dictionary<NotificationChannel, int> ChannelDistribution { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 分页通知结果DTO
/// </summary>
public class PaginatedNotificationsDto
{
    public List<NotificationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public NotificationFilterDto Filter { get; set; } = new();
}

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON格式
    /// </summary>
    Json = 0,

    /// <summary>
    /// CSV格式
    /// </summary>
    Csv = 1,

    /// <summary>
    /// Excel格式
    /// </summary>
    Excel = 2,

    /// <summary>
    /// XML格式
    /// </summary>
    Xml = 3,

    /// <summary>
    /// PDF格式
    /// </summary>
    Pdf = 4
}

/// <summary>
/// 批量通知操作请求DTO
/// </summary>
public class BulkNotificationRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多操作1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 操作类型
    /// </summary>
    [Required(ErrorMessage = "操作类型不能为空")]
    public NotificationOperation Operation { get; set; }

    /// <summary>
    /// 操作原因
    /// </summary>
    [StringLength(500, ErrorMessage = "操作原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 标记通知已读请求DTO
/// </summary>
public class MarkAsReadRequest
{
    /// <summary>
    /// 通知ID
    /// </summary>
    [Required(ErrorMessage = "通知ID不能为空")]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量标记已读请求DTO
/// </summary>
public class BulkMarkAsReadRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多标记1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 标记所有未读通知
    /// </summary>
    public bool MarkAllUnread { get; set; } = false;

    /// <summary>
    /// 接收者ID（用于标记特定用户的所有通知）
    /// </summary>
    public Guid? RecipientId { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 标记通知已送达请求DTO
/// </summary>
public class MarkAsDeliveredRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多标记1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 送达渠道
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 删除通知请求DTO
/// </summary>
public class DeleteNotificationRequest
{
    /// <summary>
    /// 通知ID
    /// </summary>
    [Required(ErrorMessage = "通知ID不能为空")]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 删除原因
    /// </summary>
    [StringLength(500, ErrorMessage = "删除原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否永久删除
    /// </summary>
    public bool PermanentDelete { get; set; } = false;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知列表响应DTO
/// </summary>
public class NotificationListResponse
{
    /// <summary>
    /// 通知列表
    /// </summary>
    public List<NotificationDto> Notifications { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }

    /// <summary>
    /// 统计信息
    /// </summary>
    public NotificationStatsResponse? Stats { get; set; }
}

/// <summary>
/// 通知摘要响应DTO
/// </summary>
public class NotificationSummaryResponse
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容摘要
    /// </summary>
    public string ContentSummary { get; set; } = string.Empty;

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知优先级
    /// </summary>
    public NotificationPriority Priority { get; set; }

    /// <summary>
    /// 发送者用户名
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// 发送者头像
    /// </summary>
    public string? SenderAvatar { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 是否已送达
    /// </summary>
    public bool IsDelivered { get; set; }

    /// <summary>
    /// 操作链接
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// 图标URL
    /// </summary>
    public string? IconUrl { get; set; }
}

/// <summary>
/// 未读通知响应DTO
/// </summary>
public class UnreadNotificationResponse
{
    /// <summary>
    /// 未读通知列表
    /// </summary>
    public List<NotificationSummaryResponse> UnreadNotifications { get; set; } = new();

    /// <summary>
    /// 未读通知总数
    /// </summary>
    public int TotalUnreadCount { get; set; }

    /// <summary>
    /// 按类型分组的未读通知
    /// </summary>
    public Dictionary<NotificationType, int> UnreadByType { get; set; } = new();

    /// <summary>
    /// 按优先级分组的未读通知
    /// </summary>
    public Dictionary<NotificationPriority, int> UnreadByPriority { get; set; } = new();

    /// <summary>
    /// 高优先级未读通知
    /// </summary>
    public List<NotificationSummaryResponse> HighPriorityNotifications { get; set; } = new();

    /// <summary>
    /// 最后获取时间
    /// </summary>
    public DateTime FetchedAt { get; set; }
}

/// <summary>
/// 通知统计响应DTO
/// </summary>
public class NotificationStatsResponse
{
    /// <summary>
    /// 总通知数
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// 未读通知数
    /// </summary>
    public int UnreadNotifications { get; set; }

    /// <summary>
    /// 已读通知数
    /// </summary>
    public int ReadNotifications { get; set; }

    /// <summary>
    /// 已送达通知数
    /// </summary>
    public int DeliveredNotifications { get; set; }

    /// <summary>
    /// 高优先级通知数
    /// </summary>
    public int HighPriorityNotifications { get; set; }

    /// <summary>
    /// 按通知类型统计
    /// </summary>
    public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();

    /// <summary>
    /// 按通知状态统计
    /// </summary>
    public Dictionary<NotificationStatus, int> NotificationsByStatus { get; set; } = new();

    /// <summary>
    /// 按通知优先级统计
    /// </summary>
    public Dictionary<NotificationPriority, int> NotificationsByPriority { get; set; } = new();

    /// <summary>
    /// 按通知渠道统计
    /// </summary>
    public Dictionary<NotificationChannel, int> NotificationsByChannel { get; set; } = new();

    /// <summary>
    /// 最新通知时间
    /// </summary>
    public DateTime? LatestNotificationAt { get; set; }
}

/// <summary>
/// 创建通知设置请求DTO
/// </summary>
public class CreateNotificationSettingsRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 通知渠道设置
    /// </summary>
    public List<NotificationChannelRequest> ChannelSettings { get; set; } = new();

    /// <summary>
    /// 免打扰设置
    /// </summary>
    public DoNotDisturbRequest? DoNotDisturb { get; set; }

    /// <summary>
    /// 频率限制
    /// </summary>
    [Range(1, 100, ErrorMessage = "频率限制必须在1-100之间")]
    public int? FrequencyLimit { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 更新通知设置请求DTO
/// </summary>
public class UpdateNotificationSettingsRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 通知渠道设置
    /// </summary>
    public List<NotificationChannelRequest>? ChannelSettings { get; set; }

    /// <summary>
    /// 免打扰设置
    /// </summary>
    public DoNotDisturbRequest? DoNotDisturb { get; set; }

    /// <summary>
    /// 频率限制
    /// </summary>
    [Range(1, 100, ErrorMessage = "频率限制必须在1-100之间")]
    public int? FrequencyLimit { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知设置响应DTO
/// </summary>
public class NotificationSettingsResponse
{
    /// <summary>
    /// 设置ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 通知渠道设置
    /// </summary>
    public List<NotificationChannelResponse> ChannelSettings { get; set; } = new();

    /// <summary>
    /// 免打扰设置
    /// </summary>
    public DoNotDisturbResponse? DoNotDisturb { get; set; }

    /// <summary>
    /// 频率限制
    /// </summary>
    public int? FrequencyLimit { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知偏好设置请求DTO
/// </summary>
public class NotificationPreferenceRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 默认通知渠道
    /// </summary>
    public NotificationChannel DefaultChannel { get; set; } = NotificationChannel.InApp;

    /// <summary>
    /// 默认优先级
    /// </summary>
    public NotificationPriority DefaultPriority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 语言偏好
    /// </summary>
    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string? Language { get; set; }

    /// <summary>
    /// 时区
    /// </summary>
    [StringLength(50, ErrorMessage = "时区长度不能超过50个字符")]
    public string? Timezone { get; set; }

    /// <summary>
    /// 邮件通知设置
    /// </summary>
    public EmailNotificationPreference? EmailPreferences { get; set; }

    /// <summary>
    /// 推送通知设置
    /// </summary>
    public PushNotificationPreference? PushPreferences { get; set; }

    /// <summary>
    /// 短信通知设置
    /// </summary>
    public SmsNotificationPreference? SmsPreferences { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知渠道设置请求DTO
/// </summary>
public class NotificationChannelRequest
{
    /// <summary>
    /// 通知渠道
    /// </summary>
    [Required(ErrorMessage = "通知渠道不能为空")]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 发送时间配置
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 免打扰设置请求DTO
/// </summary>
public class DoNotDisturbRequest
{
    /// <summary>
    /// 是否启用免打扰
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// 适用星期几（0=周日，1=周一...6=周六）
    /// </summary>
    public List<int>? DaysOfWeek { get; set; }

    /// <summary>
    /// 允许的高优先级通知
    /// </summary>
    public bool AllowHighPriority { get; set; } = true;

    /// <summary>
    /// 免打扰结束时间
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量创建通知请求DTO
/// </summary>
public class BulkCreateNotificationRequest
{
    /// <summary>
    /// 通知列表
    /// </summary>
    [Required(ErrorMessage = "通知列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(100, ErrorMessage = "一次最多创建100个通知")]
    public List<CreateNotificationDto> Notifications { get; set; } = new();

    /// <summary>
    /// 是否并行处理
    /// </summary>
    public bool ProcessInParallel { get; set; } = true;

    /// <summary>
    /// 失败时继续
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量更新通知请求DTO
/// </summary>
public class BulkUpdateNotificationRequest
{
    /// <summary>
    /// 通知更新列表
    /// </summary>
    [Required(ErrorMessage = "通知更新列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知更新")]
    [MaxLength(100, ErrorMessage = "一次最多更新100个通知")]
    public List<BulkUpdateNotificationItem> Updates { get; set; } = new();

    /// <summary>
    /// 是否并行处理
    /// </summary>
    public bool ProcessInParallel { get; set; } = true;

    /// <summary>
    /// 失败时继续
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量更新通知项DTO
/// </summary>
public class BulkUpdateNotificationItem
{
    /// <summary>
    /// 通知ID
    /// </summary>
    [Required(ErrorMessage = "通知ID不能为空")]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 更新数据
    /// </summary>
    [Required(ErrorMessage = "更新数据不能为空")]
    public UpdateNotificationDto UpdateData { get; set; } = new();
}

/// <summary>
/// 批量删除通知请求DTO
/// </summary>
public class BulkDeleteNotificationRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多删除1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 删除原因
    /// </summary>
    [StringLength(500, ErrorMessage = "删除原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 是否永久删除
    /// </summary>
    public bool PermanentDelete { get; set; } = false;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量标记已读通知请求DTO
/// </summary>
public class BulkMarkAsReadNotificationRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多标记1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 批量标记送达通知请求DTO
/// </summary>
public class BulkMarkAsDeliveredNotificationRequest
{
    /// <summary>
    /// 通知ID列表
    /// </summary>
    [Required(ErrorMessage = "通知ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知")]
    [MaxLength(1000, ErrorMessage = "一次最多标记1000个通知")]
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 送达时间
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// 送达渠道
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知类型筛选DTO
/// </summary>
public class NotificationTypeFilter
{
    /// <summary>
    /// 通知类型列表
    /// </summary>
    [Required(ErrorMessage = "通知类型列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个通知类型")]
    public List<NotificationType> Types { get; set; } = new();

    /// <summary>
    /// 排除模式
    /// </summary>
    public bool ExcludeMode { get; set; } = false;
}

/// <summary>
/// 通知优先级筛选DTO
/// </summary>
public class NotificationPriorityFilter
{
    /// <summary>
    /// 优先级列表
    /// </summary>
    [Required(ErrorMessage = "优先级列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个优先级")]
    public List<NotificationPriority> Priorities { get; set; } = new();

    /// <summary>
    /// 排除模式
    /// </summary>
    public bool ExcludeMode { get; set; } = false;
}

/// <summary>
/// 通知模板请求DTO
/// </summary>
public class NotificationTemplateRequest
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required(ErrorMessage = "模板名称不能为空")]
    [StringLength(100, ErrorMessage = "模板名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    [StringLength(500, ErrorMessage = "模板描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    [Required(ErrorMessage = "通知渠道不能为空")]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    [Required(ErrorMessage = "标题模板不能为空")]
    [StringLength(200, ErrorMessage = "标题模板长度不能超过200个字符")]
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 内容模板
    /// </summary>
    [Required(ErrorMessage = "内容模板不能为空")]
    [StringLength(2000, ErrorMessage = "内容模板长度不能超过2000个字符")]
    public string ContentTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<TemplateVariable> Variables { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 语言
    /// </summary>
    [StringLength(10, ErrorMessage = "语言代码长度不能超过10个字符")]
    public string? Language { get; set; } = "zh-CN";

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知模板响应DTO
/// </summary>
public class NotificationTemplateResponse
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 内容模板
    /// </summary>
    public string ContentTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<TemplateVariable> Variables { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; } = "zh-CN";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 发送模板化通知请求DTO
/// </summary>
public class SendTemplatedNotificationRequest
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [Required(ErrorMessage = "模板ID不能为空")]
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 接收者ID列表
    /// </summary>
    [Required(ErrorMessage = "接收者列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个接收者")]
    [MaxLength(1000, ErrorMessage = "接收者数量不能超过1000个")]
    public List<Guid> RecipientIds { get; set; } = new();

    /// <summary>
    /// 模板变量值
    /// </summary>
    public Dictionary<string, object> TemplateVariables { get; set; } = new();

    /// <summary>
    /// 通知优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 发送者ID
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 相关资源ID
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// 相关资源类型
    /// </summary>
    [StringLength(50, ErrorMessage = "资源类型长度不能超过50个字符")]
    public string? ResourceType { get; set; }

    /// <summary>
    /// 定时发送时间
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知订阅请求DTO
/// </summary>
public class NotificationSubscriptionRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 订阅类型
    /// </summary>
    [Required(ErrorMessage = "订阅类型不能为空")]
    public NotificationSubscriptionType SubscriptionType { get; set; }

    /// <summary>
    /// 订阅目标
    /// </summary>
    [StringLength(200, ErrorMessage = "订阅目标长度不能超过200个字符")]
    public string? Target { get; set; }

    /// <summary>
    /// 通知类型列表
    /// </summary>
    public List<NotificationType>? NotificationTypes { get; set; }

    /// <summary>
    /// 通知渠道列表
    /// </summary>
    public List<NotificationChannel>? Channels { get; set; }

    /// <summary>
    /// 过滤条件
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知订阅响应DTO
/// </summary>
public class NotificationSubscriptionResponse
{
    /// <summary>
    /// 订阅ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 订阅类型
    /// </summary>
    public NotificationSubscriptionType SubscriptionType { get; set; }

    /// <summary>
    /// 订阅目标
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// 通知类型列表
    /// </summary>
    public List<NotificationType> NotificationTypes { get; set; } = new();

    /// <summary>
    /// 通知渠道列表
    /// </summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 过滤条件
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 取消订阅请求DTO
/// </summary>
public class UnsubscribeRequest
{
    /// <summary>
    /// 订阅ID
    /// </summary>
    [Required(ErrorMessage = "订阅ID不能为空")]
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// 取消订阅原因
    /// </summary>
    [StringLength(500, ErrorMessage = "取消订阅原因长度不能超过500个字符")]
    public string? Reason { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 通知渠道响应DTO
/// </summary>
public class NotificationChannelResponse
{
    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// 发送时间配置
    /// </summary>
    public string? Schedule { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 免打扰设置响应DTO
/// </summary>
public class DoNotDisturbResponse
{
    /// <summary>
    /// 是否启用免打扰
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// 适用星期几
    /// </summary>
    public List<int>? DaysOfWeek { get; set; }

    /// <summary>
    /// 允许的高优先级通知
    /// </summary>
    public bool AllowHighPriority { get; set; }

    /// <summary>
    /// 免打扰结束时间
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 邮件通知偏好设置DTO
/// </summary>
public class EmailNotificationPreference
{
    /// <summary>
    /// 是否启用邮件通知
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 邮件地址
    /// </summary>
    [EmailAddress(ErrorMessage = "邮件地址格式不正确")]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// 邮件模板ID
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 发送频率
    /// </summary>
    public EmailFrequency Frequency { get; set; } = EmailFrequency.Immediate;

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 推送通知偏好设置DTO
/// </summary>
public class PushNotificationPreference
{
    /// <summary>
    /// 是否启用推送通知
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 设备令牌
    /// </summary>
    [StringLength(500, ErrorMessage = "设备令牌长度不能超过500个字符")]
    public string? DeviceToken { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceType? DeviceType { get; set; }

    /// <summary>
    /// 推送模板ID
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 短信通知偏好设置DTO
/// </summary>
public class SmsNotificationPreference
{
    /// <summary>
    /// 是否启用短信通知
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// 手机号码
    /// </summary>
    [StringLength(20, ErrorMessage = "手机号码长度不能超过20个字符")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 短信模板ID
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 自定义元数据
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// 模板变量DTO
/// </summary>
public class TemplateVariable
{
    /// <summary>
    /// 变量名
    /// </summary>
    [Required(ErrorMessage = "变量名不能为空")]
    [StringLength(50, ErrorMessage = "变量名长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量描述
    /// </summary>
    [StringLength(200, ErrorMessage = "变量描述长度不能超过200个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 变量类型
    /// </summary>
    [Required(ErrorMessage = "变量类型不能为空")]
    public TemplateVariableType Type { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 验证规则
    /// </summary>
    public string? ValidationRule { get; set; }

    /// <summary>
    /// 示例值
    /// </summary>
    public string? Example { get; set; }
}

/// <summary>
/// 通知操作枚举
/// </summary>
public enum NotificationOperation
{
    /// <summary>
    /// 标记已读
    /// </summary>
    MarkAsRead = 0,

    /// <summary>
    /// 标记未读
    /// </summary>
    MarkAsUnread = 1,

    /// <summary>
    /// 删除
    /// </summary>
    Delete = 2,

    /// <summary>
    /// 恢复
    /// </summary>
    Restore = 3,

    /// <summary>
    /// 归档
    /// </summary>
    Archive = 4,

    /// <summary>
    /// 取消归档
    /// </summary>
    Unarchive = 5,

    /// <summary>
    /// 标记重要
    /// </summary>
    MarkAsImportant = 6,

    /// <summary>
    /// 取消重要
    /// </summary>
    UnmarkAsImportant = 7
}

/// <summary>
/// 通知订阅类型枚举
/// </summary>
public enum NotificationSubscriptionType
{
    /// <summary>
    /// 用户订阅
    /// </summary>
    User = 0,

    /// <summary>
    /// 资源订阅
    /// </summary>
    Resource = 1,

    /// <summary>
    /// 系统订阅
    /// </summary>
    System = 2,

    /// <summary>
    /// 标签订阅
    /// </summary>
    Tag = 3
}

/// <summary>
/// 模板变量类型枚举
/// </summary>
public enum TemplateVariableType
{
    /// <summary>
    /// 字符串类型
    /// </summary>
    String = 0,

    /// <summary>
    /// 数字类型
    /// </summary>
    Number = 1,

    /// <summary>
    /// 布尔类型
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// 日期类型
    /// </summary>
    Date = 3,

    /// <summary>
    /// 时间类型
    /// </summary>
    DateTime = 4,

    /// <summary>
    /// 对象类型
    /// </summary>
    Object = 5,

    /// <summary>
    /// 数组类型
    /// </summary>
    Array = 6
}

/// <summary>
/// 设备类型枚举
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// iOS设备
    /// </summary>
    iOS = 0,

    /// <summary>
    /// Android设备
    /// </summary>
    Android = 1,

    /// <summary>
    /// Web设备
    /// </summary>
    Web = 2,

    /// <summary>
    /// 桌面设备
    /// </summary>
    Desktop = 3
}

/// <summary>
/// 批量操作结果DTO
/// </summary>
public class BulkNotificationOperationResult
{
    /// <summary>
    /// 总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// 成功的ID列表
    /// </summary>
    public List<Guid> SuccessfulIds { get; set; } = new();

    /// <summary>
    /// 失败的ID列表
    /// </summary>
    public List<Guid> FailedIds { get; set; } = new();

    /// <summary>
    /// 操作耗时（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// 通知搜索结果DTO
/// </summary>
public class NotificationSearchResult
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 高亮内容
    /// </summary>
    public string HighlightedContent { get; set; } = string.Empty;

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知优先级
    /// </summary>
    public NotificationPriority Priority { get; set; }

    /// <summary>
    /// 发送者用户名
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// 接收者用户名
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 搜索分数
    /// </summary>
    public double SearchScore { get; set; }

    /// <summary>
    /// 匹配的关键词
    /// </summary>
    public List<string> MatchedKeywords { get; set; } = new();

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool IsRead { get; set; }
}

/// <summary>
/// 通知搜索响应DTO
/// </summary>
public class NotificationSearchResponse
{
    /// <summary>
    /// 搜索结果列表
    /// </summary>
    public List<NotificationSearchResult> Results { get; set; } = new();

    /// <summary>
    /// 总结果数
    /// </summary>
    public int TotalResults { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string SearchKeyword { get; set; } = string.Empty;

    /// <summary>
    /// 搜索耗时（毫秒）
    /// </summary>
    public long SearchTimeMs { get; set; }

    /// <summary>
    /// 聚合统计
    /// </summary>
    public NotificationSearchAggregations? Aggregations { get; set; }
}

/// <summary>
/// 通知搜索聚合DTO
/// </summary>
public class NotificationSearchAggregations
{
    /// <summary>
    /// 按类型聚合
    /// </summary>
    public Dictionary<NotificationType, int> ByType { get; set; } = new();

    /// <summary>
    /// 按优先级聚合
    /// </summary>
    public Dictionary<NotificationPriority, int> ByPriority { get; set; } = new();

    /// <summary>
    /// 按状态聚合
    /// </summary>
    public Dictionary<NotificationStatus, int> ByStatus { get; set; } = new();

    /// <summary>
    /// 按发送者聚合
    /// </summary>
    public Dictionary<string, int> BySender { get; set; } = new();

    /// <summary>
    /// 按日期聚合
    /// </summary>
    public Dictionary<DateTime, int> ByDate { get; set; } = new();
}

/// <summary>
/// 通知搜索请求DTO
/// </summary>
public class NotificationSearchRequest
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    [Required(ErrorMessage = "搜索关键词不能为空")]
    [StringLength(200, ErrorMessage = "搜索关键词长度不能超过200个字符")]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// 接收者ID筛选
    /// </summary>
    public Guid? RecipientId { get; set; }

    /// <summary>
    /// 发送者ID筛选
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 通知类型筛选
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// 通知优先级筛选
    /// </summary>
    public NotificationPriority? Priority { get; set; }

    /// <summary>
    /// 通知状态筛选
    /// </summary>
    public NotificationStatus? Status { get; set; }

    /// <summary>
    /// 通知渠道筛选
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 标签筛选
    /// </summary>
    [StringLength(100, ErrorMessage = "标签长度不能超过100个字符")]
    public string? Tag { get; set; }

    /// <summary>
    /// 是否已读筛选
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// 是否已送达筛选
    /// </summary>
    public bool? IsDelivered { get; set; }

    /// <summary>
    /// 开始日期筛选
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期筛选
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 搜索范围
    /// </summary>
    public NotificationSearchScope SearchScope { get; set; } = NotificationSearchScope.All;

    /// <summary>
    /// 排序方式
    /// </summary>
    public NotificationSearchSort SortBy { get; set; } = NotificationSearchSort.Relevance;

    /// <summary>
    /// 页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 是否高亮匹配文本
    /// </summary>
    public bool HighlightMatches { get; set; } = true;

    /// <summary>
    /// 最大结果数
    /// </summary>
    [Range(1, 1000, ErrorMessage = "最大结果数必须在1-1000之间")]
    public int? MaxResults { get; set; }
}

/// <summary>
/// 通知搜索范围枚举
/// </summary>
public enum NotificationSearchScope
{
    /// <summary>
    /// 搜索所有内容
    /// </summary>
    All = 0,

    /// <summary>
    /// 仅搜索标题
    /// </summary>
    Title = 1,

    /// <summary>
    /// 仅搜索内容
    /// </summary>
    Content = 2,

    /// <summary>
    /// 仅搜索发送者
    /// </summary>
    Sender = 3,

    /// <summary>
    /// 搜索标题和内容
    /// </summary>
    TitleAndContent = 4
}

/// <summary>
/// 通知搜索排序枚举
/// </summary>
public enum NotificationSearchSort
{
    /// <summary>
    /// 按相关性排序
    /// </summary>
    Relevance = 0,

    /// <summary>
    /// 按创建时间降序
    /// </summary>
    CreatedAtDesc = 1,

    /// <summary>
    /// 按创建时间升序
    /// </summary>
    CreatedAtAsc = 2,

    /// <summary>
    /// 按优先级降序
    /// </summary>
    PriorityDesc = 3,

    /// <summary>
    /// 按优先级升序
    /// </summary>
    PriorityAsc = 4,

    /// <summary>
    /// 按未读状态优先排序
    /// </summary>
    UnreadFirst = 5
}