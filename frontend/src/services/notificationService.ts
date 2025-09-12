/**
 * 通知API服务
 * 
 * 本文件提供通知系统的前端API服务实现，包括：
 * - 通知CRUD操作
 * - 通知设置管理
 * - 通知批量操作
 * - 通知统计和分析
 * - 通知导入导出
 * - 通知实时通信
 * - 通知模板管理
 * - 通知过滤规则
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import api from './api'
import { webSocketService } from './websocketService'
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
  
  // 保留原有的类型以确保兼容性
  NotificationSetting,
  CreateNotificationSettingRequest,
  UpdateNotificationSettingRequest,
  NotificationSummary,
  TestNotificationRequest,
  NotificationExportOptions,
  NotificationImportData,
  ImportNotification,
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
  PaginatedNotificationFilterRules,
  NotificationSort,
  EmailNotificationFrequency,
  RelatedEntityType,
  NotificationAction,
  DeliveryStatus,
  NOTIFICATION_DEFAULTS
} from '@/types/notification'

import type { WebSocketClient, WebSocketMessage } from '@/types/websocket'

/**
 * 通知服务类 - 封装所有通知相关的API调用
 */
export class NotificationService {
  private readonly basePath = '/api/notifications'
  private readonly settingsPath = '/api/notification-settings'
  private readonly templatesPath = '/api/notification-templates'
  private readonly subscriptionsPath = '/api/notification-subscriptions'
  private readonly statsPath = '/api/notification-stats'
  private readonly searchPath = '/api/notifications/search'
  private readonly bulkPath = '/api/notifications/bulk'
  private readonly realtimePath = '/api/notifications/realtime'
  private readonly exportPath = '/api/notifications/export'
  private readonly importPath = '/api/notifications/import'
  private readonly filterRulesPath = '/api/notification-filter-rules'
  private readonly deliveryHistoryPath = '/api/notification-delivery-history'
  private readonly queuePath = '/api/notification-queue'
  private readonly analyticsPath = '/api/notification-analytics'
  
  // WebSocket客户端实例
  private wsClient: WebSocketClient | null = null
  // 实时事件订阅映射
  private realtimeSubscriptions: Map<string, Set<(event: NotificationEvent) => void>> = new Map()
  // 缓存存储
  private cache: Map<string, { data: any; timestamp: number; ttl: number }> = new Map()
  // 网络状态
  private isOnline = navigator.onLine
  // 重试队列
  private retryQueue: Map<string, { operation: () => Promise<any>; retryCount: number; maxRetries: number }> = new Map()
  // API限流状态
  private rateLimitStatus: Map<string, { resetTime: number; remaining: number }> = new Map()

  constructor() {
    this.initializeWebSocket()
    this.initializeNetworkMonitoring()
    this.startRetryQueueProcessor()
    this.startCacheCleanup()
  }

  // ==================== 通知CRUD操作 ====================

  /**
   * 获取通知列表
   * @param filter 通知筛选条件
   * @returns 通知分页结果
   */
  async getNotifications(filter: NotificationFilter): Promise<PaginatedNotifications> {
    const response = await api.get<PaginatedNotifications>(this.basePath, {
      params: filter
    })
    return response.data
  }

  /**
   * 获取指定通知的详细信息
   * @param id 通知ID
   * @returns 通知详细信息
   */
  async getNotificationById(id: string): Promise<Notification> {
    const response = await api.get<Notification>(`${this.basePath}/${id}`)
    return response.data
  }

  /**
   * 创建新通知
   * @param notification 通知创建请求
   * @returns 创建的通知信息
   */
  async createNotification(notification: CreateNotificationRequest): Promise<Notification> {
    const response = await api.post<Notification>(this.basePath, notification)
    return response.data
  }

  /**
   * 更新通知内容
   * @param id 通知ID
   * @param notification 通知更新请求
   * @returns 更新后的通知信息
   */
  async updateNotification(id: string, notification: UpdateNotificationRequest): Promise<Notification> {
    const response = await api.put<Notification>(`${this.basePath}/${id}`, notification)
    return response.data
  }

  /**
   * 删除通知
   * @param id 通知ID
   */
  async deleteNotification(id: string): Promise<void> {
    await api.delete(`${this.basePath}/${id}`)
  }

  /**
   * 标记通知为已读
   * @param id 通知ID
   */
  async markAsRead(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/read`)
  }

  /**
   * 标记通知为未读
   * @param id 通知ID
   */
  async markAsUnread(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/unread`)
  }

  /**
   * 确认通知
   * @param id 通知ID
   */
  async confirmNotification(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/confirm`)
  }

  /**
   * 归档通知
   * @param id 通知ID
   */
  async archiveNotification(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/archive`)
  }

  /**
   * 取消归档通知
   * @param id 通知ID
   */
  async unarchiveNotification(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/unarchive`)
  }

  /**
   * 获取通知权限
   * @param id 通知ID
   * @returns 通知权限信息
   */
  async getNotificationPermissions(id: string): Promise<NotificationPermissions> {
    const response = await api.get<NotificationPermissions>(`${this.basePath}/${id}/permissions`)
    return response.data
  }

  /**
   * 获取通知发送历史
   * @param id 通知ID
   * @returns 通知发送历史
   */
  async getNotificationDeliveryHistory(id: string): Promise<NotificationDeliveryHistory[]> {
    const response = await api.get<NotificationDeliveryHistory[]>(`${this.basePath}/${id}/delivery-history`)
    return response.data
  }

  // ==================== 批量操作 ====================

  /**
   * 批量标记已读
   * @param request 批量标记已读请求
   */
  async batchMarkAsRead(request: BatchMarkAsReadRequest): Promise<void> {
    await api.post(`${this.basePath}/batch/mark-read`, request)
  }

  /**
   * 批量删除通知
   * @param request 批量删除通知请求
   */
  async batchDeleteNotifications(request: BatchDeleteNotificationsRequest): Promise<void> {
    await api.post(`${this.basePath}/batch/delete`, request)
  }

  /**
   * 批量归档通知
   * @param notificationIds 通知ID数组
   */
  async batchArchiveNotifications(notificationIds: string[]): Promise<void> {
    await api.post(`${this.basePath}/batch/archive`, { notificationIds })
  }

  /**
   * 批量取消归档通知
   * @param notificationIds 通知ID数组
   */
  async batchUnarchiveNotifications(notificationIds: string[]): Promise<void> {
    await api.post(`${this.basePath}/batch/unarchive`, { notificationIds })
  }

  // ==================== 通知设置管理 ====================

  /**
   * 获取通知设置列表
   * @param userId 用户ID
   * @returns 通知设置列表
   */
  async getNotificationSettings(userId: string): Promise<NotificationSetting[]> {
    const response = await api.get<NotificationSetting[]>(this.settingsPath, {
      params: { userId }
    })
    return response.data
  }

  /**
   * 获取用户的通知设置
   * @param userId 用户ID
   * @param notificationType 通知类型（可选）
   * @returns 通知设置
   */
  async getUserNotificationSetting(userId: string, notificationType?: NotificationType): Promise<NotificationSetting> {
    const params = notificationType ? { userId, notificationType } : { userId }
    const response = await api.get<NotificationSetting>(`${this.settingsPath}/user`, {
      params
    })
    return response.data
  }

  /**
   * 创建通知设置
   * @param setting 通知设置创建请求
   * @returns 创建的通知设置
   */
  async createNotificationSetting(setting: CreateNotificationSettingRequest): Promise<NotificationSetting> {
    const response = await api.post<NotificationSetting>(this.settingsPath, setting)
    return response.data
  }

  /**
   * 更新通知设置
   * @param id 通知设置ID
   * @param setting 通知设置更新请求
   * @returns 更新后的通知设置
   */
  async updateNotificationSetting(id: string, setting: UpdateNotificationSettingRequest): Promise<NotificationSetting> {
    const response = await api.put<NotificationSetting>(`${this.settingsPath}/${id}`, setting)
    return response.data
  }

  /**
   * 删除通知设置
   * @param id 通知设置ID
   */
  async deleteNotificationSetting(id: string): Promise<void> {
    await api.delete(`${this.settingsPath}/${id}`)
  }

  /**
   * 重置通知设置为默认值
   * @param userId 用户ID
   * @returns 重置后的通知设置
   */
  async resetNotificationSettings(userId: string): Promise<NotificationSetting> {
    const response = await api.post<NotificationSetting>(`${this.settingsPath}/reset`, { userId })
    return response.data
  }

  // ==================== 通知统计和分析 ====================

  /**
   * 获取通知统计信息
   * @param userId 用户ID
   * @returns 通知统计信息
   */
  async getNotificationStats(userId: string): Promise<NotificationStats> {
    const response = await api.get<NotificationStats>(this.statsPath, {
      params: { userId }
    })
    return response.data
  }

  /**
   * 获取通知摘要
   * @param userId 用户ID
   * @returns 通知摘要
   */
  async getNotificationSummary(userId: string): Promise<NotificationSummary> {
    const response = await api.get<NotificationSummary>(`${this.statsPath}/summary`, {
      params: { userId }
    })
    return response.data
  }

  /**
   * 获取通知分析数据
   * @param filters 分析筛选条件
   * @returns 通知分析数据
   */
  async getNotificationAnalytics(filters?: any): Promise<NotificationAnalytics> {
    const response = await api.get<NotificationAnalytics>(this.analyticsPath, {
      params: filters
    })
    return response.data
  }

  // ==================== 通知导入导出 ====================

  /**
   * 导出通知
   * @param options 导出选项
   * @returns 导出的文件Blob
   */
  async exportNotifications(options: NotificationExportOptions): Promise<Blob> {
    const response = await api.post(this.exportPath, options, {
      responseType: 'blob'
    })
    return response.data
  }

  /**
   * 导入通知
   * @param data 导入数据
   * @returns 导入结果
   */
  async importNotifications(data: NotificationImportData): Promise<any> {
    const response = await api.post(this.importPath, data)
    return response.data
  }

  /**
   * 验证导入数据
   * @param data 导入数据
   * @returns 验证结果
   */
  async validateNotificationImport(data: any): Promise<any> {
    const response = await api.post(`${this.importPath}/validate`, data)
    return response.data
  }

  /**
   * 获取导入模板
   * @param format 导入格式
   * @returns 模板文件Blob
   */
  async getImportTemplate(format: 'json' | 'csv' = 'json'): Promise<Blob> {
    const response = await api.get(`${this.importPath}/template/${format}`, {
      responseType: 'blob'
    })
    return response.data
  }

  // ==================== 通知模板管理 ====================

  /**
   * 获取通知模板列表
   * @returns 通知模板列表
   */
  async getNotificationTemplates(): Promise<NotificationTemplate[]> {
    const response = await api.get<NotificationTemplate[]>(this.templatesPath)
    return response.data
  }

  /**
   * 获取指定通知模板
   * @param id 模板ID
   * @returns 通知模板
   */
  async getNotificationTemplate(id: string): Promise<NotificationTemplate> {
    const response = await api.get<NotificationTemplate>(`${this.templatesPath}/${id}`)
    return response.data
  }

  /**
   * 创建通知模板
   * @param template 通知模板
   * @returns 创建的模板
   */
  async createNotificationTemplate(template: Omit<NotificationTemplate, 'id' | 'createdAt' | 'updatedAt'>): Promise<NotificationTemplate> {
    const response = await api.post<NotificationTemplate>(this.templatesPath, template)
    return response.data
  }

  /**
   * 更新通知模板
   * @param id 模板ID
   * @param template 通知模板
   * @returns 更新后的模板
   */
  async updateNotificationTemplate(id: string, template: Partial<NotificationTemplate>): Promise<NotificationTemplate> {
    const response = await api.put<NotificationTemplate>(`${this.templatesPath}/${id}`, template)
    return response.data
  }

  /**
   * 删除通知模板
   * @param id 模板ID
   */
  async deleteNotificationTemplate(id: string): Promise<void> {
    await api.delete(`${this.templatesPath}/${id}`)
  }

  /**
   * 使用模板创建通知
   * @param templateId 模板ID
   * @param variables 模板变量
   * @param userId 接收者ID
   * @returns 创建的通知
   */
  async createNotificationFromTemplate(
    templateId: string,
    variables: Record<string, any>,
    userId: string
  ): Promise<Notification> {
    const response = await api.post<Notification>(`${this.templatesPath}/${templateId}/use`, {
      variables,
      userId
    })
    return response.data
  }

  // ==================== 通知过滤规则 ====================

  /**
   * 获取通知过滤规则列表
   * @returns 通知过滤规则列表
   */
  async getNotificationFilterRules(): Promise<NotificationFilterRule[]> {
    const response = await api.get<NotificationFilterRule[]>(this.filterRulesPath)
    return response.data
  }

  /**
   * 创建通知过滤规则
   * @param rule 过滤规则
   * @returns 创建的过滤规则
   */
  async createNotificationFilterRule(rule: Omit<NotificationFilterRule, 'id' | 'createdAt' | 'updatedAt'>): Promise<NotificationFilterRule> {
    const response = await api.post<NotificationFilterRule>(this.filterRulesPath, rule)
    return response.data
  }

  /**
   * 更新通知过滤规则
   * @param id 规则ID
   * @param rule 过滤规则
   * @returns 更新后的过滤规则
   */
  async updateNotificationFilterRule(id: string, rule: Partial<NotificationFilterRule>): Promise<NotificationFilterRule> {
    const response = await api.put<NotificationFilterRule>(`${this.filterRulesPath}/${id}`, rule)
    return response.data
  }

  /**
   * 删除通知过滤规则
   * @param id 规则ID
   */
  async deleteNotificationFilterRule(id: string): Promise<void> {
    await api.delete(`${this.filterRulesPath}/${id}`)
  }

  /**
   * 应用过滤规则
   * @param ruleId 规则ID
   * @param notifications 通知列表
   * @returns 处理结果
   */
  async applyNotificationFilterRule(ruleId: string, notifications: Notification[]): Promise<any> {
    const response = await api.post(`${this.filterRulesPath}/${ruleId}/apply`, {
      notifications: notifications.map(n => n.id)
    })
    return response.data
  }

  // ==================== 通知队列管理 ====================

  /**
   * 获取通知队列任务列表
   * @returns 队列任务列表
   */
  async getNotificationQueueTasks(): Promise<NotificationQueueTask[]> {
    const response = await api.get<NotificationQueueTask[]>(this.queuePath)
    return response.data
  }

  /**
   * 获取指定队列任务
   * @param taskId 任务ID
   * @returns 队列任务
   */
  async getNotificationQueueTask(taskId: string): Promise<NotificationQueueTask> {
    const response = await api.get<NotificationQueueTask>(`${this.queuePath}/${taskId}`)
    return response.data
  }

  /**
   * 重试队列任务
   * @param taskId 任务ID
   */
  async retryNotificationQueueTask(taskId: string): Promise<void> {
    await api.post(`${this.queuePath}/${taskId}/retry`)
  }

  /**
   * 取消队列任务
   * @param taskId 任务ID
   */
  async cancelNotificationQueueTask(taskId: string): Promise<void> {
    await api.post(`${this.queuePath}/${taskId}/cancel`)
  }

  // ==================== 发送历史管理 ====================

  /**
   * 获取通知发送历史
   * @param filter 发送历史筛选条件
   * @returns 发送历史分页结果
   */
  async getNotificationDeliveryHistoryList(filter: any): Promise<PaginatedNotificationDeliveryHistory> {
    const response = await api.get<PaginatedNotificationDeliveryHistory>(this.deliveryHistoryPath, {
      params: filter
    })
    return response.data
  }

  /**
   * 重新发送通知
   * @param notificationId 通知ID
   * @param channels 发送渠道
   */
  async resendNotification(notificationId: string, channels: NotificationChannel[]): Promise<void> {
    await api.post(`${this.basePath}/${notificationId}/resend`, { channels })
  }

  // ==================== 实时通信相关 ====================

  /**
   * 获取实时连接令牌
   * @returns 连接令牌
   */
  async getRealtimeToken(): Promise<string> {
    const response = await api.get<{ token: string }>(`${this.realtimePath}/token`)
    return response.data.token
  }

  /**
   * 订阅通知实时更新
   * @param userId 用户ID
   * @returns 订阅结果
   */
  async subscribeToNotifications(userId: string): Promise<{ success: boolean; subscriptionId: string }> {
    const response = await api.post<{ success: boolean; subscriptionId: string }>(
      `${this.realtimePath}/subscribe`,
      { userId }
    )
    return response.data
  }

  /**
   * 取消订阅通知实时更新
   * @param subscriptionId 订阅ID
   */
  async unsubscribeFromNotifications(subscriptionId: string): Promise<void> {
    await api.post(`${this.realtimePath}/unsubscribe`, { subscriptionId })
  }

  /**
   * 发送测试通知
   * @param request 测试通知请求
   * @returns 发送结果
   */
  async sendTestNotification(request: TestNotificationRequest): Promise<any> {
    const response = await api.post(`${this.basePath}/test`, request)
    return response.data
  }

  /**
   * 获取实时状态
   * @param userId 用户ID
   * @returns 实时状态
   */
  async getRealtimeStatus(userId: string): Promise<NotificationRealtimeStatus> {
    const response = await api.get<NotificationRealtimeStatus>(`${this.realtimePath}/status`, {
      params: { userId }
    })
    return response.data
  }

  // ==================== WebSocket连接管理 ====================

  private wsConnection: WebSocket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectInterval = 5000
  private heartbeatInterval: NodeJS.Timeout | null = null
  private eventListeners: Map<string, Function[]> = new Map()

  /**
   * 连接WebSocket
   * @param userId 用户ID
   * @param token 认证令牌
   */
  async connectWebSocket(userId: string, token?: string): Promise<void> {
    try {
      // 获取WebSocket连接令牌
      const wsToken = token || await this.getRealtimeToken()
      
      // 构建WebSocket连接URL
      const wsUrl = `ws://localhost:6676/ws/notifications?token=${wsToken}&userId=${userId}`
      
      this.wsConnection = new WebSocket(wsUrl)
      
      this.wsConnection.onopen = () => {
        console.log('WebSocket连接已建立')
        this.reconnectAttempts = 0
        this.startHeartbeat()
        this.emit('connected', { userId, connectedAt: new Date().toISOString() })
      }

      this.wsConnection.onmessage = (event) => {
        try {
          const message: NotificationWebSocketMessage = JSON.parse(event.data)
          this.handleWebSocketMessage(message)
        } catch (error) {
          console.error('解析WebSocket消息失败:', error)
        }
      }

      this.wsConnection.onclose = (event) => {
        console.log('WebSocket连接已关闭', event.code, event.reason)
        this.stopHeartbeat()
        this.emit('disconnected', { userId, disconnectedAt: new Date().toISOString() })
        
        // 自动重连
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
          setTimeout(() => {
            this.reconnectAttempts++
            this.connectWebSocket(userId, wsToken)
          }, this.reconnectInterval)
        }
      }

      this.wsConnection.onerror = (error) => {
        console.error('WebSocket连接错误:', error)
        this.emit('error', { userId, error: error.message || 'WebSocket连接错误' })
      }

    } catch (error) {
      console.error('连接WebSocket失败:', error)
      this.emit('error', { userId, error: '连接WebSocket失败' })
    }
  }

  /**
   * 断开WebSocket连接
   */
  disconnectWebSocket(): void {
    if (this.wsConnection) {
      this.wsConnection.close()
      this.wsConnection = null
    }
    this.stopHeartbeat()
    this.reconnectAttempts = this.maxReconnectAttempts // 阻止自动重连
  }

  /**
   * 发送WebSocket消息
   * @param message 消息内容
   */
  sendWebSocketMessage(message: any): void {
    if (this.wsConnection && this.wsConnection.readyState === WebSocket.OPEN) {
      this.wsConnection.send(JSON.stringify(message))
    } else {
      console.error('WebSocket未连接，无法发送消息')
    }
  }

  /**
   * 处理WebSocket消息
   * @param message WebSocket消息
   */
  private handleWebSocketMessage(message: NotificationWebSocketMessage): void {
    switch (message.type) {
      case 'notification':
        const notificationEvent = message.content as NotificationWebSocketEvent
        this.emit('notification', notificationEvent)
        break
      case 'heartbeat':
        this.emit('heartbeat', { timestamp: message.sentAt })
        break
      case 'connection_status':
        this.emit('connection_status', message.content)
        break
      default:
        console.warn('未知的WebSocket消息类型:', message.type)
    }
  }

  /**
   * 开始心跳检测
   */
  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      if (this.wsConnection && this.wsConnection.readyState === WebSocket.OPEN) {
        this.sendWebSocketMessage({
          type: 'heartbeat',
          timestamp: new Date().toISOString()
        })
      }
    }, 30000) // 每30秒发送一次心跳
  }

  /**
   * 停止心跳检测
   */
  private stopHeartbeat(): void {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval)
      this.heartbeatInterval = null
    }
  }

  /**
   * 添加事件监听器
   * @param event 事件名称
   * @param listener 监听器函数
   */
  on(event: string, listener: Function): void {
    if (!this.eventListeners.has(event)) {
      this.eventListeners.set(event, [])
    }
    this.eventListeners.get(event)!.push(listener)
  }

  /**
   * 移除事件监听器
   * @param event 事件名称
   * @param listener 监听器函数
   */
  off(event: string, listener: Function): void {
    const listeners = this.eventListeners.get(event)
    if (listeners) {
      const index = listeners.indexOf(listener)
      if (index > -1) {
        listeners.splice(index, 1)
      }
    }
  }

  /**
   * 触发事件
   * @param event 事件名称
   * @param data 事件数据
   */
  private emit(event: string, data: any): void {
    const listeners = this.eventListeners.get(event)
    if (listeners) {
      listeners.forEach(listener => listener(data))
    }
  }

  // ==================== 工具方法 ====================

  /**
   * 格式化通知时间差
   * @param dateString 时间字符串
   * @returns 格式化后的时间差
   */
  formatTimeSinceCreated(dateString: string): string {
    const date = new Date(dateString)
    const now = new Date()
    const diff = now.getTime() - date.getTime()
    
    const minutes = Math.floor(diff / 60000)
    const hours = Math.floor(diff / 3600000)
    const days = Math.floor(diff / 86400000)
    
    if (minutes < 1) return '刚刚'
    if (minutes < 60) return `${minutes}分钟前`
    if (hours < 24) return `${hours}小时前`
    if (days < 30) return `${days}天前`
    
    return date.toLocaleDateString()
  }

  /**
   * 获取通知图标
   * @param type 通知类型
   * @param action 通知操作
   * @returns 通知图标
   */
  getNotificationIcon(type: NotificationType, action?: NotificationAction): string {
    const iconMap: Record<string, string> = {
      [NotificationType.COMMENT]: '💬',
      [NotificationType.REPLY]: '↩️',
      [NotificationType.MESSAGE]: '✉️',
      [NotificationType.SYSTEM]: '⚙️',
      [NotificationType.LIKE]: '❤️',
      [NotificationType.SHARE]: '🔗',
      [NotificationType.FOLLOW]: '👤',
      [NotificationType.MENTION]: '@',
      [NotificationType.TAG]: '🏷️',
      [NotificationType.SECURITY]: '🔒',
      [NotificationType.ACCOUNT]: '👤',
      [NotificationType.UPDATE]: '🔄',
      [NotificationType.MAINTENANCE]: '🔧',
      [NotificationType.ANNOUNCEMENT]: '📢'
    }
    
    const actionIconMap: Record<string, string> = {
      [NotificationAction.CREATED]: '➕',
      [NotificationAction.UPDATED]: '✏️',
      [NotificationAction.DELETED]: '🗑️',
      [NotificationAction.LIKED]: '❤️',
      [NotificationAction.SHARED]: '🔗',
      [NotificationAction.FOLLOWED]: '👤',
      [NotificationAction.MENTIONED]: '@',
      [NotificationAction.COMMENTED]: '💬',
      [NotificationAction.REPLIED]: '↩️',
      [NotificationAction.REPORTED]: '🚨',
      [NotificationAction.MODERATED]: '🛡️'
    }
    
    return action && actionIconMap[action] ? actionIconMap[action] : iconMap[type] || '📢'
  }

  /**
   * 获取通知颜色
   * @param priority 通知优先级
   * @returns 通知颜色
   */
  getNotificationColor(priority: NotificationPriority): string {
    const colorMap: Record<NotificationPriority, string> = {
      [NotificationPriority.LOW]: '#6c757d',
      [NotificationPriority.NORMAL]: '#007bff',
      [NotificationPriority.HIGH]: '#fd7e14',
      [NotificationPriority.URGENT]: '#dc3545',
      [NotificationPriority.CRITICAL]: '#721c24'
    }
    
    return colorMap[priority] || '#007bff'
  }

  /**
   * 验证通知数据
   * @param notification 通知数据
   * @returns 验证结果
   */
  validateNotification(notification: CreateNotificationRequest | UpdateNotificationRequest): NotificationFormErrors {
    const errors: NotificationFormErrors = {}
    
    if (!notification.title || !notification.title.trim()) {
      errors.title = '通知标题不能为空'
    } else if (notification.title.length > NOTIFICATION_DEFAULTS.MAX_TITLE_LENGTH) {
      errors.title = `通知标题不能超过 ${NOTIFICATION_DEFAULTS.MAX_TITLE_LENGTH} 个字符`
    }
    
    if (notification.content && notification.content.length > NOTIFICATION_DEFAULTS.MAX_CONTENT_LENGTH) {
      errors.content = `通知内容不能超过 ${NOTIFICATION_DEFAULTS.MAX_CONTENT_LENGTH} 个字符`
    }
    
    if (notification.message && notification.message.length > NOTIFICATION_DEFAULTS.MAX_MESSAGE_LENGTH) {
      errors.message = `通知消息不能超过 ${NOTIFICATION_DEFAULTS.MAX_MESSAGE_LENGTH} 个字符`
    }
    
    if (!notification.type) {
      errors.type = '请选择通知类型'
    }
    
    if (!notification.priority) {
      errors.priority = '请选择通知优先级'
    }
    
    if (notification.expiresAt && new Date(notification.expiresAt) <= new Date()) {
      errors.expiresAt = '过期时间必须晚于当前时间'
    }
    
    if (notification.scheduledToSendAt && new Date(notification.scheduledToSendAt) <= new Date()) {
      errors.scheduledToSendAt = '计划发送时间必须晚于当前时间'
    }
    
    return errors
  }

  /**
   * 验证通知设置数据
   * @param setting 通知设置数据
   * @returns 验证结果
   */
  validateNotificationSetting(setting: CreateNotificationSettingRequest | UpdateNotificationSettingRequest): NotificationSettingFormErrors {
    const errors: NotificationSettingFormErrors = {}
    
    if (setting.batchIntervalMinutes !== undefined) {
      if (setting.batchIntervalMinutes < NOTIFICATION_DEFAULTS.BATCH_INTERVAL_MIN || 
          setting.batchIntervalMinutes > NOTIFICATION_DEFAULTS.BATCH_INTERVAL_MAX) {
        errors.batchIntervalMinutes = `批量通知间隔必须在 ${NOTIFICATION_DEFAULTS.BATCH_INTERVAL_MIN} 到 ${NOTIFICATION_DEFAULTS.BATCH_INTERVAL_MAX} 分钟之间`
      }
    }
    
    if (setting.language && !NOTIFICATION_DEFAULTS.LANGUAGE_FORMAT.test(setting.language)) {
      errors.language = '语言代码格式不正确'
    }
    
    if (setting.timeZone && !NOTIFICATION_DEFAULTS.TIMEZONE_FORMAT.test(setting.timeZone)) {
      errors.timeZone = '时区格式不正确'
    }
    
    if (setting.quietHoursStart && !NOTIFICATION_DEFAULTS.QUIET_HOURS_FORMAT.test(setting.quietHoursStart)) {
      errors.quietHoursStart = '免打扰开始时间格式不正确，请使用 HH:mm 格式'
    }
    
    if (setting.quietHoursEnd && !NOTIFICATION_DEFAULTS.QUIET_HOURS_FORMAT.test(setting.quietHoursEnd)) {
      errors.quietHoursEnd = '免打扰结束时间格式不正确，请使用 HH:mm 格式'
    }
    
    return errors
  }

  /**
   * 检查是否在免打扰时间内
   * @param quietHoursStart 免打扰开始时间
   * @param quietHoursEnd 免打扰结束时间
   * @returns 是否在免打扰时间内
   */
  isInQuietHours(quietHoursStart?: string, quietHoursEnd?: string): boolean {
    if (!quietHoursStart || !quietHoursEnd) return false
    
    const now = new Date()
    const currentTime = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`
    
    const start = quietHoursStart
    const end = quietHoursEnd
    
    if (start < end) {
      return currentTime >= start && currentTime <= end
    } else {
      // 跨天的情况
      return currentTime >= start || currentTime <= end
    }
  }

  /**
   * 生成通知摘要
   * @param notifications 通知列表
   * @returns 通知摘要
   */
  generateNotificationSummary(notifications: Notification[]): string {
    if (notifications.length === 0) return '暂无通知'
    
    const unreadCount = notifications.filter(n => !n.isRead).length
    const highPriorityCount = notifications.filter(n => n.priority === NotificationPriority.HIGH || n.priority === NotificationPriority.URGENT).length
    
    if (unreadCount === 0) return '所有通知已读'
    if (highPriorityCount > 0) return `您有 ${unreadCount} 条未读通知，其中 ${highPriorityCount} 条需要关注`
    return `您有 ${unreadCount} 条未读通知`
  }

  /**
   * 搜索通知
   * @param notifications 通知列表
   * @param keyword 搜索关键词
   * @returns 搜索结果
   */
  searchNotifications(notifications: Notification[], keyword: string): Notification[] {
    if (!keyword.trim()) return notifications
    
    const searchTerm = keyword.toLowerCase()
    
    return notifications.filter(notification => {
      return (
        notification.title.toLowerCase().includes(searchTerm) ||
        (notification.content && notification.content.toLowerCase().includes(searchTerm)) ||
        (notification.message && notification.message.toLowerCase().includes(searchTerm)) ||
        (notification.tag && notification.tag.toLowerCase().includes(searchTerm))
      )
    })
  }

  /**
   * 排序通知
   * @param notifications 通知列表
   * @param sortBy 排序方式
   * @returns 排序后的通知列表
   */
  sortNotifications(notifications: Notification[], sortBy: NotificationSort): Notification[] {
    const sorted = [...notifications]
    
    switch (sortBy) {
      case NotificationSort.CREATED_AT_DESC:
        return sorted.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      case NotificationSort.CREATED_AT_ASC:
        return sorted.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
      case NotificationSort.UPDATED_AT_DESC:
        return sorted.sort((a, b) => new Date(b.updatedAt || b.createdAt).getTime() - new Date(a.updatedAt || a.createdAt).getTime())
      case NotificationSort.UPDATED_AT_ASC:
        return sorted.sort((a, b) => new Date(a.updatedAt || a.createdAt).getTime() - new Date(b.updatedAt || b.createdAt).getTime())
      case NotificationSort.PRIORITY_DESC:
        return sorted.sort((a, b) => {
          const priorityOrder = {
            [NotificationPriority.CRITICAL]: 5,
            [NotificationPriority.URGENT]: 4,
            [NotificationPriority.HIGH]: 3,
            [NotificationPriority.NORMAL]: 2,
            [NotificationPriority.LOW]: 1
          }
          return priorityOrder[b.priority] - priorityOrder[a.priority]
        })
      case NotificationSort.PRIORITY_ASC:
        return sorted.sort((a, b) => {
          const priorityOrder = {
            [NotificationPriority.CRITICAL]: 5,
            [NotificationPriority.URGENT]: 4,
            [NotificationPriority.HIGH]: 3,
            [NotificationPriority.NORMAL]: 2,
            [NotificationPriority.LOW]: 1
          }
          return priorityOrder[a.priority] - priorityOrder[b.priority]
        })
      case NotificationSort.READ_AT_DESC:
        return sorted.sort((a, b) => {
          const timeA = a.readAt ? new Date(a.readAt).getTime() : 0
          const timeB = b.readAt ? new Date(b.readAt).getTime() : 0
          return timeB - timeA
        })
      case NotificationSort.READ_AT_ASC:
        return sorted.sort((a, b) => {
          const timeA = a.readAt ? new Date(a.readAt).getTime() : 0
          const timeB = b.readAt ? new Date(b.readAt).getTime() : 0
          return timeA - timeB
        })
      case NotificationSort.TITLE_ASC:
        return sorted.sort((a, b) => a.title.localeCompare(b.title))
      case NotificationSort.TITLE_DESC:
        return sorted.sort((a, b) => b.title.localeCompare(a.title))
      case NotificationSort.TYPE_ASC:
        return sorted.sort((a, b) => a.type.localeCompare(b.type))
      case NotificationSort.TYPE_DESC:
        return sorted.sort((a, b) => b.type.localeCompare(a.type))
      default:
        return sorted
    }
  }

  /**
   * 分组通知
   * @param notifications 通知列表
   * @param groupBy 分组方式
   * @returns 分组后的通知
   */
  groupNotifications(notifications: Notification[], groupBy: 'type' | 'priority' | 'status' | 'date'): Record<string, Notification[]> {
    const groups: Record<string, Notification[]> = {}
    
    notifications.forEach(notification => {
      let key = ''
      
      switch (groupBy) {
        case 'type':
          key = notification.type
          break
        case 'priority':
          key = notification.priority
          break
        case 'status':
          key = notification.status
          break
        case 'date':
          const date = new Date(notification.createdAt)
          key = date.toLocaleDateString()
          break
      }
      
      if (!groups[key]) {
        groups[key] = []
      }
      groups[key].push(notification)
    })
    
    return groups
  }
}

// ==================== 初始化方法 ====================
  
  /**
   * 初始化WebSocket连接
   */
  private initializeWebSocket() {
    if (this.wsClient) {
      return
    }
    
    // 从现有的websocketService获取客户端实例
    this.wsClient = webSocketService.getClient()
    
    if (this.wsClient) {
      this.setupWebSocketEventHandlers()
    }
  }
  
  /**
   * 初始化网络状态监控
   */
  private initializeNetworkMonitoring() {
    // 监听网络状态变化
    window.addEventListener('online', () => {
      this.isOnline = true
      this.processRetryQueue()
    })
    
    window.addEventListener('offline', () => {
      this.isOnline = false
    })
  }
  
  /**
   * 启动重试队列处理器
   */
  private startRetryQueueProcessor() {
    // 每30秒检查一次重试队列
    setInterval(() => this.processRetryQueue(), 30000)
  }
  
  /**
   * 启动缓存清理任务
   */
  private startCacheCleanup() {
    // 每5分钟清理一次过期缓存
    setInterval(() => this.cleanupCache(), 5 * 60 * 1000)
  }
  
  // ==================== WebSocket相关方法 ====================
  
  /**
   * 设置WebSocket事件处理器
   */
  private setupWebSocketEventHandlers() {
    if (!this.wsClient) return
    
    this.wsClient.on('notification', (data: any) => {
      this.handleRealtimeNotification(data)
    })
    
    this.wsClient.on('notification_update', (data: any) => {
      this.handleRealtimeNotificationUpdate(data)
    })
    
    this.wsClient.on('connection', () => {
      this.resubscribeToRealtimeEvents()
    })
  }
  
  /**
   * 处理实时通知
   */
  private handleRealtimeNotification(data: any) {
    const event: NotificationEvent = {
      type: 'notification_created',
      notification: data.notification,
      timestamp: data.timestamp || new Date().toISOString()
    }
    
    // 清除相关缓存
    this.clearCache('notifications:*')
    
    // 触发事件监听器
    this.emitRealtimeEvent('notification_created', event)
  }
  
  /**
   * 处理通知更新
   */
  private handleRealtimeNotificationUpdate(data: any) {
    const event: NotificationEvent = {
      type: data.type || 'notification_updated',
      notification: data.notification,
      timestamp: data.timestamp || new Date().toISOString()
    }
    
    // 清除相关缓存
    this.clearCache(`notification:${data.notification.id}`)
    this.clearCache('notifications:*')
    
    // 触发事件监听器
    this.emitRealtimeEvent(event.type, event)
  }
  
  /**
   * 重新订阅实时事件
   */
  private resubscribeToRealtimeEvents() {
    // 重新建立所有实时订阅
    for (const [eventType, listeners] of this.realtimeSubscriptions.entries()) {
      if (listeners.size > 0) {
        this.subscribeToRealtimeEvent(eventType)
      }
    }
  }
  
  /**
   * 订阅实时事件
   */
  private subscribeToRealtimeEvent(eventType: string) {
    if (!this.wsClient) return
    
    // 通过WebSocket发送订阅请求
    this.wsClient.send({
      type: 'subscribe',
      event: eventType,
      timestamp: new Date().toISOString()
    })
  }
  
  /**
   * 发送实时事件
   */
  private emitRealtimeEvent(eventType: string, data: NotificationEvent) {
    const listeners = this.realtimeSubscriptions.get(eventType)
    if (listeners) {
      listeners.forEach(listener => {
        try {
          listener(data)
        } catch (error) {
          console.error('实时事件监听器执行失败:', error)
        }
      })
    }
  }
  
  // ==================== 缓存管理 ====================
  
  /**
   * 获取缓存数据
   */
  private getCache(key: string): any {
    const item = this.cache.get(key)
    if (!item) return null
    
    const now = Date.now()
    if (now - item.timestamp > item.ttl) {
      this.cache.delete(key)
      return null
    }
    
    return item.data
  }
  
  /**
   * 设置缓存数据
   */
  private setCache(key: string, data: any, ttl: number = 5 * 60 * 1000) {
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl
    })
  }
  
  /**
   * 清除缓存
   */
  private clearCache(pattern: string) {
    const regex = new RegExp(pattern.replace('*', '.*'))
    for (const key of this.cache.keys()) {
      if (regex.test(key)) {
        this.cache.delete(key)
      }
    }
  }
  
  /**
   * 清理过期缓存
   */
  private cleanupCache() {
    const now = Date.now()
    for (const [key, item] of this.cache.entries()) {
      if (now - item.timestamp > item.ttl) {
        this.cache.delete(key)
      }
    }
  }
  
  // ==================== 重试机制 ====================
  
  /**
   * 添加重试任务
   */
  private addToRetryQueue(key: string, operation: () => Promise<any>, maxRetries: number = 3) {
    this.retryQueue.set(key, {
      operation,
      retryCount: 0,
      maxRetries
    })
  }
  
  /**
   * 处理重试队列
   */
  private async processRetryQueue() {
    if (!this.isOnline) return
    
    for (const [key, task] of this.retryQueue.entries()) {
      try {
        await task.operation()
        this.retryQueue.delete(key)
      } catch (error) {
        task.retryCount++
        if (task.retryCount >= task.maxRetries) {
          this.retryQueue.delete(key)
          console.error(`任务重试失败: ${key}`, error)
        }
      }
    }
  }
  
  // ==================== 错误处理 ====================
  
  /**
   * 处理API错误
   */
  private handleApiError(error: any, defaultMessage: string): Error {
    if (error.response) {
      const status = error.response.status
      const message = error.response.data?.message || defaultMessage
      
      switch (status) {
        case 401:
          return new Error('认证失败，请重新登录')
        case 403:
          return new Error('权限不足')
        case 404:
          return new Error('资源不存在')
        case 429:
          return new Error('请求过于频繁，请稍后再试')
        case 500:
          return new Error('服务器内部错误')
        default:
          return new Error(message)
      }
    } else if (error.code === 'ECONNABORTED') {
      return new Error('请求超时')
    } else if (error.message === 'Network Error') {
      return new Error('网络连接失败')
    } else {
      return new Error(defaultMessage)
    }
  }
  
  // ==================== 通知订阅管理 ====================
  
  /**
   * 获取用户的通知订阅列表
   * @param userId 用户ID
   * @param includeExpired 是否包含过期订阅
   * @returns 订阅列表
   */
  async getNotificationSubscriptions(userId: string, includeExpired = false): Promise<NotificationSubscription[]> {
    const cacheKey = `subscriptions:${userId}:${includeExpired}`
    const cached = this.getCache(cacheKey)
    if (cached) return cached

    try {
      const response = await api.get(`${this.subscriptionsPath}/${userId}`, {
        params: { includeExpired }
      })
      const subscriptions = response.data.data.map(this.mapSubscription)
      this.setCache(cacheKey, subscriptions, 5 * 60 * 1000) // 5分钟缓存
      return subscriptions
    } catch (error) {
      console.error('获取通知订阅失败:', error)
      throw this.handleApiError(error, '获取通知订阅失败')
    }
  }

  /**
   * 创建通知订阅
   * @param subscription 订阅数据
   * @returns 创建的订阅
   */
  async createNotificationSubscription(subscription: any): Promise<NotificationSubscription> {
    try {
      const response = await api.post(this.subscriptionsPath, subscription)
      const newSubscription = this.mapSubscription(response.data.data)
      
      // 清除相关缓存
      this.clearCache(`subscriptions:${subscription.userId}:*`)
      
      // 设置WebSocket订阅
      if (newSubscription.status === 'active') {
        this.setupWebSocketSubscription(newSubscription)
      }
      
      return newSubscription
    } catch (error) {
      console.error('创建通知订阅失败:', error)
      throw this.handleApiError(error, '创建通知订阅失败')
    }
  }

  /**
   * 更新通知订阅
   * @param subscriptionId 订阅ID
   * @param updates 更新数据
   * @returns 更新后的订阅
   */
  async updateNotificationSubscription(subscriptionId: string, updates: any): Promise<NotificationSubscription> {
    try {
      const response = await api.put(`${this.subscriptionsPath}/${subscriptionId}`, updates)
      const updatedSubscription = this.mapSubscription(response.data.data)
      
      // 清除相关缓存
      this.clearCache(`subscriptions:${updatedSubscription.userId}:*`)
      
      // 更新WebSocket订阅
      this.updateWebSocketSubscription(updatedSubscription)
      
      return updatedSubscription
    } catch (error) {
      console.error('更新通知订阅失败:', error)
      throw this.handleApiError(error, '更新通知订阅失败')
    }
  }

  /**
   * 暂停通知订阅
   * @param subscriptionId 订阅ID
   * @param reason 暂停原因
   * @returns 更新后的订阅
   */
  async pauseNotificationSubscription(subscriptionId: string, reason?: string): Promise<NotificationSubscription> {
    return this.updateNotificationSubscription(subscriptionId, {
      status: 'paused',
      pauseReason: reason,
      pausedAt: new Date().toISOString()
    })
  }

  /**
   * 恢复通知订阅
   * @param subscriptionId 订阅ID
   * @returns 更新后的订阅
   */
  async resumeNotificationSubscription(subscriptionId: string): Promise<NotificationSubscription> {
    return this.updateNotificationSubscription(subscriptionId, {
      status: 'active',
      pauseReason: undefined,
      pausedAt: undefined
    })
  }

  /**
   * 取消通知订阅
   * @param subscriptionId 订阅ID
   * @param reason 取消原因
   * @returns 是否成功
   */
  async cancelNotificationSubscription(subscriptionId: string, reason?: string): Promise<boolean> {
    try {
      await api.delete(`${this.subscriptionsPath}/${subscriptionId}`, {
        data: { reason }
      })
      
      // 清除相关缓存
      this.clearCache(`subscriptions:*:${subscriptionId}`)
      
      // 移除WebSocket订阅
      this.removeWebSocketSubscription(subscriptionId)
      
      return true
    } catch (error) {
      console.error('取消通知订阅失败:', error)
      throw this.handleApiError(error, '取消通知订阅失败')
    }
  }

  /**
   * 批量操作通知订阅
   * @param request 批量操作请求
   * @returns 操作结果
   */
  async batchNotificationSubscriptions(request: any): Promise<any> {
    try {
      const response = await api.post(`${this.subscriptionsPath}/batch`, request)
      
      // 清除相关缓存
      for (const subscriptionId of request.subscriptionIds) {
        this.clearCache(`subscriptions:*:${subscriptionId}`)
      }
      
      return response.data
    } catch (error) {
      console.error('批量操作通知订阅失败:', error)
      throw this.handleApiError(error, '批量操作通知订阅失败')
    }
  }
  
  // ==================== WebSocket订阅管理 ====================
  
  /**
   * 设置WebSocket订阅
   */
  private setupWebSocketSubscription(subscription: NotificationSubscription) {
    if (!this.wsClient) return
    
    this.wsClient.send({
      type: 'subscribe',
      channel: `notification:${subscription.id}`,
      data: { subscriptionId: subscription.id }
    })
  }
  
  /**
   * 更新WebSocket订阅
   */
  private updateWebSocketSubscription(subscription: NotificationSubscription) {
    if (!this.wsClient) return
    
    this.wsClient.send({
      type: 'update_subscription',
      channel: `notification:${subscription.id}`,
      data: { subscription }
    })
  }
  
  /**
   * 移除WebSocket订阅
   */
  private removeWebSocketSubscription(subscriptionId: string) {
    if (!this.wsClient) return
    
    this.wsClient.send({
      type: 'unsubscribe',
      channel: `notification:${subscriptionId}`
    })
  }
  
  // ==================== 数据映射方法 ====================
  
  /**
   * 映射订阅数据
   */
  private mapSubscription(data: any): NotificationSubscription {
    return {
      id: data.id,
      userId: data.userId,
      type: data.type,
      status: data.status,
      name: data.name,
      description: data.description,
      filters: data.filters,
      preferences: data.preferences,
      createdAt: data.createdAt,
      updatedAt: data.updatedAt,
      expiresAt: data.expiresAt,
      lastNotifiedAt: data.lastNotifiedAt,
      notificationCount: data.notificationCount || 0,
      isActive: data.status === 'active'
    }
  }
  
  /**
   * 映射模板数据
   */
  private mapTemplate(data: any): NotificationTemplate {
    return {
      id: data.id,
      name: data.name,
      description: data.description,
      type: data.type,
      content: data.content,
      variables: data.variables,
      isActive: data.isActive,
      createdAt: data.createdAt,
      updatedAt: data.updatedAt,
      createdBy: data.createdBy,
      updatedBy: data.updatedBy
    }
  }
  
  // ==================== 实时事件订阅 ====================
  
  /**
   * 订阅通知事件
   * @param eventType 事件类型
   * @param callback 回调函数
   * @returns 取消订阅函数
   */
  onNotificationEvent(eventType: string, callback: (event: NotificationEvent) => void): () => void {
    if (!this.realtimeSubscriptions.has(eventType)) {
      this.realtimeSubscriptions.set(eventType, new Set())
    }
    
    const listeners = this.realtimeSubscriptions.get(eventType)!
    listeners.add(callback)
    
    // 如果是第一个监听器，订阅WebSocket事件
    if (listeners.size === 1) {
      this.subscribeToRealtimeEvent(eventType)
    }
    
    // 返回取消订阅函数
    return () => {
      listeners.delete(callback)
      if (listeners.size === 0) {
        this.realtimeSubscriptions.delete(eventType)
      }
    }
  }
  
  /**
   * 订阅所有通知事件
   * @param callback 回调函数
   * @returns 取消订阅函数
   */
  onAllNotificationEvents(callback: (event: NotificationEvent) => void): () => void {
    const unsubscribeFunctions = [
      this.onNotificationEvent('notification_created', callback),
      this.onNotificationEvent('notification_updated', callback),
      this.onNotificationEvent('notification_deleted', callback),
      this.onNotificationEvent('notification_read', callback),
      this.onNotificationEvent('notification_delivered', callback)
    ]
    
    return () => {
      unsubscribeFunctions.forEach(unsubscribe => unsubscribe())
    }
  }
}

// 创建服务实例
export const notificationService = new NotificationService()