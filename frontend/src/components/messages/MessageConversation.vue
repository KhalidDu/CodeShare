<template>
  <div class="message-conversation-container">
    <!-- 会话头部 -->
    <div class="conversation-header">
      <div class="header-left">
        <div class="conversation-info">
          <h3 class="conversation-title">{{ conversation.title }}</h3>
          <p v-if="conversation.description" class="conversation-description">
            {{ conversation.description }}
          </p>
        </div>
        <div class="conversation-meta">
          <span class="participant-count">
            <i class="fas fa-users"></i>
            {{ conversation.participants.length }} 位参与者
          </span>
          <span class="message-count">
            <i class="fas fa-comments"></i>
            {{ conversation.messageCount }} 条消息
          </span>
          <span v-if="conversation.unreadCount > 0" class="unread-count">
            <i class="fas fa-circle"></i>
            {{ conversation.unreadCount }} 条未读
          </span>
        </div>
      </div>
      <div class="header-right">
        <div class="conversation-actions">
          <button
            v-if="conversation.canEdit"
            @click="handleEdit"
            class="action-btn"
            title="编辑会话"
          >
            <i class="fas fa-edit"></i>
          </button>
          <button
            @click="togglePin"
            class="action-btn"
            :class="{ active: conversation.isPinned }"
            :title="conversation.isPinned ? '取消置顶' : '置顶会话'"
          >
            <i class="fas fa-thumbtack"></i>
          </button>
          <button
            @click="toggleMute"
            class="action-btn"
            :class="{ active: conversation.isMuted }"
            :title="conversation.isMuted ? '取消静音' : '静音会话'"
          >
            <i class="fas" :class="conversation.isMuted ? 'fa-volume-mute' : 'fa-volume-up'"></i>
          </button>
          <button
            v-if="conversation.canDelete"
            @click="handleDelete"
            class="action-btn delete"
            title="删除会话"
          >
            <i class="fas fa-trash"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- 参与者列表 -->
    <div v-if="showParticipants" class="participants-section">
      <div class="participants-header">
        <h4>参与者</h4>
        <button
          v-if="conversation.canAddParticipants"
          @click="showAddParticipant = true"
          class="add-participant-btn"
        >
          <i class="fas fa-plus"></i>
          添加
        </button>
      </div>
      <div class="participants-list">
        <div
          v-for="participant in conversation.participants"
          :key="participant.userId"
          class="participant-item"
        >
          <div class="participant-avatar">
            <img
              v-if="participant.userAvatar"
              :src="participant.userAvatar"
              :alt="participant.userName"
              class="avatar-image"
            />
            <div v-else class="avatar-placeholder">
              {{ getAvatarInitial(participant.userName) }}
            </div>
            <div
              v-if="isParticipantOnline(participant.userId)"
              class="online-indicator"
            ></div>
          </div>
          <div class="participant-info">
            <div class="participant-name">{{ participant.userName }}</div>
            <div class="participant-role">{{ participant.role }}</div>
            <div class="participant-status">
              <span v-if="participant.unreadCount > 0" class="unread-badge">
                {{ participant.unreadCount }} 条未读
              </span>
              <span v-if="participant.lastReadAt" class="last-read">
                最后阅读: {{ formatTime(participant.lastReadAt) }}
              </span>
            </div>
          </div>
          <div v-if="conversation.canRemoveParticipants" class="participant-actions">
            <button
              @click="removeParticipant(participant.userId)"
              class="remove-btn"
              title="移除参与者"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 消息区域 -->
    <div class="messages-section">
      <!-- 消息列表 -->
      <div
        ref="messagesContainer"
        class="messages-container"
        @scroll="handleScroll"
      >
        <!-- 加载更多 -->
        <div v-if="hasMoreMessages && !loadingMore" class="load-more">
          <button @click="loadMoreMessages" class="load-more-btn">
            加载更多消息
          </button>
        </div>

        <!-- 加载中 -->
        <div v-if="loadingMore" class="loading-more">
          <div class="loading-spinner"></div>
          <span>加载中...</span>
        </div>

        <!-- 消息列表 -->
        <div v-if="messages.length > 0" class="messages-list">
          <div
            v-for="(message, index) in messages"
            :key="message.id"
            class="message-wrapper"
          >
            <!-- 日期分隔符 -->
            <div
              v-if="shouldShowDateSeparator(message, index)"
              class="date-separator"
            >
              <div class="separator-line"></div>
              <div class="separator-text">
                {{ formatDateSeparator(message.createdAt) }}
              </div>
              <div class="separator-line"></div>
            </div>

            <!-- 消息气泡 -->
            <div
              :class="getMessageBubbleClasses(message)"
              @mouseenter="handleMessageHover(message)"
              @mouseleave="handleMessageLeave(message)"
            >
              <!-- 消息内容 -->
              <div class="message-bubble">
                <!-- 发送者信息 -->
                <div v-if="!isCompactMessage(message)" class="message-sender">
                  <img
                    v-if="message.senderAvatar"
                    :src="message.senderAvatar"
                    :alt="message.senderName"
                    class="sender-avatar"
                  />
                  <div v-else class="sender-avatar-placeholder">
                    {{ getAvatarInitial(message.senderName) }}
                  </div>
                  <div class="sender-info">
                    <div class="sender-name">{{ message.senderName }}</div>
                    <div class="message-time">{{ formatMessageTime(message.createdAt) }}</div>
                  </div>
                </div>

                <!-- 消息主体 -->
                <div class="message-main">
                  <!-- 消息主题 -->
                  <div v-if="message.subject" class="message-subject">
                    {{ message.subject }}
                  </div>

                  <!-- 消息内容 -->
                  <div class="message-content">
                    <div class="message-text">{{ message.content }}</div>
                  </div>

                  <!-- 附件 -->
                  <div
                    v-if="message.attachments && message.attachments.length > 0"
                    class="message-attachments"
                  >
                    <MessageAttachmentDisplay
                      :attachments="message.attachments"
                      :message-id="message.id"
                    />
                  </div>

                  <!-- 消息状态 -->
                  <div class="message-status">
                    <span
                      v-if="message.priority === MessagePriority.URGENT"
                      class="priority-badge urgent"
                    >
                      紧急
                    </span>
                    <span
                      v-else-if="message.priority === MessagePriority.HIGH"
                      class="priority-badge high"
                    >
                      高优先级
                    </span>
                    <div class="status-icons">
                      <i
                        v-if="message.status === MessageStatus.SENT"
                        class="fas fa-paper-plane text-gray-400"
                        title="已发送"
                      ></i>
                      <i
                        v-else-if="message.status === MessageStatus.DELIVERED"
                        class="fas fa-check text-gray-400"
                        title="已送达"
                      ></i>
                      <i
                        v-else-if="message.status === MessageStatus.READ"
                        class="fas fa-check-double text-blue-500"
                        title="已读"
                      ></i>
                      <i
                        v-else-if="message.status === MessageStatus.FAILED"
                        class="fas fa-exclamation-triangle text-red-500"
                        title="发送失败"
                      ></i>
                    </div>
                  </div>
                </div>

                <!-- 消息操作 -->
                <div
                  v-if="hoveredMessageId === message.id"
                  class="message-actions"
                >
                  <button
                    v-if="!message.isRead"
                    @click="markAsRead(message)"
                    class="action-btn"
                    title="标记已读"
                  >
                    <i class="fas fa-check"></i>
                  </button>
                  <button
                    v-if="message.canReply"
                    @click="replyToMessage(message)"
                    class="action-btn"
                    title="回复"
                  >
                    <i class="fas fa-reply"></i>
                  </button>
                  <button
                    v-if="message.canForward"
                    @click="forwardMessage(message)"
                    class="action-btn"
                    title="转发"
                  >
                    <i class="fas fa-share"></i>
                  </button>
                  <button
                    v-if="message.canEdit"
                    @click="editMessage(message)"
                    class="action-btn"
                    title="编辑"
                  >
                    <i class="fas fa-edit"></i>
                  </button>
                  <button
                    v-if="message.canDelete"
                    @click="deleteMessage(message)"
                    class="action-btn delete"
                    title="删除"
                  >
                    <i class="fas fa-trash"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 空状态 -->
        <div v-else-if="!loading" class="empty-state">
          <div class="empty-icon">
            <i class="fas fa-comments"></i>
          </div>
          <h3>暂无消息</h3>
          <p>开始发送第一条消息吧</p>
        </div>

        <!-- 加载状态 -->
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <span>加载中...</span>
        </div>
      </div>

      <!-- 输入区域 -->
      <div v-if="conversation.canSendMessage" class="input-section">
        <!-- 回复提示 -->
        <div v-if="replyingTo" class="reply-indicator">
          <div class="reply-content">
            <span class="reply-label">回复 {{ replyingTo.senderName }}:</span>
            <span class="reply-text">{{ replyingTo.content }}</span>
          </div>
          <button @click="cancelReply" class="cancel-reply">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- 输入框 -->
        <div class="input-container">
          <div class="input-actions">
            <button
              @click="showEmojiPicker = !showEmojiPicker"
              class="input-action-btn"
              title="表情"
            >
              <i class="fas fa-smile"></i>
            </button>
            <button
              @click="triggerFileInput"
              class="input-action-btn"
              title="附件"
            >
              <i class="fas fa-paperclip"></i>
            </button>
          </div>
          
          <textarea
            ref="messageInput"
            v-model="newMessage"
            placeholder="输入消息..."
            class="message-input"
            :rows="inputRows"
            @keydown.enter.prevent="handleEnterKey"
            @input="handleInputChange"
          ></textarea>
          
          <div class="input-footer">
            <div class="input-options">
              <select v-model="messagePriority" class="priority-select">
                <option :value="MessagePriority.NORMAL">普通</option>
                <option :value="MessagePriority.HIGH">高优先级</option>
                <option :value="MessagePriority.URGENT">紧急</option>
              </select>
            </div>
            <div class="input-actions-right">
              <span class="char-count">{{ newMessage.length }} / {{ maxMessageLength }}</span>
              <button
                @click="sendMessage"
                class="send-btn"
                :disabled="!canSendMessage"
              >
                <i class="fas fa-paper-plane"></i>
              </button>
            </div>
          </div>
        </div>

        <!-- 附件上传 -->
        <input
          ref="fileInputRef"
          type="file"
          multiple
          @change="handleFileChange"
          class="file-input"
        />

        <!-- 表情选择器 -->
        <div v-if="showEmojiPicker" class="emoji-picker">
          <div class="emoji-grid">
            <button
              v-for="emoji in commonEmojis"
              :key="emoji"
              @click="insertEmoji(emoji)"
              class="emoji-btn"
            >
              {{ emoji }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 添加参与者模态框 -->
    <div v-if="showAddParticipant" class="modal-overlay">
      <div class="modal-content">
        <div class="modal-header">
          <h3>添加参与者</h3>
          <button @click="showAddParticipant = false" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="search-container">
            <input
              v-model="participantSearch"
              type="text"
              placeholder="搜索用户..."
              class="search-input"
            />
          </div>
          <div class="user-list">
            <div
              v-for="user in filteredUsers"
              :key="user.id"
              @click="addParticipant(user)"
              class="user-item"
            >
              <img
                v-if="user.avatar"
                :src="user.avatar"
                :alt="user.name"
                class="user-avatar"
              />
              <div v-else class="user-avatar-placeholder">
                {{ getAvatarInitial(user.name) }}
              </div>
              <div class="user-info">
                <div class="user-name">{{ user.name }}</div>
                <div v-if="user.email" class="user-email">{{ user.email }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { useMessageStore } from '@/stores/message'
import { messageService } from '@/services/messageService'
import type {
  Message,
  Conversation,
  CreateMessageRequest,
  MessagePriority,
  MessageStatus,
  User
} from '@/types/message'
import MessageAttachmentDisplay from './MessageAttachmentDisplay.vue'

// 定义Props
interface Props {
  conversation: Conversation
  showParticipants?: boolean
  autoLoad?: boolean
  enableRealtime?: boolean
  maxMessageLength?: number
  commonEmojis?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  showParticipants: true,
  autoLoad: true,
  enableRealtime: true,
  maxMessageLength: 1000,
  commonEmojis: () => ['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑', '😬', '🙄', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐', '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕', '🤑', '🤠', '😈', '👿', '👹', '👺', '🤡', '💩', '👻', '💀', '☠️', '👽', '👾', '🤖', '🎃', '😺', '😸', '😹', '😻', '😼', '😽', '🙀', '😿', '😾']
})

// 定义Emits
interface Emits {
  (e: 'edit-conversation', conversation: Conversation): void
  (e: 'delete-conversation', conversationId: string): void
  (e: 'message-sent', message: Message): void
  (e: 'message-deleted', messageId: string): void
  (e: 'participant-added', userId: string): void
  (e: 'participant-removed', userId: string): void
}

const emit = defineEmits<Emits>()

// Store
const messageStore = useMessageStore()

// 响应式状态
const messagesContainer = ref<HTMLElement>()
const messageInput = ref<HTMLTextAreaElement>()
const fileInputRef = ref<HTMLInputElement>()

const loading = ref(false)
const loadingMore = ref(false)
const hasMoreMessages = ref(true)
const currentPage = ref(1)
const pageSize = 20

const messages = ref<Message[]>([])
const newMessage = ref('')
const messagePriority = ref(MessagePriority.NORMAL)
const replyingTo = ref<Message | null>(null)
const hoveredMessageId = ref<string | null>(null)
const showEmojiPicker = ref(false)
const showAddParticipant = ref(false)
const participantSearch = ref('')
const inputRows = ref(1)

const onlineUsers = ref<Set<string>>(new Set())

// 实时相关
const realtimeSubscription = ref<{ unsubscribe: () => void } | null>(null)

// 计算属性
const filteredUsers = computed(() => {
  if (!participantSearch.value) return []
  
  const query = participantSearch.value.toLowerCase()
  return props.conversation.participants
    .filter(p => !onlineUsers.value.has(p.userId))
    .filter(p => 
      p.userName.toLowerCase().includes(query) ||
      p.userId.toLowerCase().includes(query)
    )
})

const canSendMessage = computed(() => 
  newMessage.value.trim().length > 0 && 
  newMessage.value.length <= props.maxMessageLength &&
  props.conversation.canSendMessage
)

const isCompactMessage = (message: Message) => {
  if (!messages.value.length) return false
  const prevMessage = messages.value[messages.value.indexOf(message) - 1]
  return prevMessage && 
         prevMessage.senderId === message.senderId &&
         new Date(message.createdAt).getTime() - new Date(prevMessage.createdAt).getTime() < 300000 // 5分钟内
}

// 方法定义
const loadMessages = async (page: number = 1) => {
  try {
    loading.value = true
    const filter = {
      conversationId: props.conversation.id,
      page,
      pageSize,
      sortBy: 'CREATED_AT_DESC' as const
    }
    
    const response = await messageService.getConversationMessages(
      props.conversation.id,
      filter
    )
    
    if (page === 1) {
      messages.value = response.data
    } else {
      messages.value = [...response.data, ...messages.value]
    }
    
    hasMoreMessages.value = response.data.length === pageSize
    currentPage.value = page
    
    // 标记会话为已读
    if (props.conversation.unreadCount > 0) {
      await messageStore.markConversationAsRead(props.conversation.id)
    }
  } catch (error) {
    console.error('加载消息失败:', error)
  } finally {
    loading.value = false
  }
}

const loadMoreMessages = async () => {
  if (loadingMore.value || !hasMoreMessages.value) return
  
  try {
    loadingMore.value = true
    await loadMessages(currentPage.value + 1)
  } catch (error) {
    console.error('加载更多消息失败:', error)
  } finally {
    loadingMore.value = false
  }
}

const sendMessage = async () => {
  if (!canSendMessage.value) return
  
  try {
    const messageData: CreateMessageRequest = {
      receiverId: '', // 会话消息，不需要指定接收者
      content: newMessage.value,
      messageType: 'USER' as any,
      priority: messagePriority.value,
      conversationId: props.conversation.id,
      parentId: replyingTo.value?.id
    }
    
    const message = await messageStore.createMessage(messageData)
    
    // 添加到消息列表
    messages.value.push(message)
    
    // 清空输入框
    newMessage.value = ''
    replyingTo.value = null
    inputRows.value = 1
    
    // 滚动到底部
    await nextTick()
    scrollToBottom()
    
    emit('message-sent', message)
  } catch (error) {
    console.error('发送消息失败:', error)
  }
}

const markAsRead = async (message: Message) => {
  try {
    await messageService.markAsRead(message.id)
    message.isRead = true
    message.readAt = new Date().toISOString()
  } catch (error) {
    console.error('标记已读失败:', error)
  }
}

const replyToMessage = (message: Message) => {
  replyingTo.value = message
  messageInput.value?.focus()
}

const forwardMessage = (message: Message) => {
  // 实现转发逻辑
  console.log('转发消息:', message)
}

const editMessage = (message: Message) => {
  // 实现编辑逻辑
  console.log('编辑消息:', message)
}

const deleteMessage = async (message: Message) => {
  try {
    await messageService.deleteMessage(message.id)
    messages.value = messages.value.filter(m => m.id !== message.id)
    emit('message-deleted', message.id)
  } catch (error) {
    console.error('删除消息失败:', error)
  }
}

const togglePin = async () => {
  try {
    await messageService.setConversationPinned(
      props.conversation.id,
      !props.conversation.isPinned
    )
  } catch (error) {
    console.error('切换置顶状态失败:', error)
  }
}

const toggleMute = async () => {
  try {
    await messageService.setConversationMuted(
      props.conversation.id,
      !props.conversation.isMuted
    )
  } catch (error) {
    console.error('切换静音状态失败:', error)
  }
}

const handleEdit = () => {
  emit('edit-conversation', props.conversation)
}

const handleDelete = () => {
  emit('delete-conversation', props.conversation.id)
}

const removeParticipant = async (userId: string) => {
  try {
    await messageService.removeConversationParticipant(props.conversation.id, userId)
    emit('participant-removed', userId)
  } catch (error) {
    console.error('移除参与者失败:', error)
  }
}

const addParticipant = async (user: User) => {
  try {
    await messageService.addConversationParticipant(props.conversation.id, [user.id])
    emit('participant-added', user.id)
    showAddParticipant.value = false
    participantSearch.value = ''
  } catch (error) {
    console.error('添加参与者失败:', error)
  }
}

const handleMessageHover = (message: Message) => {
  hoveredMessageId.value = message.id
}

const handleMessageLeave = (message: Message) => {
  hoveredMessageId.value = null
}

const handleScroll = () => {
  const container = messagesContainer.value
  if (!container) return
  
  // 检查是否滚动到顶部，加载更多消息
  if (container.scrollTop === 0 && hasMoreMessages.value && !loadingMore.value) {
    loadMoreMessages()
  }
}

const handleEnterKey = (event: KeyboardEvent) => {
  if (event.shiftKey) {
    // Shift+Enter 换行
    return
  }
  // Enter 发送消息
  event.preventDefault()
  sendMessage()
}

const handleInputChange = () => {
  // 动态调整输入框高度
  const lines = newMessage.value.split('\n').length
  inputRows.value = Math.max(1, Math.min(5, lines))
}

const triggerFileInput = () => {
  fileInputRef.value?.click()
}

const handleFileChange = (event: Event) => {
  const files = (event.target as HTMLInputElement).files
  if (!files || files.length === 0) return
  
  // 处理文件上传逻辑
  console.log('上传文件:', files)
}

const insertEmoji = (emoji: string) => {
  newMessage.value += emoji
  showEmojiPicker.value = false
  messageInput.value?.focus()
}

const cancelReply = () => {
  replyingTo.value = null
  messageInput.value?.focus()
}

const scrollToBottom = () => {
  const container = messagesContainer.value
  if (container) {
    container.scrollTop = container.scrollHeight
  }
}

const getAvatarInitial = (name: string) => {
  return name.charAt(0).toUpperCase()
}

const formatTime = (timeString: string) => {
  const date = new Date(timeString)
  const now = new Date()
  const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60))
  
  if (diffInMinutes < 1) {
    return '刚刚'
  } else if (diffInMinutes < 60) {
    return `${diffInMinutes}分钟前`
  } else if (diffInMinutes < 1440) {
    const hours = Math.floor(diffInMinutes / 60)
    return `${hours}小时前`
  } else if (diffInMinutes < 10080) {
    const days = Math.floor(diffInMinutes / 1440)
    return `${days}天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

const formatMessageTime = (timeString: string) => {
  const date = new Date(timeString)
  return date.toLocaleTimeString('zh-CN', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

const formatDateSeparator = (timeString: string) => {
  const date = new Date(timeString)
  const today = new Date()
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)
  
  if (date.toDateString() === today.toDateString()) {
    return '今天'
  } else if (date.toDateString() === yesterday.toDateString()) {
    return '昨天'
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

const shouldShowDateSeparator = (message: Message, index: number) => {
  if (index === 0) return true
  
  const prevMessage = messages.value[index - 1]
  const currentDate = new Date(message.createdAt).toDateString()
  const prevDate = new Date(prevMessage.createdAt).toDateString()
  
  return currentDate !== prevDate
}

const getMessageBubbleClasses = (message: Message) => {
  const classes = ['message-bubble-wrapper']
  
  if (message.isSender) {
    classes.push('sent')
  } else {
    classes.push('received')
  }
  
  if (isCompactMessage(message)) {
    classes.push('compact')
  }
  
  return classes.join(' ')
}

const isParticipantOnline = (userId: string) => {
  return onlineUsers.value.has(userId)
}

const setupRealtime = async () => {
  if (!props.enableRealtime) return
  
  try {
    // 订阅会话实时更新
    const subscription = await messageStore.subscribeToConversationUpdates(
      props.conversation.id,
      (update) => {
        // 处理实时消息更新
        console.log('收到实时更新:', update)
      }
    )
    
    realtimeSubscription.value = subscription
  } catch (error) {
    console.error('设置实时更新失败:', error)
  }
}

// 监听器
watch(() => props.conversation, (newConversation) => {
  if (newConversation) {
    messages.value = []
    currentPage.value = 1
    hasMoreMessages.value = true
    
    if (props.autoLoad) {
      loadMessages()
    }
  }
}, { immediate: true })

// 生命周期
onMounted(async () => {
  if (props.autoLoad && props.conversation) {
    await loadMessages()
    scrollToBottom()
  }
  
  if (props.enableRealtime) {
    await setupRealtime()
  }
})

onUnmounted(() => {
  if (realtimeSubscription.value) {
    realtimeSubscription.value.unsubscribe()
  }
})

// 暴露方法
defineExpose({
  loadMessages,
  sendMessage,
  scrollToBottom,
  replyToMessage
})
</script>

<style scoped>
.message-conversation-container {
  @apply flex flex-col h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg;
}

.conversation-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.header-left {
  @apply flex-1;
}

.conversation-info {
  @apply mb-2;
}

.conversation-title {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.conversation-description {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.conversation-meta {
  @apply flex items-center space-x-4 text-sm text-gray-500 dark:text-gray-400;
}

.participant-count,
.message-count {
  @apply flex items-center space-x-1;
}

.unread-count {
  @apply flex items-center space-x-1 text-blue-600 dark:text-blue-400;
}

.header-right {
  @apply flex items-center space-x-2;
}

.conversation-actions {
  @apply flex items-center space-x-1;
}

.action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.action-btn.active {
  @apply text-blue-600 dark:text-blue-400;
}

.action-btn.delete {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

.participants-section {
  @apply border-b border-gray-200 dark:border-gray-700;
}

.participants-header {
  @apply flex items-center justify-between p-4;
}

.participants-header h4 {
  @apply font-medium text-gray-900 dark:text-white;
}

.add-participant-btn {
  @apply px-3 py-1 bg-blue-600 text-white text-sm rounded-lg 
         hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.participants-list {
  @apply px-4 pb-4 space-y-2;
}

.participant-item {
  @apply flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700;
}

.participant-avatar {
  @apply relative;
}

.avatar-image {
  @apply w-10 h-10 rounded-full object-cover;
}

.avatar-placeholder {
  @apply w-10 h-10 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-sm;
}

.online-indicator {
  @apply absolute bottom-0 right-0 w-3 h-3 bg-green-500 rounded-full 
         border-2 border-white dark:border-gray-800;
}

.participant-info {
  @apply flex-1;
}

.participant-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.participant-role {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.participant-status {
  @apply flex items-center space-x-2 text-xs;
}

.unread-badge {
  @apply px-2 py-1 bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300 
         rounded-full;
}

.last-read {
  @apply text-gray-500 dark:text-gray-400;
}

.participant-actions {
  @apply flex-shrink-0;
}

.remove-btn {
  @apply p-1 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-200 
         dark:hover:bg-gray-600;
}

.messages-section {
  @apply flex-1 flex flex-col;
}

.messages-container {
  @apply flex-1 overflow-y-auto p-4 space-y-4;
}

.load-more {
  @apply flex justify-center py-2;
}

.load-more-btn {
  @apply px-4 py-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 
         rounded-lg hover:bg-gray-300 dark:hover:bg-gray-600;
}

.loading-more {
  @apply flex items-center justify-center space-x-2 py-2 text-gray-500 dark:text-gray-400;
}

.loading-spinner {
  @apply w-4 h-4 border-2 border-gray-300 border-t-blue-600 rounded-full animate-spin;
}

.messages-list {
  @apply space-y-4;
}

.message-wrapper {
  @apply relative;
}

.date-separator {
  @apply flex items-center space-x-4 my-4;
}

.separator-line {
  @apply flex-1 h-px bg-gray-300 dark:bg-gray-600;
}

.separator-text {
  @apply px-2 text-sm text-gray-500 dark:text-gray-400;
}

.message-bubble-wrapper {
  @apply flex items-start space-x-2;
}

.message-bubble-wrapper.sent {
  @apply flex-row-reverse space-x-reverse;
}

.message-bubble-wrapper.compact {
  @apply mt-1;
}

.message-bubble {
  @apply max-w-xs lg:max-w-md;
}

.message-bubble-wrapper.sent .message-bubble {
  @apply bg-blue-600 text-white;
}

.message-bubble-wrapper.received .message-bubble {
  @apply bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white;
}

.message-sender {
  @apply flex items-center space-x-2 mb-2;
}

.sender-avatar {
  @apply w-8 h-8 rounded-full object-cover;
}

.sender-avatar-placeholder {
  @apply w-8 h-8 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-xs;
}

.sender-info {
  @apply flex-1;
}

.sender-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.message-time {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.message-main {
  @apply space-y-2;
}

.message-subject {
  @apply font-medium;
}

.message-content {
  @apply break-words;
}

.message-text {
  @apply whitespace-pre-wrap;
}

.message-status {
  @apply flex items-center justify-between mt-2;
}

.priority-badge {
  @apply px-2 py-1 text-xs rounded-full;
}

.priority-badge.urgent {
  @apply bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300;
}

.priority-badge.high {
  @apply bg-yellow-100 dark:bg-yellow-900 text-yellow-700 dark:text-yellow-300;
}

.status-icons {
  @apply flex items-center space-x-1;
}

.message-actions {
  @apply absolute top-2 right-2 flex items-center space-x-1 opacity-0 
         transition-opacity duration-200;
}

.message-bubble-wrapper:hover .message-actions {
  @apply opacity-100;
}

.empty-state {
  @apply flex flex-col items-center justify-center h-96 space-y-4 text-center;
}

.empty-state .empty-icon {
  @apply w-16 h-16 text-gray-400 dark:text-gray-600;
}

.empty-state h3 {
  @apply text-lg font-medium text-gray-900 dark:text-white;
}

.empty-state p {
  @apply text-gray-500 dark:text-gray-400;
}

.loading-state {
  @apply flex items-center justify-center space-x-2 py-8;
}

.input-section {
  @apply border-t border-gray-200 dark:border-gray-700 p-4;
}

.reply-indicator {
  @apply flex items-center justify-between p-2 bg-blue-50 dark:bg-blue-900 
         rounded-lg mb-2;
}

.reply-content {
  @apply flex-1 space-x-2;
}

.reply-label {
  @apply text-sm font-medium text-blue-600 dark:text-blue-400;
}

.reply-text {
  @apply text-sm text-gray-600 dark:text-gray-300 truncate;
}

.cancel-reply {
  @apply p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300;
}

.input-container {
  @apply relative;
}

.input-actions {
  @apply absolute top-2 left-2 flex items-center space-x-1;
}

.input-action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700;
}

.message-input {
  @apply w-full pl-12 pr-32 py-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none;
}

.input-footer {
  @apply absolute bottom-2 right-2 flex items-center space-x-2;
}

.input-options {
  @apply flex items-center space-x-2;
}

.priority-select {
  @apply text-sm border border-gray-300 dark:border-gray-600 rounded 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white;
}

.input-actions-right {
  @apply flex items-center space-x-2;
}

.char-count {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.send-btn {
  @apply p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50;
}

.file-input {
  @apply hidden;
}

.emoji-picker {
  @apply absolute bottom-full left-0 mb-2 bg-white dark:bg-gray-700 
         border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg p-2;
}

.emoji-grid {
  @apply grid grid-cols-8 gap-1;
}

.emoji-btn {
  @apply p-2 text-lg hover:bg-gray-100 dark:hover:bg-gray-600 rounded;
}

.modal-overlay {
  @apply fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50;
}

.modal-content {
  @apply bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md m-4;
}

.modal-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.modal-header h3 {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.close-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700;
}

.modal-body {
  @apply p-4 space-y-4;
}

.search-container {
  @apply relative;
}

.search-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.user-list {
  @apply max-h-60 overflow-y-auto space-y-1;
}

.user-item {
  @apply flex items-center space-x-3 p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer;
}

.user-avatar {
  @apply w-10 h-10 rounded-full object-cover;
}

.user-avatar-placeholder {
  @apply w-10 h-10 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-sm;
}

.user-info {
  @apply flex-1;
}

.user-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.user-email {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

/* 深色模式优化 */
.dark .message-conversation-container {
  @apply bg-gray-800;
}

.dark .conversation-header {
  @apply border-gray-700;
}

.dark .conversation-title {
  @apply text-white;
}

.dark .conversation-description {
  @apply text-gray-400;
}

.dark .participants-section {
  @apply border-gray-700;
}

.dark .participants-header h4 {
  @apply text-white;
}

.dark .participant-name {
  @apply text-white;
}

.dark .participant-role {
  @apply text-gray-400;
}

.dark .messages-section {
  @apply border-gray-700;
}

.dark .date-separator .separator-line {
  @apply bg-gray-600;
}

.dark .date-separator .separator-text {
  @apply text-gray-400;
}

.dark .message-bubble-wrapper.received .message-bubble {
  @apply bg-gray-700 text-white;
}

.dark .message-bubble-wrapper.received .message-time {
  @apply text-gray-400;
}

.dark .empty-state h3 {
  @apply text-white;
}

.dark .empty-state p {
  @apply text-gray-400;
}

.dark .input-section {
  @apply border-gray-700;
}

.dark .message-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .priority-select {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .emoji-picker {
  @apply bg-gray-700 border-gray-600;
}

.dark .modal-content {
  @apply bg-gray-800;
}

.dark .modal-header {
  @apply border-gray-700;
}

.dark .modal-header h3 {
  @apply text-white;
}

.dark .search-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .user-item {
  @apply hover:bg-gray-700;
}

.dark .user-name {
  @apply text-white;
}

.dark .user-email {
  @apply text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .conversation-header {
    @apply flex-col space-y-2;
  }
  
  .header-right {
    @apply w-full justify-between;
  }
  
  .participants-section {
    @apply hidden;
  }
  
  .message-bubble {
    @apply max-w-xs;
  }
  
  .input-container {
    @apply flex flex-col space-y-2;
  }
  
  .input-actions {
    @apply static;
  }
  
  .message-input {
    @apply pl-12 pr-20;
  }
  
  .input-footer {
    @apply static;
  }
  
  .modal-content {
    @apply m-2;
  }
}

/* 动画效果 */
.message-bubble-wrapper {
  @apply transition-all duration-200 ease-in-out;
}

.message-bubble-wrapper:hover {
  @apply transform scale-105;
}

.message-actions {
  @apply transition-opacity duration-200;
}

.modal-overlay {
  @apply animate-fade-in;
}

.modal-content {
  @apply animate-scale-in;
}

/* 滚动条样式 */
.messages-container::-webkit-scrollbar {
  @apply w-2;
}

.messages-container::-webkit-scrollbar-track {
  @apply bg-gray-100 dark:bg-gray-800;
}

.messages-container::-webkit-scrollbar-thumb {
  @apply bg-gray-300 dark:bg-gray-600 rounded-full;
}

.messages-container::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400 dark:bg-gray-500;
}

/* 可访问性 */
.message-bubble-wrapper:focus-within {
  @apply ring-2 ring-blue-500 ring-opacity-50;
}

.message-input:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .message-bubble-wrapper {
    @apply border border-gray-300 dark:border-gray-600;
  }
  
  .message-bubble-wrapper.sent .message-bubble {
    @apply border-2 border-blue-600;
  }
  
  .message-bubble-wrapper.received .message-bubble {
    @apply border-2 border-gray-300 dark:border-gray-600;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .message-bubble-wrapper {
    @apply transition-none;
  }
  
  .message-bubble-wrapper:hover {
    @apply transform-none;
  }
  
  .modal-overlay,
  .modal-content {
    @apply animate-none;
  }
}
</style>