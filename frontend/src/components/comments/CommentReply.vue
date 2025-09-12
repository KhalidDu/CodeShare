<template>
  <div class="comment-reply" :class="replyClasses">
    <!-- 回复头部 -->
    <div class="reply-header">
      <div class="reply-info">
        <div class="reply-title">
          <i class="fas fa-reply"></i>
          回复 {{ parentComment?.userName || '评论' }}
        </div>
        <div class="reply-meta">
          <span class="reply-time">{{ formatTime(parentComment?.createdAt) }}</span>
          <button
            type="button"
            class="reply-cancel"
            @click="$emit('cancel')"
          >
            <i class="fas fa-times"></i>
            取消回复
          </button>
        </div>
      </div>
    </div>

    <!-- 父评论预览 -->
    <div v-if="parentComment" class="parent-preview">
      <div class="parent-content">
        <div class="parent-header">
          <div class="parent-avatar">
            <div 
              v-if="parentComment.userAvatar"
              class="avatar-image"
              :style="{ backgroundImage: `url(${parentComment.userAvatar})` }"
            ></div>
            <div v-else class="avatar-placeholder">
              {{ parentInitials }}
            </div>
          </div>
          <div class="parent-info">
            <div class="parent-name">{{ parentComment.userName }}</div>
            <div class="parent-text">{{ truncatedParentContent }}</div>
          </div>
        </div>
        <div class="parent-actions">
          <button
            type="button"
            class="action-btn action-btn--like"
            @click="handleLikeParent"
            :disabled="likeLoading"
          >
            <i :class="parentLiked ? 'fas fa-heart' : 'far fa-heart'"></i>
            {{ parentComment.likeCount }}
          </button>
          <button
            type="button"
            class="action-btn action-btn--expand"
            @click="showFullParent = !showFullParent"
          >
            <i :class="showFullParent ? 'fas fa-chevron-up' : 'fas fa-chevron-down'"></i>
            {{ showFullParent ? '收起' : '展开' }}
          </button>
        </div>
      </div>
      
      <!-- 展开的完整内容 -->
      <div v-if="showFullParent" class="parent-full">
        <div class="full-content">{{ parentComment.content }}</div>
      </div>
    </div>

    <!-- 回复表单 -->
    <div class="reply-form">
      <form @submit.prevent="handleSubmit" class="form-container">
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
          <!-- 快速回复选项 -->
          <div v-if="showQuickReplies" class="quick-replies">
            <div class="quick-header">快速回复：</div>
            <div class="quick-options">
              <button
                v-for="reply in quickReplyOptions"
                :key="reply.id"
                type="button"
                class="quick-btn"
                @click="insertQuickReply(reply)"
              >
                {{ reply.text }}
              </button>
            </div>
          </div>

          <!-- 文本输入区 -->
          <div class="form-textarea">
            <textarea
              ref="textareaRef"
              v-model="content"
              :placeholder="placeholder"
              :maxlength="maxLength"
              :disabled="loading"
              class="textarea-input"
              rows="3"
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

          <!-- 格式化工具栏 -->
          <div v-if="enableFormatting" class="formatting-toolbar">
            <div class="toolbar-group">
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
            </div>
            
            <div class="toolbar-group">
              <button
                type="button"
                class="toolbar-btn"
                @click="toggleEmojiPicker"
                title="表情"
              >
                <i class="fas fa-smile"></i>
              </button>
            </div>
          </div>

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

          <!-- 回复选项 -->
          <div v-if="showOptions" class="reply-options">
            <label class="option-item">
              <input
                type="checkbox"
                v-model="notifyParent"
                class="option-checkbox"
              />
              <span class="option-label">通知原作者</span>
            </label>
            <label class="option-item">
              <input
                type="checkbox"
                v-model="subscribeToThread"
                class="option-checkbox"
              />
              <span class="option-label">订阅回复</span>
            </label>
          </div>

          <!-- 表单操作 -->
          <div class="form-actions">
            <div class="actions-left">
              <button
                type="button"
                class="action-btn action-btn--preview"
                @click="togglePreview"
                v-if="enablePreview"
              >
                <i :class="showPreview ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                {{ showPreview ? '隐藏预览' : '预览' }}
              </button>
            </div>
            
            <div class="actions-right">
              <button
                type="button"
                class="btn-cancel"
                @click="$emit('cancel')"
              >
                取消
              </button>
              <AnimatedButton
                type="submit"
                variant="primary"
                size="sm"
                :disabled="!canSubmit"
                :loading="loading"
              >
                <i class="fas fa-paper-plane"></i>
                发表回复
              </AnimatedButton>
            </div>
          </div>
        </div>
      </form>
    </div>

    <!-- 预览区 -->
    <div v-if="showPreview && content" class="reply-preview">
      <div class="preview-header">
        <i class="fas fa-eye"></i>
        预览
      </div>
      <div class="preview-content" v-html="previewContent"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import type { Comment } from '@/types/comment'
import AnimatedButton from '@/components/common/AnimatedButton.vue'

// 定义Props
interface Props {
  parentComment: Comment
  snippetId: string
  loading?: boolean
  maxLength?: number
  placeholder?: string
  enableFormatting?: boolean
  enablePreview?: boolean
  showQuickReplies?: boolean
  showOptions?: boolean
  autoFocus?: boolean
  initialContent?: string
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  maxLength: 500,
  placeholder: '写下你的回复...',
  enableFormatting: true,
  enablePreview: true,
  showQuickReplies: true,
  showOptions: true,
  autoFocus: true,
  initialContent: ''
})

// 定义Emits
interface Emits {
  (e: 'submit', data: { content: string; snippetId: string; parentId: string; notifyParent?: boolean; subscribeToThread?: boolean }): void
  (e: 'cancel'): void
  (e: 'like-parent', commentId: string): void
}

const emit = defineEmits<Emits>()

// Store
const authStore = useAuthStore()

// 响应式状态
const content = ref(props.initialContent)
const loading = ref(false)
const showPreview = ref(false)
const showEmojiPicker = ref(false)
const showFullParent = ref(false)
const likeLoading = ref(false)
const notifyParent = ref(true)
const subscribeToThread = ref(false)
const textareaRef = ref<HTMLTextAreaElement>()

// 快速回复选项
const quickReplyOptions = [
  { id: 1, text: '谢谢分享！' },
  { id: 2, text: '很有帮助，学习了！' },
  { id: 3, text: '同意你的观点' },
  { id: 4, text: '我也有同样的疑问' },
  { id: 5, text: '解释得很清楚' }
]

// 常用表情
const commonEmojis = [
  '👍', '👎', '👏', '🙌', '👋', '🤝', '💪', '🎉', '❤️', '🔥',
  '💯', '✨', '⭐', '🌟', '💡', '🎯', '🚀', '💎', '🏆', '🥇',
  '😊', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😍', '🥰',
  '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨',
  '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟',
  '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭',
  '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨',
  '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐',
  '😑', '😬', '🙄', '😯', '😦', '😧', '😮', '😲', '🥱', '😴'
]

// 计算属性
const replyClasses = computed(() => {
  return [
    'comment-reply',
    {
      'comment-reply--loading': loading.value,
      'comment-reply--focused': isFocused.value,
      'comment-reply--has-parent': !!props.parentComment
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

const parentInitials = computed(() => {
  if (!props.parentComment?.userName) return 'U'
  return props.parentComment.userName.charAt(0).toUpperCase()
})

const truncatedParentContent = computed(() => {
  if (!props.parentComment?.content) return ''
  const content = props.parentComment.content
  return content.length > 120 ? content.substring(0, 120) + '...' : content
})

const parentLiked = computed(() => {
  return props.parentComment?.isLikedByCurrentUser || false
})

const canSubmit = computed(() => {
  return content.value.trim().length > 0 && 
         content.value.length <= props.maxLength && 
         !loading.value
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
    parentId: props.parentComment.id,
    notifyParent: notifyParent.value,
    subscribeToThread: subscribeToThread.value
  })
}

function handleLikeParent() {
  if (likeLoading.value) return
  
  likeLoading.value = true
  emit('like-parent', props.parentComment.id)
  
  // 模拟加载状态
  setTimeout(() => {
    likeLoading.value = false
  }, 1000)
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

function insertQuickReply(reply: { id: number; text: string }) {
  content.value = reply.text
  nextTick(() => {
    textareaRef.value?.focus()
  })
}

function formatTime(dateString?: string): string {
  if (!dateString) return ''
  
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 7) return `${days}天前`
  
  return date.toLocaleDateString()
}

// 点击外部关闭表情选择器
function handleClickOutside(event: MouseEvent) {
  const target = event.target as Element
  const reply = document.querySelector('.comment-reply')
  if (reply && !reply.contains(target)) {
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
    showPreview.value = false
    showEmojiPicker.value = false
    showFullParent.value = false
    notifyParent.value = true
    subscribeToThread.value = false
  }
})
</script>

<style scoped>
.comment-reply {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1rem;
  background: var(--gray-50);
  border: 1px solid var(--gray-200);
  border-radius: 0.75rem;
  border-left: 4px solid var(--primary-500);
  transition: all 0.2s ease;
}

.comment-reply--focused {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.comment-reply--loading {
  opacity: 0.7;
  pointer-events: none;
}

/* 回复头部 */
.reply-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid var(--gray-200);
}

.reply-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.reply-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
  color: var(--gray-800);
  font-size: 0.875rem;
}

.reply-meta {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.reply-time {
  color: var(--gray-500);
  font-size: 0.75rem;
}

.reply-cancel {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
}

.reply-cancel:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
  color: var(--gray-700);
}

/* 父评论预览 */
.parent-preview {
  background: var(--gray-100);
  border-radius: 0.5rem;
  padding: 0.75rem;
  border: 1px solid var(--gray-200);
}

.parent-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 0.75rem;
}

.parent-header {
  display: flex;
  gap: 0.5rem;
  flex: 1;
}

.parent-avatar {
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

.parent-info {
  flex: 1;
  min-width: 0;
}

.parent-name {
  font-weight: 600;
  color: var(--gray-800);
  font-size: 0.813rem;
  margin-bottom: 0.25rem;
}

.parent-text {
  color: var(--gray-600);
  font-size: 0.75rem;
  line-height: 1.4;
}

.parent-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
}

.action-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
  color: var(--gray-700);
}

.action-btn--like:hover {
  border-color: var(--red-400);
  color: var(--red-600);
  background: var(--red-50);
}

.parent-full {
  margin-top: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--gray-200);
}

.full-content {
  color: var(--gray-700);
  font-size: 0.813rem;
  line-height: 1.5;
  white-space: pre-wrap;
}

/* 回复表单 */
.reply-form {
  background: white;
  border-radius: 0.5rem;
  padding: 1rem;
  border: 1px solid var(--gray-200);
}

.form-container {
  display: flex;
  gap: 0.75rem;
}

.form-avatar {
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

.form-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* 快速回复 */
.quick-replies {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem;
  background: var(--gray-50);
  border-radius: 0.375rem;
}

.quick-header {
  font-size: 0.75rem;
  color: var(--gray-600);
  font-weight: 500;
  white-space: nowrap;
}

.quick-options {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.quick-btn {
  padding: 0.25rem 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
}

.quick-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
  color: var(--gray-700);
}

/* 文本输入区 */
.form-textarea {
  position: relative;
}

.textarea-input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  font-size: 0.813rem;
  line-height: 1.4;
  resize: vertical;
  font-family: inherit;
  background: white;
  color: var(--gray-900);
  transition: all 0.2s ease;
  min-height: 80px;
}

.textarea-input:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.char-count {
  position: absolute;
  bottom: 0.25rem;
  right: 0.25rem;
  font-size: 0.75rem;
  color: var(--gray-500);
  pointer-events: none;
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

/* 格式化工具栏 */
.formatting-toolbar {
  display: flex;
  gap: 0.75rem;
  padding: 0.5rem;
  background: var(--gray-50);
  border-radius: 0.375rem;
}

.toolbar-group {
  display: flex;
  gap: 0.25rem;
}

.toolbar-btn {
  padding: 0.375rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.25rem;
  background: white;
  color: var(--gray-600);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.75rem;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.toolbar-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
  color: var(--gray-700);
}

/* 表情选择器 */
.emoji-picker {
  position: relative;
  background: white;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  margin-top: 0.5rem;
}

.emoji-grid {
  display: grid;
  grid-template-columns: repeat(10, 1fr);
  gap: 0.25rem;
  padding: 0.5rem;
  max-height: 150px;
  overflow-y: auto;
}

.emoji-btn {
  background: none;
  border: none;
  font-size: 1rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 0.25rem;
  transition: all 0.2s ease;
}

.emoji-btn:hover {
  background: var(--gray-100);
}

/* 回复选项 */
.reply-options {
  display: flex;
  gap: 1rem;
  padding: 0.5rem;
  background: var(--gray-50);
  border-radius: 0.375rem;
}

.option-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.option-checkbox {
  width: 16px;
  height: 16px;
  accent-color: var(--primary-600);
}

.option-label {
  font-size: 0.75rem;
  color: var(--gray-700);
  cursor: pointer;
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

.btn-cancel {
  padding: 0.375rem 0.75rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-700);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.813rem;
  font-weight: 500;
}

.btn-cancel:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
}

/* 预览区 */
.reply-preview {
  background: var(--gray-50);
  border-radius: 0.5rem;
  padding: 1rem;
  border: 1px solid var(--gray-200);
}

.preview-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
  color: var(--gray-700);
  font-size: 0.813rem;
  margin-bottom: 0.5rem;
}

.preview-content {
  color: var(--gray-800);
  font-size: 0.813rem;
  line-height: 1.5;
}

.preview-content :deep(code) {
  background: var(--gray-200);
  padding: 0.125rem 0.25rem;
  border-radius: 0.25rem;
  font-family: 'Courier New', monospace;
  font-size: 0.75rem;
}

.preview-content :deep(strong) {
  font-weight: 600;
}

.preview-content :deep(em) {
  font-style: italic;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .comment-reply {
    padding: 0.75rem;
  }
  
  .reply-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .parent-content {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .parent-actions {
    width: 100%;
    justify-content: flex-end;
  }
  
  .form-container {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .form-avatar {
    align-self: flex-start;
  }
  
  .quick-replies {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .quick-options {
    width: 100%;
    justify-content: center;
  }
  
  .reply-options {
    flex-direction: column;
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
  
  .emoji-grid {
    grid-template-columns: repeat(8, 1fr);
  }
}

@media (max-width: 480px) {
  .comment-reply {
    padding: 0.5rem;
  }
  
  .reply-form {
    padding: 0.75rem;
  }
  
  .textarea-input {
    min-height: 60px;
    font-size: 0.75rem;
  }
  
  .quick-options {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .emoji-grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

/* 深色模式 */
:deep(.dark) .comment-reply {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .reply-title {
  color: var(--gray-200);
}

:deep(.dark) .reply-time {
  color: var(--gray-400);
}

:deep(.dark) .reply-cancel {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-300);
}

:deep(.dark) .reply-cancel:hover {
  background: var(--gray-700);
  border-color: var(--gray-500);
  color: var(--gray-200);
}

:deep(.dark) .parent-preview {
  background: var(--gray-700);
  border-color: var(--gray-600);
}

:deep(.dark) .parent-name {
  color: var(--gray-200);
}

:deep(.dark) .parent-text {
  color: var(--gray-400);
}

:deep(.dark) .action-btn {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-400);
}

:deep(.dark) .action-btn:hover {
  background: var(--gray-700);
  border-color: var(--gray-500);
  color: var(--gray-300);
}

:deep(.dark) .reply-form {
  background: var(--gray-800);
  border-color: var(--gray-700);
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

:deep(.dark) .quick-replies,
:deep(.dark) .formatting-toolbar,
:deep(.dark) .reply-options {
  background: var(--gray-700);
}

:deep(.dark) .quick-btn,
:deep(.dark) .toolbar-btn {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-400);
}

:deep(.dark) .quick-btn:hover,
:deep(.dark) .toolbar-btn:hover {
  background: var(--gray-700);
  border-color: var(--gray-500);
  color: var(--gray-300);
}

:deep(.dark) .emoji-picker {
  background: var(--gray-800);
  border-color: var(--gray-600);
}

:deep(.dark) .emoji-btn:hover {
  background: var(--gray-700);
}

:deep(.dark) .btn-cancel {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-300);
}

:deep(.dark) .btn-cancel:hover {
  background: var(--gray-700);
  border-color: var(--gray-500);
  color: var(--gray-200);
}

:deep(.dark) .reply-preview {
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

:deep(.dark) .option-label {
  color: var(--gray-300);
}

:deep(.dark) .full-content {
  color: var(--gray-300);
}

/* 无障碍性 */
@media (prefers-reduced-motion: reduce) {
  .comment-reply,
  .action-btn,
  .quick-btn,
  .toolbar-btn,
  .btn-cancel,
  .emoji-btn {
    transition: none;
  }
}
</style>