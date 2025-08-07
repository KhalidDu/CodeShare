<template>
  <AppLayout>
    <div class="snippet-detail-view">
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
        <p>加载中...</p>
      </div>

      <div v-else-if="error" class="error-container">
        <div class="error-icon">⚠️</div>
        <h2>加载失败</h2>
        <p>{{ error }}</p>
        <div class="error-actions">
          <button @click="loadSnippet" class="btn btn-primary">重试</button>
          <router-link to="/snippets" class="btn btn-secondary">返回列表</router-link>
        </div>
      </div>

      <div v-else-if="snippet" class="snippet-content">
        <!-- 头部信息 -->
        <div class="snippet-header">
          <div class="header-main">
            <Breadcrumb :items="breadcrumbItems" />
            <div class="title-section">
              <h1 class="snippet-title">{{ snippet.title }}</h1>
              <div class="snippet-meta">
                <span class="meta-item">
                  <span class="meta-label">创建者:</span>
                  <span class="meta-value">{{ snippet.creatorName }}</span>
                </span>
                <span class="meta-item">
                  <span class="meta-label">创建时间:</span>
                  <span class="meta-value">{{ formatDate(snippet.createdAt) }}</span>
                </span>
                <span class="meta-item">
                  <span class="meta-label">更新时间:</span>
                  <span class="meta-value">{{ formatDate(snippet.updatedAt) }}</span>
                </span>
                <span class="meta-item">
                  <span class="meta-label">查看次数:</span>
                  <span class="meta-value">{{ snippet.viewCount }}</span>
                </span>
                <span class="meta-item">
                  <span class="meta-label">复制次数:</span>
                  <span class="meta-value">{{ snippet.copyCount }}</span>
                </span>
              </div>
            </div>
          </div>

          <div class="header-actions">
            <button
              v-if="canEdit"
              @click="editSnippet"
              class="btn btn-secondary"
            >
              编辑
            </button>
            <button
              @click="copySnippet"
              class="btn btn-primary"
              :disabled="copying"
            >
              {{ copying ? '复制中...' : '复制代码' }}
            </button>
          </div>
        </div>

        <!-- 描述 -->
        <div v-if="snippet.description" class="snippet-description">
          <h3>描述</h3>
          <p>{{ snippet.description }}</p>
        </div>

        <!-- 标签 -->
        <div v-if="snippet.tags && snippet.tags.length > 0" class="snippet-tags">
          <h3>标签</h3>
          <div class="tags-list">
            <span
              v-for="tag in snippet.tags"
              :key="tag.id"
              class="tag-chip"
              :style="{ backgroundColor: tag.color }"
            >
              {{ tag.name }}
            </span>
          </div>
        </div>

        <!-- 主要内容区域 -->
        <div class="main-content">
          <!-- 标签页导航 -->
          <div class="tab-navigation">
            <button 
              @click="activeTab = 'code'"
              :class="{ active: activeTab === 'code' }"
              class="tab-button"
            >
              代码
            </button>
            <button 
              @click="activeTab = 'history'"
              :class="{ active: activeTab === 'history' }"
              class="tab-button"
            >
              版本历史
            </button>
          </div>

          <!-- 代码查看器 -->
          <div v-show="activeTab === 'code'" class="tab-content">
            <CodeViewer
              :code="displayCode"
              :language="displayLanguage"
              :height="codeViewerHeight"
              @copy="onCodeCopy"
            />
          </div>

          <!-- 版本历史 -->
          <div v-show="activeTab === 'history'" class="tab-content">
            <VersionHistory
              :snippet-id="snippetId"
              :current-version-number="currentVersionNumber"
              @version-restored="onVersionRestored"
              @version-selected="onVersionSelected"
            />
          </div>
        </div>

        <!-- 统计信息 -->
        <div class="snippet-stats">
          <div class="stats-grid">
            <div class="stat-item">
              <div class="stat-value">{{ snippet.viewCount }}</div>
              <div class="stat-label">查看次数</div>
            </div>
            <div class="stat-item">
              <div class="stat-value">{{ snippet.copyCount }}</div>
              <div class="stat-label">复制次数</div>
            </div>
            <div class="stat-item">
              <div class="stat-value">{{ getLanguageLabel(snippet.language) }}</div>
              <div class="stat-label">编程语言</div>
            </div>
            <div class="stat-item">
              <div class="stat-value">{{ snippet.isPublic ? '公开' : '私有' }}</div>
              <div class="stat-label">可见性</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import CodeViewer from '@/components/editor/CodeViewer.vue'
import VersionHistory from '@/components/snippets/VersionHistory.vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import { useAuthStore } from '@/stores/auth'
import type { CodeSnippet, SupportedLanguage, SnippetVersion } from '@/types'

// 路由和认证
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// 响应式数据
const snippet = ref<CodeSnippet | null>(null)
const loading = ref(false)
const error = ref('')
const copying = ref(false)
const codeViewerHeight = ref('500px')
const activeTab = ref<'code' | 'history'>('code')
const selectedVersion = ref<SnippetVersion | null>(null)
const currentVersionNumber = ref<number>(1)

// 支持的编程语言
const supportedLanguages: SupportedLanguage[] = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'typescript', label: 'TypeScript' },
  { value: 'python', label: 'Python' },
  { value: 'java', label: 'Java' },
  { value: 'csharp', label: 'C#' },
  { value: 'cpp', label: 'C++' },
  { value: 'c', label: 'C' },
  { value: 'html', label: 'HTML' },
  { value: 'css', label: 'CSS' },
  { value: 'scss', label: 'SCSS' },
  { value: 'json', label: 'JSON' },
  { value: 'xml', label: 'XML' },
  { value: 'yaml', label: 'YAML' },
  { value: 'markdown', label: 'Markdown' },
  { value: 'sql', label: 'SQL' },
  { value: 'shell', label: 'Shell' },
  { value: 'powershell', label: 'PowerShell' },
  { value: 'php', label: 'PHP' },
  { value: 'ruby', label: 'Ruby' },
  { value: 'go', label: 'Go' },
  { value: 'rust', label: 'Rust' },
  { value: 'swift', label: 'Swift' },
  { value: 'kotlin', label: 'Kotlin' },
  { value: 'dart', label: 'Dart' }
]

// 计算属性
const snippetId = computed(() => route.params.id as string)

const canEdit = computed(() => {
  if (!snippet.value || !authStore.user) return false

  // 管理员可以编辑所有片段
  if (authStore.user.role === 2) return true

  // 编辑者可以编辑自己的片段
  if (authStore.user.role === 1 && snippet.value.createdBy === authStore.user.id) return true

  return false
})

const breadcrumbItems = computed(() => [
  { title: '首页', path: '/' },
  { title: '代码片段', path: '/snippets' },
  { title: snippet.value?.title || '详情', path: '' }
])

const displayCode = computed(() => {
  return selectedVersion.value?.code || snippet.value?.code || ''
})

const displayLanguage = computed(() => {
  return selectedVersion.value?.language || snippet.value?.language || 'text'
})

/**
 * 获取语言显示标签
 */
const getLanguageLabel = (language: string): string => {
  const lang = supportedLanguages.find(l => l.value === language)
  return lang?.label || language.toUpperCase()
}

/**
 * 格式化日期
 */
const formatDate = (dateString: string): string => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * 加载代码片段
 */
const loadSnippet = async () => {
  if (!snippetId.value) {
    error.value = '无效的代码片段ID'
    return
  }

  loading.value = true
  error.value = ''

  try {
    snippet.value = await codeSnippetService.getSnippet(snippetId.value)
  } catch (err: any) {
    console.error('Failed to load snippet:', err)
    error.value = err.response?.data?.message || '加载代码片段失败'
  } finally {
    loading.value = false
  }
}

/**
 * 编辑代码片段
 */
const editSnippet = () => {
  router.push(`/snippets/${snippetId.value}/edit`)
}

/**
 * 复制代码片段
 */
const copySnippet = async () => {
  if (!snippet.value || copying.value) return

  copying.value = true

  try {
    // 调用复制 API 记录统计
    await codeSnippetService.copySnippet(snippetId.value)

    // 复制到剪贴板
    await navigator.clipboard.writeText(snippet.value.code)

    // 更新本地复制次数
    snippet.value.copyCount += 1

    // 显示成功提示
    console.log('代码已复制到剪贴板')

  } catch (err) {
    console.error('Failed to copy snippet:', err)

    // 降级处理：尝试选择文本
    try {
      const textArea = document.createElement('textarea')
      textArea.value = snippet.value.code
      document.body.appendChild(textArea)
      textArea.select()
      document.execCommand('copy')
      document.body.removeChild(textArea)

      // 仍然记录统计
      await codeSnippetService.copySnippet(snippetId.value)
      snippet.value.copyCount += 1

    } catch (fallbackErr) {
      console.error('Fallback copy also failed:', fallbackErr)
    }
  } finally {
    copying.value = false
  }
}

/**
 * 代码复制处理（来自 CodeViewer 组件）
 */
const onCodeCopy = async () => {
  try {
    // 记录复制统计
    await codeSnippetService.copySnippet(snippetId.value)

    // 更新本地复制次数
    if (snippet.value) {
      snippet.value.copyCount += 1
    }

    console.log('代码已复制到剪贴板')
  } catch (err) {
    console.error('Failed to record copy:', err)
  }
}

/**
 * 版本恢复处理
 */
const onVersionRestored = async (version: SnippetVersion) => {
  // 重新加载代码片段以获取最新数据
  await loadSnippet()
  
  // 切换回代码标签页
  activeTab.value = 'code'
  selectedVersion.value = null
  
  console.log(`版本 ${version.versionNumber} 已恢复`)
}

/**
 * 版本选择处理
 */
const onVersionSelected = (version: SnippetVersion) => {
  selectedVersion.value = version
  console.log(`已选择版本 ${version.versionNumber}`)
}

// 生命周期
onMounted(() => {
  loadSnippet()
})
</script>

<style scoped>
.snippet-detail-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.loading-container,
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e1e5e9;
  border-top: 4px solid #0969da;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-container .error-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.error-container h2 {
  color: #d1242f;
  margin-bottom: 8px;
}

.error-actions {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.snippet-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.snippet-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid #e1e5e9;
}

.header-main {
  flex: 1;
}

.title-section {
  margin-top: 12px;
}

.snippet-title {
  font-size: 28px;
  font-weight: 600;
  color: #24292f;
  margin: 0 0 12px 0;
  line-height: 1.2;
}

.snippet-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 14px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-label {
  color: #656d76;
  font-weight: 500;
}

.meta-value {
  color: #24292f;
}

.header-actions {
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

.snippet-description,
.snippet-tags {
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  padding: 20px;
}

.main-content {
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  overflow: hidden;
}

.tab-navigation {
  display: flex;
  border-bottom: 1px solid #e1e5e9;
  background: #f6f8fa;
}

.tab-button {
  padding: 12px 20px;
  border: none;
  background: transparent;
  color: #656d76;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border-bottom: 2px solid transparent;
}

.tab-button:hover {
  color: #24292f;
  background: rgba(0, 0, 0, 0.03);
}

.tab-button.active {
  color: #0969da;
  background: #fff;
  border-bottom-color: #0969da;
}

.tab-content {
  padding: 20px;
}

.snippet-description h3,
.snippet-tags h3,
.snippet-code h3 {
  margin: 0 0 16px 0;
  font-size: 18px;
  font-weight: 600;
  color: #24292f;
}

.snippet-description p {
  margin: 0;
  line-height: 1.6;
  color: #24292f;
}

.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.tag-chip {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  background: #0969da;
  color: #fff;
  border-radius: 16px;
  font-size: 12px;
  font-weight: 500;
}

.snippet-stats {
  background: #f6f8fa;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  padding: 20px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 20px;
}

.stat-item {
  text-align: center;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: #0969da;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: #656d76;
  text-transform: uppercase;
  font-weight: 500;
}

.btn {
  padding: 8px 16px;
  border: 1px solid;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #0969da;
  color: #fff;
  border-color: #0969da;
}

.btn-primary:hover:not(:disabled) {
  background: #0860ca;
  border-color: #0860ca;
}

.btn-secondary {
  background: #f6f8fa;
  color: #24292f;
  border-color: #d0d7de;
}

.btn-secondary:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

@media (max-width: 768px) {
  .snippet-detail-view {
    padding: 16px;
  }

  .snippet-header {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    justify-content: flex-end;
  }

  .snippet-title {
    font-size: 24px;
  }

  .snippet-meta {
    flex-direction: column;
    gap: 8px;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
  }

  .snippet-description,
  .snippet-tags {
    padding: 16px;
  }

  .tab-content {
    padding: 16px;
  }

  .tab-button {
    padding: 10px 16px;
    font-size: 13px;
  }
}
</style>
