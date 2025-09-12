using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 通知仓储接口 - 遵循单一职责原则，负责通知数据的访问和操作
/// </summary>
public interface INotificationRepository
{
    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>通知实体或null</returns>
    Task<Notification?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据ID获取通知及其关联用户信息
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>包含用户信息的通知实体或null</returns>
    Task<Notification?> GetByIdWithUserAsync(Guid id);

    /// <summary>
    /// 创建新通知
    /// </summary>
    /// <param name="notification">通知实体</param>
    /// <returns>创建的通知实体</returns>
    Task<Notification> CreateAsync(Notification notification);

    /// <summary>
    /// 更新通知
    /// </summary>
    /// <param name="notification">通知实体</param>
    /// <returns>更新后的通知实体</returns>
    Task<Notification> UpdateAsync(Notification notification);

    /// <summary>
    /// 删除通知（物理删除）
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// 软删除通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> SoftDeleteAsync(Guid id);

    // 分页查询
    /// <summary>
    /// 获取分页通知列表
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetPagedAsync(NotificationFilterDto filter);

    /// <summary>
    /// 根据用户ID获取通知列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByUserIdAsync(Guid userId, NotificationFilterDto filter);

    /// <summary>
    /// 根据通知类型获取通知列表
    /// </summary>
    /// <param name="type">通知类型</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByTypeAsync(NotificationType type, NotificationFilterDto filter);

    /// <summary>
    /// 根据相关实体获取通知列表
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByRelatedEntityAsync(RelatedEntityType entityType, string entityId, NotificationFilterDto filter);

    // 状态管理
    /// <summary>
    /// 根据状态获取通知列表
    /// </summary>
    /// <param name="status">通知状态</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByStatusAsync(NotificationStatus status, NotificationFilterDto filter);

    /// <summary>
    /// 获取未读通知列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetUnreadAsync(Guid userId, NotificationFilterDto filter);

    /// <summary>
    /// 获取已读通知列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetReadAsync(Guid userId, NotificationFilterDto filter);

    /// <summary>
    /// 获取待发送通知列表
    /// </summary>
    /// <param name="filter">筛选条件</param>
    /// <returns>待发送通知列表</returns>
    Task<IEnumerable<Notification>> GetPendingToSendAsync();

    /// <summary>
    /// 获取计划发送通知列表
    /// </summary>
    /// <param name="beforeTime">截止时间</param>
    /// <returns>计划发送通知列表</returns>
    Task<IEnumerable<Notification>> GetScheduledToSendAsync(DateTime beforeTime);

    // 搜索和筛选
    /// <summary>
    /// 搜索通知
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>匹配的通知列表</returns>
    Task<IEnumerable<Notification>> SearchAsync(string searchTerm, NotificationFilterDto filter);

    /// <summary>
    /// 根据标签获取通知
    /// </summary>
    /// <param name="tag">标签</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByTagAsync(string tag, NotificationFilterDto filter);

    /// <summary>
    /// 根据优先级获取通知
    /// </summary>
    /// <param name="priority">优先级</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByPriorityAsync(NotificationPriority priority, NotificationFilterDto filter);

    /// <summary>
    /// 根据时间范围获取通知
    /// </summary>
    /// <param name="startDate">开始时间</param>
    /// <param name="endDate">结束时间</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>分页通知结果</returns>
    Task<PaginatedResult<Notification>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, NotificationFilterDto filter);

    // 统计信息
    /// <summary>
    /// 获取用户通知统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知统计信息</returns>
    Task<NotificationStatsDto> GetStatsByUserIdAsync(Guid userId);

    /// <summary>
    /// 获取系统通知统计信息
    /// </summary>
    /// <returns>系统通知统计信息</returns>
    Task<NotificationStatsDto> GetSystemStatsAsync();

    /// <summary>
    /// 获取未读通知数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>未读通知数量</returns>
    Task<int> GetUnreadCountAsync(Guid userId);

    /// <summary>
    /// 获取通知数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <returns>通知数量</returns>
    Task<int> GetCountByFilterAsync(Guid userId, NotificationFilterDto filter);

    /// <summary>
    /// 获取最近通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">数量</param>
    /// <returns>最近通知列表</returns>
    Task<IEnumerable<Notification>> GetRecentNotificationsAsync(Guid userId, int count = 10);

    // 批量操作
    /// <summary>
    /// 批量创建通知
    /// </summary>
    /// <param name="notifications">通知列表</param>
    /// <returns>创建的通知数量</returns>
    Task<int> BulkCreateAsync(IEnumerable<Notification> notifications);

    /// <summary>
    /// 批量更新通知
    /// </summary>
    /// <param name="notifications">通知列表</param>
    /// <returns>更新的通知数量</returns>
    Task<int> BulkUpdateAsync(IEnumerable<Notification> notifications);

    /// <summary>
    /// 批量删除通知
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <returns>删除的通知数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> notificationIds);

    /// <summary>
    /// 批量标记已读
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <returns>操作是否成功</returns>
    Task<bool> BulkMarkAsReadAsync(IEnumerable<Guid> notificationIds);

    /// <summary>
    /// 批量标记未读
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <returns>操作是否成功</returns>
    Task<bool> BulkMarkAsUnreadAsync(IEnumerable<Guid> notificationIds);

    /// <summary>
    /// 批量归档通知
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <returns>操作是否成功</returns>
    Task<bool> BulkArchiveAsync(IEnumerable<Guid> notificationIds);

    /// <summary>
    /// 批量删除通知（软删除）
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <returns>操作是否成功</returns>
    Task<bool> BulkSoftDeleteAsync(IEnumerable<Guid> notificationIds);

    // 状态更新
    /// <summary>
    /// 标记通知为已读
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// 标记通知为未读
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> MarkAsUnreadAsync(Guid notificationId);

    /// <summary>
    /// 归档通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ArchiveAsync(Guid notificationId);

    /// <summary>
    /// 取消归档通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UnarchiveAsync(Guid notificationId);

    /// <summary>
    /// 确认通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ConfirmAsync(Guid notificationId);

    /// <summary>
    /// 更新通知状态
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="status">新状态</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateStatusAsync(Guid notificationId, NotificationStatus status);

    // 缓存相关
    /// <summary>
    /// 从缓存获取通知
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>通知实体或null</returns>
    Task<Notification?> GetFromCacheAsync(Guid id);

    /// <summary>
    /// 设置通知缓存
    /// </summary>
    /// <param name="notification">通知实体</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>操作是否成功</returns>
    Task<bool> SetCacheAsync(Notification notification, TimeSpan? expiration = null);

    /// <summary>
    /// 移除通知缓存
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> RemoveCacheAsync(Guid id);

    /// <summary>
    /// 清空用户通知缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> ClearUserCacheAsync(Guid userId);

    // 高级查询
    /// <summary>
    /// 获取用户的通知摘要
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>通知摘要</returns>
    Task<NotificationSummaryDto> GetSummaryAsync(Guid userId);

    /// <summary>
    /// 获取高优先级通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">数量</param>
    /// <returns>高优先级通知列表</returns>
    Task<IEnumerable<Notification>> GetHighPriorityNotificationsAsync(Guid userId, int count = 5);

    /// <summary>
    /// 获取紧急通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>紧急通知列表</returns>
    Task<IEnumerable<Notification>> GetUrgentNotificationsAsync(Guid userId);

    /// <summary>
    /// 获取需要确认的通知
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>需要确认的通知列表</returns>
    Task<IEnumerable<Notification>> GetConfirmationRequiredAsync(Guid userId);

    /// <summary>
    /// 获取过期通知
    /// </summary>
    /// <param name="beforeTime">过期时间</param>
    /// <returns>过期通知列表</returns>
    Task<IEnumerable<Notification>> GetExpiredNotificationsAsync(DateTime beforeTime);

    /// <summary>
    /// 清理过期通知
    /// </summary>
    /// <param name="beforeTime">过期时间</param>
    /// <returns>清理的通知数量</returns>
    Task<int> CleanExpiredNotificationsAsync(DateTime beforeTime);

    // 导入导出
    /// <summary>
    /// 导出通知数据
    /// </summary>
    /// <param name="options">导出选项</param>
    /// <returns>通知数据列表</returns>
    Task<IEnumerable<Notification>> ExportAsync(NotificationExportOptionsDto options);

    /// <summary>
    /// 导入通知数据
    /// </summary>
    /// <param name="notifications">通知数据列表</param>
    /// <param name="overrideExisting">是否覆盖现有数据</param>
    /// <returns>导入的通知数量</returns>
    Task<int> ImportAsync(IEnumerable<Notification> notifications, bool overrideExisting = false);

    // 权限验证
    /// <summary>
    /// 检查用户是否可以访问通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以访问</returns>
    Task<bool> CanUserAccessAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// 检查用户是否可以编辑通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以编辑</returns>
    Task<bool> CanUserEditAsync(Guid notificationId, Guid userId);

    /// <summary>
    /// 检查用户是否可以删除通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否可以删除</returns>
    Task<bool> CanUserDeleteAsync(Guid notificationId, Guid userId);

    // 事务相关
    /// <summary>
    /// 开始事务
    /// </summary>
    /// <returns>事务ID</returns>
    Task<string> BeginTransactionAsync();

    /// <summary>
    /// 提交事务
    /// </summary>
    /// <param name="transactionId">事务ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> CommitTransactionAsync(string transactionId);

    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <param name="transactionId">事务ID</param>
    /// <returns>操作是否成功</returns>
    Task<bool> RollbackTransactionAsync(string transactionId);

    // 性能优化
    /// <summary>
    /// 预加载通知数据
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">数量</param>
    /// <returns>预加载的通知列表</returns>
    Task<IEnumerable<Notification>> PreloadAsync(Guid userId, int count = 50);

    /// <summary>
    /// 获取通知数量（高性能版本）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="status">状态</param>
    /// <returns>通知数量</returns>
    Task<int> GetCountFastAsync(Guid userId, NotificationStatus? status = null);

    /// <summary>
    /// 批量更新通知状态（高性能版本）
    /// </summary>
    /// <param name="notificationIds">通知ID列表</param>
    /// <param name="status">新状态</param>
    /// <returns>更新的通知数量</returns>
    Task<int> BulkUpdateStatusFastAsync(IEnumerable<Guid> notificationIds, NotificationStatus status);
}