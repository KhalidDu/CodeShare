<template>
  <div class="websocket-connection-manager">
    <!-- 连接状态指示器 -->
    <div class="connection-status" :class="statusClass">
      <div class="status-indicator">
        <div class="status-dot"></div>
        <span class="status-text">{{ statusText }}</span>
      </div>
      <div class="connection-info" v-if="connectionInfo">
        <span class="connection-id">ID: {{ connectionInfo.connectionId?.substring(0, 8) }}...</span>
        <span class="connection-time">{{ connectedTime }}</span>
      </div>
    </div>

    <!-- 控制按钮 -->
    <div class="connection-controls">
      <button 
        v-if="!isConnected" 
        @click="handleConnect"
        :disabled="isConnecting"
        class="btn btn-primary"
      >
        <span v-if="isConnecting" class="loading-spinner"></span>
        {{ isConnecting ? '连接中...' : '连接' }}
      </button>
      
      <button 
        v-if="isConnected" 
        @click="handleDisconnect"
        class="btn btn-secondary"
      >
        断开连接
      </button>
      
      <button 
        v-if="isConnected" 
        @click="handleReconnect"
        :disabled="isReconnecting"
        class="btn btn-warning"
      >
        <span v-if="isReconnecting" class="loading-spinner"></span>
        {{ isReconnecting ? '重连中...' : '重连' }}
      </button>
    </div>

    <!-- 连接详情 -->
    <div v-if="showDetails && connectionInfo" class="connection-details">
      <div class="details-grid">
        <div class="detail-item">
          <span class="label">连接ID:</span>
          <span class="value">{{ connectionInfo.connectionId }}</span>
        </div>
        <div class="detail-item">
          <span class="label">用户ID:</span>
          <span class="value">{{ connectionInfo.userId }}</span>
        </div>
        <div class="detail-item">
          <span class="label">连接时间:</span>
          <span class="value">{{ formatDateTime(connectionInfo.connectedAt) }}</span>
        </div>
        <div class="detail-item">
          <span class="label">最后活动:</span>
          <span class="value">{{ formatDateTime(connectionInfo.lastActivityAt) }}</span>
        </div>
        <div class="detail-item">
          <span class="label">重连次数:</span>
          <span class="value">{{ connectionInfo.reconnectCount }}</span>
        </div>
        <div class="detail-item">
          <span class="label">心跳状态:</span>
          <span class="value" :class="heartbeatStatusClass">
            {{ heartbeatStatusText }}
          </span>
        </div>
      </div>
    </div>

    <!-- 统计信息 -->
    <div v-if="showStats" class="connection-stats">
      <h3>连接统计</h3>
      <div class="stats-grid">
        <div class="stat-item">
          <div class="stat-value">{{ stats.totalMessages }}</div>
          <div class="stat-label">总消息数</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ stats.messageSuccessRate.toFixed(1) }}%</div>
          <div class="stat-label">成功率</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ formatDuration(stats.uptime) }}</div>
          <div class="stat-label">运行时间</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ stats.averageMessageSendTime.toFixed(0) }}ms</div>
          <div class="stat-label">平均延迟</div>
        </div>
      </div>
    </div>

    <!-- 队列状态 -->
    <div v-if="showQueue" class="queue-status">
      <h3>消息队列</h3>
      <div class="queue-info">
        <div class="queue-metric">
          <span class="metric-label">待处理:</span>
          <span class="metric-value">{{ queueStatus.pendingCount }}</span>
        </div>
        <div class="queue-metric">
          <span class="metric-label">处理中:</span>
          <span class="metric-value">{{ queueStatus.processingCount }}</span>
        </div>
        <div class="queue-metric">
          <span class="metric-label">失败:</span>
          <span class="metric-value">{{ queueStatus.failedCount }}</span>
        </div>
        <div class="queue-metric">
          <span class="metric-label">状态:</span>
          <span class="metric-value" :class="queueStateClass">
            {{ queueStateText }}
          </span>
        </div>
      </div>
    </div>

    <!-- 错误信息 -->
    <div v-if="error" class="error-message">
      <div class="error-icon">⚠️</div>
      <div class="error-text">{{ error }}</div>
      <button @click="clearError" class="btn btn-sm">清除</button>
    </div>

    <!-- 调试信息 -->
    <div v-if="debug" class="debug-info">
      <h3>调试信息</h3>
      <pre>{{ debugInfo }}</pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { webSocketService, type WebSocketService } from '@/services/websocketService'
import type { 
  WebSocketConnectionState, 
  WebSocketConnectionStatus,
  WebSocketStats,
  WebSocketQueueStatus,
  WebSocketConfig 
} from '@/types/websocket'

interface Props {
  // WebSocket服务实例
  service?: WebSocketService
  // 是否显示详细信息
  showDetails?: boolean
  // 是否显示统计信息
  showStats?: boolean
  // 是否显示队列状态
  showQueue?: boolean
  // 是否启用调试模式
  debug?: boolean
  // 自动连接
  autoConnect?: boolean
  // 连接配置
  config?: Partial<WebSocketConfig>
}

const props = withDefaults(defineProps<Props>(), {
  showDetails: false,
  showStats: false,
  showQueue: false,
  debug: false,
  autoConnect: false
})

const emit = defineEmits<{
  connected: [connectionId: string, userId: string]
  disconnected: [connectionId: string, userId: string, reason?: string]
  error: [error: Error]
  reconnecting: [connectionId: string, attempt: number]
  reconnected: [connectionId: string]
}>()

// 响应式数据
const service = ref<WebSocketService>(props.service || webSocketService)
const connectionStatus = ref<WebSocketConnectionStatus>(service.value.connectionStatus)
const connectionInfo = ref(service.value.connectionInfo)
const stats = ref<WebSocketStats>(service.value.getStats())
const queueStatus = ref<WebSocketQueueStatus>(service.value.getQueueStatus())
const error = ref<string>('')
const isConnecting = ref(false)
const isReconnecting = ref(false)

// 计算属性
const isConnected = computed(() => connectionStatus.value.isConnected)
const isReconnecting = computed(() => 
  connectionStatus.value.state === WebSocketConnectionState.RECONNECTING
)

const statusClass = computed(() => {
  switch (connectionStatus.value.state) {
    case WebSocketConnectionState.CONNECTED:
      return 'status-connected'
    case WebSocketConnectionState.CONNECTING:
    case WebSocketConnectionState.RECONNECTING:
      return 'status-connecting'
    case WebSocketConnectionState.DISCONNECTED:
      return 'status-disconnected'
    case WebSocketConnectionState.ERROR:
    case WebSocketConnectionState.TIMEOUT:
      return 'status-error'
    default:
      return 'status-unknown'
  }
})

const statusText = computed(() => {
  switch (connectionStatus.value.state) {
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
})

const heartbeatStatusClass = computed(() => {
  switch (connectionStatus.value.heartbeatStatus) {
    case 'active':
      return 'status-active'
    case 'inactive':
      return 'status-inactive'
    case 'timeout':
      return 'status-timeout'
    default:
      return 'status-unknown'
  }
})

const heartbeatStatusText = computed(() => {
  switch (connectionStatus.value.heartbeatStatus) {
    case 'active':
      return '正常'
    case 'inactive':
      return '非活跃'
    case 'timeout':
      return '超时'
    default:
      return '未知'
  }
})

const connectedTime = computed(() => {
  if (!connectionInfo.value?.connectedAt) return '未连接'
  return formatDuration(Date.now() - connectionInfo.value.connectedAt.getTime())
})

const queueStateClass = computed(() => {
  switch (queueStatus.value.state) {
    case 'normal':
      return 'status-normal'
    case 'busy':
      return 'status-busy'
    case 'stopped':
      return 'status-stopped'
    case 'error':
      return 'status-error'
    default:
      return 'status-unknown'
  }
})

const queueStateText = computed(() => {
  switch (queueStatus.value.state) {
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
})

const debugInfo = computed(() => {
  return {
    connectionStatus: connectionStatus.value,
    connectionInfo: connectionInfo.value,
    stats: stats.value,
    queueStatus: queueStatus.value,
    config: service.value.config
  }
})

// 方法
const handleConnect = async () => {
  if (isConnecting.value) return

  isConnecting.value = true
  clearError()

  try {
    const result = await service.value.connect()
    
    if (result.success) {
      emit('connected', result.connectionId!, result.userId)
    } else {
      error.value = result.error || '连接失败'
      emit('error', new Error(result.error))
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : '连接失败'
    emit('error', err as Error)
  } finally {
    isConnecting.value = false
  }
}

const handleDisconnect = async () => {
  try {
    const result = await service.value.disconnect('用户主动断开')
    
    if (result.success) {
      emit('disconnected', result.connectionId!, result.userId, result.reason)
    } else {
      error.value = result.error || '断开连接失败'
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : '断开连接失败'
  }
}

const handleReconnect = async () => {
  if (isReconnecting.value) return

  isReconnecting.value = true
  clearError()

  try {
    const result = await service.value.reconnect()
    
    if (result.success) {
      emit('reconnected', result.connectionId!)
    } else {
      error.value = result.error || '重连失败'
      emit('error', new Error(result.error))
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : '重连失败'
    emit('error', err as Error)
  } finally {
    isReconnecting.value = false
  }
}

const clearError = () => {
  error.value = ''
}

const formatDateTime = (date: Date): string => {
  return new Intl.DateTimeFormat('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  }).format(date)
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

// 更新状态信息
const updateStatus = () => {
  connectionStatus.value = service.value.connectionStatus
  connectionInfo.value = service.value.connectionInfo
  stats.value = service.value.getStats()
  queueStatus.value = service.value.getQueueStatus()
}

// 事件监听器
const setupEventListeners = () => {
  const handlers = {
    onConnected: (connectionId: string, userId: string) => {
      updateStatus()
      emit('connected', connectionId, userId)
    },
    onDisconnected: (connectionId: string, userId: string, reason?: string) => {
      updateStatus()
      emit('disconnected', connectionId, userId, reason)
    },
    onError: (connectionId: string, error: Error) => {
      updateStatus()
      error.value = error.message
      emit('error', error)
    },
    onReconnecting: (connectionId: string, attempt: number) => {
      updateStatus()
      emit('reconnecting', connectionId, attempt)
    },
    onReconnected: (connectionId: string) => {
      updateStatus()
      emit('reconnected', connectionId)
    },
    onConnectionStateChange: () => {
      updateStatus()
    },
    onQueueStatusChange: () => {
      updateStatus()
    }
  }

  service.value.registerEventHandlers(handlers)
}

// 清理事件监听器
const cleanupEventListeners = () => {
  const handlerKeys = Object.keys(service.value.eventHandlers) as (keyof typeof service.value.eventHandlers)[]
  service.value.unregisterEventHandlers(handlerKeys)
}

// 生命周期钩子
let updateInterval: NodeJS.Timeout

onMounted(async () => {
  // 设置事件监听器
  setupEventListeners()

  // 启动状态更新定时器
  updateInterval = setInterval(() => {
    updateStatus()
  }, 1000)

  // 应用配置
  if (props.config) {
    Object.assign(service.value.config, props.config)
  }

  // 自动连接
  if (props.autoConnect) {
    await handleConnect()
  }
})

onUnmounted(() => {
  // 清理定时器
  if (updateInterval) {
    clearInterval(updateInterval)
  }

  // 清理事件监听器
  cleanupEventListeners()
})

// 监听配置变化
watch(() => props.config, (newConfig) => {
  if (newConfig) {
    Object.assign(service.value.config, newConfig)
  }
}, { deep: true })
</script>

<style scoped>
.websocket-connection-manager {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  margin: 16px 0;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/* 连接状态 */
.connection-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  border-radius: 6px;
  margin-bottom: 16px;
  font-size: 14px;
}

.status-connected {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.status-connecting {
  background: #fff3cd;
  color: #856404;
  border: 1px solid #ffeaa7;
}

.status-disconnected {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.status-error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.status-connected .status-dot {
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.connection-info {
  display: flex;
  gap: 16px;
  font-size: 12px;
  opacity: 0.8;
}

.connection-id, .connection-time {
  font-family: monospace;
}

/* 控制按钮 */
.connection-controls {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0056b3;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #545b62;
}

.btn-warning {
  background: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background: #e0a800;
}

.btn-sm {
  padding: 4px 8px;
  font-size: 12px;
}

.loading-spinner {
  width: 12px;
  height: 12px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 连接详情 */
.connection-details {
  background: white;
  border-radius: 6px;
  padding: 16px;
  margin-bottom: 16px;
  border: 1px solid #dee2e6;
}

.details-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 12px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #eee;
}

.detail-item:last-child {
  border-bottom: none;
}

.label {
  font-weight: 500;
  color: #666;
}

.value {
  font-family: monospace;
  color: #333;
}

.status-active {
  color: #28a745;
}

.status-inactive {
  color: #ffc107;
}

.status-timeout {
  color: #dc3545;
}

/* 统计信息 */
.connection-stats {
  background: white;
  border-radius: 6px;
  padding: 16px;
  margin-bottom: 16px;
  border: 1px solid #dee2e6;
}

.connection-stats h3 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: #333;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 16px;
}

.stat-item {
  text-align: center;
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #007bff;
}

.stat-label {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
}

/* 队列状态 */
.queue-status {
  background: white;
  border-radius: 6px;
  padding: 16px;
  margin-bottom: 16px;
  border: 1px solid #dee2e6;
}

.queue-status h3 {
  margin: 0 0 12px 0;
  font-size: 16px;
  color: #333;
}

.queue-info {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
}

.queue-metric {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 4px;
}

.metric-label {
  font-size: 12px;
  color: #666;
}

.metric-value {
  font-weight: bold;
  color: #333;
}

.status-normal {
  color: #28a745;
}

.status-busy {
  color: #ffc107;
}

.status-stopped {
  color: #6c757d;
}

.status-error {
  color: #dc3545;
}

/* 错误信息 */
.error-message {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #f8d7da;
  color: #721c24;
  border-radius: 4px;
  margin-bottom: 16px;
}

.error-icon {
  font-size: 16px;
}

.error-text {
  flex: 1;
  font-size: 14px;
}

/* 调试信息 */
.debug-info {
  background: #1e1e1e;
  color: #d4d4d4;
  border-radius: 6px;
  padding: 16px;
  margin-top: 16px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
}

.debug-info h3 {
  margin: 0 0 12px 0;
  color: #569cd6;
  font-size: 14px;
}

.debug-info pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
}
</style>