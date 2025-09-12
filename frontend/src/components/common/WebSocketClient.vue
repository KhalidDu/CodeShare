<template>
  <div class="websocket-client">
    <!-- 连接控制面板 -->
    <div class="control-panel">
      <WebSocketConnectionManager
        :service="service"
        :show-details="showDetails"
        :show-stats="showStats"
        :show-queue="showQueue"
        :debug="debug"
        :auto-connect="autoConnect"
        :config="config"
        @connected="handleConnected"
        @disconnected="handleDisconnected"
        @error="handleError"
        @reconnecting="handleReconnecting"
        @reconnected="handleReconnected"
      />
    </div>

    <!-- 消息收发面板 -->
    <div class="message-panel">
      <!-- 消息发送区 -->
      <div class="message-sender" v-if="showMessageSender">
        <div class="sender-header">
          <h3>发送消息</h3>
          <div class="sender-controls">
            <button 
              @click="clearMessageHistory"
              class="btn btn-sm btn-secondary"
              :disabled="!state.messageHistory.length"
            >
              清空历史
            </button>
            <button 
              @click="exportMessageHistory"
              class="btn btn-sm btn-info"
              :disabled="!state.messageHistory.length"
            >
              导出历史
            </button>
          </div>
        </div>

        <div class="message-form">
          <div class="form-group">
            <label>消息类型</label>
            <select v-model="messageForm.type" class="form-control">
              <option v-for="type in messageTypes" :key="type" :value="type">
                {{ getMessageTypeLabel(type) }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>优先级</label>
            <select v-model="messageForm.priority" class="form-control">
              <option v-for="priority in messagePriorities" :key="priority" :value="priority">
                {{ getMessagePriorityLabel(priority) }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>目标类型</label>
            <div class="radio-group">
              <label class="radio-label">
                <input type="radio" v-model="messageForm.targetType" value="user" />
                单用户
              </label>
              <label class="radio-label">
                <input type="radio" v-model="messageForm.targetType" value="group" />
                组消息
              </label>
              <label class="radio-label">
                <input type="radio" v-model="messageForm.targetType" value="broadcast" />
                广播
              </label>
            </div>
          </div>

          <div class="form-group" v-if="messageForm.targetType === 'user'">
            <label>目标用户ID</label>
            <input 
              v-model="messageForm.targetUserId" 
              type="text" 
              class="form-control"
              placeholder="输入接收者用户ID"
            />
          </div>

          <div class="form-group" v-if="messageForm.targetType === 'group'">
            <label>组名</label>
            <input 
              v-model="messageForm.targetGroup" 
              type="text" 
              class="form-control"
              placeholder="输入目标组名"
            />
            <div class="group-actions">
              <button 
                @click="joinGroup(messageForm.targetGroup)"
                class="btn btn-sm btn-success"
                :disabled="!messageForm.targetGroup"
              >
                加入组
              </button>
              <button 
                @click="leaveGroup(messageForm.targetGroup)"
                class="btn btn-sm btn-warning"
                :disabled="!messageForm.targetGroup"
              >
                离开组
              </button>
            </div>
          </div>

          <div class="form-group">
            <label>消息内容</label>
            <textarea 
              v-model="messageForm.content" 
              class="form-control"
              rows="4"
              placeholder="输入消息内容 (支持JSON格式)"
            ></textarea>
            <div class="format-hint">
              <label>
                <input type="checkbox" v-model="messageForm.isJson" />
                JSON格式
              </label>
            </div>
          </div>

          <div class="form-actions">
            <button 
              @click="sendMessage"
              class="btn btn-primary"
              :disabled="!canSendMessage"
            >
              <span v-if="isSending" class="loading-spinner"></span>
              {{ isSending ? '发送中...' : '发送消息' }}
            </button>
            <button 
              @click="clearForm"
              class="btn btn-secondary"
            >
              清空表单
            </button>
          </div>
        </div>

        <!-- 发送结果显示 -->
        <div v-if="lastSendResult" class="send-result" :class="lastSendResult.success ? 'success' : 'error'">
          <div class="result-icon">{{ lastSendResult.success ? '✓' : '✗' }}</div>
          <div class="result-details">
            <div class="result-message">{{ lastSendResult.success ? '发送成功' : '发送失败' }}</div>
            <div v-if="lastSendResult.error" class="result-error">{{ lastSendResult.error }}</div>
            <div v-if="lastSendResult.sendTimeMs" class="result-time">
              耗时: {{ lastSendResult.sendTimeMs }}ms
            </div>
          </div>
        </div>
      </div>

      <!-- 消息显示区 -->
      <div class="message-display" v-if="showMessageDisplay">
        <WebSocketMessageHandler
          :show-filter="showMessageFilter"
          :show-stats="showMessageStats"
          :show-pagination="showMessagePagination"
          :enable-scroll="enableMessageScroll"
          :page-size="messagePageSize"
          :max-history-size="maxMessageHistory"
          :auto-mark-as-read="autoMarkMessagesAsRead"
          :enable-sound="enableMessageSound"
          @messageClick="handleMessageClick"
          @messageRead="handleMessageRead"
          @messageDelete="handleMessageDelete"
          @filterChange="handleMessageFilterChange"
        />
      </div>
    </div>

    <!-- 实时统计面板 -->
    <div class="stats-panel" v-if="showRealtimeStats">
      <h3>实时统计</h3>
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-header">
            <span class="stat-title">连接状态</span>
            <span class="stat-indicator" :class="connectionStatusClass"></span>
          </div>
          <div class="stat-value">{{ getters.connectionStatusText }}</div>
          <div class="stat-subtitle">{{ getters.connectionTime }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-header">
            <span class="stat-title">消息统计</span>
          </div>
          <div class="stat-value">{{ state.stats.totalMessages }}</div>
          <div class="stat-subtitle">
            成功率: {{ state.stats.messageSuccessRate.toFixed(1) }}%
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-header">
            <span class="stat-title">队列状态</span>
          </div>
          <div class="stat-value">{{ state.queueStatus.pendingCount }}</div>
          <div class="stat-subtitle">{{ getters.queueStateText }}</div>
        </div>

        <div class="stat-card">
          <div class="stat-header">
            <span class="stat-title">连接质量</span>
          </div>
          <div class="stat-value" :class="qualityClass">
            {{ qualityScore }}
          </div>
          <div class="stat-subtitle">
            延迟: {{ state.stats.averageMessageSendTime.toFixed(0) }}ms
          </div>
        </div>
      </div>

      <!-- 性能图表 -->
      <div class="performance-chart" v-if="showPerformanceChart">
        <h4>性能趋势</h4>
        <div class="chart-container">
          <canvas ref="performanceChart"></canvas>
        </div>
      </div>
    </div>

    <!-- 调试面板 -->
    <div class="debug-panel" v-if="debug">
      <h3>调试信息</h3>
      <div class="debug-tabs">
        <button 
          v-for="tab in debugTabs" 
          :key="tab.id"
          @click="activeDebugTab = tab.id"
          class="debug-tab"
          :class="{ active: activeDebugTab === tab.id }"
        >
          {{ tab.label }}
        </button>
      </div>

      <div class="debug-content">
        <div v-if="activeDebugTab === 'connection'" class="debug-section">
          <h4>连接信息</h4>
          <pre>{{ formatDebugInfo(getters.connectionDetails) }}</pre>
        </div>

        <div v-if="activeDebugTab === 'messages'" class="debug-section">
          <h4>最近消息</h4>
          <div class="recent-messages">
            <div 
              v-for="message in getters.recentMessages" 
              :key="message.messageId"
              class="debug-message"
            >
              <div class="message-meta">
                <span class="message-type">{{ message.type }}</span>
                <span class="message-time">{{ formatTime(message.sentAt) }}</span>
              </div>
              <div class="message-content">{{ formatJson(message.data) }}</div>
            </div>
          </div>
        </div>

        <div v-if="activeDebugTab === 'performance'" class="debug-section">
          <h4>性能指标</h4>
          <pre>{{ formatDebugInfo(state.performanceMetrics) }}</pre>
        </div>

        <div v-if="activeDebugTab === 'config'" class="debug-section">
          <h4>配置信息</h4>
          <pre>{{ formatDebugInfo(service.config) }}</pre>
        </div>
      </div>
    </div>

    <!-- 消息详情弹窗 -->
    <div v-if="selectedMessage" class="message-modal" @click.self="closeMessageDetail">
      <div class="modal-content">
        <div class="modal-header">
          <h3>消息详情</h3>
          <button @click="closeMessageDetail" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <div class="detail-item">
            <span class="detail-label">消息ID:</span>
            <span class="detail-value">{{ selectedMessage.messageId }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">类型:</span>
            <span class="detail-value">{{ getMessageTypeLabel(selectedMessage.type) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">优先级:</span>
            <span class="detail-value">{{ getMessagePriorityLabel(selectedMessage.priority) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">状态:</span>
            <span class="detail-value">{{ getMessageStatusLabel(selectedMessage.status) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">发送时间:</span>
            <span class="detail-value">{{ formatDateTime(selectedMessage.sentAt) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">发送者:</span>
            <span class="detail-value">{{ selectedMessage.senderUsername || selectedMessage.senderId || '未知' }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">重试次数:</span>
            <span class="detail-value">{{ selectedMessage.retryCount }} / {{ selectedMessage.maxRetries }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">消息内容:</span>
            <div class="message-data-content">
              <pre>{{ formatJson(selectedMessage.data, true) }}</pre>
            </div>
          </div>
          <div v-if="selectedMessage.error" class="detail-item">
            <span class="detail-label">错误信息:</span>
            <span class="detail-value error">{{ selectedMessage.error }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useWebSocket, type WebSocketService } from '@/composables/useWebSocket'
import WebSocketConnectionManager from './WebSocketConnectionManager.vue'
import WebSocketMessageHandler from './WebSocketMessageHandler.vue'
import type { 
  WebSocketConfig,
  WebSocketMessageType,
  WebSocketMessagePriority,
  WebSocketMessageStatus,
  WebSocketMessage,
  WebSocketConnectionResult,
  WebSocketDisconnectionResult,
  WebSocketSendMessageResult,
  WebSocketBroadcastResult,
  WebSocketGroupOperationResult
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
  // 是否显示消息发送器
  showMessageSender?: boolean
  // 是否显示消息显示器
  showMessageDisplay?: boolean
  // 是否显示实时统计
  showRealtimeStats?: boolean
  // 是否显示性能图表
  showPerformanceChart?: boolean
  // 是否显示消息过滤器
  showMessageFilter?: boolean
  // 是否显示消息统计
  showMessageStats?: boolean
  // 是否显示消息分页
  showMessagePagination?: boolean
  // 是否启用消息滚动
  enableMessageScroll?: boolean
  // 消息页面大小
  messagePageSize?: number
  // 最大消息历史记录
  maxMessageHistory?: number
  // 自动标记消息已读
  autoMarkMessagesAsRead?: boolean
  // 启用消息声音
  enableMessageSound?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showDetails: false,
  showStats: true,
  showQueue: false,
  debug: false,
  autoConnect: false,
  showMessageSender: true,
  showMessageDisplay: true,
  showRealtimeStats: true,
  showPerformanceChart: false,
  showMessageFilter: true,
  showMessageStats: true,
  showMessagePagination: false,
  enableMessageScroll: true,
  messagePageSize: 20,
  maxMessageHistory: 1000,
  autoMarkMessagesAsRead: false,
  enableMessageSound: false
})

const emit = defineEmits<{
  connected: [connectionId: string, userId: string]
  disconnected: [connectionId: string, userId: string, reason?: string]
  error: [error: Error]
  reconnecting: [connectionId: string, attempt: number]
  reconnected: [connectionId: string]
  messageSent: [result: WebSocketSendMessageResult]
  messageReceived: [message: WebSocketMessage]
  messageClick: [message: WebSocketMessage]
  messageRead: [message: WebSocketMessage]
  messageDelete: [message: WebSocketMessage]
  filterChange: [filters: any]
  statsUpdate: [stats: any]
}>()

// WebSocket状态管理
const { state, getters, actions } = useWebSocket(props.config, props.service)

// 响应式数据
const service = computed(() => props.service || actions.service)
const isSending = ref(false)
const lastSendResult = ref<WebSocketSendMessageResult | null>(null)
const selectedMessage = ref<WebSocketMessage | null>(null)
const activeDebugTab = ref('connection')
const performanceChart = ref<HTMLCanvasElement | null>(null)

// 消息表单
const messageForm = reactive({
  type: WebSocketMessageType.USER,
  priority: WebSocketMessagePriority.NORMAL,
  targetType: 'broadcast' as 'user' | 'group' | 'broadcast',
  targetUserId: '',
  targetGroup: '',
  content: '',
  isJson: false
})

// 消息类型和优先级选项
const messageTypes = Object.values(WebSocketMessageType)
const messagePriorities = Object.values(WebSocketMessagePriority)

// 调试标签页
const debugTabs = [
  { id: 'connection', label: '连接' },
  { id: 'messages', label: '消息' },
  { id: 'performance', label: '性能' },
  { id: 'config', label: '配置' }
]

// 计算属性
const canSendMessage = computed(() => {
  return getters.isConnected && 
         messageForm.content.trim() && 
         !isSending.value &&
         (messageForm.targetType === 'broadcast' || 
          (messageForm.targetType === 'user' && messageForm.targetUserId) ||
          (messageForm.targetType === 'group' && messageForm.targetGroup))
})

const connectionStatusClass = computed(() => {
  return state.connectionStatus.isConnected ? 'connected' : 'disconnected'
})

const qualityScore = computed(() => {
  const quality = getters.connectionQuality
  const scores = {
    excellent: '优秀',
    good: '良好',
    fair: '一般',
    poor: '较差'
  }
  return scores[quality]
})

const qualityClass = computed(() => {
  return getters.connectionQuality
})

// 方法
const getMessageTypeLabel = (type: WebSocketMessageType): string => {
  const labels: Record<WebSocketMessageType, string> = {
    [WebSocketMessageType.NOTIFICATION]: '通知',
    [WebSocketMessageType.SYSTEM]: '系统',
    [WebSocketMessageType.USER]: '用户',
    [WebSocketMessageType.STATUS_UPDATE]: '状态更新',
    [WebSocketMessageType.ERROR]: '错误',
    [WebSocketMessageType.ACKNOWLEDGMENT]: '确认',
    [WebSocketMessageType.HEARTBEAT]: '心跳',
    [WebSocketMessageType.CUSTOM]: '自定义'
  }
  return labels[type] || type
}

const getMessagePriorityLabel = (priority: WebSocketMessagePriority): string => {
  const labels: Record<WebSocketMessagePriority, string> = {
    [WebSocketMessagePriority.LOW]: '低',
    [WebSocketMessagePriority.NORMAL]: '普通',
    [WebSocketMessagePriority.HIGH]: '高',
    [WebSocketMessagePriority.URGENT]: '紧急'
  }
  return labels[priority] || priority
}

const getMessageStatusLabel = (status: WebSocketMessageStatus): string => {
  const labels: Record<WebSocketMessageStatus, string> = {
    ['pending' as WebSocketMessageStatus]: '待发送',
    ['sending' as WebSocketMessageStatus]: '发送中',
    ['sent' as WebSocketMessageStatus]: '已发送',
    ['failed' as WebSocketMessageStatus]: '发送失败',
    ['expired' as WebSocketMessageStatus]: '已过期',
    ['cancelled' as WebSocketMessageStatus]: '已取消',
    ['unread' as WebSocketMessageStatus]: '未读'
  }
  return labels[status] || status
}

const formatTime = (date: Date): string => {
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) {
    return '刚刚'
  } else if (diff < 3600000) {
    return `${Math.floor(diff / 60000)}分钟前`
  } else if (diff < 86400000) {
    return `${Math.floor(diff / 3600000)}小时前`
  } else {
    return date.toLocaleTimeString('zh-CN', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }
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

const formatJson = (data: any, pretty = false): string => {
  try {
    if (pretty) {
      return JSON.stringify(data, null, 2)
    }
    return JSON.stringify(data)
  } catch {
    return String(data)
  }
}

const formatDebugInfo = (data: any): string => {
  return JSON.stringify(data, null, 2)
}

// 消息发送
const sendMessage = async () => {
  if (!canSendMessage.value) return

  isSending.value = true
  lastSendResult.value = null

  try {
    let messageData: any

    if (messageForm.isJson) {
      try {
        messageData = JSON.parse(messageForm.content)
      } catch (error) {
        throw new Error('JSON格式错误')
      }
    } else {
      messageData = messageForm.content
    }

    let result: WebSocketSendMessageResult | WebSocketBroadcastResult

    switch (messageForm.targetType) {
      case 'user':
        result = await actions.sendMessage(messageData, messageForm.type, messageForm.priority)
        break
      case 'group':
        result = await actions.sendToGroup(messageForm.targetGroup, messageData, messageForm.type)
        break
      case 'broadcast':
        result = await actions.broadcastMessage(messageData, messageForm.type)
        break
      default:
        throw new Error('无效的目标类型')
    }

    lastSendResult.value = result as WebSocketSendMessageResult
    
    if (result.success) {
      emit('messageSent', result as WebSocketSendMessageResult)
      clearForm()
    }

  } catch (error) {
    lastSendResult.value = {
      success: false,
      messageType: messageForm.type,
      sendTimeMs: 0,
      error: error instanceof Error ? error.message : '发送失败'
    }
  } finally {
    isSending.value = false
  }
}

// 组操作
const joinGroup = async (groupName: string) => {
  if (!groupName || !getters.isConnected) return

  try {
    const result = await actions.joinGroup(groupName)
    if (result.success) {
      console.log(`成功加入组: ${groupName}`)
    }
  } catch (error) {
    console.error('加入组失败:', error)
  }
}

const leaveGroup = async (groupName: string) => {
  if (!groupName || !getters.isConnected) return

  try {
    const result = await actions.leaveGroup(groupName)
    if (result.success) {
      console.log(`成功离开组: ${groupName}`)
    }
  } catch (error) {
    console.error('离开组失败:', error)
  }
}

// 表单操作
const clearForm = () => {
  messageForm.content = ''
  messageForm.targetUserId = ''
  messageForm.targetGroup = ''
  lastSendResult.value = null
}

const clearMessageHistory = () => {
  actions.clearMessageHistory()
}

const exportMessageHistory = () => {
  const data = {
    exportTime: new Date().toISOString(),
    messageCount: state.messageHistory.length,
    messages: state.messageHistory
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `websocket-messages-${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
}

// 事件处理
const handleConnected = (connectionId: string, userId: string) => {
  emit('connected', connectionId, userId)
}

const handleDisconnected = (connectionId: string, userId: string, reason?: string) => {
  emit('disconnected', connectionId, userId, reason)
}

const handleError = (error: Error) => {
  emit('error', error)
}

const handleReconnecting = (connectionId: string, attempt: number) => {
  emit('reconnecting', connectionId, attempt)
}

const handleReconnected = (connectionId: string) => {
  emit('reconnected', connectionId)
}

const handleMessageClick = (message: WebSocketMessage) => {
  selectedMessage.value = message
  emit('messageClick', message)
}

const handleMessageRead = (message: WebSocketMessage) => {
  emit('messageRead', message)
}

const handleMessageDelete = (message: WebSocketMessage) => {
  emit('messageDelete', message)
}

const handleMessageFilterChange = (filters: any) => {
  emit('filterChange', filters)
}

const closeMessageDetail = () => {
  selectedMessage.value = null
}

// 监听新消息
watch(() => state.lastMessage, (newMessage) => {
  if (newMessage) {
    emit('messageReceived', newMessage)
  }
})

// 监听统计信息变化
watch(() => state.stats, (newStats) => {
  emit('statsUpdate', newStats)
}, { deep: true })

// 生命周期钩子
onMounted(() => {
  // 注册消息处理器
  actions.registerMessageHandler(WebSocketMessageType.NOTIFICATION, (message) => {
    console.log('收到通知消息:', message)
  })
  
  actions.registerMessageHandler(WebSocketMessageType.SYSTEM, (message) => {
    console.log('收到系统消息:', message)
  })
  
  actions.registerMessageHandler(WebSocketMessageType.ERROR, (message) => {
    console.error('收到错误消息:', message)
  })

  // 自动连接
  if (props.autoConnect) {
    actions.connect().catch(error => {
      console.error('自动连接失败:', error)
    })
  }
})

onUnmounted(() => {
  // 清理消息处理器
  actions.unregisterMessageHandler(WebSocketMessageType.NOTIFICATION)
  actions.unregisterMessageHandler(WebSocketMessageType.SYSTEM)
  actions.unregisterMessageHandler(WebSocketMessageType.ERROR)
})
</script>

<style scoped>
.websocket-client {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

/* 控制面板 */
.control-panel {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

/* 消息面板 */
.message-panel {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 20px;
  margin-bottom: 20px;
}

@media (max-width: 768px) {
  .message-panel {
    grid-template-columns: 1fr;
  }
}

/* 消息发送区 */
.message-sender {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.sender-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.sender-header h3 {
  margin: 0;
  color: #333;
}

.sender-controls {
  display: flex;
  gap: 8px;
}

.message-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-group label {
  font-weight: 500;
  color: #333;
}

.form-control {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.form-control:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
}

.radio-group {
  display: flex;
  gap: 16px;
}

.radio-label {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}

.group-actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}

.format-hint {
  margin-top: 4px;
}

.form-actions {
  display: flex;
  gap: 12px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
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

.btn-success {
  background: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #1e7e34;
}

.btn-warning {
  background: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background: #e0a800;
}

.btn-info {
  background: #17a2b8;
  color: white;
}

.btn-info:hover:not(:disabled) {
  background: #117a8b;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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
  margin-right: 6px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 发送结果 */
.send-result {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 4px;
  margin-top: 12px;
}

.send-result.success {
  background: #d4edda;
  color: #155724;
  border: 1px solid #c3e6cb;
}

.send-result.error {
  background: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
}

.result-icon {
  font-size: 18px;
  font-weight: bold;
}

.result-details {
  flex: 1;
}

.result-message {
  font-weight: 500;
}

.result-error {
  font-size: 12px;
  opacity: 0.8;
}

.result-time {
  font-size: 12px;
  opacity: 0.8;
}

/* 消息显示区 */
.message-display {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

/* 实时统计面板 */
.stats-panel {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.stats-panel h3 {
  margin: 0 0 16px 0;
  color: #333;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 16px;
  border: 1px solid #dee2e6;
}

.stat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.stat-title {
  font-size: 12px;
  color: #666;
  text-transform: uppercase;
  font-weight: 500;
}

.stat-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #dc3545;
}

.stat-indicator.connected {
  background: #28a745;
}

.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #333;
  margin-bottom: 4px;
}

.stat-value.excellent {
  color: #28a745;
}

.stat-value.good {
  color: #17a2b8;
}

.stat-value.fair {
  color: #ffc107;
}

.stat-value.poor {
  color: #dc3545;
}

.stat-subtitle {
  font-size: 12px;
  color: #666;
}

/* 性能图表 */
.performance-chart {
  margin-top: 20px;
}

.performance-chart h4 {
  margin: 0 0 12px 0;
  color: #333;
}

.chart-container {
  height: 200px;
  background: #f8f9fa;
  border-radius: 4px;
  border: 1px solid #dee2e6;
}

/* 调试面板 */
.debug-panel {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.debug-panel h3 {
  margin: 0 0 16px 0;
  color: #333;
}

.debug-tabs {
  display: flex;
  gap: 4px;
  margin-bottom: 16px;
  border-bottom: 1px solid #dee2e6;
}

.debug-tab {
  padding: 8px 16px;
  border: none;
  background: none;
  cursor: pointer;
  font-size: 14px;
  color: #666;
  border-bottom: 2px solid transparent;
  transition: all 0.2s ease;
}

.debug-tab:hover {
  color: #007bff;
}

.debug-tab.active {
  color: #007bff;
  border-bottom-color: #007bff;
}

.debug-content {
  min-height: 200px;
}

.debug-section h4 {
  margin: 0 0 12px 0;
  color: #333;
}

.debug-section pre {
  background: #1e1e1e;
  color: #d4d4d4;
  padding: 12px;
  border-radius: 4px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 12px;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

.recent-messages {
  max-height: 300px;
  overflow-y: auto;
}

.debug-message {
  padding: 8px;
  border-bottom: 1px solid #f1f3f4;
}

.debug-message:last-child {
  border-bottom: none;
}

.message-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.message-type {
  font-size: 12px;
  font-weight: 500;
  color: #007bff;
  text-transform: uppercase;
}

.message-time {
  font-size: 12px;
  color: #666;
}

.message-content {
  font-size: 12px;
  color: #333;
  font-family: monospace;
}

/* 消息详情弹窗 */
.message-modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  width: 90%;
  max-width: 600px;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #dee2e6;
}

.modal-header h3 {
  margin: 0;
  color: #333;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
  color: #666;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
}

.close-btn:hover {
  background: #f1f3f4;
}

.modal-body {
  padding: 16px;
}

.detail-item {
  margin-bottom: 12px;
}

.detail-label {
  font-weight: 500;
  color: #333;
  margin-bottom: 4px;
  display: block;
}

.detail-value {
  color: #666;
  font-family: monospace;
  font-size: 14px;
}

.detail-value.error {
  color: #dc3545;
}

.message-data-content {
  background: #f5f5f5;
  padding: 12px;
  border-radius: 4px;
  margin-top: 4px;
}

.message-data-content pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
  font-size: 12px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .websocket-client {
    padding: 10px;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .sender-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .btn {
    width: 100%;
  }
}
</style>