import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { shareService } from '@/services/shareService'
import { useAuthStore } from '@/stores/auth'
import type {
  ShareToken,
  CreateShareTokenRequest,
  UpdateShareTokenRequest,
  ShareStats,
  ShareAccessRecord,
  ShareTokenFilter,
  ShareAccessFilter,
  ValidateShareTokenRequest,
  ValidateShareTokenResponse,
  AccessShareRequest,
  AccessShareResponse,
  RevokeShareTokenRequest,
  RevokeShareTokenResponse,
  BatchShareTokenRequest,
  BatchShareTokenResponse,
  ShareStatsSummary,
  ShareSettings,
  UserShareQuota,
  PaginatedResult
} from '@/types'
import { ShareTokenStatus, SharePermission } from '@/types/share'

export const useShareStore = defineStore('share', () => {
  // 依赖注入
  const authStore = useAuthStore()

  // 状态定义
  const shareTokens = ref<ShareToken[]>([])
  const currentShareToken = ref<ShareToken | null>(null)
  const shareStats = ref<ShareStats | null>(null)
  const shareAccessRecords = ref<ShareAccessRecord[]>([])
  const shareStatsSummary = ref<ShareStatsSummary | null>(null)
  const shareSettings = ref<ShareSettings | null>(null)
  const userShareQuota = ref<UserShareQuota | null>(null)
  
  // 分页状态
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(10)
  
  // 加载状态
  const isLoading = ref(false)
  const isCreating = ref(false)
  const isUpdating = ref(false)
  const isDeleting = ref(false)
  const isValidating = ref(false)
  const isAccessing = ref(false)
  
  // 错误状态
  const error = ref<string | null>(null)
  
  // 过滤器状态
  const filter = ref<ShareTokenFilter>({
    page: 1,
    pageSize: 10
  })
  
  const accessFilter = ref<ShareAccessFilter>({
    page: 1,
    pageSize: 20
  })

  // 计算属性
  const isAuthenticated = computed(() => authStore.isAuthenticated)
  const currentUserId = computed(() => authStore.user?.id)
  
  const activeShareTokens = computed(() => 
    shareTokens.value.filter(token => token.status === ShareTokenStatus.ACTIVE)
  )
  
  const expiredShareTokens = computed(() => 
    shareTokens.value.filter(token => token.status === ShareTokenStatus.EXPIRED)
  )
  
  const revokedShareTokens = computed(() => 
    shareTokens.value.filter(token => token.status === ShareTokenStatus.REVOKED)
  )
  
  const myShareTokens = computed(() => 
    shareTokens.value.filter(token => token.createdBy === currentUserId.value)
  )
  
  const hasShareQuota = computed(() => {
    if (!userShareQuota.value) return true
    return userShareQuota.value.currentActiveShares < userShareQuota.value.maxActiveShares
  })
  
  const shareQuotaPercentage = computed(() => {
    if (!userShareQuota.value) return 0
    return (userShareQuota.value.currentActiveShares / userShareQuota.value.maxActiveShares) * 100
  })

  // 持久化缓存
  const CACHE_KEYS = {
    SHARE_TOKENS: 'share_tokens',
    SHARE_SETTINGS: 'share_settings',
    SHARE_QUOTA: 'share_quota',
    SHARE_STATS_SUMMARY: 'share_stats_summary'
  }

  // 缓存管理
  function saveToCache(key: string, data: any) {
    try {
      localStorage.setItem(key, JSON.stringify({
        data,
        timestamp: Date.now(),
        userId: currentUserId.value
      }))
    } catch (err) {
      console.warn(`Failed to save to cache: ${key}`, err)
    }
  }

  function getFromCache<T>(key: string): T | null {
    try {
      const cached = localStorage.getItem(key)
      if (!cached) return null
      
      const { data, timestamp, userId } = JSON.parse(cached)
      // 检查缓存是否属于当前用户
      if (userId !== currentUserId.value) return null
      
      // 缓存有效期5分钟
      const isValid = Date.now() - timestamp < 5 * 60 * 1000
      return isValid ? data : null
    } catch (err) {
      console.warn(`Failed to get from cache: ${key}`, err)
      return null
    }
  }

  function clearCache() {
    Object.values(CACHE_KEYS).forEach(key => {
      localStorage.removeItem(key)
    })
  }

  // 错误处理
  function handleError(err: any, context: string) {
    console.error(`Share store error in ${context}:`, err)
    error.value = err.response?.data?.message || err.message || `${context} failed`
    
    // 清除相关缓存
    if (context.includes('share token')) {
      localStorage.removeItem(CACHE_KEYS.SHARE_TOKENS)
    }
    
    throw err
  }

  // 分享令牌管理
  async function fetchShareTokens(newFilter?: Partial<ShareTokenFilter>) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to fetch share tokens')
    }

    isLoading.value = true
    error.value = null
    
    try {
      if (newFilter) {
        filter.value = { ...filter.value, ...newFilter }
      }

      // 尝试从缓存获取
      const cacheKey = `${CACHE_KEYS.SHARE_TOKENS}_${JSON.stringify(filter.value)}`
      const cached = getFromCache<PaginatedResult<ShareToken>>(cacheKey)
      if (cached) {
        shareTokens.value = cached.items
        totalCount.value = cached.totalCount
        currentPage.value = cached.page
        pageSize.value = cached.pageSize
        return cached
      }

      const result: PaginatedResult<ShareToken> = await shareService.getShareTokens(filter.value)
      shareTokens.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = result.page
      pageSize.value = result.pageSize
      
      // 保存到缓存
      saveToCache(cacheKey, result)
      
      return result
    } catch (err) {
      handleError(err, 'fetch share tokens')
    } finally {
      isLoading.value = false
    }
  }

  async function fetchShareToken(id: string) {
    isLoading.value = true
    error.value = null
    
    try {
      currentShareToken.value = await shareService.getShareToken(id)
      return currentShareToken.value
    } catch (err) {
      handleError(err, 'fetch share token')
    } finally {
      isLoading.value = false
    }
  }

  async function fetchShareTokenByToken(token: string) {
    isLoading.value = true
    error.value = null
    
    try {
      currentShareToken.value = await shareService.getShareTokenByToken(token)
      return currentShareToken.value
    } catch (err) {
      handleError(err, 'fetch share token by token')
    } finally {
      isLoading.value = false
    }
  }

  async function createShareToken(request: CreateShareTokenRequest) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to create share tokens')
    }

    isCreating.value = true
    error.value = null
    
    try {
      // 验证请求参数
      const validation = shareService.validateShareRequest(request)
      if (!validation.isValid) {
        throw new Error(validation.errors.join(', '))
      }

      // 检查配额
      await checkShareQuota(request.snippetId)

      const newShareToken = await shareService.createShareToken(request)
      shareTokens.value.unshift(newShareToken)
      currentShareToken.value = newShareToken
      
      // 清除相关缓存
      clearCache()
      
      return newShareToken
    } catch (err) {
      handleError(err, 'create share token')
    } finally {
      isCreating.value = false
    }
  }

  async function updateShareToken(id: string, request: UpdateShareTokenRequest) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to update share tokens')
    }

    isUpdating.value = true
    error.value = null
    
    try {
      const updatedShareToken = await shareService.updateShareToken(id, request)
      const index = shareTokens.value.findIndex(t => t.id === id)
      if (index !== -1) {
        shareTokens.value[index] = updatedShareToken
      }
      if (currentShareToken.value?.id === id) {
        currentShareToken.value = updatedShareToken
      }
      
      // 清除相关缓存
      clearCache()
      
      return updatedShareToken
    } catch (err) {
      handleError(err, 'update share token')
    } finally {
      isUpdating.value = false
    }
  }

  async function deleteShareToken(id: string) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to delete share tokens')
    }

    isDeleting.value = true
    error.value = null
    
    try {
      await shareService.deleteShareToken(id)
      shareTokens.value = shareTokens.value.filter(t => t.id !== id)
      if (currentShareToken.value?.id === id) {
        currentShareToken.value = null
      }
      
      // 清除相关缓存
      clearCache()
    } catch (err) {
      handleError(err, 'delete share token')
    } finally {
      isDeleting.value = false
    }
  }

  async function revokeShareToken(request: RevokeShareTokenRequest) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to revoke share tokens')
    }

    isUpdating.value = true
    error.value = null
    
    try {
      const response = await shareService.revokeShareToken(request)
      const index = shareTokens.value.findIndex(t => t.id === request.tokenId)
      if (index !== -1) {
        shareTokens.value[index].status = ShareTokenStatus.REVOKED
      }
      if (currentShareToken.value?.id === request.tokenId) {
        currentShareToken.value.status = ShareTokenStatus.REVOKED
      }
      
      // 清除相关缓存
      clearCache()
      
      return response
    } catch (err) {
      handleError(err, 'revoke share token')
    } finally {
      isUpdating.value = false
    }
  }

  // 分享访问和验证
  async function validateShareToken(request: ValidateShareTokenRequest): Promise<ValidateShareTokenResponse> {
    isValidating.value = true
    error.value = null
    
    try {
      const response = await shareService.validateShareToken(request)
      if (response.shareToken) {
        currentShareToken.value = response.shareToken
      }
      return response
    } catch (err) {
      handleError(err, 'validate share token')
      throw err
    } finally {
      isValidating.value = false
    }
  }

  async function accessShare(request: AccessShareRequest): Promise<AccessShareResponse> {
    isAccessing.value = true
    error.value = null
    
    try {
      const validation = shareService.validateAccessRequest(request)
      if (!validation.isValid) {
        throw new Error(validation.errors.join(', '))
      }

      const response = await shareService.accessShare(request)
      if (response.shareToken) {
        currentShareToken.value = response.shareToken
      }
      return response
    } catch (err) {
      handleError(err, 'access share')
      throw err
    } finally {
      isAccessing.value = false
    }
  }

  // 分享统计
  async function fetchShareStats(tokenId: string) {
    isLoading.value = true
    error.value = null
    
    try {
      shareStats.value = await shareService.getShareStats(tokenId)
      return shareStats.value
    } catch (err) {
      handleError(err, 'fetch share stats')
    } finally {
      isLoading.value = false
    }
  }

  async function fetchShareAccessRecords(newFilter?: Partial<ShareAccessFilter>) {
    isLoading.value = true
    error.value = null
    
    try {
      if (newFilter) {
        accessFilter.value = { ...accessFilter.value, ...newFilter }
      }

      const result: PaginatedResult<ShareAccessRecord> = await shareService.getShareAccessRecords(accessFilter.value)
      shareAccessRecords.value = result.items
      return result
    } catch (err) {
      handleError(err, 'fetch share access records')
    } finally {
      isLoading.value = false
    }
  }

  async function fetchShareStatsSummary(snippetId?: string) {
    isLoading.value = true
    error.value = null
    
    try {
      // 尝试从缓存获取
      const cacheKey = `${CACHE_KEYS.SHARE_STATS_SUMMARY}_${snippetId || 'global'}`
      const cached = getFromCache<ShareStatsSummary>(cacheKey)
      if (cached) {
        shareStatsSummary.value = cached
        return cached
      }

      shareStatsSummary.value = await shareService.getShareStatsSummary(snippetId)
      
      // 保存到缓存
      saveToCache(cacheKey, shareStatsSummary.value)
      
      return shareStatsSummary.value
    } catch (err) {
      handleError(err, 'fetch share stats summary')
    } finally {
      isLoading.value = false
    }
  }

  // 设置和配额管理
  async function fetchShareSettings() {
    isLoading.value = true
    error.value = null
    
    try {
      // 尝试从缓存获取
      const cached = getFromCache<ShareSettings>(CACHE_KEYS.SHARE_SETTINGS)
      if (cached) {
        shareSettings.value = cached
        return cached
      }

      shareSettings.value = await shareService.getShareSettings()
      
      // 保存到缓存
      saveToCache(CACHE_KEYS.SHARE_SETTINGS, shareSettings.value)
      
      return shareSettings.value
    } catch (err) {
      handleError(err, 'fetch share settings')
    } finally {
      isLoading.value = false
    }
  }

  async function updateShareSettings(settings: Partial<ShareSettings>) {
    if (!isAuthenticated.value || !authStore.isAdmin) {
      throw new Error('Admin access required to update share settings')
    }

    isUpdating.value = true
    error.value = null
    
    try {
      shareSettings.value = await shareService.updateShareSettings(settings)
      
      // 清除缓存
      localStorage.removeItem(CACHE_KEYS.SHARE_SETTINGS)
      
      return shareSettings.value
    } catch (err) {
      handleError(err, 'update share settings')
    } finally {
      isUpdating.value = false
    }
  }

  async function fetchUserShareQuota(userId?: string) {
    isLoading.value = true
    error.value = null
    
    try {
      const targetUserId = userId || currentUserId.value
      if (!targetUserId) {
        throw new Error('User ID is required to fetch share quota')
      }

      // 尝试从缓存获取
      const cacheKey = `${CACHE_KEYS.SHARE_QUOTA}_${targetUserId}`
      const cached = getFromCache<UserShareQuota>(cacheKey)
      if (cached) {
        userShareQuota.value = cached
        return cached
      }

      userShareQuota.value = await shareService.getUserShareQuota(targetUserId)
      
      // 保存到缓存
      saveToCache(cacheKey, userShareQuota.value)
      
      return userShareQuota.value
    } catch (err) {
      handleError(err, 'fetch user share quota')
    } finally {
      isLoading.value = false
    }
  }

  async function checkShareQuota(snippetId: string) {
    try {
      const checkResult = await shareService.canCreateShareToken(snippetId)
      if (!checkResult.canCreate) {
        throw new Error(checkResult.reason || 'Share quota exceeded')
      }
      
      // 刷新配额信息
      await fetchUserShareQuota()
      
      return checkResult
    } catch (err) {
      handleError(err, 'check share quota')
    }
  }

  // 批量操作
  async function batchShareTokens(request: BatchShareTokenRequest) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to perform batch operations')
    }

    isUpdating.value = true
    error.value = null
    
    try {
      const response = await shareService.batchShareTokens(request)
      
      // 刷新分享令牌列表
      await fetchShareTokens()
      
      // 清除相关缓存
      clearCache()
      
      return response
    } catch (err) {
      handleError(err, 'batch share tokens')
    } finally {
      isUpdating.value = false
    }
  }

  // 工具方法
  function getShareLink(token: string): string {
    return shareService.getShareLink(token)
  }

  function copyShareLink(token: string) {
    const link = getShareLink(token)
    navigator.clipboard.writeText(link).catch(err => {
      console.error('Failed to copy share link:', err)
      throw new Error('Failed to copy share link to clipboard')
    })
  }

  async function refreshShareToken(tokenId: string) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to refresh share tokens')
    }

    isUpdating.value = true
    error.value = null
    
    try {
      const refreshedToken = await shareService.refreshShareToken(tokenId)
      const index = shareTokens.value.findIndex(t => t.id === tokenId)
      if (index !== -1) {
        shareTokens.value[index] = refreshedToken
      }
      if (currentShareToken.value?.id === tokenId) {
        currentShareToken.value = refreshedToken
      }
      
      // 清除相关缓存
      clearCache()
      
      return refreshedToken
    } catch (err) {
      handleError(err, 'refresh share token')
    } finally {
      isUpdating.value = false
    }
  }

  async function copyShareToken(tokenId: string) {
    if (!isAuthenticated.value) {
      throw new Error('User must be authenticated to copy share tokens')
    }

    isCreating.value = true
    error.value = null
    
    try {
      const newToken = await shareService.copyShareToken(tokenId)
      shareTokens.value.unshift(newToken)
      
      // 清除相关缓存
      clearCache()
      
      return newToken
    } catch (err) {
      handleError(err, 'copy share token')
    } finally {
      isCreating.value = false
    }
  }

  // 过滤和搜索
  async function searchShareTokens(searchTerm: string) {
    await fetchShareTokens({ search: searchTerm, page: 1 })
  }

  async function filterByStatus(status: ShareTokenStatus) {
    await fetchShareTokens({ status, page: 1 })
  }

  async function filterByPermission(permission: SharePermission) {
    await fetchShareTokens({ permission, page: 1 })
  }

  async function filterBySnippet(snippetId: string) {
    await fetchShareTokens({ snippetId, page: 1 })
  }

  async function clearFilters() {
    filter.value = { page: 1, pageSize: 10 }
    await fetchShareTokens()
  }

  // 状态重置
  function resetError() {
    error.value = null
  }

  function resetCurrentShareToken() {
    currentShareToken.value = null
  }

  function resetShareStats() {
    shareStats.value = null
    shareAccessRecords.value = []
  }

  function reset() {
    shareTokens.value = []
    currentShareToken.value = null
    shareStats.value = null
    shareAccessRecords.value = []
    shareStatsSummary.value = null
    shareSettings.value = null
    userShareQuota.value = null
    totalCount.value = 0
    currentPage.value = 1
    pageSize.value = 10
    error.value = null
    filter.value = { page: 1, pageSize: 10 }
    accessFilter.value = { page: 1, pageSize: 20 }
    clearCache()
  }

  return {
    // 状态
    shareTokens,
    currentShareToken,
    shareStats,
    shareAccessRecords,
    shareStatsSummary,
    shareSettings,
    userShareQuota,
    totalCount,
    currentPage,
    pageSize,
    isLoading,
    isCreating,
    isUpdating,
    isDeleting,
    isValidating,
    isAccessing,
    error,
    filter,
    accessFilter,
    
    // 计算属性
    isAuthenticated,
    currentUserId,
    activeShareTokens,
    expiredShareTokens,
    revokedShareTokens,
    myShareTokens,
    hasShareQuota,
    shareQuotaPercentage,
    
    // 分享令牌管理
    fetchShareTokens,
    fetchShareToken,
    fetchShareTokenByToken,
    createShareToken,
    updateShareToken,
    deleteShareToken,
    revokeShareToken,
    
    // 分享访问和验证
    validateShareToken,
    accessShare,
    
    // 分享统计
    fetchShareStats,
    fetchShareAccessRecords,
    fetchShareStatsSummary,
    
    // 设置和配额管理
    fetchShareSettings,
    updateShareSettings,
    fetchUserShareQuota,
    checkShareQuota,
    
    // 批量操作
    batchShareTokens,
    
    // 工具方法
    getShareLink,
    copyShareLink,
    refreshShareToken,
    copyShareToken,
    
    // 过滤和搜索
    searchShareTokens,
    filterByStatus,
    filterByPermission,
    filterBySnippet,
    clearFilters,
    
    // 状态重置
    resetError,
    resetCurrentShareToken,
    resetShareStats,
    reset
  }
})