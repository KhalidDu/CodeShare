/**
 * 通知相关的组合式API
 * 
 * 本文件提供通知相关的Vue 3 Composition API函数，包括：
 * - 通知状态管理
 * - 通知操作方法
 * - 通知筛选和排序
 * - WebSocket实时通信
 * - 通知设置管理
 * 
 * 使用Pinia进行状态管理，提供类型安全的TypeScript接口
 */

import { ref, computed, onMounted, onUnmounted } from 'vue'
import { notificationService } from '@/services'
import type {
  Notification,
  NotificationFilter,
  NotificationSetting,
  CreateNotificationRequest,
  UpdateNotificationRequest,
  NotificationStats,
  NotificationSummary,
  NotificationPermissions,
  NotificationType,
  NotificationPriority,
  NotificationStatus,
  NotificationChannel,
  NotificationSort,
  NotificationWebSocketEvent,
  NotificationWebSocketConnection,
  PaginatedNotifications,
  NotificationState,
  NotificationActions,
  NotificationGetters
} from '@/types/notification'

/**
 * 使用通知功能的组合式API
 * @returns 通知相关的状态和方法
 */
export function useNotifications() {
  // 响应式状态
  const notifications = ref<Notification[]>([])
  const settings = ref<NotificationSetting[]>([])
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)
  const unreadCount = ref<number>(0)
  const currentFilter = ref<NotificationFilter>({
    sortBy: NotificationSort.CREATED_AT_DESC,
    page: 1,
    pageSize: 20
  })
  const wsConnection = ref<NotificationWebSocketConnection | null>(null)
  const lastRefreshAt = ref<string | null>(null)
  const isAutoRefreshing = ref<boolean>(false)
  const autoRefreshInterval = ref<number>(30000) // 30秒
  const stats = ref<NotificationStats | null>(null)
  const summary = ref<NotificationSummary | null>(null)

  // 自动刷新定时器
  let refreshTimer: NodeJS.Timeout | null = null

  // 计算属性
  const unreadNotifications = computed(() => 
    notifications.value.filter(n => !n.isRead)
  )

  const highPriorityNotifications = computed(() => 
    notifications.value.filter(n => 
      n.priority === NotificationPriority.HIGH || 
      n.priority === NotificationPriority.URGENT ||
      n.priority === NotificationPriority.CRITICAL
    )
  )

  const systemNotifications = computed(() => 
    notifications.value.filter(n => n.type === NotificationType.SYSTEM)
  )

  const userNotifications = computed(() => 
    notifications.value.filter(n => 
      n.type !== NotificationType.SYSTEM && 
      n.type !== NotificationType.MAINTENANCE
    )
  )

  const notificationsByType = computed(() => {
    const groups: Record<NotificationType, Notification[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.type]) {
        groups[notification.type] = []
      }
      groups[notification.type].push(notification)
    })
    return groups
  })

  const notificationsByStatus = computed(() => {
    const groups: Record<NotificationStatus, Notification[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.status]) {
        groups[notification.status] = []
      }
      groups[notification.status].push(notification)
    })
    return groups
  })

  const totalPages = computed(() => {
    // 这里需要从后端API获取总页数，暂时使用简单计算
    return Math.ceil(notifications.value.length / currentFilter.value.pageSize)
  })

  const hasNextPage = computed(() => 
    currentFilter.value.page < totalPages.value
  )

  const hasPreviousPage = computed(() => 
    currentFilter.value.page > 1
  )

  const isWebSocketConnected = computed(() => 
    wsConnection.value?.status === 'connected'
  )

  const hasUnreadNotifications = computed(() => 
    unreadCount.value > 0
  )

  // 通知操作方法
  const fetchNotifications = async (filter?: NotificationFilter) => {
    try {
      loading.value = true
      error.value = null
      
      const updatedFilter = { ...currentFilter.value, ...filter }
      const result = await notificationService.getNotifications(updatedFilter)
      
      notifications.value = result.items
      currentFilter.value = updatedFilter
      lastRefreshAt.value = new Date().toISOString()
      
      // 更新未读数量
      unreadCount.value = result.items.filter(n => !n.isRead).length
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知列表失败'
      console.error('获取通知列表失败:', err)
    } finally {
      loading.value = false
    }
  }

  const markAsRead = async (notificationIds: string[]) => {
    try {
      await notificationService.batchMarkAsRead({ notificationIds })
      
      // 更新本地状态
      notifications.value = notifications.value.map(notification => 
        notificationIds.includes(notification.id) 
          ? { ...notification, isRead: true, readAt: new Date().toISOString() }
          : notification
      )
      
      // 更新未读数量
      unreadCount.value = notifications.value.filter(n => !n.isRead).length
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记通知已读失败'
      console.error('标记通知已读失败:', err)
    }
  }

  const markAllAsRead = async () => {
    try {
      await notificationService.batchMarkAsRead({ 
        markAllAsRead: true,
        userId: currentFilter.value.userId 
      })
      
      // 更新本地状态
      notifications.value = notifications.value.map(notification => ({
        ...notification,
        isRead: true,
        readAt: new Date().toISOString()
      }))
      
      unreadCount.value = 0
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记所有通知已读失败'
      console.error('标记所有通知已读失败:', err)
    }
  }

  const deleteNotifications = async (notificationIds: string[]) => {
    try {
      await notificationService.batchDeleteNotifications({ notificationIds })
      
      // 更新本地状态
      notifications.value = notifications.value.filter(notification => 
        !notificationIds.includes(notification.id)
      )
      
      // 更新未读数量
      unreadCount.value = notifications.value.filter(n => !n.isRead).length
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除通知失败'
      console.error('删除通知失败:', err)
    }
  }

  const createNotification = async (notification: CreateNotificationRequest) => {
    try {
      const newNotification = await notificationService.createNotification(notification)
      
      // 添加到列表
      notifications.value.unshift(newNotification)
      
      // 更新未读数量
      if (!newNotification.isRead) {
        unreadCount.value++
      }
      
      return newNotification
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建通知失败'
      console.error('创建通知失败:', err)
      throw err
    }
  }

  const updateNotification = async (id: string, update: UpdateNotificationRequest) => {
    try {
      const updatedNotification = await notificationService.updateNotification(id, update)
      
      // 更新本地状态
      const index = notifications.value.findIndex(n => n.id === id)
      if (index !== -1) {
        notifications.value[index] = updatedNotification
      }
      
      return updatedNotification
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知失败'
      console.error('更新通知失败:', err)
      throw err
    }
  }

  const fetchSettings = async (userId: string) => {
    try {
      loading.value = true
      error.value = null
      
      const userSettings = await notificationService.getNotificationSettings(userId)
      settings.value = userSettings
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知设置失败'
      console.error('获取通知设置失败:', err)
    } finally {
      loading.value = false
    }
  }

  const updateSettings = async (settingId: string, update: any) => {
    try {
      const updatedSetting = await notificationService.updateNotificationSetting(settingId, update)
      
      // 更新本地状态
      const index = settings.value.findIndex(s => s.id === settingId)
      if (index !== -1) {
        settings.value[index] = updatedSetting
      }
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知设置失败'
      console.error('更新通知设置失败:', err)
    }
  }

  const fetchStats = async (userId: string) => {
    try {
      stats.value = await notificationService.getNotificationStats(userId)
    } catch (err) {
      console.error('获取通知统计失败:', err)
    }
  }

  const fetchSummary = async (userId: string) => {
    try {
      summary.value = await notificationService.getNotificationSummary(userId)
    } catch (err) {
      console.error('获取通知摘要失败:', err)
    }
  }

  const refreshNotifications = async () => {
    await fetchNotifications()
    if (currentFilter.value.userId) {
      await fetchStats(currentFilter.value.userId)
      await fetchSummary(currentFilter.value.userId)
    }
  }

  const clearError = () => {
    error.value = null
  }

  const setFilter = (filter: NotificationFilter) => {
    currentFilter.value = { ...currentFilter.value, ...filter }
  }

  // WebSocket连接管理
  const connectWebSocket = async (userId: string) => {
    try {
      // 设置WebSocket事件监听器
      notificationService.on('connected', (data) => {
        wsConnection.value = {
          connectionId: '',
          userId: data.userId,
          status: 'connected',
          connectedAt: data.connectedAt,
          lastHeartbeat: data.connectedAt,
          reconnectAttempts: 0
        }
      })

      notificationService.on('disconnected', (data) => {
        if (wsConnection.value) {
          wsConnection.value.status = 'disconnected'
          wsConnection.value.reconnectAttempts = (wsConnection.value.reconnectAttempts || 0) + 1
        }
      })

      notificationService.on('error', (data) => {
        error.value = data.error
      })

      notificationService.on('notification', (event: NotificationWebSocketEvent) => {
        handleRealtimeNotification(event)
      })

      // 连接WebSocket
      await notificationService.connectWebSocket(userId)
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '连接WebSocket失败'
      console.error('连接WebSocket失败:', err)
    }
  }

  const disconnectWebSocket = () => {
    notificationService.disconnectWebSocket()
    wsConnection.value = null
  }

  const handleRealtimeNotification = (event: NotificationWebSocketEvent) => {
    switch (event.type) {
      case 'notification_created':
        const newNotification = event.data as Notification
        notifications.value.unshift(newNotification)
        if (!newNotification.isRead) {
          unreadCount.value++
        }
        break
        
      case 'notification_updated':
        const updatedNotification = event.data as Notification
        const index = notifications.value.findIndex(n => n.id === updatedNotification.id)
        if (index !== -1) {
          notifications.value[index] = updatedNotification
          // 更新未读数量
          unreadCount.value = notifications.value.filter(n => !n.isRead).length
        }
        break
        
      case 'notification_deleted':
        const deletedId = (event.data as any).id
        notifications.value = notifications.value.filter(n => n.id !== deletedId)
        unreadCount.value = notifications.value.filter(n => !n.isRead).length
        break
        
      case 'notification_status_changed':
        const statusUpdate = event.data as Notification
        const statusIndex = notifications.value.findIndex(n => n.id === statusUpdate.id)
        if (statusIndex !== -1) {
          notifications.value[statusIndex] = statusUpdate
          unreadCount.value = notifications.value.filter(n => !n.isRead).length
        }
        break
    }
  }

  // 自动刷新管理
  const startAutoRefresh = () => {
    if (refreshTimer) {
      clearInterval(refreshTimer)
    }
    
    isAutoRefreshing.value = true
    refreshTimer = setInterval(() => {
      refreshNotifications()
    }, autoRefreshInterval.value)
  }

  const stopAutoRefresh = () => {
    if (refreshTimer) {
      clearInterval(refreshTimer)
      refreshTimer = null
    }
    isAutoRefreshing.value = false
  }

  const setAutoRefreshInterval = (interval: number) => {
    autoRefreshInterval.value = interval
    if (isAutoRefreshing.value) {
      stopAutoRefresh()
      startAutoRefresh()
    }
  }

  // 分页操作
  const goToPage = (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
      setFilter({ page })
      fetchNotifications()
    }
  }

  const nextPage = () => {
    if (hasNextPage.value) {
      goToPage(currentFilter.value.page + 1)
    }
  }

  const previousPage = () => {
    if (hasPreviousPage.value) {
      goToPage(currentFilter.value.page - 1)
    }
  }

  // 筛选和排序
  const filterByType = (type: NotificationType) => {
    setFilter({ type, page: 1 })
    fetchNotifications()
  }

  const filterByStatus = (status: NotificationStatus) => {
    setFilter({ status, page: 1 })
    fetchNotifications()
  }

  const sortBy = (sortBy: NotificationSort) => {
    setFilter({ sortBy, page: 1 })
    fetchNotifications()
  }

  const searchNotifications = (keyword: string) => {
    setFilter({ search: keyword, page: 1 })
    fetchNotifications()
  }

  const clearFilters = () => {
    setFilter({
      sortBy: NotificationSort.CREATED_AT_DESC,
      page: 1,
      pageSize: 20
    })
    fetchNotifications()
  }

  // 生命周期钩子
  onMounted(() => {
    // 可以在这里初始化一些状态
  })

  onUnmounted(() => {
    stopAutoRefresh()
    disconnectWebSocket()
  })

  return {
    // 状态
    notifications,
    settings,
    loading,
    error,
    unreadCount,
    currentFilter,
    wsConnection,
    lastRefreshAt,
    isAutoRefreshing,
    autoRefreshInterval,
    stats,
    summary,
    
    // 计算属性
    unreadNotifications,
    highPriorityNotifications,
    systemNotifications,
    userNotifications,
    notificationsByType,
    notificationsByStatus,
    totalPages,
    hasNextPage,
    hasPreviousPage,
    isWebSocketConnected,
    hasUnreadNotifications,
    
    // 方法
    fetchNotifications,
    markAsRead,
    markAllAsRead,
    deleteNotifications,
    createNotification,
    updateNotification,
    fetchSettings,
    updateSettings,
    fetchStats,
    fetchSummary,
    refreshNotifications,
    clearError,
    setFilter,
    connectWebSocket,
    disconnectWebSocket,
    startAutoRefresh,
    stopAutoRefresh,
    setAutoRefreshInterval,
    goToPage,
    nextPage,
    previousPage,
    filterByType,
    filterByStatus,
    sortBy,
    searchNotifications,
    clearFilters
  }
}

/**
 * 使用通知权限的组合式API
 * @param notificationId 通知ID
 * @returns 通知权限相关的状态和方法
 */
export function useNotificationPermissions(notificationId: string) {
  const permissions = ref<NotificationPermissions | null>(null)
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)

  const fetchPermissions = async () => {
    try {
      loading.value = true
      error.value = null
      
      permissions.value = await notificationService.getNotificationPermissions(notificationId)
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知权限失败'
      console.error('获取通知权限失败:', err)
    } finally {
      loading.value = false
    }
  }

  const hasPermission = (permission: keyof NotificationPermissions): boolean => {
    return permissions.value ? permissions.value[permission] : false
  }

  return {
    permissions,
    loading,
    error,
    fetchPermissions,
    hasPermission
  }
}

/**
 * 使用通知设置的组合式API
 * @param userId 用户ID
 * @returns 通知设置相关的状态和方法
 */
export function useNotificationSettings(userId: string) {
  const settings = ref<NotificationSetting[]>([])
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)

  const fetchUserSettings = async () => {
    try {
      loading.value = true
      error.value = null
      
      settings.value = await notificationService.getNotificationSettings(userId)
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知设置失败'
      console.error('获取通知设置失败:', err)
    } finally {
      loading.value = false
    }
  }

  const updateSetting = async (settingId: string, update: any) => {
    try {
      const updatedSetting = await notificationService.updateNotificationSetting(settingId, update)
      
      const index = settings.value.findIndex(s => s.id === settingId)
      if (index !== -1) {
        settings.value[index] = updatedSetting
      }
      
      return updatedSetting
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知设置失败'
      console.error('更新通知设置失败:', err)
      throw err
    }
  }

  const resetSettings = async () => {
    try {
      const defaultSetting = await notificationService.resetNotificationSettings(userId)
      
      const index = settings.value.findIndex(s => s.userId === userId && s.isDefault)
      if (index !== -1) {
        settings.value[index] = defaultSetting
      } else {
        settings.value.push(defaultSetting)
      }
      
      return defaultSetting
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '重置通知设置失败'
      console.error('重置通知设置失败:', err)
      throw err
    }
  }

  return {
    settings,
    loading,
    error,
    fetchUserSettings,
    updateSetting,
    resetSettings
  }
}

/**
 * 使用通知WebSocket的组合式API
 * @param userId 用户ID
 * @returns WebSocket相关的状态和方法
 */
export function useNotificationWebSocket(userId: string) {
  const isConnected = ref<boolean>(false)
  const connectionStatus = ref<'connecting' | 'connected' | 'disconnected' | 'reconnecting'>('disconnected')
  const lastMessage = ref<any>(null)
  const connectionError = ref<string | null>(null)

  const connect = async () => {
    try {
      connectionStatus.value = 'connecting'
      connectionError.value = null
      
      // 设置事件监听器
      notificationService.on('connected', () => {
        isConnected.value = true
        connectionStatus.value = 'connected'
      })

      notificationService.on('disconnected', () => {
        isConnected.value = false
        connectionStatus.value = 'disconnected'
      })

      notificationService.on('error', (data) => {
        connectionError.value = data.error
      })

      notificationService.on('notification', (event) => {
        lastMessage.value = event
      })

      await notificationService.connectWebSocket(userId)
      
    } catch (err) {
      connectionError.value = err instanceof Error ? err.message : '连接WebSocket失败'
      connectionStatus.value = 'disconnected'
      console.error('连接WebSocket失败:', err)
    }
  }

  const disconnect = () => {
    notificationService.disconnectWebSocket()
    isConnected.value = false
    connectionStatus.value = 'disconnected'
  }

  const sendMessage = (message: any) => {
    notificationService.sendWebSocketMessage(message)
  }

  return {
    isConnected,
    connectionStatus,
    lastMessage,
    connectionError,
    connect,
    disconnect,
    sendMessage
  }
}

export type {
  NotificationState,
  NotificationActions,
  NotificationGetters
}