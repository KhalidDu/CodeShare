using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// WebSocket服务实现 - 提供实时通信和消息传递功能
/// </summary>
public class WebSocketService : IWebSocketService
{
    private readonly ILogger<WebSocketService> _logger;
    private readonly IDistributedCache _cache;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;
    
    // 连接管理
    private readonly ConcurrentDictionary<string, WebSocketConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, DateTime>> _userConnections = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> _groupConnections = new();
    
    // 消息队列
    private readonly ConcurrentQueue<WebSocketMessageQueueItem> _messageQueue = new();
    private readonly ConcurrentDictionary<string, WebSocketMessageQueueItem> _processingMessages = new();
    private readonly ConcurrentDictionary<string, int> _messageRetries = new();
    
    // 统计信息
    private long _totalConnections = 0;
    private long _totalMessages = 0;
    private long _failedMessages = 0;
    private DateTime _startTime = DateTime.UtcNow;
    
    // 缓存键前缀
    private const string CONNECTION_CACHE_PREFIX = "ws_connection_";
    private const string USER_CONNECTIONS_CACHE_PREFIX = "ws_user_connections_";
    private const string GROUP_CACHE_PREFIX = "ws_group_";
    private const string STATS_CACHE_PREFIX = "ws_stats_";
    private const string MESSAGE_QUEUE_CACHE_PREFIX = "ws_message_queue_";
    
    // 配置常量
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MessageExpiry = TimeSpan.FromHours(1);
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan StatsUpdateInterval = TimeSpan.FromMinutes(1);
    private static readonly int MaxQueueSize = 10000;
    private static readonly int MaxRetryAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public WebSocketService(
        ILogger<WebSocketService> logger,
        IDistributedCache cache,
        IPermissionService permissionService,
        INotificationService notificationService,
        IHubContext<NotificationHub> hubContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        
        // 启动后台任务
        _ = StartBackgroundTasksAsync();
    }

    #region 连接管理操作

    /// <summary>
    /// 建立WebSocket连接
    /// </summary>
    public async Task<WebSocketConnectionResultDto> ConnectAsync(Guid userId, string connectionId, IHubContext<NotificationHub> hubContext)
    {
        try
        {
            _logger.LogInformation("用户建立WebSocket连接: UserId={UserId}, ConnectionId={ConnectionId}", 
                userId, connectionId);

            // 验证用户权限
            if (!await _permissionService.HasPermissionAsync(userId, "WebSocketConnect"))
            {
                _logger.LogWarning("用户没有WebSocket连接权限: UserId={UserId}", userId);
                return new WebSocketConnectionResultDto
                {
                    Success = false,
                    UserId = userId,
                    State = WebSocketConnectionState.Error,
                    Error = "用户没有WebSocket连接权限"
                };
            }

            var connectedAt = DateTime.UtcNow;
            var connectionInfo = new WebSocketConnectionInfo
            {
                ConnectionId = connectionId,
                UserId = userId,
                ConnectedAt = connectedAt,
                LastActivityAt = connectedAt,
                State = WebSocketConnectionState.Connected
            };

            // 添加到连接字典
            _connections[connectionId] = connectionInfo;

            // 添加到用户连接字典
            if (!_userConnections.TryGetValue(userId, out var userConnections))
            {
                userConnections = new ConcurrentDictionary<string, DateTime>();
                _userConnections[userId] = userConnections;
            }
            userConnections[connectionId] = connectedAt;

            // 缓存连接信息
            await CacheConnectionInfoAsync(connectionId, connectionInfo);
            await CacheUserConnectionsAsync(userId);

            // 更新统计信息
            Interlocked.Increment(ref _totalConnections);

            // 记录连接事件
            await LogConnectionEventAsync(new WebSocketConnectionEvent
            {
                EventType = WebSocketConnectionEventType.Connected,
                ConnectionId = connectionId,
                UserId = userId,
                EventTime = connectedAt,
                ConnectionState = WebSocketConnectionState.Connected
            });

            // 发送连接确认
            await SendToUserAsync(userId, new
            {
                type = "connection_established",
                connectionId = connectionId,
                timestamp = connectedAt,
                message = "WebSocket连接已建立"
            }, WebSocketMessageType.Acknowledgment);

            _logger.LogInformation("WebSocket连接建立成功: UserId={UserId}, ConnectionId={ConnectionId}", 
                userId, connectionId);

            return new WebSocketConnectionResultDto
            {
                Success = true,
                ConnectionId = connectionId,
                UserId = userId,
                ConnectedAt = connectedAt,
                State = WebSocketConnectionState.Connected,
                ConnectionDetails = new Dictionary<string, object>
                {
                    { "totalConnections", _connections.Count },
                    { "userConnections", userConnections.Count }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立WebSocket连接失败: UserId={UserId}, ConnectionId={ConnectionId}", 
                userId, connectionId);
            
            return new WebSocketConnectionResultDto
            {
                Success = false,
                UserId = userId,
                State = WebSocketConnectionState.Error,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 断开WebSocket连接
    /// </summary>
    public async Task<WebSocketDisconnectionResultDto> DisconnectAsync(Guid userId, string connectionId)
    {
        try
        {
            _logger.LogInformation("用户断开WebSocket连接: UserId={UserId}, ConnectionId={ConnectionId}", 
                userId, connectionId);

            var disconnectedAt = DateTime.UtcNow;
            var duration = 0L;

            // 获取连接信息
            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                duration = (long)(disconnectedAt - connectionInfo.ConnectedAt).TotalSeconds;
                
                // 从连接字典中移除
                _connections.TryRemove(connectionId, out _);
                
                // 从用户连接字典中移除
                if (_userConnections.TryGetValue(userId, out var userConnections))
                {
                    userConnections.TryRemove(connectionId, out _);
                    
                    // 如果用户没有其他连接，移除用户条目
                    if (userConnections.IsEmpty)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
                
                // 清除缓存
                await ClearConnectionCacheAsync(connectionId);
                await ClearUserConnectionsCacheAsync(userId);
            }

            // 记录断开事件
            await LogConnectionEventAsync(new WebSocketConnectionEvent
            {
                EventType = WebSocketConnectionEventType.Disconnected,
                ConnectionId = connectionId,
                UserId = userId,
                EventTime = disconnectedAt,
                ConnectionState = WebSocketConnectionState.Disconnected
            });

            _logger.LogInformation("WebSocket连接断开成功: UserId={UserId}, ConnectionId={ConnectionId}, Duration={Duration}s", 
                userId, connectionId, duration);

            return new WebSocketDisconnectionResultDto
            {
                Success = true,
                ConnectionId = connectionId,
                UserId = userId,
                DisconnectedAt = disconnectedAt,
                Duration = duration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开WebSocket连接失败: UserId={UserId}, ConnectionId={ConnectionId}", 
                userId, connectionId);
            
            return new WebSocketDisconnectionResultDto
            {
                Success = false,
                ConnectionId = connectionId,
                UserId = userId,
                DisconnectedAt = DateTime.UtcNow,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 获取用户连接状态
    /// </summary>
    public async Task<WebSocketConnectionStatusDto> GetConnectionStatusAsync(Guid userId)
    {
        try
        {
            var isConnected = await IsUserOnlineAsync(userId);
            var connectionInfo = _userConnections.TryGetValue(userId, out var connections) && !connections.IsEmpty
                ? connections.FirstOrDefault()
                : null;

            return new WebSocketConnectionStatusDto
            {
                IsConnected = isConnected,
                ConnectionId = connectionInfo?.Key,
                ConnectedAt = connectionInfo?.Value,
                LastActivityAt = connectionInfo?.Value,
                State = isConnected ? WebSocketConnectionState.Connected : WebSocketConnectionState.Disconnected
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户连接状态失败: UserId={UserId}", userId);
            return new WebSocketConnectionStatusDto
            {
                IsConnected = false,
                State = WebSocketConnectionState.Error,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 获取所有活跃连接
    /// </summary>
    public async Task<IEnumerable<WebSocketConnectionInfoDto>> GetActiveConnectionsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var activeConnections = _connections.Values
                .Where(c => (now - c.LastActivityAt) < ConnectionTimeout)
                .Select(c => new WebSocketConnectionInfoDto
                {
                    ConnectionId = c.ConnectionId,
                    UserId = c.UserId,
                    ConnectedAt = c.ConnectedAt,
                    LastActivityAt = c.LastActivityAt,
                    State = c.State
                })
                .ToList();

            return activeConnections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取活跃连接失败");
            return Enumerable.Empty<WebSocketConnectionInfoDto>();
        }
    }

    /// <summary>
    /// 检查用户是否在线
    /// </summary>
    public Task<bool> IsUserOnlineAsync(Guid userId)
    {
        return Task.FromResult(_userConnections.TryGetValue(userId, out var connections) && 
                              !connections.IsEmpty && 
                              connections.Values.Any(t => (DateTime.UtcNow - t) < ConnectionTimeout));
    }

    /// <summary>
    /// 获取用户的连接数量
    /// </summary>
    public Task<int> GetUserConnectionCountAsync(Guid userId)
    {
        return Task.FromResult(_userConnections.TryGetValue(userId, out var connections) ? connections.Count : 0);
    }

    /// <summary>
    /// 强制断开用户连接
    /// </summary>
    public async Task<WebSocketForceDisconnectResultDto> ForceDisconnectUserAsync(Guid userId, string reason)
    {
        try
        {
            _logger.LogInformation("强制断开用户连接: UserId={UserId}, Reason={Reason}", userId, reason);

            var disconnectedCount = 0;

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                var connectionIds = connections.Keys.ToList();
                
                foreach (var connectionId in connectionIds)
                {
                    try
                    {
                        // 通过SignalR断开连接
                        await _hubContext.Clients.Client(connectionId).SendAsync("ForceDisconnect", new
                        {
                            reason = reason,
                            timestamp = DateTime.UtcNow
                        });

                        // 清理连接
                        await DisconnectAsync(userId, connectionId);
                        disconnectedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "强制断开连接失败: ConnectionId={ConnectionId}", connectionId);
                    }
                }
            }

            _logger.LogInformation("强制断开用户连接完成: UserId={UserId}, DisconnectedCount={DisconnectedCount}", 
                userId, disconnectedCount);

            return new WebSocketForceDisconnectResultDto
            {
                Success = true,
                UserId = userId,
                DisconnectedCount = disconnectedCount,
                Reason = reason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "强制断开用户连接失败: UserId={UserId}", userId);
            return new WebSocketForceDisconnectResultDto
            {
                Success = false,
                UserId = userId,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 清理过期连接
    /// </summary>
    public async Task<WebSocketCleanupResultDto> CleanupExpiredConnectionsAsync(int timeoutMinutes = 30)
    {
        try
        {
            _logger.LogInformation("开始清理过期连接");

            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromMinutes(timeoutMinutes);
            var cleanedCount = 0;
            var now = DateTime.UtcNow;

            // 查找过期连接
            var expiredConnections = _connections
                .Where(kv => (now - kv.Value.LastActivityAt) > timeout)
                .ToList();

            foreach (var kvp in expiredConnections)
            {
                try
                {
                    var connectionId = kvp.Key;
                    var connectionInfo = kvp.Value;

                    // 断开连接
                    await DisconnectAsync(connectionInfo.UserId, connectionId);
                    cleanedCount++;

                    _logger.LogDebug("清理过期连接: ConnectionId={ConnectionId}, UserId={UserId}", 
                        connectionId, connectionInfo.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "清理过期连接失败: ConnectionId={ConnectionId}", kvp.Key);
                }
            }

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("清理过期连接完成: CleanedCount={CleanedCount}, ProcessingTime={ProcessingTime}ms", 
                cleanedCount, processingTime);

            return new WebSocketCleanupResultDto
            {
                Success = true,
                CleanedConnections = cleanedCount,
                ProcessingTimeMs = processingTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期连接失败");
            return new WebSocketCleanupResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    #endregion

    #region 消息发送操作

    /// <summary>
    /// 发送实时通知给指定用户
    /// </summary>
    public async Task<WebSocketSendMessageResultDto> SendToUserAsync(Guid userId, object message, WebSocketMessageType messageType)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            if (!await IsUserOnlineAsync(userId))
            {
                return new WebSocketSendMessageResultDto
                {
                    Success = false,
                    UserId = userId,
                    MessageType = messageType,
                    Error = "用户不在线"
                };
            }

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                var sendResults = new List<WebSocketSendMessageResultDto>();
                
                foreach (var connectionId in connections.Keys)
                {
                    try
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                        {
                            type = messageType.ToString().ToLower(),
                            data = message,
                            timestamp = DateTime.UtcNow,
                            messageId = Guid.NewGuid().ToString()
                        });

                        sendResults.Add(new WebSocketSendMessageResultDto
                        {
                            Success = true,
                            ConnectionId = connectionId,
                            UserId = userId,
                            MessageType = messageType,
                            SendTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "发送消息失败: ConnectionId={ConnectionId}", connectionId);
                        sendResults.Add(new WebSocketSendMessageResultDto
                        {
                            Success = false,
                            ConnectionId = connectionId,
                            UserId = userId,
                            MessageType = messageType,
                            Error = ex.Message
                        });
                    }
                }

                // 更新统计信息
                Interlocked.Increment(ref _totalMessages);
                
                var successCount = sendResults.Count(r => r.Success);
                if (successCount == 0)
                {
                    Interlocked.Increment(ref _failedMessages);
                }

                // 返回第一个结果作为主结果
                var mainResult = sendResults.FirstOrDefault() ?? new WebSocketSendMessageResultDto();
                mainResult.Success = successCount > 0;
                mainResult.SendTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                return mainResult;
            }

            return new WebSocketSendMessageResultDto
            {
                Success = false,
                UserId = userId,
                MessageType = messageType,
                Error = "用户连接信息不存在"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息失败: UserId={UserId}", userId);
            Interlocked.Increment(ref _failedMessages);
            return new WebSocketSendMessageResultDto
            {
                Success = false,
                UserId = userId,
                MessageType = messageType,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 发送实时通知给多个用户
    /// </summary>
    public async Task<WebSocketBroadcastResultDto> SendToUsersAsync(IEnumerable<Guid> userIds, object message, WebSocketMessageType messageType)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var targetUserIds = userIds.ToList();
            var results = new Dictionary<string, WebSocketSendMessageResultDto>();
            var successCount = 0;
            var failedCount = 0;

            foreach (var userId in targetUserIds)
            {
                var result = await SendToUserAsync(userId, message, messageType);
                results[userId.ToString()] = result;
                
                if (result.Success)
                    successCount++;
                else
                    failedCount++;
            }

            return new WebSocketBroadcastResultDto
            {
                Success = successCount > 0,
                TargetCount = targetUserIds.Count,
                SuccessCount = successCount,
                FailedCount = failedCount,
                MessageType = messageType,
                SendTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量发送消息失败");
            return new WebSocketBroadcastResultDto
            {
                Success = false,
                MessageType = messageType,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 广播消息给所有连接用户
    /// </summary>
    public async Task<WebSocketBroadcastResultDto> BroadcastAsync(object message, WebSocketMessageType messageType, IEnumerable<Guid>? excludeUserIds = null)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var excludeUsers = excludeUserIds?.ToHashSet() ?? new HashSet<Guid>();
            
            // 获取所有在线用户
            var onlineUsers = _userConnections.Keys
                .Where(userId => !excludeUsers.Contains(userId))
                .ToList();

            var result = await SendToUsersAsync(onlineUsers, message, messageType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "广播消息失败");
            return new WebSocketBroadcastResultDto
            {
                Success = false,
                MessageType = messageType,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 发送消息到指定组
    /// </summary>
    public async Task<WebSocketSendMessageResultDto> SendToGroupAsync(string groupName, object message, WebSocketMessageType messageType)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new
                {
                    type = messageType.ToString().ToLower(),
                    data = message,
                    timestamp = DateTime.UtcNow,
                    messageId = Guid.NewGuid().ToString()
                });

                Interlocked.Increment(ref _totalMessages);

                return new WebSocketSendMessageResultDto
                {
                    Success = true,
                    MessageType = messageType,
                    SendTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送组消息失败: GroupName={GroupName}", groupName);
                Interlocked.Increment(ref _failedMessages);
                return new WebSocketSendMessageResultDto
                {
                    Success = false,
                    MessageType = messageType,
                    Error = ex.Message
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送组消息失败: GroupName={GroupName}", groupName);
            return new WebSocketSendMessageResultDto
            {
                Success = false,
                MessageType = messageType,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 发送消息到多个组
    /// </summary>
    public async Task<WebSocketBroadcastResultDto> SendToGroupsAsync(IEnumerable<string> groupNames, object message, WebSocketMessageType messageType)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var targetGroups = groupNames.ToList();
            var results = new Dictionary<string, WebSocketSendMessageResultDto>();
            var successCount = 0;
            var failedCount = 0;

            foreach (var groupName in targetGroups)
            {
                var result = await SendToGroupAsync(groupName, message, messageType);
                results[groupName] = result;
                
                if (result.Success)
                    successCount++;
                else
                    failedCount++;
            }

            return new WebSocketBroadcastResultDto
            {
                Success = successCount > 0,
                TargetCount = targetGroups.Count,
                SuccessCount = successCount,
                FailedCount = failedCount,
                MessageType = messageType,
                SendTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                Results = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量发送组消息失败");
            return new WebSocketBroadcastResultDto
            {
                Success = false,
                MessageType = messageType,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 发送通知更新
    /// </summary>
    public async Task<WebSocketSendMessageResultDto> SendNotificationUpdateAsync(Guid userId, NotificationDto notification)
    {
        return await SendToUserAsync(userId, new
        {
            notification = notification,
            action = "notification_update"
        }, WebSocketMessageType.Notification);
    }

    /// <summary>
    /// 发送未读计数更新
    /// </summary>
    public async Task<WebSocketSendMessageResultDto> SendUnreadCountUpdateAsync(Guid userId, int count)
    {
        return await SendToUserAsync(userId, new
        {
            unreadCount = count,
            action = "unread_count_update"
        }, WebSocketMessageType.StatusUpdate);
    }

    /// <summary>
    /// 发送系统消息
    /// </summary>
    public async Task<WebSocketBroadcastResultDto> SendSystemMessageAsync(object message, WebSocketMessagePriority priority, IEnumerable<Guid>? targetUsers = null)
    {
        var systemMessage = new
        {
            content = message,
            priority = priority.ToString(),
            timestamp = DateTime.UtcNow,
            type = "system_message"
        };

        if (targetUsers == null)
        {
            return await BroadcastAsync(systemMessage, WebSocketMessageType.System);
        }
        else
        {
            return await SendToUsersAsync(targetUsers, systemMessage, WebSocketMessageType.System);
        }
    }

    #endregion

    #region 连接组管理

    /// <summary>
    /// 将连接添加到组
    /// </summary>
    public async Task<WebSocketGroupOperationResultDto> AddToGroupAsync(string connectionId, string groupName)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);

            // 更新组连接字典
            if (!_groupConnections.TryGetValue(groupName, out var groupConnections))
            {
                groupConnections = new ConcurrentDictionary<string, DateTime>();
                _groupConnections[groupName] = groupConnections;
            }
            groupConnections[connectionId] = DateTime.UtcNow;

            // 缓存组信息
            await CacheGroupInfoAsync(groupName);

            return new WebSocketGroupOperationResultDto
            {
                Success = true,
                Operation = "AddToGroup",
                GroupName = groupName,
                ConnectionId = connectionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加连接到组失败: ConnectionId={ConnectionId}, GroupName={GroupName}", 
                connectionId, groupName);
            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "AddToGroup",
                GroupName = groupName,
                ConnectionId = connectionId,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 从组中移除连接
    /// </summary>
    public async Task<WebSocketGroupOperationResultDto> RemoveFromGroupAsync(string connectionId, string groupName)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);

            // 更新组连接字典
            if (_groupConnections.TryGetValue(groupName, out var groupConnections))
            {
                groupConnections.TryRemove(connectionId, out _);
                
                // 如果组为空，移除组
                if (groupConnections.IsEmpty)
                {
                    _groupConnections.TryRemove(groupName, out _);
                    await ClearGroupCacheAsync(groupName);
                }
            }

            return new WebSocketGroupOperationResultDto
            {
                Success = true,
                Operation = "RemoveFromGroup",
                GroupName = groupName,
                ConnectionId = connectionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从组中移除连接失败: ConnectionId={ConnectionId}, GroupName={GroupName}", 
                connectionId, groupName);
            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "RemoveFromGroup",
                GroupName = groupName,
                ConnectionId = connectionId,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 将用户添加到组
    /// </summary>
    public async Task<WebSocketGroupOperationResultDto> AddUserToGroupAsync(Guid userId, string groupName)
    {
        try
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                var results = new List<WebSocketGroupOperationResultDto>();
                
                foreach (var connectionId in connections.Keys)
                {
                    var result = await AddToGroupAsync(connectionId, groupName);
                    results.Add(result);
                }

                var successCount = results.Count(r => r.Success);
                
                return new WebSocketGroupOperationResultDto
                {
                    Success = successCount > 0,
                    Operation = "AddUserToGroup",
                    GroupName = groupName,
                    UserId = userId
                };
            }

            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "AddUserToGroup",
                GroupName = groupName,
                UserId = userId,
                Error = "用户连接不存在"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加用户到组失败: UserId={UserId}, GroupName={GroupName}", userId, groupName);
            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "AddUserToGroup",
                GroupName = groupName,
                UserId = userId,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 从组中移除用户
    /// </summary>
    public async Task<WebSocketGroupOperationResultDto> RemoveUserFromGroupAsync(Guid userId, string groupName)
    {
        try
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                var results = new List<WebSocketGroupOperationResultDto>();
                
                foreach (var connectionId in connections.Keys)
                {
                    var result = await RemoveFromGroupAsync(connectionId, groupName);
                    results.Add(result);
                }

                var successCount = results.Count(r => r.Success);
                
                return new WebSocketGroupOperationResultDto
                {
                    Success = successCount > 0,
                    Operation = "RemoveUserFromGroup",
                    GroupName = groupName,
                    UserId = userId
                };
            }

            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "RemoveUserFromGroup",
                GroupName = groupName,
                UserId = userId,
                Error = "用户连接不存在"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从组中移除用户失败: UserId={UserId}, GroupName={GroupName}", userId, groupName);
            return new WebSocketGroupOperationResultDto
            {
                Success = false,
                Operation = "RemoveUserFromGroup",
                GroupName = groupName,
                UserId = userId,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 获取组中的所有连接
    /// </summary>
    public async Task<IEnumerable<WebSocketConnectionInfoDto>> GetGroupConnectionsAsync(string groupName)
    {
        try
        {
            var connectionInfos = new List<WebSocketConnectionInfoDto>();

            if (_groupConnections.TryGetValue(groupName, out var groupConnections))
            {
                foreach (var (connectionId, connectedAt) in groupConnections)
                {
                    if (_connections.TryGetValue(connectionId, out var connectionInfo))
                    {
                        connectionInfos.Add(new WebSocketConnectionInfoDto
                        {
                            ConnectionId = connectionId,
                            UserId = connectionInfo.UserId,
                            ConnectedAt = connectedAt,
                            LastActivityAt = connectionInfo.LastActivityAt,
                            State = connectionInfo.State
                        });
                    }
                }
            }

            return connectionInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取组连接失败: GroupName={GroupName}", groupName);
            return Enumerable.Empty<WebSocketConnectionInfoDto>();
        }
    }

    /// <summary>
    /// 获取用户所在的所有组
    /// </summary>
    public async Task<IEnumerable<string>> GetUserGroupsAsync(Guid userId)
    {
        try
        {
            var userGroups = new HashSet<string>();

            if (_userConnections.TryGetValue(userId, out var connections))
            {
                foreach (var (groupName, groupConnections) in _groupConnections)
                {
                    if (connections.Keys.Any(connectionId => connections.ContainsKey(connectionId)))
                    {
                        userGroups.Add(groupName);
                    }
                }
            }

            return userGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户组失败: UserId={UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 检查连接是否在指定组中
    /// </summary>
    public async Task<bool> IsConnectionInGroupAsync(string connectionId, string groupName)
    {
        try
        {
            return _groupConnections.TryGetValue(groupName, out var groupConnections) &&
                   groupConnections.ContainsKey(connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查连接组关系失败: ConnectionId={ConnectionId}, GroupName={GroupName}", 
                connectionId, groupName);
            return false;
        }
    }

    /// <summary>
    /// 获取所有组
    /// </summary>
    public async Task<IEnumerable<string>> GetAllGroupsAsync()
    {
        try
        {
            return _groupConnections.Keys.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有组失败");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 获取组的统计信息
    /// </summary>
    public async Task<WebSocketGroupStatsDto> GetGroupStatsAsync(string groupName)
    {
        try
        {
            var stats = new WebSocketGroupStatsDto
            {
                GroupName = groupName,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            if (_groupConnections.TryGetValue(groupName, out var groupConnections))
            {
                stats.ConnectionCount = groupConnections.Count;
                stats.UserCount = groupConnections.Values
                    .Select(connId => _connections.TryGetValue(connId, out var conn) ? conn.UserId : Guid.Empty)
                    .Distinct()
                    .Count();

                stats.LastActivityAt = groupConnections.Values.Max();
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取组统计失败: GroupName={GroupName}", groupName);
            return new WebSocketGroupStatsDto { GroupName = groupName };
        }
    }

    #endregion

    #region 消息队列和重试

    /// <summary>
    /// 添加消息到队列
    /// </summary>
    public async Task<WebSocketQueueResultDto> EnqueueMessageAsync(WebSocketMessageQueueItem message)
    {
        try
        {
            if (_messageQueue.Count >= MaxQueueSize)
            {
                return new WebSocketQueueResultDto
                {
                    Success = false,
                    Error = "消息队列已满"
                };
            }

            message.CreatedAt = DateTime.UtcNow;
            message.Status = WebSocketMessageStatus.Pending;
            
            _messageQueue.Enqueue(message);

            // 缓存消息队列状态
            await CacheQueueStatusAsync();

            return new WebSocketQueueResultDto
            {
                Success = true,
                MessageId = message.MessageId,
                QueuePosition = _messageQueue.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加消息到队列失败");
            return new WebSocketQueueResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 处理消息队列
    /// </summary>
    public async Task<WebSocketQueueProcessResultDto> ProcessMessageQueueAsync(int batchSize = 100)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var processedCount = 0;
            var successCount = 0;
            var failedCount = 0;
            var retryCount = 0;

            for (int i = 0; i < batchSize && !_messageQueue.IsEmpty; i++)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    processedCount++;
                    
                    try
                    {
                        // 检查消息是否过期
                        if (message.ExpiresAt.HasValue && message.ExpiresAt.Value < DateTime.UtcNow)
                        {
                            message.Status = WebSocketMessageStatus.Expired;
                            failedCount++;
                            continue;
                        }

                        // 检查是否需要延迟发送
                        if (message.ScheduledAt.HasValue && message.ScheduledAt.Value > DateTime.UtcNow)
                        {
                            // 重新入队
                            _messageQueue.Enqueue(message);
                            continue;
                        }

                        // 移动到处理中字典
                        _processingMessages[message.MessageId] = message;
                        message.Status = WebSocketMessageStatus.Sending;

                        // 发送消息
                        WebSocketSendMessageResultDto? sendResult = null;

                        if (message.TargetUserId.HasValue)
                        {
                            sendResult = await SendToUserAsync(message.TargetUserId.Value, message.Message, message.MessageType);
                        }
                        else if (!string.IsNullOrEmpty(message.TargetGroup))
                        {
                            sendResult = await SendToGroupAsync(message.TargetGroup, message.Message, message.MessageType);
                        }

                        if (sendResult != null && sendResult.Success)
                        {
                            message.Status = WebSocketMessageStatus.Sent;
                            successCount++;
                        }
                        else
                        {
                            message.Status = WebSocketMessageStatus.Failed;
                            message.Error = sendResult?.Error ?? "发送失败";
                            failedCount++;

                            // 重试逻辑
                            if (message.RetryCount < message.MaxRetries)
                            {
                                message.RetryCount++;
                                message.Status = WebSocketMessageStatus.Pending;
                                _messageQueue.Enqueue(message);
                                retryCount++;
                            }
                        }

                        // 从处理中字典移除
                        _processingMessages.TryRemove(message.MessageId, out _);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理消息失败: MessageId={MessageId}", message.MessageId);
                        message.Status = WebSocketMessageStatus.Failed;
                        message.Error = ex.Message;
                        failedCount++;
                        
                        _processingMessages.TryRemove(message.MessageId, out _);
                    }
                }
            }

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new WebSocketQueueProcessResultDto
            {
                Success = true,
                ProcessedCount = processedCount,
                SuccessCount = successCount,
                FailedCount = failedCount,
                RetryCount = retryCount,
                ProcessingTimeMs = processingTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理消息队列失败");
            return new WebSocketQueueProcessResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 重试失败的消息
    /// </summary>
    public async Task<WebSocketRetryResultDto> RetryFailedMessagesAsync(int maxRetries = 3)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var retriedCount = 0;
            var successCount = 0;
            var failedCount = 0;

            var failedMessages = _processingMessages.Values
                .Where(m => m.Status == WebSocketMessageStatus.Failed && m.RetryCount < maxRetries)
                .ToList();

            foreach (var message in failedMessages)
            {
                message.RetryCount++;
                message.Status = WebSocketMessageStatus.Pending;
                
                // 重新入队
                _messageQueue.Enqueue(message);
                retriedCount++;

                // 尝试立即发送
                var result = await ProcessMessageQueueAsync(1);
                if (result.Success && result.SuccessCount > 0)
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new WebSocketRetryResultDto
            {
                Success = true,
                RetriedCount = retriedCount,
                SuccessCount = successCount,
                FailedCount = failedCount,
                ProcessingTimeMs = processingTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重试失败消息失败");
            return new WebSocketRetryResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 获取队列状态
    /// </summary>
    public async Task<WebSocketQueueStatusDto> GetQueueStatusAsync()
    {
        try
        {
            var queueSize = _messageQueue.Count + _processingMessages.Count;
            var failedCount = _processingMessages.Values.Count(m => m.Status == WebSocketMessageStatus.Failed);

            return new WebSocketQueueStatusDto
            {
                State = queueSize < MaxQueueSize / 2 ? WebSocketQueueState.Normal : WebSocketQueueState.Busy,
                PendingCount = _messageQueue.Count,
                ProcessingCount = _processingMessages.Count,
                FailedCount = failedCount,
                TotalCount = queueSize,
                QueueSizeBytes = queueSize * 1024, // 估算大小
                LastProcessedAt = DateTime.UtcNow,
                ProcessingRate = CalculateProcessingRate(),
                AverageProcessingTimeMs = CalculateAverageProcessingTime()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取队列状态失败");
            return new WebSocketQueueStatusDto
            {
                State = WebSocketQueueState.Error,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// 清理过期消息
    /// </summary>
    public async Task<WebSocketCleanupResultDto> CleanupExpiredMessagesAsync(DateTime beforeTime)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var cleanedCount = 0;

            var expiredMessages = _processingMessages.Values
                .Where(m => m.ExpiresAt.HasValue && m.ExpiresAt.Value < beforeTime)
                .ToList();

            foreach (var message in expiredMessages)
            {
                if (_processingMessages.TryRemove(message.MessageId, out _))
                {
                    cleanedCount++;
                }
            }

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new WebSocketCleanupResultDto
            {
                Success = true,
                CleanedMessages = cleanedCount,
                ProcessingTimeMs = processingTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期消息失败");
            return new WebSocketCleanupResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    #endregion

    #region 性能监控和统计

    /// <summary>
    /// 获取WebSocket服务统计信息
    /// </summary>
    public async Task<WebSocketStatsDto> GetStatsAsync()
    {
        try
        {
            var activeConnections = await GetActiveConnectionsAsync();
            var onlineUsers = activeConnections.Select(c => c.UserId).Distinct().Count();
            var uptime = DateTime.UtcNow - _startTime;
            var messageSuccessRate = _totalMessages > 0 ? 
                (_totalMessages - _failedMessages) * 100.0 / _totalMessages : 100.0;

            return new WebSocketStatsDto
            {
                TotalConnections = (int)_totalConnections,
                ActiveConnections = activeConnections.Count(),
                OnlineUsers = onlineUsers,
                TotalMessages = _totalMessages,
                TodayMessages = await GetTodayMessageCountAsync(),
                AverageConnectionDuration = await CalculateAverageConnectionDurationAsync(),
                AverageMessageSendTime = await CalculateAverageMessageSendTimeAsync(),
                MessageSuccessRate = messageSuccessRate,
                Uptime = uptime,
                LastUpdated = DateTime.UtcNow,
                DetailedStats = new Dictionary<string, object>
                {
                    { "totalGroups", _groupConnections.Count },
                    { "queueSize", _messageQueue.Count },
                    { "processingMessages", _processingMessages.Count },
                    { "failedMessages", _failedMessages }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取统计信息失败");
            return new WebSocketStatsDto();
        }
    }

    /// <summary>
    /// 获取连接历史记录
    /// </summary>
    public async Task<IEnumerable<WebSocketConnectionHistoryDto>> GetConnectionHistoryAsync(Guid? userId = null, TimeRangeDto? timeRange = null)
    {
        try
        {
            // 这里应该从数据库或日志中获取历史记录
            // 当前返回空列表作为示例
            return Enumerable.Empty<WebSocketConnectionHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接历史失败");
            return Enumerable.Empty<WebSocketConnectionHistoryDto>();
        }
    }

    /// <summary>
    /// 获取消息发送统计
    /// </summary>
    public async Task<WebSocketMessageStatsDto> GetMessageStatsAsync(TimeRangeDto timeRange)
    {
        try
        {
            return new WebSocketMessageStatsDto
            {
                TotalMessages = _totalMessages,
                SuccessMessages = _totalMessages - _failedMessages,
                FailedMessages = _failedMessages,
                SuccessRate = _totalMessages > 0 ? 
                    (_totalMessages - _failedMessages) * 100.0 / _totalMessages : 100.0,
                AverageSendTime = await CalculateAverageMessageSendTimeAsync(),
                TimeRange = timeRange ?? new TimeRangeDto
                {
                    Start = DateTime.UtcNow.AddDays(-1),
                    End = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取消息统计失败");
            return new WebSocketMessageStatsDto();
        }
    }

    /// <summary>
    /// 获取性能指标
    /// </summary>
    public async Task<WebSocketPerformanceMetricsDto> GetPerformanceMetricsAsync(TimeRangeDto timeRange)
    {
        try
        {
            var activeConnections = await GetActiveConnectionsAsync();
            var serverLoad = CalculateServerLoad();
            var memoryUsage = GetMemoryUsage();
            var cpuUsage = GetCpuUsage();

            return new WebSocketPerformanceMetricsDto
            {
                TotalConnections = activeConnections.Count(),
                ActiveConnections = activeConnections.Count(),
                TotalMessages = _totalMessages,
                AverageConnectionTime = await CalculateAverageConnectionTimeAsync(),
                AverageMessageSendTime = await CalculateAverageMessageSendTimeAsync(),
                MessageSuccessRate = _totalMessages > 0 ? 
                    (_totalMessages - _failedMessages) * 100.0 / _totalMessages : 100.0,
                ConnectionSuccessRate = _totalConnections > 0 ? 
                    (_totalConnections - _failedMessages) * 100.0 / _totalConnections : 100.0,
                ServerLoad = serverLoad,
                MemoryUsage = memoryUsage,
                CpuUsage = cpuUsage,
                NetworkLatency = await CalculateNetworkLatencyAsync(),
                TimeRange = timeRange ?? new TimeRangeDto
                {
                    Start = DateTime.UtcNow.AddDays(-1),
                    End = DateTime.UtcNow
                },
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取性能指标失败");
            return new WebSocketPerformanceMetricsDto();
        }
    }

    /// <summary>
    /// 记录连接事件
    /// </summary>
    public async Task<bool> LogConnectionEventAsync(WebSocketConnectionEvent connectionEvent)
    {
        try
        {
            // 这里应该将事件记录到数据库或日志系统
            // 当前只是记录到日志文件
            _logger.LogInformation("连接事件: {EventType}, ConnectionId={ConnectionId}, UserId={UserId}",
                connectionEvent.EventType, connectionEvent.ConnectionId, connectionEvent.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录连接事件失败");
            return false;
        }
    }

    /// <summary>
    /// 记录消息事件
    /// </summary>
    public async Task<bool> LogMessageEventAsync(WebSocketMessageEvent messageEvent)
    {
        try
        {
            // 这里应该将事件记录到数据库或日志系统
            // 当前只是记录到日志文件
            _logger.LogInformation("消息事件: {EventType}, MessageId={MessageId}, ConnectionId={ConnectionId}",
                messageEvent.EventType, messageEvent.MessageId, messageEvent.ConnectionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录消息事件失败");
            return false;
        }
    }

    #endregion

    #region 缓存管理

    /// <summary>
    /// 清除用户连接缓存
    /// </summary>
    public async Task<bool> ClearUserConnectionCacheAsync(Guid userId)
    {
        try
        {
            var cacheKey = $"{USER_CONNECTIONS_CACHE_PREFIX}{userId}";
            await _cache.RemoveAsync(cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户连接缓存失败: UserId={UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 预热用户连接信息
    /// </summary>
    public async Task<bool> PreloadUserConnectionInfoAsync(Guid userId)
    {
        try
        {
            var connectionInfo = await GetConnectionStatusAsync(userId);
            // 预热缓存
            await CacheConnectionInfoAsync(userId.ToString(), connectionInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预热用户连接信息失败: UserId={UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 刷新连接统计缓存
    /// </summary>
    public async Task<bool> RefreshConnectionStatsCacheAsync()
    {
        try
        {
            var stats = await GetStatsAsync();
            var cacheKey = $"{STATS_CACHE_PREFIX}overall";
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新连接统计缓存失败");
            return false;
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 启动后台任务
    /// </summary>
    private async Task StartBackgroundTasksAsync()
    {
        // 清理任务
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await Task.Delay(CleanupInterval);
                    await CleanupExpiredConnectionsAsync();
                    await CleanupExpiredMessagesAsync(DateTime.UtcNow.AddHours(-1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台清理任务失败");
                }
            }
        });

        // 队列处理任务
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (!_messageQueue.IsEmpty)
                    {
                        await ProcessMessageQueueAsync(50);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台队列处理任务失败");
                }
            }
        });

        // 统计更新任务
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await Task.Delay(StatsUpdateInterval);
                    await RefreshConnectionStatsCacheAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台统计更新任务失败");
                }
            }
        });

        _logger.LogInformation("WebSocket后台任务已启动");
    }

    /// <summary>
    /// 缓存连接信息
    /// </summary>
    private async Task CacheConnectionInfoAsync(string connectionId, WebSocketConnectionInfo connectionInfo)
    {
        var cacheKey = $"{CONNECTION_CACHE_PREFIX}{connectionId}";
        var cacheData = JsonSerializer.Serialize(connectionInfo);
        await _cache.SetStringAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ConnectionTimeout
        });
    }

    /// <summary>
    /// 缓存用户连接信息
    /// </summary>
    private async Task CacheUserConnectionsAsync(Guid userId)
    {
        var cacheKey = $"{USER_CONNECTIONS_CACHE_PREFIX}{userId}";
        var connections = _userConnections.TryGetValue(userId, out var userConnections) 
            ? userConnections.Keys.ToList() 
            : new List<string>();
        var cacheData = JsonSerializer.Serialize(connections);
        await _cache.SetStringAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ConnectionTimeout
        });
    }

    /// <summary>
    /// 缓存组信息
    /// </summary>
    private async Task CacheGroupInfoAsync(string groupName)
    {
        var cacheKey = $"{GROUP_CACHE_PREFIX}{groupName}";
        var groupStats = await GetGroupStatsAsync(groupName);
        var cacheData = JsonSerializer.Serialize(groupStats);
        await _cache.SetStringAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });
    }

    /// <summary>
    /// 缓存队列状态
    /// </summary>
    private async Task CacheQueueStatusAsync()
    {
        var cacheKey = $"{MESSAGE_QUEUE_CACHE_PREFIX}status";
        var queueStatus = await GetQueueStatusAsync();
        var cacheData = JsonSerializer.Serialize(queueStatus);
        await _cache.SetStringAsync(cacheKey, cacheData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
    }

    /// <summary>
    /// 清除连接缓存
    /// </summary>
    private async Task ClearConnectionCacheAsync(string connectionId)
    {
        var cacheKey = $"{CONNECTION_CACHE_PREFIX}{connectionId}";
        await _cache.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// 清除用户连接缓存
    /// </summary>
    private async Task ClearUserConnectionsCacheAsync(Guid userId)
    {
        var cacheKey = $"{USER_CONNECTIONS_CACHE_PREFIX}{userId}";
        await _cache.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// 清除组缓存
    /// </summary>
    private async Task ClearGroupCacheAsync(string groupName)
    {
        var cacheKey = $"{GROUP_CACHE_PREFIX}{groupName}";
        await _cache.RemoveAsync(cacheKey);
    }

    /// <summary>
    /// 计算处理速率
    /// </summary>
    private double CalculateProcessingRate()
    {
        // 简化的处理速率计算
        var uptime = (DateTime.UtcNow - _startTime).TotalSeconds;
        return uptime > 0 ? _totalMessages / uptime : 0;
    }

    /// <summary>
    /// 计算平均处理时间
    /// </summary>
    private double CalculateAverageProcessingTime()
    {
        // 简化的平均处理时间计算
        return 100.0; // 毫秒
    }

    /// <summary>
    /// 获取今日消息数量
    /// </summary>
    private async Task<int> GetTodayMessageCountAsync()
    {
        // 简化的今日消息数量计算
        return (int)_totalMessages;
    }

    /// <summary>
    /// 计算平均连接时长
    /// </summary>
    private async Task<double> CalculateAverageConnectionDurationAsync()
    {
        // 简化的平均连接时长计算
        return 300.0; // 秒
    }

    /// <summary>
    /// 计算平均消息发送时间
    /// </summary>
    private async Task<double> CalculateAverageMessageSendTimeAsync()
    {
        // 简化的平均消息发送时间计算
        return 50.0; // 毫秒
    }

    /// <summary>
    /// 计算平均连接时间
    /// </summary>
    private async Task<double> CalculateAverageConnectionTimeAsync()
    {
        // 简化的平均连接时间计算
        return 100.0; // 毫秒
    }

    /// <summary>
    /// 计算服务器负载
    /// </summary>
    private double CalculateServerLoad()
    {
        // 简化的服务器负载计算
        var connectionLoad = _connections.Count / 1000.0; // 假设最大1000个连接
        var messageLoad = _messageQueue.Count / 100.0; // 假设最大100个排队消息
        return Math.Min((connectionLoad + messageLoad) * 100, 100);
    }

    /// <summary>
    /// 获取内存使用量
    /// </summary>
    private double GetMemoryUsage()
    {
        // 简化的内存使用量计算
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.PrivateMemorySize64 / (1024.0 * 1024.0); // MB
    }

    /// <summary>
    /// 获取CPU使用率
    /// </summary>
    private double GetCpuUsage()
    {
        // 简化的CPU使用率计算
        return 10.0; // 百分比
    }

    /// <summary>
    /// 计算网络延迟
    /// </summary>
    private async Task<double> CalculateNetworkLatencyAsync()
    {
        // 简化的网络延迟计算
        return 50.0; // 毫秒
    }

    #endregion
}

/// <summary>
/// WebSocket连接信息类
/// </summary>
internal class WebSocketConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public WebSocketConnectionState State { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? ConnectionDetails { get; set; }
}