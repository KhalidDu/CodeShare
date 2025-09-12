<template>
  <div class="message-form-container">
    <form @submit.prevent="handleSubmit" class="message-form">
      <!-- 表单头部 -->
      <div class="form-header">
        <h3 class="form-title">{{ formTitle }}</h3>
        <button type="button" @click="$emit('cancel')" class="close-btn">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <!-- 收件人选择 -->
      <div class="form-group">
        <label class="form-label">
          收件人 <span class="required">*</span>
        </label>
        <div class="recipient-input-container">
          <div v-if="selectedRecipient" class="selected-recipient">
            <img
              v-if="selectedRecipient.avatar"
              :src="selectedRecipient.avatar"
              :alt="selectedRecipient.name"
              class="recipient-avatar"
            />
            <div v-else class="recipient-avatar-placeholder">
              {{ getAvatarInitial(selectedRecipient.name) }}
            </div>
            <span class="recipient-name">{{ selectedRecipient.name }}</span>
            <button
              type="button"
              @click="clearRecipient"
              class="remove-recipient"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
          <input
            v-else
            v-model="recipientSearch"
            type="text"
            placeholder="搜索用户..."
            class="recipient-input"
            @focus="showRecipientDropdown = true"
            @blur="hideRecipientDropdown"
            @input="searchRecipients"
          />
          <div
            v-if="showRecipientDropdown && recipientSearch"
            class="recipient-dropdown"
          >
            <div
              v-for="recipient in filteredRecipients"
              :key="recipient.id"
              @click="selectRecipient(recipient)"
              class="recipient-option"
            >
              <img
                v-if="recipient.avatar"
                :src="recipient.avatar"
                :alt="recipient.name"
                class="option-avatar"
              />
              <div v-else class="option-avatar-placeholder">
                {{ getAvatarInitial(recipient.name) }}
              </div>
              <div class="option-info">
                <div class="option-name">{{ recipient.name }}</div>
                <div v-if="recipient.email" class="option-email">{{ recipient.email }}</div>
              </div>
            </div>
            <div v-if="filteredRecipients.length === 0" class="no-results">
              没有找到匹配的用户
            </div>
          </div>
        </div>
      </div>

      <!-- 消息主题 -->
      <div v-if="showSubject" class="form-group">
        <label class="form-label">
          主题 <span v-if="requireSubject" class="required">*</span>
        </label>
        <input
          v-model="formData.subject"
          type="text"
          placeholder="输入消息主题..."
          class="form-input"
          :class="{ 'error': errors.subject }"
        />
        <span v-if="errors.subject" class="error-message">{{ errors.subject }}</span>
      </div>

      <!-- 消息类型 -->
      <div v-if="showMessageType" class="form-group">
        <label class="form-label">消息类型</label>
        <select
          v-model="formData.messageType"
          class="form-select"
          :class="{ 'error': errors.messageType }"
        >
          <option value="">选择消息类型</option>
          <option
            v-for="type in messageTypes"
            :key="type"
            :value="type"
          >
            {{ getMessageTypeLabel(type) }}
          </option>
        </select>
        <span v-if="errors.messageType" class="error-message">{{ errors.messageType }}</span>
      </div>

      <!-- 优先级 -->
      <div v-if="showPriority" class="form-group">
        <label class="form-label">优先级</label>
        <select
          v-model="formData.priority"
          class="form-select"
        >
          <option
            v-for="priority in priorities"
            :key="priority"
            :value="priority"
          >
            {{ getPriorityLabel(priority) }}
          </option>
        </select>
      </div>

      <!-- 消息内容 -->
      <div class="form-group">
        <label class="form-label">
          消息内容 <span class="required">*</span>
        </label>
        <textarea
          v-model="formData.content"
          placeholder="输入消息内容..."
          class="form-textarea"
          :class="{ 'error': errors.content }"
          :rows="contentRows"
          @input="updateCharCount"
        ></textarea>
        <div class="textarea-footer">
          <span class="char-count">
            {{ charCount }} / {{ maxContentLength }}
          </span>
          <span v-if="charCount > maxContentLength" class="char-count error">
            内容超出限制
          </span>
        </div>
        <span v-if="errors.content" class="error-message">{{ errors.content }}</span>
      </div>

      <!-- 附件上传 -->
      <div v-if="showAttachments" class="form-group">
        <label class="form-label">附件</label>
        <div class="attachment-upload-container">
          <input
            ref="fileInput"
            type="file"
            multiple
            :accept="allowedFileTypes"
            @change="handleFileChange"
            class="file-input"
          />
          <button
            type="button"
            @click="triggerFileInput"
            class="upload-btn"
            :disabled="isUploading"
          >
            <i class="fas fa-paperclip"></i>
            选择文件
          </button>
          <span v-if="allowedFileTypes" class="file-types">
            支持的文件类型: {{ allowedFileTypes }}
          </span>
        </div>

        <!-- 上传进度 -->
        <div v-if="isUploading" class="upload-progress">
          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: uploadProgress + '%' }"
            ></div>
          </div>
          <span class="progress-text">{{ uploadProgress }}%</span>
        </div>

        <!-- 已上传文件列表 -->
        <div v-if="uploadedFiles.length > 0" class="uploaded-files">
          <div
            v-for="file in uploadedFiles"
            :key="file.id"
            class="uploaded-file"
          >
            <div class="file-info">
              <i class="fas fa-file file-icon"></i>
              <span class="file-name">{{ file.fileName }}</span>
              <span class="file-size">{{ formatFileSize(file.fileSize) }}</span>
            </div>
            <button
              type="button"
              @click="removeFile(file.id)"
              class="remove-file"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>

      <!-- 消息标签 -->
      <div v-if="showTags" class="form-group">
        <label class="form-label">标签</label>
        <input
          v-model="formData.tag"
          type="text"
          placeholder="添加标签..."
          class="form-input"
        />
        <div v-if="availableTags.length > 0" class="tag-suggestions">
          <span
            v-for="tag in availableTags"
            :key="tag"
            @click="selectTag(tag)"
            class="tag-suggestion"
          >
            {{ tag }}
          </span>
        </div>
      </div>

      <!-- 过期时间 -->
      <div v-if="showExpiration" class="form-group">
        <label class="form-label">过期时间</label>
        <input
          v-model="formData.expiresAt"
          type="datetime-local"
          class="form-input"
          :min="minExpirationDate"
        />
        <div class="expiration-options">
          <button
            type="button"
            @click="setExpiration('1d')"
            class="expiration-btn"
          >
            1天后
          </button>
          <button
            type="button"
            @click="setExpiration('1w')"
            class="expiration-btn"
          >
            1周后
          </button>
          <button
            type="button"
            @click="setExpiration('1m')"
            class="expiration-btn"
          >
            1个月后
          </button>
          <button
            type="button"
            @click="clearExpiration"
            class="expiration-btn"
          >
            清除
          </button>
        </div>
      </div>

      <!-- 表单按钮 -->
      <div class="form-actions">
        <div class="action-buttons">
          <button
            v-if="showSaveDraft"
            type="button"
            @click="saveDraft"
            class="btn btn-secondary"
            :disabled="isSubmitting"
          >
            <i class="fas fa-save"></i>
            保存草稿
          </button>
          <button
            v-if="showPreview"
            type="button"
            @click="previewMessage"
            class="btn btn-secondary"
          >
            <i class="fas fa-eye"></i>
            预览
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="isSubmitting || !isFormValid"
          >
            <i v-if="isSubmitting" class="fas fa-spinner fa-spin"></i>
            <i v-else class="fas fa-paper-plane"></i>
            {{ submitButtonText }}
          </button>
        </div>
      </div>
    </form>

    <!-- 消息预览模态框 -->
    <div v-if="showPreviewModal" class="preview-modal">
      <div class="preview-overlay" @click="closePreview"></div>
      <div class="preview-content">
        <div class="preview-header">
          <h3>消息预览</h3>
          <button @click="closePreview" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="preview-body">
          <div class="preview-recipient">
            <strong>收件人:</strong> {{ selectedRecipient?.name }}
          </div>
          <div v-if="formData.subject" class="preview-subject">
            <strong>主题:</strong> {{ formData.subject }}
          </div>
          <div class="preview-content">
            <strong>内容:</strong>
            <div class="preview-text">{{ formData.content }}</div>
          </div>
          <div v-if="uploadedFiles.length > 0" class="preview-attachments">
            <strong>附件:</strong>
            <div class="preview-files">
              <div
                v-for="file in uploadedFiles"
                :key="file.id"
                class="preview-file"
              >
                <i class="fas fa-file"></i>
                {{ file.fileName }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { messageService } from '@/services/messageService'
import type {
  Message,
  CreateMessageRequest,
  MessageType,
  MessagePriority,
  MessageAttachment,
  User
} from '@/types/message'

// 定义Props
interface Props {
  initialData?: Partial<CreateMessageRequest>
  mode?: 'create' | 'reply' | 'forward' | 'edit'
  recipient?: User
  availableRecipients?: User[]
  availableTags?: string[]
  showSubject?: boolean
  showMessageType?: boolean
  showPriority?: boolean
  showAttachments?: boolean
  showTags?: boolean
  showExpiration?: boolean
  showSaveDraft?: boolean
  showPreview?: boolean
  requireSubject?: boolean
  maxContentLength?: number
  allowedFileTypes?: string
  maxFileSize?: number
  maxAttachments?: number
}

const props = withDefaults(defineProps<Props>(), {
  initialData: () => ({}),
  mode: 'create',
  availableRecipients: () => [],
  availableTags: () => [],
  showSubject: true,
  showMessageType: true,
  showPriority: true,
  showAttachments: true,
  showTags: true,
  showExpiration: true,
  showSaveDraft: true,
  showPreview: true,
  requireSubject: false,
  maxContentLength: 10000,
  allowedFileTypes: '*',
  maxFileSize: 10 * 1024 * 1024, // 10MB
  maxAttachments: 5
})

// 定义Emits
interface Emits {
  (e: 'submit', data: CreateMessageRequest): void
  (e: 'cancel'): void
  (e: 'save-draft', data: Partial<CreateMessageRequest>): void
  (e: 'error', error: string): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const fileInput = ref<HTMLInputElement>()
const selectedRecipient = ref<User | null>(props.recipient || null)
const recipientSearch = ref('')
const showRecipientDropdown = ref(false)
const showPreviewModal = ref(false)
const isUploading = ref(false)
const uploadProgress = ref(0)
const isSubmitting = ref(false)

const formData = ref<CreateMessageRequest>({
  receiverId: '',
  subject: '',
  content: '',
  messageType: MessageType.USER,
  priority: MessagePriority.NORMAL,
  tag: '',
  expiresAt: '',
  attachmentIds: []
})

const errors = ref<Record<string, string>>({})
const uploadedFiles = ref<MessageAttachment[]>([])

// 消息类型和优先级选项
const messageTypes = Object.values(MessageType)
const priorities = Object.values(MessagePriority)

// 计算属性
const formTitle = computed(() => {
  const titles = {
    create: '发送新消息',
    reply: '回复消息',
    forward: '转发消息',
    edit: '编辑消息'
  }
  return titles[props.mode]
})

const submitButtonText = computed(() => {
  const buttons = {
    create: '发送',
    reply: '回复',
    forward: '转发',
    edit: '更新'
  }
  return buttons[props.mode]
})

const contentRows = computed(() => {
  const lines = formData.value.content.split('\n').length
  return Math.max(4, Math.min(10, lines))
})

const charCount = computed(() => formData.value.content.length)

const filteredRecipients = computed(() => {
  if (!recipientSearch.value) return props.availableRecipients
  
  const query = recipientSearch.value.toLowerCase()
  return props.availableRecipients.filter(recipient =>
    recipient.name.toLowerCase().includes(query) ||
    recipient.email?.toLowerCase().includes(query)
  )
})

const isFormValid = computed(() => {
  return (
    selectedRecipient.value &&
    formData.value.content.trim() &&
    (!props.requireSubject || formData.value.subject?.trim()) &&
    charCount.value <= props.maxContentLength
  )
})

const minExpirationDate = computed(() => {
  const now = new Date()
  now.setHours(now.getHours() + 1)
  return now.toISOString().slice(0, 16)
})

// 方法定义
const selectRecipient = (recipient: User) => {
  selectedRecipient.value = recipient
  formData.value.receiverId = recipient.id
  recipientSearch.value = ''
  showRecipientDropdown.value = false
  delete errors.value.receiverId
}

const clearRecipient = () => {
  selectedRecipient.value = null
  formData.value.receiverId = ''
  recipientSearch.value = ''
}

const hideRecipientDropdown = () => {
  setTimeout(() => {
    showRecipientDropdown.value = false
  }, 200)
}

const searchRecipients = () => {
  showRecipientDropdown.value = true
}

const getAvatarInitial = (name: string) => {
  return name.charAt(0).toUpperCase()
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

const getPriorityLabel = (priority: MessagePriority) => {
  const labels = {
    [MessagePriority.LOW]: '低优先级',
    [MessagePriority.NORMAL]: '普通优先级',
    [MessagePriority.HIGH]: '高优先级',
    [MessagePriority.URGENT]: '紧急'
  }
  return labels[priority] || priority
}

const updateCharCount = () => {
  if (charCount.value > props.maxContentLength) {
    errors.value.content = `内容不能超过 ${props.maxContentLength} 个字符`
  } else {
    delete errors.value.content
  }
}

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileChange = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  
  if (!files || files.length === 0) return

  // 检查文件数量
  if (uploadedFiles.value.length + files.length > props.maxAttachments) {
    emit('error', `最多只能上传 ${props.maxAttachments} 个文件`)
    return
  }

  // 检查文件大小和类型
  for (let i = 0; i < files.length; i++) {
    const file = files[i]
    
    if (file.size > props.maxFileSize) {
      emit('error', `文件 ${file.name} 超过大小限制`)
      continue
    }
    
    if (!messageService.isFileTypeAllowed(file, [props.allowedFileTypes])) {
      emit('error', `文件 ${file.name} 类型不支持`)
      continue
    }
    
    await uploadFile(file)
  }
  
  // 清空文件输入
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const uploadFile = async (file: File) => {
  try {
    isUploading.value = true
    uploadProgress.value = 0

    const response = await messageService.uploadFile({
      file,
      messageType: formData.value.messageType,
      onProgress: (progress) => {
        uploadProgress.value = progress
      }
    })

    if (response.success && response.attachment) {
      uploadedFiles.value.push(response.attachment)
      formData.value.attachmentIds?.push(response.attachment.id)
    }
  } catch (error) {
    emit('error', `上传文件失败: ${error}`)
  } finally {
    isUploading.value = false
    uploadProgress.value = 0
  }
}

const removeFile = (fileId: string) => {
  uploadedFiles.value = uploadedFiles.value.filter(file => file.id !== fileId)
  formData.value.attachmentIds = formData.value.attachmentIds?.filter(id => id !== fileId)
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const selectTag = (tag: string) => {
  formData.value.tag = tag
}

const setExpiration = (period: string) => {
  const now = new Date()
  let date = new Date(now)
  
  switch (period) {
    case '1d':
      date.setDate(date.getDate() + 1)
      break
    case '1w':
      date.setDate(date.getDate() + 7)
      break
    case '1m':
      date.setMonth(date.getMonth() + 1)
      break
  }
  
  formData.value.expiresAt = date.toISOString().slice(0, 16)
}

const clearExpiration = () => {
  formData.value.expiresAt = ''
}

const validateForm = () => {
  errors.value = {}
  
  if (!selectedRecipient.value) {
    errors.value.receiverId = '请选择收件人'
  }
  
  if (!formData.value.content.trim()) {
    errors.value.content = '请输入消息内容'
  }
  
  if (props.requireSubject && !formData.value.subject?.trim()) {
    errors.value.subject = '请输入消息主题'
  }
  
  if (charCount.value > props.maxContentLength) {
    errors.value.content = `内容不能超过 ${props.maxContentLength} 个字符`
  }
  
  return Object.keys(errors.value).length === 0
}

const handleSubmit = async () => {
  if (!validateForm()) return
  
  try {
    isSubmitting.value = true
    
    const submitData: CreateMessageRequest = {
      ...formData.value,
      receiverId: selectedRecipient.value!.id
    }
    
    emit('submit', submitData)
  } catch (error) {
    emit('error', `提交失败: ${error}`)
  } finally {
    isSubmitting.value = false
  }
}

const saveDraft = () => {
  const draftData: Partial<CreateMessageRequest> = {
    ...formData.value,
    receiverId: selectedRecipient.value?.id
  }
  emit('save-draft', draftData)
}

const previewMessage = () => {
  showPreviewModal.value = true
}

const closePreview = () => {
  showPreviewModal.value = false
}

// 监听器
watch(() => props.initialData, (newData) => {
  if (newData) {
    formData.value = { ...formData.value, ...newData }
  }
}, { immediate: true })

watch(() => props.recipient, (newRecipient) => {
  if (newRecipient) {
    selectedRecipient.value = newRecipient
    formData.value.receiverId = newRecipient.id
  }
}, { immediate: true })

// 生命周期
onMounted(() => {
  // 初始化表单数据
  if (props.initialData) {
    formData.value = { ...formData.value, ...props.initialData }
  }
})

// 暴露方法
defineExpose({
  formData,
  selectedRecipient,
  uploadedFiles,
  isSubmitting,
  validateForm,
  resetForm: () => {
    formData.value = {
      receiverId: '',
      subject: '',
      content: '',
      messageType: MessageType.USER,
      priority: MessagePriority.NORMAL,
      tag: '',
      expiresAt: '',
      attachmentIds: []
    }
    selectedRecipient.value = null
    uploadedFiles.value = []
    errors.value = {}
  }
})
</script>

<style scoped>
.message-form-container {
  @apply bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6;
}

.form-header {
  @apply flex items-center justify-between mb-6;
}

.form-title {
  @apply text-xl font-semibold text-gray-900 dark:text-white;
}

.close-btn {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700;
}

.form-group {
  @apply mb-4;
}

.form-label {
  @apply block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2;
}

.required {
  @apply text-red-500;
}

.form-input,
.form-select,
.form-textarea {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.form-input.error,
.form-select.error,
.form-textarea.error {
  @apply border-red-500 focus:ring-red-500;
}

.form-textarea {
  @apply resize-none;
}

.textarea-footer {
  @apply flex items-center justify-between mt-1;
}

.char-count {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.char-count.error {
  @apply text-red-500;
}

.error-message {
  @apply text-sm text-red-500 mt-1;
}

.recipient-input-container {
  @apply relative;
}

.selected-recipient {
  @apply flex items-center space-x-2 p-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-gray-50 dark:bg-gray-700;
}

.recipient-avatar {
  @apply w-8 h-8 rounded-full object-cover;
}

.recipient-avatar-placeholder {
  @apply w-8 h-8 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-sm;
}

.recipient-name {
  @apply flex-1 font-medium text-gray-900 dark:text-white;
}

.remove-recipient {
  @apply p-1 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-200 
         dark:hover:bg-gray-600;
}

.recipient-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.recipient-dropdown {
  @apply absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-700 
         border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg z-10 
         max-h-60 overflow-y-auto;
}

.recipient-option {
  @apply flex items-center space-x-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-600 
         cursor-pointer;
}

.option-avatar {
  @apply w-10 h-10 rounded-full object-cover;
}

.option-avatar-placeholder {
  @apply w-10 h-10 rounded-full bg-blue-500 text-white flex items-center 
         justify-center font-semibold text-sm;
}

.option-info {
  @apply flex-1;
}

.option-name {
  @apply font-medium text-gray-900 dark:text-white;
}

.option-email {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.no-results {
  @apply p-3 text-center text-gray-500 dark:text-gray-400;
}

.attachment-upload-container {
  @apply flex items-center space-x-3;
}

.file-input {
  @apply hidden;
}

.upload-btn {
  @apply px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50;
}

.file-types {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.upload-progress {
  @apply mt-2;
}

.progress-bar {
  @apply w-full bg-gray-200 dark:bg-gray-600 rounded-full h-2;
}

.progress-fill {
  @apply bg-blue-600 h-2 rounded-full transition-all duration-300;
}

.progress-text {
  @apply text-sm text-gray-500 dark:text-gray-400 ml-2;
}

.uploaded-files {
  @apply mt-2 space-y-1;
}

.uploaded-file {
  @apply flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700 
         rounded-lg;
}

.file-info {
  @apply flex items-center space-x-2;
}

.file-icon {
  @apply text-gray-400;
}

.file-name {
  @apply text-sm font-medium text-gray-900 dark:text-white;
}

.file-size {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.remove-file {
  @apply p-1 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-200 
         dark:hover:bg-gray-600;
}

.tag-suggestions {
  @apply mt-1 flex flex-wrap gap-1;
}

.tag-suggestion {
  @apply px-2 py-1 text-xs bg-gray-100 dark:bg-gray-700 text-gray-700 
         dark:text-gray-300 rounded-full cursor-pointer hover:bg-gray-200 
         dark:hover:bg-gray-600;
}

.expiration-options {
  @apply mt-1 flex flex-wrap gap-1;
}

.expiration-btn {
  @apply px-2 py-1 text-xs bg-gray-100 dark:bg-gray-700 text-gray-700 
         dark:text-gray-300 rounded-full hover:bg-gray-200 dark:hover:bg-gray-600;
}

.form-actions {
  @apply flex justify-end mt-6;
}

.action-buttons {
  @apply flex items-center space-x-3;
}

.btn {
  @apply px-4 py-2 rounded-lg font-medium focus:outline-none focus:ring-2 
         focus:ring-offset-2 transition-colors duration-200;
}

.btn-primary {
  @apply bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500;
}

.btn-secondary {
  @apply bg-gray-200 dark:bg-gray-600 text-gray-700 dark:text-gray-300 
         hover:bg-gray-300 dark:hover:bg-gray-500 focus:ring-gray-500;
}

.btn:disabled {
  @apply opacity-50 cursor-not-allowed;
}

.preview-modal {
  @apply fixed inset-0 z-50 flex items-center justify-center;
}

.preview-overlay {
  @apply absolute inset-0 bg-black bg-opacity-50;
}

.preview-content {
  @apply relative bg-white dark:bg-gray-800 rounded-lg shadow-xl 
         max-w-2xl w-full max-h-[80vh] overflow-y-auto m-4;
}

.preview-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 
         dark:border-gray-700;
}

.preview-body {
  @apply p-4 space-y-4;
}

.preview-recipient,
.preview-subject,
.preview-content,
.preview-attachments {
  @apply text-gray-900 dark:text-white;
}

.preview-text {
  @apply mt-1 whitespace-pre-wrap bg-gray-50 dark:bg-gray-700 p-3 rounded-lg;
}

.preview-files {
  @apply mt-1 space-y-1;
}

.preview-file {
  @apply flex items-center space-x-2 text-sm text-gray-700 dark:text-gray-300;
}

/* 深色模式优化 */
.dark .form-header .form-title {
  @apply text-white;
}

.dark .form-label {
  @apply text-gray-300;
}

.dark .form-input,
.dark .form-select,
.dark .form-textarea {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .selected-recipient {
  @apply bg-gray-700 border-gray-600;
}

.dark .selected-recipient .recipient-name {
  @apply text-white;
}

.dark .recipient-dropdown {
  @apply bg-gray-700 border-gray-600;
}

.dark .recipient-option {
  @apply hover:bg-gray-600;
}

.dark .recipient-option .option-name {
  @apply text-white;
}

.dark .uploaded-file {
  @apply bg-gray-700;
}

.dark .uploaded-file .file-name {
  @apply text-white;
}

.dark .btn-secondary {
  @apply bg-gray-600 text-gray-300 hover:bg-gray-500;
}

.dark .preview-content {
  @apply bg-gray-800;
}

.dark .preview-recipient,
.dark .preview-subject,
.dark .preview-content,
.dark .preview-attachments {
  @apply text-white;
}

.dark .preview-text {
  @apply bg-gray-700;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .message-form-container {
    @apply p-4;
  }
  
  .action-buttons {
    @apply flex-col space-y-2 space-x-0;
  }
  
  .btn {
    @apply w-full;
  }
  
  .preview-content {
    @apply m-2;
  }
}

/* 动画效果 */
.preview-modal {
  @apply animate-fade-in;
}

.preview-content {
  @apply animate-scale-in;
}

/* 可访问性 */
.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .form-input,
  .form-select,
  .form-textarea {
    @apply border-2;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .preview-modal,
  .preview-content {
    @apply animate-none;
  }
}
</style>