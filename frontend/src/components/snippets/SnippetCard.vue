<template>
  <div class="snippet-card" :class="{ loading: isLoading }">
    <!-- 卡片头部 -->
    <div class="card-header">
      <div class="snippet-meta">
        <h3 class="snippet-title">
          <router-link :to="`/snippets/${snippet.id}`" class="title-link">
            {{ snippet.title }}
          </router-link>
        </h3>
        <div class="snippet-info">
          <span class="language-badge" :style="{ backgroundColor: getLanguageColor(snippet.language) }">
            {{ snippet.language }}
          </span>
          <span class="creator-info">
            <svg class="creator-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z"/>
            </svg>
            {{ snippet.creatorName }}
          </span>
          <span class="date-info">
            <svg class="date-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19,3H18V1H16V3H8V1H6V3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M19,19H5V8H19V19Z"/>
            </svg>
            {{ formatDate(snippet.createdAt) }}
          </span>
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="card-actions">
        <button
          @click="handleCopy"
          class="action-btn copy-btn"
          :disabled="isLoading"
          title="复制代码"
        >
          <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z"/>
          </svg>
        </button>

        <button
          v-if="canEdit"
          @click="handleEdit"
          class="action-btn edit-btn"
          :disabled="isLoading"
          title="编辑代码片段"
        >
          <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z"/>
          </svg>
        </button>

        <button
          v-if="canDelete"
          @click="handleDelete"
          class="action-btn delete-btn"
          :disabled="isLoading"
          title="删除代码片段"
        >
          <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- 描述 -->
    <div class="card-body">
      <p class="snippet-description" v-if="snippet.description">
        {{ snippet.description }}
      </p>

      <!-- 代码预览 -->
      <div class="code-preview">
        <pre class="code-block"><code :class="`language-${snippet.language}`">{{ codePreview }}</code></pre>
        <div v-if="isCodeTruncated" class="code-expand">
          <button @click="toggleCodeExpansion" class="expand-btn">
            {{ isCodeExpanded ? '收起' : '展开更多' }}
          </button>
        </div>
      </div>
    </div>

    <!-- 卡片底部 -->
    <div class="card-footer">
      <!-- 标签 -->
      <div class="snippet-tags" v-if="snippet.tags && snippet.tags.length > 0">
        <span
          v-for="tag in snippet.tags"
          :key="tag.id"
          class="tag-badge"
          :style="{ backgroundColor: tag.color }"
          @click="handleTagClick(tag)"
        >
          {{ tag.name }}
        </span>
      </div>

      <!-- 统计信息 -->
      <div class="snippet-stats">
        <span class="stat-item">
          <svg class="stat-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
          </svg>
          {{ snippet.viewCount }}
        </span>
        <span class="stat-item">
          <svg class="stat-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z"/>
          </svg>
          {{ snippet.copyCount }}
        </span>
        <span class="visibility-indicator" :class="{ public: snippet.isPublic }">
          <svg class="visibility-icon" viewBox="0 0 24 24" fill="currentColor">
            <path v-if="snippet.isPublic" d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
            <path v-else d="M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.09L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.76,7.13 11.38,7 12,7Z"/>
          </svg>
          {{ snippet.isPublic ? '公开' : '私有' }}
        </span>
      </div>
    </div>

    <!-- 复制成功提示 -->
    <div v-if="showCopySuccess" class="copy-success">
      <svg class="success-icon" viewBox="0 0 24 24" fill="currentColor">
        <path d="M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M17,9L16.25,8.25L11,13.5L7.75,10.25L7,11L11,15L17,9Z"/>
      </svg>
      复制成功！
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { CodeSnippet, Tag } from '@/types'
import { UserRole } from '@/types'

interface Props {
  snippet: CodeSnippet
  isLoading?: boolean
}

interface Emits {
  (e: 'copy', snippet: CodeSnippet): void
  (e: 'delete', snippet: CodeSnippet): void
  (e: 'tag-click', tag: Tag): void
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false
})

const emit = defineEmits<Emits>()

const router = useRouter()
const authStore = useAuthStore()

// 响应式状态
const isCodeExpanded = ref(false)
const showCopySuccess = ref(false)

// 计算属性
const canEdit = computed(() => {
  const user = authStore.user
  if (!user) return false

  return user.role === UserRole.Admin ||
         (user.role === UserRole.Editor && user.id === props.snippet.createdBy)
})

const canDelete = computed(() => {
  const user = authStore.user
  if (!user) return false

  return user.role === UserRole.Admin || user.id === props.snippet.createdBy
})

const codePreview = computed(() => {
  const maxLines = isCodeExpanded.value ? Infinity : 10
  const lines = props.snippet.code.split('\n')

  if (lines.length <= maxLines) {
    return props.snippet.code
  }

  return lines.slice(0, maxLines).join('\n')
})

const isCodeTruncated = computed(() => {
  return props.snippet.code.split('\n').length > 10
})

/**
 * 获取编程语言对应的颜色
 */
function getLanguageColor(language: string): string {
  const colors: Record<string, string> = {
    javascript: '#f7df1e',
    typescript: '#3178c6',
    python: '#3776ab',
    java: '#ed8b00',
    csharp: '#239120',
    cpp: '#00599c',
    html: '#e34f26',
    css: '#1572b6',
    vue: '#4fc08d',
    react: '#61dafb',
    angular: '#dd0031',
    php: '#777bb4',
    ruby: '#cc342d',
    go: '#00add8',
    rust: '#000000',
    swift: '#fa7343',
    kotlin: '#7f52ff',
    dart: '#0175c2',
    sql: '#336791',
    shell: '#89e051',
    powershell: '#012456',
    json: '#000000',
    xml: '#0060ac',
    yaml: '#cb171e',
    markdown: '#083fa1'
  }

  return colors[language.toLowerCase()] || '#6c757d'
}

/**
 * 格式化日期
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000)

  if (diffInSeconds < 60) {
    return '刚刚'
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60)
    return `${minutes}分钟前`
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600)
    return `${hours}小时前`
  } else if (diffInSeconds < 2592000) {
    const days = Math.floor(diffInSeconds / 86400)
    return `${days}天前`
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

/**
 * 切换代码展开状态
 */
function toggleCodeExpansion() {
  isCodeExpanded.value = !isCodeExpanded.value
}

/**
 * 处理复制操作
 */
async function handleCopy() {
  try {
    await navigator.clipboard.writeText(props.snippet.code)
    showCopySuccess.value = true

    // 触发复制事件
    emit('copy', props.snippet)

    // 3秒后隐藏成功提示
    setTimeout(() => {
      showCopySuccess.value = false
    }, 3000)
  } catch (error) {
    console.error('复制失败:', error)
    // 这里可以显示错误提示
  }
}

/**
 * 处理编辑操作
 */
function handleEdit() {
  router.push(`/snippets/${props.snippet.id}/edit`)
}

/**
 * 处理删除操作
 */
function handleDelete() {
  if (confirm('确定要删除这个代码片段吗？此操作不可撤销。')) {
    emit('delete', props.snippet)
  }
}

/**
 * 处理标签点击
 */
function handleTagClick(tag: Tag) {
  emit('tag-click', tag)
}
</script>

<style scoped>
.snippet-card {
  background: linear-gradient(135deg, #ffffff 0%, #fafbfc 100%);
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.snippet-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, #007bff 0%, #0056b3 50%, #007bff 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.snippet-card:hover {
  box-shadow: 0 8px 40px rgba(0, 123, 255, 0.15);
  transform: translateY(-4px);
  border-color: rgba(0, 123, 255, 0.1);
}

.snippet-card:hover::before {
  opacity: 1;
}

.snippet-card.loading {
  opacity: 0.7;
  pointer-events: none;
}

/* 卡片头部 */
.card-header {
  padding: 1.5rem 1.5rem 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.snippet-meta {
  flex: 1;
  min-width: 0;
}

.snippet-title {
  margin: 0 0 0.75rem 0;
  font-size: 1.125rem;
  font-weight: 600;
  line-height: 1.4;
}

.title-link {
  color: #212529;
  text-decoration: none;
  transition: color 0.3s ease;
}

.title-link:hover {
  color: #007bff;
}

.snippet-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
  font-size: 0.875rem;
  color: #6c757d;
}

.language-badge {
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
}

.creator-info,
.date-info {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.creator-icon,
.date-icon {
  width: 14px;
  height: 14px;
}

/* 操作按钮 */
.card-actions {
  display: flex;
  gap: 0.375rem;
  flex-shrink: 0;
}

.action-btn {
  width: 40px;
  height: 40px;
  border: none;
  border-radius: 12px;
  background: rgba(248, 249, 250, 0.8);
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  backdrop-filter: blur(10px);
}

.action-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.1) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.action-btn:hover::before {
  opacity: 1;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.copy-btn:hover {
  background: linear-gradient(135deg, #e7f3ff 0%, #cce7ff 100%);
  color: #007bff;
  box-shadow: 0 4px 12px rgba(0, 123, 255, 0.2);
}

.edit-btn:hover {
  background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
  color: #856404;
  box-shadow: 0 4px 12px rgba(255, 193, 7, 0.2);
}

.delete-btn:hover {
  background: linear-gradient(135deg, #f8d7da 0%, #f5c6cb 100%);
  color: #721c24;
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.2);
}

.action-icon {
  width: 18px;
  height: 18px;
}

/* 卡片主体 */
.card-body {
  padding: 0 1.5rem 1rem 1.5rem;
}

.snippet-description {
  margin: 0 0 1rem 0;
  color: #6c757d;
  line-height: 1.5;
  font-size: 0.875rem;
}

/* 代码预览 */
.code-preview {
  position: relative;
}

.code-block {
  background: linear-gradient(135deg, #1e1e1e 0%, #2d2d30 100%);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 1.25rem;
  margin: 0;
  font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 0.875rem;
  line-height: 1.6;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
  color: #d4d4d4;
  position: relative;
}

.code-block::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent 0%, rgba(0, 123, 255, 0.3) 50%, transparent 100%);
}

.code-expand {
  text-align: center;
  margin-top: 0.5rem;
}

.expand-btn {
  background: none;
  border: none;
  color: #007bff;
  cursor: pointer;
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  transition: background-color 0.3s ease;
}

.expand-btn:hover {
  background: #e7f3ff;
}

/* 卡片底部 */
.card-footer {
  padding: 1rem 1.5rem 1.5rem 1.5rem;
  border-top: 1px solid #f1f3f4;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

/* 标签 */
.snippet-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  flex: 1;
}

.tag-badge {
  color: white;
  padding: 0.375rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.tag-badge::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.1) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.tag-badge:hover {
  transform: translateY(-2px) scale(1.05);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25);
}

.tag-badge:hover::before {
  opacity: 1;
}

/* 统计信息 */
.snippet-stats {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.75rem;
  color: #6c757d;
  flex-shrink: 0;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.stat-icon {
  width: 14px;
  height: 14px;
}

.visibility-indicator {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.visibility-indicator.public {
  color: #28a745;
}

.visibility-icon {
  width: 14px;
  height: 14px;
}

/* 复制成功提示 */
.copy-success {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: #28a745;
  color: white;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  animation: slideInFadeOut 3s ease-in-out;
  z-index: 10;
}

.success-icon {
  width: 16px;
  height: 16px;
}

@keyframes slideInFadeOut {
  0% {
    opacity: 0;
    transform: translateX(100%);
  }
  10%, 90% {
    opacity: 1;
    transform: translateX(0);
  }
  100% {
    opacity: 0;
    transform: translateX(100%);
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .card-header {
    padding: 1rem 1rem 0.75rem 1rem;
  }

  .card-body {
    padding: 0 1rem 0.75rem 1rem;
  }

  .card-footer {
    padding: 0.75rem 1rem 1rem 1rem;
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .snippet-info {
    gap: 0.75rem;
  }

  .snippet-stats {
    gap: 0.75rem;
  }

  .card-actions {
    gap: 0.375rem;
  }

  .action-btn {
    width: 32px;
    height: 32px;
  }

  .action-icon {
    width: 16px;
    height: 16px;
  }
}

@media (max-width: 480px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .card-actions {
    align-self: flex-end;
  }

  .snippet-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .snippet-card {
    transition: none;
  }

  .snippet-card:hover {
    transform: none;
  }

  .copy-success {
    animation: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .snippet-card {
    border: 2px solid #000;
  }

  .code-block {
    border-width: 2px;
  }

  .card-footer {
    border-top-width: 2px;
  }
}
</style>
