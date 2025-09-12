// 消息组件类型定义
// 为所有消息组件提供完整的TypeScript类型支持

import type { Message, MessageAttachment, User, Tag } from '@/types/message'
import type { Ref } from 'vue'

// 基础消息组件Props类型
export interface BaseMessageProps {
  message: Message
  loading?: boolean
  disabled?: boolean
  compact?: boolean
  showAvatar?: boolean
  showActions?: boolean
  showStatus?: boolean
  showPriority?: boolean
  showAttachments?: boolean
  showTags?: boolean
  showDate?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageList Props
export interface MessageListProps {
  messages: Message[]
  loading?: boolean
  pagination?: {
    page: number
    pageSize: number
    total: number
    totalPages: number
  }
  selectable?: boolean
  selectedMessages?: string[]
  sortable?: boolean
  sortBy?: 'date' | 'sender' | 'priority' | 'status'
  sortOrder?: 'asc' | 'desc'
  virtualScroll?: boolean
  itemHeight?: number
  height?: string | number
  compact?: boolean
  showSearch?: boolean
  showFilters?: boolean
  showActions?: boolean
  enableBatchActions?: boolean
  enableDragAndDrop?: boolean
  enableRealTime?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageList Emits
export interface MessageListEmits {
  (e: 'message-select', message: Message): void
  (e: 'message-deselect', message: Message): void
  (e: 'message-delete', message: Message): void
  (e: 'message-archive', message: Message): void
  (e: 'message-mark-read', message: Message): void
  (e: 'message-mark-unread', message: Message): void
  (e: 'message-reply', message: Message): void
  (e: 'message-forward', message: Message): void
  (e: 'batch-delete', messages: Message[]): void
  (e: 'batch-archive', messages: Message[]): void
  (e: 'batch-mark-read', messages: Message[]): void
  (e: 'filter-change', filters: MessageFilters): void
  (e: 'search', query: string): void
  (e: 'sort-change', sortBy: string, sortOrder: string): void
  (e: 'page-change', page: number): void
  (e: 'page-size-change', pageSize: number): void
  (e: 'refresh'): void
  (e: 'load-more'): void
  (e: 'drag-start', message: Message): void
  (e: 'drag-end', message: Message): void
  (e: 'drop', targetMessage: Message, sourceMessage: Message): void
}

// MessageItem Props
export interface MessageItemProps extends BaseMessageProps {
  showPreview?: boolean
  showSender?: boolean
  showReceiver?: boolean
  showThreadCount?: boolean
  expandable?: boolean
  expanded?: boolean
  hoverable?: boolean
  clickable?: boolean
  selectionMode?: 'none' | 'single' | 'multiple'
  isSelected?: boolean
  avatarSize?: 'sm' | 'md' | 'lg'
  messageStyle?: 'default' | 'compact' | 'detailed'
}

// MessageItem Emits
export interface MessageItemEmits {
  (e: 'click', message: Message): void
  (e: 'double-click', message: Message): void
  (e: 'expand', message: Message): void
  (e: 'collapse', message: Message): void
  (e: 'reply', message: Message): void
  (e: 'forward', message: Message): void
  (e: 'delete', message: Message): void
  (e: 'archive', message: Message): void
  (e: 'mark-read', message: Message): void
  (e: 'mark-unread', message: Message): void
  (e: 'star', message: Message): void
  (e: 'unstar', message: Message): void
  (e: 'tag-add', message: Message, tag: Tag): void
  (e: 'tag-remove', message: Message, tag: Tag): void
  (e: 'attachment-click', message: Message, attachment: MessageAttachment): void
  (e: 'attachment-download', message: Message, attachment: MessageAttachment): void
  (e: 'attachment-preview', message: Message, attachment: MessageAttachment): void
}

// MessageForm Props
export interface MessageFormProps {
  mode?: 'create' | 'edit' | 'reply' | 'forward'
  message?: Message
  replyTo?: Message
  forwardFrom?: Message
  recipients?: User[]
  availableRecipients?: User[]
  enableRichText?: boolean
  enableAttachments?: boolean
  enableEmoji?: boolean
  enableTags?: boolean
  enablePriority?: boolean
  enableDraft?: boolean
  maxAttachments?: number
  maxFileSize?: number
  allowedFileTypes?: string[]
  placeholder?: string
  submitButtonText?: string
  cancelButtonText?: string
  showPreview?: boolean
  autoSave?: boolean
  autoSaveInterval?: number
  compact?: boolean
  disabled?: boolean
  loading?: boolean
}

// MessageForm Emits
export interface MessageFormEmits {
  (e: 'submit', formData: MessageFormData): void
  (e: 'cancel'): void
  (e: 'draft-save', draftData: MessageFormData): void
  (e: 'draft-load', draftData: MessageFormData): void
  (e: 'recipient-add', recipient: User): void
  (e: 'recipient-remove', recipient: User): void
  (e: 'attachment-add', attachment: File): void
  (e: 'attachment-remove', attachment: File): void
  (e: 'tag-add', tag: Tag): void
  (e: 'tag-remove', tag: Tag): void
  (e: 'priority-change', priority: string): void
  (e: 'content-change', content: string): void
  (e: 'preview-toggle'): void
  (e: 'emoji-select', emoji: string): void
  (e: 'validate', isValid: boolean, errors: string[]): void
}

// MessageConversation Props
export interface MessageConversationProps {
  conversationId: string
  messages: Message[]
  participants: User[]
  currentUser: User
  loading?: boolean
  typingUsers?: User[]
  onlineUsers?: string[]
  enableRealTime?: boolean
  enableThreads?: boolean
  enableReactions?: boolean
  enableEmoji?: boolean
  enableAttachments?: boolean
  enableVoiceMessages?: boolean
  enableVideoCalls?: boolean
  autoScrollToBottom?: boolean
  showParticipants?: boolean
  showOnlineStatus?: boolean
  showTypingIndicator?: boolean
  showMessageTime?: boolean
  showMessageStatus?: boolean
  maxHeight?: string | number
  compact?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageConversation Emits
export interface MessageConversationEmits {
  (e: 'message-send', messageData: MessageData): void
  (e: 'message-receive', message: Message): void
  (e: 'message-edit', message: Message, newContent: string): void
  (e: 'message-delete', message: Message): void
  (e: 'message-reaction', message: Message, reaction: string): void
  (e: 'typing-start', user: User): void
  (e: 'typing-stop', user: User): void
  (e: 'participant-add', participant: User): void
  (e: 'participant-remove', participant: User): void
  (e: 'participant-kick', participant: User): void
  (e: 'voice-call-start'): void
  (e: 'video-call-start'): void
  (e: 'scroll-to-bottom'): void
  (e: 'load-more'): void
  (e: 'refresh'): void
}

// MessageSearch Props
export interface MessageSearchProps {
  placeholder?: string
  initialValue?: string
  showSuggestions?: boolean
  showHistory?: boolean
  showAdvancedFilters?: boolean
  maxSuggestions?: number
  maxHistoryItems?: number
  debounceDelay?: number
  searchFields?: string[]
  searchInAttachments?: boolean
  enableRegex?: boolean
  enableFuzzySearch?: boolean
  compact?: boolean
  disabled?: boolean
  loading?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageSearch Emits
export interface MessageSearchEmits {
  (e: 'search', query: string, filters?: SearchFilters): void
  (e: 'search-change', query: string): void
  (e: 'search-clear'): void
  (e: 'suggestion-select', suggestion: SearchSuggestion): void
  (e: 'history-select', historyItem: SearchHistory): void
  (e: 'filter-change', filters: SearchFilters): void
  (e: 'export-results', results: Message[]): void
  (e: 'save-search', searchData: SearchData): void
}

// MessageFilter Props
export interface MessageFilterProps {
  title?: string
  searchPlaceholder?: string
  showQuickFilters?: boolean
  showTagFilter?: boolean
  compactMode?: boolean
  initiallyExpanded?: boolean
  debounceDelay?: number
  availableUsers?: User[]
  availableTags?: Tag[]
  enablePresets?: boolean
  enableDateRanges?: boolean
  maxActiveFilters?: number
  theme?: 'light' | 'dark' | 'auto'
}

// MessageFilter Emits
export interface MessageFilterEmits {
  (e: 'filter-change', filters: MessageFilters): void
  (e: 'filter-reset'): void
  (e: 'preset-save', preset: FilterPreset): void
  (e: 'preset-delete', presetId: string): void
  (e: 'preset-apply', preset: FilterPreset): void
}

// MessageAttachmentUpload Props
export interface MessageAttachmentUploadProps {
  maxFiles?: number
  maxFileSize?: number
  allowedFileTypes?: string[]
  enableDragAndDrop?: boolean
  enablePaste?: boolean
  enableCamera?: boolean
  enableCloudUpload?: boolean
  showPreview?: boolean
  showProgress?: boolean
  showFileList?: boolean
  autoUpload?: boolean
  chunkedUpload?: boolean
  compression?: boolean
  virusScan?: boolean
  compact?: boolean
  disabled?: boolean
  loading?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageAttachmentUpload Emits
export interface MessageAttachmentUploadEmits {
  (e: 'file-select', files: File[]): void
  (e: 'file-upload', file: File): void
  (e: 'file-progress', file: File, progress: number): void
  (e: 'file-complete', file: File, response: any): void
  (e: 'file-error', file: File, error: Error): void
  (e: 'file-cancel', file: File): void
  (e: 'file-remove', file: File): void
  (e: 'upload-start'): void
  (e: 'upload-complete', results: any[]): void
  (e: 'upload-error', error: Error): void
  (e: 'drag-enter'): void
  (e: 'drag-leave'): void
  (e: 'drop', files: File[]): void
}

// MessageAttachmentDisplay Props
export interface MessageAttachmentDisplayProps {
  attachments: MessageAttachment[]
  messageId?: string
  displayMode?: 'grid' | 'list' | 'carousel'
  showPreview?: boolean
  showActions?: boolean
  showFileInfo?: boolean
  enableDownload?: boolean
  enablePreview?: boolean
  enableZoom?: boolean
  enableRotation?: boolean
  enableFullscreen?: boolean
  autoPlay?: boolean
  compact?: boolean
  disabled?: boolean
  theme?: 'light' | 'dark' | 'auto'
}

// MessageAttachmentDisplay Emits
export interface MessageAttachmentDisplayEmits {
  (e: 'attachment-click', attachment: MessageAttachment): void
  (e: 'attachment-download', attachment: MessageAttachment): void
  (e: 'attachment-preview', attachment: MessageAttachment): void
  (e: 'attachment-zoom', attachment: MessageAttachment, zoom: number): void
  (e: 'attachment-rotate', attachment: MessageAttachment, rotation: number): void
  (e: 'attachment-fullscreen', attachment: MessageAttachment): void
  (e: 'attachment-delete', attachment: MessageAttachment): void
  (e: 'preview-open', attachment: MessageAttachment): void
  (e: 'preview-close'): void
  (e: 'display-mode-change', mode: 'grid' | 'list' | 'carousel'): void
}

// AttachmentCard Props
export interface AttachmentCardProps {
  attachment: MessageAttachment
  messageId?: string
  compactMode?: boolean
  showActions?: boolean
  showSize?: boolean
  showDate?: boolean
  showStatus?: boolean
  showProgress?: boolean
  showThumbnail?: boolean
  clickable?: boolean
  disabled?: boolean
}

// AttachmentCard Emits
export interface AttachmentCardEmits {
  (e: 'click', attachment: MessageAttachment): void
  (e: 'download', attachment: MessageAttachment): void
  (e: 'preview', attachment: MessageAttachment): void
  (e: 'delete', attachment: MessageAttachment): void
}

// 过滤器相关类型
export interface MessageFilters {
  search?: string
  messageTypes?: string[]
  statuses?: string[]
  priorities?: string[]
  senderFilter?: string
  receiverFilter?: string
  startDate?: string
  endDate?: string
  hasAttachments?: boolean
  hasImages?: boolean
  hasDocuments?: boolean
  tags?: string[]
}

export interface FilterPreset {
  id: string
  name: string
  description: string
  filters: MessageFilters
}

export interface ActiveFilter {
  id: string
  label: string
  type: string
  value: any
}

export interface QuickFilter {
  id: string
  label: string
  icon: string
  filters: Partial<MessageFilters>
}

export interface DateRange {
  id: string
  label: string
  days: number
}

// 搜索相关类型
export interface SearchFilters {
  sender?: string
  receiver?: string
  dateFrom?: string
  dateTo?: string
  messageType?: string
  status?: string
  priority?: string
  hasAttachments?: boolean
  tags?: string[]
}

export interface SearchSuggestion {
  id: string
  text: string
  type: 'message' | 'user' | 'tag' | 'recent'
  data?: any
}

export interface SearchHistory {
  id: string
  query: string
  timestamp: string
  filters?: SearchFilters
  resultCount?: number
}

export interface SearchData {
  query: string
  filters?: SearchFilters
  timestamp: string
  resultCount?: number
}

// 表单数据类型
export interface MessageFormData {
  id?: string
  recipients: User[]
  subject?: string
  content: string
  priority?: string
  attachments: File[]
  tags: Tag[]
  draft?: boolean
  replyTo?: string
  forwardFrom?: string
}

// 消息数据类型
export interface MessageData {
  conversationId: string
  content: string
  attachments?: File[]
  replyTo?: string
  threadId?: string
}

// 配置类型
export interface MessageConfig {
  itemsPerPage: number
  maxFileSize: number
  maxAttachments: number
  allowedFileTypes: string[]
  enableRealTime: boolean
  enableNotifications: boolean
  enableSound: boolean
  enableEmoji: boolean
  enableAttachments: boolean
  enableVoiceMessages: boolean
  enableVideoCalls: boolean
  autoSaveInterval: number
  debounceDelay: number
  animationDuration: number
  theme: 'light' | 'dark' | 'auto'
}

// 组件实例类型
export interface MessageComponentInstance {
  reset: () => void
  validate: () => boolean
  submit: () => void
  cancel: () => void
  focus: () => void
  blur: () => void
  scrollToTop: () => void
  scrollToBottom: () => void
  refresh: () => Promise<void>
}

// 组合式函数返回类型
export interface UseMessageActionsReturn {
  actions: {
    select: (message: Message) => void
    deselect: (message: Message) => void
    delete: (message: Message) => void
    archive: (message: Message) => void
    markRead: (message: Message) => void
    markUnread: (message: Message) => void
    reply: (message: Message) => void
    forward: (message: Message) => void
    star: (message: Message) => void
    unstar: (message: Message) => void
  }
  loading: Ref<boolean>
  error: Ref<Error | null>
}

export interface UseMessageFiltersReturn {
  filters: Ref<MessageFilters>
  activeFilters: Ref<ActiveFilter[]>
  resetFilters: () => void
  applyFilter: (filter: Partial<MessageFilters>) => void
  removeFilter: (filterId: string) => void
  savePreset: (name: string, description: string) => void
  applyPreset: (preset: FilterPreset) => void
}

export interface UseMessageSearchReturn {
  searchQuery: Ref<string>
  searchResults: Ref<Message[]>
  searchSuggestions: Ref<SearchSuggestion[]>
  searchHistory: Ref<SearchHistory[]>
  isSearching: Ref<boolean>
  search: (query: string, filters?: SearchFilters) => Promise<void>
  clearSearch: () => void
  addToHistory: (query: string, filters?: SearchFilters) => void
  removeFromHistory: (id: string) => void
}

export interface UseMessageAttachmentsReturn {
  attachments: Ref<File[]>
  uploadProgress: Ref<Record<string, number>>
  isUploading: Ref<boolean>
  addAttachment: (file: File) => void
  removeAttachment: (file: File) => void
  uploadAttachments: () => Promise<void>
  cancelUpload: () => void
  validateFile: (file: File) => boolean
}

export interface UseMessageAnimationsReturn {
  animationState: Ref<{
    isAnimating: boolean
    animationType: string | null
  }>
  animate: (type: string, element?: HTMLElement) => Promise<void>
  stopAnimation: () => void
  setAnimationDuration: (duration: number) => void
}

export interface UseMessageKeyboardShortcutsReturn {
  shortcuts: Ref<Record<string, () => void>>
  registerShortcut: (key: string, callback: () => void) => void
  unregisterShortcut: (key: string) => void
  enableShortcuts: () => void
  disableShortcuts: () => void
}

export interface UseMessageDragAndDropReturn {
  isDragging: Ref<boolean>
  dragData: Ref<any>
  dragStart: (data: any, event: DragEvent) => void
  dragEnd: () => void
  dragOver: (event: DragEvent) => void
  dragLeave: (event: DragEvent) => void
  drop: (event: DragEvent) => void
}

export interface UseMessageInfiniteScrollReturn {
  isLoading: Ref<boolean>
  hasMore: Ref<boolean>
  page: Ref<number>
  loadMore: () => Promise<void>
  reset: () => void
  scrollToTop: () => void
}

export interface UseMessageRealTimeReturn {
  isConnected: Ref<boolean>
  connectionError: Ref<Error | null>
  connect: () => Promise<void>
  disconnect: () => void
  sendMessage: (message: any) => void
  onMessage: (callback: (message: any) => void) => void
  onTyping: (callback: (user: User) => void) => void
  onConnectionChange: (callback: (connected: boolean) => void) => void
}

export interface UseMessagePermissionsReturn {
  permissions: Ref<Record<string, boolean>>
  hasPermission: (permission: string) => boolean
  requirePermission: (permission: string) => boolean
  checkPermissions: (permissions: string[]) => boolean
}