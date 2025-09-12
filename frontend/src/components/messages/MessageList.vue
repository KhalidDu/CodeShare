<template>
  <div class="message-list-container">
    <!-- 消息列表头部 -->
    <div class="message-list-header">
      <div class="header-left">
        <h2 class="list-title">
          <span v-if="title">{{ title }}</span>
          <span v-else>消息列表</span>
        </h2>
        <div v-if="showStats" class="list-stats">
          <span class="stat-item">
            <i class="fas fa-envelope"></i>
            {{ totalMessages }} 条消息
          </span>
          <span v-if="unreadCount > 0" class="stat-item unread">
            <i class="fas fa-circle"></i>
            {{ unreadCount }} 条未读
          </span>
        </div>
      </div>
      <div class="header-right">
        <slot name="header-actions"></slot>
      </div>
    </div>

    <!-- 搜索和筛选区域 -->
    <div v-if="showSearch || showFilter" class="message-list-controls">
      <div v-if="showSearch" class="search-container">
        <div class="search-input-wrapper">
          <i class="fas fa-search search-icon"></i>
          <input
            v-model="searchQuery"
            type="text"
            :placeholder="searchPlaceholder"
            class="search-input"
            @keyup.enter="handleSearch"
            @input="debouncedSearch"
          />
          <button
            v-if="searchQuery"
            @click="clearSearch"
            class="clear-search"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
      </div>
      
      <div v-if="showFilter" class="filter-container">
        <button
          @click="toggleFilterPanel"
          class="filter-button"
          :class="{ active: showFilterPanel }"
        >
          <i class="fas fa-filter"></i>
          筛选
        </button>
      </div>
    </div>

    <!-- 筛选面板 -->
    <div v-if="showFilter && showFilterPanel" class="filter-panel">
      <div class="filter-grid">
        <div class="filter-group">
          <label class="filter-label">消息类型</label>
          <select v-model="filters.messageType" class="filter-select">
            <option value="">全部类型</option>
            <option v-for="type in messageTypes" :key="type" :value="type">
              {{ getMessageTypeLabel(type) }}
            </option>
          </select>
        </div>
        
        <div class="filter-group">
          <label class="filter-label">消息状态</label>
          <select v-model="filters.status" class="filter-select">
            <option value="">全部状态</option>
            <option v-for="status in messageStatuses" :key="status" :value="status">
              {{ getMessageStatusLabel(status) }}
            </option>
          </select>
        </div>
        
        <div class="filter-group">
          <label class="filter-label">优先级</label>
          <select v-model="filters.priority" class="filter-select">
            <option value="">全部优先级</option>
            <option v-for="priority in priorities" :key="priority" :value="priority">
              {{ getPriorityLabel(priority) }}
            </option>
          </select>
        </div>
        
        <div class="filter-group">
          <label class="filter-label">已读状态</label>
          <select v-model="filters.isRead" class="filter-select">
            <option value="">全部</option>
            <option :value="true">已读</option>
            <option :value="false">未读</option>
          </select>
        </div>
      </div>
      
      <div class="filter-actions">
        <button @click="applyFilters" class="apply-filter-btn">
          应用筛选
        </button>
        <button @click="resetFilters" class="reset-filter-btn">
          重置
        </button>
      </div>
    </div>

    <!-- 消息列表主体 -->
    <div class="message-list-body">
      <!-- 加载状态 -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner">
          <div class="spinner"></div>
        </div>
        <p class="loading-text">加载中...</p>
      </div>

      <!-- 空状态 -->
      <div v-else-if="messages.length === 0" class="empty-state">
        <div class="empty-icon">
          <i class="fas fa-inbox"></i>
        </div>
        <h3 class="empty-title">暂无消息</h3>
        <p class="empty-description">
          {{ getEmptyStateMessage() }}
        </p>
        <button v-if="showEmptyAction" @click="handleEmptyAction" class="empty-action-btn">
          {{ emptyActionText }}
        </button>
      </div>

      <!-- 消息列表 -->
      <div v-else class="messages-container">
        <div
          v-for="message in messages"
          :key="message.id"
          class="message-item-wrapper"
        >
          <MessageItem
            :message="message"
            :selected="selectedMessageIds?.includes(message.id)"
            :show-avatar="showAvatar"
            :show-actions="showActions"
            :compact-mode="compactMode"
            @click="handleMessageClick(message)"
            @select="handleMessageSelect(message)"
            @mark-read="handleMarkAsRead(message)"
            @mark-unread="handleMarkAsUnread(message)"
            @delete="handleMessageDelete(message)"
            @reply="handleMessageReply(message)"
            @forward="handleMessageForward(message)"
          />
        </div>
      </div>
    </div>

    <!-- 分页控件 -->
    <div v-if="showPagination && totalPages > 1" class="pagination-container">
      <div class="pagination-info">
        显示 {{ startItem }}-{{ endItem }} 条，共 {{ totalItems }} 条
      </div>
      <div class="pagination-controls">
        <button
          @click="goToPage(currentPage - 1)"
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          <i class="fas fa-chevron-left"></i>
        </button>
        
        <div class="pagination-pages">
          <button
            v-for="page in visiblePages"
            :key="page"
            @click="goToPage(page)"
            :class="{ active: page === currentPage }"
            class="pagination-btn page-btn"
          >
            {{ page }}
          </button>
        </div>
        
        <button
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          <i class="fas fa-chevron-right"></i>
        </button>
      </div>
    </div>

    <!-- 批量操作栏 -->
    <div v-if="showBatchActions && selectedMessageIds.length > 0" class="batch-actions">
      <div class="batch-info">
        已选择 {{ selectedMessageIds.length }} 条消息
      </div>
      <div class="batch-controls">
        <button @click="batchMarkAsRead" class="batch-btn">
          <i class="fas fa-check"></i>
          标记已读
        </button>
        <button @click="batchMarkAsUnread" class="batch-btn">
          <i class="fas fa-envelope"></i>
          标记未读
        </button>
        <button @click="batchDelete" class="batch-btn delete">
          <i class="fas fa-trash"></i>
          删除
        </button>
        <button @click="clearSelection" class="batch-btn cancel">
          取消选择
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useMessageStore } from '@/stores/message'
import { messageService } from '@/services/messageService'
import type { Message, MessageFilter, MessageType, MessageStatus, MessagePriority } from '@/types/message'
import MessageItem from './MessageItem.vue'

// 定义Props
interface Props {
  messages?: Message[]
  title?: string
  loading?: boolean
  showSearch?: boolean
  showFilter?: boolean
  showPagination?: boolean
  showStats?: boolean
  showAvatar?: boolean
  showActions?: boolean
  showEmptyAction?: boolean
  compactMode?: boolean
  enableBatchActions?: boolean
  enableVirtualScroll?: boolean
  searchPlaceholder?: string
  emptyActionText?: string
  initialFilters?: Partial<MessageFilter>
  itemsPerPage?: number
}

const props = withDefaults(defineProps<Props>(), {
  messages: () => [],
  loading: false,
  showSearch: true,
  showFilter: true,
  showPagination: true,
  showStats: true,
  showAvatar: true,
  showActions: true,
  showEmptyAction: true,
  compactMode: false,
  enableBatchActions: true,
  enableVirtualScroll: false,
  searchPlaceholder: '搜索消息...',
  emptyActionText: '发送新消息',
  initialFilters: () => ({}),
  itemsPerPage: 20
})

// 定义Emits
interface Emits {
  (e: 'message-click', message: Message): void
  (e: 'message-select', message: Message, selected: boolean): void
  (e: 'search', query: string): void
  (e: 'filter-change', filters: Partial<MessageFilter>): void
  (e: 'page-change', page: number): void
  (e: 'empty-action'): void
  (e: 'batch-operation', operation: string, messageIds: string[]): void
}

const emit = defineEmits<Emits>()

// Store
const messageStore = useMessageStore()

// 响应式状态
const searchQuery = ref('')
const showFilterPanel = ref(false)
const selectedMessageIds = ref<string[]>([])
const currentPage = ref(1)
const localMessages = ref<Message[]>(props.messages)

// 筛选器
const filters = ref<Partial<MessageFilter>>({
  messageType: undefined,
  status: undefined,
  priority: undefined,
  isRead: undefined,
  sortBy: 'CREATED_AT_DESC',
  page: 1,
  pageSize: props.itemsPerPage,
  ...props.initialFilters
})

// 消息类型选项
const messageTypes = Object.values(MessageType)

// 消息状态选项
const messageStatuses = Object.values(MessageStatus)

// 优先级选项
const priorities = Object.values(MessagePriority)

// 计算属性
const totalMessages = computed(() => localMessages.value.length)

const unreadMessages = computed(() => 
  localMessages.value.filter(msg => !msg.isRead)
)

const unreadCount = computed(() => unreadMessages.value.length)

const filteredMessages = computed(() => {
  let filtered = [...localMessages.value]
  
  // 应用搜索
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(msg => 
      msg.subject?.toLowerCase().includes(query) ||
      msg.content.toLowerCase().includes(query) ||
      msg.senderName.toLowerCase().includes(query)
    )
  }
  
  // 应用筛选
  if (filters.value.messageType) {
    filtered = filtered.filter(msg => msg.messageType === filters.value.messageType)
  }
  
  if (filters.value.status) {
    filtered = filtered.filter(msg => msg.status === filters.value.status)
  }
  
  if (filters.value.priority) {
    filtered = filtered.filter(msg => msg.priority === filters.value.priority)
  }
  
  if (filters.value.isRead !== undefined) {
    filtered = filtered.filter(msg => msg.isRead === filters.value.isRead)
  }
  
  return filtered
})

const totalPages = computed(() => 
  Math.ceil(filteredMessages.value.length / props.itemsPerPage)
)

const totalItems = computed(() => filteredMessages.value.length)

const startItem = computed(() => 
  (currentPage.value - 1) * props.itemsPerPage + 1
)

const endItem = computed(() => 
  Math.min(currentPage.value * props.itemsPerPage, totalItems.value)
)

const paginatedMessages = computed(() => {
  const start = (currentPage.value - 1) * props.itemsPerPage
  const end = start + props.itemsPerPage
  return filteredMessages.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages: number[] = []
  const maxVisible = 5
  let start = Math.max(1, currentPage.value - Math.floor(maxVisible / 2))
  let end = Math.min(totalPages.value, start + maxVisible - 1)
  
  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1)
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

const showBatchActions = computed(() => 
  props.enableBatchActions && selectedMessageIds.value.length > 0
)

// 防抖搜索
let searchTimeout: NodeJS.Timeout
const debouncedSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchQuery.value) {
      handleSearch()
    }
  }, 300)
}

// 方法定义
const handleSearch = () => {
  emit('search', searchQuery.value)
  currentPage.value = 1
}

const clearSearch = () => {
  searchQuery.value = ''
  emit('search', '')
  currentPage.value = 1
}

const toggleFilterPanel = () => {
  showFilterPanel.value = !showFilterPanel.value
}

const applyFilters = () => {
  emit('filter-change', { ...filters.value })
  currentPage.value = 1
}

const resetFilters = () => {
  filters.value = {
    messageType: undefined,
    status: undefined,
    priority: undefined,
    isRead: undefined,
    sortBy: 'CREATED_AT_DESC',
    page: 1,
    pageSize: props.itemsPerPage
  }
  emit('filter-change', { ...filters.value })
  currentPage.value = 1
}

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    emit('page-change', page)
  }
}

const handleMessageClick = (message: Message) => {
  emit('message-click', message)
}

const handleMessageSelect = (message: Message, selected: boolean) => {
  if (selected) {
    selectedMessageIds.value.push(message.id)
  } else {
    selectedMessageIds.value = selectedMessageIds.value.filter(id => id !== message.id)
  }
  emit('message-select', message, selected)
}

const handleMarkAsRead = async (message: Message) => {
  try {
    await messageService.markAsRead(message.id)
    message.isRead = true
    message.readAt = new Date().toISOString()
  } catch (error) {
    console.error('标记已读失败:', error)
  }
}

const handleMarkAsUnread = async (message: Message) => {
  try {
    await messageService.markAsUnread(message.id)
    message.isRead = false
    message.readAt = undefined
  } catch (error) {
    console.error('标记未读失败:', error)
  }
}

const handleMessageDelete = async (message: Message) => {
  try {
    await messageService.deleteMessage(message.id)
    localMessages.value = localMessages.value.filter(msg => msg.id !== message.id)
  } catch (error) {
    console.error('删除消息失败:', error)
  }
}

const handleMessageReply = (message: Message) => {
  // 处理回复逻辑
  console.log('回复消息:', message)
}

const handleMessageForward = (message: Message) => {
  // 处理转发逻辑
  console.log('转发消息:', message)
}

const handleEmptyAction = () => {
  emit('empty-action')
}

const batchMarkAsRead = async () => {
  try {
    await messageStore.batchOperation({
      messageIds: selectedMessageIds.value,
      operation: 'MARK_AS_READ'
    })
    clearSelection()
  } catch (error) {
    console.error('批量标记已读失败:', error)
  }
}

const batchMarkAsUnread = async () => {
  try {
    await messageStore.batchOperation({
      messageIds: selectedMessageIds.value,
      operation: 'MARK_AS_UNREAD'
    })
    clearSelection()
  } catch (error) {
    console.error('批量标记未读失败:', error)
  }
}

const batchDelete = async () => {
  try {
    await messageStore.batchOperation({
      messageIds: selectedMessageIds.value,
      operation: 'DELETE',
      reason: '批量删除'
    })
    clearSelection()
  } catch (error) {
    console.error('批量删除失败:', error)
  }
}

const clearSelection = () => {
  selectedMessageIds.value = []
}

const getEmptyStateMessage = () => {
  if (searchQuery.value) {
    return '没有找到匹配的消息'
  }
  if (filters.value.messageType || filters.value.status || filters.value.priority) {
    return '没有符合筛选条件的消息'
  }
  return '还没有收到任何消息'
}

const getMessageTypeLabel = (type: MessageType) => {
  const labels = {
    [MessageType.USER]: '用户消息',
    [MessageType.SYSTEM]: '系统消息',
    [MessageType.NOTIFICATION]: '通知消息',
    [MessageType.BROADCAST]: '广播消息',
    [MessageType.AUTO_REPLY]: '自动回复'
  }
  return labels[type] || type
}

const getMessageStatusLabel = (status: MessageStatus) => {
  const labels = {
    [MessageStatus.DRAFT]: '草稿',
    [MessageStatus.SENT]: '已发送',
    [MessageStatus.DELIVERED]: '已送达',
    [MessageStatus.READ]: '已读',
    [MessageStatus.REPLIED]: '已回复',
    [MessageStatus.FORWARDED]: '已转发',
    [MessageStatus.DELETED]: '已删除',
    [MessageStatus.FAILED]: '发送失败',
    [MessageStatus.EXPIRED]: '已过期'
  }
  return labels[status] || status
}

const getPriorityLabel = (priority: MessagePriority) => {
  const labels = {
    [MessagePriority.LOW]: '低',
    [MessagePriority.NORMAL]: '普通',
    [MessagePriority.HIGH]: '高',
    [MessagePriority.URGENT]: '紧急'
  }
  return labels[priority] || priority
}

// 监听器
watch(() => props.messages, (newMessages) => {
  localMessages.value = newMessages
}, { deep: true })

// 生命周期
onMounted(() => {
  // 可以在这里添加一些初始化逻辑
})

onUnmounted(() => {
  clearTimeout(searchTimeout)
})

// 暴露方法
defineExpose({
  searchQuery,
  filters,
  selectedMessageIds,
  currentPage,
  clearSearch,
  resetFilters,
  clearSelection,
  handleSearch,
  applyFilters,
  goToPage
})
</script>

<style scoped>
.message-list-container {
  @apply bg-white dark:bg-gray-800 rounded-lg shadow-md;
}

.message-list-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.header-left {
  @apply flex items-center space-x-4;
}

.list-title {
  @apply text-xl font-semibold text-gray-900 dark:text-white;
}

.list-stats {
  @apply flex items-center space-x-3 text-sm text-gray-600 dark:text-gray-400;
}

.stat-item {
  @apply flex items-center space-x-1;
}

.stat-item.unread {
  @apply text-blue-600 dark:text-blue-400;
}

.header-right {
  @apply flex items-center space-x-2;
}

.message-list-controls {
  @apply flex items-center space-x-4 p-4 border-b border-gray-200 dark:border-gray-700;
}

.search-container {
  @apply flex-1 max-w-md;
}

.search-input-wrapper {
  @apply relative;
}

.search-icon {
  @apply absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400;
}

.search-input {
  @apply w-full pl-10 pr-10 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.clear-search {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 
         hover:text-gray-600 dark:hover:text-gray-300;
}

.filter-container {
  @apply flex-shrink-0;
}

.filter-button {
  @apply px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.filter-button.active {
  @apply bg-blue-50 dark:bg-blue-900 border-blue-500 text-blue-700 dark:text-blue-300;
}

.filter-panel {
  @apply p-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700;
}

.filter-grid {
  @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4;
}

.filter-group {
  @apply flex flex-col;
}

.filter-label {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300 mb-2;
}

.filter-select {
  @apply px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-800 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.filter-actions {
  @apply flex justify-end space-x-3 mt-4;
}

.apply-filter-btn {
  @apply px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.reset-filter-btn {
  @apply px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700;
}

.message-list-body {
  @apply min-h-[400px];
}

.loading-container {
  @apply flex flex-col items-center justify-center h-96 space-y-4;
}

.loading-spinner {
  @apply w-8 h-8 border-2 border-gray-200 border-t-blue-600 rounded-full animate-spin;
}

.loading-text {
  @apply text-gray-600 dark:text-gray-400;
}

.empty-state {
  @apply flex flex-col items-center justify-center h-96 space-y-4 text-center;
}

.empty-icon {
  @apply w-16 h-16 text-gray-400 dark:text-gray-600;
}

.empty-title {
  @apply text-xl font-semibold text-gray-900 dark:text-white;
}

.empty-description {
  @apply text-gray-600 dark:text-gray-400 max-w-md;
}

.empty-action-btn {
  @apply px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.messages-container {
  @apply divide-y divide-gray-200 dark:divide-gray-700;
}

.pagination-container {
  @apply flex items-center justify-between p-4 border-t border-gray-200 dark:border-gray-700;
}

.pagination-info {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.pagination-controls {
  @apply flex items-center space-x-2;
}

.pagination-btn {
  @apply p-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.pagination-btn:disabled {
  @apply opacity-50 cursor-not-allowed hover:bg-transparent;
}

.pagination-btn.page-btn {
  @apply px-3 py-2;
}

.pagination-btn.page-btn.active {
  @apply bg-blue-600 text-white border-blue-600;
}

.batch-actions {
  @apply fixed bottom-0 left-0 right-0 bg-white dark:bg-gray-800 
         border-t border-gray-200 dark:border-gray-700 shadow-lg p-4;
}

.batch-info {
  @apply text-sm font-medium text-gray-900 dark:text-white;
}

.batch-controls {
  @apply flex items-center space-x-2 mt-2;
}

.batch-btn {
  @apply px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.batch-btn.delete {
  @apply border-red-300 dark:border-red-600 text-red-700 dark:text-red-300 
         hover:bg-red-50 dark:hover:bg-red-900;
}

.batch-btn.cancel {
  @apply text-gray-500 dark:text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .message-list-header {
    @apply flex-col items-start space-y-2;
  }
  
  .message-list-controls {
    @apply flex-col space-y-3;
  }
  
  .search-container {
    @apply w-full;
  }
  
  .filter-grid {
    @apply grid-cols-1;
  }
  
  .filter-actions {
    @apply flex-col space-y-2;
  }
  
  .pagination-container {
    @apply flex-col space-y-3;
  }
  
  .batch-controls {
    @apply flex-wrap;
  }
}

/* 深色模式优化 */
.dark .message-list-container {
  @apply bg-gray-800;
}

.dark .search-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .filter-select {
  @apply bg-gray-800 text-white border-gray-600;
}

.dark .filter-panel {
  @apply bg-gray-700;
}

.dark .pagination-btn {
  @apply border-gray-600 text-gray-300 hover:bg-gray-700;
}

.dark .batch-actions {
  @apply bg-gray-800 border-gray-700;
}

/* 动画效果 */
.message-item-wrapper {
  @apply transition-all duration-200 ease-in-out;
}

.message-item-wrapper:hover {
  @apply bg-gray-50 dark:bg-gray-700;
}

/* 滚动条样式 */
.messages-container {
  @apply max-h-[600px] overflow-y-auto;
}

.messages-container::-webkit-scrollbar {
  @apply w-2;
}

.messages-container::-webkit-scrollbar-track {
  @apply bg-gray-100 dark:bg-gray-800;
}

.messages-container::-webkit-scrollbar-thumb {
  @apply bg-gray-300 dark:bg-gray-600 rounded-full;
}

.messages-container::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400 dark:bg-gray-500;
}
</style>