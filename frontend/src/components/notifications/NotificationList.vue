<template>
  <div class="notification-list" :class="containerClasses">
    <!-- 列表头部 -->
    <div class="notification-list-header" v-if="showHeader">
      <div class="notification-list-title-section">
        <h2 class="notification-list-title">{{ title }}</h2>
        <p class="notification-list-description" v-if="description">{{ description }}</p>
      </div>
      
      <!-- 操作按钮 -->
      <div class="notification-list-actions">
        <button
          v-if="showRefresh && !autoRefresh"
          @click="handleRefresh"
          class="notification-list-btn notification-list-btn-refresh"
          :disabled="loading"
          title="刷新"
        >
          <svg class="notification-list-btn-icon" :class="{ 'rotating': loading }" viewBox="0 0 24 24" fill="currentColor">
            <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z"/>
          </svg>
        </button>
        
        <button
          v-if="showMarkAllRead && notificationsStore.hasUnread"
          @click="handleMarkAllRead"
          class="notification-list-btn notification-list-btn-mark-read"
          :disabled="loading"
          title="全部标记为已读"
        >
          <svg class="notification-list-btn-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
          </svg>
        </button>
        
        <button
          v-if="showSettings"
          @click="handleSettingsClick"
          class="notification-list-btn notification-list-btn-settings"
          title="通知设置"
        >
          <svg class="notification-list-btn-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- 统计信息 -->
    <div class="notification-list-stats" v-if="showStats">
      <div class="notification-list-stat">
        <span class="notification-list-stat-label">总数</span>
        <span class="notification-list-stat-value">{{ notificationsStore.totalCount }}</span>
      </div>
      <div class="notification-list-stat">
        <span class="notification-list-stat-label">未读</span>
        <span class="notification-list-stat-value notification-list-stat-value-unread">
          {{ notificationsStore.unreadCount }}
        </span>
      </div>
      <div class="notification-list-stat">
        <span class="notification-list-stat-label">已读</span>
        <span class="notification-list-stat-value">{{ notificationsStore.readCount }}</span>
      </div>
    </div>

    <!-- 搜索和筛选 -->
    <div class="notification-list-filters" v-if="showFilters">
      <!-- 搜索框 -->
      <div class="notification-list-search" v-if="showSearch">
        <input
          v-model="searchQuery"
          @input="handleSearch"
          type="text"
          :placeholder="searchPlaceholder"
          class="notification-list-search-input"
        />
        <svg class="notification-list-search-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M15.5 14h-.79l-.28-.27A6.471 6.471 0 0 0 16 9.5 6.5 6.5 0 1 0 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z"/>
        </svg>
      </div>

      <!-- 筛选器 -->
      <div class="notification-list-filter-controls">
        <!-- 类型筛选 -->
        <select
          v-model="filters.type"
          @change="handleFilterChange"
          class="notification-list-filter-select"
        >
          <option value="">所有类型</option>
          <option 
            v-for="type in notificationTypeOptions" 
            :key="type.value"
            :value="type.value"
          >
            {{ type.label }}
          </option>
        </select>

        <!-- 状态筛选 -->
        <select
          v-model="filters.status"
          @change="handleFilterChange"
          class="notification-list-filter-select"
        >
          <option value="">所有状态</option>
          <option 
            v-for="status in notificationStatusOptions" 
            :key="status.value"
            :value="status.value"
          >
            {{ status.label }}
          </option>
        </select>

        <!-- 优先级筛选 -->
        <select
          v-model="filters.priority"
          @change="handleFilterChange"
          class="notification-list-filter-select"
        >
          <option value="">所有优先级</option>
          <option 
            v-for="priority in notificationPriorityOptions" 
            :key="priority.value"
            :value="priority.value"
          >
            {{ priority.label }}
          </option>
        </select>

        <!-- 排序方式 -->
        <select
          v-model="filters.sortBy"
          @change="handleFilterChange"
          class="notification-list-filter-select"
        >
          <option 
            v-for="sort in notificationSortOptions" 
            :key="sort.value"
            :value="sort.value"
          >
            {{ sort.label }}
          </option>
        </select>

        <!-- 每页数量 -->
        <select
          v-model="filters.pageSize"
          @change="handlePageSizeChange"
          class="notification-list-filter-select"
        >
          <option 
            v-for="size in notificationPageSizeOptions" 
            :key="size.value"
            :value="size.value"
          >
            {{ size.label }}
          </option>
        </select>
      </div>
    </div>

    <!-- 通知列表内容 -->
    <div class="notification-list-content">
      <!-- 加载状态 -->
      <div v-if="loading && notifications.length === 0" class="notification-list-loading">
        <div class="notification-list-spinner"></div>
        <p class="notification-list-loading-text">加载中...</p>
      </div>

      <!-- 空状态 -->
      <div v-else-if="notifications.length === 0" class="notification-list-empty">
        <div class="notification-list-empty-icon">📭</div>
        <h3 class="notification-list-empty-title">{{ emptyTitle }}</h3>
        <p class="notification-list-empty-description">{{ emptyDescription }}</p>
        <button
          v-if="showRefresh && !autoRefresh"
          @click="handleRefresh"
          class="notification-list-btn notification-list-btn-primary"
        >
          刷新
        </button>
      </div>

      <!-- 通知列表 -->
      <div v-else class="notification-list-items">
        <NotificationItem
          v-for="notification in displayedNotifications"
          :key="notification.id"
          :notification="notification"
          :compact="compact"
          :show-icon="showItemIcon"
          :show-title="showItemTitle"
          :show-message="showItemMessage"
          :show-description="showItemDescription"
          :show-image="showItemImage"
          :show-timestamp="showItemTimestamp"
          :show-priority="showItemPriority"
          :show-actions="showItemActions"
          :show-tags="showItemTags"
          :show-channel="showItemChannel"
          :show-type="showItemType"
          :show-related="showItemRelated"
          :show-footer="showItemFooter"
          :show-unread-indicator="showItemUnreadIndicator"
          :hoverable="itemHoverable"
          :clickable="itemClickable"
          @click="handleNotificationClick"
          @read="handleNotificationRead"
          @archive="handleNotificationArchive"
          @delete="handleNotificationDelete"
          @action="handleNotificationAction"
          @image-click="handleNotificationImageClick"
        />
      </div>

      <!-- 加载更多 -->
      <div v-if="showLoadMore && notificationsStore.hasMore && !loading" class="notification-list-load-more">
        <button
          @click="handleLoadMore"
          class="notification-list-btn notification-list-btn-load-more"
        >
          加载更多
        </button>
      </div>

      <!-- 加载更多中的状态 -->
      <div v-if="loading && notifications.length > 0" class="notification-list-loading-more">
        <div class="notification-list-spinner-small"></div>
        <span>加载更多...</span>
      </div>
    </div>

    <!-- 分页 -->
    <div v-if="showPagination && !showLoadMore" class="notification-list-pagination">
      <div class="notification-list-pagination-info">
        显示 {{ startIndex }}-{{ endIndex }} 条，共 {{ notificationsStore.totalCount }} 条
      </div>
      
      <div class="notification-list-pagination-controls">
        <button
          @click="handlePageChange(currentPage - 1)"
          :disabled="currentPage <= 1"
          class="notification-list-btn notification-list-btn-page"
        >
          上一页
        </button>
        
        <div class="notification-list-pagination-pages">
          <button
            v-for="page in displayedPages"
            :key="page"
            @click="handlePageChange(page)"
            :class="['notification-list-btn notification-list-btn-page', { active: currentPage === page }]"
          >
            {{ page }}
          </button>
        </div>
        
        <button
          @click="handlePageChange(currentPage + 1)"
          :disabled="currentPage >= totalPages"
          class="notification-list-btn notification-list-btn-page"
        >
          下一页
        </button>
      </div>
    </div>

    <!-- WebSocket状态 -->
    <div v-if="showWebSocketStatus" class="notification-list-websocket-status">
      <div 
        class="notification-list-websocket-indicator"
        :class="{ connected: notificationsStore.isWebSocketConnected }"
      />
      <span class="notification-list-websocket-text">
        {{ notificationsStore.isWebSocketConnected ? '实时连接已建立' : '实时连接已断开' }}
      </span>
      <button
        v-if="!notificationsStore.isWebSocketConnected"
        @click="handleReconnectWebSocket"
        class="notification-list-btn notification-list-btn-text"
      >
        重新连接
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useNotificationsStore } from '@/stores/notifications'
import { useAuthStore } from '@/stores/auth'
import NotificationItem from './NotificationItem.vue'
import type { 
  Notification, 
  NotificationAction,
  NotificationRequest,
  NotificationType,
  NotificationStatus,
  NotificationPriority
} from '@/types/notifications'
import { 
  NOTIFICATION_TYPE_OPTIONS,
  NOTIFICATION_STATUS_OPTIONS,
  NOTIFICATION_PRIORITY_OPTIONS,
  NOTIFICATION_SORT_OPTIONS,
  NOTIFICATION_PAGE_SIZE_OPTIONS
} from '@/types/notifications'

// Props 接口
interface Props {
  maxItems?: number
  showHeader?: boolean
  showStats?: boolean
  showFilters?: boolean
  showSearch?: boolean
  showPagination?: boolean
  showLoadMore?: boolean
  showRefresh?: boolean
  showMarkAllRead?: boolean
  showSettings?: boolean
  showWebSocketStatus?: boolean
  autoRefresh?: boolean
  refreshInterval?: number
  title?: string
  description?: string
  emptyTitle?: string
  emptyDescription?: string
  searchPlaceholder?: string
  compact?: boolean
  showItemIcon?: boolean
  showItemTitle?: boolean
  showItemMessage?: boolean
  showItemDescription?: boolean
  showItemImage?: boolean
  showItemTimestamp?: boolean
  showItemPriority?: boolean
  showItemActions?: boolean
  showItemTags?: boolean
  showItemChannel?: boolean
  showItemType?: boolean
  showItemRelated?: boolean
  showItemFooter?: boolean
  showItemUnreadIndicator?: boolean
  itemHoverable?: boolean
  itemClickable?: boolean
  defaultFilters?: Partial<NotificationRequest>
}

// Emits 接口
interface Emits {
  (e: 'notification-click', notification: Notification): void
  (e: 'notification-read', notification: Notification): void
  (e: 'notification-archive', notification: Notification): void
  (e: 'notification-delete', notification: Notification): void
  (e: 'notification-action', action: NotificationAction, notification: Notification): void
  (e: 'image-click', notification: Notification): void
  (e: 'refresh'): void
  (e: 'load-more'): void
  (e: 'filters-change', filters: NotificationRequest): void
  (e: 'settings-click'): void
}

const props = withDefaults(defineProps<Props>(), {
  maxItems: undefined,
  showHeader: true,
  showStats: true,
  showFilters: true,
  showSearch: true,
  showPagination: true,
  showLoadMore: false,
  showRefresh: true,
  showMarkAllRead: true,
  showSettings: true,
  showWebSocketStatus: true,
  autoRefresh: false,
  refreshInterval: 30000,
  title: '通知中心',
  description: '管理您的通知和消息',
  emptyTitle: '暂无通知',
  emptyDescription: '您还没有收到任何通知',
  searchPlaceholder: '搜索通知...',
  compact: false,
  showItemIcon: true,
  showItemTitle: true,
  showItemMessage: true,
  showItemDescription: false,
  showItemImage: true,
  showItemTimestamp: true,
  showItemPriority: false,
  showItemActions: true,
  showItemTags: false,
  showItemChannel: false,
  showItemType: false,
  showItemRelated: false,
  showItemFooter: false,
  showItemUnreadIndicator: true,
  itemHoverable: true,
  itemClickable: true,
  defaultFilters: () => ({})
})

const emit = defineEmits<Emits>()
const notificationsStore = useNotificationsStore()
const authStore = useAuthStore()

// 状态
const loading = ref(false)
const searchQuery = ref('')
const filters = ref<NotificationRequest>({
  pageNumber: 1,
  pageSize: 20,
  sortBy: 'createdAt',
  sortOrder: 'desc',
  ...props.defaultFilters
})
const refreshTimer = ref<NodeJS.Timeout | null>(null)

// 计算属性
const notifications = computed(() => notificationsStore.notifications)

const displayedNotifications = computed(() => {
  if (props.maxItems) {
    return notifications.value.slice(0, props.maxItems)
  }
  return notifications.value
})

const currentPage = computed(() => notificationsStore.currentPage)
const totalPages = computed(() => Math.ceil(notificationsStore.totalCount / notificationsStore.pageSize))

const startIndex = computed(() => {
  if (notificationsStore.totalCount === 0) return 0
  return (currentPage.value - 1) * notificationsStore.pageSize + 1
})

const endIndex = computed(() => {
  return Math.min(currentPage.value * notificationsStore.pageSize, notificationsStore.totalCount)
})

const displayedPages = computed(() => {
  const pages: number[] = []
  const current = currentPage.value
  const total = totalPages.value
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 4) {
      for (let i = 1; i <= 5; i++) {
        pages.push(i)
      }
      pages.push(total)
    } else if (current >= total - 3) {
      pages.push(1)
      for (let i = total - 4; i <= total; i++) {
        pages.push(i)
      }
    } else {
      pages.push(1)
      for (let i = current - 1; i <= current + 1; i++) {
        pages.push(i)
      }
      pages.push(total)
    }
  }
  
  return pages
})

// 选项
const notificationTypeOptions = NOTIFICATION_TYPE_OPTIONS
const notificationStatusOptions = NOTIFICATION_STATUS_OPTIONS
const notificationPriorityOptions = NOTIFICATION_PRIORITY_OPTIONS
const notificationSortOptions = NOTIFICATION_SORT_OPTIONS
const notificationPageSizeOptions = NOTIFICATION_PAGE_SIZE_OPTIONS

// 容器样式类
const containerClasses = computed(() => [
  `size-${props.compact ? 'compact' : 'normal'}`,
  { 'auto-refresh': props.autoRefresh }
])

// 方法
async function loadNotifications(reset = false): Promise<void> {
  try {
    loading.value = true
    
    const request: NotificationRequest = {
      ...filters.value,
      pageNumber: reset ? 1 : filters.value.pageNumber || 1
    }
    
    await notificationsStore.fetchNotifications(request)
  } catch (error) {
    console.error('加载通知失败:', error)
  } finally {
    loading.value = false
  }
}

async function handleRefresh(): Promise<void> {
  await loadNotifications(true)
  emit('refresh')
}

async function handleLoadMore(): Promise<void> {
  await loadNotifications()
  emit('load-more')
}

async function handleMarkAllRead(): Promise<void> {
  try {
    await notificationsStore.markAllAsRead()
  } catch (error) {
    console.error('标记全部为已读失败:', error)
  }
}

function handleSearch(): void {
  notificationsStore.searchNotifications(searchQuery.value)
}

function handleFilterChange(): void {
  loadNotifications(true)
  emit('filters-change', filters.value)
}

function handlePageSizeChange(): void {
  loadNotifications(true)
}

function handlePageChange(page: number): void {
  filters.value.pageNumber = page
  loadNotifications()
}

function handleNotificationClick(notification: Notification): void {
  emit('notification-click', notification)
}

function handleNotificationRead(notification: Notification): void {
  emit('notification-read', notification)
}

function handleNotificationArchive(notification: Notification): void {
  emit('notification-archive', notification)
}

function handleNotificationDelete(notification: Notification): void {
  emit('notification-delete', notification)
}

function handleNotificationAction(action: NotificationAction, notification: Notification): void {
  emit('notification-action', action, notification)
}

function handleNotificationImageClick(notification: Notification): void {
  emit('image-click', notification)
}

function handleSettingsClick(): void {
  emit('settings-click')
}

async function handleReconnectWebSocket(): Promise<void> {
  if (authStore.user) {
    try {
      await notificationsStore.connectWebSocket(authStore.user.id)
    } catch (error) {
      console.error('重连WebSocket失败:', error)
    }
  }
}

// 生命周期
onMounted(async () => {
  // 加载通知设置
  await notificationsStore.fetchNotificationSettings()
  
  // 加载通知列表
  await loadNotifications(true)
  
  // 连接WebSocket
  if (authStore.user) {
    try {
      await notificationsStore.connectWebSocket(authStore.user.id)
    } catch (error) {
      console.error('连接WebSocket失败:', error)
    }
  }
  
  // 设置自动刷新
  if (props.autoRefresh && props.refreshInterval > 0) {
    refreshTimer.value = setInterval(() => {
      handleRefresh()
    }, props.refreshInterval)
  }
})

onUnmounted(() => {
  // 清理定时器
  if (refreshTimer.value) {
    clearInterval(refreshTimer.value)
  }
  
  // 断开WebSocket连接
  notificationsStore.disconnectWebSocket()
})

// 监听筛选器变化
watch(filters, (newFilters) => {
  notificationsStore.updateFilters(newFilters)
}, { deep: true })

// 监听默认筛选器变化
watch(() => props.defaultFilters, (newFilters) => {
  filters.value = {
    ...filters.value,
    ...newFilters
  }
  loadNotifications(true)
}, { deep: true })
</script>

<style scoped>
/* 基础样式 */
.notification-list {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.size-compact {
  max-width: 400px;
}

.size-normal {
  max-width: 800px;
}

.auto-refresh {
  position: relative;
}

.auto-refresh::after {
  content: '';
  position: absolute;
  top: 0;
  right: 0;
  width: 4px;
  height: 4px;
  background: #28a745;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

/* 头部样式 */
.notification-list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.notification-list-title-section {
  flex: 1;
}

.notification-list-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 0.25rem 0;
}

.notification-list-description {
  font-size: 0.875rem;
  color: #6c757d;
  margin: 0;
}

.notification-list-actions {
  display: flex;
  gap: 0.5rem;
}

/* 按钮样式 */
.notification-list-btn {
  border: none;
  background: rgba(0, 0, 0, 0.05);
  border-radius: 0.5rem;
  color: #6c757d;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  padding: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.notification-list-btn:hover {
  background: rgba(0, 0, 0, 0.1);
  color: #212529;
  transform: translateY(-1px);
}

.notification-list-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.notification-list-btn-icon {
  width: 1.25rem;
  height: 1.25rem;
}

.notification-list-btn-icon.rotating {
  animation: rotate 1s linear infinite;
}

.notification-list-btn-refresh:hover {
  background: rgba(0, 123, 255, 0.1);
  color: #007bff;
}

.notification-list-btn-mark-read:hover {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
}

.notification-list-btn-settings:hover {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
}

.notification-list-btn-primary {
  background: #007bff;
  color: white;
  padding: 0.75rem 1.5rem;
}

.notification-list-btn-primary:hover {
  background: #0056b3;
}

.notification-list-btn-load-more {
  background: rgba(0, 123, 255, 0.1);
  color: #007bff;
  padding: 0.75rem 2rem;
  border: 1px solid rgba(0, 123, 255, 0.2);
}

.notification-list-btn-load-more:hover {
  background: rgba(0, 123, 255, 0.15);
}

.notification-list-btn-page {
  padding: 0.5rem 0.75rem;
  min-width: 2.5rem;
}

.notification-list-btn-page.active {
  background: #007bff;
  color: white;
}

.notification-list-btn-text {
  background: transparent;
  color: #007bff;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

/* 统计信息样式 */
.notification-list-stats {
  display: flex;
  gap: 2rem;
  padding: 1rem 1.5rem;
  background: rgba(0, 123, 255, 0.02);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.notification-list-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.notification-list-stat-label {
  font-size: 0.75rem;
  color: #6c757d;
  font-weight: 500;
}

.notification-list-stat-value {
  font-size: 1.25rem;
  font-weight: 600;
  color: #212529;
}

.notification-list-stat-value-unread {
  color: #007bff;
}

/* 筛选器样式 */
.notification-list-filters {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.notification-list-search {
  position: relative;
  margin-bottom: 1rem;
}

.notification-list-search-input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid rgba(0, 0, 0, 0.15);
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s ease;
}

.notification-list-search-input:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

.notification-list-search-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  width: 1rem;
  height: 1rem;
  color: #6c757d;
}

.notification-list-filter-controls {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 0.75rem;
}

.notification-list-filter-select {
  padding: 0.5rem;
  border: 1px solid rgba(0, 0, 0, 0.15);
  border-radius: 0.375rem;
  font-size: 0.813rem;
  background: white;
  cursor: pointer;
  transition: all 0.2s ease;
}

.notification-list-filter-select:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

/* 内容样式 */
.notification-list-content {
  min-height: 200px;
}

/* 加载状态 */
.notification-list-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  gap: 1rem;
}

.notification-list-spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid rgba(0, 0, 0, 0.1);
  border-top: 3px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.notification-list-loading-text {
  color: #6c757d;
  font-size: 0.875rem;
}

.notification-list-loading-more {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  gap: 0.5rem;
  color: #6c757d;
  font-size: 0.813rem;
}

.notification-list-spinner-small {
  width: 1rem;
  height: 1rem;
  border: 2px solid rgba(0, 0, 0, 0.1);
  border-top: 2px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

/* 空状态 */
.notification-list-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  text-align: center;
  gap: 1rem;
}

.notification-list-empty-icon {
  font-size: 3rem;
  opacity: 0.5;
}

.notification-list-empty-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #212529;
  margin: 0;
}

.notification-list-empty-description {
  font-size: 0.875rem;
  color: #6c757d;
  margin: 0;
}

/* 通知列表样式 */
.notification-list-items {
  display: flex;
  flex-direction: column;
}

.notification-list-items > * {
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.notification-list-items > *:last-child {
  border-bottom: none;
}

/* 加载更多 */
.notification-list-load-more {
  display: flex;
  justify-content: center;
  padding: 1rem;
}

/* 分页样式 */
.notification-list-pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.notification-list-pagination-info {
  font-size: 0.875rem;
  color: #6c757d;
}

.notification-list-pagination-controls {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.notification-list-pagination-pages {
  display: flex;
  gap: 0.25rem;
}

/* WebSocket状态 */
.notification-list-websocket-status {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: rgba(0, 0, 0, 0.02);
  border-top: 1px solid rgba(0, 0, 0, 0.05);
  font-size: 0.75rem;
  color: #6c757d;
}

.notification-list-websocket-indicator {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
  background: #dc3545;
  transition: all 0.3s ease;
}

.notification-list-websocket-indicator.connected {
  background: #28a745;
  animation: pulse 2s infinite;
}

/* 动画 */
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@keyframes rotate {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  50% {
    transform: scale(1.2);
    opacity: 0.7;
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notification-list-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .notification-list-actions {
    width: 100%;
    justify-content: space-between;
  }

  .notification-list-stats {
    gap: 1rem;
  }

  .notification-list-filter-controls {
    grid-template-columns: 1fr;
  }

  .notification-list-pagination {
    flex-direction: column;
    gap: 1rem;
  }

  .notification-list-pagination-controls {
    flex-wrap: wrap;
    justify-content: center;
  }

  .size-compact {
    max-width: 100%;
  }
}

@media (max-width: 480px) {
  .notification-list-header {
    padding: 1rem;
  }

  .notification-list-filters {
    padding: 1rem;
  }

  .notification-list-stats {
    flex-direction: column;
    gap: 0.5rem;
    align-items: flex-start;
  }

  .notification-list-actions {
    flex-wrap: wrap;
  }

  .notification-list-pagination-pages {
    display: none;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .notification-list-btn-icon.rotating,
  .notification-list-spinner,
  .notification-list-spinner-small,
  .auto-refresh::after,
  .notification-list-websocket-indicator.connected {
    animation: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .notification-list {
    border: 2px solid currentColor;
  }

  .notification-list-btn {
    border: 1px solid currentColor;
  }

  .notification-list-search-input,
  .notification-list-filter-select {
    border-width: 2px;
  }

  .notification-list-websocket-indicator {
    border: 1px solid currentColor;
  }
}

/* 焦点样式 */
.notification-list-btn:focus-visible,
.notification-list-search-input:focus-visible,
.notification-list-filter-select:focus-visible {
  outline: 2px solid #007bff;
  outline-offset: 2px;
}
</style>