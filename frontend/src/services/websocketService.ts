/**
 * WebSocket客户端服务
 * 
 * 提供完整的WebSocket通信功能，包括：
 * - 连接管理和状态监控
 * - 消息发送和接收处理
 * - 自动重连和错误恢复
 * - 心跳检测和性能优化
 * - 消息队列和缓存管理
 * - 认证和授权支持
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

import { v4 as uuidv4 } from 'uuid'
import type {
  WebSocketConfig,
  WebSocketConnectionStatus,
  WebSocketConnectionResult,
  WebSocketDisconnectionResult,
  WebSocketSendMessageResult,
  WebSocketBroadcastResult,
  WebSocketGroupOperationResult,
  WebSocketQueueStatus,
  WebSocketStats,
  WebSocketPerformanceMetrics,
  WebSocketMessage,
  WebSocketMessageQueueItem,
  WebSocketConnectionInfo,
  WebSocketEventHandler,
  WebSocketClient,
  WebSocketConnectionState,
  WebSocketMessageType,
  WebSocketMessagePriority,
  WebSocketMessageStatus,
  WebSocketQueueState,
  WebSocketConnectionEventType,
  WebSocketMessageEventType,
  WebSocketMessageSendStatus,
  DEFAULT_WEBSOCKET_CONFIG
} from '@/types/websocket'
import { authService } from './authService'
import { webSocketAuthService, type WebSocketAuthService } from './websocketAuthService'

/**
 * WebSocket客户端服务实现
 */
export class WebSocketService implements WebSocketClient {
  private socket: WebSocket | null = null
  private reconnectTimer: NodeJS.Timeout | null = null
  private heartbeatTimer: NodeJS.Timeout | null = null
  private queueProcessorTimer: NodeJS.Timeout | null = null
  private statsUpdateTimer: NodeJS.Timeout | null = null
  private connectionHealthTimer: NodeJS.Timeout | null = null
  private heartbeatResponseTimer: NodeJS.Timeout | null = null
  
  private _config: WebSocketConfig
  private _connectionStatus: WebSocketConnectionStatus
  private _eventHandlers: WebSocketEventHandler
  private _messageQueue: WebSocketMessageQueueItem[]
  private _connectionInfo?: WebSocketConnectionInfo
  private _isConnecting = false
  private _reconnectAttempts = 0
  private _lastHeartbeatTime = 0
  private _lastHeartbeatResponseTime = 0
  private _heartbeatMissedCount = 0
  private _connectionStartTime = 0
  private _totalConnections = 0
  private _totalMessages = 0
  private _failedMessages = 0
  private _processingMessages = new Map<string, WebSocketMessageQueueItem>()
  
  // 连接池管理
  private _connectionPool: Map<string, WebSocketService> = new Map()
  private _activeConnections: Set<string> = new Set()
  private _connectionHealth: Map<string, {
    isHealthy: boolean
    lastActivity: Date
    errorCount: number
    consecutiveErrors: number
  }> = new Map()
  
  // 认证服务
  private _authService: WebSocketAuthService
  
  // 性能监控
  private _connectionTimes: number[] = []
  private _messageSendTimes: number[] = []
  private _performanceMetrics: WebSocketPerformanceMetrics | null = null

  constructor(config: Partial<WebSocketConfig> = {}) {
    this._config = { ...DEFAULT_WEBSOCKET_CONFIG, ...config }
    this._connectionStatus = this.createInitialConnectionStatus()
    this._eventHandlers = {}
    this._messageQueue = []
    this._authService = webSocketAuthService
    
    this.initializeTimers()
    
    // 初始化连接池和性能监控
    if (this._config.enableConnectionPool) {
      this.initializeConnectionPool()
    }
    
    this.startPerformanceMonitoring()
    
    this.logDebug('WebSocket服务初始化完成', { config: this._config })
  }

  // 公共属性
  get config(): WebSocketConfig {
    return { ...this._config }
  }

  get connectionStatus(): WebSocketConnectionStatus {
    return { ...this._connectionStatus }
  }

  get eventHandlers(): WebSocketEventHandler {
    return { ...this._eventHandlers }
  }

  get messageQueue(): WebSocketMessageQueueItem[] {
    return [...this._messageQueue]
  }

  get connectionInfo(): WebSocketConnectionInfo | undefined {
    return this._connectionInfo
  }

  /**
   * 连接WebSocket服务器
   */
  async connect(userId?: string, token?: string): Promise<WebSocketConnectionResult> {
    if (this._isConnecting || this._connectionStatus.isConnected) {
      this.logDebug('连接已存在或正在连接中')
      return {
        success: false,
        userId: userId || '',
        connectedAt: new Date(),
        state: this._connectionStatus.state,
        error: '连接已存在或正在连接中'
      }
    }

    this._isConnecting = true
    const connectionId = uuidv4()
    const targetUserId = userId || this.getCurrentUserId()
    
    try {
      // 认证WebSocket连接
      if (this._config.enableConnectionPool || this._authService.config.enableAuthentication) {
        const authResult = await this._authService.authenticateConnection(connectionId, token)
        
        if (!authResult.success) {
          this._isConnecting = false
          return {
            success: false,
            userId: targetUserId,
            connectedAt: new Date(),
            state: WebSocketConnectionState.ERROR,
            error: authResult.error || '认证失败'
          }
        }
        
        this.logDebug('WebSocket连接认证成功', { 
          connectionId,
          userId: authResult.authInfo?.userId 
        })
      }

      // 构建WebSocket URL
      const wsUrl = this.buildWebSocketUrl(targetUserId, token)
      
      this.logDebug('开始建立WebSocket连接', { 
        url: wsUrl, 
        connectionId, 
        userId: targetUserId 
      })

      // 创建WebSocket连接
      this.socket = new WebSocket(wsUrl)
      this._connectionStartTime = Date.now()

      // 设置连接超时
      const connectionTimeout = setTimeout(() => {
        if (this.socket?.readyState === WebSocket.CONNECTING) {
          this.socket.close()
          this.handleConnectionError(new Error('连接超时'))
        }
      }, this._config.connectTimeout)

      // 等待连接建立
      await new Promise<void>((resolve, reject) => {
        const onOpen = () => {
          clearTimeout(connectionTimeout)
          this.socket?.removeEventListener('open', onOpen)
          this.socket?.removeEventListener('error', onError)
          resolve()
        }

        const onError = (event: Event) => {
          clearTimeout(connectionTimeout)
          this.socket?.removeEventListener('open', onOpen)
          this.socket?.removeEventListener('error', onError)
          reject(new Error('连接失败'))
        }

        this.socket?.addEventListener('open', onOpen)
        this.socket?.addEventListener('error', onError)
      })

      // 更新连接状态
      const connectedAt = new Date()
      const connectionTime = Date.now() - this._connectionStartTime
      this._connectionTimes.push(connectionTime)
      
      this._connectionStatus = {
        isConnected: true,
        connectionId,
        connectedAt,
        lastActivityAt: connectedAt,
        state: WebSocketConnectionState.CONNECTED,
        heartbeatStatus: 'active',
        reconnectCount: this._reconnectAttempts
      }

      this._connectionInfo = {
        connectionId,
        userId: targetUserId,
        connectedAt,
        lastActivityAt: connectedAt,
        state: WebSocketConnectionState.CONNECTED,
        reconnectCount: this._reconnectAttempts
      }

      this._totalConnections++
      this._reconnectAttempts = 0
      this._isConnecting = false

      // 注册事件监听器
      this.registerSocketEventListeners()

      // 启动心跳检测
      this.startHeartbeat()

      // 处理消息队列
      this.processMessageQueue()

      // 触发连接成功事件
      this.emitEvent('onConnected', connectionId, targetUserId)
      this.emitEvent('onConnectionStateChange', WebSocketConnectionState.CONNECTED)

      this.logDebug('WebSocket连接建立成功', { 
        connectionId, 
        userId: targetUserId, 
        connectionTime 
      })

      return {
        success: true,
        connectionId,
        userId: targetUserId,
        connectedAt,
        state: WebSocketConnectionState.CONNECTED,
        connectionDetails: {
          connectionTime,
          totalConnections: this._totalConnections
        }
      }

    } catch (error) {
      this._isConnecting = false
      this.handleConnectionError(error as Error)
      
      return {
        success: false,
        userId: targetUserId,
        connectedAt: new Date(),
        state: WebSocketConnectionState.ERROR,
        error: error instanceof Error ? error.message : '连接失败'
      }
    }
  }

  /**
   * 断开WebSocket连接
   */
  async disconnect(reason?: string): Promise<WebSocketDisconnectionResult> {
    if (!this.socket || !this._connectionStatus.isConnected) {
      return {
        success: false,
        userId: this._connectionInfo?.userId || '',
        disconnectedAt: new Date(),
        duration: 0,
        error: '连接不存在'
      }
    }

    const connectionId = this._connectionStatus.connectionId
    const userId = this._connectionInfo?.userId || ''
    const disconnectedAt = new Date()
    const duration = this._connectionStatus.connectedAt 
      ? Math.floor((disconnectedAt.getTime() - this._connectionStatus.connectedAt.getTime()) / 1000)
      : 0

    try {
      // 清理定时器
      this.clearAllTimers()

      // 发送断开连接消息
      if (this.socket.readyState === WebSocket.OPEN) {
        this.socket.send(JSON.stringify({
          type: 'disconnect',
          reason: reason || '客户端主动断开',
          timestamp: disconnectedAt.toISOString()
        }))
      }

      // 关闭WebSocket连接
      this.socket.close(1000, reason || 'Normal closure')

      // 更新连接状态
      this._connectionStatus = {
        ...this._connectionStatus,
        isConnected: false,
        state: WebSocketConnectionState.DISCONNECTED,
        lastActivityAt: disconnectedAt
      }

      if (this._connectionInfo) {
        this._connectionInfo.state = WebSocketConnectionState.DISCONNECTED
      }

      // 触发断开连接事件
      this.emitEvent('onDisconnected', connectionId, userId, reason)
      this.emitEvent('onConnectionStateChange', WebSocketConnectionState.DISCONNECTED)

      this.logDebug('WebSocket连接已断开', { 
        connectionId, 
        userId, 
        duration,
        reason 
      })

      return {
        success: true,
        connectionId,
        userId,
        disconnectedAt,
        duration,
        reason
      }

    } catch (error) {
      this.logError('断开WebSocket连接失败', error as Error)
      
      return {
        success: false,
        connectionId,
        userId,
        disconnectedAt,
        duration,
        error: error instanceof Error ? error.message : '断开连接失败'
      }
    }
  }

  /**
   * 重新连接WebSocket
   */
  async reconnect(): Promise<WebSocketConnectionResult> {
    if (this._reconnectAttempts >= this._config.maxReconnectAttempts!) {
      this.logDebug('达到最大重连次数，停止重连')
      return {
        success: false,
        userId: this._connectionInfo?.userId || '',
        connectedAt: new Date(),
        state: WebSocketConnectionState.ERROR,
        error: '达到最大重连次数'
      }
    }

    this._reconnectAttempts++
    const userId = this._connectionInfo?.userId
    const connectionId = this._connectionStatus.connectionId

    this.logDebug('开始重连WebSocket', { 
      attempt: this._reconnectAttempts,
      maxAttempts: this._config.maxReconnectAttempts 
    })

    // 触发重连事件
    this.emitEvent('onReconnecting', connectionId, this._reconnectAttempts)

    // 更新连接状态
    this._connectionStatus = {
      ...this._connectionStatus,
      state: WebSocketConnectionState.RECONNECTING,
      isConnected: false
    }

    // 断开现有连接
    if (this.socket) {
      this.socket.close()
    }

    // 等待重连延迟
    await new Promise(resolve => 
      setTimeout(resolve, this._config.reconnectDelay! * this._reconnectAttempts)
    )

    // 建立新连接
    const result = await this.connect(userId)

    if (result.success) {
      // 触发重连成功事件
      this.emitEvent('onReconnected', result.connectionId!)
    }

    return result
  }

  /**
   * 发送消息
   */
  async sendMessage(
    message: any, 
    type: WebSocketMessageType = WebSocketMessageType.USER,
    priority: WebSocketMessagePriority = WebSocketMessagePriority.NORMAL
  ): Promise<WebSocketSendMessageResult> {
    const startTime = Date.now()
    const messageId = uuidv4()
    const userId = this.getCurrentUserId()

    if (!this._connectionStatus.isConnected) {
      // 如果未连接，加入队列
      if (this._config.enableQueue) {
        return this.enqueueMessage({
          messageId,
          targetUserId: undefined,
          messageType: type,
          priority,
          message,
          createdAt: new Date(),
          retryCount: 0,
          maxRetries: this._config.maxRetryAttempts!,
          status: WebSocketMessageStatus.PENDING
        })
      } else {
        return {
          success: false,
          messageType: type,
          sendTimeMs: Date.now() - startTime,
          error: '连接未建立且队列功能已禁用'
        }
      }
    }

    try {
      const websocketMessage: WebSocketMessage = {
        messageId,
        type,
        data: message,
        priority,
        senderId: userId,
        sentAt: new Date(),
        status: WebSocketMessageStatus.SENDING,
        retryCount: 0,
        maxRetries: this._config.maxRetryAttempts!
      }

      const messageStr = this.serializeMessage(websocketMessage)
      
      await new Promise<void>((resolve, reject) => {
        const timeout = setTimeout(() => {
          reject(new Error('发送消息超时'))
        }, this._config.sendTimeout)

        this.socket?.send(messageStr)
        clearTimeout(timeout)
        resolve()
      })

      const sendTimeMs = Date.now() - startTime
      this._messageSendTimes.push(sendTimeMs)
      this._totalMessages++

      // 更新最后活动时间
      this._connectionStatus.lastActivityAt = new Date()

      const result: WebSocketSendMessageResult = {
        success: true,
        connectionId: this._connectionStatus.connectionId,
        userId,
        messageType: type,
        sendTimeMs,
        messageId
      }

      // 触发消息发送成功事件
      this.emitEvent('onMessageSent', result)

      this.logDebug('消息发送成功', { 
        messageId, 
        type, 
        sendTimeMs 
      })

      return result

    } catch (error) {
      this._failedMessages++
      
      const result: WebSocketSendMessageResult = {
        success: false,
        messageType: type,
        sendTimeMs: Date.now() - startTime,
        error: error instanceof Error ? error.message : '发送消息失败'
      }

      // 触发消息发送失败事件
      this.emitEvent('onMessageFailed', error as Error)

      this.logError('发送消息失败', error as Error)

      return result
    }
  }

  /**
   * 广播消息
   */
  async broadcastMessage(
    message: any, 
    type: WebSocketMessageType = WebSocketMessageType.SYSTEM,
    excludeUsers: string[] = []
  ): Promise<WebSocketBroadcastResult> {
    const startTime = Date.now()
    
    if (!this._connectionStatus.isConnected) {
      return {
        success: false,
        targetCount: 0,
        successCount: 0,
        failedCount: 0,
        messageType: type,
        sendTimeMs: Date.now() - startTime,
        error: '连接未建立'
      }
    }

    try {
      const broadcastMessage = {
        type: 'broadcast',
        messageType: type,
        data: message,
        excludeUsers,
        timestamp: new Date().toISOString(),
        messageId: uuidv4()
      }

      const messageStr = JSON.stringify(broadcastMessage)
      this.socket?.send(messageStr)

      const sendTimeMs = Date.now() - startTime

      this.logDebug('广播消息发送成功', { 
        type, 
        excludeUsers: excludeUsers.length,
        sendTimeMs 
      })

      return {
        success: true,
        targetCount: 0, // 实际目标数量由服务器确定
        successCount: 1,
        failedCount: 0,
        messageType: type,
        sendTimeMs
      }

    } catch (error) {
      this.logError('广播消息失败', error as Error)
      
      return {
        success: false,
        targetCount: 0,
        successCount: 0,
        failedCount: 0,
        messageType: type,
        sendTimeMs: Date.now() - startTime,
        error: error instanceof Error ? error.message : '广播消息失败'
      }
    }
  }

  /**
   * 发送消息到组
   */
  async sendToGroup(
    groupName: string, 
    message: any, 
    type: WebSocketMessageType = WebSocketMessageType.USER
  ): Promise<WebSocketSendMessageResult> {
    const startTime = Date.now()
    
    if (!this._connectionStatus.isConnected) {
      return {
        success: false,
        messageType: type,
        sendTimeMs: Date.now() - startTime,
        error: '连接未建立'
      }
    }

    try {
      const groupMessage = {
        type: 'group_message',
        messageType: type,
        groupName,
        data: message,
        timestamp: new Date().toISOString(),
        messageId: uuidv4()
      }

      const messageStr = JSON.stringify(groupMessage)
      this.socket?.send(messageStr)

      const sendTimeMs = Date.now() - startTime

      this.logDebug('组消息发送成功', { 
        groupName, 
        type, 
        sendTimeMs 
      })

      return {
        success: true,
        messageType: type,
        sendTimeMs
      }

    } catch (error) {
      this.logError('发送组消息失败', error as Error)
      
      return {
        success: false,
        messageType: type,
        sendTimeMs: Date.now() - startTime,
        error: error instanceof Error ? error.message : '发送组消息失败'
      }
    }
  }

  /**
   * 添加到组
   */
  async addToGroup(groupName: string): Promise<WebSocketGroupOperationResult> {
    if (!this._connectionStatus.isConnected) {
      return {
        success: false,
        operation: 'AddToGroup',
        groupName,
        error: '连接未建立'
      }
    }

    try {
      const joinMessage = {
        type: 'join_group',
        groupName,
        timestamp: new Date().toISOString()
      }

      const messageStr = JSON.stringify(joinMessage)
      this.socket?.send(messageStr)

      this.logDebug('加入组成功', { groupName })

      return {
        success: true,
        operation: 'AddToGroup',
        groupName,
        connectionId: this._connectionStatus.connectionId
      }

    } catch (error) {
      this.logError('加入组失败', error as Error)
      
      return {
        success: false,
        operation: 'AddToGroup',
        groupName,
        connectionId: this._connectionStatus.connectionId,
        error: error instanceof Error ? error.message : '加入组失败'
      }
    }
  }

  /**
   * 从组中移除
   */
  async removeFromGroup(groupName: string): Promise<WebSocketGroupOperationResult> {
    if (!this._connectionStatus.isConnected) {
      return {
        success: false,
        operation: 'RemoveFromGroup',
        groupName,
        error: '连接未建立'
      }
    }

    try {
      const leaveMessage = {
        type: 'leave_group',
        groupName,
        timestamp: new Date().toISOString()
      }

      const messageStr = JSON.stringify(leaveMessage)
      this.socket?.send(messageStr)

      this.logDebug('离开组成功', { groupName })

      return {
        success: true,
        operation: 'RemoveFromGroup',
        groupName,
        connectionId: this._connectionStatus.connectionId
      }

    } catch (error) {
      this.logError('离开组失败', error as Error)
      
      return {
        success: false,
        operation: 'RemoveFromGroup',
        groupName,
        connectionId: this._connectionStatus.connectionId,
        error: error instanceof Error ? error.message : '离开组失败'
      }
    }
  }

  /**
   * 获取连接状态
   */
  getConnectionStatus(): WebSocketConnectionStatus {
    return { ...this._connectionStatus }
  }

  /**
   * 获取队列状态
   */
  getQueueStatus(): WebSocketQueueStatus {
    const pendingCount = this._messageQueue.length
    const processingCount = this._processingMessages.size
    const failedCount = Array.from(this._processingMessages.values())
      .filter(msg => msg.status === WebSocketMessageStatus.FAILED).length

    return {
      state: pendingCount < this._config.maxQueueSize! / 2 
        ? WebSocketQueueState.NORMAL 
        : WebSocketQueueState.BUSY,
      pendingCount,
      processingCount,
      failedCount,
      totalCount: pendingCount + processingCount,
      queueSizeBytes: (pendingCount + processingCount) * 1024, // 估算大小
      lastProcessedAt: new Date(),
      processingRate: this.calculateProcessingRate(),
      averageProcessingTimeMs: this.calculateAverageProcessingTime()
    }
  }

  /**
   * 获取统计信息
   */
  getStats(): WebSocketStats {
    const uptime = this._connectionStartTime 
      ? Date.now() - this._connectionStartTime 
      : 0
    const messageSuccessRate = this._totalMessages > 0 
      ? ((this._totalMessages - this._failedMessages) / this._totalMessages) * 100 
      : 100

    return {
      totalConnections: this._totalConnections,
      activeConnections: this._connectionStatus.isConnected ? 1 : 0,
      onlineUsers: this._connectionStatus.isConnected ? 1 : 0,
      totalMessages: this._totalMessages,
      todayMessages: this._totalMessages,
      averageConnectionDuration: this.calculateAverageConnectionTime(),
      averageMessageSendTime: this.calculateAverageMessageSendTime(),
      messageSuccessRate,
      uptime,
      lastUpdated: new Date(),
      detailedStats: {
        queueSize: this._messageQueue.length,
        processingMessages: this._processingMessages.size,
        failedMessages: this._failedMessages,
        reconnectAttempts: this._reconnectAttempts
      }
    }
  }

  /**
   * 获取性能指标
   */
  getPerformanceMetrics(): WebSocketPerformanceMetrics {
    const activeConnections = this._connectionStatus.isConnected ? 1 : 0
    const serverLoad = this.calculateServerLoad()
    const messageSuccessRate = this._totalMessages > 0 
      ? ((this._totalMessages - this._failedMessages) / this._totalMessages) * 100 
      : 100
    const connectionSuccessRate = this._totalConnections > 0 
      ? ((this._totalConnections - this._reconnectAttempts) / this._totalConnections) * 100 
      : 100

    return {
      totalConnections: this._totalConnections,
      activeConnections,
      totalMessages: this._totalMessages,
      averageConnectionTime: this.calculateAverageConnectionTime(),
      averageMessageSendTime: this.calculateAverageMessageSendTime(),
      messageSuccessRate,
      connectionSuccessRate,
      serverLoad,
      memoryUsage: this.getMemoryUsage(),
      cpuUsage: this.getCpuUsage(),
      networkLatency: 50, // 简化的网络延迟
      timeSeries: [],
      timeRange: {
        start: new Date(Date.now() - 3600000), // 1小时前
        end: new Date()
      },
      lastUpdated: new Date()
    }
  }

  /**
   * 注册事件处理器
   */
  registerEventHandlers(handlers: Partial<WebSocketEventHandler>): void {
    Object.assign(this._eventHandlers, handlers)
    this.logDebug('事件处理器已注册', { 
      handlers: Object.keys(handlers) 
    })
  }

  /**
   * 移除事件处理器
   */
  unregisterEventHandlers(handlers: (keyof WebSocketEventHandler)[]): void {
    handlers.forEach(handler => {
      delete this._eventHandlers[handler]
    })
    this.logDebug('事件处理器已移除', { 
      handlers 
    })
  }

  /**
   * 发送心跳
   */
  async sendHeartbeat(): Promise<void> {
    if (!this._connectionStatus.isConnected) {
      return
    }

    try {
      const heartbeatMessage = {
        type: 'heartbeat',
        timestamp: new Date().toISOString(),
        connectionId: this._connectionStatus.connectionId,
        sequence: this._heartbeatMissedCount + 1
      }

      const messageStr = JSON.stringify(heartbeatMessage)
      this.socket?.send(messageStr)

      this._lastHeartbeatTime = Date.now()
      this._connectionStatus.lastHeartbeatAt = new Date()
      this._connectionStatus.heartbeatStatus = 'active'

      // 设置心跳响应超时检测
      this.setHeartbeatResponseTimeout()

      // 触发心跳事件
      this.emitEvent('onHeartbeat', this._connectionStatus.connectionId!)

      this.logDebug('心跳发送成功', { sequence: heartbeatMessage.sequence })

    } catch (error) {
      this.logError('发送心跳失败', error as Error)
      this._connectionStatus.heartbeatStatus = 'inactive'
      this.handleHeartbeatFailure()
    }
  }

  /**
   * 清理资源
   */
  async cleanup(): Promise<void> {
    this.logDebug('开始清理WebSocket资源')
    
    // 清理定时器
    this.clearAllTimers()
    
    // 断开连接
    await this.disconnect(' cleanup')
    
    // 清理认证
    if (this._connectionStatus.connectionId) {
      await this._authService.logoutConnection(this._connectionStatus.connectionId)
    }
    
    // 清理队列
    this._messageQueue = []
    this._processingMessages.clear()
    
    // 重置状态
    this._connectionStatus = this.createInitialConnectionStatus()
    this._connectionInfo = undefined
    this._reconnectAttempts = 0
    
    this.logDebug('WebSocket资源清理完成')
  }

  // 私有方法

  private createInitialConnectionStatus(): WebSocketConnectionStatus {
    return {
      isConnected: false,
      state: WebSocketConnectionState.DISCONNECTED,
      heartbeatStatus: 'inactive'
    }
  }

  private buildWebSocketUrl(userId: string, token?: string): string {
    const wsUrl = new URL(this._config.url)
    
    // 添加查询参数
    wsUrl.searchParams.append('userId', userId)
    
    if (token) {
      wsUrl.searchParams.append('token', token)
    } else {
      // 使用当前用户的token
      const currentToken = authService.getToken()
      if (currentToken) {
        wsUrl.searchParams.append('token', currentToken)
      }
    }
    
    // 添加连接ID
    wsUrl.searchParams.append('connectionId', uuidv4())
    
    // 添加时间戳
    wsUrl.searchParams.append('timestamp', Date.now().toString())
    
    return wsUrl.toString()
  }

  private getCurrentUserId(): string {
    const user = authService.getCurrentUser()
    return user?.id || ''
  }

  private registerSocketEventListeners(): void {
    if (!this.socket) return

    this.socket.addEventListener('open', this.handleSocketOpen.bind(this))
    this.socket.addEventListener('message', this.handleSocketMessage.bind(this))
    this.socket.addEventListener('close', this.handleSocketClose.bind(this))
    this.socket.addEventListener('error', this.handleSocketError.bind(this))
  }

  private handleSocketOpen = (event: Event): void => {
    this.logDebug('WebSocket连接已打开')
    // 连接成功处理已在connect方法中完成
  }

  private handleSocketMessage = (event: MessageEvent): void => {
    try {
      const data = JSON.parse(event.data)
      this.handleIncomingMessage(data)
    } catch (error) {
      this.logError('解析WebSocket消息失败', error as Error)
    }
  }

  private handleSocketClose = (event: CloseEvent): void => {
    this.logDebug('WebSocket连接已关闭', { 
      code: event.code, 
      reason: event.reason 
    })

    this._connectionStatus = {
      ...this._connectionStatus,
      isConnected: false,
      state: WebSocketConnectionState.DISCONNECTED
    }

    // 清理定时器
    this.clearAllTimers()

    // 触发断开连接事件
    this.emitEvent('onDisconnected', 
      this._connectionStatus.connectionId || '', 
      this._connectionInfo?.userId || '', 
      event.reason
    )

    // 自动重连
    if (this._config.autoReconnect && event.code !== 1000) {
      this.startReconnect()
    }
  }

  private handleSocketError = (event: Event): void => {
    const error = new Error('WebSocket连接错误')
    this.handleConnectionError(error)
  }

  private handleConnectionError(error: Error): void {
    this.logError('WebSocket连接错误', error)

    this._connectionStatus = {
      ...this._connectionStatus,
      isConnected: false,
      state: WebSocketConnectionState.ERROR,
      error: error.message
    }

    // 触发错误事件
    this.emitEvent('onError', this._connectionStatus.connectionId || '', error)
    this.emitEvent('onConnectionStateChange', WebSocketConnectionState.ERROR)

    // 自动重连
    if (this._config.autoReconnect) {
      this.startReconnect()
    }
  }

  private handleIncomingMessage(data: any): void {
    this.logDebug('收到WebSocket消息', { type: data.type })

    // 更新最后活动时间
    this._connectionStatus.lastActivityAt = new Date()

    // 处理不同类型的消息
    switch (data.type) {
      case 'heartbeat':
        this.handleHeartbeatResponse(data)
        break
      case 'message':
      case 'notification':
      case 'system':
      case 'user':
        this.handleApplicationMessage(data)
        break
      case 'error':
        this.handleErrorMessage(data)
        break
      case 'connection_established':
        this.handleConnectionEstablished(data)
        break
      case 'force_disconnect':
        this.handleForceDisconnect(data)
        break
      default:
        this.handleCustomMessage(data)
        break
    }
  }

  private handleHeartbeatResponse(data: any): void {
    this._lastHeartbeatResponseTime = Date.now()
    this._lastHeartbeatTime = Date.now()
    this._heartbeatMissedCount = 0
    this._connectionStatus.lastHeartbeatAt = new Date()
    this._connectionStatus.heartbeatStatus = 'active'
    
    // 清除心跳响应超时检测
    this.clearHeartbeatResponseTimer()
    
    // 更新连接健康状态
    this.updateConnectionHealth(this._connectionStatus.connectionId!, true)
    
    this.logDebug('收到心跳响应', { 
      sequence: data.sequence,
      responseTime: data.responseTime 
    })
  }

  private handleApplicationMessage(data: any): void {
    const message: WebSocketMessage = {
      messageId: data.messageId || uuidv4(),
      type: data.type as WebSocketMessageType,
      data: data.data,
      priority: data.priority || WebSocketMessagePriority.NORMAL,
      senderId: data.senderId,
      senderUsername: data.senderUsername,
      sentAt: new Date(data.timestamp || Date.now()),
      status: WebSocketMessageStatus.SENT,
      retryCount: 0,
      maxRetries: 0
    }

    // 触发消息接收事件
    this.emitEvent('onMessage', message)
  }

  private handleErrorMessage(data: any): void {
    this.logError('收到错误消息', new Error(data.message || 'Unknown error'))
  }

  private handleConnectionEstablished(data: any): void {
    this.logDebug('连接已确认', data)
  }

  private handleForceDisconnect(data: any): void {
    this.logDebug('收到强制断开连接指令', data)
    this.disconnect(data.reason || 'Server force disconnect')
  }

  private handleCustomMessage(data: any): void {
    this.logDebug('收到自定义消息', { type: data.type })
  }

  private startHeartbeat(): void {
    if (!this._config.enableHeartbeat) return

    this.clearHeartbeatTimer()
    
    this.heartbeatTimer = setInterval(() => {
      this.sendHeartbeat()
    }, this._config.heartbeatInterval)

    this.logDebug('心跳检测已启动')
  }

  private startReconnect(): void {
    if (this.reconnectTimer) return

    this.reconnectTimer = setTimeout(() => {
      this.reconnect()
    }, this._config.reconnectInterval)

    this.logDebug('重连定时器已启动')
  }

  private processMessageQueue(): void {
    if (!this._config.enableQueue || this._messageQueue.length === 0) return

    this.clearQueueProcessorTimer()

    this.queueProcessorTimer = setInterval(() => {
      this.processQueueItems()
    }, 1000) // 每秒处理一次队列

    this.logDebug('消息队列处理器已启动')
  }

  private async processQueueItems(): Promise<void> {
    const batchSize = 10 // 每次处理10条消息
    const processingItems = this._messageQueue.splice(0, batchSize)

    for (const item of processingItems) {
      try {
        await this.processQueueItem(item)
      } catch (error) {
        this.logError('处理队列消息失败', error as Error)
        
        // 重试逻辑
        if (item.retryCount < item.maxRetries) {
          item.retryCount++
          item.status = WebSocketMessageStatus.PENDING
          this._messageQueue.push(item)
        } else {
          item.status = WebSocketMessageStatus.FAILED
          item.error = error instanceof Error ? error.message : '处理失败'
          this._processingMessages.set(item.messageId, item)
        }
      }
    }

    // 触发队列状态变化事件
    this.emitEvent('onQueueStatusChange', this.getQueueStatus())
  }

  private async processQueueItem(item: WebSocketMessageQueueItem): Promise<void> {
    // 检查消息是否过期
    if (item.expiresAt && item.expiresAt < new Date()) {
      item.status = WebSocketMessageStatus.EXPIRED
      return
    }

    // 处理消息
    if (item.targetUserId) {
      await this.sendMessage(item.message, item.messageType, item.priority)
    } else if (item.targetGroup) {
      await this.sendToGroup(item.targetGroup, item.message, item.messageType)
    }

    item.status = WebSocketMessageStatus.SENT
  }

  private enqueueMessage(item: WebSocketMessageQueueItem): WebSocketSendMessageResult {
    if (this._messageQueue.length >= this._config.maxQueueSize!) {
      return {
        success: false,
        messageType: item.messageType,
        sendTimeMs: 0,
        error: '消息队列已满'
      }
    }

    this._messageQueue.push(item)

    return {
      success: true,
      messageType: item.messageType,
      sendTimeMs: 0,
      messageId: item.messageId
    }
  }

  private serializeMessage(message: WebSocketMessage): string {
    if (this._config.serialization === 'json') {
      return JSON.stringify(message)
    }
    
    // 支持其他序列化方式
    return JSON.stringify(message)
  }

  private initializeTimers(): void {
    // 启动统计更新定时器
    this.statsUpdateTimer = setInterval(() => {
      this.updatePerformanceMetrics()
    }, 60000) // 每分钟更新一次
  }

  private clearAllTimers(): void {
    this.clearReconnectTimer()
    this.clearHeartbeatTimer()
    this.clearQueueProcessorTimer()
    this.clearStatsUpdateTimer()
    this.clearHeartbeatResponseTimer()
    this.clearConnectionHealthTimer()
  }

  private clearReconnectTimer(): void {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
  }

  private clearHeartbeatTimer(): void {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer)
      this.heartbeatTimer = null
    }
  }

  private clearQueueProcessorTimer(): void {
    if (this.queueProcessorTimer) {
      clearInterval(this.queueProcessorTimer)
      this.queueProcessorTimer = null
    }
  }

  private clearStatsUpdateTimer(): void {
    if (this.statsUpdateTimer) {
      clearInterval(this.statsUpdateTimer)
      this.statsUpdateTimer = null
    }
  }

  private clearHeartbeatResponseTimer(): void {
    if (this.heartbeatResponseTimer) {
      clearTimeout(this.heartbeatResponseTimer)
      this.heartbeatResponseTimer = null
    }
  }

  private clearConnectionHealthTimer(): void {
    if (this.connectionHealthTimer) {
      clearInterval(this.connectionHealthTimer)
      this.connectionHealthTimer = null
    }
  }

  private updatePerformanceMetrics(): void {
    this._performanceMetrics = this.getPerformanceMetrics()
  }

  private calculateProcessingRate(): number {
    const uptime = this._connectionStartTime 
      ? (Date.now() - this._connectionStartTime) / 1000 
      : 1
    return this._totalMessages / uptime
  }

  private calculateAverageProcessingTime(): number {
    if (this._messageSendTimes.length === 0) return 0
    
    const totalTime = this._messageSendTimes.reduce((sum, time) => sum + time, 0)
    return totalTime / this._messageSendTimes.length
  }

  private calculateAverageConnectionTime(): number {
    if (this._connectionTimes.length === 0) return 0
    
    const totalTime = this._connectionTimes.reduce((sum, time) => sum + time, 0)
    return totalTime / this._connectionTimes.length
  }

  private calculateAverageMessageSendTime(): number {
    return this.calculateAverageProcessingTime()
  }

  private calculateServerLoad(): number {
    const connectionLoad = (this._connectionStatus.isConnected ? 1 : 0) / 10
    const queueLoad = this._messageQueue.length / this._config.maxQueueSize!
    return Math.min((connectionLoad + queueLoad) * 100, 100)
  }

  private getMemoryUsage(): number {
    // 简化的内存使用计算
    return performance.memory ? 
      performance.memory.usedJSHeapSize / (1024 * 1024) : 
      50 // 默认50MB
  }

  private getCpuUsage(): number {
    // 简化的CPU使用率计算
    return 10 // 默认10%
  }

  private emitEvent(eventName: keyof WebSocketEventHandler, ...args: any[]): void {
    const handler = this._eventHandlers[eventName]
    if (handler && typeof handler === 'function') {
      try {
        handler(...args)
      } catch (error) {
        this.logError(`事件处理器执行失败: ${eventName}`, error as Error)
      }
    }
  }

  private logDebug(message: string, data?: any): void {
    if (this._config.debug) {
      console.log(`[WebSocketService] ${message}`, data || '')
    }
  }

  private logError(message: string, error: Error): void {
    console.error(`[WebSocketService] ${message}`, error)
  }

  // 心跳检测相关方法
  private setHeartbeatResponseTimeout(): void {
    this.clearHeartbeatResponseTimer()
    
    this.heartbeatResponseTimer = setTimeout(() => {
      this.handleHeartbeatTimeout()
    }, this._config.heartbeatTimeout!)
  }

  private handleHeartbeatTimeout(): void {
    this._heartbeatMissedCount++
    this.logDebug('心跳响应超时', { 
      missedCount: this._heartbeatMissedCount,
      maxMissed: 3 // 允许的最大心跳丢失次数
    })

    if (this._heartbeatMissedCount >= 3) {
      this.handleHeartbeatFailure()
    } else {
      // 继续发送心跳
      this.sendHeartbeat()
    }
  }

  private handleHeartbeatFailure(): void {
    this.logDebug('心跳检测失败，连接可能已断开')
    
    this._connectionStatus.heartbeatStatus = 'timeout'
    this._connectionStatus.state = WebSocketConnectionState.ERROR
    
    // 更新连接健康状态
    this.updateConnectionHealth(this._connectionStatus.connectionId!, false)
    
    // 触发错误事件
    this.emitEvent('onError', this._connectionStatus.connectionId!, 
      new Error('心跳检测失败，连接可能已断开'))
    
    // 自动重连
    if (this._config.autoReconnect) {
      this.startReconnect()
    }
  }

  // 连接池管理方法
  private initializeConnectionPool(): void {
    if (!this._config.enableConnectionPool) return

    this.logDebug('初始化连接池', { 
      poolSize: this._config.connectionPoolSize 
    })

    // 启动连接健康检测
    this.startConnectionHealthCheck()
  }

  private startConnectionHealthCheck(): void {
    if (this.connectionHealthTimer) return

    this.connectionHealthTimer = setInterval(() => {
      this.checkAllConnectionsHealth()
    }, 30000) // 每30秒检查一次连接健康状态

    this.logDebug('连接健康检测已启动')
  }

  private checkAllConnectionsHealth(): void {
    const now = new Date()
    
    for (const [connectionId, health] of this._connectionHealth) {
      const timeSinceLastActivity = now.getTime() - health.lastActivity.getTime()
      
      // 如果超过5分钟没有活动，标记为不健康
      if (timeSinceLastActivity > 300000) { // 5分钟
        health.isHealthy = false
        health.consecutiveErrors++
        
        this.logDebug('连接健康状态异常', { 
          connectionId,
          timeSinceLastActivity,
          consecutiveErrors: health.consecutiveErrors 
        })
        
        // 如果连续3次检测不健康，关闭连接
        if (health.consecutiveErrors >= 3) {
          this.closeUnhealthyConnection(connectionId)
        }
      }
    }
  }

  private updateConnectionHealth(connectionId: string, isHealthy: boolean): void {
    if (!this._connectionHealth.has(connectionId)) {
      this._connectionHealth.set(connectionId, {
        isHealthy: true,
        lastActivity: new Date(),
        errorCount: 0,
        consecutiveErrors: 0
      })
    }

    const health = this._connectionHealth.get(connectionId)!
    health.isHealthy = isHealthy
    health.lastActivity = new Date()

    if (isHealthy) {
      health.consecutiveErrors = 0
    } else {
      health.errorCount++
      health.consecutiveErrors++
    }
  }

  private closeUnhealthyConnection(connectionId: string): void {
    const service = this._connectionPool.get(connectionId)
    if (service) {
      this.logDebug('关闭不健康连接', { connectionId })
      
      service.disconnect('连接健康检测失败')
        .catch(error => {
          this.logError('关闭不健康连接失败', error)
        })
      
      this._connectionPool.delete(connectionId)
      this._activeConnections.delete(connectionId)
      this._connectionHealth.delete(connectionId)
    }
  }

  // 连接池公共方法
  getConnectionFromPool(userId: string): WebSocketService | null {
    return this._connectionPool.get(userId) || null
  }

  addConnectionToPool(userId: string, service: WebSocketService): void {
    if (this._connectionPool.size >= (this._config.connectionPoolSize || 5)) {
      // 清理最老的连接
      const oldestConnectionId = this._connectionPool.keys().next().value
      if (oldestConnectionId) {
        this.closeUnhealthyConnection(oldestConnectionId)
      }
    }

    this._connectionPool.set(userId, service)
    this._activeConnections.add(userId)
    
    // 初始化连接健康状态
    this.updateConnectionHealth(userId, true)
    
    this.logDebug('连接已加入连接池', { 
      userId,
      poolSize: this._connectionPool.size 
    })
  }

  removeConnectionFromPool(userId: string): void {
    const service = this._connectionPool.get(userId)
    if (service) {
      service.disconnect('从连接池移除')
        .catch(error => {
          this.logError('从连接池移除连接失败', error)
        })
    }

    this._connectionPool.delete(userId)
    this._activeConnections.delete(userId)
    this._connectionHealth.delete(userId)
    
    this.logDebug('连接已从连接池移除', { 
      userId,
      poolSize: this._connectionPool.size 
    })
  }

  getConnectionPoolStatus(): {
    totalConnections: number
    activeConnections: number
    healthyConnections: number
    poolSize: number
    utilization: number
  } {
    const healthyConnections = Array.from(this._connectionHealth.values())
      .filter(health => health.isHealthy).length

    return {
      totalConnections: this._connectionPool.size,
      activeConnections: this._activeConnections.size,
      healthyConnections,
      poolSize: this._config.connectionPoolSize || 5,
      utilization: this._connectionPool.size / (this._config.connectionPoolSize || 5)
    }
  }

  // 性能监控方法
  private startPerformanceMonitoring(): void {
    // 启动连接健康检测
    this.startConnectionHealthCheck()
    
    // 定期清理历史数据
    setInterval(() => {
      this.cleanupHistoricalData()
    }, 300000) // 每5分钟清理一次
  }

  private cleanupHistoricalData(): void {
    // 保留最近100条连接时间记录
    if (this._connectionTimes.length > 100) {
      this._connectionTimes = this._connectionTimes.slice(-100)
    }

    // 保留最近1000条消息发送时间记录
    if (this._messageSendTimes.length > 1000) {
      this._messageSendTimes = this._messageSendTimes.slice(-1000)
    }

    this.logDebug('历史数据清理完成')
  }
}

// 创建默认实例
export const webSocketService = new WebSocketService()

// 导出单例工厂函数
export function createWebSocketService(config?: Partial<WebSocketConfig>): WebSocketService {
  return new WebSocketService(config)
}

// 导出类型
export type { WebSocketService }