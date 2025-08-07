import api from './api'
import type { ClipboardHistoryItem, ClipboardHistoryFilter, PaginatedResult } from '@/types'

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
}

export const clipboardService = new ClipboardService()