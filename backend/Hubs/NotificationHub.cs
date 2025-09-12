using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace CodeSnippetManager.Api.Hubs;

/// <summary>
/// 通知Hub - 处理实时通知和WebSocket连接
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly IWebSocketService _webSocketService;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 连接信息字典
    /// </summary>
    private static readonly ConcurrentDictionary<string, HubConnectionInfo> _connectionInfos = new();

    /// <summary>
    /// 用户连接映射
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    public NotificationHub(
        ILogger<NotificationHub> logger,
        IWebSocketService webSocketService,
        IPermissionService permissionService,
        INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// 客户端连接时调用
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();
            var userName = Context.User?.Identity?.Name;
            var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString();
            var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("客户端连接: ConnectionId={ConnectionId}, UserId={UserId}, UserName={UserName}", 
                connectionId, userId, userName);

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("未授权的连接尝试: ConnectionId={ConnectionId}", connectionId);
                Context.Abort();
                return;
            }

            // 验证用户权限
            if (!await _permissionService.HasPermissionAsync(userId, "WebSocketConnect"))
            {
                _logger.LogWarning("用户没有WebSocket连接权限: UserId={UserId}", userId);
                await Clients.Caller.SendAsync("ConnectionError", new
                {
                    message = "您没有WebSocket连接权限",
                    code = 403,
                    timestamp = DateTime.UtcNow
                });
                Context.Abort();
                return;
            }

            // 创建连接信息
            var connectionInfo = new HubConnectionInfo
            {
                ConnectionId = connectionId,
                UserId = userId,
                UserName = userName,
                ConnectedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                ConnectionState = WebSocketConnectionState.Connected
            };

            // 保存连接信息
            _connectionInfos[connectionId] = connectionInfo;

            // 更新用户连接映射
            if (!_userConnections.TryGetValue(userId, out var connections))
            {
                connections = new HashSet<string>();
                _userConnections[userId] = connections;
            }
            connections.Add(connectionId);

            // 建立WebSocket连接
            var connectResult = await _webSocketService.ConnectAsync(userId, connectionId, Context.Hub);

            if (connectResult.Success)
            {
                // 加入用户个人组
                await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

                // 加入角色组
                var userRoles = Context.User?.Claims?.Where(c => c.Type == ClaimTypes.Role)?.Select(c => c.Value);
                if (userRoles != null)
                {
                    foreach (var role in userRoles)
                    {
                        await Groups.AddToGroupAsync(connectionId, $"role_{role}");
                    }
                }

                // 加入默认组
                await Groups.AddToGroupAsync(connectionId, "all_users");

                // 发送连接成功消息
                await Clients.Caller.SendAsync("ConnectionEstablished", new
                {
                    connectionId = connectionId,
                    userId = userId,
                    connectedAt = DateTime.UtcNow,
                    message = "WebSocket连接已建立",
                    serverTime = DateTime.UtcNow
                });

                // 发送未读通知数量
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCountUpdate", new
                {
                    count = unreadCount,
                    timestamp = DateTime.UtcNow
                });

                // 通知其他用户该用户上线
                await Clients.OthersInGroup($"user_{userId}").SendAsync("UserOnline", new
                {
                    userId = userId,
                    userName = userName,
                    timestamp = DateTime.UtcNow
                });

                _logger.LogInformation("WebSocket连接成功: ConnectionId={ConnectionId}, UserId={UserId}", 
                    connectionId, userId);
            }
            else
            {
                _logger.LogError("WebSocket连接失败: ConnectionId={ConnectionId}, Error={Error}", 
                    connectionId, connectResult.Error);
                
                await Clients.Caller.SendAsync("ConnectionError", new
                {
                    message = "连接建立失败",
                    error = connectResult.Error,
                    code = 500,
                    timestamp = DateTime.UtcNow
                });
                Context.Abort();
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理连接时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("ConnectionError", new
            {
                message = "连接处理失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
            Context.Abort();
        }
    }

    /// <summary>
    /// 客户端断开连接时调用
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            _logger.LogInformation("客户端断开连接: ConnectionId={ConnectionId}, UserId={UserId}, HasException={HasException}", 
                connectionId, userId, exception != null);

            if (userId != Guid.Empty && _connectionInfos.TryGetValue(connectionId, out var connectionInfo))
            {
                // 更新连接状态
                connectionInfo.DisconnectedAt = DateTime.UtcNow;
                connectionInfo.ConnectionState = WebSocketConnectionState.Disconnected;
                connectionInfo.DisconnectReason = exception?.Message ?? "正常断开";

                // 从用户连接映射中移除
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(connectionId);
                    
                    // 如果用户没有其他连接，移除用户条目
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        
                        // 通知其他用户该用户下线
                        await Clients.OthersInGroup($"user_{userId}").SendAsync("UserOffline", new
                        {
                            userId = userId,
                            userName = connectionInfo.UserName,
                            timestamp = DateTime.UtcNow
                        });
                    }
                }

                // 从连接信息字典中移除
                _connectionInfos.TryRemove(connectionId, out _);

                // 断开WebSocket连接
                await _webSocketService.DisconnectAsync(userId, connectionId);

                // 离开所有组
                await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
                await Groups.RemoveFromGroupAsync(connectionId, "all_users");

                var userRoles = Context.User?.Claims?.Where(c => c.Type == ClaimTypes.Role)?.Select(c => c.Value);
                if (userRoles != null)
                {
                    foreach (var role in userRoles)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, $"role_{role}");
                    }
                }
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "客户端异常断开连接: ConnectionId={ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理断开连接时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// 心跳检测
    /// </summary>
    public async Task Heartbeat()
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            if (_connectionInfos.TryGetValue(connectionId, out var connectionInfo))
            {
                connectionInfo.LastActivityAt = DateTime.UtcNow;
                connectionInfo.LastHeartbeatAt = DateTime.UtcNow;

                // 更新WebSocket服务中的连接状态
                await _webSocketService.ConnectAsync(userId, connectionId, Context.Hub);

                // 发送心跳响应
                await Clients.Caller.SendAsync("HeartbeatResponse", new
                {
                    timestamp = DateTime.UtcNow,
                    serverTime = DateTime.UtcNow,
                    connectionId = connectionId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理心跳时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// 加入组
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(groupName))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "组名不能为空",
                    code = 400,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            // 验证用户是否有权限加入该组
            if (!await HasGroupPermissionAsync(userId, groupName, "join"))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "您没有权限加入该组",
                    code = 403,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            await Groups.AddToGroupAsync(connectionId, groupName);
            
            // 使用WebSocket服务管理组关系
            await _webSocketService.AddToGroupAsync(connectionId, groupName);

            await Clients.Caller.SendAsync("GroupJoined", new
            {
                groupName = groupName,
                connectionId = connectionId,
                timestamp = DateTime.UtcNow,
                message = $"成功加入组: {groupName}"
            });

            _logger.LogInformation("用户加入组: UserId={UserId}, ConnectionId={ConnectionId}, GroupName={GroupName}", 
                userId, connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加入组时发生错误: ConnectionId={ConnectionId}, GroupName={GroupName}", 
                Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "加入组失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 离开组
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(groupName))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "组名不能为空",
                    code = 400,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            
            // 使用WebSocket服务管理组关系
            await _webSocketService.RemoveFromGroupAsync(connectionId, groupName);

            await Clients.Caller.SendAsync("GroupLeft", new
            {
                groupName = groupName,
                connectionId = connectionId,
                timestamp = DateTime.UtcNow,
                message = $"成功离开组: {groupName}"
            });

            _logger.LogInformation("用户离开组: UserId={UserId}, ConnectionId={ConnectionId}, GroupName={GroupName}", 
                userId, connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "离开组时发生错误: ConnectionId={ConnectionId}, GroupName={GroupName}", 
                Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "离开组失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 发送消息给指定用户
    /// </summary>
    public async Task SendToUser(Guid targetUserId, object message, string messageType = "user")
    {
        try
        {
            var senderId = GetCurrentUserId();

            if (targetUserId == Guid.Empty)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "目标用户ID不能为空",
                    code = 400,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            // 验证发送权限
            if (!await _permissionService.HasPermissionAsync(senderId, "SendMessage"))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "您没有发送消息的权限",
                    code = 403,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            var messageObj = new
                {
                    fromUserId = senderId,
                    toUserId = targetUserId,
                    content = message,
                    messageType = messageType,
                    timestamp = DateTime.UtcNow
                };

            await _webSocketService.SendToUserAsync(targetUserId, messageObj, WebSocketMessageType.User);

            await Clients.Caller.SendAsync("MessageSent", new
            {
                targetUserId = targetUserId,
                messageId = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                message = "消息发送成功"
            });

            _logger.LogInformation("用户发送消息: FromUserId={FromUserId}, ToUserId={ToUserId}", 
                senderId, targetUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送用户消息时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "发送消息失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 发送消息到组
    /// </summary>
    public async Task SendToGroup(string groupName, object message, string messageType = "group")
    {
        try
        {
            var senderId = GetCurrentUserId();

            if (string.IsNullOrEmpty(groupName))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "组名不能为空",
                    code = 400,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            // 验证发送权限
            if (!await HasGroupPermissionAsync(senderId, groupName, "send"))
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "您没有向该组发送消息的权限",
                    code = 403,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            var messageObj = new
            {
                fromUserId = senderId,
                groupName = groupName,
                content = message,
                messageType = messageType,
                timestamp = DateTime.UtcNow
            };

            await _webSocketService.SendToGroupAsync(groupName, messageObj, WebSocketMessageType.User);

            await Clients.Caller.SendAsync("MessageSent", new
            {
                groupName = groupName,
                messageId = Guid.NewGuid().ToString(),
                timestamp = DateTime.UtcNow,
                message = "消息发送成功"
            });

            _logger.LogInformation("用户发送组消息: UserId={UserId}, GroupName={GroupName}", 
                senderId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送组消息时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "发送消息失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 标记通知为已读
    /// </summary>
    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (notificationId == Guid.Empty)
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "通知ID不能为空",
                    code = 400,
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            var success = await _notificationService.MarkAsReadAsync(notificationId, userId);

            if (success)
            {
                // 更新未读计数
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCountUpdate", new
                {
                    count = unreadCount,
                    timestamp = DateTime.UtcNow
                });

                await Clients.Caller.SendAsync("NotificationMarkedAsRead", new
                {
                    notificationId = notificationId,
                    timestamp = DateTime.UtcNow,
                    message = "通知已标记为已读"
                });

                _logger.LogInformation("用户标记通知为已读: UserId={UserId}, NotificationId={NotificationId}", 
                    userId, notificationId);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    message = "标记通知为已读失败",
                    code = 500,
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "标记通知为已读时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "操作失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 获取连接状态
    /// </summary>
    public async Task GetConnectionStatus()
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            var status = await _webSocketService.GetConnectionStatusAsync(userId);

            await Clients.Caller.SendAsync("ConnectionStatus", new
            {
                connectionId = connectionId,
                userId = userId,
                isConnected = status.IsConnected,
                connectedAt = status.ConnectedAt,
                lastActivityAt = status.LastActivityAt,
                state = status.State.ToString(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取连接状态时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "获取连接状态失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 获取在线用户列表
    /// </summary>
    public async Task GetOnlineUsers()
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var onlineUsers = _userConnections.Keys
                .Where(userId => userId != currentUserId)
                .Select(userId => new
                {
                    userId = userId,
                    connectionCount = _userConnections[userId].Count
                })
                .ToList();

            await Clients.Caller.SendAsync("OnlineUsers", new
            {
                users = onlineUsers,
                totalCount = onlineUsers.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取在线用户时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "获取在线用户失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 订阅通知类型
    /// </summary>
    public async Task SubscribeToNotificationType(string notificationType)
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            var groupName = $"notification_type_{notificationType}";
            await Groups.AddToGroupAsync(connectionId, groupName);

            await Clients.Caller.SendAsync("NotificationSubscribed", new
            {
                notificationType = notificationType,
                groupName = groupName,
                timestamp = DateTime.UtcNow,
                message = $"成功订阅{notificationType}通知"
            });

            _logger.LogInformation("用户订阅通知类型: UserId={UserId}, NotificationType={NotificationType}", 
                userId, notificationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "订阅通知类型时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "订阅失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 取消订阅通知类型
    /// </summary>
    public async Task UnsubscribeFromNotificationType(string notificationType)
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;

            var groupName = $"notification_type_{notificationType}";
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            await Clients.Caller.SendAsync("NotificationUnsubscribed", new
            {
                notificationType = notificationType,
                groupName = groupName,
                timestamp = DateTime.UtcNow,
                message = $"成功取消订阅{notificationType}通知"
            });

            _logger.LogInformation("用户取消订阅通知类型: UserId={UserId}, NotificationType={NotificationType}", 
                userId, notificationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消订阅通知类型时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "取消订阅失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 强制断开连接
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task ForceDisconnect(string reason = "管理员强制断开")
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userId = GetCurrentUserId();

            _logger.LogInformation("管理员强制断开连接: UserId={UserId}, ConnectionId={ConnectionId}, Reason={Reason}", 
                userId, connectionId, reason);

            await Clients.Caller.SendAsync("ForceDisconnect", new
            {
                reason = reason,
                timestamp = DateTime.UtcNow,
                message = "连接将被强制断开"
            });

            Context.Abort();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "强制断开连接时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// 检查用户是否有组权限
    /// </summary>
    private async Task<bool> HasGroupPermissionAsync(Guid userId, string groupName, string permission)
    {
        try
        {
            // 基本权限检查
            if (groupName.StartsWith("user_") && groupName.EndsWith(userId.ToString()))
            {
                return true; // 用户对自己的个人组有完全权限
            }

            if (groupName == "all_users")
            {
                return true; // 所有用户都可以加入公共组
            }

            if (groupName.StartsWith("role_"))
            {
                var roleName = groupName.Substring(5); // 移除 "role_" 前缀
                return Context.User?.IsInRole(roleName) ?? false;
            }

            // 其他组的权限检查
            return await _permissionService.HasPermissionAsync(userId, $"group_{groupName}_{permission}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查组权限时发生错误: UserId={UserId}, GroupName={GroupName}, Permission={Permission}", 
                userId, groupName, permission);
            return false;
        }
    }

    /// <summary>
    /// 获取所有连接信息
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task GetAllConnections()
    {
        try
        {
            var connections = _connectionInfos.Values.Select(info => new
            {
                connectionId = info.ConnectionId,
                userId = info.UserId,
                userName = info.UserName,
                connectedAt = info.ConnectedAt,
                lastActivityAt = info.LastActivityAt,
                state = info.ConnectionState.ToString(),
                iPAddress = info.IPAddress,
                duration = info.DisconnectedAt.HasValue ? 
                    (info.DisconnectedAt.Value - info.ConnectedAt).TotalSeconds : 
                    (DateTime.UtcNow - info.ConnectedAt).TotalSeconds
            }).ToList();

            await Clients.Caller.SendAsync("AllConnections", new
            {
                connections = connections,
                totalCount = connections.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有连接信息时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "获取连接信息失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// 获取Hub统计信息
    /// </summary>
    [Authorize(Roles = "Admin")]
    public async Task GetHubStats()
    {
        try
        {
            var stats = new
            {
                totalConnections = _connectionInfos.Count,
                totalUsers = _userConnections.Count,
                totalGroups = _userConnections.Values.Sum(connections => connections.Count),
                averageConnectionsPerUser = _userConnections.Count > 0 ? 
                    _userConnections.Values.Average(connections => connections.Count) : 0,
                uptime = DateTime.UtcNow - _connectionInfos.Values.Min(info => info.ConnectedAt),
                lastActivity = _connectionInfos.Values.Max(info => info.LastActivityAt),
                connectionStates = _connectionInfos.Values
                    .GroupBy(info => info.ConnectionState)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };

            await Clients.Caller.SendAsync("HubStats", new
            {
                stats = stats,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取Hub统计信息时发生错误: ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new
            {
                message = "获取统计信息失败",
                error = ex.Message,
                code = 500,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

/// <summary>
/// Hub连接信息类
/// </summary>
internal class HubConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? LastHeartbeatAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public WebSocketConnectionState ConnectionState { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DisconnectReason { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}