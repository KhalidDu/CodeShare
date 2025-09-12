/**
 * CodeShare项目评论系统Pinia状态管理
 * 
 * 本文件提供评论系统的前端状态管理，包含完整的功能实现：
 * - 评论状态管理
 * - 评论CRUD操作
 * - 评论互动功能
 * - 评论筛选和搜索
 * - 评论统计信息
 * - 评论管理功能
 * - 缓存和性能优化
 * - 实时更新支持
 * 
 * 遵循Vue 3 + Pinia开发模式，使用TypeScript确保类型安全
 * 支持完整的评论系统需求
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
  CommentSearchResult,
  CommentTreeNode,
  CommentSort,
  ReportReason,
  ReportStatus,
  CommentSearchScope,
  CommentSearchSort
} from '@/types/comment'

// 评论状态管理Store
export const useCommentStore = defineStore('commentStore', () => {
  // ==================== 核心评论状态 ====================
  
  // 评论数据状态
  const comments = ref<Comment[]>([])
  const currentComment = ref<Comment | null>(null)
  const commentTree = ref<CommentTreeNode[]>([])
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
  
  // ==================== 评论互动状态 ====================
  
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
  
  // ==================== 评论统计状态 ====================
  
  // 评论统计信息
  const stats = ref<CommentStats | null>(null)
  const statsDetail = ref<CommentStatsDetail | null>(null)
  const statsLoading = ref(false)
  const statsError = ref<string | null>(null)
  
  // ==================== 评论权限状态 ====================
  
  // 评论权限
  const permissions = ref<CommentPermissions | null>(null)
  const permissionsLoading = ref(false)
  const permissionsError = ref<string | null>(null)
  
  // ==================== 评论搜索状态 ====================
  
  // 评论搜索
  const searchResults = ref<CommentSearchResult[]>([])
  const searchLoading = ref(false)
  const searchError = ref<string | null>(null)
  const searchFilter = reactive<CommentSearchFilter>({
    keyword: '',
    searchScope: CommentSearchScope.ALL,
    sortBy: CommentSearchSort.RELEVANCE,
    page: 1,
    pageSize: 20
  })
  
  // ==================== 缓存和性能状态 ====================
  
  // 评论缓存
  const cache = ref<Map<string, Comment>>(new Map())
  const cacheTimestamp = ref<{ [key: string]: number }>({})
  const CACHE_DURATION = 5 * 60 * 1000 // 5分钟缓存
  
  // ==================== 评论通知状态 ====================
  
  // 评论通知
  const notifications = ref<CommentNotification[]>([])
  const notificationSettings = ref<CommentNotificationSettings | null>(null)
  const unreadNotificationCount = ref(0)
  
  // ==================== 操作状态 ====================
  
  // 操作状态跟踪
  const operationLoading = ref<{ [key: string]: boolean }>({})
  const operationError = ref<{ [key: string]: string | null }>({})
  
  // ==================== 实时更新状态 ====================
  
  // WebSocket连接状态
  const websocketConnection = ref<WebSocket | null>(null)
  const isRealtimeConnected = ref(false)
  
  // ==================== 计算属性 ====================
  
  // 基础状态计算
  const isLoading = computed(() => loading.value)
  const hasError = computed(() => error.value !== null)
  const isInitialized = computed(() => initialized.value)
  const hasComments = computed(() => comments.value.length > 0)
  const hasMoreComments = computed(() => pagination.value.hasNextPage)
  
  // 评论排序计算
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
  
  // 评论树形结构计算
  const buildCommentTree = computed(() => {
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
  
  // 缓存状态计算
  const hasActiveCache = computed(() => {
    const now = Date.now()
    return Object.values(cacheTimestamp.value).some(timestamp => 
      now - timestamp < CACHE_DURATION
    )
  })
  
  // 操作状态计算
  const isOperationLoading = computed(() => (operationId: string) => {
    return operationLoading.value[operationId] || false
  })
  
  const getOperationError = computed(() => (operationId: string) => {
    return operationError.value[operationId] || null
  })
  
  // 评论统计计算
  const totalComments = computed(() => stats.value?.totalComments || 0)
  const totalLikes = computed(() => stats.value?.totalLikes || 0)
  const activeUsers = computed(() => stats.value?.activeUsers || 0)
  
  // 通知状态计算
  const unreadNotifications = computed(() => 
    notifications.value.filter(n => !n.isRead)
  )
  
  // ==================== 工具函数 ====================
  
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
  
  // 缓存操作函数
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
  
  // ==================== 评论CRUD操作 ====================
  
  /**
   * 获取评论列表
   * @param fetchFilter 可选的筛选条件
   */
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
  
  /**
   * 加载更多评论
   */
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
  
  /**
   * 获取单个评论详情
   * @param commentId 评论ID
   */
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
  
  /**
   * 创建新评论
   * @param commentData 评论数据
   */
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
  
  /**
   * 更新评论
   * @param commentId 评论ID
   * @param updateData 更新数据
   */
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
  
  /**
   * 删除评论
   * @param commentId 评论ID
   */
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
  
  // ==================== 评论互动功能 ====================
  
  /**
   * 评论点赞
   * @param commentId 评论ID
   */
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
  
  /**
   * 获取评论点赞列表
   * @param commentId 评论ID
   * @param likeFilter 点赞筛选条件
   */
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
  
  /**
   * 检查评论点赞状态
   * @param commentId 评论ID
   */
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
  
  /**
   * 举报评论
   * @param commentId 评论ID
   * @param reportData 举报数据
   */
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
  
  /**
   * 获取我的举报列表
   * @param reportFilter 举报筛选条件
   */
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
  
  // ==================== 评论统计功能 ====================
  
  /**
   * 获取评论统计信息
   * @param snippetId 代码片段ID
   */
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
  
  /**
   * 获取评论统计详情
   * @param statsFilter 统计筛选条件
   */
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
  
  // ==================== 评论权限功能 ====================
  
  /**
   * 获取评论权限
   * @param snippetId 代码片段ID
   */
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
  
  // ==================== 评论搜索功能 ====================
  
  /**
   * 搜索评论
   * @param searchKeyword 搜索关键词
   * @param searchFilter 搜索筛选条件
   */
  async function searchComments(searchKeyword: string, searchFilter?: Partial<CommentSearchFilter>) {
    try {
      searchLoading.value = true
      searchError.value = null
      
      const mergedFilter = {
        ...searchFilter,
        keyword: searchKeyword,
        searchScope: CommentSearchScope.ALL,
        sortBy: CommentSearchSort.RELEVANCE,
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
  
  // ==================== 评论筛选功能 ====================
  
  /**
   * 更新筛选条件
   * @param newFilter 新的筛选条件
   */
  function updateFilter(newFilter: Partial<CommentFilter>) {
    Object.assign(filter, newFilter)
  }
  
  /**
   * 重置筛选条件
   */
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
  
  /**
   * 更新搜索筛选条件
   * @param newFilter 新的搜索筛选条件
   */
  function updateSearchFilter(newFilter: Partial<CommentSearchFilter>) {
    Object.assign(searchFilter, newFilter)
  }
  
  /**
   * 重置搜索筛选条件
   */
  function resetSearchFilter() {
    Object.assign(searchFilter, {
      keyword: '',
      searchScope: CommentSearchScope.ALL,
      sortBy: CommentSearchSort.RELEVANCE,
      page: 1,
      pageSize: 20
    })
    searchResults.value = []
  }
  
  // ==================== 评论管理功能 ====================
  
  /**
   * 批量操作评论
   * @param operation 操作类型
   * @param commentIds 评论ID列表
   * @param reason 操作原因
   */
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
  
  /**
   * 审核评论
   * @param commentId 评论ID
   * @param moderation 审核数据
   */
  async function moderateComment(commentId: string, moderation: ModerateCommentRequest) {
    try {
      const operationId = `moderate-${commentId}`
      operationLoading.value[operationId] = true
      operationError.value[operationId] = null
      
      const result = await commentService.moderateComment(commentId, moderation)
      
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
      const operationId = `moderate-${commentId}`
      operationError.value[operationId] = err instanceof Error ? err.message : '审核评论失败'
      throw err
    } finally {
      const operationId = `moderate-${commentId}`
      operationLoading.value[operationId] = false
    }
  }
  
  // ==================== 实时更新功能 ====================
  
  /**
   * 连接WebSocket
   */
  async function connectWebSocket() {
    try {
      if (websocketConnection.value && websocketConnection.value.readyState === WebSocket.OPEN) {
        return
      }
      
      // 创建WebSocket连接
      const wsUrl = `${import.meta.env.VITE_WS_URL}/comments`
      websocketConnection.value = new WebSocket(wsUrl)
      
      websocketConnection.value.onopen = () => {
        isRealtimeConnected.value = true
        console.log('评论WebSocket连接已建立')
      }
      
      websocketConnection.value.onmessage = (event) => {
        try {
          const update = JSON.parse(event.data)
          handleCommentRealtimeUpdate(update)
        } catch (err) {
          console.error('处理评论实时更新失败:', err)
        }
      }
      
      websocketConnection.value.onclose = () => {
        isRealtimeConnected.value = false
        console.log('评论WebSocket连接已关闭')
        
        // 5秒后重连
        setTimeout(connectWebSocket, 5000)
      }
      
      websocketConnection.value.onerror = (err) => {
        console.error('评论WebSocket连接错误:', err)
        isRealtimeConnected.value = false
      }
    } catch (err) {
      console.error('连接评论WebSocket失败:', err)
      isRealtimeConnected.value = false
    }
  }
  
  /**
   * 断开WebSocket连接
   */
  function disconnectWebSocket() {
    if (websocketConnection.value) {
      websocketConnection.value.close()
      websocketConnection.value = null
      isRealtimeConnected.value = false
    }
  }
  
  /**
   * 处理评论实时更新
   * @param update 实时更新数据
   */
  function handleCommentRealtimeUpdate(update: any) {
    switch (update.type) {
      case 'comment_created':
        handleNewComment(update)
        break
      case 'comment_updated':
        handleCommentUpdated(update)
        break
      case 'comment_deleted':
        handleCommentDeleted(update)
        break
      case 'comment_liked':
        handleCommentLiked(update)
        break
    }
  }
  
  /**
   * 处理新评论
   * @param update 实时更新数据
   */
  function handleNewComment(update: any) {
    if (update.data && update.data.comment) {
      const newComment = update.data.comment
      
      // 添加到评论列表
      const existingIndex = comments.value.findIndex(c => c.id === newComment.id)
      if (existingIndex === -1) {
        comments.value.unshift(newComment)
      }
      
      // 更新统计信息
      if (stats.value && stats.value.snippetId === newComment.snippetId) {
        stats.value.totalComments++
        if (newComment.parentId) {
          stats.value.replyComments++
        } else {
          stats.value.rootComments++
        }
      }
    }
  }
  
  /**
   * 处理评论更新
   * @param update 实时更新数据
   */
  function handleCommentUpdated(update: any) {
    if (update.data && update.data.comment) {
      const updatedComment = update.data.comment
      
      // 更新评论列表
      const index = comments.value.findIndex(c => c.id === updatedComment.id)
      if (index !== -1) {
        comments.value[index] = updatedComment
      }
      
      if (currentComment.value?.id === updatedComment.id) {
        currentComment.value = updatedComment
      }
    }
  }
  
  /**
   * 处理评论删除
   * @param update 实时更新数据
   */
  function handleCommentDeleted(update: any) {
    // 从评论列表中移除
    comments.value = comments.value.filter(c => c.id !== update.commentId)
    
    if (currentComment.value?.id === update.commentId) {
      currentComment.value = null
    }
    
    // 更新统计信息
    if (stats.value) {
      stats.value.totalComments--
    }
  }
  
  /**
   * 处理评论点赞
   * @param update 实时更新数据
   */
  function handleCommentLiked(update: any) {
    if (update.data && update.data.commentId) {
      const commentId = update.data.commentId
      const likeCount = update.data.likeCount
      const isLiked = update.data.isLiked
      
      // 更新点赞状态
      likeStats.value[commentId] = {
        count: likeCount,
        isLiked: isLiked
      }
      
      // 更新评论列表中的点赞信息
      const commentIndex = comments.value.findIndex(c => c.id === commentId)
      if (commentIndex !== -1) {
        comments.value[commentIndex].likeCount = likeCount
        comments.value[commentIndex].isLikedByCurrentUser = isLiked
      }
      
      // 更新当前评论的点赞信息
      if (currentComment.value?.id === commentId) {
        currentComment.value.likeCount = likeCount
        currentComment.value.isLikedByCurrentUser = isLiked
      }
    }
  }
  
  // ==================== 防抖函数 ====================
  
  // 防抖的搜索函数
  const debouncedSearch = debounce(searchComments, 300)
  
  // ==================== 监听器 ====================
  
  // 监听筛选条件变化
  watch(filter, () => {
    fetchComments()
  }, { deep: true })
  
  // ==================== 状态重置 ====================
  
  /**
   * 重置所有状态
   */
  function $reset() {
    comments.value = []
    currentComment.value = null
    commentTree.value = []
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
      searchScope: CommentSearchScope.ALL,
      sortBy: CommentSearchSort.RELEVANCE,
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
    
    disconnectWebSocket()
  }
  
  // ==================== 返回值 ====================
  
  return {
    // 状态
    comments,
    currentComment,
    commentTree,
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
    websocketConnection,
    isRealtimeConnected,
    
    // 计算属性
    isLoading,
    hasError,
    isInitialized,
    hasComments,
    hasMoreComments,
    sortedComments,
    buildCommentTree,
    hasActiveCache,
    isOperationLoading,
    getOperationError,
    totalComments,
    totalLikes,
    activeUsers,
    unreadNotifications,
    
    // 评论CRUD操作
    fetchComments,
    fetchMoreComments,
    fetchComment,
    createComment,
    updateComment,
    deleteComment,
    
    // 评论互动功能
    likeComment,
    fetchCommentLikes,
    checkCommentLikeStatus,
    reportComment,
    fetchMyReports,
    
    // 评论统计功能
    fetchCommentStats,
    fetchCommentStatsDetail,
    
    // 评论权限功能
    fetchCommentPermissions,
    
    // 评论搜索功能
    searchComments,
    debouncedSearch,
    
    // 评论筛选功能
    updateFilter,
    resetFilter,
    updateSearchFilter,
    resetSearchFilter,
    
    // 评论管理功能
    batchOperationComments,
    moderateComment,
    
    // 实时更新功能
    connectWebSocket,
    disconnectWebSocket,
    
    // 缓存操作
    setCache,
    getCache,
    clearCache,
    
    // 重置方法
    $reset
  }
})