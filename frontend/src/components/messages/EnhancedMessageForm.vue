<template>
  <div class="enhanced-message-form-container">
    <!-- 表单头部 -->
    <div class="form-header">
      <h3 class="form-title">
        <span v-if="mode === 'create'">发送新消息</span>
        <span v-else-if="mode === 'reply'">回复消息</span>
        <span v-else-if="mode === 'forward'">转发消息</span>
        <span v-else-if="mode === 'edit'">编辑消息</span>
      </h3>
      <div class="form-actions">
        <button
          v-if="showSaveDraft && hasUnsavedChanges"
          @click="saveDraft"
          class="action-btn draft"
          :disabled="isSavingDraft"
        >
          <i class="fas fa-save"></i>
          {{ isSavingDraft ? '保存中...' : '保存草稿' }}
        </button>
        <button
          @click="closeForm"
          class="action-btn close"
        >
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>

    <!-- 引用回复信息 -->
    <div v-if="replyToMessage" class="reply-info">
      <div class="reply-header">
        <span class="reply-label">回复：</span>
        <span class="reply-sender">{{ replyToMessage.senderName }}</span>
      </div>
      <div class="reply-content">
        {{ truncateText(replyToMessage.content, 100) }}
      </div>
    </div>

    <!-- 转发信息 -->
    <div v-if="forwardFromMessage" class="forward-info">
      <div class="forward-header">
        <span class="forward-label">转发：</span>
        <span class="forward-sender">{{ forwardFromMessage.senderName }}</span>
      </div>
      <div class="forward-content">
        {{ truncateText(forwardFromMessage.content, 100) }}
      </div>
    </div>

    <!-- 收件人选择 -->
    <div v-if="showRecipients" class="form-section">
      <label class="form-label">
        收件人
        <span v-if="requiredFields.recipients" class="required">*</span>
      </label>
      <div class="recipients-container">
        <div class="recipients-input-wrapper">
          <div class="selected-recipients">
            <span
              v-for="recipient in selectedRecipients"
              :key="recipient.id"
              class="recipient-tag"
            >
              {{ recipient.name }}
              <button
                @click="removeRecipient(recipient)"
                class="remove-recipient"
              >
                <i class="fas fa-times"></i>
              </button>
            </span>
          </div>
          <input
            ref="recipientInputRef"
            v-model="recipientSearchQuery"
            type="text"
            :placeholder="getRecipientPlaceholder()"
            class="recipients-input"
            @input="handleRecipientSearch"
            @focus="showRecipientDropdown = true"
            @keydown="handleRecipientKeydown"
            @keydown.enter.prevent="addFirstSuggestedRecipient"
            @keydown.esc="showRecipientDropdown = false"
          />
        </div>
        
        <!-- 收件人下拉菜单 -->
        <div v-if="showRecipientDropdown && filteredRecipients.length > 0" class="recipient-dropdown">
          <div
            v-for="recipient in filteredRecipients"
            :key="recipient.id"
            class="recipient-item"
            :class="{ selected: highlightedRecipientIndex === index }"
            @click="addRecipient(recipient)"
            @mouseenter="highlightedRecipientIndex = index"
          >
            <div class="recipient-avatar">
              <img
                v-if="recipient.avatar"
                :src="recipient.avatar"
                :alt="recipient.name"
                class="avatar-image"
              />
              <div v-else class="avatar-placeholder">
                {{ getAvatarInitial(recipient.name) }}
              </div>
            </div>
            <div class="recipient-info">
              <div class="recipient-name">{{ recipient.name }}</div>
              <div class="recipient-email">{{ recipient.email }}</div>
            </div>
            <div v-if="recipient.isOnline" class="online-indicator"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- 主题输入 -->
    <div v-if="showSubject" class="form-section">
      <label class="form-label">
        主题
        <span v-if="requiredFields.subject" class="required">*</span>
      </label>
      <input
        v-model="formData.subject"
        type="text"
        :placeholder="subjectPlaceholder"
        class="subject-input"
        @input="handleSubjectInput"
      />
    </div>

    <!-- 富文本编辑器 -->
    <div class="form-section">
      <label class="form-label">
        内容
        <span v-if="requiredFields.content" class="required">*</span>
      </label>
      
      <!-- 编辑器工具栏 -->
      <div class="editor-toolbar">
        <div class="toolbar-group">
          <button
            @click="formatText('bold')"
            class="toolbar-btn"
            :class="{ active: editorState.bold }"
            title="粗体"
          >
            <i class="fas fa-bold"></i>
          </button>
          <button
            @click="formatText('italic')"
            class="toolbar-btn"
            :class="{ active: editorState.italic }"
            title="斜体"
          </button>
          <button
            @click="formatText('underline')"
            class="toolbar-btn"
            :class="{ active: editorState.underline }"
            title="下划线"
          </button>
          <button
            @click="formatText('strikeThrough')"
            class="toolbar-btn"
            :class="{ active: editorState.strikeThrough }"
            title="删除线"
          </button>
        </div>
        
        <div class="toolbar-divider"></div>
        
        <div class="toolbar-group">
          <button
            @click="formatText('insertUnorderedList')"
            class="toolbar-btn"
            :class="{ active: editorState.unorderedList }"
            title="无序列表"
          >
            <i class="fas fa-list-ul"></i>
          </button>
          <button
            @click="formatText('insertOrderedList')"
            class="toolbar-btn"
            :class="{ active: editorState.orderedList }"
            title="有序列表"
          >
            <i class="fas fa-list-ol"></i>
          </button>
        </div>
        
        <div class="toolbar-divider"></div>
        
        <div class="toolbar-group">
          <button
            @click="insertLink"
            class="toolbar-btn"
            title="插入链接"
          >
            <i class="fas fa-link"></i>
          </button>
          <button
            @click="insertImage"
            class="toolbar-btn"
            title="插入图片"
          >
            <i class="fas fa-image"></i>
          </button>
          <button
            @click="insertCode"
            class="toolbar-btn"
            title="插入代码"
          >
            <i class="fas fa-code"></i>
          </button>
        </div>
        
        <div class="toolbar-divider"></div>
        
        <div class="toolbar-group">
          <button
            @click="toggleMention"
            class="toolbar-btn"
            :class="{ active: showMentionDropdown }"
            title="@提及"
          >
            <i class="fas fa-at"></i>
          </button>
          <button
            @click="insertEmoji"
            class="toolbar-btn"
            title="插入表情"
          >
            <i class="fas fa-smile"></i>
          </button>
        </div>
      </div>

      <!-- 编辑器内容区 -->
      <div
        ref="editorRef"
        class="message-editor"
        contenteditable="true"
        :placeholder="contentPlaceholder"
        @input="handleEditorInput"
        @paste="handleEditorPaste"
        @keydown="handleEditorKeydown"
        @focus="handleEditorFocus"
        @blur="handleEditorBlur"
        @click="handleEditorClick"
      ></div>

      <!-- @提及下拉菜单 -->
      <div v-if="showMentionDropdown && mentionSuggestions.length > 0" class="mention-dropdown">
        <div
          v-for="user in mentionSuggestions"
          :key="user.id"
          class="mention-item"
          :class="{ selected: highlightedMentionIndex === index }"
          @click="insertMention(user)"
          @mouseenter="highlightedMentionIndex = index"
        >
          <div class="mention-avatar">
            <img
              v-if="user.avatar"
              :src="user.avatar"
              :alt="user.name"
              class="avatar-image"
            />
            <div v-else class="avatar-placeholder">
              {{ getAvatarInitial(user.name) }}
            </div>
          </div>
          <div class="mention-info">
            <div class="mention-name">{{ user.name }}</div>
            <div class="mention-email">{{ user.email }}</div>
          </div>
        </div>
      </div>

      <!-- 字符计数器 -->
      <div class="char-counter">
        <span :class="{ 'text-warning': charCount > maxContentLength * 0.9, 'text-danger': charCount > maxContentLength }">
          {{ charCount }} / {{ maxContentLength }}
        </span>
      </div>
    </div>

    <!-- 附件上传 -->
    <div v-if="showAttachments" class="form-section">
      <label class="form-label">附件</label>
      
      <!-- 拖拽上传区域 -->
      <div
        class="upload-area"
        :class="{ 'drag-over': isDragOver }"
        @dragover.prevent="handleDragOver"
        @dragleave.prevent="handleDragLeave"
        @drop.prevent="handleDrop"
      >
        <div class="upload-content">
          <i class="fas fa-cloud-upload-alt upload-icon"></i>
          <p class="upload-text">拖拽文件到此处或点击上传</p>
          <input
            ref="fileInputRef"
            type="file"
            multiple
            :accept="allowedFileTypes"
            @change="handleFileSelect"
            class="file-input"
          />
          <button @click="triggerFileSelect" class="upload-btn">
            选择文件
          </button>
        </div>
      </div>

      <!-- 已上传附件列表 -->
      <div v-if="attachments.length > 0" class="attachments-list">
        <div
          v-for="attachment in attachments"
          :key="attachment.id"
          class="attachment-item"
        >
          <div class="attachment-info">
            <div class="attachment-icon">
              <i :class="getAttachmentIcon(attachment.type)"></i>
            </div>
            <div class="attachment-details">
              <div class="attachment-name">{{ attachment.name }}</div>
              <div class="attachment-size">{{ formatFileSize(attachment.size) }}</div>
              <div v-if="attachment.uploadProgress < 100" class="upload-progress">
                <div class="progress-bar">
                  <div
                    class="progress-fill"
                    :style="{ width: attachment.uploadProgress + '%' }"
                  ></div>
                </div>
                <span class="progress-text">{{ attachment.uploadProgress }}%</span>
              </div>
            </div>
          </div>
          <div class="attachment-actions">
            <button
              v-if="attachment.uploadProgress === 100"
              @click="previewAttachment(attachment)"
              class="attachment-action-btn"
              title="预览"
            >
              <i class="fas fa-eye"></i>
            </button>
            <button
              @click="removeAttachment(attachment)"
              class="attachment-action-btn remove"
              title="删除"
            >
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 消息设置 -->
    <div class="form-section">
      <div class="settings-grid">
        <!-- 优先级 -->
        <div class="setting-item">
          <label class="setting-label">优先级</label>
          <select v-model="formData.priority" class="setting-select">
            <option value="NORMAL">普通</option>
            <option value="HIGH">高</option>
            <option value="URGENT">紧急</option>
          </select>
        </div>

        <!-- 定时发送 -->
        <div class="setting-item">
          <label class="setting-label">
            <input
              v-model="enableScheduledSend"
              type="checkbox"
              class="setting-checkbox"
            />
            定时发送
          </label>
        </div>

        <!-- 发送时间 -->
        <div v-if="enableScheduledSend" class="setting-item full-width">
          <label class="setting-label">发送时间</label>
          <input
            v-model="formData.scheduledTime"
            type="datetime-local"
            class="setting-input"
            :min="getMinScheduledTime()"
          />
        </div>

        <!-- 消息类型 -->
        <div class="setting-item">
          <label class="setting-label">消息类型</label>
          <select v-model="formData.messageType" class="setting-select">
            <option value="USER">用户消息</option>
            <option value="NOTIFICATION">通知消息</option>
            <option value="BROADCAST">广播消息</option>
          </select>
        </div>

        <!-- 标签 -->
        <div class="setting-item full-width">
          <label class="setting-label">标签</label>
          <div class="tags-input">
            <input
              v-model="newTag"
              type="text"
              placeholder="输入标签名称"
              class="tag-input"
              @keyup.enter="addTag"
              @keyup.comma="addTag"
            />
            <div class="selected-tags">
              <span
                v-for="tag in formData.tags"
                :key="tag"
                class="tag"
              >
                {{ tag }}
                <button
                  @click="removeTag(tag)"
                  class="remove-tag"
                >
                  <i class="fas fa-times"></i>
                </button>
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 表单操作 -->
    <div class="form-actions-section">
      <div class="action-buttons">
        <!-- 预览按钮 -->
        <button
          v-if="showPreview"
          @click="togglePreview"
          class="action-btn preview"
        >
          <i class="fas fa-eye"></i>
          {{ showPreviewContent ? '隐藏预览' : '预览' }}
        </button>

        <!-- 模板选择 -->
        <div v-if="showTemplates" class="template-selector">
          <select
            v-model="selectedTemplate"
            @change="applyTemplate"
            class="template-select"
          >
            <option value="">选择模板</option>
            <option
              v-for="template in messageTemplates"
              :key="template.id"
              :value="template.id"
            >
              {{ template.name }}
            </option>
          </select>
        </div>

        <!-- 发送按钮 -->
        <button
          @click="submitForm"
          class="action-btn primary"
          :disabled="!canSubmit || isSubmitting"
        >
          <i class="fas fa-paper-plane"></i>
          {{ getSubmitButtonText() }}
        </button>
      </div>
    </div>

    <!-- 预览区域 -->
    <div v-if="showPreviewContent" class="preview-section">
      <div class="preview-header">
        <h4>消息预览</h4>
      </div>
      <div class="preview-content">
        <div v-if="formData.subject" class="preview-subject">
          <strong>主题：</strong>{{ formData.subject }}
        </div>
        <div class="preview-body" v-html="previewContent"></div>
        <div v-if="attachments.length > 0" class="preview-attachments">
          <strong>附件：</strong>
          <ul>
            <li v-for="attachment in attachments" :key="attachment.id">
              {{ attachment.name }} ({{ formatFileSize(attachment.size) }})
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { MessagePriority, MessageType } from '@/types/message'
import type { Message, User, MessageAttachment, MessageTemplate } from '@/types/message'

// 定义Props
interface Props {
  mode?: 'create' | 'reply' | 'forward' | 'edit'
  message?: Message
  replyTo?: Message
  forwardFrom?: Message
  recipients?: User[]
  availableRecipients?: User[]
  showRecipients?: boolean
  showSubject?: boolean
  showAttachments?: boolean
  showPreview?: boolean
  showTemplates?: boolean
  showSaveDraft?: boolean
  enableRichText?: boolean
  enableMentions?: boolean
  enableScheduledSend?: boolean
  enableTemplates?: boolean
  maxAttachments?: number
  maxFileSize?: number
  maxContentLength?: number
  allowedFileTypes?: string
  subjectPlaceholder?: string
  contentPlaceholder?: string
  submitButtonText?: string
  autoSave?: boolean
  autoSaveInterval?: number
  requiredFields?: {
    recipients?: boolean
    subject?: boolean
    content?: boolean
  }
}

const props = withDefaults(defineProps<Props>(), {
  mode: 'create',
  recipients: () => [],
  availableRecipients: () => [],
  showRecipients: true,
  showSubject: true,
  showAttachments: true,
  showPreview: true,
  showTemplates: true,
  showSaveDraft: true,
  enableRichText: true,
  enableMentions: true,
  enableScheduledSend: true,
  enableTemplates: true,
  maxAttachments: 5,
  maxFileSize: 10 * 1024 * 1024, // 10MB
  maxContentLength: 5000,
  allowedFileTypes: '*',
  subjectPlaceholder: '输入消息主题...',
  contentPlaceholder: '输入消息内容...',
  submitButtonText: '',
  autoSave: true,
  autoSaveInterval: 30000, // 30秒
  requiredFields: () => ({
    recipients: true,
    subject: false,
    content: true
  })
})

// 定义Emits
interface Emits {
  (e: 'submit', formData: FormData): void
  (e: 'cancel'): void
  (e: 'draft-save', draftData: FormData): void
  (e: 'preview-change', showPreview: boolean): void
  (e: 'attachment-add', attachment: File): void
  (e: 'attachment-remove', attachment: MessageAttachment): void
  (e: 'recipient-add', recipient: User): void
  (e: 'recipient-remove', recipient: User): void
  (e: 'content-change', content: string): void
  (e: 'validation', isValid: boolean, errors: string[]): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const editorRef = ref<HTMLElement | null>(null)
const recipientInputRef = ref<HTMLInputElement | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)

// 表单数据
const formData = ref({
  recipients: [] as User[],
  subject: props.message?.subject || '',
  content: props.message?.content || '',
  priority: props.message?.priority || MessagePriority.NORMAL,
  messageType: props.message?.messageType || MessageType.USER,
  scheduledTime: '',
  tags: [] as string[]
})

// UI状态
const selectedRecipients = ref<User[]>(props.recipients)
const attachments = ref<MessageAttachment[]>([])
const showRecipientDropdown = ref(false)
const showMentionDropdown = ref(false)
const showPreviewContent = ref(false)
const enableScheduledSend = ref(false)
const isDragOver = ref(false)
const isSubmitting = ref(false)
const isSavingDraft = ref(false)
const hasUnsavedChanges = ref(false)

// 搜索和筛选状态
const recipientSearchQuery = ref('')
const highlightedRecipientIndex = ref(-1)
const highlightedMentionIndex = ref(-1)
const mentionQuery = ref('')
const newTag = ref('')
const selectedTemplate = ref('')

// 编辑器状态
const editorState = ref({
  bold: false,
  italic: false,
  underline: false,
  strikeThrough: false,
  unorderedList: false,
  orderedList: false
})

// 自动保存定时器
let autoSaveTimer: number | null = null

// 计算属性
const filteredRecipients = computed(() => {
  const query = recipientSearchQuery.value.toLowerCase()
  return props.availableRecipients.filter(recipient => {
    const isSelected = selectedRecipients.value.some(r => r.id === recipient.id)
    const matchesSearch = recipient.name.toLowerCase().includes(query) ||
                         recipient.email.toLowerCase().includes(query)
    return !isSelected && matchesSearch
  }).slice(0, 10) // 限制显示数量
})

const mentionSuggestions = computed(() => {
  if (!mentionQuery.value) return []
  
  const query = mentionQuery.value.toLowerCase()
  return props.availableRecipients.filter(user =>
    user.name.toLowerCase().includes(query) ||
    user.email.toLowerCase().includes(query)
  ).slice(0, 8)
})

const charCount = computed(() => {
  return formData.value.content.length
})

const canSubmit = computed(() => {
  // 验证必填字段
  if (props.requiredFields.recipients && selectedRecipients.value.length === 0) {
    return false
  }
  
  if (props.requiredFields.subject && !formData.value.subject.trim()) {
    return false
  }
  
  if (props.requiredFields.content && !formData.value.content.trim()) {
    return false
  }
  
  // 验证附件数量
  if (attachments.value.length > props.maxAttachments) {
    return false
  }
  
  // 验证字符数
  if (charCount.value > props.maxContentLength) {
    return false
  }
  
  // 验证定时发送时间
  if (enableScheduledSend.value && formData.value.scheduledTime) {
    const scheduledTime = new Date(formData.value.scheduledTime)
    if (scheduledTime <= new Date()) {
      return false
    }
  }
  
  return !isSubmitting.value
})

const previewContent = computed(() => {
  return formData.value.content
})

// 消息模板
const messageTemplates = ref<MessageTemplate[]>([
  {
    id: 'greeting',
    name: '问候语',
    subject: '问候',
    content: '你好！希望一切都好。',
    priority: MessagePriority.NORMAL
  },
  {
    id: 'meeting',
    name: '会议邀请',
    subject: '会议邀请',
    content: '诚邀您参加会议，详情如下：\n\n时间：\n地点：\n议程：',
    priority: MessagePriority.HIGH
  },
  {
    id: 'follow-up',
    name: '跟进',
    subject: '跟进',
    content: '想跟进一下我们之前讨论的事项。',
    priority: MessagePriority.NORMAL
  }
])

// 方法定义
const closeForm = () => {
  if (hasUnsavedChanges.value) {
    if (confirm('您有未保存的更改，确定要关闭吗？')) {
      emit('cancel')
    }
  } else {
    emit('cancel')
  }
}

// 收件人相关方法
const getRecipientPlaceholder = () => {
  if (selectedRecipients.value.length === 0) {
    return '输入收件人姓名或邮箱...'
  }
  return '添加更多收件人...'
}

const handleRecipientSearch = () => {
  showRecipientDropdown.value = true
  highlightedRecipientIndex.value = -1
}

const handleRecipientKeydown = (event: KeyboardEvent) => {
  if (!showRecipientDropdown.value) return
  
  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault()
      highlightedRecipientIndex.value = Math.min(
        highlightedRecipientIndex.value + 1,
        filteredRecipients.value.length - 1
      )
      break
    case 'ArrowUp':
      event.preventDefault()
      highlightedRecipientIndex.value = Math.max(
        highlightedRecipientIndex.value - 1,
        -1
      )
      break
    case 'Enter':
      event.preventDefault()
      if (highlightedRecipientIndex.value >= 0) {
        addRecipient(filteredRecipients.value[highlightedRecipientIndex.value])
      }
      break
    case 'Escape':
      showRecipientDropdown.value = false
      highlightedRecipientIndex.value = -1
      break
  }
}

const addFirstSuggestedRecipient = () => {
  if (filteredRecipients.value.length > 0) {
    addRecipient(filteredRecipients.value[0])
  }
}

const addRecipient = (recipient: User) => {
  if (!selectedRecipients.value.some(r => r.id === recipient.id)) {
    selectedRecipients.value.push(recipient)
    recipientSearchQuery.value = ''
    showRecipientDropdown.value = false
    highlightedRecipientIndex.value = -1
    hasUnsavedChanges.value = true
    emit('recipient-add', recipient)
  }
}

const removeRecipient = (recipient: User) => {
  selectedRecipients.value = selectedRecipients.value.filter(r => r.id !== recipient.id)
  hasUnsavedChanges.value = true
  emit('recipient-remove', recipient)
}

// 编辑器相关方法
const formatText = (command: string) => {
  if (!editorRef.value) return
  
  document.execCommand(command, false)
  updateEditorState()
  hasUnsavedChanges.value = true
}

const updateEditorState = () => {
  if (!editorRef.value) return
  
  const selection = window.getSelection()
  if (!selection) return
  
  editorState.value = {
    bold: document.queryCommandState('bold'),
    italic: document.queryCommandState('italic'),
    underline: document.queryCommandState('underline'),
    strikeThrough: document.queryCommandState('strikeThrough'),
    unorderedList: document.queryCommandState('insertUnorderedList'),
    orderedList: document.queryCommandState('insertOrderedList')
  }
}

const handleEditorInput = () => {
  if (editorRef.value) {
    formData.value.content = editorRef.value.innerHTML
    hasUnsavedChanges.value = true
    emit('content-change', formData.value.content)
  }
}

const handleEditorPaste = (event: ClipboardEvent) => {
  event.preventDefault()
  
  const text = event.clipboardData?.getData('text/plain') || ''
  const html = event.clipboardData?.getData('text/html') || ''
  
  if (html && props.enableRichText) {
    document.execCommand('insertHTML', false, html)
  } else {
    document.execCommand('insertText', false, text)
  }
  
  hasUnsavedChanges.value = true
}

const handleEditorKeydown = (event: KeyboardEvent) => {
  // @提及逻辑
  if (props.enableMentions && event.key === '@' && event.target === editorRef.value) {
    const selection = window.getSelection()
    if (selection) {
      const range = selection.getRangeAt(0)
      const textBeforeCursor = range.startContainer.textContent?.substring(0, range.startOffset) || ''
      
      // 检查@前面是否有空格或是在行首
      if (textBeforeCursor.endsWith(' ') || textBeforeCursor.length === 0) {
        event.preventDefault()
        showMentionDropdown.value = true
        mentionQuery.value = ''
        highlightedMentionIndex.value = -1
      }
    }
  }
}

const handleEditorFocus = () => {
  updateEditorState()
}

const handleEditorBlur = () => {
  setTimeout(() => {
    showMentionDropdown.value = false
  }, 200)
}

const handleEditorClick = () => {
  updateEditorState()
}

const toggleMention = () => {
  showMentionDropdown.value = !showMentionDropdown.value
  if (showMentionDropdown.value) {
    nextTick(() => {
      editorRef.value?.focus()
    })
  }
}

const insertMention = (user: User) => {
  if (!editorRef.value) return
  
  const mentionText = `@${user.name}`
  document.execCommand('insertText', false, mentionText)
  
  showMentionDropdown.value = false
  hasUnsavedChanges.value = true
}

// 插入功能
const insertLink = () => {
  const url = prompt('请输入链接地址：')
  if (url) {
    const text = prompt('请输入链接文字：') || url
    document.execCommand('createLink', false, url)
    hasUnsavedChanges.value = true
  }
}

const insertImage = () => {
  const url = prompt('请输入图片地址：')
  if (url) {
    document.execCommand('insertImage', false, url)
    hasUnsavedChanges.value = true
  }
}

const insertCode = () => {
  const code = prompt('请输入代码：')
  if (code) {
    document.execCommand('insertHTML', false, `<code>${code}</code>`)
    hasUnsavedChanges.value = true
  }
}

const insertEmoji = () => {
  const emojis = ['😀', '😃', '😄', '😁', '😅', '😂', '🤣', '😊', '😇', '🙂', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏']
  const emoji = emojis[Math.floor(Math.random() * emojis.length)]
  document.execCommand('insertText', false, emoji)
  hasUnsavedChanges.value = true
}

// 附件相关方法
const triggerFileSelect = () => {
  fileInputRef.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files
  if (files) {
    handleFiles(files)
  }
}

const handleDrop = (event: DragEvent) => {
  isDragOver.value = false
  const files = event.dataTransfer?.files
  if (files) {
    handleFiles(files)
  }
}

const handleDragOver = (event: DragEvent) => {
  event.preventDefault()
  isDragOver.value = true
}

const handleDragLeave = () => {
  isDragOver.value = false
}

const handleFiles = (files: FileList) => {
  Array.from(files).forEach(file => {
    if (attachments.value.length >= props.maxAttachments) {
      alert(`最多只能上传 ${props.maxAttachments} 个附件`)
      return
    }
    
    if (file.size > props.maxFileSize) {
      alert(`文件大小不能超过 ${formatFileSize(props.maxFileSize)}`)
      return
    }
    
    addAttachment(file)
  })
}

const addAttachment = (file: File) => {
  const attachment: MessageAttachment = {
    id: Date.now().toString() + Math.random(),
    name: file.name,
    size: file.size,
    type: file.type,
    url: URL.createObjectURL(file),
    uploadProgress: 0
  }
  
  attachments.value.push(attachment)
  hasUnsavedChanges.value = true
  emit('attachment-add', file)
  
  // 模拟上传进度
  simulateUpload(attachment)
}

const simulateUpload = (attachment: MessageAttachment) => {
  const interval = setInterval(() => {
    if (attachment.uploadProgress < 100) {
      attachment.uploadProgress += Math.random() * 20
      if (attachment.uploadProgress > 100) {
        attachment.uploadProgress = 100
      }
    } else {
      clearInterval(interval)
    }
  }, 200)
}

const removeAttachment = (attachment: MessageAttachment) => {
  attachments.value = attachments.value.filter(a => a.id !== attachment.id)
  hasUnsavedChanges.value = true
  emit('attachment-remove', attachment)
}

const previewAttachment = (attachment: MessageAttachment) => {
  // 实现附件预览逻辑
  window.open(attachment.url, '_blank')
}

// 标签相关方法
const addTag = () => {
  const tag = newTag.value.trim()
  if (tag && !formData.value.tags.includes(tag)) {
    formData.value.tags.push(tag)
    newTag.value = ''
    hasUnsavedChanges.value = true
  }
}

const removeTag = (tag: string) => {
  formData.value.tags = formData.value.tags.filter(t => t !== tag)
  hasUnsavedChanges.value = true
}

// 模板相关方法
const applyTemplate = () => {
  const template = messageTemplates.value.find(t => t.id === selectedTemplate.value)
  if (template) {
    formData.value.subject = template.subject
    formData.value.content = template.content
    formData.value.priority = template.priority
    
    if (editorRef.value) {
      editorRef.value.innerHTML = template.content
    }
    
    hasUnsavedChanges.value = true
  }
}

// 自动保存
const saveDraft = async () => {
  if (!hasUnsavedChanges.value) return
  
  isSavingDraft.value = true
  
  try {
    const draftData = {
      ...formData.value,
      recipients: selectedRecipients.value,
      attachments: attachments.value,
      mode: props.mode,
      replyTo: props.replyTo,
      forwardFrom: props.forwardFrom
    }
    
    emit('draft-save', draftData)
    hasUnsavedChanges.value = false
  } catch (error) {
    console.error('保存草稿失败:', error)
  } finally {
    isSavingDraft.value = false
  }
}

const startAutoSave = () => {
  if (autoSaveTimer) {
    clearInterval(autoSaveTimer)
  }
  
  if (props.autoSave) {
    autoSaveTimer = window.setInterval(() => {
      if (hasUnsavedChanges.value) {
        saveDraft()
      }
    }, props.autoSaveInterval)
  }
}

// 表单提交
const submitForm = async () => {
  if (!canSubmit.value) return
  
  isSubmitting.value = true
  
  try {
    const submitData = {
      ...formData.value,
      recipients: selectedRecipients.value,
      attachments: attachments.value,
      mode: props.mode,
      replyTo: props.replyTo,
      forwardFrom: props.forwardFrom,
      scheduledTime: enableScheduledSend.value ? formData.value.scheduledTime : null
    }
    
    emit('submit', submitData)
    hasUnsavedChanges.value = false
  } catch (error) {
    console.error('提交失败:', error)
  } finally {
    isSubmitting.value = false
  }
}

const getSubmitButtonText = () => {
  if (props.submitButtonText) return props.submitButtonText
  
  switch (props.mode) {
    case 'create':
      return enableScheduledSend.value ? '定时发送' : '发送'
    case 'reply':
      return '回复'
    case 'forward':
      return '转发'
    case 'edit':
      return '更新'
    default:
      return '发送'
  }
}

const getMinScheduledTime = () => {
  const now = new Date()
  now.setMinutes(now.getMinutes() + 5) // 最少5分钟后
  return now.toISOString().slice(0, 16)
}

// 工具方法
const getAvatarInitial = (name: string) => {
  return name.charAt(0).toUpperCase()
}

const truncateText = (text: string, maxLength: number) => {
  if (text.length <= maxLength) return text
  return text.substring(0, maxLength) + '...'
}

const getAttachmentIcon = (type: string) => {
  const iconMap: Record<string, string> = {
    'image': 'fas fa-image',
    'video': 'fas fa-video',
    'audio': 'fas fa-music',
    'pdf': 'fas fa-file-pdf',
    'word': 'fas fa-file-word',
    'excel': 'fas fa-file-excel',
    'powerpoint': 'fas fa-file-powerpoint',
    'text': 'fas fa-file-alt',
    'archive': 'fas fa-file-archive',
    'default': 'fas fa-file'
  }
  
  const fileType = type.toLowerCase()
  if (fileType.startsWith('image/')) return iconMap.image
  if (fileType.startsWith('video/')) return iconMap.video
  if (fileType.startsWith('audio/')) return iconMap.audio
  if (fileType.includes('pdf')) return iconMap.pdf
  if (fileType.includes('word') || fileType.includes('document')) return iconMap.word
  if (fileType.includes('excel') || fileType.includes('sheet')) return iconMap.excel
  if (fileType.includes('powerpoint') || fileType.includes('presentation')) return iconMap.powerpoint
  if (fileType.includes('text')) return iconMap.text
  if (fileType.includes('zip') || fileType.includes('rar') || fileType.includes('7z')) return iconMap.archive
  
  return iconMap.default
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const togglePreview = () => {
  showPreviewContent.value = !showPreviewContent.value
  emit('preview-change', showPreviewContent.value)
}

const handleSubjectInput = () => {
  hasUnsavedChanges.value = true
}

// 生命周期
onMounted(() => {
  // 初始化编辑器
  if (editorRef.value && props.message?.content) {
    editorRef.value.innerHTML = props.message.content
  }
  
  // 开始自动保存
  startAutoSave()
  
  // 点击外部关闭下拉菜单
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  if (autoSaveTimer) {
    clearInterval(autoSaveTimer)
  }
  
  document.removeEventListener('click', handleClickOutside)
})

const handleClickOutside = (event: Event) => {
  const target = event.target as Element
  
  // 关闭收件人下拉菜单
  if (!target.closest('.recipients-container')) {
    showRecipientDropdown.value = false
    highlightedRecipientIndex.value = -1
  }
  
  // 关闭@提及下拉菜单
  if (!target.closest('.message-editor') && !target.closest('.mention-dropdown')) {
    showMentionDropdown.value = false
  }
}

// 暴露方法
defineExpose({
  formData,
  selectedRecipients,
  attachments,
  saveDraft,
  submitForm,
  resetForm: () => {
    formData.value = {
      recipients: [],
      subject: '',
      content: '',
      priority: MessagePriority.NORMAL,
      messageType: MessageType.USER,
      scheduledTime: '',
      tags: []
    }
    selectedRecipients.value = []
    attachments.value = []
    hasUnsavedChanges.value = false
    if (editorRef.value) {
      editorRef.value.innerHTML = ''
    }
  }
})
</script>

<style scoped>
.enhanced-message-form-container {
  @apply bg-white dark:bg-gray-800 rounded-lg shadow-md;
}

/* 表单头部 */
.form-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.form-title {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.form-actions {
  @apply flex items-center space-x-2;
}

.action-btn {
  @apply px-3 py-1 text-sm rounded-lg focus:outline-none focus:ring-2 
         focus:ring-blue-500 transition-colors;
}

.action-btn.draft {
  @apply bg-gray-200 text-gray-700 hover:bg-gray-300 dark:bg-gray-700 dark:text-gray-300;
}

.action-btn.close {
  @apply text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700;
}

.action-btn.primary {
  @apply bg-blue-600 text-white hover:bg-blue-700;
}

.action-btn.primary:disabled {
  @apply bg-gray-400 cursor-not-allowed;
}

/* 引用和转发信息 */
.reply-info,
.forward-info {
  @apply p-3 mb-4 bg-blue-50 dark:bg-blue-900 rounded-lg border-l-4 border-blue-500;
}

.reply-header,
.forward-header {
  @apply flex items-center space-x-2 mb-1;
}

.reply-label,
.forward-label {
  @apply text-sm font-medium text-blue-700 dark:text-blue-300;
}

.reply-sender,
.forward-sender {
  @apply text-sm font-medium text-gray-900 dark:text-white;
}

.reply-content,
.forward-content {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

/* 表单区域 */
.form-section {
  @apply p-4 border-b border-gray-200 dark:border-gray-700;
}

.form-label {
  @apply block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2;
}

.required {
  @apply text-red-500 ml-1;
}

/* 收件人选择 */
.recipients-container {
  @apply relative;
}

.recipients-input-wrapper {
  @apply flex flex-wrap items-center gap-2 p-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-700 min-h-[42px];
}

.selected-recipients {
  @apply flex flex-wrap items-center gap-1;
}

.recipient-tag {
  @apply flex items-center space-x-1 px-2 py-1 bg-blue-100 dark:bg-blue-900 
         text-blue-700 dark:text-blue-300 rounded-full text-sm;
}

.remove-recipient {
  @apply text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-200;
}

.recipients-input {
  @apply flex-1 min-w-[120px] border-none outline-none bg-transparent 
         text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400;
}

.recipient-dropdown {
  @apply absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-800 
         rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 max-h-60 overflow-y-auto z-10;
}

.recipient-item {
  @apply flex items-center space-x-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-700 
         cursor-pointer transition-colors;
}

.recipient-item.selected {
  @apply bg-blue-50 dark:bg-blue-900;
}

.recipient-avatar {
  @apply flex-shrink-0;
}

.avatar-image {
  @apply w-8 h-8 rounded-full object-cover;
}

.avatar-placeholder {
  @apply w-8 h-8 rounded-full bg-blue-500 text-white flex items-center 
         justify-center text-sm font-medium;
}

.recipient-info {
  @apply flex-1 min-w-0;
}

.recipient-name {
  @apply text-sm font-medium text-gray-900 dark:text-white truncate;
}

.recipient-email {
  @apply text-xs text-gray-500 dark:text-gray-400 truncate;
}

.online-indicator {
  @apply w-2 h-2 bg-green-500 rounded-full;
}

/* 主题输入 */
.subject-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 富文本编辑器 */
.editor-toolbar {
  @apply flex items-center space-x-2 p-2 border-b border-gray-200 dark:border-gray-700 
         bg-gray-50 dark:bg-gray-700 rounded-t-lg;
}

.toolbar-group {
  @apply flex items-center space-x-1;
}

.toolbar-divider {
  @apply w-px h-6 bg-gray-300 dark:bg-gray-600;
}

.toolbar-btn {
  @apply p-2 text-gray-600 dark:text-gray-400 hover:text-gray-800 dark:hover:text-gray-200 
         hover:bg-gray-200 dark:hover:bg-gray-600 rounded transition-colors;
}

.toolbar-btn.active {
  @apply bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400;
}

.message-editor {
  @apply w-full min-h-[150px] max-h-[400px] p-3 border border-gray-300 dark:border-gray-600 
         rounded-b-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500 overflow-y-auto;
}

.message-editor:empty:before {
  content: attr(placeholder);
  @apply text-gray-500 dark:text-gray-400;
}

/* @提及下拉菜单 */
.mention-dropdown {
  @apply absolute bg-white dark:bg-gray-800 rounded-lg shadow-lg 
         border border-gray-200 dark:border-gray-700 max-h-48 overflow-y-auto z-20;
}

.mention-item {
  @apply flex items-center space-x-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-700 
         cursor-pointer transition-colors min-w-[200px];
}

.mention-item.selected {
  @apply bg-blue-50 dark:bg-blue-900;
}

.mention-avatar {
  @apply flex-shrink-0;
}

.mention-info {
  @apply flex-1 min-w-0;
}

.mention-name {
  @apply text-sm font-medium text-gray-900 dark:text-white truncate;
}

.mention-email {
  @apply text-xs text-gray-500 dark:text-gray-400 truncate;
}

/* 字符计数器 */
.char-counter {
  @apply text-right text-sm text-gray-500 dark:text-gray-400 mt-1;
}

.text-warning {
  @apply text-yellow-600 dark:text-yellow-400;
}

.text-danger {
  @apply text-red-600 dark:text-red-400;
}

/* 附件上传 */
.upload-area {
  @apply border-2 border-dashed border-gray-300 dark:border-gray-600 
         rounded-lg p-6 text-center transition-colors;
}

.upload-area.drag-over {
  @apply border-blue-500 bg-blue-50 dark:bg-blue-900;
}

.upload-content {
  @apply space-y-3;
}

.upload-icon {
  @apply text-4xl text-gray-400 dark:text-gray-600;
}

.upload-text {
  @apply text-gray-600 dark:text-gray-400;
}

.file-input {
  @apply hidden;
}

.upload-btn {
  @apply px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 附件列表 */
.attachments-list {
  @apply mt-3 space-y-2;
}

.attachment-item {
  @apply flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 
         rounded-lg;
}

.attachment-info {
  @apply flex items-center space-x-3 flex-1 min-w-0;
}

.attachment-icon {
  @apply flex-shrink-0 text-gray-500 dark:text-gray-400;
}

.attachment-details {
  @apply flex-1 min-w-0;
}

.attachment-name {
  @apply text-sm font-medium text-gray-900 dark:text-white truncate;
}

.attachment-size {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.upload-progress {
  @apply mt-1;
}

.progress-bar {
  @apply w-full bg-gray-200 dark:bg-gray-600 rounded-full h-1;
}

.progress-fill {
  @apply bg-blue-600 h-1 rounded-full transition-all duration-300;
}

.progress-text {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.attachment-actions {
  @apply flex items-center space-x-1;
}

.attachment-action-btn {
  @apply p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded hover:bg-gray-200 dark:hover:bg-gray-600;
}

.attachment-action-btn.remove {
  @apply hover:text-red-600 dark:hover:text-red-400;
}

/* 消息设置 */
.settings-grid {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.setting-item {
  @apply space-y-1;
}

.setting-item.full-width {
  @apply md:col-span-2;
}

.setting-label {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300;
}

.setting-select,
.setting-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.setting-checkbox {
  @apply mr-2;
}

.tags-input {
  @apply flex items-center space-x-2 p-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-700;
}

.tag-input {
  @apply flex-1 border-none outline-none bg-transparent text-gray-900 dark:text-white 
         placeholder-gray-500 dark:placeholder-gray-400;
}

.selected-tags {
  @apply flex flex-wrap items-center gap-1;
}

.tag {
  @apply flex items-center space-x-1 px-2 py-1 bg-gray-100 dark:bg-gray-700 
         text-gray-700 dark:text-gray-300 rounded-full text-sm;
}

.remove-tag {
  @apply text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300;
}

/* 表单操作 */
.form-actions-section {
  @apply p-4 bg-gray-50 dark:bg-gray-700 rounded-b-lg;
}

.action-buttons {
  @apply flex items-center justify-between;
}

.template-selector {
  @apply flex-1 max-w-xs;
}

.template-select {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 预览区域 */
.preview-section {
  @apply p-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700;
}

.preview-header {
  @apply mb-3;
}

.preview-header h4 {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300;
}

.preview-content {
  @apply bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-600;
}

.preview-subject {
  @apply mb-2;
}

.preview-body {
  @apply prose prose-sm dark:prose-invert max-w-none;
}

.preview-attachments {
  @apply mt-3;
}

.preview-attachments ul {
  @apply list-disc list-inside text-sm text-gray-600 dark:text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .form-header {
    @apply flex-col items-start space-y-2;
  }
  
  .settings-grid {
    @apply grid-cols-1;
  }
  
  .action-buttons {
    @apply flex-col space-y-2;
  }
  
  .template-selector {
    @apply w-full max-w-none;
  }
  
  .recipient-dropdown {
    @apply max-h-40;
  }
  
  .mention-dropdown {
    @apply max-h-32;
  }
}

/* 深色模式优化 */
.dark .enhanced-message-form-container {
  @apply bg-gray-800;
}

.dark .form-label {
  @apply text-gray-300;
}

.dark .subject-input,
.dark .recipients-input-wrapper,
.dark .message-editor,
.dark .setting-select,
.dark .setting-input,
.dark .tags-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .recipient-tag {
  @apply bg-blue-900 text-blue-300;
}

.dark .recipient-dropdown {
  @apply bg-gray-800 border-gray-700;
}

.dark .mention-dropdown {
  @apply bg-gray-800 border-gray-700;
}

.dark .upload-area {
  @apply border-gray-600;
}

.dark .upload-area.drag-over {
  @apply bg-blue-900 border-blue-500;
}

.dark .attachment-item {
  @apply bg-gray-700;
}

.dark .progress-bar {
  @apply bg-gray-600;
}

.dark .tag {
  @apply bg-gray-700 text-gray-300;
}

/* 动画效果 */
.upload-area {
  @apply transition-all duration-200 ease-in-out;
}

.attachment-item {
  @apply transition-all duration-200 ease-in-out;
}

.attachment-item:hover {
  @apply shadow-sm;
}

/* 可访问性 */
.recipient-item:focus,
.mention-item:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.toolbar-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.message-editor:focus-within {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .enhanced-message-form-container {
    @apply border-2 border-gray-800 dark:border-gray-200;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .upload-area,
  .attachment-item {
    @apply transition-none;
  }
}

/* 打印样式 */
@media print {
  .form-actions,
  .upload-area,
  .editor-toolbar {
    @apply hidden;
  }
  
  .message-editor {
    @apply border-none;
  }
}
</style>