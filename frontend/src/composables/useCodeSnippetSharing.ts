/**
 * CodeSnippets Store 扩展示例
 * 展示如何使用新增的分享功能
 */

import { useCodeSnippetsStore } from '@/stores/codeSnippets'
import { SharePermission } from '@/types/share'

export function useCodeSnippetSharing() {
  const codeSnippetsStore = useCodeSnippetsStore()

  // 创建分享请求
  async function createShareRequest(snippetId: string) {
    const request = {
      snippetId,
      permission: SharePermission.VIEW,
      allowDownload: true,
      // 可选参数
      expiresAt: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // 7天后过期
      maxAccessCount: 100,
      password: undefined // 如果需要密码保护
    }

    try {
      const shareToken = await codeSnippetsStore.shareSnippet(request)
      if (shareToken) {
        console.log('分享链接已创建:', shareToken.token)
        return shareToken
      } else {
        throw new Error('Failed to create share token')
      }
    } catch (error) {
      console.error('创建分享失败:', error)
      throw error
    }
  }

  // 获取分享链接
  function getShareLink(token: string) {
    return codeSnippetsStore.getSnippetShareLink(token)
  }

  // 复制分享链接
  async function copyShareLink(token: string) {
    try {
      await codeSnippetsStore.copySnippetShareLink(token)
      console.log('分享链接已复制到剪贴板')
      return true
    } catch (error) {
      console.error('复制分享链接失败:', error)
      return false
    }
  }

  // 撤销分享
  async function revokeShare(tokenId: string) {
    try {
      await codeSnippetsStore.revokeSnippetShare(tokenId)
      console.log('分享已撤销')
      return true
    } catch (error) {
      console.error('撤销分享失败:', error)
      return false
    }
  }

  // 获取分享统计
  async function getShareStats(snippetId: string) {
    try {
      const stats = await codeSnippetsStore.fetchSnippetShareStats(snippetId)
      console.log('分享统计:', stats)
      return stats
    } catch (error) {
      console.error('获取分享统计失败:', error)
      throw error
    }
  }

  return {
    createShareRequest,
    getShareLink,
    copyShareLink,
    revokeShare,
    getShareStats,
    
    // 计算属性
    canShare: codeSnippetsStore.canShareSnippet,
    hasActiveShares: codeSnippetsStore.hasActiveShares,
    activeShareTokens: codeSnippetsStore.activeShareTokens,
    shareCount: codeSnippetsStore.currentSnippetShareCount,
    shareStats: codeSnippetsStore.currentSnippetShareStats,
    shareStatsSummary: codeSnippetsStore.currentSnippetShareStatsSummary
  }
}

// 使用示例：
/*
const sharing = useCodeSnippetSharing()

// 检查是否可以分享
if (sharing.canShare) {
  // 创建分享
  const shareToken = await sharing.createShareRequest('snippet-id')
  
  // 获取分享链接
  const link = sharing.getShareLink(shareToken.token)
  
  // 复制分享链接
  await sharing.copyShareLink(shareToken.token)
  
  // 获取分享统计
  const stats = await sharing.getShareStats('snippet-id')
}
*/