<template>
  <AppLayout
    page-title="剪贴板历史"
    page-subtitle="查看最近复制的代码片段"
    page-icon="M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3"
  >
    <!-- 筛选和搜索 -->
    <div class="clipboard-filters">
      <div class="filter-group">
        <input
          v-model="searchQuery"
          type="text"
          class="search-input"
          placeholder="搜索剪贴板内容..."
        />
      </div>
      <div class="filter-group">
        <select v-model="timeFilter" class="time-filter">
          <option value="all">全部时间</option>
          <option value="today">今天</option>
          <option value="week">本周</option>
          <option value="month">本月</option>
        </select>
      </div>
      <div class="filter-actions">
        <button @click="clearHistory" class="clear-btn">
          <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"/>
          </svg>
          清空历史
        </button>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="isLoading" class="loading-state">
      <SkeletonLoader
        v-for="i in 5"
        :key="i"
        type="list-item"
        :animated="true"
        class="history-skeleton"
      />
    </div>

    <!-- 空状态 -->
    <div v-else-if="filteredHistory.length === 0" class="empty-state">
      <div class="empty-icon">
        <svg viewBox="0 0 24 24" fill="currentColor">
          <path d="M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3"/>
        </svg>
      </div>
      <h3 class="empty-title">
        {{ searchQuery ? '没有找到匹配的记录' : '剪贴板历史为空' }}
      </h3>
      <p class="empty-description">
        {{ searchQuery ? '尝试调整搜索条件' : '复制代码片段后，历史记录将显示在这里' }}
      </p>
    </div>

    <!-- 历史记录列表 -->
    <div v-else class="history-list">
      <div
        v-for="item in filteredHistory"
        :key="item.id"
        class="history-item"
        :class="{ selected: selectedItems.includes(item.id) }"
      >
        <!-- 时间标签 -->
        <div class="time-label">{{ formatTime(item.copiedAt) }}</div>

        <!-- 内容预览 -->
        <div class="content-preview" @click="selectItem(item.id)">
          <div class="snippet-info">
            <h4 class="snippet-title">{{ item.snippetTitle }}</h4>
            <div class="snippet-meta">
              <span class="language-tag" :style="{ backgroundColor: getLanguageColor(item.language) }">
                {{ item.language }}
              </span>
              <span class="copy-time">{{ formatRelativeTime(item.copiedAt) }}</span>
            </div>
          </div>

          <div class="code-preview">
            <pre class="code-block"><code>{{ getCodePreview(item.content) }}</code></pre>
          </div>
        </div>

        <!-- 操作按钮 */
        <div class="item-actions">
          <button
            @click="copyToClipboard(item)"
            class="action-btn copy-btn"
            title="重新复制"
          >
            <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z"/>
            </svg>
          </button>

          <button
            @click="viewSnippet(item.snippetId)"
            class="action-btn view-btn"
            title="查看原片段"
          >
            <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
            </svg>
          </button>

          <button
            @click="removeItem(item.id)"
            class="action-btn delete-btn"
            title="删除记录"
          >
            <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"/>
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- 批量操作 -->
    <div v-if="selectedItems.length > 0" class="batch-actions">
      <div class="selection-info">
        已选择 {{ selectedItems.length }} 项
      </div>
      <div class="batch-buttons">
        <button @click="clearSelection" class="batch-btn secondary">
          取消选择
        </button>
        <button @click="deleteSelected" class="batch-btn danger">
          删除选中
        </button>
      </div>
    </div>

    <!-- 分页 -->
    <Pagination
      v-if="totalPages > 1"
      :current-page="currentPage"
      :total-pages="totalPages"
      :total-items="totalCount"
      :page-size="pageSize"
      @page-change="handlePageChange"
      @size-change="handlePageSizeChange"
    />
  </AppLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserFeedback } from '@/composables/useUserFeedback'
import AppLayout from '@/components/layout/AppLayout.vue'
import SkeletonLoader from '@/components/common/SkeletonLoader.vue'
import Pagination from '@/components/common/Pagination.vue'

interface ClipboardHistoryItem {
  id: string
  snippetId: string
  snippetTitle: string
  content: string
  language: string
  copiedAt: string
  userId: string
}

const router = useRouter()
const { showActionSuccess, showActionError, showConfirm } = useUserFeedback()

// 响应式状态
const isLoading = ref(false)
const searchQuery = ref('')
const timeFilter = ref('all')
const selectedItems = ref<string[]>([])
const currentPage = ref(1)
const pageSize = ref(20)
const totalCount = ref(0)

// 模拟数据
const historyItems = ref<ClipboardHistoryItem[]>([
  {
    id: '1',
    snippetId: 'snippet-1',
    snippetTitle: 'git stash # Stash changes',
    content: 'git stash',
    language: 'shell',
    copiedAt: new Date().toISOString(),
    userId: 'user-1'
  },
  {
    id: '2',
    snippetId: 'snippet-2',
    snippetTitle: 'git branch -d feature',
    content: 'git branch -d feature',
    language: 'shell',
    copiedAt: new Date(Date.now() - 60000).toISOString(),
    userId: 'user-1'
  },
  {
    id: '3',
    snippetId: 'snippet-3',
    snippetTitle: 'git remote add origin <url>',
    content: 'git remote add origin <url>',
    language: 'shell',
    copiedAt: new Date(Date.now() - 120000).toISOString(),
    userId: 'user-1'
  },
  {
    id: '4',
    snippetId: 'snippet-4',
    snippetTitle: 'func main() { // Routes http.HandleFunc("/hello", LoggingMiddleware(HelloHandler)) // Start s...',
    content: `func main() {
  // Routes
  http.HandleFunc("/hello", LoggingMiddleware(HelloHandler))
  // Start server
  log.Println("Server starting on :8080")
  log.Fatal(http.ListenAndServe(":8080", nil))
}`,
    language: 'go',
    copiedAt: new Date(Date.now() - 180000).toISOString(),
    userId: 'user-1'
  },
  {
    id: '5',
    snippetId: 'snippet-5',
    snippetTitle: '// HelloHandler handles the /hello endpoint func HelloHandler(w http.ResponseWriter, r *http.R...',
    content: `// HelloHandler handles the /hello endpoint
func HelloHandler(w http.ResponseWriter, r *http.Request) {
  fmt.Fprintf(w, "Hello, World!")
}`,
    language: 'go',
    copiedAt: new Date(Date.now() - 240000).toISOString(),
    userId: 'user-1'
  }
])

// 计算属性
const filteredHistory = computed(() => {
  let filtered = historyItems.value

  // 搜索过滤
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(item =>
      item.snippetTitle.toLowerCase().includes(query) ||
      item.content.toLowerCase().includes(query) ||
      item.language.toLowerCase().includes(query)
    )
  }

  // 时间过滤
  if (timeFilter.value !== 'all') {
    const now = new Date()
    const filterDate = new Date()

    switch (timeFilter.value) {
      case 'today':
        filterDate.setHours(0, 0, 0, 0)
        break
      case 'week':
        filterDate.setDate(now.getDate() - 7)
        break
      case 'month':
        filterDate.setMonth(now.getMonth() - 1)
        break
    }

    filtered = filtered.filter(item => new Date(item.copiedAt) >= filterDate)
  }

  return filtered.sort((a, b) => new Date(b.copiedAt).getTime() - new Date(a.copiedAt).getTime())
})

const totalPages = computed(() => Math.ceil(filteredHistory.value.length / pageSize.value))

/**
 * 获取编程语言颜色
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
    go: '#00add8',
    shell: '#89e051',
    sql: '#336791'
  }
  return colors[language.toLowerCase()] || '#6c757d'
}

/**
 * 格式化时间
 */
function formatTime(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * 格式化相对时间
 */
function formatRelativeTime(dateString: string): string {
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
  } else {
    const days = Math.floor(diffInSeconds / 86400)
    return `${days}天前`
  }
}

/**
 * 获取代码预览
 */
function getCodePreview(content: string): string {
  const lines = content.split('\n')
  if (lines.length <= 3) {
    return content
  }
  return lines.slice(0, 3).join('\n') + '\n...'
}

/**
 * 选择项目
 */
function selectItem(id: string) {
  const index = selectedItems.value.indexOf(id)
  if (index > -1) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(id)
  }
}

/**
 * 复制到剪贴板
 */
async function copyToClipboard(item: ClipboardHistoryItem) {
  try {
    await navigator.clipboard.writeText(item.content)
    showActionSuccess('复制', '内容已复制到剪贴板')
  } catch (error) {
    console.error('复制失败:', error)
    showActionError('复制', '复制失败，请重试')
  }
}

/**
 * 查看代码片段
 */
function viewSnippet(snippetId: string) {
  router.push(`/snippets/${snippetId}`)
}

/**
 * 删除单个项目
 */
function removeItem(id: string) {
  showConfirm(
    '确定要删除这条剪贴板记录吗？',
    () => {
      const index = historyItems.value.findIndex(item => item.id === id)
      if (index > -1) {
        historyItems.value.splice(index, 1)
        showActionSuccess('删除', '记录已删除')
      }
    }
  )
}

/**
 * 清空历史
 */
function clearHistory() {
  showConfirm(
    '确定要清空所有剪贴板历史记录吗？此操作不可撤销。',
    () => {
      historyItems.value = []
      selectedItems.value = []
      showActionSuccess('清空', '剪贴板历史已清空')
    },
    undefined,
    {
      type: 'error',
      confirmText: '清空',
      cancelText: '取消'
    }
  )
}

/**
 * 清除选择
 */
function clearSelection() {
  selectedItems.value = []
}

/**
 * 删除选中项目
 */
function deleteSelected() {
  showConfirm(
    `确定要删除选中的 ${selectedItems.value.length} 条记录吗？`,
    () => {
      historyItems.value = historyItems.value.filter(item => !selectedItems.value.includes(item.id))
      selectedItems.value = []
      showActionSuccess('删除', '选中的记录已删除')
    }
  )
}

/**
 * 处理页面变化
 */
function handlePageChange(page: number) {
  currentPage.value = page
}

/**
 * 处理页面大小变化
 */
function handlePageSizeChange(size: number) {
  pageSize.value = size
  currentPage.value = 1
}

// 生命周期
onMounted(() => {
  totalCount.value = historyItems.value.length
})
</script>

<style scoped>
/* 筛选器 */
.clipboard-filters {
  display: flex;
  gap: var(--space-4);
  align-items: center;
  padding: var(--space-5) var(--space-6);
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.04);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-md);
  margin-bottom: var(--space-6);
}

.filter-group {
  flex: 1;
}

.search-input,
.time-filter {
  width: 100%;
  padding: var(--space-3) var(--space-4);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  transition: all var(--transition-normal);
}

.search-input:focus,
.time-filter:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
  background: rgba(255, 255, 255, 1);
}

.filter-actions {
  flex-shrink: 0;
}

.clear-btn {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-3) var(--space-4);
  background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
  color: white;
  border: none;
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  cursor: pointer;
  transition: all var(--transition-normal);
  box-shadow: var(--shadow-sm);
}

.clear-btn:hover {
  background: linear-gradient(135deg, #c82333 0%, #a71e2a 100%);
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.btn-icon {
  width: 16px;
  height: 16px;
}

/* 加载状态 */
.loading-state {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.history-skeleton {
  height: 120px;
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-16) var(--space-8);
  text-align: center;
}

.empty-icon {
  width: 80px;
  height: 80px;
  color: var(--gray-300);
  margin-bottom: var(--space-6);
}

.empty-icon svg {
  width: 100%;
  height: 100%;
}

.empty-title {
  font-size: var(--text-xl);
  font-weight: var(--font-semibold);
  color: var(--gray-700);
  margin: 0 0 var(--space-2) 0;
}

.empty-description {
  color: var(--gray-500);
  font-size: var(--text-base);
  line-height: var(--leading-relaxed);
  margin: 0;
  max-width: 400px;
}

/* 历史记录列表 */
.history-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.history-item {
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.04);
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: all var(--transition-normal);
  position: relative;
}

.history-item:hover {
  box-shadow: var(--shadow-lg);
  transform: translateY(-2px);
}

.history-item.selected {
  border-color: var(--primary-500);
  box-shadow: var(--shadow-primary);
}

.time-label {
  position: absolute;
  top: var(--space-4);
  right: var(--space-4);
  font-size: var(--text-xs);
  color: var(--gray-500);
  background: rgba(255, 255, 255, 0.9);
  padding: var(--space-1) var(--space-2);
  border-radius: var(--radius-md);
  backdrop-filter: blur(10px);
}

.content-preview {
  padding: var(--space-5) var(--space-6);
  cursor: pointer;
}

.snippet-info {
  margin-bottom: var(--space-4);
}

.snippet-title {
  font-size: var(--text-base);
  font-weight: var(--font-medium);
  color: var(--gray-800);
  margin: 0 0 var(--space-2) 0;
  line-height: var(--leading-tight);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.snippet-meta {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.language-tag {
  color: white;
  padding: var(--space-1) var(--space-2);
  border-radius: var(--radius-md);
  font-size: var(--text-xs);
  font-weight: var(--font-semibold);
  text-transform: uppercase;
}

.copy-time {
  font-size: var(--text-xs);
  color: var(--gray-500);
}

.code-preview {
  background: linear-gradient(135deg, #1e1e1e 0%, #2d2d30 100%);
  border-radius: var(--radius-lg);
  padding: var(--space-4);
  overflow: hidden;
}

.code-block {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  line-height: var(--leading-relaxed);
  color: #d4d4d4;
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
}

.item-actions {
  display: flex;
  gap: var(--space-2);
  padding: var(--space-4) var(--space-6);
  background: rgba(248, 249, 250, 0.5);
  border-top: 1px solid rgba(0, 0, 0, 0.04);
}

.action-btn {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: var(--radius-lg);
  background: rgba(255, 255, 255, 0.9);
  color: var(--gray-600);
  cursor: pointer;
  transition: all var(--transition-normal);
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
}

.action-btn:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.copy-btn:hover {
  background: linear-gradient(135deg, #e7f3ff 0%, #cce7ff 100%);
  color: var(--primary-600);
}

.view-btn:hover {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  color: var(--info-500);
}

.delete-btn:hover {
  background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
  color: var(--error-500);
}

.action-icon {
  width: 18px;
  height: 18px;
}

/* 批量操作 */
.batch-actions {
  position: fixed;
  bottom: var(--space-6);
  left: 50%;
  transform: translateX(-50%);
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  padding: var(--space-4) var(--space-6);
  display: flex;
  align-items: center;
  gap: var(--space-4);
  z-index: var(--z-fixed);
  backdrop-filter: blur(20px);
}

.selection-info {
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  color: var(--gray-700);
}

.batch-buttons {
  display: flex;
  gap: var(--space-2);
}

.batch-btn {
  padding: var(--space-2) var(--space-4);
  border: none;
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
  font-weight: var(--font-medium);
  cursor: pointer;
  transition: all var(--transition-normal);
}

.batch-btn.secondary {
  background: var(--gray-100);
  color: var(--gray-700);
}

.batch-btn.secondary:hover {
  background: var(--gray-200);
}

.batch-btn.danger {
  background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
  color: white;
}

.batch-btn.danger:hover {
  background: linear-gradient(135deg, #c82333 0%, #a71e2a 100%);
  transform: translateY(-1px);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .clipboard-filters {
    flex-direction: column;
    gap: var(--space-3);
    padding: var(--space-4);
  }

  .filter-group {
    width: 100%;
  }

  .filter-actions {
    width: 100%;
  }

  .clear-btn {
    width: 100%;
    justify-content: center;
  }

  .content-preview {
    padding: var(--space-4);
  }

  .item-actions {
    padding: var(--space-3) var(--space-4);
    justify-content: center;
  }

  .batch-actions {
    left: var(--space-4);
    right: var(--space-4);
    transform: none;
    flex-direction: column;
    gap: var(--space-3);
  }

  .batch-buttons {
    width: 100%;
  }

  .batch-btn {
    flex: 1;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .history-item,
  .action-btn,
  .batch-btn {
    transition: none;
  }

  .history-item:hover,
  .action-btn:hover {
    transform: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .clipboard-filters,
  .history-item {
    border-width: 2px;
  }

  .search-input,
  .time-filter {
    border-width: 2px;
  }
}
</style>
