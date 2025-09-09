import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { codeSnippetService } from '@/services/codeSnippetService'
import { useShareStore } from '@/stores/share'
import type {
  CodeSnippet,
  CreateSnippetRequest,
  UpdateSnippetRequest,
  PaginatedResult,
  SnippetFilter,
  ShareToken,
  ShareStats,
  ShareStatsSummary,
  CreateShareTokenRequest,
  SharePermission
} from '@/types'
import { ShareTokenStatus } from '@/types/share'

export const useCodeSnippetsStore = defineStore('codeSnippets', () => {
  // 依赖注入
  const shareStore = useShareStore()

  // 状态
  const snippets = ref<CodeSnippet[]>([])
  const currentSnippet = ref<CodeSnippet | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)
  const pageSize = ref(10)
  const isLoading = ref(false)
  const filter = ref<SnippetFilter>({
    page: 1,
    pageSize: 10
  })

  // 分享相关状态
  const snippetShareTokens = ref<ShareToken[]>([])
  const currentSnippetShareStats = ref<ShareStats | null>(null)
  const currentSnippetShareStatsSummary = ref<ShareStatsSummary | null>(null)
  const isSharing = ref(false)
  const isFetchingShareStats = ref(false)

  // 计算属性
  const activeShareTokens = computed(() => 
    snippetShareTokens.value.filter(token => token.status === ShareTokenStatus.ACTIVE)
  )
  
  const expiredShareTokens = computed(() => 
    snippetShareTokens.value.filter(token => token.status === ShareTokenStatus.EXPIRED)
  )
  
  const revokedShareTokens = computed(() => 
    snippetShareTokens.value.filter(token => token.status === ShareTokenStatus.REVOKED)
  )
  
  const hasActiveShares = computed(() => activeShareTokens.value.length > 0)
  
  const totalShareCount = computed(() => snippetShareTokens.value.length)
  
  const currentSnippetShareCount = computed(() => 
    currentSnippet.value?.shareCount || 0
  )
  
  const canShareSnippet = computed(() => {
    if (!currentSnippet.value) return false
    return shareStore.hasShareQuota
  })

  // 获取代码片段列表
  async function fetchSnippets(newFilter?: Partial<SnippetFilter>) {
    isLoading.value = true
    try {
      if (newFilter) {
        filter.value = { ...filter.value, ...newFilter }
      }

      const result: PaginatedResult<CodeSnippet> = await codeSnippetService.getSnippets(filter.value)
      snippets.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = result.page
      pageSize.value = result.pageSize
    } finally {
      isLoading.value = false
    }
  }

  // 获取单个代码片段
  async function fetchSnippet(id: string) {
    isLoading.value = true
    try {
      currentSnippet.value = await codeSnippetService.getSnippet(id)
      
      // 自动加载分享信息
      await onSnippetChange(currentSnippet.value)
      
      return currentSnippet.value
    } finally {
      isLoading.value = false
    }
  }

  // 创建代码片段
  async function createSnippet(snippet: CreateSnippetRequest) {
    isLoading.value = true
    try {
      const newSnippet = await codeSnippetService.createSnippet(snippet)
      snippets.value.unshift(newSnippet)
      return newSnippet
    } finally {
      isLoading.value = false
    }
  }

  // 更新代码片段
  async function updateSnippet(id: string, snippet: UpdateSnippetRequest) {
    isLoading.value = true
    try {
      const updatedSnippet = await codeSnippetService.updateSnippet(id, snippet)
      const index = snippets.value.findIndex(s => s.id === id)
      if (index !== -1) {
        snippets.value[index] = updatedSnippet
      }
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = updatedSnippet
      }
      return updatedSnippet
    } finally {
      isLoading.value = false
    }
  }

  // 删除代码片段
  async function deleteSnippet(id: string) {
    isLoading.value = true
    try {
      await codeSnippetService.deleteSnippet(id)
      snippets.value = snippets.value.filter(s => s.id !== id)
      
      // 清除相关的分享状态
      if (currentSnippet.value?.id === id) {
        currentSnippet.value = null
        clearSnippetShareState()
      }
    } finally {
      isLoading.value = false
    }
  }

  // 复制代码片段
  async function copySnippet(id: string) {
    try {
      await codeSnippetService.copySnippet(id)
      // 更新复制计数
      const snippet = snippets.value.find(s => s.id === id)
      if (snippet) {
        snippet.copyCount++
      }
      if (currentSnippet.value?.id === id) {
        currentSnippet.value.copyCount++
      }
    } catch (error) {
      console.error('Failed to copy snippet:', error)
      throw error
    }
  }

  // 搜索代码片段
  async function searchSnippets(searchTerm: string) {
    await fetchSnippets({ search: searchTerm, page: 1 })
  }

  // 按语言筛选
  async function filterByLanguage(language: string) {
    await fetchSnippets({ language, page: 1 })
  }

  // 按标签筛选
  async function filterByTag(tag: string) {
    await fetchSnippets({ tag, page: 1 })
  }

  // 清除筛选
  async function clearFilters() {
    filter.value = { page: 1, pageSize: 10 }
    await fetchSnippets()
  }

  // 分享相关方法
  async function fetchSnippetShareTokens(snippetId: string) {
    isSharing.value = true
    try {
      await shareStore.fetchShareTokens({ snippetId, page: 1, pageSize: 50 })
      snippetShareTokens.value = shareStore.shareTokens.filter(token => token.snippetId === snippetId)
      return snippetShareTokens.value
    } catch (error) {
      console.error('Failed to fetch snippet share tokens:', error)
      throw error
    } finally {
      isSharing.value = false
    }
  }

  async function shareSnippet(request: CreateShareTokenRequest) {
    if (!currentSnippet.value) {
      throw new Error('No snippet selected for sharing')
    }

    isSharing.value = true
    try {
      const shareToken = await shareStore.createShareToken(request)
      
      if (!shareToken) {
        throw new Error('Failed to create share token')
      }
      
      // 更新本地状态
      snippetShareTokens.value.unshift(shareToken)
      
      // 更新代码片段的分享计数
      if (currentSnippet.value.id === request.snippetId) {
        currentSnippet.value.shareCount = (currentSnippet.value.shareCount || 0) + 1
        currentSnippet.value.lastSharedAt = new Date().toISOString()
      }
      
      // 更新代码片段列表中的分享计数
      const snippetIndex = snippets.value.findIndex(s => s.id === request.snippetId)
      if (snippetIndex !== -1) {
        snippets.value[snippetIndex].shareCount = (snippets.value[snippetIndex].shareCount || 0) + 1
        snippets.value[snippetIndex].lastSharedAt = new Date().toISOString()
      }
      
      return shareToken
    } catch (error) {
      console.error('Failed to share snippet:', error)
      throw error
    } finally {
      isSharing.value = false
    }
  }

  async function fetchSnippetShareStats(snippetId: string) {
    isFetchingShareStats.value = true
    try {
      // 获取分享统计汇总
      const summary = await shareStore.fetchShareStatsSummary(snippetId)
      currentSnippetShareStatsSummary.value = summary || null
      
      // 获取当前代码片段的分享令牌统计
      if (snippetShareTokens.value.length > 0) {
        const activeToken = snippetShareTokens.value.find(token => token.status === ShareTokenStatus.ACTIVE)
        if (activeToken) {
          const stats = await shareStore.fetchShareStats(activeToken.id)
          currentSnippetShareStats.value = stats || null
        }
      }
      
      return {
        stats: currentSnippetShareStats.value,
        summary: currentSnippetShareStatsSummary.value
      }
    } catch (error) {
      console.error('Failed to fetch snippet share stats:', error)
      throw error
    } finally {
      isFetchingShareStats.value = false
    }
  }

  async function revokeSnippetShare(tokenId: string) {
    isSharing.value = true
    try {
      await shareStore.revokeShareToken({ tokenId })
      
      // 更新本地状态
      const tokenIndex = snippetShareTokens.value.findIndex(token => token.id === tokenId)
      if (tokenIndex !== -1) {
        snippetShareTokens.value[tokenIndex].status = ShareTokenStatus.REVOKED
      }
      
      return true
    } catch (error) {
      console.error('Failed to revoke snippet share:', error)
      throw error
    } finally {
      isSharing.value = false
    }
  }

  async function deleteSnippetShare(tokenId: string) {
    isSharing.value = true
    try {
      await shareStore.deleteShareToken(tokenId)
      
      // 更新本地状态
      snippetShareTokens.value = snippetShareTokens.value.filter(token => token.id !== tokenId)
      
      return true
    } catch (error) {
      console.error('Failed to delete snippet share:', error)
      throw error
    } finally {
      isSharing.value = false
    }
  }

  function getSnippetShareLink(token: string): string {
    return shareStore.getShareLink(token)
  }

  async function copySnippetShareLink(token: string) {
    try {
      shareStore.copyShareLink(token)
      return true
    } catch (error) {
      console.error('Failed to copy snippet share link:', error)
      throw error
    }
  }

  // 清除分享相关状态
  function clearSnippetShareState() {
    snippetShareTokens.value = []
    currentSnippetShareStats.value = null
    currentSnippetShareStatsSummary.value = null
  }

  // 当切换当前代码片段时，加载相关的分享信息
  async function onSnippetChange(snippet: CodeSnippet | null) {
    if (snippet) {
      // 清除之前的分享状态
      clearSnippetShareState()
      
      // 获取该代码片段的分享令牌
      await fetchSnippetShareTokens(snippet.id)
      
      // 获取分享统计
      await fetchSnippetShareStats(snippet.id)
    } else {
      clearSnippetShareState()
    }
  }

  return {
    // 状态
    snippets,
    currentSnippet,
    totalCount,
    currentPage,
    pageSize,
    isLoading,
    filter,
    
    // 分享相关状态
    snippetShareTokens,
    currentSnippetShareStats,
    currentSnippetShareStatsSummary,
    isSharing,
    isFetchingShareStats,
    
    // 计算属性
    activeShareTokens,
    expiredShareTokens,
    revokedShareTokens,
    hasActiveShares,
    totalShareCount,
    currentSnippetShareCount,
    canShareSnippet,
    
    // 方法
    fetchSnippets,
    fetchSnippet,
    createSnippet,
    updateSnippet,
    deleteSnippet,
    copySnippet,
    searchSnippets,
    filterByLanguage,
    filterByTag,
    clearFilters,
    
    // 分享相关方法
    fetchSnippetShareTokens,
    shareSnippet,
    fetchSnippetShareStats,
    revokeSnippetShare,
    deleteSnippetShare,
    getSnippetShareLink,
    copySnippetShareLink,
    clearSnippetShareState,
    onSnippetChange
  }
})
