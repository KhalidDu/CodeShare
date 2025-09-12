<template>
  <div class="comment-list" :class="listClasses">
    <!-- 评论头部信息 -->
    <div v-if="showHeader" class="comment-list-header">
      <div class="comment-stats">
        <h3 class="comment-title">
          <span class="comment-count">{{ totalCount }}</span>
          条评论
        </h3>
        <div v-if="stats" class="comment-meta">
          <span class="meta-item">
            <i class="fas fa-heart"></i>
            {{ stats.totalLikes }} 点赞
          </span>
          <span class="meta-item">
            <i class="fas fa-users"></i>
            {{ stats.activeUsers }} 位用户参与
          </span>
          <span v-if="stats.latestCommentAt" class="meta-item">
            <i class="fas fa-clock"></i>
            {{ formatRelativeTime(stats.latestCommentAt) }}
          </span>
        </div>
      </div>
      
      <!-- 评论排序选项 -->
      <div v-if="showSortOptions" class="comment-sort">
        <select 
          v-model="currentSort" 
          class="sort-select"
          @change="handleSortChange"
        >
          <option :value="CommentSort.CREATED_AT_DESC">最新发布</option>
          <option :value="CommentSort.CREATED_AT_ASC">最早发布</option>
          <option :value="CommentSort.LIKE_COUNT_DESC">最多点赞</option>
          <option :value="CommentSort.LIKE_COUNT_ASC">最少点赞</option>
          <option :value="CommentSort.REPLY_COUNT_DESC">最多回复</option>
          <option :value="CommentSort.REPLY_COUNT_ASC">最少回复</option>
        </select>
      </div>
    </div>

    <!-- 评论搜索栏 -->
    <div v-if="showSearch" class="comment-search-bar">
      <CommentSearch
        v-model:keyword="searchKeyword"
        :placeholder="searchPlaceholder"
        :loading="searchLoading"
        @search="handleSearch"
        @clear="handleSearchClear"
      />
    </div>

    <!-- 评论列表内容 -->
    <div class="comment-list-content">
      <!-- 加载状态 -->
      <div v-if="loading && comments.length === 0" class="comment-loading">
        <EnhancedLoading 
          variant="skeleton" 
          :count="skeletonCount"
          type="comment"
        />
      </div>

      <!-- 空状态 -->
      <div v-else-if="!loading && comments.length === 0" class="comment-empty">
        <div class="empty-icon">
          <i class="fas fa-comments"></i>
        </div>
        <h3 class="empty-title">{{ emptyTitle }}</h3>
        <p class="empty-description">{{ emptyDescription }}</p>
        <div v-if="showCreateButton && permissions?.canCreate" class="empty-action">
          <AnimatedButton
            variant="primary"
            size="md"
            @click="$emit('create-comment')"
          >
            <i class="fas fa-plus"></i>
            发表第一条评论
          </AnimatedButton>
        </div>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="comment-error">
        <div class="error-icon">
          <i class="fas fa-exclamation-triangle"></i>
        </div>
        <h3 class="error-title">加载评论失败</h3>
        <p class="error-message">{{ error }}</p>
        <div class="error-action">
          <AnimatedButton
            variant="outline"
            size="sm"
            @click="retry"
          >
            <i class="fas fa-redo"></i>
            重试
          </AnimatedButton>
        </div>
      </div>

      <!-- 评论树形列表 -->
      <div v-else class="comment-tree">
        <TransitionGroup 
          name="comment-item"
          tag="div"
          class="comment-items"
        >
          <CommentItem
            v-for="commentNode in commentTree"
            :key="commentNode.comment.id"
            :comment="commentNode.comment"
            :replies="commentNode.children"
            :depth="0"
            :max-depth="maxReplyDepth"
            :show-avatar="showAvatar"
            :show-actions="showActions"
            :show-timestamp="showTimestamp"
            :show-like-button="showLikeButton"
            :show-reply-button="showReplyButton"
            :show-report-button="showReportButton"
            :current-user-id="currentUserId"
            :permissions="permissions"
            :highlighted-comment-id="highlightedCommentId"
            :is-collapsed="collapsedComments.has(commentNode.comment.id)"
            @like="handleLike"
            @reply="handleReply"
            @edit="handleEdit"
            @delete="handleDelete"
            @report="handleReport"
            @toggle-collapse="handleToggleCollapse"
            @load-more="handleLoadMoreReplies"
          />
        </TransitionGroup>
      </div>

      <!-- 加载更多 -->
      <div v-if="hasMore && !loading" class="comment-load-more">
        <AnimatedButton
          variant="outline"
          size="md"
          :loading="loadingMore"
          @click="loadMore"
        >
          <i class="fas fa-chevron-down"></i>
          加载更多评论
        </AnimatedButton>
      </div>

      <!-- 加载更多状态 -->
      <div v-if="loadingMore" class="comment-loading-more">
        <EnhancedLoading variant="spinner" size="sm" />
        <span>正在加载更多评论...</span>
      </div>
    </div>

    <!-- 评论表单 -->
    <div v-if="showForm && permissions?.canCreate" class="comment-form-container">
      <CommentForm
        :snippet-id="snippetId"
        :parent-id="replyToCommentId"
        :loading="formLoading"
        :max-length="formMaxLength"
        :placeholder="formPlaceholder"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted } from 'vue'
import { useCommentStore } from '@/stores/comment'
import { useAuthStore } from '@/stores/auth'
import type { Comment, CommentSort, CommentPermissions, CommentStats } from '@/types/comment'
import CommentItem from './CommentItem.vue'
import CommentSearch from './CommentSearch.vue'
import CommentForm from './CommentForm.vue'
import AnimatedButton from '@/components/common/AnimatedButton.vue'
import EnhancedLoading from '@/components/common/EnhancedLoading.vue'

// 定义Props
interface Props {
  snippetId: string
  comments?: Comment[]
  loading?: boolean
  error?: string | null
  totalCount?: number
  hasMore?: boolean
  stats?: CommentStats | null
  permissions?: CommentPermissions | null
  currentUserId?: string | null
  highlightedCommentId?: string | null
  
  // 显示选项
  showHeader?: boolean
  showSearch?: boolean
  showSortOptions?: boolean
  showForm?: boolean
  showAvatar?: boolean
  showActions?: boolean
  showTimestamp?: boolean
  showLikeButton?: boolean
  showReplyButton?: boolean
  showReportButton?: boolean
  showCreateButton?: boolean
  
  // 功能选项
  enableReply?: boolean
  enableLike?: boolean
  enableReport?: boolean
  enableSort?: boolean
  enableSearch?: boolean
  
  // 样式选项
  variant?: 'default' | 'compact' | 'detailed'
  size?: 'sm' | 'md' | 'lg'
  maxReplyDepth?: number
  
  // 表单选项
  formPlaceholder?: string
  formMaxLength?: number
  
  // 空状态选项
  emptyTitle?: string
  emptyDescription?: string
  
  // 搜索选项
  searchPlaceholder?: string
  
  // 加载选项
  skeletonCount?: number
}

const props = withDefaults(defineProps<Props>(), {
  comments: () => [],
  loading: false,
  error: null,
  totalCount: 0,
  hasMore: false,
  stats: null,
  permissions: null,
  currentUserId: null,
  highlightedCommentId: null,
  
  showHeader: true,
  showSearch: true,
  showSortOptions: true,
  showForm: true,
  showAvatar: true,
  showActions: true,
  showTimestamp: true,
  showLikeButton: true,
  showReplyButton: true,
  showReportButton: true,
  showCreateButton: true,
  
  enableReply: true,
  enableLike: true,
  enableReport: true,
  enableSort: true,
  enableSearch: true,
  
  variant: 'default',
  size: 'md',
  maxReplyDepth: 5,
  
  formPlaceholder: '写下你的评论...',
  formMaxLength: 1000,
  
  emptyTitle: '暂无评论',
  emptyDescription: '成为第一个发表评论的人吧！',
  
  searchPlaceholder: '搜索评论...',
  
  skeletonCount: 3
})

// 定义Emits
interface Emits {
  (e: 'like', commentId: string): void
  (e: 'reply', commentId: string): void
  (e: 'edit', commentId: string): void
  (e: 'delete', commentId: string): void
  (e: 'report', commentId: string): void
  (e: 'load-more'): void
  (e: 'sort-change', sort: CommentSort): void
  (e: 'search', keyword: string): void
  (e: 'search-clear'): void
  (e: 'submit', data: any): void
  (e: 'cancel'): void
  (e: 'create-comment'): void
  (e: 'toggle-collapse', commentId: string): void
  (e: 'load-more-replies', commentId: string): void
  (e: 'retry'): void
}

const emit = defineEmits<Emits>()

// Store
const commentStore = useCommentStore()
const authStore = useAuthStore()

// 响应式状态
const currentSort = ref<CommentSort>(CommentSort.CREATED_AT_DESC)
const searchKeyword = ref('')
const searchLoading = ref(false)
const loadingMore = ref(false)
const formLoading = ref(false)
const replyToCommentId = ref<string | null>(null)
const collapsedComments = ref<Set<string>>(new Set())

// 计算属性
const listClasses = computed(() => {
  return [
    'comment-list',
    `comment-list--${props.variant}`,
    `comment-list--${props.size}`,
    {
      'comment-list--loading': props.loading,
      'comment-list--error': !!props.error,
      'comment-list--empty': props.comments.length === 0 && !props.loading
    }
  ]
})

const commentTree = computed(() => {
  const commentMap = new Map<string, any>()
  const rootComments: any[] = []

  // 创建所有评论节点
  props.comments.forEach(comment => {
    commentMap.set(comment.id, {
      comment,
      children: []
    })
  })

  // 构建树形结构
  props.comments.forEach(comment => {
    const node = commentMap.get(comment.id)!
    if (comment.parentId) {
      const parentNode = commentMap.get(comment.parentId)
      if (parentNode) {
        parentNode.children.push(node)
      }
    } else {
      rootComments.push(node)
    }
  })

  return rootComments
})

// 方法
function handleLike(commentId: string) {
  if (!props.enableLike) return
  emit('like', commentId)
}

function handleReply(commentId: string) {
  if (!props.enableReply) return
  replyToCommentId.value = commentId
  emit('reply', commentId)
  
  // 滚动到表单
  scrollToForm()
}

function handleEdit(commentId: string) {
  emit('edit', commentId)
}

function handleDelete(commentId: string) {
  emit('delete', commentId)
}

function handleReport(commentId: string) {
  if (!props.enableReport) return
  emit('report', commentId)
}

function handleToggleCollapse(commentId: string) {
  if (collapsedComments.value.has(commentId)) {
    collapsedComments.value.delete(commentId)
  } else {
    collapsedComments.value.add(commentId)
  }
  emit('toggle-collapse', commentId)
}

function handleLoadMoreReplies(commentId: string) {
  emit('load-more-replies', commentId)
}

function handleSortChange() {
  emit('sort-change', currentSort.value)
}

function handleSearch(keyword: string) {
  searchKeyword.value = keyword
  searchLoading.value = true
  emit('search', keyword)
}

function handleSearchClear() {
  searchKeyword.value = ''
  emit('search-clear')
}

async function handleSubmit(data: any) {
  try {
    formLoading.value = true
    await emit('submit', data)
    replyToCommentId.value = null
  } finally {
    formLoading.value = false
  }
}

function handleCancel() {
  replyToCommentId.value = null
  emit('cancel')
}

async function loadMore() {
  if (loadingMore.value) return
  
  try {
    loadingMore.value = true
    await emit('load-more')
  } finally {
    loadingMore.value = false
  }
}

function retry() {
  emit('retry')
}

function scrollToForm() {
  setTimeout(() => {
    const form = document.querySelector('.comment-form-container')
    if (form) {
      form.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
    }
  }, 100)
}

function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()
  
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)
  
  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 30) return `${days}天前`
  
  return date.toLocaleDateString()
}

// 监听器
watch(() => props.loading, (newLoading) => {
  if (!newLoading) {
    searchLoading.value = false
  }
})

// 生命周期
onMounted(() => {
  // 自动高亮指定评论
  if (props.highlightedCommentId) {
    setTimeout(() => {
      const element = document.querySelector(`[data-comment-id="${props.highlightedCommentId}"]`)
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' })
        element.classList.add('comment-item--highlighted')
        setTimeout(() => {
          element.classList.remove('comment-item--highlighted')
        }, 3000)
      }
    }, 500)
  }
})
</script>

<style scoped>
.comment-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  width: 100%;
}

/* 评论头部 */
.comment-list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  border-bottom: 1px solid var(--gray-200);
}

.comment-stats {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.comment-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--gray-900);
  margin: 0;
}

.comment-count {
  color: var(--primary-600);
  font-weight: 700;
}

.comment-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.875rem;
  color: var(--gray-600);
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.comment-sort {
  display: flex;
  align-items: center;
}

.sort-select {
  padding: 0.5rem 1rem;
  border: 1px solid var(--gray-300);
  border-radius: 0.5rem;
  background: white;
  color: var(--gray-700);
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s ease;
}

.sort-select:hover {
  border-color: var(--primary-500);
}

.sort-select:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* 搜索栏 */
.comment-search-bar {
  margin-bottom: 1rem;
}

/* 评论内容 */
.comment-list-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* 加载状态 */
.comment-loading {
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: center;
}

/* 空状态 */
.comment-empty {
  padding: 3rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.empty-icon {
  font-size: 3rem;
  color: var(--gray-400);
  margin-bottom: 1rem;
}

.empty-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--gray-900);
  margin: 0;
}

.empty-description {
  color: var(--gray-600);
  margin: 0;
}

.empty-action {
  margin-top: 1rem;
}

/* 错误状态 */
.comment-error {
  padding: 3rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.error-icon {
  font-size: 3rem;
  color: var(--error-500);
  margin-bottom: 1rem;
}

.error-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--error-700);
  margin: 0;
}

.error-message {
  color: var(--error-600);
  margin: 0;
}

.error-action {
  margin-top: 1rem;
}

/* 评论树 */
.comment-tree {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.comment-items {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* 加载更多 */
.comment-load-more {
  padding: 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
}

.comment-loading-more {
  padding: 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.5rem;
  color: var(--gray-600);
  font-size: 0.875rem;
}

/* 表单容器 */
.comment-form-container {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 1px solid var(--gray-200);
}

/* 变体样式 */
.comment-list--compact {
  gap: 1rem;
}

.comment-list--compact .comment-list-header {
  padding: 0.75rem 0;
}

.comment-list--compact .comment-title {
  font-size: 1rem;
}

.comment-list--compact .comment-meta {
  font-size: 0.75rem;
  gap: 0.75rem;
}

.comment-list--detailed {
  gap: 2rem;
}

.comment-list--detailed .comment-list-header {
  padding: 1.5rem 0;
}

.comment-list--detailed .comment-title {
  font-size: 1.5rem;
}

.comment-list--detailed .comment-meta {
  font-size: 1rem;
  gap: 1.5rem;
}

/* 尺寸样式 */
.comment-list--sm {
  gap: 1rem;
}

.comment-list--sm .comment-list-header {
  padding: 0.5rem 0;
}

.comment-list--sm .comment-title {
  font-size: 0.875rem;
}

.comment-list--sm .comment-meta {
  font-size: 0.75rem;
  gap: 0.5rem;
}

.comment-list--lg {
  gap: 2rem;
}

.comment-list--lg .comment-list-header {
  padding: 1.5rem 0;
}

.comment-list--lg .comment-title {
  font-size: 1.5rem;
}

.comment-list--lg .comment-meta {
  font-size: 1rem;
  gap: 1.5rem;
}

/* 过渡动画 */
.comment-item-enter-active {
  transition: all 0.3s ease;
}

.comment-item-leave-active {
  transition: all 0.3s ease;
}

.comment-item-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.comment-item-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

.comment-item-move {
  transition: transform 0.3s ease;
}

/* 高亮状态 */
:deep(.comment-item--highlighted) {
  background: linear-gradient(90deg, rgba(59, 130, 246, 0.1) 0%, transparent 100%);
  border-left: 3px solid var(--primary-500);
  animation: highlight-pulse 2s ease-in-out;
}

@keyframes highlight-pulse {
  0%, 100% {
    background: linear-gradient(90deg, rgba(59, 130, 246, 0.1) 0%, transparent 100%);
  }
  50% {
    background: linear-gradient(90deg, rgba(59, 130, 246, 0.2) 0%, transparent 100%);
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .comment-list-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .comment-stats {
    width: 100%;
  }
  
  .comment-sort {
    width: 100%;
  }
  
  .sort-select {
    width: 100%;
  }
  
  .comment-meta {
    flex-wrap: wrap;
    gap: 0.5rem;
  }
  
  .comment-empty,
  .comment-error {
    padding: 2rem 1rem;
  }
}

@media (max-width: 480px) {
  .comment-list {
    gap: 1rem;
  }
  
  .comment-list-header {
    padding: 0.5rem 0;
  }
  
  .comment-title {
    font-size: 1rem;
  }
  
  .comment-meta {
    font-size: 0.75rem;
  }
}

/* 深色模式 */
:deep(.dark) .comment-list-header {
  border-bottom-color: var(--gray-700);
}

:deep(.dark) .comment-title {
  color: var(--gray-100);
}

:deep(.dark) .comment-meta {
  color: var(--gray-400);
}

:deep(.dark) .sort-select {
  background: var(--gray-800);
  border-color: var(--gray-600);
  color: var(--gray-200);
}

:deep(.dark) .form-container {
  border-top-color: var(--gray-700);
}

/* 无障碍性 */
@media (prefers-reduced-motion: reduce) {
  .comment-item-enter-active,
  .comment-item-leave-active,
  .comment-item-move {
    transition: none;
  }
  
  :deep(.comment-item--highlighted) {
    animation: none;
    background: rgba(59, 130, 246, 0.1);
  }
}
</style>