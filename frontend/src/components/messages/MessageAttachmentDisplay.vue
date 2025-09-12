<template>
  <div class="attachment-display-container">
    <!-- 单个附件显示 -->
    <div v-if="attachments.length === 1" class="single-attachment">
      <AttachmentCard
        :attachment="attachments[0]"
        :message-id="messageId"
        :compact-mode="compactMode"
        :show-actions="showActions"
        :show-size="showSize"
        :show-date="showDate"
        @download="handleDownload"
        @preview="handlePreview"
        @delete="handleDelete"
      />
    </div>

    <!-- 多个附件显示 -->
    <div v-else class="multiple-attachments">
      <div class="attachments-header">
        <div class="attachments-count">
          <i class="fas fa-paperclip"></i>
          {{ attachments.length }} 个附件
        </div>
        <div class="attachments-actions">
          <button
            v-if="showActions"
            @click="downloadAll"
            class="action-btn"
            title="下载全部"
          >
            <i class="fas fa-download"></i>
          </button>
          <button
            v-if="viewMode === 'grid'"
            @click="viewMode = 'list'"
            class="action-btn"
            title="列表视图"
          >
            <i class="fas fa-list"></i>
          </button>
          <button
            v-else
            @click="viewMode = 'grid'"
            class="action-btn"
            title="网格视图"
          >
            <i class="fas fa-th"></i>
          </button>
        </div>
      </div>

      <!-- 网格视图 -->
      <div v-if="viewMode === 'grid'" class="grid-view">
        <div
          v-for="attachment in attachments"
          :key="attachment.id"
          class="grid-item"
        >
          <AttachmentCard
            :attachment="attachment"
            :message-id="messageId"
            :compact-mode="true"
            :show-actions="showActions"
            :show-size="showSize"
            :show-date="showDate"
            @download="handleDownload"
            @preview="handlePreview"
            @delete="handleDelete"
          />
        </div>
      </div>

      <!-- 列表视图 -->
      <div v-else class="list-view">
        <div
          v-for="attachment in attachments"
          :key="attachment.id"
          class="list-item"
        >
          <AttachmentCard
            :attachment="attachment"
            :message-id="messageId"
            :compact-mode="compactMode"
            :show-actions="showActions"
            :show-size="showSize"
            :show-date="showDate"
            @download="handleDownload"
            @preview="handlePreview"
            @delete="handleDelete"
          />
        </div>
      </div>
    </div>

    <!-- 预览模态框 -->
    <div v-if="previewAttachment" class="preview-modal">
      <div class="preview-overlay" @click="closePreview"></div>
      <div class="preview-content">
        <div class="preview-header">
          <div class="preview-title">
            <i :class="getFileIcon(previewAttachment)"></i>
            {{ previewAttachment.fileName }}
          </div>
          <div class="preview-actions">
            <button
              @click="downloadAttachment(previewAttachment)"
              class="preview-btn"
              title="下载"
            >
              <i class="fas fa-download"></i>
            </button>
            <button
              v-if="canDelete(previewAttachment)"
              @click="deleteAttachment(previewAttachment)"
              class="preview-btn delete"
              title="删除"
            >
              <i class="fas fa-trash"></i>
            </button>
            <button
              @click="closePreview"
              class="preview-btn close"
              title="关闭"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>

        <div class="preview-body">
          <!-- 图片预览 -->
          <div v-if="isImageFile(previewAttachment)" class="image-preview">
            <div class="preview-controls">
              <button
                @click="zoomOut"
                class="zoom-btn"
                :disabled="zoomLevel <= 0.5"
              >
                <i class="fas fa-search-minus"></i>
              </button>
              <span class="zoom-level">{{ Math.round(zoomLevel * 100) }}%</span>
              <button
                @click="zoomIn"
                class="zoom-btn"
                :disabled="zoomLevel >= 3"
              >
                <i class="fas fa-search-plus"></i>
              </button>
              <button
                @click="resetZoom"
                class="zoom-btn"
              >
                <i class="fas fa-compress"></i>
              </button>
            </div>
            <div class="image-container" @wheel="handleWheel">
              <img
                :src="previewAttachment.fileUrl"
                :alt="previewAttachment.fileName"
                class="preview-image"
                :style="{ transform: `scale(${zoomLevel})` }"
                @load="handleImageLoad"
                @error="handleImageError"
              />
            </div>
          </div>

          <!-- 视频预览 -->
          <div v-else-if="isVideoFile(previewAttachment)" class="video-preview">
            <video
              ref="videoPlayer"
              :src="previewAttachment.fileUrl"
              controls
              class="preview-video"
              @loadedmetadata="handleVideoLoad"
            >
              您的浏览器不支持视频播放
            </video>
          </div>

          <!-- 音频预览 -->
          <div v-else-if="isAudioFile(previewAttachment)" class="audio-preview">
            <div class="audio-player">
              <div class="audio-info">
                <i :class="getFileIcon(previewAttachment)"></i>
                <div class="audio-details">
                  <div class="audio-title">{{ previewAttachment.fileName }}</div>
                  <div class="audio-meta">
                    <span class="audio-size">{{ formatFileSize(previewAttachment.fileSize) }}</span>
                    <span class="audio-type">{{ previewAttachment.contentType }}</span>
                  </div>
                </div>
              </div>
              <audio
                ref="audioPlayer"
                :src="previewAttachment.fileUrl"
                controls
                class="audio-controls"
              ></audio>
            </div>
          </div>

          <!-- PDF预览 -->
          <div v-else-if="isPdfFile(previewAttachment)" class="pdf-preview">
            <iframe
              :src="previewAttachment.fileUrl"
              class="pdf-viewer"
              frameborder="0"
            ></iframe>
          </div>

          <!-- 文本预览 -->
          <div v-else-if="isTextFile(previewAttachment)" class="text-preview">
            <div class="text-controls">
              <div class="text-info">
                <i :class="getFileIcon(previewAttachment)"></i>
                <span>{{ previewAttachment.fileName }}</span>
                <span class="text-size">{{ formatFileSize(previewAttachment.fileSize) }}</span>
              </div>
              <div class="text-actions">
                <button
                  @click="copyTextContent"
                  class="text-action-btn"
                  title="复制内容"
                >
                  <i class="fas fa-copy"></i>
                </button>
                <button
                  @click="wrapText"
                  class="text-action-btn"
                  :title="wrapLongLines ? '取消换行' : '自动换行'"
                >
                  <i class="fas fa-align-left"></i>
                </button>
              </div>
            </div>
            <pre
              class="text-content"
              :class="{ 'wrap-text': wrapLongLines }"
            >{{ textContent }}</pre>
          </div>

          <!-- 代码预览 -->
          <div v-else-if="isCodeFile(previewAttachment)" class="code-preview">
            <div class="code-controls">
              <div class="code-info">
                <i :class="getFileIcon(previewAttachment)"></i>
                <span>{{ previewAttachment.fileName }}</span>
                <select
                  v-model="codeLanguage"
                  class="language-select"
                >
                  <option value="auto">自动检测</option>
                  <option value="javascript">JavaScript</option>
                  <option value="typescript">TypeScript</option>
                  <option value="python">Python</option>
                  <option value="java">Java</option>
                  <option value="cpp">C++</option>
                  <option value="html">HTML</option>
                  <option value="css">CSS</option>
                  <option value="json">JSON</option>
                  <option value="xml">XML</option>
                </select>
              </div>
              <div class="code-actions">
                <button
                  @click="copyCodeContent"
                  class="code-action-btn"
                  title="复制代码"
                >
                  <i class="fas fa-copy"></i>
                </button>
                <button
                  @click="toggleLineNumbers"
                  class="code-action-btn"
                  title="切换行号"
                >
                  <i class="fas fa-list-ol"></i>
                </button>
              </div>
            </div>
            <pre
              class="code-content"
              :class="{ 'show-line-numbers': showLineNumbers }"
            >{{ codeContent }}</pre>
          </div>

          <!-- 其他文件类型预览 -->
          <div v-else class="file-preview">
            <div class="file-preview-icon">
              <i :class="getFileIcon(previewAttachment)"></i>
            </div>
            <div class="file-preview-info">
              <div class="file-preview-name">{{ previewAttachment.fileName }}</div>
              <div class="file-preview-meta">
                <span class="file-size">{{ formatFileSize(previewAttachment.fileSize) }}</span>
                <span class="file-type">{{ previewAttachment.contentType || '未知类型' }}</span>
              </div>
              <div class="file-preview-actions">
                <button
                  @click="downloadAttachment(previewAttachment)"
                  class="file-action-btn"
                >
                  <i class="fas fa-download"></i>
                  下载文件
                </button>
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
import { messageService } from '@/services/messageService'
import type { MessageAttachment, AttachmentType } from '@/types/message'
import AttachmentCard from './AttachmentCard.vue'

// 定义Props
interface Props {
  attachments: MessageAttachment[]
  messageId?: string
  compactMode?: boolean
  showActions?: boolean
  showSize?: boolean
  showDate?: boolean
  defaultView?: 'grid' | 'list'
}

const props = withDefaults(defineProps<Props>(), {
  compactMode: false,
  showActions: true,
  showSize: true,
  showDate: false,
  defaultView: 'grid'
})

// 定义Emits
interface Emits {
  (e: 'download', attachment: MessageAttachment): void
  (e: 'preview', attachment: MessageAttachment): void
  (e: 'delete', attachment: MessageAttachment): void
  (e: 'download-all', attachments: MessageAttachment[]): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const viewMode = ref<'grid' | 'list'>(props.defaultView)
const previewAttachment = ref<MessageAttachment | null>(null)
const zoomLevel = ref(1)
const wrapLongLines = ref(false)
const showLineNumbers = ref(false)
const codeLanguage = ref('auto')

const textContent = ref('')
const codeContent = ref('')

const videoPlayer = ref<HTMLVideoElement>()
const audioPlayer = ref<HTMLAudioElement>()

// 计算属性
const canDelete = (attachment: MessageAttachment) => {
  return props.showActions && attachment.attachmentStatus !== 'DELETED'
}

// 方法定义
const handleDownload = (attachment: MessageAttachment) => {
  emit('download', attachment)
}

const handlePreview = (attachment: MessageAttachment) => {
  previewAttachment.value = attachment
  loadPreviewContent(attachment)
}

const handleDelete = (attachment: MessageAttachment) => {
  emit('delete', attachment)
}

const downloadAll = () => {
  emit('download-all', props.attachments)
}

const closePreview = () => {
  previewAttachment.value = null
  zoomLevel.value = 1
  wrapLongLines.value = false
  showLineNumbers.value = false
  codeLanguage.value = 'auto'
  textContent.value = ''
  codeContent.value = ''
}

const downloadAttachment = async (attachment: MessageAttachment) => {
  try {
    const blob = await messageService.downloadAttachment(attachment.id)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = attachment.fileName
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  } catch (error) {
    console.error('下载文件失败:', error)
  }
}

const deleteAttachment = async (attachment: MessageAttachment) => {
  try {
    await messageService.deleteAttachment(attachment.id)
    emit('delete', attachment)
    closePreview()
  } catch (error) {
    console.error('删除文件失败:', error)
  }
}

const loadPreviewContent = async (attachment: MessageAttachment) => {
  try {
    if (isTextFile(attachment) || isCodeFile(attachment)) {
      const blob = await messageService.downloadAttachment(attachment.id)
      const content = await blob.text()
      
      if (isCodeFile(attachment)) {
        codeContent.value = content
      } else {
        textContent.value = content
      }
    }
  } catch (error) {
    console.error('加载预览内容失败:', error)
  }
}

// 图片预览控制
const zoomIn = () => {
  if (zoomLevel.value < 3) {
    zoomLevel.value += 0.1
  }
}

const zoomOut = () => {
  if (zoomLevel.value > 0.5) {
    zoomLevel.value -= 0.1
  }
}

const resetZoom = () => {
  zoomLevel.value = 1
}

const handleWheel = (event: WheelEvent) => {
  if (event.ctrlKey) {
    event.preventDefault()
    if (event.deltaY < 0) {
      zoomIn()
    } else {
      zoomOut()
    }
  }
}

const handleImageLoad = () => {
  console.log('图片加载完成')
}

const handleImageError = () => {
  console.log('图片加载失败')
}

// 视频预览控制
const handleVideoLoad = () => {
  console.log('视频加载完成')
}

// 文本预览控制
const copyTextContent = async () => {
  try {
    await navigator.clipboard.writeText(textContent.value)
    // 显示复制成功提示
  } catch (error) {
    console.error('复制失败:', error)
  }
}

const wrapText = () => {
  wrapLongLines.value = !wrapLongLines.value
}

// 代码预览控制
const copyCodeContent = async () => {
  try {
    await navigator.clipboard.writeText(codeContent.value)
    // 显示复制成功提示
  } catch (error) {
    console.error('复制失败:', error)
  }
}

const toggleLineNumbers = () => {
  showLineNumbers.value = !showLineNumbers.value
}

// 文件类型检查
const isImageFile = (attachment: MessageAttachment): boolean => {
  return attachment.attachmentType === AttachmentType.IMAGE
}

const isVideoFile = (attachment: MessageAttachment): boolean => {
  return attachment.attachmentType === AttachmentType.VIDEO
}

const isAudioFile = (attachment: MessageAttachment): boolean => {
  return attachment.attachmentType === AttachmentType.AUDIO
}

const isPdfFile = (attachment: MessageAttachment): boolean => {
  return attachment.contentType?.includes('pdf') || false
}

const isTextFile = (attachment: MessageAttachment): boolean => {
  const textTypes = [
    'text/plain',
    'text/html',
    'text/css',
    'text/javascript',
    'application/json',
    'application/xml',
    'text/markdown',
    'text/csv'
  ]
  return textTypes.includes(attachment.contentType || '')
}

const isCodeFile = (attachment: MessageAttachment): boolean => {
  return attachment.attachmentType === AttachmentType.CODE
}

// 工具方法
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

// 暴露方法
defineExpose({
  viewMode,
  previewAttachment,
  zoomLevel,
  handleDownload,
  handlePreview,
  handleDelete,
  downloadAll,
  closePreview
})
</script>

<style scoped>
.attachment-display-container {
  @apply space-y-3;
}

.single-attachment {
  @apply w-full;
}

.multiple-attachments {
  @apply space-y-3;
}

.attachments-header {
  @apply flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.attachments-count {
  @apply flex items-center space-x-2 text-sm font-medium text-gray-700 dark:text-gray-300;
}

.attachments-actions {
  @apply flex items-center space-x-1;
}

.action-btn {
  @apply p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
         dark:hover:text-gray-200 rounded hover:bg-gray-200 dark:hover:bg-gray-600;
}

.grid-view {
  @apply grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3;
}

.grid-item {
  @apply relative;
}

.list-view {
  @apply space-y-2;
}

.list-item {
  @apply relative;
}

.preview-modal {
  @apply fixed inset-0 z-50 flex items-center justify-center;
}

.preview-overlay {
  @apply absolute inset-0 bg-black bg-opacity-50;
}

.preview-content {
  @apply relative bg-white dark:bg-gray-800 rounded-lg shadow-xl 
         max-w-6xl w-full max-h-[90vh] overflow-hidden m-4;
}

.preview-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.preview-title {
  @apply flex items-center space-x-3 flex-1;
}

.preview-title i {
  @apply text-xl text-gray-500 dark:text-gray-400;
}

.preview-title span {
  @apply font-medium text-gray-900 dark:text-white truncate;
}

.preview-actions {
  @apply flex items-center space-x-2;
}

.preview-btn {
  @apply p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
         dark:hover:text-gray-200 rounded hover:bg-gray-100 dark:hover:bg-gray-700;
}

.preview-btn.delete {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

.preview-btn.close {
  @apply hover:text-gray-700 dark:hover:text-gray-300;
}

.preview-body {
  @apply p-4 max-h-[calc(90vh-80px)] overflow-auto;
}

/* 图片预览 */
.image-preview {
  @apply flex flex-col items-center;
}

.preview-controls {
  @apply flex items-center justify-center space-x-4 mb-4;
}

.zoom-btn {
  @apply p-2 bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 
         rounded hover:bg-gray-300 dark:hover:bg-gray-600 disabled:opacity-50;
}

.zoom-level {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300 min-w-[60px] text-center;
}

.image-container {
  @apply overflow-auto max-h-[60vh] flex items-center justify-center;
}

.preview-image {
  @apply max-w-full h-auto object-contain transition-transform duration-200;
}

/* 视频预览 */
.video-preview {
  @apply flex justify-center;
}

.preview-video {
  @apply max-w-full max-h-[60vh];
}

/* 音频预览 */
.audio-preview {
  @apply flex flex-col items-center space-y-4;
}

.audio-player {
  @apply w-full max-w-md bg-gray-50 dark:bg-gray-700 rounded-lg p-4;
}

.audio-info {
  @apply flex items-center space-x-3 mb-4;
}

.audio-info i {
  @apply text-2xl text-gray-500 dark:text-gray-400;
}

.audio-details {
  @apply flex-1;
}

.audio-title {
  @apply font-medium text-gray-900 dark:text-white;
}

.audio-meta {
  @apply flex items-center space-x-4 text-sm text-gray-500 dark:text-gray-400;
}

.audio-controls {
  @apply w-full;
}

/* PDF预览 */
.pdf-preview {
  @apply flex justify-center;
}

.pdf-viewer {
  @apply w-full h-[60vh] border-0;
}

/* 文本预览 */
.text-preview {
  @apply flex flex-col space-y-4;
}

.text-controls {
  @apply flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.text-info {
  @apply flex items-center space-x-3;
}

.text-info i {
  @apply text-gray-500 dark:text-gray-400;
}

.text-size {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.text-actions {
  @apply flex items-center space-x-2;
}

.text-action-btn {
  @apply p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
         dark:hover:text-gray-200 rounded hover:bg-gray-200 dark:hover:bg-gray-600;
}

.text-content {
  @apply bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 
         rounded-lg p-4 text-sm text-gray-900 dark:text-white overflow-auto 
         max-h-[50vh] font-mono;
}

.text-content.wrap-text {
  @apply whitespace-pre-wrap;
}

/* 代码预览 */
.code-preview {
  @apply flex flex-col space-y-4;
}

.code-controls {
  @apply flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.code-info {
  @apply flex items-center space-x-3;
}

.code-info i {
  @apply text-gray-500 dark:text-gray-400;
}

.language-select {
  @apply px-3 py-1 border border-gray-300 dark:border-gray-600 rounded 
         bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm;
}

.code-actions {
  @apply flex items-center space-x-2;
}

.code-action-btn {
  @apply p-2 text-gray-500 hover:text-gray-700 dark:text-gray-400 
         dark:hover:text-gray-200 rounded hover:bg-gray-200 dark:hover:bg-gray-600;
}

.code-content {
  @apply bg-gray-900 text-gray-100 rounded-lg p-4 overflow-auto 
         max-h-[50vh] font-mono text-sm;
}

.code-content.show-line-numbers {
  @apply bg-gradient-to-r from-gray-800 to-gray-900;
}

/* 文件预览 */
.file-preview {
  @apply flex flex-col items-center justify-center space-y-6 py-12;
}

.file-preview-icon {
  @apply text-6xl text-gray-400 dark:text-gray-600;
}

.file-preview-info {
  @apply text-center space-y-2;
}

.file-preview-name {
  @apply text-lg font-medium text-gray-900 dark:text-white;
}

.file-preview-meta {
  @apply flex items-center justify-center space-x-4 text-sm text-gray-500 dark:text-gray-400;
}

.file-size,
.file-type {
  @apply flex items-center space-x-1;
}

.file-preview-actions {
  @apply mt-4;
}

.file-action-btn {
  @apply px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 深色模式优化 */
.dark .preview-content {
  @apply bg-gray-800;
}

.dark .preview-header {
  @apply border-gray-700;
}

.dark .preview-title span {
  @apply text-white;
}

.dark .preview-btn {
  @apply text-gray-400 hover:text-gray-200 hover:bg-gray-700;
}

.dark .preview-btn.delete {
  @apply hover:text-red-400;
}

.dark .preview-controls .zoom-btn {
  @apply bg-gray-700 text-gray-300 hover:bg-gray-600;
}

.dark .zoom-level {
  @apply text-gray-300;
}

.dark .audio-player {
  @apply bg-gray-700;
}

.dark .audio-title {
  @apply text-white;
}

.dark .audio-meta {
  @apply text-gray-400;
}

.dark .text-controls {
  @apply bg-gray-700;
}

.dark .text-info span {
  @apply text-gray-300;
}

.dark .text-size {
  @apply text-gray-400;
}

.dark .text-action-btn {
  @apply text-gray-400 hover:text-gray-200 hover:bg-gray-600;
}

.dark .text-content {
  @apply bg-gray-900 text-white border-gray-700;
}

.dark .code-controls {
  @apply bg-gray-700;
}

.dark .code-info span {
  @apply text-gray-300;
}

.dark .language-select {
  @apply bg-gray-800 text-white border-gray-600;
}

.dark .code-action-btn {
  @apply text-gray-400 hover:text-gray-200 hover:bg-gray-600;
}

.dark .file-preview-name {
  @apply text-white;
}

.dark .file-size,
.dark .file-type {
  @apply text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .preview-content {
    @apply m-2;
  }
  
  .preview-header {
    @apply flex-col space-y-2 items-start;
  }
  
  .preview-actions {
    @apply w-full justify-end;
  }
  
  .grid-view {
    @apply grid-cols-1;
  }
  
  .preview-controls {
    @apply space-x-2;
  }
  
  .zoom-level {
    @apply min-w-[50px];
  }
  
  .preview-video {
    @apply max-h-[40vh];
  }
  
  .pdf-viewer {
    @apply h-[40vh];
  }
  
  .text-content,
  .code-content {
    @apply max-h-[30vh];
  }
}

/* 动画效果 */
.preview-modal {
  @apply animate-fade-in;
}

.preview-content {
  @apply animate-scale-in;
}

.preview-image {
  @apply transition-transform duration-200 ease-in-out;
}

/* 可访问性 */
.preview-content:focus-within {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.preview-btn:focus,
.action-btn:focus,
.text-action-btn:focus,
.code-action-btn:focus,
.file-action-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .preview-content {
    @apply border-2 border-gray-300 dark:border-gray-600;
  }
  
  .preview-btn,
  .action-btn,
  .text-action-btn,
  .code-action-btn {
    @apply border border-gray-300 dark:border-gray-600;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .preview-modal,
  .preview-content {
    @apply animate-none;
  }
  
  .preview-image {
    @apply transition-none;
  }
}
</style>