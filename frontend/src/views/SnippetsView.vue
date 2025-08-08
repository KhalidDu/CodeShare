<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <!-- 页面头部 -->
    <div class="bg-white/80 backdrop-blur-xl border-b border-slate-200/60 sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center space-x-4">
            <div class="flex items-center space-x-3">
              <div class="w-8 h-8 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center">
                <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"/>
                </svg>
              </div>
              <div>
                <h1 class="text-xl font-semibold text-slate-900">代码片段</h1>
                <p class="text-sm text-slate-500">管理您的代码片段集合</p>
              </div>
            </div>
          </div>

          <div class="flex items-center space-x-3">
            <router-link
              v-if="canCreateSnippet"
              to="/snippets/create"
              class="inline-flex items-center px-4 py-2 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white text-sm font-medium rounded-lg transition-all duration-200 shadow-sm hover:shadow-md"
            >
              <svg class="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 24 24">
                <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
              </svg>
              新建片段
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 搜索和筛选区域 -->
      <div class="mb-8">
        <div class="bg-white/70 backdrop-blur-xl rounded-2xl border border-slate-200/60 shadow-sm">
          <div class="p-6">
            <!-- 搜索栏 -->
            <div class="relative mb-6">
              <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                </svg>
              </div>
              <input
                v-model="searchQuery"
                type="text"
                placeholder="搜索代码片段、标签或描述..."
                class="block w-full pl-12 pr-4 py-3 border border-slate-200 rounded-xl text-slate-900 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white/80 backdrop-blur-sm transition-all duration-200"
                @input="handleSearchInput"
                @keyup.enter="handleSearch"
              />
            </div>

            <!-- 筛选器 -->
            <div class="flex flex-wrap items-center gap-4">
              <!-- 语言筛选 -->
              <div class="relative">
                <select
                  v-model="selectedLanguage"
                  @change="handleLanguageChange"
                  class="appearance-none bg-white/80 backdrop-blur-sm border border-slate-200 rounded-lg px-4 py-2 pr-10 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                >
                  <option value="">所有语言</option>
                  <option
                    v-for="language in availableLanguages"
                    :key="language.value"
                    :value="language.value"
                  >
                    {{ language.label }} ({{ language.count }})
                  </option>
                </select>
                <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                  <svg class="h-4 w-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                  </svg>
                </div>
              </div>

              <!-- 排序 -->
              <div class="relative">
                <select
                  v-model="sortBy"
                  @change="handleSortChange"
                  class="appearance-none bg-white/80 backdrop-blur-sm border border-slate-200 rounded-lg px-4 py-2 pr-10 text-sm text-slate-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
                >
                  <option value="createdAt_desc">最新创建</option>
                  <option value="updatedAt_desc">最近更新</option>
                  <option value="viewCount_desc">最多查看</option>
                  <option value="copyCount_desc">最多复制</option>
                  <option value="title_asc">标题 A-Z</option>
                </select>
                <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                  <svg class="h-4 w-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                  </svg>
                </div>
              </div>

              <!-- 视图切换 -->
              <div class="flex items-center bg-slate-100 rounded-lg p-1">
                <button
                  @click="viewMode = 'grid'"
                  :class="[
                    'flex items-center justify-center w-8 h-8 rounded-md transition-all duration-200',
                    viewMode === 'grid'
                      ? 'bg-white text-blue-600 shadow-sm'
                      : 'text-slate-500 hover:text-slate-700'
                  ]"
                  title="网格视图"
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M3,11H11V3H3M3,21H11V13H3M13,21H21V13H13M13,3V11H21V3"/>
                  </svg>
                </button>
                <button
                  @click="viewMode = 'list'"
                  :class="[
                    'flex items-center justify-center w-8 h-8 rounded-md transition-all duration-200',
                    viewMode === 'list'
                      ? 'bg-white text-blue-600 shadow-sm'
                      : 'text-slate-500 hover:text-slate-700'
                  ]"
                  title="列表视图"
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M9,5V9H21V5M9,19H21V15H9M9,14H21V10H9M4,9H8V5H4M4,19H8V15H4M4,14H8V10H4"/>
                  </svg>
                </button>
              </div>

              <!-- 清除筛选 -->
              <button
                v-if="hasActiveFilters"
                @click="clearAllFilters"
                class="inline-flex items-center px-3 py-2 text-sm text-slate-600 hover:text-slate-900 bg-slate-100 hover:bg-slate-200 rounded-lg transition-all duration-200"
              >
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                </svg>
                清除筛选
              </button>
            </div>

            <!-- 结果统计 -->
            <div class="mt-4 flex items-center justify-between text-sm text-slate-600">
              <span>
                共找到 <span class="font-medium text-slate-900">{{ totalCount }}</span> 个代码片段
                <span v-if="hasActiveFilters" class="text-blue-600">（已筛选）</span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 代码片段列表 -->
      <div v-if="isLoading && snippets.length === 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="i in 6" :key="i" class="bg-white/70 backdrop-blur-xl rounded-2xl border border-slate-200/60 p-6 animate-pulse">
          <div class="h-4 bg-slate-200 rounded w-3/4 mb-3"></div>
          <div class="h-3 bg-slate-200 rounded w-1/2 mb-4"></div>
          <div class="h-20 bg-slate-200 rounded mb-4"></div>
          <div class="flex space-x-2">
            <div class="h-6 bg-slate-200 rounded-full w-16"></div>
            <div class="h-6 bg-slate-200 rounded-full w-20"></div>
          </div>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-else-if="!isLoading && snippets.length === 0" class="text-center py-16">
        <div class="w-24 h-24 mx-auto mb-6 bg-gradient-to-br from-slate-100 to-slate-200 rounded-2xl flex items-center justify-center">
          <svg class="w-12 h-12 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
          </svg>
        </div>
        <h3 class="text-xl font-semibold text-slate-900 mb-2">
          {{ hasActiveFilters ? '没有找到匹配的代码片段' : '还没有代码片段' }}
        </h3>
        <p class="text-slate-600 mb-8 max-w-md mx-auto">
          {{ hasActiveFilters ? '尝试调整筛选条件或清除筛选器' : '创建您的第一个代码片段，开始构建您的代码库' }}
        </p>
        <div class="flex items-center justify-center space-x-4">
          <button
            v-if="hasActiveFilters"
            @click="clearAllFilters"
            class="inline-flex items-center px-4 py-2 text-slate-600 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors duration-200"
          >
            清除筛选器
          </button>
          <router-link
            v-if="canCreateSnippet && !hasActiveFilters"
            to="/snippets/create"
            class="inline-flex items-center px-6 py-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-sm hover:shadow-md"
          >
            <svg class="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 24 24">
              <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
            </svg>
            创建第一个片段
          </router-link>
        </div>
      </div>

      <!-- 代码片段网格 -->
      <div v-else :class="[
        'grid gap-6',
        viewMode === 'grid' ? 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3' : 'grid-cols-1'
      ]">
        <SnippetCard
          v-for="snippet in snippets"
          :key="snippet.id"
          :snippet="snippet"
          :view-mode="viewMode"
          :is-loading="isLoading"
          @copy="handleSnippetCopy"
          @delete="handleSnippetDelete"
          @tag-click="handleTagClick"
        />
      </div>

      <!-- 分页 -->
      <div v-if="totalPages > 1" class="mt-12 flex justify-center">
        <Pagination
          :current-page="currentPage"
          :total-pages="totalPages"
          :total-items="totalCount"
          :page-size="pageSize"
          @page-change="handlePageChange"
          @size-change="handlePageSizeChange"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCodeSnippetsStore } from '@/stores/codeSnippets'
import { useUserFeedback } from '@/composables/useUserFeedback'
import { useLoading } from '@/composables/useLoading'
import { usePerformance } from '@/composables/usePerformance'
import SnippetCard from '@/components/snippets/SnippetCard.vue'
import Pagination from '@/components/common/Pagination.vue'

import type { CodeSnippet, Tag } from '@/types'
import { UserRole } from '@/types'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const snippetsStore = useCodeSnippetsStore()
const { showActionSuccess, showActionError, showConfirm } = useUserFeedback()
const { withLoading } = useLoading('加载代码片段中...')
const { mark, measure } = usePerformance()

// 响应式状态
const viewMode = ref<'grid' | 'list'>('grid')
const searchQuery = ref('')
const selectedLanguage = ref('')
const sortBy = ref('createdAt_desc')
const searchTimeout = ref<number | null>(null)

const currentFilters = ref({
  search: '',
  language: '',
  tag: '',
  creator: '',
  showPublic: true,
  showPrivate: true,
  sortBy: 'createdAt_desc'
})

// 计算属性
const snippets = computed(() => snippetsStore.snippets)
const isLoading = computed(() => snippetsStore.isLoading)
const currentPage = computed(() => snippetsStore.currentPage)
const totalPages = computed(() => Math.ceil(snippetsStore.totalCount / snippetsStore.pageSize))
const totalCount = computed(() => snippetsStore.totalCount)
const pageSize = computed(() => snippetsStore.pageSize)

const canCreateSnippet = computed(() => {
  const user = authStore.user
  if (!user) return false
  return user.role === UserRole.Admin || user.role === UserRole.Editor
})

const hasActiveFilters = computed(() => {
  return !!(currentFilters.value.search ||
           currentFilters.value.language ||
           currentFilters.value.tag ||
           currentFilters.value.creator ||
           (!currentFilters.value.showPublic || !currentFilters.value.showPrivate))
})

// 模拟可用的筛选选项数据
const availableLanguages = computed(() => [
  { value: 'javascript', label: 'JavaScript', count: 45 },
  { value: 'typescript', label: 'TypeScript', count: 32 },
  { value: 'python', label: 'Python', count: 28 },
  { value: 'java', label: 'Java', count: 21 },
  { value: 'csharp', label: 'C#', count: 18 },
  { value: 'html', label: 'HTML', count: 15 },
  { value: 'css', label: 'CSS', count: 12 },
  { value: 'vue', label: 'Vue', count: 10 },
  { value: 'react', label: 'React', count: 8 }
])

const availableTags = computed(() => [
  { id: '1', name: '工具函数', color: '#007bff', count: 25, createdBy: '1', createdAt: '2024-01-01' },
  { id: '2', name: '组件', color: '#28a745', count: 20, createdBy: '1', createdAt: '2024-01-01' },
  { id: '3', name: '算法', color: '#dc3545', count: 15, createdBy: '1', createdAt: '2024-01-01' },
  { id: '4', name: 'API', color: '#ffc107', count: 12, createdBy: '1', createdAt: '2024-01-01' },
  { id: '5', name: '样式', color: '#17a2b8', count: 10, createdBy: '1', createdAt: '2024-01-01' }
])

const availableCreators = computed(() => [
  { id: '1', name: '张三', count: 35 },
  { id: '2', name: '李四', count: 28 },
  { id: '3', name: '王五', count: 22 },
  { id: '4', name: '赵六', count: 15 }
])

// 生命周期
onMounted(() => {
  // 从 URL 查询参数初始化筛选器
  initializeFiltersFromQuery()

  // 加载代码片段
  loadSnippets()
})

// 监听路由查询参数变化
watch(() => route.query, () => {
  initializeFiltersFromQuery()
  loadSnippets()
})

/**
 * 从 URL 查询参数初始化筛选器
 */
function initializeFiltersFromQuery() {
  currentFilters.value = {
    search: (route.query.search as string) || '',
    language: (route.query.language as string) || '',
    tag: (route.query.tag as string) || '',
    creator: (route.query.creator as string) || '',
    showPublic: route.query.showPublic !== 'false',
    showPrivate: route.query.showPrivate !== 'false',
    sortBy: (route.query.sortBy as string) || 'createdAt_desc'
  }
}

/**
 * 加载代码片段
 */
const loadSnippets = withLoading(async () => {
  mark('snippets-load-start')

  await snippetsStore.fetchSnippets({
    page: parseInt((route.query.page as string) || '1'),
    pageSize: parseInt((route.query.pageSize as string) || '20'),
    search: currentFilters.value.search,
    language: currentFilters.value.language,
    tag: currentFilters.value.tag,
    creator: currentFilters.value.creator,
    showPublic: currentFilters.value.showPublic,
    showPrivate: currentFilters.value.showPrivate,
    sortBy: currentFilters.value.sortBy
  })

  mark('snippets-load-end')
  const loadTime = measure('snippets-load-time', 'snippets-load-start', 'snippets-load-end')
  console.log(`代码片段加载时间: ${loadTime.toFixed(2)}ms`)
})

/**
 * 处理搜索输入
 */
function handleSearchInput() {
  if (searchTimeout.value) {
    clearTimeout(searchTimeout.value)
  }

  searchTimeout.value = window.setTimeout(() => {
    currentFilters.value.search = searchQuery.value
    updateQueryParams()
  }, 300)
}

/**
 * 处理语言变化
 */
function handleLanguageChange() {
  currentFilters.value.language = selectedLanguage.value
  updateQueryParams()
}

/**
 * 处理排序变化
 */
function handleSortChange() {
  currentFilters.value.sortBy = sortBy.value
  updateQueryParams()
}

/**
 * 处理筛选器变化
 */
function handleFilterChange(filters: typeof currentFilters.value) {
  currentFilters.value = { ...filters }
  updateQueryParams()
}

/**
 * 处理搜索
 */
function handleSearch(query: string) {
  currentFilters.value.search = query
  updateQueryParams()
}

/**
 * 处理页面变化
 */
function handlePageChange(page: number) {
  updateQueryParams({ page: page.toString() })
}

/**
 * 处理页面大小变化
 */
function handlePageSizeChange(size: number) {
  updateQueryParams({ pageSize: size.toString(), page: '1' })
}

/**
 * 处理代码片段复制
 */
async function handleSnippetCopy(snippet: CodeSnippet) {
  try {
    // 这里可以调用 API 记录复制统计
    console.log('代码片段已复制:', snippet.title)
    showActionSuccess('复制', `"${snippet.title}" 已复制到剪贴板`)
  } catch (error) {
    console.error('记录复制统计失败:', error)
    showActionError('复制', '复制失败，请重试')
  }
}

/**
 * 处理代码片段删除
 */
function handleSnippetDelete(snippet: CodeSnippet) {
  showConfirm(
    `确定要删除代码片段 "${snippet.title}" 吗？此操作无法撤销。`,
    async () => {
      try {
        await snippetsStore.deleteSnippet(snippet.id)
        showActionSuccess('删除', `"${snippet.title}" 已删除`)
        // 重新加载当前页面的数据
        await loadSnippets()
      } catch (error) {
        console.error('删除代码片段失败:', error)
        showActionError('删除', '删除失败，请重试')
      }
    },
    undefined,
    {
      type: 'error',
      confirmText: '删除',
      cancelText: '取消'
    }
  )
}

/**
 * 处理标签点击
 */
function handleTagClick(tag: Tag) {
  currentFilters.value.tag = tag.id
  updateQueryParams()
}

/**
 * 清除所有筛选器
 */
function clearAllFilters() {
  currentFilters.value = {
    search: '',
    language: '',
    tag: '',
    creator: '',
    showPublic: true,
    showPrivate: true,
    sortBy: 'createdAt_desc'
  }
  updateQueryParams()
}

/**
 * 更新 URL 查询参数
 */
function updateQueryParams(additionalParams: Record<string, string> = {}) {
  const query: Record<string, string> = {
    ...additionalParams
  }

  // 只添加非默认值的参数
  if (currentFilters.value.search) query.search = currentFilters.value.search
  if (currentFilters.value.language) query.language = currentFilters.value.language
  if (currentFilters.value.tag) query.tag = currentFilters.value.tag
  if (currentFilters.value.creator) query.creator = currentFilters.value.creator
  if (!currentFilters.value.showPublic) query.showPublic = 'false'
  if (!currentFilters.value.showPrivate) query.showPrivate = 'false'
  if (currentFilters.value.sortBy !== 'createdAt_desc') query.sortBy = currentFilters.value.sortBy

  // 保持当前页面参数（如果没有在 additionalParams 中指定）
  if (!additionalParams.page && route.query.page) {
    query.page = route.query.page as string
  }
  if (!additionalParams.pageSize && route.query.pageSize) {
    query.pageSize = route.query.pageSize as string
  }

  router.push({ query })
}
</script>

<style scoped>
/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
    animation: none !important;
  }
}
</style>
