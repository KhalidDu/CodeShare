/**
 * 评论管理服务 - 管理员功能
 * 
 * 本文件提供评论系统的管理员功能API服务实现，包括：
 * - 评论审核管理
 * - 举报处理管理
 * - 批量操作管理
 * - 统计分析管理
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import api from './api'
import type {
  Comment,
  CommentFilter,
  CommentReport,
  CommentReportFilter,
  ModerateCommentRequest,
  HandleCommentReportRequest,
  BatchCommentOperationRequest,
  BatchOperationResult,
  CommentStats,
  CommentStatsDetail,
  CommentStatsFilter,
  PaginatedComments,
  PaginatedCommentReports
} from '@/types/comment'

export class CommentAdminService {
  /**
   * 获取待审核评论列表
   * @param filter 筛选条件
   * @returns 待审核评论列表
   */
  async getPendingComments(filter: CommentFilter): Promise<PaginatedComments> {
    const response = await api.get<PaginatedComments>('/comments/moderation/pending', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取已删除评论列表
   * @param filter 筛选条件
   * @returns 已删除评论列表
   */
  async getDeletedComments(filter: CommentFilter): Promise<PaginatedComments> {
    const response = await api.get<PaginatedComments>('/comments/moderation/deleted', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取隐藏评论列表
   * @param filter 筛选条件
   * @returns 隐藏评论列表
   */
  async getHiddenComments(filter: CommentFilter): Promise<PaginatedComments> {
    const response = await api.get<PaginatedComments>('/comments/moderation/hidden', {
      params: filter
    })
    return response.data
  }

  /**
   * 审核评论
   * @param id 评论ID
   * @param moderation 审核请求
   * @returns 审核后的评论
   */
  async moderateComment(id: string, moderation: ModerateCommentRequest): Promise<Comment> {
    const response = await api.post<Comment>(`/comments/${id}/moderate`, moderation)
    return response.data
  }

  /**
   * 批量审核评论
   * @param ids 评论ID列表
   * @param moderation 审核请求
   * @returns 批量审核结果
   */
  async batchModerateComments(ids: string[], moderation: ModerateCommentRequest): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch-moderate', {
      commentIds: ids,
      ...moderation
    })
    return response.data
  }

  /**
   * 获取评论举报列表
   * @param filter 举报筛选条件
   * @returns 举报列表
   */
  async getCommentReports(filter: CommentReportFilter): Promise<PaginatedCommentReports> {
    const response = await api.get<PaginatedCommentReports>('/comments/reports', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取待处理举报列表
   * @param filter 举报筛选条件
   * @returns 待处理举报列表
   */
  async getPendingReports(filter: CommentReportFilter): Promise<PaginatedCommentReports> {
    const response = await api.get<PaginatedCommentReports>('/comments/reports/pending', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取高优先级举报列表
   * @param filter 举报筛选条件
   * @returns 高优先级举报列表
   */
  async getHighPriorityReports(filter: CommentReportFilter): Promise<PaginatedCommentReports> {
    const response = await api.get<PaginatedCommentReports>('/comments/reports/high-priority', {
      params: filter
    })
    return response.data
  }

  /**
   * 处理举报
   * @param reportId 举报ID
   * @param handle 处理请求
   * @returns 处理后的举报
   */
  async handleCommentReport(reportId: string, handle: HandleCommentReportRequest): Promise<CommentReport> {
    const response = await api.post<CommentReport>(`/comments/reports/${reportId}/handle`, handle)
    return response.data
  }

  /**
   * 批量处理举报
   * @param reportIds 举报ID列表
   * @param handle 处理请求
   * @returns 批量处理结果
   */
  async batchHandleReports(reportIds: string[], handle: HandleCommentReportRequest): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/reports/batch-handle', {
      reportIds,
      ...handle
    })
    return response.data
  }

  /**
   * 批量操作评论
   * @param batch 批量操作请求
   * @returns 操作结果
   */
  async batchOperationComments(batch: BatchCommentOperationRequest): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch', batch)
    return response.data
  }

  /**
   * 批量删除评论
   * @param ids 评论ID列表
   * @param reason 删除原因
   * @returns 操作结果
   */
  async batchDeleteComments(ids: string[], reason?: string): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch-delete', {
      commentIds: ids,
      reason
    })
    return response.data
  }

  /**
   * 批量隐藏评论
   * @param ids 评论ID列表
   * @param reason 隐藏原因
   * @returns 操作结果
   */
  async batchHideComments(ids: string[], reason?: string): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch-hide', {
      commentIds: ids,
      reason
    })
    return response.data
  }

  /**
   * 批量显示评论
   * @param ids 评论ID列表
   * @param reason 显示原因
   * @returns 操作结果
   */
  async batchShowComments(ids: string[], reason?: string): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch-show', {
      commentIds: ids,
      reason
    })
    return response.data
  }

  /**
   * 批量批准评论
   * @param ids 评论ID列表
   * @param reason 批准原因
   * @returns 操作结果
   */
  async batchApproveComments(ids: string[], reason?: string): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch-approve', {
      commentIds: ids,
      reason
    })
    return response.data
  }

  /**
   * 获取评论统计信息
   * @param snippetId 代码片段ID
   * @returns 统计信息
   */
  async getCommentStats(snippetId: string): Promise<CommentStats> {
    const response = await api.get<CommentStats>('/comments/stats', {
      params: { snippetId }
    })
    return response.data
  }

  /**
   * 获取全局评论统计
   * @returns 全局统计信息
   */
  async getGlobalCommentStats(): Promise<CommentStatsDetail> {
    const response = await api.get<CommentStatsDetail>('/comments/stats/global')
    return response.data
  }

  /**
   * 获取评论统计详细信息
   * @param filter 统计筛选条件
   * @returns 统计详细信息
   */
  async getCommentStatsDetail(filter: CommentStatsFilter): Promise<CommentStatsDetail> {
    const response = await api.get<CommentStatsDetail>('/comments/stats/detail', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取评论趋势分析
   * @param days 天数
   * @returns 趋势分析数据
   */
  async getCommentTrends(days: number = 30): Promise<any> {
    const response = await api.get('/comments/stats/trends', {
      params: { days }
    })
    return response.data
  }

  /**
   * 获取用户评论排行
   * @param limit 限制数量
   * @returns 用户排行
   */
  async getTopCommenters(limit: number = 10): Promise<any> {
    const response = await api.get('/comments/stats/top-commenters', {
      params: { limit }
    })
    return response.data
  }

  /**
   * 获取热门评论排行
   * @param limit 限制数量
   * @returns 热门评论排行
   */
  async getTopComments(limit: number = 10): Promise<Comment[]> {
    const response = await api.get<Comment[]>('/comments/stats/top-comments', {
      params: { limit }
    })
    return response.data
  }

  /**
   * 获取评论设置
   * @returns 评论设置
   */
  async getCommentSettings(): Promise<any> {
    const response = await api.get('/comments/settings')
    return response.data
  }

  /**
   * 更新评论设置
   * @param settings 评论设置
   * @returns 更新后的设置
   */
  async updateCommentSettings(settings: any): Promise<any> {
    const response = await api.put('/comments/settings', settings)
    return response.data
  }

  /**
   * 获取评论审核队列状态
   * @returns 审核队列状态
   */
  async getModerationQueueStatus(): Promise<any> {
    const response = await api.get('/comments/moderation/queue-status')
    return response.data
  }

  /**
   * 获取举报处理队列状态
   * @returns 举报队列状态
   */
  async getReportQueueStatus(): Promise<any> {
    const response = await api.get('/comments/reports/queue-status')
    return response.data
  }

  /**
   * 清理过期数据
   * @param options 清理选项
   * @returns 清理结果
   */
  async cleanupExpiredData(options: {
    deleteOldReports?: boolean
    deleteOldDeletedComments?: boolean
    daysThreshold?: number
  }): Promise<any> {
    const response = await api.post('/comments/cleanup', options)
    return response.data
  }

  /**
   * 导出管理数据
   * @param options 导出选项
   * @returns 导出的文件数据
   */
  async exportAdminData(options: {
    format: 'json' | 'csv' | 'excel'
    dataType: 'comments' | 'reports' | 'stats'
    startDate?: string
    endDate?: string
    includeDeleted?: boolean
  }): Promise<Blob> {
    const response = await api.get('/comments/admin/export', {
      params: options,
      responseType: 'blob'
    })
    return response.data
  }
}

// 导出单例实例
export const commentAdminService = new CommentAdminService()