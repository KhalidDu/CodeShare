<template>
  <div
    :class="cardClasses"
    @click="handleClick"
    @mouseenter="isHovered = true"
    @mouseleave="isHovered = false"
  >
    <!-- 文件图标 -->
    <div class="file-icon">
      <i :class="iconClasses"></i>
    </div>

    <!-- 文件信息 -->
    <div class="file-info">
      <div class="file-name" :title="attachment.fileName">
        {{ truncatedFileName }}
      </div>
      <div v-if="showSize" class="file-size">
        {{ formatFileSize(attachment.fileSize) }}
      </div>
      <div v-if="showDate" class="file-date">
        {{ formatDate(attachment.uploadedAt) }}
      </div>
      <div v-if="showStatus" class="file-status">
        <span :class="statusClasses">
          {{ getStatusText() }}
        </span>
      </div>
    </div>

    <!-- 操作按钮 -->
    <div v-if="showActions && isHovered" class="file-actions">
      <button
        v-if="canDownload"
        @click.stop="handleDownload"
        class="action-btn"
        title="下载"
      >
        <i class="fas fa-download"></i>
      </button>
      <button
        v-if="canPreview"
        @click.stop="handlePreview"
        class="action-btn"
        title="预览"
      >
        <i class="fas fa-eye"></i>
      </button>
      <button
        v-if="canDelete"
        @click.stop="handleDelete"
        class="action-btn delete"
        title="删除"
      >
        <i class="fas fa-trash"></i>
      </button>
    </div>

    <!-- 上传进度 -->
    <div v-if="showProgress" class="file-progress">
      <div class="progress-bar">
        <div
          class="progress-fill"
          :style="{ width: attachment.uploadProgress + '%' }"
        ></div>
      </div>
      <span class="progress-text">{{ attachment.uploadProgress }}%</span>
    </div>

    <!-- 缩略图 -->
    <div v-if="showThumbnail && hasThumbnail" class="file-thumbnail">
      <img
        :src="attachment.thumbnailUrl || attachment.fileUrl"
        :alt="attachment.fileName"
        class="thumbnail-image"
        @error="handleThumbnailError"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { messageService } from '@/services/messageService'
import type { MessageAttachment, AttachmentType, AttachmentStatus } from '@/types/message'

// 定义Props
interface Props {
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

const props = withDefaults(defineProps<Props>(), {
  compactMode: false,
  showActions: true,
  showSize: true,
  showDate: false,
  showStatus: false,
  showProgress: false,
  showThumbnail: true,
  clickable: true,
  disabled: false
})

// 定义Emits
interface Emits {
  (e: 'click', attachment: MessageAttachment): void
  (e: 'download', attachment: MessageAttachment): void
  (e: 'preview', attachment: MessageAttachment): void
  (e: 'delete', attachment: MessageAttachment): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const isHovered = ref(false)
const thumbnailError = ref(false)

// 计算属性
const cardClasses = computed(() => {
  const baseClasses = [
    'attachment-card',
    'flex',
    'items-center',
    'space-x-3',
    'p-3',
    'border',
    'border-gray-200',
    'dark:border-gray-600',
    'rounded-lg',
    'transition-all',
    'duration-200',
    'ease-in-out'
  ]

  if (props.compactMode) {
    baseClasses.push('compact-mode', 'p-2', 'space-x-2')
  }

  if (props.clickable && !props.disabled) {
    baseClasses.push('cursor-pointer', 'hover:shadow-md', 'hover:border-blue-300')
  }

  if (props.disabled) {
    baseClasses.push('opacity-50', 'cursor-not-allowed')
  }

  if (hasError.value) {
    baseClasses.push('border-red-300', 'dark:border-red-600')
  }

  return baseClasses.join(' ')
})

const iconClasses = computed(() => {
  const baseIcon = getFileIcon(props.attachment)
  return `${baseIcon} text-2xl`
})

const truncatedFileName = computed(() => {
  const maxLength = props.compactMode ? 20 : 30
  const fileName = props.attachment.fileName
  
  if (fileName.length <= maxLength) {
    return fileName
  }
  
  const extension = fileName.split('.').pop()
  const nameWithoutExtension = fileName.replace(`.${extension}`, '')
  
  if (nameWithoutExtension.length <= maxLength - extension.length - 3) {
    return fileName
  }
  
  return `${nameWithoutExtension.substring(0, maxLength - extension.length - 3)}...${extension}`
})

const statusClasses = computed(() => {
  const status = props.attachment.attachmentStatus
  const baseClasses = ['text-xs', 'px-2', 'py-1', 'rounded-full']
  
  switch (status) {
    case AttachmentStatus.ACTIVE:
      return [...baseClasses, 'bg-green-100', 'text-green-700', 'dark:bg-green-900', 'dark:text-green-300']
    case AttachmentStatus.UPLOADING:
      return [...baseClasses, 'bg-blue-100', 'text-blue-700', 'dark:bg-blue-900', 'dark:text-blue-300']
    case AttachmentStatus.UPLOAD_FAILED:
      return [...baseClasses, 'bg-red-100', 'text-red-700', 'dark:bg-red-900', 'dark:text-red-300']
    case AttachmentStatus.DELETED:
      return [...baseClasses, 'bg-gray-100', 'text-gray-700', 'dark:bg-gray-700', 'dark:text-gray-300']
    case AttachmentStatus.EXPIRED:
      return [...baseClasses, 'bg-yellow-100', 'text-yellow-700', 'dark:bg-yellow-900', 'dark:text-yellow-300']
    case AttachmentStatus.VIRUS_SCANNING:
      return [...baseClasses, 'bg-purple-100', 'text-purple-700', 'dark:bg-purple-900', 'dark:text-purple-300']
    case AttachmentStatus.VIRUS_DETECTED:
      return [...baseClasses, 'bg-red-100', 'text-red-700', 'dark:bg-red-900', 'dark:text-red-300']
    default:
      return [...baseClasses, 'bg-gray-100', 'text-gray-700', 'dark:bg-gray-700', 'dark:text-gray-300']
  }
})

const hasError = computed(() => {
  return props.attachment.attachmentStatus === AttachmentStatus.UPLOAD_FAILED ||
         props.attachment.attachmentStatus === AttachmentStatus.VIRUS_DETECTED ||
         props.attachment.attachmentStatus === AttachmentStatus.EXPIRED ||
         thumbnailError.value
})

const canDownload = computed(() => {
  return props.attachment.attachmentStatus === AttachmentStatus.ACTIVE &&
         props.attachment.fileUrl &&
         !props.disabled
})

const canPreview = computed(() => {
  return canDownload.value && isPreviewable(props.attachment)
})

const canDelete = computed(() => {
  return props.showActions &&
         props.attachment.attachmentStatus !== AttachmentStatus.DELETED &&
         !props.disabled
})

const showProgress = computed(() => {
  return props.showProgress &&
         props.attachment.attachmentStatus === AttachmentStatus.UPLOADING &&
         props.attachment.uploadProgress < 100
})

const hasThumbnail = computed(() => {
  return props.showThumbnail &&
         (props.attachment.thumbnailUrl || isImageFile(props.attachment)) &&
         !thumbnailError.value
})

// 方法定义
const handleClick = () => {
  if (props.clickable && !props.disabled) {
    emit('click', props.attachment)
  }
}

const handleDownload = () => {
  emit('download', props.attachment)
}

const handlePreview = () => {
  emit('preview', props.attachment)
}

const handleDelete = () => {
  emit('delete', props.attachment)
}

const handleThumbnailError = () => {
  thumbnailError.value = true
}

const getFileIcon = (attachment: MessageAttachment): string => {
  return messageService.getFileTypeIcon(attachment.fileName)
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatDate = (dateString?: string): string => {
  if (!dateString) return ''
  
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))
  
  if (diffInHours < 1) {
    return '刚刚'
  } else if (diffInHours < 24) {
    return `${diffInHours}小时前`
  } else if (diffInHours < 48) {
    return '昨天'
  } else if (diffInHours < 168) {
    return `${Math.floor(diffInHours / 24)}天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

const getStatusText = (): string => {
  const status = props.attachment.attachmentStatus
  const statusMap = {
    [AttachmentStatus.ACTIVE]: '正常',
    [AttachmentStatus.UPLOADING]: '上传中',
    [AttachmentStatus.UPLOAD_FAILED]: '上传失败',
    [AttachmentStatus.DELETED]: '已删除',
    [AttachmentStatus.EXPIRED]: '已过期',
    [AttachmentStatus.VIRUS_SCANNING]: '病毒扫描中',
    [AttachmentStatus.VIRUS_DETECTED]: '检测到病毒'
  }
  return statusMap[status] || '未知状态'
}

const isImageFile = (attachment: MessageAttachment): boolean => {
  return attachment.attachmentType === AttachmentType.IMAGE
}

const isPreviewable = (attachment: MessageAttachment): boolean => {
  const imageTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
  const textTypes = ['text/plain', 'text/html', 'text/css', 'text/javascript', 'application/json']
  const pdfTypes = ['application/pdf']
  
  return imageTypes.includes(attachment.contentType || '') ||
         textTypes.includes(attachment.contentType || '') ||
         pdfTypes.includes(attachment.contentType || '') ||
         attachment.attachmentType === AttachmentType.VIDEO ||
         attachment.attachmentType === AttachmentType.AUDIO
}

// 暴露方法
defineExpose({
  isHovered,
  thumbnailError,
  handleClick,
  handleDownload,
  handlePreview,
  handleDelete
})
</script>

<style scoped>
.attachment-card {
  @apply relative;
}

.attachment-card.compact-mode {
  @apply p-2 space-x-2;
}

.attachment-card:hover {
  @apply shadow-md border-blue-300 dark:border-blue-600;
}

.file-icon {
  @apply flex-shrink-0;
}

.file-info {
  @apply flex-1 min-w-0;
}

.file-name {
  @apply font-medium text-gray-900 dark:text-white truncate;
}

.file-size {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.file-date {
  @apply text-xs text-gray-400 dark:text-gray-500;
}

.file-status {
  @apply mt-1;
}

.file-actions {
  @apply flex items-center space-x-1;
}

.action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-100 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.action-btn.delete {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

.file-progress {
  @apply absolute bottom-0 left-0 right-0 bg-black bg-opacity-50 rounded-b-lg;
}

.progress-bar {
  @apply w-full bg-gray-200 dark:bg-gray-700 rounded-b-lg h-1;
}

.progress-fill {
  @apply h-1 bg-blue-600 rounded-b-lg transition-all duration-300;
}

.progress-text {
  @apply absolute bottom-1 right-2 text-xs text-white;
}

.file-thumbnail {
  @apply absolute top-2 right-2 w-12 h-12 rounded overflow-hidden border border-gray-200 dark:border-gray-600;
}

.thumbnail-image {
  @apply w-full h-full object-cover;
}

/* 深色模式优化 */
.dark .attachment-card {
  @apply border-gray-600;
}

.dark .attachment-card:hover {
  @apply border-blue-600;
}

.dark .file-name {
  @apply text-white;
}

.dark .file-size {
  @apply text-gray-400;
}

.dark .file-date {
  @apply text-gray-500;
}

.dark .action-btn {
  @apply text-gray-400 hover:text-gray-300 hover:bg-gray-700;
}

.dark .action-btn.delete {
  @apply hover:text-red-400;
}

.dark .file-progress {
  @apply bg-black bg-opacity-70;
}

.dark .progress-bar {
  @apply bg-gray-700;
}

.dark .file-thumbnail {
  @apply border-gray-600;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .attachment-card {
    @apply p-2 space-x-2;
  }
  
  .file-actions {
    @apply static mt-2 opacity-100;
  }
  
  .file-thumbnail {
    @apply w-10 h-10;
  }
}

/* 动画效果 */
.attachment-card {
  @apply transition-all duration-200 ease-in-out;
}

.attachment-card:hover {
  @apply transform -translate-y-0.5;
}

.progress-fill {
  @apply transition-all duration-300 ease-in-out;
}

/* 可访问性 */
.attachment-card:focus-within {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.action-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .attachment-card {
    @apply border-2;
  }
  
  .attachment-card:hover {
    @apply border-4;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .attachment-card {
    @apply transition-none;
  }
  
  .attachment-card:hover {
    @apply transform-none;
  }
  
  .progress-fill {
    @apply transition-none;
  }
}

/* 加载状态 */
.attachment-card.disabled {
  @apply pointer-events-none opacity-50;
}

/* 错误状态 */
.attachment-card.border-red-300 {
  @apply border-red-500;
}

.dark .attachment-card.border-red-600 {
  @apply border-red-500;
}

/* 上传中状态 */
.attachment-card.uploading {
  @apply border-blue-300 dark:border-blue-600;
}

.dark .attachment-card.uploading {
  @apply border-blue-600;
}
</style>