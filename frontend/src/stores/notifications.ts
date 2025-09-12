import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { notificationService } from '@/services/notificationService'
import type { 
  Notification,
  NotificationSettings,
  NotificationRequest,
  NotificationResponse,
  NotificationStatistics,
  NotificationState,
  DEFAULT_NOTIFICATION_SETTINGS
} from '@/types/notifications'

/**
 * 通知状态管理 - 使用Pinia进行通知系统的状态管理
 */
export const useNotificationsStore = defineStore('notifications', () => {
  // 基础状态
  const notifications = ref<Notification[]>([])
  const totalCount = ref(0)
  const unreadCount = ref(0)
  const readCount = ref(0)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const settings = ref<NotificationSettings | null>(null)
  const websocketConnected = ref(false)
  
  // 分页和筛选状态
  const filters = ref<NotificationRequest>({
    pageNumber: 1,
    pageSize: 20,
    sortBy: 'createdAt',
    sortOrder: 'desc'
  })
  const currentPage = ref(1)
  const pageSize = ref(20)
  const hasMore = ref(true)
  
  // 统计信息
  const statistics = ref<NotificationStatistics | null>(null)
  
  // 计算属性
  const isLoading = computed(() => loading.value)
  const hasError = computed(() => error.value !== null)
  const hasUnread = computed(() => unreadCount.value > 0)
  const isWebSocketConnected = computed(() => websocketConnected.value)
  
  // 分组通知
  const unreadNotifications = computed(() => 
    notifications.value.filter(n => n.status === 'Unread')
  )
  
  const readNotifications = computed(() => 
    notifications.value.filter(n => n.status === 'Read')
  )
  
  const highPriorityNotifications = computed(() => 
    notifications.value.filter(n => 
      n.priority === 'High' || n.priority === 'Urgent'
    )
  )
  
  // 通知分类
  const notificationsByType = computed(() => {
    const grouped: Record<string, Notification[]> = {}
    notifications.value.forEach(notification => {
      const type = notification.type
      if (!grouped[type]) {
        grouped[type] = []
      }
      grouped[type].push(notification)
    })
    return grouped
  })
  
  // 通知按日期分组
  const notificationsByDate = computed(() => {
    const grouped: Record<string, Notification[]> = {}
    notifications.value.forEach(notification => {
      const date = new Date(notification.createdAt).toLocaleDateString()
      if (!grouped[date]) {
        grouped[date] = []
      }
      grouped[date].push(notification)
    })
    return grouped
  })

  // 基础通知操作
  async function fetchNotifications(request?: NotificationRequest): Promise<NotificationResponse> {
    try {
      loading.value = true
      error.value = null
      
      const mergedRequest = { ...filters.value, ...request }
      const response = await notificationService.getNotifications(mergedRequest)
      
      // 如果是第一页，替换所有通知
      if (mergedRequest.pageNumber === 1) {
        notifications.value = response.items
      } else {
        // 否则追加通知
        notifications.value.push(...response.items)
      }
      
      totalCount.value = response.totalCount
      unreadCount.value = response.unreadCount
      readCount.value = response.readCount
      currentPage.value = response.pageNumber
      pageSize.value = response.pageSize
      hasMore.value = response.pageNumber < response.totalPages
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchMoreNotifications(): Promise<void> {
    if (!hasMore.value || loading.value) return
    
    const nextPage = currentPage.value + 1
    await fetchNotifications({ ...filters.value, pageNumber: nextPage })
  }

  async function refreshNotifications(): Promise<void> {
    await fetchNotifications({ ...filters.value, pageNumber: 1 })
  }

  async function getNotification(id: string): Promise<Notification> {
    try {
      loading.value = true
      error.value = null
      
      const notification = await notificationService.getNotification(id)
      
      // 更新本地通知
      const index = notifications.value.findIndex(n => n.id === id)
      if (index !== -1) {
        notifications.value[index] = notification
      }
      
      return notification
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知详情失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function markAsRead(id: string): Promise<void> {
    try {
      await notificationService.markAsRead(id)
      
      // 更新本地状态
      const notification = notifications.value.find(n => n.id === id)
      if (notification) {
        notification.status = 'Read'
        notification.readAt = new Date().toISOString()
        unreadCount.value = Math.max(0, unreadCount.value - 1)
        readCount.value += 1
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记为已读失败'
      throw err
    }
  }

  async function markAsUnread(id: string): Promise<void> {
    try {
      await notificationService.markAsUnread(id)
      
      // 更新本地状态
      const notification = notifications.value.find(n => n.id === id)
      if (notification) {
        notification.status = 'Unread'
        notification.readAt = undefined
        unreadCount.value += 1
        readCount.value = Math.max(0, readCount.value - 1)
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记为未读失败'
      throw err
    }
  }

  async function markAllAsRead(): Promise<number> {
    try {
      const count = await notificationService.markAllAsRead()
      
      // 更新本地状态
      notifications.value.forEach(notification => {
        notification.status = 'Read'
        notification.readAt = new Date().toISOString()
      })
      unreadCount.value = 0
      readCount.value = totalCount.value
      
      return count
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记全部为已读失败'
      throw err
    }
  }

  async function archiveNotification(id: string): Promise<void> {
    try {
      await notificationService.archiveNotification(id)
      
      // 更新本地状态
      const notification = notifications.value.find(n => n.id === id)
      if (notification) {
        notification.status = 'Archived'
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '归档通知失败'
      throw err
    }
  }

  async function deleteNotification(id: string): Promise<void> {
    try {
      await notificationService.deleteNotification(id)
      
      // 更新本地状态
      const index = notifications.value.findIndex(n => n.id === id)
      if (index !== -1) {
        const notification = notifications.value[index]
        notifications.value.splice(index, 1)
        totalCount.value = Math.max(0, totalCount.value - 1)
        
        if (notification.status === 'Unread') {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
        } else if (notification.status === 'Read') {
          readCount.value = Math.max(0, readCount.value - 1)
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除通知失败'
      throw err
    }
  }

  // 批量操作
  async function batchMarkAsRead(notificationIds: string[]): Promise<number> {
    try {
      const count = await notificationService.batchMarkAsRead({
        notificationIds,
        operation: 'mark_read'
      })
      
      // 更新本地状态
      notificationIds.forEach(id => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && notification.status === 'Unread') {
          notification.status = 'Read'
          notification.readAt = new Date().toISOString()
          unreadCount.value = Math.max(0, unreadCount.value - 1)
          readCount.value += 1
        }
      })
      
      return count
    } catch (err) {
      error.value = err instanceof Error ? err.message : '批量标记为已读失败'
      throw err
    }
  }

  async function batchDeleteNotifications(notificationIds: string[]): Promise<number> {
    try {
      const count = await notificationService.batchDelete({
        notificationIds,
        operation: 'delete'
      })
      
      // 更新本地状态
      const deletedNotifications = notifications.value.filter(n => 
        notificationIds.includes(n.id)
      )
      
      notifications.value = notifications.value.filter(n => 
        !notificationIds.includes(n.id)
      )
      
      totalCount.value = Math.max(0, totalCount.value - count)
      
      // 更新未读和已读计数
      const deletedUnread = deletedNotifications.filter(n => n.status === 'Unread').length
      const deletedRead = deletedNotifications.filter(n => n.status === 'Read').length
      
      unreadCount.value = Math.max(0, unreadCount.value - deletedUnread)
      readCount.value = Math.max(0, readCount.value - deletedRead)
      
      return count
    } catch (err) {
      error.value = err instanceof Error ? err.message : '批量删除失败'
      throw err
    }
  }

  // 通知设置
  async function fetchNotificationSettings(): Promise<NotificationSettings> {
    try {
      loading.value = true
      error.value = null
      
      const fetchedSettings = await notificationService.getOrCreateNotificationSettings()
      settings.value = fetchedSettings
      
      return fetchedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知设置失败'
      // 如果获取失败，使用默认设置
      settings.value = { ...DEFAULT_NOTIFICATION_SETTINGS }
      throw err
    } finally {
      loading.value = false
    }
  }

  async function updateNotificationSettings(newSettings: NotificationSettings): Promise<NotificationSettings> {
    try {
      loading.value = true
      error.value = null
      
      const updatedSettings = await notificationService.updateNotificationSettings(newSettings)
      settings.value = updatedSettings
      
      return updatedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知设置失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 统计信息
  async function fetchNotificationStatistics(): Promise<NotificationStatistics> {
    try {
      loading.value = true
      error.value = null
      
      const stats = await notificationService.getNotificationStatistics()
      statistics.value = stats
      
      return stats
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知统计失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchUnreadCount(): Promise<number> {
    try {
      const count = await notificationService.getUnreadCount()
      unreadCount.value = count
      return count
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取未读数量失败'
      throw err
    }
  }

  // WebSocket连接
  async function connectWebSocket(userId: string): Promise<void> {
    try {
      await notificationService.connectWebSocket(userId)
      websocketConnected.value = true
      
      // 监听新通知
      notificationService.onMessage('notification', (notification: Notification) => {
        addNotification(notification)
      })
      
      // 监听通知更新
      notificationService.onMessage('notification_update', (notification: Notification) => {
        updateNotificationInList(notification)
      })
      
      // 监听通知删除
      notificationService.onMessage('notification_delete', (notificationId: string) => {
        removeNotificationFromList(notificationId)
      })
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '连接WebSocket失败'
      websocketConnected.value = false
      throw err
    }
  }

  function disconnectWebSocket(): void {
    notificationService.disconnectWebSocket()
    websocketConnected.value = false
  }

  // WebSocket消息处理
  function addNotification(notification: Notification): void {
    // 添加到通知列表开头
    notifications.value.unshift(notification)
    totalCount.value += 1
    
    if (notification.status === 'Unread') {
      unreadCount.value += 1
    } else if (notification.status === 'Read') {
      readCount.value += 1
    }
    
    // 如果通知数量过多，移除最早的通知
    if (notifications.value.length > 100) {
      const removed = notifications.value.pop()
      if (removed) {
        totalCount.value = Math.max(0, totalCount.value - 1)
        if (removed.status === 'Unread') {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
        } else if (removed.status === 'Read') {
          readCount.value = Math.max(0, readCount.value - 1)
        }
      }
    }
  }

  function updateNotificationInList(updatedNotification: Notification): void {
    const index = notifications.value.findIndex(n => n.id === updatedNotification.id)
    if (index !== -1) {
      const oldNotification = notifications.value[index]
      notifications.value[index] = updatedNotification
      
      // 更新计数
      if (oldNotification.status !== updatedNotification.status) {
        if (oldNotification.status === 'Unread' && updatedNotification.status === 'Read') {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
          readCount.value += 1
        } else if (oldNotification.status === 'Read' && updatedNotification.status === 'Unread') {
          unreadCount.value += 1
          readCount.value = Math.max(0, readCount.value - 1)
        }
      }
    }
  }

  function removeNotificationFromList(notificationId: string): void {
    const index = notifications.value.findIndex(n => n.id === notificationId)
    if (index !== -1) {
      const notification = notifications.value[index]
      notifications.value.splice(index, 1)
      totalCount.value = Math.max(0, totalCount.value - 1)
      
      if (notification.status === 'Unread') {
        unreadCount.value = Math.max(0, unreadCount.value - 1)
      } else if (notification.status === 'Read') {
        readCount.value = Math.max(0, readCount.value - 1)
      }
    }
  }

  // 筛选和搜索
  function updateFilters(newFilters: Partial<NotificationRequest>): void {
    filters.value = { ...filters.value, ...newFilters }
  }

  function resetFilters(): void {
    filters.value = {
      pageNumber: 1,
      pageSize: 20,
      sortBy: 'createdAt',
      sortOrder: 'desc'
    }
  }

  function searchNotifications(searchTerm: string): void {
    updateFilters({ searchTerm })
  }

  // 工具方法
  function getNotificationById(id: string): Notification | undefined {
    return notifications.value.find(n => n.id === id)
  }

  function hasNotification(id: string): boolean {
    return notifications.value.some(n => n.id === id)
  }

  function clearNotifications(): void {
    notifications.value = []
    totalCount.value = 0
    unreadCount.value = 0
    readCount.value = 0
  }

  function clearError(): void {
    error.value = null
  }

  // 重置状态
  function $reset(): void {
    notifications.value = []
    totalCount.value = 0
    unreadCount.value = 0
    readCount.value = 0
    loading.value = false
    error.value = null
    settings.value = null
    websocketConnected.value = false
    statistics.value = null
    filters.value = {
      pageNumber: 1,
      pageSize: 20,
      sortBy: 'createdAt',
      sortOrder: 'desc'
    }
    currentPage.value = 1
    pageSize.value = 20
    hasMore.value = true
  }

  return {
    // 状态
    notifications,
    totalCount,
    unreadCount,
    readCount,
    loading,
    error,
    settings,
    websocketConnected,
    filters,
    currentPage,
    pageSize,
    hasMore,
    statistics,
    
    // 计算属性
    isLoading,
    hasError,
    hasUnread,
    isWebSocketConnected,
    unreadNotifications,
    readNotifications,
    highPriorityNotifications,
    notificationsByType,
    notificationsByDate,
    
    // 方法
    fetchNotifications,
    fetchMoreNotifications,
    refreshNotifications,
    getNotification,
    markAsRead,
    markAsUnread,
    markAllAsRead,
    archiveNotification,
    deleteNotification,
    batchMarkAsRead,
    batchDeleteNotifications,
    fetchNotificationSettings,
    updateNotificationSettings,
    fetchNotificationStatistics,
    fetchUnreadCount,
    connectWebSocket,
    disconnectWebSocket,
    updateFilters,
    resetFilters,
    searchNotifications,
    getNotificationById,
    hasNotification,
    clearNotifications,
    clearError,
    $reset
  }
})