<template>
  <div
    :class="enhancedMessageItemClasses"
    @click="handleClick"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @contextmenu="handleContextMenu"
  >
    <!-- 选择框 -->
    <div v-if="showSelect" class="message-select">
      <input
        type="checkbox"
        :checked="selected"
        @change="handleSelectChange"
        @click.stop
        class="message-checkbox"
      />
    </div>

    <!-- 头像区域 -->
    <div v-if="showAvatar" class="message-avatar">
      <img
        v-if="message.senderAvatar"
        :src="message.senderAvatar"
        :alt="message.senderName"
        class="avatar-image"
        @error="handleAvatarError"
      />
      <div v-else class="avatar-placeholder">
        {{ getAvatarInitial(message.senderName) }}
      </div>
      <!-- 在线状态指示器 -->
      <div v-if="showOnlineStatus && isOnline" class="online-indicator"></div>
      <!-- 消息类型图标 -->
      <div v-if="showTypeIcon" class="type-icon" :title="getMessageTypeLabel(message.messageType)">
        <i :class="getTypeIconClass(message.messageType)"></i>
      </div>
    </div>

    <!-- 消息内容区域 -->
    <div class="message-content">
      <div class="message-header">
        <div class="message-sender">
          <span class="sender-name">{{ message.senderName }}</span>
          <span v-if="message.isSender" class="sender-badge">我</span>
          <span v-if="message.isEdited" class="edited-badge" title="已编辑">
            <i class="fas fa-edit"></i>
          </span>
          <span v-if="message.isForwarded" class="forwarded-badge" title="已转发">
            <i class="fas fa-share"></i>
          </span>
        </div>
        <div class="message-meta">
          <span class="message-time">{{ formatTime(message.createdAt) }}</span>
          <span v-if="message.editedAt" class="edited-time" title="编辑时间">
            {{ formatTime(message.editedAt) }}
          </span>
          <span v-if="message.priority === MessagePriority.URGENT" class="priority-indicator urgent">
            <i class="fas fa-exclamation-circle"></i>
          </span>
          <span v-else-if="message.priority === MessagePriority.HIGH" class="priority-indicator high">
            <i class="fas fa-exclamation-triangle"></i>
          </span>
        </div>
      </div>

      <!-- 引用回复 -->
      <div v-if="message.replyTo" class="message-reply">
        <div class="reply-content">
          <span class="reply-sender">{{ message.replyTo.senderName }}:</span>
          <span class="reply-text">{{ truncateText(message.replyTo.content, 50) }}</span>
        </div>
      </div>

      <div class="message-body">
        <div v-if="message.subject" class="message-subject">
          {{ message.subject }}
        </div>
        <div class="message-text">
          <!-- 编辑状态 -->
          <div v-if="isEditing" class="edit-container">
            <textarea
              ref="editTextareaRef"
              v-model="editContent"
              class="edit-textarea"
              :placeholder="编辑消息内容..."
              @keydown.ctrl.enter="saveEdit"
              @keydown.esc="cancelEdit"
              @input="handleEditInput"
            ></textarea>
            <div class="edit-actions">
              <button @click="saveEdit" class="edit-btn save" :disabled="!editContent.trim()">
                <i class="fas fa-check"></i> 保存
              </button>
              <button @click="cancelEdit" class="edit-btn cancel">
                <i class="fas fa-times"></i> 取消
              </button>
            </div>
          </div>
          <!-- 正常显示 -->
          <div v-else>
            <div v-if="contentTooLong" class="collapsed-content">
              {{ truncatedContent }}
              <span class="read-more" @click.stop="handleExpand">
                {{ isExpanded ? '收起' : '展开' }}
              </span>
            </div>
            <div v-else class="full-content">
              {{ message.content }}
            </div>
          </div>
        </div>
      </div>

      <!-- 附件区域 -->
      <div v-if="message.attachments && message.attachments.length > 0" class="message-attachments">
        <div class="attachment-grid">
          <div
            v-for="attachment in message.attachments"
            :key="attachment.id"
            class="attachment-item"
            @click.stop="handleAttachmentClick(attachment)"
          >
            <div class="attachment-icon">
              <i :class="getAttachmentIcon(attachment.type)"></i>
            </div>
            <div class="attachment-info">
              <div class="attachment-name">{{ attachment.name }}</div>
              <div class="attachment-size">{{ formatFileSize(attachment.size) }}</div>
            </div>
            <div class="attachment-actions">
              <button
                @click.stop="handleAttachmentDownload(attachment)"
                class="attachment-action-btn"
                title="下载"
              >
                <i class="fas fa-download"></i>
              </button>
              <button
                @click.stop="handleAttachmentPreview(attachment)"
                class="attachment-action-btn"
                title="预览"
              >
                <i class="fas fa-eye"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 标签区域 -->
      <div v-if="message.tags && message.tags.length > 0" class="message-tags">
        <span
          v-for="tag in message.tags"
          :key="tag.id"
          class="tag"
          :style="{ backgroundColor: tag.color + '20', color: tag.color }"
        >
          {{ tag.name }}
        </span>
      </div>

      <!-- 消息反应 -->
      <div v-if="message.reactions && message.reactions.length > 0" class="message-reactions">
        <div class="reactions-container">
          <button
            v-for="reaction in groupedReactions"
            :key="reaction.emoji"
            class="reaction-item"
            :class="{ 'user-reacted': reaction.userReacted }"
            @click.stop="handleReactionClick(reaction.emoji)"
          >
            <span class="reaction-emoji">{{ reaction.emoji }}</span>
            <span class="reaction-count">{{ reaction.count }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 状态指示器 -->
    <div class="message-status">
      <div v-if="!message.isRead" class="unread-indicator"></div>
      <div class="status-icon">
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
          @click.stop="handleResend"
        ></i>
        <i
          v-else-if="message.status === MessageStatus.SENDING"
          class="fas fa-clock text-yellow-500 animate-spin"
          title="发送中"
        ></i>
      </div>
    </div>

    <!-- 操作按钮 -->
    <div v-if="showActions && (isHovered || showActionsAlways)" class="message-actions">
      <!-- 基础操作 -->
      <button
        v-if="!message.isRead"
        @click.stop="handleMarkAsRead"
        class="action-btn"
        title="标记已读"
      >
        <i class="fas fa-check"></i>
      </button>
      <button
        v-else
        @click.stop="handleMarkAsUnread"
        class="action-btn"
        title="标记未读"
      >
        <i class="fas fa-envelope"></i>
      </button>
      
      <!-- 回复操作 -->
      <button
        v-if="message.canReply"
        @click.stop="handleReply"
        class="action-btn"
        title="回复"
      >
        <i class="fas fa-reply"></i>
      </button>
      
      <!-- 转发操作 -->
      <button
        v-if="message.canForward"
        @click.stop="handleForward"
        class="action-btn"
        title="转发"
      >
        <i class="fas fa-share"></i>
      </button>
      
      <!-- 编辑操作 -->
      <button
        v-if="message.canEdit && !isEditing"
        @click.stop="startEdit"
        class="action-btn"
        title="编辑"
      >
        <i class="fas fa-edit"></i>
      </button>
      
      <!-- 删除操作 -->
      <button
        v-if="message.canDelete"
        @click.stop="handleDelete"
        class="action-btn delete"
        title="删除"
      >
        <i class="fas fa-trash"></i>
      </button>
      
      <!-- 更多操作 -->
      <div class="more-actions-dropdown">
        <button
          @click.stop="toggleMoreActions"
          class="action-btn more"
          title="更多"
        >
          <i class="fas fa-ellipsis-v"></i>
        </button>
        <div v-if="showMoreActions" class="more-actions-menu">
          <button
            v-if="message.canPin"
            @click.stop="handlePin"
            class="more-action-item"
          >
            <i class="fas fa-thumbtack"></i> 置顶
          </button>
          <button
            v-if="message.canStar"
            @click.stop="handleStar"
            class="more-action-item"
          >
            <i class="fas fa-star"></i> 收藏
          </button>
          <button
            v-if="message.canCopy"
            @click.stop="handleCopy"
            class="more-action-item"
          >
            <i class="fas fa-copy"></i> 复制
          </button>
          <button
            @click.stop="handleShare"
            class="more-action-item"
          >
            <i class="fas fa-share-alt"></i> 分享
          </button>
          <button
            @click.stop="handleReport"
            class="more-action-item report"
          >
            <i class="fas fa-flag"></i> 举报
          </button>
        </div>
      </div>
    </div>

    <!-- 反应选择器 -->
    <div v-if="showReactionPicker" class="reaction-picker">
      <div class="reaction-picker-content">
        <button
          v-for="emoji in commonEmojis"
          :key="emoji"
          class="reaction-emoji-btn"
          @click.stop="handleReactionSelect(emoji)"
        >
          {{ emoji }}
        </button>
        <button
          @click.stop="toggleReactionPicker"
          class="reaction-picker-close"
        >
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>

    <!-- 右键菜单 -->
    <div v-if="showContextMenu" class="context-menu" :style="contextMenuStyle">
      <button
        v-if="message.canEdit"
        @click.stop="startEdit"
        class="context-menu-item"
      >
        <i class="fas fa-edit"></i> 编辑
      </button>
      <button
        v-if="message.canCopy"
        @click.stop="handleCopy"
        class="context-menu-item"
      >
        <i class="fas fa-copy"></i> 复制
      </button>
      <button
        @click.stop="handleReply"
        class="context-menu-item"
      >
        <i class="fas fa-reply"></i> 回复
      </button>
      <button
        @click.stop="handleForward"
        class="context-menu-item"
      >
        <i class="fas fa-share"></i> 转发
      </button>
      <hr class="context-menu-divider" />
      <button
        v-if="message.canDelete"
        @click.stop="handleDelete"
        class="context-menu-item delete"
      >
        <i class="fas fa-trash"></i> 删除
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { MessageStatus, MessagePriority, MessageType } from '@/types/message'
import type { Message, MessageAttachment, Tag, MessageReaction } from '@/types/message'

// 定义Props
interface Props {
  message: Message
  selected?: boolean
  showSelect?: boolean
  showAvatar?: boolean
  showActions?: boolean
  showOnlineStatus?: boolean
  showTypeIcon?: boolean
  showActionsAlways?: boolean
  compactMode?: boolean
  isOnline?: boolean
  maxContentLength?: number
  enableReactions?: boolean
  enableEdit?: boolean
  enableContextMenu?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  selected: false,
  showSelect: false,
  showAvatar: true,
  showActions: true,
  showOnlineStatus: false,
  showTypeIcon: false,
  showActionsAlways: false,
  compactMode: false,
  isOnline: false,
  maxContentLength: 100,
  enableReactions: true,
  enableEdit: true,
  enableContextMenu: true
})

// 定义Emits
interface Emits {
  (e: 'click', message: Message): void
  (e: 'select', message: Message, selected: boolean): void
  (e: 'mark-read', message: Message): void
  (e: 'mark-unread', message: Message): void
  (e: 'reply', message: Message): void
  (e: 'forward', message: Message): void
  (e: 'edit', message: Message, newContent: string): void
  (e: 'delete', message: Message): void
  (e: 'reaction-add', message: Message, emoji: string): void
  (e: 'reaction-remove', message: Message, emoji: string): void
  (e: 'pin', message: Message): void
  (e: 'unpin', message: Message): void
  (e: 'star', message: Message): void
  (e: 'unstar', message: Message): void
  (e: 'copy', message: Message): void
  (e: 'share', message: Message): void
  (e: 'attachment-click', message: Message, attachment: MessageAttachment): void
  (e: 'attachment-download', message: Message, attachment: MessageAttachment): void
  (e: 'attachment-preview', message: Message, attachment: MessageAttachment): void
  (e: 'resend', message: Message): void
  (e: 'report', message: Message): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const isHovered = ref(false)
const isExpanded = ref(false)
const isEditing = ref(false)
const editContent = ref('')
const editTextareaRef = ref<HTMLTextAreaElement | null>(null)
const showMoreActions = ref(false)
const showReactionPicker = ref(false)
const showContextMenu = ref(false)
const contextMenuPosition = ref({ x: 0, y: 0 })

// 常用表情符号
const commonEmojis = ref(['👍', '👎', '❤️', '😄', '😮', '😢', '😡', '🎉', '🔥', '💯'])

// 计算属性
const enhancedMessageItemClasses = computed(() => {
  const baseClasses = [
    'enhanced-message-item',
    'flex',
    'items-start',
    'p-4',
    'space-x-3',
    'cursor-pointer',
    'transition-all',
    'duration-200',
    'ease-in-out',
    'hover:bg-gray-50',
    'dark:hover:bg-gray-700'
  ]

  if (props.compactMode) {
    baseClasses.push('compact-mode', 'p-2', 'space-x-2')
  }

  if (props.selected) {
    baseClasses.push('selected', 'bg-blue-50', 'dark:bg-blue-900')
  }

  if (!props.message.isRead) {
    baseClasses.push('unread', 'font-semibold')
  }

  if (props.message.priority === MessagePriority.URGENT) {
    baseClasses.push('priority-urgent')
  }

  if (props.message.priority === MessagePriority.HIGH) {
    baseClasses.push('priority-high')
  }

  if (isEditing.value) {
    baseClasses.push('editing-mode')
  }

  return baseClasses.join(' ')
})

const contentTooLong = computed(() => 
  props.message.content.length > props.maxContentLength
)

const truncatedContent = computed(() => {
  if (isExpanded.value) {
    return props.message.content
  }
  
  if (contentTooLong.value) {
    return props.message.content.substring(0, props.maxContentLength) + '...'
  }
  
  return props.message.content
})

const groupedReactions = computed(() => {
  if (!props.message.reactions) return []
  
  const reactionMap = new Map<string, { count: number; userReacted: boolean }>()
  
  props.message.reactions.forEach(reaction => {
    const existing = reactionMap.get(reaction.emoji)
    if (existing) {
      existing.count++
      existing.userReacted = existing.userReacted || reaction.isCurrentUser
    } else {
      reactionMap.set(reaction.emoji, {
        count: 1,
        userReacted: reaction.isCurrentUser
      })
    }
  })
  
  return Array.from(reactionMap.entries()).map(([emoji, data]) => ({
    emoji,
    count: data.count,
    userReacted: data.userReacted
  }))
})

const contextMenuStyle = computed(() => ({
  left: `${contextMenuPosition.value.x}px`,
  top: `${contextMenuPosition.value.y}px`
}))

// 方法定义
const handleClick = () => {
  emit('click', props.message)
}

const handleSelectChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('select', props.message, target.checked)
}

const handleMouseEnter = () => {
  isHovered.value = true
}

const handleMouseLeave = () => {
  isHovered.value = false
}

const handleContextMenu = (event: MouseEvent) => {
  if (!props.enableContextMenu) return
  
  event.preventDefault()
  contextMenuPosition.value = { x: event.clientX, y: event.clientY }
  showContextMenu.value = true
}

const handleMarkAsRead = () => {
  emit('mark-read', props.message)
}

const handleMarkAsUnread = () => {
  emit('mark-unread', props.message)
}

const handleReply = () => {
  emit('reply', props.message)
}

const handleForward = () => {
  emit('forward', props.message)
}

const handleDelete = () => {
  emit('delete', props.message)
}

const handleResend = () => {
  emit('resend', props.message)
}

// 编辑相关方法
const startEdit = () => {
  if (!props.enableEdit || !props.message.canEdit) return
  
  isEditing.value = true
  editContent.value = props.message.content
  
  nextTick(() => {
    if (editTextareaRef.value) {
      editTextareaRef.value.focus()
      editTextareaRef.value.setSelectionRange(
        editTextareaRef.value.value.length,
        editTextareaRef.value.value.length
      )
    }
  })
  
  closeAllMenus()
}

const saveEdit = () => {
  if (!editContent.value.trim()) return
  
  emit('edit', props.message, editContent.value)
  isEditing.value = false
  editContent.value = ''
}

const cancelEdit = () => {
  isEditing.value = false
  editContent.value = ''
}

const handleEditInput = () => {
  // 自动调整文本框高度
  nextTick(() => {
    if (editTextareaRef.value) {
      editTextareaRef.value.style.height = 'auto'
      editTextareaRef.value.style.height = editTextareaRef.value.scrollHeight + 'px'
    }
  })
}

// 反应相关方法
const handleReactionClick = (emoji: string) => {
  const reaction = groupedReactions.value.find(r => r.emoji === emoji)
  
  if (reaction && reaction.userReacted) {
    emit('reaction-remove', props.message, emoji)
  } else {
    emit('reaction-add', props.message, emoji)
  }
}

const handleReactionSelect = (emoji: string) => {
  emit('reaction-add', props.message, emoji)
  showReactionPicker.value = false
}

const toggleReactionPicker = (event?: MouseEvent) => {
  if (event) {
    event.stopPropagation()
  }
  showReactionPicker.value = !showReactionPicker.value
  
  if (showReactionPicker.value) {
    closeOtherMenus('reactionPicker')
  }
}

// 更多操作方法
const toggleMoreActions = (event?: MouseEvent) => {
  if (event) {
    event.stopPropagation()
  }
  showMoreActions.value = !showMoreActions.value
  
  if (showMoreActions.value) {
    closeOtherMenus('moreActions')
  }
}

const handlePin = () => {
  emit('pin', props.message)
  showMoreActions.value = false
}

const handleStar = () => {
  emit('star', props.message)
  showMoreActions.value = false
}

const handleCopy = () => {
  emit('copy', props.message)
  showMoreActions.value = false
}

const handleShare = () => {
  emit('share', props.message)
  showMoreActions.value = false
}

const handleReport = () => {
  emit('report', props.message)
  showMoreActions.value = false
}

// 附件相关方法
const handleAttachmentClick = (attachment: MessageAttachment) => {
  emit('attachment-click', props.message, attachment)
}

const handleAttachmentDownload = (attachment: MessageAttachment) => {
  emit('attachment-download', props.message, attachment)
}

const handleAttachmentPreview = (attachment: MessageAttachment) => {
  emit('attachment-preview', props.message, attachment)
}

// 展开收起
const handleExpand = () => {
  isExpanded.value = !isExpanded.value
}

// 头像错误处理
const handleAvatarError = (event: Event) => {
  const img = event.target as HTMLImageElement
  img.style.display = 'none'
}

// 工具方法
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

const truncateText = (text: string, maxLength: number) => {
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

const getMessageTypeLabel = (type: MessageType) => {
  const labels = {
    [MessageType.USER]: '用户消息',
    [MessageType.SYSTEM]: '系统消息',
    [MessageType.NOTIFICATION]: '通知消息',
    [MessageType.BROADCAST]: '广播消息',
    [MessageType.AUTO_REPLY]: '自动回复'
  }
  return labels[type] || type
}

const getTypeIconClass = (type: MessageType) => {
  const icons = {
    [MessageType.USER]: 'fas fa-user',
    [MessageType.SYSTEM]: 'fas fa-cog',
    [MessageType.NOTIFICATION]: 'fas fa-bell',
    [MessageType.BROADCAST]: 'fas fa-bullhorn',
    [MessageType.AUTO_REPLY]: 'fas fa-robot'
  }
  return icons[type] || 'fas fa-comment'
}

const getAttachmentIcon = (type: string) => {
  const iconMap: Record<string, string> = {
    'image': 'fas fa-image',
    'video': 'fas fa-video',
    'audio': 'fas fa-music',
    'pdf': 'fas fa-file-pdf',
    'doc': 'fas fa-file-word',
    'docx': 'fas fa-file-word',
    'xls': 'fas fa-file-excel',
    'xlsx': 'fas fa-file-excel',
    'ppt': 'fas fa-file-powerpoint',
    'pptx': 'fas fa-file-powerpoint',
    'txt': 'fas fa-file-alt',
    'zip': 'fas fa-file-archive',
    'rar': 'fas fa-file-archive',
    'default': 'fas fa-file'
  }
  
  const extension = type.toLowerCase().split('.').pop() || type
  return iconMap[extension] || iconMap.default
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// 菜单管理
const closeAllMenus = () => {
  showMoreActions.value = false
  showReactionPicker.value = false
  showContextMenu.value = false
}

const closeOtherMenus = (except: string) => {
  if (except !== 'moreActions') showMoreActions.value = false
  if (except !== 'reactionPicker') showReactionPicker.value = false
  if (except !== 'contextMenu') showContextMenu.value = false
}

const handleClickOutside = (event: MouseEvent) => {
  const target = event.target as Element
  
  // 检查点击是否在组件外部
  if (!target.closest('.enhanced-message-item')) {
    closeAllMenus()
  }
}

// 键盘事件处理
const handleKeydown = (event: KeyboardEvent) => {
  if (event.key === 'Escape') {
    if (isEditing.value) {
      cancelEdit()
    } else {
      closeAllMenus()
    }
  }
  
  if (event.key === 'Enter' && event.ctrlKey && isEditing.value) {
    saveEdit()
  }
}

// 生命周期
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  document.removeEventListener('keydown', handleKeydown)
})

// 暴露方法
defineExpose({
  isHovered,
  isExpanded,
  isEditing,
  startEdit,
  cancelEdit,
  saveEdit,
  toggleReactionPicker,
  handleExpand
})
</script>

<style scoped>
.enhanced-message-item {
  @apply relative;
}

.enhanced-message-item.selected {
  @apply bg-blue-50 dark:bg-blue-900;
}

.enhanced-message-item.unread {
  @apply bg-gray-50 dark:bg-gray-800;
}

.enhanced-message-item.compact-mode {
  @apply p-2 space-x-2;
}

.enhanced-message-item.priority-urgent {
  @apply border-l-4 border-red-500;
}

.enhanced-message-item.priority-high {
  @apply border-l-4 border-yellow-500;
}

.enhanced-message-item.editing-mode {
  @apply bg-yellow-50 dark:bg-yellow-900;
}

.message-select {
  @apply flex-shrink-0 mt-1;
}

.message-checkbox {
  @apply w-4 h-4 text-blue-600 border-gray-300 rounded 
         focus:ring-blue-500 focus:ring-2;
}

.message-avatar {
  @apply flex-shrink-0 relative;
}

.avatar-image {
  @apply w-10 h-10 rounded-full object-cover;
}

.compact-mode .avatar-image {
  @apply w-8 h-8;
}

.avatar-placeholder {
  @apply w-10 h-10 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-sm;
}

.compact-mode .avatar-placeholder {
  @apply w-8 h-8 text-xs;
}

.online-indicator {
  @apply absolute bottom-0 right-0 w-3 h-3 bg-green-500 rounded-full 
         border-2 border-white dark:border-gray-800;
}

.type-icon {
  @apply absolute -top-1 -right-1 w-5 h-5 bg-blue-500 text-white rounded-full 
         flex items-center justify-center text-xs;
}

.message-content {
  @apply flex-1 min-w-0;
}

.message-header {
  @apply flex items-center justify-between mb-1;
}

.message-sender {
  @apply flex items-center space-x-2;
}

.sender-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.sender-badge {
  @apply px-2 py-1 text-xs bg-blue-100 dark:bg-blue-900 text-blue-700 
         dark:text-blue-300 rounded-full;
}

.edited-badge {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.forwarded-badge {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.message-meta {
  @apply flex items-center space-x-2;
}

.message-time {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.edited-time {
  @apply text-xs text-gray-400 dark:text-gray-500;
}

.priority-indicator {
  @apply text-sm;
}

.priority-indicator.urgent {
  @apply text-red-500;
}

.priority-indicator.high {
  @apply text-yellow-500;
}

.message-reply {
  @apply mb-2 p-2 bg-gray-100 dark:bg-gray-700 rounded-lg border-l-2 
         border-blue-500;
}

.reply-content {
  @apply text-sm;
}

.reply-sender {
  @apply font-medium text-gray-700 dark:text-gray-300;
}

.reply-text {
  @apply text-gray-600 dark:text-gray-400 ml-1;
}

.message-body {
  @apply space-y-1;
}

.message-subject {
  @apply font-medium text-gray-900 dark:text-white;
}

.message-text {
  @apply text-gray-700 dark:text-gray-300 text-sm break-words;
}

.collapsed-content,
.full-content {
  @apply whitespace-pre-wrap break-words;
}

.read-more {
  @apply text-blue-600 dark:text-blue-400 cursor-pointer hover:underline ml-1;
}

.edit-container {
  @apply space-y-2;
}

.edit-textarea {
  @apply w-full p-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-800 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none;
  min-height: 60px;
  max-height: 200px;
}

.edit-actions {
  @apply flex justify-end space-x-2;
}

.edit-btn {
  @apply px-3 py-1 text-sm rounded-lg focus:outline-none focus:ring-2 
         focus:ring-blue-500;
}

.edit-btn.save {
  @apply bg-blue-600 text-white hover:bg-blue-700;
}

.edit-btn.save:disabled {
  @apply bg-gray-400 cursor-not-allowed;
}

.edit-btn.cancel {
  @apply bg-gray-300 text-gray-700 hover:bg-gray-400;
}

.message-attachments {
  @apply mt-2;
}

.attachment-grid {
  @apply grid grid-cols-1 gap-2;
}

.attachment-item {
  @apply flex items-center space-x-3 p-2 bg-gray-50 dark:bg-gray-700 
         rounded-lg hover:bg-gray-100 dark:hover:bg-gray-600 
         cursor-pointer transition-colors;
}

.attachment-icon {
  @apply flex-shrink-0 text-gray-500 dark:text-gray-400;
}

.attachment-info {
  @apply flex-1 min-w-0;
}

.attachment-name {
  @apply text-sm font-medium text-gray-900 dark:text-white truncate;
}

.attachment-size {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.attachment-actions {
  @apply flex items-center space-x-1;
}

.attachment-action-btn {
  @apply p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-200 dark:hover:bg-gray-500;
}

.message-tags {
  @apply mt-2 flex flex-wrap gap-1;
}

.tag {
  @apply inline-block px-2 py-1 text-xs rounded-full;
}

.message-reactions {
  @apply mt-2;
}

.reactions-container {
  @apply flex flex-wrap gap-1;
}

.reaction-item {
  @apply flex items-center space-x-1 px-2 py-1 bg-gray-100 dark:bg-gray-700 
         rounded-full hover:bg-gray-200 dark:hover:bg-gray-600 
         transition-colors cursor-pointer;
}

.reaction-item.user-reacted {
  @apply bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300;
}

.reaction-emoji {
  @apply text-sm;
}

.reaction-count {
  @apply text-xs text-gray-600 dark:text-gray-400;
}

.message-status {
  @apply flex-shrink-0 flex flex-col items-end space-y-1;
}

.unread-indicator {
  @apply w-2 h-2 bg-blue-600 rounded-full;
}

.status-icon {
  @apply text-sm;
}

.message-actions {
  @apply absolute top-4 right-4 flex items-center space-x-1 opacity-0 
         transition-opacity duration-200;
}

.enhanced-message-item:hover .message-actions,
.message-actions.always-visible {
  @apply opacity-100;
}

.action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-200 dark:hover:bg-gray-600 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.action-btn.delete {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

.more-actions-dropdown {
  @apply relative;
}

.more-actions-menu {
  @apply absolute right-0 top-full mt-1 w-32 bg-white dark:bg-gray-800 
         rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 
         py-1 z-10;
}

.more-action-item {
  @apply w-full px-3 py-2 text-left text-sm text-gray-700 dark:text-gray-300 
         hover:bg-gray-100 dark:hover:bg-gray-700 focus:outline-none;
}

.more-action-item.report {
  @apply text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900;
}

.reaction-picker {
  @apply absolute bottom-full left-0 mb-2 bg-white dark:bg-gray-800 
         rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 
         p-2 z-20;
}

.reaction-picker-content {
  @apply grid grid-cols-5 gap-1;
}

.reaction-emoji-btn {
  @apply p-2 text-lg hover:bg-gray-100 dark:hover:bg-gray-700 
         rounded transition-colors;
}

.reaction-picker-close {
  @apply absolute top-1 right-1 p-1 text-gray-400 hover:text-gray-600 
         dark:hover:text-gray-300;
}

.context-menu {
  @apply fixed bg-white dark:bg-gray-800 rounded-lg shadow-lg 
         border border-gray-200 dark:border-gray-700 py-1 z-30;
}

.context-menu-item {
  @apply w-full px-3 py-2 text-left text-sm text-gray-700 dark:text-gray-300 
         hover:bg-gray-100 dark:hover:bg-gray-700 focus:outline-none;
}

.context-menu-item.delete {
  @apply text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900;
}

.context-menu-divider {
  @apply my-1 border-t border-gray-200 dark:border-gray-700;
}

/* 深色模式优化 */
.dark .enhanced-message-item {
  @apply hover:bg-gray-700;
}

.dark .enhanced-message-item.selected {
  @apply bg-blue-900;
}

.dark .sender-name {
  @apply text-white;
}

.dark .message-subject {
  @apply text-white;
}

.dark .message-text {
  @apply text-gray-300;
}

.dark .avatar-placeholder {
  @apply bg-blue-600 text-white;
}

.dark .online-indicator {
  @apply border-gray-800;
}

.dark .tag {
  @apply bg-gray-700 text-gray-300;
}

.dark .action-btn {
  @apply text-gray-400 hover:text-gray-300 hover:bg-gray-600;
}

.dark .action-btn.delete {
  @apply hover:text-red-400;
}

.dark .edit-textarea {
  @apply bg-gray-800 text-white border-gray-600;
}

.dark .attachment-item {
  @apply bg-gray-700 hover:bg-gray-600;
}

.dark .reaction-item {
  @apply bg-gray-700 hover:bg-gray-600;
}

.dark .reaction-item.user-reacted {
  @apply bg-blue-900 text-blue-300;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .enhanced-message-item {
    @apply p-2 space-x-2;
  }
  
  .avatar-image,
  .avatar-placeholder {
    @apply w-8 h-8;
  }
  
  .message-header {
    @apply flex-col items-start space-y-1;
  }
  
  .message-meta {
    @apply w-full justify-between;
  }
  
  .message-actions {
    @apply static mt-2 opacity-100;
  }
  
  .more-actions-menu {
    @apply w-40;
  }
  
  .reaction-picker {
    @apply grid-cols-4;
  }
  
  .attachment-grid {
    @apply grid-cols-1;
  }
}

/* 动画效果 */
.enhanced-message-item {
  @apply transition-all duration-200 ease-in-out;
}

.enhanced-message-item:hover {
  @apply transform -translate-y-0.5 shadow-sm;
}

/* 可访问性 */
.enhanced-message-item:focus-within {
  @apply ring-2 ring-blue-500 ring-opacity-50;
}

.action-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .enhanced-message-item {
    @apply border border-gray-300 dark:border-gray-600;
  }
  
  .enhanced-message-item.selected {
    @apply border-2 border-blue-500;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .enhanced-message-item {
    @apply transition-none;
  }
  
  .enhanced-message-item:hover {
    @apply transform-none;
  }
}

/* 性能优化 */
.message-content {
  contain: layout;
  will-change: auto;
}

.attachment-item {
  contain: layout;
}

/* 打印样式 */
@media print {
  .message-actions,
  .message-status,
  .message-select {
    @apply hidden;
  }
  
  .enhanced-message-item {
    @apply border-b border-gray-300;
  }
}
</style>