/**
 * 评论API服务
 * 
 * 本文件提供评论系统的前端API服务实现，包括：
 * - 评论CRUD操作
 * - 评论点赞功能
 * - 评论举报功能
 * - 评论审核功能
 * - 评论统计功能
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import api from './api'
import type {
  Comment,
  CreateCommentRequest,
  UpdateCommentRequest,
  CommentFilter,
  CommentLikeRequest,
  CommentLikeResponse,
  CommentReport,
  CreateCommentReportRequest,
  HandleCommentReportRequest,
  ModerateCommentRequest,
  CommentStats,
  CommentStatsDetail,
  CommentStatsFilter,
  CommentSearchFilter,
  CommentSearchResult,
  BatchCommentOperationRequest,
  BatchOperationResult,
  PaginatedComments,
  PaginatedCommentReports,
  PaginatedCommentLikes,
  PaginatedCommentSearchResults,
  CommentReportFilter,
  CommentLikeFilter,
  CommentPermissions
} from '@/types/comment'

export class CommentService {
  /**
   * 获取评论列表
   * @param filter 评论筛选条件
   * @returns 评论分页结果
   */
  async getComments(filter: CommentFilter): Promise<PaginatedComments> {
    const response = await api.get<PaginatedComments>('/comments', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取指定评论的详细信息
   * @param id 评论ID
   * @returns 评论详细信息
   */
  async getComment(id: string): Promise<Comment> {
    const response = await api.get<Comment>(`/comments/${id}`)
    return response.data
  }

  /**
   * 创建新评论
   * @param comment 评论创建请求
   * @returns 创建的评论信息
   */
  async createComment(comment: CreateCommentRequest): Promise<Comment> {
    const response = await api.post<Comment>('/comments', comment)
    return response.data
  }

  /**
   * 更新评论内容
   * @param id 评论ID
   * @param comment 评论更新请求
   * @returns 更新后的评论信息
   */
  async updateComment(id: string, comment: UpdateCommentRequest): Promise<Comment> {
    const response = await api.put<Comment>(`/comments/${id}`, comment)
    return response.data
  }

  /**
   * 删除评论
   * @param id 评论ID
   */
  async deleteComment(id: string): Promise<void> {
    await api.delete(`/comments/${id}`)
  }

  /**
   * 点赞评论
   * @param id 评论ID
   * @returns 点赞结果
   */
  async likeComment(id: string): Promise<{ isLiked: boolean; likeCount: number }> {
    const response = await api.post<{ isLiked: boolean; likeCount: number }>(`/comments/${id}/like`)
    return response.data
  }

  /**
   * 获取评论点赞列表
   * @param id 评论ID
   * @param filter 点赞筛选条件
   * @returns 评论点赞列表
   */
  async getCommentLikes(id: string, filter: CommentLikeFilter): Promise<PaginatedCommentLikes> {
    const response = await api.get<PaginatedCommentLikes>(`/comments/${id}/likes`, {
      params: filter
    })
    return response.data
  }

  /**
   * 检查用户是否已点赞指定评论
   * @param id 评论ID
   * @returns 点赞状态
   */
  async getCommentLikeStatus(id: string): Promise<{ isLiked: boolean }> {
    const response = await api.get<{ isLiked: boolean }>(`/comments/${id}/like-status`)
    return response.data
  }

  /**
   * 举报评论
   * @param id 评论ID
   * @param report 举报信息
   * @returns 举报结果
   */
  async reportComment(id: string, report: CreateCommentReportRequest): Promise<CommentReport> {
    const response = await api.post<CommentReport>(`/comments/${id}/report`, report)
    return response.data
  }

  /**
   * 获取评论举报详情
   * @param commentId 评论ID
   * @param reportId 举报ID
   * @returns 举报详情
   */
  async getCommentReport(commentId: string, reportId: string): Promise<CommentReport> {
    const response = await api.get<CommentReport>(`/comments/${commentId}/reports/${reportId}`)
    return response.data
  }

  /**
   * 获取用户的评论举报列表
   * @param filter 举报筛选条件
   * @returns 举报列表
   */
  async getMyCommentReports(filter: CommentReportFilter): Promise<PaginatedCommentReports> {
    const response = await api.get<PaginatedCommentReports>('/comments/my-reports', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取评论统计信息
   * @param snippetId 代码片段ID
   * @returns 评论统计信息
   */
  async getCommentStats(snippetId: string): Promise<CommentStats> {
    const response = await api.get<CommentStats>('/comments/stats', {
      params: { snippetId }
    })
    return response.data
  }

  /**
   * 获取评论审核列表（管理员）
   * @param filter 审核筛选条件
   * @returns 待审核评论列表
   */
  async getPendingComments(filter: CommentFilter): Promise<PaginatedComments> {
    const response = await api.get<PaginatedComments>('/comments/moderation/pending', {
      params: filter
    })
    return response.data
  }

  /**
   * 审核评论（管理员）
   * @param id 评论ID
   * @param moderation 审核结果
   * @returns 审核后的评论信息
   */
  async moderateComment(id: string, moderation: ModerateCommentRequest): Promise<Comment> {
    const response = await api.post<Comment>(`/comments/${id}/moderate`, moderation)
    return response.data
  }

  /**
   * 获取评论举报管理列表（管理员）
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
   * 处理评论举报（管理员）
   * @param reportId 举报ID
   * @param handle 处理结果
   * @returns 处理后的举报信息
   */
  async handleCommentReport(reportId: string, handle: HandleCommentReportRequest): Promise<CommentReport> {
    const response = await api.post<CommentReport>(`/comments/reports/${reportId}/handle`, handle)
    return response.data
  }

  /**
   * 批量操作评论（管理员）
   * @param batch 批量操作请求
   * @returns 操作结果
   */
  async batchOperationComments(batch: BatchCommentOperationRequest): Promise<BatchOperationResult> {
    const response = await api.post<BatchOperationResult>('/comments/batch', batch)
    return response.data
  }

  /**
   * 搜索评论
   * @param filter 搜索筛选条件
   * @returns 搜索结果
   */
  async searchComments(filter: CommentSearchFilter): Promise<PaginatedCommentSearchResults> {
    const response = await api.get<PaginatedCommentSearchResults>('/comments/search', {
      params: filter
    })
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
   * 获取用户评论权限
   * @param snippetId 代码片段ID
   * @returns 用户权限信息
   */
  async getCommentPermissions(snippetId: string): Promise<CommentPermissions> {
    const response = await api.get<CommentPermissions>('/comments/permissions', {
      params: { snippetId }
    })
    return response.data
  }

  /**
   * 导出评论数据
   * @param options 导出选项
   * @returns 导出的文件数据
   */
  async exportComments(options: {
    format: 'json' | 'csv' | 'excel'
    snippetId?: string
    status?: number
    startDate?: string
    endDate?: string
    includeReplies: boolean
    includeUserInfo: boolean
  }): Promise<Blob> {
    const response = await api.get('/comments/export', {
      params: options,
      responseType: 'blob'
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
   * 更新评论设置（管理员）
   * @param settings 评论设置
   * @returns 更新后的设置
   */
  async updateCommentSettings(settings: any): Promise<any> {
    const response = await api.put('/comments/settings', settings)
    return response.data
  }
}

// 导出单例实例
export const commentService = new CommentService()