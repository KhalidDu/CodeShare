<template>
  <div class="websocket-message-handler">
    <!-- 消息过滤器 -->
    <div v-if="showFilter" class="message-filter">
      <div class="filter-controls">
        <select 
          v-model="filterType" 
          class="filter-select"
          @change="applyFilters"
        >
          <option value="">所有类型</option>
          <option 
            v-for="type in messageTypes" 
            :key="type" 
            :value="type"
          >
            {{ getMessageTypeLabel(type) }}
          </option>
        </select>

        <select 
          v-model="filterPriority" 
          class="filter-select"
          @change="applyFilters"
        >
          <option value="">所有优先级</option>
          <option 
            v-for="priority in messagePriorities" 
            :key="priority" 
            :value="priority"
          >
            {{ getMessagePriorityLabel(priority) }}
          </option>
        </select>

        <input 
          v-model="searchText" 
          type="text" 
          placeholder="搜索消息..."
          class="search-input"
          @input="applyFilters"
        />

        <button 
          @click="clearFilters"
          class="btn btn-secondary btn-sm"
        >
          清除筛选
        </button>
      </div>
    </div>

    <!-- 消息统计 -->
    <div v-if="showStats" class="message-stats">
      <div class="stat-item">
        <span class="stat-value">{{ filteredMessages.length }}</span>
        <span class="stat-label">显示消息</span>
      </div>
      <div class="stat-item">
        <span class="stat-value">{{ totalMessages }}</span>
        <span class="stat-label">总消息数</span>
      </div>
      <div class="stat-item">
        <span class="stat-value">{{ unreadCount }}</span>
        <span class="stat-label">未读消息</span>
      </div>
    </div>

    <!-- 消息列表 -->
    <div class="message-list" :class="{ 'with-scroll': enableScroll }">
      <div 
        v-for="message in filteredMessages" 
        :key="message.messageId"
        class="message-item"
        :class="getMessageClass(message)"
        @click="handleMessageClick(message)"
      >
        <!-- 消息头部 -->
        <div class="message-header">
          <div class="message-type" :class="message.type.toLowerCase()">
            {{ getMessageTypeLabel(message.type) }}
          </div>
          <div class="message-priority" :class="message.priority.toLowerCase()">
            {{ getMessagePriorityLabel(message.priority) }}
          </div>
          <div class="message-time">
            {{ formatTime(message.sentAt) }}
          </div>
        </div>

        <!-- 消息内容 -->
        <div class="message-content">
          <div class="message-data">
            <pre v-if="isJsonObject(message.data)">{{ formatJson(message.data) }}</pre>
            <span v-else>{{ message.data }}</span>
          </div>
        </div>

        <!-- 消息元信息 -->
        <div class="message-meta">
          <div class="message-sender" v-if="message.senderId">
            发送者: {{ message.senderUsername || message.senderId }}
          </div>
          <div class="message-id">
            ID: {{ message.messageId.substring(0, 8) }}...
          </div>
          <div class="message-status" :class="message.status.toLowerCase()">
            {{ getMessageStatusLabel(message.status) }}
          </div>
        </div>

        <!-- 消息操作 -->
        <div class="message-actions">
          <button 
            v-if="message.status === 'unread'"
            @click.stop="markAsRead(message)"
            class="btn btn-xs btn-primary"
          >
            标记已读
          </button>
          <button 
            @click.stop="copyMessage(message)"
            class="btn btn-xs btn-secondary"
          >
            复制
          </button>
          <button 
            @click.stop="deleteMessage(message)"
            class="btn btn-xs btn-danger"
          >
            删除
          </button>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="filteredMessages.length === 0" class="empty-state">
        <div class="empty-icon">📨</div>
        <div class="empty-text">{{ emptyStateText }}</div>
      </div>
    </div>

    <!-- 分页控制 -->
    <div v-if="showPagination && totalPages > 1" class="pagination">
      <button 
        @click="previousPage"
        :disabled="currentPage === 1"
        class="btn btn-sm"
      >
        上一页
      </button>
      
      <span class="page-info">
        {{ currentPage }} / {{ totalPages }}
      </span>
      
      <button 
        @click="nextPage"
        :disabled="currentPage === totalPages"
        class="btn btn-sm"
      >
        下一页
      </button>
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
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useWebSocket, type WebSocketMessage } from '@/composables/useWebSocket'
import type { 
  WebSocketMessageType, 
  WebSocketMessagePriority, 
  WebSocketMessageStatus 
} from '@/types/websocket'

interface Props {
  // 是否显示过滤器
  showFilter?: boolean
  // 是否显示统计信息
  showStats?: boolean
  // 是否显示分页
  showPagination?: boolean
  // 是否启用滚动
  enableScroll?: boolean
  // 每页显示数量
  pageSize?: number
  // 最大历史记录数量
  maxHistorySize?: number
  // 自动标记已读
  autoMarkAsRead?: boolean
  // 是否启用声音提醒
  enableSound?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showFilter: true,
  showStats: true,
  showPagination: false,
  enableScroll: true,
  pageSize: 20,
  maxHistorySize: 1000,
  autoMarkAsRead: false,
  enableSound: false
})

const emit = defineEmits<{
  messageClick: [message: WebSocketMessage]
  messageRead: [message: WebSocketMessage]
  messageDelete: [message: WebSocketMessage]
  filterChange: [filters: MessageFilters]
}>()

// WebSocket状态管理
const { state, getters, actions } = useWebSocket()

// 响应式数据
const filterType = ref<WebSocketMessageType | ''>('')
const filterPriority = ref<WebSocketMessagePriority | ''>('')
const searchText = ref('')
const currentPage = ref(1)
const selectedMessage = ref<WebSocketMessage | null>(null)

// 消息类型和优先级选项
const messageTypes = Object.values(WebSocketMessageType)
const messagePriorities = Object.values(WebSocketMessagePriority)

// 过滤器接口
interface MessageFilters {
  type: WebSocketMessageType | ''
  priority: WebSocketMessagePriority | ''
  search: string
}

// 计算属性
const totalMessages = computed(() => state.messageHistory.length)

const unreadCount = computed(() => 
  state.messageHistory.filter(msg => msg.status === 'unread').length
)

const filteredMessages = computed(() => {
  let messages = [...state.messageHistory]

  // 应用类型筛选
  if (filterType.value) {
    messages = messages.filter(msg => msg.type === filterType.value)
  }

  // 应用优先级筛选
  if (filterPriority.value) {
    messages = messages.filter(msg => msg.priority === filterPriority.value)
  }

  // 应用搜索筛选
  if (searchText.value) {
    const searchLower = searchText.value.toLowerCase()
    messages = messages.filter(msg => {
      const dataStr = JSON.stringify(msg.data).toLowerCase()
      return dataStr.includes(searchLower) ||
        msg.messageId.toLowerCase().includes(searchLower) ||
        (msg.senderUsername?.toLowerCase().includes(searchLower) || false)
    })
  }

  // 按时间倒序排列
  messages.sort((a, b) => b.sentAt.getTime() - a.sentAt.getTime())

  return messages
})

const paginatedMessages = computed(() => {
  if (!props.showPagination) {
    return filteredMessages.value
  }

  const startIndex = (currentPage.value - 1) * props.pageSize
  const endIndex = startIndex + props.pageSize
  return filteredMessages.value.slice(startIndex, endIndex)
})

const totalPages = computed(() => {
  if (!props.showPagination) return 1
  return Math.ceil(filteredMessages.value.length / props.pageSize)
})

const emptyStateText = computed(() => {
  if (state.messageHistory.length === 0) {
    return '暂无消息'
  } else if (filterType.value || filterPriority.value || searchText.value) {
    return '没有符合筛选条件的消息'
  } else {
    return '暂无消息'
  }
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

const getMessageClass = (message: WebSocketMessage): string => {
  const classes = []
  
  if (message.status === 'unread') {
    classes.push('unread')
  }
  
  classes.push(`type-${message.type.toLowerCase()}`)
  classes.push(`priority-${message.priority.toLowerCase()}`)
  classes.push(`status-${message.status.toLowerCase()}`)
  
  return classes.join(' ')
}

const formatTime = (date: Date): string => {
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  if (diff < 60000) { // 1分钟内
    return '刚刚'
  } else if (diff < 3600000) { // 1小时内
    return `${Math.floor(diff / 60000)}分钟前`
  } else if (diff < 86400000) { // 1天内
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

const isJsonObject = (data: any): boolean => {
  return data && typeof data === 'object' && !Array.isArray(data)
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

const applyFilters = () => {
  currentPage.value = 1
  emit('filterChange', {
    type: filterType.value,
    priority: filterPriority.value,
    search: searchText.value
  })
}

const clearFilters = () => {
  filterType.value = ''
  filterPriority.value = ''
  searchText.value = ''
  currentPage.value = 1
  applyFilters()
}

const handleMessageClick = (message: WebSocketMessage) => {
  selectedMessage.value = message
  emit('messageClick', message)
}

const closeMessageDetail = () => {
  selectedMessage.value = null
}

const markAsRead = (message: WebSocketMessage) => {
  // 在实际应用中，这里会调用API标记消息为已读
  message.status = 'sent' // 假设已读状态为sent
  emit('messageRead', message)
}

const copyMessage = async (message: WebSocketMessage) => {
  try {
    const text = `${getMessageTypeLabel(message.type)}: ${formatJson(message.data)}`
    await navigator.clipboard.writeText(text)
    // 这里可以添加成功提示
  } catch (error) {
    console.error('复制消息失败:', error)
  }
}

const deleteMessage = (message: WebSocketMessage) => {
  const index = state.messageHistory.findIndex(msg => msg.messageId === message.messageId)
  if (index > -1) {
    state.messageHistory.splice(index, 1)
    emit('messageDelete', message)
  }
}

const previousPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

const playNotificationSound = () => {
  if (!props.enableSound) return
  
  // 创建音频上下文
  const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
  const oscillator = audioContext.createOscillator()
  const gainNode = audioContext.createGain()
  
  oscillator.connect(gainNode)
  gainNode.connect(audioContext.destination)
  
  oscillator.frequency.setValueAtTime(800, audioContext.currentTime)
  oscillator.frequency.setValueAtTime(600, audioContext.currentTime + 0.1)
  
  gainNode.gain.setValueAtTime(0.1, audioContext.currentTime)
  gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1)
  
  oscillator.start(audioContext.currentTime)
  oscillator.stop(audioContext.currentTime + 0.1)
}

// 监听新消息
watch(() => state.messageHistory, (newMessages, oldMessages) => {
  if (newMessages.length > (oldMessages?.length || 0)) {
    const newMessage = newMessages[newMessages.length - 1]
    
    // 限制历史记录大小
    if (state.messageHistory.length > props.maxHistorySize) {
      state.messageHistory = state.messageHistory.slice(-props.maxHistorySize)
    }
    
    // 播放提示音
    if (props.enableSound) {
      playNotificationSound()
    }
    
    // 自动标记已读
    if (props.autoMarkAsRead && newMessage.status === 'unread') {
      setTimeout(() => markAsRead(newMessage), 3000)
    }
  }
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
})

onUnmounted(() => {
  // 清理消息处理器
  actions.unregisterMessageHandler(WebSocketMessageType.NOTIFICATION)
  actions.unregisterMessageHandler(WebSocketMessageType.SYSTEM)
  actions.unregisterMessageHandler(WebSocketMessageType.ERROR)
})
</script>

<style scoped>
.websocket-message-handler {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

/* 消息过滤器 */
.message-filter {
  background: white;
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 16px;
  border: 1px solid #dee2e6;
}

.filter-controls {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.filter-select, .search-input {
  padding: 8px 12px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 14px;
}

.search-input {
  flex: 1;
  min-width: 200px;
}

/* 消息统计 */
.message-stats {
  display: flex;
  gap: 24px;
  padding: 12px 0;
  margin-bottom: 16px;
  border-bottom: 1px solid #dee2e6;
}

.stat-item {
  text-align: center;
}

.stat-value {
  display: block;
  font-size: 24px;
  font-weight: bold;
  color: #007bff;
}

.stat-label {
  font-size: 12px;
  color: #666;
  margin-top: 4px;
}

/* 消息列表 */
.message-list {
  background: white;
  border-radius: 8px;
  border: 1px solid #dee2e6;
  max-height: 600px;
  overflow-y: auto;
}

.message-list.with-scroll {
  overflow-y: auto;
}

.message-item {
  padding: 16px;
  border-bottom: 1px solid #f1f3f4;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.message-item:hover {
  background: #f8f9fa;
}

.message-item.unread {
  background: #e3f2fd;
  border-left: 4px solid #2196f3;
}

.message-item:last-child {
  border-bottom: none;
}

/* 消息头部 */
.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.message-type, .message-priority, .message-status {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  text-transform: uppercase;
}

.message-type {
  background: #e3f2fd;
  color: #1976d2;
}

.message-priority {
  background: #fff3e0;
  color: #f57c00;
}

.message-priority.high {
  background: #ffebee;
  color: #d32f2f;
}

.message-priority.urgent {
  background: #d32f2f;
  color: white;
}

.message-time {
  font-size: 12px;
  color: #666;
}

/* 消息内容 */
.message-content {
  margin-bottom: 8px;
}

.message-data {
  font-size: 14px;
  line-height: 1.5;
  color: #333;
}

.message-data pre {
  background: #f5f5f5;
  padding: 8px;
  border-radius: 4px;
  font-size: 12px;
  white-space: pre-wrap;
  word-break: break-all;
  margin: 0;
}

/* 消息元信息 */
.message-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: #666;
  margin-bottom: 8px;
}

.message-sender, .message-id, .message-status {
  flex: 1;
}

.message-status {
  text-align: right;
}

.message-status.sent {
  color: #28a745;
}

.message-status.failed {
  color: #dc3545;
}

/* 消息操作 */
.message-actions {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 4px 8px;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-xs {
  padding: 2px 6px;
  font-size: 11px;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-danger {
  background: #dc3545;
  color: white;
}

/* 空状态 */
.empty-state {
  text-align: center;
  padding: 40px;
  color: #666;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-text {
  font-size: 16px;
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  margin-top: 16px;
}

.page-info {
  font-size: 14px;
  color: #666;
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

/* 滚动条样式 */
.message-list::-webkit-scrollbar {
  width: 6px;
}

.message-list::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.message-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .filter-controls {
    flex-direction: column;
    align-items: stretch;
  }

  .search-input {
    min-width: auto;
  }

  .message-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }

  .message-meta {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }

  .message-actions {
    flex-wrap: wrap;
  }

  .modal-content {
    width: 95%;
    margin: 20px;
  }
}
</style>