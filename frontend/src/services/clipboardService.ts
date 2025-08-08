import api from './api'
import type { ClipboardHistoryItem, ClipboardHistoryFilter, PaginatedResult } from '@/types'

export interface ClipboardHistoryWithSnippet extends ClipboardHistoryItem {
  snippet: {
    id: string
    title: string
    language: string
    code: string
  }
}

export interface CopyStats {
  snippetId: string
  copyCount: number
  lastCopiedAt: string
}

export interface BatchCopyStatsRequest {
  snippetIds: string[]
}

/**
 * 剪贴板历史服务类
 * 负责管理剪贴板历史记录的 API 调用
 */
export class ClipboardService {
  /**
   * 获取剪贴板历史记录
   * @param filter 筛选条件
   * @returns 分页的剪贴板历史记录
   */
  async getClipboardHistory(filter: ClipboardHistoryFilter): Promise<PaginatedResult<ClipboardHistoryItem>> {
    const response = await api.get<PaginatedResult<ClipboardHistoryItem>>('/clipboard/history', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取剪贴板历史记录（包含代码片段详情）
   * @param limit 返回记录数限制
   * @returns 剪贴板历史列表
   */
  async getClipboardHistoryWithSnippets(limit: number = 50): Promise<ClipboardHistoryWithSnippet[]> {
    const response = await api.get<ClipboardHistoryWithSnippet[]>('/clipboard/history', {
      params: { limit }
    })
    return response.data
  }

  /**
   * 获取剪贴板历史记录数量
   * @returns 历史记录数量
   */
  async getClipboardHistoryCount(): Promise<number> {
    const response = await api.get<number>('/clipboard/history/count')
    return response.data
  }

  /**
   * 清空剪贴板历史记录
   */
  async clearClipboardHistory(): Promise<void> {
    await api.delete('/clipboard/history')
  }

  /**
   * 删除单个剪贴板历史记录
   * @param id 历史记录ID
   */
  async deleteClipboardHistoryItem(id: string): Promise<void> {
    await api.delete(`/clipboard/history/${id}`)
  }

  /**
   * 记录代码片段复制
   * @param snippetId 代码片段ID
   */
  async recordCopy(snippetId: string): Promise<void> {
    await api.post(`/clipboard/copy/${snippetId}`)
  }

  /**
   * 从历史记录重新复制
   * @param historyId 历史记录ID
   * @returns 复制记录
   */
  async recopyFromHistory(historyId: string): Promise<ClipboardHistoryItem> {
    const response = await api.post<ClipboardHistoryItem>(`/clipboard/history/${historyId}/recopy`)
    return response.data
  }

  /**
   * 获取代码片段的复制统计
   * @param snippetId 代码片段ID
   * @returns 复制统计信息
   */
  async getCopyStats(snippetId: string): Promise<CopyStats> {
    const response = await api.get<CopyStats>(`/clipboard/stats/${snippetId}`)
    return response.data
  }

  /**
   * 批量获取代码片段的复制统计
   * @param request 批量查询请求
   * @returns 复制统计字典
   */
  async getBatchCopyStats(request: BatchCopyStatsRequest): Promise<Record<string, number>> {
    const response = await api.post<Record<string, number>>('/clipboard/stats/batch', request)
    return response.data
  }
}

export const clipboardService = new ClipboardService()
