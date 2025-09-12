/**
 * CodeShare项目通知系统Pinia状态管理
 * 
 * 本文件提供通知系统的完整前端状态管理，包括：
 * - 通知状态管理
 * - 通知CRUD操作
 * - 通知状态管理（已读/未读/归档/恢复）
 * - 通知设置管理
 * - 通知筛选和搜索
 * - 通知统计信息
 * - 通知批量操作
 * - 实时通知更新
 * - 通知模板管理
 * - 通知订阅管理
 * 
 * 遵循Vue 3 + Composition API开发模式，使用Pinia进行状态管理，使用TypeScript确保类型安全
 * 参考现有的commentStore.ts和messageStore.ts实现模式
 * 
 * @version 2.0.0
 * @lastUpdated 2025-09-12
 */

import { defineStore } from 'pinia'
import { ref, computed, reactive, watch } from 'vue'
import { notificationService } from '@/services/notificationService'
import type {
  // 基础类型
  Notification,
  NotificationResponse,
  NotificationSettings,
  NotificationTemplate,
  NotificationSubscription,
  NotificationStats,
  NotificationPermissions,
  NotificationDeliveryHistory,
  
  // 枚举类型
  NotificationType,
  NotificationPriority,
  NotificationStatus,
  NotificationDeliveryStatus,
  NotificationChannel,
  NotificationFrequency,
  SubscriptionType,
  BulkNotificationOperationType,
  NotificationEventType,
  
  // 请求和响应类型
  CreateNotificationRequest,
  UpdateNotificationRequest,
  NotificationFilter,
  NotificationSearchOptions,
  BulkNotificationOperation,
  BulkNotificationResult,
  NotificationEvent,
  NotificationRealtimeUpdate,
  
  // 设置相关类型
  DoNotDisturbSettings,
  NotificationPreference,
  UnsubscribeRequest,
  
  // 模板相关类型
  TemplateVariable,
  TemplateVariableType,
  
  // 分页结果类型
  PaginatedNotifications,
  PaginatedNotificationSettings,
  PaginatedNotificationTemplates,
  PaginatedNotificationSubscriptions,
  PaginatedNotificationDeliveryHistory,
  
  // 工具类型
  NotificationCreateOptions,
  NotificationUpdateOptions,
  NotificationQueryOptions,
  
  // 保留原有类型以确保兼容性
  NotificationSetting,
  CreateNotificationSettingRequest,
  UpdateNotificationSettingRequest,
  NotificationSummary,
  TestNotificationRequest,
  NotificationExportOptions,
  NotificationImportData,
  NotificationFilterRule,
  NotificationAnalytics,
  NotificationQueueTask,
  NotificationWebSocketEvent,
  NotificationWebSocketConnection,
  NotificationRealtimeStatus,
  NotificationFormErrors,
  NotificationSettingFormErrors,
  NotificationStatusUpdateRequest,
  BatchMarkAsReadRequest,
  BatchDeleteNotificationsRequest,
  NotificationSort,
  EmailNotificationFrequency,
  RelatedEntityType,
  NotificationAction,
  DeliveryStatus,
  NOTIFICATION_DEFAULTS
} from '@/types/notification'

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
export const useNotificationStore = defineStore('notificationStore', () => {
  // ==================== 核心通知状态 ====================
  
  // 通知数据状态
  const notifications = ref<NotificationResponse[]>([])
  const currentNotification = ref<NotificationResponse | null>(null)
  const notificationStats = ref<NotificationStats | null>(null)
  const notificationAnalytics = ref<NotificationAnalytics | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const initialized = ref(false)
  const lastUpdated = ref<string | null>(null)
  
  // 通知筛选和分页状态
  const filter = reactive<NotificationFilter>({ ...DEFAULT_FILTER })
  const pagination = ref({
    totalCount: 0,
    currentPage: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  })
  
  // 未读通知计数
  const unreadCount = ref(0)
  const highPriorityUnreadCount = ref(0)
  
  // ==================== 通知设置状态 ====================
  
  // 通知设置
  const settings = ref<NotificationSetting[]>([])
  const currentSetting = ref<NotificationSetting | null>(null)
  const globalSettings = ref<NotificationSettings | null>(null)
  
  // 通知偏好设置
  const preferences = ref<NotificationPreference[]>([])
  const doNotDisturbSettings = ref<DoNotDisturbSettings>({
    enabled: false,
    startTime: '22:00',
    endTime: '08:00',
    daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
    allowUrgent: true
  })
  
  // ==================== 通知模板状态 ====================
  
  // 通知模板
  const templates = ref<NotificationTemplate[]>([])
  const currentTemplate = ref<NotificationTemplate | null>(null)
  const templateVariables = ref<TemplateVariable[]>([])
  
  // ==================== 通知订阅状态 ====================
  
  // 通知订阅
  const subscriptions = ref<NotificationSubscription[]>([])
  const currentSubscription = ref<NotificationSubscription | null>(null)
  const activeSubscriptions = ref<NotificationSubscription[]>([])
  
  // ==================== 通知筛选和搜索状态 ====================
  
  // 搜索和筛选
  const searchResults = ref<NotificationResponse[]>([])
  const searchLoading = ref(false)
  const searchFilter = reactive<NotificationSearchOptions>({
    keyword: '',
    searchScope: 'ALL' as any,
    searchFields: ['title', 'content', 'message'],
    filter: { ...DEFAULT_FILTER },
    sortBy: { field: 'createdAt', order: 'desc' },
    pagination: { page: 1, pageSize: 20 },
    highlightMatches: true,
    fuzzySearch: false
  })
  
  // 通知类型筛选
  const typeFilter = reactive({
    includeTypes: Object.values(NotificationType),
    excludeTypes: [] as NotificationType[],
    priorityFilter: {
      includePriorities: Object.values(NotificationPriority),
      excludePriorities: [] as NotificationPriority[],
      minPriority: undefined as NotificationPriority | undefined,
      maxPriority: undefined as NotificationPriority | undefined
    },
    channelFilter: {
      includeChannels: Object.values(NotificationChannel),
      excludeChannels: [] as NotificationChannel[],
      enabledOnly: true
    }
  })
  
  // ==================== 通知统计状态 ====================
  
  // 统计信息
  const stats = ref<NotificationStats | null>(null)
  const summary = ref<NotificationSummary | null>(null)
  const channelStats = ref<Record<NotificationChannel, any>>({})
  const typeStats = ref<Record<NotificationType, any>>({})
  const priorityStats = ref<Record<NotificationPriority, any>>({})
  
  // ==================== 批量操作状态 ====================
  
  // 批量操作
  const bulkOperations = ref<BulkNotificationOperation[]>([])
  const bulkResults = ref<BulkNotificationResult[]>([])
  const selectedNotifications = ref<Set<string>>(new Set())
  const bulkLoading = ref(false)
  const bulkError = ref<string | null>(null)
  
  // ==================== 实时通信状态 ====================
  
  // WebSocket连接状态
  const websocketConnection = ref<WebSocket | null>(null)
  const isRealtimeConnected = ref(false)
  const realtimeEvents = ref<NotificationEvent[]>([])
  const realtimeStatus = ref<NotificationRealtimeStatus | null>(null)
  
  // ==================== 缓存和性能状态 ====================
  
  // 通知缓存
  const cache = ref<Map<string, NotificationResponse>>(new Map())
  const cacheTimestamp = ref<{ [key: string]: number }>({})
  const CACHE_DURATION = 5 * 60 * 1000 // 5分钟缓存
  
  // 自动刷新状态
  const isAutoRefreshing = ref(false)
  const autoRefreshInterval = ref(30000)
  let autoRefreshTimer: NodeJS.Timeout | null = null
  
  // ==================== 计算属性 ====================
  
  // 基础状态计算
  const isLoading = computed(() => loading.value)
  const hasError = computed(() => error.value !== null)
  const isInitialized = computed(() => initialized.value)
  const hasNotifications = computed(() => notifications.value.length > 0)
  const hasMoreNotifications = computed(() => pagination.value.hasNextPage)
  
  // 通知状态计算
  const unreadNotifications = computed(() => 
    notifications.value.filter(notification => !notification.isRead)
  )
  
  const readNotifications = computed(() => 
    notifications.value.filter(notification => notification.isRead)
  )
  
  const archivedNotifications = computed(() => 
    notifications.value.filter(notification => notification.isArchived)
  )
  
  const deletedNotifications = computed(() => 
    notifications.value.filter(notification => notification.isDeleted)
  )
  
  const highPriorityNotifications = computed(() => 
    notifications.value.filter(notification => 
      notification.priority === NotificationPriority.HIGH || 
      notification.priority === NotificationPriority.URGENT ||
      notification.priority === NotificationPriority.CRITICAL
    )
  )
  
  // 按类型分组的通知
  const notificationsByType = computed(() => {
    const groups: Record<NotificationType, NotificationResponse[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.type]) {
        groups[notification.type] = []
      }
      groups[notification.type].push(notification)
    })
    return groups
  })
  
  // 按状态分组的通知
  const notificationsByStatus = computed(() => {
    const groups: Record<NotificationStatus, NotificationResponse[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.status]) {
        groups[notification.status] = []
      }
      groups[notification.status].push(notification)
    })
    return groups
  })
  
  // 按优先级分组的通知
  const notificationsByPriority = computed(() => {
    const groups: Record<NotificationPriority, NotificationResponse[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.priority]) {
        groups[notification.priority] = []
      }
      groups[notification.priority].push(notification)
    })
    return groups
  })
  
  // 按渠道分组的通知
  const notificationsByChannel = computed(() => {
    const groups: Record<NotificationChannel, NotificationResponse[]> = {} as any
    notifications.value.forEach(notification => {
      if (!groups[notification.channel]) {
        groups[notification.channel] = []
      }
      groups[notification.channel].push(notification)
    })
    return groups
  })
  
  // 缓存状态计算
  const hasActiveCache = computed(() => {
    const now = Date.now()
    return Object.values(cacheTimestamp.value).some(timestamp => 
      now - timestamp < CACHE_DURATION
    )
  })
  
  // 批量操作状态计算
  const hasSelectedNotifications = computed(() => selectedNotifications.value.size > 0)
  const selectedNotificationsList = computed(() => 
    notifications.value.filter(n => selectedNotifications.value.has(n.id))
  )
  
  // 实时状态计算
  const recentRealtimeEvents = computed(() => 
    realtimeEvents.value.slice(-10) // 最近10个事件
  )
  
  // ==================== 工具函数 ====================
  
  // 防抖函数
  const debounce = <T extends (...args: any[]) => any>(
    func: T,
    delay: number
  ): (...args: Parameters<T>) => void => {
    let timeoutId: NodeJS.Timeout
    return (...args: Parameters<T>) => {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(null, args), delay)
    }
  }
  
  // 缓存操作函数
  const setCache = (key: string, data: NotificationResponse) => {
    cache.value.set(key, data)
    cacheTimestamp.value[key] = Date.now()
  }
  
  const getCache = (key: string): NotificationResponse | null => {
    const cached = cache.value.get(key)
    if (cached && cacheTimestamp.value[key]) {
      const now = Date.now()
      if (now - cacheTimestamp.value[key] < CACHE_DURATION) {
        return cached
      }
    }
    return null
  }
  
  const clearCache = (key?: string) => {
    if (key) {
      cache.value.delete(key)
      delete cacheTimestamp.value[key]
    } else {
      cache.value.clear()
      Object.keys(cacheTimestamp.value).forEach(k => {
        delete cacheTimestamp.value[k]
      })
    }
  }
  
  // ==================== 通知CRUD操作 ====================
  
  /**
   * 获取通知列表
   * @param fetchFilter 可选的筛选条件
   */
  async function fetchNotifications(fetchFilter?: Partial<NotificationFilter>) {
    try {
      loading.value = true
      error.value = null
      
      const mergedFilter = { ...filter, ...fetchFilter }
      const result = await notificationService.getNotifications(mergedFilter as NotificationFilter)
      
      notifications.value = result.items
      pagination.value = {
        totalCount: result.totalCount,
        currentPage: result.pageNumber,
        pageSize: result.pageSize,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
        hasNextPage: result.pageNumber < Math.ceil(result.totalCount / result.pageSize),
        hasPreviousPage: result.pageNumber > 1
      }
      
      // 缓存通知
      result.items.forEach(notification => {
        setCache(notification.id, notification)
      })
      
      // 更新未读计数
      unreadCount.value = result.items.filter(n => !n.isRead).length
      highPriorityUnreadCount.value = result.items.filter(n => 
        !n.isRead && (n.priority === NotificationPriority.HIGH || n.priority === NotificationPriority.URGENT)
      ).length
      
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知列表失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 加载更多通知
   */
  async function fetchMoreNotifications() {
    if (loading.value || !hasMoreNotifications.value) return

    try {
      loading.value = true
      error.value = null
      
      const newFilter = { ...filter, page: pagination.value.currentPage + 1 }
      const result = await notificationService.getNotifications(newFilter as NotificationFilter)
      
      notifications.value = [...notifications.value, ...result.items]
      pagination.value = {
        totalCount: result.totalCount,
        currentPage: result.pageNumber,
        pageSize: result.pageSize,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
        hasNextPage: result.pageNumber < Math.ceil(result.totalCount / result.pageSize),
        hasPreviousPage: result.pageNumber > 1
      }
      
      // 缓存新通知
      result.items.forEach(notification => {
        setCache(notification.id, notification)
      })
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载更多通知失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 获取单个通知详情
   * @param notificationId 通知ID
   */
  async function fetchNotification(notificationId: string) {
    try {
      loading.value = true
      error.value = null
      
      // 检查缓存
      const cached = getCache(notificationId)
      if (cached) {
        currentNotification.value = cached
        return cached
      }
      
      const result = await notificationService.getNotificationById(notificationId)
      currentNotification.value = result
      
      // 缓存通知
      setCache(notificationId, result)
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知详情失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 创建新通知
   * @param notificationData 通知数据
   */
  async function createNotification(notificationData: CreateNotificationRequest) {
    try {
      const operationId = `create-${Date.now()}`
      bulkLoading.value = true
      bulkError.value = null
      
      const result = await notificationService.createNotification(notificationData)
      
      // 添加到通知列表
      notifications.value.unshift(result)
      
      // 更新未读计数
      if (!result.isRead) {
        unreadCount.value++
        if (result.priority === NotificationPriority.HIGH || result.priority === NotificationPriority.URGENT) {
          highPriorityUnreadCount.value++
        }
      }
      
      // 缓存新通知
      setCache(result.id, result)
      
      // 更新统计信息
      if (stats.value) {
        stats.value.totalNotifications++
        if (!result.isRead) {
          stats.value.unreadNotifications++
        }
      }
      
      return result
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '创建通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 更新通知
   * @param notificationId 通知ID
   * @param updateData 更新数据
   */
  async function updateNotification(notificationId: string, updateData: UpdateNotificationRequest) {
    try {
      const operationId = `update-${notificationId}`
      bulkLoading.value = true
      bulkError.value = null
      
      const result = await notificationService.updateNotification(notificationId, updateData)
      
      // 更新通知列表
      const notificationIndex = notifications.value.findIndex(n => n.id === notificationId)
      if (notificationIndex !== -1) {
        const oldNotification = notifications.value[notificationIndex]
        notifications.value[notificationIndex] = result
        
        // 更新未读计数
        if (oldNotification.isRead !== result.isRead) {
          if (result.isRead) {
            unreadCount.value = Math.max(0, unreadCount.value - 1)
            if (result.priority === NotificationPriority.HIGH || result.priority === NotificationPriority.URGENT) {
              highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 1)
            }
          } else {
            unreadCount.value++
            if (result.priority === NotificationPriority.HIGH || result.priority === NotificationPriority.URGENT) {
              highPriorityUnreadCount.value++
            }
          }
        }
      }
      
      // 更新当前通知
      if (currentNotification.value?.id === notificationId) {
        currentNotification.value = result
      }
      
      // 更新缓存
      setCache(notificationId, result)
      
      return result
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '更新通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 删除通知
   * @param notificationId 通知ID
   */
  async function deleteNotification(notificationId: string) {
    try {
      const operationId = `delete-${notificationId}`
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.deleteNotification(notificationId)
      
      // 从通知列表中移除
      const deletedNotification = notifications.value.find(n => n.id === notificationId)
      notifications.value = notifications.value.filter(n => n.id !== notificationId)
      
      // 更新未读计数
      if (deletedNotification && !deletedNotification.isRead) {
        unreadCount.value = Math.max(0, unreadCount.value - 1)
        if (deletedNotification.priority === NotificationPriority.HIGH || deletedNotification.priority === NotificationPriority.URGENT) {
          highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 1)
        }
      }
      
      // 清除当前通知
      if (currentNotification.value?.id === notificationId) {
        currentNotification.value = null
      }
      
      // 清除缓存
      clearCache(notificationId)
      
      // 更新统计信息
      if (stats.value) {
        stats.value.totalNotifications--
        if (deletedNotification && !deletedNotification.isRead) {
          stats.value.unreadNotifications--
        }
      }
      
      return true
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '删除通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  // ==================== 通知状态管理 ====================
  
  /**
   * 标记通知为已读
   * @param notificationIds 通知ID数组
   */
  async function markAsRead(notificationIds: string[]) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
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
      const markedCount = notificationIds.filter(id => {
        const notification = notifications.value.find(n => n.id === id)
        return notification && !notification.isRead
      }).length
      
      unreadCount.value = Math.max(0, unreadCount.value - markedCount)
      highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 
        notificationIds.filter(id => {
          const notification = notifications.value.find(n => n.id === id)
          return notification && !notification.isRead && 
            (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT)
        }).length
      )
      
      // 更新当前通知
      if (currentNotification.value && notificationIds.includes(currentNotification.value.id)) {
        currentNotification.value.isRead = true
        currentNotification.value.readAt = new Date().toISOString()
      }
      
      // 更新缓存
      notificationIds.forEach(id => {
        const cached = getCache(id)
        if (cached) {
          cached.isRead = true
          cached.readAt = new Date().toISOString()
          setCache(id, cached)
        }
      })
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '标记已读失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 标记通知为未读
   * @param notificationIds 通知ID数组
   */
  async function markAsUnread(notificationIds: string[]) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      // 批量标记未读（需要调用服务）
      for (const id of notificationIds) {
        await notificationService.markAsUnread(id)
      }
      
      // 更新本地状态
      notificationIds.forEach(id => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && notification.isRead) {
          notification.isRead = false
          notification.readAt = undefined
        }
      })
      
      // 更新未读数量
      const unmarkedCount = notificationIds.filter(id => {
        const notification = notifications.value.find(n => n.id === id)
        return notification && notification.isRead
      }).length
      
      unreadCount.value += unmarkedCount
      highPriorityUnreadCount.value += notificationIds.filter(id => {
        const notification = notifications.value.find(n => n.id === id)
        return notification && notification.isRead && 
          (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT)
      }).length
      
      // 更新当前通知
      if (currentNotification.value && notificationIds.includes(currentNotification.value.id)) {
        currentNotification.value.isRead = false
        currentNotification.value.readAt = undefined
      }
      
      // 更新缓存
      notificationIds.forEach(id => {
        const cached = getCache(id)
        if (cached) {
          cached.isRead = false
          cached.readAt = undefined
          setCache(id, cached)
        }
      })
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '标记未读失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 标记所有通知为已读
   */
  async function markAllAsRead() {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.batchMarkAsRead({ markAllAsRead: true })
      
      // 更新本地状态
      notifications.value.forEach(notification => {
        if (!notification.isRead) {
          notification.isRead = true
          notification.readAt = new Date().toISOString()
        }
      })
      
      // 更新当前通知
      if (currentNotification.value && !currentNotification.value.isRead) {
        currentNotification.value.isRead = true
        currentNotification.value.readAt = new Date().toISOString()
      }
      
      // 更新未读数量
      unreadCount.value = 0
      highPriorityUnreadCount.value = 0
      
      // 更新缓存
      notifications.value.forEach(notification => {
        const cached = getCache(notification.id)
        if (cached) {
          cached.isRead = true
          cached.readAt = new Date().toISOString()
          setCache(notification.id, cached)
        }
      })
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '标记全部已读失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 归档通知
   * @param notificationIds 通知ID数组
   */
  async function archiveNotifications(notificationIds: string[]) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.batchArchiveNotifications(notificationIds)
      
      // 更新本地状态
      notificationIds.forEach(id => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && !notification.isArchived) {
          notification.isArchived = true
          notification.archivedAt = new Date().toISOString()
        }
      })
      
      // 更新当前通知
      if (currentNotification.value && notificationIds.includes(currentNotification.value.id)) {
        currentNotification.value.isArchived = true
        currentNotification.value.archivedAt = new Date().toISOString()
      }
      
      // 更新缓存
      notificationIds.forEach(id => {
        const cached = getCache(id)
        if (cached) {
          cached.isArchived = true
          cached.archivedAt = new Date().toISOString()
          setCache(id, cached)
        }
      })
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '归档通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 恢复通知
   * @param notificationIds 通知ID数组
   */
  async function unarchiveNotifications(notificationIds: string[]) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.batchUnarchiveNotifications(notificationIds)
      
      // 更新本地状态
      notificationIds.forEach(id => {
        const notification = notifications.value.find(n => n.id === id)
        if (notification && notification.isArchived) {
          notification.isArchived = false
          notification.archivedAt = undefined
        }
      })
      
      // 更新当前通知
      if (currentNotification.value && notificationIds.includes(currentNotification.value.id)) {
        currentNotification.value.isArchived = false
        currentNotification.value.archivedAt = undefined
      }
      
      // 更新缓存
      notificationIds.forEach(id => {
        const cached = getCache(id)
        if (cached) {
          cached.isArchived = false
          cached.archivedAt = undefined
          setCache(id, cached)
        }
      })
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '恢复通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 确认通知
   * @param notificationId 通知ID
   */
  async function confirmNotification(notificationId: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.confirmNotification(notificationId)
      
      // 更新本地状态
      const notification = notifications.value.find(n => n.id === notificationId)
      if (notification) {
        notification.requiresConfirmation = false
        notification.confirmedAt = new Date().toISOString()
      }
      
      // 更新当前通知
      if (currentNotification.value?.id === notificationId) {
        currentNotification.value.requiresConfirmation = false
        currentNotification.value.confirmedAt = new Date().toISOString()
      }
      
      // 更新缓存
      const cached = getCache(notificationId)
      if (cached) {
        cached.requiresConfirmation = false
        cached.confirmedAt = new Date().toISOString()
        setCache(notificationId, cached)
      }
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '确认通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  // ==================== 通知设置管理 ====================
  
  /**
   * 获取通知设置
   * @param userId 用户ID
   */
  async function fetchSettings(userId: string) {
    try {
      loading.value = true
      error.value = null
      
      const userSettings = await notificationService.getNotificationSettings(userId)
      settings.value = userSettings
      
      // 获取全局设置
      const globalSetting = await notificationService.getUserNotificationSetting(userId)
      globalSettings.value = {
        id: globalSetting.id,
        userId: globalSetting.userId,
        enableNotifications: globalSetting.enableNotifications || true,
        defaultFrequency: globalSetting.frequency || NotificationFrequency.IMMEDIATE,
        doNotDisturb: globalSetting.quietHoursEnabled ? {
          enabled: true,
          startTime: globalSetting.quietHoursStart || '22:00',
          endTime: globalSetting.quietHoursEnd || '08:00',
          daysOfWeek: globalSetting.quietHoursDays || [0, 1, 2, 3, 4, 5, 6],
          allowUrgent: globalSetting.quietHoursAllowUrgent || true
        } : {
          enabled: false,
          startTime: '22:00',
          endTime: '08:00',
          daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
          allowUrgent: true
        },
        preferences: userSettings.map(setting => ({
          type: setting.notificationType,
          enabled: setting.enableInApp,
          channels: [NotificationChannel.IN_APP],
          priorities: [NotificationPriority.NORMAL, NotificationPriority.HIGH, NotificationPriority.URGENT],
          frequency: setting.frequency,
          enableSound: setting.enableSound,
          enableDesktop: setting.enableDesktop,
          customSettings: {}
        })),
        batchIntervalMinutes: globalSetting.batchIntervalMinutes || 30,
        enableBatching: globalSetting.enableBatching || false,
        emailFrequency: globalSetting.emailFrequency || NotificationFrequency.NEVER,
        language: globalSetting.language || 'zh-CN',
        timeZone: globalSetting.timeZone || 'Asia/Shanghai',
        createdAt: globalSetting.createdAt,
        updatedAt: globalSetting.updatedAt,
        isDefault: globalSetting.isDefault || false,
        isActive: globalSetting.isActive || true
      }
      
      return userSettings
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
  async function createSetting(setting: CreateNotificationSettingRequest) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const newSetting = await notificationService.createNotificationSetting(setting)
      settings.value.push(newSetting)
      
      return newSetting
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '创建通知设置失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 更新通知设置
   * @param settingId 设置ID
   * @param setting 设置更新请求
   */
  async function updateSetting(settingId: string, setting: UpdateNotificationSettingRequest) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const updatedSetting = await notificationService.updateNotificationSetting(settingId, setting)
      
      const index = settings.value.findIndex(s => s.id === settingId)
      if (index !== -1) {
        settings.value[index] = updatedSetting
      }
      
      return updatedSetting
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '更新通知设置失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 删除通知设置
   * @param settingId 设置ID
   */
  async function deleteSetting(settingId: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.deleteNotificationSetting(settingId)
      
      settings.value = settings.value.filter(s => s.id !== settingId)
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '删除通知设置失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 重置通知设置
   * @param userId 用户ID
   */
  async function resetSettings(userId: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const resetSetting = await notificationService.resetNotificationSettings(userId)
      
      const index = settings.value.findIndex(s => s.userId === userId)
      if (index !== -1) {
        settings.value[index] = resetSetting
      } else {
        settings.value.push(resetSetting)
      }
      
      return resetSetting
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '重置通知设置失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  // ==================== 通知统计功能 ====================
  
  /**
   * 获取通知统计信息
   * @param userId 用户ID
   */
  async function fetchStats(userId: string) {
    try {
      loading.value = true
      error.value = null
      
      const notificationStats = await notificationService.getNotificationStats(userId)
      stats.value = notificationStats
      
      // 更新未读数量
      unreadCount.value = notificationStats.unreadNotifications
      highPriorityUnreadCount.value = notificationStats.highPriorityNotifications
      
      return notificationStats
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
  async function fetchSummary(userId: string) {
    try {
      loading.value = true
      error.value = null
      
      const notificationSummary = await notificationService.getNotificationSummary(userId)
      summary.value = notificationSummary
      
      return notificationSummary
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知摘要失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 获取通知分析数据
   * @param filters 分析筛选条件
   */
  async function fetchAnalytics(filters?: any) {
    try {
      loading.value = true
      error.value = null
      
      const analytics = await notificationService.getNotificationAnalytics(filters)
      notificationAnalytics.value = analytics
      
      return analytics
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知分析失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // ==================== 通知筛选和搜索 ====================
  
  /**
   * 搜索通知
   * @param keyword 搜索关键词
   * @param searchOptions 搜索选项
   */
  async function searchNotifications(keyword: string, searchOptions?: Partial<NotificationSearchOptions>) {
    try {
      searchLoading.value = true
      error.value = null
      
      const options = {
        ...searchFilter,
        keyword,
        ...searchOptions
      }
      
      const results = notificationService.searchNotifications(notifications.value, keyword)
      searchResults.value = results
      
      return results
    } catch (err) {
      error.value = err instanceof Error ? err.message : '搜索通知失败'
      throw err
    } finally {
      searchLoading.value = false
    }
  }
  
  /**
   * 更新筛选条件
   * @param newFilter 新的筛选条件
   */
  function updateFilter(newFilter: Partial<NotificationFilter>) {
    Object.assign(filter, newFilter)
  }
  
  /**
   * 重置筛选条件
   */
  function resetFilter() {
    Object.assign(filter, DEFAULT_FILTER)
  }
  
  /**
   * 更新搜索筛选条件
   * @param newFilter 新的搜索筛选条件
   */
  function updateSearchFilter(newFilter: Partial<NotificationSearchOptions>) {
    Object.assign(searchFilter, newFilter)
  }
  
  /**
   * 重置搜索筛选条件
   */
  function resetSearchFilter() {
    Object.assign(searchFilter, {
      keyword: '',
      searchScope: 'ALL' as any,
      searchFields: ['title', 'content', 'message'],
      filter: { ...DEFAULT_FILTER },
      sortBy: { field: 'createdAt', order: 'desc' },
      pagination: { page: 1, pageSize: 20 },
      highlightMatches: true,
      fuzzySearch: false
    })
    searchResults.value = []
  }
  
  // ==================== 通知批量操作 ====================
  
  /**
   * 批量删除通知
   * @param notificationIds 通知ID数组
   */
  async function batchDeleteNotifications(notificationIds: string[]) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
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
      
      highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 
        deletedNotifications.filter(n => !n.isRead && 
          (n.priority === NotificationPriority.HIGH || n.priority === NotificationPriority.URGENT)
        ).length
      )
      
      // 清除选中状态
      notificationIds.forEach(id => selectedNotifications.value.delete(id))
      
      // 清除缓存
      notificationIds.forEach(id => clearCache(id))
      
      // 更新统计信息
      if (stats.value) {
        stats.value.totalNotifications -= deletedNotifications.length
        stats.value.unreadNotifications -= deletedUnreadCount
      }
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '批量删除失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 选择通知
   * @param notificationId 通知ID
   * @param selected 是否选中
   */
  function selectNotification(notificationId: string, selected: boolean) {
    if (selected) {
      selectedNotifications.value.add(notificationId)
    } else {
      selectedNotifications.value.delete(notificationId)
    }
  }
  
  /**
   * 全选通知
   * @param selected 是否选中
   */
  function selectAllNotifications(selected: boolean) {
    if (selected) {
      notifications.value.forEach(n => selectedNotifications.value.add(n.id))
    } else {
      selectedNotifications.value.clear()
    }
  }
  
  /**
   * 清除选中状态
   */
  function clearSelection() {
    selectedNotifications.value.clear()
  }
  
  // ==================== 通知模板管理 ====================
  
  /**
   * 获取通知模板列表
   */
  async function fetchTemplates() {
    try {
      loading.value = true
      error.value = null
      
      const templateList = await notificationService.getNotificationTemplates()
      templates.value = templateList
      
      return templateList
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知模板失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 获取指定模板
   * @param templateId 模板ID
   */
  async function fetchTemplate(templateId: string) {
    try {
      loading.value = true
      error.value = null
      
      const template = await notificationService.getNotificationTemplate(templateId)
      currentTemplate.value = template
      
      return template
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取模板详情失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 创建通知模板
   * @param template 模板数据
   */
  async function createTemplate(template: Omit<NotificationTemplate, 'id' | 'createdAt' | 'updatedAt'>) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const newTemplate = await notificationService.createNotificationTemplate(template)
      templates.value.push(newTemplate)
      
      return newTemplate
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '创建模板失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 更新通知模板
   * @param templateId 模板ID
   * @param template 模板数据
   */
  async function updateTemplate(templateId: string, template: Partial<NotificationTemplate>) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const updatedTemplate = await notificationService.updateNotificationTemplate(templateId, template)
      
      const index = templates.value.findIndex(t => t.id === templateId)
      if (index !== -1) {
        templates.value[index] = updatedTemplate
      }
      
      if (currentTemplate.value?.id === templateId) {
        currentTemplate.value = updatedTemplate
      }
      
      return updatedTemplate
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '更新模板失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 删除通知模板
   * @param templateId 模板ID
   */
  async function deleteTemplate(templateId: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      await notificationService.deleteNotificationTemplate(templateId)
      
      templates.value = templates.value.filter(t => t.id !== templateId)
      
      if (currentTemplate.value?.id === templateId) {
        currentTemplate.value = null
      }
      
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '删除模板失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 使用模板创建通知
   * @param templateId 模板ID
   * @param variables 模板变量
   * @param userId 接收者ID
   */
  async function createNotificationFromTemplate(
    templateId: string,
    variables: Record<string, any>,
    userId: string
  ) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const notification = await notificationService.createNotificationFromTemplate(templateId, variables, userId)
      notifications.value.unshift(notification)
      
      // 更新未读计数
      if (!notification.isRead) {
        unreadCount.value++
        if (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT) {
          highPriorityUnreadCount.value++
        }
      }
      
      // 缓存新通知
      setCache(notification.id, notification)
      
      return notification
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '使用模板创建通知失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  // ==================== 通知订阅管理 ====================
  
  /**
   * 获取通知订阅列表
   * @param userId 用户ID
   * @param includeExpired 是否包含过期订阅
   */
  async function fetchSubscriptions(userId: string, includeExpired = false) {
    try {
      loading.value = true
      error.value = null
      
      const subscriptionList = await notificationService.getNotificationSubscriptions(userId, includeExpired)
      subscriptions.value = subscriptionList
      
      // 过滤出活跃订阅
      activeSubscriptions.value = subscriptionList.filter(s => s.isActive)
      
      return subscriptionList
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知订阅失败'
      throw err
    } finally {
      loading.value = false
    }
  }
  
  /**
   * 创建通知订阅
   * @param subscription 订阅数据
   */
  async function createSubscription(subscription: any) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const newSubscription = await notificationService.createNotificationSubscription(subscription)
      subscriptions.value.push(newSubscription)
      
      if (newSubscription.isActive) {
        activeSubscriptions.value.push(newSubscription)
      }
      
      return newSubscription
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '创建通知订阅失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 更新通知订阅
   * @param subscriptionId 订阅ID
   * @param updates 更新数据
   */
  async function updateSubscription(subscriptionId: string, updates: any) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const updatedSubscription = await notificationService.updateNotificationSubscription(subscriptionId, updates)
      
      const index = subscriptions.value.findIndex(s => s.id === subscriptionId)
      if (index !== -1) {
        subscriptions.value[index] = updatedSubscription
      }
      
      // 更新活跃订阅列表
      const activeIndex = activeSubscriptions.value.findIndex(s => s.id === subscriptionId)
      if (updatedSubscription.isActive) {
        if (activeIndex === -1) {
          activeSubscriptions.value.push(updatedSubscription)
        } else {
          activeSubscriptions.value[activeIndex] = updatedSubscription
        }
      } else {
        if (activeIndex !== -1) {
          activeSubscriptions.value.splice(activeIndex, 1)
        }
      }
      
      if (currentSubscription.value?.id === subscriptionId) {
        currentSubscription.value = updatedSubscription
      }
      
      return updatedSubscription
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '更新通知订阅失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 暂停通知订阅
   * @param subscriptionId 订阅ID
   * @param reason 暂停原因
   */
  async function pauseSubscription(subscriptionId: string, reason?: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const pausedSubscription = await notificationService.pauseNotificationSubscription(subscriptionId, reason)
      
      const index = subscriptions.value.findIndex(s => s.id === subscriptionId)
      if (index !== -1) {
        subscriptions.value[index] = pausedSubscription
      }
      
      // 从活跃订阅中移除
      const activeIndex = activeSubscriptions.value.findIndex(s => s.id === subscriptionId)
      if (activeIndex !== -1) {
        activeSubscriptions.value.splice(activeIndex, 1)
      }
      
      if (currentSubscription.value?.id === subscriptionId) {
        currentSubscription.value = pausedSubscription
      }
      
      return pausedSubscription
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '暂停通知订阅失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 恢复通知订阅
   * @param subscriptionId 订阅ID
   */
  async function resumeSubscription(subscriptionId: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const resumedSubscription = await notificationService.resumeNotificationSubscription(subscriptionId)
      
      const index = subscriptions.value.findIndex(s => s.id === subscriptionId)
      if (index !== -1) {
        subscriptions.value[index] = resumedSubscription
      }
      
      // 添加到活跃订阅
      if (!activeSubscriptions.value.find(s => s.id === subscriptionId)) {
        activeSubscriptions.value.push(resumedSubscription)
      }
      
      if (currentSubscription.value?.id === subscriptionId) {
        currentSubscription.value = resumedSubscription
      }
      
      return resumedSubscription
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '恢复通知订阅失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  /**
   * 取消通知订阅
   * @param subscriptionId 订阅ID
   * @param reason 取消原因
   */
  async function cancelSubscription(subscriptionId: string, reason?: string) {
    try {
      bulkLoading.value = true
      bulkError.value = null
      
      const success = await notificationService.cancelNotificationSubscription(subscriptionId, reason)
      
      if (success) {
        // 从订阅列表中移除
        subscriptions.value = subscriptions.value.filter(s => s.id !== subscriptionId)
        
        // 从活跃订阅中移除
        activeSubscriptions.value = activeSubscriptions.value.filter(s => s.id !== subscriptionId)
        
        if (currentSubscription.value?.id === subscriptionId) {
          currentSubscription.value = null
        }
      }
      
      return success
    } catch (err) {
      bulkError.value = err instanceof Error ? err.message : '取消通知订阅失败'
      throw err
    } finally {
      bulkLoading.value = false
    }
  }
  
  // ==================== 实时通信支持 ====================
  
  /**
   * 连接WebSocket
   * @param userId 用户ID
   */
  async function connectWebSocket(userId: string) {
    try {
      if (websocketConnection.value && websocketConnection.value.readyState === WebSocket.OPEN) {
        return
      }
      
      // 获取实时连接令牌
      const token = await notificationService.getRealtimeToken()
      
      // 创建WebSocket连接
      const wsUrl = `${import.meta.env.VITE_WS_URL}/notifications?token=${token}&userId=${userId}`
      websocketConnection.value = new WebSocket(wsUrl)
      
      websocketConnection.value.onopen = () => {
        isRealtimeConnected.value = true
        console.log('通知WebSocket连接已建立')
      }
      
      websocketConnection.value.onmessage = (event) => {
        try {
          const update: NotificationRealtimeUpdate = JSON.parse(event.data)
          handleNotificationRealtimeUpdate(update)
        } catch (err) {
          console.error('处理通知实时更新失败:', err)
        }
      }
      
      websocketConnection.value.onclose = () => {
        isRealtimeConnected.value = false
        console.log('通知WebSocket连接已关闭')
        
        // 5秒后重连
        setTimeout(() => connectWebSocket(userId), 5000)
      }
      
      websocketConnection.value.onerror = (err) => {
        console.error('通知WebSocket连接错误:', err)
        isRealtimeConnected.value = false
      }
    } catch (err) {
      console.error('连接通知WebSocket失败:', err)
      isRealtimeConnected.value = false
    }
  }
  
  /**
   * 断开WebSocket连接
   */
  function disconnectWebSocket() {
    if (websocketConnection.value) {
      websocketConnection.value.close()
      websocketConnection.value = null
      isRealtimeConnected.value = false
    }
  }
  
  /**
   * 处理通知实时更新
   * @param update 实时更新数据
   */
  function handleNotificationRealtimeUpdate(update: NotificationRealtimeUpdate) {
    switch (update.updateType) {
      case 'created':
        handleNotificationCreated(update.notification!)
        break
      case 'updated':
        handleNotificationUpdated(update.notification!)
        break
      case 'deleted':
        handleNotificationDeleted(update.notification?.id!)
        break
      case 'read':
        handleNotificationRead(update.notification?.id!)
        break
      case 'delivered':
        handleNotificationDelivered(update.notification?.id!)
        break
      case 'bulk_operation':
        handleBulkOperation(update.bulkResult!)
        break
      case 'settings_updated':
        handleSettingsUpdated(update.settingsUpdate!)
        break
      case 'stats_updated':
        handleStatsUpdated(update.statsUpdate!)
        break
    }
    
    // 添加到实时事件列表
    const event: NotificationEvent = {
      type: update.updateType as NotificationEventType,
      eventId: `event_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      notificationId: update.notification?.id,
      userId: update.targetUserIds?.[0] || '',
      data: update,
      timestamp: update.updatedAt,
      source: 'system',
      version: '1.0.0',
      relatedData: {
        entityType: update.notification?.relatedEntityType,
        entityId: update.notification?.relatedEntityId,
        action: update.notification?.action
      }
    }
    
    realtimeEvents.value.push(event)
    
    // 保持事件列表在合理大小
    if (realtimeEvents.value.length > 100) {
      realtimeEvents.value = realtimeEvents.value.slice(-50)
    }
  }
  
  /**
   * 处理通知创建
   * @param notification 新通知
   */
  function handleNotificationCreated(notification: NotificationResponse) {
    // 添加到通知列表
    const existingIndex = notifications.value.findIndex(n => n.id === notification.id)
    if (existingIndex === -1) {
      notifications.value.unshift(notification)
    }
    
    // 更新未读计数
    if (!notification.isRead) {
      unreadCount.value++
      if (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT) {
        highPriorityUnreadCount.value++
      }
    }
    
    // 更新统计信息
    if (stats.value) {
      stats.value.totalNotifications++
      if (!notification.isRead) {
        stats.value.unreadNotifications++
      }
    }
    
    // 缓存通知
    setCache(notification.id, notification)
  }
  
  /**
   * 处理通知更新
   * @param notification 更新的通知
   */
  function handleNotificationUpdated(notification: NotificationResponse) {
    const index = notifications.value.findIndex(n => n.id === notification.id)
    if (index !== -1) {
      const oldNotification = notifications.value[index]
      notifications.value[index] = notification
      
      // 更新未读计数
      if (oldNotification.isRead !== notification.isRead) {
        if (notification.isRead) {
          unreadCount.value = Math.max(0, unreadCount.value - 1)
          if (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT) {
            highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 1)
          }
        } else {
          unreadCount.value++
          if (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT) {
            highPriorityUnreadCount.value++
          }
        }
      }
    }
    
    if (currentNotification.value?.id === notification.id) {
      currentNotification.value = notification
    }
    
    // 更新缓存
    setCache(notification.id, notification)
  }
  
  /**
   * 处理通知删除
   * @param notificationId 通知ID
   */
  function handleNotificationDeleted(notificationId: string) {
    // 从通知列表中移除
    const deletedNotification = notifications.value.find(n => n.id === notificationId)
    notifications.value = notifications.value.filter(n => n.id !== notificationId)
    
    if (currentNotification.value?.id === notificationId) {
      currentNotification.value = null
    }
    
    // 更新未读计数
    if (deletedNotification && !deletedNotification.isRead) {
      unreadCount.value = Math.max(0, unreadCount.value - 1)
      if (deletedNotification.priority === NotificationPriority.HIGH || deletedNotification.priority === NotificationPriority.URGENT) {
        highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 1)
      }
    }
    
    // 更新统计信息
    if (stats.value) {
      stats.value.totalNotifications--
      if (deletedNotification && !deletedNotification.isRead) {
        stats.value.unreadNotifications--
      }
    }
    
    // 清除缓存
    clearCache(notificationId)
  }
  
  /**
   * 处理通知已读
   * @param notificationId 通知ID
   */
  function handleNotificationRead(notificationId: string) {
    const notification = notifications.value.find(n => n.id === notificationId)
    if (notification && !notification.isRead) {
      notification.isRead = true
      notification.readAt = new Date().toISOString()
      
      // 更新未读计数
      unreadCount.value = Math.max(0, unreadCount.value - 1)
      if (notification.priority === NotificationPriority.HIGH || notification.priority === NotificationPriority.URGENT) {
        highPriorityUnreadCount.value = Math.max(0, highPriorityUnreadCount.value - 1)
      }
    }
    
    if (currentNotification.value?.id === notificationId) {
      currentNotification.value.isRead = true
      currentNotification.value.readAt = new Date().toISOString()
    }
    
    // 更新缓存
    const cached = getCache(notificationId)
    if (cached) {
      cached.isRead = true
      cached.readAt = new Date().toISOString()
      setCache(notificationId, cached)
    }
  }
  
  /**
   * 处理通知已送达
   * @param notificationId 通知ID
   */
  function handleNotificationDelivered(notificationId: string) {
    const notification = notifications.value.find(n => n.id === notificationId)
    if (notification) {
      notification.deliveredAt = new Date().toISOString()
      
      // 更新缓存
      const cached = getCache(notificationId)
      if (cached) {
        cached.deliveredAt = new Date().toISOString()
        setCache(notificationId, cached)
      }
    }
  }
  
  /**
   * 处理批量操作
   * @param bulkResult 批量操作结果
   */
  function handleBulkOperation(bulkResult: BulkNotificationResult) {
    bulkResults.value.push(bulkResult)
    
    // 保持结果列表在合理大小
    if (bulkResults.value.length > 50) {
      bulkResults.value = bulkResults.value.slice(-25)
    }
    
    // 根据操作类型更新本地状态
    // 这里可以根据具体的操作类型来实现相应的状态更新
  }
  
  /**
   * 处理设置更新
   * @param settingsUpdate 设置更新数据
   */
  function handleSettingsUpdated(settingsUpdate: Partial<NotificationSettings>) {
    if (globalSettings.value) {
      globalSettings.value = { ...globalSettings.value, ...settingsUpdate }
    }
  }
  
  /**
   * 处理统计更新
   * @param statsUpdate 统计更新数据
   */
  function handleStatsUpdated(statsUpdate: Partial<NotificationStats>) {
    if (stats.value) {
      stats.value = { ...stats.value, ...statsUpdate }
      
      // 更新未读计数
      if (statsUpdate.unreadNotifications !== undefined) {
        unreadCount.value = statsUpdate.unreadNotifications
      }
      if (statsUpdate.highPriorityNotifications !== undefined) {
        highPriorityUnreadCount.value = statsUpdate.highPriorityNotifications
      }
    }
  }
  
  // ==================== 自动刷新管理 ====================
  
  /**
   * 启动自动刷新
   */
  function startAutoRefresh() {
    if (autoRefreshTimer) {
      clearInterval(autoRefreshTimer)
    }
    
    isAutoRefreshing.value = true
    autoRefreshTimer = setInterval(async () => {
      try {
        await fetchNotifications()
      } catch (err) {
        console.error('自动刷新通知失败:', err)
      }
    }, autoRefreshInterval.value)
  }
  
  /**
   * 停止自动刷新
   */
  function stopAutoRefresh() {
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
  function setAutoRefreshInterval(interval: number) {
    autoRefreshInterval.value = interval
    
    if (isAutoRefreshing.value) {
      stopAutoRefresh()
      startAutoRefresh()
    }
  }
  
  // ==================== 持久化存储支持 ====================
  
  /**
   * 从本地存储加载数据
   */
  function loadFromStorage() {
    try {
      const saved = localStorage.getItem('notification_store_state')
      if (saved) {
        const state = JSON.parse(saved)
        
        // 恢复基本状态
        if (state.filter) {
          Object.assign(filter, state.filter)
        }
        if (state.unreadCount !== undefined) {
          unreadCount.value = state.unreadCount
        }
        if (state.highPriorityUnreadCount !== undefined) {
          highPriorityUnreadCount.value = state.highPriorityUnreadCount
        }
        if (state.doNotDisturbSettings) {
          doNotDisturbSettings.value = state.doNotDisturbSettings
        }
        if (state.autoRefreshInterval !== undefined) {
          autoRefreshInterval.value = state.autoRefreshInterval
        }
      }
    } catch (err) {
      console.error('从本地存储加载通知状态失败:', err)
    }
  }
  
  /**
   * 保存数据到本地存储
   */
  function saveToStorage() {
    try {
      const state = {
        filter,
        unreadCount: unreadCount.value,
        highPriorityUnreadCount: highPriorityUnreadCount.value,
        doNotDisturbSettings: doNotDisturbSettings.value,
        autoRefreshInterval: autoRefreshInterval.value
      }
      localStorage.setItem('notification_store_state', JSON.stringify(state))
    } catch (err) {
      console.error('保存通知状态到本地存储失败:', err)
    }
  }
  
  // ==================== 监听器 ====================
  
  // 监听筛选条件变化
  watch(filter, () => {
    fetchNotifications()
  }, { deep: true })
  
  // 监听未读数量变化，更新页面标题
  watch(unreadCount, (newCount) => {
    if (newCount > 0) {
      document.title = `(${newCount}) ${document.title.replace(/^\(\d+\)\s*/, '')}`
    } else {
      document.title = document.title.replace(/^\(\d+\)\s*/, '')
    }
  })
  
  // 监听状态变化，自动保存到本地存储
  watch([filter, doNotDisturbSettings, autoRefreshInterval], () => {
    saveToStorage()
  }, { deep: true })
  
  // ==================== 状态重置 ====================
  
  /**
   * 重置所有状态
   */
  function $reset() {
    // 停止自动刷新
    stopAutoRefresh()
    
    // 断开WebSocket连接
    disconnectWebSocket()
    
    // 重置核心状态
    notifications.value = []
    currentNotification.value = null
    notificationStats.value = null
    notificationAnalytics.value = null
    loading.value = false
    error.value = null
    initialized.value = false
    lastUpdated.value = null
    
    // 重置筛选和分页
    Object.assign(filter, DEFAULT_FILTER)
    pagination.value = {
      totalCount: 0,
      currentPage: 1,
      pageSize: 20,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false
    }
    
    // 重置计数
    unreadCount.value = 0
    highPriorityUnreadCount.value = 0
    
    // 重置设置
    settings.value = []
    currentSetting.value = null
    globalSettings.value = null
    preferences.value = []
    doNotDisturbSettings.value = {
      enabled: false,
      startTime: '22:00',
      endTime: '08:00',
      daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
      allowUrgent: true
    }
    
    // 重置模板
    templates.value = []
    currentTemplate.value = null
    templateVariables.value = []
    
    // 重置订阅
    subscriptions.value = []
    currentSubscription.value = null
    activeSubscriptions.value = []
    
    // 重置搜索和筛选
    searchResults.value = []
    searchLoading.value = false
    Object.assign(searchFilter, {
      keyword: '',
      searchScope: 'ALL' as any,
      searchFields: ['title', 'content', 'message'],
      filter: { ...DEFAULT_FILTER },
      sortBy: { field: 'createdAt', order: 'desc' },
      pagination: { page: 1, pageSize: 20 },
      highlightMatches: true,
      fuzzySearch: false
    })
    
    // 重置统计
    stats.value = null
    summary.value = null
    channelStats.value = {}
    typeStats.value = {}
    priorityStats.value = {}
    
    // 重置批量操作
    bulkOperations.value = []
    bulkResults.value = []
    selectedNotifications.value.clear()
    bulkLoading.value = false
    bulkError.value = null
    
    // 重置实时通信
    realtimeEvents.value = []
    realtimeStatus.value = null
    
    // 重置缓存
    cache.value.clear()
    Object.keys(cacheTimestamp.value).forEach(k => {
      delete cacheTimestamp.value[k]
    })
    
    // 清除本地存储
    localStorage.removeItem('notification_store_state')
  }
  
  // ==================== 初始化 ====================
  
  /**
   * 初始化Store
   * @param userId 用户ID
   */
  async function initialize(userId: string) {
    try {
      // 从本地存储加载数据
      loadFromStorage()
      
      // 获取通知列表
      await fetchNotifications({ userId })
      
      // 获取通知设置
      await fetchSettings(userId)
      
      // 获取统计信息
      await fetchStats(userId)
      
      // 获取通知模板
      await fetchTemplates()
      
      // 获取通知订阅
      await fetchSubscriptions(userId)
      
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
  
  // ==================== 返回值 ====================
  
  return {
    // 核心状态
    notifications,
    currentNotification,
    notificationStats,
    notificationAnalytics,
    loading,
    error,
    initialized,
    lastUpdated,
    filter,
    pagination,
    unreadCount,
    highPriorityUnreadCount,
    
    // 通知设置状态
    settings,
    currentSetting,
    globalSettings,
    preferences,
    doNotDisturbSettings,
    
    // 通知模板状态
    templates,
    currentTemplate,
    templateVariables,
    
    // 通知订阅状态
    subscriptions,
    currentSubscription,
    activeSubscriptions,
    
    // 搜索和筛选状态
    searchResults,
    searchLoading,
    searchFilter,
    typeFilter,
    
    // 统计信息状态
    stats,
    summary,
    channelStats,
    typeStats,
    priorityStats,
    
    // 批量操作状态
    bulkOperations,
    bulkResults,
    selectedNotifications,
    bulkLoading,
    bulkError,
    
    // 实时通信状态
    websocketConnection,
    isRealtimeConnected,
    realtimeEvents,
    realtimeStatus,
    
    // 缓存和性能状态
    cache,
    isAutoRefreshing,
    autoRefreshInterval,
    
    // 计算属性
    isLoading,
    hasError,
    isInitialized,
    hasNotifications,
    hasMoreNotifications,
    unreadNotifications,
    readNotifications,
    archivedNotifications,
    deletedNotifications,
    highPriorityNotifications,
    notificationsByType,
    notificationsByStatus,
    notificationsByPriority,
    notificationsByChannel,
    hasActiveCache,
    hasSelectedNotifications,
    selectedNotificationsList,
    recentRealtimeEvents,
    
    // 通知CRUD操作
    fetchNotifications,
    fetchMoreNotifications,
    fetchNotification,
    createNotification,
    updateNotification,
    deleteNotification,
    
    // 通知状态管理
    markAsRead,
    markAsUnread,
    markAllAsRead,
    archiveNotifications,
    unarchiveNotifications,
    confirmNotification,
    
    // 通知设置管理
    fetchSettings,
    createSetting,
    updateSetting,
    deleteSetting,
    resetSettings,
    
    // 通知统计功能
    fetchStats,
    fetchSummary,
    fetchAnalytics,
    
    // 通知筛选和搜索
    searchNotifications,
    updateFilter,
    resetFilter,
    updateSearchFilter,
    resetSearchFilter,
    
    // 通知批量操作
    batchDeleteNotifications,
    selectNotification,
    selectAllNotifications,
    clearSelection,
    
    // 通知模板管理
    fetchTemplates,
    fetchTemplate,
    createTemplate,
    updateTemplate,
    deleteTemplate,
    createNotificationFromTemplate,
    
    // 通知订阅管理
    fetchSubscriptions,
    createSubscription,
    updateSubscription,
    pauseSubscription,
    resumeSubscription,
    cancelSubscription,
    
    // 实时通信支持
    connectWebSocket,
    disconnectWebSocket,
    
    // 自动刷新管理
    startAutoRefresh,
    stopAutoRefresh,
    setAutoRefreshInterval,
    
    // 缓存操作
    setCache,
    getCache,
    clearCache,
    
    // 持久化存储
    loadFromStorage,
    saveToStorage,
    
    // 初始化和重置
    initialize,
    $reset
  }
})

// 导出类型
export type NotificationStore = ReturnType<typeof useNotificationStore>