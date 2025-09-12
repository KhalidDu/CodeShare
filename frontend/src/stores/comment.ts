/**
 * 评论Pinia Store - 实现完整的评论状态管理
 * 
 * 本文件提供评论系统的前端状态管理，包括：
 * - 评论CRUD操作
 * - 评论点赞管理
 * - 评论举报管理
 * - 评论筛选和分页状态
 * - 评论缓存和性能优化
 * - 持久化存储支持
 * - 完整的错误处理机制
 * 
 * 遵循Vue 3 + Pinia开发模式，使用TypeScript确保类型安全
 * 支持FR-01和FE-07需求规格
 */

import { defineStore } from 'pinia'
import { ref, computed, reactive, watch } from 'vue'
import { commentService } from '@/services/commentService'
import type {
  Comment,
  CreateCommentRequest,
  UpdateCommentRequest,
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
  CreateCommentReportRequest,
  ModerateCommentRequest,
  HandleCommentReportRequest,
  CommentReport,
  CommentLikeResponse,
  CommentStatus,
  CommentOperation,
  CommentBatchOperationType,
  BatchOperationResult,
  CommentNotification,
  CommentNotificationSettings,
  CommentSearchResult
} from '@/types/comment'

import {
  CommentSort,
  ReportReason,
  ReportStatus
} from '@/types/comment'

// 导入持久化类型
import type { PersistedCommentState } from './commentPersistence'

/**
 * 评论状态管理 - 使用Pinia进行状态管理
 */
export const useCommentStore = defineStore('comment', () => {
  // 核心状态
  const comments = ref<Comment[]>([])
  const currentComment = ref<Comment | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const initialized = ref(false)
  const lastUpdated = ref<string | null>(null)

  // 评论筛选和分页状态
  const filter = reactive<CommentFilter>({
    sortBy: CommentSort.CREATED_AT_DESC,
    page: 1,
    pageSize: 20,
    includeUser: true,
    includeReplies: true,
    includeLikes: true
  })

  const pagination = ref({
    totalCount: 0,
    currentPage: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  })

  // 评论点赞状态
  const likes = ref<CommentLikeResponse[]>([])
  const likeStats = ref<{ [commentId: string]: { count: number; isLiked: boolean } }>({})
  const likesLoading = ref(false)
  const likesError = ref<string | null>(null)

  // 评论举报状态
  const reports = ref<CommentReport[]>([])
  const myReports = ref<CommentReport[]>([])
  const reportsLoading = ref(false)
  const reportsError = ref<string | null>(null)

  // 评论统计状态
  const stats = ref<CommentStats | null>(null)
  const statsDetail = ref<CommentStatsDetail | null>(null)
  const statsLoading = ref(false)
  const statsError = ref<string | null>(null)

  // 评论权限状态
  const permissions = ref<CommentPermissions | null>(null)
  const permissionsLoading = ref(false)
  const permissionsError = ref<string | null>(null)

  // 评论搜索状态
  const searchResults = ref<CommentSearchResult[]>([])
  const searchLoading = ref(false)
  const searchError = ref<string | null>(null)
  const searchFilter = reactive<CommentSearchFilter>({
    keyword: '',
    searchScope: 0, // CommentSearchScope.ALL
    sortBy: 0, // CommentSearchSort.RELEVANCE
    page: 1,
    pageSize: 20
  })

  // 评论缓存状态
  const cache = ref<Map<string, Comment>>(new Map())
  const cacheTimestamp = ref<{ [key: string]: number }>({})
  const CACHE_DURATION = 5 * 60 * 1000 // 5分钟缓存

  // 评论通知状态
  const notifications = ref<CommentNotification[]>([])
  const notificationSettings = ref<CommentNotificationSettings | null>(null)
  const unreadNotificationCount = ref(0)

  // 评论操作状态
  const operationLoading = ref<{ [key: string]: boolean }>({})
  const operationError = ref<{ [key: string]: string | null }>({})

  // 计算属性
  const isLoading = computed(() => loading.value)
  const hasError = computed(() => error.value !== null)
  const isInitialized = computed(() => initialized.value)
  const hasComments = computed(() => comments.value.length > 0)
  const hasMoreComments = computed(() => pagination.value.hasNextPage)

  const sortedComments = computed(() => {
    return [...comments.value].sort((a, b) => {
      switch (filter.sortBy) {
        case CommentSort.CREATED_AT_DESC:
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        case CommentSort.CREATED_AT_ASC:
          return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
        case CommentSort.LIKE_COUNT_DESC:
          return b.likeCount - a.likeCount
        case CommentSort.LIKE_COUNT_ASC:
          return a.likeCount - b.likeCount
        case CommentSort.REPLY_COUNT_DESC:
          return b.replyCount - a.replyCount
        case CommentSort.REPLY_COUNT_ASC:
          return a.replyCount - b.replyCount
        default:
          return 0
      }
    })
  })

  const commentTree = computed(() => {
    const commentMap = new Map<string, CommentTreeNode>()
    const rootComments: CommentTreeNode[] = []

    // 创建所有评论节点
    sortedComments.value.forEach(comment => {
      commentMap.set(comment.id, {
        comment,
        children: []
      })
    })

    // 构建树形结构
    sortedComments.value.forEach(comment => {
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

  const hasActiveCache = computed(() => {
    const now = Date.now()
    return Object.values(cacheTimestamp.value).some(timestamp => 
      now - timestamp < CACHE_DURATION
    )
  })

  const isOperationLoading = computed(() => (operationId: string) => {
    return operationLoading.value[operationId] || false
  })

  const getOperationError = computed(() => (operationId: string) => {
    return operationError.value[operationId] || null
  })

  // 防抖函数
  const debounce = <T extends (...args: any[]) => any>(
    func: T,
    delay: number
  ): (...args: Parameters<T>) => void => {
    let timeoutId: NodeJS.Timeout
    return (...args: Parameters<T>) => {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(null, args), delay)
    }
  }

  // 缓存操作
  const setCache = (key: string, data: Comment) => {
    cache.value.set(key, data)
    cacheTimestamp.value[key] = Date.now()
  }

  const getCache = (key: string): Comment | null => {
    const cached = cache.value.get(key)
    if (cached && cacheTimestamp.value[key]) {
      const now = Date.now()
      if (now - cacheTimestamp.value[key] < CACHE_DURATION) {
        return cached
      }
    }
    return null
  }

  const clearCache = (key?: string) => {
    if (key) {
      cache.value.delete(key)
      delete cacheTimestamp.value[key]
    } else {
      cache.value.clear()
      Object.keys(cacheTimestamp.value).forEach(k => {
        delete cacheTimestamp.value[k]
      })
    }
  }

  // 基础评论操作
  async function fetchComments(fetchFilter?: Partial<CommentFilter>) {
    try {
      loading.value = true
      error.value = null
      
      const mergedFilter = { ...filter, ...fetchFilter }
      const result = await commentService.getComments(mergedFilter)
      
      comments.value = result.items
      pagination.value = {
        totalCount: result.totalCount,
        currentPage: result.page,
        pageSize: result.pageSize,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
        hasNextPage: result.page < Math.ceil(result.totalCount / result.pageSize),
        hasPreviousPage: result.page > 1
      }
      
      // 缓存评论
      result.items.forEach(comment => {
        setCache(comment.id, comment)
      })
      
      initialized.value = true
      lastUpdated.value = new Date().toISOString()
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取评论列表失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchMoreComments() {
    if (loading.value || !hasMoreComments.value) return

    try {
      loading.value = true
      error.value = null
      
      const newFilter = { ...filter, page: pagination.value.currentPage + 1 }
      const result = await commentService.getComments(newFilter)
      
      comments.value = [...comments.value, ...result.items]
      pagination.value = {
        totalCount: result.totalCount,
        currentPage: result.page,
        pageSize: result.pageSize,
        totalPages: Math.ceil(result.totalCount / result.pageSize),
        hasNextPage: result.page < Math.ceil(result.totalCount / result.pageSize),
        hasPreviousPage: result.page > 1
      }
      
      // 缓存新评论
      result.items.forEach(comment => {
        setCache(comment.id, comment)
      })
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载更多评论失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchComment(commentId: string) {
    try {
      loading.value = true
      error.value = null
      
      // 检查缓存
      const cached = getCache(commentId)
      if (cached) {
        currentComment.value = cached
        return cached
      }
      
      const result = await commentService.getComment(commentId)
      currentComment.value = result
      
      // 缓存评论
      setCache(commentId, result)
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '获取评论详情失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createComment(commentData: CreateCommentRequest) {
    try {
      const operationId = `create-${Date.now()}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.createComment(commentData)
      
      // 添加到评论列表
      if (filter.snippetId === commentData.snippetId) {
        comments.value.unshift(result)
        pagination.value.totalCount++
      }
      
      // 缓存新评论
      setCache(result.id, result)
      
      // 更新统计信息
      if (stats.value && stats.value.snippetId === commentData.snippetId) {
        stats.value.totalComments++
        if (commentData.parentId) {
          stats.value.replyComments++
        } else {
          stats.value.rootComments++
        }
      }
      
      return result
    } catch (err) {
      const operationId = `create-${Date.now()}`
      operationError.value[operationId] = err instanceof Error ? err.message : '创建评论失败'
      throw err
    } finally {
      const operationId = `create-${Date.now()}`
      operationLoading.value[operationId] = false
    }
  }

  async function updateComment(commentId: string, updateData: UpdateCommentRequest) {
    try {
      const operationId = `update-${commentId}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.updateComment(commentId, updateData)
      
      // 更新评论列表
      const commentIndex = comments.value.findIndex(c => c.id === commentId)
      if (commentIndex !== -1) {
        comments.value[commentIndex] = result
      }
      
      // 更新当前评论
      if (currentComment.value?.id === commentId) {
        currentComment.value = result
      }
      
      // 更新缓存
      setCache(commentId, result)
      
      return result
    } catch (err) {
      const operationId = `update-${commentId}`
      operationError.value[operationId] = err instanceof Error ? err.message : '更新评论失败'
      throw err
    } finally {
      const operationId = `update-${commentId}`
      operationLoading.value[operationId] = false
    }
  }

  async function deleteComment(commentId: string) {
    try {
      const operationId = `delete-${commentId}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      await commentService.deleteComment(commentId)
      
      // 从评论列表中移除
      comments.value = comments.value.filter(c => c.id !== commentId)
      pagination.value.totalCount--
      
      // 清除当前评论
      if (currentComment.value?.id === commentId) {
        currentComment.value = null
      }
      
      // 清除缓存
      clearCache(commentId)
      
      // 更新统计信息
      if (stats.value) {
        stats.value.totalComments--
      }
      
      return true
    } catch (err) {
      const operationId = `delete-${commentId}`
      operationError.value[operationId] = err instanceof Error ? err.message : '删除评论失败'
      throw err
    } finally {
      const operationId = `delete-${commentId}`
      operationLoading.value[operationId] = false
    }
  }

  // 评论点赞操作
  async function likeComment(commentId: string) {
    try {
      const operationId = `like-${commentId}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.likeComment(commentId)
      
      // 更新点赞状态
      likeStats.value[commentId] = {
        count: result.likeCount,
        isLiked: result.isLiked
      }
      
      // 更新评论列表中的点赞信息
      const commentIndex = comments.value.findIndex(c => c.id === commentId)
      if (commentIndex !== -1) {
        comments.value[commentIndex].likeCount = result.likeCount
        comments.value[commentIndex].isLikedByCurrentUser = result.isLiked
      }
      
      // 更新当前评论的点赞信息
      if (currentComment.value?.id === commentId) {
        currentComment.value.likeCount = result.likeCount
        currentComment.value.isLikedByCurrentUser = result.isLiked
      }
      
      // 更新缓存
      const cached = getCache(commentId)
      if (cached) {
        cached.likeCount = result.likeCount
        cached.isLikedByCurrentUser = result.isLiked
        setCache(commentId, cached)
      }
      
      return result
    } catch (err) {
      const operationId = `like-${commentId}`
      operationError.value[operationId] = err instanceof Error ? err.message : '点赞评论失败'
      throw err
    } finally {
      const operationId = `like-${commentId}`
      operationLoading.value[operationId] = false
    }
  }

  async function fetchCommentLikes(commentId: string, likeFilter?: CommentLikeFilter) {
    try {
      likesLoading.value = true
      likesError.value = null
      
      const result = await commentService.getCommentLikes(commentId, {
        page: 1,
        pageSize: 20,
        ...likeFilter
      })
      
      likes.value = result.items
      
      return result
    } catch (err) {
      likesError.value = err instanceof Error ? err.message : '获取评论点赞列表失败'
      throw err
    } finally {
      likesLoading.value = false
    }
  }

  async function checkCommentLikeStatus(commentId: string) {
    try {
      const result = await commentService.getCommentLikeStatus(commentId)
      likeStats.value[commentId] = {
        count: likeStats.value[commentId]?.count || 0,
        isLiked: result.isLiked
      }
      return result
    } catch (err) {
      likesError.value = err instanceof Error ? err.message : '检查点赞状态失败'
      throw err
    }
  }

  // 评论举报操作
  async function reportComment(commentId: string, reportData: CreateCommentReportRequest) {
    try {
      const operationId = `report-${commentId}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.reportComment(commentId, reportData)
      
      // 添加到我的举报列表
      myReports.value.unshift(result)
      
      return result
    } catch (err) {
      const operationId = `report-${commentId}`
      operationError.value[operationId] = err instanceof Error ? err.message : '举报评论失败'
      throw err
    } finally {
      const operationId = `report-${commentId}`
      operationLoading.value[operationId] = false
    }
  }

  async function fetchMyReports(reportFilter?: Partial<CommentReportFilter>) {
    try {
      reportsLoading.value = true
      reportsError.value = null
      
      const result = await commentService.getMyCommentReports({
        sortBy: 0, // ReportSort.CREATED_AT_DESC
        page: 1,
        pageSize: 20,
        ...reportFilter
      })
      
      myReports.value = result.items
      
      return result
    } catch (err) {
      reportsError.value = err instanceof Error ? err.message : '获取我的举报列表失败'
      throw err
    } finally {
      reportsLoading.value = false
    }
  }

  // 评论统计操作
  async function fetchCommentStats(snippetId: string) {
    try {
      statsLoading.value = true
      statsError.value = null
      
      const result = await commentService.getCommentStats(snippetId)
      stats.value = result
      
      return result
    } catch (err) {
      statsError.value = err instanceof Error ? err.message : '获取评论统计失败'
      throw err
    } finally {
      statsLoading.value = false
    }
  }

  async function fetchCommentStatsDetail(statsFilter?: any) {
    try {
      statsLoading.value = true
      statsError.value = null
      
      const result = await commentService.getCommentStatsDetail(statsFilter || {})
      statsDetail.value = result
      
      return result
    } catch (err) {
      statsError.value = err instanceof Error ? err.message : '获取评论统计详情失败'
      throw err
    } finally {
      statsLoading.value = false
    }
  }

  // 评论权限操作
  async function fetchCommentPermissions(snippetId: string) {
    try {
      permissionsLoading.value = true
      permissionsError.value = null
      
      const result = await commentService.getCommentPermissions(snippetId)
      permissions.value = result
      
      return result
    } catch (err) {
      permissionsError.value = err instanceof Error ? err.message : '获取评论权限失败'
      throw err
    } finally {
      permissionsLoading.value = false
    }
  }

  // 评论搜索操作
  async function searchComments(searchKeyword: string, searchFilter?: Partial<CommentSearchFilter>) {
    try {
      searchLoading.value = true
      searchError.value = null
      
      const mergedFilter = {
        ...searchFilter,
        keyword: searchKeyword,
        searchScope: 0, // CommentSearchScope.ALL
        sortBy: 0, // CommentSearchSort.RELEVANCE
        page: 1,
        pageSize: 20
      }
      
      const result = await commentService.searchComments(mergedFilter)
      searchResults.value = result.items
      
      return result
    } catch (err) {
      searchError.value = err instanceof Error ? err.message : '搜索评论失败'
      throw err
    } finally {
      searchLoading.value = false
    }
  }

  // 筛选和排序操作
  function updateFilter(newFilter: Partial<CommentFilter>) {
    Object.assign(filter, newFilter)
  }

  function resetFilter() {
    Object.assign(filter, {
      sortBy: CommentSort.CREATED_AT_DESC,
      page: 1,
      pageSize: 20,
      includeUser: true,
      includeReplies: true,
      includeLikes: true
    })
  }

  function updateSearchFilter(newFilter: Partial<CommentSearchFilter>) {
    Object.assign(searchFilter, newFilter)
  }

  function resetSearchFilter() {
    Object.assign(searchFilter, {
      keyword: '',
      searchScope: 0,
      sortBy: 0,
      page: 1,
      pageSize: 20
    })
    searchResults.value = []
  }

  // 批量操作（管理员功能）
  async function batchOperationComments(operation: CommentOperation, commentIds: string[], reason?: string) {
    try {
      const operationId = `batch-${Date.now()}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.batchOperationComments({
        commentIds,
        operation,
        reason
      })
      
      // 更新评论列表
      comments.value = comments.value.filter(comment => !commentIds.includes(comment.id))
      
      // 清除相关缓存
      commentIds.forEach(id => clearCache(id))
      
      return result
    } catch (err) {
      const operationId = `batch-${Date.now()}`
      operationError.value[operationId] = err instanceof Error ? err.message : '批量操作失败'
      throw err
    } finally {
      const operationId = `batch-${Date.now()}`
      operationLoading.value[operationId] = false
    }
  }

  // 防抖的搜索函数
  const debouncedSearch = debounce(searchComments, 300)

  // 监听筛选条件变化
  watch(filter, () => {
    fetchComments()
  }, { deep: true })

  // 清理状态
  function $reset() {
    comments.value = []
    currentComment.value = null
    loading.value = false
    error.value = null
    initialized.value = false
    lastUpdated.value = null
    
    Object.assign(filter, {
      sortBy: CommentSort.CREATED_AT_DESC,
      page: 1,
      pageSize: 20,
      includeUser: true,
      includeReplies: true,
      includeLikes: true
    })
    
    pagination.value = {
      totalCount: 0,
      currentPage: 1,
      pageSize: 20,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false
    }
    
    likes.value = []
    likeStats.value = {}
    likesLoading.value = false
    likesError.value = null
    
    reports.value = []
    myReports.value = []
    reportsLoading.value = false
    reportsError.value = null
    
    stats.value = null
    statsDetail.value = null
    statsLoading.value = false
    statsError.value = null
    
    permissions.value = null
    permissionsLoading.value = false
    permissionsError.value = null
    
    searchResults.value = []
    searchLoading.value = false
    searchError.value = null
    
    Object.assign(searchFilter, {
      keyword: '',
      searchScope: 0,
      sortBy: 0,
      page: 1,
      pageSize: 20
    })
    
    cache.value.clear()
    Object.keys(cacheTimestamp.value).forEach(k => {
      delete cacheTimestamp.value[k]
    })
    
    notifications.value = []
    notificationSettings.value = null
    unreadNotificationCount.value = 0
    
    operationLoading.value = {}
    operationError.value = {}
  }

  return {
    // 状态
    comments,
    currentComment,
    loading,
    error,
    initialized,
    lastUpdated,
    filter,
    pagination,
    likes,
    likeStats,
    likesLoading,
    likesError,
    reports,
    myReports,
    reportsLoading,
    reportsError,
    stats,
    statsDetail,
    statsLoading,
    statsError,
    permissions,
    permissionsLoading,
    permissionsError,
    searchResults,
    searchLoading,
    searchError,
    searchFilter,
    cache,
    notifications,
    notificationSettings,
    unreadNotificationCount,
    operationLoading,
    operationError,
    
    // 计算属性
    isLoading,
    hasError,
    isInitialized,
    hasComments,
    hasMoreComments,
    sortedComments,
    commentTree,
    hasActiveCache,
    isOperationLoading,
    getOperationError,
    
    // 方法
    fetchComments,
    fetchMoreComments,
    fetchComment,
    createComment,
    updateComment,
    deleteComment,
    likeComment,
    fetchCommentLikes,
    checkCommentLikeStatus,
    reportComment,
    fetchMyReports,
    fetchCommentStats,
    fetchCommentStatsDetail,
    fetchCommentPermissions,
    searchComments,
    debouncedSearch,
    updateFilter,
    resetFilter,
    updateSearchFilter,
    resetSearchFilter,
    batchOperationComments,
    setCache,
    getCache,
    clearCache,
    
    // 持久化存储方法（由插件动态添加）
    resetPersistedData: () => {},
    getPersistenceStats: () => ({}),
    cleanupPersistedData: () => false,
    saveCurrentState: () => {},
    
    $reset
  }
})

// 评论树节点接口
interface CommentTreeNode {
  comment: Comment
  children: CommentTreeNode[]
}