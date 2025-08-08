<template>
  <AppLayout
    page-title="代码片段"
    page-subtitle="浏览和管理您的代码片段集合"
    page-icon="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"
  >
    <template #pageActions>
      <router-link
        v-if="canCreateSnippet"
        to="/snippets/create"
        class="create-btn"
      >
        <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
          <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
        </svg>
        创建片段
      </router-link>
    </template>

    <!-- 搜索和筛选 -->
    <SearchFilter
      :available-languages="availableLanguages"
      :available-tags="availableTags"
      :available-creators="availableCreators"
      :total-results="totalCount"
      :initial-filters="currentFilters"
      @filter-change="handleFilterChange"
      @search="handleSearch"
    />

    <!-- 加载状态 - 使用新的骨架屏组件 -->
    <div v-if="isLoading && snippets.length === 0" class="loading-state">
      <SkeletonLoader
        v-for="i in 6"
        :key="i"
        type="card"
        :animated="true"
        class="snippet-skeleton"
      />
    </div>

    <!-- 空状态 -->
    <div v-else-if="!isLoading && snippets.length === 0" class="empty-state">
      <div class="empty-icon">
        <svg viewBox="0 0 24 24" fill="currentColor">
          <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"/>
        </svg>
      </div>
      <h3 class="empty-title">
        {{ hasActiveFilters ? '没有找到匹配的代码片段' : '还没有代码片段' }}
      </h3>
      <p class="empty-description">
        {{ hasActiveFilters ? '尝试调整筛选条件或清除筛选器' : '创建您的第一个代码片段，开始构建您的代码库' }}
      </p>
      <div class="empty-actions">
        <button v-if="hasActiveFilters" @click="clearAllFilters" class="secondary-btn">
          清除筛选器
        </button>
        <router-link
          v-if="canCreateSnippet && !hasActiveFilters"
          to="/snippets/create"
          class="primary-btn"
        >
          <svg class="btn-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
          </svg>
          创建第一个片段
        </router-link>
      </div>
    </div>

    <!-- 代码片段列表 -->
    <div v-else class="snippets-container">
      <!-- 列表视图切换 -->
      <div class="view-controls">
        <div class="view-toggle">
          <button
            @click="viewMode = 'grid'"
            :class="['view-btn', { active: viewMode === 'grid' }]"
            title="网格视图"
          >
            <svg class="view-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M3,11H11V3H3M3,21H11V13H3M13,21H21V13H13M13,3V11H21V3"/>
            </svg>
          </button>
          <button
            @click="viewMode = 'list'"
            :class="['view-btn', { active: viewMode === 'list' }]"
            title="列表视图"
          >
            <svg class="view-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M9,5V9H21V5M9,19H21V15H9M9,14H21V10H9M4,9H8V5H4M4,19H8V15H4M4,14H8V10H4"/>
            </svg>
          </button>
        </div>

        <div class="sort-info">
          <span class="result-count">共 {{ totalCount }} 个片段</span>
        </div>
      </div>

      <!-- 代码片段网格/列表 -->
      <div v-if="useVirtualScroll && snippets.length > 50" class="virtual-scroll-container">
        <VirtualList
          :items="snippets"
          :item-height="viewMode === 'grid' ? 280 : 120"
          :container-height="600"
          :buffer="5"
          key-field="id"
        >
          <template #default="{ item: snippet }">
            <div class="virtual-snippet-item">
              <SnippetCard
                :snippet="snippet"
                :is-loading="isLoading"
                @copy="handleSnippetCopy"
                @delete="handleSnippetDelete"
                @tag-click="handleTagClick"
              />
            </div>
          </template>
        </VirtualList>
      </div>
      <div v-else :class="['snippets-list', `view-${viewMode}`]">
        <SnippetCard
          v-for="snippet in snippets"
          :key="snippet.id"
          :snippet="snippet"
          :is-loading="isLoading"
          @copy="handleSnippetCopy"
          @delete="handleSnippetDelete"
          @tag-click="handleTagClick"
        />
      </div>

      <!-- 加载更多指示器 -->
      <div v-if="isLoading && snippets.length > 0" class="loading-more">
        <div class="loading-spinner small">
          <svg class="spinner-icon" viewBox="0 0 24 24">
            <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" opacity="0.25"/>
            <path fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"/>
          </svg>
        </div>
        <span>加载更多...</span>
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
    </div>

    <!-- 浮动操作按钮 -->
    <router-link
      v-if="canCreateSnippet"
      to="/snippets/create"
      class="floating-action-btn"
      title="创建新的代码片段"
    >
      <svg class="fab-icon" viewBox="0 0 24 24" fill="currentColor">
        <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
      </svg>
    </router-link>
  </AppLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCodeSnippetsStore } from '@/stores/codeSnippets'
import { useUserFeedback } from '@/composables/useUserFeedback'
import { useLoading } from '@/composables/useLoading'
import { usePerformance } from '@/composables/usePerformance'
import AppLayout from '@/components/layout/AppLayout.vue'
import SearchFilter from '@/components/snippets/SearchFilter.vue'
import SnippetCard from '@/components/snippets/SnippetCard.vue'
import Pagination from '@/components/common/Pagination.vue'
import SkeletonLoader from '@/components/common/SkeletonLoader.vue'
import VirtualList from '@/components/common/VirtualList.vue'
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
const useVirtualScroll = ref(true)
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
/* 创建按钮 */
.create-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 500;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 123, 255, 0.2);
}

.create-btn:hover {
  background: linear-gradient(135deg, #0056b3 0%, #004085 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 123, 255, 0.3);
}

.btn-icon {
  width: 18px;
  height: 18px;
}

/* 加载状态 */
.loading-state {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  padding: 1rem 0;
}

.snippet-skeleton {
  height: 200px;
}

.loading-spinner {
  margin-bottom: 1rem;
}

.loading-spinner.small {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  padding: 1rem;
  font-size: 0.875rem;
  color: #6c757d;
}

.spinner-icon {
  width: 32px;
  height: 32px;
  color: #007bff;
  animation: spin 1s linear infinite;
}

.loading-spinner.small .spinner-icon {
  width: 20px;
  height: 20px;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.loading-text {
  color: #6c757d;
  font-size: 1rem;
  margin: 0;
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.empty-icon {
  width: 80px;
  height: 80px;
  color: #dee2e6;
  margin-bottom: 1.5rem;
}

.empty-icon svg {
  width: 100%;
  height: 100%;
}

.empty-title {
  font-size: 1.5rem;
  font-weight: 600;
  color: #495057;
  margin: 0 0 0.5rem 0;
}

.empty-description {
  color: #6c757d;
  font-size: 1rem;
  line-height: 1.5;
  margin: 0 0 2rem 0;
  max-width: 400px;
}

.empty-actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  justify-content: center;
}

.primary-btn,
.secondary-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  text-decoration: none;
  transition: all 0.3s ease;
  border: none;
  cursor: pointer;
}

.primary-btn {
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: white;
  box-shadow: 0 2px 4px rgba(0, 123, 255, 0.2);
}

.primary-btn:hover {
  background: linear-gradient(135deg, #0056b3 0%, #004085 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 123, 255, 0.3);
}

.secondary-btn {
  background: #f8f9fa;
  color: #6c757d;
  border: 1px solid #dee2e6;
}

.secondary-btn:hover {
  background: #e9ecef;
  color: #495057;
  border-color: #adb5bd;
}

/* 代码片段容器 */
.snippets-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* 视图控制 */
.view-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.75rem;
  background: linear-gradient(135deg, #ffffff 0%, #fafbfc 100%);
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  border: 1px solid rgba(0, 0, 0, 0.04);
}

.view-toggle {
  display: flex;
  gap: 0.25rem;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  padding: 0.375rem;
  border-radius: 12px;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.06);
}

.view-btn {
  padding: 0.625rem;
  border: none;
  background: none;
  color: #6c757d;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.view-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(0, 123, 255, 0.1) 0%, rgba(0, 86, 179, 0.1) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.view-btn:hover {
  color: #007bff;
  transform: translateY(-1px);
}

.view-btn:hover::before {
  opacity: 1;
}

.view-btn.active {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  color: #007bff;
  box-shadow: 0 2px 8px rgba(0, 123, 255, 0.15);
  transform: translateY(-1px);
}

.view-btn.active::before {
  opacity: 0;
}

.view-icon {
  width: 18px;
  height: 18px;
  z-index: 1;
}

.sort-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.875rem;
  color: #6c757d;
}

.result-count {
  font-weight: 500;
}

/* 代码片段列表 */
.snippets-list {
  display: grid;
  gap: 1.5rem;
}

.snippets-list.view-grid {
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
}

.snippets-list.view-list {
  grid-template-columns: 1fr;
}

/* 虚拟滚动容器 */
.virtual-scroll-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.virtual-snippet-item {
  padding: 1rem;
  border-bottom: 1px solid #f1f3f4;
}

.virtual-snippet-item:last-child {
  border-bottom: none;
}

/* 加载更多 */
.loading-more {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1rem;
  font-size: 0.875rem;
  color: #6c757d;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .view-controls {
    padding: 0.75rem 1rem;
    flex-direction: column;
    gap: 0.75rem;
    align-items: stretch;
  }

  .sort-info {
    justify-content: center;
  }

  .snippets-list.view-grid {
    grid-template-columns: 1fr;
  }

  .empty-actions {
    flex-direction: column;
    align-items: center;
  }

  .primary-btn,
  .secondary-btn {
    width: 100%;
    max-width: 200px;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .empty-state {
    padding: 2rem 1rem;
  }

  .empty-icon {
    width: 60px;
    height: 60px;
  }

  .empty-title {
    font-size: 1.25rem;
  }

  .empty-description {
    font-size: 0.875rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .create-btn,
  .primary-btn,
  .secondary-btn,
  .view-btn {
    transition: none;
  }

  .create-btn:hover,
  .primary-btn:hover {
    transform: none;
  }

  .spinner-icon {
    animation: none;
  }
}

/* 浮动操作按钮 */
.floating-action-btn {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  width: 64px;
  height: 64px;
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  text-decoration: none;
  box-shadow: 0 8px 32px rgba(0, 123, 255, 0.3);
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1000;
  overflow: hidden;
  position: relative;
}

.floating-action-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.2) 0%, rgba(255, 255, 255, 0.1) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.floating-action-btn:hover {
  transform: translateY(-4px) scale(1.05);
  box-shadow: 0 12px 48px rgba(0, 123, 255, 0.4);
}

.floating-action-btn:hover::before {
  opacity: 1;
}

.floating-action-btn:active {
  transform: translateY(-2px) scale(1.02);
}

.fab-icon {
  width: 28px;
  height: 28px;
  z-index: 1;
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .view-controls {
    border: 2px solid #000;
  }

  .view-btn.active {
    border: 2px solid #007bff;
  }

  .primary-btn,
  .secondary-btn {
    border-width: 2px;
  }

  .floating-action-btn {
    border: 3px solid #fff;
  }
}
</style>
