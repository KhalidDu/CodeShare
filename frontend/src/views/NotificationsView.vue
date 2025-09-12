<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <div class="max-w-7xl mx-auto page-container py-xl">
      <!-- 页面标题和操作 -->
      <div class="flex items-center justify-between mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 mb-2">通知中心</h1>
          <p class="text-gray-600">查看和管理您的通知</p>
        </div>
        <div class="flex items-center gap-3">
          <button
            @click="markAllAsRead"
            class="px-4 py-2 text-blue-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg transition-colors"
            :disabled="loading || !hasUnread"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
            全部已读
          </button>
          <button
            @click="showSettings = true"
            class="px-4 py-2 bg-gray-100 text-gray-700 hover:bg-gray-200 rounded-lg transition-colors"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            通知设置
          </button>
        </div>
      </div>

      <!-- 统计卡片 -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">全部通知</p>
              <p class="text-2xl font-bold text-gray-900">{{ totalCount }}</p>
            </div>
            <div class="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">未读通知</p>
              <p class="text-2xl font-bold text-orange-600">{{ unreadCount }}</p>
            </div>
            <div class="w-12 h-12 bg-orange-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">今日通知</p>
              <p class="text-2xl font-bold text-green-600">{{ todayCount }}</p>
            </div>
            <div class="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">重要通知</p>
              <p class="text-2xl font-bold text-red-600">{{ importantCount }}</p>
            </div>
            <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- 过滤器和搜索 -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
        <div class="flex flex-col lg:flex-row gap-4">
          <!-- 搜索 -->
          <div class="flex-1">
            <div class="relative">
              <input
                v-model="searchKeyword"
                type="text"
                placeholder="搜索通知..."
                class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                @input="handleSearch"
              />
              <svg class="absolute left-3 top-2.5 w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>

          <!-- 类型过滤 -->
          <div class="lg:w-48">
            <select
              v-model="selectedType"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="handleFilterChange"
            >
              <option value="">全部类型</option>
              <option value="system">系统通知</option>
              <option value="user">用户通知</option>
              <option value="security">安全通知</option>
              <option value="update">更新通知</option>
            </select>
          </div>

          <!-- 状态过滤 -->
          <div class="lg:w-48">
            <select
              v-model="selectedStatus"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="handleFilterChange"
            >
              <option value="">全部状态</option>
              <option value="unread">未读</option>
              <option value="read">已读</option>
            </select>
          </div>

          <!-- 时间范围 -->
          <div class="lg:w-48">
            <select
              v-model="selectedTimeRange"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              @change="handleFilterChange"
            >
              <option value="">全部时间</option>
              <option value="today">今天</option>
              <option value="week">本周</option>
              <option value="month">本月</option>
              <option value="year">今年</option>
            </select>
          </div>

          <!-- 刷新按钮 -->
          <button
            @click="refreshNotifications"
            class="px-4 py-2 bg-gray-100 text-gray-700 hover:bg-gray-200 rounded-lg transition-colors"
            :disabled="loading"
          >
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            刷新
          </button>
        </div>
      </div>

      <!-- 通知列表 -->
      <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <!-- 加载状态 -->
        <div v-if="loading && notifications.length === 0" class="flex items-center justify-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <p class="mt-4 text-gray-600">加载中...</p>
          </div>
        </div>

        <!-- 空状态 -->
        <div v-else-if="notifications.length === 0" class="flex items-center justify-center py-12">
          <div class="text-center">
            <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
            <h3 class="text-lg font-medium text-gray-900 mb-2">暂无通知</h3>
            <p class="text-gray-600">您还没有任何通知</p>
          </div>
        </div>

        <!-- 通知列表内容 -->
        <div v-else class="divide-y divide-gray-200">
          <div
            v-for="notification in notifications"
            :key="notification.id"
            class="p-6 hover:bg-gray-50 transition-colors"
            :class="{ 'bg-blue-50': !notification.isRead }"
          >
            <div class="flex items-start gap-4">
              <!-- 通知图标 -->
              <div class="flex-shrink-0">
                <div
                  class="w-10 h-10 rounded-full flex items-center justify-center"
                  :class="getNotificationIconClass(notification.type)"
                >
                  <svg
                    class="w-5 h-5"
                    :class="getNotificationIconColorClass(notification.type)"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      :d="getNotificationIconPath(notification.type)"
                    />
                  </svg>
                </div>
              </div>

              <!-- 通知内容 -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between mb-2">
                  <div class="flex-1">
                    <h3 class="text-sm font-medium text-gray-900 mb-1">
                      {{ notification.title }}
                      <span
                        v-if="!notification.isRead"
                        class="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                      >
                        未读
                      </span>
                      <span
                        v-if="notification.isImportant"
                        class="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800"
                      >
                        重要
                      </span>
                    </h3>
                    <p class="text-sm text-gray-600 mb-2">{{ notification.message }}</p>
                    <div class="flex items-center gap-4 text-xs text-gray-500">
                      <span>{{ formatDateTime(notification.createdAt) }}</span>
                      <span>{{ getNotificationTypeLabel(notification.type) }}</span>
                    </div>
                  </div>
                  
                  <!-- 操作按钮 -->
                  <div class="flex items-center gap-2 ml-4">
                    <button
                      v-if="!notification.isRead"
                      @click="markAsRead(notification.id)"
                      class="p-1.5 text-gray-600 hover:text-gray-800 hover:bg-gray-200 rounded-lg transition-colors"
                      title="标记为已读"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                      </svg>
                    </button>
                    <button
                      v-if="notification.action"
                      @click="handleActionClick(notification)"
                      class="p-1.5 text-blue-600 hover:text-blue-800 hover:bg-blue-100 rounded-lg transition-colors"
                      title="查看详情"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                      </svg>
                    </button>
                    <button
                      @click="deleteNotification(notification.id)"
                      class="p-1.5 text-red-600 hover:text-red-800 hover:bg-red-100 rounded-lg transition-colors"
                      title="删除"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </div>

                <!-- 相关数据 -->
                <div v-if="notification.data" class="mt-3 p-3 bg-gray-50 rounded-lg">
                  <div class="text-xs text-gray-600">
                    <div v-for="(value, key) in notification.data" :key="key" class="flex items-center gap-2">
                      <span class="font-medium">{{ key }}:</span>
                      <span>{{ value }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 加载更多 -->
        <div v-if="hasMore && !loading" class="p-6 border-t border-gray-200">
          <button
            @click="loadMore"
            class="w-full py-2 text-blue-600 hover:text-blue-700 font-medium transition-colors"
            :disabled="loadingMore"
          >
            {{ loadingMore ? '加载中...' : '加载更多' }}
          </button>
        </div>
      </div>
    </div>

    <!-- 通知设置对话框 -->
    <NotificationSettings
      v-if="showSettings"
      :visible="showSettings"
      @close="showSettings = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useNotificationStore } from '@/stores/notification'
import { useToastStore } from '@/stores/toast'
import NotificationSettings from '@/components/notifications/NotificationSettings.vue'
import type { Notification } from '@/types/notification'

const router = useRouter()
const authStore = useAuthStore()
const notificationStore = useNotificationStore()
const toastStore = useToastStore()

// 响应式状态
const notifications = ref<Notification[]>([])
const loading = ref(false)
const loadingMore = ref(false)
const hasMore = ref(false)
const searchKeyword = ref('')
const selectedType = ref('')
const selectedStatus = ref('')
const selectedTimeRange = ref('')
const showSettings = ref(false)

// 计算属性
const totalCount = computed(() => notifications.value.length)
const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)
const todayCount = computed(() => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return notifications.value.filter(n => new Date(n.createdAt) >= today).length
})
const importantCount = computed(() => notifications.value.filter(n => 
  n.priority === 'HIGH' || n.priority === 'URGENT' || n.priority === 'CRITICAL'
).length)
const hasUnread = computed(() => unreadCount.value > 0)

// 方法
async function loadNotifications(reset = true) {
  loading.value = true
  
  try {
    const filter: any = {
      page: reset ? 1 : Math.floor(notifications.value.length / 20) + 1,
      pageSize: 20,
      type: selectedType.value !== 'all' ? selectedType.value : undefined,
      status: selectedStatus.value !== 'all' ? selectedStatus.value : undefined,
      search: searchKeyword.value || undefined
    }
    
    const response = await notificationStore.fetchNotifications(filter)
    
    if (reset) {
      notifications.value = response.items || []
    } else {
      notifications.value = [...notifications.value, ...(response.items || [])]
    }
    
    hasMore.value = (response.items?.length || 0) >= 20
  } catch (error) {
    console.error('Failed to load notifications:', error)
    toastStore.error('加载通知失败')
  } finally {
    loading.value = false
  }
}

async function loadMore() {
  if (loadingMore.value) return
  
  loadingMore.value = true
  try {
    await loadNotifications(false)
  } finally {
    loadingMore.value = false
  }
}

async function markAsRead(notificationId: string) {
  try {
    await notificationStore.markAsRead([notificationId])
    
    // 更新本地状态
    const notification = notifications.value.find(n => n.id === notificationId)
    if (notification) {
      notification.isRead = true
    }
    
    toastStore.success('已标记为已读')
  } catch (error) {
    console.error('Failed to mark notification as read:', error)
    toastStore.error('操作失败')
  }
}

async function markAllAsRead() {
  if (!confirm('确定要将所有通知标记为已读吗？')) return
  
  try {
    await notificationStore.markAllAsRead()
    
    // 更新本地状态
    notifications.value.forEach(notification => {
      notification.isRead = true
    })
    
    toastStore.success('全部通知已标记为已读')
  } catch (error) {
    console.error('Failed to mark all notifications as read:', error)
    toastStore.error('操作失败')
  }
}

async function deleteNotification(notificationId: string) {
  if (!confirm('确定要删除这条通知吗？')) return
  
  try {
    await notificationStore.deleteNotification(notificationId)
    
    // 从列表中移除
    notifications.value = notifications.value.filter(n => n.id !== notificationId)
    
    toastStore.success('通知已删除')
  } catch (error) {
    console.error('Failed to delete notification:', error)
    toastStore.error('删除失败')
  }
}

function handleActionClick(notification: Notification) {
  if (notification.action) {
    if (notification.action.startsWith('/')) {
      router.push(notification.action)
    } else {
      window.open(notification.action, '_blank')
    }
  }
}

function handleSearch() {
  loadNotifications()
}

function handleFilterChange() {
  loadNotifications()
}

function refreshNotifications() {
  loadNotifications()
}

function getNotificationIconClass(type: string): string {
  const iconClasses: Record<string, string> = {
    system: 'bg-blue-100',
    user: 'bg-green-100',
    security: 'bg-red-100',
    update: 'bg-purple-100'
  }
  return iconClasses[type] || 'bg-gray-100'
}

function getNotificationIconColorClass(type: string): string {
  const iconColors: Record<string, string> = {
    system: 'text-blue-600',
    user: 'text-green-600',
    security: 'text-red-600',
    update: 'text-purple-600'
  }
  return iconColors[type] || 'text-gray-600'
}

function getNotificationIconPath(type: string): string {
  const iconPaths: Record<string, string> = {
    system: 'M9 3v2m6-2v2M9 19v2m6-2v2M5 9H3m2 6H3m18-6h-2m2 6h-2M7 19h10a2 2 0 002-2V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2zM9 9h6v6H9V9z',
    user: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
    security: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z',
    update: 'M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15'
  }
  return iconPaths[type] || 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
}

function getNotificationTypeLabel(type: string): string {
  const typeLabels: Record<string, string> = {
    system: '系统通知',
    user: '用户通知',
    security: '安全通知',
    update: '更新通知'
  }
  return typeLabels[type] || '其他'
}

function formatDateTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 7) return `${days}天前`
  
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 生命周期
onMounted(() => {
  loadNotifications()
})
</script>

<style scoped>
.prose {
  line-height: 1.6;
}

.prose p {
  margin: 0;
}
</style>