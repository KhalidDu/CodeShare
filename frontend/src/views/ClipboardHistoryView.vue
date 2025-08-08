<template>
  <AppLayout
    page-title="剪贴板历史"
    page-subtitle="查看最近复制的代码片段"
    page-icon="fas fa-clipboard-list"
  >
    <!-- 筛选和搜索 -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 mb-6">
      <div class="flex flex-wrap gap-4 items-center">
        <div class="flex-1 min-w-64">
          <input
            v-model="searchQuery"
            type="text"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            placeholder="搜索剪贴板内容..."
          />
        </div>
        <div>
          <select v-model="timeFilter" class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent">
            <option value="all">全部时间</option>
            <option value="today">今天</option>
            <option value="week">本周</option>
            <option value="month">本月</option>
          </select>
        </div>
        <div>
          <button @click="clearHistory" class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors duration-200 flex items-center gap-2">
            <i class="fas fa-trash"></i>
            清空历史
          </button>
        </div>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="i in 5" :key="i" class="bg-white dark:bg-gray-800 rounded-lg p-4 animate-pulse">
        <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
        <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
      </div>
    </div>

    <!-- 空状态 -->
    <div v-else-if="filteredHistory.length === 0" class="text-center py-12">
      <div class="text-gray-400 dark:text-gray-500 mb-4">
        <i class="fas fa-clipboard-list text-6xl"></i>
      </div>
      <h3 class="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">
        {{ searchQuery ? '没有找到匹配的记录' : '剪贴板历史为空' }}
      </h3>
      <p class="text-gray-500 dark:text-gray-400">
        {{ searchQuery ? '尝试调整搜索条件' : '复制代码片段后，历史记录将显示在这里' }}
      </p>
    </div>

    <!-- 历史记录列表 -->
    <div v-else class="space-y-4">
      <div
        v-for="item in filteredHistory"
        :key="item.id"
        class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 hover:shadow-md transition-shadow duration-200"
      >
        <!-- 头部信息 -->
        <div class="flex items-center justify-between mb-3">
          <div class="flex items-center gap-3">
            <h4 class="font-medium text-gray-900 dark:text-gray-100 truncate">{{ item.snippetTitle }}</h4>
            <span class="px-2 py-1 text-xs font-medium rounded-full text-white" :style="{ backgroundColor: getLanguageColor(item.language) }">
              {{ item.language }}
            </span>
          </div>
          <div class="text-sm text-gray-500 dark:text-gray-400">
            {{ formatRelativeTime(item.copiedAt) }}
          </div>
        </div>

        <!-- 代码预览 -->
        <div class="bg-gray-900 dark:bg-gray-950 rounded-lg p-3 mb-3">
          <pre class="text-sm text-gray-300 font-mono overflow-x-auto"><code>{{ getCodePreview(item.content) }}</code></pre>
        </div>

        <!-- 操作按钮 -->
        <div class="flex items-center gap-2">
          <button
            @click="copyToClipboard(item)"
            class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg transition-colors duration-200 flex items-center gap-2"
          >
            <i class="fas fa-copy"></i>
            重新复制
          </button>
          <button
            @click="viewSnippet(item.snippetId)"
            class="px-3 py-1.5 bg-gray-600 hover:bg-gray-700 text-white text-sm rounded-lg transition-colors duration-200 flex items-center gap-2"
          >
            <i class="fas fa-eye"></i>
            查看原片段
          </button>
          <button
            @click="removeItem(item.id)"
            class="px-3 py-1.5 bg-red-600 hover:bg-red-700 text-white text-sm rounded-lg transition-colors duration-200 flex items-center gap-2"
          >
            <i class="fas fa-trash"></i>
            删除
          </button>
        </div>
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
      class="mt-6"
    />
  </AppLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
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

// 响应式状态
const isLoading = ref(false)
const searchQuery = ref('')
const timeFilter = ref('all')
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
 * 复制到剪贴板
 */
async function copyToClipboard(item: ClipboardHistoryItem) {
  try {
    await navigator.clipboard.writeText(item.content)
    console.log('复制成功')
  } catch (error) {
    console.error('复制失败:', error)
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
  if (confirm('确定要删除这条剪贴板记录吗？')) {
    const index = historyItems.value.findIndex(item => item.id === id)
    if (index > -1) {
      historyItems.value.splice(index, 1)
    }
  }
}

/**
 * 清空历史
 */
function clearHistory() {
  if (confirm('确定要清空所有剪贴板历史记录吗？此操作不可撤销。')) {
    historyItems.value = []
  }
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
/* 自定义样式 */
</style>
