/**
 * 增强消息系统类型定义
 * 
 * 本文件扩展现有消息类型定义，添加新的功能特性支持
 * 包括虚拟滚动、无限滚动、实时更新、富文本编辑等功能
 */

import type { 
  Message, 
  MessageAttachment, 
  MessageReaction, 
  MessageType, 
  MessageStatus, 
  MessagePriority,
  User,
  Tag 
} from './message'

// 导出原有类型（保持向后兼容）
export * from './message'

// ==============================
// 增强的消息类型定义
// ==============================

// 扩展的消息类型枚举
export enum EnhancedMessageType {
  // 原有类型
  TEXT = 0,
  IMAGE = 1,
  FILE = 2,
  AUDIO = 3,
  VIDEO = 4,
  LOCATION = 5,
  SYSTEM = 6,
  NOTICE = 7,
  // 新增类型
  RICH_TEXT = 8,        // 富文本消息
  TEMPLATE = 9,         // 模板消息
  INTERACTIVE = 10,     // 交互式消息
  POLL = 11,           // 投票消息
  TASK = 12,           // 任务消息
  NOTIFICATION = 13,   // 通知消息
  BROADCAST = 14,      // 广播消息
  AUTO_REPLY = 15      // 自动回复
}

// 扩展的消息状态枚举
export enum EnhancedMessageStatus {
  // 原有状态
  DRAFT = 0,
  SENDING = 1,
  SENT = 2,
  DELIVERED = 3,
  READ = 4,
  FAILED = 5,
  DELETED = 6,
  RECALLED = 7,
  // 新增状态
  SCHEDULED = 8,        // 定时发送
  PROCESSING = 9,      // 处理中
  ARCHIVED = 10,       // 已归档
  SPAM = 11,          // 垃圾消息
  FLAGGED = 12,       // 标记消息
  APPROVED = 13,      // 已审核
  REJECTED = 14       // 已拒绝
}

// 实时连接状态枚举
export enum RealtimeConnectionStatus {
  DISCONNECTED = 'disconnected',
  CONNECTING = 'connecting',
  CONNECTED = 'connected',
  ERROR = 'error',
  RECONNECTING = 'reconnecting'
}

// 消息编辑模式枚举
export enum MessageEditMode {
  NONE = 'none',
  INLINE = 'inline',
  FULL = 'full'
}

// 滚动模式枚举
export enum ScrollMode {
  TRADITIONAL = 'traditional',
  VIRTUAL = 'virtual',
  INFINITE = 'infinite'
}

// ==============================
// 增强的接口定义
// ==============================

// 增强的用户接口
export interface EnhancedUser extends User {
  isOnline: boolean
  lastSeenAt?: string
  statusMessage?: string
  typingStatus: 'idle' | 'typing' | 'paused'
  permissions: UserPermission[]
  preferences: UserPreferences
}

// 用户权限枚举
export enum UserPermission {
  SEND_MESSAGE = 'send_message',
  EDIT_MESSAGE = 'edit_message',
  DELETE_MESSAGE = 'delete_message',
  PIN_MESSAGE = 'pin_message',
  REACT_TO_MESSAGE = 'react_to_message',
  FORWARD_MESSAGE = 'forward_message',
  REPLY_MESSAGE = 'reply_message',
  MANAGE_CONVERSATION = 'manage_conversation',
  BROADCAST_MESSAGE = 'broadcast_message',
  SCHEDULE_MESSAGE = 'schedule_message',
  USE_RICH_TEXT = 'use_rich_text',
  UPLOAD_FILES = 'upload_files',
  CREATE_POLLS = 'create_polls',
  MANAGE_TAGS = 'manage_tags'
}

// 用户偏好设置接口
export interface UserPreferences {
  theme: 'light' | 'dark' | 'auto'
  language: string
  timezone: string
  notifications: NotificationSettings
  privacy: PrivacySettings
  accessibility: AccessibilitySettings
  message: MessageSettings
}

// 通知设置接口
export interface NotificationSettings {
  enabled: boolean
  sound: boolean
  vibration: boolean
  desktop: boolean
  email: boolean
  push: boolean
  quietHours: {
    enabled: boolean
    startTime: string
    endTime: string
  }
  mentionOnly: boolean
}

// 隐私设置接口
export interface PrivacySettings {
  readReceipts: boolean
  lastSeen: boolean
  onlineStatus: boolean
  profilePhoto: boolean
  statusMessage: boolean
  activityStatus: boolean
}

// 无障碍设置接口
export interface AccessibilitySettings {
  fontSize: 'small' | 'medium' | 'large' | 'extra-large'
  highContrast: boolean
  reduceMotion: boolean
  screenReader: boolean
  keyboardNavigation: boolean
}

// 消息设置接口
export interface MessageSettings {
  autoSaveDraft: boolean
  autoSaveInterval: number
  enterToSend: boolean
  showTypingIndicators: boolean
  showReadReceipts: boolean
  enableRichText: boolean
  enableMentions: boolean
  enableEmojis: boolean
  enableAttachments: boolean
  maxAttachments: number
  maxFileSize: number
  defaultPriority: MessagePriority
}

// 增强的消息接口
export interface EnhancedMessage extends Message {
  // 实时相关
  isRealtime: boolean
  realtimeId?: string
  // 编辑相关
  editMode: MessageEditMode
  editHistory: MessageEditHistory[]
  // 交互相关
  isInteractive: boolean
  interactiveData?: InteractiveData
  // 任务相关
  isTask: boolean
  taskData?: TaskData
  // 投票相关
  isPoll: boolean
  pollData?: PollData
  // 模板相关
  isTemplate: boolean
  templateId?: string
  // 定时发送
  scheduledAt?: string
  scheduledTimezone?: string
  // 归档相关
  archivedAt?: string
  archivedBy?: string
  // 标签相关
  tags: Tag[]
  // 权限相关
  permissions: MessagePermission[]
  // 元数据
  metadata: Record<string, any>
  // 扩展字段
  canReply: boolean
  canForward: boolean
  canEdit: boolean
  canDelete: boolean
  canPin: boolean
  canReact: boolean
  canStar: boolean
  canReport: boolean
  canCopy: boolean
  canShare: boolean
  isStarred: boolean
  isPinned: boolean
  isFlagged: boolean
  isSpam: boolean
  isSender: boolean
  isRead: boolean
}

// 消息权限接口
export interface MessagePermission {
  userId: string
  permissions: UserPermission[]
  grantedAt: string
  grantedBy: string
  expiresAt?: string
}

// 消息编辑历史接口
export interface MessageEditHistory {
  id: string
  messageId: string
  oldContent: string
  newContent: string
  editedBy: string
  editedAt: string
  editReason?: string
}

// 交互式数据接口
export interface InteractiveData {
  type: 'button' | 'list' | 'menu' | 'form'
  elements: InteractiveElement[]
  actions: InteractiveAction[]
}

// 交互元素接口
export interface InteractiveElement {
  id: string
  type: string
  label: string
  value: any
  style?: Record<string, any>
  action?: InteractiveAction
}

// 交互动作接口
export interface InteractiveAction {
  type: 'reply' | 'open_url' | 'postback' | 'call' | 'share'
  payload: any
}

// 任务数据接口
export interface TaskData {
  id: string
  title: string
  description: string
  status: 'pending' | 'in_progress' | 'completed' | 'cancelled'
  priority: 'low' | 'medium' | 'high' | 'urgent'
  assignees: string[]
  dueDate?: string
  completedAt?: string
  completedBy?: string
  attachments: TaskAttachment[]
}

// 任务附件接口
export interface TaskAttachment {
  id: string
  name: string
  url: string
  type: string
  size: number
  uploadedBy: string
  uploadedAt: string
}

// 投票数据接口
export interface PollData {
  id: string
  question: string
  options: PollOption[]
  allowMultipleSelection: boolean
  anonymous: boolean
  expiresAt?: string
  totalVotes: number
  userVotes: string[]
}

// 投票选项接口
export interface PollOption {
  id: string
  text: string
  votes: number
  percentage: number
  selectedByUser: boolean
}

// 增强的附件接口
export interface EnhancedMessageAttachment extends MessageAttachment {
  // 上传进度
  uploadProgress: number
  uploadSpeed: number
  uploadStatus: 'uploading' | 'uploaded' | 'failed' | 'processing'
  // 预览相关
  previewUrl?: string
  thumbnailUrl?: string
  previewType?: 'image' | 'video' | 'audio' | 'document' | 'code'
  // 处理状态
  processingStatus?: 'pending' | 'processing' | 'completed' | 'failed'
  processingProgress?: number
  // 元数据
  metadata: AttachmentMetadata
  // 权限
  canDownload: boolean
  canPreview: boolean
  canDelete: boolean
  canShare: boolean
}

// 附件元数据接口
export interface AttachmentMetadata {
  // 图片元数据
  width?: number
  height?: number
  format?: string
  quality?: number
  // 视频元数据
  duration?: number
  framerate?: number
  bitrate?: number
  // 音频元数据
  sampleRate?: number
  channels?: number
  codec?: string
  // 文档元数据
  pageCount?: number
  author?: string
  title?: string
  // 通用元数据
  checksum?: string
  virusScanStatus?: 'pending' | 'scanning' | 'clean' | 'infected'
  compressionRatio?: number
}

// 增强的反应接口
export interface EnhancedMessageReaction extends MessageReaction {
  isCurrentUser: boolean
  userAvatar?: string
  userStatus?: 'online' | 'offline' | 'away' | 'busy'
  createdAt: string
  updatedAt?: string
}

// 增强的标签接口
export interface EnhancedTag extends Tag {
  usageCount: number
  createdBy: string
  createdAt: string
  color: string
  icon?: string
  description?: string
  isSystem: boolean
  isPublic: boolean
  permissions: TagPermission[]
}

// 标签权限接口
export interface TagPermission {
  userId: string
  canAssign: boolean
  canRemove: boolean
  canEdit: boolean
  canDelete: boolean
}

// 消息模板接口
export interface MessageTemplate {
  id: string
  name: string
  description?: string
  subject?: string
  content: string
  messageType: MessageType
  priority: MessagePriority
  tags: string[]
  variables: TemplateVariable[]
  isPublic: boolean
  isSystem: boolean
  createdBy: string
  createdAt: string
  updatedAt: string
  usageCount: number
  category?: string
}

// 模板变量接口
export interface TemplateVariable {
  name: string
  type: 'text' | 'number' | 'date' | 'boolean' | 'select'
  description?: string
  required: boolean
  defaultValue?: any
  options?: TemplateVariableOption[]
  validation?: TemplateVariableValidation
}

// 模板变量选项接口
export interface TemplateVariableOption {
  value: any
  label: string
  description?: string
}

// 模板变量验证接口
export interface TemplateVariableValidation {
  min?: number
  max?: number
  pattern?: string
  custom?: (value: any) => boolean
}

// ==============================
// 虚拟滚动相关类型
// ==============================

// 虚拟滚动项目接口
export interface VirtualScrollItem<T = any> {
  id: string
  data: T
  index: number
  height: number
  offsetY: number
  isVisible: boolean
}

// 虚拟滚动配置接口
export interface VirtualScrollConfig {
  itemHeight: number | ((item: any, index: number) => number)
  containerHeight: number
  overscan: number
  buffer: number
  dynamicHeight: boolean
  estimatedItemHeight: number
}

// 虚拟滚动状态接口
export interface VirtualScrollState {
  startIndex: number
  endIndex: number
  scrollTop: number
  isScrolling: boolean
  totalHeight: number
  visibleItems: VirtualScrollItem[]
}

// ==============================
// 无限滚动相关类型
// ==============================

// 无限滚动配置接口
export interface InfiniteScrollConfig {
  threshold: number
  distance: number
  direction: 'vertical' | 'horizontal'
  immediate: boolean
  debounce: number
  disabled: boolean
  pageSize: number
}

// 无限滚动状态接口
export interface InfiniteScrollState {
  isLoading: boolean
  hasMore: boolean
  page: number
  error: Error | null
  totalItems: number
  loadedItems: any[]
}

// 分页信息接口
export interface PaginationInfo {
  page: number
  pageSize: number
  total: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

// ==============================
// 实时消息相关类型
// ==============================

// 实时消息配置接口
export interface RealtimeMessageConfig {
  url: string
  reconnectInterval: number
  maxReconnectAttempts: number
  heartbeatInterval: number
  debug: boolean
  enableCompression: boolean
  enableEncryption: boolean
}

// 实时消息状态接口
export interface RealtimeMessageState {
  isConnected: boolean
  connectionError: Error | null
  connectionStatus: RealtimeConnectionStatus
  reconnectAttempts: number
  lastConnectedAt?: string
  latency: number
}

// WebSocket消息接口
export interface WebSocketMessage {
  type: string
  payload: any
  timestamp: string
  messageId?: string
  userId?: string
}

// 实时消息事件接口
export interface RealtimeMessageEvent {
  type: 'message' | 'reaction' | 'typing' | 'read_receipt' | 'status_change'
  data: any
  timestamp: string
}

// ==============================
// 富文本编辑器相关类型
// ==============================

// 富文本编辑器配置接口
export interface RichTextEditorConfig {
  enableToolbar: boolean
  enableMentions: boolean
  enableEmojis: boolean
  enableAttachments: boolean
  enableFormatting: boolean
  enableLists: boolean
  enableLinks: boolean
  enableImages: boolean
  enableCode: boolean
  enableTables: boolean
  maxContentLength: number
  placeholder: string
  autoFocus: boolean
  spellCheck: boolean
}

// 编辑器状态接口
export interface EditorState {
  content: string
  plainText: string
  html: string
  selection: {
    start: number
    end: number
    text: string
  }
  formatting: {
    bold: boolean
    italic: boolean
    underline: boolean
    strikethrough: boolean
    unorderedList: boolean
    orderedList: boolean
    alignment: 'left' | 'center' | 'right' | 'justify'
  }
  wordCount: number
  characterCount: number
}

// 编辑器工具栏按钮接口
export interface EditorToolbarButton {
  id: string
  icon: string
  label: string
  action: string
  shortcut?: string
  isActive?: boolean
  isDisabled?: boolean
  tooltip?: string
}

// @提及建议接口
export interface MentionSuggestion {
  id: string
  name: string
  email: string
  avatar?: string
  isOnline: boolean
  lastSeenAt?: string
  score: number
}

// 表情符号接口
export interface Emoji {
  id: string
  name: string
  unicode: string
  shortcodes: string[]
  category: string
  sortOrder: number
}

// ==============================
// 表单数据相关类型
// ==============================

// 消息表单数据接口
export interface MessageFormData {
  id?: string
  mode: 'create' | 'reply' | 'forward' | 'edit'
  recipients: EnhancedUser[]
  subject?: string
  content: string
  messageType: MessageType
  priority: MessagePriority
  attachments: EnhancedMessageAttachment[]
  tags: string[]
  scheduledTime?: string
  scheduledTimezone?: string
  replyTo?: EnhancedMessage
  forwardFrom?: EnhancedMessage
  templateId?: string
  isDraft: boolean
  permissions: MessagePermission[]
}

// 表单验证错误接口
export interface FormValidationError {
  field: string
  message: string
  type: 'required' | 'format' | 'length' | 'size' | 'custom'
  value?: any
}

// 表单状态接口
export interface FormState {
  isSubmitting: boolean
  isSavingDraft: boolean
  isValid: boolean
  errors: FormValidationError[]
  hasUnsavedChanges: boolean
  lastSavedAt?: string
}

// ==============================
// 缓存相关类型
// ==============================

// 缓存配置接口
export interface CacheConfig {
  enabled: boolean
  ttl: number
  maxSize: number
  strategy: 'lru' | 'fifo' | 'lfu'
  compression: boolean
  encryption: boolean
}

// 缓存项接口
export interface CacheItem<T = any> {
  key: string
  value: T
  timestamp: number
  ttl: number
  hits: number
  size: number
}

// ==============================
// 性能监控相关类型
// ==============================

// 性能指标接口
export interface PerformanceMetrics {
  renderTime: number
  memoryUsage: number
  cpuUsage: number
  networkLatency: number
  frameRate: number
  bundleSize: number
}

// 监控配置接口
export interface MonitoringConfig {
  enabled: boolean
  sampleRate: number
  maxSamples: number
  reportInterval: number
  endpoints: string[]
}

// ==============================
// 工具函数类型
// ==============================

// 消息格式化函数接口
export interface MessageFormatters {
  formatTime: (timestamp: string) => string
  formatDate: (timestamp: string) => string
  formatFileSize: (bytes: number) => string
  formatContent: (content: string) => string
  formatUserName: (user: EnhancedUser) => string
}

// 消息验证函数接口
export interface MessageValidators {
  validateContent: (content: string) => boolean
  validateRecipients: (recipients: EnhancedUser[]) => boolean
  validateAttachments: (attachments: EnhancedMessageAttachment[]) => boolean
  validatePermissions: (permissions: MessagePermission[]) => boolean
}

// 消息工具函数接口
export interface MessageUtils {
  debounce: <T extends (...args: any[]) => any>(func: T, wait: number) => T
  throttle: <T extends (...args: any[]) => any>(func: T, limit: number) => T
  deepClone: <T>(obj: T) => T
  generateId: () => string
  sanitizeHtml: (html: string) => string
  extractMentions: (content: string) => string[]
  extractEmojis: (content: string) => string[]
  extractLinks: (content: string) => string[]
}

// ==============================
// 事件类型
// ==============================

// 消息事件接口
export interface MessageEvent {
  type: 'click' | 'select' | 'edit' | 'delete' | 'reply' | 'forward' | 'react' | 'pin' | 'star'
  message: EnhancedMessage
  timestamp: string
  userId?: string
  data?: any
}

// 滚动事件接口
export interface ScrollEvent {
  type: 'scroll' | 'scroll_end' | 'scroll_start'
  scrollTop: number
  scrollHeight: number
  clientHeight: number
  direction: 'up' | 'down'
  timestamp: string
}

// 实时事件接口
export interface RealtimeEvent {
  type: 'connect' | 'disconnect' | 'message' | 'typing' | 'read_receipt' | 'error'
  data: any
  timestamp: string
  connectionId?: string
}