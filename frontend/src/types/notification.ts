/**
 * 通知系统TypeScript类型定义
 * 
 * 本文件定义了CodeShare项目通知系统前端的完整类型定义
 * 包含通知实体、设置、批量操作、筛选搜索、模板、订阅等所有相关类型
 * 与后端API接口保持一致，支持Vue 3 + TypeScript开发
 * 
 * @version 2.0.0
 * @lastUpdated 2025-09-12
 */

// ============ 基础枚举类型定义 ============

/**
 * 通知类型枚举
 * 定义系统中所有可能的通知类型
 */
export enum NotificationType {
  COMMENT = 'COMMENT',          // 评论通知
  REPLY = 'REPLY',              // 回复通知
  MESSAGE = 'MESSAGE',          // 消息通知
  SYSTEM = 'SYSTEM',            // 系统通知
  LIKE = 'LIKE',                // 点赞通知
  SHARE = 'SHARE',              // 分享通知
  FOLLOW = 'FOLLOW',            // 关注通知
  MENTION = 'MENTION',          // 提及通知
  TAG = 'TAG',                  // 标签通知
  SECURITY = 'SECURITY',        // 安全通知
  ACCOUNT = 'ACCOUNT',          // 账户通知
  UPDATE = 'UPDATE',            // 更新通知
  MAINTENANCE = 'MAINTENANCE',  // 维护通知
  ANNOUNCEMENT = 'ANNOUNCEMENT',// 公告通知
  CUSTOM = 'CUSTOM'             // 自定义通知
}

/**
 * 通知优先级枚举
 * 定义通知的优先级级别
 */
export enum NotificationPriority {
  LOW = 'LOW',          // 低优先级
  NORMAL = 'NORMAL',    // 普通优先级
  HIGH = 'HIGH',        // 高优先级
  URGENT = 'URGENT',    // 紧急优先级
  CRITICAL = 'CRITICAL'  // 严重优先级
}

/**
 * 通知状态枚举
 * 定义通知的生命周期状态
 */
export enum NotificationStatus {
  PENDING = 'PENDING',      // 待发送
  SENDING = 'SENDING',      // 发送中
  SENT = 'SENT',            // 已发送
  DELIVERED = 'DELIVERED',  // 已送达
  READ = 'READ',            // 已读
  UNREAD = 'UNREAD',        // 未读
  CONFIRMED = 'CONFIRMED',  // 已确认
  FAILED = 'FAILED',        // 发送失败
  EXPIRED = 'EXPIRED',      // 已过期
  CANCELLED = 'CANCELLED',  // 已取消
  ARCHIVED = 'ARCHIVED'     // 已归档
}

/**
 * 通知送达状态枚举
 * 定义通知的送达状态
 */
export enum NotificationDeliveryStatus {
  PENDING = 'PENDING',     // 待发送
  SENDING = 'SENDING',     // 发送中
  SENT = 'SENT',           // 已发送
  DELIVERED = 'DELIVERED', // 已送达
  READ = 'READ',           // 已读
  FAILED = 'FAILED',       // 发送失败
  SKIPPED = 'SKIPPED',     // 已跳过
  CANCELLED = 'CANCELLED', // 已取消
  EXPIRED = 'EXPIRED'      // 已过期
}

// ============ 通知设置相关类型 ============

/**
 * 通知渠道枚举
 * 定义通知发送的渠道
 */
export enum NotificationChannel {
  IN_APP = 'IN_APP',        // 应用内通知
  EMAIL = 'EMAIL',          // 邮件通知
  PUSH = 'PUSH',            // 推送通知
  DESKTOP = 'DESKTOP',      // 桌面通知
  SMS = 'SMS',              // 短信通知
  WEBHOOK = 'WEBHOOK',      // Webhook通知
  SLACK = 'SLACK',          // Slack通知
  WE_CHAT = 'WE_CHAT',      // 微信通知
  DING_TALK = 'DING_TALK',  // 钉钉通知
  WE_COM = 'WE_COM'         // 企业微信通知
}

/**
 * 通知频率枚举
 * 定义通知发送的频率
 */
export enum NotificationFrequency {
  IMMEDIATE = 'IMMEDIATE',  // 立即发送
  HOURLY = 'HOURLY',        // 每小时发送
  DAILY = 'DAILY',          // 每日发送
  WEEKLY = 'WEEKLY',        // 每周发送
  MONTHLY = 'MONTHLY',      // 每月发送
  NEVER = 'NEVER'           // 永不发送
}

/**
 * 免打扰设置接口
 */
export interface DoNotDisturbSettings {
  /** 是否启用免打扰模式 */
  enabled: boolean
  /** 免打扰开始时间 - 格式：HH:mm */
  startTime: string
  /** 免打扰结束时间 - 格式：HH:mm */
  endTime: string
  /** 免打扰的星期几 - 0-6 代表周日到周六 */
  daysOfWeek?: number[]
  /** 是否在免打扰期间发送紧急通知 */
  allowUrgent: boolean
}

/**
 * 通知偏好设置接口
 */
export interface NotificationPreference {
  /** 通知类型 */
  type: NotificationType
  /** 是否启用该类型通知 */
  enabled: boolean
  /** 通知渠道列表 */
  channels: NotificationChannel[]
  /** 通知优先级过滤 */
  priorities: NotificationPriority[]
  /** 通知频率 */
  frequency: NotificationFrequency
  /** 是否启用声音提醒 */
  enableSound: boolean
  /** 是否启用桌面通知 */
  enableDesktop: boolean
  /** 自定义设置 */
  customSettings?: Record<string, any>
}

/**
 * 通知设置接口
 */
export interface NotificationSettings {
  /** 设置ID */
  id: string
  /** 用户ID */
  userId: string
  /** 全局通知启用状态 */
  enableNotifications: boolean
  /** 默认通知频率 */
  defaultFrequency: NotificationFrequency
  /** 免打扰设置 */
  doNotDisturb: DoNotDisturbSettings
  /** 通知偏好设置列表 */
  preferences: NotificationPreference[]
  /** 批量通知间隔（分钟） */
  batchIntervalMinutes: number
  /** 是否启用批量通知 */
  enableBatching: boolean
  /** 邮件通知频率 */
  emailFrequency: NotificationFrequency
  /** 语言设置 */
  language: string
  /** 时区设置 */
  timeZone: string
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt?: string
  /** 是否为默认设置 */
  isDefault: boolean
  /** 设置是否活跃 */
  isActive: boolean
}

// ============ 通知批量操作类型 ============

/**
 * 批量通知操作类型枚举
 */
export enum BulkNotificationOperationType {
  MARK_AS_READ = 'MARK_AS_READ',        // 标记为已读
  MARK_AS_UNREAD = 'MARK_AS_UNREAD',    // 标记为未读
  ARCHIVE = 'ARCHIVE',                  // 归档
  UNARCHIVE = 'UNARCHIVE',              // 取消归档
  DELETE = 'DELETE',                    // 删除
  RESTORE = 'RESTORE',                  // 恢复
  CONFIRM = 'CONFIRM',                  // 确认
  CANCEL = 'CANCEL',                    // 取消
  SEND = 'SEND',                        // 发送
  RESEND = 'RESEND'                     // 重新发送
}

/**
 * 批量通知操作接口
 */
export interface BulkNotificationOperation {
  /** 通知ID列表 */
  notificationIds: string[]
  /** 操作类型 */
  operation: BulkNotificationOperationType
  /** 操作原因 */
  reason?: string
  /** 操作者ID */
  operatorId: string
  /** 操作者名称 */
  operatorName: string
  /** 操作时间 */
  timestamp: string
  /** 是否发送通知给用户 */
  sendNotification: boolean
  /** 自定义参数 */
  parameters?: Record<string, any>
}

/**
 * 批量操作结果接口
 */
export interface BulkNotificationResult {
  /** 总数量 */
  totalCount: number
  /** 成功数量 */
  successCount: number
  /** 失败数量 */
  failedCount: number
  /** 错误消息列表 */
  errorMessages: string[]
  /** 成功的ID列表 */
  successfulIds: string[]
  /** 失败的ID列表 */
  failedIds: string[]
  /** 操作耗时（毫秒） */
  executionTimeMs: number
  /** 批量操作ID */
  batchId?: string
  /** 操作状态 */
  status: 'pending' | 'processing' | 'completed' | 'failed'
  /** 完成时间 */
  completedAt?: string
}

// ============ 通知筛选和搜索类型 ============

/**
 * 通知筛选条件接口
 */
export interface NotificationFilter {
  /** 用户ID筛选 */
  userId?: string
  /** 通知类型筛选 */
  type?: NotificationType
  /** 通知优先级筛选 */
  priority?: NotificationPriority
  /** 通知状态筛选 */
  status?: NotificationStatus
  /** 通知渠道筛选 */
  channel?: NotificationChannel
  /** 是否已读筛选 */
  isRead?: boolean
  /** 是否已归档筛选 */
  isArchived?: boolean
  /** 是否已删除筛选 */
  isDeleted?: boolean
  /** 相关实体类型筛选 */
  relatedEntityType?: string
  /** 相关实体ID筛选 */
  relatedEntityId?: string
  /** 触发用户ID筛选 */
  triggeredByUserId?: string
  /** 通知标签筛选 */
  tag?: string
  /** 开始日期筛选 */
  startDate?: string
  /** 结束日期筛选 */
  endDate?: string
  /** 搜索关键词 */
  search?: string
  /** 排序方式 */
  sortBy: NotificationSortOptions
  /** 分页信息 */
  pagination: NotificationPagination
  /** 是否包含已删除的通知 */
  includeDeleted?: boolean
  /** 是否包含已归档的通知 */
  includeArchived?: boolean
  /** 是否只显示需要确认的通知 */
  requiresConfirmationOnly?: boolean
}

/**
 * 通知排序选项接口
 */
export interface NotificationSortOptions {
  /** 排序字段 */
  field: 'createdAt' | 'updatedAt' | 'priority' | 'status' | 'readAt' | 'deliveredAt' | 'title'
  /** 排序方向 */
  order: 'asc' | 'desc'
  /** 多级排序 */
  thenBy?: NotificationSortOptions
}

/**
 * 通知分页接口
 */
export interface NotificationPagination {
  /** 当前页码 */
  page: number
  /** 每页数量 */
  pageSize: number
  /** 总数量 */
  totalItems?: number
  /** 总页数 */
  totalPages?: number
  /** 是否有下一页 */
  hasNextPage?: boolean
  /** 是否有上一页 */
  hasPreviousPage?: boolean
  /** 游标（用于无限滚动） */
  cursor?: string
}

/**
 * 通知搜索选项接口
 */
export interface NotificationSearchOptions {
  /** 搜索关键词 */
  keyword: string
  /** 搜索范围 */
  searchScope: NotificationSearchScope
  /** 搜索字段 */
  searchFields: NotificationSearchField[]
  /** 筛选条件 */
  filter?: NotificationFilter
  /** 排序选项 */
  sortBy?: NotificationSortOptions
  /** 分页信息 */
  pagination: NotificationPagination
  /** 是否高亮匹配结果 */
  highlightMatches: boolean
  /** 是否模糊搜索 */
  fuzzySearch: boolean
  /** 搜索语言 */
  language?: string
}

/**
 * 通知搜索范围枚举
 */
export enum NotificationSearchScope {
  ALL = 'ALL',                    // 全部搜索
  TITLE = 'TITLE',                // 标题搜索
  CONTENT = 'CONTENT',            // 内容搜索
  MESSAGE = 'MESSAGE',            // 消息搜索
  TAGS = 'TAGS',                  // 标签搜索
  TITLE_AND_CONTENT = 'TITLE_AND_CONTENT' // 标题和内容搜索
}

/**
 * 通知搜索字段枚举
 */
export enum NotificationSearchField {
  TITLE = 'title',
  CONTENT = 'content',
  MESSAGE = 'message',
  TAGS = 'tags',
  DATA = 'data'
}

/**
 * 通知类型筛选接口
 */
export interface NotificationTypeFilter {
  /** 包含的通知类型 */
  includeTypes: NotificationType[]
  /** 排除的通知类型 */
  excludeTypes: NotificationType[]
  /** 优先级筛选 */
  priorityFilter: NotificationPriorityFilter
  /** 渠道筛选 */
  channelFilter: NotificationChannelFilter
}

/**
 * 通知优先级筛选接口
 */
export interface NotificationPriorityFilter {
  /** 包含的优先级 */
  includePriorities: NotificationPriority[]
  /** 排除的优先级 */
  excludePriorities: NotificationPriority[]
  /** 最低优先级 */
  minPriority?: NotificationPriority
  /** 最高优先级 */
  maxPriority?: NotificationPriority
}

/**
 * 通知渠道筛选接口
 */
export interface NotificationChannelFilter {
  /** 包含的渠道 */
  includeChannels: NotificationChannel[]
  /** 排除的渠道 */
  excludeChannels: NotificationChannel[]
  /** 是否只显示已启用的渠道 */
  enabledOnly?: boolean
}

// ============ 通知模板类型 ============

/**
 * 模板变量类型枚举
 */
export enum TemplateVariableType {
  STRING = 'string',        // 字符串类型
  NUMBER = 'number',        // 数字类型
  BOOLEAN = 'boolean',      // 布尔类型
  DATE = 'date',            // 日期类型
  OBJECT = 'object',        // 对象类型
  ARRAY = 'array',          // 数组类型
  CUSTOM = 'custom'         // 自定义类型
}

/**
 * 模板变量接口
 */
export interface TemplateVariable {
  /** 变量名称 */
  name: string
  /** 变量描述 */
  description: string
  /** 变量类型 */
  type: TemplateVariableType
  /** 是否必需 */
  required: boolean
  /** 默认值 */
  defaultValue?: any
  /** 验证规则 */
  validation?: {
    pattern?: string
    minLength?: number
    maxLength?: number
    min?: number
    max?: number
    enum?: any[]
  }
  /** 示例值 */
  example?: any
}

/**
 * 通知模板接口
 */
export interface NotificationTemplate {
  /** 模板ID */
  id: string
  /** 模板名称 */
  name: string
  /** 模板描述 */
  description: string
  /** 通知类型 */
  type: NotificationType
  /** 通知渠道 */
  channel: NotificationChannel
  /** 标题模板 */
  titleTemplate: string
  /** 内容模板 */
  contentTemplate: string
  /** 消息模板 */
  messageTemplate?: string
  /** 模板变量列表 */
  variables: TemplateVariable[]
  /** 优先级 */
  priority: NotificationPriority
  /** 是否启用 */
  isActive: boolean
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt: string
  /** 创建者ID */
  createdBy: string
  /** 创建者名称 */
  createdByUsername: string
  /** 使用次数 */
  usageCount: number
  /** 最后使用时间 */
  lastUsedAt?: string
  /** 版本号 */
  version: string
  /** 是否为系统模板 */
  isSystemTemplate: boolean
  /** 标签 */
  tags?: string[]
}

// ============ 通知订阅类型 ============

/**
 * 订阅类型枚举
 */
export enum SubscriptionType {
  ENTITY = 'ENTITY',          // 实体订阅
  USER = 'USER',              // 用户订阅
  TAG = 'TAG',                // 标签订阅
  SEARCH = 'SEARCH',          // 搜索订阅
  CUSTOM = 'CUSTOM'           // 自定义订阅
}

/**
 * 通知订阅接口
 */
export interface NotificationSubscription {
  /** 订阅ID */
  id: string
  /** 用户ID */
  userId: string
  /** 订阅名称 */
  name: string
  /** 订阅描述 */
  description?: string
  /** 订阅类型 */
  type: SubscriptionType
  /** 订阅的目标实体类型 */
  entityType?: string
  /** 订阅的目标实体ID */
  entityId?: string
  /** 订阅条件 */
  conditions: Record<string, any>
  /** 通知类型列表 */
  notificationTypes: NotificationType[]
  /** 通知渠道列表 */
  channels: NotificationChannel[]
  /** 通知频率 */
  frequency: NotificationFrequency
  /** 是否启用 */
  isActive: boolean
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt: string
  /** 下次通知时间 */
  nextNotificationAt?: string
  /** 最后通知时间 */
  lastNotificationAt?: string
  /** 通知次数 */
  notificationCount: number
  /** 过期时间 */
  expiresAt?: string
  /** 免打扰设置 */
  doNotDisturb?: DoNotDisturbSettings
}

/**
 * 取消订阅请求接口
 */
export interface UnsubscribeRequest {
  /** 订阅ID */
  subscriptionId: string
  /** 取消订阅原因 */
  reason?: string
  /** 用户ID */
  userId: string
  /** 是否永久取消订阅 */
  permanent: boolean
  /** 替代订阅ID（用于转移订阅） */
  replacementSubscriptionId?: string
}

// ============ API请求和响应类型 ============

/**
 * 创建通知请求接口
 */
export interface CreateNotificationRequest {
  /** 用户ID列表，如果为空则发送给所有用户 */
  userIds?: string[]
  /** 通知类型 */
  type: NotificationType
  /** 通知标题 */
  title: string
  /** 通知内容 */
  content?: string
  /** 通知消息 */
  message?: string
  /** 通知优先级 */
  priority: NotificationPriority
  /** 通知渠道列表 */
  channels: NotificationChannel[]
  /** 相关实体类型 */
  relatedEntityType?: string
  /** 相关实体ID */
  relatedEntityId?: string
  /** 触发用户ID */
  triggeredByUserId?: string
  /** 触发用户名称 */
  triggeredByUserName?: string
  /** 通知操作类型 */
  action?: string
  /** 过期时间 */
  expiresAt?: string
  /** 计划发送时间 */
  scheduledToSendAt?: string
  /** 附加数据 */
  data?: Record<string, any>
  /** 通知标签 */
  tags?: string[]
  /** 通知图标 */
  icon?: string
  /** 通知颜色 */
  color?: string
  /** 是否需要确认 */
  requiresConfirmation: boolean
  /** 模板ID */
  templateId?: string
  /** 模板变量 */
  templateVariables?: Record<string, any>
}

/**
 * 更新通知请求接口
 */
export interface UpdateNotificationRequest {
  /** 通知标题 */
  title?: string
  /** 通知内容 */
  content?: string
  /** 通知消息 */
  message?: string
  /** 通知优先级 */
  priority?: NotificationPriority
  /** 通知操作类型 */
  action?: string
  /** 过期时间 */
  expiresAt?: string
  /** 计划发送时间 */
  scheduledToSendAt?: string
  /** 附加数据 */
  data?: Record<string, any>
  /** 通知标签 */
  tags?: string[]
  /** 通知图标 */
  icon?: string
  /** 通知颜色 */
  color?: string
  /** 是否需要确认 */
  requiresConfirmation?: boolean
}

/**
 * 通知响应接口
 */
export interface NotificationResponse {
  /** 通知ID */
  id: string
  /** 接收通知的用户ID */
  userId: string
  /** 接收通知的用户名称 */
  userName: string
  /** 接收通知的用户头像 */
  userAvatar?: string
  /** 通知类型 */
  type: NotificationType
  /** 通知标题 */
  title: string
  /** 通知内容 */
  content?: string
  /** 通知消息 */
  message?: string
  /** 通知优先级 */
  priority: NotificationPriority
  /** 通知状态 */
  status: NotificationStatus
  /** 通知渠道 */
  channel: NotificationChannel
  /** 是否已读 */
  isRead: boolean
  /** 读取时间 */
  readAt?: string
  /** 送达时间 */
  deliveredAt?: string
  /** 相关实体类型 */
  relatedEntityType?: string
  /** 相关实体ID */
  relatedEntityId?: string
  /** 触发通知的用户ID */
  triggeredByUserId?: string
  /** 触发通知的用户名称 */
  triggeredByUserName?: string
  /** 触发通知的用户头像 */
  triggeredByUserAvatar?: string
  /** 通知操作类型 */
  action?: string
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt?: string
  /** 过期时间 */
  expiresAt?: string
  /** 计划发送时间 */
  scheduledToSendAt?: string
  /** 发送次数 */
  sendCount: number
  /** 最后发送时间 */
  lastSentAt?: string
  /** 错误信息 */
  errorMessage?: string
  /** 附加数据 */
  data?: Record<string, any>
  /** 通知标签 */
  tag?: string
  /** 通知图标 */
  icon?: string
  /** 通知颜色 */
  color?: string
  /** 是否需要确认 */
  requiresConfirmation: boolean
  /** 确认时间 */
  confirmedAt?: string
  /** 是否已归档 */
  isArchived: boolean
  /** 归档时间 */
  archivedAt?: string
  /** 是否已删除 */
  isDeleted: boolean
  /** 删除时间 */
  deletedAt?: string
  /** 通知发送历史记录 */
  deliveryHistory: NotificationDeliveryHistory[]
}

/**
 * 通知列表响应接口
 */
export interface NotificationListResponse {
  /** 通知列表 */
  notifications: NotificationResponse[]
  /** 分页信息 */
  pagination: NotificationPagination
  /** 筛选条件 */
  filter: NotificationFilter
  /** 统计信息 */
  stats: NotificationStats
  /** 是否有更多数据 */
  hasMore: boolean
  /** 下一页游标 */
  nextCursor?: string
}

/**
 * 通知统计响应接口
 */
export interface NotificationStatsResponse {
  /** 统计信息 */
  stats: NotificationStats
  /** 时间范围 */
  timeRange: {
    start: string
    end: string
  }
  /** 汇总信息 */
  summary: {
    totalNotifications: number
    unreadNotifications: number
    highPriorityNotifications: number
    failedNotifications: number
    averageDeliveryTime: number
    averageReadTime: number
  }
}

/**
 * 通知设置响应接口
 */
export interface NotificationSettingsResponse {
  /** 通知设置 */
  settings: NotificationSettings
  /** 可用通知类型 */
  availableTypes: NotificationType[]
  /** 可用通知渠道 */
  availableChannels: NotificationChannel[]
  /** 系统默认设置 */
  systemDefaults: Partial<NotificationSettings>
  /** 用户权限 */
  permissions: NotificationPermissions
}

// ============ 通知实时类型 ============

/**
 * 通知事件类型枚举
 */
export enum NotificationEventType {
  CREATED = 'created',          // 通知创建
  UPDATED = 'updated',          // 通知更新
  DELETED = 'deleted',          // 通知删除
  READ = 'read',                // 通知已读
  DELIVERED = 'delivered',      // 通知已送达
  FAILED = 'failed',            // 通知发送失败
  CONFIRMED = 'confirmed',      // 通知已确认
  ARCHIVED = 'archived',        // 通知已归档
  BULK_OPERATION = 'bulk_operation', // 批量操作
  SETTINGS_UPDATED = 'settings_updated' // 设置更新
}

/**
 * 通知事件接口
 */
export interface NotificationEvent {
  /** 事件类型 */
  type: NotificationEventType
  /** 事件ID */
  eventId: string
  /** 通知ID */
  notificationId?: string
  /** 用户ID */
  userId: string
  /** 事件数据 */
  data: any
  /** 事件时间 */
  timestamp: string
  /** 事件源 */
  source: 'system' | 'user' | 'api'
  /** 事件版本 */
  version: string
  /** 关联数据 */
  relatedData?: {
    entityType?: string
    entityId?: string
    action?: string
  }
}

/**
 * 通知实时更新接口
 */
export interface NotificationRealtimeUpdate {
  /** 更新类型 */
  updateType: NotificationEventType
  /** 通知数据 */
  notification?: NotificationResponse
  /** 批量操作结果 */
  bulkResult?: BulkNotificationResult
  /** 设置更新数据 */
  settingsUpdate?: Partial<NotificationSettings>
  /** 统计更新数据 */
  statsUpdate?: Partial<NotificationStats>
  /** 更新时间 */
  updatedAt: string
  /** 目标用户ID列表 */
  targetUserIds?: string[]
  /** 广播标识 */
  isBroadcast: boolean
}

// ============ 统计类型 ============

/**
 * 通知统计接口
 */
export interface NotificationStats {
  /** 总通知数量 */
  totalNotifications: number
  /** 未读通知数量 */
  unreadNotifications: number
  /** 已读通知数量 */
  readNotifications: number
  /** 发送失败数量 */
  failedNotifications: number
  /** 已归档数量 */
  archivedNotifications: number
  /** 已删除数量 */
  deletedNotifications: number
  /** 高优先级通知数量 */
  highPriorityNotifications: number
  /** 今日通知数量 */
  todayNotifications: number
  /** 本周通知数量 */
  weekNotifications: number
  /** 本月通知数量 */
  monthNotifications: number
  /** 按类型统计 */
  byType: Record<NotificationType, number>
  /** 按优先级统计 */
  byPriority: Record<NotificationPriority, number>
  /** 按状态统计 */
  byStatus: Record<NotificationStatus, number>
  /** 按渠道统计 */
  byChannel: Record<NotificationChannel, number>
  /** 平均送达时间（秒） */
  averageDeliveryTime: number
  /** 平均阅读时间（秒） */
  averageReadTime: number
  /** 送达率 */
  deliveryRate: number
  /** 阅读率 */
  readRate: number
  /** 最后通知时间 */
  lastNotificationAt?: string
}

/**
 * 通知渠道统计接口
 */
export interface NotificationChannelStats {
  /** 渠道 */
  channel: NotificationChannel
  /** 发送数量 */
  sentCount: number
  /** 送达数量 */
  deliveredCount: number
  /** 阅读数量 */
  readCount: number
  /** 失败数量 */
  failedCount: number
  /** 送达率 */
  deliveryRate: number
  /** 阅读率 */
  readRate: number
  /** 平均送达时间（秒） */
  averageDeliveryTime: number
  /** 平均阅读时间（秒） */
  averageReadTime: number
  /** 成本统计 */
  cost?: {
    totalCost: number
    averageCost: number
    currency: string
  }
}

/**
 * 通知类型统计接口
 */
export interface NotificationTypeStats {
  /** 通知类型 */
  type: NotificationType
  /** 总数量 */
  totalCount: number
  /** 未读数量 */
  unreadCount: number
  /** 已读数量 */
  readCount: number
  /** 失败数量 */
  failedCount: number
  /** 平均优先级 */
  averagePriority: number
  /** 最常用渠道 */
  topChannels: Array<{
    channel: NotificationChannel
    count: number
    percentage: number
  }>
  /** 按日期分布 */
  dailyDistribution: Array<{
    date: string
    count: number
    unreadCount: number
  }>
}

// ============ 基础通知实体类型 ============

/**
 * 通知基础接口
 */
export interface Notification {
  /** 通知唯一标识符 */
  id: string
  /** 接收通知的用户ID */
  userId: string
  /** 通知类型 */
  type: NotificationType
  /** 通知标题 */
  title: string
  /** 通知内容 */
  content?: string
  /** 通知消息 */
  message?: string
  /** 通知优先级 */
  priority: NotificationPriority
  /** 通知状态 */
  status: NotificationStatus
  /** 通知渠道 */
  channel: NotificationChannel
  /** 是否已读 */
  isRead: boolean
  /** 读取时间 */
  readAt?: string
  /** 送达时间 */
  deliveredAt?: string
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt?: string
  /** 过期时间 */
  expiresAt?: string
  /** 计划发送时间 */
  scheduledToSendAt?: string
  /** 相关实体类型 */
  relatedEntityType?: string
  /** 相关实体ID */
  relatedEntityId?: string
  /** 触发通知的用户ID */
  triggeredByUserId?: string
  /** 通知操作类型 */
  action?: string
  /** 附加数据 */
  data?: Record<string, any>
  /** 通知标签 */
  tag?: string
  /** 通知图标 */
  icon?: string
  /** 通知颜色 */
  color?: string
  /** 是否需要确认 */
  requiresConfirmation: boolean
  /** 确认时间 */
  confirmedAt?: string
  /** 是否已归档 */
  isArchived: boolean
  /** 归档时间 */
  archivedAt?: string
  /** 是否已删除 */
  isDeleted: boolean
  /** 删除时间 */
  deletedAt?: string
  /** 发送次数 */
  sendCount: number
  /** 最后发送时间 */
  lastSentAt?: string
  /** 错误信息 */
  errorMessage?: string
  /** 通知发送历史记录 */
  deliveryHistory: NotificationDeliveryHistory[]
}

/**
 * 通知发送历史接口
 */
export interface NotificationDeliveryHistory {
  /** 历史记录ID */
  id: string
  /** 通知ID */
  notificationId: string
  /** 通知渠道 */
  channel: NotificationChannel
  /** 送达状态 */
  status: NotificationDeliveryStatus
  /** 发送时间 */
  sentAt: string
  /** 送达时间 */
  deliveredAt?: string
  /** 阅读时间 */
  readAt?: string
  /** 错误信息 */
  errorMessage?: string
  /** 重试次数 */
  retryCount: number
  /** 最后重试时间 */
  lastRetryAt?: string
  /** 接收地址 */
  recipientAddress?: string
  /** 服务提供商 */
  provider?: string
  /** 成本 */
  cost?: number
  /** 元数据 */
  metadata?: Record<string, any>
}

/**
 * 通知权限接口
 */
export interface NotificationPermissions {
  /** 是否可以创建通知 */
  canCreate: boolean
  /** 是否可以读取通知 */
  canRead: boolean
  /** 是否可以更新通知 */
  canUpdate: boolean
  /** 是否可以删除通知 */
  canDelete: boolean
  /** 是否可以标记为已读 */
  canMarkAsRead: boolean
  /** 是否可以归档通知 */
  canArchive: boolean
  /** 是否可以确认通知 */
  canConfirm: boolean
  /** 是否可以管理设置 */
  canManageSettings: boolean
  /** 是否可以发送测试通知 */
  canSendTest: boolean
  /** 是否可以导出通知 */
  canExport: boolean
  /** 是否可以导入通知 */
  canImport: boolean
  /** 是否可以查看统计 */
  canViewStats: boolean
  /** 是否可以管理其他用户通知 */
  canManageOthers: boolean
  /** 是否可以管理模板 */
  canManageTemplates: boolean
  /** 是否可以管理订阅 */
  canManageSubscriptions: boolean
}

// ============ 分页结果类型定义 ============

// 导入分页结果基础类型
import type { PaginatedResult } from './index'

/**
 * 通知分页结果类型
 */
export type PaginatedNotifications = PaginatedResult<NotificationResponse>

/**
 * 通知设置分页结果类型
 */
export type PaginatedNotificationSettings = PaginatedResult<NotificationSettings>

/**
 * 通知模板分页结果类型
 */
export type PaginatedNotificationTemplates = PaginatedResult<NotificationTemplate>

/**
 * 通知订阅分页结果类型
 */
export type PaginatedNotificationSubscriptions = PaginatedResult<NotificationSubscription>

/**
 * 通知发送历史分页结果类型
 */
export type PaginatedNotificationDeliveryHistory = PaginatedResult<NotificationDeliveryHistory>

/**
 * 批量操作结果分页类型
 */
export type PaginatedBulkNotificationResults = PaginatedResult<BulkNotificationResult>

// ============ 工具类型和默认值 ============

/**
 * 通知类型到字符串的映射
 */
export type NotificationTypeLabel = Record<NotificationType, string>

/**
 * 通知优先级到字符串的映射
 */
export type NotificationPriorityLabel = Record<NotificationPriority, string>

/**
 * 通知状态到字符串的映射
 */
export type NotificationStatusLabel = Record<NotificationStatus, string>

/**
 * 通知渠道到字符串的映射
 */
export type NotificationChannelLabel = Record<NotificationChannel, string>

/**
 * 通知创建请求选项
 */
export interface NotificationCreateOptions {
  /** 是否立即发送 */
  sendImmediately?: boolean
  /** 是否重试失败的通知 */
  retryOnFailure?: boolean
  /** 最大重试次数 */
  maxRetries?: number
  /** 重试间隔（秒） */
  retryInterval?: number
  /** 是否发送回执 */
  sendReceipt?: boolean
  /** 是否记录到历史 */
  logToHistory?: boolean
}

/**
 * 通知更新请求选项
 */
export interface NotificationUpdateOptions {
  /** 是否更新发送历史 */
  updateHistory?: boolean
  /** 是否发送更新通知 */
  sendUpdateNotification?: boolean
  /** 是否强制更新 */
  forceUpdate?: boolean
  /** 是否记录更新历史 */
  logUpdateHistory?: boolean
}

/**
 * 通知查询选项
 */
export interface NotificationQueryOptions {
  /** 是否包含发送历史 */
  includeDeliveryHistory?: boolean
  /** 是否包含用户信息 */
  includeUserInfo?: boolean
  /** 是否包含触发用户信息 */
  includeTriggerUserInfo?: boolean
  /** 是否包含相关实体信息 */
  includeRelatedEntityInfo?: boolean
  /** 是否包含统计数据 */
  includeStats?: boolean
  /** 缓存时间（秒） */
  cacheTime?: number
  /** 是否使用缓存 */
  useCache?: boolean
}

/**
 * 默认通知设置
 */
export const DEFAULT_NOTIFICATION_SETTINGS: Partial<NotificationSettings> = {
  enableNotifications: true,
  defaultFrequency: NotificationFrequency.IMMEDIATE,
  batchIntervalMinutes: 30,
  enableBatching: true,
  emailFrequency: NotificationFrequency.DAILY,
  language: 'zh-CN',
  timeZone: 'Asia/Shanghai',
  isDefault: false,
  isActive: true,
  doNotDisturb: {
    enabled: false,
    startTime: '22:00',
    endTime: '08:00',
    daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
    allowUrgent: true
  }
}

/**
 * 默认通知筛选条件
 */
export const DEFAULT_NOTIFICATION_FILTER: Partial<NotificationFilter> = {
  isRead: false,
  isArchived: false,
  isDeleted: false,
  includeDeleted: false,
  includeArchived: false,
  requiresConfirmationOnly: false,
  sortBy: {
    field: 'createdAt',
    order: 'desc'
  },
  pagination: {
    page: 1,
    pageSize: 20
  }
}

/**
 * 默认免打扰设置
 */
export const DEFAULT_DO_NOT_DISTURB_SETTINGS: DoNotDisturbSettings = {
  enabled: false,
  startTime: '22:00',
  endTime: '08:00',
  daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
  allowUrgent: true
}

/**
 * 默认通知偏好设置
 */
export const DEFAULT_NOTIFICATION_PREFERENCE: Partial<NotificationPreference> = {
  enabled: true,
  channels: [NotificationChannel.IN_APP],
  priorities: [NotificationPriority.NORMAL, NotificationPriority.HIGH, NotificationPriority.URGENT],
  frequency: NotificationFrequency.IMMEDIATE,
  enableSound: true,
  enableDesktop: true
}

// ============ 导出所有类型 ============

export {
  Notification,
  NotificationResponse,
  NotificationSettings,
  NotificationTemplate,
  NotificationSubscription,
  NotificationFilter,
  NotificationSortOptions,
  NotificationPagination,
  NotificationSearchOptions,
  NotificationTypeFilter,
  NotificationPriorityFilter,
  NotificationChannelFilter,
  NotificationDeliveryHistory,
  NotificationPermissions,
  BulkNotificationOperation,
  BulkNotificationResult,
  NotificationEvent,
  NotificationRealtimeUpdate,
  NotificationStats,
  NotificationChannelStats,
  NotificationTypeStats,
  CreateNotificationRequest,
  UpdateNotificationRequest,
  NotificationListResponse,
  NotificationStatsResponse,
  NotificationSettingsResponse,
  UnsubscribeRequest,
  DoNotDisturbSettings,
  NotificationPreference,
  TemplateVariable,
  NotificationCreateOptions,
  NotificationUpdateOptions,
  NotificationQueryOptions,
  DEFAULT_NOTIFICATION_SETTINGS,
  DEFAULT_NOTIFICATION_FILTER,
  DEFAULT_DO_NOT_DISTURB_SETTINGS,
  DEFAULT_NOTIFICATION_PREFERENCE
}

// 导出枚举类型
export {
  NotificationType,
  NotificationPriority,
  NotificationStatus,
  NotificationDeliveryStatus,
  NotificationChannel,
  NotificationFrequency,
  TemplateVariableType,
  SubscriptionType,
  NotificationSearchScope,
  NotificationSearchField,
  BulkNotificationOperationType,
  NotificationEventType
}

// 导出分页结果类型
export {
  PaginatedNotifications,
  PaginatedNotificationSettings,
  PaginatedNotificationTemplates,
  PaginatedNotificationSubscriptions,
  PaginatedNotificationDeliveryHistory,
  PaginatedBulkNotificationResults
}