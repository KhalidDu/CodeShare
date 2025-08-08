<template>
  <div class="search-filter">
    <!-- 搜索栏 -->
    <div class="search-section">
      <div class="search-input-wrapper">
        <svg class="search-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
        </svg>
        <input
          v-model="searchQuery"
          type="text"
          class="search-input"
          placeholder="搜索代码片段..."
          @input="handleSearchInput"
          @keyup.enter="handleSearch"
        />
        <button
          v-if="searchQuery"
          @click="clearSearch"
          class="clear-search-btn"
          title="清除搜索"
        >
          <svg class="clear-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"/>
          </svg>
        </button>
      </div>

      <!-- 快速搜索建议 -->
      <div v-if="showSuggestions && suggestions.length > 0" class="search-suggestions">
        <div
          v-for="suggestion in suggestions"
          :key="suggestion"
          class="suggestion-item"
          @click="applySuggestion(suggestion)"
        >
          <svg class="suggestion-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M12,6A6,6 0 0,0 6,12A6,6 0 0,0 12,18A6,6 0 0,0 18,12A6,6 0 0,0 12,6Z"/>
          </svg>
          {{ suggestion }}
        </div>
      </div>
    </div>

    <!-- 筛选器 -->
    <div class="filter-section">
      <!-- 语言筛选 -->
      <div class="filter-group">
        <label class="filter-label">编程语言</label>
        <select v-model="selectedLanguage" @change="handleLanguageChange" class="filter-select">
          <option value="">全部语言</option>
          <option
            v-for="language in availableLanguages"
            :key="language.value"
            :value="language.value"
          >
            {{ language.label }} ({{ language.count }})
          </option>
        </select>
      </div>

      <!-- 标签筛选 -->
      <div class="filter-group">
        <label class="filter-label">标签</label>
        <div class="tag-filter-wrapper">
          <select v-model="selectedTag" @change="handleTagChange" class="filter-select">
            <option value="">全部标签</option>
            <option
              v-for="tag in availableTags"
              :key="tag.id"
              :value="tag.id"
            >
              {{ tag.name }} ({{ tag.count }})
            </option>
          </select>

          <!-- 标签快速选择 -->
          <div class="tag-quick-select" v-if="popularTags.length > 0">
            <span
              v-for="tag in popularTags"
              :key="tag.id"
              class="tag-chip"
              :class="{ active: selectedTag === tag.id }"
              :style="{ backgroundColor: tag.color }"
              @click="toggleTag(tag.id)"
            >
              {{ tag.name }}
            </span>
          </div>
        </div>
      </div>

      <!-- 创建者筛选 -->
      <div class="filter-group">
        <label class="filter-label">创建者</label>
        <select v-model="selectedCreator" @change="handleCreatorChange" class="filter-select">
          <option value="">全部创建者</option>
          <option
            v-for="creator in availableCreators"
            :key="creator.id"
            :value="creator.id"
          >
            {{ creator.name }} ({{ creator.count }})
          </option>
        </select>
      </div>

      <!-- 可见性筛选 -->
      <div class="filter-group">
        <label class="filter-label">可见性</label>
        <div class="visibility-filter">
          <label class="checkbox-label">
            <input
              type="checkbox"
              v-model="showPublic"
              @change="handleVisibilityChange"
              class="checkbox-input"
            />
            <span class="checkbox-custom"></span>
            <span class="checkbox-text">公开</span>
          </label>
          <label class="checkbox-label">
            <input
              type="checkbox"
              v-model="showPrivate"
              @change="handleVisibilityChange"
              class="checkbox-input"
            />
            <span class="checkbox-custom"></span>
            <span class="checkbox-text">私有</span>
          </label>
        </div>
      </div>

      <!-- 排序选项 -->
      <div class="filter-group">
        <label class="filter-label">排序方式</label>
        <select v-model="sortBy" @change="handleSortChange" class="filter-select">
          <option value="createdAt_desc">最新创建</option>
          <option value="createdAt_asc">最早创建</option>
          <option value="updatedAt_desc">最近更新</option>
          <option value="updatedAt_asc">最早更新</option>
          <option value="viewCount_desc">查看次数</option>
          <option value="copyCount_desc">复制次数</option>
          <option value="title_asc">标题 A-Z</option>
          <option value="title_desc">标题 Z-A</option>
        </select>
      </div>
    </div>

    <!-- 活跃筛选器显示 -->
    <div v-if="hasActiveFilters" class="active-filters">
      <span class="active-filters-label">当前筛选:</span>
      <div class="filter-chips">
        <span v-if="searchQuery" class="filter-chip">
          搜索: {{ searchQuery }}
          <button @click="clearSearch" class="chip-remove">×</button>
        </span>
        <span v-if="selectedLanguage" class="filter-chip">
          语言: {{ getLanguageLabel(selectedLanguage) }}
          <button @click="clearLanguage" class="chip-remove">×</button>
        </span>
        <span v-if="selectedTag" class="filter-chip">
          标签: {{ getTagLabel(selectedTag) }}
          <button @click="clearTag" class="chip-remove">×</button>
        </span>
        <span v-if="selectedCreator" class="filter-chip">
          创建者: {{ getCreatorLabel(selectedCreator) }}
          <button @click="clearCreator" class="chip-remove">×</button>
        </span>
        <button @click="clearAllFilters" class="clear-all-btn">清除全部</button>
      </div>
    </div>

    <!-- 结果统计 -->
    <div class="results-summary">
      <span class="results-count">
        找到 {{ totalResults }} 个代码片段
        <span v-if="hasActiveFilters">（已筛选）</span>
      </span>
      <button
        @click="toggleAdvancedFilters"
        class="advanced-toggle"
        :class="{ active: showAdvancedFilters }"
      >
        <svg class="toggle-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M3,17V19H9V17H3M3,5V7H13V5H3M13,21V19H21V17H13V15H11V21H13M7,9V11H3V13H7V15H9V9H7M21,13V11H11V13H21M15,9H13V7H21V5H13V3H11V9H15Z"/>
        </svg>
        高级筛选
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Tag } from '@/types'

interface LanguageOption {
  value: string
  label: string
  count: number
}

interface TagOption extends Tag {
  count: number
}

interface CreatorOption {
  id: string
  name: string
  count: number
}

interface FilterState {
  search: string
  language: string
  tag: string
  creator: string
  showPublic: boolean
  showPrivate: boolean
  sortBy: string
}

interface Props {
  availableLanguages?: LanguageOption[]
  availableTags?: TagOption[]
  availableCreators?: CreatorOption[]
  totalResults?: number
  initialFilters?: Partial<FilterState>
}

interface Emits {
  (e: 'filter-change', filters: FilterState): void
  (e: 'search', query: string): void
}

const props = withDefaults(defineProps<Props>(), {
  availableLanguages: () => [],
  availableTags: () => [],
  availableCreators: () => [],
  totalResults: 0,
  initialFilters: () => ({})
})

const emit = defineEmits<Emits>()

// 响应式状态
const searchQuery = ref(props.initialFilters.search || '')
const selectedLanguage = ref(props.initialFilters.language || '')
const selectedTag = ref(props.initialFilters.tag || '')
const selectedCreator = ref(props.initialFilters.creator || '')
const showPublic = ref(props.initialFilters.showPublic ?? true)
const showPrivate = ref(props.initialFilters.showPrivate ?? true)
const sortBy = ref(props.initialFilters.sortBy || 'createdAt_desc')

const showSuggestions = ref(false)
const showAdvancedFilters = ref(false)
const searchTimeout = ref<number | null>(null)

// 计算属性
const suggestions = computed(() => {
  if (!searchQuery.value || searchQuery.value.length < 2) return []

  // 这里可以根据历史搜索、热门搜索等生成建议
  const mockSuggestions = [
    'React Hook',
    'Vue 组件',
    'JavaScript 工具函数',
    'CSS 动画',
    'Python 爬虫',
    'SQL 查询'
  ]

  return mockSuggestions.filter(s =>
    s.toLowerCase().includes(searchQuery.value.toLowerCase())
  ).slice(0, 5)
})

const popularTags = computed(() => {
  return props.availableTags
    .filter(tag => tag.count > 5)
    .sort((a, b) => b.count - a.count)
    .slice(0, 8)
})

const hasActiveFilters = computed(() => {
  return !!(searchQuery.value ||
           selectedLanguage.value ||
           selectedTag.value ||
           selectedCreator.value ||
           (!showPublic.value || !showPrivate.value))
})

const currentFilters = computed((): FilterState => ({
  search: searchQuery.value,
  language: selectedLanguage.value,
  tag: selectedTag.value,
  creator: selectedCreator.value,
  showPublic: showPublic.value,
  showPrivate: showPrivate.value,
  sortBy: sortBy.value
}))

// 监听筛选器变化
watch(currentFilters, (newFilters) => {
  emit('filter-change', newFilters)
}, { deep: true })

/**
 * 处理搜索输入
 */
function handleSearchInput() {
  showSuggestions.value = true

  // 防抖处理
  if (searchTimeout.value) {
    clearTimeout(searchTimeout.value)
  }

  searchTimeout.value = window.setTimeout(() => {
    handleSearch()
  }, 300)
}

/**
 * 处理搜索
 */
function handleSearch() {
  showSuggestions.value = false
  emit('search', searchQuery.value)
}

/**
 * 应用搜索建议
 */
function applySuggestion(suggestion: string) {
  searchQuery.value = suggestion
  showSuggestions.value = false
  handleSearch()
}

/**
 * 清除搜索
 */
function clearSearch() {
  searchQuery.value = ''
  showSuggestions.value = false
}

/**
 * 处理语言变化
 */
function handleLanguageChange() {
  // 筛选器变化会通过 watch 自动触发
}

/**
 * 处理标签变化
 */
function handleTagChange() {
  // 筛选器变化会通过 watch 自动触发
}

/**
 * 切换标签选择
 */
function toggleTag(tagId: string) {
  selectedTag.value = selectedTag.value === tagId ? '' : tagId
}

/**
 * 处理创建者变化
 */
function handleCreatorChange() {
  // 筛选器变化会通过 watch 自动触发
}

/**
 * 处理可见性变化
 */
function handleVisibilityChange() {
  // 筛选器变化会通过 watch 自动触发
}

/**
 * 处理排序变化
 */
function handleSortChange() {
  // 筛选器变化会通过 watch 自动触发
}

/**
 * 获取语言标签
 */
function getLanguageLabel(languageValue: string): string {
  const language = props.availableLanguages.find(l => l.value === languageValue)
  return language?.label || languageValue
}

/**
 * 获取标签标签
 */
function getTagLabel(tagId: string): string {
  const tag = props.availableTags.find(t => t.id === tagId)
  return tag?.name || tagId
}

/**
 * 获取创建者标签
 */
function getCreatorLabel(creatorId: string): string {
  const creator = props.availableCreators.find(c => c.id === creatorId)
  return creator?.name || creatorId
}

/**
 * 清除语言筛选
 */
function clearLanguage() {
  selectedLanguage.value = ''
}

/**
 * 清除标签筛选
 */
function clearTag() {
  selectedTag.value = ''
}

/**
 * 清除创建者筛选
 */
function clearCreator() {
  selectedCreator.value = ''
}

/**
 * 清除所有筛选器
 */
function clearAllFilters() {
  searchQuery.value = ''
  selectedLanguage.value = ''
  selectedTag.value = ''
  selectedCreator.value = ''
  showPublic.value = true
  showPrivate.value = true
  sortBy.value = 'createdAt_desc'
}

/**
 * 切换高级筛选器显示
 */
function toggleAdvancedFilters() {
  showAdvancedFilters.value = !showAdvancedFilters.value
}

// 点击外部关闭建议
function handleClickOutside(event: Event) {
  const target = event.target as Element
  if (!target.closest('.search-section')) {
    showSuggestions.value = false
  }
}

// 添加全局点击监听器
if (typeof window !== 'undefined') {
  document.addEventListener('click', handleClickOutside)
}
</script>

<style scoped>
.search-filter {
  background: var(--gradient-surface);
  border: 1px solid rgba(0, 0, 0, 0.04);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-md);
  padding: 2rem;
  margin-bottom: 1.5rem;
  transition: all var(--transition-normal);
}

.search-filter:hover {
  box-shadow: var(--shadow-lg);
}

/* 搜索部分 */
.search-section {
  position: relative;
  margin-bottom: 1.5rem;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 1rem;
  width: 20px;
  height: 20px;
  color: #6c757d;
  z-index: 2;
}

.search-input {
  width: 100%;
  padding: 1rem 1.25rem 1rem 3.5rem;
  border: 2px solid rgba(0, 0, 0, 0.06);
  border-radius: var(--radius-lg);
  font-size: 1rem;
  font-weight: var(--font-medium);
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  transition: all var(--transition-normal);
  color: var(--gray-800);
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 4px rgba(0, 123, 255, 0.1);
  background: rgba(255, 255, 255, 1);
  transform: translateY(-1px);
}

.search-input::placeholder {
  color: var(--gray-500);
  font-weight: var(--font-normal);
}

.clear-search-btn {
  position: absolute;
  right: 0.75rem;
  background: none;
  border: none;
  color: #6c757d;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: color 0.3s ease;
}

.clear-search-btn:hover {
  color: #495057;
}

.clear-icon {
  width: 18px;
  height: 18px;
}

/* 搜索建议 */
.search-suggestions {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #e9ecef;
  border-top: none;
  border-radius: 0 0 8px 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  max-height: 200px;
  overflow-y: auto;
}

.suggestion-item {
  padding: 0.75rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: background-color 0.3s ease;
}

.suggestion-item:hover {
  background-color: #f8f9fa;
}

.suggestion-icon {
  width: 16px;
  height: 16px;
  color: #6c757d;
}

/* 筛选部分 */
.filter-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.filter-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  background: white;
  transition: border-color 0.3s ease;
}

.filter-select:focus {
  outline: none;
  border-color: #3b82f6;
}

/* 标签筛选 */
.tag-filter-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.tag-quick-select {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.tag-chip {
  color: white;
  padding: 0.375rem 0.75rem;
  border-radius: var(--radius-2xl);
  font-size: var(--text-xs);
  font-weight: var(--font-semibold);
  cursor: pointer;
  transition: all var(--transition-normal);
  opacity: 0.7;
  position: relative;
  overflow: hidden;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
  box-shadow: var(--shadow-sm);
}

.tag-chip::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.1) 100%);
  opacity: 0;
  transition: opacity var(--transition-normal);
}

.tag-chip:hover,
.tag-chip.active {
  opacity: 1;
  transform: translateY(-2px) scale(1.05);
  box-shadow: var(--shadow-md);
}

.tag-chip:hover::before,
.tag-chip.active::before {
  opacity: 1;
}

/* 可见性筛选 */
.visibility-filter {
  display: flex;
  gap: 1rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.875rem;
  color: #374151;
}

.checkbox-input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.checkbox-custom {
  width: 16px;
  height: 16px;
  border: 2px solid #d1d5db;
  border-radius: 3px;
  background-color: #ffffff;
  transition: all 0.3s ease;
  position: relative;
  flex-shrink: 0;
}

.checkbox-custom::after {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%) scale(0);
  width: 8px;
  height: 8px;
  background-color: #3b82f6;
  border-radius: 1px;
  transition: transform 0.2s ease;
}

.checkbox-input:checked + .checkbox-custom {
  border-color: #3b82f6;
  background-color: #3b82f6;
}

.checkbox-input:checked + .checkbox-custom::after {
  transform: translate(-50%, -50%) scale(1);
  background-color: white;
}

.checkbox-text {
  user-select: none;
}

/* 活跃筛选器 */
.active-filters {
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.active-filters-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #6c757d;
  margin-bottom: 0.5rem;
  display: block;
}

.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
}

.filter-chip {
  background: #007bff;
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.chip-remove {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  font-size: 1rem;
  line-height: 1;
  padding: 0;
  margin-left: 0.25rem;
}

.clear-all-btn {
  background: #6c757d;
  color: white;
  border: none;
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.clear-all-btn:hover {
  background: #5a6268;
}

/* 结果统计 */
.results-summary {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #e9ecef;
  font-size: 0.875rem;
}

.results-count {
  color: #6c757d;
}

.advanced-toggle {
  background: none;
  border: 1px solid #d1d5db;
  color: #6c757d;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.advanced-toggle:hover,
.advanced-toggle.active {
  border-color: #007bff;
  color: #007bff;
}

.toggle-icon {
  width: 16px;
  height: 16px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .search-filter {
    padding: 1rem;
  }

  .filter-section {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .visibility-filter {
    flex-direction: column;
    gap: 0.5rem;
  }

  .results-summary {
    flex-direction: column;
    gap: 0.75rem;
    align-items: flex-start;
  }

  .filter-chips {
    justify-content: flex-start;
  }
}

@media (max-width: 480px) {
  .search-input {
    padding: 0.75rem 1rem 0.75rem 2.5rem;
  }

  .search-icon {
    left: 0.75rem;
    width: 18px;
    height: 18px;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .search-input,
  .filter-select,
  .checkbox-custom,
  .tag-chip,
  .advanced-toggle {
    transition: none;
  }

  .tag-chip:hover,
  .tag-chip.active {
    transform: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .search-filter {
    border: 2px solid #000;
  }

  .search-input,
  .filter-select {
    border-width: 2px;
  }

  .checkbox-custom {
    border-width: 2px;
  }
}
</style>
