/**
 * 通知Pinia Store - 简化版本
 * 
 * 本文件提供通知系统的核心前端状态管理实现，包括：
 * - 通知数据状态管理
 * - 通知操作方法（创建、更新、删除等）
 * - 通知设置管理
 * - 批量操作管理
 * - 实时通知更新
 * - 持久化存储支持
 * 
 * 遵循Vue 3 + Composition API开发模式，使用Pinia进行状态管理
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { notificationService } from '../services/notificationService'
import type {
  Notification,
  CreateNotificationRequest,
  UpdateNotificationRequest,
  NotificationFilter,
  NotificationSetting,
  CreateNotificationSettingRequest,
  UpdateNotificationSettingRequest,
  NotificationStats,
  NotificationSummary,
  NotificationWebSocketEvent,
  NotificationWebSocketConnection,
  NotificationType,
  NotificationPriority,
  NotificationStatus,
  NotificationChannel,
  PaginatedNotifications
} from '../types/notification'

// 默认筛选条件
const DEFAULT_FILTER: NotificationFilter = {
  userId: '',
  type: undefined,
  priority: undefined,
  status: undefined,
  relatedEntityType: undefined,
  relatedEntityId: undefined,
  triggeredByUserId: undefined,
  action: undefined,
  channel: undefined,
  isRead: undefined,
  isArchived: false,
  isDeleted: false,
  tag: undefined,
  startDate: undefined,
  endDate: undefined,
  expiresBefore: undefined,
  expiresAfter: undefined,
  requiresConfirmation: undefined,
  search: undefined,
  sortBy: 'CREATED_AT_DESC' as any,
  page: 1,
  pageSize: 20
}

// 默认通知设置
const DEFAULT_NOTIFICATION_SETTING: Partial<NotificationSetting> = {
  enableInApp: true,
  enableEmail: false,
  enablePush: false,
  enableDesktop: false,
  enableSound: true,
  frequency: 'IMMEDIATE' as any,
  enableQuietHours: false,
  emailFrequency: 'NEVER' as any,
  batchIntervalMinutes: 30,
  enableBatching: false,
  language: 'zh-CN',
  timeZone: 'Asia/Shanghai',
  isDefault: false,
  isActive: true
}

/**
 * 通知状态管理Store
 */
export const useNotificationStore = defineStore('notification', () => {
  // ==================== 基础状态 ====================
  
  /** 通知列表 */
  const notifications = ref<Notification[]>([])
  
  /** 通知设置 */
  const settings = ref<NotificationSetting[]>([])
  
  /** 加载状态 */
  const loading = ref(false)
  
  /** 保存状态 */
  const saving = ref(false)
  
  /** 错误信息 */
  const error = ref<string | null>(null)
  
  /** 未读通知数量 */
  const unreadCount = ref(0)
  
  /** 当前筛选条件 */
  const currentFilter = ref<NotificationFilter>({ ...DEFAULT_FILTER })
  
  /** 分页信息 */
  const pagination = ref({
    totalCount: 0,
    totalPages: 0,
    currentPage: 1,
    pageSize: 20,
    hasNextPage: false,
    hasPreviousPage: false
  })
  
  /** 统计信息 */
  const stats = ref<NotificationStats | null>(null)
  
  /** 通知摘要 */
  const summary = ref<NotificationSummary | null>(null)
  
  /** 初始化状态 */
  const initialized = ref(false)
  
  /** 最后刷新时间 */
  const lastRefreshAt = ref<string | null>(null)
  
  /** 是否正在自动刷新 */
  const isAutoRefreshing = ref(false)
  
  /** 自动刷新间隔（毫秒） */
  const autoRefreshInterval = ref(30000)
  
  /** 自动刷新定时器 */
  let autoRefreshTimer: NodeJS.Timeout | null = null

  // ==================== WebSocket状态 ====================
  
  /** WebSocket连接状态 */
  const wsConnection = ref<NotificationWebSocketConnection | null>(null)
  
  /** WebSocket连接状态 */
  const wsStatus = ref<'disconnected' | 'connecting' | 'connected' | 'error'>('disconnected')
  
  /** WebSocket重连次数 */
  const wsReconnectAttempts = ref(0)
  
  /** WebSocket最大重连次数 */
  const wsMaxReconnectAttempts = ref(5)
  
  /** WebSocket重连间隔（毫秒） */
  const wsReconnectInterval = ref(5000)

  // ==================== 持久化存储 ====================
  
  /** 持久化存储键名 */
  const STORAGE_KEYS = {
    NOTIFICATIONS: 'notifications',
    SETTINGS: 'notification_settings',
    FILTERS: 'notification_filters',
    UNREAD_COUNT: 'notification_unread_count',
    LAST_REFRESH: 'notification_last_refresh',
    WS_CONNECTION: 'notification_ws_connection'
  }

  /**
   * 从本地存储加载数据
   */
  function loadFromStorage() {
    try {
      // 加载通知设置
      const savedSettings = localStorage.getItem(STORAGE_KEYS.SETTINGS)
      if (savedSettings) {
        settings.value = JSON.parse(savedSettings)
      }
      
      // 加载筛选条件
      const savedFilter = localStorage.getItem(STORAGE_KEYS.FILTERS)
      if (savedFilter) {
        currentFilter.value = { ...DEFAULT_FILTER, ...JSON.parse(savedFilter) }
      }
      
      // 加载未读数量
      const savedUnreadCount = localStorage.getItem(STORAGE_KEYS.UNREAD_COUNT)
      if (savedUnreadCount) {
        unreadCount.value = parseInt(savedUnreadCount)
      }
      
      // 加载最后刷新时间
      const savedLastRefresh = localStorage.getItem(STORAGE_KEYS.LAST_REFRESH)
      if (savedLastRefresh) {
        lastRefreshAt.value = savedLastRefresh
      }
      
    } catch (err) {
      console.error('从本地存储加载通知数据失败:', err)
    }
  }

  /**
   * 保存数据到本地存储
   */
  function saveToStorage() {
    try {
      // 保存通知设置
      localStorage.setItem(STORAGE_KEYS.SETTINGS, JSON.stringify(settings.value))
      
      // 保存筛选条件
      localStorage.setItem(STORAGE_KEYS.FILTERS, JSON.stringify(currentFilter.value))
      
      // 保存未读数量
      localStorage.setItem(STORAGE_KEYS.UNREAD_COUNT, unreadCount.value.toString())
      
      // 保存最后刷新时间
      if (lastRefreshAt.value) {
        localStorage.setItem(STORAGE_KEYS.LAST_REFRESH, lastRefreshAt.value)
      }
      
    } catch (err) {
      console.error('保存通知数据到本地存储失败:', err)
    }
  }

  // ==================== 计算属性 ====================
  
  /** 是否已初始化 */
  const isInitialized = computed(() => initialized.value)
  
  /** 是否有错误 */
  const hasError = computed(() => error.value !== null)
  
  /** 是否正在加载 */
  const isLoading = computed(() => loading.value)
  
  /** 是否正在保存 */
  const isSaving = computed(() => saving.value)
  
  /** 是否有未读通知 */
  const hasUnreadNotifications = computed(() => unreadCount.value > 0)
  
  /** WebSocket是否已连接 */
  const isWebSocketConnected = computed(() => wsStatus.value === 'connected')
  
  /** 未读通知 */
  const unreadNotifications = computed(() => 
    notifications.value.filter(notification => !notification.isRead)
  )
  
  /** 高优先级通知 */
  const highPriorityNotifications = computed(() => 
    notifications.value.filter(notification => 
      notification.priority === NotificationPriority.HIGH || 
      notification.priority === NotificationPriority.URGENT ||
      notification.priority === NotificationPriority.CRITICAL
    )
  )
  
  /** 系统通知 */
  const systemNotifications = computed(() => 
    notifications.value.filter(notification => 
      notification.type === NotificationType.SYSTEM ||
      notification.type === NotificationType.ANNOUNCEMENT ||
      notification.type === NotificationType.MAINTENANCE
    )
  )
  
  /** 用户通知 */
  const userNotifications = computed(() => 
    notifications.value.filter(notification => 
      notification.type === NotificationType.COMMENT ||
      notification.type === NotificationType.REPLY ||
      notification.type === NotificationType.MESSAGE ||
      notification.type === NotificationType.LIKE ||
      notification.type === NotificationType.SHARE ||
      notification.type === NotificationType.FOLLOW ||
      notification.type === NotificationType.MENTION
    )
  )
  
  /** 按类型分组的通知 */
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
  
  /** 按状态分组的通知 */
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
  
  /** 当前页通知 */
  const currentPageNotifications = computed(() => {
    const start = (pagination.value.currentPage - 1) * pagination.value.pageSize
    const end = start + pagination.value.pageSize
    return notifications.value.slice(start, end)
  })
  
  /** 总页数 */
  const totalPages = computed(() => pagination.value.totalPages)
  
  /** 是否有下一页 */
  const hasNextPage = computed(() => pagination.value.hasNextPage)
  
  /** 是否有上一页 */
  const hasPreviousPage = computed(() => pagination.value.hasPreviousPage)

  // ==================== 基础操作方法 ====================
  
  /**
   * 获取通知列表
   * @param filter 筛选条件
   */
  async function fetchNotifications(filter?: Partial<NotificationFilter>) {
    try {
      loading.value = true
      error.value = null
      
      const finalFilter = { ...currentFilter.value, ...filter }
      const response = await notificationService.getNotifications(finalFilter as NotificationFilter)
      
      notifications.value = response.items
      pagination.value = {
        totalCount: response.totalCount,
        totalPages: Math.ceil(response.totalCount / response.pageSize),
        currentPage: response.pageNumber,
        pageSize: response.pageSize,
        hasNextPage: response.pageNumber < Math.ceil(response.totalCount / response.pageSize),
        hasPreviousPage: response.pageNumber > 1
      }
      
      // 更新当前筛选条件
      currentFilter.value = finalFilter as NotificationFilter
      
      // 更新未读数量
      unreadCount.value = notifications.value.filter(n => !n.isRead).length
      
      // 更新最后刷新时间
      lastRefreshAt.value = new Date().toISOString()
      
      // 保存到本地存储
      saveToStorage()
      
      initialized.value = true
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 刷新通知列表
   */
  async function refreshNotifications() {
    await fetchNotifications()
  }
  
  /**
   * 创建通知
   * @param notification 通知创建请求
   */
  async function createNotification(notification: CreateNotificationRequest): Promise<Notification> {
    try {
      saving.value = true
      error.value = null
      
      const newNotification = await notificationService.createNotification(notification)
      
      // 添加到通知列表
      notifications.value.unshift(newNotification)
      
      // 更新未读数量
      if (!newNotification.isRead) {
        unreadCount.value++
      }
      
      // 保存到本地存储
      saveToStorage()
      
      return newNotification
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建通知失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 更新通知
   * @param id 通知ID
   * @param update 更新请求
   */
  async function updateNotification(id: string, update: UpdateNotificationRequest): Promise<Notification> {
    try {
      saving.value = true
      error.value = null
      
      const updatedNotification = await notificationService.updateNotification(id, update)
      
      // 更新通知列表
      const index = notifications.value.findIndex(n => n.id === id)
      if (index !== -1) {
        const oldNotification = notifications.value[index]
        notifications.value[index] = updatedNotification
        
        // 更新未读数量
        if (oldNotification.isRead !== updatedNotification.isRead) {
          if (updatedNotification.isRead) {
            unreadCount.value = Math.max(0, unreadCount.value - 1)
          } else {
            unreadCount.value++
          }
        }
      }
      
      // 保存到本地存储
      saveToStorage()
      
      return updatedNotification
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 删除通知
   * @param id 通知ID
   */
  async function deleteNotification(id: string): Promise<void> {
    try {
      saving.value = true
      error.value = null
      
      await notificationService.deleteNotification(id)
      
      // 从通知列表中移除
      const index = notifications.value.findIndex(n => n.id === id)
      if (index !== -1) {
        const deletedNotification = notifications.value[index]
        notifications.value.splice(index, 1)
        
        // 更新未读数量
        if (!deletedNotification.isRead) {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
        }
      }
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除通知失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 标记通知为已读
   * @param notificationIds 通知ID数组
   */
  async function markAsRead(notificationIds: string[]): Promise<void> {
    try {
      saving.value = true
      error.value = null
      
      // 批量标记已读
      await notificationService.batchMarkAsRead({ notificationIds })
      
      // 更新本地状态
      notificationIds.forEach(id => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && !notification.isRead) {
          notification.isRead = true
          notification.readAt = new Date().toISOString()
        }
      })
      
      // 更新未读数量
      unreadCount.value = Math.max(0, unreadCount.value - notificationIds.length)
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记已读失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 标记所有通知为已读
   */
  async function markAllAsRead(): Promise<void> {
    try {
      saving.value = true
      error.value = null
      
      await notificationService.batchMarkAsRead({ markAllAsRead: true })
      
      // 更新本地状态
      notifications.value.forEach(notification => {
        if (!notification.isRead) {
          notification.isRead = true
          notification.readAt = new Date().toISOString()
        }
      })
      
      // 更新未读数量
      unreadCount.value = 0
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记全部已读失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 批量删除通知
   * @param notificationIds 通知ID数组
   */
  async function deleteNotifications(notificationIds: string[]): Promise<void> {
    try {
      saving.value = true
      error.value = null
      
      await notificationService.batchDeleteNotifications({ notificationIds })
      
      // 从通知列表中移除
      const deletedNotifications = notifications.value.filter(n => 
        notificationIds.includes(n.id)
      )
      
      notifications.value = notifications.value.filter(n => 
        !notificationIds.includes(n.id)
      )
      
      // 更新未读数量
      const deletedUnreadCount = deletedNotifications.filter(n => !n.isRead).length
      unreadCount.value = Math.max(0, unreadCount.value - deletedUnreadCount)
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '批量删除失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // ==================== 通知设置管理 ====================
  
  /**
   * 获取通知设置
   * @param userId 用户ID
   */
  async function fetchSettings(userId: string): Promise<void> {
    try {
      loading.value = true
      error.value = null
      
      const userSettings = await notificationService.getNotificationSettings(userId)
      settings.value = userSettings
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知设置失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 创建通知设置
   * @param setting 设置创建请求
   */
  async function createSetting(setting: CreateNotificationSettingRequest): Promise<NotificationSetting> {
    try {
      saving.value = true
      error.value = null
      
      const newSetting = await notificationService.createNotificationSetting(setting)
      settings.value.push(newSetting)
      
      // 保存到本地存储
      saveToStorage()
      
      return newSetting
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建通知设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 更新通知设置
   * @param id 设置ID
   * @param setting 设置更新请求
   */
  async function updateSetting(id: string, setting: UpdateNotificationSettingRequest): Promise<NotificationSetting> {
    try {
      saving.value = true
      error.value = null
      
      const updatedSetting = await notificationService.updateNotificationSetting(id, setting)
      
      const index = settings.value.findIndex(s => s.id === id)
      if (index !== -1) {
        settings.value[index] = updatedSetting
      }
      
      // 保存到本地存储
      saveToStorage()
      
      return updatedSetting
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }
  
  /**
   * 删除通知设置
   * @param id 设置ID
   */
  async function deleteSetting(id: string): Promise<void> {
    try {
      saving.value = true
      error.value = null
      
      await notificationService.deleteNotificationSetting(id)
      
      settings.value = settings.value.filter(s => s.id !== id)
      
      // 保存到本地存储
      saveToStorage()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除通知设置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  // ==================== 统计和分析 ====================
  
  /**
   * 获取通知统计信息
   * @param userId 用户ID
   */
  async function fetchStats(userId: string): Promise<void> {
    try {
      loading.value = true
      error.value = null
      
      const notificationStats = await notificationService.getNotificationStats(userId)
      stats.value = notificationStats
      
      // 更新未读数量
      unreadCount.value = notificationStats.unreadCount
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知统计失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 获取通知摘要
   * @param userId 用户ID
   */
  async function fetchSummary(userId: string): Promise<void> {
    try {
      loading.value = true
      error.value = null
      
      const notificationSummary = await notificationService.getNotificationSummary(userId)
      summary.value = notificationSummary
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知摘要失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // ==================== WebSocket实时通信 ====================
  
  /**
   * 连接WebSocket
   * @param userId 用户ID
   */
  async function connectWebSocket(userId: string): Promise<void> {
    try {
      wsStatus.value = 'connecting'
      error.value = null
      
      // 设置事件监听器
      notificationService.on('connected', (data) => {
        wsStatus.value = 'connected'
        wsReconnectAttempts.value = 0
        
        wsConnection.value = {
          connectionId: data.connectionId || `conn_${Date.now()}`,
          userId: data.userId,
          status: 'connected',
          connectedAt: data.connectedAt,
          lastHeartbeat: data.connectedAt,
          reconnectAttempts: 0
        }
      })
      
      notificationService.on('disconnected', (data) => {
        wsStatus.value = 'disconnected'
        if (wsConnection.value) {
          wsConnection.value.status = 'disconnected'
        }
      })
      
      notificationService.on('error', (data) => {
        wsStatus.value = 'error'
        error.value = data.error
        if (wsConnection.value) {
          wsConnection.value.error = data.error
        }
      })
      
      notificationService.on('notification', (event: NotificationWebSocketEvent) => {
        handleWebSocketNotification(event)
      })
      
      // 连接WebSocket
      await notificationService.connectWebSocket(userId)
      
    } catch (err) {
      wsStatus.value = 'error'
      error.value = err instanceof Error ? err.message : '连接WebSocket失败'
      throw err
    }
  }
  
  /**
   * 断开WebSocket连接
   */
  function disconnectWebSocket(): void {
    notificationService.disconnectWebSocket()
    wsStatus.value = 'disconnected'
    wsConnection.value = null
  }
  
  /**
   * 处理WebSocket通知事件
   * @param event 通知事件
   */
  function handleWebSocketNotification(event: NotificationWebSocketEvent): void {
    switch (event.type) {
      case 'notification_created':
        handleNotificationCreated(event.data as Notification)
        break
      case 'notification_updated':
        handleNotificationUpdated(event.data as Notification)
        break
      case 'notification_deleted':
        handleNotificationDeleted(event.data as any)
        break
      case 'notification_status_changed':
        handleNotificationStatusChanged(event.data as any)
        break
    }
  }
  
  /**
   * 处理通知创建事件
   * @param notification 新通知
   */
  function handleNotificationCreated(notification: Notification): void {
    // 添加到通知列表
    notifications.value.unshift(notification)
    
    // 更新未读数量
    if (!notification.isRead) {
      unreadCount.value++
    }
    
    // 保存到本地存储
    saveToStorage()
  }
  
  /**
   * 处理通知更新事件
   * @param notification 更新的通知
   */
  function handleNotificationUpdated(notification: Notification): void {
    const index = notifications.value.findIndex(n => n.id === notification.id)
    if (index !== -1) {
      const oldNotification = notifications.value[index]
      notifications.value[index] = notification
      
      // 更新未读数量
      if (oldNotification.isRead !== notification.isRead) {
        if (notification.isRead) {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
        } else {
          unreadCount.value++
        }
      }
      
      // 保存到本地存储
      saveToStorage()
    }
  }
  
  /**
   * 处理通知删除事件
   * @param data 删除数据
   */
  function handleNotificationDeleted(data: any): void {
    // 这里需要根据实际情况实现删除逻辑
    console.log('通知删除事件:', data)
  }
  
  /**
   * 处理通知状态变化事件
   * @param data 状态变化数据
   */
  function handleNotificationStatusChanged(data: any): void {
    // 这里需要根据实际情况实现状态变化逻辑
    console.log('通知状态变化事件:', data)
  }

  // ==================== 自动刷新管理 ====================
  
  /**
   * 启动自动刷新
   */
  function startAutoRefresh(): void {
    if (autoRefreshTimer) {
      clearInterval(autoRefreshTimer)
    }
    
    isAutoRefreshing.value = true
    autoRefreshTimer = setInterval(async () => {
      try {
        await refreshNotifications()
      } catch (err) {
        console.error('自动刷新通知失败:', err)
      }
    }, autoRefreshInterval.value)
  }
  
  /**
   * 停止自动刷新
   */
  function stopAutoRefresh(): void {
    if (autoRefreshTimer) {
      clearInterval(autoRefreshTimer)
      autoRefreshTimer = null
    }
    isAutoRefreshing.value = false
  }
  
  /**
   * 设置自动刷新间隔
   * @param interval 刷新间隔（毫秒）
   */
  function setAutoRefreshInterval(interval: number): void {
    autoRefreshInterval.value = interval
    
    if (isAutoRefreshing.value) {
      stopAutoRefresh()
      startAutoRefresh()
    }
  }

  // ==================== 筛选和搜索 ====================
  
  /**
   * 设置筛选条件
   * @param filter 筛选条件
   */
  function setFilter(filter: Partial<NotificationFilter>): void {
    currentFilter.value = { ...currentFilter.value, ...filter }
    
    // 保存到本地存储
    saveToStorage()
  }
  
  /**
   * 重置筛选条件
   */
  function resetFilter(): void {
    currentFilter.value = { ...DEFAULT_FILTER }
    
    // 保存到本地存储
    saveToStorage()
  }
  
  /**
   * 搜索通知
   * @param keyword 搜索关键词
   */
  function searchNotifications(keyword: string): Notification[] {
    if (!keyword.trim()) return notifications.value
    
    const searchTerm = keyword.toLowerCase()
    
    return notifications.value.filter(notification => {
      return (
        notification.title.toLowerCase().includes(searchTerm) ||
        (notification.content && notification.content.toLowerCase().includes(searchTerm)) ||
        (notification.message && notification.message.toLowerCase().includes(searchTerm)) ||
        (notification.tag && notification.tag.toLowerCase().includes(searchTerm))
      )
    })
  }

  // ==================== 错误处理 ====================
  
  /**
   * 清除错误
   */
  function clearError(): void {
    error.value = null
  }
  
  /**
   * 重置所有状态
   */
  function $reset(): void {
    // 停止自动刷新
    stopAutoRefresh()
    
    // 断开WebSocket连接
    disconnectWebSocket()
    
    // 重置所有状态
    notifications.value = []
    settings.value = []
    loading.value = false
    saving.value = false
    error.value = null
    unreadCount.value = 0
    currentFilter.value = { ...DEFAULT_FILTER }
    pagination.value = {
      totalCount: 0,
      totalPages: 0,
      currentPage: 1,
      pageSize: 20,
      hasNextPage: false,
      hasPreviousPage: false
    }
    stats.value = null
    summary.value = null
    initialized.value = false
    lastRefreshAt.value = null
    isAutoRefreshing.value = false
    autoRefreshInterval.value = 30000
    wsConnection.value = null
    wsStatus.value = 'disconnected'
    wsReconnectAttempts.value = 0
    
    // 清除本地存储
    Object.values(STORAGE_KEYS).forEach(key => {
      localStorage.removeItem(key)
    })
  }

  // ==================== 初始化 ====================
  
  /**
   * 初始化Store
   * @param userId 用户ID
   */
  async function initialize(userId: string): Promise<void> {
    try {
      // 从本地存储加载数据
      loadFromStorage()
      
      // 获取通知列表
      await fetchNotifications({ userId })
      
      // 获取通知设置
      await fetchSettings(userId)
      
      // 获取统计信息
      await fetchStats(userId)
      
      // 连接WebSocket
      await connectWebSocket(userId)
      
      // 启动自动刷新
      startAutoRefresh()
      
      initialized.value = true
      
    } catch (err) {
      error.value = err instanceof Error ? err.message : '初始化通知Store失败'
      throw err
    }
  }

  // ==================== 监听器 ====================
  
  // 监听设置变化，保存到本地存储
  watch(settings, () => {
    saveToStorage()
  }, { deep: true })
  
  // 监听未读数量变化，更新页面标题
  watch(unreadCount, (newCount) => {
    if (newCount > 0) {
      document.title = `(${newCount}) ${document.title.replace(/^\(\d+\)\s*/, '')}`
    } else {
      document.title = document.title.replace(/^\(\d+\)\s*/, '')
    }
  })

  // ==================== 导出 ====================
  
  return {
    // 状态
    notifications,
    settings,
    loading,
    saving,
    error,
    unreadCount,
    currentFilter,
    pagination,
    stats,
    summary,
    initialized,
    lastRefreshAt,
    isAutoRefreshing,
    autoRefreshInterval,
    wsConnection,
    wsStatus,
    wsReconnectAttempts,
    wsMaxReconnectAttempts,
    wsReconnectInterval,
    
    // 计算属性
    isInitialized,
    hasError,
    isLoading,
    isSaving,
    hasUnreadNotifications,
    isWebSocketConnected,
    unreadNotifications,
    highPriorityNotifications,
    systemNotifications,
    userNotifications,
    notificationsByType,
    notificationsByStatus,
    currentPageNotifications,
    totalPages,
    hasNextPage,
    hasPreviousPage,
    
    // 方法
    fetchNotifications,
    refreshNotifications,
    createNotification,
    updateNotification,
    deleteNotification,
    markAsRead,
    markAllAsRead,
    deleteNotifications,
    fetchSettings,
    createSetting,
    updateSetting,
    deleteSetting,
    fetchStats,
    fetchSummary,
    connectWebSocket,
    disconnectWebSocket,
    startAutoRefresh,
    stopAutoRefresh,
    setAutoRefreshInterval,
    setFilter,
    resetFilter,
    searchNotifications,
    clearError,
    initialize,
    $reset
  }
})

// 导出类型
export type NotificationStore = ReturnType<typeof useNotificationStore>