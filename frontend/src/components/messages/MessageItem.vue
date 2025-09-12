<template>
  <div
    :class="messageItemClasses"
    @click="handleClick"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
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
      />
      <div v-else class="avatar-placeholder">
        {{ getAvatarInitial(message.senderName) }}
      </div>
      <!-- 在线状态指示器 -->
      <div v-if="showOnlineStatus && isOnline" class="online-indicator"></div>
    </div>

    <!-- 消息内容区域 -->
    <div class="message-content">
      <div class="message-header">
        <div class="message-sender">
          <span class="sender-name">{{ message.senderName }}</span>
          <span v-if="message.isSender" class="sender-badge">我</span>
        </div>
        <div class="message-meta">
          <span class="message-time">{{ formatTime(message.createdAt) }}</span>
          <span v-if="message.priority === MessagePriority.URGENT" class="priority-indicator urgent">
            <i class="fas fa-exclamation-circle"></i>
          </span>
          <span v-else-if="message.priority === MessagePriority.HIGH" class="priority-indicator high">
            <i class="fas fa-exclamation-triangle"></i>
          </span>
        </div>
      </div>

      <div class="message-body">
        <div v-if="message.subject" class="message-subject">
          {{ message.subject }}
        </div>
        <div class="message-text">
          {{ truncatedContent }}
          <span v-if="contentTooLong" class="read-more" @click.stop="handleExpand">
            {{ isExpanded ? '收起' : '展开' }}
          </span>
        </div>
      </div>

      <!-- 附件区域 -->
      <div v-if="message.attachments && message.attachments.length > 0" class="message-attachments">
        <div class="attachment-preview">
          <i class="fas fa-paperclip"></i>
          <span>{{ message.attachments.length }} 个附件</span>
        </div>
      </div>

      <!-- 标签区域 -->
      <div v-if="message.tag" class="message-tags">
        <span class="tag">{{ message.tag }}</span>
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
        ></i>
      </div>
    </div>

    <!-- 操作按钮 -->
    <div v-if="showActions && isHovered" class="message-actions">
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
      <button
        v-if="message.canReply"
        @click.stop="handleReply"
        class="action-btn"
        title="回复"
      >
        <i class="fas fa-reply"></i>
      </button>
      <button
        v-if="message.canForward"
        @click.stop="handleForward"
        class="action-btn"
        title="转发"
      >
        <i class="fas fa-share"></i>
      </button>
      <button
        v-if="message.canEdit"
        @click.stop="handleEdit"
        class="action-btn"
        title="编辑"
      >
        <i class="fas fa-edit"></i>
      </button>
      <button
        v-if="message.canDelete"
        @click.stop="handleDelete"
        class="action-btn delete"
        title="删除"
      >
        <i class="fas fa-trash"></i>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { MessageStatus, MessagePriority } from '@/types/message'
import type { Message } from '@/types/message'

// 定义Props
interface Props {
  message: Message
  selected?: boolean
  showSelect?: boolean
  showAvatar?: boolean
  showActions?: boolean
  showOnlineStatus?: boolean
  compactMode?: boolean
  isOnline?: boolean
  maxContentLength?: number
}

const props = withDefaults(defineProps<Props>(), {
  selected: false,
  showSelect: false,
  showAvatar: true,
  showActions: true,
  showOnlineStatus: false,
  compactMode: false,
  isOnline: false,
  maxContentLength: 100
})

// 定义Emits
interface Emits {
  (e: 'click', message: Message): void
  (e: 'select', message: Message, selected: boolean): void
  (e: 'mark-read', message: Message): void
  (e: 'mark-unread', message: Message): void
  (e: 'reply', message: Message): void
  (e: 'forward', message: Message): void
  (e: 'edit', message: Message): void
  (e: 'delete', message: Message): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const isHovered = ref(false)
const isExpanded = ref(false)

// 计算属性
const messageItemClasses = computed(() => {
  const baseClasses = [
    'message-item',
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

const handleEdit = () => {
  emit('edit', props.message)
}

const handleDelete = () => {
  emit('delete', props.message)
}

const handleExpand = () => {
  isExpanded.value = !isExpanded.value
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

// 暴露方法
defineExpose({
  isHovered,
  isExpanded,
  handleExpand
})
</script>

<style scoped>
.message-item {
  @apply relative;
}

.message-item.selected {
  @apply bg-blue-50 dark:bg-blue-900;
}

.message-item.unread {
  @apply bg-gray-50 dark:bg-gray-800;
}

.message-item.compact-mode {
  @apply p-2 space-x-2;
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

.message-meta {
  @apply flex items-center space-x-2;
}

.message-time {
  @apply text-sm text-gray-500 dark:text-gray-400;
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

.message-body {
  @apply space-y-1;
}

.message-subject {
  @apply font-medium text-gray-900 dark:text-white;
}

.message-text {
  @apply text-gray-700 dark:text-gray-300 text-sm break-words;
}

.read-more {
  @apply text-blue-600 dark:text-blue-400 cursor-pointer hover:underline;
}

.message-attachments {
  @apply mt-2;
}

.attachment-preview {
  @apply flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400;
}

.message-tags {
  @apply mt-2;
}

.tag {
  @apply inline-block px-2 py-1 text-xs bg-gray-100 dark:bg-gray-700 
         text-gray-700 dark:text-gray-300 rounded-full;
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

.message-item:hover .message-actions {
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

/* 深色模式优化 */
.dark .message-item {
  @apply hover:bg-gray-700;
}

.dark .message-item.selected {
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

/* 响应式设计 */
@media (max-width: 768px) {
  .message-item {
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
}

/* 动画效果 */
.message-item {
  @apply transition-all duration-200 ease-in-out;
}

.message-item:hover {
  @apply transform -translate-y-0.5 shadow-sm;
}

/* 可访问性 */
.message-item:focus-within {
  @apply ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .message-item {
    @apply border border-gray-300 dark:border-gray-600;
  }
  
  .message-item.selected {
    @apply border-2 border-blue-500;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .message-item {
    @apply transition-none;
  }
  
  .message-item:hover {
    @apply transform-none;
  }
}
</style>