using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// WebSocket服务接口 - 提供实时通信和消息传递功能
/// </summary>
public interface IWebSocketService
{
    #region 连接管理操作

    /// <summary>
    /// 建立WebSocket连接
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="connectionId">连接ID</param>
    /// <param name="hubContext">SignalR Hub上下文</param>
    /// <returns>连接结果</returns>
    Task<WebSocketConnectionResultDto> ConnectAsync(Guid userId, string connectionId, IHubContext<NotificationHub> hubContext);

    /// <summary>
    /// 断开WebSocket连接
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="connectionId">连接ID</param>
    /// <returns>断开结果</returns>
    Task<WebSocketDisconnectionResultDto> DisconnectAsync(Guid userId, string connectionId);

    /// <summary>
    /// 获取用户连接状态
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>连接状态</returns>
    Task<WebSocketConnectionStatusDto> GetConnectionStatusAsync(Guid userId);

    /// <summary>
    /// 获取所有活跃连接
    /// </summary>
    /// <returns>活跃连接列表</returns>
    Task<IEnumerable<WebSocketConnectionInfoDto>> GetActiveConnectionsAsync();

    /// <summary>
    /// 检查用户是否在线
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否在线</returns>
    Task<bool> IsUserOnlineAsync(Guid userId);

    /// <summary>
    /// 获取用户的连接数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>连接数量</returns>
    Task<int> GetUserConnectionCountAsync(Guid userId);

    /// <summary>
    /// 强制断开用户连接
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="reason">断开原因</param>
    /// <returns>断开结果</returns>
    Task<WebSocketForceDisconnectResultDto> ForceDisconnectUserAsync(Guid userId, string reason);

    /// <summary>
    /// 清理过期连接
    /// </summary>
    /// <param name="timeoutMinutes">超时时间（分钟）</param>
    /// <returns>清理结果</returns>
    Task<WebSocketCleanupResultDto> CleanupExpiredConnectionsAsync(int timeoutMinutes = 30);

    #endregion

    #region 消息发送操作

    /// <summary>
    /// 发送实时通知给指定用户
    /// </summary>
    /// <param name="userId">目标用户ID</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <returns>发送结果</returns>
    Task<WebSocketSendMessageResultDto> SendToUserAsync(Guid userId, object message, WebSocketMessageType messageType);

    /// <summary>
    /// 发送实时通知给多个用户
    /// </summary>
    /// <param name="userIds">目标用户ID列表</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <returns>发送结果</returns>
    Task<WebSocketBroadcastResultDto> SendToUsersAsync(IEnumerable<Guid> userIds, object message, WebSocketMessageType messageType);

    /// <summary>
    /// 广播消息给所有连接用户
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="excludeUserIds">排除的用户ID列表</param>
    /// <returns>广播结果</returns>
    Task<WebSocketBroadcastResultDto> BroadcastAsync(object message, WebSocketMessageType messageType, IEnumerable<Guid>? excludeUserIds = null);

    /// <summary>
    /// 发送消息到指定组
    /// </summary>
    /// <param name="groupName">组名</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <returns>发送结果</returns>
    Task<WebSocketSendMessageResultDto> SendToGroupAsync(string groupName, object message, WebSocketMessageType messageType);

    /// <summary>
    /// 发送消息到多个组
    /// </summary>
    /// <param name="groupNames">组名列表</param>
    /// <param name="message">消息内容</param>
    /// <param name="messageType">消息类型</param>
    /// <returns>发送结果</returns>
    Task<WebSocketBroadcastResultDto> SendToGroupsAsync(IEnumerable<string> groupNames, object message, WebSocketMessageType messageType);

    /// <summary>
    /// 发送通知更新
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="notification">通知数据</param>
    /// <returns>发送结果</returns>
    Task<WebSocketSendMessageResultDto> SendNotificationUpdateAsync(Guid userId, NotificationDto notification);

    /// <summary>
    /// 发送未读计数更新
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">未读数量</param>
    /// <returns>发送结果</returns>
    Task<WebSocketSendMessageResultDto> SendUnreadCountUpdateAsync(Guid userId, int count);

    /// <summary>
    /// 发送系统消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="priority">消息优先级</param>
    /// <param name="targetUsers">目标用户列表（null表示所有用户）</param>
    /// <returns>发送结果</returns>
    Task<WebSocketBroadcastResultDto> SendSystemMessageAsync(object message, WebSocketMessagePriority priority, IEnumerable<Guid>? targetUsers = null);

    #endregion

    #region 连接组管理

    /// <summary>
    /// 将连接添加到组
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="groupName">组名</param>
    /// <returns>添加结果</returns>
    Task<WebSocketGroupOperationResultDto> AddToGroupAsync(string connectionId, string groupName);

    /// <summary>
    /// 从组中移除连接
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="groupName">组名</param>
    /// <returns>移除结果</returns>
    Task<WebSocketGroupOperationResultDto> RemoveFromGroupAsync(string connectionId, string groupName);

    /// <summary>
    /// 将用户添加到组
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupName">组名</param>
    /// <returns>添加结果</returns>
    Task<WebSocketGroupOperationResultDto> AddUserToGroupAsync(Guid userId, string groupName);

    /// <summary>
    /// 从组中移除用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupName">组名</param>
    /// <returns>移除结果</returns>
    Task<WebSocketGroupOperationResultDto> RemoveUserFromGroupAsync(Guid userId, string groupName);

    /// <summary>
    /// 获取组中的所有连接
    /// </summary>
    /// <param name="groupName">组名</param>
    /// <returns>连接列表</returns>
    Task<IEnumerable<WebSocketConnectionInfoDto>> GetGroupConnectionsAsync(string groupName);

    /// <summary>
    /// 获取用户所在的所有组
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>组名列表</returns>
    Task<IEnumerable<string>> GetUserGroupsAsync(Guid userId);

    /// <summary>
    /// 检查连接是否在指定组中
    /// </summary>
    /// <param name="connectionId">连接ID</param>
    /// <param name="groupName">组名</param>
    /// <returns>是否在组中</returns>
    Task<bool> IsConnectionInGroupAsync(string connectionId, string groupName);

    /// <summary>
    /// 获取所有组
    /// </summary>
    /// <returns>组名列表</returns>
    Task<IEnumerable<string>> GetAllGroupsAsync();

    /// <summary>
    /// 获取组的统计信息
    /// </summary>
    /// <param name="groupName">组名</param>
    /// <returns>组统计信息</returns>
    Task<WebSocketGroupStatsDto> GetGroupStatsAsync(string groupName);

    #endregion

    #region 消息队列和重试

    /// <summary>
    /// 添加消息到队列
    /// </summary>
    /// <param name="message">消息数据</param>
    /// <returns>队列结果</returns>
    Task<WebSocketQueueResultDto> EnqueueMessageAsync(WebSocketMessageQueueItem message);

    /// <summary>
    /// 处理消息队列
    /// </summary>
    /// <param name="batchSize">批次大小</param>
    /// <returns>处理结果</returns>
    Task<WebSocketQueueProcessResultDto> ProcessMessageQueueAsync(int batchSize = 100);

    /// <summary>
    /// 重试失败的消息
    /// </summary>
    /// <param name="maxRetries">最大重试次数</param>
    /// <returns>重试结果</returns>
    Task<WebSocketRetryResultDto> RetryFailedMessagesAsync(int maxRetries = 3);

    /// <summary>
    /// 获取队列状态
    /// </summary>
    /// <returns>队列状态</returns>
    Task<WebSocketQueueStatusDto> GetQueueStatusAsync();

    /// <summary>
    /// 清理过期消息
    /// </summary>
    /// <param name="beforeTime">清理时间</param>
    /// <returns>清理结果</returns>
    Task<WebSocketCleanupResultDto> CleanupExpiredMessagesAsync(DateTime beforeTime);

    #endregion

    #region 性能监控和统计

    /// <summary>
    /// 获取WebSocket服务统计信息
    /// </summary>
    /// <returns>统计信息</returns>
    Task<WebSocketStatsDto> GetStatsAsync();

    /// <summary>
    /// 获取连接历史记录
    /// </summary>
    /// <param name="userId">用户ID（可选）</param>
    /// <param name="timeRange">时间范围</param>
    /// <returns>连接历史</returns>
    Task<IEnumerable<WebSocketConnectionHistoryDto>> GetConnectionHistoryAsync(Guid? userId = null, TimeRangeDto? timeRange = null);

    /// <summary>
    /// 获取消息发送统计
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>发送统计</returns>
    Task<WebSocketMessageStatsDto> GetMessageStatsAsync(TimeRangeDto timeRange);

    /// <summary>
    /// 获取性能指标
    /// </summary>
    /// <param name="timeRange">时间范围</param>
    /// <returns>性能指标</returns>
    Task<WebSocketPerformanceMetricsDto> GetPerformanceMetricsAsync(TimeRangeDto timeRange);

    /// <summary>
    /// 记录连接事件
    /// </summary>
    /// <param name="connectionEvent">连接事件</param>
    /// <returns>记录结果</returns>
    Task<bool> LogConnectionEventAsync(WebSocketConnectionEvent connectionEvent);

    /// <summary>
    /// 记录消息事件
    /// </summary>
    /// <param name="messageEvent">消息事件</param>
    /// <returns>记录结果</returns>
    Task<bool> LogMessageEventAsync(WebSocketMessageEvent messageEvent);

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除用户连接缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>清除结果</returns>
    Task<bool> ClearUserConnectionCacheAsync(Guid userId);

    /// <summary>
    /// 预热用户连接信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>预热结果</returns>
    Task<bool> PreloadUserConnectionInfoAsync(Guid userId);

    /// <summary>
    /// 刷新连接统计缓存
    /// </summary>
    /// <returns>刷新结果</returns>
    Task<bool> RefreshConnectionStatsCacheAsync();

    #endregion
}

/// <summary>
/// WebSocket消息类型枚举
/// </summary>
public enum WebSocketMessageType
{
    /// <summary>
    /// 通知消息
    /// </summary>
    Notification = 0,

    /// <summary>
    /// 系统消息
    /// </summary>
    System = 1,

    /// <summary>
    /// 用户消息
    /// </summary>
    User = 2,

    /// <summary>
    /// 状态更新
    /// </summary>
    StatusUpdate = 3,

    /// <summary>
    /// 错误消息
    /// </summary>
    Error = 4,

    /// <summary>
    /// 确认消息
    /// </summary>
    Acknowledgment = 5,

    /// <summary>
    /// 心跳消息
    /// </summary>
    Heartbeat = 6,

    /// <summary>
    /// 自定义消息
    /// </summary>
    Custom = 7
}

/// <summary>
/// WebSocket消息优先级枚举
/// </summary>
public enum WebSocketMessagePriority
{
    /// <summary>
    /// 低优先级
    /// </summary>
    Low = 0,

    /// <summary>
    /// 普通优先级
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 高优先级
    /// </summary>
    High = 2,

    /// <summary>
    /// 紧急优先级
    /// </summary>
    Urgent = 3
}

/// <summary>
/// WebSocket连接状态枚举
/// </summary>
public enum WebSocketConnectionState
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
    /// 错误状态
    /// </summary>
    Error = 4,

    /// <summary>
    /// 超时状态
    /// </summary>
    Timeout = 5
}

#region WebSocket相关DTO定义

/// <summary>
/// WebSocket连接结果DTO
/// </summary>
public class WebSocketConnectionResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public WebSocketConnectionState State { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 连接详情
    /// </summary>
    public Dictionary<string, object>? ConnectionDetails { get; set; }
}

/// <summary>
/// WebSocket断开连接结果DTO
/// </summary>
public class WebSocketDisconnectionResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 断开时间
    /// </summary>
    public DateTime DisconnectedAt { get; set; }

    /// <summary>
    /// 连接时长（秒）
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// 断开原因
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket连接状态DTO
/// </summary>
public class WebSocketConnectionStatusDto
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
    public WebSocketConnectionState State { get; set; }

    /// <summary>
    /// 连接详情
    /// </summary>
    public Dictionary<string, object>? ConnectionDetails { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket连接信息DTO
/// </summary>
public class WebSocketConnectionInfoDto
{
    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public WebSocketConnectionState State { get; set; }

    /// <summary>
    /// 连接IP地址
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 连接详情
    /// </summary>
    public Dictionary<string, object>? ConnectionDetails { get; set; }
}

/// <summary>
/// WebSocket强制断开结果DTO
/// </summary>
public class WebSocketForceDisconnectResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 断开的连接数量
    /// </summary>
    public int DisconnectedCount { get; set; }

    /// <summary>
    /// 断开原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket清理结果DTO
/// </summary>
public class WebSocketCleanupResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 清理的连接数量
    /// </summary>
    public int CleanedConnections { get; set; }

    /// <summary>
    /// 清理的消息数量
    /// </summary>
    public int CleanedMessages { get; set; }

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
/// WebSocket发送消息结果DTO
/// </summary>
public class WebSocketSendMessageResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType MessageType { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket广播结果DTO
/// </summary>
public class WebSocketBroadcastResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 目标用户数量
    /// </summary>
    public int TargetCount { get; set; }

    /// <summary>
    /// 成功发送数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败发送数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType MessageType { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 发送结果详情
    /// </summary>
    public Dictionary<string, WebSocketSendMessageResultDto>? Results { get; set; }
}

/// <summary>
/// WebSocket组操作结果DTO
/// </summary>
public class WebSocketGroupOperationResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 组名
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 连接ID
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket组统计信息DTO
/// </summary>
public class WebSocketGroupStatsDto
{
    /// <summary>
    /// 组名
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 连接数量
    /// </summary>
    public int ConnectionCount { get; set; }

    /// <summary>
    /// 用户数量
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后活动时间
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public long MessageCount { get; set; }

    /// <summary>
    /// 组详情
    /// </summary>
    public Dictionary<string, object>? GroupDetails { get; set; }
}

/// <summary>
/// WebSocket消息队列项
/// </summary>
public class WebSocketMessageQueueItem
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// 目标组名
    /// </summary>
    public string? TargetGroup { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType MessageType { get; set; }

    /// <summary>
    /// 消息优先级
    /// </summary>
    public WebSocketMessagePriority Priority { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public object Message { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 计划发送时间
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 消息状态
    /// </summary>
    public WebSocketMessageStatus Status { get; set; } = WebSocketMessageStatus.Pending;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// 消息元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// WebSocket消息状态枚举
/// </summary>
public enum WebSocketMessageStatus
{
    /// <summary>
    /// 待发送
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 发送中
    /// </summary>
    Sending = 1,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 2,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 4,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 5
}

/// <summary>
/// WebSocket队列结果DTO
/// </summary>
public class WebSocketQueueResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// 队列位置
    /// </summary>
    public int? QueuePosition { get; set; }

    /// <summary>
    /// 预计发送时间
    /// </summary>
    public DateTime? EstimatedSendTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket队列处理结果DTO
/// </summary>
public class WebSocketQueueProcessResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 处理的消息数量
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
    /// 重试数量
    /// </summary>
    public int RetryCount { get; set; }

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
/// WebSocket重试结果DTO
/// </summary>
public class WebSocketRetryResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 重试的消息数量
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
    /// 处理时间（毫秒）
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket队列状态DTO
/// </summary>
public class WebSocketQueueStatusDto
{
    /// <summary>
    /// 队列状态
    /// </summary>
    public WebSocketQueueState State { get; set; }

    /// <summary>
    /// 待处理消息数量
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 处理中消息数量
    /// </summary>
    public int ProcessingCount { get; set; }

    /// <summary>
    /// 失败消息数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 总消息数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 队列大小
    /// </summary>
    public long QueueSizeBytes { get; set; }

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
/// WebSocket队列状态枚举
/// </summary>
public enum WebSocketQueueState
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
/// WebSocket统计信息DTO
/// </summary>
public class WebSocketStatsDto
{
    /// <summary>
    /// 总连接数
    /// </summary>
    public int TotalConnections { get; set; }

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// 在线用户数
    /// </summary>
    public int OnlineUsers { get; set; }

    /// <summary>
    /// 总消息数
    /// </summary>
    public long TotalMessages { get; set; }

    /// <summary>
    /// 今日消息数
    /// </summary>
    public int TodayMessages { get; set; }

    /// <summary>
    /// 平均连接时长（秒）
    /// </summary>
    public double AverageConnectionDuration { get; set; }

    /// <summary>
    /// 平均消息发送时间（毫秒）
    /// </summary>
    public double AverageMessageSendTime { get; set; }

    /// <summary>
    /// 消息成功率
    /// </summary>
    public double MessageSuccessRate { get; set; }

    /// <summary>
    /// 系统运行时间
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// 详细的统计信息
    /// </summary>
    public Dictionary<string, object>? DetailedStats { get; set; }
}

/// <summary>
/// WebSocket连接历史DTO
/// </summary>
public class WebSocketConnectionHistoryDto
{
    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// 断开时间
    /// </summary>
    public DateTime? DisconnectedAt { get; set; }

    /// <summary>
    /// 连接时长（秒）
    /// </summary>
    public long? Duration { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public WebSocketConnectionState State { get; set; }

    /// <summary>
    /// 连接IP地址
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 断开原因
    /// </summary>
    public string? DisconnectReason { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket消息统计DTO
/// </summary>
public class WebSocketMessageStatsDto
{
    /// <summary>
    /// 总消息数
    /// </summary>
    public long TotalMessages { get; set; }

    /// <summary>
    /// 成功消息数
    /// </summary>
    public long SuccessMessages { get; set; }

    /// <summary>
    /// 失败消息数
    /// </summary>
    public long FailedMessages { get; set; }

    /// <summary>
    /// 消息成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTime { get; set; }

    /// <summary>
    /// 按类型统计
    /// </summary>
    public Dictionary<WebSocketMessageType, WebSocketMessageTypeStats> TypeStats { get; set; } = new();

    /// <summary>
    /// 按优先级统计
    /// </summary>
    public Dictionary<WebSocketMessagePriority, WebSocketMessagePriorityStats> PriorityStats { get; set; } = new();

    /// <summary>
    /// 时间序列数据
    /// </summary>
    public List<WebSocketMessageTimeSeries> TimeSeries { get; set; } = new();

    /// <summary>
    /// 统计时间范围
    /// </summary>
    public TimeRangeDto TimeRange { get; set; } = new();
}

/// <summary>
/// WebSocket消息类型统计
/// </summary>
public class WebSocketMessageTypeStats
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType Type { get; set; }

    /// <summary>
    /// 消息数量
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
    public double AverageSendTime { get; set; }
}

/// <summary>
/// WebSocket消息优先级统计
/// </summary>
public class WebSocketMessagePriorityStats
{
    /// <summary>
    /// 消息优先级
    /// </summary>
    public WebSocketMessagePriority Priority { get; set; }

    /// <summary>
    /// 消息数量
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
    public double AverageSendTime { get; set; }
}

/// <summary>
/// WebSocket消息时间序列
/// </summary>
public class WebSocketMessageTimeSeries
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息数量
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
    public double AverageSendTime { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }
}

/// <summary>
/// WebSocket性能指标DTO
/// </summary>
public class WebSocketPerformanceMetricsDto
{
    /// <summary>
    /// 总连接数
    /// </summary>
    public int TotalConnections { get; set; }

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// 总消息数
    /// </summary>
    public long TotalMessages { get; set; }

    /// <summary>
    /// 平均连接建立时间（毫秒）
    /// </summary>
    public double AverageConnectionTime { get; set; }

    /// <summary>
    /// 平均消息发送时间（毫秒）
    /// </summary>
    public double AverageMessageSendTime { get; set; }

    /// <summary>
    /// 消息成功率
    /// </summary>
    public double MessageSuccessRate { get; set; }

    /// <summary>
    /// 连接成功率
    /// </summary>
    public double ConnectionSuccessRate { get; set; }

    /// <summary>
    /// 服务器负载
    /// </summary>
    public double ServerLoad { get; set; }

    /// <summary>
    /// 内存使用量（MB）
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// CPU使用率
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// 网络延迟（毫秒）
    /// </summary>
    public double NetworkLatency { get; set; }

    /// <summary>
    /// 时间序列数据
    /// </summary>
    public List<WebSocketPerformanceTimeSeries> TimeSeries { get; set; } = new();

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
/// WebSocket性能时间序列
/// </summary>
public class WebSocketPerformanceTimeSeries
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 活跃连接数
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 平均发送时间（毫秒）
    /// </summary>
    public double AverageSendTime { get; set; }

    /// <summary>
    /// 成功率
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// 服务器负载
    /// </summary>
    public double ServerLoad { get; set; }

    /// <summary>
    /// 内存使用量（MB）
    /// </summary>
    public double MemoryUsage { get; set; }

    /// <summary>
    /// CPU使用率
    /// </summary>
    public double CpuUsage { get; set; }
}

/// <summary>
/// WebSocket连接事件
/// </summary>
public class WebSocketConnectionEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 事件类型
    /// </summary>
    public WebSocketConnectionEventType EventType { get; set; }

    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 连接状态
    /// </summary>
    public WebSocketConnectionState ConnectionState { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 事件详情
    /// </summary>
    public Dictionary<string, object>? EventDetails { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket连接事件类型枚举
/// </summary>
public enum WebSocketConnectionEventType
{
    /// <summary>
    /// 连接建立
    /// </summary>
    Connected = 0,

    /// <summary>
    /// 连接断开
    /// </summary>
    Disconnected = 1,

    /// <summary>
    /// 连接重连
    /// </summary>
    Reconnected = 2,

    /// <summary>
    /// 连接错误
    /// </summary>
    Error = 3,

    /// <summary>
    /// 连接超时
    /// </summary>
    Timeout = 4,

    /// <summary>
    /// 心跳检测
    /// </summary>
    Heartbeat = 5,

    /// <summary>
    /// 强制断开
    /// </summary>
    ForceDisconnected = 6
}

/// <summary>
/// WebSocket消息事件
/// </summary>
public class WebSocketMessageEvent
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 事件类型
    /// </summary>
    public WebSocketMessageEventType EventType { get; set; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// 连接ID
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 消息类型
    /// </summary>
    public WebSocketMessageType MessageType { get; set; }

    /// <summary>
    /// 消息优先级
    /// </summary>
    public WebSocketMessagePriority Priority { get; set; }

    /// <summary>
    /// 目标用户ID
    /// </summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>
    /// 目标组名
    /// </summary>
    public string? TargetGroup { get; set; }

    /// <summary>
    /// 发送状态
    /// </summary>
    public WebSocketMessageSendStatus SendStatus { get; set; }

    /// <summary>
    /// 发送时间（毫秒）
    /// </summary>
    public long SendTimeMs { get; set; }

    /// <summary>
    /// 事件详情
    /// </summary>
    public Dictionary<string, object>? EventDetails { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket消息事件类型枚举
/// </summary>
public enum WebSocketMessageEventType
{
    /// <summary>
    /// 消息发送
    /// </summary>
    Sent = 0,

    /// <summary>
    /// 消息接收
    /// </summary>
    Received = 1,

    /// <summary>
    /// 消息失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 消息重试
    /// </summary>
    Retry = 3,

    /// <summary>
    /// 消息过期
    /// </summary>
    Expired = 4,

    /// <summary>
    /// 消息取消
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// 消息确认
    /// </summary>
    Acknowledgment = 6
}

/// <summary>
/// WebSocket消息发送状态枚举
/// </summary>
public enum WebSocketMessageSendStatus
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 0,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 1,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 2,

    /// <summary>
    /// 拒绝
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// 队列满
    /// </summary>
    QueueFull = 4
}

#endregion