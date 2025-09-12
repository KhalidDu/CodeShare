<template>
  <div class="snippet-detail-enhanced">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-container">
      <div class="loading-content">
        <div class="loading-spinner"></div>
        <p class="loading-text">{{ loadingText }}</p>
      </div>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="error" class="error-container">
      <div class="error-content">
        <div class="error-icon">
          <svg class="w-16 h-16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h2 class="error-title">{{ errorTitle }}</h2>
        <p class="error-message">{{ error }}</p>
        <div class="error-actions">
          <button @click="retry" class="btn btn-primary">
            重试
          </button>
          <router-link to="/snippets" class="btn btn-secondary">
            返回列表
          </router-link>
        </div>
      </div>
    </div>

    <!-- 主内容 -->
    <div v-else-if="snippet" class="main-container">
      <!-- 顶部导航栏 -->
      <div class="top-nav">
        <div class="nav-left">
          <router-link to="/snippets" class="back-button">
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            返回代码片段列表
          </router-link>
        </div>
        <div class="nav-right">
          <!-- 操作按钮 -->
          <div class="action-buttons">
            <!-- 分享按钮 -->
            <button
              v-if="canShare"
              @click="handleShare"
              class="btn btn-icon btn-share"
              title="分享代码片段"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m9.632 4.316C18.114 15.062 18 14.518 18 14c0-.482.114-.938.316-1.342m0 2.684a3 3 0 110-2.684M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </button>

            <!-- 编辑按钮 -->
            <button
              v-if="canEdit"
              @click="handleEdit"
              class="btn btn-icon btn-edit"
              title="编辑代码片段"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
              </svg>
            </button>

            <!-- 复制按钮 -->
            <button
              @click="handleCopy"
              class="btn btn-icon btn-copy"
              title="复制代码"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
            </button>

            <!-- 设置按钮 -->
            <button
              @click="toggleSettings"
              class="btn btn-icon btn-settings"
              title="设置"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- 主要内容区域 -->
      <div class="content-area">
        <!-- 左侧信息面板 -->
        <div class="info-panel">
          <!-- 标题区域 -->
          <div class="title-section">
            <h1 class="snippet-title">{{ snippet.title || '无标题' }}</h1>
            <div class="snippet-meta">
              <!-- 语言标签 -->
              <div class="language-tag">
                <div 
                  class="language-dot"
                  :style="{ backgroundColor: getLanguageColor(snippet.language) }"
                ></div>
                <span>{{ getLanguageDisplayName(snippet.language) }}</span>
              </div>
              
              <!-- 可见性状态 -->
              <div class="visibility-tag">
                <svg 
                  v-if="snippet.isPublic" 
                  class="w-4 h-4 text-green-600" 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 104 0 2 2 0 002-2h1.064M15 20.488V18a2 2 0 002-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <svg 
                  v-else 
                  class="w-4 h-4 text-gray-500" 
                  fill="none" 
                  stroke="currentColor" 
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2h-8a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
                <span>{{ snippet.isPublic ? '公开' : '私有' }}</span>
              </div>
            </div>
          </div>

          <!-- 描述信息 -->
          <div v-if="snippet.description" class="description-section">
            <h3 class="section-title">描述</h3>
            <p class="description-text">{{ snippet.description }}</p>
          </div>

          <!-- 标签 -->
          <div v-if="snippet.tags && snippet.tags.length > 0" class="tags-section">
            <h3 class="section-title">标签</h3>
            <div class="tags-container">
              <span
                v-for="tag in snippet.tags"
                :key="tag.id"
                class="tag"
                :style="{ backgroundColor: tag.color + '20', color: tag.color, border: `1px solid ${tag.color}40` }"
              >
                {{ tag.name }}
              </span>
            </div>
          </div>

          <!-- 统计信息 -->
          <div class="stats-section">
            <h3 class="section-title">统计信息</h3>
            <div class="stats-grid">
              <div class="stat-item">
                <div class="stat-icon">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                </div>
                <div class="stat-content">
                  <div class="stat-value">{{ snippet.viewCount || 0 }}</div>
                  <div class="stat-label">浏览次数</div>
                </div>
              </div>
              
              <div class="stat-item">
                <div class="stat-icon">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                </div>
                <div class="stat-content">
                  <div class="stat-value">{{ snippet.copyCount || 0 }}</div>
                  <div class="stat-label">复制次数</div>
                </div>
              </div>
              
              <div v-if="snippet.shareCount" class="stat-item">
                <div class="stat-icon">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m9.632 4.316C18.114 15.062 18 14.518 18 14c0-.482.114-.938.316-1.342m0 2.684a3 3 0 110-2.684M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </div>
                <div class="stat-content">
                  <div class="stat-value">{{ snippet.shareCount }}</div>
                  <div class="stat-label">分享次数</div>
                </div>
              </div>
            </div>
          </div>

          <!-- 创建信息 -->
          <div class="created-section">
            <h3 class="section-title">创建信息</h3>
            <div class="created-info">
              <div class="info-item">
                <span class="info-label">创建者:</span>
                <span class="info-value">{{ snippet.creatorName || '未知' }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">创建时间:</span>
                <span class="info-value">{{ formatDate(snippet.createdAt) }}</span>
              </div>
              <div class="info-item">
                <span class="info-label">更新时间:</span>
                <span class="info-value">{{ formatDate(snippet.updatedAt) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 右侧代码编辑器区域 -->
        <div class="editor-panel">
          <CodeViewerEnhanced
            ref="codeViewer"
            :code="snippet.code"
            :language="snippet.language"
            :theme="currentTheme"
            :read-only="!canEdit"
            :show-line-numbers="showLineNumbers"
            :show-minimap="showMinimap"
            :word-wrap="wordWrap"
            @content-change="handleCodeChange"
            @cursor-change="handleCursorChange"
            @selection-change="handleSelectionChange"
          />
        </div>
      </div>
    </div>

    <!-- 设置面板 -->
    <SettingsPanel
      v-if="showSettings"
      :visible="showSettings"
      :settings="editorSettings"
      @close="showSettings = false"
      @settings-change="handleSettingsChange"
    />

    <!-- 分享对话框 -->
    <ShareDialog
      v-if="showShareDialog"
      :visible="showShareDialog"
      :snippet-id="snippet?.id"
      :snippet-title="snippet?.title"
      @close="showShareDialog = false"
      @share-created="handleShareCreated"
    />

    <!-- 提示消息 -->
    <Toast
      v-if="toast.show"
      :type="toast.type"
      :message="toast.message"
      :duration="toast.duration"
      @close="hideToast"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSnippetStore } from '@/stores/snippets'
import { useToastStore } from '@/stores/toast'
import type { CodeSnippet } from '@/types'
import { UserRole } from '@/types'
import CodeViewerEnhanced from './CodeViewerEnhanced.vue'
import SettingsPanel from './SettingsPanel.vue'
import ShareDialog from '@/components/sharing/ShareDialog.vue'
import Toast from '@/components/common/Toast.vue'

// 路由和状态管理
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const snippetStore = useSnippetStore()
const toastStore = useToastStore()

// 响应式状态
const snippet = ref<CodeSnippet | null>(null)
const loading = ref(false)
const error = ref('')
const loadingText = ref('加载代码片段...')
const errorTitle = ref('加载失败')

// 编辑器引用
const codeViewer = ref()

// 设置面板状态
const showSettings = ref(false)
const showShareDialog = ref(false)

// 编辑器设置
const currentTheme = ref('vs-dark')
const showLineNumbers = ref(true)
const showMinimap = ref(true)
const wordWrap = ref('on')

// 编辑器设置对象
const editorSettings = computed(() => ({
  theme: currentTheme.value,
  showLineNumbers: showLineNumbers.value,
  showMinimap: showMinimap.value,
  wordWrap: wordWrap.value
}))

// 提示消息状态
const toast = ref({
  show: false,
  type: 'info',
  message: '',
  duration: 3000
})

// 计算属性
const snippetId = computed(() => route.params.id as string)

const canEdit = computed(() => {
  if (!snippet.value || !authStore.user) return false
  return authStore.user.role === UserRole.Admin || 
         (authStore.user.role === UserRole.Editor && authStore.user.id === snippet.value.createdBy)
})

const canShare = computed(() => {
  if (!snippet.value || !authStore.user) return false
  return authStore.user.role === UserRole.Admin || 
         authStore.user.id === snippet.value.createdBy
})

// 方法
/**
 * 加载代码片段数据
 */
async function loadSnippet() {
  if (!snippetId.value) {
    error.value = '无效的代码片段ID'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const result = await snippetStore.fetchSnippetById(snippetId.value)
    snippet.value = result
  } catch (err: unknown) {
    console.error('Failed to load snippet:', err)
    const axiosError = err as { response?: { data?: { message?: string } } }
    error.value = axiosError.response?.data?.message || '加载代码片段失败'
  } finally {
    loading.value = false
  }
}

/**
 * 重试加载
 */
function retry() {
  loadSnippet()
}

/**
 * 处理分享
 */
function handleShare() {
  showShareDialog.value = true
}

/**
 * 处理编辑
 */
function handleEdit() {
  router.push(`/snippets/${snippetId.value}/edit`)
}

/**
 * 处理复制
 */
async function handleCopy() {
  if (!snippet.value) return

  try {
    await navigator.clipboard.writeText(snippet.value.code)
    showToast('success', '代码已复制到剪贴板')
    
    // 更新复制次数
    if (snippet.value) {
      snippet.value.copyCount = (snippet.value.copyCount || 0) + 1
    }
  } catch (error) {
    console.error('复制失败:', error)
    showToast('error', '复制失败，请手动复制')
  }
}

/**
 * 切换设置面板
 */
function toggleSettings() {
  showSettings.value = !showSettings.value
}

/**
 * 处理代码变化
 */
function handleCodeChange(content: string) {
  // 这里可以处理代码变化逻辑
  console.log('Code changed:', content.length)
}

/**
 * 处理光标变化
 */
function handleCursorChange(position: any) {
  // 这里可以处理光标变化逻辑
  console.log('Cursor position:', position)
}

/**
 * 处理选择变化
 */
function handleSelectionChange(selection: any) {
  // 这里可以处理选择变化逻辑
  console.log('Selection:', selection)
}

/**
 * 处理设置变化
 */
function handleSettingsChange(settings: any) {
  currentTheme.value = settings.theme
  showLineNumbers.value = settings.showLineNumbers
  showMinimap.value = settings.showMinimap
  wordWrap.value = settings.wordWrap
}

/**
 * 处理分享创建
 */
function handleShareCreated() {
  showShareDialog.value = false
  showToast('success', '分享链接创建成功！')
  // 刷新数据以更新分享统计
  loadSnippet()
}

/**
 * 显示提示消息
 */
function showToast(type: string, message: string, duration = 3000) {
  toast.value = {
    show: true,
    type,
    message,
    duration
  }
}

/**
 * 隐藏提示消息
 */
function hideToast() {
  toast.value.show = false
}

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
    php: '#777bb4',
    ruby: '#cc342d',
    go: '#00add8',
    rust: '#000000',
    sql: '#336791',
    shell: '#89e051',
    json: '#000000',
    xml: '#0060ac',
    yaml: '#cb171e',
    markdown: '#083fa1'
  }

  return colors[language.toLowerCase()] || '#6c757d'
}

/**
 * 获取编程语言的显示名称
 */
function getLanguageDisplayName(language: string): string {
  const displayNames: Record<string, string> = {
    javascript: 'JavaScript',
    typescript: 'TypeScript',
    python: 'Python',
    java: 'Java',
    csharp: 'C#',
    cpp: 'C++',
    html: 'HTML',
    css: 'CSS',
    vue: 'Vue',
    react: 'React',
    php: 'PHP',
    ruby: 'Ruby',
    go: 'Go',
    rust: 'Rust',
    sql: 'SQL',
    shell: 'Shell',
    json: 'JSON',
    xml: 'XML',
    yaml: 'YAML',
    markdown: 'Markdown'
  }

  return displayNames[language.toLowerCase()] || language
}

/**
 * 格式化日期
 */
function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 生命周期
onMounted(() => {
  loadSnippet()
})

onUnmounted(() => {
  // 清理资源
})
</script>

<style scoped>
/* 主容器样式 */
.snippet-detail-enhanced {
  min-height: 100vh;
  background: linear-gradient(135deg, #1e1e1e 0%, #2d2d2d 100%);
  color: #d4d4d4;
}

/* 加载状态 */
.loading-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: rgba(0, 0, 0, 0.3);
}

.loading-content {
  text-align: center;
  padding: 2rem;
  background: rgba(30, 30, 30, 0.9);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top: 3px solid #007acc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-text {
  color: #d4d4d4;
  font-size: 1rem;
}

/* 错误状态 */
.error-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: rgba(0, 0, 0, 0.3);
}

.error-content {
  text-align: center;
  padding: 2rem;
  background: rgba(30, 30, 30, 0.9);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  max-width: 400px;
}

.error-icon {
  color: #dc3545;
  margin-bottom: 1rem;
}

.error-title {
  color: #dc3545;
  font-size: 1.25rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.error-message {
  color: #d4d4d4;
  margin-bottom: 1.5rem;
}

.error-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

/* 主容器 */
.main-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* 顶部导航 */
.top-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: rgba(30, 30, 30, 0.9);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.back-button {
  display: inline-flex;
  align-items: center;
  color: #007acc;
  text-decoration: none;
  font-size: 0.875rem;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.back-button:hover {
  background: rgba(0, 122, 204, 0.1);
  color: #007acc;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
}

.btn-icon {
  width: 36px;
  height: 36px;
  padding: 0;
  border-radius: 6px;
  background: transparent;
  color: #d4d4d4;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.btn-icon:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(255, 255, 255, 0.2);
}

.btn-primary {
  background: #007acc;
  color: white;
}

.btn-primary:hover {
  background: #005a9e;
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.1);
  color: #d4d4d4;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.btn-secondary:hover {
  background: rgba(255, 255, 255, 0.15);
}

/* 主要内容区域 */
.content-area {
  flex: 1;
  display: flex;
  height: calc(100vh - 73px);
}

/* 信息面板 */
.info-panel {
  width: 320px;
  background: rgba(30, 30, 30, 0.9);
  border-right: 1px solid rgba(255, 255, 255, 0.1);
  padding: 1.5rem;
  overflow-y: auto;
  backdrop-filter: blur(10px);
}

.title-section {
  margin-bottom: 2rem;
}

.snippet-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 1rem;
  line-height: 1.4;
}

.snippet-meta {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.language-tag,
.visibility-tag {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.language-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 0.75rem;
}

.description-text {
  color: #d4d4d4;
  line-height: 1.6;
  margin-bottom: 0;
}

.tags-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.tag {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
}

.stats-grid {
  display: grid;
  gap: 1rem;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.stat-icon {
  color: #007acc;
  flex-shrink: 0;
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 600;
  color: #ffffff;
}

.stat-label {
  font-size: 0.75rem;
  color: #8b92a0;
}

.created-info {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.info-label {
  font-size: 0.875rem;
  color: #8b92a0;
}

.info-value {
  font-size: 0.875rem;
  color: #d4d4d4;
  font-weight: 500;
}

/* 编辑器面板 */
.editor-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #1e1e1e;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .content-area {
    flex-direction: column;
  }
  
  .info-panel {
    width: 100%;
    border-right: none;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    max-height: 300px;
  }
  
  .top-nav {
    padding: 1rem;
  }
  
  .action-buttons {
    gap: 0.25rem;
  }
  
  .btn-icon {
    width: 32px;
    height: 32px;
  }
}

@media (max-width: 480px) {
  .snippet-title {
    font-size: 1.25rem;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .error-actions {
    flex-direction: column;
  }
}
</style>