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
    Task<object> SendRealtimeNotificationAsync(Guid userId, NotificationDto notification);

    /// <summary>
    /// 广播系统通知
    /// </summary>
    /// <param name="notification">通知数据</param>
    /// <param name="targetUsers">目标用户列表（null表示所有用户）</param>
    /// <returns>广播结果</returns>
    Task<object> BroadcastSystemNotificationAsync(NotificationDto notification, IEnumerable<Guid>? targetUsers = null);

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
    Task<object> ProcessSubscriptionNotificationsAsync(NotificationDto notification);

    // 通知模板管理
    /// <summary>
    /// 获取通知模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>通知模板</returns>
    Task<object> GetNotificationTemplateAsync(Guid templateId);

    /// <summary>
    /// 获取通知模板列表
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>通知模板列表</returns>
    Task<object> GetNotificationTemplatesAsync(object filter);

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
    Task<object> ExportNotificationsAsync(object options, Guid userId);

    /// <summary>
    /// 导入通知数据
    /// </summary>
    /// <param name="data">导入数据</param>
    /// <param name="userId">用户ID</param>
    /// <returns>导入结果</returns>
    Task<object> ImportNotificationsAsync(object data, Guid userId);

    /// <summary>
    /// 导出通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置导出数据</returns>
    Task<object> ExportNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 导入通知设置
    /// </summary>
    /// <param name="settings">通知设置</param>
    /// <param name="userId">用户ID</param>
    /// <param name="overrideExisting">是否覆盖现有设置</param>
    /// <returns>导入结果</returns>
    Task<object> ImportNotificationSettingsAsync(object settings, Guid userId, bool overrideExisting = false);

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
    Task<object> GetNotificationQueueStatusAsync();

    /// <summary>
    /// 处理待发送通知
    /// </summary>
    /// <param name="batchSize">批次大小</param>
    /// <returns>处理结果</returns>
    Task<object> ProcessPendingNotificationsAsync(int batchSize = 100);

    /// <summary>
    /// 清理过期通知
    /// </summary>
    /// <param name="beforeDate">清理日期</param>
    /// <returns>清理结果</returns>
    Task<object> CleanExpiredNotificationsAsync(DateTime beforeDate);

    /// <summary>
    /// 获取系统通知设置
    /// </summary>
    /// <returns>系统通知设置</returns>
    Task<object> GetSystemNotificationSettingsAsync();

    /// <summary>
    /// 更新系统通知设置
    /// </summary>
    /// <param name="settings">系统通知设置</param>
    /// <returns>更新后的设置</returns>
    Task<object> UpdateSystemNotificationSettingsAsync(object settings);

    // 异常处理和重试
    /// <summary>
    /// 重试失败的通知发送
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>重试结果</returns>
    Task<object> RetryFailedNotificationAsync(Guid notificationId);

    /// <summary>
    /// 批量重试失败的通知
    /// </summary>
    /// <param name="beforeDate">重试日期</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>重试结果</returns>
    Task<object> BatchRetryFailedNotificationsAsync(DateTime beforeDate, int maxRetries = 3);

    /// <summary>
    /// 获取通知发送日志
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>发送日志</returns>
    Task<object> GetNotificationDeliveryLogsAsync(Guid notificationId);

    // 性能监控
    /// <summary>
    /// 获取通知性能指标
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>性能指标</returns>
    Task<object> GetNotificationPerformanceMetricsAsync(object timeRange);

    /// <summary>
    /// 获取通知发送统计
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <returns>发送统计</returns>
    Task<object> GetNotificationSendStatsAsync(DateTime startDate, DateTime endDate);

    // 聚合和分析
    /// <summary>
    /// 获取通知聚合数据
    /// </summary>
    /// <param name="aggregation">聚合参数</param>
    /// <returns>聚合结果</returns>
    Task<object> GetNotificationAggregationAsync(object aggregation);

    /// <summary>
    /// 获取通知趋势分析
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <param name="groupBy">分组字段</param>
    /// <returns>趋势分析</returns>
    Task<object> GetNotificationTrendAnalysisAsync(object timeRange, object groupBy);

    /// <summary>
    /// 获取通知设置
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知设置</returns>
    Task<NotificationSettingsDto> GetNotificationSettingsAsync(Guid userId);

    /// <summary>
    /// 获取通知发送历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>通知发送历史</returns>
    Task<PaginatedResult<NotificationDeliveryHistoryDto>> GetNotificationDeliveryHistoryAsync(Guid userId, NotificationDeliveryHistoryFilterDto filter);

  }