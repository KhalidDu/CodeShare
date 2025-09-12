using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知高级筛选条件DTO
/// </summary>
public class NotificationAdvancedFilterDto
{
    /// <summary>
    /// 用户ID列表
    /// </summary>
    public List<Guid>? UserIds { get; set; }

    /// <summary>
    /// 排除的用户ID列表
    /// </summary>
    public List<Guid>? ExcludeUserIds { get; set; }

    /// <summary>
    /// 通知类型列表
    /// </summary>
    public List<NotificationType>? Types { get; set; }

    /// <summary>
    /// 排除的通知类型列表
    /// </summary>
    public List<NotificationType>? ExcludeTypes { get; set; }

    /// <summary>
    /// 优先级列表
    /// </summary>
    public List<NotificationPriority>? Priorities { get; set; }

    /// <summary>
    /// 状态列表
    /// </summary>
    public List<NotificationStatus>? Statuses { get; set; }

    /// <summary>
    /// 通知渠道列表
    /// </summary>
    public List<NotificationChannel>? Channels { get; set; }

    /// <summary>
    /// 相关实体类型列表
    /// </summary>
    public List<RelatedEntityType>? RelatedEntityTypes { get; set; }

    /// <summary>
    /// 相关实体ID列表
    /// </summary>
    public List<string>? RelatedEntityIds { get; set; }

    /// <summary>
    /// 触发用户ID列表
    /// </summary>
    public List<Guid>? TriggeredByUserIds { get; set; }

    /// <summary>
    /// 操作类型列表
    /// </summary>
    public List<NotificationAction>? Actions { get; set; }

    /// <summary>
    /// 标签列表
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// 排除的标签列表
    /// </summary>
    public List<string>? ExcludeTags { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// 是否已归档
    /// </summary>
    public bool? IsArchived { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool? IsDeleted { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool? RequiresConfirmation { get; set; }

    /// <summary>
    /// 是否过期
    /// </summary>
    public bool? IsExpired { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 过期开始日期
    /// </summary>
    public DateTime? ExpiresAfter { get; set; }

    /// <summary>
    /// 过期结束日期
    /// </summary>
    public DateTime? ExpiresBefore { get; set; }

    /// <summary>
    /// 创建开始日期
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// 创建结束日期
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// 更新开始日期
    /// </summary>
    public DateTime? UpdatedAfter { get; set; }

    /// <summary>
    /// 更新结束日期
    /// </summary>
    public DateTime? UpdatedBefore { get; set; }

    /// <summary>
    /// 阅读开始日期
    /// </summary>
    public DateTime? ReadAfter { get; set; }

    /// <summary>
    /// 阅读结束日期
    /// </summary>
    public DateTime? ReadBefore { get; set; }

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 搜索范围（标题、内容、消息）
    /// </summary>
    public List<NotificationSearchScope> SearchScopes { get; set; } = new();

    /// <summary>
    /// 排序字段
    /// </summary>
    public NotificationSortField SortField { get; set; } = NotificationSortField.CreatedAt;

    /// <summary>
    /// 排序方向
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 最大页大小
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// 是否包含已删除的通知
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// 是否包含已归档的通知
    /// </summary>
    public bool IncludeArchived { get; set; } = false;

    /// <summary>
    /// 是否包含关联的用户信息
    /// </summary>
    public bool IncludeUserInfo { get; set; } = false;

    /// <summary>
    /// 是否包含发送历史
    /// </summary>
    public bool IncludeDeliveryHistory { get; set; } = false;

    /// <summary>
    /// 是否只统计数量
    /// </summary>
    public bool CountOnly { get; set; } = false;
}

/// <summary>
/// 通知聚合查询参数DTO
/// </summary>
public class NotificationAggregationDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 聚合类型
    /// </summary>
    public NotificationAggregationType AggregationType { get; set; }

    /// <summary>
    /// 聚合字段
    /// </summary>
    public NotificationAggregationField AggregationField { get; set; }

    /// <summary>
    /// 分组字段
    /// </summary>
    public List<NotificationAggregationField> GroupByFields { get; set; } = new();

    /// <summary>
    /// 筛选条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 时间间隔（用于时间序列聚合）
    /// </summary>
    public TimeInterval? TimeInterval { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 限制数量
    /// </summary>
    public int? Limit { get; set; }
}

/// <summary>
/// 通知批量操作参数DTO
/// </summary>
public class NotificationBatchOperationDto
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public NotificationBatchOperationType OperationType { get; set; }

    /// <summary>
    /// 通知ID列表
    /// </summary>
    public List<Guid> NotificationIds { get; set; } = new();

    /// <summary>
    /// 筛选条件（用于批量操作所有匹配的通知）
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 操作参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 是否忽略错误
    /// </summary>
    public bool IgnoreErrors { get; set; } = false;

    /// <summary>
    /// 操作描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 通知订阅参数DTO
/// </summary>
public class NotificationSubscriptionDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 订阅类型
    /// </summary>
    public NotificationSubscriptionType SubscriptionType { get; set; }

    /// <summary>
    /// 订阅条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 订阅名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 订阅描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 订阅元数据
    /// </summary>
    public string? MetadataJson { get; set; }
}

/// <summary>
/// 通知发送请求DTO
/// </summary>
public class NotificationSendRequestDto
{
    /// <summary>
    /// 接收用户ID列表
    /// </summary>
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 通知消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 通知优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 通知渠道
    /// </summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 相关实体类型
    /// </summary>
    public RelatedEntityType? RelatedEntityType { get; set; }

    /// <summary>
    /// 相关实体ID
    /// </summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// 触发用户ID
    /// </summary>
    public Guid? TriggeredByUserId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public NotificationAction? Action { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 计划发送时间
    /// </summary>
    public DateTime? ScheduledToSendAt { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// 通知数据
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 是否立即发送
    /// </summary>
    public bool SendImmediately { get; set; } = true;

    /// <summary>
    /// 是否跳过用户设置检查
    /// </summary>
    public bool SkipUserSettingsCheck { get; set; } = false;

    /// <summary>
    /// 是否跳过免打扰检查
    /// </summary>
    public bool SkipQuietHoursCheck { get; set; } = false;

    /// <summary>
    /// 重试策略
    /// </summary>
    public NotificationRetryPolicy? RetryPolicy { get; set; }
}

/// <summary>
/// 通知重试策略DTO
/// </summary>
public class NotificationRetryPolicy
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 重试间隔（秒）
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// 指数退避因子
    /// </summary>
    public double ExponentialBackoffFactor { get; set; } = 2.0;

    /// <summary>
    /// 最大重试间隔（秒）
    /// </summary>
    public int MaxRetryIntervalSeconds { get; set; } = 3600;

    /// <summary>
    /// 是否只在特定错误时重试
    /// </summary>
    public bool RetryOnSpecificErrorsOnly { get; set; } = false;

    /// <summary>
    /// 可重试的错误类型
    /// </summary>
    public List<string> RetryableErrorTypes { get; set; } = new();
}

/// <summary>
/// 通知搜索范围枚举
/// </summary>
public enum NotificationSearchScope
{
    /// <summary>
    /// 搜索标题
    /// </summary>
    Title = 0,

    /// <summary>
    /// 搜索内容
    /// </summary>
    Content = 1,

    /// <summary>
    /// 搜索消息
    /// </summary>
    Message = 2,

    /// <summary>
    /// 搜索标签
    /// </summary>
    Tag = 3,

    /// <summary>
    /// 搜索所有文本字段
    /// </summary>
    AllText = 4
}

/// <summary>
/// 通知排序字段枚举
/// </summary>
public enum NotificationSortField
{
    /// <summary>
    /// 创建时间
    /// </summary>
    CreatedAt = 0,

    /// <summary>
    /// 更新时间
    /// </summary>
    UpdatedAt = 1,

    /// <summary>
    /// 优先级
    /// </summary>
    Priority = 2,

    /// <summary>
    /// 标题
    /// </summary>
    Title = 3,

    /// <summary>
    /// 通知类型
    /// </summary>
    Type = 4,

    /// <summary>
    /// 状态
    /// </summary>
    Status = 5,

    /// <summary>
    /// 阅读时间
    /// </summary>
    ReadAt = 6,

    /// <summary>
    /// 过期时间
    /// </summary>
    ExpiresAt = 7,

    /// <summary>
    /// 发送次数
    /// </summary>
    SendCount = 8,

    /// <summary>
    /// 最后发送时间
    /// </summary>
    LastSentAt = 9
}

/// <summary>
/// 排序方向枚举
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// 升序
    /// </summary>
    Ascending = 0,

    /// <summary>
    /// 降序
    /// </summary>
    Descending = 1
}

/// <summary>
/// 通知聚合类型枚举
/// </summary>
public enum NotificationAggregationType
{
    /// <summary>
    /// 计数
    /// </summary>
    Count = 0,

    /// <summary>
    /// 求和
    /// </summary>
    Sum = 1,

    /// <summary>
    /// 平均值
    /// </summary>
    Average = 2,

    /// <summary>
    /// 最大值
    /// </summary>
    Max = 3,

    /// <summary>
    /// 最小值
    /// </summary>
    Min = 4,

    /// <summary>
    /// 分组计数
    /// </summary>
    GroupCount = 5
}

/// <summary>
/// 通知聚合字段枚举
/// </summary>
public enum NotificationAggregationField
{
    /// <summary>
    /// 通知类型
    /// </summary>
    Type = 0,

    /// <summary>
    /// 优先级
    /// </summary>
    Priority = 1,

    /// <summary>
    /// 状态
    /// </summary>
    Status = 2,

    /// <summary>
    /// 渠道
    /// </summary>
    Channel = 3,

    /// <summary>
    /// 用户ID
    /// </summary>
    UserId = 4,

    /// <summary>
    /// 创建日期
    /// </summary>
    CreatedDate = 5,

    /// <summary>
    /// 创建月份
    /// </summary>
    CreatedMonth = 6,

    /// <summary>
    /// 创建年份
    /// </summary>
    CreatedYear = 7,

    /// <summary>
    /// 创建星期
    /// </summary>
    CreatedWeekday = 8,

    /// <summary>
    /// 创建小时
    /// </summary>
    CreatedHour = 9,

    /// <summary>
    /// 标签
    /// </summary>
    Tag = 10,

    /// <summary>
    /// 相关实体类型
    /// </summary>
    RelatedEntityType = 11,

    /// <summary>
    /// 操作类型
    /// </summary>
    Action = 12
}

/// <summary>
/// 时间间隔枚举
/// </summary>
public enum TimeInterval
{
    /// <summary>
    /// 小时
    /// </summary>
    Hour = 0,

    /// <summary>
    /// 天
    /// </summary>
    Day = 1,

    /// <summary>
    /// 周
    /// </summary>
    Week = 2,

    /// <summary>
    /// 月
    /// </summary>
    Month = 3,

    /// <summary>
    /// 年
    /// </summary>
    Year = 4
}

/// <summary>
/// 通知批量操作类型枚举
/// </summary>
public enum NotificationBatchOperationType
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
    /// 归档
    /// </summary>
    Archive = 2,

    /// <summary>
    /// 取消归档
    /// </summary>
    Unarchive = 3,

    /// <summary>
    /// 删除（软删除）
    /// </summary>
    SoftDelete = 4,

    /// <summary>
    /// 彻底删除
    /// </summary>
    PermanentDelete = 5,

    /// <summary>
    /// 确认
    /// </summary>
    Confirm = 6,

    /// <summary>
    /// 更新状态
    /// </summary>
    UpdateStatus = 7,

    /// <summary>
    /// 更新优先级
    /// </summary>
    UpdatePriority = 8,

    /// <summary>
    /// 设置过期时间
    /// </summary>
    SetExpiration = 9,

    /// <summary>
    /// 添加标签
    /// </summary>
    AddTag = 10,

    /// <summary>
    /// 移除标签
    /// </summary>
    RemoveTag = 11,

    /// <summary>
    /// 重新发送
    /// </summary>
    Resend = 12
}

/// <summary>
/// 通知订阅类型枚举
/// </summary>
public enum NotificationSubscriptionType
{
    /// <summary>
    /// 用户通知订阅
    /// </summary>
    UserNotifications = 0,

    /// <summary>
    /// 系统通知订阅
    /// </summary>
    SystemNotifications = 1,

    /// <summary>
    /// 实体通知订阅
    /// </summary>
    EntityNotifications = 2,

    /// <summary>
    /// 类型通知订阅
    /// </summary>
    TypeNotifications = 3,

    /// <summary>
    /// 高优先级通知订阅
    /// </summary>
    HighPriorityNotifications = 4,

    /// <summary>
    /// 自定义通知订阅
    /// </summary>
    CustomNotifications = 5
}

/// <summary>
/// 通知聚合结果DTO - 用于返回聚合查询结果
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
    /// 聚合结果值
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 分组字段
    /// </summary>
    public List<NotificationAggregationField> GroupByFields { get; set; } = new();

    /// <summary>
    /// 分组结果
    /// </summary>
    public List<NotificationAggregationGroupDto> Groups { get; set; } = new();

    /// <summary>
    /// 时间序列数据
    /// </summary>
    public List<NotificationTimeSeriesDataDto> TimeSeries { get; set; } = new();

    /// <summary>
    /// 筛选条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 时间间隔
    /// </summary>
    public TimeInterval? TimeInterval { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// 处理耗时
    /// </summary>
    public TimeSpan ProcessingDuration { get; set; }
}

/// <summary>
/// 通知聚合分组DTO
/// </summary>
public class NotificationAggregationGroupDto
{
    /// <summary>
    /// 分组键
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 分组标签
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 分组值
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 分组统计信息
    /// </summary>
    public Dictionary<string, decimal> Statistics { get; set; } = new();

    /// <summary>
    /// 子分组
    /// </summary>
    public List<NotificationAggregationGroupDto> SubGroups { get; set; } = new();
}

/// <summary>
/// 通知时间序列数据DTO
/// </summary>
public class NotificationTimeSeriesDataDto
{
    /// <summary>
    /// 时间点
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 数值
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// 通知订阅响应DTO - 用于返回订阅详情
/// </summary>
public class NotificationSubscriptionResponseDto
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
    /// 订阅类型
    /// </summary>
    public NotificationSubscriptionType SubscriptionType { get; set; }

    /// <summary>
    /// 订阅条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 订阅名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 订阅描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 订阅元数据
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最后触发时间
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    public int TriggerCount { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfoDto? User { get; set; }
}

/// <summary>
/// 通知订阅列表DTO - 用于返回订阅列表
/// </summary>
public class NotificationSubscriptionListDto
{
    /// <summary>
    /// 订阅列表
    /// </summary>
    public List<NotificationSubscriptionResponseDto> Subscriptions { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
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

/// <summary>
/// 通知订阅创建请求DTO
/// </summary>
public class NotificationSubscriptionCreateDto
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
    /// 订阅条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    [MinLength(1, ErrorMessage = "至少需要选择一个通知渠道")]
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>
    /// 订阅名称
    /// </summary>
    [StringLength(100, ErrorMessage = "订阅名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 订阅描述
    /// </summary>
    [StringLength(500, ErrorMessage = "订阅描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 订阅元数据
    /// </summary>
    [StringLength(2000, ErrorMessage = "订阅元数据长度不能超过2000个字符")]
    public string? MetadataJson { get; set; }
}

/// <summary>
/// 通知订阅更新请求DTO
/// </summary>
public class NotificationSubscriptionUpdateDto
{
    /// <summary>
    /// 订阅类型
    /// </summary>
    public NotificationSubscriptionType? SubscriptionType { get; set; }

    /// <summary>
    /// 订阅条件
    /// </summary>
    public NotificationFilterDto? Filter { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public List<NotificationChannel>? Channels { get; set; }

    /// <summary>
    /// 订阅名称
    /// </summary>
    [StringLength(100, ErrorMessage = "订阅名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 订阅描述
    /// </summary>
    [StringLength(500, ErrorMessage = "订阅描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 订阅元数据
    /// </summary>
    [StringLength(2000, ErrorMessage = "订阅元数据长度不能超过2000个字符")]
    public string? MetadataJson { get; set; }
}

/// <summary>
/// 用户信息DTO - 用于返回用户基本信息
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 头像URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime? LastActivityAt { get; set; }
}