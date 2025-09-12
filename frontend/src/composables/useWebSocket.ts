/**
 * WebSocket组合式函数
 * 
 * 提供WebSocket通信的完整状态管理和消息处理功能：
 * - 连接状态管理
 * - 消息发送和接收
 * - 自动重连和错误处理
 * - 心跳检测和性能监控
 * - 消息队列和缓存管理
 * - 类型安全的消息处理
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

import { ref, reactive, computed, watch, onMounted, onUnmounted, type Ref } from 'vue'
import { webSocketService, type WebSocketService } from '@/services/websocketService'
import type { 
  WebSocketConfig,
  WebSocketConnectionStatus,
  WebSocketMessage,
  WebSocketStats,
  WebSocketQueueStatus,
  WebSocketPerformanceMetrics,
  WebSocketConnectionState,
  WebSocketMessageType,
  WebSocketMessagePriority,
  WebSocketEventHandler,
  WebSocketConnectionResult,
  WebSocketDisconnectionResult,
  WebSocketSendMessageResult,
  WebSocketBroadcastResult,
  WebSocketGroupOperationResult
} from '@/types/websocket'

/**
 * WebSocket状态管理接口
 */
export interface WebSocketState {
  /** 连接状态 */
  connectionStatus: WebSocketConnectionStatus
  /** 统计信息 */
  stats: WebSocketStats
  /** 队列状态 */
  queueStatus: WebSocketQueueStatus
  /** 性能指标 */
  performanceMetrics: WebSocketPerformanceMetrics
  /** 错误信息 */
  error: string | null
  /** 是否正在连接 */
  isConnecting: boolean
  /** 是否正在重连 */
  isReconnecting: boolean
  /** 最后收到的消息 */
  lastMessage: WebSocketMessage | null
  /** 消息历史记录 */
  messageHistory: WebSocketMessage[]
  /** 连接历史记录 */
  connectionHistory: Array<{
    timestamp: Date
    event: string
    details?: any
  }>
}

/**
 * WebSocket操作接口
 */
export interface WebSocketActions {
  /** 连接WebSocket */
  connect: (userId?: string, token?: string) => Promise<WebSocketConnectionResult>
  /** 断开连接 */
  disconnect: (reason?: string) => Promise<WebSocketDisconnectionResult>
  /** 重新连接 */
  reconnect: () => Promise<WebSocketConnectionResult>
  /** 发送消息 */
  sendMessage: (message: any, type: WebSocketMessageType, priority?: WebSocketMessagePriority) => Promise<WebSocketSendMessageResult>
  /** 广播消息 */
  broadcastMessage: (message: any, type: WebSocketMessageType, excludeUsers?: string[]) => Promise<WebSocketBroadcastResult>
  /** 发送到组 */
  sendToGroup: (groupName: string, message: any, type: WebSocketMessageType) => Promise<WebSocketSendMessageResult>
  /** 加入组 */
  joinGroup: (groupName: string) => Promise<WebSocketGroupOperationResult>
  /** 离开组 */
  leaveGroup: (groupName: string) => Promise<WebSocketGroupOperationResult>
  /** 清除错误 */
  clearError: () => void
  /** 清空消息历史 */
  clearMessageHistory: () => void
  /** 更新状态 */
  updateStatus: () => void
  /** 注册消息处理器 */
  registerMessageHandler: (type: WebSocketMessageType, handler: (message: WebSocketMessage) => void) => void
  /** 移除消息处理器 */
  unregisterMessageHandler: (type: WebSocketMessageType) => void
  /** 注册事件处理器 */
  registerEventHandlers: (handlers: Partial<WebSocketEventHandler>) => void
  /** 移除事件处理器 */
  unregisterEventHandlers: (handlerNames: (keyof WebSocketEventHandler)[]) => void
}

/**
 * WebSocket计算属性接口
 */
export interface WebSocketGetters {
  /** 是否已连接 */
  isConnected: boolean
  /** 是否正在连接 */
  isConnecting: boolean
  /** 是否正在重连 */
  isReconnecting: boolean
  /** 连接状态文本 */
  connectionStatusText: string
  /** 连接时间 */
  connectionTime: string
  /** 心跳状态文本 */
  heartbeatStatusText: string
  /** 队列状态文本 */
  queueStateText: string
  /** 未处理消息数量 */
  pendingMessageCount: number
  /** 失败消息数量 */
  failedMessageCount: number
  /** 连接质量评分 */
  connectionQuality: 'excellent' | 'good' | 'fair' | 'poor'
  /** 按类型分组的消息 */
  messagesByType: Record<WebSocketMessageType, WebSocketMessage[]>
  /** 按优先级分组的消息 */
  messagesByPriority: Record<WebSocketMessagePriority, WebSocketMessage[]>
  /** 最近的消息 */
  recentMessages: WebSocketMessage[]
  /** 连接详情 */
  connectionDetails: {
    id?: string
    userId?: string
    connectedAt?: Date
    lastActivityAt?: Date
    reconnectCount: number
    heartbeatStatus: string
  }
}

/**
 * 使用WebSocket的组合式函数
 * @param config WebSocket配置
 * @param service 自定义WebSocket服务实例
 * @returns WebSocket状态管理对象
 */
export function useWebSocket(
  config?: Partial<WebSocketConfig>,
  service?: WebSocketService
) {
  // 响应式状态
  const state = reactive<WebSocketState>({
    connectionStatus: {
      isConnected: false,
      state: WebSocketConnectionState.DISCONNECTED,
      heartbeatStatus: 'inactive'
    },
    stats: {
      totalConnections: 0,
      activeConnections: 0,
      onlineUsers: 0,
      totalMessages: 0,
      todayMessages: 0,
      averageConnectionDuration: 0,
      averageMessageSendTime: 0,
      messageSuccessRate: 100,
      uptime: 0,
      lastUpdated: new Date(),
      detailedStats: {}
    },
    queueStatus: {
      state: 'normal',
      pendingCount: 0,
      processingCount: 0,
      failedCount: 0,
      totalCount: 0,
      queueSizeBytes: 0,
      lastProcessedAt: new Date(),
      processingRate: 0,
      averageProcessingTimeMs: 0
    },
    performanceMetrics: {
      totalConnections: 0,
      activeConnections: 0,
      totalMessages: 0,
      averageConnectionTime: 0,
      averageMessageSendTime: 0,
      messageSuccessRate: 100,
      connectionSuccessRate: 100,
      serverLoad: 0,
      memoryUsage: 0,
      cpuUsage: 0,
      networkLatency: 0,
      timeSeries: [],
      timeRange: {
        start: new Date(),
        end: new Date()
      },
      lastUpdated: new Date()
    },
    error: null,
    isConnecting: false,
    isReconnecting: false,
    lastMessage: null,
    messageHistory: [],
    connectionHistory: []
  })

  // 消息处理器映射
  const messageHandlers = new Map<WebSocketMessageType, (message: WebSocketMessage) => void>()

  // WebSocket服务实例
  const webSocketServiceRef = ref<WebSocketService>(service || webSocketService)

  // 定时器引用
  let statusUpdateInterval: NodeJS.Timeout | null = null
  let metricsUpdateInterval: NodeJS.Timeout | null = null

  // 计算属性
  const getters: WebSocketGetters = {
    isConnected: computed(() => state.connectionStatus.isConnected).value,
    isConnecting: computed(() => state.isConnecting).value,
    isReconnecting: computed(() => state.isReconnecting).value,
    
    connectionStatusText: computed(() => {
      switch (state.connectionStatus.state) {
        case WebSocketConnectionState.CONNECTED:
          return '已连接'
        case WebSocketConnectionState.CONNECTING:
          return '连接中...'
        case WebSocketConnectionState.RECONNECTING:
          return '重连中...'
        case WebSocketConnectionState.DISCONNECTED:
          return '已断开'
        case WebSocketConnectionState.ERROR:
          return '连接错误'
        case WebSocketConnectionState.TIMEOUT:
          return '连接超时'
        default:
          return '未知状态'
      }
    }).value,

    connectionTime: computed(() => {
      if (!state.connectionStatus.connectedAt) return '未连接'
      const duration = Date.now() - state.connectionStatus.connectedAt.getTime()
      return formatDuration(duration)
    }).value,

    heartbeatStatusText: computed(() => {
      switch (state.connectionStatus.heartbeatStatus) {
        case 'active':
          return '正常'
        case 'inactive':
          return '非活跃'
        case 'timeout':
          return '超时'
        default:
          return '未知'
      }
    }).value,

    queueStateText: computed(() => {
      switch (state.queueStatus.state) {
        case 'normal':
          return '正常'
        case 'busy':
          return '繁忙'
        case 'stopped':
          return '已停止'
        case 'error':
          return '错误'
        default:
          return '未知'
      }
    }).value,

    pendingMessageCount: computed(() => state.queueStatus.pendingCount).value,
    failedMessageCount: computed(() => state.queueStatus.failedCount).value,

    connectionQuality: computed(() => {
      const successRate = state.stats.messageSuccessRate
      const avgLatency = state.stats.averageMessageSendTime
      const uptime = state.stats.uptime

      if (successRate >= 99 && avgLatency < 100 && uptime > 300000) {
        return 'excellent'
      } else if (successRate >= 95 && avgLatency < 200 && uptime > 60000) {
        return 'good'
      } else if (successRate >= 90 && avgLatency < 500 && uptime > 30000) {
        return 'fair'
      } else {
        return 'poor'
      }
    }).value,

    messagesByType: computed(() => {
      const groups: Record<WebSocketMessageType, WebSocketMessage[]> = {} as any
      
      Object.values(WebSocketMessageType).forEach(type => {
        groups[type] = state.messageHistory.filter(msg => msg.type === type)
      })
      
      return groups
    }).value,

    messagesByPriority: computed(() => {
      const groups: Record<WebSocketMessagePriority, WebSocketMessage[]> = {} as any
      
      Object.values(WebSocketMessagePriority).forEach(priority => {
        groups[priority] = state.messageHistory.filter(msg => msg.priority === priority)
      })
      
      return groups
    }).value,

    recentMessages: computed(() => {
      return state.messageHistory
        .slice(-10)
        .reverse()
    }).value,

    connectionDetails: computed(() => ({
      id: state.connectionStatus.connectionId,
      userId: webSocketServiceRef.value.connectionInfo?.userId,
      connectedAt: state.connectionStatus.connectedAt,
      lastActivityAt: state.connectionStatus.lastActivityAt,
      reconnectCount: webSocketServiceRef.value.connectionInfo?.reconnectCount || 0,
      heartbeatStatus: state.connectionStatus.heartbeatStatus
    })).value
  }

  // 操作方法
  const actions: WebSocketActions = {
    async connect(userId?: string, token?: string) {
      state.isConnecting = true
      state.error = null

      try {
        // 应用配置
        if (config) {
          Object.assign(webSocketServiceRef.value.config, config)
        }

        const result = await webSocketServiceRef.value.connect(userId, token)
        
        if (result.success) {
          addConnectionHistory('connected', { connectionId: result.connectionId, userId })
        } else {
          state.error = result.error || '连接失败'
          addConnectionHistory('connect_failed', { error: state.error })
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '连接失败'
        state.error = errorMessage
        addConnectionHistory('connect_error', { error: errorMessage })
        throw error
      } finally {
        state.isConnecting = false
      }
    },

    async disconnect(reason?: string) {
      try {
        const result = await webSocketServiceRef.value.disconnect(reason)
        
        if (result.success) {
          addConnectionHistory('disconnected', { 
            connectionId: result.connectionId, 
            reason: result.reason 
          })
        } else {
          state.error = result.error || '断开连接失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '断开连接失败'
        state.error = errorMessage
        throw error
      }
    },

    async reconnect() {
      state.isReconnecting = true
      state.error = null

      try {
        const result = await webSocketServiceRef.value.reconnect()
        
        if (result.success) {
          addConnectionHistory('reconnected', { connectionId: result.connectionId })
        } else {
          state.error = result.error || '重连失败'
          addConnectionHistory('reconnect_failed', { error: state.error })
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '重连失败'
        state.error = errorMessage
        addConnectionHistory('reconnect_error', { error: errorMessage })
        throw error
      } finally {
        state.isReconnecting = false
      }
    },

    async sendMessage(message: any, type: WebSocketMessageType, priority: WebSocketMessagePriority = WebSocketMessagePriority.NORMAL) {
      try {
        const result = await webSocketServiceRef.value.sendMessage(message, type, priority)
        
        if (!result.success) {
          state.error = result.error || '发送消息失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '发送消息失败'
        state.error = errorMessage
        throw error
      }
    },

    async broadcastMessage(message: any, type: WebSocketMessageType, excludeUsers: string[] = []) {
      try {
        const result = await webSocketServiceRef.value.broadcastMessage(message, type, excludeUsers)
        
        if (!result.success) {
          state.error = result.error || '广播消息失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '广播消息失败'
        state.error = errorMessage
        throw error
      }
    },

    async sendToGroup(groupName: string, message: any, type: WebSocketMessageType) {
      try {
        const result = await webSocketServiceRef.value.sendToGroup(groupName, message, type)
        
        if (!result.success) {
          state.error = result.error || '发送组消息失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '发送组消息失败'
        state.error = errorMessage
        throw error
      }
    },

    async joinGroup(groupName: string) {
      try {
        const result = await webSocketServiceRef.value.addToGroup(groupName)
        
        if (!result.success) {
          state.error = result.error || '加入组失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '加入组失败'
        state.error = errorMessage
        throw error
      }
    },

    async leaveGroup(groupName: string) {
      try {
        const result = await webSocketServiceRef.value.removeFromGroup(groupName)
        
        if (!result.success) {
          state.error = result.error || '离开组失败'
        }

        return result
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : '离开组失败'
        state.error = errorMessage
        throw error
      }
    },

    clearError() {
      state.error = null
    },

    clearMessageHistory() {
      state.messageHistory = []
      state.lastMessage = null
    },

    updateStatus() {
      state.connectionStatus = webSocketServiceRef.value.connectionStatus
      state.stats = webSocketServiceRef.value.getStats()
      state.queueStatus = webSocketServiceRef.value.getQueueStatus()
      state.performanceMetrics = webSocketServiceRef.value.getPerformanceMetrics()
    },

    registerMessageHandler(type: WebSocketMessageType, handler: (message: WebSocketMessage) => void) {
      messageHandlers.set(type, handler)
    },

    unregisterMessageHandler(type: WebSocketMessageType) {
      messageHandlers.delete(type)
    },

    registerEventHandlers(handlers: Partial<WebSocketEventHandler>) {
      webSocketServiceRef.value.registerEventHandlers(handlers)
    },

    unregisterEventHandlers(handlerNames: (keyof WebSocketEventHandler)[]) {
      webSocketServiceRef.value.unregisterEventHandlers(handlerNames)
    }
  }

  // 内部方法
  const addConnectionHistory = (event: string, details?: any) => {
    state.connectionHistory.push({
      timestamp: new Date(),
      event,
      details
    })

    // 保持历史记录在合理范围内
    if (state.connectionHistory.length > 100) {
      state.connectionHistory = state.connectionHistory.slice(-50)
    }
  }

  const handleMessage = (message: WebSocketMessage) => {
    state.lastMessage = message
    state.messageHistory.push(message)

    // 保持消息历史在合理范围内
    if (state.messageHistory.length > 1000) {
      state.messageHistory = state.messageHistory.slice(-500)
    }

    // 调用注册的消息处理器
    const handler = messageHandlers.get(message.type)
    if (handler) {
      try {
        handler(message)
      } catch (error) {
        console.error('WebSocket消息处理器执行失败:', error)
      }
    }
  }

  const formatDuration = (milliseconds: number): string => {
    if (milliseconds < 1000) return `${milliseconds}ms`
    
    const seconds = Math.floor(milliseconds / 1000)
    const minutes = Math.floor(seconds / 60)
    const hours = Math.floor(minutes / 60)
    const days = Math.floor(hours / 24)

    if (days > 0) return `${days}d ${hours % 24}h`
    if (hours > 0) return `${hours}h ${minutes % 60}m`
    if (minutes > 0) return `${minutes}m ${seconds % 60}s`
    return `${seconds}s`
  }

  // 设置事件监听器
  const setupEventListeners = () => {
    const eventHandlers: WebSocketEventHandler = {
      onConnected: (connectionId: string, userId: string) => {
        actions.updateStatus()
        addConnectionHistory('connected', { connectionId, userId })
      },

      onDisconnected: (connectionId: string, userId: string, reason?: string) => {
        actions.updateStatus()
        addConnectionHistory('disconnected', { connectionId, userId, reason })
      },

      onError: (connectionId: string, error: Error) => {
        actions.updateStatus()
        state.error = error.message
        addConnectionHistory('error', { connectionId, error: error.message })
      },

      onReconnecting: (connectionId: string, attempt: number) => {
        state.isReconnecting = true
        actions.updateStatus()
        addConnectionHistory('reconnecting', { connectionId, attempt })
      },

      onReconnected: (connectionId: string) => {
        state.isReconnecting = false
        actions.updateStatus()
        addConnectionHistory('reconnected', { connectionId })
      },

      onMessage: handleMessage,

      onConnectionStateChange: () => {
        actions.updateStatus()
      },

      onQueueStatusChange: () => {
        actions.updateStatus()
      }
    }

    webSocketServiceRef.value.registerEventHandlers(eventHandlers)
  }

  // 启动定时器
  const startTimers = () => {
    // 状态更新定时器
    statusUpdateInterval = setInterval(() => {
      actions.updateStatus()
    }, 1000)

    // 性能指标更新定时器
    metricsUpdateInterval = setInterval(() => {
      state.performanceMetrics = webSocketServiceRef.value.getPerformanceMetrics()
    }, 10000) // 每10秒更新一次性能指标
  }

  // 停止定时器
  const stopTimers = () => {
    if (statusUpdateInterval) {
      clearInterval(statusUpdateInterval)
      statusUpdateInterval = null
    }

    if (metricsUpdateInterval) {
      clearInterval(metricsUpdateInterval)
      metricsUpdateInterval = null
    }
  }

  // 监听配置变化
  if (config) {
    watch(() => config, (newConfig) => {
      if (newConfig) {
        Object.assign(webSocketServiceRef.value.config, newConfig)
      }
    }, { deep: true })
  }

  // 生命周期钩子
  onMounted(() => {
    setupEventListeners()
    startTimers()
    actions.updateStatus()
  })

  onUnmounted(async () => {
    stopTimers()
    // 清理事件监听器
    const handlerKeys = Object.keys(webSocketServiceRef.value.eventHandlers) as (keyof WebSocketEventHandler)[]
    webSocketServiceRef.value.unregisterEventHandlers(handlerKeys)
  })

  return {
    state,
    getters,
    actions,
    // 便捷方法
    service: webSocketServiceRef
  }
}

// 导出类型
export type { WebSocketState, WebSocketActions, WebSocketGetters }

// 导出工具函数
export { useWebSocket as default }