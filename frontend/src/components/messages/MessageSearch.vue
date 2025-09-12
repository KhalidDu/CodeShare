<template>
  <div class="message-search-container">
    <!-- 搜索输入区域 -->
    <div class="search-input-area">
      <div class="search-input-wrapper">
        <div class="search-input-container">
          <i class="fas fa-search search-icon"></i>
          <input
            ref="searchInputRef"
            v-model="searchQuery"
            type="text"
            :placeholder="placeholder"
            class="search-input"
            @focus="handleFocus"
            @blur="handleBlur"
            @input="handleInput"
            @keyup.enter="handleEnter"
            @keyup.up="handleUpKey"
            @keyup.down="handleDownKey"
            @keyup.esc="handleEscape"
          />
          <button
            v-if="searchQuery"
            @click="clearSearch"
            class="clear-search-btn"
          >
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <!-- 搜索选项 -->
        <div class="search-options">
          <button
            @click="toggleAdvancedSearch"
            class="option-btn"
            :class="{ active: showAdvancedSearch }"
          >
            <i class="fas fa-cog"></i>
            高级搜索
          </button>
          <button
            @click="toggleSearchHistory"
            class="option-btn"
            :class="{ active: showSearchHistory }"
          >
            <i class="fas fa-history"></i>
            历史
          </button>
        </div>
      </div>

      <!-- 搜索建议 -->
      <div v-if="showSuggestions && suggestions.length > 0" class="search-suggestions">
        <div class="suggestions-header">
          <span>搜索建议</span>
          <button
            @click="clearSuggestions"
            class="clear-suggestions"
          >
            清空
          </button>
        </div>
        <div class="suggestions-list">
          <div
            v-for="(suggestion, index) in suggestions"
            :key="suggestion"
            @click="selectSuggestion(suggestion)"
            @mouseenter="selectedSuggestionIndex = index"
            class="suggestion-item"
            :class="{ active: selectedSuggestionIndex === index }"
          >
            <i class="fas fa-search suggestion-icon"></i>
            <span class="suggestion-text">{{ suggestion }}</span>
          </div>
        </div>
      </div>

      <!-- 搜索历史 -->
      <div v-if="showSearchHistory && searchHistory.length > 0" class="search-history">
        <div class="history-header">
          <span>搜索历史</span>
          <button
            @click="clearHistory"
            class="clear-history"
          >
            清空历史
          </button>
        </div>
        <div class="history-list">
          <div
            v-for="(history, index) in searchHistory"
            :key="history.id || history"
            @click="selectHistory(history)"
            @mouseenter="selectedHistoryIndex = index"
            class="history-item"
            :class="{ active: selectedHistoryIndex === index }"
          >
            <i class="fas fa-clock history-icon"></i>
            <span class="history-text">{{ history.query || history }}</span>
            <span v-if="history.timestamp" class="history-time">
              {{ formatTime(history.timestamp) }}
            </span>
            <button
              @click.stop="removeHistory(history)"
              class="remove-history"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 高级搜索面板 -->
    <div v-if="showAdvancedSearch" class="advanced-search-panel">
      <div class="panel-header">
        <h3>高级搜索</h3>
        <button @click="toggleAdvancedSearch" class="close-panel">
          <i class="fas fa-times"></i>
        </button>
      </div>
      
      <div class="panel-content">
        <div class="search-fields">
          <!-- 搜索范围 -->
          <div class="field-group">
            <label class="field-label">搜索范围</label>
            <div class="checkbox-group">
              <label v-for="scope in searchScopes" :key="scope.value" class="checkbox-label">
                <input
                  v-model="advancedFilters.scopes"
                  type="checkbox"
                  :value="scope.value"
                  class="checkbox-input"
                />
                <span class="checkbox-text">{{ scope.label }}</span>
              </label>
            </div>
          </div>

          <!-- 消息类型 -->
          <div class="field-group">
            <label class="field-label">消息类型</label>
            <select
              v-model="advancedFilters.messageType"
              class="select-input"
            >
              <option value="">全部类型</option>
              <option
                v-for="type in messageTypes"
                :key="type"
                :value="type"
              >
                {{ getMessageTypeLabel(type) }}
              </option>
            </select>
          </div>

          <!-- 发送者 -->
          <div class="field-group">
            <label class="field-label">发送者</label>
            <input
              v-model="advancedFilters.sender"
              type="text"
              placeholder="输入发送者名称或ID"
              class="text-input"
            />
          </div>

          <!-- 接收者 -->
          <div class="field-group">
            <label class="field-label">接收者</label>
            <input
              v-model="advancedFilters.receiver"
              type="text"
              placeholder="输入接收者名称或ID"
              class="text-input"
            />
          </div>

          <!-- 时间范围 -->
          <div class="field-group">
            <label class="field-label">时间范围</label>
            <div class="date-range">
              <input
                v-model="advancedFilters.startDate"
                type="date"
                class="date-input"
              />
              <span class="date-separator">至</span>
              <input
                v-model="advancedFilters.endDate"
                type="date"
                class="date-input"
              />
            </div>
          </div>

          <!-- 状态筛选 -->
          <div class="field-group">
            <label class="field-label">消息状态</label>
            <div class="checkbox-group">
              <label v-for="status in messageStatuses" :key="status" class="checkbox-label">
                <input
                  v-model="advancedFilters.statuses"
                  type="checkbox"
                  :value="status"
                  class="checkbox-input"
                />
                <span class="checkbox-text">{{ getMessageStatusLabel(status) }}</span>
              </label>
            </div>
          </div>

          <!-- 附件筛选 -->
          <div class="field-group">
            <label class="field-label">附件</label>
            <div class="radio-group">
              <label class="radio-label">
                <input
                  v-model="advancedFilters.hasAttachments"
                  type="radio"
                  value=""
                  class="radio-input"
                />
                <span class="radio-text">全部</span>
              </label>
              <label class="radio-label">
                <input
                  v-model="advancedFilters.hasAttachments"
                  type="radio"
                  value="true"
                  class="radio-input"
                />
                <span class="radio-text">包含附件</span>
              </label>
              <label class="radio-label">
                <input
                  v-model="advancedFilters.hasAttachments"
                  type="radio"
                  value="false"
                  class="radio-input"
                />
                <span class="radio-text">无附件</span>
              </label>
            </div>
          </div>

          <!-- 排序选项 -->
          <div class="field-group">
            <label class="field-label">排序方式</label>
            <select
              v-model="advancedFilters.sortBy"
              class="select-input"
            >
              <option
                v-for="sort in sortOptions"
                :key="sort.value"
                :value="sort.value"
              >
                {{ sort.label }}
              </option>
            </select>
          </div>
        </div>

        <div class="panel-actions">
          <button
            @click="resetAdvancedFilters"
            class="action-btn reset"
          >
            重置
          </button>
          <button
            @click="applyAdvancedSearch"
            class="action-btn apply"
          >
            应用搜索
          </button>
        </div>
      </div>
    </div>

    <!-- 搜索结果 -->
    <div v-if="searchResults && searchResults.messages.length > 0" class="search-results">
      <div class="results-header">
        <div class="results-info">
          <span class="results-count">
            找到 {{ searchResults.totalCount }} 条结果
          </span>
          <span v-if="searchResults.searchTime" class="search-time">
            (耗时 {{ searchResults.searchTime }}ms)
          </span>
        </div>
        <div class="results-actions">
          <button
            @click="exportResults"
            class="action-btn"
            title="导出结果"
          >
            <i class="fas fa-download"></i>
            导出
          </button>
          <button
            @click="saveSearch"
            class="action-btn"
            title="保存搜索"
          >
            <i class="fas fa-save"></i>
            保存
          </button>
        </div>
      </div>

      <div class="results-list">
        <div
          v-for="message in searchResults.messages"
          :key="message.id"
          class="result-item"
          @click="selectResult(message)"
        >
          <div class="result-content">
            <div class="result-header">
              <span class="result-sender">{{ message.senderName }}</span>
              <span class="result-time">{{ formatTime(message.createdAt) }}</span>
            </div>
            <div class="result-body">
              <div v-if="message.subject" class="result-subject">
                {{ highlightText(message.subject, searchResults.highlightedTerms) }}
              </div>
              <div class="result-text">
                {{ highlightText(message.content, searchResults.highlightedTerms) }}
              </div>
            </div>
            <div class="result-meta">
              <span class="result-type">{{ getMessageTypeLabel(message.messageType) }}</span>
              <span v-if="message.attachments.length > 0" class="result-attachments">
                <i class="fas fa-paperclip"></i>
                {{ message.attachments.length }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 分页 -->
      <div v-if="totalPages > 1" class="results-pagination">
        <div class="pagination-info">
          第 {{ currentPage }} 页，共 {{ totalPages }} 页
        </div>
        <div class="pagination-controls">
          <button
            @click="goToPage(currentPage - 1)"
            :disabled="currentPage === 1"
            class="pagination-btn"
          >
            <i class="fas fa-chevron-left"></i>
          </button>
          <div class="pagination-pages">
            <button
              v-for="page in visiblePages"
              :key="page"
              @click="goToPage(page)"
              :class="{ active: page === currentPage }"
              class="page-btn"
            >
              {{ page }}
            </button>
          </div>
          <button
            @click="goToPage(currentPage + 1)"
            :disabled="currentPage === totalPages"
            class="pagination-btn"
          >
            <i class="fas fa-chevron-right"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- 无结果状态 -->
    <div v-else-if="hasSearched && searchResults?.messages.length === 0" class="no-results">
      <div class="no-results-icon">
        <i class="fas fa-search"></i>
      </div>
      <h3>未找到匹配的消息</h3>
      <p>尝试使用不同的关键词或调整搜索条件</p>
      <button @click="clearSearch" class="retry-btn">
        重新搜索
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { messageService } from '@/services/messageService'
import type {
  MessageSearchRequest,
  MessageSearchResult,
  Message,
  MessageType,
  MessageStatus,
  MessageSearchSort
} from '@/types/message'

// 定义Props
interface Props {
  placeholder?: string
  initialQuery?: string
  enableSuggestions?: boolean
  enableHistory?: boolean
  enableAdvancedSearch?: boolean
  maxSuggestions?: number
  maxHistoryItems?: number
  autoSearch?: boolean
  searchDelay?: number
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '搜索消息...',
  initialQuery: '',
  enableSuggestions: true,
  enableHistory: true,
  enableAdvancedSearch: true,
  maxSuggestions: 5,
  maxHistoryItems: 10,
  autoSearch: true,
  searchDelay: 300
})

// 定义Emits
interface Emits {
  (e: 'search', request: MessageSearchRequest): void
  (e: 'result-select', message: Message): void
  (e: 'query-change', query: string): void
  (e: 'advanced-search', filters: any): void
}

const emit = defineEmits<Emits>()

// 响应式状态
const searchInputRef = ref<HTMLInputElement>()
const searchQuery = ref(props.initialQuery)
const showSuggestions = ref(false)
const showSearchHistory = ref(false)
const showAdvancedSearch = ref(false)
const selectedSuggestionIndex = ref(-1)
const selectedHistoryIndex = ref(-1)

const suggestions = ref<string[]>([])
const searchHistory = ref<Array<{ query: string; timestamp: string }>>([])
const searchResults = ref<MessageSearchResult | null>(null)

const currentPage = ref(1)
const pageSize = 10
const hasSearched = ref(false)

// 高级搜索过滤器
const advancedFilters = ref({
  scopes: ['SUBJECT', 'CONTENT'] as string[],
  messageType: '',
  sender: '',
  receiver: '',
  startDate: '',
  endDate: '',
  statuses: [] as string[],
  hasAttachments: '',
  sortBy: 'RELEVANCE' as MessageSearchSort
})

// 搜索选项
const searchScopes = [
  { value: 'SUBJECT', label: '主题' },
  { value: 'CONTENT', label: '内容' },
  { value: 'TAG', label: '标签' },
  { value: 'SENDER', label: '发送者' },
  { value: 'RECEIVER', label: '接收者' }
]

const messageTypes = Object.values(MessageType)

const messageStatuses = Object.values(MessageStatus)

const sortOptions = [
  { value: 'RELEVANCE', label: '相关性' },
  { value: 'CREATED_AT_DESC', label: '时间倒序' },
  { value: 'CREATED_AT_ASC', label: '时间正序' },
  { value: 'PRIORITY_DESC', label: '优先级倒序' },
  { value: 'UNREAD_FIRST', label: '未读优先' }
]

// 计算属性
const totalPages = computed(() => {
  if (!searchResults.value) return 0
  return Math.ceil(searchResults.value.totalCount / pageSize)
})

const visiblePages = computed(() => {
  const pages: number[] = []
  const maxVisible = 5
  let start = Math.max(1, currentPage.value - Math.floor(maxVisible / 2))
  let end = Math.min(totalPages.value, start + maxVisible - 1)
  
  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1)
  }
  
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  
  return pages
})

// 防抖搜索
let searchTimeout: NodeJS.Timeout

const debouncedSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchQuery.value.trim()) {
      performSearch()
    }
  }, props.searchDelay)
}

// 方法定义
const handleFocus = () => {
  if (props.enableHistory && searchHistory.value.length > 0) {
    showSearchHistory.value = true
  }
}

const handleBlur = () => {
  setTimeout(() => {
    showSuggestions.value = false
    showSearchHistory.value = false
  }, 200)
}

const handleInput = () => {
  emit('query-change', searchQuery.value)
  
  if (props.enableSuggestions && searchQuery.value.trim()) {
    fetchSuggestions()
  } else {
    suggestions.value = []
    showSuggestions.value = false
  }
  
  if (props.autoSearch && searchQuery.value.trim()) {
    debouncedSearch()
  }
}

const handleEnter = () => {
  if (showSuggestions.value && selectedSuggestionIndex >= 0) {
    selectSuggestion(suggestions.value[selectedSuggestionIndex])
  } else if (showSearchHistory.value && selectedHistoryIndex >= 0) {
    selectHistory(searchHistory.value[selectedHistoryIndex])
  } else {
    performSearch()
  }
}

const handleUpKey = () => {
  if (showSuggestions.value && suggestions.value.length > 0) {
    selectedSuggestionIndex.value = selectedSuggestionIndex.value <= 0 
      ? suggestions.value.length - 1 
      : selectedSuggestionIndex.value - 1
  } else if (showSearchHistory.value && searchHistory.value.length > 0) {
    selectedHistoryIndex.value = selectedHistoryIndex.value <= 0 
      ? searchHistory.value.length - 1 
      : selectedHistoryIndex.value - 1
  }
}

const handleDownKey = () => {
  if (showSuggestions.value && suggestions.value.length > 0) {
    selectedSuggestionIndex.value = selectedSuggestionIndex.value >= suggestions.value.length - 1 
      ? 0 
      : selectedSuggestionIndex.value + 1
  } else if (showSearchHistory.value && searchHistory.value.length > 0) {
    selectedHistoryIndex.value = selectedHistoryIndex.value >= searchHistory.value.length - 1 
      ? 0 
      : selectedHistoryIndex.value + 1
  }
}

const handleEscape = () => {
  showSuggestions.value = false
  showSearchHistory.value = false
  selectedSuggestionIndex.value = -1
  selectedHistoryIndex.value = -1
}

const clearSearch = () => {
  searchQuery.value = ''
  suggestions.value = []
  searchResults.value = null
  hasSearched.value = false
  currentPage.value = 1
  showSuggestions.value = false
  showSearchHistory.value = false
  emit('query-change', '')
}

const clearSuggestions = () => {
  suggestions.value = []
  showSuggestions.value = false
}

const clearHistory = () => {
  searchHistory.value = []
  showSearchHistory.value = false
}

const removeHistory = (history: any) => {
  const index = searchHistory.value.indexOf(history)
  if (index > -1) {
    searchHistory.value.splice(index, 1)
  }
}

const toggleAdvancedSearch = () => {
  showAdvancedSearch.value = !showAdvancedSearch.value
}

const toggleSearchHistory = () => {
  showSearchHistory.value = !showSearchHistory.value
  showSuggestions.value = false
}

const selectSuggestion = (suggestion: string) => {
  searchQuery.value = suggestion
  showSuggestions.value = false
  selectedSuggestionIndex.value = -1
  performSearch()
}

const selectHistory = (history: any) => {
  searchQuery.value = history.query || history
  showSearchHistory.value = false
  selectedHistoryIndex.value = -1
  performSearch()
}

const selectResult = (message: Message) => {
  emit('result-select', message)
}

const fetchSuggestions = async () => {
  try {
    const response = await messageService.getSearchSuggestions(searchQuery.value)
    suggestions.value = response.slice(0, props.maxSuggestions)
    showSuggestions.value = true
    selectedSuggestionIndex.value = -1
  } catch (error) {
    console.error('获取搜索建议失败:', error)
  }
}

const performSearch = async () => {
  if (!searchQuery.value.trim()) return
  
  try {
    hasSearched.value = true
    showSuggestions.value = false
    showSearchHistory.value = false
    
    const request: MessageSearchRequest = {
      keyword: searchQuery.value.trim(),
      searchScope: getSearchScope(),
      sortBy: advancedFilters.value.sortBy,
      page: currentPage.value,
      pageSize: pageSize,
      highlightMatches: true
    }
    
    // 添加高级筛选条件
    if (advancedFilters.value.messageType) {
      request.messageType = advancedFilters.value.messageType as any
    }
    
    if (advancedFilters.value.sender) {
      request.senderId = advancedFilters.value.sender
    }
    
    if (advancedFilters.value.receiver) {
      request.receiverId = advancedFilters.value.receiver
    }
    
    if (advancedFilters.value.startDate) {
      request.startDate = advancedFilters.value.startDate
    }
    
    if (advancedFilters.value.endDate) {
      request.endDate = advancedFilters.value.endDate
    }
    
    if (advancedFilters.value.statuses.length > 0) {
      request.status = advancedFilters.value.statuses[0] as any
    }
    
    if (advancedFilters.value.hasAttachments !== '') {
      request.hasAttachments = advancedFilters.value.hasAttachments === 'true'
    }
    
    const result = await messageService.searchMessages(request)
    searchResults.value = result
    
    // 添加到搜索历史
    addToHistory(searchQuery.value.trim())
    
    emit('search', request)
  } catch (error) {
    console.error('搜索失败:', error)
  }
}

const applyAdvancedSearch = () => {
  currentPage.value = 1
  performSearch()
}

const resetAdvancedFilters = () => {
  advancedFilters.value = {
    scopes: ['SUBJECT', 'CONTENT'],
    messageType: '',
    sender: '',
    receiver: '',
    startDate: '',
    endDate: '',
    statuses: [],
    hasAttachments: '',
    sortBy: 'RELEVANCE'
  }
}

const exportResults = async () => {
  if (!searchResults.value) return
  
  try {
    const options = {
      format: 'EXCEL' as any,
      includeAttachments: true,
      includeReplies: true
    }
    
    const blob = await messageService.exportMessages(options)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `搜索结果_${new Date().toISOString().split('T')[0]}.xlsx`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  } catch (error) {
    console.error('导出结果失败:', error)
  }
}

const saveSearch = async () => {
  if (!searchQuery.value.trim()) return
  
  try {
    await messageService.saveSearchHistory(searchQuery.value.trim())
    // 显示保存成功提示
  } catch (error) {
    console.error('保存搜索历史失败:', error)
  }
}

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    performSearch()
  }
}

const addToHistory = (query: string) => {
  const existing = searchHistory.value.find(h => h.query === query)
  if (existing) {
    existing.timestamp = new Date().toISOString()
  } else {
    searchHistory.value.unshift({
      query,
      timestamp: new Date().toISOString()
    })
    
    // 限制历史记录数量
    if (searchHistory.value.length > props.maxHistoryItems) {
      searchHistory.value = searchHistory.value.slice(0, props.maxHistoryItems)
    }
  }
}

const getSearchScope = () => {
  if (advancedFilters.value.scopes.length === 0) return 'ALL'
  if (advancedFilters.value.scopes.length === 1) return advancedFilters.value.scopes[0] as any
  return 'SUBJECT_AND_CONTENT' as any
}

const getMessageTypeLabel = (type: MessageType) => {
  const labels = {
    [MessageType.USER]: '用户消息',
    [MessageType.SYSTEM]: '系统消息',
    [MessageType.NOTIFICATION]: '通知消息',
    [MessageType.BROADCAST]: '广播消息',
    [MessageType.AUTO_REPLY]: '自动回复'
  }
  return labels[type] || type
}

const getMessageStatusLabel = (status: MessageStatus) => {
  const labels = {
    [MessageStatus.DRAFT]: '草稿',
    [MessageStatus.SENT]: '已发送',
    [MessageStatus.DELIVERED]: '已送达',
    [MessageStatus.READ]: '已读',
    [MessageStatus.REPLIED]: '已回复',
    [MessageStatus.FORWARDED]: '已转发',
    [MessageStatus.DELETED]: '已删除',
    [MessageStatus.FAILED]: '发送失败',
    [MessageStatus.EXPIRED]: '已过期'
  }
  return labels[status] || status
}

const formatTime = (timeString: string) => {
  const date = new Date(timeString)
  return date.toLocaleString('zh-CN')
}

const highlightText = (text: string, terms: string[]) => {
  if (!terms.length) return text
  
  let highlightedText = text
  terms.forEach(term => {
    if (term) {
      const regex = new RegExp(`(${term})`, 'gi')
      highlightedText = highlightedText.replace(regex, '<mark>$1</mark>')
    }
  })
  
  return highlightedText
}

// 监听器
watch(() => props.initialQuery, (newQuery) => {
  searchQuery.value = newQuery
  if (newQuery) {
    performSearch()
  }
})

// 生命周期
onMounted(async () => {
  // 加载搜索历史
  try {
    searchHistory.value = await messageService.getSearchHistory()
  } catch (error) {
    console.error('加载搜索历史失败:', error)
  }
  
  // 如果有初始查询，执行搜索
  if (props.initialQuery) {
    performSearch()
  }
})

// 暴露方法
defineExpose({
  searchQuery,
  searchResults,
  showAdvancedSearch,
  advancedFilters,
  clearSearch,
  performSearch,
  toggleAdvancedSearch,
  resetAdvancedFilters
})
</script>

<style scoped>
.message-search-container {
  @apply space-y-4;
}

.search-input-area {
  @apply relative;
}

.search-input-wrapper {
  @apply space-y-2;
}

.search-input-container {
  @apply relative;
}

.search-icon {
  @apply absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400;
}

.search-input {
  @apply w-full pl-10 pr-10 py-3 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.clear-search-btn {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 
         hover:text-gray-600 dark:hover:text-gray-300;
}

.search-options {
  @apply flex items-center space-x-2;
}

.option-btn {
  @apply px-3 py-1 text-sm border border-gray-300 dark:border-gray-600 
         rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 
         dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.option-btn.active {
  @apply bg-blue-50 dark:bg-blue-900 border-blue-500 text-blue-700 dark:text-blue-300;
}

.search-suggestions,
.search-history {
  @apply absolute top-full left-0 right-0 mt-1 bg-white dark:bg-gray-800 
         border border-gray-300 dark:border-gray-600 rounded-lg shadow-lg z-10;
}

.suggestions-header,
.history-header {
  @apply flex items-center justify-between p-3 border-b border-gray-200 dark:border-gray-700;
}

.clear-suggestions,
.clear-history {
  @apply text-sm text-red-600 hover:text-red-700 dark:text-red-400 
         dark:hover:text-red-300;
}

.suggestions-list,
.history-list {
  @apply max-h-60 overflow-y-auto;
}

.suggestion-item,
.history-item {
  @apply flex items-center space-x-3 p-3 hover:bg-gray-50 dark:hover:bg-gray-700 
         cursor-pointer;
}

.suggestion-item.active,
.history-item.active {
  @apply bg-blue-50 dark:bg-blue-900;
}

.suggestion-icon,
.history-icon {
  @apply text-gray-400 dark:text-gray-600;
}

.suggestion-text,
.history-text {
  @apply flex-1 text-gray-900 dark:text-white;
}

.history-time {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.remove-history {
  @apply p-1 text-gray-400 hover:text-red-500 rounded-full hover:bg-gray-200 
         dark:hover:bg-gray-600;
}

.advanced-search-panel {
  @apply bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 
         rounded-lg shadow-lg;
}

.panel-header {
  @apply flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700;
}

.panel-header h3 {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.close-panel {
  @apply p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 
         rounded-full hover:bg-gray-100 dark:hover:bg-gray-700;
}

.panel-content {
  @apply p-4;
}

.search-fields {
  @apply grid grid-cols-1 md:grid-cols-2 gap-4;
}

.field-group {
  @apply space-y-2;
}

.field-label {
  @apply block text-sm font-medium text-gray-700 dark:text-gray-300;
}

.checkbox-group,
.radio-group {
  @apply space-y-2;
}

.checkbox-label,
.radio-label {
  @apply flex items-center space-x-2 cursor-pointer;
}

.checkbox-input,
.radio-input {
  @apply w-4 h-4 text-blue-600 border-gray-300 rounded 
         focus:ring-blue-500;
}

.checkbox-text,
.radio-text {
  @apply text-sm text-gray-700 dark:text-gray-300;
}

.select-input,
.text-input {
  @apply w-full px-3 py-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.date-range {
  @apply flex items-center space-x-2;
}

.date-input {
  @apply flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 
         rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.date-separator {
  @apply text-gray-500 dark:text-gray-400;
}

.panel-actions {
  @apply flex justify-end space-x-3 mt-6;
}

.action-btn {
  @apply px-4 py-2 rounded-lg font-medium focus:outline-none focus:ring-2 
         focus:ring-offset-2 transition-colors duration-200;
}

.action-btn.reset {
  @apply bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 
         hover:bg-gray-300 dark:hover:bg-gray-600 focus:ring-gray-500;
}

.action-btn.apply {
  @apply bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500;
}

.search-results {
  @apply space-y-4;
}

.results-header {
  @apply flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.results-info {
  @apply flex items-center space-x-2;
}

.results-count {
  @apply font-medium text-gray-900 dark:text-white;
}

.search-time {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.results-actions {
  @apply flex items-center space-x-2;
}

.results-list {
  @apply space-y-2;
}

.result-item {
  @apply p-4 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 
         rounded-lg hover:shadow-md cursor-pointer transition-all duration-200;
}

.result-content {
  @apply space-y-2;
}

.result-header {
  @apply flex items-center justify-between;
}

.result-sender {
  @apply font-medium text-gray-900 dark:text-white;
}

.result-time {
  @apply text-sm text-gray-500 dark:text-gray-400;
}

.result-body {
  @apply space-y-1;
}

.result-subject {
  @apply font-medium text-gray-900 dark:text-white;
}

.result-text {
  @apply text-sm text-gray-700 dark:text-gray-300 line-clamp-2;
}

.result-meta {
  @apply flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400;
}

.result-type {
  @apply px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded-full;
}

.result-attachments {
  @apply flex items-center space-x-1;
}

.results-pagination {
  @apply flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg;
}

.pagination-info {
  @apply text-sm text-gray-600 dark:text-gray-400;
}

.pagination-controls {
  @apply flex items-center space-x-2;
}

.pagination-btn {
  @apply p-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.pagination-btn:disabled {
  @apply opacity-50 cursor-not-allowed hover:bg-transparent;
}

.pagination-pages {
  @apply flex items-center space-x-1;
}

.page-btn {
  @apply px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg 
         text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700;
}

.page-btn.active {
  @apply bg-blue-600 text-white border-blue-600;
}

.no-results {
  @apply flex flex-col items-center justify-center py-12 text-center;
}

.no-results-icon {
  @apply w-16 h-16 text-gray-400 dark:text-gray-600 mb-4;
}

.no-results h3 {
  @apply text-lg font-semibold text-gray-900 dark:text-white mb-2;
}

.no-results p {
  @apply text-gray-600 dark:text-gray-400 mb-4;
}

.retry-btn {
  @apply px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
         focus:outline-none focus:ring-2 focus:ring-blue-500;
}

/* 深色模式优化 */
.dark .search-input-container {
  @apply bg-gray-800;
}

.dark .search-input {
  @apply bg-gray-800 text-white border-gray-600;
}

.dark .search-suggestions,
.dark .search-history {
  @apply bg-gray-800 border-gray-600;
}

.dark .suggestions-header,
.dark .history-header {
  @apply border-gray-700;
}

.dark .suggestion-item,
.dark .history-item {
  @apply hover:bg-gray-700;
}

.dark .suggestion-item.active,
.dark .history-item.active {
  @apply bg-blue-900;
}

.dark .suggestion-text,
.dark .history-text {
  @apply text-white;
}

.dark .advanced-search-panel {
  @apply bg-gray-800 border-gray-600;
}

.dark .panel-header {
  @apply border-gray-700;
}

.dark .panel-header h3 {
  @apply text-white;
}

.dark .field-label {
  @apply text-gray-300;
}

.dark .checkbox-text,
.dark .radio-text {
  @apply text-gray-300;
}

.dark .select-input,
.dark .text-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .date-input {
  @apply bg-gray-700 text-white border-gray-600;
}

.dark .action-btn.reset {
  @apply bg-gray-700 text-gray-300 hover:bg-gray-600;
}

.dark .results-header {
  @apply bg-gray-700;
}

.dark .results-count {
  @apply text-white;
}

.dark .result-item {
  @apply bg-gray-800 border-gray-700;
}

.dark .result-sender,
.dark .result-subject {
  @apply text-white;
}

.dark .result-text {
  @apply text-gray-300;
}

.dark .result-meta {
  @apply text-gray-400;
}

.dark .result-type {
  @apply bg-gray-700 text-gray-300;
}

.dark .results-pagination {
  @apply bg-gray-700;
}

.dark .pagination-info {
  @apply text-gray-400;
}

.dark .pagination-btn {
  @apply border-gray-600 text-gray-300 hover:bg-gray-700;
}

.dark .page-btn {
  @apply border-gray-600 text-gray-300 hover:bg-gray-700;
}

.dark .no-results h3 {
  @apply text-white;
}

.dark .no-results p {
  @apply text-gray-400;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .search-input-container {
    @apply relative;
  }
  
  .search-options {
    @apply flex-wrap gap-1;
  }
  
  .option-btn {
    @apply text-xs px-2 py-1;
  }
  
  .search-suggestions,
  .search-history {
    @apply max-h-48;
  }
  
  .advanced-search-panel {
    @apply m-2;
  }
  
  .search-fields {
    @apply grid-cols-1;
  }
  
  .results-header {
    @apply flex-col space-y-2 items-start;
  }
  
  .results-actions {
    @apply w-full justify-end;
  }
  
  .results-pagination {
    @apply flex-col space-y-2;
  }
  
  .pagination-controls {
    @apply w-full justify-center;
  }
}

/* 动画效果 */
.search-suggestions,
.search-history,
.advanced-search-panel {
  @apply animate-fade-in;
}

.result-item {
  @apply transition-all duration-200 ease-in-out;
}

.result-item:hover {
  @apply transform -translate-y-0.5;
}

/* 可访问性 */
.search-input:focus,
.select-input:focus,
.text-input:focus,
.date-input:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

.option-btn:focus,
.action-btn:focus,
.pagination-btn:focus,
.page-btn:focus,
.retry-btn:focus {
  @apply outline-none ring-2 ring-blue-500 ring-opacity-50;
}

/* 高对比度模式 */
@media (prefers-contrast: high) {
  .search-input,
  .select-input,
  .text-input,
  .date-input {
    @apply border-2;
  }
  
  .result-item {
    @apply border-2;
  }
}

/* 减少动画 */
@media (prefers-reduced-motion: reduce) {
  .search-suggestions,
  .search-history,
  .advanced-search-panel {
    @apply animate-none;
  }
  
  .result-item {
    @apply transition-none;
  }
  
  .result-item:hover {
    @apply transform-none;
  }
}
</style>