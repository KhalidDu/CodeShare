<template>
  <div class="attachment-upload-container">
    <!-- 上传区域 -->
    <div
      class="upload-area"
      :class="{
        'drag-over': isDragOver,
        'uploading': isUploading,
        'error': hasError
      }"
      @dragover.prevent="handleDragOver"
      @dragleave.prevent="handleDragLeave"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <div v-if="isUploading" class="uploading-state">
        <div class="uploading-spinner">
          <div class="spinner"></div>
        </div>
        <div class="uploading-text">
          <div class="uploading-progress">{{ uploadProgress }}%</div>
          <div class="uploading-filename">{{ currentUploadFile?.name }}</div>
        </div>
      </div>
      
      <div v-else class="upload-prompt">
        <div class="upload-icon">
          <i class="fas fa-cloud-upload-alt"></i>
        </div>
        <div class="upload-text">
          <div class="upload-title">{{ uploadTitle }}</div>
          <div class="upload-description">{{ uploadDescription }}</div>
          <div v-if="allowedFileTypes" class="upload-restrictions">
            支持的文件类型: {{ formatFileTypes(allowedFileTypes) }}
          </div>
          <div v-if="maxFileSize" class="upload-restrictions">
            最大文件大小: {{ formatFileSize(maxFileSize) }}
          </div>
          <div v-if="maxFiles" class="upload-restrictions">
            最多上传 {{ maxFiles }} 个文件
          </div>
        </div>
        <button class="upload-button">
          <i class="fas fa-plus"></i>
          选择文件
        </button>
      </div>
    </div>

    <!-- 文件输入 -->
    <input
      ref="fileInput"
      type="file"
      :multiple="allowMultiple"
      :accept="formatFileTypes(allowedFileTypes)"
      @change="handleFileSelect"
      class="file-input"
    />

    <!-- 上传队列 -->
    <div v-if="uploadQueue.length > 0" class="upload-queue">
      <div class="queue-header">
        <h4>上传队列 ({{ uploadQueue.length }})</h4>
        <button
          v-if="!isUploading"
          @click="clearQueue"
          class="clear-queue"
        >
          清空队列
        </button>
      </div>
      <div class="queue-items">
        <div
          v-for="(item, index) in uploadQueue"
          :key="item.id"
          class="queue-item"
          :class="{
            'uploading': item.status === 'uploading',
            'completed': item.status === 'completed',
            'failed': item.status === 'failed'
          }"
        >
          <div class="item-info">
            <div class="item-icon">
              <i :class="getFileIcon(item.file)"></i>
            </div>
            <div class="item-details">
              <div class="item-name">{{ item.file.name }}</div>
              <div class="item-meta">
                <span class="item-size">{{ formatFileSize(item.file.size) }}</span>
                <span v-if="item.status === 'uploading'" class="item-progress">
                  {{ item.progress }}%
                </span>
                <span v-else-if="item.status === 'failed'" class="item-error">
                  {{ item.error }}
                </span>
                <span v-else-if="item.status === 'completed'" class="item-success">
                  上传成功
                </span>
              </div>
            </div>
          </div>
          
          <div class="item-actions">
            <div v-if="item.status === 'uploading'" class="progress-bar">
              <div
                class="progress-fill"
                :style="{ width: item.progress + '%' }"
              ></div>
            </div>
            <button
              v-if="item.status === 'failed'"
              @click="retryUpload(item)"
              class="retry-btn"
              title="重试"
            >
              <i class="fas fa-redo"></i>
            </button>
            <button
              v-if="item.status !== 'uploading'"
              @click="removeFromQueue(item.id)"
              class="remove-btn"
              title="移除"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 已上传文件 -->
    <div v-if="uploadedFiles.length > 0" class="uploaded-files">
      <div class="uploaded-header">
        <h4>已上传文件 ({{ uploadedFiles.length }})</h4>
        <button
          @click="clearUploaded"
          class="clear-uploaded"
        >
          清空列表
        </button>
      </div>
      <div class="uploaded-list">
        <div
          v-for="file in uploadedFiles"
          :key="file.id"
          class="uploaded-item"
        >
          <div class="file-info">
            <div class="file-icon">
              <i :class="getFileIconByType(file.attachmentType)"></i>
            </div>
            <div class="file-details">
              <div class="file-name">{{ file.fileName }}</div>
              <div class="file-meta">
                <span class="file-size">{{ formatFileSize(file.fileSize) }}</span>
                <span class="file-uploads">下载 {{ file.downloadCount }} 次</span>
                <span class="file-date">{{ formatUploadTime(file.uploadedAt) }}</span>
              </div>
            </div>
          </div>
          
          <div class="file-actions">
            <button
              @click="downloadFile(file)"
              class="action-btn"
              title="下载"
            >
              <i class="fas fa-download"></i>
            </button>
            <button
              @click="previewFile(file)"
              class="action-btn"
              title="预览"
            >
              <i class="fas fa-eye"></i>
            </button>
            <button
              @click="copyFileUrl(file)"
              class="action-btn"
              title="复制链接"
            >
              <i class="fas fa-link"></i>
            </button>
            <button
              v-if="allowDelete"
              @click="deleteFile(file)"
              class="action-btn delete"
              title="删除"
            >
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 预览模态框 -->
    <div v-if="previewFile" class="preview-modal">
      <div class="preview-overlay" @click="closePreview"></div>
      <div class="preview-content">
        <div class="preview-header">
          <h3>{{ previewFile.fileName }}</h3>
          <button @click="closePreview" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="preview-body">
          <div v-if="isImageFile(previewFile)" class="image-preview">
            <img
              :src="previewFile.fileUrl"
              :alt="previewFile.fileName"
              class="preview-image"
            />
          </div>
          <div v-else-if="isTextFile(previewFile)" class="text-preview">
            <pre class="preview-text">{{ previewContent }}</pre>
          </div>
          <div v-else class="file-preview">
            <div class="file-preview-icon">
              <i :class="getFileIconByType(previewFile.attachmentType)"></i>
            </div>
            <div class="file-preview-info">
              <div class="file-preview-name">{{ previewFile.fileName }}</div>
              <div class="file-preview-size">{{ formatFileSize(previewFile.fileSize) }}</div>
              <div class="file-preview-type">{{ previewFile.contentType }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { messageService } from '@/services/messageService'
import type {
  MessageAttachment,
  FileUploadRequest,
  FileUploadResponse,
  AttachmentType
} from '@/types/message'

// 定义Props
interface Props {
  uploadTitle?: string
  uploadDescription?: string
  allowMultiple?: boolean
  allowDelete?: boolean
  allowedFileTypes?: string
  maxFileSize?: number
  maxFiles?: number
  messageType?: string
  attachmentType?: AttachmentType
  autoUpload?: boolean
  showQueue?: boolean
  showUploaded?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  uploadTitle: '上传文件',
  uploadDescription: '拖拽文件到此处或点击选择文件',
  allowMultiple: true,
  allowDelete: true,
  allowedFileTypes: '*',
  maxFileSize: 10 * 1024 * 1024, // 10MB
  maxFiles: 10,
  messageType: 'USER',
  attachmentType: AttachmentType.OTHER,
  autoUpload: true,
  showQueue: true,
  showUploaded: true
})

// 定义Emits
interface Emits {
  (e: 'upload-complete', attachments: MessageAttachment[]): void
  (e: 'upload-error', error: string): void
  (e: 'file-delete', fileId: string): void
  (e: 'queue-change', queue: UploadQueueItem[]): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const fileInput = ref<HTMLInputElement>()
const isDragOver = ref(false)
const isUploading = ref(false)
const hasError = ref(false)
const uploadProgress = ref(0)
const currentUploadFile = ref<File | null>(null)

const uploadQueue = ref<UploadQueueItem[]>([])
const uploadedFiles = ref<MessageAttachment[]>([])
const previewFile = ref<MessageAttachment | null>(null)
const previewContent = ref<string>('')

// 上传队列项接口
interface UploadQueueItem {
  id: string
  file: File
  status: 'pending' | 'uploading' | 'completed' | 'failed'
  progress: number
  error?: string
  attachment?: MessageAttachment
}

// 计算属性
const totalUploadProgress = computed(() => {
  if (uploadQueue.value.length === 0) return 0
  
  const totalProgress = uploadQueue.value.reduce((sum, item) => sum + item.progress, 0)
  return Math.round(totalProgress / uploadQueue.value.length)
})

// 方法定义
const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  
  if (!files || files.length === 0) return
  
  handleFiles(Array.from(files))
}

const handleDragOver = (event: DragEvent) => {
  isDragOver.value = true
  event.dataTransfer!.dropEffect = 'copy'
}

const handleDragLeave = () => {
  isDragOver.value = false
}

const handleDrop = (event: DragEvent) => {
  isDragOver.value = false
  hasError.value = false
  
  const files = event.dataTransfer?.files
  if (!files || files.length === 0) return
  
  handleFiles(Array.from(files))
}

const handleFiles = (files: File[]) => {
  // 检查文件数量
  if (uploadQueue.value.length + files.length > props.maxFiles) {
    emit('upload-error', `最多只能上传 ${props.maxFiles} 个文件`)
    return
  }
  
  // 验证并添加到队列
  for (const file of files) {
    if (validateFile(file)) {
      addToQueue(file)
    }
  }
  
  // 自动上传
  if (props.autoUpload) {
    startUpload()
  }
}

const validateFile = (file: File): boolean => {
  // 检查文件大小
  if (file.size > props.maxFileSize) {
    emit('upload-error', `文件 ${file.name} 超过大小限制`)
    return false
  }
  
  // 检查文件类型
  if (!messageService.isFileTypeAllowed(file, [props.allowedFileTypes])) {
    emit('upload-error', `文件 ${file.name} 类型不支持`)
    return false
  }
  
  return true
}

const addToQueue = (file: File) => {
  const queueItem: UploadQueueItem = {
    id: generateUniqueId(),
    file,
    status: 'pending',
    progress: 0
  }
  
  uploadQueue.value.push(queueItem)
  emit('queue-change', uploadQueue.value)
}

const startUpload = async () => {
  const pendingItems = uploadQueue.value.filter(item => item.status === 'pending')
  
  if (pendingItems.length === 0) return
  
  isUploading.value = true
  hasError.value = false
  
  try {
    for (const item of pendingItems) {
      await uploadFile(item)
    }
    
    // 上传完成
    const completedAttachments = uploadQueue.value
      .filter(item => item.status === 'completed' && item.attachment)
      .map(item => item.attachment!)
    
    if (completedAttachments.length > 0) {
      uploadedFiles.value.push(...completedAttachments)
      emit('upload-complete', completedAttachments)
    }
  } catch (error) {
    console.error('上传失败:', error)
    hasError.value = true
  } finally {
    isUploading.value = false
    currentUploadFile.value = null
    uploadProgress.value = 0
  }
}

const uploadFile = async (queueItem: UploadQueueItem): Promise<void> => {
  queueItem.status = 'uploading'
  currentUploadFile.value = queueItem.file
  
  try {
    const request: FileUploadRequest = {
      file: queueItem.file,
      messageType: props.messageType as any,
      attachmentType: props.attachmentType,
      onProgress: (progress) => {
        queueItem.progress = progress
        uploadProgress.value = totalUploadProgress.value
      }
    }
    
    const response = await messageService.uploadFile(request)
    
    if (response.success && response.attachment) {
      queueItem.status = 'completed'
      queueItem.progress = 100
      queueItem.attachment = response.attachment
    } else {
      queueItem.status = 'failed'
      queueItem.error = response.error || '上传失败'
    }
  } catch (error) {
    queueItem.status = 'failed'
    queueItem.error = error instanceof Error ? error.message : '上传失败'
  }
}

const retryUpload = async (queueItem: UploadQueueItem) => {
  queueItem.status = 'pending'
  queueItem.progress = 0
  queueItem.error = undefined
  
  if (props.autoUpload) {
    await startUpload()
  }
}

const removeFromQueue = (itemId: string) => {
  uploadQueue.value = uploadQueue.value.filter(item => item.id !== itemId)
  emit('queue-change', uploadQueue.value)
}

const clearQueue = () => {
  uploadQueue.value = []
  emit('queue-change', uploadQueue.value)
}

const clearUploaded = () => {
  uploadedFiles.value = []
}

const downloadFile = async (file: MessageAttachment) => {
  try {
    const blob = await messageService.downloadAttachment(file.id)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = file.fileName
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  } catch (error) {
    console.error('下载文件失败:', error)
    emit('upload-error', `下载 ${file.fileName} 失败`)
  }
}

const previewFile = async (file: MessageAttachment) => {
  previewFile.value = file
  
  if (isTextFile(file)) {
    try {
      const blob = await messageService.downloadAttachment(file.id)
      previewContent.value = await blob.text()
    } catch (error) {
      console.error('预览文件失败:', error)
      previewContent.value = '无法预览此文件'
    }
  }
}

const closePreview = () => {
  previewFile.value = null
  previewContent.value = ''
}

const copyFileUrl = (file: MessageAttachment) => {
  if (file.fileUrl) {
    navigator.clipboard.writeText(file.fileUrl).then(() => {
      // 显示复制成功提示
      console.log('文件链接已复制')
    })
  }
}

const deleteFile = async (file: MessageAttachment) => {
  try {
    await messageService.deleteAttachment(file.id)
    uploadedFiles.value = uploadedFiles.value.filter(f => f.id !== file.id)
    emit('file-delete', file.id)
  } catch (error) {
    console.error('删除文件失败:', error)
    emit('upload-error', `删除 ${file.fileName} 失败`)
  }
}

// 工具方法
const generateUniqueId = (): string => {
  return Date.now().toString(36) + Math.random().toString(36).substr(2)
}

const formatFileTypes = (types: string): string => {
  if (types === '*') return '所有文件'
  return types.split(',').map(type => type.trim()).join(', ')
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatUploadTime = (timeString: string): string => {
  const date = new Date(timeString)
  return date.toLocaleString('zh-CN')
}

const getFileIcon = (file: File): string => {
  return messageService.getFileTypeIcon(file.name)
}

const getFileIconByType = (type: AttachmentType): string => {
  const iconMap = {
    [AttachmentType.IMAGE]: 'fas fa-image',
    [AttachmentType.DOCUMENT]: 'fas fa-file-alt',
    [AttachmentType.VIDEO]: 'fas fa-video',
    [AttachmentType.AUDIO]: 'fas fa-music',
    [AttachmentType.ARCHIVE]: 'fas fa-file-archive',
    [AttachmentType.CODE]: 'fas fa-code',
    [AttachmentType.OTHER]: 'fas fa-file'
  }
  return iconMap[type] || 'fas fa-file'
}

const isImageFile = (file: MessageAttachment): boolean => {
  return file.attachmentType === AttachmentType.IMAGE && file.fileUrl
}

const isTextFile = (file: MessageAttachment): boolean => {
  const textTypes = ['text/plain', 'text/html', 'text/css', 'text/javascript', 'application/json', 'application/xml']
  return textTypes.includes(file.contentType || '')
}

// 暴露方法
defineExpose({
  uploadQueue,
  uploadedFiles,
  isUploading,
  startUpload,
  clearQueue,
  clearUploaded,
  addToQueue,
  triggerFileInput
})
</script>

<style scoped>
.attachment-upload-container {
  @apply space-y-4;
}

.upload-area {
  @apply border-2 border-dashed border-gray-300 dark:border-gray-600 
         rounded-lg p-8 text-center cursor-pointer transition-all duration-200
         hover:border-blue-400 dark:hover:border-blue-500 hover:bg-gray-50 
         dark:hover:bg-gray-700;
}

.upload-area.drag-over {
  @apply border-blue-500 bg-blue-50 dark:bg-blue-900;
}

.upload-area.uploading {
  @apply border-blue-400 bg-blue-50 dark:bg-blue-900;
}

.upload-area.error {
  @apply border-red-400 bg-red-50 dark:bg-red-900;
}

.uploading-state {
  @apply flex items-center justify-center space-x-4;
}

.uploading-spinner {
  @apply relative;
}

.spinner {
  @apply w-12 h-12 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin;
}

.uploading-text {
  @apply text-center;
}

.uploading-progress {
  @apply text-2xl font-bold text-blue-600 dark:text-blue-400;
}

.uploading-filename {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.upload-prompt {
  @apply space-y-4;
}

.upload-icon {
  @apply text-4xl text-gray-400 dark:text-gray-600;
}

.upload-text {
  @apply space-y-2;
}

.upload-title {
  @apply text-lg font-medium text-gray-900 dark:text-white;
}

.upload-description {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.upload-restrictions {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.upload-button {
  @apply px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.file-input {
  @apply hidden;
}

.upload-queue {
  @apply space-y-2;
}

.queue-header {
  @apply flex items-center justify-between;
}

.queue-header h4 {
  @apply font-medium text-gray-900 dark:text-white;
}

.clear-queue {
  @apply text-sm text-red-600 hover:text-red-700 dark:text-red-400 
         dark:hover:text-red-300;
}

.queue-items {
  @apply space-y-2;
}

.queue-item {
  @apply flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 
         rounded-lg;
}

.queue-item.uploading {
  @apply bg-blue-50 dark:bg-blue-900;
}

.queue-item.completed {
  @apply bg-green-50 dark:bg-green-900;
}

.queue-item.failed {
  @apply bg-red-50 dark:bg-red-900;
}

.item-info {
  @apply flex items-center space-x-3 flex-1;
}

.item-icon {
  @apply text-xl text-gray-400 dark:text-gray-600;
}

.item-details {
  @apply flex-1;
}

.item-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.item-meta {
  @apply flex items-center space-x-4 text-sm;
}

.item-size {
  @apply text-gray-500 dark:text-gray-400;
}

.item-progress {
  @apply text-blue-600 dark:text-blue-400;
}

.item-error {
  @apply text-red-600 dark:text-red-400;
}

.item-success {
  @apply text-green-600 dark:text-green-400;
}

.item-actions {
  @apply flex items-center space-x-2;
}

.progress-bar {
  @apply w-24 bg-gray-200 dark:bg-gray-600 rounded-full h-2;
}

.progress-fill {
  @apply h-2 bg-blue-600 rounded-full transition-all duration-300;
}

.retry-btn {
  @apply p-1 text-yellow-600 hover:text-yellow-700 dark:text-yellow-400 
         dark:hover:text-yellow-300;
}

.remove-btn {
  @apply p-1 text-gray-400 hover:text-red-600 dark:hover:text-red-400;
}

.uploaded-files {
  @apply space-y-2;
}

.uploaded-header {
  @apply flex items-center justify-between;
}

.uploaded-header h4 {
  @apply font-medium text-gray-900 dark:text-white;
}

.clear-uploaded {
  @apply text-sm text-red-600 hover:text-red-700 dark:text-red-400 
         dark:hover:text-red-300;
}

.uploaded-list {
  @apply space-y-2;
}

.uploaded-item {
  @apply flex items-center justify-between p-3 bg-white dark:bg-gray-800 
         border border-gray-200 dark:border-gray-700 rounded-lg;
}

.file-info {
  @apply flex items-center space-x-3 flex-1;
}

.file-icon {
  @apply text-xl text-gray-400 dark:text-gray-600;
}

.file-details {
  @apply flex-1;
}

.file-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.file-meta {
  @apply flex items-center space-x-4 text-sm;
}

.file-size {
  @apply text-gray-500 dark:text-gray-400;
}

.file-uploads {
  @apply text-gray-500 dark:text-gray-400;
}

.file-date {
  @apply text-gray-500 dark:text-gray-400;
}

.file-actions {
  @apply flex items-center space-x-1;
}

.action-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-100 dark:hover:bg-gray-700;
}

.action-btn.delete {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

.preview-modal {
  @apply fixed inset-0 z-50 flex items-center justify-center;
}

.preview-overlay {
  @apply absolute inset-0 bg-black bg-opacity-50;
}

.preview-content {
  @apply relative bg-white dark:bg-gray-800 rounded-lg shadow-xl 
         max-w-4xl w-full max-h-[90vh] overflow-hidden m-4;
}

.preview-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 
         dark:border-gray-700;
}

.preview-header h3 {
  @apply font-medium text-gray-900 dark:text-white;
}

.close-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700;
}

.preview-body {
  @apply p-4 max-h-[calc(90vh-80px)] overflow-auto;
}

.image-preview {
  @apply flex justify-center;
}

.preview-image {
  @apply max-w-full max-h-full object-contain;
}

.text-preview {
  @apply bg-gray-50 dark:bg-gray-900 rounded-lg p-4;
}

.preview-text {
  @apply text-sm text-gray-900 dark:text-white whitespace-pre-wrap 
         overflow-x-auto;
}

.file-preview {
  @apply flex flex-col items-center justify-center space-y-4 py-8;
}

.file-preview-icon {
  @apply text-6xl text-gray-400 dark:text-gray-600;
}

.file-preview-info {
  @apply text-center space-y-1;
}

.file-preview-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.file-preview-size {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.file-preview-type {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

/* 深色模式优化 */
.dark .upload-area {
  @apply border-gray-600 hover:border-blue-500 hover:bg-gray-700;
}

.dark .upload-area.drag-over {
  @apply border-blue-500 bg-blue-900;
}

.dark .upload-area.uploading {
  @apply border-blue-500 bg-blue-900;
}

.dark .upload-area.error {
  @apply border-red-500 bg-red-900;
}

.dark .upload-title {
  @apply text-white;
}

.dark .upload-description {
  @apply text-gray-400;
}

.dark .upload-restrictions {
  @apply text-gray-400;
}

.dark .queue-header h4 {
  @apply text-white;
}

.dark .queue-item {
  @apply bg-gray-700;
}

.dark .queue-item.uploading {
  @apply bg-blue-900;
}

.dark .queue-item.completed {
  @apply bg-green-900;
}

.dark .queue-item.failed {
  @apply bg-red-900;
}

.dark .item-name {
  @apply text-white;
}

.dark .item-size {
  @apply text-gray-400;
}

.dark .uploaded-header h4 {
  @apply text-white;
}

.dark .uploaded-item {
  @apply bg-gray-800 border-gray-700;
}

.dark .file-name {
  @apply text-white;
}

.dark .file-size,
.dark .file-uploads,
.dark .file-date {
  @apply text-gray-400;
}

.dark .preview-content {
  @apply bg-gray-800;
}

.dark .preview-header {
  @apply border-gray-700;
}

.dark .preview-header h3 {
  @apply text-white;
}

.dark .preview-text {
  @apply text-white;
}

.dark .file-preview-name {
  @apply text-white;
}

.dark .file-preview-size,
.dark .file-preview-type {
  @apply text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .upload-area {
    @apply p-4;
  }
  
  .upload-prompt {
    @apply space-y-2;
  }
  
  .upload-icon {
    @apply text-3xl;
  }
  
  .upload-title {
    @apply text-base;
  }
  
  .upload-description {
    @apply text-xs;
  }
  
  .upload-restrictions {
    @apply text-xs;
  }
  
  .queue-item,
  .uploaded-item {
    @apply flex-col space-y-2 items-start;
  }
  
  .item-info,
  .file-info {
    @apply w-full;
  }
  
  .preview-content {
    @apply m-2;
  }
}

/* 动画效果 */
.upload-area {
  @apply transition-all duration-200 ease-in-out;
}

.upload-area:hover {
  @apply transform scale-105;
}

.preview-modal {
  @apply animate-fade-in;
}

.preview-content {
  @apply animate-scale-in;
}

/* 可访问性 */
.upload-area:focus-within {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.action-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .upload-area {
    @apply border-2;
  }
  
  .upload-area.drag-over {
    @apply border-4;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .upload-area {
    @apply transition-none;
  }
  
  .upload-area:hover {
    @apply transform-none;
  }
  
  .preview-modal,
  .preview-content {
    @apply animate-none;
  }
}
</style>