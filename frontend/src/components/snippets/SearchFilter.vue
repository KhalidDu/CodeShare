<template>
  <div class="p-4 border-b border-slate-300 dark:border-gray-600 bg-slate-100 dark:bg-gray-700">
    <!-- 搜索栏 -->
    <div class="relative mb-4">
      <div class="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-500 dark:text-slate-400 z-10 pointer-events-none">
        <i class="fas fa-search w-4 h-4"></i>
      </div>
      <input
        v-model="searchQuery"
        type="text"
        placeholder="搜索代码片段..."
        class="w-full pl-10 pr-10 py-2.5 border border-slate-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-gray-800 backdrop-blur-sm focus:outline-none focus:border-blue-400 dark:focus:border-blue-500 focus:bg-white dark:focus:bg-gray-800 focus:shadow-sm transition-all duration-200 text-slate-700 dark:text-slate-100 placeholder-slate-500 dark:placeholder-gray-400"
        @input="handleSearchInput"
        @keyup.enter="handleSearch"
      />
      <button
        v-if="searchQuery"
        @click="clearSearch"
        class="absolute right-3 top-1/2 transform -translate-y-1/2 text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 transition-colors duration-200"
        title="清除搜索"
      >
        <i class="fas fa-times w-4 h-4"></i>
      </button>
    </div>

    <!-- 筛选器行 -->
    <div class="flex flex-wrap items-center gap-3 mb-4">
      <!-- 语言筛选 -->
      <div class="relative">
        <select
          v-model="selectedLanguage"
          @change="handleLanguageChange"
          class="appearance-none bg-white dark:bg-gray-800 backdrop-blur-sm border border-slate-300 dark:border-gray-600 rounded-lg px-3 py-2 pr-8 text-sm text-slate-700 dark:text-slate-200 focus:outline-none focus:border-blue-400 dark:focus:border-blue-500 transition-all duration-200 cursor-pointer"
        >
          <option value="">全部语言</option>
          <option
            v-for="language in availableLanguages"
            :key="language.value"
            :value="language.value"
          >
            {{ language.label }} ({{ language.count }})
          </option>
        </select>
        <i class="fas fa-chevron-down absolute right-3 top-1/2 transform -translate-y-1/2 text-slate-500 dark:text-slate-400 w-3 h-3 pointer-events-none"></i>
      </div>

      <!-- 排序选择 -->
      <div class="relative">
        <select
          v-model="sortBy"
          @change="handleSortChange"
          class="appearance-none bg-white dark:bg-gray-800 backdrop-blur-sm border border-slate-300 dark:border-gray-600 rounded-lg px-3 py-2 pr-8 text-sm text-slate-700 dark:text-slate-200 focus:outline-none focus:border-blue-400 dark:focus:border-blue-500 transition-all duration-200 cursor-pointer"
        >
          <option value="createdAt_desc">最新创建</option>
          <option value="createdAt_asc">最早创建</option>
          <option value="updatedAt_desc">最近更新</option>
          <option value="updatedAt_asc">最早更新</option>
          <option value="viewCount_desc">查看次数</option>
          <option value="copyCount_desc">复制次数</option>
          <option value="title_asc">标题 A-Z</option>
          <option value="title_desc">标题 Z-A</option>
        </select>
        <i class="fas fa-chevron-down absolute right-3 top-1/2 transform -translate-y-1/2 text-slate-500 dark:text-slate-400 w-3 h-3 pointer-events-none"></i>
      </div>

      <!-- 可见性筛选 -->
      <div class="flex items-center gap-2">
        <label class="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-300 cursor-pointer">
          <input
            type="checkbox"
            v-model="showPublic"
            @change="handleVisibilityChange"
            class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
          />
          <span>公开</span>
        </label>
        <label class="flex items-center gap-2 text-sm text-slate-600 dark:text-slate-300 cursor-pointer">
          <input
            type="checkbox"
            v-model="showPrivate"
            @change="handleVisibilityChange"
            class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
          />
          <span>私有</span>
        </label>
      </div>

      <!-- 清除筛选器按钮 -->
      <button
        v-if="hasActiveFilters"
        @click="clearAllFilters"
        class="flex items-center gap-2 px-3 py-2 bg-red-100 hover:bg-red-200 dark:bg-red-900 dark:hover:bg-red-800 text-red-600 dark:text-red-400 rounded-lg text-sm font-medium transition-all duration-200"
      >
        <i class="fas fa-times w-3 h-3"></i>
        <span>清除筛选</span>
      </button>
    </div>

    <!-- 结果统计 -->
    <div class="flex items-center justify-between text-sm text-slate-600 dark:text-slate-400">
      <span>
        找到 {{ totalResults }} 个代码片段
        <span v-if="hasActiveFilters" class="text-blue-600 dark:text-blue-400">（已筛选）</span>
      </span>
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

const searchTimeout = ref<number | null>(null)

// 计算属性
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
  emit('search', searchQuery.value)
}

/**
 * 清除搜索
 */
function clearSearch() {
  searchQuery.value = ''
}

/**
 * 处理语言变化
 */
function handleLanguageChange() {
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
 * 清除语言筛选
 */
function clearLanguage() {
  selectedLanguage.value = ''
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
</script>

<style scoped>
/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  * {
    transition: none !important;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .bg-white {
    background: white !important;
  }

  .dark .bg-gray-800 {
    background: black !important;
  }

  .border-slate-300 {
    border-color: black !important;
    border-width: 2px !important;
  }

  .dark .border-gray-600 {
    border-color: white !important;
    border-width: 2px !important;
  }
}
</style>
