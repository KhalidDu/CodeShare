/**
 * 评论组合式API服务
 * 
 * 本文件提供评论系统的组合式API服务，便于在Vue组件中使用：
 * - 响应式状态管理
 * - 自动加载和刷新
 * - 错误处理和加载状态
 * - 缓存和性能优化
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import { ref, reactive, computed, watch } from 'vue'
import { commentService } from './commentService'
import type {
  Comment,
  CommentFilter,
  CommentReportFilter,
  CommentLikeFilter,
  CommentStats,
  CommentStatsDetail,
  CommentSearchFilter,
  CommentPermissions,
  PaginatedComments,
  PaginatedCommentReports,
  PaginatedCommentLikes,
  PaginatedCommentSearchResults,
  CreateCommentRequest,
  UpdateCommentRequest,
  CreateCommentReportRequest,
  ModerateCommentRequest,
  HandleCommentReportRequest
} from '@/types/comment'

/**
 * 评论管理组合式API
 */
export function useComments() {
  // 状态管理
  const comments = ref<Comment[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(20)
  const filter = reactive<CommentFilter>({
    sortBy: 0, // CommentSort.CREATED_AT_DESC
    page: 1,
    pageSize: 20
  })

  // 计算属性
  const hasMore = computed(() => {
    return comments.value.length < totalCount.value
  })

  const isLoading = computed(() => loading.value)

  const hasError = computed(() => error.value !== null)

  // 加载评论列表
  const loadComments = async (newFilter?: Partial<CommentFilter>) => {
    try {
      loading.value = true
      error.value = null

      // 合并筛选条件
      const mergedFilter = { ...filter, ...newFilter }
      
      const result = await commentService.getComments(mergedFilter)
      
      comments.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = result.page
      pageSize.value = result.pageSize
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 加载更多评论
  const loadMoreComments = async () => {
    if (loading.value || !hasMore.value) return

    try {
      loading.value = true
      const newFilter = { ...filter, page: currentPage.value + 1 }
      
      const result = await commentService.getComments(newFilter)
      
      comments.value = [...comments.value, ...result.items]
      totalCount.value = result.totalCount
      currentPage.value = result.page
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载更多评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 刷新评论列表
  const refreshComments = async () => {
    await loadComments()
  }

  // 重置筛选条件
  const resetFilter = () => {
    Object.assign(filter, {
      sortBy: 0,
      page: 1,
      pageSize: 20
    })
    comments.value = []
    totalCount.value = 0
    currentPage.value = 1
  }

  // 监听筛选条件变化
  watch(filter, () => {
    loadComments()
  }, { deep: true })

  return {
    // 状态
    comments,
    loading,
    error,
    totalCount,
    currentPage,
    pageSize,
    filter,
    
    // 计算属性
    hasMore,
    isLoading,
    hasError,
    
    // 方法
    loadComments,
    loadMoreComments,
    refreshComments,
    resetFilter
  }
}

/**
 * 评论详情组合式API
 */
export function useComment(commentId?: string) {
  // 状态管理
  const comment = ref<Comment | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 加载评论详情
  const loadComment = async (id: string) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.getComment(id)
      comment.value = result
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载评论详情失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 更新评论
  const updateComment = async (id: string, data: UpdateCommentRequest) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.updateComment(id, data)
      comment.value = result
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 删除评论
  const deleteComment = async (id: string) => {
    try {
      loading.value = true
      error.value = null
      
      await commentService.deleteComment(id)
      comment.value = null
      
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 点赞评论
  const likeComment = async (id: string) => {
    try {
      const result = await commentService.likeComment(id)
      
      if (comment.value && comment.value.id === id) {
        comment.value.likeCount = result.likeCount
        comment.value.isLikedByCurrentUser = result.isLiked
      }
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '点赞评论失败'
      throw err
    }
  }

  // 自动加载评论详情
  if (commentId) {
    loadComment(commentId)
  }

  return {
    // 状态
    comment,
    loading,
    error,
    
    // 方法
    loadComment,
    updateComment,
    deleteComment,
    likeComment
  }
}

/**
 * 评论点赞组合式API
 */
export function useCommentLikes(commentId?: string) {
  // 状态管理
  const likes = ref<any[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const isLiked = ref(false)

  // 加载点赞列表
  const loadLikes = async (id: string, filter?: CommentLikeFilter) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.getCommentLikes(id, {
        page: 1,
        pageSize: 20,
        ...filter
      })
      
      likes.value = result.items
      totalCount.value = result.totalCount
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载点赞列表失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 检查点赞状态
  const checkLikeStatus = async (id: string) => {
    try {
      const result = await commentService.getCommentLikeStatus(id)
      isLiked.value = result.isLiked
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '检查点赞状态失败'
      throw err
    }
  }

  // 点赞评论
  const likeComment = async (id: string) => {
    try {
      const result = await commentService.likeComment(id)
      isLiked.value = result.isLiked
      totalCount.value = result.likeCount
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '点赞评论失败'
      throw err
    }
  }

  return {
    // 状态
    likes,
    loading,
    error,
    totalCount,
    isLiked,
    
    // 方法
    loadLikes,
    checkLikeStatus,
    likeComment
  }
}

/**
 * 评论举报组合式API
 */
export function useCommentReports() {
  // 状态管理
  const reports = ref<any[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const filter = reactive<CommentReportFilter>({
    sortBy: 0, // ReportSort.CREATED_AT_DESC
    page: 1,
    pageSize: 20
  })

  // 加载举报列表
  const loadReports = async (newFilter?: Partial<CommentReportFilter>) => {
    try {
      loading.value = true
      error.value = null
      
      const mergedFilter = { ...filter, ...newFilter }
      const result = await commentService.getMyCommentReports(mergedFilter)
      
      reports.value = result.items
      totalCount.value = result.totalCount
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载举报列表失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 举报评论
  const reportComment = async (commentId: string, data: CreateCommentReportRequest) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.reportComment(commentId, data)
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '举报评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // 状态
    reports,
    loading,
    error,
    totalCount,
    filter,
    
    // 方法
    loadReports,
    reportComment
  }
}

/**
 * 评论统计组合式API
 */
export function useCommentStats() {
  // 状态管理
  const stats = ref<CommentStats | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 加载统计信息
  const loadStats = async (snippetId: string) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.getCommentStats(snippetId)
      stats.value = result
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载统计信息失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // 状态
    stats,
    loading,
    error,
    
    // 方法
    loadStats
  }
}

/**
 * 评论权限组合式API
 */
export function useCommentPermissions() {
  // 状态管理
  const permissions = ref<CommentPermissions | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 加载权限信息
  const loadPermissions = async (snippetId: string) => {
    try {
      loading.value = true
      error.value = null
      
      const result = await commentService.getCommentPermissions(snippetId)
      permissions.value = result
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载权限信息失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // 状态
    permissions,
    loading,
    error,
    
    // 方法
    loadPermissions
  }
}