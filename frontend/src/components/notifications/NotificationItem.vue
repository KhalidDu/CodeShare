<template>
  <div
    :class="notificationItemClasses"
    @click="handleClick"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @keydown.enter="handleKeyEnter"
    @keydown.space="handleKeySpace"
    tabindex="0"
    role="article"
    :aria-label="`通知: ${notification.title}`"
  >
    <!-- 选择框 -->
    <div v-if="showSelect" class="notification-select">
      <input
        type="checkbox"
        :checked="selected"
        @change="handleSelectChange"
        @click.stop
        class="notification-checkbox"
        :aria-label="`选择通知: ${notification.title}`"
      />
    </div>

    <!-- 通知图标区域 -->
    <div v-if="showIcon" class="notification-icon">
      <div :class="iconContainerClasses">
        <i v-if="notification.icon" :class="iconClasses"></i>
        <i v-else :class="defaultIconClasses"></i>
      </div>
      <!-- 状态指示器 -->
      <div v-if="showStatusIndicator" :class="statusIndicatorClasses"></div>
    </div>

    <!-- 通知内容区域 -->
    <div class="notification-content">
      <div class="notification-header">
        <div class="notification-title-area">
          <h3 class="notification-title" :title="notification.title">
            {{ notification.title }}
            <span v-if="notification.requiresConfirmation" class="confirmation-badge">
              <i class="fas fa-exclamation-circle"></i>
              需确认
            </span>
          </h3>
          <div v-if="notification.tag" class="notification-tag">
            <span class="tag">{{ notification.tag }}</span>
          </div>
        </div>
        <div class="notification-meta">
          <span class="notification-time" :title="formatFullTime(notification.createdAt)">
            {{ formatTime(notification.createdAt) }}
          </span>
          <div class="notification-actions-meta">
            <!-- 优先级指示器 -->
            <span v-if="showPriority" :class="priorityIndicatorClasses" :title="getPriorityLabel(notification.priority)">
              <i :class="priorityIconClasses"></i>
            </span>
            <!-- 渠道指示器 -->
            <span v-if="showChannel" :class="channelIndicatorClasses" :title="getChannelLabel(notification.channel)">
              <i :class="channelIconClasses"></i>
            </span>
          </div>
        </div>
      </div>

      <!-- 通知主体内容 -->
      <div class="notification-body">
        <div v-if="notification.content" class="notification-text">
          {{ truncatedContent }}
          <button
            v-if="contentTooLong"
            @click.stop="handleExpand"
            class="expand-btn"
            :aria-expanded="isExpanded"
          >
            {{ isExpanded ? '收起' : '展开' }}
          </button>
        </div>
        
        <!-- 相关实体信息 -->
        <div v-if="showRelatedInfo && notification.relatedEntityType" class="notification-related">
          <span class="related-label">相关:</span>
          <span class="related-info">
            {{ getRelatedEntityLabel(notification.relatedEntityType) }}
            <span v-if="notification.relatedEntityId" class="related-id">
              #{{ notification.relatedEntityId }}
            </span>
          </span>
        </div>

        <!-- 触发用户信息 -->
        <div v-if="showTriggerInfo && notification.triggeredByUserId" class="notification-trigger">
          <span class="trigger-label">触发者:</span>
          <span class="trigger-info">
            {{ notification.triggeredByUserName || '系统' }}
          </span>
        </div>

        <!-- 附加数据预览 -->
        <div v-if="showDataPreview && notification.data" class="notification-data-preview">
          <div class="data-preview-header">
            <i class="fas fa-code"></i>
            <span>附加数据</span>
          </div>
          <pre class="data-preview-content">{{ formattedDataPreview }}</pre>
        </div>
      </div>

      <!-- 通知操作按钮 -->
      <div v-if="showActions && isHovered" class="notification-actions">
        <button
          v-if="!notification.isRead && canMarkAsRead"
          @click.stop="handleMarkAsRead"
          class="action-btn"
          title="标记已读"
          aria-label="标记已读"
        >
          <i class="fas fa-check"></i>
        </button>
        <button
          v-if="notification.isRead && canMarkAsUnread"
          @click.stop="handleMarkAsUnread"
          class="action-btn"
          title="标记未读"
          aria-label="标记未读"
        >
          <i class="fas fa-envelope"></i>
        </button>
        <button
          v-if="notification.requiresConfirmation && !notification.confirmedAt"
          @click.stop="handleConfirm"
          class="action-btn confirm"
          title="确认通知"
          aria-label="确认通知"
        >
          <i class="fas fa-check-circle"></i>
        </button>
        <button
          v-if="canArchive && !notification.isArchived"
          @click.stop="handleArchive"
          class="action-btn"
          title="归档"
          aria-label="归档通知"
        >
          <i class="fas fa-archive"></i>
        </button>
        <button
          v-if="canUnarchive && notification.isArchived"
          @click.stop="handleUnarchive"
          class="action-btn"
          title="取消归档"
          aria-label="取消归档通知"
        >
          <i class="fas fa-undo"></i>
        </button>
        <button
          v-if="canDelete"
          @click.stop="handleDelete"
          class="action-btn delete"
          title="删除"
          aria-label="删除通知"
        >
          <i class="fas fa-trash"></i>
        </button>
        <button
          @click.stop="handleMenuToggle"
          class="action-btn menu"
          title="更多操作"
          aria-label="更多操作"
          aria-haspopup="true"
          :aria-expanded="showMenu"
        >
          <i class="fas fa-ellipsis-v"></i>
        </button>
      </div>

      <!-- 通知状态 -->
      <div class="notification-status">
        <!-- 未读指示器 -->
        <div v-if="!notification.isRead" class="unread-indicator"></div>
        <!-- 状态图标 -->
        <div class="status-icon" :title="getStatusTooltip(notification)">
          <i
            v-if="notification.status === NotificationStatus.SENT"
            class="fas fa-paper-plane text-gray-400"
          ></i>
            <i
              v-else-if="notification.status === NotificationStatus.DELIVERED"
              class="fas fa-check text-gray-400"
            ></i>
            <i
              v-else-if="notification.status === NotificationStatus.READ"
              class="fas fa-check-double text-blue-500"
            ></i>
            <i
              v-else-if="notification.status === NotificationStatus.FAILED"
              class="fas fa-exclamation-triangle text-red-500"
            ></i>
            <i
              v-else-if="notification.status === NotificationStatus.CONFIRMED"
              class="fas fa-check-circle text-green-500"
            ></i>
          </div>
        </div>
      </div>

      <!-- 下拉菜单 -->
      <div v-if="showMenu" class="notification-menu" @click.stop>
        <div class="menu-header">
          <span class="menu-title">通知操作</span>
          <button @click="showMenu = false" class="menu-close" aria-label="关闭菜单">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="menu-items">
          <button
            v-if="canViewDetails"
            @click="handleViewDetails"
            class="menu-item"
          >
            <i class="fas fa-eye"></i>
            查看详情
          </button>
          <button
            v-if="canCopy"
            @click="handleCopy"
            class="menu-item"
          >
            <i class="fas fa-copy"></i>
            复制内容
          </button>
          <button
            v-if="canShare"
            @click="handleShare"
            class="menu-item"
          >
            <i class="fas fa-share"></i>
            分享
          </button>
          <div class="menu-divider"></div>
          <button
            v-if="notification.errorMessage"
            @click="handleViewError"
            class="menu-item error"
          >
            <i class="fas fa-exclamation-triangle"></i>
            查看错误
          </button>
          <button
            v-if="canViewHistory"
            @click="handleViewHistory"
            class="menu-item"
          >
            <i class="fas fa-history"></i>
            查看历史
          </button>
        </div>
      </div>

      <!-- 操作历史时间线 -->
      <div v-if="showHistory && expanded" class="notification-history">
        <div class="history-header">
          <i class="fas fa-history"></i>
          <span>操作历史</span>
        </div>
        <div class="history-timeline">
          <div
            v-for="(history, index) in notification.deliveryHistory"
            :key="history.id"
            class="history-item"
          >
            <div class="history-time">{{ formatTime(history.sentAt) }}</div>
            <div class="history-content">
              <div class="history-channel">{{ getChannelLabel(history.channel) }}</div>
              <div class="history-status" :class="history.status.toLowerCase()">
                {{ getDeliveryStatusLabel(history.status) }}
              </div>
              <div v-if="history.errorMessage" class="history-error">
                {{ history.errorMessage }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useNotificationStore } from '@/stores/notification'
import { notificationService } from '@/services/notificationService'
import type {
  Notification,
  NotificationType,
  NotificationPriority,
  NotificationStatus,
  NotificationChannel,
  NotificationDeliveryStatus
} from '@/types/notification'

// 定义Props
interface Props {
  notification: Notification
  selected?: boolean
  showSelect?: boolean
  showIcon?: boolean
  showActions?: boolean
  showPriority?: boolean
  showChannel?: boolean
  showRelatedInfo?: boolean
  showTriggerInfo?: boolean
  showDataPreview?: boolean
  showStatusIndicator?: boolean
  showHistory?: boolean
  compactMode?: boolean
  canMarkAsRead?: boolean
  canMarkAsUnread?: boolean
  canArchive?: boolean
  canUnarchive?: boolean
  canDelete?: boolean
  canViewDetails?: boolean
  canCopy?: boolean
  canShare?: boolean
  canViewHistory?: boolean
  maxContentLength?: number
  maxDataPreviewLength?: number
}

const props = withDefaults(defineProps<Props>(), {
  selected: false,
  showSelect: false,
  showIcon: true,
  showActions: true,
  showPriority: true,
  showChannel: false,
  showRelatedInfo: true,
  showTriggerInfo: true,
  showDataPreview: false,
  showStatusIndicator: true,
  showHistory: false,
  compactMode: false,
  canMarkAsRead: true,
  canMarkAsUnread: true,
  canArchive: true,
  canUnarchive: true,
  canDelete: true,
  canViewDetails: true,
  canCopy: true,
  canShare: true,
  canViewHistory: true,
  maxContentLength: 150,
  maxDataPreviewLength: 200
})

// 定义Emits
interface Emits {
  (e: 'click', notification: Notification): void
  (e: 'select', notification: Notification, selected: boolean): void
  (e: 'mark-read', notification: Notification): void
  (e: 'mark-unread', notification: Notification): void
  (e: 'confirm', notification: Notification): void
  (e: 'archive', notification: Notification): void
  (e: 'unarchive', notification: Notification): void
  (e: 'delete', notification: Notification): void
  (e: 'view-details', notification: Notification): void
  (e: 'copy', notification: Notification): void
  (e: 'share', notification: Notification): void
  (e: 'view-error', notification: Notification): void
  (e: 'view-history', notification: Notification): void
}

const emit = defineEmits<Emits>()

// Store
const notificationStore = useNotificationStore()

// 响应式状态
const isHovered = ref(false)
const isExpanded = ref(false)
const showMenu = ref(false)

// 计算属性
const notificationItemClasses = computed(() => {
  const baseClasses = [
    'notification-item',
    'relative',
    'flex',
    'items-start',
    'p-4',
    'space-x-3',
    'cursor-pointer',
    'transition-all',
    'duration-200',
    'ease-in-out',
    'hover:bg-gray-50',
    'dark:hover:bg-gray-700',
    'focus:outline-none',
    'focus:ring-2',
    'focus:ring-blue-500',
    'focus:ring-opacity-50'
  ]

  if (props.compactMode) {
    baseClasses.push('compact-mode', 'p-2', 'space-x-2')
  }

  if (props.selected) {
    baseClasses.push('selected', 'bg-blue-50', 'dark:bg-blue-900')
  }

  if (!props.notification.isRead) {
    baseClasses.push('unread', 'bg-gray-50', 'dark:bg-gray-800')
  }

  if (props.notification.isArchived) {
    baseClasses.push('archived', 'opacity-60')
  }

  if (props.notification.priority === NotificationPriority.URGENT) {
    baseClasses.push('urgent', 'border-l-4', 'border-red-500')
  } else if (props.notification.priority === NotificationPriority.HIGH) {
    baseClasses.push('high-priority', 'border-l-4', 'border-yellow-500')
  }

  if (props.notification.requiresConfirmation && !props.notification.confirmedAt) {
    baseClasses.push('requires-confirmation', 'border-l-4', 'border-purple-500')
  }

  return baseClasses.join(' ')
})

const iconContainerClasses = computed(() => {
  const baseClasses = [
    'icon-container',
    'w-10',
    'h-10',
    'rounded-full',
    'flex',
    'items-center',
    'justify-center',
    'text-white',
    'font-semibold',
    'text-sm'
  ]

  if (props.compactMode) {
    baseClasses.push('w-8', 'h-8', 'text-xs')
  }

  // 根据通知类型设置背景色
  const typeColors = {
    [NotificationType.COMMENT]: 'bg-blue-500',
    [NotificationType.REPLY]: 'bg-green-500',
    [NotificationType.MESSAGE]: 'bg-purple-500',
    [NotificationType.SYSTEM]: 'bg-gray-500',
    [NotificationType.LIKE]: 'bg-pink-500',
    [NotificationType.SHARE]: 'bg-indigo-500',
    [NotificationType.FOLLOW]: 'bg-yellow-500',
    [NotificationType.MENTION]: 'bg-orange-500',
    [NotificationType.SECURITY]: 'bg-red-500',
    [NotificationType.ACCOUNT]: 'bg-teal-500',
    [NotificationType.UPDATE]: 'bg-cyan-500',
    [NotificationType.MAINTENANCE]: 'bg-amber-500',
    [NotificationType.ANNOUNCEMENT]: 'bg-violet-500'
  }

  const colorClass = typeColors[props.notification.type] || 'bg-gray-500'
  baseClasses.push(colorClass)

  return baseClasses.join(' ')
})

const iconClasses = computed(() => {
  return props.notification.icon || 'fas fa-bell'
})

const defaultIconClasses = computed(() => {
  const typeIcons = {
    [NotificationType.COMMENT]: 'fas fa-comment',
    [NotificationType.REPLY]: 'fas fa-reply',
    [NotificationType.MESSAGE]: 'fas fa-envelope',
    [NotificationType.SYSTEM]: 'fas fa-cog',
    [NotificationType.LIKE]: 'fas fa-heart',
    [NotificationType.SHARE]: 'fas fa-share',
    [NotificationType.FOLLOW]: 'fas fa-user-plus',
    [NotificationType.MENTION]: 'fas fa-at',
    [NotificationType.SECURITY]: 'fas fa-shield-alt',
    [NotificationType.ACCOUNT]: 'fas fa-user',
    [NotificationType.UPDATE]: 'fas fa-sync',
    [NotificationType.MAINTENANCE]: 'fas fa-tools',
    [NotificationType.ANNOUNCEMENT]: 'fas fa-bullhorn'
  }

  return typeIcons[props.notification.type] || 'fas fa-bell'
})

const statusIndicatorClasses = computed(() => {
  const baseClasses = [
    'status-indicator',
    'absolute',
    'bottom-0',
    'right-0',
    'w-3',
    'h-3',
    'rounded-full',
    'border-2',
    'border-white',
    'dark:border-gray-800'
  ]

  if (!props.notification.isRead) {
    baseClasses.push('bg-blue-600')
  } else if (props.notification.status === NotificationStatus.FAILED) {
    baseClasses.push('bg-red-500')
  } else if (props.notification.status === NotificationStatus.CONFIRMED) {
    baseClasses.push('bg-green-500')
  } else {
    baseClasses.push('bg-gray-400')
  }

  return baseClasses.join(' ')
})

const priorityIndicatorClasses = computed(() => {
  const baseClasses = ['priority-indicator', 'text-sm']

  if (props.notification.priority === NotificationPriority.URGENT) {
    baseClasses.push('text-red-500')
  } else if (props.notification.priority === NotificationPriority.HIGH) {
    baseClasses.push('text-yellow-500')
  } else if (props.notification.priority === NotificationPriority.CRITICAL) {
    baseClasses.push('text-red-600')
  } else {
    baseClasses.push('text-gray-400')
  }

  return baseClasses.join(' ')
})

const priorityIconClasses = computed(() => {
  const priorityIcons = {
    [NotificationPriority.LOW]: 'fas fa-arrow-down',
    [NotificationPriority.NORMAL]: 'fas fa-minus',
    [NotificationPriority.HIGH]: 'fas fa-arrow-up',
    [NotificationPriority.URGENT]: 'fas fa-exclamation-circle',
    [NotificationPriority.CRITICAL]: 'fas fa-exclamation-triangle'
  }

  return priorityIcons[props.notification.priority] || 'fas fa-minus'
})

const channelIndicatorClasses = computed(() => {
  const baseClasses = ['channel-indicator', 'text-sm', 'text-gray-500']

  return baseClasses.join(' ')
})

const channelIconClasses = computed(() => {
  const channelIcons = {
    [NotificationChannel.IN_APP]: 'fas fa-desktop',
    [NotificationChannel.EMAIL]: 'fas fa-envelope',
    [NotificationChannel.PUSH]: 'fas fa-bell',
    [NotificationChannel.DESKTOP]: 'fas fa-desktop',
    [NotificationChannel.SMS]: 'fas fa-sms',
    [NotificationChannel.WEBHOOK]: 'fas fa-link',
    [NotificationChannel.SLACK]: 'fab fa-slack',
    [NotificationChannel.WE_CHAT]: 'fab fa-weixin',
    [NotificationChannel.DING_TALK]: 'fas fa-comments',
    [NotificationChannel.WE_COM]: 'fab fa-weixin'
  }

  return channelIcons[props.notification.channel] || 'fas fa-bell'
})

const contentTooLong = computed(() => 
  props.notification.content?.length > props.maxContentLength
)

const truncatedContent = computed(() => {
  if (isExpanded.value || !props.notification.content) {
    return props.notification.content
  }
  
  if (contentTooLong.value) {
    return props.notification.content.substring(0, props.maxContentLength) + '...'
  }
  
  return props.notification.content
})

const formattedDataPreview = computed(() => {
  if (!props.notification.data) return ''
  
  const dataStr = JSON.stringify(props.notification.data, null, 2)
  if (dataStr.length > props.maxDataPreviewLength) {
    return dataStr.substring(0, props.maxDataPreviewLength) + '...'
  }
  return dataStr
})

const expanded = computed(() => isExpanded.value)

// 方法定义
const handleClick = () => {
  emit('click', props.notification)
}

const handleSelectChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('select', props.notification, target.checked)
}

const handleMouseEnter = () => {
  isHovered.value = true
}

const handleMouseLeave = () => {
  isHovered.value = false
  showMenu.value = false
}

const handleKeyEnter = () => {
  handleClick()
}

const handleKeySpace = () => {
  handleClick()
}

const handleExpand = () => {
  isExpanded.value = !isExpanded.value
}

const handleMarkAsRead = async () => {
  try {
    await notificationService.markAsRead(props.notification.id)
    emit('mark-read', props.notification)
  } catch (error) {
    console.error('标记已读失败:', error)
  }
}

const handleMarkAsUnread = async () => {
  try {
    await notificationService.markAsUnread(props.notification.id)
    emit('mark-unread', props.notification)
  } catch (error) {
    console.error('标记未读失败:', error)
  }
}

const handleConfirm = async () => {
  try {
    await notificationService.confirmNotification(props.notification.id)
    emit('confirm', props.notification)
  } catch (error) {
    console.error('确认通知失败:', error)
  }
}

const handleArchive = async () => {
  try {
    await notificationService.archiveNotification(props.notification.id)
    emit('archive', props.notification)
  } catch (error) {
    console.error('归档通知失败:', error)
  }
}

const handleUnarchive = async () => {
  try {
    await notificationService.unarchiveNotification(props.notification.id)
    emit('unarchive', props.notification)
  } catch (error) {
    console.error('取消归档通知失败:', error)
  }
}

const handleDelete = async () => {
  try {
    await notificationService.deleteNotification(props.notification.id)
    emit('delete', props.notification)
  } catch (error) {
    console.error('删除通知失败:', error)
  }
}

const handleViewDetails = () => {
  emit('view-details', props.notification)
}

const handleCopy = () => {
  const text = `${props.notification.title}\n\n${props.notification.content || ''}`
  navigator.clipboard.writeText(text).then(() => {
    emit('copy', props.notification)
  })
}

const handleShare = () => {
  emit('share', props.notification)
}

const handleViewError = () => {
  emit('view-error', props.notification)
}

const handleViewHistory = () => {
  emit('view-history', props.notification)
}

const handleMenuToggle = () => {
  showMenu.value = !showMenu.value
}

// 工具方法
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

const formatFullTime = (timeString: string) => {
  return new Date(timeString).toLocaleString('zh-CN')
}

const getPriorityLabel = (priority: NotificationPriority) => {
  const labels = {
    [NotificationPriority.LOW]: '低',
    [NotificationPriority.NORMAL]: '普通',
    [NotificationPriority.HIGH]: '高',
    [NotificationPriority.URGENT]: '紧急',
    [NotificationPriority.CRITICAL]: '严重'
  }
  return labels[priority] || priority
}

const getChannelLabel = (channel: NotificationChannel) => {
  const labels = {
    [NotificationChannel.IN_APP]: '应用内',
    [NotificationChannel.EMAIL]: '邮件',
    [NotificationChannel.PUSH]: '推送',
    [NotificationChannel.DESKTOP]: '桌面',
    [NotificationChannel.SMS]: '短信',
    [NotificationChannel.WEBHOOK]: 'Webhook',
    [NotificationChannel.SLACK]: 'Slack',
    [NotificationChannel.WE_CHAT]: '微信',
    [NotificationChannel.DING_TALK]: '钉钉',
    [NotificationChannel.WE_COM]: '企业微信'
  }
  return labels[channel] || channel
}

const getDeliveryStatusLabel = (status: NotificationDeliveryStatus) => {
  const labels = {
    [NotificationDeliveryStatus.PENDING]: '待发送',
    [NotificationDeliveryStatus.SENDING]: '发送中',
    [NotificationDeliveryStatus.SENT]: '已发送',
    [NotificationDeliveryStatus.DELIVERED]: '已送达',
    [NotificationDeliveryStatus.READ]: '已读',
    [NotificationDeliveryStatus.FAILED]: '发送失败',
    [NotificationDeliveryStatus.SKIPPED]: '已跳过',
    [NotificationDeliveryStatus.CANCELLED]: '已取消',
    [NotificationDeliveryStatus.EXPIRED]: '已过期'
  }
  return labels[status] || status
}

const getRelatedEntityLabel = (entityType: string) => {
  const labels = {
    'comment': '评论',
    'message': '消息',
    'snippet': '代码片段',
    'user': '用户',
    'share': '分享',
    'system': '系统'
  }
  return labels[entityType] || entityType
}

const getStatusTooltip = (notification: Notification) => {
  if (notification.status === NotificationStatus.FAILED) {
    return `发送失败: ${notification.errorMessage || '未知错误'}`
  }
  
  const statusLabels = {
    [NotificationStatus.PENDING]: '待发送',
    [NotificationStatus.SENDING]: '发送中',
    [NotificationStatus.SENT]: '已发送',
    [NotificationStatus.DELIVERED]: '已送达',
    [NotificationStatus.READ]: '已读',
    [NotificationStatus.UNREAD]: '未读',
    [NotificationStatus.CONFIRMED]: '已确认',
    [NotificationStatus.FAILED]: '发送失败',
    [NotificationStatus.EXPIRED]: '已过期',
    [NotificationStatus.CANCELLED]: '已取消',
    [NotificationStatus.ARCHIVED]: '已归档'
  }
  
  return statusLabels[notification.status] || notification.status
}

// 暴露方法
defineExpose({
  isHovered,
  isExpanded,
  showMenu,
  handleExpand,
  handleMenuToggle
})
</script>

<style scoped>
.notification-item {
  @apply relative;
}

.notification-item.selected {
  @apply bg-blue-50 dark:bg-blue-900;
}

.notification-item.unread {
  @apply bg-gray-50 dark:bg-gray-800;
}

.notification-item.archived {
  @apply opacity-60;
}

.notification-item.urgent {
  @apply border-l-4 border-red-500;
}

.notification-item.high-priority {
  @apply border-l-4 border-yellow-500;
}

.notification-item.requires-confirmation {
  @apply border-l-4 border-purple-500;
}

.notification-item.compact-mode {
  @apply p-2 space-x-2;
}

.notification-select {
  @apply flex-shrink-0 mt-1;
}

.notification-checkbox {
  @apply w-4 h-4 text-blue-600 border-gray-300 rounded 
         focus:ring-blue-500 focus:ring-2;
}

.notification-icon {
  @apply flex-shrink-0 relative;
}

.icon-container {
  @apply w-10 h-10 rounded-full flex items-center justify-center text-white font-semibold text-sm;
}

.compact-mode .icon-container {
  @apply w-8 h-8 text-xs;
}

.status-indicator {
  @apply absolute bottom-0 right-0 w-3 h-3 rounded-full border-2 border-white dark:border-gray-800;
}

.notification-content {
  @apply flex-1 min-w-0;
}

.notification-header {
  @apply flex items-start justify-between mb-2;
}

.notification-title-area {
  @apply flex-1 min-w-0;
}

.notification-title {
  @apply text-base font-semibold text-gray-900 dark:text-white truncate;
}

.confirmation-badge {
  @apply ml-2 px-2 py-1 text-xs bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300 rounded-full;
}

.notification-tag {
  @apply mt-1;
}

.tag {
  @apply inline-block px-2 py-1 text-xs bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-full;
}

.notification-meta {
  @apply flex items-center space-x-2 ml-2;
}

.notification-time {
  @apply text-sm text-gray-500 dark:text-gray-400 whitespace-nowrap;
}

.notification-actions-meta {
  @apply flex items-center space-x-1;
}

.priority-indicator {
  @apply text-sm;
}

.channel-indicator {
  @apply text-sm text-gray-500;
}

.notification-body {
  @apply space-y-2;
}

.notification-text {
  @apply text-sm text-gray-700 dark:text-gray-300 leading-relaxed;
}

.expand-btn {
  @apply text-blue-600 dark:text-blue-400 hover:underline text-sm ml-1;
}

.notification-related,
.notification-trigger {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.related-label,
.trigger-label {
  @apply font-medium;
}

.notification-data-preview {
  @apply mt-2 p-2 bg-gray-100 dark:bg-gray-800 rounded-lg;
}

.data-preview-header {
  @apply flex items-center space-x-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-1;
}

.data-preview-content {
  @apply text-xs text-gray-600 dark:text-gray-400 overflow-x-auto;
}

.notification-actions {
  @apply absolute top-4 right-4 flex items-center space-x-1 opacity-0 
         transition-opacity duration-200 z-10;
}

.notification-item:hover .notification-actions {
  @apply opacity-100;
}

.action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-200 dark:hover:bg-gray-600 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.action-btn.confirm {
  @apply text-purple-600 hover:text-purple-700 dark:hover:text-purple-400;
}

.action-btn.delete {
  @apply text-red-600 hover:text-red-700 dark:hover:text-red-400;
}

.notification-status {
  @apply flex-shrink-0 flex flex-col items-end space-y-1 ml-2;
}

.unread-indicator {
  @apply w-2 h-2 bg-blue-600 rounded-full;
}

.status-icon {
  @apply text-sm;
}

.notification-menu {
  @apply absolute top-16 right-4 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 
         rounded-lg shadow-lg z-50 min-w-[200px];
}

.menu-header {
  @apply flex items-center justify-between p-3 border-b border-gray-200 dark:border-gray-700;
}

.menu-title {
  @apply font-medium text-gray-900 dark:text-white;
}

.menu-close {
  @apply p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded;
}

.menu-items {
  @apply p-2;
}

.menu-item {
  @apply w-full flex items-center space-x-3 px-3 py-2 text-left text-sm text-gray-700 dark:text-gray-300 
         hover:bg-gray-100 dark:hover:bg-gray-700 rounded focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.menu-item.error {
  @apply text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900;
}

.menu-divider {
  @apply my-2 border-t border-gray-200 dark:border-gray-700;
}

.notification-history {
  @apply mt-4 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg;
}

.history-header {
  @apply flex items-center space-x-2 text-sm font-medium text-gray-700 dark:text-gray-300 mb-2;
}

.history-timeline {
  @apply space-y-2;
}

.history-item {
  @apply flex items-start space-x-3 text-sm;
}

.history-time {
  @apply text-gray-500 dark:text-gray-400 whitespace-nowrap;
}

.history-content {
  @apply flex-1;
}

.history-channel {
  @apply font-medium text-gray-700 dark:text-gray-300;
}

.history-status {
  @apply text-xs px-1 py-0.5 rounded-full;
}

.history-status.delivered {
  @apply bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300;
}

.history-status.failed {
  @apply bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300;
}

.history-status.read {
  @apply bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300;
}

.history-error {
  @apply text-xs text-red-600 dark:text-red-400 mt-1;
}

/* 深色模式优化 */
.dark .notification-item {
  @apply hover:bg-gray-700;
}

.dark .notification-item.selected {
  @apply bg-blue-900;
}

.dark .notification-title {
  @apply text-white;
}

.dark .notification-text {
  @apply text-gray-300;
}

.dark .related-label,
.dark .trigger-label {
  @apply text-gray-300;
}

.dark .data-preview-header {
  @apply text-gray-300;
}

.dark .data-preview-content {
  @apply text-gray-400;
}

.dark .status-indicator {
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

.dark .notification-menu {
  @apply bg-gray-800 border-gray-700;
}

.dark .menu-title {
  @apply text-white;
}

.dark .menu-item {
  @apply text-gray-300 hover:bg-gray-700;
}

.dark .menu-divider {
  @apply border-gray-700;
}

.dark .notification-history {
  @apply bg-gray-800;
}

.dark .history-header {
  @apply text-gray-300;
}

.dark .history-channel {
  @apply text-gray-300;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notification-item {
    @apply p-3 space-x-2;
  }
  
  .notification-item.compact-mode {
    @apply p-2 space-x-1;
  }
  
  .icon-container,
  .icon-container.compact-mode {
    @apply w-8 h-8 text-xs;
  }
  
  .notification-header {
    @apply flex-col space-y-1;
  }
  
  .notification-title {
    @apply text-sm;
  }
  
  .notification-meta {
    @apply w-full justify-between;
  }
  
  .notification-actions {
    @apply static mt-2 opacity-100 flex-wrap;
  }
  
  .notification-menu {
    @apply right-2 top-12;
  }
  
  .notification-history {
    @apply mt-2;
  }
}

/* 动画效果 */
.notification-item {
  @apply transition-all duration-200 ease-in-out;
}

.notification-item:hover {
  @apply transform -translate-y-0.5 shadow-sm;
}

/* 可访问性 */
.notification-item:focus-within {
  @apply ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .notification-item {
    @apply border border-gray-300 dark:border-gray-600;
  }
  
  .notification-item.selected {
    @apply border-2 border-blue-500;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .notification-item {
    @apply transition-none;
  }
  
  .notification-item:hover {
    @apply transform-none;
  }
}
</style>