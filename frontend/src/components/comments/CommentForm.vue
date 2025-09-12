<template>
  <div class="comment-form" :class="formClasses">
    <form @submit.prevent="handleSubmit" class="form-container">
      <!-- 回复信息 -->
      <div v-if="parentId" class="reply-info">
        <div class="reply-header">
          <span class="reply-label">回复评论</span>
          <button
            type="button"
            class="reply-cancel"
            @click="handleCancel"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div v-if="parentComment" class="reply-preview">
          <div class="reply-avatar">
            <div 
              v-if="parentComment.userAvatar"
              class="avatar-image"
              :style="{ backgroundImage: `url(${parentComment.userAvatar})` }"
            ></div>
            <div v-else class="avatar-placeholder">
              {{ parentCommentInitials }}
            </div>
          </div>
          <div class="reply-content">
            <div class="reply-user">{{ parentComment.userName }}</div>
            <div class="reply-text">{{ truncatedParentContent }}</div>
          </div>
        </div>
      </div>

      <!-- 用户头像 -->
      <div class="form-avatar">
        <div 
          v-if="currentUserAvatar"
          class="avatar-image"
          :style="{ backgroundImage: `url(${currentUserAvatar})` }"
        ></div>
        <div v-else class="avatar-placeholder">
          {{ currentUserInitials }}
        </div>
      </div>

      <!-- 表单主体 -->
      <div class="form-body">
        <!-- 文本输入区 -->
        <div class="form-textarea">
          <textarea
            ref="textareaRef"
            v-model="content"
            :placeholder="placeholder"
            :maxlength="maxLength"
            :disabled="loading"
            class="textarea-input"
            rows="4"
            @input="handleInput"
            @keydown="handleKeydown"
            @focus="handleFocus"
            @blur="handleBlur"
          ></textarea>
          
          <!-- 字符计数 -->
          <div class="char-count">
            <span :class="charCountClasses">{{ content.length }}</span>
            <span class="char-separator">/</span>
            <span class="char-max">{{ maxLength }}</span>
          </div>
        </div>

        <!-- 表单工具栏 -->
        <div class="form-toolbar">
          <!-- 格式化工具 -->
          <div class="toolbar-section">
            <button
              type="button"
              class="toolbar-btn"
              @click="insertBold"
              title="粗体"
            >
              <i class="fas fa-bold"></i>
            </button>
            <button
              type="button"
              class="toolbar-btn"
              @click="insertItalic"
              title="斜体"
            >
              <i class="fas fa-italic"></i>
            </button>
            <button
              type="button"
              class="toolbar-btn"
              @click="insertCode"
              title="代码"
            >
              <i class="fas fa-code"></i>
            </button>
            <button
              type="button"
              class="toolbar-btn"
              @click="insertLink"
              title="链接"
            >
              <i class="fas fa-link"></i>
            </button>
          </div>

          <!-- 表情选择器 -->
          <div class="toolbar-section">
            <button
              type="button"
              class="toolbar-btn"
              @click="toggleEmojiPicker"
              title="表情"
            >
              <i class="fas fa-smile"></i>
            </button>
          </div>

          <!-- 上传工具 -->
          <div v-if="enableUpload" class="toolbar-section">
            <button
              type="button"
              class="toolbar-btn"
              @click="triggerFileUpload"
              title="上传图片"
            >
              <i class="fas fa-image"></i>
            </button>
            <input
              ref="fileInputRef"
              type="file"
              class="file-input"
              accept="image/*"
              @change="handleFileUpload"
            />
          </div>
        </div>

        <!-- 预览区 -->
        <div v-if="showPreview && content" class="form-preview">
          <div class="preview-header">
            <span>预览</span>
          </div>
          <div class="preview-content" v-html="previewContent"></div>
        </div>

        <!-- 图片预览 -->
        <div v-if="uploadedImages.length > 0" class="image-preview">
          <div class="preview-grid">
            <div
              v-for="(image, index) in uploadedImages"
              :key="index"
              class="preview-item"
            >
              <img
                :src="image.url"
                :alt="image.name"
                class="preview-image"
              />
              <button
                type="button"
                class="preview-remove"
                @click="removeImage(index)"
              >
                <i class="fas fa-times"></i>
              </button>
            </div>
          </div>
        </div>

        <!-- 表单操作 -->
        <div class="form-actions">
          <!-- 左侧操作 -->
          <div class="actions-left">
            <button
              type="button"
              class="action-btn"
              @click="togglePreview"
            >
              <i :class="showPreview ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              {{ showPreview ? '隐藏预览' : '显示预览' }}
            </button>
          </div>

          <!-- 右侧操作 -->
          <div class="actions-right">
            <button
              type="button"
              class="btn-cancel"
              @click="handleCancel"
            >
              取消
            </button>
            <AnimatedButton
              type="submit"
              variant="primary"
              size="md"
              :disabled="!canSubmit"
              :loading="loading"
            >
              <i class="fas fa-paper-plane"></i>
              {{ submitButtonText }}
            </AnimatedButton>
          </div>
        </div>
      </div>
    </form>

    <!-- 表情选择器 -->
    <div v-if="showEmojiPicker" class="emoji-picker">
      <div class="emoji-grid">
        <button
          v-for="emoji in commonEmojis"
          :key="emoji"
          type="button"
          class="emoji-btn"
          @click="insertEmoji(emoji)"
        >
          {{ emoji }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import AnimatedButton from '@/components/common/AnimatedButton.vue'

// 定义Props
interface Props {
  snippetId: string
  parentId?: string | null
  parentComment?: any
  loading?: boolean
  maxLength?: number
  placeholder?: string
  enableUpload?: boolean
  enablePreview?: boolean
  autoFocus?: boolean
  initialContent?: string
}

const props = withDefaults(defineProps<Props>(), {
  parentId: null,
  parentComment: null,
  loading: false,
  maxLength: 1000,
  placeholder: '写下你的评论...',
  enableUpload: true,
  enablePreview: true,
  autoFocus: false,
  initialContent: ''
})

// 定义Emits
interface Emits {
  (e: 'submit', data: { content: string; snippetId: string; parentId?: string }): void
  (e: 'cancel'): void
  (e: 'upload', file: File): void
}

const emit = defineEmits<Emits>()

// Store
const authStore = useAuthStore()

// 响应式状态
const content = ref(props.initialContent)
const loading = ref(false)
const showPreview = ref(false)
const showEmojiPicker = ref(false)
const uploadedImages = ref<Array<{ url: string; name: string }>>([])
const textareaRef = ref<HTMLTextAreaElement>()
const fileInputRef = ref<HTMLInputElement>()

// 常用表情
const commonEmojis = [
  '😀', '😃', '😄', '😁', '😅', '😂', '🤣', '😊', '😇', '🙂', '😉', '😌', '😍',
  '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓',
  '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖',
  '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶',
  '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑',
  '😬', '🙄', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐',
  '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕', '🤑', '🤠', '😈', '👿', '👹', '👺',
  '🤡', '💩', '👻', '💀', '☠️', '👽', '👾', '🤖', '🎃', '😺', '😸', '😹', '😻',
  '😼', '😽', '🙀', '😿', '😾', '👋', '🤚', '🖐', '✋', '🖖', '👌', '🤌', '🤏',
  '✌️', '🤞', '🤟', '🤘', '🤙', '👈', '👉', '👆', '🖕', '👇', '☝️', '👍', '👎',
  '✊', '👊', '🤛', '🤜', '👏', '🙌', '👐', '🤲', '🤝', '🙏', '✍️', '💅', '🤳',
  '💪', '🦾', '🦿', '🦵', '🦶', '👂', '🦻', '👃', '🧠', '🫀', '🫁', '🦷', '🦴',
  '👀', '👁️', '👅', '👄', '👶', '🧒', '👦', '👧', '🧑', '👱', '👨', '🧔', '👩',
  '🧓', '👴', '👵', '🙍', '🙎', '🙅', '🙆', '💁', '🙋', '🧏', '🙇', '🤦', '🤷',
  '👮', '🕵️', '💂', '🥷', '👷', '🤴', '👸', '👳', '👲', '🧕', '🤰', '🤱', '👼',
  '🎅', '🤶', '🦸', '🦹', '🧙', '🧚', '🧛', '🧜', '🧝', '🧞', '🧟', '💆', '💇',
  '🚶', '🧍', '🧎', '🏃', '💃', '🕺', '🕴', '👯', '🧖', '🧗', '🤺', '🏇', '⛷',
  '🏂', '🏌️', '🏄', '🚣', '🏊', '⛹️', '🚴', '🚵', '🤸', '🤼', '🤽', '🤾', '🤹',
  '🧘', '🛀', '🛌', '👭', '👫', '👬', '💏', '💑', '👪', '🗣', '👤', '👥', '🫂'
]

// 计算属性
const formClasses = computed(() => {
  return [
    'comment-form',
    {
      'comment-form--loading': loading.value,
      'comment-form--reply': !!props.parentId,
      'comment-form--focused': isFocused.value
    }
  ]
})

const currentUser = computed(() => authStore.user)
const currentUserInitials = computed(() => {
  if (!currentUser.value?.username) return 'U'
  return currentUser.value.username.charAt(0).toUpperCase()
})

const currentUserAvatar = computed(() => {
  return currentUser.value?.avatar
})

const parentCommentInitials = computed(() => {
  if (!props.parentComment?.userName) return 'U'
  return props.parentComment.userName.charAt(0).toUpperCase()
})

const truncatedParentContent = computed(() => {
  if (!props.parentComment?.content) return ''
  const content = props.parentComment.content
  return content.length > 100 ? content.substring(0, 100) + '...' : content
})

const canSubmit = computed(() => {
  return content.value.trim().length > 0 && 
         content.value.length <= props.maxLength && 
         !loading.value
})

const submitButtonText = computed(() => {
  return props.parentId ? '回复评论' : '发表评论'
})

const charCountClasses = computed(() => {
  return [
    'char-current',
    {
      'char-count--warning': content.value.length > props.maxLength * 0.9,
      'char-count--error': content.value.length >= props.maxLength
    }
  ]
})

const previewContent = computed(() => {
  // 简单的Markdown解析
  let html = content.value
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>')
    .replace(/`(.*?)`/g, '<code>$1</code>')
    .replace(/\n/g, '<br>')
  
  return html
})

const isFocused = ref(false)

// 方法
function handleSubmit() {
  if (!canSubmit.value) return
  
  emit('submit', {
    content: content.value.trim(),
    snippetId: props.snippetId,
    parentId: props.parentId || undefined
  })
}

function handleCancel() {
  emit('cancel')
}

function handleInput() {
  // 处理输入事件
}

function handleKeydown(event: KeyboardEvent) {
  // Ctrl+Enter 提交
  if (event.ctrlKey && event.key === 'Enter') {
    event.preventDefault()
    if (canSubmit.value) {
      handleSubmit()
    }
  }
  
  // Tab键缩进
  if (event.key === 'Tab') {
    event.preventDefault()
    const textarea = textareaRef.value
    if (textarea) {
      const start = textarea.selectionStart
      const end = textarea.selectionEnd
      const newContent = content.value.substring(0, start) + '  ' + content.value.substring(end)
      content.value = newContent
      nextTick(() => {
        textarea.selectionStart = textarea.selectionEnd = start + 2
      })
    }
  }
}

function handleFocus() {
  isFocused.value = true
}

function handleBlur() {
  isFocused.value = false
}

function togglePreview() {
  showPreview.value = !showPreview.value
}

function toggleEmojiPicker() {
  showEmojiPicker.value = !showEmojiPicker.value
}

function insertEmoji(emoji: string) {
  const textarea = textareaRef.value
  if (textarea) {
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const newContent = content.value.substring(0, start) + emoji + content.value.substring(end)
    content.value = newContent
    nextTick(() => {
      textarea.selectionStart = textarea.selectionEnd = start + emoji.length
      textarea.focus()
    })
  }
  showEmojiPicker.value = false
}

function insertBold() {
  insertText('**', '**', '粗体文本')
}

function insertItalic() {
  insertText('*', '*', '斜体文本')
}

function insertCode() {
  insertText('`', '`', '代码')
}

function insertLink() {
  insertText('[', '](https://example.com)', '链接文本')
}

function insertText(before: string, after: string, placeholder: string) {
  const textarea = textareaRef.value
  if (textarea) {
    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const selectedText = content.value.substring(start, end)
    const textToInsert = selectedText || placeholder
    const newContent = content.value.substring(0, start) + before + textToInsert + after + content.value.substring(end)
    content.value = newContent
    nextTick(() => {
      textarea.selectionStart = start + before.length
      textarea.selectionEnd = start + before.length + textToInsert.length
      textarea.focus()
    })
  }
}

function triggerFileUpload() {
  fileInputRef.value?.click()
}

function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    // 这里应该处理文件上传
    emit('upload', file)
    
    // 模拟上传成功
    const reader = new FileReader()
    reader.onload = (e) => {
      uploadedImages.value.push({
        url: e.target?.result as string,
        name: file.name
      })
    }
    reader.readAsDataURL(file)
  }
}

function removeImage(index: number) {
  uploadedImages.value.splice(index, 1)
}

// 点击外部关闭表情选择器
function handleClickOutside(event: MouseEvent) {
  const target = event.target as Element
  const form = document.querySelector('.comment-form')
  if (form && !form.contains(target)) {
    showEmojiPicker.value = false
  }
}

// 生命周期
onMounted(() => {
  if (props.autoFocus) {
    nextTick(() => {
      textareaRef.value?.focus()
    })
  }
  
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

// 暴露方法
defineExpose({
  focus: () => textareaRef.value?.focus(),
  reset: () => {
    content.value = ''
    uploadedImages.value = []
    showPreview.value = false
    showEmojiPicker.value = false
  }
})
</script>

<style scoped>
.comment-form {
  position: relative;
  background: var(--gray-50);
  border: 1px solid var(--gray-200);
  border-radius: 0.75rem;
  padding: 1rem;
  transition: all 0.2s ease;
}

.comment-form--focused {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.comment-form--loading {
  opacity: 0.7;
  pointer-events: none;
}

.comment-form--reply {
  border-left: 4px solid var(--primary-500);
}

.form-container {
  display: flex;
  gap: 1rem;
}

/* 回复信息 */
.reply-info {
  flex: 1;
  margin-bottom: 1rem;
}

.reply-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.reply-label {
  font-weight: 600;
  color: var(--gray-700);
  font-size: 0.875rem;
}

.reply-cancel {
  background: none;
  border: none;
  color: var(--gray-500);
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 0.25rem;
  transition: all 0.2s ease;
}

.reply-cancel:hover {
  background: var(--gray-200);
  color: var(--gray-700);
}

.reply-preview {
  display: flex;
  gap: 0.75rem;
  padding: 0.75rem;
  background: var(--gray-100);
  border-radius: 0.5rem;
  border-left: 3px solid var(--primary-500);
}

.reply-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  overflow: hidden;
  background: var(--gray-200);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: var(--gray-600);
  font-size: 0.75rem;
  flex-shrink: 0;
}

.avatar-image {
  width: 100%;
  height: 100%;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: var(--gray-600);
}

.reply-content {
  flex: 1;
}

.reply-user {
  font-weight: 600;
  color: var(--gray-800);
  font-size: 0.813rem;
  margin-bottom: 0.25rem;
}

.reply-text {
  color: var(--gray-600);
  font-size: 0.75rem;
  line-height: 1.4;
}

/* 用户头像 */
.form-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  background: var(--gray-200);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: var(--gray-600);
  font-size: 0.875rem;
  flex-shrink: 0;
}

/* 表单主体 */
.form-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* 文本输入区 */
.form-textarea {
  position: relative;
}

.textarea-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  font-size: 0.875rem;
  line-height: 1.5;
  resize: vertical;
  font-family: inherit;
  background: white;
  color: var(--gray-900);
  transition: all 0.2s ease;
  min-height: 100px;
}

.textarea-input:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.textarea-input:disabled {
  background: var(--gray-100);
  color: var(--gray-500);
  cursor: not-allowed;
}

.char-count {
  position: absolute;
  bottom: 0.5rem;
  right: 0.5rem;
  font-size: 0.75rem;
  color: var(--gray-500);
  pointer-events: none;
  user-select: none;
}

.char-current {
  font-weight: 500;
}

.char-count--warning {
  color: var(--warning-600);
}

.char-count--error {
  color: var(--error-600);
}

.char-separator,
.char-max {
  color: var(--gray-400);
}

/* 表单工具栏 */
.form-toolbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.5rem;
  background: var(--gray-100);
  border-radius: 0.5rem;
}

.toolbar-section {
  display: flex;
  gap: 0.25rem;
}

.toolbar-btn {
  padding: 0.5rem;
  border: none;
  border-radius: 0.375rem;
  background: transparent;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.875rem;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.toolbar-btn:hover {
  background: var(--gray-200);
  color: var(--gray-800);
}

/* 预览区 */
.form-preview {
  padding: 1rem;
  background: var(--gray-100);
  border-radius: 0.5rem;
  border: 1px solid var(--gray-200);
}

.preview-header {
  font-weight: 600;
  color: var(--gray-700);
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
}

.preview-content {
  color: var(--gray-800);
  font-size: 0.875rem;
  line-height: 1.5;
}

.preview-content :deep(code) {
  background: var(--gray-200);
  padding: 0.125rem 0.25rem;
  border-radius: 0.25rem;
  font-family: 'Courier New', monospace;
  font-size: 0.813rem;
}

.preview-content :deep(strong) {
  font-weight: 600;
}

.preview-content :deep(em) {
  font-style: italic;
}

/* 图片预览 */
.image-preview {
  margin-top: 0.5rem;
}

.preview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 0.5rem;
}

.preview-item {
  position: relative;
  border-radius: 0.5rem;
  overflow: hidden;
  background: var(--gray-200);
}

.preview-image {
  width: 100%;
  height: 100px;
  object-fit: cover;
}

.preview-remove {
  position: absolute;
  top: 0.25rem;
  right: 0.25rem;
  background: rgba(0, 0, 0, 0.5);
  color: white;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 0.75rem;
  transition: all 0.2s ease;
}

.preview-remove:hover {
  background: rgba(0, 0, 0, 0.7);
}

/* 表单操作 */
.form-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.actions-left,
.actions-right {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.action-btn {
  padding: 0.5rem 1rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-700);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.875rem;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.action-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
}

.btn-cancel {
  padding: 0.5rem 1rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-700);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.875rem;
  font-weight: 500;
}

.btn-cancel:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
}

/* 表情选择器 */
.emoji-picker {
  position: absolute;
  bottom: 100%;
  left: 0;
  background: white;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  margin-bottom: 0.5rem;
}

.emoji-grid {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 0.25rem;
  padding: 0.5rem;
  max-height: 200px;
  overflow-y: auto;
}

.emoji-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 0.25rem;
  transition: all 0.2s ease;
}

.emoji-btn:hover {
  background: var(--gray-100);
}

/* 文件输入 */
.file-input {
  display: none;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .comment-form {
    padding: 0.75rem;
  }
  
  .form-container {
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .form-avatar {
    align-self: flex-start;
  }
  
  .form-toolbar {
    flex-wrap: wrap;
    gap: 0.5rem;
  }
  
  .form-actions {
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .actions-left,
  .actions-right {
    width: 100%;
    justify-content: center;
  }
  
  .preview-grid {
    grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
  }
  
  .emoji-grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

@media (max-width: 480px) {
  .comment-form {
    padding: 0.5rem;
  }
  
  .textarea-input {
    min-height: 80px;
    font-size: 0.813rem;
  }
  
  .toolbar-btn {
    width: 28px;
    height: 28px;
    font-size: 0.813rem;
  }
  
  .action-btn,
  .btn-cancel {
    padding: 0.375rem 0.75rem;
    font-size: 0.813rem;
  }
  
  .preview-grid {
    grid-template-columns: repeat(auto-fill, minmax(60px, 1fr));
  }
  
  .emoji-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}

/* 深色模式 */
:deep(.dark) .comment-form {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .form-avatar,
:deep(.dark) .reply-avatar {
  background: var(--gray-700);
  color: var(--gray-300);
}

:deep(.dark) .reply-preview {
  background: var(--gray-700);
  border-color: var(--gray-600);
}

:deep(.dark) .reply-user {
  color: var(--gray-200);
}

:deep(.dark) .reply-text {
  color: var(--gray-400);
}

:deep(.dark) .textarea-input {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-200);
}

:deep(.dark) .textarea-input:focus {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

:deep(.dark) .char-count {
  color: var(--gray-400);
}

:deep(.dark) .form-toolbar {
  background: var(--gray-700);
}

:deep(.dark) .toolbar-btn {
  color: var(--gray-400);
}

:deep(.dark) .toolbar-btn:hover {
  background: var(--gray-600);
  color: var(--gray-200);
}

:deep(.dark) .form-preview {
  background: var(--gray-700);
  border-color: var(--gray-600);
}

:deep(.dark) .preview-header {
  color: var(--gray-200);
}

:deep(.dark) .preview-content {
  color: var(--gray-300);
}

:deep(.dark) .preview-content :deep(code) {
  background: var(--gray-600);
}

:deep(.dark) .preview-item {
  background: var(--gray-700);
}

:deep(.dark) .action-btn,
:deep(.dark) .btn-cancel {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-300);
}

:deep(.dark) .action-btn:hover,
:deep(.dark) .btn-cancel:hover {
  background: var(--gray-700);
  border-color: var(--gray-500);
  color: var(--gray-200);
}

:deep(.dark) .emoji-picker {
  background: var(--gray-800);
  border-color: var(--gray-600);
}

:deep(.dark) .emoji-btn:hover {
  background: var(--gray-700);
}

/* 无障碍性 */
@media (prefers-reduced-motion: reduce) {
  .comment-form,
  .toolbar-btn,
  .action-btn,
  .btn-cancel,
  .emoji-btn {
    transition: none;
  }
}
</style>