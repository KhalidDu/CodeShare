// 通知系统相关的TypeScript类型定义

// 通知类型枚举
export enum NotificationType {
  Info = 'info',
  Success = 'success',
  Warning = 'warning',
  Error = 'error',
  System = 'system',
  User = 'user',
  Security = 'security',
  Update = 'update'
}

// 通知优先级枚举
export enum NotificationPriority {
  Low = 'low',
  Normal = 'normal',
  High = 'high',
  Urgent = 'urgent'
}

// 通知状态枚举
export enum NotificationStatus {
  Unread = 'unread',
  Read = 'read',
  Archived = 'archived',
  Deleted = 'deleted'
}

// 通知渠道枚举
export enum NotificationChannel {
  InApp = 'in_app',
  Email = 'email',
  Push = 'push',
  SMS = 'sms'
}

// 通知操作类型枚举
export enum NotificationActionType {
  Link = 'link',
  Button = 'button',
  Modal = 'modal',
  ApiCall = 'api_call'
}

// 通知操作接口
export interface NotificationAction {
  id: string
  type: NotificationActionType
  label: string
  value: string
  icon?: string
  style?: 'primary' | 'secondary' | 'success' | 'warning' | 'danger'
  confirmMessage?: string
  callback?: (action: NotificationAction, notification: Notification) => void
}

// 通知接口
export interface Notification {
  id: string
  userId: string
  type: NotificationType
  title: string
  message: string
  description?: string
  priority: NotificationPriority
  status: NotificationStatus
  channel: NotificationChannel
  data?: Record<string, any>
  actions?: NotificationAction[]
  relatedId?: string
  relatedType?: string
  imageUrl?: string
  videoUrl?: string
  documentUrl?: string
  createdAt: string
  updatedAt: string
  readAt?: string
  expiresAt?: string
  tags?: string[]
  metadata?: Record<string, any>
}

// 通知设置接口
export interface NotificationSettings {
  userId: string
  enableNotifications: boolean
  enableSound: boolean
  enableDesktopNotifications: boolean
  enableEmailNotifications: boolean
  enablePushNotifications: boolean
  enableSMSNotifications: boolean
  quietHours: {
    enabled: boolean
    startTime: string
    endTime: string
  }
  channels: {
    [key in NotificationChannel]: {
      enabled: boolean
      sound: boolean
      priority: NotificationPriority[]
    }
  }
  types: {
    [key in NotificationType]?: {
      enabled: boolean
      channel: NotificationChannel[]
    }
  }
  frequency: {
    digest: boolean
    digestInterval: number // 分钟
    maxNotificationsPerHour: number
    maxNotificationsPerDay: number
  }
  appearance: {
    theme: 'light' | 'dark' | 'system'
    position: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'
    animation: boolean
    duration: number // 毫秒
    maxVisible: number
  }
  privacy: {
    showContent: boolean
    showPreview: boolean
    showSender: boolean
    allowReadReceipts: boolean
  }
}

// 通知查询请求接口
export interface NotificationRequest {
  pageNumber?: number
  pageSize?: number
  type?: NotificationType
  priority?: NotificationPriority
  status?: NotificationStatus
  channel?: NotificationChannel
  startDate?: string
  endDate?: string
  tags?: string[]
  relatedType?: string
  relatedId?: string
  searchTerm?: string
  sortBy?: 'createdAt' | 'updatedAt' | 'priority' | 'status'
  sortOrder?: 'asc' | 'desc'
}

// 通知查询响应接口
export interface NotificationResponse {
  items: Notification[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  unreadCount: number
  readCount: number
  totalCountByStatus: Record<NotificationStatus, number>
  totalCountByType: Record<NotificationType, number>
}

// 通知统计信息接口
export interface NotificationStatistics {
  totalCount: number
  unreadCount: number
  readCount: number
  archivedCount: number
  deletedCount: number
  countsByType: Record<NotificationType, number>
  countsByPriority: Record<NotificationPriority, number>
  countsByChannel: Record<NotificationChannel, number>
  recentNotifications: Notification[]
  topUsers: Array<{
    userId: string
    username: string
    notificationCount: number
  }>
  dailyStats: Array<{
    date: string
    count: number
    unreadCount: number
  }>
}

// 通知WebSocket消息接口
export interface NotificationWebSocketMessage {
  type: 'notification' | 'notification_update' | 'notification_delete' | 'notification_settings_update'
  data: Notification | NotificationSettings
  timestamp: string
}

// 通知创建请求接口
export interface CreateNotificationRequest {
  userId?: string // 如果为空则发送给所有用户
  type: NotificationType
  title: string
  message: string
  description?: string
  priority: NotificationPriority
  channel: NotificationChannel[]
  data?: Record<string, any>
  actions?: NotificationAction[]
  relatedId?: string
  relatedType?: string
  imageUrl?: string
  videoUrl?: string
  documentUrl?: string
  expiresAt?: string
  tags?: string[]
  metadata?: Record<string, any>
}

// 通知批量操作请求接口
export interface BatchNotificationRequest {
  notificationIds: string[]
  operation: 'mark_read' | 'mark_unread' | 'archive' | 'delete'
}

// 通知设置更新请求接口
export interface UpdateNotificationSettingsRequest {
  enableNotifications?: boolean
  enableSound?: boolean
  enableDesktopNotifications?: boolean
  enableEmailNotifications?: boolean
  enablePushNotifications?: boolean
  enableSMSNotifications?: boolean
  quietHours?: {
    enabled?: boolean
    startTime?: string
    endTime?: string
  }
  channels?: Partial<{
    [key in NotificationChannel]: {
      enabled?: boolean
      sound?: boolean
      priority?: NotificationPriority[]
    }
  }>
  types?: Partial<{
    [key in NotificationType]?: {
      enabled?: boolean
      channel?: NotificationChannel[]
    }
  }>
  frequency?: {
    digest?: boolean
    digestInterval?: number
    maxNotificationsPerHour?: number
    maxNotificationsPerDay?: number
  }
  appearance?: {
    theme?: 'light' | 'dark' | 'system'
    position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'
    animation?: boolean
    duration?: number
    maxVisible?: number
  }
  privacy?: {
    showContent?: boolean
    showPreview?: boolean
    showSender?: boolean
    allowReadReceipts?: boolean
  }
}

// 通知状态接口
export interface NotificationState {
  notifications: Notification[]
  totalCount: number
  unreadCount: number
  readCount: number
  loading: boolean
  error: string | null
  settings: NotificationSettings | null
  websocketConnected: boolean
  filters: NotificationRequest
  currentPage: number
  pageSize: number
  hasMore: boolean
}

// 通知组件Props接口
export interface NotificationListProps {
  maxItems?: number
  showActions?: boolean
  showFilters?: boolean
  showSearch?: boolean
  showPagination?: boolean
  autoRefresh?: boolean
  refreshInterval?: number
  defaultFilters?: Partial<NotificationRequest>
  onNotificationClick?: (notification: Notification) => void
  onNotificationRead?: (notification: Notification) => void
  onNotificationDelete?: (notification: Notification) => void
  onNotificationAction?: (action: NotificationAction, notification: Notification) => void
}

export interface NotificationItemProps {
  notification: Notification
  showActions?: boolean
  showImage?: boolean
  compact?: boolean
  showTimestamp?: boolean
  showPriority?: boolean
  onClick?: (notification: Notification) => void
  onRead?: (notification: Notification) => void
  onDelete?: (notification: Notification) => void
  onAction?: (action: NotificationAction, notification: Notification) => void
}

export interface NotificationSettingsProps {
  settings: NotificationSettings
  onSave?: (settings: NotificationSettings) => void
  onCancel?: () => void
  onTest?: (channel: NotificationChannel) => void
}

// 通知组件事件接口
export interface NotificationListEmits {
  (e: 'notification-click', notification: Notification): void
  (e: 'notification-read', notification: Notification): void
  (e: 'notification-delete', notification: Notification): void
  (e: 'notification-action', action: NotificationAction, notification: Notification): void
  (e: 'refresh'): void
  (e: 'load-more'): void
  (e: 'filters-change', filters: NotificationRequest): void
}

export interface NotificationItemEmits {
  (e: 'click', notification: Notification): void
  (e: 'read', notification: Notification): void
  (e: 'delete', notification: Notification): void
  (e: 'action', action: NotificationAction, notification: Notification): void
}

export interface NotificationSettingsEmits {
  (e: 'save', settings: NotificationSettings): void
  (e: 'cancel'): void
  (e: 'test', channel: NotificationChannel): void
}

// 默认通知设置
export const DEFAULT_NOTIFICATION_SETTINGS: NotificationSettings = {
  userId: '',
  enableNotifications: true,
  enableSound: true,
  enableDesktopNotifications: true,
  enableEmailNotifications: true,
  enablePushNotifications: true,
  enableSMSNotifications: false,
  quietHours: {
    enabled: false,
    startTime: '22:00',
    endTime: '08:00'
  },
  channels: {
    [NotificationChannel.InApp]: {
      enabled: true,
      sound: true,
      priority: [NotificationPriority.High, NotificationPriority.Urgent, NotificationPriority.Normal]
    },
    [NotificationChannel.Email]: {
      enabled: true,
      sound: false,
      priority: [NotificationPriority.High, NotificationPriority.Urgent]
    },
    [NotificationChannel.Push]: {
      enabled: true,
      sound: true,
      priority: [NotificationPriority.High, NotificationPriority.Urgent]
    },
    [NotificationChannel.SMS]: {
      enabled: false,
      sound: false,
      priority: [NotificationPriority.Urgent]
    }
  },
  types: {
    [NotificationType.Info]: {
      enabled: true,
      channel: [NotificationChannel.InApp]
    },
    [NotificationType.Success]: {
      enabled: true,
      channel: [NotificationChannel.InApp]
    },
    [NotificationType.Warning]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email]
    },
    [NotificationType.Error]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Push]
    },
    [NotificationType.System]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email]
    },
    [NotificationType.User]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email]
    },
    [NotificationType.Security]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Push, NotificationChannel.SMS]
    },
    [NotificationType.Update]: {
      enabled: true,
      channel: [NotificationChannel.InApp, NotificationChannel.Email]
    }
  },
  frequency: {
    digest: false,
    digestInterval: 60,
    maxNotificationsPerHour: 50,
    maxNotificationsPerDay: 200
  },
  appearance: {
    theme: 'system',
    position: 'top-right',
    animation: true,
    duration: 5000,
    maxVisible: 5
  },
  privacy: {
    showContent: true,
    showPreview: true,
    showSender: true,
    allowReadReceipts: true
  }
}

// 通知类型选项
export const NOTIFICATION_TYPE_OPTIONS = [
  { value: NotificationType.Info, label: '信息', icon: 'info', color: 'blue' },
  { value: NotificationType.Success, label: '成功', icon: 'check', color: 'green' },
  { value: NotificationType.Warning, label: '警告', icon: 'warning', color: 'yellow' },
  { value: NotificationType.Error, label: '错误', icon: 'error', color: 'red' },
  { value: NotificationType.System, label: '系统', icon: 'system', color: 'gray' },
  { value: NotificationType.User, label: '用户', icon: 'user', color: 'purple' },
  { value: NotificationType.Security, label: '安全', icon: 'shield', color: 'orange' },
  { value: NotificationType.Update, label: '更新', icon: 'update', color: 'indigo' }
] as const

// 通知优先级选项
export const NOTIFICATION_PRIORITY_OPTIONS = [
  { value: NotificationPriority.Low, label: '低', color: 'gray', weight: 100 },
  { value: NotificationPriority.Normal, label: '普通', color: 'blue', weight: 200 },
  { value: NotificationPriority.High, label: '高', color: 'orange', weight: 300 },
  { value: NotificationPriority.Urgent, label: '紧急', color: 'red', weight: 400 }
] as const

// 通知状态选项
export const NOTIFICATION_STATUS_OPTIONS = [
  { value: NotificationStatus.Unread, label: '未读', color: 'red' },
  { value: NotificationStatus.Read, label: '已读', color: 'green' },
  { value: NotificationStatus.Archived, label: '已归档', color: 'gray' },
  { value: NotificationStatus.Deleted, label: '已删除', color: 'gray' }
] as const

// 通知渠道选项
export const NOTIFICATION_CHANNEL_OPTIONS = [
  { value: NotificationChannel.InApp, label: '应用内', icon: 'app', description: '在应用内显示通知' },
  { value: NotificationChannel.Email, label: '邮件', icon: 'email', description: '发送邮件通知' },
  { value: NotificationChannel.Push, label: '推送', icon: 'push', description: '发送推送通知' },
  { value: NotificationChannel.SMS, label: '短信', icon: 'sms', description: '发送短信通知' }
] as const

// 通知动作类型选项
export const NOTIFICATION_ACTION_TYPE_OPTIONS = [
  { value: NotificationActionType.Link, label: '链接', icon: 'link' },
  { value: NotificationActionType.Button, label: '按钮', icon: 'button' },
  { value: NotificationActionType.Modal, label: '弹窗', icon: 'modal' },
  { value: NotificationActionType.ApiCall, label: 'API调用', icon: 'api' }
] as const

// 通知页面大小选项
export const NOTIFICATION_PAGE_SIZE_OPTIONS = [
  { value: 10, label: '10 条/页' },
  { value: 20, label: '20 条/页' },
  { value: 50, label: '50 条/页' },
  { value: 100, label: '100 条/页' }
] as const

// 通知排序选项
export const NOTIFICATION_SORT_OPTIONS = [
  { value: 'createdAt', label: '创建时间' },
  { value: 'updatedAt', label: '更新时间' },
  { value: 'priority', label: '优先级' },
  { value: 'status', label: '状态' }
] as const

// 通知位置选项
export const NOTIFICATION_POSITION_OPTIONS = [
  { value: 'top-right', label: '右上角' },
  { value: 'top-left', label: '左上角' },
  { value: 'bottom-right', label: '右下角' },
  { value: 'bottom-left', label: '左下角' }
] as const

// 通知主题选项
export const NOTIFICATION_THEME_OPTIONS = [
  { value: 'light', label: '浅色' },
  { value: 'dark', label: '深色' },
  { value: 'system', label: '跟随系统' }
] as const