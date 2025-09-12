/**
 * 消息API服务
 * 
 * 本文件提供消息系统的前端API服务实现，包括：
 * - 消息CRUD操作
 * - 消息附件管理
 * - 消息草稿管理
 * - 会话管理
 * - 消息搜索和筛选
 * - 消息批量操作
 * - 消息导入导出
 * - 消息统计和分析
 * - 消息实时通信
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import api from './api'
import { webSocketService } from './websocketService'
import type {
  Message,
  CreateMessageRequest,
  UpdateMessageRequest,
  MessageFilter,
  MessageAttachment,
  MessageDraft,
  CreateMessageDraftRequest,
  UpdateMessageDraftRequest,
  Conversation,
  CreateConversationRequest,
  UpdateConversationRequest,
  ConversationFilter,
  BatchMessageOperationRequest,
  MessageSearchRequest,
  MessageSearchResult,
  MessageStats,
  MessageExportOptions,
  MessageImportData,
  ValidationResult,
  MessageAnalysisMetrics,
  FileUploadRequest,
  FileUploadResponse,
  BatchFileUploadRequest,
  MessageRealtimeUpdate,
  MessageOperationHistory,
  MessageTemplate,
  MessageFilterRule,
  PaginatedMessages,
  PaginatedConversations,
  PaginatedMessageDrafts,
  MessagePermissions,
  MessageNotificationPreferences,
  MessageReaction,
  MessageReadReceipt,
  ConversationParticipant,
  MessageRealtimeEvent,
  WebSocketMessage,
  WebSocketMessageType,
  WebSocketMessagePriority,
  UploadProgress,
  DownloadInfo
} from '@/types/message'
import type { WebSocketClient } from '@/types/websocket'

/**
 * 消息服务类 - 封装所有消息相关的API调用
 */
export class MessageService {
  private readonly basePath = '/api/messages'
  private readonly attachmentsPath = '/api/message-attachments'
  private readonly draftsPath = '/api/message-drafts'
  private readonly conversationsPath = '/api/conversations'
  private readonly searchPath = '/api/messages/search'
  private readonly exportPath = '/api/messages/export'
  private readonly importPath = '/api/messages/import'
  private readonly statsPath = '/api/messages/stats'
  private readonly analyticsPath = '/api/messages/analytics'
  private readonly realtimePath = '/api/messages/realtime'
  
  // WebSocket客户端实例
  private wsClient: WebSocketClient | null = null
  // 实时事件订阅映射
  private realtimeSubscriptions: Map<string, Set<(event: MessageRealtimeEvent) => void>> = new Map()
  // 上传进度跟踪
  private uploadProgress: Map<string, UploadProgress> = new Map()
  // 重试队列
  private retryQueue: Map<string, { operation: () => Promise<any>; retryCount: number; maxRetries: number }> = new Map()
  // 缓存存储
  private cache: Map<string, { data: any; timestamp: number; ttl: number }> = new Map()
  // 网络状态
  private isOnline = navigator.onLine
  // API限流状态
  private rateLimitStatus: Map<string, { resetTime: number; remaining: number }> = new Map()

  constructor() {
    this.initializeNetworkMonitoring()
    this.initializeWebSocket()
    this.startRetryQueueProcessor()
    this.startCacheCleanup()
  }

  // ==================== 消息CRUD操作 ====================

  /**
   * 获取消息列表
   * @param filter 消息筛选条件
   * @returns 消息分页结果
   */
  async getMessages(filter: MessageFilter): Promise<PaginatedMessages> {
    const response = await api.get<PaginatedMessages>(this.basePath, {
      params: filter
    })
    return response.data
  }

  /**
   * 获取指定消息的详细信息
   * @param id 消息ID
   * @returns 消息详细信息
   */
  async getMessageById(id: string): Promise<Message> {
    const response = await api.get<Message>(`${this.basePath}/${id}`)
    return response.data
  }

  /**
   * 创建新消息
   * @param message 消息创建请求
   * @returns 创建的消息信息
   */
  async createMessage(message: CreateMessageRequest): Promise<Message> {
    const response = await api.post<Message>(this.basePath, message)
    return response.data
  }

  /**
   * 更新消息内容
   * @param id 消息ID
   * @param message 消息更新请求
   * @returns 更新后的消息信息
   */
  async updateMessage(id: string, message: UpdateMessageRequest): Promise<Message> {
    const response = await api.put<Message>(`${this.basePath}/${id}`, message)
    return response.data
  }

  /**
   * 删除消息
   * @param id 消息ID
   */
  async deleteMessage(id: string): Promise<void> {
    await api.delete(`${this.basePath}/${id}`)
  }

  /**
   * 标记消息为已读
   * @param id 消息ID
   */
  async markAsRead(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/read`)
  }

  /**
   * 标记消息为未读
   * @param id 消息ID
   */
  async markAsUnread(id: string): Promise<void> {
    await api.post(`${this.basePath}/${id}/unread`)
  }

  /**
   * 获取消息权限
   * @param id 消息ID
   * @returns 消息权限信息
   */
  async getMessagePermissions(id: string): Promise<MessagePermissions> {
    const response = await api.get<MessagePermissions>(`${this.basePath}/${id}/permissions`)
    return response.data
  }

  /**
   * 获取消息操作历史
   * @param id 消息ID
   * @returns 消息操作历史
   */
  async getMessageOperationHistory(id: string): Promise<MessageOperationHistory[]> {
    const response = await api.get<MessageOperationHistory[]>(`${this.basePath}/${id}/history`)
    return response.data
  }

  // ==================== 消息批量操作 ====================

  /**
   * 批量操作消息
   * @param request 批量操作请求
   */
  async batchOperation(request: BatchMessageOperationRequest): Promise<void> {
    await api.post(`${this.basePath}/batch`, request)
  }

  // ==================== 消息附件管理 ====================

  /**
   * 上传单个文件
   * @param request 文件上传请求
   * @returns 上传响应
   */
  async uploadFile(request: FileUploadRequest): Promise<FileUploadResponse> {
    const formData = new FormData()
    formData.append('file', request.file)
    
    if (request.messageType) {
      formData.append('messageType', request.messageType)
    }
    
    if (request.attachmentType) {
      formData.append('attachmentType', request.attachmentType)
    }

    const response = await api.post<FileUploadResponse>(
      `${this.attachmentsPath}/upload`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data'
        },
        onUploadProgress: (progressEvent) => {
          if (request.onProgress && progressEvent.total) {
            const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
            request.onProgress(progress)
          }
        }
      }
    )

    if (response.data.success && request.onSuccess) {
      request.onSuccess(response.data.attachment!)
    }

    if (!response.data.success && request.onError) {
      request.onError(response.data.error || '上传失败')
    }

    return response.data
  }

  /**
   * 批量上传文件
   * @param request 批量文件上传请求
   * @returns 上传响应数组
   */
  async uploadMultipleFiles(request: BatchFileUploadRequest): Promise<FileUploadResponse[]> {
    const responses: FileUploadResponse[] = []
    const errors: string[] = []

    for (let i = 0; i < request.files.length; i++) {
      const file = request.files[i]
      try {
        const response = await this.uploadFile({
          file,
          messageType: request.messageType,
          attachmentType: request.attachmentType,
          onProgress: (progress) => {
            if (request.onProgress) {
              request.onProgress(progress, file.name)
            }
          }
        })
        responses.push(response)
      } catch (error) {
        const errorMessage = `文件 ${file.name} 上传失败: ${error}`
        errors.push(errorMessage)
        responses.push({
          success: false,
          error: errorMessage,
          progress: 0
        })
      }
    }

    if (errors.length > 0 && request.onError) {
      request.onError(errors)
    }

    if (errors.length === 0 && request.onSuccess) {
      const attachments = responses
        .filter(r => r.success && r.attachment)
        .map(r => r.attachment!)
      request.onSuccess(attachments)
    }

    return responses
  }

  /**
   * 删除附件
   * @param id 附件ID
   */
  async deleteAttachment(id: string): Promise<void> {
    await api.delete(`${this.attachmentsPath}/${id}`)
  }

  /**
   * 下载附件
   * @param id 附件ID
   * @returns 文件Blob
   */
  async downloadAttachment(id: string): Promise<Blob> {
    const response = await api.get(`${this.attachmentsPath}/${id}/download`, {
      responseType: 'blob'
    })
    return response.data
  }

  /**
   * 获取附件信息
   * @param id 附件ID
   * @returns 附件信息
   */
  async getAttachment(id: string): Promise<MessageAttachment> {
    const response = await api.get<MessageAttachment>(`${this.attachmentsPath}/${id}`)
    return response.data
  }

  // ==================== 消息草稿管理 ====================

  /**
   * 获取消息草稿列表
   * @param filter 草稿筛选条件
   * @returns 草稿分页结果
   */
  async getDrafts(filter: MessageFilter): Promise<PaginatedMessageDrafts> {
    const response = await api.get<PaginatedMessageDrafts>(this.draftsPath, {
      params: filter
    })
    return response.data
  }

  /**
   * 获取指定草稿的详细信息
   * @param id 草稿ID
   * @returns 草稿详细信息
   */
  async getDraftById(id: string): Promise<MessageDraft> {
    const response = await api.get<MessageDraft>(`${this.draftsPath}/${id}`)
    return response.data
  }

  /**
   * 创建新草稿
   * @param draft 草稿创建请求
   * @returns 创建的草稿信息
   */
  async createDraft(draft: CreateMessageDraftRequest): Promise<MessageDraft> {
    const response = await api.post<MessageDraft>(this.draftsPath, draft)
    return response.data
  }

  /**
   * 更新草稿内容
   * @param id 草稿ID
   * @param draft 草稿更新请求
   * @returns 更新后的草稿信息
   */
  async updateDraft(id: string, draft: UpdateMessageDraftRequest): Promise<MessageDraft> {
    const response = await api.put<MessageDraft>(`${this.draftsPath}/${id}`, draft)
    return response.data
  }

  /**
   * 删除草稿
   * @param id 草稿ID
   */
  async deleteDraft(id: string): Promise<void> {
    await api.delete(`${this.draftsPath}/${id}`)
  }

  /**
   * 发送草稿
   * @param id 草稿ID
   * @returns 发送后的消息信息
   */
  async sendDraft(id: string): Promise<Message> {
    const response = await api.post<Message>(`${this.draftsPath}/${id}/send`)
    return response.data
  }

  /**
   * 自动保存草稿
   * @param id 草稿ID
   * @param content 草稿内容
   */
  async autoSaveDraft(id: string, content: string): Promise<void> {
    await api.post(`${this.draftsPath}/${id}/autosave`, { content })
  }

  // ==================== 会话管理 ====================

  /**
   * 获取会话列表
   * @param filter 会话筛选条件
   * @returns 会话分页结果
   */
  async getConversations(filter: ConversationFilter): Promise<PaginatedConversations> {
    const response = await api.get<PaginatedConversations>(this.conversationsPath, {
      params: filter
    })
    return response.data
  }

  /**
   * 获取指定会话的详细信息
   * @param id 会话ID
   * @returns 会话详细信息
   */
  async getConversationById(id: string): Promise<Conversation> {
    const response = await api.get<Conversation>(`${this.conversationsPath}/${id}`)
    return response.data
  }

  /**
   * 创建新会话
   * @param conversation 会话创建请求
   * @returns 创建的会话信息
   */
  async createConversation(conversation: CreateConversationRequest): Promise<Conversation> {
    const response = await api.post<Conversation>(this.conversationsPath, conversation)
    return response.data
  }

  /**
   * 更新会话信息
   * @param id 会话ID
   * @param conversation 会话更新请求
   * @returns 更新后的会话信息
   */
  async updateConversation(id: string, conversation: UpdateConversationRequest): Promise<Conversation> {
    const response = await api.put<Conversation>(`${this.conversationsPath}/${id}`, conversation)
    return response.data
  }

  /**
   * 删除会话
   * @param id 会话ID
   */
  async deleteConversation(id: string): Promise<void> {
    await api.delete(`${this.conversationsPath}/${id}`)
  }

  /**
   * 获取会话中的消息
   * @param conversationId 会话ID
   * @param filter 消息筛选条件
   * @returns 消息分页结果
   */
  async getConversationMessages(conversationId: string, filter: MessageFilter): Promise<PaginatedMessages> {
    const response = await api.get<PaginatedMessages>(
      `${this.conversationsPath}/${conversationId}/messages`,
      { params: filter }
    )
    return response.data
  }

  /**
   * 添加会话参与者
   * @param conversationId 会话ID
   * @param participantIds 参与者ID数组
   */
  async addConversationParticipants(conversationId: string, participantIds: string[]): Promise<void> {
    await api.post(`${this.conversationsPath}/${conversationId}/participants`, { participantIds })
  }

  /**
   * 移除会话参与者
   * @param conversationId 会话ID
   * @param participantId 参与者ID
   */
  async removeConversationParticipant(conversationId: string, participantId: string): Promise<void> {
    await api.delete(`${this.conversationsPath}/${conversationId}/participants/${participantId}`)
  }

  /**
   * 设置会话置顶状态
   * @param conversationId 会话ID
   * @param isPinned 是否置顶
   */
  async setConversationPinned(conversationId: string, isPinned: boolean): Promise<void> {
    await api.put(`${this.conversationsPath}/${conversationId}/pinned`, { isPinned })
  }

  /**
   * 设置会话静音状态
   * @param conversationId 会话ID
   * @param isMuted 是否静音
   */
  async setConversationMuted(conversationId: string, isMuted: boolean): Promise<void> {
    await api.put(`${this.conversationsPath}/${conversationId}/muted`, { isMuted })
  }

  /**
   * 归档或取消归档会话
   * @param conversationId 会话ID
   * @param isArchived 是否归档
   */
  async setConversationArchived(conversationId: string, isArchived: boolean): Promise<void> {
    await api.put(`${this.conversationsPath}/${conversationId}/archived`, { isArchived })
  }

  // ==================== 消息搜索 ====================

  /**
   * 搜索消息
   * @param request 搜索请求
   * @returns 搜索结果
   */
  async searchMessages(request: MessageSearchRequest): Promise<MessageSearchResult> {
    const response = await api.post<MessageSearchResult>(this.searchPath, request)
    return response.data
  }

  /**
   * 获取搜索建议
   * @param keyword 搜索关键词
   * @returns 搜索建议列表
   */
  async getSearchSuggestions(keyword: string): Promise<string[]> {
    const response = await api.get<string[]>(`${this.searchPath}/suggestions`, {
      params: { keyword }
    })
    return response.data
  }

  /**
   * 保存搜索历史
   * @param keyword 搜索关键词
   */
  async saveSearchHistory(keyword: string): Promise<void> {
    await api.post(`${this.searchPath}/history`, { keyword })
  }

  /**
   * 获取搜索历史
   * @returns 搜索历史列表
   */
  async getSearchHistory(): Promise<string[]> {
    const response = await api.get<string[]>(`${this.searchPath}/history`)
    return response.data
  }

  /**
   * 清除搜索历史
   */
  async clearSearchHistory(): Promise<void> {
    await api.delete(`${this.searchPath}/history`)
  }

  // ==================== 消息统计和分析 ====================

  /**
   * 获取消息统计信息
   * @returns 消息统计信息
   */
  async getMessageStats(): Promise<MessageStats> {
    const response = await api.get<MessageStats>(this.statsPath)
    return response.data
  }

  /**
   * 获取消息分析数据
   * @param filters 分析筛选条件
   * @returns 消息分析数据
   */
  async getMessageAnalytics(filters?: any): Promise<MessageAnalysisMetrics> {
    const response = await api.get<MessageAnalysisMetrics>(this.analyticsPath, {
      params: filters
    })
    return response.data
  }

  /**
   * 获取用户消息统计
   * @param userId 用户ID
   * @returns 用户消息统计
   */
  async getUserMessageStats(userId: string): Promise<MessageStats> {
    const response = await api.get<MessageStats>(`${this.statsPath}/user/${userId}`)
    return response.data
  }

  /**
   * 获取会话统计
   * @param conversationId 会话ID
   * @returns 会话统计
   */
  async getConversationStats(conversationId: string): Promise<MessageStats> {
    const response = await api.get<MessageStats>(`${this.statsPath}/conversation/${conversationId}`)
    return response.data
  }

  // ==================== 消息导入导出 ====================

  /**
   * 导出消息
   * @param options 导出选项
   * @returns 导出的文件Blob
   */
  async exportMessages(options: MessageExportOptions): Promise<Blob> {
    const response = await api.post(this.exportPath, options, {
      responseType: 'blob'
    })
    return response.data
  }

  /**
   * 导入消息
   * @param data 导入数据
   * @returns 验证结果
   */
  async importMessages(data: MessageImportData): Promise<ValidationResult> {
    const response = await api.post<ValidationResult>(this.importPath, data)
    return response.data
  }

  /**
   * 验证导入数据
   * @param data 导入数据
   * @returns 验证结果
   */
  async validateImportData(data: any): Promise<ValidationResult> {
    const response = await api.post<ValidationResult>(`${this.importPath}/validate`, data)
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

  // ==================== 消息模板 ====================

  /**
   * 获取消息模板列表
   * @returns 消息模板列表
   */
  async getMessageTemplates(): Promise<MessageTemplate[]> {
    const response = await api.get<MessageTemplate[]>(`${this.basePath}/templates`)
    return response.data
  }

  /**
   * 获取指定消息模板
   * @param id 模板ID
   * @returns 消息模板
   */
  async getMessageTemplate(id: string): Promise<MessageTemplate> {
    const response = await api.get<MessageTemplate>(`${this.basePath}/templates/${id}`)
    return response.data
  }

  /**
   * 使用模板创建消息
   * @param templateId 模板ID
   * @param variables 模板变量
   * @param receiverId 接收者ID
   * @returns 创建的消息
   */
  async createMessageFromTemplate(
    templateId: string,
    variables: Record<string, any>,
    receiverId: string
  ): Promise<Message> {
    const response = await api.post<Message>(`${this.basePath}/templates/${templateId}/use`, {
      variables,
      receiverId
    })
    return response.data
  }

  // ==================== 消息过滤规则 ====================

  /**
   * 获取消息过滤规则列表
   * @returns 消息过滤规则列表
   */
  async getMessageFilterRules(): Promise<MessageFilterRule[]> {
    const response = await api.get<MessageFilterRule[]>(`${this.basePath}/filter-rules`)
    return response.data
  }

  /**
   * 创建消息过滤规则
   * @param rule 过滤规则
   * @returns 创建的过滤规则
   */
  async createMessageFilterRule(rule: Omit<MessageFilterRule, 'id' | 'createdAt' | 'updatedAt'>): Promise<MessageFilterRule> {
    const response = await api.post<MessageFilterRule>(`${this.basePath}/filter-rules`, rule)
    return response.data
  }

  /**
   * 更新消息过滤规则
   * @param id 规则ID
   * @param rule 过滤规则
   * @returns 更新后的过滤规则
   */
  async updateMessageFilterRule(id: string, rule: Partial<MessageFilterRule>): Promise<MessageFilterRule> {
    const response = await api.put<MessageFilterRule>(`${this.basePath}/filter-rules/${id}`, rule)
    return response.data
  }

  /**
   * 删除消息过滤规则
   * @param id 规则ID
   */
  async deleteMessageFilterRule(id: string): Promise<void> {
    await api.delete(`${this.basePath}/filter-rules/${id}`)
  }

  // ==================== 消息通知设置 ====================

  /**
   * 获取消息通知设置
   * @returns 消息通知设置
   */
  async getMessageNotificationSettings(): Promise<MessageNotificationPreferences> {
    const response = await api.get<MessageNotificationPreferences>(`${this.basePath}/notifications`)
    return response.data
  }

  /**
   * 更新消息通知设置
   * @param settings 通知设置
   * @returns 更新后的通知设置
   */
  async updateMessageNotificationSettings(settings: MessageNotificationPreferences): Promise<MessageNotificationPreferences> {
    const response = await api.put<MessageNotificationPreferences>(`${this.basePath}/notifications`, settings)
    return response.data
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
   * 订阅会话实时更新
   * @param conversationId 会话ID
   * @returns 订阅结果
   */
  async subscribeToConversation(conversationId: string): Promise<{ success: boolean; subscriptionId: string }> {
    const response = await api.post<{ success: boolean; subscriptionId: string }>(
      `${this.realtimePath}/subscribe`,
      { conversationId }
    )
    return response.data
  }

  /**
   * 取消订阅会话实时更新
   * @param subscriptionId 订阅ID
   */
  async unsubscribeFromConversation(subscriptionId: string): Promise<void> {
    await api.post(`${this.realtimePath}/unsubscribe`, { subscriptionId })
  }

  /**
   * 发送正在输入状态
   * @param conversationId 会话ID
   */
  async sendTypingStatus(conversationId: string): Promise<void> {
    await api.post(`${this.realtimePath}/typing`, { conversationId })
  }

  /**
   * 停止输入状态
   * @param conversationId 会话ID
   */
  async stopTypingStatus(conversationId: string): Promise<void> {
    await api.post(`${this.realtimePath}/typing-stop`, { conversationId })
  }

  // ==================== 工具方法 ====================

  /**
   * 获取文件类型图标
   * @param fileName 文件名
   * @returns 文件类型图标
   */
  getFileTypeIcon(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase()
    
    const iconMap: Record<string, string> = {
      'pdf': '📄',
      'doc': '📝',
      'docx': '📝',
      'xls': '📊',
      'xlsx': '📊',
      'ppt': '📽️',
      'pptx': '📽️',
      'txt': '📃',
      'jpg': '🖼️',
      'jpeg': '🖼️',
      'png': '🖼️',
      'gif': '🖼️',
      'svg': '🖼️',
      'mp4': '🎥',
      'avi': '🎥',
      'mov': '🎥',
      'mp3': '🎵',
      'wav': '🎵',
      'zip': '📦',
      'rar': '📦',
      '7z': '📦',
      'tar': '📦',
      'gz': '📦',
      'js': '📜',
      'ts': '📜',
      'py': '📜',
      'java': '📜',
      'cpp': '📜',
      'html': '🌐',
      'css': '🎨',
      'json': '📋',
      'xml': '📋',
      'md': '📝',
      'log': '📋'
    }
    
    return iconMap[extension || ''] || '📎'
  }

  /**
   * 格式化文件大小
   * @param bytes 字节数
   * @returns 格式化后的文件大小
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes'
    
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  /**
   * 检查文件类型是否允许上传
   * @param file 文件对象
   * @param allowedTypes 允许的文件类型数组
   * @returns 是否允许上传
   */
  isFileTypeAllowed(file: File, allowedTypes: string[]): boolean {
    if (!allowedTypes.length) return true
    
    const fileName = file.name.toLowerCase()
    const fileExtension = fileName.split('.').pop()
    
    return allowedTypes.some(type => {
      if (type.startsWith('.')) {
        return fileName.endsWith(type)
      } else if (type.includes('/')) {
        return file.type === type
      } else {
        return fileExtension === type
      }
    })
  }

  /**
   * 检查文件大小是否超过限制
   * @param file 文件对象
   * @param maxSize 最大文件大小（字节）
   * @returns 是否超过限制
   */
  isFileSizeExceeded(file: File, maxSize: number): boolean {
    return file.size > maxSize
  }

  /**
   * 生成唯一的文件名
   * @param originalFileName 原始文件名
   * @returns 唯一的文件名
   */
  generateUniqueFileName(originalFileName: string): string {
    const timestamp = Date.now()
    const randomString = Math.random().toString(36).substring(2, 8)
    const fileExtension = originalFileName.split('.').pop()
    const fileNameWithoutExtension = originalFileName.replace(`.${fileExtension}`, '')
    
    return `${fileNameWithoutExtension}_${timestamp}_${randomString}.${fileExtension}`
  }

  /**
   * 消息搜索结果高亮
   * @param text 原始文本
   * @param keywords 关键词数组
   * @returns 高亮后的HTML
   */
  highlightSearchResults(text: string, keywords: string[]): string {
    if (!keywords.length) return text
    
    let highlightedText = text
    
    keywords.forEach(keyword => {
      if (!keyword) return
      
      const regex = new RegExp(`(${keyword})`, 'gi')
      highlightedText = highlightedText.replace(regex, '<mark>$1</mark>')
    })
    
    return highlightedText
  }

  /**
   * 验证消息内容
   * @param content 消息内容
   * @param maxLength 最大长度
   * @returns 验证结果
   */
  validateMessageContent(content: string, maxLength: number = 10000): { isValid: boolean; errors: string[] } {
    const errors: string[] = []
    
    if (!content || !content.trim()) {
      errors.push('消息内容不能为空')
    }
    
    if (content.length > maxLength) {
      errors.push(`消息内容不能超过 ${maxLength} 个字符`)
    }
    
    // 检查是否包含潜在的恶意内容
    const maliciousPatterns = [
      /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
      /javascript:/gi,
      /on\w+\s*=/gi
    ]
    
    maliciousPatterns.forEach(pattern => {
      if (pattern.test(content)) {
        errors.push('消息内容包含不安全的内容')
      }
    })
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }

  /**
   * 批量操作验证
   * @param request 批量操作请求
   * @returns 验证结果
   */
  validateBatchOperation(request: BatchMessageOperationRequest): { isValid: boolean; errors: string[] } {
    const errors: string[] = []
    
    if (!request.messageIds || request.messageIds.length === 0) {
      errors.push('请选择要操作的消息')
    }
    
    if (request.messageIds.length > 100) {
      errors.push('一次最多只能操作 100 条消息')
    }
    
    if (!request.operation) {
      errors.push('请选择操作类型')
    }
    
    // 特定操作的额外验证
    if (request.operation === MessageOperation.DELETE && !request.reason) {
      errors.push('删除操作需要提供理由')
    }
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }

  // ==================== 实时通信功能 ====================

  /**
   * 初始化WebSocket连接
   */
  private initializeWebSocket(): void {
    this.wsClient = webSocketService
    
    // 注册消息实时事件处理器
    this.wsClient?.registerEventHandlers({
      onMessage: this.handleWebSocketMessage.bind(this),
      onConnected: this.handleWebSocketConnected.bind(this),
      onDisconnected: this.handleWebSocketDisconnected.bind(this),
      onError: this.handleWebSocketError.bind(this)
    })
  }

  /**
   * 初始化网络状态监控
   */
  private initializeNetworkMonitoring(): void {
    window.addEventListener('online', () => {
      this.isOnline = true
      this.handleNetworkRecovery()
    })

    window.addEventListener('offline', () => {
      this.isOnline = false
      this.handleNetworkDisconnection()
    })
  }

  /**
   * 订阅会话实时事件
   * @param conversationId 会话ID
   * @param callback 事件回调函数
   * @returns 取消订阅函数
   */
  subscribeToConversation(conversationId: string, callback: (event: MessageRealtimeEvent) => void): () => void {
    if (!this.realtimeSubscriptions.has(conversationId)) {
      this.realtimeSubscriptions.set(conversationId, new Set())
    }
    
    this.realtimeSubscriptions.get(conversationId)!.add(callback)
    
    // 如果WebSocket未连接，尝试连接
    if (!this.wsClient?.connectionStatus.isConnected) {
      this.connectWebSocket()
    }

    // 返回取消订阅函数
    return () => {
      const callbacks = this.realtimeSubscriptions.get(conversationId)
      if (callbacks) {
        callbacks.delete(callback)
        if (callbacks.size === 0) {
          this.realtimeSubscriptions.delete(conversationId)
        }
      }
    }
  }

  /**
   * 连接WebSocket
   */
  private async connectWebSocket(): Promise<void> {
    try {
      if (this.wsClient && !this.wsClient.connectionStatus.isConnected) {
        await this.wsClient.connect()
      }
    } catch (error) {
      console.error('WebSocket连接失败:', error)
    }
  }

  /**
   * 处理WebSocket连接成功
   */
  private handleWebSocketConnected(connectionId: string, userId: string): void {
    console.log('WebSocket连接成功:', { connectionId, userId })
    
    // 重新订阅所有会话
    for (const conversationId of this.realtimeSubscriptions.keys()) {
      this.subscribeToConversationOnServer(conversationId)
    }
  }

  /**
   * 处理WebSocket连接断开
   */
  private handleWebSocketDisconnected(connectionId: string, userId: string, reason?: string): void {
    console.log('WebSocket连接断开:', { connectionId, userId, reason })
    
    // 标记连接为离线状态
    this.isOnline = false
  }

  /**
   * 处理WebSocket错误
   */
  private handleWebSocketError(connectionId: string, error: Error): void {
    console.error('WebSocket错误:', { connectionId, error })
  }

  /**
   * 处理WebSocket消息
   */
  private handleWebSocketMessage(message: WebSocketMessage): void {
    try {
      const realtimeEvent: MessageRealtimeEvent = {
        type: message.type as any,
        messageId: message.messageId,
        conversationId: message.data?.conversationId,
        userId: message.senderId || '',
        data: message.data,
        timestamp: new Date().toISOString()
      }

      // 根据会话ID分发事件
      const conversationId = realtimeEvent.conversationId
      if (conversationId && this.realtimeSubscriptions.has(conversationId)) {
        const callbacks = this.realtimeSubscriptions.get(conversationId)!
        callbacks.forEach(callback => {
          try {
            callback(realtimeEvent)
          } catch (error) {
            console.error('实时事件回调执行失败:', error)
          }
        })
      }
    } catch (error) {
      console.error('处理WebSocket消息失败:', error)
    }
  }

  /**
   * 在服务器上订阅会话
   */
  private async subscribeToConversationOnServer(conversationId: string): Promise<void> {
    try {
      if (this.wsClient?.connectionStatus.isConnected) {
        await this.wsClient.addToGroup(`conversation_${conversationId}`)
      }
    } catch (error) {
      console.error('订阅会话失败:', error)
    }
  }

  /**
   * 发送实时消息
   * @param conversationId 会话ID
   * @param message 消息内容
   * @param messageType 消息类型
   */
  async sendRealtimeMessage(
    conversationId: string, 
    message: string, 
    messageType: WebSocketMessageType = WebSocketMessageType.USER
  ): Promise<boolean> {
    try {
      if (!this.wsClient?.connectionStatus.isConnected) {
        return false
      }

      const messageData = {
        conversationId,
        content: message,
        messageType,
        timestamp: new Date().toISOString()
      }

      const result = await this.wsClient.sendToGroup(
        `conversation_${conversationId}`,
        messageData,
        messageType
      )

      return result.success
    } catch (error) {
      console.error('发送实时消息失败:', error)
      return false
    }
  }

  // ==================== 消息回复和转发功能 ====================

  /**
   * 回复消息
   * @param messageId 原消息ID
   * @param content 回复内容
   * @param attachments 附件数组
   */
  async replyToMessage(
    messageId: string, 
    content: string, 
    attachments?: string[]
  ): Promise<Message> {
    const originalMessage = await this.getMessageById(messageId)
    
    const replyRequest: CreateMessageRequest = {
      conversationId: originalMessage.conversationId,
      content,
      messageType: MessageType.TEXT,
      replyToMessageId: messageId,
      attachments
    }

    return this.createMessage(replyRequest)
  }

  /**
   * 转发消息
   * @param messageId 原消息ID
   * @param targetConversationIds 目标会话ID数组
   */
  async forwardMessage(
    messageId: string, 
    targetConversationIds: string[]
  ): Promise<Message[]> {
    const originalMessage = await this.getMessageById(messageId)
    const results: Message[] = []

    for (const targetConversationId of targetConversationIds) {
      try {
        const forwardRequest: CreateMessageRequest = {
          conversationId: targetConversationId,
          content: originalMessage.content,
          messageType: originalMessage.messageType,
          attachments: originalMessage.attachments.map(att => att.id)
        }

        const forwardedMessage = await this.createMessage(forwardRequest)
        results.push(forwardedMessage)
      } catch (error) {
        console.error(`转发消息到会话 ${targetConversationId} 失败:`, error)
      }
    }

    return results
  }

  /**
   * 获取消息的回复列表
   * @param messageId 消息ID
   * @param filter 筛选条件
   */
  async getMessageReplies(messageId: string, filter?: MessageFilter): Promise<PaginatedMessages> {
    const response = await api.get<PaginatedMessages>(`${this.basePath}/${messageId}/replies`, {
      params: filter
    })
    return response.data
  }

  // ==================== 消息反应功能 ====================

  /**
   * 添加消息反应
   * @param messageId 消息ID
   * @param emoji 表情符号
   */
  async addMessageReaction(messageId: string, emoji: string): Promise<MessageReaction> {
    const response = await api.post<MessageReaction>(`${this.basePath}/${messageId}/reactions`, {
      emoji
    })
    return response.data
  }

  /**
   * 移除消息反应
   * @param messageId 消息ID
   * @param emoji 表情符号
   */
  async removeMessageReaction(messageId: string, emoji: string): Promise<void> {
    await api.delete(`${this.basePath}/${messageId}/reactions`, {
      params: { emoji }
    })
  }

  /**
   * 获取消息反应列表
   * @param messageId 消息ID
   */
  async getMessageReactions(messageId: string): Promise<MessageReaction[]> {
    const response = await api.get<MessageReaction[]>(`${this.basePath}/${messageId}/reactions`)
    return response.data
  }

  // ==================== 已读回执功能 ====================

  /**
   * 发送已读回执
   * @param messageId 消息ID
   */
  async sendReadReceipt(messageId: string): Promise<MessageReadReceipt> {
    const response = await api.post<MessageReadReceipt>(`${this.basePath}/${messageId}/read-receipt`)
    return response.data
  }

  /**
   * 获取消息已读回执列表
   * @param messageId 消息ID
   */
  async getMessageReadReceipts(messageId: string): Promise<MessageReadReceipt[]> {
    const response = await api.get<MessageReadReceipt[]>(`${this.basePath}/${messageId}/read-receipts`)
    return response.data
  }

  /**
   * 批量标记消息为已读
   * @param messageIds 消息ID数组
   * @param conversationId 会话ID
   */
  async markMessagesAsRead(messageIds: string[], conversationId: string): Promise<void> {
    await api.post(`${this.basePath}/batch-read`, {
      messageIds,
      conversationId
    })
  }

  // ==================== 缓存功能 ====================

  /**
   * 获取缓存数据
   * @param key 缓存键
   */
  private getCache<T>(key: string): T | null {
    const cached = this.cache.get(key)
    if (!cached) return null

    const now = Date.now()
    if (now - cached.timestamp > cached.ttl) {
      this.cache.delete(key)
      return null
    }

    return cached.data as T
  }

  /**
   * 设置缓存数据
   * @param key 缓存键
   * @param data 数据
   * @param ttl 过期时间（毫秒）
   */
  private setCache<T>(key: string, data: T, ttl: number = 300000): void {
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
      ttl
    })
  }

  /**
   * 清除缓存
   * @param key 缓存键
   */
  private clearCache(key: string): void {
    this.cache.delete(key)
  }

  /**
   * 启动缓存清理定时器
   */
  private startCacheCleanup(): void {
    setInterval(() => {
      const now = Date.now()
      for (const [key, cached] of this.cache.entries()) {
        if (now - cached.timestamp > cached.ttl) {
          this.cache.delete(key)
        }
      }
    }, 60000) // 每分钟清理一次
  }

  // ==================== 重试机制 ====================

  /**
   * 添加重试任务
   * @param key 任务键
   * @param operation 操作函数
   * @param maxRetries 最大重试次数
   */
  private addRetryTask(
    key: string, 
    operation: () => Promise<any>, 
    maxRetries: number = 3
  ): void {
    this.retryQueue.set(key, {
      operation,
      retryCount: 0,
      maxRetries
    })
  }

  /**
   * 启动重试队列处理器
   */
  private startRetryQueueProcessor(): void {
    setInterval(async () => {
      const now = Date.now()
      for (const [key, task] of this.retryQueue.entries()) {
        try {
          await task.operation()
          this.retryQueue.delete(key)
        } catch (error) {
          task.retryCount++
          if (task.retryCount >= task.maxRetries) {
            this.retryQueue.delete(key)
            console.error(`重试任务失败: ${key}`, error)
          }
        }
      }
    }, 5000) // 每5秒处理一次重试队列
  }

  // ==================== 网络状态处理 ====================

  /**
   * 处理网络恢复
   */
  private handleNetworkRecovery(): void {
    console.log('网络已恢复，重新连接WebSocket')
    this.connectWebSocket()
    
    // 重新处理失败的任务
    this.retryPendingOperations()
  }

  /**
   * 处理网络断开
   */
  private handleNetworkDisconnection(): void {
    console.log('网络已断开，标记为离线状态')
  }

  /**
   * 重试待处理的操作
   */
  private retryPendingOperations(): void {
    // 这里可以实现具体的重试逻辑
    console.log('重试待处理的操作')
  }

  // ==================== 权限验证和限流处理 ====================

  /**
   * 检查API限流状态
   * @param endpoint API端点
   */
  private checkRateLimit(endpoint: string): boolean {
    const rateLimit = this.rateLimitStatus.get(endpoint)
    if (!rateLimit) return true

    const now = Date.now()
    if (now > rateLimit.resetTime) {
      this.rateLimitStatus.delete(endpoint)
      return true
    }

    return rateLimit.remaining > 0
  }

  /**
   * 更新限流状态
   * @param endpoint API端点
   * @param resetTime 重置时间
   * @param remaining 剩余次数
   */
  private updateRateLimit(endpoint: string, resetTime: number, remaining: number): void {
    this.rateLimitStatus.set(endpoint, { resetTime, remaining })
  }

  /**
   * 增强的API请求方法，包含错误处理和重试逻辑
   */
  private async enhancedApiRequest<T>(
    method: 'GET' | 'POST' | 'PUT' | 'DELETE',
    url: string,
    data?: any,
    config?: any
  ): Promise<T> {
    const endpoint = url.split('/').pop() || 'default'
    
    // 检查限流
    if (!this.checkRateLimit(endpoint)) {
      throw new Error('API请求频率超限，请稍后再试')
    }

    try {
      const response = await api.request<T>({
        method,
        url,
        data,
        ...config
      })

      return response.data
    } catch (error: any) {
      // 处理限流响应
      if (error.response?.status === 429) {
        const resetTime = error.response.headers['x-ratelimit-reset'] || Date.now() + 60000
        const remaining = error.response.headers['x-ratelimit-remaining'] || 0
        this.updateRateLimit(endpoint, resetTime, remaining)
        
        throw new Error('API请求频率超限，请稍后再试')
      }

      // 处理网络错误
      if (!this.isOnline) {
        throw new Error('网络连接已断开，请检查网络设置')
      }

      throw error
    }
  }

  // ==================== 消息类型和优先级功能 ====================

  /**
   * 获取支持的消息类型
   */
  async getSupportedMessageTypes(): Promise<Array<{ value: MessageType; label: string; description: string }>> {
    const cacheKey = 'supported_message_types'
    const cached = this.getCache<Array<{ value: MessageType; label: string; description: string }>>(cacheKey)
    
    if (cached) {
      return cached
    }

    try {
      const response = await this.enhancedApiRequest<Array<{ value: MessageType; label: string; description: string }>>(
        'GET',
        `${this.basePath}/types`
      )
      
      this.setCache(cacheKey, response, 3600000) // 缓存1小时
      return response
    } catch (error) {
      console.error('获取消息类型失败:', error)
      return []
    }
  }

  /**
   * 获取支持的消息优先级
   */
  async getSupportedMessagePriorities(): Promise<Array<{ value: MessagePriority; label: string; description: string }>> {
    const cacheKey = 'supported_message_priorities'
    const cached = this.getCache<Array<{ value: MessagePriority; label: string; description: string }>>(cacheKey)
    
    if (cached) {
      return cached
    }

    try {
      const response = await this.enhancedApiRequest<Array<{ value: MessagePriority; label: string; description: string }>>(
        'GET',
        `${this.basePath}/priorities`
      )
      
      this.setCache(cacheKey, response, 3600000) // 缓存1小时
      return response
    } catch (error) {
      console.error('获取消息优先级失败:', error)
      return []
    }
  }

  /**
   * 设置消息优先级
   * @param messageId 消息ID
   * @param priority 优先级
   */
  async setMessagePriority(messageId: string, priority: MessagePriority): Promise<Message> {
    const response = await this.enhancedApiRequest<Message>(
      'PUT',
      `${this.basePath}/${messageId}/priority`,
      { priority }
    )
    return response
  }

  // ==================== 工具方法 ====================

  /**
   * 清理资源
   */
  cleanup(): void {
    // 清理WebSocket连接
    if (this.wsClient) {
      this.wsClient.cleanup()
    }

    // 清理缓存
    this.cache.clear()

    // 清理重试队列
    this.retryQueue.clear()

    // 清理上传进度
    this.uploadProgress.clear()

    // 清理实时订阅
    this.realtimeSubscriptions.clear()

    // 清理限流状态
    this.rateLimitStatus.clear()
  }
}

// 创建服务实例
export const messageService = new MessageService()