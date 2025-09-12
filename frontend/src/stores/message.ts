/**
 * 消息状态管理 Store
 * 
 * 本文件提供消息系统的前端状态管理实现，包括：
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
 * 遵循Vue 3 + Composition API开发模式，使用Pinia进行状态管理，使用TypeScript确保类型安全
 */

import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { messageService } from '@/services/messageService'
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
  MessageSort,
  MessageType,
  MessageStatus,
  MessagePriority,
  DraftStatus,
  MessageOperation,
  ConversationType,
  AttachmentType,
  MessageState,
  MessageActions,
  MessageGetters
} from '@/types/message'

/**
 * 消息状态管理 - 使用Pinia进行状态管理
 */
export const useMessageStore = defineStore('message', () => {
  // ==================== 基础状态 ====================
  
  // 消息状态
  const messages = ref<Message[]>([])
  const currentMessage = ref<Message | null>(null)
  const messageStats = ref<MessageStats | null>(null)
  const messageAnalytics = ref<MessageAnalysisMetrics | null>(null)
  
  // 会话状态
  const conversations = ref<Conversation[]>([])
  const currentConversation = ref<Conversation | null>(null)
  
  // 草稿状态
  const drafts = ref<MessageDraft[]>([])
  const currentDraft = ref<MessageDraft | null>(null)
  const autoSaveDraftTimer = ref<NodeJS.Timeout | null>(null)
  
  // 附件状态
  const attachments = ref<MessageAttachment[]>([])
  const uploadingFiles = ref<FileUploadResponse[]>([])
  const activeUploads = ref<Set<string>>(new Set())
  
  // 搜索和筛选状态
  const searchResults = ref<MessageSearchResult | null>(null)
  const currentFilter = ref<MessageFilter>({
    sortBy: MessageSort.CREATED_AT_DESC,
    page: 1,
    pageSize: 20
  })
  
  // 实时通信状态
  const websocketConnection = ref<WebSocket | null>(null)
  const isRealtimeConnected = ref(false)
  const typingUsers = ref<Set<string>>(new Set())
  const onlineUsers = ref<Set<string>>(new Set())
  
  // 通知设置
  const notificationPreferences = ref<MessageNotificationPreferences | null>(null)
  const mutedConversations = ref<Set<string>>(new Set())
  
  // 消息模板和过滤规则
  const messageTemplates = ref<MessageTemplate[]>([])
  const messageFilterRules = ref<MessageFilterRule[]>([])
  
  // UI状态
  const isLoading = ref(false)
  const isSending = ref(false)
  const isUploading = ref(false)
  const error = ref<string | null>(null)
  const success = ref<string | null>(null)
  const lastUpdated = ref<string | null>(null)
  
  // ==================== 计算属性 ====================
  
  // 基础状态计算
  const hasError = computed(() => error.value !== null)
  const hasSuccess = computed(() => success.value !== null)
  const isInitialized = computed(() => lastUpdated.value !== null)
  const totalMessages = computed(() => messages.value.length)
  const totalConversations = computed(() => conversations.value.length)
  const totalDrafts = computed(() => drafts.value.length)
  const totalAttachments = computed(() => attachments.value.length)
  
  // 消息相关计算
  const unreadMessages = computed(() => 
    messages.value.filter(msg => !msg.isRead)
  )
  const unreadCount = computed(() => unreadMessages.value.length)
  const sentMessages = computed(() => 
    messages.value.filter(msg => msg.isSender)
  )
  const receivedMessages = computed(() => 
    messages.value.filter(msg => msg.isReceiver)
  )
  
  // 会话相关计算
  const activeConversations = computed(() => 
    conversations.value.filter(conv => conv.isActive && !conv.isArchived)
  )
  const archivedConversations = computed(() => 
    conversations.value.filter(conv => conv.isArchived)
  )
  const pinnedConversations = computed(() => 
    conversations.value.filter(conv => conv.isPinned)
  )
  const mutedConversationsList = computed(() => 
    conversations.value.filter(conv => conv.isMuted)
  )
  const conversationsWithUnread = computed(() => 
    conversations.value.filter(conv => conv.hasUnreadMessages)
  )
  
  // 草稿相关计算
  const activeDrafts = computed(() => 
    drafts.value.filter(draft => draft.status === DraftStatus.DRAFT)
  )
  const scheduledDrafts = computed(() => 
    drafts.value.filter(draft => draft.isScheduled)
  )
  const expiredDrafts = computed(() => 
    drafts.value.filter(draft => draft.isExpired)
  )
  
  // 附件相关计算
  const uploadingAttachments = computed(() => 
    attachments.value.filter(att => att.attachmentStatus === 'UPLOADING')
  )
  const failedUploads = computed(() => 
    attachments.value.filter(att => att.attachmentStatus === 'UPLOAD_FAILED')
  )
  const totalAttachmentSize = computed(() => 
    attachments.value.reduce((total, att) => total + att.fileSize, 0)
  )
  
  // 实时状态计算
  const isTypingInConversation = computed(() => 
    typingUsers.value.size > 0
  )
  const currentUserOnline = computed(() => 
    onlineUsers.value.size > 0
  )
  
  // 权限相关计算
  const canSendMessage = computed(() => 
    currentConversation.value?.canSendMessage ?? false
  )
  const canEditConversation = computed(() => 
    currentConversation.value?.canEdit ?? false
  )
  const canDeleteConversation = computed(() => 
    currentConversation.value?.canDelete ?? false
  )
  
  // ==================== 消息CRUD操作 ====================
  
  /**
   * 获取消息列表
   * @param filter 消息筛选条件
   */
  async function fetchMessages(filter: MessageFilter = currentFilter.value) {
    try {
      isLoading.value = true
      error.value = null
      
      const response = await messageService.getMessages(filter)
      messages.value = response.items
      currentFilter.value = filter
      lastUpdated.value = new Date().toISOString()
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 获取指定消息的详细信息
   * @param id 消息ID
   */
  async function fetchMessageById(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      const message = await messageService.getMessageById(id)
      currentMessage.value = message
      
      // 更新本地消息列表
      const index = messages.value.findIndex(msg => msg.id === id)
      if (index !== -1) {
        messages.value[index] = message
      } else {
        messages.value.unshift(message)
      }
      
      return message
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息详情失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 创建新消息
   * @param message 消息创建请求
   */
  async function createMessage(message: CreateMessageRequest) {
    try {
      isSending.value = true
      error.value = null
      
      const newMessage = await messageService.createMessage(message)
      
      // 添加到消息列表
      messages.value.unshift(newMessage)
      
      // 如果是当前会话的消息，添加到会话
      if (currentConversation.value && message.conversationId === currentConversation.value.id) {
        currentConversation.value.lastMessage = newMessage
        currentConversation.value.messageCount++
        currentConversation.value.lastActivity = newMessage.createdAt
      }
      
      success.value = '消息发送成功'
      setTimeout(() => { success.value = null }, 3000)
      
      return newMessage
    } catch (err) {
      error.value = err instanceof Error ? err.message : '发送消息失败'
      throw err
    } finally {
      isSending.value = false
    }
  }
  
  /**
   * 更新消息内容
   * @param id 消息ID
   * @param message 消息更新请求
   */
  async function updateMessage(id: string, message: UpdateMessageRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const updatedMessage = await messageService.updateMessage(id, message)
      
      // 更新本地消息列表
      const index = messages.value.findIndex(msg => msg.id === id)
      if (index !== -1) {
        messages.value[index] = updatedMessage
      }
      
      if (currentMessage.value?.id === id) {
        currentMessage.value = updatedMessage
      }
      
      lastUpdated.value = new Date().toISOString()
      
      return updatedMessage
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 删除消息
   * @param id 消息ID
   */
  async function deleteMessage(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      await messageService.deleteMessage(id)
      
      // 从本地消息列表中移除
      messages.value = messages.value.filter(msg => msg.id !== id)
      
      if (currentMessage.value?.id === id) {
        currentMessage.value = null
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 标记消息为已读
   * @param id 消息ID
   */
  async function markAsRead(id: string) {
    try {
      await messageService.markAsRead(id)
      
      // 更新本地消息状态
      const message = messages.value.find(msg => msg.id === id)
      if (message) {
        message.isRead = true
        message.readAt = new Date().toISOString()
      }
      
      if (currentMessage.value?.id === id) {
        currentMessage.value.isRead = true
        currentMessage.value.readAt = new Date().toISOString()
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记已读失败'
      throw err
    }
  }
  
  /**
   * 标记消息为未读
   * @param id 消息ID
   */
  async function markAsUnread(id: string) {
    try {
      await messageService.markAsUnread(id)
      
      // 更新本地消息状态
      const message = messages.value.find(msg => msg.id === id)
      if (message) {
        message.isRead = false
        message.readAt = undefined
      }
      
      if (currentMessage.value?.id === id) {
        currentMessage.value.isRead = false
        currentMessage.value.readAt = undefined
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记未读失败'
      throw err
    }
  }
  
  /**
   * 批量操作消息
   * @param request 批量操作请求
   */
  async function batchOperation(request: BatchMessageOperationRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      await messageService.batchOperation(request)
      
      // 根据操作类型更新本地状态
      switch (request.operation) {
        case MessageOperation.MARK_AS_READ:
          request.messageIds.forEach(id => {
            const message = messages.value.find(msg => msg.id === id)
            if (message) {
              message.isRead = true
              message.readAt = new Date().toISOString()
            }
          })
          break
        case MessageOperation.MARK_AS_UNREAD:
          request.messageIds.forEach(id => {
            const message = messages.value.find(msg => msg.id === id)
            if (message) {
              message.isRead = false
              message.readAt = undefined
            }
          })
          break
        case MessageOperation.DELETE:
          messages.value = messages.value.filter(msg => !request.messageIds.includes(msg.id))
          break
        case MessageOperation.ARCHIVE:
          // 实现归档逻辑
          break
        case MessageOperation.RESTORE:
          // 实现恢复逻辑
          break
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '批量操作失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  // ==================== 会话管理 ====================
  
  /**
   * 获取会话列表
   * @param filter 会话筛选条件
   */
  async function fetchConversations(filter: ConversationFilter) {
    try {
      isLoading.value = true
      error.value = null
      
      const response = await messageService.getConversations(filter)
      conversations.value = response.items
      lastUpdated.value = new Date().toISOString()
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取会话列表失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 获取指定会话的详细信息
   * @param id 会话ID
   */
  async function fetchConversationById(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      const conversation = await messageService.getConversationById(id)
      currentConversation.value = conversation
      
      // 更新本地会话列表
      const index = conversations.value.findIndex(conv => conv.id === id)
      if (index !== -1) {
        conversations.value[index] = conversation
      } else {
        conversations.value.unshift(conversation)
      }
      
      return conversation
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取会话详情失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 创建新会话
   * @param conversation 会话创建请求
   */
  async function createConversation(conversation: CreateConversationRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const newConversation = await messageService.createConversation(conversation)
      conversations.value.unshift(newConversation)
      currentConversation.value = newConversation
      
      lastUpdated.value = new Date().toISOString()
      
      return newConversation
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建会话失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 更新会话信息
   * @param id 会话ID
   * @param conversation 会话更新请求
   */
  async function updateConversation(id: string, conversation: UpdateConversationRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const updatedConversation = await messageService.updateConversation(id, conversation)
      
      // 更新本地会话列表
      const index = conversations.value.findIndex(conv => conv.id === id)
      if (index !== -1) {
        conversations.value[index] = updatedConversation
      }
      
      if (currentConversation.value?.id === id) {
        currentConversation.value = updatedConversation
      }
      
      lastUpdated.value = new Date().toISOString()
      
      return updatedConversation
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新会话失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 删除会话
   * @param id 会话ID
   */
  async function deleteConversation(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      await messageService.deleteConversation(id)
      
      // 从本地会话列表中移除
      conversations.value = conversations.value.filter(conv => conv.id !== id)
      
      if (currentConversation.value?.id === id) {
        currentConversation.value = null
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除会话失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 设置当前会话
   * @param conversation 会话对象
   */
  function setCurrentConversation(conversation: Conversation | null) {
    currentConversation.value = conversation
    
    if (conversation) {
      // 标记会话为已读
      markConversationAsRead(conversation.id)
    }
  }
  
  /**
   * 标记会话为已读
   * @param conversationId 会话ID
   */
  async function markConversationAsRead(conversationId: string) {
    try {
      const conversation = conversations.value.find(conv => conv.id === conversationId)
      if (conversation && conversation.unreadCount > 0) {
        // 标记会话中的所有未读消息为已读
        const unreadMessages = messages.value.filter(msg => 
          msg.conversationId === conversationId && !msg.isRead
        )
        
        for (const message of unreadMessages) {
          await markAsRead(message.id)
        }
        
        // 更新会话未读计数
        conversation.unreadCount = 0
        conversation.hasUnreadMessages = false
        conversation.lastReadAt = new Date().toISOString()
      }
    } catch (err) {
      console.error('标记会话已读失败:', err)
    }
  }
  
  // ==================== 消息草稿管理 ====================
  
  /**
   * 获取消息草稿列表
   * @param filter 草稿筛选条件
   */
  async function fetchDrafts(filter: MessageFilter) {
    try {
      isLoading.value = true
      error.value = null
      
      const response = await messageService.getDrafts(filter)
      drafts.value = response.items
      lastUpdated.value = new Date().toISOString()
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取草稿列表失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 获取指定草稿的详细信息
   * @param id 草稿ID
   */
  async function fetchDraftById(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      const draft = await messageService.getDraftById(id)
      currentDraft.value = draft
      
      // 更新本地草稿列表
      const index = drafts.value.findIndex(d => d.id === id)
      if (index !== -1) {
        drafts.value[index] = draft
      } else {
        drafts.value.unshift(draft)
      }
      
      return draft
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取草稿详情失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 创建新草稿
   * @param draft 草稿创建请求
   */
  async function createDraft(draft: CreateMessageDraftRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const newDraft = await messageService.createDraft(draft)
      drafts.value.unshift(newDraft)
      currentDraft.value = newDraft
      
      lastUpdated.value = new Date().toISOString()
      
      return newDraft
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建草稿失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 更新草稿内容
   * @param id 草稿ID
   * @param draft 草稿更新请求
   */
  async function updateDraft(id: string, draft: UpdateMessageDraftRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const updatedDraft = await messageService.updateDraft(id, draft)
      
      // 更新本地草稿列表
      const index = drafts.value.findIndex(d => d.id === id)
      if (index !== -1) {
        drafts.value[index] = updatedDraft
      }
      
      if (currentDraft.value?.id === id) {
        currentDraft.value = updatedDraft
      }
      
      lastUpdated.value = new Date().toISOString()
      
      return updatedDraft
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新草稿失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 删除草稿
   * @param id 草稿ID
   */
  async function deleteDraft(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      await messageService.deleteDraft(id)
      
      // 从本地草稿列表中移除
      drafts.value = drafts.value.filter(d => d.id !== id)
      
      if (currentDraft.value?.id === id) {
        currentDraft.value = null
      }
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除草稿失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 发送草稿
   * @param id 草稿ID
   */
  async function sendDraft(id: string) {
    try {
      isSending.value = true
      error.value = null
      
      const message = await messageService.sendDraft(id)
      
      // 从草稿列表中移除
      drafts.value = drafts.value.filter(d => d.id !== id)
      
      if (currentDraft.value?.id === id) {
        currentDraft.value = null
      }
      
      // 添加到消息列表
      messages.value.unshift(message)
      
      success.value = '草稿发送成功'
      setTimeout(() => { success.value = null }, 3000)
      
      return message
    } catch (err) {
      error.value = err instanceof Error ? err.message : '发送草稿失败'
      throw err
    } finally {
      isSending.value = false
    }
  }
  
  /**
   * 自动保存草稿
   * @param id 草稿ID
   * @param content 草稿内容
   */
  async function autoSaveDraft(id: string, content: string) {
    try {
      // 清除之前的定时器
      if (autoSaveDraftTimer.value) {
        clearTimeout(autoSaveDraftTimer.value)
      }
      
      // 设置新的定时器
      autoSaveDraftTimer.value = setTimeout(async () => {
        try {
          await messageService.autoSaveDraft(id, content)
          
          // 更新本地草稿内容
          const draft = drafts.value.find(d => d.id === id)
          if (draft) {
            draft.content = content
            draft.lastAutoSavedAt = new Date().toISOString()
          }
          
          if (currentDraft.value?.id === id) {
            currentDraft.value.content = content
            currentDraft.value.lastAutoSavedAt = new Date().toISOString()
          }
        } catch (err) {
          console.error('自动保存草稿失败:', err)
        }
      }, 2000) // 2秒后自动保存
    } catch (err) {
      console.error('设置自动保存失败:', err)
    }
  }
  
  // ==================== 消息附件管理 ====================
  
  /**
   * 上传单个文件
   * @param request 文件上传请求
   */
  async function uploadFile(request: FileUploadRequest) {
    try {
      isUploading.value = true
      error.value = null
      
      const uploadId = `upload_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
      activeUploads.value.add(uploadId)
      
      const response = await messageService.uploadFile({
        ...request,
        onProgress: (progress) => {
          const uploadResponse = uploadingFiles.value.find(u => u.attachment?.fileName === request.file.name)
          if (uploadResponse) {
            uploadResponse.progress = progress
          } else {
            uploadingFiles.value.push({
              success: false,
              progress,
              error: undefined
            })
          }
          
          if (request.onProgress) {
            request.onProgress(progress)
          }
        },
        onSuccess: (attachment) => {
          attachments.value.push(attachment)
          activeUploads.value.delete(uploadId)
          
          if (request.onSuccess) {
            request.onSuccess(attachment)
          }
        },
        onError: (error) => {
          activeUploads.value.delete(uploadId)
          
          if (request.onError) {
            request.onError(error)
          }
        }
      })
      
      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : '上传文件失败'
      throw err
    } finally {
      isUploading.value = false
    }
  }
  
  /**
   * 批量上传文件
   * @param request 批量文件上传请求
   */
  async function uploadMultipleFiles(request: BatchFileUploadRequest) {
    try {
      isUploading.value = true
      error.value = null
      
      const responses = await messageService.uploadMultipleFiles({
        ...request,
        onProgress: (progress, fileName) => {
          const uploadResponse = uploadingFiles.value.find(u => u.attachment?.fileName === fileName)
          if (uploadResponse) {
            uploadResponse.progress = progress
          } else {
            uploadingFiles.value.push({
              success: false,
              progress,
              error: undefined
            })
          }
          
          if (request.onProgress) {
            request.onProgress(progress, fileName)
          }
        },
        onSuccess: (attachments) => {
          attachments.forEach(att => attachments.value.push(att))
          
          if (request.onSuccess) {
            request.onSuccess(attachments)
          }
        },
        onError: (errors) => {
          if (request.onError) {
            request.onError(errors)
          }
        }
      })
      
      return responses
    } catch (err) {
      error.value = err instanceof Error ? err.message : '批量上传文件失败'
      throw err
    } finally {
      isUploading.value = false
    }
  }
  
  /**
   * 删除附件
   * @param id 附件ID
   */
  async function deleteAttachment(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      await messageService.deleteAttachment(id)
      
      // 从本地附件列表中移除
      attachments.value = attachments.value.filter(att => att.id !== id)
      
      lastUpdated.value = new Date().toISOString()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除附件失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 下载附件
   * @param id 附件ID
   */
  async function downloadAttachment(id: string) {
    try {
      isLoading.value = true
      error.value = null
      
      const blob = await messageService.downloadAttachment(id)
      
      // 更新下载计数
      const attachment = attachments.value.find(att => att.id === id)
      if (attachment) {
        attachment.downloadCount++
      }
      
      return blob
    } catch (err) {
      error.value = err instanceof Error ? err.message : '下载附件失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  // ==================== 消息搜索和筛选 ====================
  
  /**
   * 搜索消息
   * @param request 搜索请求
   */
  async function searchMessages(request: MessageSearchRequest) {
    try {
      isLoading.value = true
      error.value = null
      
      const result = await messageService.searchMessages(request)
      searchResults.value = result
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '搜索消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 清除搜索结果
   */
  function clearSearchResults() {
    searchResults.value = null
  }
  
  /**
   * 设置消息筛选条件
   * @param filter 筛选条件
   */
  function setFilter(filter: Partial<MessageFilter>) {
    currentFilter.value = { ...currentFilter.value, ...filter }
  }
  
  /**
   * 重置筛选条件
   */
  function resetFilter() {
    currentFilter.value = {
      sortBy: MessageSort.CREATED_AT_DESC,
      page: 1,
      pageSize: 20
    }
  }
  
  // ==================== 消息统计和分析 ====================
  
  /**
   * 获取消息统计信息
   */
  async function fetchMessageStats() {
    try {
      isLoading.value = true
      error.value = null
      
      const stats = await messageService.getMessageStats()
      messageStats.value = stats
      
      return stats
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息统计失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 获取消息分析数据
   * @param filters 分析筛选条件
   */
  async function fetchMessageAnalytics(filters?: any) {
    try {
      isLoading.value = true
      error.value = null
      
      const analytics = await messageService.getMessageAnalytics(filters)
      messageAnalytics.value = analytics
      
      return analytics
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息分析失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  // ==================== 消息导入导出 ====================
  
  /**
   * 导出消息
   * @param options 导出选项
   */
  async function exportMessages(options: MessageExportOptions) {
    try {
      isLoading.value = true
      error.value = null
      
      const blob = await messageService.exportMessages(options)
      
      // 创建下载链接
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `messages_${new Date().toISOString().split('T')[0]}.${options.format.toLowerCase()}`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
      
      return blob
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导出消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 导入消息
   * @param data 导入数据
   */
  async function importMessages(data: MessageImportData) {
    try {
      isLoading.value = true
      error.value = null
      
      const result = await messageService.importMessages(data)
      
      if (result.isValid) {
        // 重新获取消息列表
        await fetchMessages()
        success.value = `成功导入 ${result.validCount} 条消息`
        setTimeout(() => { success.value = null }, 3000)
      } else {
        error.value = `导入失败: ${result.errors.join(', ')}`
      }
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导入消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  // ==================== 实时通信支持 ====================
  
  /**
   * 连接WebSocket
   */
  async function connectWebSocket() {
    try {
      if (websocketConnection.value && websocketConnection.value.readyState === WebSocket.OPEN) {
        return
      }
      
      // 获取实时连接令牌
      const token = await messageService.getRealtimeToken()
      
      // 创建WebSocket连接
      const wsUrl = `${import.meta.env.VITE_WS_URL}/messages?token=${token}`
      websocketConnection.value = new WebSocket(wsUrl)
      
      websocketConnection.value.onopen = () => {
        isRealtimeConnected.value = true
        console.log('WebSocket连接已建立')
      }
      
      websocketConnection.value.onmessage = (event) => {
        try {
          const update: MessageRealtimeUpdate = JSON.parse(event.data)
          handleMessageRealtimeUpdate(update)
        } catch (err) {
          console.error('处理实时更新失败:', err)
        }
      }
      
      websocketConnection.value.onclose = () => {
        isRealtimeConnected.value = false
        console.log('WebSocket连接已关闭')
        
        // 5秒后重连
        setTimeout(connectWebSocket, 5000)
      }
      
      websocketConnection.value.onerror = (err) => {
        console.error('WebSocket连接错误:', err)
        isRealtimeConnected.value = false
      }
    } catch (err) {
      console.error('连接WebSocket失败:', err)
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
   * 处理实时消息更新
   * @param update 实时更新数据
   */
  function handleMessageRealtimeUpdate(update: MessageRealtimeUpdate) {
    switch (update.type) {
      case 'message_created':
        handleNewMessage(update)
        break
      case 'message_updated':
        handleMessageUpdated(update)
        break
      case 'message_deleted':
        handleMessageDeleted(update)
        break
      case 'message_read':
        handleMessageRead(update)
        break
      case 'typing_start':
        handleTypingStart(update)
        break
      case 'typing_stop':
        handleTypingStop(update)
        break
    }
  }
  
  /**
   * 处理新消息
   * @param update 实时更新数据
   */
  function handleNewMessage(update: MessageRealtimeUpdate) {
    if (update.data && update.data.message) {
      const newMessage = update.data.message
      
      // 添加到消息列表
      const existingIndex = messages.value.findIndex(msg => msg.id === newMessage.id)
      if (existingIndex === -1) {
        messages.value.unshift(newMessage)
      }
      
      // 更新会话信息
      if (update.conversationId && currentConversation.value?.id === update.conversationId) {
        currentConversation.value.lastMessage = newMessage
        currentConversation.value.messageCount++
        currentConversation.value.lastActivity = newMessage.createdAt
      }
    }
  }
  
  /**
   * 处理消息更新
   * @param update 实时更新数据
   */
  function handleMessageUpdated(update: MessageRealtimeUpdate) {
    if (update.data && update.data.message) {
      const updatedMessage = update.data.message
      
      // 更新消息列表
      const index = messages.value.findIndex(msg => msg.id === updatedMessage.id)
      if (index !== -1) {
        messages.value[index] = updatedMessage
      }
      
      if (currentMessage.value?.id === updatedMessage.id) {
        currentMessage.value = updatedMessage
      }
    }
  }
  
  /**
   * 处理消息删除
   * @param update 实时更新数据
   */
  function handleMessageDeleted(update: MessageRealtimeUpdate) {
    // 从消息列表中移除
    messages.value = messages.value.filter(msg => msg.id !== update.messageId)
    
    if (currentMessage.value?.id === update.messageId) {
      currentMessage.value = null
    }
  }
  
  /**
   * 处理消息已读
   * @param update 实时更新数据
   */
  function handleMessageRead(update: MessageRealtimeUpdate) {
    const message = messages.value.find(msg => msg.id === update.messageId)
    if (message) {
      message.isRead = true
      message.readAt = new Date().toISOString()
    }
    
    if (currentMessage.value?.id === update.messageId) {
      currentMessage.value.isRead = true
      currentMessage.value.readAt = new Date().toISOString()
    }
  }
  
  /**
   * 处理用户开始输入
   * @param update 实时更新数据
   */
  function handleTypingStart(update: MessageRealtimeUpdate) {
    if (update.senderId && update.senderId !== getCurrentUserId()) {
      typingUsers.value.add(update.senderId)
      
      // 10秒后自动停止输入状态
      setTimeout(() => {
        typingUsers.value.delete(update.senderId)
      }, 10000)
    }
  }
  
  /**
   * 处理用户停止输入
   * @param update 实时更新数据
   */
  function handleTypingStop(update: MessageRealtimeUpdate) {
    if (update.senderId) {
      typingUsers.value.delete(update.senderId)
    }
  }
  
  /**
   * 发送正在输入状态
   * @param conversationId 会话ID
   */
  async function sendTypingStatus(conversationId: string) {
    try {
      if (isRealtimeConnected.value) {
        await messageService.sendTypingStatus(conversationId)
      }
    } catch (err) {
      console.error('发送输入状态失败:', err)
    }
  }
  
  /**
   * 停止输入状态
   * @param conversationId 会话ID
   */
  async function stopTypingStatus(conversationId: string) {
    try {
      if (isRealtimeConnected.value) {
        await messageService.stopTypingStatus(conversationId)
      }
    } catch (err) {
      console.error('停止输入状态失败:', err)
    }
  }
  
  /**
   * 获取当前用户ID
   */
  function getCurrentUserId(): string {
    // 这里应该从认证状态中获取用户ID
    // 暂时返回空字符串，实际实现需要从认证store中获取
    return ''
  }
  
  // ==================== 消息模板和过滤规则 ====================
  
  /**
   * 获取消息模板列表
   */
  async function fetchMessageTemplates() {
    try {
      isLoading.value = true
      error.value = null
      
      const templates = await messageService.getMessageTemplates()
      messageTemplates.value = templates
      
      return templates
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息模板失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 获取消息过滤规则列表
   */
  async function fetchMessageFilterRules() {
    try {
      isLoading.value = true
      error.value = null
      
      const rules = await messageService.getMessageFilterRules()
      messageFilterRules.value = rules
      
      return rules
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取消息过滤规则失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 使用模板创建消息
   * @param templateId 模板ID
   * @param variables 模板变量
   * @param receiverId 接收者ID
   */
  async function createMessageFromTemplate(
    templateId: string,
    variables: Record<string, any>,
    receiverId: string
  ) {
    try {
      isLoading.value = true
      error.value = null
      
      const message = await messageService.createMessageFromTemplate(templateId, variables, receiverId)
      messages.value.unshift(message)
      
      return message
    } catch (err) {
      error.value = err instanceof Error ? err.message : '使用模板创建消息失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  // ==================== 消息通知设置 ====================
  
  /**
   * 获取消息通知设置
   */
  async function fetchMessageNotificationSettings() {
    try {
      isLoading.value = true
      error.value = null
      
      const settings = await messageService.getMessageNotificationSettings()
      notificationPreferences.value = settings
      mutedConversations.value = new Set(settings.mutedConversations)
      
      return settings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取通知设置失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 更新消息通知设置
   * @param settings 通知设置
   */
  async function updateMessageNotificationSettings(settings: MessageNotificationPreferences) {
    try {
      isLoading.value = true
      error.value = null
      
      const updatedSettings = await messageService.updateMessageNotificationSettings(settings)
      notificationPreferences.value = updatedSettings
      mutedConversations.value = new Set(updatedSettings.mutedConversations)
      
      return updatedSettings
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新通知设置失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }
  
  /**
   * 设置会话静音状态
   * @param conversationId 会话ID
   * @param isMuted 是否静音
   */
  async function setConversationMuted(conversationId: string, isMuted: boolean) {
    try {
      await messageService.setConversationMuted(conversationId, isMuted)
      
      if (isMuted) {
        mutedConversations.value.add(conversationId)
      } else {
        mutedConversations.value.delete(conversationId)
      }
      
      // 更新本地会话状态
      const conversation = conversations.value.find(conv => conv.id === conversationId)
      if (conversation) {
        conversation.isMuted = isMuted
      }
      
      if (currentConversation.value?.id === conversationId) {
        currentConversation.value.isMuted = isMuted
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '设置会话静音失败'
      throw err
    }
  }
  
  // ==================== 工具方法 ====================
  
  /**
   * 清除错误信息
   */
  function clearError() {
    error.value = null
  }
  
  /**
   * 清除成功信息
   */
  function clearSuccess() {
    success.value = null
  }
  
  /**
   * 设置加载状态
   * @param loading 是否加载中
   */
  function setLoading(loading: boolean) {
    isLoading.value = loading
  }
  
  /**
   * 设置错误信息
   * @param err 错误信息
   */
  function setError(err: string | null) {
    error.value = err
  }
  
  /**
   * 重置所有状态
   */
  function resetState() {
    messages.value = []
    currentMessage.value = null
    messageStats.value = null
    messageAnalytics.value = null
    conversations.value = []
    currentConversation.value = null
    drafts.value = []
    currentDraft.value = null
    attachments.value = []
    uploadingFiles.value = []
    activeUploads.value.clear()
    searchResults.value = null
    messageTemplates.value = []
    messageFilterRules.value = []
    notificationPreferences.value = null
    mutedConversations.value.clear()
    typingUsers.value.clear()
    onlineUsers.value.clear()
    
    isLoading.value = false
    isSending.value = false
    isUploading.value = false
    error.value = null
    success.value = null
    lastUpdated.value = null
    
    disconnectWebSocket()
  }
  
  /**
   * 清理资源
   */
  function cleanup() {
    if (autoSaveDraftTimer.value) {
      clearTimeout(autoSaveDraftTimer.value)
    }
    disconnectWebSocket()
  }
  
  // ==================== 持久化存储 ====================
  
  /**
   * 从本地存储恢复状态
   */
  function loadFromStorage() {
    try {
      const saved = localStorage.getItem('message_store_state')
      if (saved) {
        const state = JSON.parse(saved)
        
        // 恢复基本状态
        if (state.currentFilter) {
          currentFilter.value = state.currentFilter
        }
        if (state.mutedConversations) {
          mutedConversations.value = new Set(state.mutedConversations)
        }
        if (state.notificationPreferences) {
          notificationPreferences.value = state.notificationPreferences
        }
      }
    } catch (err) {
      console.error('从本地存储恢复状态失败:', err)
    }
  }
  
  /**
   * 保存状态到本地存储
   */
  function saveToStorage() {
    try {
      const state = {
        currentFilter: currentFilter.value,
        mutedConversations: Array.from(mutedConversations.value),
        notificationPreferences: notificationPreferences.value
      }
      localStorage.setItem('message_store_state', JSON.stringify(state))
    } catch (err) {
      console.error('保存状态到本地存储失败:', err)
    }
  }
  
  // 监听状态变化，自动保存到本地存储
  watch([currentFilter, mutedConversations, notificationPreferences], () => {
    saveToStorage()
  }, { deep: true })
  
  // ==================== 初始化 ====================
  
  // 初始化时从本地存储恢复状态
  loadFromStorage()
  
  // 组件卸载时清理资源
  if (typeof window !== 'undefined') {
    window.addEventListener('beforeunload', cleanup)
  }
  
  // ==================== 返回值 ====================
  
  return {
    // 状态
    messages,
    currentMessage,
    messageStats,
    messageAnalytics,
    conversations,
    currentConversation,
    drafts,
    currentDraft,
    attachments,
    uploadingFiles,
    activeUploads,
    searchResults,
    currentFilter,
    websocketConnection,
    isRealtimeConnected,
    typingUsers,
    onlineUsers,
    notificationPreferences,
    mutedConversations,
    messageTemplates,
    messageFilterRules,
    isLoading,
    isSending,
    isUploading,
    error,
    success,
    lastUpdated,
    
    // 计算属性
    hasError,
    hasSuccess,
    isInitialized,
    totalMessages,
    totalConversations,
    totalDrafts,
    totalAttachments,
    unreadMessages,
    unreadCount,
    sentMessages,
    receivedMessages,
    activeConversations,
    archivedConversations,
    pinnedConversations,
    mutedConversationsList,
    conversationsWithUnread,
    activeDrafts,
    scheduledDrafts,
    expiredDrafts,
    uploadingAttachments,
    failedUploads,
    totalAttachmentSize,
    isTypingInConversation,
    currentUserOnline,
    canSendMessage,
    canEditConversation,
    canDeleteConversation,
    
    // 消息操作
    fetchMessages,
    fetchMessageById,
    createMessage,
    updateMessage,
    deleteMessage,
    markAsRead,
    markAsUnread,
    batchOperation,
    
    // 会话操作
    fetchConversations,
    fetchConversationById,
    createConversation,
    updateConversation,
    deleteConversation,
    setCurrentConversation,
    markConversationAsRead,
    
    // 草稿操作
    fetchDrafts,
    fetchDraftById,
    createDraft,
    updateDraft,
    deleteDraft,
    sendDraft,
    autoSaveDraft,
    
    // 附件操作
    uploadFile,
    uploadMultipleFiles,
    deleteAttachment,
    downloadAttachment,
    
    // 搜索和筛选
    searchMessages,
    clearSearchResults,
    setFilter,
    resetFilter,
    
    // 统计和分析
    fetchMessageStats,
    fetchMessageAnalytics,
    
    // 导入导出
    exportMessages,
    importMessages,
    
    // 实时通信
    connectWebSocket,
    disconnectWebSocket,
    sendTypingStatus,
    stopTypingStatus,
    
    // 模板和过滤规则
    fetchMessageTemplates,
    fetchMessageFilterRules,
    createMessageFromTemplate,
    
    // 通知设置
    fetchMessageNotificationSettings,
    updateMessageNotificationSettings,
    setConversationMuted,
    
    // 工具方法
    clearError,
    clearSuccess,
    setLoading,
    setError,
    resetState,
    cleanup,
    loadFromStorage,
    saveToStorage,
    
    // Pinia重置方法
    $reset: resetState
  }
})