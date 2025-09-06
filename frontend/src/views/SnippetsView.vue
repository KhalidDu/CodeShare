<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-50 via-white to-slate-100">
    <!-- 主要内容区域 -->
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 搜索和筛选区域 -->
      <div class="mb-8">
        <div class="glass rounded-2xl shadow-sm">
          <div class="p-6">
            <!-- 搜索栏 -->
            <div class="relative mb-6">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              <input v-model="searchQuery" type="text" placeholder="搜索代码片段、标签或描述..."
                class="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white text-gray-900 placeholder-gray-500 text-sm transition-colors duration-200"
                @input="handleSearchInput" @keyup.enter="handleSearchEnter" />
            </div>

            <!-- 筛选器 -->
            <div class="flex flex-wrap items-center gap-2">
              <!-- 语言筛选多选tab -->
              <div class="relative">
                <div class="flex items-center gap-2">
                  <!-- 语言选择输入框 -->
                  <div class="relative">
                    <input
                      v-model="languageSearchQuery"
                      type="text"
                      placeholder="搜索语言..."
                      class="w-32 pl-3 pr-8 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white text-gray-900 text-xs transition-colors duration-200"
                      @input="handleLanguageSearchInput"
                      @focus="showLanguageDropdown = true"
                      @blur="handleLanguageBlur"
                    />
                    <div class="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                      <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                      </svg>
                    </div>
                    
                    <!-- 语言下拉列表 -->
                    <div v-if="showLanguageDropdown" class="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-300 rounded-md shadow-lg z-10 max-h-60 overflow-y-auto">
                      <div
                        v-for="language in filteredLanguages"
                        :key="language.value"
                        class="flex items-center justify-between px-3 py-2 hover:bg-gray-50 cursor-pointer text-xs"
                        @mousedown="toggleLanguageSelection(language)"
                      >
                        <div class="flex items-center gap-2">
                          <div
                            class="w-2 h-2 rounded-full"
                            :style="{ backgroundColor: getLanguageColor(language.value) }"
                          ></div>
                          <span>{{ language.label }}</span>
                          <span class="text-gray-500">({{ language.count }})</span>
                        </div>
                        <div
                          v-if="selectedLanguages.includes(language.value)"
                          class="w-4 h-4 rounded bg-blue-500 flex items-center justify-center"
                        >
                          <svg class="w-3 h-3 text-white" fill="currentColor" viewBox="0 0 24 24">
                            <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41L9 16.17z" />
                          </svg>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <!-- 已选语言标签 -->
                  <div v-if="selectedLanguages.length > 0" class="flex flex-wrap gap-1">
                    <div
                      v-for="lang in selectedLanguages"
                      :key="lang"
                      class="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium transition-all duration-200"
                      :style="{
                        backgroundColor: getLanguageColor(lang) + '20',
                        color: getLanguageColor(lang),
                        border: `1px solid ${getLanguageColor(lang)}40`
                      }"
                    >
                      <div
                        class="w-1.5 h-1.5 rounded-full"
                        :style="{ backgroundColor: getLanguageColor(lang) }"
                      ></div>
                      {{ getLanguageDisplayName(lang) }}
                      <button
                        @click="removeLanguage(lang)"
                        class="ml-1 hover:text-red-500 transition-colors duration-200"
                      >
                        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- 排序 -->
              <div class="relative">
                <select v-model="sortBy" @change="handleSortChange"
                  class="appearance-none pl-3 pr-8 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 bg-white text-gray-900 text-xs transition-colors duration-200">
                  <option value="createdAt_desc">最新创建</option>
                  <option value="updatedAt_desc">最近更新</option>
                  <option value="viewCount_desc">最多查看</option>
                  <option value="copyCount_desc">最多复制</option>
                  <option value="title_asc">标题 A-Z</option>
                </select>
                <div class="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </div>
              </div>

              <!-- 视图切换 -->
              <div class="flex items-center bg-gray-100 rounded-md p-1">
                <button @click="viewMode = 'grid'" :class="[
                  'flex items-center justify-center w-7 h-7 rounded-sm transition-all duration-200',
                  viewMode === 'grid'
                    ? 'bg-white text-blue-600 shadow-sm'
                    : 'text-gray-500 hover:text-gray-700'
                ]" title="网格视图">
                  <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M3,11H11V3H3M3,21H11V13H3M13,21H21V13H13M13,3V11H21V3" />
                  </svg>
                </button>
                <button @click="viewMode = 'list'" :class="[
                  'flex items-center justify-center w-7 h-7 rounded-sm transition-all duration-200',
                  viewMode === 'list'
                    ? 'bg-white text-blue-600 shadow-sm'
                    : 'text-gray-500 hover:text-gray-700'
                ]" title="列表视图">
                  <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M9,5V9H21V5M9,19H21V15H9M9,14H21V10H9M4,9H8V5H4M4,19H8V15H4M4,14H8V10H4" />
                  </svg>
                </button>
              </div>

              <!-- 清除筛选 -->
              <button v-if="hasActiveFilters" @click="clearAllFilters"
                class="flex items-center px-3 py-2 text-xs text-gray-600 bg-white border border-gray-300 rounded-md hover:bg-gray-50 hover:text-gray-800 transition-colors duration-200">
                <svg class="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
                清除筛选
              </button>
            </div>

            <!-- 结果统计和新建按钮 -->
            <div class="mt-4 flex items-center justify-between text-xs text-slate-600">
              <span>
                共找到 <span class="font-medium text-slate-900">{{ totalCount }}</span> 个代码片段
                <span v-if="hasActiveFilters" class="text-blue-600">（已筛选）</span>
              </span>
              <router-link v-if="canCreateSnippet" to="/snippets/create"
                class="inline-flex items-center px-3 py-1.5 bg-blue-600 text-white text-xs font-medium rounded-md hover:bg-blue-700 transition-colors duration-200">
                <svg class="w-3.5 h-3.5 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
                </svg>
                新建片段
              </router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- 代码片段列表 -->
      <div v-if="isLoading && snippets.length === 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="i in 6" :key="i"
          class="glass rounded-2xl p-6 animate-pulse">
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
        <div
          class="w-24 h-24 mx-auto mb-6 bg-gradient-to-br from-slate-100 to-slate-200 rounded-2xl flex items-center justify-center">
          <svg class="w-16 h-16 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 class="text-xl font-semibold text-slate-900 mb-2">
          {{ hasActiveFilters ? '没有找到匹配的代码片段' : '还没有代码片段' }}
        </h3>
        <p class="text-slate-600 mb-8 max-w-md mx-auto">
          {{ hasActiveFilters ? '尝试调整筛选条件或清除筛选器' : '创建您的第一个代码片段，开始构建您的代码库' }}
        </p>
        <div class="flex items-center justify-center space-x-4">
          <button v-if="hasActiveFilters" @click="clearAllFilters"
            class="btn-secondary">
            清除筛选器
          </button>
          <router-link v-if="canCreateSnippet && !hasActiveFilters" to="/snippets/create"
            class="btn-primary px-6 py-2">
            <svg class="w-4 h-4 mr-2" fill="currentColor" viewBox="0 0 24 24">
              <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z" />
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
        <SnippetCard v-for="snippet in snippets" :key="snippet.id" :snippet="snippet" :view-mode="viewMode"
          :is-loading="isLoading" @copy="handleSnippetCopy" @delete="handleSnippetDelete" @tag-click="handleTagClick" />
      </div>

      <!-- 分页 -->
      <div v-if="totalPages > 1" class="mt-12 flex justify-center">
        <Pagination :current-page="currentPage" :total-pages="totalPages" :total-items="totalCount"
          :page-size="pageSize" @page-change="handlePageChange" @size-change="handlePageSizeChange" />
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
const selectedLanguages = ref<string[]>([])
const languageSearchQuery = ref('')
const showLanguageDropdown = ref(false)
const sortBy = ref('createdAt_desc')
const searchTimeout = ref<number | null>(null)
const languageBlurTimeout = ref<number | null>(null)

const currentFilters = ref({
  search: '',
  languages: [] as string[],
  tag: '',
  creator: '',
  showPublic: true,
  showPrivate: false,
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
    currentFilters.value.languages.length > 0 ||
    currentFilters.value.tag ||
    currentFilters.value.creator ||
    (!currentFilters.value.showPublic || !currentFilters.value.showPrivate))
})

// 语言相关的计算属性
const filteredLanguages = computed(() => {
  if (!languageSearchQuery.value) {
    return availableLanguages.value
  }
  
  const query = languageSearchQuery.value.toLowerCase()
  return availableLanguages.value.filter(lang => 
    lang.label.toLowerCase().includes(query) ||
    lang.value.toLowerCase().includes(query)
  )
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
  const isAuthenticated = authStore.isAuthenticated
  
  // 处理多语言选择
  const languagesQuery = route.query.languages as string
  const languages = languagesQuery ? languagesQuery.split(',') : []
  
  currentFilters.value = {
    search: (route.query.search as string) || '',
    languages: languages,
    tag: (route.query.tag as string) || '',
    creator: (route.query.creator as string) || '',
    showPublic: route.query.showPublic !== 'false',
    showPrivate: isAuthenticated ? route.query.showPrivate !== 'false' : false,
    sortBy: (route.query.sortBy as string) || 'createdAt_desc'
  }
  
  // 同步到响应式变量
  selectedLanguages.value = languages
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
    languages: currentFilters.value.languages,
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
 * 处理语言搜索输入
 */
function handleLanguageSearchInput() {
  // 输入时自动显示下拉列表
  showLanguageDropdown.value = true
}

/**
 * 处理语言输入框失焦
 */
function handleLanguageBlur() {
  // 延迟隐藏下拉列表，以便能够点击选项
  languageBlurTimeout.value = window.setTimeout(() => {
    showLanguageDropdown.value = false
  }, 200)
}

/**
 * 切换语言选择状态
 */
function toggleLanguageSelection(language: { value: string; label: string; count: number }) {
  const langValue = language.value
  const index = selectedLanguages.value.indexOf(langValue)
  
  if (index > -1) {
    // 如果已选中，则移除
    selectedLanguages.value.splice(index, 1)
  } else {
    // 如果未选中，则添加
    selectedLanguages.value.push(langValue)
  }
  
  // 更新过滤器
  currentFilters.value.languages = [...selectedLanguages.value]
  updateQueryParams()
}

/**
 * 移除单个语言选择
 */
function removeLanguage(lang: string) {
  const index = selectedLanguages.value.indexOf(lang)
  if (index > -1) {
    selectedLanguages.value.splice(index, 1)
    currentFilters.value.languages = [...selectedLanguages.value]
    updateQueryParams()
  }
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
 * 处理搜索回车键
 */
function handleSearchEnter() {
  handleSearch(searchQuery.value)
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
    // 成功提示已在 SnippetCard 组件中处理，这里不需要额外的提示
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
    angular: 'Angular',
    php: 'PHP',
    ruby: 'Ruby',
    go: 'Go',
    rust: 'Rust',
    swift: 'Swift',
    kotlin: 'Kotlin',
    dart: 'Dart',
    sql: 'SQL',
    shell: 'Shell',
    powershell: 'PowerShell',
    json: 'JSON',
    xml: 'XML',
    yaml: 'YAML',
    markdown: 'Markdown'
  }

  return displayNames[language.toLowerCase()] || language
}

/**
 * 清除所有筛选器
 */
function clearAllFilters() {
  const isAuthenticated = authStore.isAuthenticated
  
  currentFilters.value = {
    search: '',
    languages: [],
    tag: '',
    creator: '',
    showPublic: true,
    showPrivate: isAuthenticated,
    sortBy: 'createdAt_desc'
  }
  
  // 清除响应式变量
  selectedLanguages.value = []
  languageSearchQuery.value = ''
  
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
  if (currentFilters.value.languages.length > 0) query.languages = currentFilters.value.languages.join(',')
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
/* 组件特定样式 */
</style>
