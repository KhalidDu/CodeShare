<template>
  <AppLayout>
    <div class="clipboard-history-view">
      <!-- 页面头部 -->
      <div class="page-header">
        <Breadcrumb :items="breadcrumbItems" />
        <div class="header-content">
          <div class="header-main">
            <h1 class="page-title">剪贴板历史</h1>
            <p class="page-description">查看和管理您最近复制的代码片段</p>
          </div>
          <div class="header-actions">
            <button
              @click="refreshHistory"
              :disabled="loading"
              class="btn btn-secondary"
            >
              <svg viewBox="0 0 16 16" width="16" height="16">
                <path d="M1.705 8.005a.75.75 0 0 1 .834.656 5.5 5.5 0 0 0 9.592 2.97l-1.204-1.204a.25.25 0 0 1 .177-.427h3.646a.25.25 0 0 1 .25.25v3.646a.25.25 0 0 1-.427.177l-1.38-1.38A7.002 7.002 0 0 1 1.05 8.84a.75.75 0 0 1 .656-.834ZM8 2.5a5.487 5.487 0 0 0-4.131 1.869l1.204 1.204A.25.25 0 0 1 4.896 6H1.25A.25.25 0 0 1 1 5.75V2.104a.25.25 0 0 1 .427-.177l1.38 1.38A7.002 7.002 0 0 1 14.95 7.16a.75.75 0 0 1-1.49.178A5.5 5.5 0 0 0 8 2.5Z"></path>
              </svg>
              刷新
            </button>
            <button
              @click="showClearConfirm = true"
              :disabled="loading || !historyItems.length"
              class="btn btn-danger"
            >
              <svg viewBox="0 0 16 16" width="16" height="16">
                <path d="M11 1.75V3h2.25a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1 0-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75ZM4.496 6.675l.66 6.6a.25.25 0 0 0 .249.225h5.19a.25.25 0 0 0 .249-.225l.66-6.6a.75.75 0 0 1 1.492.149l-.66 6.6A1.748 1.748 0 0 1 10.595 15h-5.19a1.748 1.748 0 0 1-1.741-1.575l-.66-6.6a.75.75 0 1 1 1.492-.15ZM6.5 1.75V3h3V1.75a.25.25 0 0 0-.25-.25h-2.5a.25.25 0 0 0-.25.25Z"></path>
              </svg>
              清空历史
            </button>
          </div>
        </div>
      </div>

      <!-- 筛选器 -->
      <div class="filters-section">
        <div class="filters-row">
          <div class="filter-group">
            <label class="filter-label">时间范围</label>
            <select v-model="dateFilter" @change="applyFilters" class="filter-select">
              <option value="all">全部时间</option>
              <option value="today">今天</option>
              <option value="week">最近一周</option>
              <option value="month">最近一个月</option>
            </select>
          </div>
          <div class="filter-group">
            <label class="filter-label">每页显示</label>
            <select v-model="pageSize" @change="applyFilters" class="filter-select">
              <option :value="10">10 条</option>
              <option :value="20">20 条</option>
              <option :value="50">50 条</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
        <p>加载中...</p>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="error-container">
        <div class="error-icon">⚠️</div>
        <h3>加载失败</h3>
        <p>{{ error }}</p>
        <button @click="loadHistory" class="btn btn-primary">重试</button>
      </div>

      <!-- 空状态 -->
      <div v-else-if="!historyItems.length" class="empty-container">
        <div class="empty-icon">📋</div>
        <h3>暂无复制历史</h3>
        <p>您还没有复制过任何代码片段</p>
        <router-link to="/snippets" class="btn btn-primary">浏览代码片段</router-link>
      </div>

      <!-- 历史记录列表 -->
      <div v-else class="history-content">
        <div class="history-list">
          <div
            v-for="item in historyItems"
            :key="item.id"
            class="history-item"
          >
            <div class="item-header">
              <div class="item-title">
                <router-link
                  :to="`/snippets/${item.snippetId}`"
                  class="snippet-link"
                >
                  {{ item.snippetTitle }}
                </router-link>
                <span class="language-badge">{{ getLanguageLabel(item.snippetLanguage) }}</span>
              </div>
              <div class="item-actions">
                <CopyButton
                  :text="item.snippetCode"
                  :snippet-id="item.snippetId"
                  :show-text="false"
                  button-class="copy-button--minimal"
                  tooltip="重新复制"
                  @copy-success="onReCopySuccess(item)"
                />
                <button
                  @click="deleteHistoryItem(item.id)"
                  class="btn-icon btn-danger"
                  title="删除记录"
                >
                  <svg viewBox="0 0 16 16" width="14" height="14">
                    <path d="M11 1.75V3h2.25a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1 0-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75ZM4.496 6.675l.66 6.6a.25.25 0 0 0 .249.225h5.19a.25.25 0 0 0 .249-.225l.66-6.6a.75.75 0 0 1 1.492.149l-.66 6.6A1.748 1.748 0 0 1 10.595 15h-5.19a1.748 1.748 0 0 1-1.741-1.575l-.66-6.6a.75.75 0 1 1 1.492-.15ZM6.5 1.75V3h3V1.75a.25.25 0 0 0-.25-.25h-2.5a.25.25 0 0 0-.25.25Z"></path>
                  </svg>
                </button>
              </div>
            </div>

            <div class="item-meta">
              <span class="meta-item">
                <svg viewBox="0 0 16 16" width="14" height="14">
                  <path d="M8 0a8 8 0 1 1 0 16A8 8 0 0 1 8 0ZM1.5 8a6.5 6.5 0 1 0 13 0 6.5 6.5 0 0 0-13 0Zm7-3.25v2.992l2.028.812a.75.75 0 0 1-.557 1.392l-2.5-1A.751.751 0 0 1 7 8.25v-3.5a.75.75 0 0 1 1.5 0Z"></path>
                </svg>
                {{ formatDate(item.copiedAt) }}
              </span>
            </div>

            <div class="item-code">
              <pre><code>{{ truncateCode(item.snippetCode) }}</code></pre>
              <div v-if="item.snippetCode.length > 200" class="code-expand">
                <button
                  @click="toggleCodeExpansion(item.id)"
                  class="btn-link"
                >
                  {{ expandedItems.has(item.id) ? '收起' : '展开全部' }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- 分页 -->
        <div v-if="totalPages > 1" class="pagination-container">
          <div class="pagination">
            <button
              @click="goToPage(currentPage - 1)"
              :disabled="currentPage <= 1"
              class="pagination-btn"
            >
              上一页
            </button>
            
            <div class="pagination-info">
              第 {{ currentPage }} 页，共 {{ totalPages }} 页
            </div>
            
            <button
              @click="goToPage(currentPage + 1)"
              :disabled="currentPage >= totalPages"
              class="pagination-btn"
            >
              下一页
            </button>
          </div>
          
          <div class="pagination-summary">
            共 {{ totalCount }} 条记录
          </div>
        </div>
      </div>

      <!-- 清空确认对话框 -->
      <div v-if="showClearConfirm" class="modal-overlay" @click="showClearConfirm = false">
        <div class="modal-dialog" @click.stop>
          <div class="modal-header">
            <h3>确认清空</h3>
            <button @click="showClearConfirm = false" class="modal-close">
              <svg viewBox="0 0 16 16" width="16" height="16">
                <path d="M3.72 3.72a.75.75 0 0 1 1.06 0L8 6.94l3.22-3.22a.75.75 0 1 1 1.06 1.06L9.06 8l3.22 3.22a.75.75 0 1 1-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 0 1-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 0 1 0-1.06Z"></path>
              </svg>
            </button>
          </div>
          <div class="modal-body">
            <p>确定要清空所有剪贴板历史记录吗？此操作不可撤销。</p>
          </div>
          <div class="modal-footer">
            <button @click="showClearConfirm = false" class="btn btn-secondary">
              取消
            </button>
            <button @click="clearAllHistory" class="btn btn-danger">
              确认清空
            </button>
          </div>
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import Breadcrumb from '@/components/common/Breadcrumb.vue'
import CopyButton from '@/components/common/CopyButton.vue'
import { clipboardService } from '@/services/clipboardService'
import { useToast } from '@/composables/useToast'
import type { ClipboardHistoryItem, SupportedLanguage } from '@/types'

// 响应式数据
const historyItems = ref<ClipboardHistoryItem[]>([])
const loading = ref(false)
const error = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)
const totalPages = ref(0)
const dateFilter = ref('all')
const showClearConfirm = ref(false)
const expandedItems = ref(new Set<string>())

// Toast 消息
const toast = useToast()

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
const breadcrumbItems = computed(() => [
  { title: '首页', path: '/' },
  { title: '剪贴板历史', path: '' }
])

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
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMinutes = Math.floor(diffMs / (1000 * 60))
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))

  if (diffMinutes < 1) {
    return '刚刚'
  } else if (diffMinutes < 60) {
    return `${diffMinutes} 分钟前`
  } else if (diffHours < 24) {
    return `${diffHours} 小时前`
  } else if (diffDays < 7) {
    return `${diffDays} 天前`
  } else {
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}

/**
 * 截断代码显示
 */
const truncateCode = (code: string): string => {
  const item = historyItems.value.find(item => item.snippetCode === code)
  if (!item || expandedItems.value.has(item.id)) {
    return code
  }
  return code.length > 200 ? code.substring(0, 200) + '...' : code
}

/**
 * 切换代码展开状态
 */
const toggleCodeExpansion = (itemId: string) => {
  if (expandedItems.value.has(itemId)) {
    expandedItems.value.delete(itemId)
  } else {
    expandedItems.value.add(itemId)
  }
}

/**
 * 获取日期筛选范围
 */
const getDateRange = () => {
  const now = new Date()
  let startDate: string | undefined
  
  switch (dateFilter.value) {
    case 'today':
      startDate = new Date(now.getFullYear(), now.getMonth(), now.getDate()).toISOString()
      break
    case 'week':
      const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
      startDate = weekAgo.toISOString()
      break
    case 'month':
      const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
      startDate = monthAgo.toISOString()
      break
    default:
      startDate = undefined
  }
  
  return { startDate }
}

/**
 * 加载剪贴板历史
 */
const loadHistory = async () => {
  loading.value = true
  error.value = ''

  try {
    const { startDate } = getDateRange()
    const result = await clipboardService.getClipboardHistory({
      page: currentPage.value,
      pageSize: pageSize.value,
      startDate
    })

    historyItems.value = result.items
    totalCount.value = result.totalCount
    totalPages.value = result.totalPages
  } catch (err: any) {
    console.error('Failed to load clipboard history:', err)
    error.value = err.response?.data?.message || '加载剪贴板历史失败'
  } finally {
    loading.value = false
  }
}

/**
 * 刷新历史记录
 */
const refreshHistory = () => {
  currentPage.value = 1
  loadHistory()
}

/**
 * 应用筛选器
 */
const applyFilters = () => {
  currentPage.value = 1
  loadHistory()
}

/**
 * 跳转到指定页面
 */
const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    loadHistory()
  }
}

/**
 * 删除单个历史记录
 */
const deleteHistoryItem = async (itemId: string) => {
  try {
    await clipboardService.deleteClipboardHistoryItem(itemId)
    
    // 从本地列表中移除
    const index = historyItems.value.findIndex(item => item.id === itemId)
    if (index > -1) {
      historyItems.value.splice(index, 1)
      totalCount.value -= 1
    }
    
    toast.success('历史记录已删除')
    
    // 如果当前页没有数据了，跳转到上一页
    if (historyItems.value.length === 0 && currentPage.value > 1) {
      currentPage.value -= 1
      loadHistory()
    }
  } catch (err: any) {
    console.error('Failed to delete history item:', err)
    toast.error('删除失败，请重试')
  }
}

/**
 * 清空所有历史记录
 */
const clearAllHistory = async () => {
  try {
    await clipboardService.clearClipboardHistory()
    
    historyItems.value = []
    totalCount.value = 0
    totalPages.value = 0
    currentPage.value = 1
    showClearConfirm.value = false
    
    toast.success('剪贴板历史已清空')
  } catch (err: any) {
    console.error('Failed to clear history:', err)
    toast.error('清空失败，请重试')
  }
}

/**
 * 重新复制成功处理
 */
const onReCopySuccess = (item: ClipboardHistoryItem) => {
  toast.success(`${item.snippetTitle} 已重新复制到剪贴板`)
}

// 生命周期
onMounted(() => {
  loadHistory()
})
</script>

<style scoped>
.clipboard-history-view {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  margin-bottom: 24px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  margin-top: 16px;
}

.header-main {
  flex: 1;
}

.page-title {
  font-size: 32px;
  font-weight: 600;
  color: #24292f;
  margin: 0 0 8px 0;
  line-height: 1.2;
}

.page-description {
  font-size: 16px;
  color: #656d76;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

.filters-section {
  background: #f6f8fa;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 24px;
}

.filters-row {
  display: flex;
  gap: 20px;
  align-items: end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.filter-label {
  font-size: 12px;
  font-weight: 500;
  color: #656d76;
  text-transform: uppercase;
}

.filter-select {
  padding: 6px 12px;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  background: #fff;
  font-size: 14px;
  color: #24292f;
}

.loading-container,
.error-container,
.empty-container {
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

.error-container .error-icon,
.empty-container .empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.error-container h3,
.empty-container h3 {
  color: #24292f;
  margin-bottom: 8px;
}

.error-container p,
.empty-container p {
  color: #656d76;
  margin-bottom: 20px;
}

.history-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.history-item {
  background: #fff;
  border: 1px solid #e1e5e9;
  border-radius: 8px;
  padding: 20px;
  transition: all 0.2s ease;
}

.history-item:hover {
  border-color: #d0d7de;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 12px;
}

.item-title {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.snippet-link {
  font-size: 16px;
  font-weight: 600;
  color: #0969da;
  text-decoration: none;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.snippet-link:hover {
  text-decoration: underline;
}

.language-badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  background: #0969da;
  color: #fff;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 500;
  text-transform: uppercase;
  flex-shrink: 0;
}

.item-actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.item-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
  font-size: 14px;
  color: #656d76;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.meta-item svg {
  fill: currentColor;
}

.item-code {
  position: relative;
  background: #f6f8fa;
  border: 1px solid #e1e5e9;
  border-radius: 6px;
  padding: 16px;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}

.item-code pre {
  margin: 0;
  white-space: pre-wrap;
  word-wrap: break-word;
}

.item-code code {
  font-size: 13px;
  line-height: 1.5;
  color: #24292f;
}

.code-expand {
  margin-top: 12px;
  text-align: right;
}

.pagination-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 0;
  border-top: 1px solid #e1e5e9;
}

.pagination {
  display: flex;
  align-items: center;
  gap: 16px;
}

.pagination-btn {
  padding: 8px 16px;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  background: #f6f8fa;
  color: #24292f;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: #f3f4f6;
  border-color: #8c959f;
}

.pagination-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.pagination-info {
  font-size: 14px;
  color: #656d76;
}

.pagination-summary {
  font-size: 14px;
  color: #656d76;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-dialog {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e1e5e9;
}

.modal-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #24292f;
}

.modal-close {
  padding: 4px;
  border: none;
  background: transparent;
  color: #656d76;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.modal-close:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #24292f;
}

.modal-close svg {
  fill: currentColor;
}

.modal-body {
  padding: 20px;
}

.modal-body p {
  margin: 0;
  color: #656d76;
  line-height: 1.6;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #e1e5e9;
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
  gap: 6px;
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

.btn-danger {
  background: #da3633;
  color: #fff;
  border-color: #da3633;
}

.btn-danger:hover:not(:disabled) {
  background: #b62324;
  border-color: #b62324;
}

.btn-icon {
  padding: 6px;
  border: none;
  background: transparent;
  color: #656d76;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.btn-icon:hover {
  background: rgba(0, 0, 0, 0.05);
  color: #24292f;
}

.btn-icon.btn-danger:hover {
  background: rgba(218, 54, 51, 0.1);
  color: #da3633;
}

.btn-icon svg {
  fill: currentColor;
}

.btn-link {
  border: none;
  background: transparent;
  color: #0969da;
  font-size: 12px;
  cursor: pointer;
  text-decoration: underline;
}

.btn-link:hover {
  color: #0860ca;
}

.btn svg {
  fill: currentColor;
}

@media (max-width: 768px) {
  .clipboard-history-view {
    padding: 16px;
  }

  .header-content {
    flex-direction: column;
    align-items: stretch;
  }

  .header-actions {
    justify-content: flex-end;
  }

  .page-title {
    font-size: 28px;
  }

  .filters-row {
    flex-direction: column;
    gap: 16px;
  }

  .history-item {
    padding: 16px;
  }

  .item-header {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .item-title {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .item-actions {
    justify-content: flex-end;
  }

  .pagination-container {
    flex-direction: column;
    gap: 12px;
    text-align: center;
  }

  .modal-dialog {
    margin: 20px;
  }
}
</style>