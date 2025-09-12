/**
 * 消息系统类型定义
 * 
 * 本文件定义了消息系统前端所需的完整类型定义
 * 与后端API接口保持一致，支持Vue 3 + TypeScript开发
 */

// 消息类型枚举（数值型，与后端对应）
export enum MessageType {
  TEXT = 0,
  IMAGE = 1,
  FILE = 2,
  AUDIO = 3,
  VIDEO = 4,
  LOCATION = 5,
  SYSTEM = 6,
  NOTICE = 7
}

// 消息状态枚举（数值型，与后端对应）
export enum MessageStatus {
  DRAFT = 0,
  SENDING = 1,
  SENT = 2,
  DELIVERED = 3,
  READ = 4,
  FAILED = 5,
  DELETED = 6,
  RECALLED = 7
}

// 消息优先级枚举（数值型，与后端对应）
export enum MessagePriority {
  LOW = 0,
  NORMAL = 1,
  HIGH = 2,
  URGENT = 3
}

// 会话状态枚举（数值型，与后端对应）
export enum ConversationStatus {
  ACTIVE = 0,
  ARCHIVED = 1,
  MUTED = 2,
  BLOCKED = 3,
  DELETED = 4
}

// 会话类型枚举（数值型，与后端对应）
export enum ConversationType {
  SINGLE = 0,
  GROUP = 1,
  SYSTEM = 2,
  ANNOUNCEMENT = 3
}

// 附件类型枚举（数值型，与后端对应）
export enum AttachmentType {
  IMAGE = 0,
  VIDEO = 1,
  AUDIO = 2,
  DOCUMENT = 3,
  ARCHIVE = 4,
  OTHER = 5
}

// 附件状态枚举（数值型，与后端对应）
export enum AttachmentStatus {
  UPLOADING = 0,
  UPLOADED = 1,
  FAILED = 2,
  DELETED = 3,
  PROCESSING = 4
}

// 草稿状态枚举（数值型，与后端对应）
export enum DraftStatus {
  DRAFT = 0,
  SCHEDULED = 1,
  SENT = 2,
  CANCELLED = 3
}

// 草稿类型枚举（数值型，与后端对应）
export enum DraftType {
  MESSAGE = 0,
  REPLY = 1,
  FORWARD = 2,
  ANNOUNCEMENT = 3
}

// 消息排序枚举（数值型，与后端对应）
export enum MessageSort {
  CREATED_AT_DESC = 0,
  CREATED_AT_ASC = 1,
  PRIORITY_DESC = 2,
  PRIORITY_ASC = 3,
  TYPE = 4,
  STATUS = 5
}

// 会话排序枚举（数值型，与后端对应）
export enum ConversationSort {
  LAST_MESSAGE_DESC = 0,
  LAST_MESSAGE_ASC = 1,
  CREATED_AT_DESC = 2,
  CREATED_AT_ASC = 3,
  NAME_ASC = 4,
  NAME_DESC = 5,
  PARTICIPANT_COUNT = 6
}


// 草稿状态枚举
export enum DraftStatus {
  DRAFT = 'DRAFT',
  SENT = 'SENT',
  CANCELLED = 'CANCELLED',
  EXPIRED = 'EXPIRED'
}

// 消息操作类型枚举
export enum MessageOperation {
  MARK_AS_READ = 'MARK_AS_READ',
  MARK_AS_UNREAD = 'MARK_AS_UNREAD',
  DELETE = 'DELETE',
  ARCHIVE = 'ARCHIVE',
  RESTORE = 'RESTORE',
  PIN = 'PIN',
  UNPIN = 'UNPIN',
  MUTE = 'MUTE',
  UNMUTE = 'UNMUTE'
}

// 消息排序枚举
export enum MessageSort {
  CREATED_AT_DESC = 'CREATED_AT_DESC',
  CREATED_AT_ASC = 'CREATED_AT_ASC',
  UPDATED_AT_DESC = 'UPDATED_AT_DESC',
  UPDATED_AT_ASC = 'UPDATED_AT_ASC',
  PRIORITY_DESC = 'PRIORITY_DESC',
  PRIORITY_ASC = 'PRIORITY_ASC',
  READ_AT_DESC = 'READ_AT_DESC',
  READ_AT_ASC = 'READ_AT_ASC'
}

// 导出格式枚举
export enum ExportFormat {
  JSON = 'JSON',
  CSV = 'CSV',
  EXCEL = 'EXCEL',
  PDF = 'PDF'
}

// 消息附件接口
export interface MessageAttachment {
  id: string
  messageId: string
  fileName: string
  fileSize: number
  fileType: string
  attachmentType: AttachmentType
  filePath?: string
  downloadUrl?: string
  thumbnailUrl?: string
  status: AttachmentStatus
  uploadedAt: string
  createdAt: string
  updatedAt: string
}

// 草稿接口
export interface Draft {
  id: string
  conversationId?: string
  type: DraftType
  content: string
  messageType: MessageType
  attachments: MessageAttachment[]
  replyToMessageId?: string
  scheduledSend?: ScheduledSend
  status: DraftStatus
  title?: string
  recipients?: string[]
  tags?: string[]
  createdAt: string
  updatedAt: string
  expiresAt?: string
}

// 定时发送配置接口
export interface ScheduledSend {
  sendAt: string
  timezone: string
  isRecurring?: boolean
  recurringPattern?: string
  nextSendAt?: string
}

// 会话参与者接口
export interface ConversationParticipant {
  id: string
  conversationId: string
  userId: string
  userName: string
  userAvatar?: string
  role: 'owner' | 'admin' | 'moderator' | 'member'
  joinedAt: string
  lastReadMessageId?: string
  lastReadAt?: string
  isMuted: boolean
  isPinned: boolean
  hasLeft: boolean
}

// 会话接口
export interface Conversation {
  id: string
  name?: string
  description?: string
  conversationType: ConversationType
  status: ConversationStatus
  participants: ConversationParticipant[]
  lastMessage?: Message
  unreadCount: number
  totalMessages: number
  createdAt: string
  updatedAt: string
  avatar?: string
  isPinned: boolean
  isMuted: boolean
  customSettings?: Record<string, any>
}

// 消息基础接口
export interface Message {
  id: string
  conversationId: string
  senderId: string
  senderName: string
  senderAvatar?: string
  content: string
  messageType: MessageType
  status: MessageStatus
  priority: MessagePriority
  replyToMessageId?: string
  replyToMessage?: Message
  attachments: MessageAttachment[]
  isEdited: boolean
  editedAt?: string
  isForwarded: boolean
  forwardedFrom?: string
  isPinned: boolean
  readReceipts: MessageReadReceipt[]
  reactions: MessageReaction[]
  createdAt: string
  updatedAt: string
  deliveredAt?: string
  readAt?: string
  deletedAt?: string
}

// 消息已读回执接口
export interface MessageReadReceipt {
  id: string
  messageId: string
  userId: string
  userName: string
  readAt: string
  deviceInfo?: string
}

// 消息反应接口
export interface MessageReaction {
  id: string
  messageId: string
  userId: string
  userName: string
  emoji: string
  createdAt: string
}

// 附件接口
export interface Attachment {
  id: string
  fileName: string
  fileSize: number
  fileType: string
  attachmentType: AttachmentType
  filePath?: string
  downloadUrl?: string
  thumbnailUrl?: string
  status: AttachmentStatus
  uploadedBy: string
  uploadedAt: string
  metadata?: Record<string, any>
}

// 上传进度接口
export interface UploadProgress {
  id: string
  fileName: string
  progress: number
  speed: number
  uploadedBytes: number
  totalBytes: number
  status: 'uploading' | 'completed' | 'failed' | 'cancelled'
  error?: string
  startTime: string
  estimatedTimeRemaining?: number
}

// 下载信息接口
export interface DownloadInfo {
  id: string
  fileName: string
  downloadUrl: string
  fileSize: number
  fileType: string
  downloadCount: number
  lastDownloadedAt?: string
  expiresAt?: string
}

// 消息筛选条件接口
export interface MessageFilter {
  conversationId?: string
  senderId?: string
  messageType?: MessageType
  messageStatus?: MessageStatus
  priority?: MessagePriority
  hasAttachments?: boolean
  isEdited?: boolean
  isForwarded?: boolean
  isPinned?: boolean
  startDate?: string
  endDate?: string
  search?: string
  sortBy: MessageSort
  page: number
  pageSize: number
  includeAttachments?: boolean
  includeReactions?: boolean
  includeReadReceipts?: boolean
  currentUserId?: string
}

// 消息排序选项接口
export interface MessageSortOptions {
  field: 'createdAt' | 'updatedAt' | 'priority' | 'type' | 'status'
  order: 'asc' | 'desc'
}

// 消息分页接口
export interface MessagePagination {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

// 会话筛选条件接口
export interface ConversationFilter {
  conversationType?: ConversationType
  status?: ConversationStatus
  participantId?: string
  isPinned?: boolean
  isMuted?: boolean
  hasUnreadMessages?: boolean
  name?: string
  startDate?: string
  endDate?: string
  sortBy: ConversationSort
  page: number
  pageSize: number
  includeParticipants?: boolean
  includeLastMessage?: boolean
  currentUserId?: string
}

// 消息搜索选项接口
export interface MessageSearchOptions {
  keyword: string
  conversationId?: string
  senderId?: string
  messageType?: MessageType
  startDate?: string
  endDate?: string
  searchIn: 'content' | 'sender' | 'attachments' | 'all'
  sortBy: MessageSort
  page: number
  pageSize: number
  includeDeleted?: boolean
  includeAttachments?: boolean
}

// 创建消息请求接口
export interface CreateMessageRequest {
  conversationId: string
  content: string
  messageType: MessageType
  replyToMessageId?: string
  priority?: MessagePriority
  attachments?: string[]
  scheduledSend?: ScheduledSend
  tags?: string[]
}

// 更新消息请求接口
export interface UpdateMessageRequest {
  content?: string
  priority?: MessagePriority
  isPinned?: boolean
  tags?: string[]
}

// 发送消息请求接口
export interface SendMessageRequest {
  conversationId: string
  content: string
  messageType: MessageType
  attachments?: File[]
  replyToMessageId?: string
  priority?: MessagePriority
}

// 消息响应接口
export interface MessageResponse {
  id: string
  conversationId: string
  senderId: string
  senderName: string
  senderAvatar?: string
  content: string
  messageType: MessageType
  status: MessageStatus
  priority: MessagePriority
  replyToMessageId?: string
  attachments: MessageAttachment[]
  isEdited: boolean
  editedAt?: string
  isForwarded: boolean
  isPinned: boolean
  readCount: number
  reactionCount: number
  createdAt: string
  updatedAt: string
}

// 消息列表响应接口
export interface MessageListResponse {
  messages: MessageResponse[]
  pagination: MessagePagination
  hasMore: boolean
  nextCursor?: string
}

// 会话响应接口
export interface ConversationResponse {
  id: string
  name?: string
  description?: string
  conversationType: ConversationType
  status: ConversationStatus
  participantCount: number
  lastMessage?: MessageResponse
  unreadCount: number
  totalMessages: number
  createdAt: string
  updatedAt: string
  avatar?: string
  isPinned: boolean
  isMuted: boolean
}

// 会话列表响应接口
export interface ConversationListResponse {
  conversations: ConversationResponse[]
  pagination: MessagePagination
  hasMore: boolean
  nextCursor?: string
}

// 消息统计接口
export interface MessageStats {
  totalMessages: number
  unreadMessages: number
  sentMessages: number
  receivedMessages: number
  deletedMessages: number
  failedMessages: number
  messagesByType: Record<MessageType, number>
  messagesByStatus: Record<MessageStatus, number>
  messagesByPriority: Record<MessagePriority, number>
  averageMessagesPerDay: number
  mostActiveDay?: string
  leastActiveDay?: string
}

// 会话统计接口
export interface ConversationStats {
  totalConversations: number
  activeConversations: number
  archivedConversations: number
  groupConversations: number
  singleConversations: number
  totalParticipants: number
  averageParticipantsPerConversation: number
  messagesPerConversation: number
  mostActiveConversation?: ConversationResponse
  conversationByType: Record<ConversationType, number>
  conversationByStatus: Record<ConversationStatus, number>
}

// 附件统计接口
export interface AttachmentStats {
  totalAttachments: number
  totalSize: number
  attachmentsByType: Record<AttachmentType, number>
  storageUsage: {
    used: number
    available: number
    total: number
    percentage: number
  }
  averageAttachmentSize: number
  largestAttachment?: Attachment
  mostDownloadedAttachment?: Attachment
  recentUploads: Attachment[]
}

// 消息权限接口
export interface MessagePermissions {
  canSendMessage: boolean
  canEditMessage: boolean
  canDeleteMessage: boolean
  canRecallMessage: boolean
  canForwardMessage: boolean
  canPinMessage: boolean
  canReactToMessage: boolean
  canViewReadReceipts: boolean
  canUploadAttachments: boolean
  canDownloadAttachments: boolean
  canScheduleMessages: boolean
  canCreateConversation: boolean
  canEditConversation: boolean
  canDeleteConversation: boolean
  canAddParticipants: boolean
  canRemoveParticipants: boolean
  canManageParticipants: boolean
}

// 消息通知设置接口
export interface MessageNotificationSettings {
  onNewMessage: boolean
  onReply: boolean
  onMention: boolean
  onReaction: boolean
  onAttachmentUpload: boolean
  onMessageEdit: boolean
  onMessageDelete: boolean
  onConversationUpdate: boolean
  emailNotifications: boolean
  pushNotifications: boolean
  soundEnabled: boolean
  vibrationEnabled: boolean
  mutedConversations: string[]
  mutedUsers: string[]
  quietHours?: {
    enabled: boolean
    startTime: string
    endTime: string
    daysOfWeek: number[]
  }
}

// 消息通知接口
export interface MessageNotification {
  id: string
  type: 'new_message' | 'reply' | 'mention' | 'reaction' | 'system'
  messageId?: string
  conversationId: string
  conversationName?: string
  triggeredBy: {
    userId: string
    userName: string
    userAvatar?: string
  }
  message: string
  data?: any
  isRead: boolean
  createdAt: string
  expiresAt?: string
}

// 消息搜索结果接口
export interface MessageSearchResult {
  id: string
  content: string
  highlightedContent: string
  conversationId: string
  conversationName?: string
  senderId: string
  senderName: string
  messageType: MessageType
  createdAt: string
  searchScore: number
  matchedKeywords: string[]
  attachments: MessageAttachment[]
}

// 消息导入导出接口
export interface MessageExportOptions {
  format: 'json' | 'csv' | 'excel' | 'txt'
  conversationId?: string
  messageType?: MessageType
  startDate?: string
  endDate?: string
  includeAttachments: boolean
  includeDeleted: boolean
  includeMetadata: boolean
}

export interface MessageImportData {
  messages: ImportMessage[]
  conversationId?: string
}

export interface ImportMessage {
  content: string
  messageType: MessageType
  senderId?: string
  senderName?: string
  createdAt?: string
  attachments?: ImportAttachment[]
}

export interface ImportAttachment {
  fileName: string
  fileSize: number
  fileType: string
  filePath?: string
}

// 消息批量操作接口
export interface MessageBatchOperation {
  messageIds: string[]
  operation: 'delete' | 'recall' | 'mark_read' | 'mark_unread' | 'pin' | 'unpin'
  reason?: string
  operatorId: string
}

export interface BatchOperationResult {
  totalCount: number
  successCount: number
  failedCount: number
  errorMessages: string[]
  successfulIds: string[]
  failedIds: string[]
}

// 消息实时事件接口
export interface MessageRealtimeEvent {
  type: 'message_sent' | 'message_updated' | 'message_deleted' | 'message_read' | 'reaction_added' | 'reaction_removed'
  messageId?: string
  conversationId: string
  userId: string
  data?: any
  timestamp: string
}

// 消息设置接口
export interface MessageSettings {
  maxMessageLength: number
  maxAttachmentSize: number
  maxAttachmentsPerMessage: number
  allowedAttachmentTypes: string[]
  enableMessageEdit: boolean
  enableMessageDelete: boolean
  enableMessageRecall: boolean
  enableMessageForward: boolean
  enableMessagePin: boolean
  enableReactions: boolean
  enableReadReceipts: boolean
  enableTypingIndicators: boolean
  enableOnlineStatus: boolean
  autoDeleteAfterDays?: number
  messageRetentionDays: number
  maxConversationParticipants: number
  enableScheduledMessages: boolean
  enableMessageSearch: boolean
}



// 批量操作消息请求
export interface BatchMessageOperationRequest {
  messageIds: string[]
  operation: MessageOperation
  reason?: string
}




// 消息分析数据
export interface MessageAnalytics {
  totalMessages: number
  totalAttachments: number
  averageMessageLength: number
  mostActiveUsers: Array<{
    userId: string
    userName: string
    messageCount: number
    attachmentCount: number
  }>
  mostUsedAttachmentTypes: Array<{
    type: AttachmentType
    count: number
    size: number
  }>
  messageTrends: Array<{
    date: string
    messageCount: number
    attachmentCount: number
    userCount: number
  }>
  conversationStats: Array<{
    conversationId: string
    messageCount: number
    participantCount: number
    lastActivity: string
  }>
}


// 消息实时状态
export interface MessageRealtimeStatus {
  messageId: string
  status: MessageStatus
  isRead: boolean
  readAt?: string
  isTyping: boolean
  typingSince?: string
  onlineUsers: string[]
}

// 消息队列任务
export interface MessageQueueTask {
  id: string
  taskId: string
  messageType: MessageType
  payload: any
  priority: MessagePriority
  status: 'pending' | 'processing' | 'completed' | 'failed'
  retryCount: number
  maxRetries: number
  scheduledAt?: string
  processedAt?: string
  errorMessage?: string
}


// 消息搜索范围枚举
export enum MessageSearchScope {
  ALL = 'ALL',
  SUBJECT = 'SUBJECT',
  CONTENT = 'CONTENT',
  TAG = 'TAG',
  SUBJECT_AND_CONTENT = 'SUBJECT_AND_CONTENT'
}

// 消息搜索排序枚举
export enum MessageSearchSort {
  RELEVANCE = 'RELEVANCE',
  CREATED_AT_DESC = 'CREATED_AT_DESC',
  CREATED_AT_ASC = 'CREATED_AT_ASC',
  PRIORITY_DESC = 'PRIORITY_DESC',
  PRIORITY_ASC = 'PRIORITY_ASC',
  UNREAD_FIRST = 'UNREAD_FIRST'
}

// 文件上传接口
export interface FileUploadRequest {
  file: File
  messageType?: MessageType
  attachmentType?: AttachmentType
  onProgress?: (progress: number) => void
  onSuccess?: (attachment: MessageAttachment) => void
  onError?: (error: string) => void
}

// 文件上传响应
export interface FileUploadResponse {
  success: boolean
  attachment?: MessageAttachment
  error?: string
  progress: number
}

// 批量文件上传接口
export interface BatchFileUploadRequest {
  files: File[]
  messageType?: MessageType
  attachmentType?: AttachmentType
  onProgress?: (progress: number, fileName: string) => void
  onSuccess?: (attachments: MessageAttachment[]) => void
  onError?: (errors: string[]) => void
}

// 消息导入导出接口
export interface MessageImportExport {
  importMessages: (data: MessageImportData) => Promise<boolean>
  exportMessages: (options: MessageExportOptions) => Promise<Blob>
  validateImportData: (data: any) => ValidationResult
}

// 导入验证结果
export interface ValidationResult {
  isValid: boolean
  errors: string[]
  warnings: string[]
  validCount: number
  invalidCount: number
}

// 会话管理接口
export interface ConversationManagement {
  id: string
  title: string
  description?: string
  conversationType: ConversationType
  participantIds: string[]
  participants: ConversationParticipant[]
  initialMessage?: string
  tag?: string
  isPinned: boolean
  isMuted: boolean
  isArchived: boolean
  createdAt: string
  updatedAt: string
  messageCount: number
  unreadCount: number
  lastActivity: string
}

// 创建会话请求
export interface CreateConversationRequest {
  title: string
  description?: string
  conversationType: ConversationType
  participantIds: string[]
  initialMessage?: string
  tag?: string
  isPinned?: boolean
  isMuted?: boolean
}

// 更新会话请求
export interface UpdateConversationRequest {
  title?: string
  description?: string
  tag?: string
  isPinned?: boolean
  isMuted?: boolean
  isArchived?: boolean
}


// 消息实时更新接口
export interface MessageRealtimeUpdate {
  type: 'message_created' | 'message_updated' | 'message_deleted' | 'message_read' | 'typing_start' | 'typing_stop'
  messageId: string
  conversationId?: string
  senderId: string
  data?: any
  timestamp: string
}


// 消息搜索请求
export interface MessageSearchRequest {
  keyword: string
  senderId?: string
  receiverId?: string
  messageType?: MessageType
  status?: MessageStatus
  priority?: MessagePriority
  conversationId?: string
  tag?: string
  hasAttachments?: boolean
  startDate?: string
  endDate?: string
  searchScope: MessageSearchScope
  sortBy: MessageSearchSort
  page: number
  pageSize: number
  includeSender?: boolean
  includeReceiver?: boolean
  includeAttachments?: boolean
  highlightMatches?: boolean
}

// 消息操作历史
export interface MessageOperationHistory {
  id: string
  messageId: string
  operation: MessageOperation
  userId: string
  userName: string
  reason?: string
  timestamp: string
  details?: any
}


// 消息分析接口
export interface MessageAnalysisMetrics {
  totalMessages: number
  averageResponseTime: number
  peakHours: Array<{
    hour: number
    messageCount: number
  }>
  topUsers: Array<{
    userId: string
    userName: string
    messageCount: number
  }>
  attachmentStats: {
    totalAttachments: number
    totalSize: number
    averageSize: number
    mostCommonTypes: Array<{
      type: AttachmentType
      count: number
    }>
  }
  conversationStats: {
    totalConversations: number
    averageMessagesPerConversation: number
    mostActiveConversations: Array<{
      conversationId: string
      messageCount: number
    }>
  }
}

// 消息模板
export interface MessageTemplate {
  id: string
  name: string
  subject: string
  content: string
  messageType: MessageType
  priority: MessagePriority
  variables: string[]
  isActive: boolean
  createdAt: string
  updatedAt: string
}

// 消息过滤规则
export interface MessageFilterRule {
  id: string
  name: string
  conditions: Array<{
    field: string
    operator: 'equals' | 'contains' | 'starts_with' | 'ends_with' | 'regex'
    value: string
  }>
  actions: Array<{
    type: 'mark_as_read' | 'move_to_folder' | 'delete' | 'forward' | 'reply'
    parameters?: any
  }>
  isActive: boolean
  priority: number
  createdAt: string
  updatedAt: string
}



// 重新导入分页结果类型
import type { PaginatedResult } from './index'

// 分页结果类型
export type PaginatedMessages = PaginatedResult<MessageResponse>
export type PaginatedConversations = PaginatedResult<ConversationResponse>
export type PaginatedMessageSearchResults = PaginatedResult<MessageSearchResult>

// ============ Pinia 状态管理相关类型 ============

// 消息状态管理接口
export interface MessageState {
  messages: Message[]
  conversations: Conversation[]
  drafts: MessageDraft[]
  currentConversation?: Conversation
  currentMessage?: Message
  messageStats: MessageStats
  isLoading: boolean
  error: string | null
  lastUpdated: string
  unreadCount: number
  totalMessages: number
  filters: MessageFilter
  searchResults: MessageSearchResult | null
  uploadingFiles: FileUploadResponse[]
  activeUploads: Set<string>
}

// 消息操作接口
export interface MessageActions {
  // 消息相关操作
  fetchMessages: (filter: MessageFilter) => Promise<void>
  fetchMessageById: (id: string) => Promise<Message>
  createMessage: (message: CreateMessageRequest) => Promise<Message>
  updateMessage: (id: string, message: UpdateMessageRequest) => Promise<Message>
  deleteMessage: (id: string) => Promise<void>
  markAsRead: (id: string) => Promise<void>
  markAsUnread: (id: string) => Promise<void>
  batchOperation: (request: BatchMessageOperationRequest) => Promise<void>
  
  // 会话相关操作
  fetchConversations: (filter: ConversationFilter) => Promise<void>
  fetchConversationById: (id: string) => Promise<Conversation>
  createConversation: (conversation: CreateConversationRequest) => Promise<Conversation>
  updateConversation: (id: string, conversation: UpdateConversationRequest) => Promise<Conversation>
  deleteConversation: (id: string) => Promise<void>
  setCurrentConversation: (conversation: Conversation | undefined) => void
  
  // 草稿相关操作
  fetchDrafts: (filter: MessageFilter) => Promise<void>
  fetchDraftById: (id: string) => Promise<MessageDraft>
  createDraft: (draft: CreateMessageDraftRequest) => Promise<MessageDraft>
  updateDraft: (id: string, draft: UpdateMessageDraftRequest) => Promise<MessageDraft>
  deleteDraft: (id: string) => Promise<void>
  sendDraft: (id: string) => Promise<Message>
  autoSaveDraft: (id: string, content: string) => Promise<void>
  
  // 附件相关操作
  uploadFile: (request: FileUploadRequest) => Promise<FileUploadResponse>
  uploadMultipleFiles: (request: BatchFileUploadRequest) => Promise<FileUploadResponse[]>
  deleteAttachment: (id: string) => Promise<void>
  downloadAttachment: (id: string) => Promise<Blob>
  
  // 搜索相关操作
  searchMessages: (request: MessageSearchRequest) => Promise<MessageSearchResult>
  clearSearchResults: () => void
  
  // 导入导出相关操作
  exportMessages: (options: MessageExportOptions) => Promise<Blob>
  importMessages: (data: MessageImportData) => Promise<ValidationResult>
  
  // 统计相关操作
  fetchMessageStats: () => Promise<void>
  fetchMessageAnalytics: (filters?: any) => Promise<MessageAnalysisMetrics>
  
  // 实时相关操作
  subscribeToMessageUpdates: (conversationId: string) => () => void
  unsubscribeFromMessageUpdates: (conversationId: string) => void
  handleMessageRealtimeUpdate: (update: MessageRealtimeUpdate) => void
  
  // 工具方法
  clearError: () => void
  resetState: () => void
  setLoading: (loading: boolean) => void
  setError: (error: string | null) => void
}

// 消息Getters接口
export interface MessageGetters {
  // 消息相关getters
  getMessageById: (id: string) => Message | undefined
  getMessagesByConversation: (conversationId: string) => Message[]
  getUnreadMessages: () => Message[]
  getMessagesByType: (type: MessageType) => Message[]
  getMessagesByStatus: (status: MessageStatus) => Message[]
  getMessagesByPriority: (priority: MessagePriority) => Message[]
  
  // 会话相关getters
  getConversationById: (id: string) => Conversation | undefined
  getConversationsByType: (type: ConversationType) => Conversation[]
  getArchivedConversations: () => Conversation[]
  getPinnedConversations: () => Conversation[]
  getMutedConversations: () => Conversation[]
  
  // 草稿相关getters
  getDraftById: (id: string) => MessageDraft | undefined
  getDraftsByAuthor: (authorId: string) => MessageDraft[]
  getDraftsByStatus: (status: DraftStatus) => MessageDraft[]
  getExpiredDrafts: () => MessageDraft[]
  getScheduledDrafts: () => MessageDraft[]
  
  // 附件相关getters
  getAttachmentById: (id: string) => MessageAttachment | undefined
  getAttachmentsByMessage: (messageId: string) => MessageAttachment[]
  getAttachmentsByType: (type: AttachmentType) => MessageAttachment[]
  
  // 统计相关getters
  getTotalMessages: () => number
  getUnreadCount: () => number
  getDraftCount: () => number
  getConversationCount: () => number
  getAttachmentCount: () => number
  
  // 过滤和搜索相关getters
  getFilteredMessages: () => Message[]
  getFilteredConversations: () => Conversation[]
  getFilteredDrafts: () => MessageDraft[]
  getSearchResults: () => Message[] | null
  
  // 权限相关getters
  canEditMessage: (message: Message) => boolean
  canDeleteMessage: (message: Message) => boolean
  canReplyToMessage: (message: Message) => boolean
  canForwardMessage: (message: Message) => boolean
  canEditConversation: (conversation: Conversation) => boolean
  canDeleteConversation: (conversation: Conversation) => boolean
  canAddParticipants: (conversation: Conversation) => boolean
  canRemoveParticipants: (conversation: Conversation) => boolean
  
  // 工具getters
  isLoading: () => boolean
  getError: () => string | null
  getLastUpdated: () => string | null
  hasUnreadMessages: () => boolean
  getUploadingFiles: () => FileUploadResponse[]
  isUploading: () => boolean
}

// Pinia Store类型
export type MessageStore = MessageState & MessageActions & MessageGetters

// 消息服务接口
export interface MessageService {
  // API调用方法
  api: {
    messages: {
      getAll: (filter: MessageFilter) => Promise<PaginatedMessages>
      getById: (id: string) => Promise<Message>
      create: (message: CreateMessageRequest) => Promise<Message>
      update: (id: string, message: UpdateMessageRequest) => Promise<Message>
      delete: (id: string) => Promise<void>
      markAsRead: (id: string) => Promise<void>
      markAsUnread: (id: string) => Promise<void>
      batchOperation: (request: BatchMessageOperationRequest) => Promise<void>
    }
    conversations: {
      getAll: (filter: ConversationFilter) => Promise<PaginatedConversations>
      getById: (id: string) => Promise<Conversation>
      create: (conversation: CreateConversationRequest) => Promise<Conversation>
      update: (id: string, conversation: UpdateConversationRequest) => Promise<Conversation>
      delete: (id: string) => Promise<void>
    }
    drafts: {
      getAll: (filter: MessageFilter) => Promise<PaginatedMessageDrafts>
      getById: (id: string) => Promise<MessageDraft>
      create: (draft: CreateMessageDraftRequest) => Promise<MessageDraft>
      update: (id: string, draft: UpdateMessageDraftRequest) => Promise<MessageDraft>
      delete: (id: string) => Promise<void>
      send: (id: string) => Promise<Message>
    }
    attachments: {
      upload: (file: File, metadata?: any) => Promise<MessageAttachment>
      delete: (id: string) => Promise<void>
      download: (id: string) => Promise<Blob>
    }
    search: {
      messages: (request: MessageSearchRequest) => Promise<MessageSearchResult>
    }
    export: {
      messages: (options: MessageExportOptions) => Promise<Blob>
    }
    import: {
      messages: (data: MessageImportData) => Promise<ValidationResult>
    }
    stats: {
      getMessageStats: () => Promise<MessageStats>
      getAnalytics: (filters?: any) => Promise<MessageAnalysisMetrics>
    }
  }
  
  // 实时通信方法
  realtime: {
    connect: () => Promise<void>
    disconnect: () => void
    subscribe: (event: string, callback: (data: any) => void) => () => void
    unsubscribe: (event: string, callback: (data: any) => void) => void
  }
  
  // 缓存方法
  cache: {
    get: (key: string) => any
    set: (key: string, value: any, ttl?: number) => void
    invalidate: (key: string) => void
    clear: () => void
  }
  
  // 错误处理
  error: {
    handle: (error: any) => void
    clear: () => void
    getLastError: () => string | null
  }
}