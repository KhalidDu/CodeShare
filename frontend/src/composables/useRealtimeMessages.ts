// 实时消息更新工具函数
// 为消息列表提供WebSocket实时更新支持

import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import type { Ref } from 'vue'

export interface WebSocketOptions {
  url: string
  reconnectInterval?: number
  maxReconnectAttempts?: number
  heartbeatInterval?: number
  debug?: boolean
}

export interface RealtimeMessageResult {
  isConnected: Ref<boolean>
  connectionError: Ref<Error | null>
  connectionStatus: Ref<'connecting' | 'connected' | 'disconnected' | 'error'>
  messages: Ref<any[]>
  typingUsers: Ref<string[]>
  onlineUsers: Ref<string[]>
  connect: () => Promise<void>
  disconnect: () => void
  sendMessage: (message: any) => void
  sendTyping: (isTyping: boolean) => void
  markAsRead: (messageId: string) => void
  deleteMessage: (messageId: string) => void
  editMessage: (messageId: string, content: string) => void
  onMessage: (callback: (message: any) => void) => void
  onTyping: (callback: (userId: string, isTyping: boolean) => void) => void
  onOnlineStatus: (callback: (users: string[]) => void) => void
  onConnectionChange: (callback: (connected: boolean) => void) => void
  onError: (callback: (error: Error) => void) => void
}

export function useRealtimeMessages(
  options: WebSocketOptions
): RealtimeMessageResult {
  const {
    url,
    reconnectInterval = 3000,
    maxReconnectAttempts = 5,
    heartbeatInterval = 30000,
    debug = false
  } = options

  // 响应式状态
  const isConnected = ref(false)
  const connectionError = ref<Error | null>(null)
  const connectionStatus = ref<'connecting' | 'connected' | 'disconnected' | 'error'>('disconnected')
  const messages = ref<any[]>([])
  const typingUsers = ref<string[]>([])
  const onlineUsers = ref<string[]>([])
  
  let ws: WebSocket | null = null
  let reconnectAttempts = 0
  let heartbeatTimer: number | null = null
  let messageCallbacks: ((message: any) => void)[] = []
  let typingCallbacks: ((userId: string, isTyping: boolean) => void)[] = []
  let onlineCallbacks: ((users: string[]) => void)[] = []
  let connectionCallbacks: ((connected: boolean) => void)[] = []
  let errorCallbacks: ((error: Error) => void)[] = []

  // 日志函数
  const log = (...args: any[]) => {
    if (debug) {
      console.log('[RealtimeMessages]', ...args)
    }
  }

  // 建立WebSocket连接
  const connect = async () => {
    if (isConnected.value || connectionStatus.value === 'connecting') {
      return
    }

    connectionStatus.value = 'connecting'
    connectionError.value = null

    try {
      ws = new WebSocket(url)

      ws.onopen = () => {
        log('WebSocket连接已建立')
        isConnected.value = true
        connectionStatus.value = 'connected'
        reconnectAttempts = 0
        
        // 开始心跳
        startHeartbeat()
        
        // 通知连接变化
        connectionCallbacks.forEach(callback => callback(true))
      }

      ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data)
          handleMessage(data)
        } catch (error) {
          log('解析消息失败:', error)
        }
      }

      ws.onclose = (event) => {
        log('WebSocket连接已关闭:', event.code, event.reason)
        isConnected.value = false
        connectionStatus.value = 'disconnected'
        
        // 清理心跳
        stopHeartbeat()
        
        // 通知连接变化
        connectionCallbacks.forEach(callback => callback(false))
        
        // 尝试重连
        if (reconnectAttempts < maxReconnectAttempts) {
          reconnectAttempts++
          log(`尝试重连 (${reconnectAttempts}/${maxReconnectAttempts})`)
          setTimeout(connect, reconnectInterval)
        } else {
          connectionStatus.value = 'error'
          connectionError.value = new Error('连接失败，请刷新页面重试')
          errorCallbacks.forEach(callback => callback(connectionError.value))
        }
      }

      ws.onerror = (error) => {
        log('WebSocket连接错误:', error)
        connectionStatus.value = 'error'
        connectionError.value = new Error('连接错误')
        errorCallbacks.forEach(callback => callback(connectionError.value))
      }

    } catch (error) {
      log('创建WebSocket连接失败:', error)
      connectionStatus.value = 'error'
      connectionError.value = error instanceof Error ? error : new Error('连接失败')
      errorCallbacks.forEach(callback => callback(connectionError.value))
    }
  }

  // 断开连接
  const disconnect = () => {
    if (ws) {
      ws.close()
      ws = null
    }
    
    isConnected.value = false
    connectionStatus.value = 'disconnected'
    stopHeartbeat()
  }

  // 处理接收到的消息
  const handleMessage = (data: any) => {
    const { type, payload } = data

    switch (type) {
      case 'message':
        handleNewMessage(payload)
        break
      case 'message_updated':
        handleMessageUpdate(payload)
        break
      case 'message_deleted':
        handleMessageDelete(payload)
        break
      case 'message_read':
        handleMessageRead(payload)
        break
      case 'typing_start':
        handleTypingStart(payload)
        break
      case 'typing_stop':
        handleTypingStop(payload)
        break
      case 'online_users':
        handleOnlineUsers(payload)
        break
      case 'heartbeat':
        handleHeartbeat()
        break
      case 'error':
        handleError(payload)
        break
      default:
        log('未知消息类型:', type)
    }
  }

  // 处理新消息
  const handleNewMessage = (message: any) => {
    messages.value = [message, ...messages.value]
    messageCallbacks.forEach(callback => callback(message))
  }

  // 处理消息更新
  const handleMessageUpdate = (payload: any) => {
    const { messageId, updates } = payload
    const index = messages.value.findIndex(msg => msg.id === messageId)
    if (index !== -1) {
      messages.value[index] = { ...messages.value[index], ...updates }
    }
  }

  // 处理消息删除
  const handleMessageDelete = (payload: any) => {
    const { messageId } = payload
    messages.value = messages.value.filter(msg => msg.id !== messageId)
  }

  // 处理消息已读
  const handleMessageRead = (payload: any) => {
    const { messageIds } = payload
    messages.value = messages.value.map(msg => ({
      ...msg,
      isRead: messageIds.includes(msg.id) ? true : msg.isRead
    }))
  }

  // 处理输入状态
  const handleTypingStart = (payload: any) => {
    const { userId } = payload
    if (!typingUsers.value.includes(userId)) {
      typingUsers.value.push(userId)
    }
    typingCallbacks.forEach(callback => callback(userId, true))
  }

  const handleTypingStop = (payload: any) => {
    const { userId } = payload
    typingUsers.value = typingUsers.value.filter(id => id !== userId)
    typingCallbacks.forEach(callback => callback(userId, false))
  }

  // 处理在线用户
  const handleOnlineUsers = (payload: any) => {
    onlineUsers.value = payload.users
    onlineCallbacks.forEach(callback => callback(payload.users))
  }

  // 处理心跳
  const handleHeartbeat = () => {
    log('收到心跳响应')
  }

  // 处理错误
  const handleError = (payload: any) => {
    const error = new Error(payload.message || '服务器错误')
    errorCallbacks.forEach(callback => callback(error))
  }

  // 发送消息
  const sendMessage = (message: any) => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify({
        type: 'message',
        payload: message
      }))
    } else {
      log('WebSocket未连接，无法发送消息')
    }
  }

  // 发送输入状态
  const sendTyping = (isTyping: boolean) => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify({
        type: isTyping ? 'typing_start' : 'typing_stop',
        payload: {}
      }))
    }
  }

  // 标记消息为已读
  const markAsRead = (messageId: string) => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify({
        type: 'mark_read',
        payload: { messageId }
      }))
    }
  }

  // 删除消息
  const deleteMessage = (messageId: string) => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify({
        type: 'delete_message',
        payload: { messageId }
      }))
    }
  }

  // 编辑消息
  const editMessage = (messageId: string, content: string) => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify({
        type: 'edit_message',
        payload: { messageId, content }
      }))
    }
  }

  // 开始心跳
  const startHeartbeat = () => {
    if (heartbeatTimer) {
      clearInterval(heartbeatTimer)
    }

    heartbeatTimer = window.setInterval(() => {
      if (ws && ws.readyState === WebSocket.OPEN) {
        ws.send(JSON.stringify({ type: 'heartbeat', payload: {} }))
      }
    }, heartbeatInterval)
  }

  // 停止心跳
  const stopHeartbeat = () => {
    if (heartbeatTimer) {
      clearInterval(heartbeatTimer)
      heartbeatTimer = null
    }
  }

  // 事件监听器
  const onMessage = (callback: (message: any) => void) => {
    messageCallbacks.push(callback)
  }

  const onTyping = (callback: (userId: string, isTyping: boolean) => void) => {
    typingCallbacks.push(callback)
  }

  const onOnlineStatus = (callback: (users: string[]) => void) => {
    onlineCallbacks.push(callback)
  }

  const onConnectionChange = (callback: (connected: boolean) => void) => {
    connectionCallbacks.push(callback)
  }

  const onError = (callback: (error: Error) => void) => {
    errorCallbacks.push(callback)
  }

  // 组件挂载时自动连接
  onMounted(() => {
    connect()
  })

  // 组件卸载时断开连接
  onUnmounted(() => {
    disconnect()
  })

  return {
    isConnected,
    connectionError,
    connectionStatus,
    messages: computed(() => messages.value),
    typingUsers: computed(() => typingUsers.value),
    onlineUsers: computed(() => onlineUsers.value),
    connect,
    disconnect,
    sendMessage,
    sendTyping,
    markAsRead,
    deleteMessage,
    editMessage,
    onMessage,
    onTyping,
    onOnlineStatus,
    onConnectionChange,
    onError
  }
}

// 消息缓存管理
export function useMessageCache() {
  const cache = new Map<string, any>()
  const cacheTime = new Map<string, number>()
  const defaultTTL = 5 * 60 * 1000 // 5分钟

  // 获取缓存的消息
  const get = (key: string): any | null => {
    const cached = cache.get(key)
    const time = cacheTime.get(key)
    
    if (!cached || !time) {
      return null
    }
    
    if (Date.now() - time > defaultTTL) {
      cache.delete(key)
      cacheTime.delete(key)
      return null
    }
    
    return cached
  }

  // 设置缓存
  const set = (key: string, value: any, ttl: number = defaultTTL) => {
    cache.set(key, value)
    cacheTime.set(key, Date.now())
    
    // 设置过期时间
    setTimeout(() => {
      cache.delete(key)
      cacheTime.delete(key)
    }, ttl)
  }

  // 删除缓存
  const remove = (key: string) => {
    cache.delete(key)
    cacheTime.delete(key)
  }

  // 清空缓存
  const clear = () => {
    cache.clear()
    cacheTime.clear()
  }

  // 获取缓存大小
  const size = () => cache.size

  return {
    get,
    set,
    remove,
    clear,
    size
  }
}

// 消息状态同步
export function useMessageSync() {
  const pendingOperations = new Map<string, Promise<any>>()
  const operationQueue: Array<() => Promise<any>> = []
  const isProcessing = ref(false)

  // 执行操作
  const executeOperation = async (operation: () => Promise<any>) => {
    const operationId = Date.now().toString() + Math.random()
    
    const promise = operation()
    pendingOperations.set(operationId, promise)
    
    try {
      const result = await promise
      return result
    } finally {
      pendingOperations.delete(operationId)
    }
  }

  // 添加操作到队列
  const queueOperation = (operation: () => Promise<any>) => {
    return new Promise((resolve, reject) => {
      operationQueue.push(async () => {
        try {
          const result = await operation()
          resolve(result)
        } catch (error) {
          reject(error)
        }
      })
      
      if (!isProcessing.value) {
        processQueue()
      }
    })
  }

  // 处理操作队列
  const processQueue = async () => {
    if (isProcessing.value || operationQueue.length === 0) {
      return
    }

    isProcessing.value = true

    while (operationQueue.length > 0) {
      const operation = operationQueue.shift()
      try {
        await operation?.()
      } catch (error) {
        console.error('操作执行失败:', error)
      }
    }

    isProcessing.value = false
  }

  return {
    executeOperation,
    queueOperation,
    isProcessing: computed(() => isProcessing.value),
    pendingCount: computed(() => pendingOperations.size)
  }
}