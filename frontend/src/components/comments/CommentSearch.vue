<template>
  <div class="comment-search" :class="searchClasses">
    <!-- 搜索输入框 -->
    <div class="search-container">
      <div class="search-input-wrapper">
        <i class="fas fa-search search-icon"></i>
        <input
          ref="searchInputRef"
          v-model="searchKeyword"
          :placeholder="placeholder"
          :disabled="loading"
          type="text"
          class="search-input"
          @input="handleInput"
          @keydown="handleKeydown"
          @focus="handleFocus"
          @blur="handleBlur"
        />
        <button
          v-if="searchKeyword"
          type="button"
          class="search-clear"
          @click="handleClear"
        >
          <i class="fas fa-times"></i>
        </button>
        <button
          type="button"
          class="search-submit"
          @click="handleSearch"
          :disabled="!canSearch || loading"
        >
          <i :class="loading ? 'fas fa-spinner fa-spin' : 'fas fa-search'"></i>
        </button>
      </div>
    </div>

    <!-- 高级搜索选项 -->
    <div v-if="showAdvanced" class="advanced-search">
      <div class="advanced-header">
        <button
          type="button"
          class="advanced-toggle"
          @click="showAdvancedOptions = !showAdvancedOptions"
        >
          <i :class="showAdvancedOptions ? 'fas fa-chevron-up' : 'fas fa-chevron-down'"></i>
          高级搜索
        </button>
      </div>
      
      <div v-if="showAdvancedOptions" class="advanced-options">
        <!-- 搜索范围 -->
        <div class="option-group">
          <label class="option-label">搜索范围</label>
          <div class="option-buttons">
            <button
              v-for="scope in searchScopes"
              :key="scope.value"
              type="button"
              :class="getScopeButtonClasses(scope.value)"
              @click="selectedScope = scope.value"
            >
              {{ scope.label }}
            </button>
          </div>
        </div>

        <!-- 时间范围 -->
        <div class="option-group">
          <label class="option-label">时间范围</label>
          <div class="date-range">
            <input
              v-model="startDate"
              type="date"
              class="date-input"
              placeholder="开始日期"
            />
            <span class="date-separator">至</span>
            <input
              v-model="endDate"
              type="date"
              class="date-input"
              placeholder="结束日期"
            />
          </div>
        </div>

        <!-- 评论状态 -->
        <div class="option-group">
          <label class="option-label">评论状态</label>
          <div class="option-buttons">
            <button
              v-for="status in commentStatuses"
              :key="status.value"
              type="button"
              :class="getStatusButtonClasses(status.value)"
              @click="toggleStatus(status.value)"
            >
              {{ status.label }}
            </button>
          </div>
        </div>

        <!-- 排序方式 -->
        <div class="option-group">
          <label class="option-label">排序方式</label>
          <select v-model="selectedSort" class="sort-select">
            <option
              v-for="sort in sortOptions"
              :key="sort.value"
              :value="sort.value"
            >
              {{ sort.label }}
            </option>
          </select>
        </div>

        <!-- 用户筛选 -->
        <div class="option-group">
          <label class="option-label">用户筛选</label>
          <input
            v-model="userFilter"
            type="text"
            class="user-input"
            placeholder="输入用户名"
          />
        </div>
      </div>
    </div>

    <!-- 搜索建议 -->
    <div v-if="showSuggestions && suggestions.length > 0" class="search-suggestions">
      <div class="suggestions-header">
        <i class="fas fa-lightbulb"></i>
        搜索建议
      </div>
      <div class="suggestions-list">
        <button
          v-for="suggestion in suggestions"
          :key="suggestion.id"
          type="button"
          class="suggestion-item"
          @click="applySuggestion(suggestion)"
        >
          <div class="suggestion-text">{{ suggestion.text }}</div>
          <div class="suggestion-meta">{{ suggestion.type }}</div>
        </button>
      </div>
    </div>

    <!-- 搜索历史 -->
    <div v-if="showHistory && searchHistory.length > 0 && !searchKeyword" class="search-history">
      <div class="history-header">
        <div class="history-title">
          <i class="fas fa-history"></i>
          搜索历史
        </div>
        <button
          type="button"
          class="history-clear"
          @click="clearHistory"
        >
          清空历史
        </button>
      </div>
      <div class="history-list">
        <button
          v-for="history in searchHistory"
          :key="history.id"
          type="button"
          class="history-item"
          @click="applyHistory(history)"
        >
          <i class="fas fa-clock"></i>
          {{ history.keyword }}
          <span class="history-time">{{ formatTime(history.timestamp) }}</span>
        </button>
      </div>
    </div>

    <!-- 搜索过滤器标签 -->
    <div v-if="hasActiveFilters" class="active-filters">
      <div class="filters-header">
        <span>活跃过滤器</span>
        <button
          type="button"
          class="filters-clear"
          @click="clearAllFilters"
        >
          清空全部
        </button>
      </div>
      <div class="filters-list">
        <div
          v-for="filter in activeFilters"
          :key="filter.key"
          class="filter-tag"
        >
          <span class="filter-label">{{ filter.label }}</span>
          <button
            type="button"
            class="filter-remove"
            @click="removeFilter(filter.key)"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- 搜索统计 -->
    <div v-if="showStats && searchStats" class="search-stats">
      <div class="stats-content">
        <div class="stat-item">
          <i class="fas fa-search"></i>
          <span class="stat-label">搜索结果</span>
          <span class="stat-value">{{ searchStats.totalResults }}</span>
        </div>
        <div class="stat-item">
          <i class="fas fa-clock"></i>
          <span class="stat-label">耗时</span>
          <span class="stat-value">{{ searchStats.duration }}ms</span>
        </div>
        <div class="stat-item">
          <i class="fas fa-filter"></i>
          <span class="stat-label">过滤器</span>
          <span class="stat-value">{{ activeFiltersCount }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, onUnmounted, watch } from 'vue'
import type { CommentSearchScope, CommentSearchSort, CommentStatus } from '@/types/comment'

// 定义Props
interface Props {
  modelValue?: string
  placeholder?: string
  loading?: boolean
  disabled?: boolean
  showAdvanced?: boolean
  showSuggestions?: boolean
  showHistory?: boolean
  showStats?: boolean
  autoFocus?: boolean
  debounceTime?: number
  maxHistoryItems?: number
  enableHotkeys?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: '搜索评论...',
  loading: false,
  disabled: false,
  showAdvanced: true,
  showSuggestions: true,
  showHistory: true,
  showStats: true,
  autoFocus: false,
  debounceTime: 300,
  maxHistoryItems: 10,
  enableHotkeys: true
})

// 定义Emits
interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'search', data: SearchData): void
  (e: 'clear'): void
  (e: 'focus'): void
  (e: 'blur'): void
  (e: 'suggestion-select', suggestion: any): void
  (e: 'history-select', history: any): void
  (e: 'filter-change', filters: SearchFilters): void
}

const emit = defineEmits<Emits>()

// 类型定义
interface SearchData {
  keyword: string
  scope: CommentSearchScope
  startDate?: string
  endDate?: string
  statuses?: CommentStatus[]
  sort: CommentSearchSort
  userFilter?: string
}

interface SearchFilters {
  scope: CommentSearchScope
  startDate?: string
  endDate?: string
  statuses: CommentStatus[]
  sort: CommentSearchSort
  userFilter?: string
}

interface SearchSuggestion {
  id: string
  text: string
  type: string
  keyword: string
}

interface SearchHistory {
  id: string
  keyword: string
  timestamp: number
  filters?: SearchFilters
}

interface SearchStats {
  totalResults: number
  duration: number
  filters: number
}

// 响应式状态
const searchKeyword = ref(props.modelValue)
const loading = ref(false)
const showAdvancedOptions = ref(false)
const selectedScope = ref<CommentSearchScope>(CommentSearchScope.ALL)
const selectedSort = ref<CommentSearchSort>(CommentSearchSort.RELEVANCE)
const startDate = ref('')
const endDate = ref('')
const selectedStatuses = ref<CommentStatus[]>([])
const userFilter = ref('')
const searchInputRef = ref<HTMLInputElement>()
const isFocused = ref(false)

// 搜索历史和建议
const searchHistory = ref<SearchHistory[]>([])
const suggestions = ref<SearchSuggestion[]>([])
const searchStats = ref<SearchStats | null>(null)

// 防抖计时器
let debounceTimer: NodeJS.Timeout | null = null

// 搜索选项
const searchScopes = [
  { value: CommentSearchScope.ALL, label: '全部' },
  { value: CommentSearchScope.CONTENT, label: '内容' },
  { value: CommentSearchScope.USER_NAME, label: '用户名' },
  { value: CommentSearchScope.SNIPPET_TITLE, label: '代码片段' }
]

const sortOptions = [
  { value: CommentSearchSort.RELEVANCE, label: '相关度' },
  { value: CommentSearchSort.CREATED_AT_DESC, label: '最新发布' },
  { value: CommentSearchSort.CREATED_AT_ASC, label: '最早发布' },
  { value: CommentSearchSort.LIKE_COUNT_DESC, label: '最多点赞' },
  { value: CommentSearchSort.REPLY_COUNT_DESC, label: '最多回复' }
]

const commentStatuses = [
  { value: CommentStatus.NORMAL, label: '正常' },
  { value: CommentStatus.PENDING, label: '待审核' },
  { value: CommentStatus.HIDDEN, label: '已隐藏' },
  { value: CommentStatus.DELETED, label: '已删除' }
]

// 计算属性
const searchClasses = computed(() => {
  return [
    'comment-search',
    {
      'comment-search--loading': loading.value,
      'comment-search--focused': isFocused.value,
      'comment-search--disabled': props.disabled,
      'comment-search--has-filters': hasActiveFilters.value
    }
  ]
})

const canSearch = computed(() => {
  return searchKeyword.value.trim().length > 0 || hasActiveFilters.value
})

const hasActiveFilters = computed(() => {
  return selectedScope.value !== CommentSearchScope.ALL ||
         startDate.value ||
         endDate.value ||
         selectedStatuses.value.length > 0 ||
         selectedSort.value !== CommentSearchSort.RELEVANCE ||
         userFilter.value
})

const activeFilters = computed(() => {
  const filters = []
  
  if (selectedScope.value !== CommentSearchScope.ALL) {
    const scope = searchScopes.find(s => s.value === selectedScope.value)
    filters.push({ key: 'scope', label: `范围: ${scope?.label}` })
  }
  
  if (startDate.value) {
    filters.push({ key: 'startDate', label: `开始: ${startDate.value}` })
  }
  
  if (endDate.value) {
    filters.push({ key: 'endDate', label: `结束: ${endDate.value}` })
  }
  
  if (selectedStatuses.value.length > 0) {
    const statusLabels = selectedStatuses.value.map(status => {
      return commentStatuses.find(s => s.value === status)?.label
    }).join(', ')
    filters.push({ key: 'statuses', label: `状态: ${statusLabels}` })
  }
  
  if (selectedSort.value !== CommentSearchSort.RELEVANCE) {
    const sort = sortOptions.find(s => s.value === selectedSort.value)
    filters.push({ key: 'sort', label: `排序: ${sort?.label}` })
  }
  
  if (userFilter.value) {
    filters.push({ key: 'userFilter', label: `用户: ${userFilter.value}` })
  }
  
  return filters
})

const activeFiltersCount = computed(() => {
  return activeFilters.value.length
})

// 方法
function handleInput(event: Event) {
  const target = event.target as HTMLInputElement
  searchKeyword.value = target.value
  emit('update:modelValue', target.value)
  
  // 防抖搜索
  if (debounceTimer) {
    clearTimeout(debounceTimer)
  }
  
  debounceTimer = setTimeout(() => {
    if (searchKeyword.value.trim()) {
      handleSearch()
    }
  }, props.debounceTime)
}

function handleKeydown(event: KeyboardEvent) {
  // Enter 键搜索
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    handleSearch()
  }
  
  // Escape 键清空
  if (event.key === 'Escape') {
    handleClear()
  }
  
  // 快捷键支持
  if (props.enableHotkeys && event.ctrlKey) {
    switch (event.key) {
      case 'f':
        event.preventDefault()
        searchInputRef.value?.focus()
        break
      case 'k':
        event.preventDefault()
        handleClear()
        break
    }
  }
}

function handleFocus() {
  isFocused.value = true
  emit('focus')
  
  // 加载搜索历史
  loadSearchHistory()
  
  // 生成搜索建议
  if (searchKeyword.value.trim()) {
    generateSuggestions()
  }
}

function handleBlur() {
  isFocused.value = false
  emit('blur')
}

function handleSearch() {
  if (!canSearch.value || loading.value) return
  
  const searchData: SearchData = {
    keyword: searchKeyword.value.trim(),
    scope: selectedScope.value,
    startDate: startDate.value || undefined,
    endDate: endDate.value || undefined,
    statuses: selectedStatuses.value.length > 0 ? selectedStatuses.value : undefined,
    sort: selectedSort.value,
    userFilter: userFilter.value.trim() || undefined
  }
  
  loading.value = true
  emit('search', searchData)
  
  // 模拟搜索延迟
  setTimeout(() => {
    loading.value = false
    // 模拟搜索统计
    searchStats.value = {
      totalResults: Math.floor(Math.random() * 100),
      duration: Math.floor(Math.random() * 100) + 10,
      filters: activeFiltersCount.value
    }
  }, 500)
  
  // 添加到搜索历史
  addToSearchHistory(searchData)
}

function handleClear() {
  searchKeyword.value = ''
  startDate.value = ''
  endDate.value = ''
  selectedStatuses.value = []
  userFilter.value = ''
  selectedScope.value = CommentSearchScope.ALL
  selectedSort.value = CommentSearchSort.RELEVANCE
  
  emit('update:modelValue', '')
  emit('clear')
  
  searchInputRef.value?.focus()
}

function getScopeButtonClasses(scope: CommentSearchScope) {
  return [
    'option-btn',
    {
      'option-btn--active': selectedScope.value === scope
    }
  ]
}

function getStatusButtonClasses(status: CommentStatus) {
  return [
    'option-btn',
    {
      'option-btn--active': selectedStatuses.value.includes(status)
    }
  ]
}

function toggleStatus(status: CommentStatus) {
  const index = selectedStatuses.value.indexOf(status)
  if (index > -1) {
    selectedStatuses.value.splice(index, 1)
  } else {
    selectedStatuses.value.push(status)
  }
}

function removeFilter(filterKey: string) {
  switch (filterKey) {
    case 'scope':
      selectedScope.value = CommentSearchScope.ALL
      break
    case 'startDate':
      startDate.value = ''
      break
    case 'endDate':
      endDate.value = ''
      break
    case 'statuses':
      selectedStatuses.value = []
      break
    case 'sort':
      selectedSort.value = CommentSearchSort.RELEVANCE
      break
    case 'userFilter':
      userFilter.value = ''
      break
  }
}

function clearAllFilters() {
  handleClear()
}

function generateSuggestions() {
  const keyword = searchKeyword.value.trim().toLowerCase()
  if (!keyword) return
  
  // 模拟搜索建议
  const mockSuggestions: SearchSuggestion[] = [
    { id: '1', text: `包含 "${keyword}" 的评论`, type: '内容', keyword },
    { id: '2', text: `用户名包含 "${keyword}"`, type: '用户', keyword },
    { id: '3', text: `标题包含 "${keyword}" 的代码片段`, type: '代码片段', keyword },
    { id: '4', text: `"${keyword}" 的相关讨论`, type: '话题', keyword }
  ]
  
  suggestions.value = mockSuggestions
}

function applySuggestion(suggestion: SearchSuggestion) {
  searchKeyword.value = suggestion.keyword
  emit('update:modelValue', suggestion.keyword)
  emit('suggestion-select', suggestion)
  handleSearch()
}

function loadSearchHistory() {
  // 从本地存储加载搜索历史
  try {
    const history = localStorage.getItem('commentSearchHistory')
    if (history) {
      searchHistory.value = JSON.parse(history)
    }
  } catch (error) {
    console.error('Failed to load search history:', error)
  }
}

function addToSearchHistory(searchData: SearchData) {
  if (!searchData.keyword) return
  
  const historyItem: SearchHistory = {
    id: Date.now().toString(),
    keyword: searchData.keyword,
    timestamp: Date.now(),
    filters: {
      scope: searchData.scope,
      startDate: searchData.startDate,
      endDate: searchData.endDate,
      statuses: searchData.statuses || [],
      sort: searchData.sort,
      userFilter: searchData.userFilter
    }
  }
  
  // 避免重复
  const existingIndex = searchHistory.value.findIndex(h => h.keyword === searchData.keyword)
  if (existingIndex > -1) {
    searchHistory.value.splice(existingIndex, 1)
  }
  
  // 添加到开头
  searchHistory.value.unshift(historyItem)
  
  // 限制数量
  if (searchHistory.value.length > props.maxHistoryItems) {
    searchHistory.value = searchHistory.value.slice(0, props.maxHistoryItems)
  }
  
  // 保存到本地存储
  try {
    localStorage.setItem('commentSearchHistory', JSON.stringify(searchHistory.value))
  } catch (error) {
    console.error('Failed to save search history:', error)
  }
}

function applyHistory(history: SearchHistory) {
  searchKeyword.value = history.keyword
  emit('update:modelValue', history.keyword)
  
  if (history.filters) {
    selectedScope.value = history.filters.scope
    startDate.value = history.filters.startDate || ''
    endDate.value = history.filters.endDate || ''
    selectedStatuses.value = history.filters.statuses
    selectedSort.value = history.filters.sort
    userFilter.value = history.filters.userFilter || ''
  }
  
  emit('history-select', history)
  handleSearch()
}

function clearHistory() {
  searchHistory.value = []
  try {
    localStorage.removeItem('commentSearchHistory')
  } catch (error) {
    console.error('Failed to clear search history:', error)
  }
}

function formatTime(timestamp: number): string {
  const date = new Date(timestamp)
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

// 监听器
watch(() => props.modelValue, (newValue) => {
  searchKeyword.value = newValue
})

// 生命周期
onMounted(() => {
  if (props.autoFocus) {
    nextTick(() => {
      searchInputRef.value?.focus()
    })
  }
  
  // 加载搜索历史
  loadSearchHistory()
  
  // 全局快捷键
  if (props.enableHotkeys) {
    const handleGlobalKeydown = (event: KeyboardEvent) => {
      if (event.ctrlKey && event.key === 'f') {
        event.preventDefault()
        searchInputRef.value?.focus()
      }
    }
    
    document.addEventListener('keydown', handleGlobalKeydown)
    onUnmounted(() => {
      document.removeEventListener('keydown', handleGlobalKeydown)
    })
  }
})

// 暴露方法
defineExpose({
  focus: () => searchInputRef.value?.focus(),
  blur: () => searchInputRef.value?.blur(),
  clear: handleClear,
  search: handleSearch
})
</script>

<style scoped>
.comment-search {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
}

/* 搜索容器 */
.search-container {
  position: relative;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  background: white;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  overflow: hidden;
  transition: all 0.2s ease;
}

.search-input-wrapper:focus-within {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.comment-search--focused .search-input-wrapper {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.search-icon {
  position: absolute;
  left: 1rem;
  color: var(--gray-400);
  z-index: 1;
}

.search-input {
  flex: 1;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: none;
  background: transparent;
  color: var(--gray-900);
  font-size: 0.875rem;
  outline: none;
}

.search-input::placeholder {
  color: var(--gray-400);
}

.search-clear {
  position: absolute;
  right: 3rem;
  padding: 0.5rem;
  background: none;
  border: none;
  color: var(--gray-400);
  cursor: pointer;
  transition: all 0.2s ease;
  z-index: 1;
}

.search-clear:hover {
  color: var(--gray-600);
}

.search-submit {
  position: absolute;
  right: 0;
  top: 0;
  bottom: 0;
  padding: 0 1rem;
  background: var(--primary-500);
  border: none;
  color: white;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.search-submit:hover:not(:disabled) {
  background: var(--primary-600);
}

.search-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 高级搜索 */
.advanced-search {
  background: var(--gray-50);
  border-radius: 0.5rem;
  border: 1px solid var(--gray-200);
}

.advanced-header {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--gray-200);
}

.advanced-toggle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: none;
  border: none;
  color: var(--gray-700);
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
}

.advanced-options {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.option-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.option-label {
  font-size: 0.813rem;
  font-weight: 600;
  color: var(--gray-700);
}

.option-buttons {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.option-btn {
  padding: 0.375rem 0.75rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  background: white;
  color: var(--gray-700);
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.813rem;
}

.option-btn:hover {
  background: var(--gray-50);
  border-color: var(--gray-400);
}

.option-btn--active {
  background: var(--primary-500);
  color: white;
  border-color: var(--primary-500);
}

.date-range {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.date-input {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  font-size: 0.813rem;
  background: white;
  color: var(--gray-900);
}

.date-input:focus {
  outline: none;
  border-color: var(--primary-500);
}

.date-separator {
  color: var(--gray-500);
  font-size: 0.813rem;
}

.sort-select {
  padding: 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  font-size: 0.813rem;
  background: white;
  color: var(--gray-900);
  cursor: pointer;
}

.sort-select:focus {
  outline: none;
  border-color: var(--primary-500);
}

.user-input {
  padding: 0.5rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  font-size: 0.813rem;
  background: white;
  color: var(--gray-900);
}

.user-input:focus {
  outline: none;
  border-color: var(--primary-500);
}

/* 搜索建议 */
.search-suggestions {
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: 0.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.suggestions-header {
  padding: 0.75rem 1rem;
  background: var(--gray-50);
  border-bottom: 1px solid var(--gray-200);
  font-size: 0.813rem;
  font-weight: 600;
  color: var(--gray-700);
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.suggestions-list {
  max-height: 200px;
  overflow-y: auto;
}

.suggestion-item {
  width: 100%;
  padding: 0.75rem 1rem;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.suggestion-item:hover {
  background: var(--gray-50);
}

.suggestion-text {
  color: var(--gray-900);
  font-size: 0.875rem;
}

.suggestion-meta {
  color: var(--gray-500);
  font-size: 0.75rem;
  background: var(--gray-100);
  padding: 0.125rem 0.375rem;
  border-radius: 0.25rem;
}

/* 搜索历史 */
.search-history {
  background: white;
  border: 1px solid var(--gray-200);
  border-radius: 0.5rem;
  overflow: hidden;
}

.history-header {
  padding: 0.75rem 1rem;
  background: var(--gray-50);
  border-bottom: 1px solid var(--gray-200);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.history-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.813rem;
  font-weight: 600;
  color: var(--gray-700);
}

.history-clear {
  background: none;
  border: none;
  color: var(--gray-500);
  cursor: pointer;
  font-size: 0.75rem;
  transition: all 0.2s ease;
}

.history-clear:hover {
  color: var(--gray-700);
}

.history-list {
  max-height: 150px;
  overflow-y: auto;
}

.history-item {
  width: 100%;
  padding: 0.75rem 1rem;
  background: none;
  border: none;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.history-item:hover {
  background: var(--gray-50);
}

.history-item i {
  color: var(--gray-400);
}

.history-time {
  margin-left: auto;
  color: var(--gray-500);
  font-size: 0.75rem;
}

/* 活跃过滤器 */
.active-filters {
  background: var(--gray-50);
  border-radius: 0.5rem;
  border: 1px solid var(--gray-200);
  padding: 1rem;
}

.filters-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.filters-header span {
  font-size: 0.813rem;
  font-weight: 600;
  color: var(--gray-700);
}

.filters-clear {
  background: none;
  border: none;
  color: var(--gray-500);
  cursor: pointer;
  font-size: 0.75rem;
  transition: all 0.2s ease;
}

.filters-clear:hover {
  color: var(--gray-700);
}

.filters-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.filter-tag {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.375rem 0.75rem;
  background: white;
  border: 1px solid var(--gray-300);
  border-radius: 0.375rem;
  font-size: 0.75rem;
  color: var(--gray-700);
}

.filter-label {
  white-space: nowrap;
}

.filter-remove {
  background: none;
  border: none;
  color: var(--gray-500);
  cursor: pointer;
  font-size: 0.625rem;
  transition: all 0.2s ease;
}

.filter-remove:hover {
  color: var(--gray-700);
}

/* 搜索统计 */
.search-stats {
  background: var(--gray-50);
  border-radius: 0.5rem;
  border: 1px solid var(--gray-200);
  padding: 1rem;
}

.stats-content {
  display: flex;
  gap: 2rem;
  justify-content: space-around;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
}

.stat-item i {
  color: var(--gray-500);
  font-size: 1rem;
}

.stat-label {
  font-size: 0.75rem;
  color: var(--gray-600);
}

.stat-value {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--gray-900);
}

/* 状态样式 */
.comment-search--loading {
  opacity: 0.7;
  pointer-events: none;
}

.comment-search--disabled {
  opacity: 0.5;
  pointer-events: none;
}

.comment-search--disabled .search-input {
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .advanced-options {
    padding: 0.75rem;
  }
  
  .option-buttons {
    flex-direction: column;
  }
  
  .date-range {
    flex-direction: column;
    align-items: stretch;
  }
  
  .stats-content {
    flex-direction: column;
    gap: 1rem;
  }
  
  .filters-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
}

@media (max-width: 480px) {
  .search-input {
    padding: 0.5rem 1rem 0.5rem 2.5rem;
    font-size: 0.813rem;
  }
  
  .search-icon {
    left: 0.75rem;
  }
  
  .search-submit {
    padding: 0 0.75rem;
  }
  
  .option-buttons {
    grid-template-columns: 1fr;
  }
  
  .filters-list {
    flex-direction: column;
  }
  
  .filter-tag {
    justify-content: space-between;
  }
}

/* 深色模式 */
:deep(.dark) .search-input-wrapper {
  background: var(--gray-800);
  border-color: var(--gray-600);
}

:deep(.dark) .search-input-wrapper:focus-within {
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
}

:deep(.dark) .search-icon {
  color: var(--gray-400);
}

:deep(.dark) .search-input {
  color: var(--gray-200);
}

:deep(.dark) .search-input::placeholder {
  color: var(--gray-500);
}

:deep(.dark) .search-clear {
  color: var(--gray-400);
}

:deep(.dark) .search-clear:hover {
  color: var(--gray-200);
}

:deep(.dark) .search-submit {
  background: var(--primary-600);
}

:deep(.dark) .search-submit:hover:not(:disabled) {
  background: var(--primary-700);
}

:deep(.dark) .advanced-search {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .advanced-header {
  background: var(--gray-700);
  border-color: var(--gray-600);
}

:deep(.dark) .advanced-toggle {
  color: var(--gray-300);
}

:deep(.dark) .option-label {
  color: var(--gray-300);
}

:deep(.dark) .option-btn {
  background: var(--gray-700);
  border-color: var(--gray-600);
  color: var(--gray-300);
}

:deep(.dark) .option-btn:hover {
  background: var(--gray-600);
  border-color: var(--gray-500);
}

:deep(.dark) .option-btn--active {
  background: var(--primary-600);
  color: white;
  border-color: var(--primary-600);
}

:deep(.dark) .date-input,
:deep(.dark) .sort-select,
:deep(.dark) .user-input {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-200);
}

:deep(.dark) .date-input:focus,
:deep(.dark) .sort-select:focus,
:deep(.dark) .user-input:focus {
  border-color: var(--primary-500);
}

:deep(.dark) .search-suggestions,
:deep(.dark) .search-history {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .suggestions-header,
:deep(.dark) .history-header {
  background: var(--gray-700);
  border-color: var(--gray-600);
}

:deep(.dark) .suggestions-header,
:deep(.dark) .history-title {
  color: var(--gray-200);
}

:deep(.dark) .suggestion-item,
:deep(.dark) .history-item {
  color: var(--gray-200);
}

:deep(.dark) .suggestion-item:hover,
:deep(.dark) .history-item:hover {
  background: var(--gray-700);
}

:deep(.dark) .suggestion-meta {
  color: var(--gray-400);
  background: var(--gray-600);
}

:deep(.dark) .history-item i {
  color: var(--gray-500);
}

:deep(.dark) .history-time {
  color: var(--gray-400);
}

:deep(.dark) .history-clear:hover {
  color: var(--gray-300);
}

:deep(.dark) .active-filters {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .filters-header span {
  color: var(--gray-200);
}

:deep(.dark) .filter-tag {
  background: var(--gray-700);
  border-color: var(--gray-600);
  color: var(--gray-300);
}

:deep(.dark) .filter-remove {
  color: var(--gray-400);
}

:deep(.dark) .filter-remove:hover {
  color: var(--gray-200);
}

:deep(.dark) .search-stats {
  background: var(--gray-800);
  border-color: var(--gray-700);
}

:deep(.dark) .stat-item i {
  color: var(--gray-400);
}

:deep(.dark) .stat-label {
  color: var(--gray-400);
}

:deep(.dark) .stat-value {
  color: var(--gray-200);
}

/* 无障碍性 */
@media (prefers-reduced-motion: reduce) {
  .search-input-wrapper,
  .option-btn,
  .search-clear,
  .search-submit,
  .advanced-toggle,
  .history-clear,
  .filters-clear,
  .filter-remove,
  .suggestion-item,
  .history-item {
    transition: none;
  }
}
</style>