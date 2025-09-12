using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 通知服务接口 - 提供完整的通知管理功能，遵循单一职责和接口隔离原则
/// </summary>
public interface INotificationService
{
    // 通知创建和发送操作
    /// <summary>
    /// 创建并发送单个通知
    /// </summary>
    /// <param name="request">通知发送请求</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>创建的通知DTO</returns>
    Task<NotificationDto> CreateAndSendNotificationAsync(NotificationSendRequestDto request, Guid? senderId = null);

    /// <summary>
    /// 批量创建并发送通知
    /// </summary>
    /// <param name="requests">通知发送请求列表</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>批量发送结果</returns>
    Task<NotificationBatchSendResultDto> BatchCreateAndSendNotificationsAsync(IEnumerable<NotificationSendRequestDto> requests, Guid? senderId = null);

    /// <summary>
    /// 发送测试通知
    /// </summary>
    /// <param name="request">测试通知请求</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>测试通知结果</returns>
    Task<NotificationTestResultDto> SendTestNotificationAsync(TestNotificationDto request, Guid? senderId = null);

    /// <summary>
    /// 计划发送通知
    /// </summary>
    /// <param name="request">通知发送请求</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>计划的通知DTO</returns>
    Task<NotificationDto> ScheduleNotificationAsync(NotificationSendRequestDto request, Guid? senderId = null);

    /// <summary>
    /// 取消计划发送的通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="cancelReason">取消原因</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>是否成功取消</returns>
    Task<bool> CancelScheduledNotificationAsync(Guid notificationId, string cancelReason, Guid userId);

    // 通知管理操作
    /// <summary>
    /// 获取通知详情
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">当前用户ID</param>
    /// <returns>通知详情DTO</returns>
    Task<NotificationDto?> GetNotificationByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// 获取分页通知列表
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <param name="userId">当前用户ID</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<NotificationDto>> GetNotificationsAsync(NotificationFilterDto filter, Guid userId);

    /// <summary>
    /// 高级搜索通知
    /// </summary>
    /// <param name="filter">高级筛选条件</param>
    /// <param name="userId">当前用户ID</param>
    /// <returns>搜索结果</returns>
    Task<PaginatedResult<NotificationDto>> SearchNotificationsAsync(NotificationAdvancedFilterDto filter, Guid userId);

    /// <summary>
    /// 更新通知内容
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>更新后的通知DTO</returns>
    Task<NotificationDto> UpdateNotificationAsync(Guid id, UpdateNotificationDto request, Guid userId);

    /// <summary>
    /// 更新通知状态
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="request">状态更新请求</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>更新后的通知DTO</returns>
    Task<NotificationDto> UpdateNotificationStatusAsync(Guid id, NotificationStatusUpdateDto request, Guid userId);

    /// <summary>
    /// 标记通知为已读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> MarkAsReadAsync(Guid id, Guid userId);

    /// <summary>
    /// 标记通知为未读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> MarkAsUnreadAsync(Guid id, Guid userId);

    /// <summary>
    /// 确认通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ConfirmNotificationAsync(Guid id, Guid userId);

    /// <summary>
    /// 归档通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ArchiveNotificationAsync(Guid id, Guid userId);

    /// <summary>
    /// 取消归档通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> UnarchiveNotificationAsync(Guid id, Guid userId);

    /// <summary>
    /// 删除通知（软删除）
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteNotificationAsync(Guid id, Guid userId);

    /// <summary>
    /// 彻底删除通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> PermanentDeleteNotificationAsync(Guid id, Guid userId);

    // 批量通知管理操作
    /// <summary>
    /// 批量标记已读
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量操作结果</returns>
    Task<NotificationBatchOperationResultDto> BatchMarkAsReadAsync(BatchMarkAsReadDto request, Guid userId);

    /// <summary>
    /// 批量删除通知
    /// </summary>
    /// <param name="request">批量删除请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量操作结果</returns>
    Task<NotificationBatchOperationResultDto> BatchDeleteNotificationsAsync(BatchDeleteNotificationsDto request, Guid userId);

    /// <summary>
    /// 批量归档通知
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量操作结果</returns>
    Task<NotificationBatchOperationResultDto> BatchArchiveNotificationsAsync(List<Guid> notificationIds, Guid userId);

    /// <summary>
    /// 执行批量操作
    /// </summary>
    /// <param name="operation">批量操作参数</param>
    /// <param name="userId">用户ID</param>
    /// <returns>批量操作结果</returns>
    Task<NotificationBatchOperationResultDto> ExecuteBatchOperationAsync(NotificationBatchOperationDto operation, Guid userId);

    // 通知统计和摘要
    /// <summary>
    /// 获取用户通知统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知统计信息</returns>
    Task<NotificationStatsDto> GetNotificationStatsAsync(Guid userId);

    /// <summary>
    /// 获取系统通知统计信息
    /// </summary>
    /// <returns>系统通知统计信息</returns>
    Task<SystemNotificationStatsDto> GetSystemNotificationStatsAsync();

    /// <summary>
    /// 获取用户通知摘要
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知摘要</returns>
    Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId);

    /// <summary>
    /// 获取未读通知数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>未读通知数量</returns>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// 获取最近通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">数量</param>
    /// <returns>最近通知列表</returns>
    Task<IEnumerable<NotificationDto>> GetRecentNotificationsAsync(Guid userId, int count = 10);

    /// <summary>
    /// 获取高优先级通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">数量</param>
    /// <returns>高优先级通知列表</returns>
    Task<IEnumerable<NotificationDto>> GetHighPriorityNotificationsAsync(Guid userId, int count = 5);

    /// <summary>
    /// 获取需要确认的通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>需要确认的通知列表</returns>
    Task<IEnumerable<NotificationDto>> GetConfirmationRequiredNotificationsAsync(Guid userId);

    // 通知设置管理操作
    /// <summary>
    /// 获取用户通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户通知设置列表</returns>
    Task<IEnumerable<NotificationSettingDto>> GetUserNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 获取用户默认通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>默认通知设置</returns>
    Task<NotificationSettingDto?> GetUserDefaultNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 获取用户特定类型的通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>通知设置</returns>
    Task<NotificationSettingDto?> GetUserNotificationSettingForTypeAsync(Guid userId, NotificationType notificationType);

    /// <summary>
    /// 创建通知设置
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的通知设置</returns>
    Task<NotificationSettingDto> CreateNotificationSettingAsync(CreateNotificationSettingDto request, Guid userId);

    /// <summary>
    /// 更新通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的通知设置</returns>
    Task<NotificationSettingDto> UpdateNotificationSettingAsync(Guid id, UpdateNotificationSettingDto request, Guid userId);

    /// <summary>
    /// 删除通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteNotificationSettingAsync(Guid id, Guid userId);

    /// <summary>
    /// 设置默认通知设置
    /// </summary>
    /// <param name="settingId">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> SetDefaultNotificationSettingAsync(Guid settingId, Guid userId);

    /// <summary>
    /// 初始化用户默认通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的默认设置列表</returns>
    Task<IEnumerable<NotificationSettingDto>> InitializeDefaultNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 检查用户是否启用特定类型的通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationType">通知类型</param>
    /// <param name="channel">通知渠道</param>
    /// <returns>是否启用</returns>
    Task<bool> IsNotificationEnabledAsync(Guid userId, NotificationType notificationType, NotificationChannel channel);

    /// <summary>
    /// 更新用户免打扰设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="enable">是否启用</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateQuietHoursAsync(Guid userId, bool enable, TimeSpan? startTime, TimeSpan? endTime);

    /// <summary>
    /// 更新用户通知频率设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="frequency">通知频率</param>
    /// <param name="notificationType">通知类型</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateNotificationFrequencyAsync(Guid userId, NotificationFrequency frequency, NotificationType? notificationType = null);

    // 实时通知支持操作
    /// <summary>
    /// 建立通知连接
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="connectionId">连接ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ConnectNotificationAsync(Guid userId, string connectionId);

    /// <summary>
    /// 断开通知连接
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="connectionId">连接ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DisconnectNotificationAsync(Guid userId, string connectionId);

    /// <summary>
    /// 获取用户的通知连接状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>连接状态信息</returns>
    Task<NotificationConnectionStatusDto> GetConnectionStatusAsync(Guid userId);

    /// <summary>
    /// 发送实时通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notification">通知数据</param>
    /// <returns>发送结果</returns>
    Task<NotificationRealtimeResultDto> SendRealtimeNotificationAsync(Guid userId, NotificationDto notification);

    /// <summary>
    /// 广播系统通知
    /// </summary>
    /// <param name="notification">通知数据</param>
    /// <param name="targetUsers">目标用户列表（null表示所有用户）</param>
    /// <returns>广播结果</returns>
    Task<NotificationBroadcastResultDto> BroadcastSystemNotificationAsync(NotificationDto notification, IEnumerable<Guid>? targetUsers = null);

    /// <summary>
    /// 发送实时通知状态更新
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notificationId">通知ID</param>
    /// <param name="status">新状态</param>
    /// <returns>发送结果</returns>
    Task<bool> SendNotificationStatusUpdateAsync(Guid userId, Guid notificationId, NotificationStatus status);

    /// <summary>
    /// 发送未读计数更新
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">未读数量</param>
    /// <returns>发送结果</returns>
    Task<bool> SendUnreadCountUpdateAsync(Guid userId, int count);

    // 订阅管理
    /// <summary>
    /// 创建通知订阅
    /// </summary>
    /// <param name="subscription">订阅信息</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的订阅</returns>
    Task<NotificationSubscriptionDto> CreateSubscriptionAsync(NotificationSubscriptionDto subscription, Guid userId);

    /// <summary>
    /// 更新通知订阅
    /// </summary>
    /// <param name="subscriptionId">订阅ID</param>
    /// <param name="subscription">订阅信息</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的订阅</returns>
    Task<NotificationSubscriptionDto> UpdateSubscriptionAsync(Guid subscriptionId, NotificationSubscriptionDto subscription, Guid userId);

    /// <summary>
    /// 删除通知订阅
    /// </summary>
    /// <param name="subscriptionId">订阅ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteSubscriptionAsync(Guid subscriptionId, Guid userId);

    /// <summary>
    /// 获取用户的通知订阅
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户订阅列表</returns>
    Task<IEnumerable<NotificationSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId);

    /// <summary>
    /// 处理订阅通知
    /// </summary>
    /// <param name="notification">通知数据</param>
    /// <returns>处理结果</returns>
    Task<NotificationSubscriptionResultDto> ProcessSubscriptionNotificationsAsync(NotificationDto notification);

    // 通知模板管理
    /// <summary>
    /// 获取通知模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>通知模板</returns>
    Task<NotificationTemplateDto?> GetNotificationTemplateAsync(Guid templateId);

    /// <summary>
    /// 获取通知模板列表
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>通知模板列表</returns>
    Task<IEnumerable<NotificationTemplateDto>> GetNotificationTemplatesAsync(NotificationTemplateFilterDto filter);

    /// <summary>
    /// 使用模板发送通知
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="userId">目标用户ID</param>
    /// <param name="data">模板数据</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>发送结果</returns>
    Task<NotificationDto> SendTemplateNotificationAsync(Guid templateId, Guid userId, Dictionary<string, object> data, Guid? senderId = null);

    // 数据导入导出
    /// <summary>
    /// 导出通知数据
    /// </summary>
    /// <param name="options">导出选项</param>
    /// <param name="userId">用户ID</param>
    /// <returns>导出结果</returns>
    Task<NotificationExportResultDto> ExportNotificationsAsync(NotificationExportOptionsDto options, Guid userId);

    /// <summary>
    /// 导入通知数据
    /// </summary>
    /// <param name="data">导入数据</param>
    /// <param name="userId">用户ID</param>
    /// <returns>导入结果</returns>
    Task<NotificationImportResultDto> ImportNotificationsAsync(NotificationImportDataDto data, Guid userId);

    /// <summary>
    /// 导出通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置导出数据</returns>
    Task<IEnumerable<NotificationSettingDto>> ExportNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 导入通知设置
    /// </summary>
    /// <param name="settings">通知设置</param>
    /// <param name="userId">用户ID</param>
    /// <param name="overrideExisting">是否覆盖现有设置</param>
    /// <returns>导入结果</returns>
    Task<NotificationSettingsImportResultDto> ImportNotificationSettingsAsync(IEnumerable<NotificationSettingDto> settings, Guid userId, bool overrideExisting = false);

    // 缓存管理
    /// <summary>
    /// 清除用户通知缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ClearUserNotificationCacheAsync(Guid userId);

    /// <summary>
    /// 预加载用户通知数据
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>预加载结果</returns>
    Task<bool> PreloadUserNotificationsAsync(Guid userId);

    /// <summary>
    /// 刷新通知统计缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> RefreshNotificationStatsCacheAsync(Guid userId);

    // 权限验证
    /// <summary>
    /// 获取用户通知权限
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户权限</returns>
    Task<NotificationPermissionsDto> GetUserNotificationPermissionsAsync(Guid userId);

    /// <summary>
    /// 检查用户是否有权限执行操作
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="operation">操作类型</param>
    /// <param name="targetUserId">目标用户ID（可选）</param>
    /// <returns>是否有权限</returns>
    Task<bool> HasPermissionAsync(Guid userId, string operation, Guid? targetUserId = null);

    // 系统管理操作
    /// <summary>
    /// 获取系统通知队列状态
    /// </summary>
    /// <returns>队列状态</returns>
    Task<NotificationQueueStatusDto> GetNotificationQueueStatusAsync();

    /// <summary>
    /// 处理待发送通知
    /// </summary>
    /// <param name="batchSize">批次大小</param>
    /// <returns>处理结果</returns>
    Task<NotificationProcessingResultDto> ProcessPendingNotificationsAsync(int batchSize = 100);

    /// <summary>
    /// 清理过期通知
    /// </summary>
    /// <param name="beforeDate">清理日期</param>
    /// <returns>清理结果</returns>
    Task<NotificationCleanupResultDto> CleanExpiredNotificationsAsync(DateTime beforeDate);

    /// <summary>
    /// 获取系统通知设置
    /// </summary>
    /// <returns>系统通知设置</returns>
    Task<SystemNotificationSettingsDto> GetSystemNotificationSettingsAsync();

    /// <summary>
    /// 更新系统通知设置
    /// </summary>
    /// <param name="settings">系统通知设置</param>
    /// <returns>更新后的设置</returns>
    Task<SystemNotificationSettingsDto> UpdateSystemNotificationSettingsAsync(SystemNotificationSettingsDto settings);

    // 异常处理和重试
    /// <summary>
    /// 重试失败的通知发送
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>重试结果</returns>
    Task<NotificationRetryResultDto> RetryFailedNotificationAsync(Guid notificationId);

    /// <summary>
    /// 批量重试失败的通知
    /// </summary>
    /// <param name="beforeDate">重试日期</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>重试结果</returns>
    Task<NotificationBatchRetryResultDto> BatchRetryFailedNotificationsAsync(DateTime beforeDate, int maxRetries = 3);

    /// <summary>
    /// 获取通知发送日志
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>发送日志</returns>
    Task<IEnumerable<NotificationDeliveryHistoryDto>> GetNotificationDeliveryLogsAsync(Guid notificationId);

    // 性能监控
    /// <summary>
    /// 获取通知性能指标
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>性能指标</returns>
    Task<NotificationPerformanceMetricsDto> GetNotificationPerformanceMetricsAsync(TimeRangeDto timeRange);

    /// <summary>
    /// 获取通知发送统计
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>发送统计</returns>
    Task<NotificationSendStatsDto> GetNotificationSendStatsAsync(DateTime startDate, DateTime endDate);

    // 聚合和分析
    /// <summary>
    /// 获取通知聚合数据
    /// </summary>
    /// <param name="aggregation">聚合参数</param>
    /// <returns>聚合结果</returns>
    Task<NotificationAggregationResultDto> GetNotificationAggregationAsync(NotificationAggregationDto aggregation);

    /// <summary>
    /// 获取通知趋势分析
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <param name="groupBy">分组字段</param>
    /// <returns>趋势分析</returns>
    Task<NotificationTrendAnalysisDto> GetNotificationTrendAnalysisAsync(TimeRangeDto timeRange, NotificationAggregationField groupBy);

    // 通知设置管理
    /// <summary>
    /// 获取用户通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置列表</returns>
    Task<IEnumerable<NotificationSettingDto>> GetNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 创建通知设置
    /// </summary>
    /// <param name="request">创建设置请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>创建的通知设置</returns>
    Task<NotificationSettingDto> CreateNotificationSettingAsync(CreateNotificationSettingDto request, Guid userId);

    /// <summary>
    /// 更新通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="userId">用户ID</param>
    /// <returns>更新后的通知设置</returns>
    Task<NotificationSettingDto> UpdateNotificationSettingAsync(Guid id, UpdateNotificationSettingDto request, Guid userId);

    /// <summary>
    /// 删除通知设置
    /// </summary>
    /// <param name="id">设置ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteNotificationSettingAsync(Guid id, Guid userId);

    // 通知发送历史管理
    /// <summary>
    /// 获取通知发送历史
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="userId">用户ID</param>
    /// <returns>分页的发送历史</returns>
    Task<PaginatedResult<NotificationDeliveryHistoryDto>> GetNotificationDeliveryHistoryAsync(Guid notificationId, int page, int pageSize, Guid userId);
}

// 支持的DTO类型定义
/// <summary>
/// 通知批量发送结果DTO
/// </summary>
public class NotificationBatchSendResultDto
{
    /// <summary>
    /// 总请求数量
    /// </summary>
    public int TotalRequests { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 跳过发送数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 创建的通知ID列表
    /// </summary>
    public List<Guid> CreatedNotificationIds { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public Dictionary<Guid, string> Errors { get; set; } = new();

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 发送状态
    /// </summary>
    public NotificationBatchSendStatus Status { get; set; }
}

/// <summary>
/// 通知批量发送状态枚举
/// </summary>
public enum NotificationBatchSendStatus
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 部分成功
    /// </summary>
    PartialSuccess = 1,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 3
}

/// <summary>
/// 通知测试结果DTO
/// </summary>
public class NotificationTestResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid? NotificationId { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 发送详情
    /// </summary>
    public Dictionary<NotificationChannel, NotificationChannelTestResult> ChannelResults { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }
}

/// <summary>
/// 通知渠道测试结果
/// </summary>
public class NotificationChannelTestResult
{
    /// <summary>
    /// 渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 接收地址
    /// </summary>
    public string? RecipientAddress { get; set; }
}

/// <summary>
/// 通知批量操作结果DTO
/// </summary>
public class NotificationBatchOperationResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 处理的总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功的数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败的数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 跳过的数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public Dictionary<Guid, string> Errors { get; set; } = new();

    /// <summary>
    /// 操作结果详情
    /// </summary>
    public Dictionary<Guid, NotificationOperationResult> OperationResults { get; set; } = new();

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public NotificationBatchOperationType OperationType { get; set; }
}

/// <summary>
/// 通知操作结果
/// </summary>
public class NotificationOperationResult
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 更新后的状态
    /// </summary>
    public NotificationStatus? NewStatus { get; set; }
}

/// <summary>
/// 系统通知统计信息DTO
/// </summary>
public class SystemNotificationStatsDto
{
    /// <summary>
    /// 总通知数量
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// 今日通知数量
    /// </summary>
    public int TodayNotifications { get; set; }

    /// <summary>
    /// 本周通知数量
    /// </summary>
    public int ThisWeekNotifications { get; set; }

    /// <summary>
    /// 本月通知数量
    /// </summary>
    public int ThisMonthNotifications { get; set; }

    /// <summary>
    /// 未读通知数量
    /// </summary>
    public int UnreadNotifications { get; set; }

    /// <summary>
    /// 发送失败数量
    /// </summary>
    public int FailedNotifications { get; set; }

    /// <summary>
    /// 待发送通知数量
    /// </summary>
    public int PendingNotifications { get; set; }

    /// <summary>
    /// 按类型统计
    /// </summary>
    public Dictionary<NotificationType, int> TypeStats { get; set; } = new();

    /// <summary>
    /// 按渠道统计
    /// </summary>
    public Dictionary<NotificationChannel, int> ChannelStats { get; set; } = new();

    /// <summary>
    /// 按状态统计
    /// </summary>
    public Dictionary<NotificationStatus, int> StatusStats { get; set; } = new();

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 通知连接状态DTO
/// </summary>
public class NotificationConnectionStatusDto
{
    /// <summary>
    /// 是否连接
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime? ConnectedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public NotificationConnectionState State { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 通知连接状态枚举
/// </summary>
public enum NotificationConnectionState
{
    /// <summary>
    /// 已连接
    /// </summary>
    Connected = 0,

    /// <summary>
    /// 连接中
    /// </summary>
    Connecting = 1,

    /// <summary>
    /// 已断开
    /// </summary>
    Disconnected = 2,

    /// <summary>
    /// 重连中
    /// </summary>
    Reconnecting = 3,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 4
}

/// <summary>
/// 通知实时发送结果DTO
/// </summary>
public class NotificationRealtimeResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 通知广播结果DTO
/// </summary>
public class NotificationBroadcastResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 目标用户数量
    /// </summary>
    public int TargetUsersCount { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 发送结果详情
    /// </summary>
    public Dictionary<Guid, NotificationRealtimeResultDto> Results { get; set; } = new();

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 通知订阅结果DTO
/// </summary>
public class NotificationSubscriptionResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 处理的订阅数量
    /// </summary>
    public int ProcessedSubscriptions { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 订阅结果详情
    /// </summary>
    public Dictionary<Guid, NotificationRealtimeResultDto> SubscriptionResults { get; set; } = new();

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 通知模板DTO
/// </summary>
public class NotificationTemplateDto
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
    public NotificationType Type { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 内容模板
    /// </summary>
    public string? ContentTemplate { get; set; }

    /// <summary>
    /// 消息模板
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 支持的渠道
    /// </summary>
    public List<NotificationChannel> SupportedChannels { get; set; } = new();

    /// <summary>
    /// 默认优先级
    /// </summary>
    public NotificationPriority DefaultPriority { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<string> TemplateVariables { get; set; } = new();

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// 通知模板筛选条件DTO
/// </summary>
public class NotificationTemplateFilterDto
{
    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 支持的渠道
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool? RequiresConfirmation { get; set; }
}

/// <summary>
/// 通知导出结果DTO
/// </summary>
public class NotificationExportResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 导出的通知数量
    /// </summary>
    public int ExportedCount { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 下载URL
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 导出格式
    /// </summary>
    public ExportFormat Format { get; set; }

    /// <summary>
    /// 导出时间
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 通知导入结果DTO
/// </summary>
public class NotificationImportResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 导入的通知数量
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// 跳过的数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 失败的数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 创建的通知ID列表
    /// </summary>
    public List<Guid> CreatedNotificationIds { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public Dictionary<int, string> Errors { get; set; } = new();

    /// <summary>
    /// 验证报告
    /// </summary>
    public string? ValidationReport { get; set; }

    /// <summary>
    /// 导入时间
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 通知设置导入结果DTO
/// </summary>
public class NotificationSettingsImportResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 导入的设置数量
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// 更新的设置数量
    /// </summary>
    public int UpdatedCount { get; set; }

    /// <summary>
    /// 跳过的数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 创建的设置ID列表
    /// </summary>
    public List<Guid> CreatedSettingIds { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public Dictionary<int, string> Errors { get; set; } = new();

    /// <summary>
    /// 导入时间
    /// </summary>
    public DateTime ImportedAt { get; set; }

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 通知队列状态DTO
/// </summary>
public class NotificationQueueStatusDto
{
    /// <summary>
    /// 待发送通知数量
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 处理中通知数量
    /// </summary>
    public int ProcessingCount { get; set; }

    /// <summary>
    /// 失败通知数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 计划发送通知数量
    /// </summary>
    public int ScheduledCount { get; set; }

    /// <summary>
    /// 队列状态
    /// </summary>
    public NotificationQueueState State { get; set; }

    /// <summary>
    /// 最后处理时间
    /// </summary>
    public DateTime? LastProcessedAt { get; set; }

    /// <summary>
    /// 处理速率（每秒）
    /// </summary>
    public double ProcessingRate { get; set; }

    /// <summary>
    /// 平均处理时间（毫秒）
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 通知队列状态枚举
/// </summary>
public enum NotificationQueueState
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 繁忙
    /// </summary>
    Busy = 1,

    /// <summary>
    /// 停止
    /// </summary>
    Stopped = 2,

    /// <summary>
    /// 错误
    /// </summary>
    Error = 3
}

/// <summary>
/// 通知处理结果DTO
/// </summary>
public class NotificationProcessingResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 处理的通知数量
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 跳过数量
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 重试数量
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 处理结果详情
    /// </summary>
    public Dictionary<Guid, NotificationChannelProcessResult> ChannelResults { get; set; } = new();
}

/// <summary>
/// 通知渠道处理结果
/// </summary>
public class NotificationChannelProcessResult
{
    /// <summary>
    /// 渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 平均处理时间（毫秒）
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// 通知清理结果DTO
/// </summary>
public class NotificationCleanupResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 清理的通知数量
    /// </summary>
    public int CleanedCount { get; set; }

    /// <summary>
    /// 释放的空间（字节）
    /// </summary>
    public long FreedSpace { get; set; }

    /// <summary>
    /// 清理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 系统通知设置DTO
/// </summary>
public class SystemNotificationSettingsDto
{
    /// <summary>
    /// 系统通知是否启用
    /// </summary>
    public bool SystemNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// 默认通知渠道
    /// </summary>
    public List<NotificationChannel> DefaultChannels { get; set; } = new();

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 通知过期时间（天）
    /// </summary>
    public int NotificationExpirationDays { get; set; } = 30;

    /// <summary>
    /// 批量处理大小
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 处理间隔（秒）
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 启用的通知类型
    /// </summary>
    public List<NotificationType> EnabledNotificationTypes { get; set; } = new();

    /// <summary>
    /// 是否启用实时通知
    /// </summary>
    public bool RealtimeNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用通知模板
    /// </summary>
    public bool TemplatesEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用通知订阅
    /// </summary>
    public bool SubscriptionsEnabled { get; set; } = true;

    /// <summary>
    /// 是否启用通知缓存
    /// </summary>
    public bool CacheEnabled { get; set; } = true;

    /// <summary>
    /// 缓存过期时间（分钟）
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool PerformanceMonitoringEnabled { get; set; } = true;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 通知重试结果DTO
/// </summary>
public class NotificationRetryResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryAttempt { get; set; }

    /// <summary>
    /// 重试结果
    /// </summary>
    public Dictionary<NotificationChannel, NotificationChannelRetryResult> ChannelResults { get; set; } = new();

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 重试时间（毫秒）
    /// </summary>
    public long RetryTimeMs { get; set; }
}

/// <summary>
/// 通知渠道重试结果
/// </summary>
public class NotificationChannelRetryResult
{
    /// <summary>
    /// 渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 重试时间（毫秒）
    /// </summary>
    public long RetryTimeMs { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryAttempt { get; set; }
}

/// <summary>
/// 通知批量重试结果DTO
/// </summary>
public class NotificationBatchRetryResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 重试的通知数量
    /// </summary>
    public int RetriedCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 重试结果详情
    /// </summary>
    public Dictionary<Guid, NotificationRetryResultDto> RetryResults { get; set; } = new();

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 时间范围DTO
/// </summary>
public class TimeRangeDto
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// 时间范围类型
    /// </summary>
    public TimeRangeType Type { get; set; } = TimeRangeType.Custom;
}

/// <summary>
/// 时间范围类型枚举
/// </summary>
public enum TimeRangeType
{
    /// <summary>
    /// 自定义
    /// </summary>
    Custom = 0,

    /// <summary>
    /// 今日
    /// </summary>
    Today = 1,

    /// <summary>
    /// 昨日
    /// </summary>
    Yesterday = 2,

    /// <summary>
    /// 本周
    /// </summary>
    ThisWeek = 3,

    /// <summary>
    /// 上周
    /// </summary>
    LastWeek = 4,

    /// <summary>
    /// 本月
    /// </summary>
    ThisMonth = 5,

    /// <summary>
    /// 上月
    /// </summary>
    LastMonth = 6,

    /// <summary>
    /// 本年
    /// </summary>
    ThisYear = 7,

    /// <summary>
    /// 上年
    /// </summary>
    LastYear = 8,

    /// <summary>
    /// 最近7天
    /// </summary>
    Last7Days = 9,

    /// <summary>
    /// 最近30天
    /// </summary>
    Last30Days = 10,

    /// <summary>
    /// 最近90天
    /// </summary>
    Last90Days = 11
}

/// <summary>
/// 通知性能指标DTO
/// </summary>
public class NotificationPerformanceMetricsDto
{
    /// <summary>
    /// 总通知数量
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }

    /// <summary>
    /// 最小发送时间（毫秒）
    /// </summary>
    public double MinSendTimeMs { get; set; }

    /// <summary>
    /// 最大发送时间（毫秒）
    /// </summary>
    public double MaxSendTimeMs { get; set; }

    /// <summary>
    /// P95发送时间（毫秒）
    /// </summary>
    public double P95SendTimeMs { get; set; }

    /// <summary>
    /// P99发送时间（毫秒）
    /// </summary>
    public double P99SendTimeMs { get; set; }

    /// <summary>
    /// 按渠道统计的性能指标
    /// </summary>
    public Dictionary<NotificationChannel, NotificationChannelPerformanceMetrics> ChannelMetrics { get; set; } = new();

    /// <summary>
    /// 按类型统计的性能指标
    /// </summary>
    public Dictionary<NotificationType, NotificationTypePerformanceMetrics> TypeMetrics { get; set; } = new();

    /// <summary>
    /// 时间序列数据
    /// </summary>
    public List<NotificationTimeSeriesData> TimeSeries { get; set; } = new();

    /// <summary>
    /// 统计时间范围
    /// </summary>
    public TimeRangeDto TimeRange { get; set; } = new();

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 通知渠道性能指标
/// </summary>
public class NotificationChannelPerformanceMetrics
{
    /// <summary>
    /// 渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 通知数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }

    /// <summary>
    /// 最小发送时间（毫秒）
    /// </summary>
    public double MinSendTimeMs { get; set; }

    /// <summary>
    /// 最大发送时间（毫秒）
    /// </summary>
    public double MaxSendTimeMs { get; set; }

    /// <summary>
    /// P95发送时间（毫秒）
    /// </summary>
    public double P95SendTimeMs { get; set; }

    /// <summary>
    /// P99发送时间（毫秒）
    /// </summary>
    public double P99SendTimeMs { get; set; }
}

/// <summary>
/// 通知类型性能指标
/// </summary>
public class NotificationTypePerformanceMetrics
{
    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }

    /// <summary>
    /// 最小发送时间（毫秒）
    /// </summary>
    public double MinSendTimeMs { get; set; }

    /// <summary>
    /// 最大发送时间（毫秒）
    /// </summary>
    public double MaxSendTimeMs { get; set; }

    /// <summary>
    /// P95发送时间（毫秒）
    /// </summary>
    public double P95SendTimeMs { get; set; }

    /// <summary>
    /// P99发送时间（毫秒）
    /// </summary>
    public double P99SendTimeMs { get; set; }
}

/// <summary>
/// 通知时间序列数据
/// </summary>
public class NotificationTimeSeriesData
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 通知数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }
}

/// <summary>
/// 通知发送统计DTO
/// </summary>
public class NotificationSendStatsDto
{
    /// <summary>
    /// 总发送数量
    /// </summary>
    public int TotalSent { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 按日期统计
    /// </summary>
    public Dictionary<DateTime, NotificationDailyStats> DailyStats { get; set; } = new();

    /// <summary>
    /// 按渠道统计
    /// </summary>
    public Dictionary<NotificationChannel, NotificationChannelStats> ChannelStats { get; set; } = new();

    /// <summary>
    /// 按类型统计
    /// </summary>
    public Dictionary<NotificationType, NotificationTypeStats> TypeStats { get; set; } = new();

    /// <summary>
    /// 按优先级统计
    /// </summary>
    public Dictionary<NotificationPriority, NotificationPriorityStats> PriorityStats { get; set; } = new();

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// 通知每日统计
/// </summary>
public class NotificationDailyStats
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 发送数量
    /// </summary>
    public int SentCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }
}

/// <summary>
/// 通知渠道统计
/// </summary>
public class NotificationChannelStats
{
    /// <summary>
    /// 渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 发送数量
    /// </summary>
    public int SentCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }
}

/// <summary>
/// 通知类型统计
/// </summary>
public class NotificationTypeStats
{
    /// <summary>
    /// 类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 发送数量
    /// </summary>
    public int SentCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }
}

/// <summary>
/// 通知优先级统计
/// </summary>
public class NotificationPriorityStats
{
    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority Priority { get; set; }

    /// <summary>
    /// 发送数量
    /// </summary>
    public int SentCount { get; set; }

    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTimeMs { get; set; }
}

/// <summary>
/// 通知聚合结果DTO
/// </summary>
public class NotificationAggregationResultDto
{
    /// <summary>
    /// 聚合类型
    /// </summary>
    public NotificationAggregationType AggregationType { get; set; }

    /// <summary>
    /// 聚合字段
    /// </summary>
    public NotificationAggregationField AggregationField { get; set; }

    /// <summary>
    /// 聚合结果
    /// </summary>
    public Dictionary<string, object> Results { get; set; } = new();

    /// <summary>
    /// 分组结果
    /// </summary>
    public Dictionary<string, NotificationGroupResult> Groups { get; set; } = new();

    /// <summary>
    /// 总计
    /// </summary>
    public object Total { get; set; } = null!;

    /// <summary>
    /// 统计时间范围
    /// </summary>
    public TimeRangeDto TimeRange { get; set; } = new();

    /// <summary>
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// 通知分组结果
/// </summary>
public class NotificationGroupResult
{
    /// <summary>
    /// 分组键
    /// </summary>
    public string GroupKey { get; set; } = string.Empty;

    /// <summary>
    /// 分组值
    /// </summary>
    public object GroupValue { get; set; } = null!;

    /// <summary>
    /// 计数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 求和
    /// </summary>
    public double Sum { get; set; }

    /// <summary>
    /// 平均值
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public double Max { get; set; }
}

/// <summary>
/// 通知趋势分析DTO
/// </summary>
public class NotificationTrendAnalysisDto
{
    /// <summary>
    /// 趋势数据
    /// </summary>
    public List<NotificationTrendData> TrendData { get; set; } = new();

    /// <summary>
    /// 趋势统计
    /// </summary>
    public NotificationTrendStatistics Statistics { get; set; } = new();

    /// <summary>
    /// 增长率
    /// </summary>
    public double GrowthRate { get; set; }

    /// <summary>
    /// 预测数据
    /// </summary>
    public List<NotificationTrendPrediction> Predictions { get; set; } = new();

    /// <summary>
    /// 异常点
    /// </summary>
    public List<NotificationTrendAnomaly> Anomalies { get; set; } = new();

    /// <summary>
    /// 时间范围
    /// </summary>
    public TimeRangeDto TimeRange { get; set; } = new();

    /// <summary>
    /// 分组字段
    /// </summary>
    public NotificationAggregationField GroupBy { get; set; }

    /// <summary>
    /// 分析时间
    /// </summary>
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// 通知趋势数据
/// </summary>
public class NotificationTrendData
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// 分组键
    /// </summary>
    public string? GroupKey { get; set; }

    /// <summary>
    /// 移动平均值
    /// </summary>
    public double? MovingAverage { get; set; }

    /// <summary>
    /// 趋势方向
    /// </summary>
    public TrendDirection TrendDirection { get; set; }

    /// <summary>
    /// 变化率
    /// </summary>
    public double ChangeRate { get; set; }
}

/// <summary>
/// 趋势方向枚举
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// 上升
    /// </summary>
    Up = 0,

    /// <summary>
    /// 下降
    /// </summary>
    Down = 1,

    /// <summary>
    /// 平稳
    /// </summary>
    Stable = 2
}

/// <summary>
/// 通知趋势统计
/// </summary>
public class NotificationTrendStatistics
{
    /// <summary>
    /// 平均值
    /// </summary>
    public double Mean { get; set; }

    /// <summary>
    /// 中位数
    /// </summary>
    public double Median { get; set; }

    /// <summary>
    /// 标准差
    /// </summary>
    public double StandardDeviation { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// 变异系数
    /// </summary>
    public double CoefficientOfVariation { get; set; }

    /// <summary>
    /// 百分位数P25
    /// </summary>
    public double P25 { get; set; }

    /// <summary>
    /// 百分位数P75
    /// </summary>
    public double P75 { get; set; }

    /// <summary>
    /// 百分位数P90
    /// </summary>
    public double P90 { get; set; }

    /// <summary>
    /// 百分位数P95
    /// </summary>
    public double P95 { get; set; }

    /// <summary>
    /// 百分位数P99
    /// </summary>
    public double P99 { get; set; }
}

/// <summary>
/// 通知趋势预测
/// </summary>
public class NotificationTrendPrediction
{
    /// <summary>
    /// 预测时间
    /// </summary>
    public DateTime PredictedAt { get; set; }

    /// <summary>
    /// 预测值
    /// </summary>
    public double PredictedValue { get; set; }

    /// <summary>
    /// 置信区间下限
    /// </summary>
    public double ConfidenceIntervalLower { get; set; }

    /// <summary>
    /// 置信区间上限
    /// </summary>
    public double ConfidenceIntervalUpper { get; set; }

    /// <summary>
    /// 置信度
    /// </summary>
    public double ConfidenceLevel { get; set; }

    /// <summary>
    /// 预测方法
    /// </summary>
    public string PredictionMethod { get; set; } = string.Empty;
}

/// <summary>
/// 通知趋势异常
/// </summary>
public class NotificationTrendAnomaly
{
    /// <summary>
    /// 异常时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 异常值
    /// </summary>
    public double AnomalyValue { get; set; }

    /// <summary>
    /// 预期值
    /// </summary>
    public double ExpectedValue { get; set; }

    /// <summary>
    /// 偏差程度
    /// </summary>
    public double Deviation { get; set; }

    /// <summary>
    /// 异常类型
    /// </summary>
    public AnomalyType AnomalyType { get; set; }

    /// <summary>
    /// 异常分数
    /// </summary>
    public double AnomalyScore { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 异常类型枚举
/// </summary>
public enum AnomalyType
{
    /// <summary>
    /// 突增
    /// </summary>
    Spike = 0,

    /// <summary>
    /// 突降
    /// </summary>
    Drop = 1,

    /// <summary>
    /// 异常值
    /// </summary>
    Outlier = 2,

    /// <summary>
    /// 趋势变化
    /// </summary>
    TrendChange = 3,

    /// <summary>
    /// 周期性异常
    /// </summary>
    SeasonalAnomaly = 4
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
/// 分页结果DTO基类
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 页大小
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
}