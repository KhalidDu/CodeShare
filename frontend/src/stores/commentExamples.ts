/**
 * 评论Store使用示例和最佳实践
 * 
 * 本文件提供评论Pinia Store的使用示例和最佳实践指南：
 * - 基础使用方法
 * - 高级功能使用
 * - 性能优化建议
 * - 错误处理最佳实践
 * 
 * 遵循Vue 3 + Composition API开发模式
 */

import { defineComponent, ref, computed, onMounted, onUnmounted } from 'vue'
import { useCommentStore } from '@/stores/comment'
import type { Comment, CommentFilter, CommentSort } from '@/types/comment'

/**
 * 基础评论组件示例
 */
export const CommentSection = defineComponent({
  name: 'CommentSection',
  props: {
    snippetId: {
      type: String,
      required: true
    }
  },
  setup(props) {
    const commentStore = useCommentStore()
    const newCommentContent = ref('')
    const isLoading = computed(() => commentStore.isLoading)
    const hasError = computed(() => commentStore.hasError)
    const comments = computed(() => commentStore.comments)
    const hasMoreComments = computed(() => commentStore.hasMoreComments)

    // 加载评论列表
    const loadComments = async () => {
      try {
        await commentStore.fetchComments({
          snippetId: props.snippetId,
          includeReplies: true,
          includeUser: true
        })
      } catch (error) {
        console.error('Failed to load comments:', error)
      }
    }

    // 创建新评论
    const createComment = async () => {
      if (!newCommentContent.value.trim()) return
      
      try {
        await commentStore.createComment({
          content: newCommentContent.value,
          snippetId: props.snippetId
        })
        newCommentContent.value = ''
      } catch (error) {
        console.error('Failed to create comment:', error)
      }
    }

    // 加载更多评论
    const loadMoreComments = async () => {
      try {
        await commentStore.fetchMoreComments()
      } catch (error) {
        console.error('Failed to load more comments:', error)
      }
    }

    // 点赞评论
    const likeComment = async (commentId: string) => {
      try {
        await commentStore.likeComment(commentId)
      } catch (error) {
        console.error('Failed to like comment:', error)
      }
    }

    // 举报评论
    const reportComment = async (commentId: string, reason: number, description?: string) => {
      try {
        await commentStore.reportComment(commentId, {
          commentId,
          reason,
          description
        })
      } catch (error) {
        console.error('Failed to report comment:', error)
      }
    }

    // 组件挂载时加载评论
    onMounted(() => {
      loadComments()
    })

    return {
      newCommentContent,
      isLoading,
      hasError,
      comments,
      hasMoreComments,
      loadComments,
      createComment,
      loadMoreComments,
      likeComment,
      reportComment
    }
  }
})

/**
 * 高级评论管理组件示例
 */
export const CommentManagement = defineComponent({
  name: 'CommentManagement',
  setup() {
    const commentStore = useCommentStore()
    const selectedComments = ref<string[]>([])
    const sortBy = ref<CommentSort>(CommentSort.CREATED_AT_DESC)
    const searchKeyword = ref('')

    // 计算属性
    const sortedComments = computed(() => commentStore.sortedComments)
    const commentTree = computed(() => commentStore.commentTree)
    const isLoading = computed(() => commentStore.isLoading)
    const hasError = computed(() => commentStore.hasError)

    // 筛选评论
    const filterComments = async (filter: Partial<CommentFilter>) => {
      try {
        await commentStore.updateFilter(filter)
      } catch (error) {
        console.error('Failed to filter comments:', error)
      }
    }

    // 搜索评论
    const searchComments = async () => {
      if (!searchKeyword.value.trim()) return
      
      try {
        await commentStore.searchComments(searchKeyword.value)
      } catch (error) {
        console.error('Failed to search comments:', error)
      }
    }

    // 批量操作评论（管理员功能）
    const batchDeleteComments = async () => {
      if (selectedComments.value.length === 0) return
      
      try {
        await commentStore.batchOperationComments(
          CommentOperation.DELETE,
          selectedComments.value,
          '批量删除违规评论'
        )
        selectedComments.value = []
      } catch (error) {
        console.error('Failed to batch delete comments:', error)
      }
    }

    // 防抖搜索
    const debouncedSearch = commentStore.debouncedSearch

    // 监听搜索关键词变化
    const handleSearchInput = (keyword: string) => {
      searchKeyword.value = keyword
      if (keyword.trim()) {
        debouncedSearch(keyword)
      }
    }

    // 获取持久化统计信息
    const getPersistenceStats = () => {
      return commentStore.getPersistenceStats()
    }

    // 清理持久化数据
    const cleanupPersistedData = async () => {
      try {
        const success = await commentStore.cleanupPersistedData()
        if (success) {
          console.log('Successfully cleaned up persisted data')
        }
      } catch (error) {
        console.error('Failed to cleanup persisted data:', error)
      }
    }

    return {
      selectedComments,
      sortBy,
      searchKeyword,
      sortedComments,
      commentTree,
      isLoading,
      hasError,
      filterComments,
      searchComments,
      handleSearchInput,
      batchDeleteComments,
      getPersistenceStats,
      cleanupPersistedData
    }
  }
})

/**
 * 评论统计组件示例
 */
export const CommentStats = defineComponent({
  name: 'CommentStats',
  props: {
    snippetId: {
      type: String,
      required: true
    }
  },
  setup(props) {
    const commentStore = useCommentStore()
    const stats = computed(() => commentStore.stats)
    const statsDetail = computed(() => commentStore.statsDetail)
    const isLoading = computed(() => commentStore.statsLoading)
    const hasError = computed(() => commentStore.statsError)

    // 加载评论统计
    const loadStats = async () => {
      try {
        await commentStore.fetchCommentStats(props.snippetId)
      } catch (error) {
        console.error('Failed to load comment stats:', error)
      }
    }

    // 加载详细统计
    const loadDetailedStats = async () => {
      try {
        await commentStore.fetchCommentStatsDetail({
          snippetIds: [props.snippetId],
          groupByDay: true,
          groupByUser: true
        })
      } catch (error) {
        console.error('Failed to load detailed stats:', error)
      }
    }

    // 组件挂载时加载统计
    onMounted(() => {
      loadStats()
    })

    return {
      stats,
      statsDetail,
      isLoading,
      hasError,
      loadStats,
      loadDetailedStats
    }
  }
})

/**
 * 最佳实践示例
 */

// 1. 错误处理最佳实践
export const useCommentWithErrorHandling = () => {
  const commentStore = useCommentStore()
  const error = ref<string | null>(null)

  const safeOperation = async <T>(
    operation: () => Promise<T>,
    errorMessage: string
  ): Promise<T | null> => {
    try {
      error.value = null
      return await operation()
    } catch (err) {
      error.value = errorMessage
      console.error(errorMessage, err)
      return null
    }
  }

  const createCommentSafely = async (commentData: any) => {
    return safeOperation(
      () => commentStore.createComment(commentData),
      'Failed to create comment'
    )
  }

  const likeCommentSafely = async (commentId: string) => {
    return safeOperation(
      () => commentStore.likeComment(commentId),
      'Failed to like comment'
    )
  }

  return {
    error,
    createCommentSafely,
    likeCommentSafely
  }
}

// 2. 性能优化最佳实践
export const useOptimizedComments = () => {
  const commentStore = useCommentStore()
  const isAutoRefreshEnabled = ref(false)
  let refreshInterval: number | null = null

  // 自动刷新评论
  const startAutoRefresh = (interval: number = 30000) => {
    if (isAutoRefreshEnabled.value) return
    
    isAutoRefreshEnabled.value = true
    refreshInterval = window.setInterval(() => {
      // 只在用户活跃时刷新
      if (document.hasFocus()) {
        commentStore.fetchComments()
      }
    }, interval)
  }

  const stopAutoRefresh = () => {
    if (refreshInterval) {
      clearInterval(refreshInterval)
      refreshInterval = null
    }
    isAutoRefreshEnabled.value = false
  }

  // 组件卸载时清理
  onUnmounted(() => {
    stopAutoRefresh()
  })

  return {
    isAutoRefreshEnabled,
    startAutoRefresh,
    stopAutoRefresh
  }
}

// 3. 缓存使用最佳实践
export const useCommentCache = () => {
  const commentStore = useCommentStore()

  const getCachedComment = (commentId: string): Comment | null => {
    return commentStore.getCache(commentId)
  }

  const prefetchComment = async (commentId: string) => {
    // 如果评论不在缓存中，预加载
    if (!getCachedComment(commentId)) {
      try {
        await commentStore.fetchComment(commentId)
      } catch (error) {
        console.error('Failed to prefetch comment:', error)
      }
    }
  }

  const clearCommentCache = (commentId?: string) => {
    commentStore.clearCache(commentId)
  }

  return {
    getCachedComment,
    prefetchComment,
    clearCommentCache
  }
}

// 4. 权限检查最佳实践
export const useCommentPermissions = (snippetId: string) => {
  const commentStore = useCommentStore()
  const permissions = computed(() => commentStore.permissions)
  const isLoading = computed(() => commentStore.permissionsLoading)

  const loadPermissions = async () => {
    try {
      await commentStore.fetchCommentPermissions(snippetId)
    } catch (error) {
      console.error('Failed to load comment permissions:', error)
    }
  }

  const canCreateComment = computed(() => permissions.value?.canCreate || false)
  const canEditComment = computed(() => permissions.value?.canEdit || false)
  const canDeleteComment = computed(() => permissions.value?.canDelete || false)
  const canReportComment = computed(() => permissions.value?.canReport || false)
  const canLikeComment = computed(() => permissions.value?.canLike || false)
  const canModerateComment = computed(() => permissions.value?.canModerate || false)

  onMounted(() => {
    loadPermissions()
  })

  return {
    permissions,
    isLoading,
    canCreateComment,
    canEditComment,
    canDeleteComment,
    canReportComment,
    canLikeComment,
    canModerateComment,
    loadPermissions
  }
}

// 5. 通知管理最佳实践
export const useCommentNotifications = () => {
  const commentStore = useCommentStore()
  const notifications = computed(() => commentStore.notifications)
  const unreadCount = computed(() => commentStore.unreadNotificationCount)

  const markNotificationsAsRead = () => {
    // 这里可以实现标记通知为已读的逻辑
    commentStore.unreadNotificationCount = 0
  }

  const clearNotifications = () => {
    commentStore.notifications = []
    commentStore.unreadNotificationCount = 0
  }

  return {
    notifications,
    unreadCount,
    markNotificationsAsRead,
    clearNotifications
  }
}

// 导出所有示例组件和组合函数
export default {
  CommentSection,
  CommentManagement,
  CommentStats,
  useCommentWithErrorHandling,
  useOptimizedComments,
  useCommentCache,
  useCommentPermissions,
  useCommentNotifications
}